//! Fly camera in GTA's coordinate system: right-handed, **Z up**, yaw about Z
//! measured from +X, pitch up from the horizon.

use glam::{Mat4, Vec3, Vec4};

use crate::pick::Ray;

const MAX_PITCH: f32 = 89.0 * std::f32::consts::PI / 180.0;

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Camera {
    pub position: Vec3,
    pub yaw: f32,
    pub pitch: f32,
    pub fov_y: f32,
    pub near: f32,
    pub far: f32,
}

impl Default for Camera {
    fn default() -> Self {
        // above and behind the origin, looking at it
        let mut c = Self {
            position: Vec3::ZERO,
            yaw: std::f32::consts::FRAC_PI_4,
            pitch: -0.5,
            fov_y: 60f32.to_radians(),
            near: 0.1,
            far: 30000.0,
        };
        c.focus(Vec3::ZERO, 40.0);
        c
    }
}

impl Camera {
    pub fn forward(&self) -> Vec3 {
        let (sy, cy) = self.yaw.sin_cos();
        let (sp, cp) = self.pitch.sin_cos();
        Vec3::new(cp * cy, cp * sy, sp)
    }

    pub fn right(&self) -> Vec3 {
        // forward is never parallel to Z because pitch is clamped below 90°
        self.forward().cross(Vec3::Z).normalize()
    }

    pub fn view(&self) -> Mat4 {
        Mat4::look_to_rh(self.position, self.forward(), Vec3::Z)
    }

    pub fn proj(&self, aspect: f32) -> Mat4 {
        Mat4::perspective_rh(self.fov_y, aspect.max(1e-3), self.near, self.far)
    }

    pub fn view_proj(&self, aspect: f32) -> Mat4 {
        self.proj(aspect) * self.view()
    }

    /// Turns the view by a mouse movement in pixels (`sensitivity` radians per pixel).
    pub fn look(&mut self, dx: f32, dy: f32, sensitivity: f32) {
        self.yaw = (self.yaw - dx * sensitivity).rem_euclid(std::f32::consts::TAU);
        self.pitch = (self.pitch - dy * sensitivity).clamp(-MAX_PITCH, MAX_PITCH);
    }

    /// Moves along the view axes: forward/back, strafe, and world-up.
    pub fn fly(&mut self, forward: f32, right: f32, up: f32) {
        self.position += self.forward() * forward + self.right() * right + Vec3::Z * up;
    }

    /// Places the camera `distance` in front of `target`, keeping its orientation.
    pub fn focus(&mut self, target: Vec3, distance: f32) {
        self.position = target - self.forward() * distance.max(0.5);
    }

    /// Orbits around `pivot` by a mouse movement, keeping the distance.
    pub fn orbit(&mut self, pivot: Vec3, dx: f32, dy: f32, sensitivity: f32) {
        let dist = (self.position - pivot).length().max(0.5);
        self.look(dx, dy, sensitivity);
        self.focus(pivot, dist);
    }

    /// Moves toward/away from `pivot`; never passes through it.
    pub fn dolly(&mut self, pivot: Vec3, amount: f32) {
        let dist = (self.position - pivot).length();
        self.focus(pivot, (dist * (1.0 - amount)).max(0.5));
    }

    /// Frames an axis-aligned box so it fills about half of the view.
    pub fn frame(&mut self, min: Vec3, max: Vec3) {
        let center = (min + max) * 0.5;
        let radius = ((max - min).length() * 0.5).max(2.0);
        self.focus(center, radius / (self.fov_y * 0.5).tan() * 1.1);
    }

    /// World-space ray through a point given in normalised device coordinates
    /// (`x`, `y` in -1..1, y up).
    pub fn ray(&self, ndc_x: f32, ndc_y: f32, aspect: f32) -> Ray {
        let inv = self.view_proj(aspect).inverse();
        let unproject = |z: f32| {
            let p = inv * Vec4::new(ndc_x, ndc_y, z, 1.0);
            p.truncate() / p.w
        };
        let (near, far) = (unproject(0.0), unproject(1.0));
        Ray { origin: near, dir: (far - near).normalize() }
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
        c.yaw = std::f32::consts::FRAC_PI_2;
        assert!(close(c.forward(), Vec3::Y));
        c.pitch = 1.0;
        assert!(c.forward().z > 0.8, "positive pitch looks up");
    }

    #[test]
    fn mouse_right_turns_clockwise_and_pitch_is_clamped() {
        let mut c = Camera { yaw: 1.0, pitch: 0.0, ..Default::default() };
        c.look(100.0, 0.0, 0.005);
        assert!(c.yaw < 1.0, "dragging right decreases yaw");
        c.look(0.0, -1.0e6, 0.005);
        assert!(c.pitch <= MAX_PITCH + 1e-6 && c.pitch > 1.5, "pitch stops short of straight up");
        c.look(0.0, 1.0e6, 0.005);
        assert!(c.pitch >= -MAX_PITCH - 1e-6);
        assert!(c.right().is_finite(), "no gimbal singularity at the clamp");
    }

    #[test]
    fn fly_moves_in_view_space() {
        let mut c = Camera { position: Vec3::ZERO, yaw: 0.0, pitch: 0.0, ..Default::default() };
        c.fly(2.0, 0.0, 0.0);
        assert!(close(c.position, Vec3::new(2.0, 0.0, 0.0)));
        c.fly(0.0, 1.0, 3.0);
        assert!(
            close(c.position, Vec3::new(2.0, -1.0, 3.0)),
            "strafe right is -Y, up is world +Z: {:?}",
            c.position
        );
    }

    #[test]
    fn center_ray_matches_the_view_direction() {
        let c = Camera { position: Vec3::new(5.0, -3.0, 20.0), yaw: 0.7, pitch: -0.4, ..Default::default() };
        let r = c.ray(0.0, 0.0, 16.0 / 9.0);
        assert!(close(r.dir, c.forward()), "{:?} vs {:?}", r.dir, c.forward());
        assert!((r.origin - c.position).length() < c.near * 1.5);
    }

    #[test]
    fn rays_through_screen_edges_diverge_as_expected() {
        let c = Camera { yaw: 0.0, pitch: 0.0, ..Default::default() };
        let left = c.ray(-1.0, 0.0, 1.0).dir;
        let right = c.ray(1.0, 0.0, 1.0).dir;
        assert!(left.y > 0.0 && right.y < 0.0, "left of the view is +Y when looking down +X");
        let angle = left.angle_between(right);
        assert!(
            (angle - c.fov_y).abs() < 1e-2,
            "square viewport: horizontal fov equals vertical, got {angle}"
        );
        assert!(c.ray(0.0, 1.0, 1.0).dir.z > 0.0, "top of the screen looks up");
    }

    #[test]
    fn orbit_and_dolly_preserve_what_they_should() {
        let mut c = Camera::default();
        let pivot = Vec3::new(10.0, 10.0, 0.0);
        c.focus(pivot, 30.0);
        c.orbit(pivot, 40.0, 10.0, 0.01);
        assert!(((c.position - pivot).length() - 30.0).abs() < 1e-3, "orbit keeps the distance");
        assert!(close(c.position + c.forward() * 30.0, pivot), "and keeps looking at the pivot");
        c.dolly(pivot, 0.5);
        assert!(((c.position - pivot).length() - 15.0).abs() < 1e-3);
        c.dolly(pivot, 5.0);
        assert!((c.position - pivot).length() >= 0.5 - 1e-3, "never passes through the pivot");
    }

    #[test]
    fn framing_a_box_keeps_it_in_view() {
        let mut c = Camera::default();
        let (lo, hi) = (Vec3::new(-100.0, -100.0, 0.0), Vec3::new(100.0, 100.0, 30.0));
        c.frame(lo, hi);
        let vp = c.view_proj(16.0 / 9.0);
        for corner in [lo, hi, Vec3::new(lo.x, hi.y, lo.z), Vec3::new(hi.x, lo.y, hi.z)] {
            let p = vp * corner.extend(1.0);
            let ndc = p.truncate() / p.w;
            assert!(p.w > 0.0 && ndc.x.abs() <= 1.0 && ndc.y.abs() <= 1.0, "{corner:?} -> {ndc:?}");
        }
    }
}
