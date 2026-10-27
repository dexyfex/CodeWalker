//! The `mods/` overlay.
//!
//! GTA V (with OpenIV's `OpenIV.asi`, or CodeWalker's own edit mode) reads
//! `<game>/mods/<same relative path>` in preference to the original file.
//! Managing mods is therefore managing that folder: copy an archive in to
//! make an editable copy, switch overrides on and off, remove them.
//!
//! "Off" is a `.disabled` suffix. The loader does not look for such files, so
//! the game falls back to the original, and nothing is deleted.

use std::{
    fs, io,
    path::{Component, Path, PathBuf},
};

use serde::Serialize;

pub const MODS_DIR: &str = "mods";
const DISABLED: &str = ".disabled";

#[derive(Debug, thiserror::Error)]
pub enum ModError {
    #[error("{0}")]
    Invalid(String),
    #[error("not found: {0}")]
    NotFound(String),
    #[error("already exists: {0}")]
    Exists(String),
    #[error(transparent)]
    Io(#[from] io::Error),
}

#[derive(Debug, Clone, Serialize, PartialEq, Eq)]
pub struct ModFile {
    /// Path inside `mods/`, `/` separated, without the `.disabled` suffix.
    pub path: String,
    pub size: u64,
    pub enabled: bool,
    pub archive: bool,
    /// The game folder holds an original at the same relative path.
    pub overrides_game: bool,
}

pub fn mods_dir(game: &Path) -> PathBuf {
    game.join(MODS_DIR)
}

/// Validates a user/archive supplied relative path: no absolute paths, drive
/// letters, `..` or NULs. Returns it normalised to components.
pub fn safe_rel(rel: &str) -> Result<PathBuf, ModError> {
    let norm = rel.replace('\\', "/");
    let trimmed = norm.trim_matches('/');
    if trimmed.is_empty() || trimmed.contains('\0') {
        return Err(ModError::Invalid("empty or invalid path".into()));
    }
    let mut out = PathBuf::new();
    for part in trimmed.split('/') {
        if part.is_empty() || part == "." || part == ".." || part.contains(':') {
            return Err(ModError::Invalid(format!("unsafe path component `{part}` in `{rel}`")));
        }
        out.push(part);
    }
    if out.components().any(|c| !matches!(c, Component::Normal(_))) {
        return Err(ModError::Invalid(format!("unsafe path `{rel}`")));
    }
    Ok(out)
}

fn rel_string(p: &Path) -> String {
    p.components().map(|c| c.as_os_str().to_string_lossy()).collect::<Vec<_>>().join("/")
}

/// Every regular file under `mods/`. Symlinks are listed as files but never followed.
pub fn list(game: &Path) -> Result<Vec<ModFile>, ModError> {
    let root = mods_dir(game);
    let mut out = Vec::new();
    if root.is_dir() {
        walk(&root, &root, game, &mut out)?;
    }
    out.sort_by(|a, b| cw_core::text::natural_cmp(&a.path, &b.path));
    Ok(out)
}

fn walk(root: &Path, dir: &Path, game: &Path, out: &mut Vec<ModFile>) -> Result<(), ModError> {
    for entry in fs::read_dir(dir)?.flatten() {
        let meta = entry.metadata()?; // does not follow symlinks for file_type, but size is of the link target
        let ft = entry.file_type()?;
        let path = entry.path();
        if ft.is_dir() {
            walk(root, &path, game, out)?;
        } else if ft.is_file() || ft.is_symlink() {
            let rel =
                path.strip_prefix(root).map_err(|_| ModError::Invalid("path left the mods folder".into()))?;
            let rel = rel_string(rel);
            if rel.ends_with(PARTIAL) {
                continue; // left by an interrupted copy; not a mod
            }
            let (name, enabled) = match rel.strip_suffix(DISABLED) {
                Some(n) => (n.to_string(), false),
                None => (rel.clone(), true),
            };
            let archive = name.to_lowercase().ends_with(".rpf");
            let overrides_game = safe_rel(&name).map(|p| game.join(p).is_file()).unwrap_or(false);
            out.push(ModFile { path: name, size: meta.len(), enabled, archive, overrides_game });
        }
    }
    Ok(())
}

/// Resolves `rel` inside `mods/` and checks the result really is inside it
/// (a symlinked directory must not smuggle writes out of the folder).
fn inside_mods(game: &Path, rel: &Path) -> Result<PathBuf, ModError> {
    let root = mods_dir(game);
    let full = root.join(rel);
    if let (Ok(r), Some(parent)) = (root.canonicalize(), full.parent())
        && let Ok(p) = parent.canonicalize()
        && !p.starts_with(&r)
    {
        return Err(ModError::Invalid(format!("`{}` resolves outside the mods folder", rel.display())));
    }
    Ok(full)
}

/// Copies an original game file into `mods/` at the same relative path.
pub fn copy_to_mods(game: &Path, rel: &str, overwrite: bool) -> Result<String, ModError> {
    let rel_path = safe_rel(rel)?;
    let src = game.join(&rel_path);
    if !src.is_file() {
        return Err(ModError::NotFound(format!("{rel} is not in the game folder")));
    }
    if rel_path.starts_with(MODS_DIR) {
        return Err(ModError::Invalid("that file is already in the mods folder".into()));
    }
    let dst = inside_mods(game, &rel_path)?;
    let disabled = with_suffix(&dst);
    if (dst.exists() || disabled.exists()) && !overwrite {
        return Err(ModError::Exists(rel_string(&rel_path)));
    }
    if let Some(dir) = dst.parent() {
        fs::create_dir_all(dir)?;
    }
    copy_atomic(&src, &dst)?;
    if disabled.exists() {
        fs::remove_file(disabled)?;
    }
    Ok(rel_string(&rel_path))
}

/// Suffix of the sibling a copy is written to before it is renamed into place.
const PARTIAL: &str = ".cwtmp";

/// Copies through a temporary sibling, so a failed or interrupted copy never
/// leaves a truncated archive where the game would load it.
fn copy_atomic(src: &Path, dst: &Path) -> std::io::Result<()> {
    let mut tmp = dst.as_os_str().to_owned();
    tmp.push(PARTIAL);
    let tmp = PathBuf::from(tmp);
    let done = fs::copy(src, &tmp).and_then(|_| fs::rename(&tmp, dst));
    if done.is_err() {
        let _ = fs::remove_file(&tmp);
    }
    done
}

fn with_suffix(p: &Path) -> PathBuf {
    let mut s = p.as_os_str().to_owned();
    s.push(DISABLED);
    PathBuf::from(s)
}

/// Turns an override on or off by adding/removing the `.disabled` suffix.
pub fn set_enabled(game: &Path, rel: &str, enabled: bool) -> Result<(), ModError> {
    let rel_path = safe_rel(rel)?;
    let on = inside_mods(game, &rel_path)?;
    let off = with_suffix(&on);
    let (from, to) = if enabled { (off, on) } else { (on, off) };
    if !from.is_file() {
        // already in the requested state?
        return if to.is_file() { Ok(()) } else { Err(ModError::NotFound(rel.to_string())) };
    }
    if to.exists() {
        return Err(ModError::Exists(rel_string(&rel_path)));
    }
    fs::rename(from, to)?;
    Ok(())
}

pub fn remove(game: &Path, rel: &str) -> Result<(), ModError> {
    let rel_path = safe_rel(rel)?;
    let on = inside_mods(game, &rel_path)?;
    let off = with_suffix(&on);
    let target = if on.is_file() {
        on
    } else if off.is_file() {
        off
    } else {
        return Err(ModError::NotFound(rel.to_string()));
    };
    fs::remove_file(target)?;
    // tidy empty folders left behind, but never the mods folder itself
    let mut dir = rel_path.parent().map(|p| mods_dir(game).join(p));
    while let Some(d) = dir {
        if d == mods_dir(game) || fs::remove_dir(&d).is_err() {
            break;
        }
        dir = d.parent().map(Path::to_path_buf);
    }
    Ok(())
}

/// Copies a file or a folder tree into `mods/`. A folder keeps its internal
/// layout (so a package laid out like the game folder lands correctly);
/// `dest` is an optional sub-path inside `mods/`. Existing files are never
/// replaced unless `overwrite` is set. Returns the installed paths.
pub fn install(
    game: &Path,
    source: &Path,
    dest: Option<&str>,
    overwrite: bool,
) -> Result<Vec<String>, ModError> {
    let base = match dest.filter(|d| !d.trim().is_empty()) {
        Some(d) => safe_rel(d)?,
        None => PathBuf::new(),
    };
    let mut plan: Vec<(PathBuf, PathBuf)> = Vec::new(); // (source file, relative destination)
    let meta = fs::symlink_metadata(source).map_err(|_| ModError::NotFound(source.display().to_string()))?;
    if meta.is_file() {
        let name = source.file_name().ok_or_else(|| ModError::Invalid("source has no file name".into()))?;
        // with an explicit `dest`, a single file is installed *at* that path
        let rel = if dest.is_some_and(|d| !d.trim().is_empty()) { base.clone() } else { base.join(name) };
        plan.push((source.to_path_buf(), rel));
    } else if meta.is_dir() {
        collect(source, source, &base, &mut plan)?;
    } else {
        return Err(ModError::Invalid("only regular files and folders can be installed".into()));
    }
    if plan.is_empty() {
        return Err(ModError::Invalid("nothing to install".into()));
    }
    // validate everything first so a conflict does not leave a half-installed mod
    for (_, rel) in &plan {
        safe_rel(&rel_string(rel))?;
        let dst = inside_mods(game, rel)?;
        if !overwrite && (dst.exists() || with_suffix(&dst).exists()) {
            return Err(ModError::Exists(rel_string(rel)));
        }
    }
    let mut done = Vec::new();
    for (src, rel) in plan {
        let dst = inside_mods(game, &rel)?;
        if let Some(dir) = dst.parent() {
            fs::create_dir_all(dir)?;
        }
        copy_atomic(&src, &dst)?;
        let off = with_suffix(&dst);
        if off.exists() {
            fs::remove_file(off)?;
        }
        done.push(rel_string(&rel));
    }
    Ok(done)
}

fn collect(root: &Path, dir: &Path, base: &Path, out: &mut Vec<(PathBuf, PathBuf)>) -> Result<(), ModError> {
    for e in fs::read_dir(dir)?.flatten() {
        let ft = e.file_type()?;
        let p = e.path();
        if ft.is_dir() {
            collect(root, &p, base, out)?;
        } else if ft.is_file() {
            let rel =
                p.strip_prefix(root).map_err(|_| ModError::Invalid("path left the source folder".into()))?;
            out.push((p.clone(), base.join(rel)));
        }
        // symlinks and special files are skipped on purpose
    }
    Ok(())
}

#[cfg(test)]
mod tests;
