use std::path::PathBuf;

use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine},
    error::{CmdError, CmdResult},
    mods::{self, ModError},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

impl From<ModError> for CmdError {
    fn from(e: ModError) -> Self {
        let code = match &e {
            ModError::Invalid(_) => "invalid_args",
            ModError::NotFound(_) => "not_found",
            ModError::Exists(_) => "already_exists",
            ModError::Io(_) => "io",
        };
        CmdError::new(code, e.to_string())
    }
}

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new(
            "mods.list",
            "mods",
            "List the files in the game's mods folder and whether they are on",
        ),
        list,
    );
    e.register(
        CommandSpec::new("mods.copy", "mods", "Make an editable copy of a game archive in the mods folder")
            .param(
                ParamSpec::new(
                    "path",
                    K::String,
                    "an archive (or anything inside one); its top-level .rpf is copied",
                )
                .required(),
            )
            .param(ParamSpec::new("overwrite", K::Boolean, "replace an existing copy"))
            .destructive(),
        copy,
    );
    for (name, summary, enable) in [
        ("mods.enable", "Turn a mod override on", true),
        ("mods.disable", "Turn a mod override off (the file is kept)", false),
    ] {
        e.register(
            CommandSpec::new(name, "mods", summary)
                .param(ParamSpec::new("path", K::String, "path inside the mods folder").required())
                .destructive(),
            move |ctx, args| set_enabled(ctx, args, enable),
        );
    }
    e.register(
        CommandSpec::new("mods.remove", "mods", "Delete a mod override from the mods folder")
            .param(ParamSpec::new("path", K::String, "path inside the mods folder").required())
            .destructive(),
        remove,
    );
    e.register(
        CommandSpec::new(
            "mods.install",
            "mods",
            "Copy a file or a folder (laid out like the game folder) into the mods folder",
        )
        .param(ParamSpec::new("source", K::Path, "file or folder on disk").required())
        .param(ParamSpec::new(
            "dest",
            K::String,
            "path inside mods/ (a folder source: sub-folder; a file: its full path)",
        ))
        .param(ParamSpec::new("overwrite", K::Boolean, "replace files that already exist"))
        .destructive(),
        install,
    );
}

fn game_folder(ctx: &Ctx) -> Result<PathBuf, CmdError> {
    ctx.session()
        .config
        .game_folder
        .clone()
        .filter(|p| p.is_dir())
        .ok_or_else(|| CmdError::state("no game folder is set; run `game.open` first"))
}

fn list(ctx: &Ctx, _: Value) -> CmdResult {
    let game = game_folder(ctx)?;
    let files = mods::list(&game)?;
    Ok(
        json!({ "game": game, "mods_dir": mods::mods_dir(&game), "exists": mods::mods_dir(&game).is_dir(), "files": files }),
    )
}

#[derive(Deserialize)]
struct CopyArgs {
    path: String,
    overwrite: Option<bool>,
}

fn copy(ctx: &Ctx, args: Value) -> CmdResult {
    let a: CopyArgs = parse(args)?;
    let game = game_folder(ctx)?;
    let tree = ctx.tree()?;
    let r = tree.find(&a.path).ok_or_else(|| CmdError::not_found(format!("no entry `{}`", a.path)))?;
    // climb to the archive that lives on disk
    let mut id = r.archive;
    while let Some(p) = tree.archive(id).parent {
        id = p;
    }
    let top = tree.archive(id);
    // the archive path is relative to the scanned folder: it must be the game folder
    let on_disk = game.join(&top.path);
    let same = match (std::fs::canonicalize(&on_disk), std::fs::canonicalize(&top.physical)) {
        (Ok(a), Ok(b)) => a == b,
        _ => false, // two paths that cannot be resolved are not "the same file"
    };
    if !same {
        return Err(CmdError::invalid(format!(
            "{} is not inside the game folder, so it has no mods/ counterpart",
            top.name
        )));
    }
    let rel = original_case_path(&game, &top.path);
    let done = mods::copy_to_mods(&game, &rel, a.overwrite.unwrap_or(false))?;
    Ok(json!({ "copied": done, "to": mods::mods_dir(&game).join(&done) }))
}

/// Archive paths are lower-cased internally; recover the real spelling on disk.
fn original_case_path(game: &std::path::Path, lower: &str) -> String {
    let mut cur = game.to_path_buf();
    let mut out = Vec::new();
    for part in lower.split('/') {
        let real = std::fs::read_dir(&cur)
            .ok()
            .and_then(|rd| {
                rd.flatten()
                    .map(|e| e.file_name().to_string_lossy().into_owned())
                    .find(|n| n.to_lowercase() == part)
            })
            .unwrap_or_else(|| part.to_string());
        cur.push(&real);
        out.push(real);
    }
    out.join("/")
}

#[derive(Deserialize)]
struct PathArg {
    path: String,
}

fn set_enabled(ctx: &Ctx, args: Value, enabled: bool) -> CmdResult {
    let a: PathArg = parse(args)?;
    mods::set_enabled(&game_folder(ctx)?, &a.path, enabled)?;
    Ok(json!({ "path": a.path, "enabled": enabled }))
}

fn remove(ctx: &Ctx, args: Value) -> CmdResult {
    let a: PathArg = parse(args)?;
    mods::remove(&game_folder(ctx)?, &a.path)?;
    Ok(json!({ "removed": a.path }))
}

#[derive(Deserialize)]
struct InstallArgs {
    source: PathBuf,
    dest: Option<String>,
    overwrite: Option<bool>,
}

fn install(ctx: &Ctx, args: Value) -> CmdResult {
    let a: InstallArgs = parse(args)?;
    let done = mods::install(&game_folder(ctx)?, &a.source, a.dest.as_deref(), a.overwrite.unwrap_or(false))?;
    Ok(json!({ "installed": done.len(), "files": done }))
}
