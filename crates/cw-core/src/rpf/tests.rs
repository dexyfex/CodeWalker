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
    assert_eq!(&inflate(&exported[16..]).unwrap(), b"payload-of-a-drawable");
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

#[test]
fn hostile_directory_ranges_cannot_loop_or_overflow() {
    let mut bytes = archive(&sample_items(), None);
    // root now claims entries [0 ..= u32::MAX): itself and everything else
    bytes[16 + 8..16 + 12].copy_from_slice(&0u32.to_le_bytes());
    bytes[16 + 12..16 + 16].copy_from_slice(&u32::MAX.to_le_bytes());
    let tree = scan(bytes, None).unwrap();
    assert!(tree.archive(0).entries.len() > 1);
}

#[test]
fn entry_data_outside_the_archive_is_an_error() {
    let mut t = scan(archive(&[Item::file("a.bin", vec![1u8; 10])], None), None).unwrap();
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

#[test]
fn builder_rejects_bad_names() {
    let bad = RpfBuilder::new(vec![Item::file("a/b.txt", &b"x"[..])]).build();
    assert!(matches!(bad, Err(Error::InvalidRpf(_))));
    assert!(RpfBuilder::new(vec![Item::file("", &b"x"[..])]).build().is_err());
    assert!(RpfBuilder::new(vec![Item::dir("ok", vec![Item::file("nul\0", &b"x"[..])])]).build().is_err());
}
