//! Loose resource files (the OpenIV-compatible form of `.ytd`, `.ydr`, `.ymap`…):
//! a 16-byte `RSC7` header followed by a raw-deflate payload.

use crate::{
    error::{Error, Result},
    rpf::{RSC7_MAGIC, ResourceData, inflate, page_flags_size, resource_version},
};

/// `Some` when `data` starts with an RSC7 header.
pub fn has_rsc7_header(data: &[u8]) -> bool {
    data.len() >= 16 && u32::from_le_bytes([data[0], data[1], data[2], data[3]]) == RSC7_MAGIC
}

/// Reads a loose resource file. The payload is inflated; if it does not
/// inflate it is returned as stored (some tools write uncompressed payloads).
pub fn parse_loose(data: &[u8]) -> Result<ResourceData> {
    if !has_rsc7_header(data) {
        return Err(Error::InvalidRpf("not an RSC7 resource (missing header)".into()));
    }
    let word = |o: usize| u32::from_le_bytes([data[o], data[o + 1], data[o + 2], data[o + 3]]);
    let (sys_flags, gfx_flags) = (word(8), word(12));
    let payload = &data[16..];
    let expected = page_flags_size(sys_flags) as u64 + page_flags_size(gfx_flags) as u64;
    let data = match inflate(payload, expected) {
        Ok(d) => d,
        Err(e @ Error::TooLarge(..)) => return Err(e),
        Err(_) => payload.to_vec(),
    };
    Ok(ResourceData { sys_flags, gfx_flags, version: resource_version(sys_flags, gfx_flags), data })
}

/// Human-readable summary of resource flags (used by the info panel).
pub fn describe_flags(sys_flags: u32, gfx_flags: u32) -> (u32, u32, u32) {
    (resource_version(sys_flags, gfx_flags), page_flags_size(sys_flags), page_flags_size(gfx_flags))
}

/// Reads structures out of a decompressed resource by *virtual address*.
///
/// A resource payload is the system segment followed by the graphics segment.
/// Pointers inside it carry the segment in their top bits (`0x5…` system,
/// `0x6…` graphics) and an offset in the low 28 bits. Reads past the end of a
/// segment return zeros instead of failing, as the original reader does, so a
/// truncated or hostile file cannot make the parser panic or over-allocate.
pub struct ResourceReader<'a> {
    system: &'a [u8],
    graphics: &'a [u8],
}

pub const SYSTEM_BASE: u64 = 0x5000_0000;
pub const GRAPHICS_BASE: u64 = 0x6000_0000;
const OFFSET_MASK: u64 = 0x0FFF_FFFF;
/// Largest single read honoured (defence against absurd sizes in hostile files).
const MAX_READ: usize = 256 * 1024 * 1024;

impl<'a> ResourceReader<'a> {
    pub fn new(data: &'a [u8], sys_flags: u32, gfx_flags: u32) -> Self {
        let sys = (page_flags_size(sys_flags) as usize).min(data.len());
        let gfx = (page_flags_size(gfx_flags) as usize).min(data.len() - sys);
        Self { system: &data[..sys], graphics: &data[sys..sys + gfx] }
    }

    fn segment(&self, addr: u64) -> Option<(&'a [u8], usize)> {
        let off = (addr & OFFSET_MASK) as usize;
        if addr & SYSTEM_BASE == SYSTEM_BASE && addr & GRAPHICS_BASE != GRAPHICS_BASE {
            Some((self.system, off))
        } else if addr & GRAPHICS_BASE == GRAPHICS_BASE {
            Some((self.graphics, off))
        } else {
            None
        }
    }

    /// Whether `len` bytes at `addr` lie entirely inside a segment.
    pub fn contains(&self, addr: u64, len: usize) -> bool {
        addr != 0
            && self
                .segment(addr)
                .is_some_and(|(seg, off)| off.checked_add(len).is_some_and(|e| e <= seg.len()))
    }

    /// `len` bytes at `addr`, zero-filled where they fall outside the segment.
    /// Returns `None` for a null/invalid address or an oversized request.
    pub fn bytes_at(&self, addr: u64, len: usize) -> Option<Vec<u8>> {
        if addr == 0 || len > MAX_READ {
            return None;
        }
        let (seg, off) = self.segment(addr)?;
        let mut out = vec![0u8; len];
        if off < seg.len() {
            let n = len.min(seg.len() - off);
            out[..n].copy_from_slice(&seg[off..off + n]);
        }
        Some(out)
    }

    pub fn u8_at(&self, addr: u64) -> u8 {
        self.bytes_at(addr, 1).map_or(0, |b| b[0])
    }

    pub fn u16_at(&self, addr: u64) -> u16 {
        self.bytes_at(addr, 2).map_or(0, |b| u16::from_le_bytes([b[0], b[1]]))
    }

    pub fn u32_at(&self, addr: u64) -> u32 {
        self.bytes_at(addr, 4).map_or(0, |b| u32::from_le_bytes([b[0], b[1], b[2], b[3]]))
    }

    pub fn u64_at(&self, addr: u64) -> u64 {
        self.bytes_at(addr, 8).map_or(0, |b| u64::from_le_bytes(b.try_into().unwrap()))
    }

    /// NUL-terminated UTF-8, at most `max` bytes.
    pub fn cstr_at(&self, addr: u64, max: usize) -> String {
        let Some(b) = self.bytes_at(addr, max) else { return String::new() };
        let end = b.iter().position(|&c| c == 0).unwrap_or(b.len());
        String::from_utf8_lossy(&b[..end]).into_owned()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::rpf::build::{Item, RpfBuilder};

    #[test]
    fn parses_what_the_archive_exporter_writes() {
        let bytes = RpfBuilder::new(vec![Item::resource(
            "a.ytd",
            &b"texture-bytes"[..],
            1 << 17,
            0x9000_0000 | (1 << 11),
        )])
        .build()
        .unwrap();
        let tree = crate::RpfTree::scan_blob(
            crate::Blob::from_vec(bytes),
            "t.rpf".into(),
            "t.rpf".into(),
            "t.rpf",
            None,
        )
        .unwrap();
        let exported = tree.export(tree.find("t.rpf/a.ytd").unwrap()).unwrap();
        assert!(has_rsc7_header(&exported));
        let r = parse_loose(&exported).unwrap();
        assert_eq!(r.data, b"texture-bytes");
        assert_eq!(describe_flags(r.sys_flags, r.gfx_flags), (9, 8192, 16384));
    }

    #[test]
    fn rejects_files_without_the_header() {
        assert!(parse_loose(b"hello").is_err());
        assert!(!has_rsc7_header(&[0; 8]));
    }

    #[test]
    fn falls_back_to_stored_payload_when_not_deflated() {
        let mut v = Vec::new();
        for w in [RSC7_MAGIC, 0x0, 0x6000_0000, 0x9000_0000] {
            v.extend_from_slice(&w.to_le_bytes());
        }
        v.extend_from_slice(&[0xFF; 8]); // not a valid deflate stream
        assert_eq!(parse_loose(&v).unwrap().data, vec![0xFF; 8]);
    }

    #[test]
    fn reader_splits_segments_and_zero_fills() {
        // system: 8 KiB (one 0x2000 page), graphics: 8 KiB
        let flags = 1u32 << 17;
        let mut data = vec![0u8; 16384];
        data[0x10..0x14].copy_from_slice(&0xAABB_CCDDu32.to_le_bytes());
        data[8192 + 0x20] = 0x7F;
        data[8191] = 0xEE; // last system byte
        let r = ResourceReader::new(&data, flags, flags);
        assert_eq!(r.u32_at(SYSTEM_BASE + 0x10), 0xAABB_CCDD);
        assert_eq!(r.u8_at(GRAPHICS_BASE + 0x20), 0x7F);
        assert_eq!(r.u8_at(SYSTEM_BASE + 8191), 0xEE, "last byte of the system segment");
        // reading across the end of a segment pads with zeros instead of leaking the next one
        assert_eq!(r.bytes_at(SYSTEM_BASE + 8190, 4).unwrap(), [0, 0xEE, 0, 0]);
        assert_eq!(r.u32_at(SYSTEM_BASE + 0x00FF_FFFF), 0, "far past the end");
        // null and foreign addresses yield nothing
        assert!(r.bytes_at(0, 4).is_none());
        assert!(r.bytes_at(0x4000_0000, 4).is_none());
        assert_eq!(r.u32_at(0x1234_5678), 0);
        assert!(r.bytes_at(SYSTEM_BASE, usize::MAX).is_none(), "absurd sizes are refused");
        assert!(r.contains(SYSTEM_BASE + 8188, 4) && !r.contains(SYSTEM_BASE + 8189, 4));
        assert!(!r.contains(0, 1) && !r.contains(0x4000_0000, 1) && !r.contains(SYSTEM_BASE, usize::MAX));
    }

    #[test]
    fn reader_tolerates_flags_larger_than_the_data() {
        let r = ResourceReader::new(&[1, 2, 3], 127 << 17, 127 << 17);
        assert_eq!(r.u8_at(SYSTEM_BASE + 2), 3);
        assert_eq!(r.u8_at(GRAPHICS_BASE), 0, "nothing is left for the graphics segment");
        let empty = ResourceReader::new(&[], 0, 0);
        assert_eq!(empty.u64_at(SYSTEM_BASE), 0);
    }

    #[test]
    fn cstr_stops_at_nul_and_respects_the_limit() {
        let mut data = vec![0u8; 8192 * 2];
        data[0x40..0x46].copy_from_slice(b"hello\0");
        data[0x50..0x58].copy_from_slice(b"abcdefgh");
        let r = ResourceReader::new(&data, 1 << 17, 1 << 17);
        assert_eq!(r.cstr_at(SYSTEM_BASE + 0x40, 64), "hello");
        assert_eq!(r.cstr_at(SYSTEM_BASE + 0x50, 4), "abcd");
        assert_eq!(r.cstr_at(0, 8), "");
    }
}
