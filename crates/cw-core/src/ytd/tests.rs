use super::*;
use crate::{
    Blob, RpfTree,
    rpf::build::{Item, RpfBuilder},
    texture::{Format, mip_range},
};

fn tex(format: Format, w: u32, h: u32, levels: u32, seed: u8) -> Texture {
    let n = chain_size(format, w, h, levels);
    Texture {
        width: w,
        height: h,
        levels,
        format,
        data: (0..n).map(|i| (i as u8).wrapping_mul(31).wrapping_add(seed)).collect(),
    }
}

#[test]
fn built_dictionaries_parse_back_exactly() {
    let a = tex(Format::Dxt1, 64, 32, 3, 1);
    let b = tex(Format::A8R8G8B8, 16, 16, 1, 2);
    let c = tex(Format::Dxt5, 128, 128, 5, 3);
    let res = build_ytd(&[("Road_Diffuse", &a), ("lamp_n", &b), ("Glass", &c)]);
    assert_eq!(res.version, 13, "legacy ytd: system nibble 0, graphics nibble 0xD");

    let got = parse_ytd(&res).unwrap();
    assert_eq!(got.len(), 3);
    assert_eq!(got.iter().map(|t| t.name.as_str()).collect::<Vec<_>>(), ["Road_Diffuse", "lamp_n", "Glass"]);
    assert_eq!(got[0].name_hash, joaat_lower("road_diffuse"), "hash list uses lower-case names");
    assert_eq!(got[0].texture, a);
    assert_eq!(got[1].texture, b);
    assert_eq!(got[2].texture, c);
}

#[test]
fn parsed_textures_decode_to_pixels() {
    let mut solid = vec![0u8; 8];
    let red = 0xF800u16;
    solid[0..2].copy_from_slice(&red.to_le_bytes());
    solid[2..4].copy_from_slice(&red.to_le_bytes());
    let t = Texture { width: 4, height: 4, levels: 1, format: Format::Dxt1, data: solid };
    let parsed = parse_ytd(&build_ytd(&[("red", &t)])).unwrap();
    let img = parsed[0].texture.decode(0).unwrap();
    assert_eq!(&img.pixels[..4], [255, 0, 0, 255]);
    assert_eq!(mip_range(Format::Dxt1, 4, 4, 0), (0, 8));
}

#[test]
fn works_end_to_end_through_an_archive() {
    let a = tex(Format::Dxt1, 32, 32, 6, 9);
    let res = build_ytd(&[("wall", &a)]);
    let rpf =
        RpfBuilder::new(vec![Item::resource("wall.ytd", res.data.clone(), res.sys_flags, res.gfx_flags)])
            .build()
            .unwrap();
    let tree =
        RpfTree::scan_blob(Blob::from_vec(rpf), "t.rpf".into(), "t.rpf".into(), "t.rpf", None).unwrap();
    let extracted = tree.extract_resource(tree.find("t.rpf/wall.ytd").unwrap()).unwrap();
    assert_eq!(extracted.version, 13);
    let got = parse_ytd(&extracted).unwrap();
    assert_eq!(got[0].texture, a);

    // and as a loose OpenIV-style file
    let loose =
        crate::resource::parse_loose(&tree.export(tree.find("t.rpf/wall.ytd").unwrap()).unwrap()).unwrap();
    assert_eq!(parse_ytd(&loose).unwrap()[0].texture, a);
}

#[test]
fn gen9_and_foreign_versions_are_reported_not_misread() {
    let mut res = build_ytd(&[("x", &tex(Format::Dxt1, 4, 4, 1, 0))]);
    res.version = 5; // gen9 dictionary
    let e = parse_ytd(&res).unwrap_err().to_string();
    assert!(e.contains("version 5") && e.contains("not supported"), "{e}");
    res.version = 165; // a drawable, not a dictionary
    assert!(parse_ytd(&res).is_err());
}

#[test]
fn empty_and_hostile_dictionaries_do_not_panic() {
    // no textures
    assert!(parse_ytd(&build_ytd(&[])).unwrap().is_empty());

    let base = build_ytd(&[("x", &tex(Format::Dxt1, 4, 4, 1, 0))]);

    // absurd texture count is clamped; pointers beyond the segment read as zeros
    let mut huge = base.clone();
    huge.data[0x38..0x3A].copy_from_slice(&u16::MAX.to_le_bytes());
    let many = parse_ytd(&huge).unwrap();
    assert_eq!(many.len(), 1, "null pointers are skipped, only the real texture remains");

    // a pointer array entry aimed outside the segment is skipped, not read as zeros
    let mut wild = base.clone();
    let ptrs_at = u64::from_le_bytes(wild.data[0x30..0x38].try_into().unwrap()) as usize & 0x0FFF_FFFF;
    wild.data[ptrs_at..ptrs_at + 8].copy_from_slice(&0x5FFF_FFF0u64.to_le_bytes());
    assert!(parse_ytd(&wild).unwrap().is_empty());

    // truncated payload: header fields still parse, pixels are zero-filled
    let mut cut = base.clone();
    let sys = crate::rpf::page_flags_size(cut.sys_flags) as usize;
    cut.data.truncate(sys); // graphics segment missing entirely
    let r = parse_ytd(&cut).unwrap();
    assert_eq!(r[0].texture.width, 4);
    assert!(r[0].texture.data.iter().all(|&b| b == 0));

    // nothing at all
    let empty = ResourceData { sys_flags: 0, gfx_flags: 0xD000_0000, version: 13, data: Vec::new() };
    assert!(parse_ytd(&empty).unwrap().is_empty());
}

#[test]
fn unknown_formats_and_odd_level_counts_are_survivable() {
    let weird = Texture { width: 8, height: 8, levels: 1, format: Format::Unknown(0x1234), data: Vec::new() };
    let mut res = build_ytd(&[("odd", &weird)]);
    let parsed = parse_ytd(&res).unwrap();
    assert_eq!(parsed[0].texture.format, Format::Unknown(0x1234));
    assert!(parsed[0].texture.decode(0).is_err(), "undecodable, but still listed");

    // zero mip levels in the file means one
    let a = tex(Format::A8, 8, 8, 1, 0);
    res = build_ytd(&[("a", &a)]);
    let tex_ptr = u64::from_le_bytes(res.data[0x30..0x38].try_into().unwrap()) as usize & 0x0FFF_FFFF;
    let tex_addr =
        u64::from_le_bytes(res.data[tex_ptr..tex_ptr + 8].try_into().unwrap()) as usize & 0x0FFF_FFFF;
    res.data[tex_addr + 0x5D] = 0;
    assert_eq!(parse_ytd(&res).unwrap()[0].texture.levels, 1);
}

/// Address (inside the resource) of texture header `i`.
fn header_at(res: &ResourceData, i: usize) -> usize {
    let ptrs = u64::from_le_bytes(res.data[0x30..0x38].try_into().unwrap()) as usize & 0x0FFF_FFFF;
    u64::from_le_bytes(res.data[ptrs + i * 8..ptrs + i * 8 + 8].try_into().unwrap()) as usize & 0x0FFF_FFFF
}

#[test]
fn pixel_data_cannot_add_up_to_more_than_the_file_holds() {
    let a = tex(Format::Dxt1, 16, 16, 1, 1);
    let names: Vec<String> = (0..12).map(|i| format!("t{i}")).collect();
    let refs: Vec<(&str, &Texture)> = names.iter().map(|n| (n.as_str(), &a)).collect();
    let mut res = build_ytd(&refs);
    let first_data = {
        let h = header_at(&res, 0);
        res.data[h + 0x70..h + 0x78].to_vec()
    };
    // every header now claims 128x128 DXT1 (8 KiB) read from the same place: 96 KiB in all,
    // from a resource of 16 KiB
    for i in 0..12 {
        let h = header_at(&res, i);
        res.data[h + 0x50..h + 0x52].copy_from_slice(&128u16.to_le_bytes());
        res.data[h + 0x52..h + 0x54].copy_from_slice(&128u16.to_le_bytes());
        res.data[h + 0x70..h + 0x78].copy_from_slice(&first_data);
    }
    let parsed = parse_ytd(&res).unwrap();
    assert_eq!(parsed.len(), 12, "all are listed");
    let held: usize = parsed.iter().map(|t| t.texture.data.len()).sum();
    assert!(held <= res.data.len(), "{held} bytes of pixels from a {} byte file", res.data.len());
    assert!(
        held > 0 && parsed.iter().any(|t| t.texture.data.is_empty()),
        "the first ones fit, the rest do not"
    );
}

#[test]
fn a_header_claiming_a_giant_picture_costs_nothing() {
    let a = tex(Format::A8R8G8B8, 8, 8, 1, 1);
    let mut res = build_ytd(&[("a", &a)]);
    let h = header_at(&res, 0);
    // 16383 x 16383 with a full mip chain would be gigabytes
    res.data[h + 0x50..h + 0x52].copy_from_slice(&16383u16.to_le_bytes());
    res.data[h + 0x52..h + 0x54].copy_from_slice(&16383u16.to_le_bytes());
    res.data[h + 0x5D] = 14;
    let parsed = parse_ytd(&res).unwrap();
    assert_eq!((parsed[0].texture.width, parsed[0].texture.levels), (16383, 14));
    assert!(parsed[0].texture.data.is_empty(), "no pixels were allocated for it");
    // and decoding what has no pixels is refused rather than attempted at that size
    assert!(parsed[0].texture.decode(0).is_err());
}
