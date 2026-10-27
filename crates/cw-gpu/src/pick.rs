//! Ray casting against scene entities and gizmo handles (CPU, no GPU needed).

use glam::{Mat4, Vec3};

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Ray {
    pub origin: Vec3,
    pub dir: Vec3,
}

impl Ray {
    pub fn at(&self, t: f32) -> Vec3 {
        self.origin + self.dir * t
    }
}

/// Distance along `ray` to a unit cube (side 1, centred on the origin)
/// transformed by `model`. `None` on a miss, behind the ray, or when the
/// transform cannot be inverted.
pub fn ray_obb(ray: &Ray, model: &Mat4) -> Option<f32> {
    if model.determinant().abs() < 1e-12 {
        return None;
    }
    let inv = model.inverse();
    let o = inv.transform_point3(ray.origin);
    let d = inv.transform_vector3(ray.dir);
    let (mut t_min, mut t_max) = (f32::NEG_INFINITY, f32::INFINITY);
    for i in 0..3 {
        if d[i].abs() < 1e-9 {
            if o[i].abs() > 0.5 {
                return None;
            }
        } else {
            let (a, b) = ((-0.5 - o[i]) / d[i], (0.5 - o[i]) / d[i]);
            t_min = t_min.max(a.min(b));
            t_max = t_max.min(a.max(b));
        }
    }
    if t_max < t_min.max(0.0) {
        return None;
    }
    // local t is the same parameter as world t because `d` was not renormalised
    Some(if t_min >= 0.0 { t_min } else { t_max })
}

/// Parameter along the infinite line `origin + t * dir` of the point closest to `ray`.
/// `None` when the lines are (nearly) parallel.
pub fn closest_on_line(ray: &Ray, origin: Vec3, dir: Vec3) -> Option<f32> {
    let w0 = ray.origin - origin;
    let (a, b, c) = (ray.dir.dot(ray.dir), ray.dir.dot(dir), dir.dot(dir));
    let (d, e) = (ray.dir.dot(w0), dir.dot(w0));
    let denom = a * c - b * b;
    if denom.abs() < 1e-8 * a * c {
        return None;
    }
    Some((a * e - b * d) / denom)
}

/// Shortest distance from `ray` to the segment `a..b`, and the parameter (0..1) of the closest point.
pub fn ray_segment_distance(ray: &Ray, a: Vec3, b: Vec3) -> (f32, f32) {
    let seg = b - a;
    let t = closest_on_line(ray, a, seg).unwrap_or(0.0).clamp(0.0, 1.0);
    let p = a + seg * t;
    // distance from p to the ray (clamped to the ray's forward half)
    let s = (p - ray.origin).dot(ray.dir).max(0.0);
    ((p - ray.at(s)).length(), t)
}

/// Distance along `ray` to the plane through `point` with `normal`.
pub fn ray_plane(ray: &Ray, point: Vec3, normal: Vec3) -> Option<f32> {
    let denom = ray.dir.dot(normal);
    if denom.abs() < 1e-8 {
        return None;
    }
    let t = (point - ray.origin).dot(normal) / denom;
    (t >= 0.0).then_some(t)
}

#[cfg(test)]
mod tests {
    use glam::Quat;

    use super::*;

    fn ray(o: Vec3, d: Vec3) -> Ray {
        Ray { origin: o, dir: d.normalize() }
    }

    #[test]
    fn hits_a_unit_cube_head_on() {
        let t = ray_obb(&ray(Vec3::new(-5.0, 0.0, 0.0), Vec3::X), &Mat4::IDENTITY).unwrap();
        assert!((t - 4.5).abs() < 1e-5);
        assert!(
            ray_obb(&ray(Vec3::new(-5.0, 2.0, 0.0), Vec3::X), &Mat4::IDENTITY).is_none(),
            "passes beside it"
        );
        assert!(
            ray_obb(&ray(Vec3::new(5.0, 0.0, 0.0), Vec3::X), &Mat4::IDENTITY).is_none(),
            "cube is behind the ray"
        );
    }

    #[test]
    fn respects_scale_rotation_and_translation() {
        let m = Mat4::from_scale_rotation_translation(
            Vec3::new(4.0, 1.0, 1.0),
            Quat::from_rotation_z(std::f32::consts::FRAC_PI_2),
            Vec3::new(10.0, 0.0, 0.0),
        );
        // rotated 90° about Z, the long (4 m) side now runs along Y
        assert!(ray_obb(&ray(Vec3::new(10.0, -10.0, 0.0), Vec3::Y), &m).is_some());
        let hit = ray_obb(&ray(Vec3::new(10.0, -10.0, 0.0), Vec3::Y), &m).unwrap();
        assert!((hit - 8.0).abs() < 1e-4, "front face at y=-2: {hit}");
        assert!(
            ray_obb(&ray(Vec3::new(10.0, 3.0, 0.0), Vec3::X), &m).is_none(),
            "beyond the 2 m half-length"
        );
        assert!(
            ray_obb(&ray(Vec3::new(6.0, 0.0, 0.0), Vec3::X), &m).is_some(),
            "short side along X is only 1 m wide, hit at x=9.5"
        );
    }

    #[test]
    fn starting_inside_returns_the_exit_and_degenerate_models_miss() {
        let t = ray_obb(&ray(Vec3::ZERO, Vec3::X), &Mat4::IDENTITY).unwrap();
        assert!((t - 0.5).abs() < 1e-5);
        assert!(
            ray_obb(&ray(Vec3::new(-5.0, 0.0, 0.0), Vec3::X), &Mat4::from_scale(Vec3::new(1.0, 0.0, 1.0)))
                .is_none()
        );
    }

    #[test]
    fn closest_point_on_an_axis_line() {
        // a ray straight down at x=3 touches the X axis at 3
        let t = closest_on_line(&ray(Vec3::new(3.0, 0.0, 10.0), -Vec3::Z), Vec3::ZERO, Vec3::X).unwrap();
        assert!((t - 3.0).abs() < 1e-5);
        assert!(
            closest_on_line(&ray(Vec3::new(0.0, 1.0, 1.0), Vec3::X), Vec3::ZERO, Vec3::X).is_none(),
            "parallel"
        );
    }

    #[test]
    fn segment_distance_clamps_to_the_ends() {
        let (d, t) = ray_segment_distance(
            &ray(Vec3::new(1.0, 0.5, 5.0), -Vec3::Z),
            Vec3::ZERO,
            Vec3::new(2.0, 0.0, 0.0),
        );
        assert!((d - 0.5).abs() < 1e-5 && (t - 0.5).abs() < 1e-5);
        let (d2, t2) = ray_segment_distance(
            &ray(Vec3::new(9.0, 0.0, 5.0), -Vec3::Z),
            Vec3::ZERO,
            Vec3::new(2.0, 0.0, 0.0),
        );
        assert_eq!(t2, 1.0);
        assert!((d2 - 7.0).abs() < 1e-5);
    }

    #[test]
    fn plane_intersection() {
        let t = ray_plane(&ray(Vec3::new(0.0, 0.0, 10.0), -Vec3::Z), Vec3::ZERO, Vec3::Z).unwrap();
        assert!((t - 10.0).abs() < 1e-5);
        assert!(
            ray_plane(&ray(Vec3::new(0.0, 0.0, 10.0), Vec3::X), Vec3::ZERO, Vec3::Z).is_none(),
            "parallel"
        );
        assert!(
            ray_plane(&ray(Vec3::new(0.0, 0.0, 10.0), Vec3::Z), Vec3::ZERO, Vec3::Z).is_none(),
            "plane is behind"
        );
    }
}
