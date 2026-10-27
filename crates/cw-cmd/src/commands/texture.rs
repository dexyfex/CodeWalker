//! Texture dictionaries (`.ytd`) and DDS files as commands.

use std::path::{Path, PathBuf};

use cw_core::{
    image::{Channels, TiledImage, channel_filter, invert_filter},
    texture::{self, Texture},
    ytd::{self, YtdTexture},
};
use serde::Deserialize;
use serde_json::{Value, json};

use super::{
    args::parse,
    rpf::safe_component,
    source::{self, Loaded, Source},
};
use crate::{
    engine::{Ctx, Engine},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new("texture.list", "textures", "List the textures in a .ytd or .dds")
            .param(ParamSpec::new("path", K::String, "entry path of a .ytd inside the open archives"))
            .param(ParamSpec::new("file", K::Path, "a .ytd or .dds file on disk (instead of `path`)")),
        list,
    );
    e.register(
        CommandSpec::new(
            "texture.export",
            "textures",
            "Save textures as .dds (lossless) or .png (decoded, filtered)",
        )
        .param(ParamSpec::new("path", K::String, "entry path of a .ytd inside the open archives"))
        .param(ParamSpec::new("file", K::Path, "a .ytd or .dds file on disk (instead of `path`)"))
        .param(ParamSpec::new("index", K::Integer, "only this texture (default: all)"))
        .param(ParamSpec::new("dest", K::Path, "destination folder").required())
        .param(ParamSpec::new(
            "format",
            K::String,
            "`dds` (default, keeps every mip and the format) or `png`",
        ))
        .param(ParamSpec::new("mip", K::Integer, "png only: mip level to decode (default 0)"))
        .param(ParamSpec::new("channels", K::String, "png only: rgba (default), rgb, r, g, b or a"))
        .param(ParamSpec::new("invert", K::Boolean, "png only: invert the colours"))
        .destructive(),
        export,
    );
}

/// Loads the textures of a dictionary or DDS from an archive entry or a disk file.
fn load(ctx: &Ctx, src: &Source) -> Result<Vec<YtdTexture>, CmdError> {
    let (loaded, name) = source::read(ctx, src)?;
    match loaded {
        Loaded::Resource(res) => Ok(ytd::parse_ytd(&res)?),
        Loaded::Plain(data) => {
            let t = texture::parse_dds(&data)?;
            let stem = name.rsplit_once('.').map_or(name.as_str(), |(s, _)| s).to_string();
            Ok(vec![YtdTexture { name_hash: cw_core::hash::joaat_lower(&stem), name: stem, texture: t }])
        }
    }
}

fn summary(i: usize, t: &YtdTexture) -> Value {
    let tx: &Texture = &t.texture;
    json!({
        "index": i, "name": t.name, "hash": format!("0x{:08X}", t.name_hash),
        "width": tx.width, "height": tx.height, "levels": tx.levels,
        "format": tx.format.label(), "bytes": tx.data.len(),
    })
}

fn list(ctx: &Ctx, args: Value) -> CmdResult {
    let src: Source = parse(args)?;
    let textures = load(ctx, &src)?;
    Ok(
        json!({ "count": textures.len(), "textures": textures.iter().enumerate().map(|(i, t)| summary(i, t)).collect::<Vec<_>>() }),
    )
}

fn export(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        #[serde(flatten)]
        src: Source,
        index: Option<usize>,
        dest: PathBuf,
        format: Option<String>,
        mip: Option<u32>,
        channels: Option<String>,
        invert: Option<bool>,
    }
    let a: A = parse(args)?;
    let png = match a.format.as_deref().map(str::to_ascii_lowercase).as_deref() {
        None | Some("dds") => false,
        Some("png") => true,
        Some(other) => return Err(CmdError::invalid(format!("unknown format `{other}`; use dds or png"))),
    };
    if !png && (a.mip.is_some() || a.channels.is_some() || a.invert.is_some()) {
        return Err(CmdError::invalid(
            "`mip`, `channels` and `invert` apply to png output; add `--format png`",
        ));
    }
    let channels = match a.channels.as_deref() {
        None => Channels::Rgba,
        Some(c) => Channels::parse(c).ok_or_else(|| {
            CmdError::invalid(format!("unknown channels `{c}`; use rgba, rgb, r, g, b or a"))
        })?,
    };
    let textures = load(ctx, &a.src)?;
    let picked: Vec<(usize, &YtdTexture)> = match a.index {
        Some(i) => vec![(i, textures.get(i).ok_or_else(|| CmdError::not_found(format!("no texture #{i}")))?)],
        None => textures.iter().enumerate().collect(),
    };
    std::fs::create_dir_all(&a.dest)?;
    let (mut written, mut skipped) = (Vec::new(), Vec::new());
    for (i, t) in picked {
        let stem = if t.name.is_empty() { format!("texture_{i}") } else { safe_component(&t.name) };
        let encoded = if png {
            render_png(&t.texture, a.mip.unwrap_or(0), channels, a.invert.unwrap_or(false))
        } else {
            texture::write_dds(&t.texture).map_err(CmdError::from)
        };
        match encoded {
            Ok(bytes) => {
                let path = unique(&a.dest, &stem, if png { "png" } else { "dds" });
                std::fs::write(&path, bytes)?;
                written.push(path.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default());
            }
            Err(e) => skipped.push(json!({ "name": t.name, "reason": e.to_string() })),
        }
    }
    Ok(json!({ "written": written.len(), "files": written, "skipped": skipped, "dest": a.dest }))
}

/// The decoded mip, through the same tiled filters the viewer uses, as a PNG.
fn render_png(t: &Texture, mip: u32, channels: Channels, invert: bool) -> Result<Vec<u8>, CmdError> {
    let px = t.decode(mip)?;
    let mut image = TiledImage::from_rgba(px.width as usize, px.height as usize, &px.pixels);
    if channels != Channels::Rgba {
        image = image.apply(&channel_filter(channels));
    }
    if invert {
        image = image.apply(&invert_filter);
    }
    Ok(image.to_png())
}

/// `name.ext`, or `name_2.ext` … when it already exists (several textures can share a name).
fn unique(dir: &Path, stem: &str, ext: &str) -> PathBuf {
    let mut p = dir.join(format!("{stem}.{ext}"));
    let mut n = 2;
    while p.exists() {
        p = dir.join(format!("{stem}_{n}.{ext}"));
        n += 1;
    }
    p
}
