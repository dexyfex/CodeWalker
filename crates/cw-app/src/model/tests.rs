use cw_core::{
    Blob, RpfTree,
    rpf::build::{Item as B, RpfBuilder},
};

use super::*;

fn archive(items: Vec<B>) -> Vec<u8> {
    RpfBuilder::new(items).build().unwrap()
}

fn tree_from(files: &[(&str, Vec<u8>)]) -> Arc<RpfTree> {
    let mut tree: Option<RpfTree> = None;
    for (rel, bytes) in files {
        let name = rel.rsplit('/').next().unwrap().to_string();
        let t = RpfTree::scan_blob(Blob::from_vec(bytes.clone()), rel.into(), name, rel, None).unwrap();
        match tree.as_mut() {
            Some(m) => m.merge(t),
            None => tree = Some(t),
        }
    }
    Arc::new(tree.unwrap())
}

fn sample() -> Explorer {
    let common = archive(vec![
        B::file("zeta.txt", &b"z"[..]),
        B::dir(
            "levels",
            vec![
                B::packed("map10.xml", "<a/>".repeat(50).into_bytes()),
                B::file("map2.xml", &b"<b/>"[..]),
                B::resource("prop.ydr", &b"payload"[..], 1 << 17, 0x9000_0000),
            ],
        ),
        B::archive("inner.rpf", archive(vec![B::file("deep.txt", &b"deep"[..])])),
    ]);
    let update = archive(vec![B::file("patch.dat", &b"p"[..])]);
    let dlc = archive(vec![B::file("dlc.xml", &b"<d/>"[..])]);
    let mut e = Explorer::default();
    e.set_tree(Some(tree_from(&[
        ("common.rpf", common),
        ("update/update.rpf", update),
        ("update/x64/dlcpacks/mp/dlc.rpf", dlc),
    ])));
    e
}

fn names(e: &Explorer) -> Vec<String> {
    let t = e.tree.as_ref().unwrap();
    e.listing
        .iter()
        .map(|i| match i {
            Item::Folder(p) => format!("[{}]", p.rsplit('/').next().unwrap()),
            Item::Entry(r) => t.entry(*r).name.clone(),
        })
        .collect()
}

fn labels(e: &Explorer) -> Vec<String> {
    e.rows.iter().map(|r| format!("{}{}", "  ".repeat(r.depth as usize), r.label)).collect()
}

#[test]
fn virtual_folders_group_top_level_archives_by_directory() {
    let e = sample();
    assert_eq!(names(&e), ["[update]", "common.rpf"], "folders first, then archives");
    let mut e = e;
    e.navigate(NodeId::Folder("update".into()));
    assert_eq!(names(&e), ["[x64]", "update.rpf"]);
    e.navigate(NodeId::Folder("update/x64/dlcpacks/mp".into()));
    assert_eq!(names(&e), ["dlc.rpf"]);
}

#[test]
fn tree_rows_follow_expansion_state() {
    let mut e = sample();
    assert_eq!(labels(&e), ["Game files", "  update", "  common.rpf"]);
    e.toggle(&NodeId::Folder("update".into()));
    assert_eq!(labels(&e), ["Game files", "  update", "    x64", "    update.rpf", "  common.rpf"]);
    e.toggle(&NodeId::Folder("update".into()));
    assert_eq!(labels(&e).len(), 3);
    let common = NodeId::Cont(EntryRef { archive: 0, index: 0 });
    e.toggle(&common);
    assert_eq!(
        labels(&e)[3..],
        ["    inner.rpf", "    levels"].map(String::from),
        "archives and folders inside are nodes"
    );
}

#[test]
fn entering_an_archive_lists_its_contents_with_containers_first() {
    let mut e = sample();
    e.navigate(NodeId::Cont(EntryRef { archive: 0, index: 0 }));
    assert_eq!(names(&e), ["levels", "inner.rpf", "zeta.txt"]);
    let levels = e.listing[0].clone();
    assert!(e.open_item(&levels));
    assert_eq!(names(&e), ["map2.xml", "map10.xml", "prop.ydr"], "natural order");
    let crumbs: Vec<String> = e.breadcrumbs(&e.current).into_iter().map(|(l, _)| l).collect();
    assert_eq!(crumbs, ["Game files", "common.rpf", "levels"]);
}

#[test]
fn nested_archive_parent_chain_reaches_the_host_directory() {
    let mut e = sample();
    let inner = e.tree.as_ref().unwrap().find("common.rpf/inner.rpf").unwrap();
    let node = e.container_of(inner).unwrap();
    e.navigate(node.clone());
    let crumbs: Vec<String> = e.breadcrumbs(&node).into_iter().map(|(l, _)| l).collect();
    assert_eq!(crumbs, ["Game files", "common.rpf", "inner.rpf"]);
    e.up();
    assert_eq!(e.current, NodeId::Cont(EntryRef { archive: 0, index: 0 }));
    e.up();
    assert_eq!(e.current, NodeId::Folder(String::new()));
    e.up();
    assert_eq!(e.current, NodeId::Folder(String::new()), "the root has no parent");
}

#[test]
fn history_goes_back_and_forward_and_truncates_on_new_navigation() {
    let mut e = sample();
    e.navigate(NodeId::Folder("update".into()));
    e.navigate(NodeId::Cont(EntryRef { archive: 0, index: 0 }));
    assert!(e.can_back() && !e.can_forward());
    e.back();
    assert_eq!(e.current, NodeId::Folder("update".into()));
    e.forward();
    assert_eq!(e.current, NodeId::Cont(EntryRef { archive: 0, index: 0 }));
    e.back();
    e.back();
    assert_eq!(e.current, NodeId::Folder(String::new()));
    e.navigate(NodeId::Folder("update".into()));
    assert!(!e.can_forward(), "a new location discards the forward history");
}

fn enter_levels(e: &mut Explorer) {
    let levels = e.tree.as_ref().unwrap().find("common.rpf/levels").unwrap();
    e.navigate(NodeId::Cont(levels));
}

#[test]
fn sorting_flips_files_but_keeps_containers_on_top() {
    let mut e = sample();
    e.navigate(NodeId::Cont(EntryRef { archive: 0, index: 0 }));
    e.set_sort(SortKey::Name); // same key again: descending
    assert!(e.descending);
    assert_eq!(names(&e), ["levels", "inner.rpf", "zeta.txt"], "folders, then archives, then files");

    enter_levels(&mut e);
    e.sort = SortKey::Name;
    e.descending = true;
    e.refresh();
    assert_eq!(names(&e), ["prop.ydr", "map10.xml", "map2.xml"]);

    e.set_sort(SortKey::Size); // new key: ascending
    assert!(!e.descending);
    assert_eq!(names(&e), ["map2.xml", "map10.xml", "prop.ydr"]);
    e.set_sort(SortKey::Size);
    assert_eq!(names(&e), ["prop.ydr", "map10.xml", "map2.xml"]);

    e.set_sort(SortKey::Type);
    assert_eq!(names(&e)[0], "prop.ydr", "Drawable sorts before XML");
}

#[test]
fn filter_narrows_the_listing_and_clears_on_navigation() {
    let mut e = sample();
    enter_levels(&mut e);
    e.set_filter("MAP".into());
    assert_eq!(names(&e), ["map2.xml", "map10.xml"], "case-insensitive");
    e.set_filter("nothing-matches".into());
    assert!(e.listing.is_empty());
    e.up();
    assert!(e.filter.is_empty(), "moving away resets the filter");
    assert!(!e.listing.is_empty());
}

#[test]
fn search_results_replace_the_listing_until_navigation() {
    let mut e = sample();
    let t = e.tree.clone().unwrap();
    let hits = t.search_all("map");
    assert_eq!(hits.len(), 2);
    e.show_search("map".into(), hits.len(), hits.clone());
    assert_eq!(names(&e), ["map2.xml", "map10.xml"]);
    assert_eq!(e.search.as_ref().unwrap().total, 2);

    // revealing a result goes to its folder and selects it
    e.reveal(hits[0]);
    assert!(e.search.is_none());
    assert_eq!(
        e.selected.as_ref().and_then(|i| if let Item::Entry(r) = i { Some(*r) } else { None }),
        Some(hits[0])
    );
    assert_eq!(e.breadcrumbs(&e.current).last().unwrap().0, "levels");
    assert!(names(&e).contains(&"map10.xml".to_string()) || names(&e).contains(&"map2.xml".to_string()));
}

#[test]
fn keyboard_selection_is_clamped_and_starts_from_either_end() {
    let mut e = sample();
    enter_levels(&mut e);
    assert!(e.selected.is_none());
    e.move_selection(1);
    assert_eq!(e.selected_index(), Some(0), "down from nothing selects the first row");
    e.move_selection(10);
    assert_eq!(e.selected_index(), Some(2), "clamped to the last row");
    e.move_selection(-10);
    assert_eq!(e.selected_index(), Some(0));
    e.selected = None;
    e.move_selection(-1);
    assert_eq!(e.selected_index(), Some(2), "up from nothing selects the last row");
}

#[test]
fn files_are_selected_not_entered() {
    let mut e = sample();
    enter_levels(&mut e);
    let file = e.listing[0].clone();
    assert!(!e.open_item(&file));
    assert_eq!(e.selected, Some(file));
    assert!(
        e.container_of(
            e.selected.as_ref().and_then(|i| if let Item::Entry(r) = i { Some(*r) } else { None }).unwrap()
        )
        .is_none()
    );
}

#[test]
fn replacing_the_tree_keeps_a_valid_location_and_resets_an_invalid_one() {
    let mut e = sample();
    e.navigate(NodeId::Folder("update".into()));
    let t = e.tree.clone();
    e.set_tree(t);
    assert_eq!(e.current, NodeId::Folder("update".into()), "same tree: stay put");

    e.set_tree(Some(tree_from(&[("only.rpf", archive(vec![B::file("a.txt", &b"a"[..])]))])));
    assert_eq!(e.current, NodeId::Folder(String::new()), "location vanished: back to the root");
    assert_eq!(names(&e), ["only.rpf"]);

    e.set_tree(None);
    assert!(e.is_empty() && e.rows.is_empty() && e.listing.is_empty());
}

#[test]
fn item_paths_are_what_the_commands_expect() {
    let mut e = sample();
    enter_levels(&mut e);
    assert_eq!(e.item_path(&e.listing[2].clone()), "common.rpf/levels/prop.ydr");
    assert_eq!(e.item_path(&Item::Folder("update".into())), "update");
}

#[test]
fn child_counts_cover_folders_archives_and_directories() {
    let e = sample();
    assert_eq!(e.child_count(&Item::Folder(String::new())), Some(2), "update + common.rpf");
    assert_eq!(e.child_count(&Item::Folder("update".into())), Some(2));
    let common = Item::Entry(EntryRef { archive: 0, index: 0 });
    assert_eq!(e.child_count(&common), Some(3));
    let inner = e.tree.as_ref().unwrap().find("common.rpf/inner.rpf").unwrap();
    assert_eq!(e.child_count(&Item::Entry(inner)), Some(1), "a nested archive counts its root's children");
    let levels = e.tree.as_ref().unwrap().find("common.rpf/levels").unwrap();
    assert_eq!(e.child_count(&Item::Entry(levels)), Some(3));
    let file = e.tree.as_ref().unwrap().find("common.rpf/zeta.txt").unwrap();
    assert_eq!(e.child_count(&Item::Entry(file)), None);
}
