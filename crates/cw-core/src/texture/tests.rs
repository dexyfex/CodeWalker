use super::*;

#[test]
fn format_codes_round_trip() {
    for f in [
        Format::A8R8G8B8,
        Format::X8R8G8B8,
        Format::A1R5G5B5,
        Format::A8,
        Format::L8,
        Format::A8B8G8R8,
        Format::Dxt1,
        Format::Dxt3,
        Format::Dxt5,
        Format::Ati1,
        Format::Ati2,
        Format::Bc7,
    ] {
        assert_eq!(Format::from_d3d(f.to_d3d()), f, "{f:?}");
        assert!(f.is_known());
    }
    assert_eq!(Format::from_d3d(0xDEAD), Format::Unknown(0xDEAD));
    assert_eq!(Format::Dxt1.to_d3d(), 0x3154_5844, "'DXT1' little-endian");
    assert_eq!(Format::Bc7.to_d3d(), 0x2037_4342, "'BC7 ' from the C# source");
}

#[test]
fn level_sizes_follow_block_rules() {
    assert_eq!(Format::Dxt1.level_size(256, 256), 64 * 64 * 8);
    assert_eq!(Format::Dxt5.level_size(256, 256), 64 * 64 * 16);
    assert_eq!(Format::Dxt1.level_size(1, 1), 8, "a 1x1 mip still occupies a whole block");
    assert_eq!(Format::Dxt1.level_size(2, 2), 8);
    assert_eq!(Format::Dxt1.level_size(5, 5), 4 * 8, "5 pixels need two blocks per axis");
    assert_eq!(Format::A8R8G8B8.level_size(7, 3), 7 * 3 * 4);
    assert_eq!(Format::A1R5G5B5.level_size(4, 4), 32);
    assert_eq!(Format::L8.level_size(8, 8), 64);
    assert_eq!(Format::Unknown(1).level_size(8, 8), 0);
}

#[test]
fn mip_chain_is_exact_not_the_legacy_stride_loop() {
    // 256x256 DXT1 with 9 levels: 8192 + 2048 + 512 + 128 + 32 + 8 + 8 + 8 + 8
    let sizes: Vec<usize> = (0..9).map(|l| mip_range(Format::Dxt1, 256, 256, l).1).collect();
    assert_eq!(sizes, [32768, 8192, 2048, 512, 128, 32, 8, 8, 8]);
    assert_eq!(chain_size(Format::Dxt1, 256, 256, 9), sizes.iter().sum::<usize>());
    assert_eq!(mip_range(Format::Dxt1, 256, 256, 2).0, 32768 + 8192);
    assert_eq!(mip_dims(256, 64, 3), (32, 8));
    assert_eq!(mip_dims(4, 1, 5), (1, 1), "dimensions never reach zero");
    assert_eq!(chain_size(Format::Dxt1, 16, 16, 0), 128, "0 levels is treated as 1");
}

fn solid_bc1(r5: u16, g6: u16, b5: u16) -> [u8; 8] {
    let c = (r5 << 11) | (g6 << 5) | b5;
    let mut blk = [0u8; 8];
    blk[0..2].copy_from_slice(&c.to_le_bytes());
    blk[2..4].copy_from_slice(&c.to_le_bytes());
    blk // indices 0: every pixel uses colour 0
}

#[test]
fn bc1_decodes_a_solid_block() {
    let t =
        Texture { width: 4, height: 4, levels: 1, format: Format::Dxt1, data: solid_bc1(31, 0, 0).to_vec() };
    let img = t.decode(0).unwrap();
    assert_eq!((img.width, img.height), (4, 4));
    for px in img.pixels.chunks(4) {
        assert_eq!(px, [255, 0, 0, 255]);
    }
}

#[test]
fn partial_blocks_are_clipped_to_the_image() {
    // 6x3 needs 2x1 blocks; two different solid blocks side by side
    let mut data = solid_bc1(31, 0, 0).to_vec();
    data.extend_from_slice(&solid_bc1(0, 0, 31));
    let img = decode_rgba(Format::Dxt1, 6, 3, &data).unwrap();
    assert_eq!(img.pixels.len(), 6 * 3 * 4);
    assert_eq!(&img.pixels[0..4], [255, 0, 0, 255], "left block is red");
    assert_eq!(&img.pixels[5 * 4..5 * 4 + 4], [0, 0, 255, 255], "pixel 5 belongs to the right block");
}

#[test]
fn missing_data_decodes_as_zeros_without_panicking() {
    let img = decode_rgba(Format::Dxt5, 8, 8, &[1, 2, 3]).unwrap();
    assert_eq!(img.pixels.len(), 8 * 8 * 4);
    let img2 = decode_rgba(Format::A8R8G8B8, 4, 4, &[]).unwrap();
    assert!(img2.pixels.iter().all(|&b| b == 0));
}

#[test]
fn uncompressed_formats_map_channels_correctly() {
    // D3DFMT_A8R8G8B8 is B,G,R,A in memory
    let img = decode_rgba(Format::A8R8G8B8, 1, 1, &[10, 20, 30, 40]).unwrap();
    assert_eq!(img.pixels, [30, 20, 10, 40]);
    let x = decode_rgba(Format::X8R8G8B8, 1, 1, &[10, 20, 30, 0]).unwrap();
    assert_eq!(x.pixels, [30, 20, 10, 255], "X8 has no alpha");
    // D3DFMT_A8B8G8R8 is R,G,B,A in memory
    let r = decode_rgba(Format::A8B8G8R8, 1, 1, &[1, 2, 3, 4]).unwrap();
    assert_eq!(r.pixels, [1, 2, 3, 4]);
    let a1 = decode_rgba(Format::A1R5G5B5, 2, 1, &[0xFF, 0xFF, 0x00, 0x00]).unwrap();
    assert_eq!(&a1.pixels[..4], [255, 255, 255, 255]);
    assert_eq!(&a1.pixels[4..], [0, 0, 0, 0]);
    assert_eq!(decode_rgba(Format::L8, 1, 1, &[77]).unwrap().pixels, [77, 77, 77, 255]);
    assert_eq!(decode_rgba(Format::A8, 1, 1, &[200]).unwrap().pixels, [200, 200, 200, 255]);
}

#[test]
fn hostile_sizes_and_formats_are_refused() {
    assert!(decode_rgba(Format::Dxt1, 0, 4, &[]).is_err());
    assert!(decode_rgba(Format::Dxt1, 100_000, 100_000, &[]).is_err(), "would allocate tens of GB");
    assert!(decode_rgba(Format::Unknown(7), 4, 4, &[]).is_err());
    let t = Texture { width: 4, height: 4, levels: 1, format: Format::Dxt1, data: vec![0; 8] };
    assert!(t.decode(1).is_err(), "no such mip level");
}

#[test]
fn mip_levels_decode_at_their_own_size() {
    let mut data = solid_bc1(31, 0, 0).to_vec(); // 4x4
    data.extend_from_slice(&solid_bc1(0, 63, 0)); // 2x2 (one block)
    let t = Texture { width: 4, height: 4, levels: 2, format: Format::Dxt1, data };
    let m1 = t.decode(1).unwrap();
    assert_eq!((m1.width, m1.height), (2, 2));
    assert_eq!(&m1.pixels[..4], [0, 255, 0, 255]);
}

fn sample(format: Format) -> Texture {
    let (w, h, levels) = (8, 8, 2);
    let n = chain_size(format, w, h, levels);
    Texture { width: w, height: h, levels, format, data: (0..n).map(|i| (i * 7 + 3) as u8).collect() }
}

#[test]
fn dds_round_trips_every_supported_format() {
    for f in [
        Format::A8R8G8B8,
        Format::X8R8G8B8,
        Format::A8B8G8R8,
        Format::A1R5G5B5,
        Format::L8,
        Format::A8,
        Format::Dxt1,
        Format::Dxt3,
        Format::Dxt5,
        Format::Ati1,
        Format::Ati2,
        Format::Bc7,
    ] {
        let t = sample(f);
        let bytes = write_dds(&t).unwrap_or_else(|e| panic!("{f:?}: {e}"));
        assert_eq!(&bytes[..4], b"DDS ");
        let back = parse_dds(&bytes).unwrap_or_else(|e| panic!("{f:?}: {e}"));
        assert_eq!(back, t, "{f:?}");
    }
}

#[test]
fn dds_header_fields_are_where_other_tools_expect_them() {
    let bytes = write_dds(&sample(Format::Dxt5)).unwrap();
    assert_eq!(le32(&bytes, 4), 124, "header size");
    assert_eq!((le32(&bytes, 12), le32(&bytes, 16)), (8, 8), "height, width");
    assert_eq!(le32(&bytes, 28), 2, "mip count");
    assert_eq!(le32(&bytes, 84), 0x3554_5844, "fourCC 'DXT5'");
    assert_eq!(bytes.len(), 128 + chain_size(Format::Dxt5, 8, 8, 2));
    let bc7 = write_dds(&sample(Format::Bc7)).unwrap();
    assert_eq!(le32(&bc7, 84), 0x3031_5844, "BC7 uses the DX10 extension");
    assert_eq!(le32(&bc7, 128), 98, "DXGI_FORMAT_BC7_UNORM");
    assert_eq!(bc7.len(), 148 + chain_size(Format::Bc7, 8, 8, 2));
}

#[test]
fn dds_parser_rejects_garbage_without_panicking() {
    assert!(parse_dds(b"").is_err());
    assert!(parse_dds(&[0u8; 200]).is_err());
    let mut ok = write_dds(&sample(Format::Dxt1)).unwrap();
    ok[16..20].copy_from_slice(&0u32.to_le_bytes()); // zero width
    assert!(parse_dds(&ok).is_err());
    let mut huge = write_dds(&sample(Format::Dxt1)).unwrap();
    huge[16..20].copy_from_slice(&u32::MAX.to_le_bytes());
    assert!(parse_dds(&huge).is_err());
    let mut four = write_dds(&sample(Format::Dxt1)).unwrap();
    four[84..88].copy_from_slice(b"WAT?");
    assert!(parse_dds(&four).is_err(), "unknown FourCC");
    // a truncated body keeps what exists; decoding pads with zeros
    let t = write_dds(&sample(Format::Dxt1)).unwrap();
    let cut = parse_dds(&t[..t.len() - 20]).unwrap();
    assert!(cut.data.len() < chain_size(Format::Dxt1, 8, 8, 2));
    assert!(cut.decode(0).is_ok());
}

#[test]
fn writing_unknown_formats_is_an_error() {
    let t = Texture { width: 4, height: 4, levels: 1, format: Format::Unknown(9), data: vec![] };
    assert!(write_dds(&t).is_err());
}

#[test]
fn a_texture_without_its_pixels_is_not_written_as_a_dds() {
    let full = Texture { width: 8, height: 8, levels: 1, format: Format::A8R8G8B8, data: vec![1; 8 * 8 * 4] };
    assert!(write_dds(&full).is_ok());
    for len in [0, 1, 8 * 8 * 4 - 1] {
        let cut = Texture { data: vec![1; len], ..full.clone() };
        let e = write_dds(&cut).unwrap_err().to_string();
        assert!(e.contains("bytes of pixel data"), "{len}: {e}");
    }
}
