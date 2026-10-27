use super::*;
use crate::{
    Blob, RpfTree,
    dotnet_random::DotNetRandom,
    rpf::build::{Item, RpfBuilder},
};

/// (type hash, bytes) of each META block.
type Blocks = Vec<(u32, Vec<u8>)>;

fn entity(i: u32) -> YmapEntity {
    YmapEntity {
        archetype: 0xC0DE_0000 + i,
        guid: 1000 + i,
        position: [i as f32, -(i as f32) * 2.0, 10.5 + i as f32],
        scale_xy: 1.0 + i as f32 * 0.25,
        scale_z: 2.0,
        ..Default::default()
    }
}

fn sample() -> Ymap {
    let mut hd = entity(0);
    hd.parent_index = 3;
    hd.rotation = [0.0, 0.0, 0.70710677, 0.70710677];
    let mut orphan = entity(1);
    orphan.lod = LodLevel::OrphanHd;
    let mut lod = entity(2);
    lod.lod = LodLevel::Lod;
    lod.num_children = 1;
    lod.parent_index = 4;
    let mut slod = entity(3);
    slod.lod = LodLevel::Slod1;
    let mut interior = entity(4);
    interior.mlo = true;
    Ymap {
        name: 0x1234_5678,
        parent: 0x9ABC_DEF0,
        flags: 1,
        streaming_extents: ([-100.0, -200.0, -10.0], [300.0, 400.0, 90.0]),
        entity_extents: ([-50.0, -60.0, 0.0], [70.0, 80.0, 40.0]),
        // plain entities first, then interior instances: the order the parser returns
        entities: vec![hd, orphan, lod, slod, interior],
    }
}

#[test]
fn type_names_hash_to_the_values_the_original_tool_uses() {
    // `MetaNames.cs`: CMapData, CEntityDef and CMloInstanceDef
    assert_eq!(joaat("CMapData"), 3_545_841_574);
    assert_eq!(joaat("CEntityDef"), 3_461_354_627);
    assert_eq!(joaat("CMloInstanceDef"), 164_374_718);
}

#[test]
fn built_maps_parse_back_exactly() {
    let map = sample();
    assert_eq!(parse_ymap(&build_ymap(&map)).unwrap(), map);
    let empty = Ymap { name: 7, ..Default::default() };
    assert_eq!(parse_ymap(&build_ymap(&empty)).unwrap(), empty, "a map with no entities is a map");
}

#[test]
fn large_maps_are_split_over_blocks_and_keep_their_order() {
    let entities: Vec<YmapEntity> = (0..5000).map(entity).collect();
    let map = Ymap { entities, ..Default::default() };
    let res = build_ymap(&map);
    assert!(Meta::parse(&res).unwrap().blocks.len() > 4, "2048 entities to a block");
    let got = parse_ymap(&res).unwrap();
    assert_eq!(got.entities.len(), 5000);
    assert_eq!(got.entities, map.entities);
}

#[test]
fn maps_survive_a_trip_through_an_archive() {
    let res = build_ymap(&sample());
    let rpf =
        RpfBuilder::new(vec![Item::resource("hills.ymap", res.data.clone(), res.sys_flags, res.gfx_flags)])
            .build()
            .unwrap();
    let tree =
        RpfTree::scan_blob(Blob::from_vec(rpf), "t.rpf".into(), "t.rpf".into(), "t.rpf", None).unwrap();
    let r = tree.find("t.rpf/hills.ymap").unwrap();
    assert_eq!(parse_ymap(&tree.extract_resource(r).unwrap()).unwrap(), sample());
    // and as a loose OpenIV-style file
    let loose = crate::resource::parse_loose(&tree.export(r).unwrap()).unwrap();
    assert_eq!(parse_ymap(&loose).unwrap(), sample());
}

#[test]
fn the_stored_rotation_is_inverted_to_get_the_orientation() {
    let s = std::f32::consts::FRAC_1_SQRT_2;
    let e = YmapEntity { rotation: [0.0, 0.0, s, s], ..Default::default() }; // 90 degrees about Z
    let [x, y, z, w] = e.orientation();
    assert!(
        x.abs() < 1e-6 && y.abs() < 1e-6 && (z + s).abs() < 1e-6 && (w - s).abs() < 1e-6,
        "{:?}",
        e.orientation()
    );
    assert_eq!(YmapEntity::default().orientation(), [0.0, 0.0, 0.0, 1.0], "identity stays identity");
    // not normalised in the file: the inverse is still the true inverse
    let big = YmapEntity { rotation: [0.0, 0.0, 2.0 * s, 2.0 * s], ..Default::default() };
    let o = big.orientation();
    let prod = |a: [f32; 4], b: [f32; 4]| {
        // z-axis rotations only: (0,0,z,w)*(0,0,z',w') = (0,0,w z'+z w', w w' - z z')
        [0.0, 0.0, a[3] * b[2] + a[2] * b[3], a[3] * b[3] - a[2] * b[2]]
    };
    let identity = prod(big.rotation, o);
    assert!(identity[2].abs() < 1e-5 && (identity[3] - 1.0).abs() < 1e-5, "{identity:?}");
    // garbage never produces NaN
    let nan = YmapEntity { rotation: [f32::NAN, 0.0, 0.0, 1.0], ..Default::default() };
    assert_eq!(nan.orientation(), [0.0, 0.0, 0.0, 1.0]);
    assert_eq!(YmapEntity { rotation: [0.0; 4], ..Default::default() }.orientation(), [0.0, 0.0, 0.0, 1.0]);
    assert_eq!(YmapEntity { scale_xy: 2.0, scale_z: 3.0, ..Default::default() }.scale(), [2.0, 2.0, 3.0]);
}

#[test]
fn the_lod_tree_has_the_roots_the_original_rule_gives() {
    let at = |lod, parent, flags| YmapEntity { lod, parent_index: parent, flags, ..Default::default() };
    let map = Ymap {
        entities: vec![
            at(LodLevel::Slod1, -1, 0),   // 0: no parent
            at(LodLevel::Lod, 0, 0),      // 1: its parent is coarser
            at(LodLevel::Hd, 1, 0),       // 2: so is this one's
            at(LodLevel::Hd, 1, 8),       // 3: parent lives in another file
            at(LodLevel::Lod, 2, 0),      // 4: parent is finer (or equal): a root
            at(LodLevel::Hd, 99, 0),      // 5: parent index out of range
            at(LodLevel::OrphanHd, 5, 0), // 6: equal level
            at(LodLevel::Hd, 6, 0),       // 7: parent is an orphan, child is not
        ],
        ..Default::default()
    };
    let roots: Vec<usize> = (0..map.entities.len()).filter(|&i| map.is_lod_root(i)).collect();
    assert_eq!(roots, [0, 3, 4, 5, 6, 7]);
    assert!(!map.is_lod_root(100), "no such entity");
    assert_eq!(
        map.lod_counts(),
        [(LodLevel::Hd, 4), (LodLevel::Lod, 2), (LodLevel::Slod1, 1), (LodLevel::OrphanHd, 1)]
    );
    assert!(LodLevel::Hd.is_detailed() && LodLevel::OrphanHd.is_detailed() && !LodLevel::Lod.is_detailed());
    for raw in 0..9 {
        assert_eq!(LodLevel::from_raw(raw).to_raw(), raw);
    }
}

#[test]
fn damaged_maps_give_what_is_readable_or_an_error() {
    let good = build_ymap(&sample());
    let meta = Meta::parse(&good).unwrap();
    let rebuilt = |edit: &dyn Fn(&mut Blocks)| {
        let mut blocks: Blocks = meta.blocks.iter().map(|b| (b.type_hash, b.data.clone())).collect();
        edit(&mut blocks);
        parse_ymap(&build_meta(1, &blocks))
    };

    // no CMapData, or one too short to hold the entity list
    assert!(rebuilt(&|b| b[0].0 = 1).is_err());
    assert!(rebuilt(&|b| b[0].1.truncate(100)).is_err());
    // the pointer list is gone, or is not a pointer list: no entities, not an error
    assert!(rebuilt(&|b| b[1].0 = 5).unwrap().entities.is_empty());
    // a count that overruns the list reads what is there
    let overrun = rebuilt(&|b| b[0].1[104..106].copy_from_slice(&u16::MAX.to_le_bytes())).unwrap();
    assert_eq!(overrun.entities.len(), 5);
    // one pointer aimed at the wrong type of block, one past the end of its block: both skipped
    let skipped = rebuilt(&|b| {
        b[1].1[0..8].copy_from_slice(&MetaPointer { block: 1, offset: 0 }.to_raw().to_le_bytes());
        b[1].1[8..16].copy_from_slice(&MetaPointer { block: 3, offset: 0xFFFFF }.to_raw().to_le_bytes());
    })
    .unwrap();
    assert_eq!(skipped.entities.len(), 3);
    // a block that does not hold a whole structure is skipped, not half-read
    let cut = rebuilt(&|b| b[2].1.truncate(100)).unwrap();
    assert_eq!(cut.entities.len(), 1, "only the interior instance remains");

    let rpf = RpfBuilder::new(vec![Item::file("not_a_map.ymap", vec![0u8; 64])]).build().unwrap();
    let tree =
        RpfTree::scan_blob(Blob::from_vec(rpf), "t.rpf".into(), "t.rpf".into(), "t.rpf", None).unwrap();
    let raw = tree.extract(tree.find("t.rpf/not_a_map.ymap").unwrap()).unwrap();
    let as_res = ResourceData { sys_flags: 1 << 17, gfx_flags: 2 << 28, version: 2, data: raw };
    assert!(parse_ymap(&as_res).is_err(), "zeros are not a map");
}

#[test]
fn random_damage_never_panics_or_runs_away() {
    let good = build_ymap(&Ymap { entities: (0..300).map(entity).collect(), ..Default::default() });
    let mut rnd = DotNetRandom::new(2024);
    for round in 0..400 {
        let mut res = good.clone();
        for _ in 0..1 + round % 12 {
            let at = rnd.next_i32() as usize % res.data.len();
            res.data[at] = rnd.next_i32() as u8;
        }
        // the header and table are the interesting part: hit them more often
        let at = rnd.next_i32() as usize % 0x400;
        res.data[at] = rnd.next_i32() as u8;
        if let Ok(map) = parse_ymap(&res) {
            assert!(map.entities.len() <= u16::MAX as usize);
        }
    }
}
