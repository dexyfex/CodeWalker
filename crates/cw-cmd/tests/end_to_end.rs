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
    ok(&e, "file.close", json!({ "path": note }));
    assert!(e.session().files.is_empty());
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
