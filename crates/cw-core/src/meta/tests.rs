use super::*;
use crate::rpf::{flags_for_size, page_flags_size};

fn blocks() -> Vec<(u32, Vec<u8>)> {
    vec![(0xAAAA_0001, vec![1, 2, 3, 4, 5]), (TYPE_POINTER, vec![0; 16]), (0xBBBB_0002, (0..200u8).collect())]
}

#[test]
fn flag_words_describe_at_least_the_size_they_were_made_for() {
    for len in [0usize, 1, 8192, 8193, 127 * 8192, 127 * 8192 + 1, 1 << 20, 5_000_000, 100_000_000] {
        let f = flags_for_size(len, 2);
        assert!(page_flags_size(f) as usize >= len, "{len}: {}", page_flags_size(f));
        assert_eq!(f >> 28, 2, "version nibble");
        // and not wildly more: under twice the size, or one minimum page
        assert!((page_flags_size(f) as usize) < len.max(8192) * 2, "{len} -> {}", page_flags_size(f));
    }
}

#[test]
fn pointers_round_trip_and_split_into_block_and_offset() {
    let p = MetaPointer { block: 7, offset: 0x12345 };
    assert_eq!(MetaPointer::from_raw(p.to_raw()), p);
    assert_eq!(p.to_raw(), (0x12345 << 12) | 7);
    // the high 32 bits are not part of the pointer
    assert_eq!(MetaPointer::from_raw(0xFFFF_FFFF_0000_1002), MetaPointer { block: 2, offset: 1 });
}

#[test]
fn built_files_parse_back_block_for_block() {
    let res = build_meta(3, &blocks());
    let meta = Meta::parse(&res).unwrap();
    assert_eq!(meta.root_block, 3);
    assert_eq!(meta.blocks.len(), 3);
    for (got, (hash, data)) in meta.blocks.iter().zip(blocks()) {
        assert_eq!((got.type_hash, &got.data), (hash, &data));
    }
    assert_eq!(meta.root().unwrap().type_hash, 0xBBBB_0002);
    assert_eq!(meta.block(1).unwrap().data, [1, 2, 3, 4, 5]);
    assert!(meta.block(0).is_none() && meta.block(4).is_none(), "ids are 1-based and bounded");
    assert_eq!(meta.first_of(TYPE_POINTER).unwrap().data.len(), 16);
    assert!(meta.first_of(0xDEAD).is_none());
}

#[test]
fn a_pointer_list_is_read_through_its_pointer_block() {
    // block 1: the owner, holding an Array_StructurePointer at 8 -> block 2 offset 8, three entries
    let mut owner = vec![0u8; 32];
    owner[8..16].copy_from_slice(&MetaPointer { block: 2, offset: 8 }.to_raw().to_le_bytes());
    owner[16..18].copy_from_slice(&3u16.to_le_bytes());
    // block 2: padding, then three pointers
    let mut list = vec![0u8; 8];
    for (b, o) in [(3, 0), (3, 4), (9, 0)] {
        list.extend(MetaPointer { block: b, offset: o }.to_raw().to_le_bytes());
    }
    let meta =
        Meta::parse(&build_meta(1, &[(0x11, owner.clone()), (TYPE_POINTER, list), (0x22, vec![0; 8])]))
            .unwrap();
    let got = meta.pointer_array(&meta.blocks[0].data, 8);
    assert_eq!(got.iter().map(|p| (p.block, p.offset)).collect::<Vec<_>>(), [(3, 0), (3, 4), (9, 0)]);

    // a count that runs off the end of the list yields what is there
    let mut short = owner.clone();
    short[16..18].copy_from_slice(&500u16.to_le_bytes());
    assert_eq!(meta.pointer_array(&short, 8).len(), 3);
    // a list in a block of the wrong type is not a list
    let mut wrong = owner.clone();
    wrong[8..16].copy_from_slice(&MetaPointer { block: 3, offset: 0 }.to_raw().to_le_bytes());
    assert!(meta.pointer_array(&wrong, 8).is_empty());
    // and an array record that is not even in the data
    assert!(meta.pointer_array(&owner, 30).is_empty());
}

#[test]
fn structures_must_be_of_the_asked_type_and_inside_their_block() {
    let meta = Meta::parse(&build_meta(1, &[(0x22, (0..16u8).collect())])).unwrap();
    let at = |offset| MetaPointer { block: 1, offset };
    assert_eq!(meta.structure(at(4), 0x22, 8).unwrap(), &(4..12u8).collect::<Vec<_>>()[..]);
    assert!(meta.structure(at(4), 0x23, 8).is_none(), "another type");
    assert!(meta.structure(at(12), 0x22, 8).is_none(), "runs past the end");
    assert!(meta.structure(at(0xFFFFF), 0x22, usize::MAX).is_none(), "size overflow");
    assert!(meta.structure(MetaPointer { block: 0, offset: 0 }, 0x22, 1).is_none(), "null");
}

#[test]
fn hostile_tables_are_refused_or_emptied_never_followed() {
    let good = build_meta(1, &blocks());
    let patch = |f: &dyn Fn(&mut Vec<u8>)| {
        let mut r = good.clone();
        f(&mut r.data);
        r
    };
    // no table, no blocks, too many blocks, a table outside the resource
    assert!(Meta::parse(&patch(&|d| d[0x30..0x38].fill(0))).is_err());
    assert!(Meta::parse(&patch(&|d| d[0x4C..0x4E].fill(0))).is_err());
    assert!(Meta::parse(&patch(&|d| d[0x4C..0x4E].copy_from_slice(&u16::MAX.to_le_bytes()))).is_err());
    assert!(Meta::parse(&patch(&|d| d[0x30..0x38].copy_from_slice(&0x5FFF_F000u64.to_le_bytes()))).is_err());
    assert!(Meta::parse(&ResourceData { data: vec![0; 64], ..good.clone() }).is_err(), "a truncated payload");

    // a block that claims more than the resource holds (or a negative length) is emptied
    let table = u64::from_le_bytes(good.data[0x30..0x38].try_into().unwrap()) as usize & 0x0FFF_FFFF;
    let huge =
        Meta::parse(&patch(&|d| d[table + 4..table + 8].copy_from_slice(&i32::MAX.to_le_bytes()))).unwrap();
    assert!(huge.blocks[0].data.is_empty() && huge.blocks[2].data.len() == 200, "only that block is lost");
    let negative =
        Meta::parse(&patch(&|d| d[table + 4..table + 8].copy_from_slice(&(-5i32).to_le_bytes()))).unwrap();
    assert!(negative.blocks[0].data.is_empty());

    // many blocks that each claim most of the resource cannot add up to more than it holds
    let big = build_meta(1, &vec![(1, vec![7u8; 4000]); 6]);
    let mut shared = big.clone();
    let t = u64::from_le_bytes(shared.data[0x30..0x38].try_into().unwrap()) as usize & 0x0FFF_FFFF;
    let first_addr: [u8; 8] = shared.data[t + 8..t + 16].try_into().unwrap();
    let half = (shared.data.len() / 2) as i32;
    for i in 1..6 {
        shared.data[t + i * 16 + 8..t + i * 16 + 16].copy_from_slice(&first_addr); // all overlap block 1
        shared.data[t + i * 16 + 4..t + i * 16 + 8].copy_from_slice(&half.to_le_bytes());
    }
    let parsed = Meta::parse(&shared).unwrap();
    let total: usize = parsed.blocks.iter().map(|b| b.data.len()).sum();
    assert!(total <= shared.data.len(), "{total} bytes from a {} byte resource", shared.data.len());
}
