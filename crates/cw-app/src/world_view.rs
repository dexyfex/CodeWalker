//! Geometry and picking for the map viewport. Pure functions over the scene
//! (no egui, no GPU), so they can be tested.

use cw_gpu::{
    BoxInstance, LineVertex,
    pick::{self, Ray},
};
use cw_world::{EntityId, Scene, Transform};
use glam::{Mat4, Vec3};

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Axis {
    X,
    Y,
    Z,
}

impl Axis {
    pub const ALL: [Axis; 3] = [Axis::X, Axis::Y, Axis::Z];

    pub fn dir(self) -> Vec3 {
        match self {
            Axis::X => Vec3::X,
            Axis::Y => Vec3::Y,
            Axis::Z => Vec3::Z,
        }
    }

    pub fn color(self) -> [f32; 4] {
        match self {
            Axis::X => [0.90, 0.32, 0.36, 1.0],
            Axis::Y => [0.36, 0.80, 0.50, 1.0],
            Axis::Z => [0.43, 0.66, 1.00, 1.0],
        }
    }
}

pub const OUTLINE: [f32; 4] = [0.43, 0.66, 1.0, 1.0];

/// A calm per-model colour so repeated props read as a group. Deterministic
/// in the archetype hash; low saturation keeps the viewport neutral.
pub fn entity_color(hash: u32) -> [f32; 4] {
    let h = (hash.wrapping_mul(2_654_435_761) >> 8) % 360;
    hsv(h as f32, 0.30, 0.78)
}

fn hsv(h: f32, s: f32, v: f32) -> [f32; 4] {
    let (c, hp) = (v * s, h / 60.0);
    let x = c * (1.0 - (hp % 2.0 - 1.0).abs());
    let (r, g, b) = match hp as u32 {
        0 => (c, x, 0.0),
        1 => (x, c, 0.0),
        2 => (0.0, c, x),
        3 => (0.0, x, c),
        4 => (x, 0.0, c),
        _ => (c, 0.0, x),
    };
    let m = v - c;
    [r + m, g + m, b + m, 1.0]
}

/// The transform to draw an entity with: the live drag preview when there is one.
fn effective(scene_t: &Transform, id: EntityId, preview: Option<(EntityId, Transform)>) -> Transform {
    match preview {
        Some((pid, t)) if pid == id => t,
        _ => *scene_t,
    }
}

pub fn instances(scene: &Scene, preview: Option<(EntityId, Transform)>) -> Vec<BoxInstance> {
    scene
        .entities
        .iter()
        .map(|e| {
            let t = effective(&e.transform, e.id, preview);
            let mut color = entity_color(e.archetype_hash);
            if scene.selection.contains(&e.id) {
                for c in &mut color[..3] {
                    *c = (*c * 0.6 + 0.4).min(1.0); // lift selected boxes toward white
                }
            }
            BoxInstance { model: t.matrix().to_cols_array_2d(), color }
        })
        .collect()
}

/// The 12 edges of a unit cube transformed by `model`, as line-list vertices.
pub fn outline(model: &Mat4, color: [f32; 4]) -> Vec<LineVertex> {
    let corner = |i: usize| {
        Vec3::new(
            if i & 1 == 0 { -0.5 } else { 0.5 },
            if i & 2 == 0 { -0.5 } else { 0.5 },
            if i & 4 == 0 { -0.5 } else { 0.5 },
        )
    };
    let mut out = Vec::with_capacity(24);
    for a in 0..8usize {
        for bit in [1usize, 2, 4] {
            let b = a | bit;
            if b != a {
                for p in [a, b] {
                    out.push(LineVertex { pos: model.transform_point3(corner(p)).to_array(), color });
                }
            }
        }
    }
    out
}

pub fn selection_outlines(scene: &Scene, preview: Option<(EntityId, Transform)>) -> Vec<LineVertex> {
    scene
        .entities
        .iter()
        .filter(|e| scene.selection.contains(&e.id))
        .flat_map(|e| outline(&effective(&e.transform, e.id, preview).matrix(), OUTLINE))
        .collect()
}

/// Nearest entity under the ray.
pub fn pick_entity(scene: &Scene, ray: &Ray, preview: Option<(EntityId, Transform)>) -> Option<EntityId> {
    scene
        .entities
        .iter()
        .filter_map(|e| {
            pick::ray_obb(ray, &effective(&e.transform, e.id, preview).matrix()).map(|t| (t, e.id))
        })
        .min_by(|a, b| a.0.total_cmp(&b.0))
        .map(|(_, id)| id)
}

/// World length of a gizmo axis so it keeps a constant size on screen.
pub fn gizmo_length(camera: Vec3, origin: Vec3) -> f32 {
    ((camera - origin).length() * 0.16).max(0.5)
}

pub fn gizmo_lines(origin: Vec3, length: f32, hover: Option<Axis>, active: Option<Axis>) -> Vec<LineVertex> {
    let mut out = Vec::new();
    for axis in Axis::ALL {
        let mut c = axis.color();
        if Some(axis) == hover || Some(axis) == active {
            for v in &mut c[..3] {
                *v = (*v * 0.5 + 0.5).min(1.0);
            }
        }
        let d = axis.dir();
        let tip = origin + d * length;
        let (u, w) = match axis {
            Axis::X => (Vec3::Y, Vec3::Z),
            Axis::Y => (Vec3::X, Vec3::Z),
            Axis::Z => (Vec3::X, Vec3::Y),
        };
        let seg = |a: Vec3, b: Vec3| {
            [LineVertex { pos: a.to_array(), color: c }, LineVertex { pos: b.to_array(), color: c }]
        };
        out.extend(seg(origin, tip));
        let back = tip - d * length * 0.2;
        for off in [u, -u, w, -w] {
            out.extend(seg(tip, back + off * length * 0.07));
        }
    }
    out
}

/// Which axis handle the ray is closest to (within a screen-proportional reach).
pub fn pick_axis(ray: &Ray, origin: Vec3, length: f32) -> Option<Axis> {
    Axis::ALL
        .into_iter()
        .map(|a| (pick::ray_segment_distance(ray, origin, origin + a.dir() * length).0, a))
        .filter(|(d, _)| *d < length * 0.09)
        .min_by(|a, b| a.0.total_cmp(&b.0))
        .map(|(_, a)| a)
}

/// Where along `axis` the ray points, in world units from `origin`.
pub fn axis_param(ray: &Ray, origin: Vec3, axis: Axis) -> Option<f32> {
    pick::closest_on_line(ray, origin, axis.dir())
}

#[cfg(test)]
mod tests {
    use cw_world::World;

    use super::*;

    fn world_with(positions: &[(f32, f32, f32)]) -> World {
        let mut w = World::new("t");
        for (i, p) in positions.iter().enumerate() {
            w.add(
                &format!("prop_{i}"),
                Transform { position: Vec3::new(p.0, p.1, p.2), ..Default::default() },
            )
            .unwrap();
        }
        w
    }

    #[test]
    fn colours_are_stable_and_in_range() {
        assert_eq!(entity_color(123), entity_color(123));
        assert_ne!(entity_color(1), entity_color(2));
        for h in [0u32, 1, 0xFFFF_FFFF, 0xDEAD_BEEF] {
            let c = entity_color(h);
            assert!(c.iter().all(|v| (0.0..=1.0).contains(v)) && c[3] == 1.0, "{c:?}");
            assert!(c[..3].iter().cloned().fold(0.0, f32::max) <= 0.79, "kept calm, not neon: {c:?}");
        }
    }

    #[test]
    fn selected_boxes_are_lighter() {
        let mut w = world_with(&[(0.0, 0.0, 0.0), (5.0, 0.0, 0.0)]);
        w.select([2], false);
        let i = instances(&w.scene, None);
        let lum = |c: [f32; 4]| c[0] + c[1] + c[2];
        assert!(
            lum(i[1].color) > lum(i[0].color)
                || entity_color(w.scene.entities[0].archetype_hash) == i[0].color
        );
        assert_eq!(
            i[0].color,
            entity_color(w.scene.entities[0].archetype_hash),
            "unselected keeps its colour"
        );
        assert!(lum(i[1].color) > lum(entity_color(w.scene.entities[1].archetype_hash)));
    }

    #[test]
    fn preview_overrides_only_the_dragged_entity() {
        let w = world_with(&[(0.0, 0.0, 0.0), (5.0, 0.0, 0.0)]);
        let moved = Transform { position: Vec3::new(9.0, 9.0, 9.0), ..Default::default() };
        // entity ids start at 1: id 1 is the first box, id 2 the second
        let first = instances(&w.scene, Some((1, moved)));
        assert_eq!(first[0].model[3][..3], [9.0, 9.0, 9.0], "translation lives in the last column");
        assert_eq!(first[1].model[3][0], 5.0);
        let second = instances(&w.scene, Some((2, moved)));
        assert_eq!(second[0].model[3][0], 0.0);
        assert_eq!(second[1].model[3][..3], [9.0, 9.0, 9.0]);
        assert_eq!(
            instances(&w.scene, Some((99, moved)))[0].model[3][0],
            0.0,
            "an unknown id changes nothing"
        );
    }

    #[test]
    fn outline_is_the_twelve_cube_edges() {
        let o = outline(&Mat4::from_scale(Vec3::splat(2.0)), OUTLINE);
        assert_eq!(o.len(), 24);
        assert!(
            o.iter().all(|v| v.pos.iter().all(|c| (c.abs() - 1.0).abs() < 1e-5)),
            "every vertex is a corner of the 2 m cube"
        );
        for pair in o.chunks(2) {
            let d = (Vec3::from(pair[0].pos) - Vec3::from(pair[1].pos)).abs();
            assert_eq!(
                (d.x > 0.0) as u8 + (d.y > 0.0) as u8 + (d.z > 0.0) as u8,
                1,
                "an edge moves along exactly one axis"
            );
            assert!((d.max_element() - 2.0).abs() < 1e-5);
        }
    }

    #[test]
    fn picking_returns_the_nearest_hit() {
        let w = world_with(&[(10.0, 0.0, 0.0), (20.0, 0.0, 0.0), (10.0, 5.0, 0.0)]);
        let ray = Ray { origin: Vec3::ZERO, dir: Vec3::X };
        assert_eq!(pick_entity(&w.scene, &ray, None), Some(1), "the nearer of two on the same line");
        let up = Ray { origin: Vec3::new(10.0, 5.0, 10.0), dir: -Vec3::Z };
        assert_eq!(pick_entity(&w.scene, &up, None), Some(3));
        let miss = Ray { origin: Vec3::new(0.0, 50.0, 0.0), dir: Vec3::X };
        assert_eq!(pick_entity(&w.scene, &miss, None), None);
        // while dragging, the preview position is what can be hit
        let moved = Transform { position: Vec3::new(0.0, 50.0, 0.0), ..Default::default() };
        assert_eq!(
            pick_entity(&w.scene, &miss, Some((1, moved))),
            Some(1),
            "the dragged preview position is what can be hit"
        );
    }

    #[test]
    fn gizmo_axis_picking_and_dragging() {
        let origin = Vec3::new(100.0, 100.0, 0.0);
        let len = gizmo_length(Vec3::new(100.0, 60.0, 30.0), origin);
        assert!(len > 5.0, "{len}");
        // looking straight down at the middle of the X arm
        let down = Ray { origin: origin + Vec3::new(len * 0.5, 0.0, 50.0), dir: -Vec3::Z };
        assert_eq!(pick_axis(&down, origin, len), Some(Axis::X));
        let far = Ray { origin: origin + Vec3::new(len * 0.5, len * 0.6, 50.0), dir: -Vec3::Z };
        assert_eq!(pick_axis(&far, origin, len), None, "well off the arm");

        // dragging: the parameter difference is the world distance moved along the axis
        let a =
            axis_param(&Ray { origin: origin + Vec3::new(2.0, 0.0, 50.0), dir: -Vec3::Z }, origin, Axis::X)
                .unwrap();
        let b =
            axis_param(&Ray { origin: origin + Vec3::new(7.5, 3.0, 50.0), dir: -Vec3::Z }, origin, Axis::X)
                .unwrap();
        assert!((b - a - 5.5).abs() < 1e-4, "{a} {b}");
    }

    #[test]
    fn gizmo_has_a_shaft_and_four_head_lines_per_axis() {
        let g = gizmo_lines(Vec3::ZERO, 10.0, None, None);
        assert_eq!(g.len(), 3 * (2 + 4 * 2));
        let hot = gizmo_lines(Vec3::ZERO, 10.0, Some(Axis::Y), None);
        assert!(hot[10].color[0] > g[10].color[0], "the hovered axis is brightened");
        assert_eq!(hot[0].color, g[0].color, "others are not");
    }
}
