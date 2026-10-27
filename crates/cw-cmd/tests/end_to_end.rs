//! Drives the whole engine through the same JSON entry point the CLI, the
//! control channel and MCP use, against a real archive written to disk.

use std::path::PathBuf;

use cw_cmd::Engine;
use cw_core::rpf::build::{Item, RpfBuilder};
use serde_json::{Value, json};

struct Fixture {
    dir: PathBuf,
}

impl Fixture {
    fn new(tag: &str) -> Self {
        let dir = std::env::temp_dir().join(format!("cw-e2e-{tag}-{}", std::process::id()));
        let _ = std::fs::remove_dir_all(&dir);
        std::fs::create_dir_all(dir.join("game/x64")).unwrap();
        let inner =
            RpfBuilder::new(vec![Item::file("note.txt", &b"inside a nested archive"[..])]).build().unwrap();
        let rpf = RpfBuilder::new(vec![
            Item::file("readme.txt", &b"hello"[..]),
            Item::dir(
                "levels",
                vec![
                    Item::packed("map10.xml", "<a/>".repeat(100).into_bytes()),
                    Item::packed("map2.xml", &b"<b/>"[..]),
                    Item::resource("prop.ydr", &b"drawable-payload"[..], 1 << 17, 0x9000_0000),
                    Item::file("../escape.txt", &b"x"[..]),
                ],
            ),
            Item::archive("child.rpf", inner),
        ]);
        // the traversal-named entry is rejected by the builder; drop it and keep the rest
        let mut rpf = rpf;
        if let Item::Dir { items, .. } = &mut rpf.items[1] {
            items.pop();
        }
        std::fs::write(dir.join("game/x64/a.rpf"), rpf.build().unwrap()).unwrap();
        Self { dir }
    }
}

impl Drop for Fixture {
    fn drop(&mut self) {
        let _ = std::fs::remove_dir_all(&self.dir);
    }
}

fn ok(e: &Engine, cmd: &str, args: Value) -> Value {
    e.execute(cmd, args).unwrap_or_else(|err| panic!("{cmd} failed: {err}"))
}

#[test]
fn open_browse_search_read_and_extract() {
    let fx = Fixture::new("flow");
    let e = Engine::ephemeral();

    // browsing before opening is a clear, recoverable error
    assert_eq!(e.execute("rpf.list", json!({})).unwrap_err().code, "bad_state");

    let summary = ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));
    assert_eq!(summary["top_level"], 1);
    assert_eq!(summary["archives"], 2);
    assert!(summary["key_warning"].is_string(), "no exe in the fixture, so keys are absent");

    let top = ok(&e, "rpf.list", json!({}));
    assert_eq!(top["entries"][0]["path"], "x64/a.rpf");

    let inside = ok(&e, "rpf.list", json!({ "path": "x64/a.rpf" }));
    let names: Vec<&str> =
        inside["entries"].as_array().unwrap().iter().map(|v| v["name"].as_str().unwrap()).collect();
    assert_eq!(names, ["child.rpf", "levels", "readme.txt"], "folders and archives first, then files");
    assert_eq!(inside["entries"][0]["kind"], "archive");

    // natural ordering: map2 before map10
    let levels = ok(&e, "rpf.list", json!({ "path": "x64/a.rpf/levels" }));
    let names: Vec<&str> =
        levels["entries"].as_array().unwrap().iter().map(|v| v["name"].as_str().unwrap()).collect();
    assert_eq!(names, ["map2.xml", "map10.xml", "prop.ydr"]);
    assert_eq!(levels["entries"][2]["type"], "Drawable");
    assert_eq!(levels["entries"][2]["resource_version"], 9);

    // pagination
    let page = ok(&e, "rpf.list", json!({ "path": "x64/a.rpf/levels", "offset": 1, "limit": 1 }));
    assert_eq!(page["total"], 3);
    assert_eq!(page["entries"][0]["name"], "map10.xml");

    let hits = ok(&e, "rpf.search", json!({ "query": "NOTE" }));
    assert_eq!(hits["entries"][0]["path"], "x64/a.rpf/child.rpf/note.txt");

    let read =
        ok(&e, "rpf.read", json!({ "path": "x64/a.rpf/child.rpf/note.txt", "offset": 3, "length": 6 }));
    assert_eq!(read["text"], "ide a ");
    assert_eq!(read["total"], 23);

    let out = fx.dir.join("out");
    ok(&e, "rpf.extract", json!({ "path": "x64/a.rpf", "dest": out }));
    assert_eq!(std::fs::read(out.join("a.rpf/readme.txt")).unwrap(), b"hello");
    assert_eq!(std::fs::read(out.join("a.rpf/child.rpf/note.txt")).unwrap(), b"inside a nested archive");
    assert_eq!(std::fs::read(out.join("a.rpf/levels/map10.xml")).unwrap().len(), 400);
    let ydr = std::fs::read(out.join("a.rpf/levels/prop.ydr")).unwrap();
    assert_eq!(&ydr[..4], b"RSC7", "resources are exported with an RSC7 header");

    let stats = ok(&e, "rpf.stats", json!({}));
    assert_eq!(stats["resources"], 1);
    assert_eq!(stats["archives"], 2);
}

#[test]
fn single_file_extract_and_missing_paths() {
    let fx = Fixture::new("single");
    let e = Engine::ephemeral();
    ok(&e, "rpf.open", json!({ "path": fx.dir.join("game/x64/a.rpf") }));

    let dest = fx.dir.join("copy.txt");
    ok(&e, "rpf.extract", json!({ "path": "a.rpf/readme.txt", "dest": dest }));
    assert_eq!(std::fs::read(&dest).unwrap(), b"hello");

    assert_eq!(e.execute("rpf.info", json!({ "path": "a.rpf/nope.txt" })).unwrap_err().code, "not_found");
    assert_eq!(
        e.execute("rpf.list", json!({ "path": "a.rpf/readme.txt" })).unwrap_err().code,
        "invalid_args"
    );
    assert_eq!(
        e.execute("game.open", json!({ "folder": fx.dir.join("missing") })).unwrap_err().code,
        "not_found"
    );
}

#[test]
fn ephemeral_engine_never_touches_the_config_file() {
    let fx = Fixture::new("cfg");
    let before =
        cw_cmd::Config::path().and_then(|p| std::fs::metadata(p).ok()).and_then(|m| m.modified().ok());
    let e = Engine::ephemeral();
    ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));
    let after =
        cw_cmd::Config::path().and_then(|p| std::fs::metadata(p).ok()).and_then(|m| m.modified().ok());
    assert_eq!(before, after);
    assert_eq!(e.session().config.game_folder.as_deref(), Some(fx.dir.join("game").as_path()));
}

#[test]
fn adding_archives_keeps_the_open_ones_and_loose_files_are_tracked() {
    let fx = Fixture::new("add");
    let e = Engine::ephemeral();
    ok(&e, "rpf.open", json!({ "path": fx.dir.join("game/x64/a.rpf") }));
    let second = RpfBuilder::new(vec![Item::file("extra.txt", &b"more"[..])]).build().unwrap();
    std::fs::write(fx.dir.join("b.rpf"), second).unwrap();

    ok(&e, "rpf.open", json!({ "path": fx.dir.join("b.rpf"), "add": true }));
    let roots = ok(&e, "rpf.list", json!({}));
    assert_eq!(roots["total"], 2);
    ok(&e, "rpf.info", json!({ "path": "a.rpf/readme.txt" }));
    ok(&e, "rpf.info", json!({ "path": "b.rpf/extra.txt" }));

    // without `add` the previous archives are replaced
    ok(&e, "rpf.open", json!({ "path": fx.dir.join("b.rpf") }));
    assert_eq!(ok(&e, "rpf.list", json!({}))["total"], 1);

    let note = fx.dir.join("note.txt");
    std::fs::write(&note, "loose text").unwrap();
    let info = ok(&e, "file.open", json!({ "path": note }));
    assert_eq!(info["type"], "Text");
    assert_eq!(e.session().files, std::slice::from_ref(&note));
    ok(&e, "file.open", json!({ "path": note })); // idempotent
    assert_eq!(e.session().files.len(), 1);
    assert_eq!(ok(&e, "file.read", json!({ "path": note, "length": 5 }))["text"], "loose");
    let tail = ok(&e, "file.read", json!({ "path": note, "offset": 6, "length": 100 }));
    assert_eq!(
        (tail["text"].as_str(), tail["total"].as_u64(), tail["offset"].as_u64()),
        (Some("text"), Some(10), Some(6))
    );
    ok(&e, "file.close", json!({ "path": note }));
    assert!(e.session().files.is_empty());
    // reading is limited to files that were opened on purpose
    let refused = e.execute("file.read", json!({ "path": note })).unwrap_err();
    assert_eq!(refused.code, "invalid_args");
    assert!(refused.message.contains("file.open"), "{}", refused.message);
    assert_eq!(e.execute("file.open", json!({ "path": fx.dir.join("nope") })).unwrap_err().code, "not_found");
    assert_eq!(e.execute("file.open", json!({ "path": fx.dir })).unwrap_err().code, "invalid_args");
}

#[test]
fn mods_workflow_copy_toggle_remove_through_commands() {
    let fx = Fixture::new("mods");
    let e = Engine::ephemeral();
    // no game folder yet: a clear, recoverable error
    assert_eq!(e.execute("mods.list", json!({})).unwrap_err().code, "bad_state");

    ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));
    let l = ok(&e, "mods.list", json!({}));
    assert_eq!(l["exists"], false);
    assert_eq!(l["files"].as_array().unwrap().len(), 0);

    // any entry inside the archive resolves to its top-level .rpf
    let copied = ok(&e, "mods.copy", json!({ "path": "x64/a.rpf/child.rpf/note.txt" }));
    assert_eq!(copied["copied"], "x64/a.rpf");
    assert!(fx.dir.join("game/mods/x64/a.rpf").is_file());
    assert_eq!(e.execute("mods.copy", json!({ "path": "x64/a.rpf" })).unwrap_err().code, "already_exists");
    ok(&e, "mods.copy", json!({ "path": "x64/a.rpf", "overwrite": true }));

    let l = ok(&e, "mods.list", json!({}));
    assert_eq!(l["files"][0]["path"], "x64/a.rpf");
    assert_eq!(l["files"][0]["overrides_game"], true);
    assert_eq!(l["files"][0]["enabled"], true);

    ok(&e, "mods.disable", json!({ "path": "x64/a.rpf" }));
    assert_eq!(ok(&e, "mods.list", json!({}))["files"][0]["enabled"], false);
    ok(&e, "mods.enable", json!({ "path": "x64/a.rpf" }));
    assert_eq!(ok(&e, "mods.list", json!({}))["files"][0]["enabled"], true);

    assert_eq!(e.execute("mods.remove", json!({ "path": "../a.rpf" })).unwrap_err().code, "invalid_args");
    ok(&e, "mods.remove", json!({ "path": "x64/a.rpf" }));
    assert_eq!(ok(&e, "mods.list", json!({}))["files"].as_array().unwrap().len(), 0);

    let src = fx.dir.join("pkg");
    std::fs::create_dir_all(src.join("data")).unwrap();
    std::fs::write(src.join("data/x.meta"), "m").unwrap();
    assert_eq!(ok(&e, "mods.install", json!({ "source": src }))["installed"], 1);
    assert_eq!(e.execute("mods.install", json!({ "source": src })).unwrap_err().code, "already_exists");
}

#[test]
fn world_editing_commands_with_undo_redo_and_persistence() {
    let fx = Fixture::new("world");
    let e = Engine::ephemeral();
    let a = ok(
        &e,
        "world.add",
        json!({ "archetype": "prop_bench_01", "x": 10.0, "y": 20.0, "z": 30.0, "rz": 90.0 }),
    );
    assert_eq!(a["id"], 1);
    assert_eq!(a["hash"], format!("0x{:08X}", cw_core::hash::joaat_lower("prop_bench_01")));
    assert!((a["rz"].as_f64().unwrap() - 90.0).abs() < 1e-3);

    // unspecified fields stay; one edit is one undo step
    let moved = ok(&e, "world.set", json!({ "id": 1, "x": 11.5 }));
    assert_eq!(moved["x"], 11.5);
    assert_eq!(moved["y"], 20.0);
    assert!((moved["rz"].as_f64().unwrap() - 90.0).abs() < 1e-3, "rotation survives an x-only edit");
    assert_eq!(ok(&e, "world.undo", json!({}))["undone"], true);
    assert_eq!(ok(&e, "world.list", json!({}))["entities"][0]["x"], 10.0);
    ok(&e, "world.redo", json!({}));
    assert_eq!(ok(&e, "world.list", json!({}))["entities"][0]["x"], 11.5);

    // validation: bad values are rejected and change nothing
    assert_eq!(e.execute("world.set", json!({ "id": 1, "sx": 0.0 })).unwrap_err().code, "invalid_args");
    assert_eq!(e.execute("world.set", json!({ "id": 9, "x": 1.0 })).unwrap_err().code, "not_found");
    assert_eq!(e.execute("world.add", json!({ "archetype": "  " })).unwrap_err().code, "invalid_args");
    assert_eq!(ok(&e, "world.list", json!({}))["entities"][0]["sx"], 1.0);

    ok(&e, "world.add", json!({ "archetype": "prop_bin_02", "x": -5.0 }));
    ok(&e, "world.select", json!({ "id": 2 }));
    assert_eq!(ok(&e, "world.stats", json!({}))["selected"], 1);
    assert_eq!(e.execute("world.select", json!({ "id": 99 })).unwrap_err().code, "not_found");
    ok(&e, "world.select", json!({}));
    assert_eq!(ok(&e, "world.stats", json!({}))["selected"], 0);

    let file = fx.dir.join("scene.json");
    ok(&e, "world.save", json!({ "path": file }));
    assert_eq!(ok(&e, "world.stats", json!({}))["dirty"], false);
    ok(&e, "world.new", json!({ "name": "other" }));
    assert_eq!(ok(&e, "world.stats", json!({}))["entities"], 0);
    assert_eq!(ok(&e, "world.load", json!({ "path": file }))["entities"], 2);
    assert_eq!(ok(&e, "world.list", json!({}))["entities"][1]["archetype"], "prop_bin_02");
    std::fs::write(&file, "{ broken").unwrap();
    assert_eq!(e.execute("world.load", json!({ "path": file })).unwrap_err().code, "io");
    assert_eq!(ok(&e, "world.stats", json!({}))["entities"], 2, "a failed load keeps the scene");
}

#[test]
fn texture_commands_list_and_export_dds_from_archives_and_disk() {
    use cw_core::{
        texture::{Format, Texture, chain_size, parse_dds},
        ytd::build_ytd,
    };
    let fx = Fixture::new("tex");
    let t = |fmt, w, h, lv, seed: u8| {
        let n = chain_size(fmt, w, h, lv);
        Texture {
            width: w,
            height: h,
            levels: lv,
            format: fmt,
            data: (0..n).map(|i| (i as u8).wrapping_add(seed)).collect(),
        }
    };
    let (a, b) = (t(Format::Dxt1, 16, 16, 3, 1), t(Format::A8R8G8B8, 8, 8, 1, 2));
    let res = build_ytd(&[("wall_diff", &a), ("wall_diff", &b)]); // two textures, same name
    let rpf =
        RpfBuilder::new(vec![Item::resource("wall.ytd", res.data.clone(), res.sys_flags, res.gfx_flags)])
            .build()
            .unwrap();
    std::fs::write(fx.dir.join("game/t.rpf"), rpf).unwrap();

    let e = Engine::ephemeral();
    ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));
    let l = ok(&e, "texture.list", json!({ "path": "t.rpf/wall.ytd" }));
    assert_eq!(l["count"], 2);
    assert_eq!(l["textures"][0]["format"], "DXT1 (BC1)");
    assert_eq!(l["textures"][1]["width"], 8);

    let out = fx.dir.join("dds");
    let r = ok(&e, "texture.export", json!({ "path": "t.rpf/wall.ytd", "dest": out }));
    assert_eq!(r["written"], 2);
    assert_eq!(
        r["files"],
        json!(["wall_diff.dds", "wall_diff_2.dds"]),
        "same-named textures do not overwrite each other"
    );
    assert_eq!(parse_dds(&std::fs::read(out.join("wall_diff.dds")).unwrap()).unwrap(), a);
    assert_eq!(parse_dds(&std::fs::read(out.join("wall_diff_2.dds")).unwrap()).unwrap(), b);

    // a single texture, from a loose OpenIV-style file on disk
    let exported = ok(&e, "rpf.extract", json!({ "path": "t.rpf", "dest": fx.dir.join("raw") }));
    assert_eq!(exported["files"], 1);
    let loose = fx.dir.join("raw/t.rpf/wall.ytd");
    assert_eq!(ok(&e, "texture.list", json!({ "file": loose }))["count"], 2);
    let one = ok(&e, "texture.export", json!({ "file": loose, "index": 1, "dest": fx.dir.join("one") }));
    assert_eq!(one["files"], json!(["wall_diff.dds"]));

    // a dds file is a one-texture "dictionary"
    let dds = fx.dir.join("one/wall_diff.dds");
    assert_eq!(ok(&e, "texture.list", json!({ "file": dds }))["textures"][0]["name"], "wall_diff");

    // errors
    assert_eq!(e.execute("texture.list", json!({})).unwrap_err().code, "invalid_args");
    assert_eq!(e.execute("texture.list", json!({ "path": "t.rpf/nope.ytd" })).unwrap_err().code, "not_found");
    assert_eq!(
        e.execute("texture.export", json!({ "path": "t.rpf/wall.ytd", "index": 9, "dest": out }))
            .unwrap_err()
            .code,
        "not_found"
    );
    let bad = fx.dir.join("junk.dds");
    std::fs::write(&bad, b"not a dds").unwrap();
    assert_eq!(e.execute("texture.list", json!({ "file": bad })).unwrap_err().code, "archive");
}

#[test]
fn the_edition_comes_from_the_folder_unless_the_caller_says_otherwise() {
    let fx = Fixture::new("edition");
    let game = fx.dir.join("game");
    std::fs::write(game.join("GTA5_Enhanced.exe"), b"not a real executable").unwrap();
    let e = Engine::ephemeral();

    let s = ok(&e, "game.open", json!({ "folder": &game }));
    assert_eq!(s["edition"], "enhanced", "nobody said which; the folder has the Enhanced executable");
    assert!(e.session().config.enhanced_edition, "and that is what gets remembered");

    let s = ok(&e, "game.open", json!({ "folder": &game, "enhanced": false }));
    assert_eq!(s["edition"], "legacy", "an explicit choice wins");

    std::fs::remove_file(game.join("GTA5_Enhanced.exe")).unwrap();
    std::fs::write(game.join("GTA5.exe"), b"nor this").unwrap();
    let s = ok(&e, "game.open", json!({ "folder": &game }));
    assert_eq!(s["edition"], "legacy", "a saved Enhanced setting does not override a legacy folder");
    assert!(!e.session().config.enhanced_edition);

    std::fs::remove_file(game.join("GTA5.exe")).unwrap();
    ok(&e, "config.set", json!({ "enhanced_edition": true }));
    let s = ok(&e, "game.open", json!({ "folder": &game }));
    assert_eq!(s["edition"], "enhanced", "no executable at all: fall back to the saved setting");
}

#[test]
fn game_open_leaves_the_mods_folder_to_the_mods_view() {
    let fx = Fixture::new("mods-scan");
    let game = fx.dir.join("game");
    std::fs::create_dir_all(game.join("mods/x64")).unwrap();
    std::fs::copy(game.join("x64/a.rpf"), game.join("mods/x64/a.rpf")).unwrap();
    // a top-level archive whose name merely starts with "mods" is still a game archive
    std::fs::copy(game.join("x64/a.rpf"), game.join("modsets.rpf")).unwrap();
    let e = Engine::ephemeral();

    let s = ok(&e, "game.open", json!({ "folder": &game }));
    assert_eq!(s["top_level"], 2, "x64/a.rpf and modsets.rpf, not mods/x64/a.rpf");
    let top = ok(&e, "rpf.list", json!({}));
    let paths: Vec<&str> =
        top["entries"].as_array().unwrap().iter().map(|v| v["path"].as_str().unwrap()).collect();
    assert!(!paths.iter().any(|p| p.starts_with("mods/")), "{paths:?}");

    let listed = ok(&e, "mods.list", json!({}));
    assert!(listed.to_string().contains("x64/a.rpf"), "the override is still managed: {listed}");
}

#[test]
fn adding_to_nothing_is_the_same_as_opening() {
    let fx = Fixture::new("add-empty");
    let folder = fx.dir.join("game");
    let e = Engine::ephemeral();

    // a front-end that does not know whether anything is open yet can always say "add"
    let s = ok(&e, "rpf.open", json!({ "path": &folder, "add": true }));
    assert_eq!(s["top_level"], 1);
    assert_eq!(e.session().root.as_deref(), Some(folder.as_path()), "the folder that was opened is the root");

    // and a second one lands next to it instead of replacing it
    let extra = fx.dir.join("extra.rpf");
    std::fs::write(&extra, RpfBuilder::new(vec![Item::file("x.txt", &b"x"[..])]).build().unwrap()).unwrap();
    ok(&e, "rpf.open", json!({ "path": &extra, "add": true }));
    assert_eq!(ok(&e, "rpf.stats", json!({}))["top_level"], 2);
    assert_eq!(e.session().root.as_deref(), Some(folder.as_path()), "adding keeps the root");
}

#[test]
fn texture_export_renders_filtered_pngs_with_the_viewers_pipeline() {
    use cw_core::{
        texture::{Format, Texture, chain_size},
        ytd::build_ytd,
    };
    let fx = Fixture::new("tex-png");
    let (w, h, levels) = (8u32, 4u32, 2u32);
    let n = chain_size(Format::A8R8G8B8, w, h, levels);
    let tex = Texture {
        width: w,
        height: h,
        levels,
        format: Format::A8R8G8B8,
        data: (0..n).map(|i| (i as u8).wrapping_mul(7).wrapping_add(3)).collect(),
    };
    let res = build_ytd(&[("wall", &tex)]);
    std::fs::write(
        fx.dir.join("game/t.rpf"),
        RpfBuilder::new(vec![Item::resource("wall.ytd", res.data.clone(), res.sys_flags, res.gfx_flags)])
            .build()
            .unwrap(),
    )
    .unwrap();
    let e = Engine::ephemeral();
    ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));

    let decode = |path: &std::path::Path| {
        image::load_from_memory_with_format(&std::fs::read(path).unwrap(), image::ImageFormat::Png)
            .unwrap()
            .to_rgba8()
    };
    let truth = tex.decode(0).unwrap(); // what the file says, before any filter
    let out = fx.dir.join("png");

    let r = ok(&e, "texture.export", json!({ "path": "t.rpf/wall.ytd", "dest": &out, "format": "png" }));
    assert_eq!(r["files"], json!(["wall.png"]));
    let plain = decode(&out.join("wall.png"));
    assert_eq!((plain.width(), plain.height()), (w, h));
    assert_eq!(plain.as_raw(), &truth.pixels, "unfiltered PNG is the decoded texture");

    ok(
        &e,
        "texture.export",
        json!({ "path": "t.rpf/wall.ytd", "dest": &out, "format": "PNG", "channels": "g", "invert": true }),
    );
    let filtered = decode(&out.join("wall_2.png"));
    let want: Vec<u8> =
        truth.pixels.chunks(4).flat_map(|p| [255 - p[1], 255 - p[1], 255 - p[1], 255]).collect();
    assert_eq!(filtered.as_raw(), &want, "green channel as grey, then inverted");

    ok(
        &e,
        "texture.export",
        json!({ "path": "t.rpf/wall.ytd", "dest": &out, "format": "png", "mip": 1, "channels": "alpha" }),
    );
    let small = decode(&out.join("wall_3.png"));
    assert_eq!((small.width(), small.height()), (4, 2), "mip 1 is half the size");

    // misuse is explained, not guessed at
    let bad = |args: serde_json::Value| e.execute("texture.export", args).unwrap_err();
    assert_eq!(bad(json!({ "path": "t.rpf/wall.ytd", "dest": &out, "channels": "g" })).code, "invalid_args");
    assert_eq!(
        bad(json!({ "path": "t.rpf/wall.ytd", "dest": &out, "format": "png", "channels": "x" })).code,
        "invalid_args"
    );
    assert_eq!(bad(json!({ "path": "t.rpf/wall.ytd", "dest": &out, "format": "jpg" })).code, "invalid_args");
    let skipped = ok(
        &e,
        "texture.export",
        json!({ "path": "t.rpf/wall.ytd", "dest": &out, "format": "png", "mip": 9 }),
    );
    assert_eq!(skipped["written"], 0, "a mip that does not exist is reported, not written");
    assert_eq!(skipped["skipped"].as_array().unwrap().len(), 1);
}

#[test]
fn ymap_info_and_import_place_a_maps_entities_in_the_scene() {
    use cw_core::ymap::{LodLevel, Ymap, YmapEntity, build_ymap};
    let fx = Fixture::new("ymap");
    let s = std::f32::consts::FRAC_1_SQRT_2;
    let hd = |i: u32, x: f32| YmapEntity {
        archetype: 0xAA00_0000 + i,
        guid: i,
        position: [x, 20.0, 5.0],
        scale_xy: 2.0,
        scale_z: 3.0,
        rotation: [0.0, 0.0, s, s],
        ..Default::default()
    };
    let lod = |i: u32, parent: i32| YmapEntity { lod: LodLevel::Lod, parent_index: parent, ..hd(i, 0.0) };
    let map = Ymap {
        name: 0x1234_ABCD,
        flags: 1,
        entity_extents: ([-5.0, 0.0, 0.0], [50.0, 60.0, 9.0]),
        entities: vec![
            YmapEntity { archetype: cw_core::hash::joaat_lower("prop_bench_01"), ..hd(1, 10.0) },
            hd(2, 11.0),
            YmapEntity { lod: LodLevel::OrphanHd, ..hd(3, 12.0) },
            lod(4, -1),
            YmapEntity { scale_xy: 0.0, ..hd(5, 13.0) }, // cannot be placed
        ],
        ..Default::default()
    };
    let res = build_ymap(&map);
    std::fs::write(
        fx.dir.join("game/m.rpf"),
        RpfBuilder::new(vec![
            Item::resource("hills.ymap", res.data.clone(), res.sys_flags, res.gfx_flags),
            // a model file: its name is how entity 1 gets its name back from the hash
            Item::resource("Prop_Bench_01.ydr", &b"model"[..], 1 << 17, 0x9000_0000),
        ])
        .build()
        .unwrap(),
    )
    .unwrap();
    let e = Engine::ephemeral();
    ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));

    let info = ok(&e, "ymap.info", json!({ "path": "m.rpf/hills.ymap", "limit": 2, "offset": 1 }));
    assert_eq!(
        (info["name"].as_str(), info["entities"].as_u64(), info["scripted"].as_bool()),
        (Some("0x1234ABCD"), Some(5), Some(true))
    );
    assert_eq!(
        info["lod"],
        json!([{ "level": "HD", "count": 3 }, { "level": "LOD", "count": 1 }, { "level": "Orphan HD", "count": 1 }])
    );
    assert_eq!(info["entity_extents"]["max"], json!([50.0, 60.0, 9.0]));
    assert_eq!(info["page"]["items"].as_array().unwrap().len(), 2);
    assert_eq!(info["page"]["items"][0]["archetype"], "0xAA000002");
    assert!(info["page"]["items"][0]["model"].is_null());
    let first_page = ok(&e, "ymap.info", json!({ "path": "m.rpf/hills.ymap", "limit": 1 }));
    assert_eq!(first_page["page"]["items"][0]["model"], "prop_bench_01");
    assert_eq!(info["lod_roots"], 5, "none of them has a coarser parent in this file");

    // the default import takes the detailed ones and says what it left out
    let r = ok(&e, "world.import", json!({ "path": "m.rpf/hills.ymap" }));
    assert_eq!((r["imported"].as_u64(), r["entities_in_file"].as_u64()), (Some(3), Some(5)));
    assert_eq!((r["left_out"]["by_lod"].as_u64(), r["left_out"]["unplaceable"].as_u64()), (Some(1), Some(1)));
    let listed = ok(&e, "world.list", json!({}));
    let first = &listed["entities"][0];
    let bench = format!("0x{:08X}", cw_core::hash::joaat_lower("prop_bench_01"));
    assert_eq!(
        (first["archetype"].as_str(), first["hash"].as_str()),
        (Some("prop_bench_01"), Some(bench.as_str())),
        "named after the model file, and still the same hash"
    );
    assert_eq!(listed["entities"][1]["archetype"], "0xAA000002", "no file for this one: the hash stands in");
    assert_eq!(r["named"], 1);
    assert_eq!(first["source"], "m.rpf/hills.ymap");
    assert_eq!(
        (first["x"].as_f64(), first["y"].as_f64(), first["z"].as_f64()),
        (Some(10.0), Some(20.0), Some(5.0))
    );
    assert_eq!(
        (first["sx"].as_f64(), first["sy"].as_f64(), first["sz"].as_f64()),
        (Some(2.0), Some(2.0), Some(3.0))
    );
    assert!(
        (first["rz"].as_f64().unwrap() + 90.0).abs() < 1e-3,
        "stored +90 about Z, placed at -90: {first}"
    );

    // one undo takes the whole import away; redo brings it back
    assert_eq!(ok(&e, "world.undo", json!({}))["undone"], true);
    assert_eq!(ok(&e, "world.stats", json!({}))["entities"], 0);
    ok(&e, "world.redo", json!({}));
    assert_eq!(ok(&e, "world.stats", json!({}))["entities"], 3);

    // every level, added to what is there; then a replacement
    let all = ok(&e, "world.import", json!({ "path": "m.rpf/hills.ymap", "lod": "ALL" }));
    assert_eq!((all["imported"].as_u64(), all["scene_entities"].as_u64()), (Some(4), Some(7)));
    let replaced = ok(&e, "world.import", json!({ "path": "m.rpf/hills.ymap", "replace": true }));
    assert_eq!(replaced["scene_entities"], 3);
    assert_eq!(ok(&e, "world.stats", json!({}))["name"], "m.rpf/hills.ymap");

    // a loose file works the same way
    let raw = ok(&e, "rpf.extract", json!({ "path": "m.rpf", "dest": fx.dir.join("raw") }));
    assert_eq!(raw["files"], 2, "the map and the model file");
    let loose = fx.dir.join("raw/m.rpf/hills.ymap");
    assert_eq!(ok(&e, "ymap.info", json!({ "file": &loose }))["entities"], 5);
    assert_eq!(ok(&e, "world.import", json!({ "file": loose, "replace": true }))["imported"], 3);

    // misuse and bad files are explained
    let code = |cmd: &str, args: serde_json::Value| e.execute(cmd, args).unwrap_err().code;
    assert_eq!(code("world.import", json!({ "path": "m.rpf/hills.ymap", "lod": "some" })), "invalid_args");
    assert_eq!(code("world.import", json!({})), "invalid_args");
    assert_eq!(code("ymap.info", json!({ "path": "m.rpf/nope.ymap" })), "not_found");
    assert_eq!(code("ymap.info", json!({ "path": "m.rpf" })), "invalid_args", "an archive is not a map");
    std::fs::write(fx.dir.join("pso.ymap"), b"PSIN....").unwrap();
    let pso = e.execute("ymap.info", json!({ "file": fx.dir.join("pso.ymap") })).unwrap_err();
    assert!(pso.message.contains("Enhanced"), "{}", pso.message);
    std::fs::write(fx.dir.join("junk.ymap"), b"not a map at all").unwrap();
    assert_eq!(code("world.import", json!({ "file": fx.dir.join("junk.ymap") })), "invalid_args");
    assert_eq!(ok(&e, "world.stats", json!({}))["entities"], 3, "failures leave the scene alone");

    // the command line opens the game folder first for these
    for name in ["ymap.info", "world.import", "texture.list", "rpf.list"] {
        assert!(e.spec(name).unwrap().reads_archives(), "{name}");
    }
    assert!(!e.spec("world.add").unwrap().reads_archives());
}

#[test]
fn game_check_samples_every_reader_and_names_what_fails() {
    use cw_core::{
        GtaKeys,
        texture::{Format, Texture, chain_size},
        ymap::{Ymap, YmapEntity, build_ymap},
        ytd::build_ytd,
    };
    let fx = Fixture::new("check");
    let tex = Texture {
        width: 8,
        height: 8,
        levels: 1,
        format: Format::A8R8G8B8,
        data: vec![7; chain_size(Format::A8R8G8B8, 8, 8, 1)],
    };
    let ytd = build_ytd(&[("wall", &tex)]);
    let full = build_ymap(&Ymap { entities: vec![YmapEntity::default(); 3], ..Default::default() });
    let empty = build_ymap(&Ymap::default());
    let rpf = RpfBuilder::new(vec![
        Item::resource("good.ytd", ytd.data.clone(), ytd.sys_flags, ytd.gfx_flags),
        Item::resource("junk.ytd", vec![0xAB; 300], 1 << 17, 0xD000_0000),
        Item::resource("good.ymap", full.data.clone(), full.sys_flags, full.gfx_flags),
        Item::resource("nothing.ymap", empty.data.clone(), empty.sys_flags, empty.gfx_flags),
        Item::file("readme.txt", &b"plain"[..]),
        Item::packed("big.xml", "<a/>".repeat(100).into_bytes()),
    ]);
    std::fs::write(fx.dir.join("game/t.rpf"), rpf.build().unwrap()).unwrap();
    // an archive that needs keys nobody has loaded
    let keys = GtaKeys::synthetic();
    let mut locked = RpfBuilder::new(vec![Item::file("x.txt", &b"x"[..])]);
    locked.aes_key = Some(keys.aes_key);
    std::fs::write(fx.dir.join("game/locked.rpf"), locked.build().unwrap()).unwrap();

    let e = Engine::ephemeral();
    assert_eq!(e.execute("game.check", json!({})).unwrap_err().code, "bad_state", "nothing is open yet");
    ok(&e, "game.open", json!({ "folder": fx.dir.join("game") }));
    let c = ok(&e, "game.check", json!({ "sample": 50 }));

    assert_eq!(c["keys"], false);
    assert_eq!(c["archives"]["errors"], 1, "the AES archive cannot be opened without keys");
    assert!(c["archives"]["first_errors"][0]["path"].as_str().unwrap().contains("locked.rpf"));
    assert_eq!(c["archives"]["encryption"]["open"], 3, "a.rpf, its nested archive and t.rpf");
    assert_eq!((c["ytd"]["tried"].as_u64(), c["ytd"]["parsed"].as_u64()), (Some(2), Some(1)));
    assert_eq!((c["ytd"]["textures"].as_u64(), c["ytd"]["decodable"].as_u64()), (Some(1), Some(1)));
    assert_eq!(c["ytd"]["failures"].as_array().unwrap().len(), 1, "the junk dictionary, with its reason");
    assert!(c["ytd"]["failures"][0]["examples"][0].as_str().unwrap().ends_with("junk.ytd"));
    assert_eq!(
        (c["ymap"]["tried"].as_u64(), c["ymap"]["parsed"].as_u64(), c["ymap"]["entities"].as_u64()),
        (Some(2), Some(2), Some(3))
    );
    assert_eq!(c["ymap"]["failures"][0]["reason"], "parsed, but holds no entities");
    assert_eq!(c["extract"]["failed"], 0, "everything that was sampled could be read");
    let report: Vec<&str> = c["report"].as_array().unwrap().iter().map(|v| v.as_str().unwrap()).collect();
    assert!(report.iter().any(|l| l.starts_with("keys: NOT derived")), "{report:?}");
    assert!(report.iter().any(|l| l.starts_with("ytd: 1 of 2 parsed")), "{report:?}");

    // a sample smaller than the file count still spreads over all of them
    let small = ok(&e, "game.check", json!({ "sample": 1 }));
    assert_eq!(small["ytd"]["tried"], 1);
}
