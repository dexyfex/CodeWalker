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
