//! RPF7 archive reader.
//!
//! Layout (little endian), see `CodeWalker.Core/GameFiles/RpfFile.cs`:
//!
//! ```text
//! u32 magic 'RPF7' | u32 entry_count | u32 names_length | u32 encryption
//! entry_count * 16 bytes   (TOC, encrypted according to `encryption`)
//! names_length bytes       (NUL terminated UTF-8 names, same encryption)
//! ```
//!
//! File data lives at `start_pos + offset * 512`. Nested `.rpf` files are
//! plain binary entries whose data is again an RPF7 archive, so a whole game
//! installation becomes a tree of archives that all share the same mapping.

use std::{
    collections::HashMap,
    io::Read,
    path::{Path, PathBuf},
    sync::{Arc, OnceLock},
};

use flate2::read::DeflateDecoder;
use rayon::prelude::*;

use crate::{
    Blob,
    crypto::{decrypt_aes, decrypt_ng},
    error::{Error, Result},
    keys::GtaKeys,
};

pub const RPF7_MAGIC: u32 = 0x5250_4637;
pub const RSC7_MAGIC: u32 = 0x3743_5352;
const DIR_IDENT: u32 = 0x7FFF_FF00;
const BLOCK: u64 = 512;
const MAX_DEPTH: u32 = 8;

pub type ArchiveId = u32;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord)]
pub struct EntryRef {
    pub archive: ArchiveId,
    pub index: u32,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum RpfEncryption {
    None,
    Open,
    Aes,
    Ng,
    Unknown(u32),
}

impl From<u32> for RpfEncryption {
    fn from(v: u32) -> Self {
        match v {
            0 => Self::None,
            0x4E45_504F => Self::Open,
            0x0FFF_FFF9 => Self::Aes,
            0x0FEF_FFFF => Self::Ng,
            other => Self::Unknown(other),
        }
    }
}

impl RpfEncryption {
    pub fn label(self) -> &'static str {
        match self {
            Self::None => "None",
            Self::Open => "Open",
            Self::Aes => "AES",
            Self::Ng => "NG",
            Self::Unknown(_) => "Unknown (NG)",
        }
    }
}

#[derive(Debug, Clone)]
pub enum EntryKind {
    /// Children are `entries[first .. first + count]`.
    Directory {
        first: u32,
        count: u32,
    },
    Binary {
        offset: u32,
        size: u32,
        uncompressed_size: u32,
        encrypted: bool,
    },
    Resource {
        offset: u32,
        size: u32,
        sys_flags: u32,
        gfx_flags: u32,
        encrypted: bool,
    },
}

#[derive(Debug, Clone)]
pub struct RpfEntry {
    pub name: String,
    pub name_lower: String,
    /// Lower-case, `/` separated, starting with the archive path.
    pub path: String,
    pub parent: Option<u32>,
    pub kind: EntryKind,
    /// Set for `.rpf` binaries that were parsed as nested archives.
    pub child_archive: Option<ArchiveId>,
}

impl RpfEntry {
    pub fn is_dir(&self) -> bool {
        matches!(self.kind, EntryKind::Directory { .. })
    }

    pub fn is_resource(&self) -> bool {
        matches!(self.kind, EntryKind::Resource { .. })
    }

    /// Size shown in listings: uncompressed for binaries, the sum of the
    /// resource page sizes for resources.
    pub fn display_size(&self) -> u64 {
        match self.kind {
            EntryKind::Directory { .. } => 0,
            EntryKind::Binary { size, uncompressed_size, .. } => {
                if uncompressed_size > 0 {
                    uncompressed_size as u64
                } else {
                    size as u64
                }
            }
            EntryKind::Resource { sys_flags, gfx_flags, .. } => {
                page_flags_size(sys_flags) as u64 + page_flags_size(gfx_flags) as u64
            }
        }
    }

    /// Bytes occupied inside the archive.
    pub fn stored_size(&self) -> u64 {
        match self.kind {
            EntryKind::Directory { .. } => 0,
            EntryKind::Binary { size, uncompressed_size, .. } => {
                if size == 0 {
                    uncompressed_size as u64
                } else {
                    size as u64
                }
            }
            EntryKind::Resource { size, sys_flags, gfx_flags, .. } => {
                if size == 0 {
                    page_flags_size(sys_flags) as u64 + page_flags_size(gfx_flags) as u64
                } else {
                    size as u64
                }
            }
        }
    }

    pub fn extension(&self) -> &str {
        self.name_lower.rsplit_once('.').map(|(_, e)| e).unwrap_or("")
    }

    pub fn short_name_lower(&self) -> &str {
        match self.name_lower.rfind('.') {
            Some(i) if i > 0 => &self.name_lower[..i],
            _ => &self.name_lower,
        }
    }
}

/// Total size described by resource page flags (`RpfResourceFileEntry.GetSizeFromFlags`).
pub fn page_flags_size(flags: u32) -> u32 {
    let s0 = (flags >> 27) & 0x1;
    let s1 = ((flags >> 26) & 0x1) << 1;
    let s2 = ((flags >> 25) & 0x1) << 2;
    let s3 = ((flags >> 24) & 0x1) << 3;
    let s4 = ((flags >> 17) & 0x7F) << 4;
    let s5 = ((flags >> 11) & 0x3F) << 5;
    let s6 = ((flags >> 7) & 0xF) << 6;
    let s7 = ((flags >> 5) & 0x3) << 7;
    let s8 = ((flags >> 4) & 0x1) << 8;
    let base = 0x200u32 << (flags & 0xF);
    base.wrapping_mul(s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8)
}

/// `(system << 4) + graphics` resource version from the two flag words.
pub fn resource_version(sys_flags: u32, gfx_flags: u32) -> u32 {
    (((sys_flags >> 28) & 0xF) << 4) + ((gfx_flags >> 28) & 0xF)
}

#[derive(Debug, Clone)]
pub struct ResourceData {
    pub sys_flags: u32,
    pub gfx_flags: u32,
    pub version: u32,
    /// Decompressed payload (without the 16-byte RSC7 header).
    pub data: Vec<u8>,
}

pub struct RpfArchive {
    pub id: ArchiveId,
    pub name: String,
    pub name_lower: String,
    pub path: String,
    /// The file on disk that (transitively) contains this archive.
    pub physical: PathBuf,
    pub size: u64,
    pub start_pos: u64,
    pub encryption: RpfEncryption,
    pub parent: Option<ArchiveId>,
    pub parent_entry: Option<u32>,
    pub children: Vec<ArchiveId>,
    pub entries: Vec<RpfEntry>,
    blob: Blob,
    keys: Option<Arc<GtaKeys>>,
}

impl RpfArchive {
    pub fn is_aes(&self) -> bool {
        self.encryption == RpfEncryption::Aes
    }

    pub fn root(&self) -> &RpfEntry {
        &self.entries[0]
    }

    pub fn dir_children(&self, dir: u32) -> &[RpfEntry] {
        match self.entries.get(dir as usize).map(|e| &e.kind) {
            Some(EntryKind::Directory { first, count }) => {
                let a = (*first as usize).min(self.entries.len());
                let b = (a + *count as usize).min(self.entries.len());
                &self.entries[a..b]
            }
            _ => &[],
        }
    }

    pub fn file_count(&self) -> usize {
        self.entries.iter().filter(|e| !e.is_dir()).count()
    }
}

#[derive(Debug, Clone)]
pub struct ScanError {
    pub path: String,
    pub message: String,
}

#[derive(Default)]
pub struct RpfTree {
    pub archives: Vec<Arc<RpfArchive>>,
    pub roots: Vec<ArchiveId>,
    pub errors: Vec<ScanError>,
    index: OnceLock<HashMap<String, EntryRef>>,
}

impl RpfTree {
    /// Parses one RPF on disk (and everything nested inside it).
    pub fn scan_file(path: &Path, rel_path: &str, keys: Option<Arc<GtaKeys>>) -> Result<RpfTree> {
        let blob = Blob::map_file(path)?;
        let name = path.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
        Self::scan_blob(blob, path.to_path_buf(), name, rel_path, keys)
    }

    /// Parses an archive held in memory (used by tests and import tooling).
    pub fn scan_blob(
        blob: Blob,
        physical: PathBuf,
        name: String,
        rel_path: &str,
        keys: Option<Arc<GtaKeys>>,
    ) -> Result<RpfTree> {
        let size = blob.len() as u64;
        let mut tree = RpfTree::default();
        let path = normalize_path(rel_path);
        let ctx = ScanCtx { blob: &blob, physical: &physical, keys: &keys };
        let mut archives = Vec::new();
        let id = parse_archive(&ctx, 0, name, path, size, None, 0, &mut archives, &mut tree.errors)?;
        tree.archives = archives.into_iter().map(Arc::new).collect();
        tree.roots.push(id);
        Ok(tree)
    }

    /// Scans every `*.rpf` below `root` in parallel. Files in `exclude`d
    /// relative-path prefixes are skipped; broken archives are recorded in
    /// `errors` instead of aborting the scan.
    pub fn scan_folder(
        root: &Path,
        keys: Option<Arc<GtaKeys>>,
        exclude: &[String],
        progress: &(dyn Fn(&str) + Sync),
    ) -> RpfTree {
        let mut files = Vec::new();
        collect_rpfs(root, &mut files);
        files.sort();

        let results: Vec<(String, Result<RpfTree>)> = files
            .par_iter()
            .filter_map(|p| {
                let rel = normalize_path(&p.strip_prefix(root).unwrap_or(p).to_string_lossy());
                if exclude.iter().any(|x| rel.starts_with(&x.to_lowercase())) {
                    return None;
                }
                progress(&rel);
                Some((rel.clone(), Self::scan_file(p, &rel, keys.clone())))
            })
            .collect();

        let mut tree = RpfTree::default();
        for (rel, result) in results {
            match result {
                Ok(t) => tree.merge(t),
                Err(e) => tree.errors.push(ScanError { path: rel, message: e.to_string() }),
            }
        }
        tree
    }

    /// Appends `other` (ids are rebased; existing archives are untouched).
    pub fn merge(&mut self, mut other: RpfTree) {
        let off = self.archives.len() as u32;
        for a in other.archives.iter_mut() {
            let a = Arc::get_mut(a).expect("a freshly scanned tree owns its archives");
            a.id += off;
            a.parent = a.parent.map(|p| p + off);
            for c in a.children.iter_mut() {
                *c += off;
            }
            for e in a.entries.iter_mut() {
                e.child_archive = e.child_archive.map(|c| c + off);
            }
        }
        self.roots.extend(other.roots.iter().map(|r| r + off));
        self.archives.append(&mut other.archives);
        self.errors.append(&mut other.errors);
        self.index = OnceLock::new();
    }

    /// A new tree holding this tree's archives plus `other`'s. Cheap: archives
    /// are shared, not copied. Entry references into `self` stay valid.
    pub fn merged(&self, other: RpfTree) -> RpfTree {
        let mut t = RpfTree {
            archives: self.archives.clone(),
            roots: self.roots.clone(),
            errors: self.errors.clone(),
            index: OnceLock::new(),
        };
        t.merge(other);
        t
    }

    pub fn archive(&self, id: ArchiveId) -> &RpfArchive {
        &self.archives[id as usize]
    }

    pub fn entry(&self, r: EntryRef) -> &RpfEntry {
        &self.archives[r.archive as usize].entries[r.index as usize]
    }

    pub fn total_entries(&self) -> usize {
        self.archives.iter().map(|a| a.entries.len()).sum()
    }

    /// Lower-case path -> entry, built on first use.
    pub fn find(&self, path: &str) -> Option<EntryRef> {
        let idx = self.index.get_or_init(|| {
            let mut m = HashMap::with_capacity(self.total_entries());
            for a in &self.archives {
                for (i, e) in a.entries.iter().enumerate() {
                    // A nested archive's root directory shares its path with the
                    // `.rpf` file entry that contains it; keep the file entry,
                    // which reaches the archive through `child_archive`.
                    if i == 0 && a.parent.is_some() {
                        continue;
                    }
                    m.insert(e.path.clone(), EntryRef { archive: a.id, index: i as u32 });
                }
            }
            m
        });
        idx.get(&normalize_path(path)).copied()
    }

    /// Entries (not directories) whose lower-case path contains `needle`.
    pub fn search<'a>(&'a self, needle: &'a str) -> impl ParallelIterator<Item = EntryRef> + 'a {
        let needle = needle.to_lowercase();
        self.archives.par_iter().flat_map_iter(move |a| {
            let needle = needle.clone();
            a.entries.iter().enumerate().filter_map(move |(i, e)| {
                (!e.is_dir() && e.path.contains(&needle))
                    .then_some(EntryRef { archive: a.id, index: i as u32 })
            })
        })
    }

    // ---------------------------------------------------------------- data

    fn stored(&self, r: EntryRef) -> Result<(&RpfArchive, &RpfEntry, &[u8])> {
        let a = self.archive(r.archive);
        let e = self.entry(r);
        let (offset, len) = match e.kind {
            EntryKind::Directory { .. } => return Err(Error::NotAFile),
            EntryKind::Binary { offset, .. } | EntryKind::Resource { offset, .. } => {
                (offset, e.stored_size())
            }
        };
        let start = a.start_pos + offset as u64 * BLOCK;
        let end = start + len;
        let blob = a.blob.as_slice();
        if end > blob.len() as u64 {
            return Err(Error::InvalidRpf(format!("{}: data lies outside the archive", e.path)));
        }
        Ok((a, e, &blob[start as usize..end as usize]))
    }

    fn decrypt(&self, a: &RpfArchive, e: &RpfEntry, data: &[u8], ng_length: u32) -> Result<Vec<u8>> {
        if a.is_aes() {
            let keys = a.keys.as_ref().ok_or(Error::MissingKeys)?;
            Ok(decrypt_aes(data, &keys.aes_key))
        } else {
            // Like CodeWalker, anything that is not AES is treated as NG.
            let keys = a.keys.as_ref().ok_or(Error::MissingKeys)?;
            Ok(decrypt_ng(data, &e.name, ng_length, keys))
        }
    }

    /// Decompressed contents of a binary entry, or the decompressed payload
    /// of a resource (`RpfFile.ExtractFile`).
    pub fn extract(&self, r: EntryRef) -> Result<Vec<u8>> {
        match &self.entry(r).kind {
            EntryKind::Resource { .. } => Ok(self.extract_resource(r)?.data),
            _ => self.extract_binary(r),
        }
    }

    fn extract_binary(&self, r: EntryRef) -> Result<Vec<u8>> {
        let (a, e, raw) = self.stored(r)?;
        let EntryKind::Binary { size, uncompressed_size, encrypted, .. } = e.kind else {
            return Err(Error::NotAFile);
        };
        if raw.is_empty() {
            return Ok(Vec::new());
        }
        let data = if encrypted { self.decrypt(a, e, raw, uncompressed_size)? } else { raw.to_vec() };
        if size > 0 { inflate(&data) } else { Ok(data) }
    }

    pub fn extract_resource(&self, r: EntryRef) -> Result<ResourceData> {
        let (a, e, raw) = self.stored(r)?;
        let EntryKind::Resource { size, sys_flags, gfx_flags, encrypted, .. } = e.kind else {
            return Err(Error::NotAFile);
        };
        if raw.len() < 16 {
            return Err(Error::InvalidRpf(format!("{}: resource is shorter than its header", e.path)));
        }
        let payload = &raw[16..];
        let payload = if encrypted { self.decrypt(a, e, payload, size)? } else { payload.to_vec() };
        // Resources are raw deflate; if that fails the data is stored as-is.
        let data = inflate(&payload).unwrap_or(payload);
        Ok(ResourceData { sys_flags, gfx_flags, version: resource_version(sys_flags, gfx_flags), data })
    }

    /// Bytes for "Extract" on disk. Binary files are decompressed; resources
    /// keep their compressed payload and get an OpenIV-compatible RSC7
    /// header, so other tools can read them.
    pub fn export(&self, r: EntryRef) -> Result<Vec<u8>> {
        let (a, e, raw) = self.stored(r)?;
        match e.kind {
            EntryKind::Resource { size, sys_flags, gfx_flags, encrypted, .. } => {
                if raw.len() < 16 {
                    return Err(Error::InvalidRpf(format!(
                        "{}: resource is shorter than its header",
                        e.path
                    )));
                }
                let payload =
                    if encrypted { self.decrypt(a, e, &raw[16..], size)? } else { raw[16..].to_vec() };
                let mut out = Vec::with_capacity(16 + payload.len());
                out.extend_from_slice(&RSC7_MAGIC.to_le_bytes());
                out.extend_from_slice(&resource_version(sys_flags, gfx_flags).to_le_bytes());
                out.extend_from_slice(&sys_flags.to_le_bytes());
                out.extend_from_slice(&gfx_flags.to_le_bytes());
                out.extend_from_slice(&payload);
                Ok(out)
            }
            _ => self.extract_binary(r),
        }
    }
}

// ------------------------------------------------------------------ parsing

struct ScanCtx<'a> {
    blob: &'a Blob,
    physical: &'a Path,
    keys: &'a Option<Arc<GtaKeys>>,
}

fn u32_at(b: &[u8], o: usize) -> u32 {
    u32::from_le_bytes([b[o], b[o + 1], b[o + 2], b[o + 3]])
}

fn le24(b: &[u8]) -> u32 {
    b[0] as u32 | (b[1] as u32) << 8 | (b[2] as u32) << 16
}

#[allow(clippy::too_many_arguments)]
fn parse_archive(
    ctx: &ScanCtx,
    start_pos: u64,
    name: String,
    path: String,
    size: u64,
    parent: Option<(ArchiveId, u32)>,
    depth: u32,
    out: &mut Vec<RpfArchive>,
    errors: &mut Vec<ScanError>,
) -> Result<ArchiveId> {
    let blob = ctx.blob.as_slice();
    let start = start_pos as usize;
    if start.checked_add(16).is_none_or(|e| e > blob.len()) {
        return Err(Error::InvalidRpf(format!("{path}: truncated header")));
    }
    if u32_at(blob, start) != RPF7_MAGIC {
        return Err(Error::InvalidRpf(format!("{path}: not a GTA V (RPF7) archive")));
    }
    let entry_count = u32_at(blob, start + 4) as usize;
    let names_len = u32_at(blob, start + 8) as usize;
    let encryption = RpfEncryption::from(u32_at(blob, start + 12));

    let toc_start = start + 16;
    let toc_end = entry_count
        .checked_mul(16)
        .and_then(|n| toc_start.checked_add(n))
        .ok_or_else(|| Error::InvalidRpf(format!("{path}: bad entry count")))?;
    let names_end = toc_end
        .checked_add(names_len)
        .filter(|&e| e <= blob.len())
        .ok_or_else(|| Error::InvalidRpf(format!("{path}: table of contents is out of range")))?;
    if entry_count == 0 {
        return Err(Error::InvalidRpf(format!("{path}: empty table of contents")));
    }

    let mut toc = blob[toc_start..toc_end].to_vec();
    let mut names = blob[toc_end..names_end].to_vec();
    match encryption {
        RpfEncryption::None | RpfEncryption::Open => {}
        RpfEncryption::Aes => {
            let keys = ctx.keys.as_ref().ok_or(Error::MissingKeys)?;
            toc = decrypt_aes(&toc, &keys.aes_key);
            names = decrypt_aes(&names, &keys.aes_key);
        }
        RpfEncryption::Ng | RpfEncryption::Unknown(_) => {
            let keys = ctx.keys.as_ref().ok_or(Error::MissingKeys)?;
            toc = decrypt_ng(&toc, &name, size as u32, keys);
            names = decrypt_ng(&names, &name, size as u32, keys);
        }
    }

    let mut entries = Vec::with_capacity(entry_count);
    for i in 0..entry_count {
        let rec = &toc[i * 16..i * 16 + 16];
        let y = u32_at(rec, 0);
        let x = u32_at(rec, 4);
        let (name_offset, kind) = if x == DIR_IDENT {
            (y, EntryKind::Directory { first: u32_at(rec, 8), count: u32_at(rec, 12) })
        } else if x & 0x8000_0000 == 0 {
            let buf = u64::from_le_bytes(rec[..8].try_into().unwrap());
            let enc = u32_at(rec, 12);
            if enc > 1 {
                return Err(Error::InvalidRpf(format!("{path}: bad binary entry #{i}")));
            }
            (
                (buf & 0xFFFF) as u32,
                EntryKind::Binary {
                    size: ((buf >> 16) & 0xFF_FFFF) as u32,
                    offset: ((buf >> 40) & 0xFF_FFFF) as u32,
                    uncompressed_size: u32_at(rec, 8),
                    encrypted: enc == 1,
                },
            )
        } else {
            let mut file_size = le24(&rec[2..5]);
            let offset = le24(&rec[5..8]) & 0x7F_FFFF;
            if file_size == 0xFF_FFFF {
                // Sizes >= 16 MiB are stored in the resource's own header.
                let at = start + offset as usize * BLOCK as usize;
                if let Some(h) = blob.get(at..at + 16) {
                    file_size = h[7] as u32 | (h[14] as u32) << 8 | (h[5] as u32) << 16 | (h[2] as u32) << 24;
                }
            }
            (
                u16::from_le_bytes([rec[0], rec[1]]) as u32,
                EntryKind::Resource {
                    size: file_size,
                    offset,
                    sys_flags: u32_at(rec, 8),
                    gfx_flags: u32_at(rec, 12),
                    encrypted: false,
                },
            )
        };

        let mut entry_name = read_cstr(&names, name_offset as usize);
        if entry_name.chars().count() > 256 {
            entry_name = entry_name.chars().take(256).collect();
        }
        let name_lower = entry_name.to_lowercase();
        let kind = match kind {
            EntryKind::Resource { size, offset, sys_flags, gfx_flags, .. } => EntryKind::Resource {
                size,
                offset,
                sys_flags,
                gfx_flags,
                encrypted: name_lower.ends_with(".ysc"),
            },
            k => k,
        };
        entries.push(RpfEntry {
            name: entry_name,
            name_lower,
            path: String::new(),
            parent: None,
            kind,
            child_archive: None,
        });
    }

    if !entries[0].is_dir() {
        return Err(Error::InvalidRpf(format!("{path}: first entry is not the root directory")));
    }

    // Build the directory hierarchy and paths. Ranges are validated and
    // every entry is claimed at most once, so a hostile TOC cannot loop us.
    // The root directory has no name in the table; it is the archive itself.
    entries[0].name = name.clone();
    entries[0].name_lower = name.to_lowercase();
    entries[0].path = path.clone();
    let mut stack = vec![0u32];
    let mut claimed = vec![false; entries.len()];
    claimed[0] = true;
    while let Some(dir) = stack.pop() {
        let (first, count) = match entries[dir as usize].kind {
            EntryKind::Directory { first, count } => (first as usize, count as usize),
            _ => continue,
        };
        let end = first.saturating_add(count).min(entries.len());
        let dir_path = entries[dir as usize].path.clone();
        for i in first.min(end)..end {
            if claimed[i] {
                continue;
            }
            claimed[i] = true;
            entries[i].parent = Some(dir);
            entries[i].path = format!("{}/{}", dir_path, entries[i].name_lower);
            if entries[i].is_dir() {
                stack.push(i as u32);
            }
        }
    }

    let id = out.len() as ArchiveId;
    out.push(RpfArchive {
        id,
        name_lower: name.to_lowercase(),
        name,
        path,
        physical: ctx.physical.to_path_buf(),
        size,
        start_pos,
        encryption,
        parent: parent.map(|p| p.0),
        parent_entry: parent.map(|p| p.1),
        children: Vec::new(),
        entries,
        blob: ctx.blob.clone(),
        keys: ctx.keys.clone(),
    });

    if depth >= MAX_DEPTH {
        return Ok(id);
    }

    // Nested archives.
    let nested: Vec<(u32, String, String, u32, u64)> = out[id as usize]
        .entries
        .iter()
        .enumerate()
        .filter_map(|(i, e)| match e.kind {
            EntryKind::Binary { offset, size, uncompressed_size, .. }
                if e.name_lower.ends_with(".rpf") && is_valid_path(&e.path) && !e.path.is_empty() =>
            {
                let len = if size == 0 { uncompressed_size } else { size } as u64;
                Some((i as u32, e.name.clone(), e.path.clone(), offset, len))
            }
            _ => None,
        })
        .collect();

    for (index, child_name, child_path, offset, len) in nested {
        let child_start = start_pos + offset as u64 * BLOCK;
        match parse_archive(
            ctx,
            child_start,
            child_name,
            child_path.clone(),
            len,
            Some((id, index)),
            depth + 1,
            out,
            errors,
        ) {
            Ok(cid) => {
                out[id as usize].entries[index as usize].child_archive = Some(cid);
                out[id as usize].children.push(cid);
            }
            Err(e) => errors.push(ScanError { path: child_path, message: e.to_string() }),
        }
    }
    Ok(id)
}

fn read_cstr(names: &[u8], offset: usize) -> String {
    let Some(tail) = names.get(offset..) else { return String::new() };
    let end = tail.iter().position(|&b| b == 0).unwrap_or(tail.len());
    String::from_utf8_lossy(&tail[..end]).into_owned()
}

/// Port of `RpfFile.IsValidPath`: refuses hostile nested-archive paths.
fn is_valid_path(path: &str) -> bool {
    if path.is_empty() || path.len() > 500 {
        return false;
    }
    let mut dirs = 0;
    for c in path.chars() {
        match c {
            ':' | ';' => return false,
            '/' | '\\' => dirs += 1,
            _ => {}
        }
    }
    dirs <= 20
}

fn inflate(data: &[u8]) -> Result<Vec<u8>> {
    let mut out = Vec::with_capacity(data.len() * 2);
    DeflateDecoder::new(data).read_to_end(&mut out).map_err(|e| Error::Decompress(e.to_string()))?;
    Ok(out)
}

fn collect_rpfs(dir: &Path, out: &mut Vec<PathBuf>) {
    let Ok(rd) = std::fs::read_dir(dir) else { return };
    for entry in rd.flatten() {
        let p = entry.path();
        match entry.file_type() {
            Ok(t) if t.is_dir() => collect_rpfs(&p, out),
            Ok(t) if t.is_file() && p.extension().is_some_and(|e| e.eq_ignore_ascii_case("rpf")) => {
                out.push(p)
            }
            _ => {}
        }
    }
}

/// Lower-cases and converts `\` to `/` so lookups work with either style.
pub fn normalize_path(p: &str) -> String {
    p.replace('\\', "/").trim_matches('/').to_lowercase()
}

pub mod build;

#[cfg(test)]
mod tests;
