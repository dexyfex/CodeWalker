use glam::{Quat, Vec3};

use super::*;

fn at(x: f32, y: f32, z: f32) -> Transform {
    Transform { position: Vec3::new(x, y, z), ..Default::default() }
}

#[test]
fn add_remove_undo_redo_restore_exact_order() {
    let mut w = World::new("t");
    let a = w.add("prop_a", at(1.0, 0.0, 0.0)).unwrap();
    let b = w.add("prop_b", at(2.0, 0.0, 0.0)).unwrap();
    let c = w.add("prop_c", at(3.0, 0.0, 0.0)).unwrap();
    w.remove(b).unwrap();
    let ids = |w: &World| w.scene.entities.iter().map(|e| e.id).collect::<Vec<_>>();
    assert_eq!(ids(&w), [a, c]);

    assert!(w.undo()); // un-remove: b returns between a and c
    assert_eq!(ids(&w), [a, b, c]);
    assert!(w.undo()); // un-add c
    assert_eq!(ids(&w), [a, b]);
    assert!(w.redo());
    assert_eq!(ids(&w), [a, b, c]);
    assert!(w.redo());
    assert_eq!(ids(&w), [a, c]);
    assert!(!w.redo(), "nothing left to redo");
}

#[test]
fn new_edits_discard_the_redo_stack() {
    let mut w = World::new("t");
    let a = w.add("x", at(0.0, 0.0, 0.0)).unwrap();
    w.set_transform(a, at(1.0, 0.0, 0.0)).unwrap();
    w.undo();
    assert!(w.can_redo());
    w.set_transform(a, at(5.0, 0.0, 0.0)).unwrap();
    assert!(!w.can_redo());
}

#[test]
fn a_drag_is_one_undo_step() {
    let mut w = World::new("t");
    let a = w.add("x", at(0.0, 0.0, 0.0)).unwrap();
    let depth = w.undo_depth();
    w.begin_gesture(a).unwrap();
    for i in 1..=50 {
        w.update_gesture(at(i as f32, 0.0, 0.0)).unwrap();
    }
    assert_eq!(w.undo_depth(), depth, "live updates are not history");
    w.end_gesture();
    assert_eq!(w.undo_depth(), depth + 1);
    assert_eq!(w.scene.get(a).unwrap().transform.position.x, 50.0);
    w.undo();
    assert_eq!(w.scene.get(a).unwrap().transform.position.x, 0.0);
}

#[test]
fn an_unchanged_drag_records_nothing_and_cancel_restores() {
    let mut w = World::new("t");
    let a = w.add("x", at(4.0, 0.0, 0.0)).unwrap();
    let depth = w.undo_depth();
    w.begin_gesture(a).unwrap();
    w.end_gesture();
    assert_eq!(w.undo_depth(), depth);

    w.begin_gesture(a).unwrap();
    w.update_gesture(at(9.0, 9.0, 9.0)).unwrap();
    w.cancel_gesture();
    assert_eq!(w.scene.get(a).unwrap().transform.position.x, 4.0);
    assert_eq!(w.undo_depth(), depth);
}

#[test]
fn undo_during_a_drag_closes_the_drag_first() {
    let mut w = World::new("t");
    let a = w.add("x", at(0.0, 0.0, 0.0)).unwrap();
    w.begin_gesture(a).unwrap();
    w.update_gesture(at(7.0, 0.0, 0.0)).unwrap();
    assert!(w.undo(), "undoes the drag itself");
    assert_eq!(w.scene.get(a).unwrap().transform.position.x, 0.0);
}

#[test]
fn invalid_transforms_are_refused_and_leave_the_scene_untouched() {
    let mut w = World::new("t");
    let a = w.add("x", at(1.0, 1.0, 1.0)).unwrap();
    let nan = Transform { position: Vec3::new(f32::NAN, 0.0, 0.0), ..Default::default() };
    assert!(matches!(w.set_transform(a, nan), Err(Error::BadTransform)));
    let flat = Transform { scale: Vec3::new(1.0, 0.0, 1.0), ..Default::default() };
    assert!(w.set_transform(a, flat).is_err());
    let skew = Transform { rotation: Quat::from_xyzw(0.0, 0.0, 0.0, 2.0), ..Default::default() };
    assert!(w.set_transform(a, skew).is_err(), "rotations must be unit quaternions");
    assert!(w.add("y", nan).is_err());
    assert_eq!(w.scene.get(a).unwrap().transform.position, Vec3::ONE);
    assert!(matches!(w.set_transform(99, at(0.0, 0.0, 0.0)), Err(Error::NoEntity(99))));
}

#[test]
fn selection_ignores_unknown_ids_and_drops_removed_entities() {
    let mut w = World::new("t");
    let a = w.add("x", Transform::default()).unwrap();
    let b = w.add("y", Transform::default()).unwrap();
    w.select([a, 77], false);
    assert_eq!(w.scene.selection.iter().copied().collect::<Vec<_>>(), [a]);
    w.select([b], true);
    assert_eq!(w.scene.selection.len(), 2);
    w.remove(a).unwrap();
    assert_eq!(w.scene.selection.iter().copied().collect::<Vec<_>>(), [b]);
    w.undo(); // un-remove does not magically reselect
    assert_eq!(w.scene.selection.len(), 1);
}

#[test]
fn rename_updates_the_hash_and_is_undoable() {
    let mut w = World::new("t");
    let a = w.add("prop_bench_01", Transform::default()).unwrap();
    let h0 = w.scene.get(a).unwrap().archetype_hash;
    assert_eq!(h0, cw_core::hash::joaat_lower("PROP_BENCH_01"), "hashes ignore case");
    w.rename(a, "prop_bin_02").unwrap();
    assert_ne!(w.scene.get(a).unwrap().archetype_hash, h0);
    w.undo();
    assert_eq!(w.scene.get(a).unwrap().archetype_hash, h0);
    assert_eq!(w.scene.get(a).unwrap().archetype, "prop_bench_01");
}

#[test]
fn json_round_trip_and_hostile_input() {
    let mut w = World::new("scratch");
    w.add("prop_a", at(1.0, 2.0, 3.0)).unwrap();
    let b = w.add("prop_b", Transform::default().with_euler_degrees(Vec3::new(0.0, 0.0, 90.0))).unwrap();
    let text = w.to_json();

    let mut w2 = World::new("other");
    w2.load_json(&text).unwrap();
    assert_eq!(w2.scene.name, "scratch");
    assert_eq!(w2.scene.entities.len(), 2);
    assert!(!w2.can_undo(), "loading starts a fresh history");
    let rot = w2.scene.get(b).unwrap().transform.euler_degrees();
    assert!((rot.z - 90.0).abs() < 1e-3, "{rot:?}");
    assert!(w2.add("c", Transform::default()).unwrap() > b, "new ids never collide with loaded ones");

    // duplicate ids, non-finite values and zero scale are dropped, not trusted
    let evil = r#"{"name":"x","entities":[
        {"id":1,"archetype":"a","archetype_hash":0,"transform":{"position":[0,0,0],"rotation":[0,0,0,1],"scale":[1,1,1]}},
        {"id":1,"archetype":"dup","archetype_hash":0,"transform":{"position":[0,0,0],"rotation":[0,0,0,1],"scale":[1,1,1]}},
        {"id":2,"archetype":"flat","archetype_hash":0,"transform":{"position":[0,0,0],"rotation":[0,0,0,1],"scale":[0,1,1]}}
    ],"next_id":0}"#;
    w2.load_json(evil).unwrap();
    assert_eq!(w2.scene.entities.len(), 1);
    assert_eq!(w2.scene.entities[0].archetype, "a");
    assert!(w2.load_json("{ not json").is_err());
    assert_eq!(w2.scene.entities.len(), 1, "a failed load keeps the current scene");
}

#[test]
fn euler_conversion_round_trips() {
    for e in [Vec3::new(10.0, 20.0, 30.0), Vec3::new(-45.0, 0.0, 170.0), Vec3::ZERO] {
        let t = Transform::default().with_euler_degrees(e);
        let back = t.euler_degrees();
        assert!((back - e).abs().max_element() < 1e-3, "{e:?} -> {back:?}");
    }
}

#[test]
fn ids_cannot_overflow_or_collide_even_with_hostile_files() {
    let mut w = World::new("t");
    let max = r#"{"name":"x","entities":[
        {"id":4294967295,"archetype":"a","archetype_hash":0,"transform":{"position":[0,0,0],"rotation":[0,0,0,1],"scale":[1,1,1]}}
    ],"next_id":4294967295}"#;
    w.load_json(max).unwrap();
    assert_eq!(w.scene.entities.len(), 1);
    assert!(
        matches!(w.add("b", Transform::default()), Err(Error::Full)),
        "no id left: refuse instead of wrapping"
    );
    assert_eq!(w.scene.entities.len(), 1);
}

#[test]
fn revision_moves_on_every_visible_change_and_never_goes_back() {
    let mut w = World::new("t");
    let mut last = w.revision();
    let mut bump = |w: &World, what: &str| {
        assert!(w.revision() > last, "{what} must change the revision");
        last = w.revision();
    };
    let a = w.add("x", Transform::default()).unwrap();
    bump(&w, "add");
    w.select([a], false);
    bump(&w, "select");
    w.set_transform(a, at(1.0, 0.0, 0.0)).unwrap();
    bump(&w, "set");
    w.undo();
    bump(&w, "undo");
    w.redo();
    bump(&w, "redo");
    w.clear_selection();
    bump(&w, "deselect");
    w.begin_gesture(a).unwrap();
    w.update_gesture(at(3.0, 0.0, 0.0)).unwrap();
    bump(&w, "live drag update");
    w.cancel_gesture();
    bump(&w, "cancel");
    let before = w.revision();
    w.load_json(&w.to_json()).unwrap();
    assert!(w.revision() > before, "loading must not reset the counter or views would miss it");
}

#[test]
fn euler_display_has_no_negative_zero_or_noise() {
    let t = Transform::default().with_euler_degrees(Vec3::new(0.0, -0.000_01, 30.0));
    let e = t.euler_degrees();
    assert_eq!(e.y.to_bits(), 0.0f32.to_bits(), "tiny negatives display as +0, got {:?}", e.y);
    assert_eq!(Transform::default().euler_degrees().to_array().map(f32::to_bits), [0u32; 3]);
}

fn new_entity(n: u32, x: f32) -> NewEntity {
    NewEntity {
        archetype: format!("0x{n:08X}"),
        archetype_hash: n,
        transform: at(x, 0.0, 0.0),
        source: "x64a.rpf/hills.ymap".into(),
    }
}

#[test]
fn an_import_is_one_undo_step_that_keeps_order_and_what_the_file_said() {
    let mut w = World::new("t");
    let first = w.add("existing", at(0.0, 0.0, 0.0)).unwrap();
    let depth = w.undo_depth();

    let (ids, skipped) = w.add_many((0..4).map(|i| new_entity(0xAB00 + i, i as f32)).collect()).unwrap();
    assert_eq!((ids.len(), skipped), (4, 0));
    assert_eq!(w.undo_depth(), depth + 1, "one step for the whole batch");
    let e = w.scene.get(ids[2]).unwrap();
    assert_eq!(
        (e.archetype.as_str(), e.archetype_hash, e.source.as_str()),
        ("0x0000AB02", 0xAB02, "x64a.rpf/hills.ymap")
    );
    assert_eq!(w.scene.entities.len(), 5);
    assert!(ids.iter().all(|i| *i != first), "fresh ids");

    w.select(ids.clone(), false);
    assert!(w.undo());
    assert_eq!(w.scene.entities.iter().map(|e| e.id).collect::<Vec<_>>(), [first], "all of it is gone");
    assert!(w.scene.selection.is_empty(), "and nothing stays selected that no longer exists");
    assert!(w.redo());
    assert_eq!(
        w.scene.entities.iter().skip(1).map(|e| e.id).collect::<Vec<_>>(),
        ids,
        "back in the same order"
    );
    assert!(w.can_undo() && !w.can_redo());

    // later edits still work on imported entities, and undo them one at a time
    w.set_transform(ids[0], at(9.0, 9.0, 9.0)).unwrap();
    assert!(w.undo());
    assert_eq!(w.scene.get(ids[0]).unwrap().transform, at(0.0, 0.0, 0.0));
}

#[test]
fn an_import_leaves_out_what_cannot_be_placed_and_changes_nothing_if_it_cannot_all_fit() {
    let mut w = World::new("t");
    let mut bad = new_entity(2, f32::NAN);
    bad.transform.position.x = f32::NAN;
    let (ids, skipped) = w.add_many(vec![new_entity(1, 1.0), bad, new_entity(3, 3.0)]).unwrap();
    assert_eq!((ids.len(), skipped), (2, 1));

    let depth = w.undo_depth();
    assert_eq!(w.add_many(Vec::new()).unwrap(), (Vec::new(), 0));
    assert_eq!(w.add_many(vec![new_entity(9, f32::INFINITY)]).unwrap().1, 1);
    assert_eq!(w.undo_depth(), depth, "nothing to add is not an edit");

    // ids almost used up: the whole batch is refused and the counter is not advanced
    w.scene.next_id = EntityId::MAX - 2;
    let count = w.scene.entities.len();
    let err = w.add_many((0..5).map(|i| new_entity(i, 0.0)).collect()).unwrap_err();
    assert!(matches!(err, WorldError::Full));
    assert_eq!((w.scene.entities.len(), w.undo_depth(), w.scene.next_id), (count, depth, EntityId::MAX - 2));
}

#[test]
fn a_very_large_import_undoes_and_redoes_at_once() {
    let mut w = World::new("t");
    let (ids, _) = w.add_many((0..60_000).map(|i| new_entity(i, i as f32)).collect()).unwrap();
    assert_eq!(ids.len(), 60_000);
    assert!(w.undo());
    assert!(w.scene.entities.is_empty());
    assert!(w.redo());
    assert_eq!(w.scene.entities.len(), 60_000);
    assert_eq!(w.scene.entities[59_999].id, ids[59_999]);
}

#[test]
fn importing_into_a_scene_that_is_already_big_does_not_rescan_it_per_entity() {
    let mut w = World::new("t");
    w.add_many((0..60_000).map(|i| new_entity(i, 0.0)).collect()).unwrap();
    // 60k x 60k id checks would take minutes; a single pass is a few milliseconds
    let started = std::time::Instant::now();
    let (ids, _) = w.add_many((0..60_000).map(|i| new_entity(i, 1.0)).collect()).unwrap();
    assert_eq!(ids.len(), 60_000);
    assert_eq!(w.scene.entities.len(), 120_000);
    assert!(started.elapsed().as_secs() < 5, "took {:?}", started.elapsed());
    let mut sorted = ids.clone();
    sorted.dedup();
    assert_eq!(sorted.len(), ids.len(), "ids are distinct");
}

#[test]
fn ids_come_in_runs_and_refuse_to_collide_with_what_is_there() {
    let mut s = Scene::new("t");
    assert_eq!(s.alloc_ids(0), Some(1..1));
    assert_eq!(s.alloc_ids(3), Some(1..4));
    assert_eq!(s.alloc_id(), Some(4));

    // a scene loaded without cleaning could hold an id the counter has not reached
    s.entities.push(Entity::new(7, "x", Transform::default()));
    assert_eq!(s.alloc_ids(10), None, "5..15 would collide with 7");
    assert_eq!(s.next_id, 5, "and a refusal uses nothing");
    assert_eq!(s.alloc_ids(2), Some(5..7));

    s.next_id = EntityId::MAX - 2;
    assert_eq!(s.alloc_ids(2), Some(EntityId::MAX - 2..EntityId::MAX), "the last id is never handed out");
    s.next_id = EntityId::MAX - 2;
    assert_eq!(s.alloc_ids(3), None);
    assert_eq!(s.alloc_ids(usize::MAX), None);
}

#[test]
fn replacing_the_scene_is_one_step_and_undo_brings_the_old_one_back() {
    let mut w = World::new("old");
    let a = w.add("a", at(1.0, 0.0, 0.0)).unwrap();
    let b = w.add("b", at(2.0, 0.0, 0.0)).unwrap();
    w.select([a], false);
    let depth = w.undo_depth();
    let seen = w.revision();

    let (ids, skipped) =
        w.replace_all("hills.ymap", (0..3).map(|i| new_entity(i, i as f32)).collect()).unwrap();
    assert_eq!((ids.len(), skipped), (3, 0));
    assert_eq!(w.undo_depth(), depth + 1);
    assert!(w.revision() > seen);
    assert_eq!(w.scene.name, "hills.ymap");
    assert_eq!(w.scene.entities.iter().map(|e| e.id).collect::<Vec<_>>(), ids);
    assert!(ids.iter().all(|i| *i != a && *i != b), "old ids are not handed out again");
    assert!(w.scene.selection.is_empty());

    assert!(w.undo());
    assert_eq!(w.scene.name, "old");
    assert_eq!(w.scene.entities.iter().map(|e| e.id).collect::<Vec<_>>(), [a, b]);
    assert_eq!(w.scene.get(b).unwrap().transform, at(2.0, 0.0, 0.0));
    assert!(w.redo());
    assert_eq!((w.scene.name.as_str(), w.scene.entities.len()), ("hills.ymap", 3));

    // earlier steps are still there under it
    assert!(w.undo() && w.undo());
    assert_eq!(w.scene.entities.iter().map(|e| e.id).collect::<Vec<_>>(), [a]);
}

#[test]
fn a_replace_that_cannot_fit_leaves_the_scene_and_history_alone() {
    let mut w = World::new("old");
    w.add("a", Transform::default()).unwrap();
    let depth = w.undo_depth();
    w.scene.next_id = EntityId::MAX - 1;
    let err = w.replace_all("new", (0..5).map(|i| new_entity(i, 0.0)).collect()).unwrap_err();
    assert!(matches!(err, WorldError::Full));
    assert_eq!((w.scene.name.as_str(), w.scene.entities.len(), w.undo_depth()), ("old", 1, depth));
}

#[test]
fn a_reset_scene_never_repeats_a_revision_a_view_may_have_seen() {
    let mut w = World::new("a");
    for i in 0..3 {
        w.add("m", at(i as f32, 0.0, 0.0)).unwrap();
    }
    let seen = w.revision();
    w.reset("b");
    assert!(w.revision() > seen, "reset moves forward");
    assert!(w.scene.entities.is_empty() && !w.can_undo() && !w.is_dirty());
    assert_eq!(w.scene.name, "b");
    for i in 0..5 {
        w.add("m", at(i as f32, 0.0, 0.0)).unwrap();
    }
    assert!(w.revision() > seen + 1, "and keeps going, so 3 + new edits never lands on a stale value");
}
