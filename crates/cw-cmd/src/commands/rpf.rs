use std::path::{Path, PathBuf};

use base64::Engine as _;
use cw_core::{EntryKind, EntryRef, RpfTree, filekind::FileKind};
use rayon::prelude::*;
use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

const MAX_READ: usize = 16 * 1024 * 1024;

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new(
            "rpf.list",
            "archive",
            "List the entries of a directory or archive (empty path: top-level archives)",
        )
        .param(ParamSpec::new("path", K::String, "archive path such as `x64a.rpf/levels`"))
        .param(ParamSpec::new("offset", K::Integer, "first row (default 0)"))
        .param(ParamSpec::new("limit", K::Integer, "max rows (default 1000)")),
        list,
    );
    e.register(
        CommandSpec::new("rpf.info", "archive", "Details of one entry")
            .param(ParamSpec::new("path", K::String, "entry path").required()),
        info,
    );
    e.register(
        CommandSpec::new("rpf.search", "archive", "Find files whose path contains the text")
            .param(ParamSpec::new("query", K::String, "case-insensitive text").required())
            .param(ParamSpec::new("limit", K::Integer, "max results (default 200)")),
        search,
    );
    e.register(
        CommandSpec::new(
            "rpf.read",
            "archive",
            "Read bytes of a file (decompressed) as base64, plus text when it is UTF-8",
        )
        .param(ParamSpec::new("path", K::String, "entry path").required())
        .param(ParamSpec::new("offset", K::Integer, "byte offset (default 0)"))
        .param(ParamSpec::new("length", K::Integer, "bytes to read (default 4096, max 16 MiB)")),
        read,
    );
    e.register(
        CommandSpec::new("rpf.extract", "archive", "Write a file or a whole directory to disk")
            .param(ParamSpec::new("path", K::String, "entry path").required())
            .param(
                ParamSpec::new("dest", K::Path, "destination folder (or file name for a single file)")
                    .required(),
            )
            .mutating(),
        extract,
    );
    e.register(CommandSpec::new("rpf.stats", "archive", "Counts for the open archives"), stats);
}

fn resolve(tree: &RpfTree, path: &str) -> Result<EntryRef, CmdError> {
    tree.find(path).ok_or_else(|| CmdError::not_found(format!("no entry `{path}`")))
}

pub fn entry_json(tree: &RpfTree, r: EntryRef) -> Value {
    let a = tree.archive(r.archive);
    let e = tree.entry(r);
    let (kind, compressed, encrypted) = match e.kind {
        EntryKind::Directory { .. } => ("directory", false, false),
        EntryKind::Binary { size, encrypted, .. } => {
            (if e.child_archive.is_some() { "archive" } else { "file" }, size > 0, encrypted)
        }
        EntryKind::Resource { encrypted, .. } => ("resource", true, encrypted),
    };
    let children = match (&e.kind, e.child_archive) {
        (EntryKind::Directory { count, .. }, _) => Some(*count as u64),
        (_, Some(c)) => match tree.archive(c).root().kind {
            EntryKind::Directory { count, .. } => Some(count as u64),
            _ => None,
        },
        _ => None,
    };
    let mut v = json!({
        "name": e.name,
        "path": e.path,
        "kind": kind,
        "type": FileKind::from_name(&e.name_lower).label(),
        "size": e.display_size(),
        "stored_size": e.stored_size(),
        "compressed": compressed,
        "encrypted": encrypted,
        "archive": a.name,
        "children": children,
    });
    if let EntryKind::Resource { sys_flags, gfx_flags, .. } = e.kind {
        v["resource_version"] = json!(cw_core::rpf::resource_version(sys_flags, gfx_flags));
    }
    v
}

#[derive(Deserialize, Default)]
struct ListArgs {
    path: Option<String>,
    offset: Option<usize>,
    limit: Option<usize>,
}

fn list(ctx: &Ctx, args: Value) -> CmdResult {
    let a: ListArgs = parse(args)?;
    let tree = ctx.tree()?;
    let offset = a.offset.unwrap_or(0);
    let limit = a.limit.unwrap_or(1000).min(100_000);

    let mut refs: Vec<EntryRef> = match a.path.as_deref().map(str::trim).filter(|p| !p.is_empty()) {
        None => tree.roots.iter().map(|&id| EntryRef { archive: id, index: 0 }).collect(),
        Some(p) => {
            let r = resolve(&tree, p)?;
            let (archive, dir) = match (&tree.entry(r).kind, tree.entry(r).child_archive) {
                (_, Some(child)) => (child, 0),
                (EntryKind::Directory { .. }, None) => (r.archive, r.index),
                _ => return Err(CmdError::invalid(format!("`{p}` is not a directory or archive"))),
            };
            let a = tree.archive(archive);
            let first = match a.entries[dir as usize].kind {
                EntryKind::Directory { first, .. } => first,
                _ => 0,
            };
            (0..a.dir_children(dir).len() as u32).map(|i| EntryRef { archive, index: first + i }).collect()
        }
    };
    // folders first, then by name
    refs.sort_by(|&x, &y| {
        let (ex, ey) = (tree.entry(x), tree.entry(y));
        let dx = ex.is_dir() || ex.child_archive.is_some();
        let dy = ey.is_dir() || ey.child_archive.is_some();
        dy.cmp(&dx).then_with(|| cw_core::text::natural_cmp(&ex.name_lower, &ey.name_lower))
    });
    let total = refs.len();
    let rows: Vec<Value> = refs.into_iter().skip(offset).take(limit).map(|r| entry_json(&tree, r)).collect();
    Ok(json!({ "total": total, "offset": offset, "entries": rows }))
}

#[derive(Deserialize)]
struct PathArg {
    path: String,
}

fn info(ctx: &Ctx, args: Value) -> CmdResult {
    let a: PathArg = parse(args)?;
    let tree = ctx.tree()?;
    let r = resolve(&tree, &a.path)?;
    Ok(entry_json(&tree, r))
}

#[derive(Deserialize)]
struct SearchArgs {
    query: String,
    limit: Option<usize>,
}

fn search(ctx: &Ctx, args: Value) -> CmdResult {
    let a: SearchArgs = parse(args)?;
    if a.query.trim().is_empty() {
        return Err(CmdError::invalid("query is empty"));
    }
    let tree = ctx.tree()?;
    let limit = a.limit.unwrap_or(200).min(10_000);
    let mut hits: Vec<EntryRef> = tree.search(&a.query).collect();
    hits.sort();
    let total = hits.len();
    let rows: Vec<Value> = hits.into_iter().take(limit).map(|r| entry_json(&tree, r)).collect();
    Ok(json!({ "total": total, "entries": rows }))
}

#[derive(Deserialize)]
struct ReadArgs {
    path: String,
    offset: Option<usize>,
    length: Option<usize>,
}

fn read(ctx: &Ctx, args: Value) -> CmdResult {
    let a: ReadArgs = parse(args)?;
    let tree = ctx.tree()?;
    let r = resolve(&tree, &a.path)?;
    let data = tree.extract(r)?;
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

#[derive(Deserialize)]
struct ExtractArgs {
    path: String,
    dest: PathBuf,
}

fn extract(ctx: &Ctx, args: Value) -> CmdResult {
    let a: ExtractArgs = parse(args)?;
    let tree = ctx.tree()?;
    let r = resolve(&tree, &a.path)?;
    let e = tree.entry(r);

    // A directory (or nested archive) becomes a folder tree; a file is one write.
    let dir_target = match (&e.kind, e.child_archive) {
        (EntryKind::Directory { .. }, _) => Some((r.archive, r.index)),
        (_, Some(child)) => Some((child, 0)),
        _ => None,
    };
    let Some((archive, dir)) = dir_target else {
        let out = if a.dest.is_dir() { a.dest.join(safe_component(&e.name)) } else { a.dest.clone() };
        let bytes = tree.export(r)?;
        write_file(&out, &bytes)?;
        return Ok(json!({ "files": 1, "bytes": bytes.len() }));
    };

    let mut files = Vec::new();
    collect_files(&tree, archive, dir, PathBuf::new(), &mut files);
    let root = a.dest.join(safe_component(&e.name));
    let results: Vec<Result<usize, CmdError>> = files
        .par_iter()
        .map(|(rel, fr)| {
            let bytes = tree.export(*fr)?;
            write_file(&root.join(rel), &bytes)?;
            Ok(bytes.len())
        })
        .collect();
    let failed: Vec<String> =
        results.iter().filter_map(|r| r.as_ref().err().map(|e| e.message.clone())).collect();
    let bytes: usize = results.iter().filter_map(|r| r.as_ref().ok()).sum();
    Ok(json!({
        "files": results.len() - failed.len(),
        "bytes": bytes,
        "failed": failed.len(),
        "first_error": failed.first(),
        "dest": root,
    }))
}

fn collect_files(tree: &RpfTree, archive: u32, dir: u32, rel: PathBuf, out: &mut Vec<(PathBuf, EntryRef)>) {
    let a = tree.archive(archive);
    let first = match a.entries[dir as usize].kind {
        EntryKind::Directory { first, .. } => first,
        _ => return,
    };
    for (i, e) in a.dir_children(dir).iter().enumerate() {
        let r = EntryRef { archive, index: first + i as u32 };
        let sub = rel.join(safe_component(&e.name));
        match (&e.kind, e.child_archive) {
            (EntryKind::Directory { .. }, _) => collect_files(tree, archive, r.index, sub, out),
            (_, Some(child)) => collect_files(tree, child, 0, sub, out),
            _ => out.push((sub, r)),
        }
    }
}

/// Archive names are untrusted: strip separators and drive/parent tricks so
/// extraction can never write outside the chosen folder.
pub fn safe_component(name: &str) -> String {
    let s: String = name
        .chars()
        .map(|c| {
            if matches!(c, '/' | '\\' | ':' | '*' | '?' | '"' | '<' | '>' | '|') || c.is_control() {
                '_'
            } else {
                c
            }
        })
        .collect();
    let s = s.trim().trim_end_matches('.').to_string();
    if s.is_empty() || s == ".." { "_".to_string() } else { s }
}

fn write_file(path: &Path, bytes: &[u8]) -> Result<(), CmdError> {
    if let Some(dir) = path.parent() {
        std::fs::create_dir_all(dir)?;
    }
    std::fs::write(path, bytes)?;
    Ok(())
}

fn stats(ctx: &Ctx, _args: Value) -> CmdResult {
    let tree = ctx.tree()?;
    let (mut files, mut dirs, mut resources, mut encrypted) = (0u64, 0u64, 0u64, 0u64);
    for a in &tree.archives {
        for e in &a.entries {
            match e.kind {
                EntryKind::Directory { .. } => dirs += 1,
                EntryKind::Binary { encrypted: enc, .. } => {
                    files += 1;
                    encrypted += enc as u64;
                }
                EntryKind::Resource { encrypted: enc, .. } => {
                    files += 1;
                    resources += 1;
                    encrypted += enc as u64;
                }
            }
        }
    }
    Ok(json!({
        "archives": tree.archives.len(),
        "top_level": tree.roots.len(),
        "directories": dirs,
        "files": files,
        "resources": resources,
        "encrypted_files": encrypted,
        "scan_errors": tree.errors.len(),
    }))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn safe_component_blocks_traversal() {
        assert_eq!(safe_component("../../etc/passwd"), ".._.._etc_passwd");
        assert_eq!(safe_component(".."), "_");
        assert_eq!(safe_component("C:\\evil"), "C__evil");
        assert_eq!(safe_component(""), "_");
        assert_eq!(safe_component("adder.yft"), "adder.yft");
    }
}
