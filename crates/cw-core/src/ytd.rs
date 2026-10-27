//! Texture dictionaries (`.ytd`), legacy (version 13) layout.
//!
//! Offsets come from `Resources/Texture.cs` and `FileTypes/YtdFile.cs`:
//!
//! ```text
//! TextureDictionary @ 0x50000000
//!   +0x20  u64 ptr, u16 count, u16 capacity   texture name hashes (u32 each)
//!   +0x30  u64 ptr, u16 count, u16 capacity   pointers (u64 each) to Texture blocks
//! Texture (144 bytes)
//!   +0x28  u64 name pointer (UTF-8, no extension)
//!   +0x50  u16 width   +0x52 u16 height   +0x58 u32 format   +0x5D u8 levels
//!   +0x70  u64 pixel data pointer (graphics segment), mips back to back
//! ```
//!
//! Gen9 dictionaries (resource version 5) use a different layout and are
//! reported as unsupported rather than misread. Nothing here has been checked
//! against real game files (none are available in the repository); the layout
//! is transcribed from the C# source and exercised by synthetic files.

use crate::{
    error::{Error, Result},
    hash::joaat_lower,
    resource::{ResourceReader, SYSTEM_BASE},
    rpf::ResourceData,
    texture::{Format, Texture, chain_size},
};

const LEGACY_VERSION: u32 = 13;
const MAX_TEXTURES: usize = 4096;
const MAX_NAME: usize = 256;

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct YtdTexture {
    pub name: String,
    /// From the dictionary's hash list when present, else the hash of `name`.
    pub name_hash: u32,
    pub texture: Texture,
}

pub fn parse_ytd(res: &ResourceData) -> Result<Vec<YtdTexture>> {
    if res.version != LEGACY_VERSION {
        return Err(Error::InvalidRpf(format!(
            "texture dictionary version {} is not supported (only legacy version {LEGACY_VERSION})",
            res.version
        )));
    }
    let r = ResourceReader::new(&res.data, res.sys_flags, res.gfx_flags);
    let root = SYSTEM_BASE;

    let hashes_ptr = r.u64_at(root + 0x20);
    let tex_ptr = r.u64_at(root + 0x30);
    let count = r.u16_at(root + 0x38) as usize;
    if tex_ptr == 0 || count == 0 {
        return Ok(Vec::new());
    }
    // the pointer array holds `capacity` entries; a larger count would read past it
    let capacity = r.u16_at(root + 0x3A) as usize;
    let count = count.min(capacity).min(MAX_TEXTURES);
    let hash_count = r.u16_at(root + 0x28) as usize;

    // All pixel data together cannot be larger than the resource it came from. Without this, a
    // few thousand headers aimed at one block would ask for terabytes between them.
    let mut budget = res.data.len();
    let mut out = Vec::with_capacity(count);
    for i in 0..count {
        let t_addr = r.u64_at(tex_ptr + i as u64 * 8);
        // not a block inside the resource (null, garbage, or past the segment): skip it
        if !r.contains(t_addr, 0x78) {
            continue;
        }
        let width = r.u16_at(t_addr + 0x50) as u32;
        let height = r.u16_at(t_addr + 0x52) as u32;
        let format = Format::from_d3d(r.u32_at(t_addr + 0x58));
        let levels = (r.u8_at(t_addr + 0x5D) as u32).clamp(1, 16);
        let data_ptr = r.u64_at(t_addr + 0x70);
        let name = r.cstr_at(r.u64_at(t_addr + 0x28), MAX_NAME);

        let size = chain_size(format, width, height, levels);
        // an unknown format, a zero size, pixels that are not inside the resource or that would
        // overspend the budget: keep the entry for listing, without pixels
        let data = if size == 0 || size > budget || !r.contains(data_ptr, size) {
            Vec::new()
        } else {
            budget -= size;
            r.bytes_at(data_ptr, size).unwrap_or_default()
        };
        let name_hash = if i < hash_count && hashes_ptr != 0 {
            r.u32_at(hashes_ptr + i as u64 * 4)
        } else {
            joaat_lower(&name)
        };
        out.push(YtdTexture { name, name_hash, texture: Texture { width, height, levels, format, data } });
    }
    Ok(out)
}

/// Builds a legacy `.ytd` payload (system + graphics segments, uncompressed)
/// together with the flags describing it. For tests and mod-making tools.
pub fn build_ytd(textures: &[(&str, &Texture)]) -> ResourceData {
    use crate::rpf::{flags_for_size, page_flags_size, resource_version};

    let n = textures.len();
    // --- system segment layout
    let mut sys = vec![0u8; 0x40];
    let put = |v: &mut Vec<u8>, at: usize, bytes: &[u8]| {
        if v.len() < at + bytes.len() {
            v.resize(at + bytes.len(), 0);
        }
        v[at..at + bytes.len()].copy_from_slice(bytes);
    };
    let hashes_at = sys.len();
    for (name, _) in textures {
        sys.extend_from_slice(&joaat_lower(name).to_le_bytes());
    }
    while !sys.len().is_multiple_of(16) {
        sys.push(0);
    }
    let ptrs_at = sys.len();
    sys.resize(ptrs_at + n * 8, 0);
    let mut tex_addrs = Vec::new();
    let mut gfx = Vec::new();
    for (i, (name, t)) in textures.iter().enumerate() {
        while !sys.len().is_multiple_of(16) {
            sys.push(0);
        }
        let name_at = sys.len();
        sys.extend_from_slice(name.as_bytes());
        sys.push(0);
        while !sys.len().is_multiple_of(16) {
            sys.push(0);
        }
        let tex_at = sys.len();
        sys.resize(tex_at + 144, 0);
        while !gfx.len().is_multiple_of(16) {
            gfx.push(0);
        }
        let data_at = gfx.len();
        gfx.extend_from_slice(&t.data);
        put(&mut sys, tex_at + 0x28, &(SYSTEM_BASE + name_at as u64).to_le_bytes());
        put(&mut sys, tex_at + 0x50, &(t.width as u16).to_le_bytes());
        put(&mut sys, tex_at + 0x52, &(t.height as u16).to_le_bytes());
        put(&mut sys, tex_at + 0x58, &t.format.to_d3d().to_le_bytes());
        put(&mut sys, tex_at + 0x5D, &[t.levels as u8]);
        put(&mut sys, tex_at + 0x70, &(crate::resource::GRAPHICS_BASE + data_at as u64).to_le_bytes());
        tex_addrs.push(SYSTEM_BASE + tex_at as u64);
        put(&mut sys, ptrs_at + i * 8, &tex_addrs[i].to_le_bytes());
    }
    put(&mut sys, 0x20, &(SYSTEM_BASE + hashes_at as u64).to_le_bytes());
    put(&mut sys, 0x28, &(n as u16).to_le_bytes());
    put(&mut sys, 0x2A, &(n as u16).to_le_bytes());
    put(&mut sys, 0x30, &(SYSTEM_BASE + ptrs_at as u64).to_le_bytes());
    put(&mut sys, 0x38, &(n as u16).to_le_bytes());
    put(&mut sys, 0x3A, &(n as u16).to_le_bytes());

    // legacy ytd is version 13 = (system nibble 0) << 4 | (graphics nibble 0xD)
    let (sys_flags, gfx_flags) = (flags_for_size(sys.len(), 0), flags_for_size(gfx.len(), 0xD));
    sys.resize(page_flags_size(sys_flags) as usize, 0);
    gfx.resize(page_flags_size(gfx_flags) as usize, 0);
    sys.extend_from_slice(&gfx);
    ResourceData { sys_flags, gfx_flags, version: resource_version(sys_flags, gfx_flags), data: sys }
}

#[cfg(test)]
mod tests;
