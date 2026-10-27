//! Cameras in GTA's coordinate system: right-handed, **Z up**, yaw about Z measured from +X, pitch up
//! from the horizon. Pure data and maths, no GPU.
//!
//! A [`Camera`] is what a frame is rendered from; [`OrbitCamera`] (model viewer) and [`FlyCamera`]
//! (world) are the controllers that produce one. Positions are `f64` because the world is +-8 km wide;
//! every matrix built here is **camera-relative** (the eye sits at the origin) so that `f32` stays
//! precise whatever the camera position is.

use glam::{DVec3, Mat4, Vec3, Vec4};

use super::math::{Aabb, Frustum, WorldRay, perspective_reversed_z};

const MAX_PITCH: f32 = 89.0 * std::f32::consts::PI / 180.0;

/// Where a frame is rendered from.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Camera {
    pub position: DVec3,
    pub yaw: f32,
    pub pitch: f32,
    pub fov_y: f32,
    pub near: f32,
    pub far: f32,
}

impl Default for Camera {
    fn default() -> Self {
        Self {
            position: DVec3::new(0.0, -10.0, 4.0),
            yaw: std::f32::consts::FRAC_PI_2,
            pitch: -0.3,
            fov_y: 60f32.to_radians(),
            near: 0.1,
            far: 20_000.0,
        }
    }
}

impl Camera {
    pub fn forward(&self) -> Vec3 {
        let (sy, cy) = self.yaw.sin_cos();
        let (sp, cp) = self.pitch.clamp(-MAX_PITCH, MAX_PITCH).sin_cos();
        Vec3::new(cp * cy, cp * sy, sp)
    }

    pub fn right(&self) -> Vec3 {
        // forward is never parallel to Z because pitch is clamped below 90 degrees
        self.forward().cross(Vec3::Z).normalize()
    }

    pub fn up(&self) -> Vec3 {
        self.right().cross(self.forward())
    }

    /// Rotation-only view matrix: the eye is the origin. Combine with world positions that were made
    /// relative to [`Self::position`] on the CPU.
    pub fn view_rotation(&self) -> Mat4 {
        Mat4::look_to_rh(Vec3::ZERO, self.forward(), Vec3::Z)
    }

    /// Reversed-Z projection (near maps to depth 1, far to 0).
    pub fn projection(&self, aspect: f32) -> Mat4 {
        perspective_reversed_z(self.fov_y, aspect, self.near, self.far)
    }

    /// Camera-relative view-projection matrix.
    pub fn view_proj(&self, aspect: f32) -> Mat4 {
        self.projection(aspect) * self.view_rotation()
    }

    /// View volume planes in camera-relative space.
    pub fn frustum(&self, aspect: f32) -> Frustum {
        Frustum::from_matrix(&self.view_proj(aspect))
    }

    /// A world position expressed relative to the eye, converted to `f32` after the subtraction.
    pub fn relative(&self, world: DVec3) -> Vec3 {
        (world - self.position).as_vec3()
    }

    pub fn distance_to(&self, world: DVec3) -> f32 {
        (world - self.position).length() as f32
    }

    /// World-space ray through the pixel `(px, py)` of a `viewport` (width, height) in pixels, with
    /// the origin at the top left and y down, like mouse coordinates.
    pub fn ray(&self, px: f32, py: f32, viewport: (f32, f32)) -> WorldRay {
        let aspect = viewport.0 / viewport.1.max(1.0);
        let ndc = Vec3::new(px / viewport.0.max(1.0) * 2.0 - 1.0, 1.0 - py / viewport.1.max(1.0) * 2.0, 0.0);
        let inv = self.view_proj(aspect).inverse();
        // near plane is depth 1 under reversed Z, the far plane 0: unproject both and take the difference
        let unproject = |z: f32| {
            let p = inv * Vec4::new(ndc.x, ndc.y, z, 1.0);
            p.truncate() / p.w
        };
        let dir = (unproject(0.0) - unproject(1.0)).normalize();
        WorldRay { origin: self.position, dir }
    }

    /// Pixel position (y down) and depth along the view axis of a world point, or `None` when it is
    /// behind the camera. Used for labels, gizmo hit tests and box selection.
    pub fn project(&self, world: DVec3, viewport: (f32, f32)) -> Option<(f32, f32, f32)> {
        let aspect = viewport.0 / viewport.1.max(1.0);
        let rel = self.relative(world);
        let clip = self.view_proj(aspect) * rel.extend(1.0);
        if clip.w <= 1e-6 {
            return None;
        }
        let (nx, ny) = (clip.x / clip.w, clip.y / clip.w);
        Some(((nx * 0.5 + 0.5) * viewport.0, (0.5 - ny * 0.5) * viewport.1, clip.w))
    }

    /// The view-projection matrix restricted to the pixel rectangle `[x0, x1) x [y0, y1)` of the
    /// viewport, so that the rectangle fills the whole render target. Rendering with it into a target
    /// the size of the rectangle draws only what is under those pixels (the id-buffer pick pass).
    pub fn view_proj_region(&self, viewport: (f32, f32), rect: (f32, f32, f32, f32)) -> Mat4 {
        let aspect = viewport.0 / viewport.1.max(1.0);
        let (w, h) = (viewport.0.max(1.0), viewport.1.max(1.0));
        let (nx0, nx1) = (rect.0 / w * 2.0 - 1.0, rect.2 / w * 2.0 - 1.0);
        // pixel rows grow downwards, NDC y grows upwards
        let (ny0, ny1) = (1.0 - rect.3 / h * 2.0, 1.0 - rect.1 / h * 2.0);
        let (dx, dy) = ((nx1 - nx0).max(1e-6), (ny1 - ny0).max(1e-6));
        let (sx, sy) = (2.0 / dx, 2.0 / dy);
        let (tx, ty) = (-(nx1 + nx0) / dx, -(ny1 + ny0) / dy);
        let crop = Mat4::from_cols(
            Vec4::new(sx, 0.0, 0.0, 0.0),
            Vec4::new(0.0, sy, 0.0, 0.0),
            Vec4::new(0.0, 0.0, 1.0, 0.0),
            Vec4::new(tx, ty, 0.0, 1.0),
        );
        crop * self.view_proj(aspect)
    }
}

/// Orbit / pan / zoom around a target, for looking at one model.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct OrbitCamera {
    pub target: DVec3,
    pub distance: f32,
    pub yaw: f32,
    pub pitch: f32,
    pub fov_y: f32,
    pub min_distance: f32,
    pub max_distance: f32,
}

impl Default for OrbitCamera {
    fn default() -> Self {
        Self {
            target: DVec3::ZERO,
            distance: 6.0,
            // three-quarter view from the front-left, slightly above
            yaw: std::f32::consts::FRAC_PI_2 + 0.6,
            pitch: -0.35,
            fov_y: 45f32.to_radians(),
            min_distance: 0.05,
            max_distance: 5000.0,
        }
    }
}

impl OrbitCamera {
    /// Radians of rotation per pixel of mouse movement.
    pub const SENSITIVITY: f32 = 0.005;

    fn direction(&self) -> Vec3 {
        Camera { yaw: self.yaw, pitch: self.pitch, ..Camera::default() }.forward()
    }

    /// The camera that looks at the target from `distance`. The near plane follows the distance so a
    /// 30 cm prop and a 40 m building both fill the depth range well.
    pub fn camera(&self) -> Camera {
        let dir = self.direction();
        Camera {
            position: self.target - dir.as_dvec3() * self.distance as f64,
            yaw: self.yaw,
            pitch: self.pitch.clamp(-MAX_PITCH, MAX_PITCH),
            fov_y: self.fov_y,
            near: (self.distance * 0.01).clamp(0.01, 1.0),
            far: 20_000.0,
        }
    }

    /// Turns around the target by a mouse movement in pixels (dragging right moves the model with it).
    pub fn orbit(&mut self, dx: f32, dy: f32) {
        self.yaw = (self.yaw - dx * Self::SENSITIVITY).rem_euclid(std::f32::consts::TAU);
        self.pitch = (self.pitch - dy * Self::SENSITIVITY).clamp(-MAX_PITCH, MAX_PITCH);
    }

    /// Slides the target in the view plane so the model follows the cursor: `dx`, `dy` are pixels,
    /// `viewport_height` the height of the view in pixels.
    pub fn pan(&mut self, dx: f32, dy: f32, viewport_height: f32) {
        let per_pixel = 2.0 * self.distance * (self.fov_y * 0.5).tan() / viewport_height.max(1.0);
        let cam = self.camera();
        let shift = (-cam.right() * dx + cam.up() * dy) * per_pixel;
        self.target += shift.as_dvec3();
    }

    /// Scroll steps: positive moves closer, each step is 10 percent of the distance.
    pub fn zoom(&mut self, steps: f32) {
        self.distance = (self.distance * 0.9f32.powf(steps)).clamp(self.min_distance, self.max_distance);
    }

    /// Centres on `bounds` and backs off until the whole box fits a viewport of `aspect`.
    pub fn frame(&mut self, bounds: &Aabb, aspect: f32) {
        if bounds.is_empty() {
            return;
        }
        let radius = bounds.radius().max(0.05);
        let half_y = (self.fov_y * 0.5).tan();
        let half = half_y.min(half_y * aspect.max(0.1));
        self.target = bounds.center().as_dvec3();
        // a sphere of `radius` fits when its tangent cone is inside the narrower half angle
        let distance = radius / half.atan().sin() * 1.05;
        self.distance = distance.clamp(self.min_distance, self.max_distance);
    }
}

/// Keys and wheel for [`FlyCamera::update`]: each axis is -1, 0 or 1.
#[derive(Debug, Clone, Copy, Default, PartialEq)]
pub struct FlyInput {
    pub forward: f32,
    pub right: f32,
    pub up: f32,
    pub boost: bool,
}

/// Free flight (WASD + mouse look) for the world.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct FlyCamera {
    pub position: DVec3,
    pub yaw: f32,
    pub pitch: f32,
    pub fov_y: f32,
    /// Metres per second at normal speed.
    pub speed: f32,
    /// Speed multiplier while boosting (shift).
    pub boost: f32,
}

impl Default for FlyCamera {
    fn default() -> Self {
        Self {
            position: DVec3::new(0.0, -20.0, 10.0),
            yaw: std::f32::consts::FRAC_PI_2,
            pitch: -0.2,
            fov_y: 60f32.to_radians(),
            speed: 20.0,
            boost: 8.0,
        }
    }
}

impl FlyCamera {
    pub const MIN_SPEED: f32 = 0.5;
    pub const MAX_SPEED: f32 = 3000.0;
    pub const SENSITIVITY: f32 = 0.003;

    pub fn camera(&self) -> Camera {
        Camera {
            position: self.position,
            yaw: self.yaw,
            pitch: self.pitch.clamp(-MAX_PITCH, MAX_PITCH),
            fov_y: self.fov_y,
            ..Camera::default()
        }
    }

    /// Turns the view by a mouse movement in pixels.
    pub fn look(&mut self, dx: f32, dy: f32) {
        self.yaw = (self.yaw - dx * Self::SENSITIVITY).rem_euclid(std::f32::consts::TAU);
        self.pitch = (self.pitch - dy * Self::SENSITIVITY).clamp(-MAX_PITCH, MAX_PITCH);
    }

    /// Moves along the view axes (forward follows the look direction, up is world +Z) for `dt` seconds.
    pub fn update(&mut self, dt: f32, input: FlyInput) {
        let cam = self.camera();
        let wish = cam.forward() * input.forward + cam.right() * input.right + Vec3::Z * input.up;
        if wish.length_squared() < 1e-9 {
            return;
        }
        let speed = self.speed * if input.boost { self.boost } else { 1.0 };
        self.position += (wish.normalize() * speed * dt.clamp(0.0, 0.25)).as_dvec3();
    }

    /// Scroll steps scale the speed by 20 percent each.
    pub fn adjust_speed(&mut self, steps: f32) {
        self.speed = (self.speed * 1.2f32.powf(steps)).clamp(Self::MIN_SPEED, Self::MAX_SPEED);
    }

    /// Turns to face `target` without moving.
    pub fn look_at(&mut self, target: DVec3) {
        let d = (target - self.position).as_vec3();
        if d.length_squared() < 1e-12 {
            return;
        }
        let d = d.normalize();
        self.yaw = d.y.atan2(d.x).rem_euclid(std::f32::consts::TAU);
        self.pitch = d.z.clamp(-1.0, 1.0).asin().clamp(-MAX_PITCH, MAX_PITCH);
    }

    /// Moves back from `bounds` so all of it is in view, looking at its centre.
    pub fn frame(&mut self, bounds: &Aabb) {
        if bounds.is_empty() {
            return;
        }
        let radius = bounds.radius().max(1.0);
        let center = bounds.center().as_dvec3();
        let distance = radius / (self.fov_y * 0.5).tan() * 1.1;
        let dir = self.camera().forward();
        self.position = center - dir.as_dvec3() * distance as f64;
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn close(a: Vec3, b: Vec3) -> bool {
        (a - b).length() < 1e-3
    }

    #[test]
    fn axes_follow_the_gta_convention() {
        let mut c = Camera { yaw: 0.0, pitch: 0.0, ..Default::default() };
        assert!(close(c.forward(), Vec3::X), "yaw 0 looks along +X");
        assert!(close(c.right(), -Vec3::Y), "right of +X in a Z-up right-handed world is -Y");
        assert!(close(c.up(), Vec3::Z));
        c.yaw = std::f32::consts::FRAC_PI_2;
        assert!(close(c.forward(), Vec3::Y));
        c.pitch = 1.0;
        assert!(c.forward().z > 0.8, "positive pitch looks up");
        assert!(c.up().dot(c.forward()).abs() < 1e-4, "up is perpendicular to the view direction");
    }

    #[test]
    fn center_ray_matches_the_view_direction_and_starts_at_the_eye() {
        let c = Camera {
            position: DVec3::new(7000.25, -3000.5, 20.0),
            yaw: 0.7,
            pitch: -0.4,
            ..Default::default()
        };
        let r = c.ray(160.0, 90.0, (320.0, 180.0));
        assert!(close(r.dir, c.forward()), "{:?} vs {:?}", r.dir, c.forward());
        assert_eq!(r.origin, c.position, "the f64 origin is kept exactly");
    }

    #[test]
    fn rays_through_screen_edges_diverge_by_the_field_of_view() {
        let c = Camera { yaw: 0.0, pitch: 0.0, ..Default::default() };
        let left = c.ray(0.0, 50.0, (100.0, 100.0)).dir;
        let right = c.ray(100.0, 50.0, (100.0, 100.0)).dir;
        assert!(left.y > 0.0 && right.y < 0.0, "left of the view is +Y when looking down +X");
        assert!(
            (left.angle_between(right) - c.fov_y).abs() < 1e-2,
            "square viewport: horizontal fov = vertical"
        );
        assert!(c.ray(50.0, 0.0, (100.0, 100.0)).dir.z > 0.0, "the top of the screen looks up");
    }

    #[test]
    fn project_and_ray_are_inverses() {
        let c = Camera {
            position: DVec3::new(-5000.0, 6200.0, 80.0),
            yaw: 2.0,
            pitch: -0.2,
            ..Default::default()
        };
        let vp = (1280.0, 720.0);
        let target = c.position + c.ray(900.0, 200.0, vp).dir.as_dvec3() * 123.0;
        let (x, y, depth) = c.project(target, vp).expect("in front");
        assert!((x - 900.0).abs() < 0.05 && (y - 200.0).abs() < 0.05, "{x} {y}");
        assert!(depth > 0.0);
        assert!(c.project(c.position - c.forward().as_dvec3() * 5.0, vp).is_none(), "behind the eye");
    }

    #[test]
    fn region_matrix_crops_to_the_pixel_rectangle() {
        let c = Camera { yaw: 0.0, pitch: 0.0, position: DVec3::ZERO, ..Default::default() };
        let vp = (200.0, 100.0);
        // the ray through pixel (150, 30) must land exactly in the middle of a 1-pixel region around it
        let ray = c.ray(150.5, 30.5, vp);
        let m = c.view_proj_region(vp, (150.0, 30.0, 151.0, 31.0));
        let clip = m * (ray.dir * 40.0).extend(1.0);
        let (x, y) = (clip.x / clip.w, clip.y / clip.w);
        assert!(x.abs() < 1e-3 && y.abs() < 1e-3, "centre of the cropped view: {x} {y}");
        // a point one pixel to the right leaves the cropped view
        let other = c.ray(152.5, 30.5, vp);
        let clip = m * (other.dir * 40.0).extend(1.0);
        assert!((clip.x / clip.w).abs() > 1.0);
    }

    #[test]
    fn frustum_is_camera_relative_and_culls_what_is_behind() {
        let c =
            Camera { position: DVec3::new(7000.0, 7000.0, 0.0), yaw: 0.0, pitch: 0.0, ..Default::default() };
        let f = c.frustum(1.0);
        assert!(f.contains_point(c.relative(DVec3::new(7010.0, 7000.0, 0.0))));
        assert!(!f.contains_point(c.relative(DVec3::new(6990.0, 7000.0, 0.0))));
        assert!(!f.contains_point(c.relative(DVec3::new(7000.0 + 30_000.0, 7000.0, 0.0))), "beyond far");
    }

    #[test]
    fn orbit_keeps_the_target_in_the_middle_of_the_view() {
        let mut o = OrbitCamera { target: DVec3::new(100.0, 200.0, 30.0), ..Default::default() };
        o.orbit(120.0, -40.0);
        let c = o.camera();
        assert!(((c.position - o.target).length() as f32 - o.distance).abs() < 1e-3);
        let r = c.ray(50.0, 50.0, (100.0, 100.0));
        let to_target = (o.target - c.position).as_vec3().normalize();
        assert!(close(r.dir, to_target), "{:?} vs {:?}", r.dir, to_target);
        o.orbit(0.0, 1.0e7);
        assert!(o.camera().right().is_finite(), "pitch stops short of straight down");
        o.orbit(0.0, -1.0e7);
        assert!(o.pitch <= MAX_PITCH + 1e-6);
    }

    #[test]
    fn dragging_right_turns_the_view_so_the_model_follows() {
        let mut o = OrbitCamera::default();
        let before = o.yaw;
        o.orbit(100.0, 0.0);
        assert!(o.yaw < before || o.yaw > std::f32::consts::PI, "yaw decreases (wrapping into 0..tau)");
    }

    #[test]
    fn pan_moves_the_target_with_the_cursor() {
        let mut o = OrbitCamera { distance: 10.0, ..Default::default() };
        let cam = o.camera();
        let start = o.target;
        o.pan(100.0, 0.0, 500.0);
        let moved = (o.target - start).as_vec3();
        // dragging right moves the model right on screen, so the target moves left in world terms
        assert!(moved.dot(cam.right()) < 0.0 && moved.dot(cam.forward()).abs() < 1e-4);
        let world_per_pixel = 2.0 * 10.0 * (o.fov_y * 0.5).tan() / 500.0;
        assert!((moved.length() - 100.0 * world_per_pixel).abs() < 1e-3);
    }

    #[test]
    fn zoom_is_clamped_and_multiplicative() {
        let mut o = OrbitCamera { distance: 10.0, ..Default::default() };
        o.zoom(1.0);
        assert!((o.distance - 9.0).abs() < 1e-4);
        o.zoom(-1.0);
        assert!((o.distance - 10.0).abs() < 1e-3);
        o.zoom(10_000.0);
        assert_eq!(o.distance, o.min_distance);
        o.zoom(-10_000.0);
        assert_eq!(o.distance, o.max_distance);
    }

    #[test]
    fn framing_a_box_keeps_every_corner_in_view_for_any_aspect() {
        for aspect in [0.5, 1.0, 16.0 / 9.0] {
            let mut o = OrbitCamera::default();
            let b = Aabb::new(Vec3::new(-3.0, -1.0, 0.0), Vec3::new(3.0, 1.0, 2.0));
            o.frame(&b, aspect);
            let cam = o.camera();
            let vp = cam.view_proj(aspect);
            for corner in b.corners() {
                let p = vp * cam.relative(corner.as_dvec3()).extend(1.0);
                let ndc = p.truncate() / p.w;
                assert!(
                    p.w > 0.0 && ndc.x.abs() <= 1.0 && ndc.y.abs() <= 1.0,
                    "aspect {aspect}: {corner:?} -> {ndc:?}"
                );
            }
            assert!(o.distance < b.radius() * 8.0, "does not back off absurdly far: {}", o.distance);
        }
    }

    #[test]
    fn fly_moves_in_view_space_at_the_set_speed() {
        let mut f = FlyCamera {
            position: DVec3::ZERO,
            yaw: 0.0,
            pitch: 0.0,
            speed: 10.0,
            boost: 4.0,
            ..Default::default()
        };
        f.update(0.1, FlyInput { forward: 1.0, ..Default::default() });
        assert!((f.position - DVec3::new(1.0, 0.0, 0.0)).length() < 1e-5);
        f.update(0.1, FlyInput { right: 1.0, up: 1.0, boost: true, ..Default::default() });
        let step = f.position - DVec3::new(1.0, 0.0, 0.0);
        assert!((step.length() - 4.0).abs() < 1e-4, "boosted diagonal keeps the speed: {step:?}");
        assert!(step.y < 0.0 && step.z > 0.0, "right is -Y, up is +Z");
        f.update(1.0, FlyInput::default());
        assert!((f.position - DVec3::new(1.0, 0.0, 0.0) - step).length() < 1e-9, "no keys, no movement");
    }

    #[test]
    fn fly_keeps_eight_kilometre_positions_precise() {
        let mut f = FlyCamera {
            position: DVec3::new(7999.0, -7999.0, 100.0),
            yaw: 0.0,
            pitch: 0.0,
            speed: 1.0,
            ..Default::default()
        };
        for _ in 0..1000 {
            f.update(0.001, FlyInput { forward: 1.0, ..Default::default() });
        }
        assert!((f.position.x - 8000.0).abs() < 1e-6, "{}", f.position.x);
    }

    #[test]
    fn fly_look_clamps_pitch_and_look_at_faces_the_target() {
        let mut f = FlyCamera::default();
        f.look(0.0, -1.0e7);
        assert!(f.pitch <= MAX_PITCH + 1e-6 && f.pitch > 1.5);
        f.look(0.0, 1.0e7);
        assert!(f.pitch >= -MAX_PITCH - 1e-6);
        f.position = DVec3::new(10.0, 10.0, 10.0);
        f.look_at(DVec3::new(10.0, 110.0, 10.0));
        assert!(close(f.camera().forward(), Vec3::Y));
        f.look_at(DVec3::new(10.0, 10.0, 10.0));
        assert!(f.camera().forward().is_finite(), "looking at itself changes nothing");
    }

    #[test]
    fn fly_speed_scrolls_within_limits() {
        let mut f = FlyCamera::default();
        let s = f.speed;
        f.adjust_speed(1.0);
        assert!((f.speed - s * 1.2).abs() < 1e-4);
        f.adjust_speed(1000.0);
        assert_eq!(f.speed, FlyCamera::MAX_SPEED);
        f.adjust_speed(-1000.0);
        assert_eq!(f.speed, FlyCamera::MIN_SPEED);
    }
}
