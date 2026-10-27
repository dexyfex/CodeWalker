//! Tests build small RPF7 archives byte by byte and read them back, so the
//! parser is checked against the documented layout instead of against itself.

use std::io::Write;

use flate2::{Compression, write::DeflateEncoder};

use super::*;
use crate::crypto::encrypt_aes;

enum Item {
    Dir(&'static str, Vec<Item>),
    /// Stored (uncompressed) binary file.
    Bin(&'static str, Vec<u8>),
    /// Deflated binary file; `size` in the TOC is the compressed length.
    Packed(&'static str, Vec<u8>),
    /// Resource with an RSC7 header; payload is deflated.
    Res(&'static str, Vec<u8>, u32, u32),
    Rpf(&'static str, Vec<u8>),
}

fn deflate(data: &[u8]) -> Vec<u8> {
    let mut e = DeflateEncoder::new(Vec::new(), Compression::default());
    e.write_all(data).unwrap();
    e.finish().unwrap()
}

fn le24(v: u32) -> [u8; 3] {
    [v as u8, (v >> 8) as u8, (v >> 16) as u8]
}

struct Node<'a> {
    name: &'a str,
    item: Option<&'a Item>,
    children: Vec<usize>,
}

/// Builds an RPF7 with the given encryption (OPEN or AES with `key`).
fn build(root: &[Item], aes: Option<&[u8; 32]>) -> Vec<u8> {
    // flatten breadth-first so each directory's children are contiguous
    let mut nodes: Vec<Node> = vec![Node { name: "", item: None, children: vec![] }];
    let mut queue: Vec<(usize, &[Item])> = vec![(0, root)];
    let mut qi = 0;
    while qi < queue.len() {
        let (parent, items) = queue[qi];
        qi += 1;
        let first = nodes.len();
        for it in items {
            let name = match it {
                Item::Dir(n, _)
                | Item::Bin(n, _)
                | Item::Packed(n, _)
                | Item::Res(n, ..)
                | Item::Rpf(n, _) => *n,
            };
            nodes.push(Node { name, item: Some(it), children: vec![] });
        }
        nodes[parent].children = (first..nodes.len()).collect();
        for (k, it) in items.iter().enumerate() {
            if let Item::Dir(_, kids) = it {
                queue.push((first + k, kids));
            }
        }
    }

    // names table
    let mut names = Vec::new();
    let mut name_off = Vec::new();
    for n in &nodes {
        name_off.push(names.len() as u32);
        names.extend_from_slice(n.name.as_bytes());
        names.push(0);
    }
    while names.len() % 16 != 0 {
        names.push(0);
    }

    let header_len = 16 + nodes.len() * 16 + names.len();
    let mut data_start = header_len.div_ceil(512) * 512;
    let mut blobs: Vec<(usize, Vec<u8>)> = Vec::new();
    let mut toc = Vec::new();
    for (i, n) in nodes.iter().enumerate() {
        let off = name_off[i];
        match n.item {
            None | Some(Item::Dir(..)) => {
                let (first, count) =
                    n.children.first().map_or((0, 0), |f| (*f as u32, n.children.len() as u32));
                for v in [off, DIR_IDENT, first, count] {
                    toc.extend_from_slice(&v.to_le_bytes());
                }
            }
            Some(item) => {
                let (stored, kind): (Vec<u8>, _) = match item {
                    Item::Bin(_, d) => (d.clone(), (0u32, d.len() as u32)),
                    Item::Packed(_, d) => {
                        let z = deflate(d);
                        let len = z.len() as u32;
                        (z, (len, d.len() as u32))
                    }
                    Item::Rpf(_, d) => (d.clone(), (0, d.len() as u32)),
                    Item::Res(_, d, ..) => {
                        let mut v = vec![0u8; 16];
                        v.extend(deflate(d));
                        let len = v.len() as u32;
                        (v, (len, 0))
                    }
                    Item::Dir(..) => unreachable!(),
                };
                let block = (data_start / 512) as u32;
                data_start += stored.len().div_ceil(512) * 512;
                blobs.push((block as usize * 512, stored));
                if let Item::Res(_, _, sys, gfx) = item {
                    toc.extend_from_slice(&(off as u16).to_le_bytes());
                    toc.extend_from_slice(&le24(kind.0));
                    let mut o = le24(block);
                    o[2] |= 0x80;
                    toc.extend_from_slice(&o);
                    toc.extend_from_slice(&sys.to_le_bytes());
                    toc.extend_from_slice(&gfx.to_le_bytes());
                } else {
                    let packed = (off as u64 & 0xFFFF) | ((kind.0 as u64) << 16) | ((block as u64) << 40);
                    toc.extend_from_slice(&packed.to_le_bytes());
                    toc.extend_from_slice(&kind.1.to_le_bytes());
                    toc.extend_from_slice(&0u32.to_le_bytes());
                }
            }
        }
    }

    let enc_word: u32 = if aes.is_some() { 0x0FFF_FFF9 } else { 0x4E45_504F };
    let (toc, names) = match aes {
        Some(k) => (encrypt_aes(&toc, k), encrypt_aes(&names, k)),
        None => (toc, names),
    };
    let mut out = Vec::new();
    for v in [RPF7_MAGIC, nodes.len() as u32, names.len() as u32, enc_word] {
        out.extend_from_slice(&v.to_le_bytes());
    }
    out.extend_from_slice(&toc);
    out.extend_from_slice(&names);
    out.resize(data_start, 0);
    for (at, bytes) in blobs {
        out[at..at + bytes.len()].copy_from_slice(&bytes);
    }
    out
}

fn scan(bytes: Vec<u8>, keys: Option<Arc<GtaKeys>>) -> Result<RpfTree> {
    RpfTree::scan_blob(Blob::from_vec(bytes), PathBuf::from("test.rpf"), "test.rpf".into(), "test.rpf", keys)
}

fn sample_items() -> Vec<Item> {
    let inner = build(&[Item::Bin("inner.txt", b"nested!".to_vec())], None);
    vec![
        Item::Bin("readme.txt", b"hello rpf".to_vec()),
        Item::Dir(
            "Levels",
            vec![
                Item::Packed("Big.xml", "<a>text</a>".repeat(200).into_bytes()),
                Item::Res(
                    "prop.ydr",
                    b"payload-of-a-drawable".to_vec(),
                    0x6000_0000 | (1 << 17),
                    0x9000_0000,
                ),
            ],
        ),
        Item::Rpf("child.rpf", inner),
    ]
}

#[test]
fn reads_directories_files_and_nested_archives() {
    let tree = scan(build(&sample_items(), None), None).unwrap();
    assert!(tree.errors.is_empty(), "{:?}", tree.errors);
    assert_eq!(tree.archives.len(), 2);
    let root = tree.archive(0);
    assert_eq!(root.encryption, RpfEncryption::Open);
    assert_eq!(root.dir_children(0).len(), 3);

    let readme = tree.find("test.rpf/readme.txt").expect("readme");
    assert_eq!(tree.extract(readme).unwrap(), b"hello rpf");

    // lookups are case/separator insensitive
    let big = tree.find("TEST.RPF\\Levels\\big.xml").expect("big");
    let text = tree.extract(big).unwrap();
    assert_eq!(text.len(), 11 * 200);
    assert!(tree.entry(big).stored_size() < text.len() as u64, "should be stored deflated");

    let nested = tree.find("test.rpf/child.rpf/inner.txt").expect("nested");
    assert_eq!(tree.extract(nested).unwrap(), b"nested!");
    assert_eq!(tree.archive(nested.archive).parent, Some(0));
    assert_eq!(tree.entry(tree.find("test.rpf/child.rpf").unwrap()).child_archive, Some(1));
}

#[test]
fn resources_expose_flags_and_export_an_rsc7_header() {
    let tree = scan(build(&sample_items(), None), None).unwrap();
    let r = tree.find("test.rpf/levels/prop.ydr").unwrap();
    let res = tree.extract_resource(r).unwrap();
    assert_eq!(res.data, b"payload-of-a-drawable");
    assert_eq!(res.version, (6 << 4) + 9);
    assert_eq!(tree.entry(r).display_size(), 8192);
    let exported = tree.export(r).unwrap();
    assert_eq!(u32::from_le_bytes(exported[0..4].try_into().unwrap()), RSC7_MAGIC);
    assert_eq!(u32::from_le_bytes(exported[4..8].try_into().unwrap()), res.version);
    assert_eq!(&inflate(&exported[16..]).unwrap(), b"payload-of-a-drawable");
}

#[test]
fn aes_table_of_contents_needs_and_uses_the_key() {
    let keys = Arc::new(GtaKeys::synthetic());
    let bytes = build(&sample_items(), Some(&keys.aes_key));

    assert!(matches!(scan(bytes.clone(), None), Err(Error::MissingKeys)));

    let tree = scan(bytes, Some(keys)).unwrap();
    assert_eq!(tree.archive(0).encryption, RpfEncryption::Aes);
    let f = tree.find("test.rpf/readme.txt").unwrap();
    assert_eq!(tree.extract(f).unwrap(), b"hello rpf");
}

#[test]
fn search_finds_files_but_not_directories() {
    let tree = scan(build(&sample_items(), None), None).unwrap();
    let mut hits: Vec<String> = tree.search("levels").map(|r| tree.entry(r).name.clone()).collect();
    hits.sort();
    assert_eq!(hits, ["Big.xml", "prop.ydr"]);
}

#[test]
fn rejects_garbage_without_panicking() {
    assert!(matches!(scan(vec![], None), Err(Error::InvalidRpf(_))));
    assert!(matches!(scan(vec![0u8; 64], None), Err(Error::InvalidRpf(_))));
    let mut ok = build(&sample_items(), None);
    ok.truncate(40);
    assert!(matches!(scan(ok, None), Err(Error::InvalidRpf(_))));
    // absurd entry count
    let mut bad = build(&sample_items(), None);
    bad[4..8].copy_from_slice(&u32::MAX.to_le_bytes());
    assert!(scan(bad, None).is_err());
}

#[test]
fn hostile_directory_ranges_cannot_loop_or_overflow() {
    let mut bytes = build(&sample_items(), None);
    // root now claims entries [0 ..= u32::MAX): itself and everything else
    bytes[16 + 8..16 + 12].copy_from_slice(&0u32.to_le_bytes());
    bytes[16 + 12..16 + 16].copy_from_slice(&u32::MAX.to_le_bytes());
    let tree = scan(bytes, None).unwrap();
    assert!(tree.archive(0).entries.len() > 1);
}

#[test]
fn entry_data_outside_the_archive_is_an_error() {
    let tree = scan(build(&[Item::Bin("a.bin", vec![1; 10])], None), None).unwrap();
    let mut t = tree;
    if let EntryKind::Binary { offset, .. } = &mut t.archives[0].entries[1].kind {
        *offset = 0xFF_FFFF;
    }
    let r = t.find("test.rpf/a.bin").unwrap();
    assert!(matches!(t.extract(r), Err(Error::InvalidRpf(_))));
}

#[test]
fn page_flag_sizes_match_known_examples() {
    // from the table of examples in RpfFile.cs
    assert_eq!(page_flags_size(1 << 17), 8192); // s4:1, ss:0  (ytd)
    assert_eq!(page_flags_size(1 << 11), 16384); // s5:1       (ytyp)
    assert_eq!(page_flags_size((1 << 17) | (1 << 11)), 24576);
    assert_eq!(page_flags_size(1 | (1 << 4)), 262144); // ss:1, s8:1 (yft)
}

#[test]
fn path_validation_matches_csharp_rules() {
    assert!(is_valid_path("x64a.rpf/levels/gta5"));
    assert!(!is_valid_path(""));
    assert!(!is_valid_path("a:b.rpf"));
    assert!(!is_valid_path("a;b.rpf"));
    assert!(!is_valid_path(&"a/".repeat(21)));
    assert!(!is_valid_path(&"x".repeat(501)));
}
