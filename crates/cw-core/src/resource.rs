//! Loose resource files (the OpenIV-compatible form of `.ytd`, `.ydr`, `.ymap`…):
//! a 16-byte `RSC7` header followed by a raw-deflate payload.

use std::io::Read;

use flate2::read::DeflateDecoder;

use crate::{
    error::{Error, Result},
    rpf::{RSC7_MAGIC, ResourceData, page_flags_size, resource_version},
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
    let mut inflated = Vec::with_capacity(payload.len() * 2);
    let data = match DeflateDecoder::new(payload).read_to_end(&mut inflated) {
        Ok(_) => inflated,
        Err(_) => payload.to_vec(),
    };
    Ok(ResourceData { sys_flags, gfx_flags, version: resource_version(sys_flags, gfx_flags), data })
}

/// Human-readable summary of resource flags (used by the info panel).
pub fn describe_flags(sys_flags: u32, gfx_flags: u32) -> (u32, u32, u32) {
    (resource_version(sys_flags, gfx_flags), page_flags_size(sys_flags), page_flags_size(gfx_flags))
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
}
