//! Writes a small GTA-V-shaped folder of real (unencrypted) RPF archives, for
//! trying the app, the CLI and screenshots without owning the game.
//!
//!     cargo run -p cw-core --example fixture -- /tmp/fake-gta

use std::{fs, path::PathBuf};

use cw_core::{
    dotnet_random::DotNetRandom,
    rpf::build::{Item, RpfBuilder},
    texture::{Format, Texture, write_dds},
    ytd::build_ytd,
};

fn noise(seed: i32, len: usize) -> Vec<u8> {
    let mut r = DotNetRandom::new(seed);
    let mut v = vec![0u8; len];
    r.next_bytes(&mut v);
    v
}

fn png(w: u32, h: u32) -> Vec<u8> {
    let mut img = image::RgbaImage::new(w, h);
    for (x, y, p) in img.enumerate_pixels_mut() {
        let t = x as f32 / w as f32;
        *p = image::Rgba([
            (250.0 * t) as u8,
            (120.0 + 80.0 * (y as f32 / h as f32)) as u8,
            (200.0 - 150.0 * t) as u8,
            255,
        ]);
    }
    let mut out = Vec::new();
    img.write_to(&mut std::io::Cursor::new(&mut out), image::ImageFormat::Png).unwrap();
    out
}

/// A texture drawn by `f` (RGBA per pixel) with every mip made by averaging 2x2 blocks.
fn drawn(w: u32, h: u32, levels: u32, f: impl Fn(u32, u32) -> [u8; 4]) -> Texture {
    let mut level: Vec<[u8; 4]> = (0..w * h).map(|i| f(i % w, i / w)).collect();
    let mut data = Vec::new();
    let (mut lw, mut lh) = (w, h);
    for l in 0..levels {
        data.extend(level.iter().flatten());
        if l + 1 == levels {
            break;
        }
        let (nw, nh) = ((lw / 2).max(1), (lh / 2).max(1));
        let at = |x: u32, y: u32| level[(y.min(lh - 1) * lw + x.min(lw - 1)) as usize];
        level = (0..nw * nh)
            .map(|i| {
                let (x, y) = ((i % nw) * 2, (i / nw) * 2);
                let px = [at(x, y), at(x + 1, y), at(x, y + 1), at(x + 1, y + 1)];
                std::array::from_fn(|c| (px.iter().map(|p| p[c] as u32).sum::<u32>() / 4) as u8)
            })
            .collect();
        (lw, lh) = (nw, nh);
    }
    // A8B8G8R8 stores the bytes in R, G, B, A order
    Texture { width: w, height: h, levels, format: Format::A8B8G8R8, data }
}

/// A DXT1 texture of coloured blocks, each blending from its own colour to a darker one.
fn blocks(w: u32, h: u32) -> Texture {
    let rgb565 = |r: u32, g: u32, b: u32| ((r >> 3) << 11 | (g >> 2) << 5 | (b >> 3)) as u16;
    let mut data = Vec::new();
    for by in 0..h / 4 {
        for bx in 0..w / 4 {
            let hue = ((bx * 7 + by * 13) % 24) as f32 / 24.0;
            let (r, g, b) = (
                (127.5 + 127.5 * (hue * std::f32::consts::TAU).cos()) as u32,
                (127.5 + 127.5 * ((hue + 0.33) * std::f32::consts::TAU).cos()) as u32,
                (127.5 + 127.5 * ((hue + 0.66) * std::f32::consts::TAU).cos()) as u32,
            );
            data.extend(rgb565(r, g, b).max(rgb565(r / 4, g / 4, b / 4) + 1).to_le_bytes());
            data.extend(rgb565(r / 4, g / 4, b / 4).to_le_bytes());
            data.extend([0xE4; 4]); // every row: colour 0, colour 1, then the two blends
        }
    }
    Texture { width: w, height: h, levels: 1, format: Format::Dxt1, data }
}

fn textures() -> Vec<(&'static str, Texture)> {
    let disc = drawn(256, 256, 5, |x, y| {
        let (dx, dy) = (x as f32 - 127.5, y as f32 - 127.5);
        let d = (dx * dx + dy * dy).sqrt();
        let a = if d < 110.0 { 255 } else { (255.0 - (d - 110.0) * 12.0).clamp(0.0, 255.0) as u8 };
        [
            (255.0 * (x as f32 / 255.0)) as u8,
            (90.0 + 120.0 * (1.0 - d / 180.0).max(0.0)) as u8,
            (255.0 * (y as f32 / 255.0)) as u8,
            a,
        ]
    });
    let checker = drawn(128, 128, 4, |x, y| {
        if ((x / 16) + (y / 16)) % 2 == 0 { [235, 235, 238, 255] } else { [60, 64, 90, 255] }
    });
    let sign = drawn(128, 64, 3, |x, y| {
        let bar = (y / 8) % 2 == 0 && x > 8 && x < 120 - (y / 8) * 6;
        let a = if bar { 255 } else { 70 };
        [250, (180 - y * 2) as u8, 40, a]
    });
    let normal = drawn(64, 64, 4, |x, y| {
        let (u, v) = ((x as f32 / 63.0 - 0.5) * 2.0, (y as f32 / 63.0 - 0.5) * 2.0);
        let z = (1.0 - (u * u + v * v).min(1.0)).sqrt();
        [(127.5 + 127.5 * u) as u8, (127.5 + 127.5 * v) as u8, (127.5 + 127.5 * z) as u8, 255]
    });
    vec![
        ("bench_diff", disc),
        ("wall_checker", checker),
        ("sign_alpha", sign),
        ("road_normal", normal),
        ("tile_blocks", blocks(128, 128)),
    ]
}

fn props(count: usize, ext: &str, seed: i32, sys: u32, gfx: u32) -> Vec<Item> {
    (0..count)
        .map(|i| {
            let name =
                format!("prop_{}_{:02}.{ext}", ["bench", "crate", "bin", "lamp", "sign"][i % 5], i / 5 + 1);
            Item::resource(&name, noise(seed + i as i32, 2048 + (i * 937) % 60_000), sys, gfx)
        })
        .collect()
}

fn xml(name: &str, rows: usize) -> Item {
    let mut s = String::from(
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<CVehicleModelInfo__InitDataList>\n  <residentTxd>vehshare</residentTxd>\n  <InitDatas>\n",
    );
    for i in 0..rows {
        s.push_str(&format!("    <Item>\n      <modelName>veh_{i:03}</modelName>\n      <txdName>veh_{i:03}</txdName>\n      <handlingId>VEH_{i:03}</handlingId>\n      <type>VEHICLE_TYPE_CAR</type>\n    </Item>\n"));
    }
    s.push_str("  </InitDatas>\n</CVehicleModelInfo__InitDataList>\n");
    Item::packed(name, s.into_bytes())
}

fn main() {
    let root = PathBuf::from(std::env::args().nth(1).expect("usage: fixture <output folder>"));
    let write = |rel: &str, items: Vec<Item>| {
        let p = root.join(rel);
        fs::create_dir_all(p.parent().unwrap()).unwrap();
        fs::write(&p, RpfBuilder::new(items).build().unwrap()).unwrap();
    };

    let ytd = (1 << 17, 0x9000_0000 | (1 << 11));
    let ydr = (1 << 17 | 1 << 11, 0x9000_0000 | (1 << 17));

    // one dictionary of real, drawable textures among the noise
    let drawn_set = textures();
    let refs: Vec<(&str, &Texture)> = drawn_set.iter().map(|(n, t)| (*n, t)).collect();
    let dict = build_ytd(&refs);
    let mut texture_items = props(14, "ytd", 100, ytd.0, ytd.1);
    texture_items.push(Item::resource("vehshare.ytd", dict.data, dict.sys_flags, dict.gfx_flags));
    texture_items.push(Item::file("sign_alpha.dds", write_dds(&drawn_set[2].1).unwrap()));
    let x64a = RpfBuilder::new(vec![Item::dir("textures", texture_items)]).build().unwrap();
    write(
        "common.rpf",
        vec![
            Item::dir(
                "data",
                vec![
                    Item::file("gta.dat", &b"IMG platform:/models/cdimages/props.img\nIDE platform:/levels/gta5/_citye/vb_34/vb_34.ide\n"[..]),
                    xml("vehicles.meta", 120),
                    Item::file("handling.meta", &b"<?xml version=\"1.0\"?>\n<CHandlingDataMgr/>\n"[..]),
                    Item::dir("levels", vec![Item::file("readme.txt", "Los Santos\nBlaine County\nCayo Perico\n".as_bytes())]),
                ],
            ),
            Item::dir("models", vec![Item::dir("cdimages", props(40, "ydr", 500, ydr.0, ydr.1))]),
            Item::dir(
                "textures",
                vec![
                    Item::file("sunset.png", png(640, 360)),
                    Item::file("detail.png", png(128, 128)),
                    Item::file("legend.txt", noise(9, 64).iter().map(|b| (b % 26 + b'a') as char).collect::<String>().into_bytes()),
                ],
            ),
            Item::archive("x64a.rpf", x64a),
            Item::file("blob.bin", noise(7, 150_000)),
        ],
    );
    write(
        "update/update.rpf",
        vec![Item::dir(
            "x64",
            vec![Item::dir(
                "patch",
                vec![
                    xml("vehicles.meta", 12),
                    Item::resource("lamp_post.ydr", noise(3, 30_000), ydr.0, ydr.1),
                ],
            )],
        )],
    );
    write(
        "x64/levels/gta5/generic.rpf",
        vec![
            Item::dir("props", props(25, "ydr", 900, ydr.0, ydr.1)),
            Item::dir("map", props(6, "ymap", 1200, 1 << 17, 0x9000_0000)),
        ],
    );
    fs::write(root.join("GTA5.exe"), b"MZ placeholder: not a real executable").unwrap();
    println!("wrote fixture to {}", root.display());
}
