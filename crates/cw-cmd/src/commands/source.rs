//! Where a command reads a file from: an entry of the open archives, or a file on disk.

use std::path::PathBuf;

use cw_core::{resource, rpf::ResourceData};
use serde::Deserialize;

use crate::{engine::Ctx, error::CmdError};

#[derive(Deserialize)]
pub struct Source {
    pub path: Option<String>,
    pub file: Option<PathBuf>,
}

pub enum Loaded {
    /// A resource (`.ytd`, `.ymap`, ...): flags and decompressed payload.
    Resource(ResourceData),
    /// Anything else, as stored.
    Plain(Vec<u8>),
}

/// Reads the file `src` names, and its file name. Exactly one of `path` and `file` must be given.
pub fn read(ctx: &Ctx, src: &Source) -> Result<(Loaded, String), CmdError> {
    match (&src.path, &src.file) {
        (Some(p), None) => {
            let tree = ctx.tree()?;
            let r = tree.find(p).ok_or_else(|| CmdError::not_found(format!("no entry `{p}`")))?;
            let e = tree.entry(r);
            if e.is_dir() || e.child_archive.is_some() {
                return Err(CmdError::invalid(format!("`{p}` is a folder or an archive, not a file")));
            }
            let loaded = if e.is_resource() {
                Loaded::Resource(tree.extract_resource(r)?)
            } else {
                Loaded::Plain(tree.extract(r)?)
            };
            Ok((loaded, e.name.clone()))
        }
        (None, Some(f)) => {
            let raw = std::fs::read(f)?;
            let name = f.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
            let loaded = if resource::has_rsc7_header(&raw) {
                Loaded::Resource(resource::parse_loose(&raw)?)
            } else {
                Loaded::Plain(raw)
            };
            Ok((loaded, name))
        }
        _ => Err(CmdError::invalid("give exactly one of `path` or `file`")),
    }
}
