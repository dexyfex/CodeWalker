//! Loose files on disk (not inside an archive): `.ytd`, `.ydr`, `.xml`, `.dds`…

use std::{
    io::{Read, Seek, SeekFrom},
    path::PathBuf,
};

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
/// Largest resource file that is decompressed to answer a read.
const MAX_RESOURCE_FILE: u64 = 256 * 1024 * 1024;

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

fn read(ctx: &Ctx, args: Value) -> CmdResult {
    let a: ReadArgs = parse(args)?;
    // only what was opened on purpose: this command is also reachable by remote callers
    if !ctx.session().files.contains(&a.path) {
        return Err(CmdError::invalid(format!(
            "{} is not open; open it with `file.open` first",
            a.path.display()
        )));
    }
    let mut f = std::fs::File::open(&a.path)?;
    let size = f.metadata()?.len();
    let mut head = [0u8; 4];
    let head_len = f.read(&mut head)?;
    f.seek(SeekFrom::Start(0))?;

    let (total, offset, bytes) = if resource::has_rsc7_header(&head[..head_len]) {
        // resources are deflated: the whole file is needed, up to a sane size
        if size > MAX_RESOURCE_FILE {
            return Err(CmdError::invalid(format!("{} is too large to decompress here", a.path.display())));
        }
        let mut raw = Vec::with_capacity(size as usize);
        f.read_to_end(&mut raw)?;
        let data = resource::parse_loose(&raw)?.data;
        let offset = a.offset.unwrap_or(0).min(data.len());
        let len = a.length.unwrap_or(4096).min(MAX_READ).min(data.len() - offset);
        (data.len() as u64, offset, data[offset..offset + len].to_vec())
    } else {
        // anything else is read where it lies, never the whole file
        let offset = (a.offset.unwrap_or(0) as u64).min(size);
        let len = (a.length.unwrap_or(4096).min(MAX_READ) as u64).min(size - offset);
        f.seek(SeekFrom::Start(offset))?;
        let mut buf = Vec::with_capacity(len as usize);
        f.take(len).read_to_end(&mut buf)?;
        (size, offset as usize, buf)
    };
    Ok(json!({
        "total": total,
        "offset": offset,
        "length": bytes.len(),
        "base64": base64::engine::general_purpose::STANDARD.encode(&bytes),
        "text": std::str::from_utf8(&bytes).ok(),
    }))
}
