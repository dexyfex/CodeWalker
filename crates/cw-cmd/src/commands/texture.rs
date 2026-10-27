//! Texture dictionaries (`.ytd`) and DDS files as commands.

use std::path::{Path, PathBuf};

use cw_core::{
    resource,
    texture::{self, Texture},
    ytd::{self, YtdTexture},
};
use serde::Deserialize;
use serde_json::{Value, json};

use super::{args::parse, rpf::safe_component};
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
        CommandSpec::new("texture.export", "textures", "Save textures as .dds files")
            .param(ParamSpec::new("path", K::String, "entry path of a .ytd inside the open archives"))
            .param(ParamSpec::new("file", K::Path, "a .ytd or .dds file on disk (instead of `path`)"))
            .param(ParamSpec::new("index", K::Integer, "only this texture (default: all)"))
            .param(ParamSpec::new("dest", K::Path, "destination folder").required())
            .destructive(),
        export,
    );
}

#[derive(Deserialize)]
struct Source {
    path: Option<String>,
    file: Option<PathBuf>,
}

/// Loads the textures of a dictionary or DDS from an archive entry or a disk file.
fn load(ctx: &Ctx, src: &Source) -> Result<Vec<YtdTexture>, CmdError> {
    let (bytes, name_hint): (Loaded, String) = match (&src.path, &src.file) {
        (Some(p), None) => {
            let tree = ctx.tree()?;
            let r = tree.find(p).ok_or_else(|| CmdError::not_found(format!("no entry `{p}`")))?;
            let e = tree.entry(r);
            if e.is_resource() {
                (Loaded::Resource(tree.extract_resource(r)?), e.name.clone())
            } else {
                (Loaded::Plain(tree.extract(r)?), e.name.clone())
            }
        }
        (None, Some(f)) => {
            let raw = std::fs::read(f)?;
            let name = f.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
            if resource::has_rsc7_header(&raw) {
                (Loaded::Resource(resource::parse_loose(&raw)?), name)
            } else {
                (Loaded::Plain(raw), name)
            }
        }
        _ => return Err(CmdError::invalid("give exactly one of `path` or `file`")),
    };
    match bytes {
        Loaded::Resource(res) => Ok(ytd::parse_ytd(&res)?),
        Loaded::Plain(data) => {
            let t = texture::parse_dds(&data)?;
            let stem = name_hint.rsplit_once('.').map_or(name_hint.as_str(), |(s, _)| s).to_string();
            Ok(vec![YtdTexture { name_hash: cw_core::hash::joaat_lower(&stem), name: stem, texture: t }])
        }
    }
}

enum Loaded {
    Resource(cw_core::rpf::ResourceData),
    Plain(Vec<u8>),
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
    }
    let a: A = parse(args)?;
    let textures = load(ctx, &a.src)?;
    let picked: Vec<(usize, &YtdTexture)> = match a.index {
        Some(i) => vec![(i, textures.get(i).ok_or_else(|| CmdError::not_found(format!("no texture #{i}")))?)],
        None => textures.iter().enumerate().collect(),
    };
    std::fs::create_dir_all(&a.dest)?;
    let (mut written, mut skipped) = (Vec::new(), Vec::new());
    for (i, t) in picked {
        let stem = if t.name.is_empty() { format!("texture_{i}") } else { safe_component(&t.name) };
        match texture::write_dds(&t.texture) {
            Ok(bytes) => {
                let path = unique(&a.dest, &stem);
                std::fs::write(&path, bytes)?;
                written.push(path.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default());
            }
            Err(e) => skipped.push(json!({ "name": t.name, "reason": e.to_string() })),
        }
    }
    Ok(json!({ "written": written.len(), "files": written, "skipped": skipped, "dest": a.dest }))
}

/// `name.dds`, or `name_2.dds` … when it already exists (several textures can share a name).
fn unique(dir: &Path, stem: &str) -> PathBuf {
    let mut p = dir.join(format!("{stem}.dds"));
    let mut n = 2;
    while p.exists() {
        p = dir.join(format!("{stem}_{n}.dds"));
        n += 1;
    }
    p
}
