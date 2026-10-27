//! Writes a small GTA-V-shaped folder of real (unencrypted) RPF archives, for
//! trying the app, the CLI and screenshots without owning the game.
//!
//!     cargo run -p cw-core --example fixture -- /tmp/fake-gta

use std::{fs, path::PathBuf};

use cw_core::{
    dotnet_random::DotNetRandom,
    rpf::build::{Item, RpfBuilder},
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

    let x64a =
        RpfBuilder::new(vec![Item::dir("textures", props(14, "ytd", 100, ytd.0, ytd.1))]).build().unwrap();
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
