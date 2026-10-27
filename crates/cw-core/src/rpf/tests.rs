//! Tests build small RPF7 archives with [`RpfBuilder`] and read them back,
//! plus hand-corrupted variants to make sure hostile input cannot hang or panic.

use super::{
    build::{Item, RpfBuilder},
    *,
};

fn archive(items: &[Item], aes: Option<&[u8; 32]>) -> Vec<u8> {
    let mut b = RpfBuilder::new(items.to_vec());
    b.aes_key = aes.copied();
    b.build().unwrap()
}

fn scan(bytes: Vec<u8>, keys: Option<Arc<GtaKeys>>) -> Result<RpfTree> {
    RpfTree::scan_blob(Blob::from_vec(bytes), PathBuf::from("test.rpf"), "test.rpf".into(), "test.rpf", keys)
}

fn sample_items() -> Vec<Item> {
    let inner = archive(&[Item::file("inner.txt", &b"nested!"[..])], None);
    vec![
        Item::file("readme.txt", &b"hello rpf"[..]),
        Item::dir(
            "Levels",
            vec![
                Item::packed("Big.xml", "<a>text</a>".repeat(200).into_bytes()),
                Item::resource(
                    "prop.ydr",
                    &b"payload-of-a-drawable"[..],
                    0x6000_0000 | (1 << 17),
                    0x9000_0000,
                ),
            ],
        ),
        Item::archive("child.rpf", inner),
    ]
}

#[test]
fn reads_directories_files_and_nested_archives() {
    let tree = scan(archive(&sample_items(), None), None).unwrap();
    assert!(tree.errors.is_empty(), "{:?}", tree.errors);
    assert_eq!(tree.archives.len(), 2);
    let root = tree.archive(0);
    assert_eq!(root.encryption, RpfEncryption::Open);
    assert_eq!(root.dir_children(0).len(), 3);
    assert_eq!(root.root().name, "test.rpf", "the root directory takes the archive's name");

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
    let tree = scan(archive(&sample_items(), None), None).unwrap();
    let r = tree.find("test.rpf/levels/prop.ydr").unwrap();
    let res = tree.extract_resource(r).unwrap();
    assert_eq!(res.data, b"payload-of-a-drawable");
    assert_eq!(res.version, (6 << 4) + 9);
    assert_eq!(tree.entry(r).display_size(), 8192);
    let exported = tree.export(r).unwrap();
    assert_eq!(u32::from_le_bytes(exported[0..4].try_into().unwrap()), RSC7_MAGIC);
    assert_eq!(u32::from_le_bytes(exported[4..8].try_into().unwrap()), res.version);
    assert_eq!(&inflate(&exported[16..], 0).unwrap(), b"payload-of-a-drawable");
}

#[test]
fn aes_table_of_contents_needs_and_uses_the_key() {
    let keys = Arc::new(GtaKeys::synthetic());
    let bytes = archive(&sample_items(), Some(&keys.aes_key));

    assert!(matches!(scan(bytes.clone(), None), Err(Error::MissingKeys)));

    let tree = scan(bytes, Some(keys)).unwrap();
    assert_eq!(tree.archive(0).encryption, RpfEncryption::Aes);
    let f = tree.find("test.rpf/readme.txt").unwrap();
    assert_eq!(tree.extract(f).unwrap(), b"hello rpf");
}

#[test]
fn search_finds_files_but_not_directories() {
    let tree = scan(archive(&sample_items(), None), None).unwrap();
    let mut hits: Vec<String> = tree.search("levels").map(|r| tree.entry(r).name.clone()).collect();
    hits.sort();
    assert_eq!(hits, ["Big.xml", "prop.ydr"]);
}

#[test]
fn rejects_garbage_without_panicking() {
    assert!(matches!(scan(vec![], None), Err(Error::InvalidRpf(_))));
    assert!(matches!(scan(vec![0u8; 64], None), Err(Error::InvalidRpf(_))));
    let mut ok = archive(&sample_items(), None);
    ok.truncate(40);
    assert!(matches!(scan(ok, None), Err(Error::InvalidRpf(_))));
    // absurd entry count
    let mut bad = archive(&sample_items(), None);
    bad[4..8].copy_from_slice(&u32::MAX.to_le_bytes());
    assert!(scan(bad, None).is_err());
}

/// Every entry is reachable from the root exactly once, and a directory's children are exactly
/// the entries that name it as their parent: walking the tree always ends.
fn assert_is_a_tree(a: &RpfArchive) {
    let mut seen = vec![false; a.entries.len()];
    let mut stack = vec![0u32];
    seen[0] = true;
    let mut steps = 0;
    while let Some(d) = stack.pop() {
        steps += 1;
        assert!(steps <= a.entries.len(), "walked more directories than there are entries");
        let EntryKind::Directory { first, count } = a.entries[d as usize].kind else { continue };
        for i in first..first + count {
            let i = i as usize;
            assert!(i < a.entries.len(), "child {i} is outside the table");
            assert!(!seen[i], "entry {i} is reachable twice");
            seen[i] = true;
            assert_eq!(a.entries[i].parent, Some(d), "entry {i} is listed under {d} but does not say so");
            stack.push(i as u32);
        }
    }
    for (i, e) in a.entries.iter().enumerate() {
        assert_eq!(seen[i], !e.path.is_empty(), "entry {i} {:?}: reachable iff it has a path", e.name);
    }
}

fn set_dir_range(bytes: &mut [u8], entry: usize, first: u32, count: u32) {
    let rec = 16 + entry * 16;
    bytes[rec + 8..rec + 12].copy_from_slice(&first.to_le_bytes());
    bytes[rec + 12..rec + 16].copy_from_slice(&count.to_le_bytes());
}

#[test]
fn hostile_directory_ranges_cannot_loop_or_overflow() {
    // entries: 0 root, 1 readme.txt, 2 Levels, 3 child.rpf, 4 Big.xml, 5 prop.ydr
    let good = scan(archive(&sample_items(), None), None).unwrap();
    assert_is_a_tree(good.archive(0));
    assert!(good.errors.is_empty());

    // the root claims everything from itself on: it cannot contain itself, so it contains nothing,
    // and what it pointed at is reported as unreachable instead of being invented into the tree
    let mut bytes = archive(&sample_items(), None);
    set_dir_range(&mut bytes, 0, 0, u32::MAX);
    let tree = scan(bytes, None).unwrap();
    assert_is_a_tree(tree.archive(0));
    assert!(tree.archive(0).dir_children(0).is_empty());
    assert!(tree.errors.iter().any(|e| e.message.contains("5 entries are not part of the directory tree")));

    // a directory whose range includes its own parent
    let mut bytes = archive(&sample_items(), None);
    set_dir_range(&mut bytes, 2, 0, 6);
    let tree = scan(bytes, None).unwrap();
    assert_is_a_tree(tree.archive(0));
    assert!(tree.archive(0).dir_children(2).is_empty());

    // two directories claiming the same children: the first one (the root) keeps them
    let mut bytes = archive(&sample_items(), None);
    set_dir_range(&mut bytes, 0, 1, 5); // the root now also covers Big.xml and prop.ydr
    let tree = scan(bytes, None).unwrap();
    assert_is_a_tree(tree.archive(0));
    assert_eq!(tree.archive(0).dir_children(0).len(), 5);
    assert!(tree.archive(0).dir_children(2).is_empty(), "Levels lost what the root already had");

    // an out-of-range start, and a count that would overflow
    for (first, count) in [(u32::MAX, 1), (7, 3), (3, u32::MAX), (u32::MAX, u32::MAX)] {
        let mut bytes = archive(&sample_items(), None);
        set_dir_range(&mut bytes, 2, first, count);
        let tree = scan(bytes, None).unwrap();
        assert_is_a_tree(tree.archive(0));
    }
}

#[test]
fn entry_data_outside_the_archive_is_an_error() {
    let mut t = scan(archive(&[Item::file("a.bin", vec![1u8; 10])], None), None).unwrap();
    if let EntryKind::Binary { offset, .. } = &mut Arc::get_mut(&mut t.archives[0]).unwrap().entries[1].kind {
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

#[test]
fn builder_rejects_bad_names() {
    let bad = RpfBuilder::new(vec![Item::file("a/b.txt", &b"x"[..])]).build();
    assert!(matches!(bad, Err(Error::InvalidRpf(_))));
    assert!(RpfBuilder::new(vec![Item::file("", &b"x"[..])]).build().is_err());
    assert!(RpfBuilder::new(vec![Item::dir("ok", vec![Item::file("nul\0", &b"x"[..])])]).build().is_err());
}

#[test]
fn merged_trees_share_archives_and_rebase_ids() {
    let a = scan(archive(&sample_items(), None), None).unwrap();
    let b = scan(
        archive(&[Item::archive("x.rpf", archive(&[Item::file("y.txt", &b"yy"[..])], None))], None),
        None,
    )
    .unwrap();
    let m = a.merged(b);
    assert_eq!(m.archives.len(), 4);
    assert_eq!(m.roots, [0, 2]);
    assert!(Arc::ptr_eq(&a.archives[0], &m.archives[0]), "existing archives are shared, not copied");
    let y = m.find("test.rpf/x.rpf/y.txt").expect("found in the second tree");
    assert_eq!(y.archive, 3);
    assert_eq!(m.extract(y).unwrap(), b"yy");
    assert_eq!(m.archive(3).parent, Some(2));
    // the original tree is unchanged
    assert_eq!(a.archives.len(), 2);
}

/// Builds an archive holding one nested archive, then rewrites the nested
/// entry's block offset / size inside the TOC. Entry 1 is the nested file.
fn with_nested_entry(offset_blocks: u64, size: u64) -> Vec<u8> {
    let inner = archive(&[Item::file("x.txt", &b"x"[..])], None);
    let mut bytes = archive(&[Item::archive("c.rpf", inner)], None);
    let at = 16 + 16; // header, then root entry, then entry 1
    let old = u64::from_le_bytes(bytes[at..at + 8].try_into().unwrap());
    let name_off = old & 0xFFFF;
    let packed = name_off | (size << 16) | (offset_blocks << 40);
    bytes[at..at + 8].copy_from_slice(&packed.to_le_bytes());
    bytes[at + 8..at + 12].copy_from_slice(&(size as u32).to_le_bytes());
    bytes
}

#[test]
fn an_archive_that_contains_itself_is_rejected_not_expanded() {
    let len = archive(&[Item::archive("c.rpf", archive(&[Item::file("x.txt", &b"x"[..])], None))], None).len()
        as u64;
    let bytes = with_nested_entry(0, len); // the "child" starts at block 0: the file itself
    let tree = scan(bytes, None).unwrap();
    assert_eq!(tree.archives.len(), 1, "no recursion happened");
    assert!(tree.errors.iter().any(|e| e.message.contains("inside its parent")), "{:?}", tree.errors);
}

#[test]
fn children_must_lie_inside_the_parents_data_area() {
    let inner_len = archive(&[Item::file("x.txt", &b"x"[..])], None).len() as u64;
    // starts inside the parent's own header/TOC
    assert!(scan(with_nested_entry(0, inner_len), None).unwrap().errors.len() == 1);
    // runs past the end of the parent
    let tree = scan(with_nested_entry(1, inner_len * 100), None).unwrap();
    assert_eq!(tree.archives.len(), 1);
    assert_eq!(tree.errors.len(), 1);
    // a well-formed child is still accepted
    let ok = scan(
        archive(&[Item::archive("c.rpf", archive(&[Item::file("x.txt", &b"x"[..])], None))], None),
        None,
    )
    .unwrap();
    assert!(ok.errors.is_empty() && ok.archives.len() == 2);
}

#[test]
fn scan_budgets_stop_pathological_fan_out() {
    // a chain of ten archives nested in each other: legal, but over a budget of 4
    let mut inner = archive(&[Item::file("leaf.txt", &b"x"[..])], None);
    for _ in 0..9 {
        inner = archive(&[Item::archive("n.rpf", inner)], None);
    }
    let limits = ScanLimits { max_archives: 4, max_entries: 1_000_000 };
    let tree = RpfTree::scan_blob_with(
        Blob::from_vec(inner.clone()),
        "t.rpf".into(),
        "t.rpf".into(),
        "t.rpf",
        None,
        limits,
    )
    .unwrap();
    assert!(tree.archives.len() <= 4, "stopped at the budget, got {}", tree.archives.len());
    assert!(!tree.errors.is_empty());
    // the same file is fine with the default budget
    let all = scan(inner.clone(), None).unwrap();
    assert!(all.archives.len() >= 8, "nesting depth is capped separately: {}", all.archives.len());

    // an entry budget spent in a nested archive keeps what was read and reports the rest
    let few = ScanLimits { max_archives: 100, max_entries: 3 };
    let kept = RpfTree::scan_blob_with(
        Blob::from_vec(inner.clone()),
        "t.rpf".into(),
        "t.rpf".into(),
        "t.rpf",
        None,
        few,
    )
    .unwrap();
    assert!(kept.archives.len() < 10, "stopped descending, got {}", kept.archives.len());
    assert!(!kept.errors.is_empty());

    // one that the top-level table itself exceeds is rejected outright
    let tiny = ScanLimits { max_archives: 100, max_entries: 1 };
    let r =
        RpfTree::scan_blob_with(Blob::from_vec(inner), "t.rpf".into(), "t.rpf".into(), "t.rpf", None, tiny);
    assert!(r.is_err(), "the top-level table alone has two entries");
}

#[test]
fn a_small_file_cannot_inflate_past_its_declared_size() {
    let bomb = vec![0u8; 400_000];
    let mut bytes = archive(&[Item::packed("bomb.bin", bomb.clone())], None);
    let tree = scan(bytes.clone(), None).unwrap();
    let r = tree.find("test.rpf/bomb.bin").unwrap();
    assert_eq!(tree.extract(r).unwrap(), bomb, "an honest size is accepted");

    // entry 1 is bomb.bin; its uncompressed size sits 8 bytes into the 16-byte record
    let at = 16 + 16 + 8;
    bytes[at..at + 4].copy_from_slice(&16u32.to_le_bytes());
    let tree = scan(bytes, None).unwrap();
    let r = tree.find("test.rpf/bomb.bin").unwrap();
    assert!(matches!(tree.extract(r), Err(Error::TooLarge(..))), "lying size must be refused");
}

#[test]
fn resources_are_limited_by_their_page_flags() {
    let flags = (0x6000_0000 | (1 << 17), 0x9000_0000); // 8 KiB system, nothing graphics
    let tree = scan(archive(&[Item::resource("big.ydr", vec![0u8; 300_000], flags.0, flags.1)], None), None)
        .unwrap();
    let r = tree.find("test.rpf/big.ydr").unwrap();
    assert!(matches!(tree.extract_resource(r), Err(Error::TooLarge(..))));

    let exported = tree.export(r).unwrap();
    assert!(matches!(crate::resource::parse_loose(&exported), Err(Error::TooLarge(..))));

    let ok =
        scan(archive(&[Item::resource("ok.ydr", vec![7u8; 6_000], flags.0, flags.1)], None), None).unwrap();
    assert_eq!(ok.extract_resource(ok.find("test.rpf/ok.ydr").unwrap()).unwrap().data.len(), 6_000);
}

#[test]
fn the_absolute_cap_stops_a_stream_with_no_declared_size() {
    let z = {
        use std::io::Write;
        let mut e = flate2::write::DeflateEncoder::new(Vec::new(), flate2::Compression::fast());
        e.write_all(&vec![0u8; 1 << 20]).unwrap();
        e.finish().unwrap()
    };
    assert_eq!(inflate_capped(&z, 1 << 20).unwrap().len(), 1 << 20);
    assert!(matches!(inflate_capped(&z, (1 << 20) - 1), Err(Error::TooLarge(..))));
}

fn plain_and_big() -> (Vec<u8>, Vec<u8>) {
    // 53 bytes: three blocks and a partial one, which encryption must leave alone
    let short: Vec<u8> = (0..53u8).collect();
    let big = "<xml>compressible</xml>".repeat(120).into_bytes();
    (short, big)
}

#[test]
fn aes_encrypted_entries_are_decrypted_then_inflated() {
    let keys = Arc::new(GtaKeys::synthetic());
    let (short, big) = plain_and_big();
    let bytes = archive(
        &[
            Item::encrypted("short.bin", short.clone(), false),
            Item::encrypted("big.xml", big.clone(), true),
            Item::file("open.txt", &b"not encrypted"[..]),
            Item::resource("script.ysc", &b"script-bytes-needing-decryption"[..], 1 << 17, 0x9000_0000),
        ],
        Some(&keys.aes_key),
    );
    let tree = scan(bytes.clone(), Some(keys)).unwrap();
    let get = |n: &str| tree.extract(tree.find(&format!("test.rpf/{n}")).unwrap()).unwrap();
    assert_eq!(get("short.bin"), short);
    assert_eq!(get("big.xml"), big);
    assert_eq!(get("open.txt"), b"not encrypted");
    assert_eq!(get("script.ysc"), b"script-bytes-needing-decryption");

    let flags: Vec<bool> = ["short.bin", "big.xml", "open.txt", "script.ysc"]
        .iter()
        .map(|n| match tree.entry(tree.find(&format!("test.rpf/{n}")).unwrap()).kind {
            EntryKind::Binary { encrypted, .. } | EntryKind::Resource { encrypted, .. } => encrypted,
            EntryKind::Directory { .. } => unreachable!(),
        })
        .collect();
    assert_eq!(flags, [true, true, false, true]);
    // the bytes on disk really are not the plaintext
    assert!(!bytes.windows(big.len().min(32)).any(|w| w == &big[..big.len().min(32)]));
}

#[test]
fn ng_entries_are_keyed_by_name_and_uncompressed_size() {
    let keys = Arc::new(GtaKeys::synthetic_invertible());
    let (short, big) = plain_and_big();
    let build = |names: [&str; 2]| {
        RpfBuilder::new(vec![
            Item::encrypted(names[0], short.clone(), false),
            Item::encrypted(names[1], big.clone(), true),
            Item::resource("main.ysc", &b"ng-encrypted-script"[..], 1 << 17, 0x9000_0000),
        ])
        .with_ng_entries(keys.clone())
        .build()
        .unwrap()
    };
    let bytes = build(["short.bin", "big.xml"]);
    assert!(
        matches!(
            scan(bytes.clone(), None).and_then(|t| {
                let r = t.find("test.rpf/short.bin").unwrap();
                t.extract(r)
            }),
            Err(Error::MissingKeys)
        ),
        "NG entries need the keys too"
    );

    let tree = scan(bytes.clone(), Some(keys.clone())).unwrap();
    let get = |n: &str| tree.extract(tree.find(&format!("test.rpf/{n}")).unwrap()).unwrap();
    assert_eq!(get("short.bin"), short);
    assert_eq!(get("big.xml"), big);
    assert_eq!(get("main.ysc"), b"ng-encrypted-script");

    // a different uncompressed size selects a different key: no silent wrong answer
    let mut lying = bytes.clone();
    // entries: 0 root, 1 short.bin, 2 big.xml, 3 main.ysc; uncompressed size is 8 bytes into a record
    let at = 16 + 2 * 16 + 8;
    lying[at..at + 4].copy_from_slice(&(big.len() as u32 + 1).to_le_bytes());
    let tree = scan(lying, Some(keys.clone())).unwrap();
    let r = tree.find("test.rpf/big.xml").unwrap();
    assert!(tree.extract(r).map_or(true, |d| d != big), "wrong key must not yield the original");

    // and so does a different name: renamed in the (open) names table, the same bytes no longer decrypt
    let mut renamed = bytes;
    let at = renamed.windows(8).position(|w| w == b"big.xml\0").expect("the name is stored in clear");
    renamed[at..at + 7].copy_from_slice(b"bag.xml");
    let tree = scan(renamed, Some(keys)).unwrap();
    let r = tree.find("test.rpf/bag.xml").unwrap();
    assert!(tree.extract(r).map_or(true, |d| d != big), "the key follows the name");
}

#[test]
fn encrypted_entries_without_any_keys_in_the_builder_are_refused() {
    let r = RpfBuilder::new(vec![Item::encrypted("a.bin", vec![1u8; 20], false)]).build();
    assert!(matches!(r, Err(Error::InvalidRpf(m)) if m.contains("no keys")));
}

#[test]
fn sizes_of_16_mib_and_more_are_read_from_the_resource_header() {
    let mut bytes = archive(&[Item::resource("huge.ydr", &b"x"[..], 1 << 17, 0x9000_0000)], None);
    // the TOC size field of entry 1 (record bytes 2..5) says "see the header" ...
    let rec = 16 + 16;
    bytes[rec + 2..rec + 5].copy_from_slice(&[0xFF, 0xFF, 0xFF]);
    // ... and the header (at the entry's block) keeps the real size in bytes 7, 14, 5 and 2
    let block =
        (u32::from_le_bytes([bytes[rec + 5], bytes[rec + 6], bytes[rec + 7], 0]) & 0x7F_FFFF) as usize;
    let at = block * 512;
    let want: u32 = 0x0123_4567;
    bytes[at + 7] = want as u8;
    bytes[at + 14] = (want >> 8) as u8;
    bytes[at + 5] = (want >> 16) as u8;
    bytes[at + 2] = (want >> 24) as u8;
    let tree = scan(bytes, None).unwrap();
    let e = tree.entry(tree.find("test.rpf/huge.ydr").unwrap());
    assert!(matches!(e.kind, EntryKind::Resource { size, .. } if size == want), "{:?}", e.kind);
}
