//! Writes new RPF7 archives (unencrypted "OPEN" or AES), e.g. for mod
//! packages. Reading back what we write is covered by the tests in `tests.rs`.

use std::io::Write;

use flate2::{Compression, write::DeflateEncoder};

use super::{DIR_IDENT, RPF7_MAGIC, RSC7_MAGIC, resource_version};
use crate::{
    crypto::encrypt_aes,
    error::{Error, Result},
};

#[derive(Debug, Clone)]
pub enum Item {
    Dir {
        name: String,
        items: Vec<Item>,
    },
    /// Plain file; `compress` stores it deflated.
    File {
        name: String,
        data: Vec<u8>,
        compress: bool,
    },
    /// Resource: `data` is the uncompressed payload; the RSC7 header is written for you.
    Resource {
        name: String,
        data: Vec<u8>,
        sys_flags: u32,
        gfx_flags: u32,
    },
    /// A complete RPF stored as a file (nested archive).
    Archive {
        name: String,
        bytes: Vec<u8>,
    },
}

impl Item {
    pub fn name(&self) -> &str {
        match self {
            Item::Dir { name, .. }
            | Item::File { name, .. }
            | Item::Resource { name, .. }
            | Item::Archive { name, .. } => name,
        }
    }

    pub fn dir(name: &str, items: Vec<Item>) -> Item {
        Item::Dir { name: name.into(), items }
    }

    pub fn file(name: &str, data: impl Into<Vec<u8>>) -> Item {
        Item::File { name: name.into(), data: data.into(), compress: false }
    }

    pub fn packed(name: &str, data: impl Into<Vec<u8>>) -> Item {
        Item::File { name: name.into(), data: data.into(), compress: true }
    }

    pub fn resource(name: &str, data: impl Into<Vec<u8>>, sys_flags: u32, gfx_flags: u32) -> Item {
        Item::Resource { name: name.into(), data: data.into(), sys_flags, gfx_flags }
    }

    pub fn archive(name: &str, bytes: Vec<u8>) -> Item {
        Item::Archive { name: name.into(), bytes }
    }
}

#[derive(Debug, Clone, Default)]
pub struct RpfBuilder {
    pub items: Vec<Item>,
    /// `Some(key)` encrypts the table of contents with AES.
    pub aes_key: Option<[u8; 32]>,
}

struct Node<'a> {
    name: &'a str,
    item: Option<&'a Item>,
    children: Vec<usize>,
}

const MAX_BINARY: usize = 0xFF_FFFF;
const MAX_BLOCK: usize = 0x7F_FFFF;

fn deflate(data: &[u8]) -> Vec<u8> {
    let mut e = DeflateEncoder::new(Vec::new(), Compression::default());
    e.write_all(data).expect("writing to a Vec cannot fail");
    e.finish().expect("writing to a Vec cannot fail")
}

fn le24(v: u32) -> [u8; 3] {
    [v as u8, (v >> 8) as u8, (v >> 16) as u8]
}

impl RpfBuilder {
    pub fn new(items: Vec<Item>) -> Self {
        Self { items, aes_key: None }
    }

    pub fn with_aes(mut self, key: [u8; 32]) -> Self {
        self.aes_key = Some(key);
        self
    }

    pub fn build(&self) -> Result<Vec<u8>> {
        // Breadth-first, so each directory's children are one contiguous range.
        let mut nodes = vec![Node { name: "", item: None, children: vec![] }];
        let mut queue: Vec<(usize, &[Item])> = vec![(0, &self.items)];
        let mut qi = 0;
        while qi < queue.len() {
            let (parent, items) = queue[qi];
            qi += 1;
            let first = nodes.len();
            for it in items {
                if it.name().is_empty() || it.name().contains(['/', '\\', '\0']) {
                    return Err(Error::InvalidRpf(format!("bad entry name {:?}", it.name())));
                }
                nodes.push(Node { name: it.name(), item: Some(it), children: vec![] });
            }
            nodes[parent].children = (first..nodes.len()).collect();
            for (k, it) in items.iter().enumerate() {
                if let Item::Dir { items, .. } = it {
                    queue.push((first + k, items));
                }
            }
        }

        let mut names = Vec::new();
        let mut name_off = Vec::with_capacity(nodes.len());
        for n in &nodes {
            name_off.push(names.len());
            names.extend_from_slice(n.name.as_bytes());
            names.push(0);
        }
        while names.len() % 16 != 0 {
            names.push(0);
        }
        if names.len() > 0xFFFF + 256 || name_off.iter().any(|&o| o > 0xFFFF) {
            return Err(Error::InvalidRpf("too many names for the 16-bit name offsets".into()));
        }

        let header_len = 16 + nodes.len() * 16 + names.len();
        let mut data_end = header_len.div_ceil(512) * 512;
        let mut blobs: Vec<(usize, Vec<u8>)> = Vec::new();
        let mut toc = Vec::with_capacity(nodes.len() * 16);

        for (i, n) in nodes.iter().enumerate() {
            let off = name_off[i] as u32;
            let Some(item) = n.item.filter(|it| !matches!(it, Item::Dir { .. })) else {
                let (first, count) =
                    n.children.first().map_or((0, 0), |f| (*f as u32, n.children.len() as u32));
                for v in [off, DIR_IDENT, first, count] {
                    toc.extend_from_slice(&v.to_le_bytes());
                }
                continue;
            };

            // (stored bytes, size field, uncompressed size field)
            let (stored, size_field, raw_len) = match item {
                Item::File { data, compress: false, .. } | Item::Archive { bytes: data, .. } => {
                    (data.clone(), 0usize, data.len())
                }
                Item::File { data, compress: true, .. } => {
                    let z = deflate(data);
                    let l = z.len();
                    (z, l, data.len())
                }
                Item::Resource { data, sys_flags, gfx_flags, .. } => {
                    let mut v = Vec::with_capacity(16 + data.len());
                    v.extend_from_slice(&RSC7_MAGIC.to_le_bytes());
                    v.extend_from_slice(&resource_version(*sys_flags, *gfx_flags).to_le_bytes());
                    v.extend_from_slice(&sys_flags.to_le_bytes());
                    v.extend_from_slice(&gfx_flags.to_le_bytes());
                    v.extend(deflate(data));
                    let l = v.len();
                    (v, l, 0)
                }
                Item::Dir { .. } => unreachable!(),
            };
            if size_field > MAX_BINARY || raw_len > u32::MAX as usize {
                return Err(Error::InvalidRpf(format!("{} is too large for an RPF entry", n.name)));
            }
            let block = data_end / 512;
            if block > MAX_BLOCK {
                return Err(Error::InvalidRpf("archive is too large (more than 4 GiB of blocks)".into()));
            }
            data_end += stored.len().div_ceil(512) * 512;
            blobs.push((block * 512, stored));

            if let Item::Resource { sys_flags, gfx_flags, .. } = item {
                toc.extend_from_slice(&(off as u16).to_le_bytes());
                toc.extend_from_slice(&le24(size_field as u32));
                let mut o = le24(block as u32);
                o[2] |= 0x80;
                toc.extend_from_slice(&o);
                toc.extend_from_slice(&sys_flags.to_le_bytes());
                toc.extend_from_slice(&gfx_flags.to_le_bytes());
            } else {
                let packed = (off as u64 & 0xFFFF) | ((size_field as u64) << 16) | ((block as u64) << 40);
                toc.extend_from_slice(&packed.to_le_bytes());
                toc.extend_from_slice(&(raw_len as u32).to_le_bytes());
                toc.extend_from_slice(&0u32.to_le_bytes());
            }
        }

        let (toc, names, enc_word) = match &self.aes_key {
            Some(k) => (encrypt_aes(&toc, k), encrypt_aes(&names, k), 0x0FFF_FFF9u32),
            None => (toc, names, 0x4E45_504Fu32),
        };
        let mut out = Vec::with_capacity(data_end);
        for v in [RPF7_MAGIC, nodes.len() as u32, names.len() as u32, enc_word] {
            out.extend_from_slice(&v.to_le_bytes());
        }
        out.extend_from_slice(&toc);
        out.extend_from_slice(&names);
        out.resize(data_end, 0);
        for (at, bytes) in blobs {
            out[at..at + bytes.len()].copy_from_slice(&bytes);
        }
        Ok(out)
    }
}
