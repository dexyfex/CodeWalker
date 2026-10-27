//! Loose files on disk (not inside an archive): `.ytd`, `.ydr`, `.xml`, `.dds`…

use std::path::PathBuf;

use base64::Engine as _;
use cw_core::{filekind::FileKind, resource};
use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine, Event},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

const MAX_READ: usize = 16 * 1024 * 1024;

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new("file.open", "files", "Open a loose file next to the archives")
            .param(ParamSpec::new("path", K::Path, "file on disk").required())
            .mutating(),
        open,
    );
    e.register(
        CommandSpec::new("file.close", "files", "Close a loose file")
            .param(ParamSpec::new("path", K::Path, "file on disk").required())
            .mutating(),
        close,
    );
    e.register(
        CommandSpec::new("file.read", "files", "Read bytes of a loose file (resources are decompressed)")
            .param(ParamSpec::new("path", K::Path, "file on disk").required())
            .param(ParamSpec::new("offset", K::Integer, "byte offset (default 0)"))
            .param(ParamSpec::new("length", K::Integer, "bytes to read (default 4096, max 16 MiB)")),
        read,
    );
}

#[derive(Deserialize)]
struct PathArg {
    path: PathBuf,
}

fn open(ctx: &Ctx, args: Value) -> CmdResult {
    let a: PathArg = parse(args)?;
    let meta = std::fs::metadata(&a.path)
        .map_err(|_| CmdError::not_found(format!("{} does not exist", a.path.display())))?;
    if !meta.is_file() {
        return Err(CmdError::invalid(format!("{} is not a file", a.path.display())));
    }
    let name = a.path.file_name().map(|n| n.to_string_lossy().to_lowercase()).unwrap_or_default();
    let kind = FileKind::from_name(&name);
    {
        let mut s = ctx.session_mut();
        if !s.files.contains(&a.path) {
            s.files.push(a.path.clone());
        }
    }
    ctx.emit(Event::FilesChanged);
    Ok(json!({ "path": a.path, "type": kind.label(), "size": meta.len(), "resource": kind.is_resource() }))
}

fn close(ctx: &Ctx, args: Value) -> CmdResult {
    let a: PathArg = parse(args)?;
    ctx.session_mut().files.retain(|p| p != &a.path);
    ctx.emit(Event::FilesChanged);
    Ok(json!({ "closed": true }))
}

#[derive(Deserialize)]
struct ReadArgs {
    path: PathBuf,
    offset: Option<usize>,
    length: Option<usize>,
}

fn read(_: &Ctx, args: Value) -> CmdResult {
    let a: ReadArgs = parse(args)?;
    let raw = std::fs::read(&a.path)?;
    let data = if resource::has_rsc7_header(&raw) { resource::parse_loose(&raw)?.data } else { raw };
    let offset = a.offset.unwrap_or(0).min(data.len());
    let len = a.length.unwrap_or(4096).min(MAX_READ).min(data.len() - offset);
    let slice = &data[offset..offset + len];
    Ok(json!({
        "total": data.len(),
        "offset": offset,
        "length": len,
        "base64": base64::engine::general_purpose::STANDARD.encode(slice),
        "text": std::str::from_utf8(slice).ok(),
    }))
}
