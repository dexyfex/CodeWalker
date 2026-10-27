//! Bounds, frustum culling, rays, the reversed-Z projection and LOD selection. Pure maths, no GPU.
//!
//! Everything the GPU sees is **camera-relative**: world positions are `f64` ([`DVec3`]) and the camera
//! position is subtracted on the CPU before anything is converted to `f32`, so a model 8 km from the
//! origin has the same precision as one at the origin.

use glam::{DVec3, Mat4, Vec3, Vec4, Vec4Swizzles};

/// Axis-aligned box. An empty box has `min > max` and contains nothing.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Aabb {
    pub min: Vec3,
    pub max: Vec3,
}

impl Aabb {
    pub const EMPTY: Self = Self { min: Vec3::splat(f32::INFINITY), max: Vec3::splat(f32::NEG_INFINITY) };

    pub fn new(min: Vec3, max: Vec3) -> Self {
        Self { min, max }
    }

    pub fn from_points(points: impl IntoIterator<Item = Vec3>) -> Self {
        points.into_iter().fold(Self::EMPTY, |b, p| b.including(p))
    }

    pub fn is_empty(&self) -> bool {
        !(self.min.cmple(self.max).all())
    }

    pub fn including(self, p: Vec3) -> Self {
        Self { min: self.min.min(p), max: self.max.max(p) }
    }

    pub fn union(self, o: Self) -> Self {
        Self { min: self.min.min(o.min), max: self.max.max(o.max) }
    }

    pub fn center(&self) -> Vec3 {
        (self.min + self.max) * 0.5
    }

    pub fn half_extents(&self) -> Vec3 {
        (self.max - self.min) * 0.5
    }

    /// Radius of the sphere around [`Self::center`] that holds the box.
    pub fn radius(&self) -> f32 {
        if self.is_empty() { 0.0 } else { self.half_extents().length() }
    }

    pub fn corners(&self) -> [Vec3; 8] {
        let (a, b) = (self.min, self.max);
        [0, 1, 2, 3, 4, 5, 6, 7].map(|i| {
            Vec3::new(
                if i & 1 == 0 { a.x } else { b.x },
                if i & 2 == 0 { a.y } else { b.y },
                if i & 4 == 0 { a.z } else { b.z },
            )
        })
    }

    /// The box around this box after an affine transform (Arvo's method: exact for the corners).
    pub fn transformed(&self, m: &Mat4) -> Self {
        if self.is_empty() {
            return *self;
        }
        let (c, e) = (self.center(), self.half_extents());
        let center = m.transform_point3(c);
        let abs = |v: Vec4| v.xyz().abs();
        let extent = abs(m.x_axis) * e.x + abs(m.y_axis) * e.y + abs(m.z_axis) * e.z;
        Self { min: center - extent, max: center + extent }
    }

    pub fn contains(&self, p: Vec3) -> bool {
        p.cmpge(self.min).all() && p.cmple(self.max).all()
    }
}

/// `normal · p + d >= 0` on the inside.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Plane {
    pub normal: Vec3,
    pub d: f32,
}

impl Plane {
    fn from_row(v: Vec4) -> Self {
        let len = v.xyz().length();
        // a degenerate row (the far plane of an infinite projection) keeps everything
        if len < 1e-12 {
            Self { normal: Vec3::ZERO, d: 1.0 }
        } else {
            Self { normal: v.xyz() / len, d: v.w / len }
        }
    }

    pub fn distance(&self, p: Vec3) -> f32 {
        self.normal.dot(p) + self.d
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Containment {
    Outside,
    Intersects,
    Inside,
}

/// Six planes of a view volume, in whatever space the matrix maps from (camera-relative for
/// [`crate::render::Camera::frustum`]).
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Frustum {
    pub planes: [Plane; 6],
}

impl Frustum {
    /// Gribb-Hartmann extraction from a clip matrix with wgpu's depth range (0..w). Works for both
    /// depth directions: reversed Z only swaps which of the two depth planes is "near".
    pub fn from_matrix(m: &Mat4) -> Self {
        let (r0, r1, r2, r3) = (m.row(0), m.row(1), m.row(2), m.row(3));
        Self { planes: [r3 + r0, r3 - r0, r3 + r1, r3 - r1, r2, r3 - r2].map(Plane::from_row) }
    }

    pub fn contains_point(&self, p: Vec3) -> bool {
        self.planes.iter().all(|pl| pl.distance(p) >= 0.0)
    }

    /// True unless the sphere is entirely outside one plane (may keep a few spheres near corners).
    pub fn intersects_sphere(&self, center: Vec3, radius: f32) -> bool {
        self.planes.iter().all(|pl| pl.distance(center) >= -radius)
    }

    pub fn classify_aabb(&self, b: &Aabb) -> Containment {
        let mut inside = true;
        for pl in &self.planes {
            // the corner furthest along the normal, and the one furthest against it
            let pos = Vec3::select(pl.normal.cmpge(Vec3::ZERO), b.max, b.min);
            let neg = Vec3::select(pl.normal.cmpge(Vec3::ZERO), b.min, b.max);
            if pl.distance(pos) < 0.0 {
                return Containment::Outside;
            }
            if pl.distance(neg) < 0.0 {
                inside = false;
            }
        }
        if inside { Containment::Inside } else { Containment::Intersects }
    }

    pub fn intersects_aabb(&self, b: &Aabb) -> bool {
        self.classify_aabb(b) != Containment::Outside
    }
}

/// Right-handed perspective with **reversed Z**: the near plane maps to depth 1 and the far plane to 0.
/// With a `Depth32Float` buffer this spreads float precision evenly over distance, so a 0.1 m near
/// plane and a 20 km far plane do not z-fight. Depth tests use `GreaterEqual` and clear to 0.
pub fn perspective_reversed_z(fov_y: f32, aspect: f32, near: f32, far: f32) -> Mat4 {
    let near = near.max(1e-4);
    let far = far.max(near * 2.0);
    // glam maps its `z_near` argument to 0 and `z_far` to 1: swapping them reverses the range
    Mat4::perspective_rh(fov_y.clamp(1e-3, 3.1), aspect.max(1e-4), far, near)
}

/// A ray in world space with an `f64` origin (picking at 8 km must not lose centimetres).
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct WorldRay {
    pub origin: DVec3,
    /// Unit length.
    pub dir: Vec3,
}

impl WorldRay {
    pub fn at(&self, t: f64) -> DVec3 {
        self.origin + self.dir.as_dvec3() * t
    }

    /// Distance along the ray to an axis-aligned box given relative to `base` (for example a
    /// mesh's bounds around an instance position), or `None` when it misses or lies behind.
    pub fn hit_aabb(&self, base: DVec3, b: &Aabb) -> Option<f32> {
        ray_aabb((self.origin - base).as_vec3(), self.dir, b)
    }
}

/// Slab test; `t` of the entry point, or of the exit point when the origin is inside.
pub fn ray_aabb(origin: Vec3, dir: Vec3, b: &Aabb) -> Option<f32> {
    if b.is_empty() {
        return None;
    }
    let (mut t0, mut t1) = (f32::NEG_INFINITY, f32::INFINITY);
    for i in 0..3 {
        if dir[i].abs() < 1e-12 {
            if origin[i] < b.min[i] || origin[i] > b.max[i] {
                return None;
            }
        } else {
            let inv = 1.0 / dir[i];
            let (a, c) = ((b.min[i] - origin[i]) * inv, (b.max[i] - origin[i]) * inv);
            t0 = t0.max(a.min(c));
            t1 = t1.min(a.max(c));
        }
    }
    if t1 < t0.max(0.0) {
        return None;
    }
    Some(if t0 >= 0.0 { t0 } else { t1 })
}

/// Distance along a unit ray to a sphere, `None` on a miss or when it lies behind.
pub fn ray_sphere(origin: Vec3, dir: Vec3, center: Vec3, radius: f32) -> Option<f32> {
    let oc = origin - center;
    let b = oc.dot(dir);
    let c = oc.length_squared() - radius * radius;
    let disc = b * b - c;
    if disc < 0.0 {
        return None;
    }
    let s = disc.sqrt();
    let (near, far) = (-b - s, -b + s);
    if far < 0.0 { None } else { Some(if near >= 0.0 { near } else { far }) }
}

/// Picks one of up to four detail levels (high, medium, low, very low) by distance, the way a GTA
/// drawable's `LodDistHigh/Med/Low/Vlow` work: level `i` is drawn while `distance <= switch[i]`.
/// `available[i]` is false for a level the model does not have; it is skipped (the next coarser
/// level that exists is used). Beyond the last available level nothing is drawn (`None`).
///
/// `hysteresis` (metres) stops flicker when the camera sits on a switch distance: with `previous`
/// set, moving to a coarser level needs `distance > switch + hysteresis` and moving back to a finer
/// one needs `distance < switch - hysteresis`.
pub fn select_lod(
    switch: [f32; 4],
    available: [bool; 4],
    distance: f32,
    previous: Option<usize>,
    hysteresis: f32,
) -> Option<usize> {
    let pick = |d: f32| (0..4).find(|&i| available[i] && d <= switch[i]);
    let plain = pick(distance);
    let Some(prev) = previous.filter(|&p| p < 4 && available[p]) else { return plain };
    let h = hysteresis.max(0.0);
    match plain {
        // getting coarser (or disappearing): only once clearly past the previous level's distance
        Some(i) if i > prev => {
            if distance > switch[prev] + h {
                pick(distance)
            } else {
                Some(prev)
            }
        }
        None => (distance <= switch[prev] + h).then_some(prev),
        // getting finer: only once clearly inside the finer level's distance
        Some(i) if i < prev => {
            if distance < switch[i] - h {
                Some(i)
            } else {
                (i + 1..=prev).find(|&j| available[j] && distance < switch[j] - h).or(Some(prev))
            }
        }
        same => same,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn aabb_basics_and_transform() {
        let b = Aabb::from_points([Vec3::new(1.0, 2.0, 3.0), Vec3::new(-1.0, 0.0, 5.0)]);
        assert_eq!(b, Aabb::new(Vec3::new(-1.0, 0.0, 3.0), Vec3::new(1.0, 2.0, 5.0)));
        assert!(Aabb::EMPTY.is_empty() && !b.is_empty());
        assert_eq!(Aabb::EMPTY.radius(), 0.0);
        assert!(b.contains(Vec3::new(0.0, 1.0, 4.0)) && !b.contains(Vec3::ZERO));
        let rot = Mat4::from_rotation_z(std::f32::consts::FRAC_PI_2) * Mat4::from_translation(Vec3::X * 10.0);
        let t = b.transformed(&rot);
        for c in b.corners() {
            let p = rot.transform_point3(c);
            assert!(t.min.cmple(p + 1e-4).all() && t.max.cmpge(p - 1e-4).all(), "{p:?} outside {t:?}");
        }
        assert!((t.half_extents() - Vec3::new(1.0, 1.0, 1.0)).length() < 1e-4, "rotation swaps x and y");
    }

    fn view_frustum() -> (Mat4, Frustum) {
        // camera at the origin looking down +X, Z up
        let view = Mat4::look_to_rh(Vec3::ZERO, Vec3::X, Vec3::Z);
        let vp = perspective_reversed_z(60f32.to_radians(), 1.0, 0.1, 1000.0) * view;
        (vp, Frustum::from_matrix(&vp))
    }

    #[test]
    fn reversed_z_maps_near_to_one_and_far_to_zero_monotonically() {
        let (vp, _) = view_frustum();
        let depth = |d: f32| {
            let c = vp * Vec4::new(d, 0.0, 0.0, 1.0);
            c.z / c.w
        };
        assert!((depth(0.1) - 1.0).abs() < 1e-5, "{}", depth(0.1));
        assert!(depth(1000.0).abs() < 1e-5, "{}", depth(1000.0));
        let mut last = 2.0;
        for d in [0.2, 1.0, 10.0, 100.0, 5000.0 / 5.0] {
            assert!(depth(d) < last, "depth decreases with distance");
            last = depth(d);
        }
        // the point of reversed Z: two surfaces 1 cm apart at 15 km still get different f32 depths
        let wide = perspective_reversed_z(1.0, 1.0, 0.1, 20000.0);
        let far = |d: f32| {
            let c = wide * Vec4::new(0.0, 0.0, -d, 1.0);
            c.z / c.w
        };
        assert!(far(15000.0) > far(15000.01));
    }

    #[test]
    fn frustum_culls_spheres_and_boxes() {
        let (_, f) = view_frustum();
        assert!(f.contains_point(Vec3::new(10.0, 0.0, 0.0)));
        assert!(!f.contains_point(Vec3::new(-10.0, 0.0, 0.0)), "behind");
        assert!(!f.contains_point(Vec3::new(2000.0, 0.0, 0.0)), "beyond the far plane");
        assert!(f.intersects_sphere(Vec3::new(10.0, 0.0, 0.0), 1.0));
        assert!(!f.intersects_sphere(Vec3::new(10.0, 50.0, 0.0), 1.0), "far to the side");
        assert!(f.intersects_sphere(Vec3::new(10.0, 6.0, 0.0), 1.0), "straddles the left plane");
        let inside = Aabb::new(Vec3::new(9.0, -1.0, -1.0), Vec3::new(11.0, 1.0, 1.0));
        assert_eq!(f.classify_aabb(&inside), Containment::Inside);
        let straddle = Aabb::new(Vec3::new(9.0, -1.0, -1.0), Vec3::new(11.0, 20.0, 1.0));
        assert_eq!(f.classify_aabb(&straddle), Containment::Intersects);
        let behind = Aabb::new(Vec3::new(-11.0, -1.0, -1.0), Vec3::new(-9.0, 1.0, 1.0));
        assert_eq!(f.classify_aabb(&behind), Containment::Outside);
        assert_eq!(f.classify_aabb(&Aabb::new(Vec3::splat(-1.0), Vec3::splat(1.0))), Containment::Intersects);
    }

    #[test]
    fn rays_hit_boxes_and_spheres() {
        let b = Aabb::new(Vec3::splat(-1.0), Vec3::splat(1.0));
        assert_eq!(ray_aabb(Vec3::new(-5.0, 0.0, 0.0), Vec3::X, &b), Some(4.0));
        assert_eq!(ray_aabb(Vec3::ZERO, Vec3::X, &b), Some(1.0), "from inside: the exit");
        assert_eq!(ray_aabb(Vec3::new(-5.0, 3.0, 0.0), Vec3::X, &b), None);
        assert_eq!(ray_aabb(Vec3::new(5.0, 0.0, 0.0), Vec3::X, &b), None, "behind");
        assert_eq!(ray_aabb(Vec3::new(0.0, 0.0, 5.0), Vec3::X, &b), None, "parallel, outside the slab");
        assert_eq!(ray_aabb(Vec3::ZERO, Vec3::X, &Aabb::EMPTY), None);
        assert_eq!(ray_sphere(Vec3::new(-5.0, 0.0, 0.0), Vec3::X, Vec3::ZERO, 1.0), Some(4.0));
        assert_eq!(ray_sphere(Vec3::ZERO, Vec3::X, Vec3::ZERO, 2.0), Some(2.0));
        assert_eq!(ray_sphere(Vec3::new(-5.0, 2.0, 0.0), Vec3::X, Vec3::ZERO, 1.0), None);

        // an f64 ray at 7 km keeps sub-millimetre precision once made relative to the target
        let r = WorldRay { origin: DVec3::new(7000.0, -3000.25, 10.0), dir: Vec3::NEG_Z };
        let t = r.hit_aabb(
            DVec3::new(7000.0005, -3000.25, 0.0),
            &Aabb::new(Vec3::splat(-0.001), Vec3::splat(0.001)),
        );
        assert!((t.unwrap() - 9.999).abs() < 1e-4);
        assert!((r.at(10.0) - DVec3::new(7000.0, -3000.25, 0.0)).length() < 1e-9);
    }

    #[test]
    fn lod_follows_the_switch_distances() {
        let sw = [50.0, 150.0, 400.0, 1200.0];
        let all = [true; 4];
        assert_eq!(select_lod(sw, all, 10.0, None, 0.0), Some(0));
        assert_eq!(select_lod(sw, all, 50.0, None, 0.0), Some(0), "inclusive");
        assert_eq!(select_lod(sw, all, 51.0, None, 0.0), Some(1));
        assert_eq!(select_lod(sw, all, 1000.0, None, 0.0), Some(3));
        assert_eq!(select_lod(sw, all, 1300.0, None, 0.0), None, "beyond the last level");
        // missing medium level: the low one covers its range
        assert_eq!(select_lod(sw, [true, false, true, true], 100.0, None, 0.0), Some(2));
        assert_eq!(select_lod(sw, [false; 4], 1.0, None, 0.0), None);
    }

    #[test]
    fn lod_hysteresis_holds_the_level_near_a_switch() {
        let sw = [50.0, 150.0, 400.0, 1200.0];
        let all = [true; 4];
        // moving out: stays high until 55
        assert_eq!(select_lod(sw, all, 53.0, Some(0), 5.0), Some(0));
        assert_eq!(select_lod(sw, all, 56.0, Some(0), 5.0), Some(1));
        // moving back in: stays medium until below 45
        assert_eq!(select_lod(sw, all, 47.0, Some(1), 5.0), Some(1));
        assert_eq!(select_lod(sw, all, 44.0, Some(1), 5.0), Some(0));
        // a big jump still lands on the right level
        assert_eq!(select_lod(sw, all, 500.0, Some(0), 5.0), Some(3));
        assert_eq!(select_lod(sw, all, 10.0, Some(3), 5.0), Some(0));
        // fading out at the end
        assert_eq!(select_lod(sw, all, 1203.0, Some(3), 5.0), Some(3));
        assert_eq!(select_lod(sw, all, 1206.0, Some(3), 5.0), None);
        // just inside low's range is not yet "clearly" inside: very low stays
        assert_eq!(select_lod(sw, all, 397.0, Some(3), 5.0), Some(3));
        assert_eq!(select_lod(sw, all, 394.0, Some(3), 5.0), Some(2));
        // from very low straight to 48 m: high is not clearly reached, medium is
        assert_eq!(select_lod(sw, all, 48.0, Some(3), 5.0), Some(1));
        // an unavailable previous level is ignored
        assert_eq!(select_lod(sw, [false, true, true, true], 10.0, Some(0), 5.0), Some(1));
    }
}
