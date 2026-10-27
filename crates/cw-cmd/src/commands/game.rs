use std::{path::PathBuf, sync::Arc};

use cw_core::{GtaKeys, RpfTree};
use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine, Event},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new(
            "keys.load",
            "game",
            "Derive the archive keys from GTA5.exe or from a base64 AES key",
        )
        .param(ParamSpec::new("exe", K::Path, "path to GTA5.exe (default: inside the game folder)"))
        .param(ParamSpec::new("key", K::String, "32-byte AES key, base64 (alternative to `exe`)"))
        .mutating(),
        keys_load,
    );
    e.register(
        CommandSpec::new("game.open", "game", "Scan a GTA V folder and open every archive in it")
            .param(ParamSpec::new("folder", K::Path, "game folder (default: the saved one)"))
            .param(ParamSpec::new("key", K::String, "base64 AES key instead of reading the executable"))
            .param(ParamSpec::new("enhanced", K::Boolean, "use gta5_enhanced.exe (Enhanced Edition)"))
            .mutating(),
        game_open,
    );
    e.register(
        CommandSpec::new("rpf.open", "game", "Open a single .rpf file, or a folder of them")
            .param(ParamSpec::new("path", K::Path, "file or folder").required())
            .param(ParamSpec::new("add", K::Boolean, "keep what is already open and add this to it"))
            .mutating(),
        rpf_open,
    );
}

fn load_keys(ctx: &Ctx, exe: Option<PathBuf>, key: Option<String>) -> Result<Arc<GtaKeys>, CmdError> {
    ctx.emit(Event::Progress { task: "keys".into(), message: "Deriving archive keys…".into() });
    let keys = match (key, exe) {
        (Some(k), _) => GtaKeys::from_base64_key(&k)?,
        (None, Some(exe)) => GtaKeys::from_exe_path(&exe)?,
        (None, None) => return Err(CmdError::invalid("pass `exe` or `key`")),
    };
    let keys = Arc::new(keys);
    ctx.session_mut().keys = Some(keys.clone());
    ctx.emit(Event::KeysChanged);
    Ok(keys)
}

fn exe_in(folder: &std::path::Path, enhanced: bool) -> PathBuf {
    folder.join(if enhanced { "GTA5_Enhanced.exe" } else { "GTA5.exe" })
}

fn find_exe(folder: &std::path::Path, enhanced: bool) -> Option<PathBuf> {
    let want = exe_in(folder, enhanced);
    let name = want.file_name()?.to_string_lossy().to_lowercase();
    std::fs::read_dir(folder)
        .ok()?
        .flatten()
        .find(|e| e.file_name().to_string_lossy().to_lowercase() == name)
        .map(|e| e.path())
}

#[derive(Deserialize)]
struct KeysArgs {
    exe: Option<PathBuf>,
    key: Option<String>,
}

fn keys_load(ctx: &Ctx, args: Value) -> CmdResult {
    let a: KeysArgs = parse(args)?;
    let exe = a.exe.or_else(|| {
        let s = ctx.session();
        s.config.game_folder.as_deref().and_then(|f| find_exe(f, s.config.enhanced_edition))
    });
    load_keys(ctx, exe, a.key)?;
    Ok(json!({ "loaded": true }))
}

#[derive(Deserialize)]
struct OpenArgs {
    folder: Option<PathBuf>,
    key: Option<String>,
    enhanced: Option<bool>,
}

fn game_open(ctx: &Ctx, args: Value) -> CmdResult {
    let a: OpenArgs = parse(args)?;
    let (folder, enhanced) = {
        let s = ctx.session();
        (a.folder.or_else(|| s.config.game_folder.clone()), a.enhanced.unwrap_or(s.config.enhanced_edition))
    };
    let folder = folder.ok_or_else(|| CmdError::invalid("no game folder given and none saved"))?;
    if !folder.is_dir() {
        return Err(CmdError::not_found(format!("{} is not a folder", folder.display())));
    }

    // Keys are optional: unencrypted ("OPEN") archives still work without them.
    let mut key_warning = None;
    let keys = match (a.key, find_exe(&folder, enhanced)) {
        (Some(k), _) => Some(load_keys(ctx, None, Some(k))?),
        (None, Some(exe)) => match load_keys(ctx, Some(exe), None) {
            Ok(k) => Some(k),
            Err(e) => {
                key_warning = Some(e.message);
                None
            }
        },
        (None, None) => {
            key_warning = Some(format!(
                "{} not found; encrypted archives will be skipped",
                exe_in(&folder, enhanced).display()
            ));
            None
        }
    };

    let tree = scan(ctx, &folder, keys)?;
    let summary = summarize(&tree, key_warning);
    {
        let mut s = ctx.session_mut();
        s.tree = Some(Arc::new(tree));
        s.root = Some(folder.clone());
        s.config.game_folder = Some(folder.clone());
        s.config.enhanced_edition = enhanced;
        push_recent(&mut s.config.recent, folder);
    }
    let _ = ctx.save_config();
    ctx.emit(Event::TreeChanged);
    Ok(summary)
}

#[derive(Deserialize)]
struct OpenPathArgs {
    path: PathBuf,
    add: Option<bool>,
}

fn rpf_open(ctx: &Ctx, args: Value) -> CmdResult {
    let a: OpenPathArgs = parse(args)?;
    let keys = ctx.session().keys.clone();
    let tree = if a.path.is_dir() {
        scan(ctx, &a.path, keys)?
    } else if a.path.is_file() {
        let name = a.path.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
        RpfTree::scan_file(&a.path, &name, keys)?
    } else {
        return Err(CmdError::not_found(format!("{} does not exist", a.path.display())));
    };
    let summary = summarize(&tree, None);
    {
        let mut s = ctx.session_mut();
        s.tree = Some(Arc::new(match (a.add.unwrap_or(false), s.tree.take()) {
            (true, Some(existing)) => existing.merged(tree),
            _ => tree,
        }));
        if !a.add.unwrap_or(false) {
            s.root = a.path.is_dir().then(|| a.path.clone());
        }
        push_recent(&mut s.config.recent, a.path);
    }
    let _ = ctx.save_config();
    ctx.emit(Event::TreeChanged);
    Ok(summary)
}

fn scan(ctx: &Ctx, folder: &std::path::Path, keys: Option<Arc<GtaKeys>>) -> Result<RpfTree, CmdError> {
    let progress =
        |rel: &str| ctx.emit(Event::Progress { task: "scan".into(), message: format!("Scanning {rel}") });
    let tree = RpfTree::scan_folder(folder, keys, &[], &progress);
    if tree.archives.is_empty() && !tree.errors.is_empty() {
        return Err(CmdError::new(
            "archive",
            format!("{} archives failed to open; first: {}", tree.errors.len(), tree.errors[0].message),
        ));
    }
    Ok(tree)
}

fn summarize(tree: &RpfTree, key_warning: Option<String>) -> Value {
    json!({
        "archives": tree.archives.len(),
        "top_level": tree.roots.len(),
        "entries": tree.total_entries(),
        "errors": tree.errors.iter().take(20).map(|e| json!({ "path": e.path, "message": e.message })).collect::<Vec<_>>(),
        "error_count": tree.errors.len(),
        "key_warning": key_warning,
    })
}

fn push_recent(recent: &mut Vec<PathBuf>, p: PathBuf) {
    recent.retain(|r| r != &p);
    recent.insert(0, p);
    recent.truncate(10);
}
