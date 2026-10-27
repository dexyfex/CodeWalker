use super::*;

struct Dir(PathBuf);

impl Dir {
    fn new(tag: &str) -> Self {
        let p = std::env::temp_dir().join(format!("cw-mods-{tag}-{}", std::process::id()));
        let _ = fs::remove_dir_all(&p);
        fs::create_dir_all(&p).unwrap();
        Dir(p)
    }

    fn write(&self, rel: &str, bytes: &[u8]) -> PathBuf {
        let p = self.0.join(rel);
        fs::create_dir_all(p.parent().unwrap()).unwrap();
        fs::write(&p, bytes).unwrap();
        p
    }
}

impl Drop for Dir {
    fn drop(&mut self) {
        let _ = fs::remove_dir_all(&self.0);
    }
}

#[test]
fn rejects_paths_that_could_escape() {
    for bad in ["", "/", "..", "../x", "a/../../x", "a/./b", "C:/x", "a//b/../c", "x\0y", "a:b/c"] {
        assert!(safe_rel(bad).is_err(), "{bad:?} must be rejected");
    }
    assert_eq!(safe_rel("update\\x64\\dlc.rpf").unwrap(), Path::new("update").join("x64").join("dlc.rpf"));
    assert_eq!(safe_rel("/common.rpf/").unwrap(), Path::new("common.rpf"));
}

#[test]
fn copy_creates_an_override_and_refuses_to_clobber_it() {
    let g = Dir::new("copy");
    g.write("update/x64/dlc.rpf", b"original");
    assert_eq!(copy_to_mods(&g.0, "update/x64/dlc.rpf", false).unwrap(), "update/x64/dlc.rpf");
    assert_eq!(fs::read(g.0.join("mods/update/x64/dlc.rpf")).unwrap(), b"original");

    fs::write(g.0.join("mods/update/x64/dlc.rpf"), b"edited").unwrap();
    assert!(matches!(copy_to_mods(&g.0, "update/x64/dlc.rpf", false), Err(ModError::Exists(_))));
    assert_eq!(fs::read(g.0.join("mods/update/x64/dlc.rpf")).unwrap(), b"edited", "the edit survives");
    copy_to_mods(&g.0, "update/x64/dlc.rpf", true).unwrap();
    assert_eq!(fs::read(g.0.join("mods/update/x64/dlc.rpf")).unwrap(), b"original");

    assert!(matches!(copy_to_mods(&g.0, "missing.rpf", false), Err(ModError::NotFound(_))));
    assert!(matches!(copy_to_mods(&g.0, "mods/update/x64/dlc.rpf", false), Err(ModError::Invalid(_))));
    assert!(copy_to_mods(&g.0, "../outside.rpf", false).is_err());
}

#[test]
fn a_failed_copy_leaves_no_partial_file_and_no_listing() {
    let g = Dir::new("atomic");
    g.write("a.rpf", b"original");
    // something that is not a file is in the way: the rename must fail cleanly
    fs::create_dir_all(g.0.join("mods/a.rpf")).unwrap();
    assert!(copy_to_mods(&g.0, "a.rpf", true).is_err());
    assert!(g.0.join("mods/a.rpf").is_dir(), "what was there is untouched");
    assert!(!g.0.join("mods/a.rpf.cwtmp").exists(), "the temporary copy is cleaned up");

    fs::remove_dir(g.0.join("mods/a.rpf")).unwrap();
    copy_to_mods(&g.0, "a.rpf", false).unwrap();
    assert!(!g.0.join("mods/a.rpf.cwtmp").exists());
    g.write("mods/crashed.rpf.cwtmp", b"half");
    let listed: Vec<String> = list(&g.0).unwrap().into_iter().map(|m| m.path).collect();
    assert_eq!(listed, ["a.rpf"], "a leftover from a crash is not presented as a mod");
}

#[test]
fn listing_reports_state_and_overrides() {
    let g = Dir::new("list");
    g.write("common.rpf", b"orig");
    g.write("mods/common.rpf", b"mod!");
    g.write("mods/extra/readme.txt", b"hi");
    g.write("mods/x64a.rpf.disabled", b"off");
    let l = list(&g.0).unwrap();
    let by = |p: &str| l.iter().find(|m| m.path == p).unwrap_or_else(|| panic!("{p} missing in {l:?}"));
    assert!(by("common.rpf").enabled && by("common.rpf").overrides_game && by("common.rpf").archive);
    assert_eq!(by("common.rpf").size, 4);
    assert!(!by("extra/readme.txt").overrides_game && !by("extra/readme.txt").archive);
    assert!(!by("x64a.rpf").enabled, "the .disabled suffix is hidden and means off");
    assert_eq!(l.len(), 3);
    assert!(list(&Dir::new("empty").0).unwrap().is_empty(), "no mods folder is not an error");
}

#[test]
fn enable_and_disable_round_trip_without_losing_data() {
    let g = Dir::new("toggle");
    g.write("mods/common.rpf", b"data");
    set_enabled(&g.0, "common.rpf", false).unwrap();
    assert!(!g.0.join("mods/common.rpf").exists() && g.0.join("mods/common.rpf.disabled").exists());
    set_enabled(&g.0, "common.rpf", false).unwrap(); // idempotent
    set_enabled(&g.0, "common.rpf", true).unwrap();
    assert_eq!(fs::read(g.0.join("mods/common.rpf")).unwrap(), b"data");
    assert!(matches!(set_enabled(&g.0, "nope.rpf", true), Err(ModError::NotFound(_))));

    // both states present: refuse rather than overwrite one of them
    g.write("mods/common.rpf.disabled", b"other");
    assert!(matches!(set_enabled(&g.0, "common.rpf", false), Err(ModError::Exists(_))));
}

#[test]
fn remove_deletes_either_state_and_tidies_empty_folders() {
    let g = Dir::new("remove");
    g.write("mods/update/x64/dlc.rpf.disabled", b"x");
    g.write("mods/keep.txt", b"k");
    remove(&g.0, "update/x64/dlc.rpf").unwrap();
    assert!(!g.0.join("mods/update").exists(), "empty parents are removed");
    assert!(g.0.join("mods").is_dir() && g.0.join("mods/keep.txt").exists());
    assert!(matches!(remove(&g.0, "update/x64/dlc.rpf"), Err(ModError::NotFound(_))));
    assert!(remove(&g.0, "../keep.txt").is_err());
}

#[test]
fn installing_a_folder_keeps_its_layout_and_is_all_or_nothing() {
    let g = Dir::new("install");
    let pkg = Dir::new("pkg");
    pkg.write("update/x64/dlcpacks/mp/dlc.rpf", b"dlc");
    pkg.write("data/handling.meta", b"h");
    let done = install(&g.0, &pkg.0, None, false).unwrap();
    assert_eq!(done.len(), 2);
    assert_eq!(fs::read(g.0.join("mods/update/x64/dlcpacks/mp/dlc.rpf")).unwrap(), b"dlc");

    // a conflict on any file installs nothing from the second attempt
    pkg.write("brand_new.txt", b"n");
    assert!(matches!(install(&g.0, &pkg.0, None, false), Err(ModError::Exists(_))));
    assert!(!g.0.join("mods/brand_new.txt").exists(), "no partial install");
    install(&g.0, &pkg.0, None, true).unwrap();
    assert!(g.0.join("mods/brand_new.txt").exists());
}

#[test]
fn installing_a_single_file_uses_dest_as_its_path() {
    let g = Dir::new("single");
    let src = g.write("downloads/handling.meta", b"h");
    assert_eq!(install(&g.0, &src, None, false).unwrap(), ["handling.meta"]);
    assert_eq!(
        install(&g.0, &src, Some("common/data/handling.meta"), false).unwrap(),
        ["common/data/handling.meta"]
    );
    assert!(g.0.join("mods/common/data/handling.meta").exists());
    assert!(install(&g.0, &src, Some("../escape.meta"), false).is_err());
    assert!(matches!(install(&g.0, &g.0.join("nope"), None, false), Err(ModError::NotFound(_))));
}

#[cfg(unix)]
#[cfg(unix)]
#[test]
fn symlinked_folders_cannot_redirect_writes_out_of_mods() {
    let g = Dir::new("symlink");
    let outside = Dir::new("outside");
    fs::create_dir_all(g.0.join("mods")).unwrap();
    std::os::unix::fs::symlink(&outside.0, g.0.join("mods/link")).unwrap();
    g.write("common.rpf", b"x");
    let src = g.write("pkg/f.txt", b"f");
    assert!(install(&g.0, &src, Some("link/f.txt"), false).is_err());
    assert!(!outside.0.join("f.txt").exists(), "nothing was written through the link");
}

#[cfg(unix)]
#[test]
fn a_link_cannot_be_used_through_folders_that_do_not_exist_yet() {
    let g = Dir::new("deep-link");
    let outside = Dir::new("deep-outside");
    fs::create_dir_all(g.0.join("mods")).unwrap();
    std::os::unix::fs::symlink(&outside.0, g.0.join("mods/link")).unwrap();
    let src = g.write("pkg/f.txt", b"f");
    // `link/new/` does not exist, so only an ancestor can reveal where it would be created
    assert!(install(&g.0, &src, Some("link/new/deeper/f.txt"), false).is_err());
    assert!(!outside.0.join("new").exists(), "no folder was created behind the link");

    // a dangling link in place of the mods folder is refused rather than followed
    let h = Dir::new("dangling");
    std::os::unix::fs::symlink(h.0.join("nowhere"), h.0.join("mods")).unwrap();
    let src = h.write("pkg/f.txt", b"f");
    assert!(install(&h.0, &src, None, false).is_err());

    // ordinary nested destinations in a fresh game folder still work
    let ok = Dir::new("fresh");
    let src = ok.write("pkg/f.txt", b"f");
    assert_eq!(install(&ok.0, &src, Some("a/b/f.txt"), false).unwrap(), ["a/b/f.txt"]);
}
