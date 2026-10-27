//! From evaluated time-cycle values to what a renderer takes: the global lights and the sky constants.
//!
//! Ported from `CodeWalker/Rendering/Renderer.cs` (`UpdateGlobalLights`) and
//! `CodeWalker/Rendering/Shaders/SkydomeShader.cs` (`UpdateSkyLocals`), so the numbers match what the
//! original tool's shaders were fed.

use super::timecycle::{TimeCycleParams, Timecycle, wrap_hour};

fn scale(c: [f32; 4], s: f32) -> [f32; 4] {
    c.map(|v| v * s)
}

fn clamp_max(c: [f32; 4], max: f32) -> [f32; 4] {
    c.map(|v| v.min(max))
}

fn neg(v: [f32; 3]) -> [f32; 3] {
    v.map(|x| -x)
}

/// The lights of the scene (`ShaderGlobalLights`): one directional light (the sun by day, the moon at
/// night) and the ambient terms, as RGBA that is already multiplied by its intensity.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct GlobalLights {
    pub hdr: bool,
    /// Brightness scale for tone mapping: the sky's HDR value in HDR mode, otherwise 1; at least 1.
    pub hdr_intensity: f32,
    pub sun_direction: [f32; 3],
    pub moon_direction: [f32; 3],
    pub moon_axis: [f32; 3],
    /// Direction the light comes from. The sun's, or the moon's from 21:00 to 5:00, never below the
    /// horizon (its height is raised to zero, and it is not renormalised, as in the C#).
    pub light_direction: [f32; 3],
    pub dir_colour: [f32; 4],
    pub dir_ambient_colour: [f32; 4],
    pub natural_ambient_up: [f32; 4],
    pub natural_ambient_down: [f32; 4],
    pub artificial_ambient_up: [f32; 4],
    pub artificial_ambient_down: [f32; 4],
}

impl GlobalLights {
    /// Lights for an hour of the day. `params` are the evaluated time-cycle values; without them the
    /// lights are plain white with grey ambient light. `hdr` selects the HDR path: without it the
    /// colours are capped to what a display can show. `swap_hemisphere` mirrors the sun north to south
    /// (the C# does this for the model viewers to get better lighting).
    pub fn from_time(
        timecycle: &Timecycle,
        hour: f32,
        params: Option<&TimeCycleParams>,
        hdr: bool,
        swap_hemisphere: bool,
    ) -> Self {
        let hour = wrap_hour(hour);
        let sm = timecycle.sun_moon(hour);
        let mut sun = sm.sun_direction;
        if swap_hemisphere {
            sun[1] = -sun[1];
        }
        let mut light = sun;
        if !(5.0..=21.0).contains(&hour) {
            light = sm.moon_direction;
        }
        if light[2] < 0.0 {
            light[2] = 0.0;
        }

        let mut lights = Self {
            hdr,
            hdr_intensity: 1.0,
            sun_direction: sun,
            moon_direction: sm.moon_direction,
            moon_axis: sm.moon_axis,
            light_direction: light,
            dir_colour: [1.0; 4],
            dir_ambient_colour: [0.5, 0.5, 0.5, 1.0],
            natural_ambient_up: [0.0; 4],
            natural_ambient_down: [0.0; 4],
            artificial_ambient_up: [0.0; 4],
            artificial_ambient_down: [0.0; 4],
        };
        let Some(p) = params else { return lights };

        let minmult = if hdr { 0.0 } else { 0.5 };
        let dir = p.light_dir_col;
        let amb = p.light_dir_amb_col;
        let up = p.light_natural_amb_up;
        let down = p.light_natural_amb_down;
        let (aup, adown) = (p.light_artificial_ext_up, p.light_artificial_ext_down);
        lights.dir_colour = scale(dir, dir[3].max(minmult));
        lights.dir_ambient_colour = scale(amb, amb[3] * p.light_dir_amb_intensity_mult);
        lights.natural_ambient_up = scale(up, up[3] * p.light_natural_amb_up_intensity_mult);
        lights.natural_ambient_down = scale(down, down[3]);
        lights.artificial_ambient_up = scale(aup, aup[3]);
        lights.artificial_ambient_down = scale(adown, adown[3]);
        if hdr {
            lights.hdr_intensity = p.sky_hdr.max(1.0);
        } else {
            lights.dir_colour = clamp_max(lights.dir_colour, 1.0);
            lights.dir_ambient_colour = clamp_max(lights.dir_ambient_colour, 0.5);
            lights.natural_ambient_up = clamp_max(lights.natural_ambient_up, 0.5);
            lights.natural_ambient_down = clamp_max(lights.natural_ambient_down, 0.5);
            lights.artificial_ambient_up = clamp_max(lights.artificial_ambient_up, 0.5);
            lights.artificial_ambient_down = clamp_max(lights.artificial_ambient_down, 0.5);
        }
        lights
    }

    /// Lights from a direction chosen by hand instead of the time of day (`controllightdir`, the model
    /// viewer's light control): `angle_x` turns it around the vertical axis, `angle_y` raises it, both
    /// in radians. Only the directional light and the sun and moon follow it; ambient terms are zero.
    pub fn manual(angle_x: f32, angle_y: f32, params: Option<&TimeCycleParams>, hdr: bool) -> Self {
        let (sx, cx) = ((-angle_x as f64).sin() as f32, (-angle_x as f64).cos() as f32);
        let cry = (angle_y as f64).cos() as f32;
        let dir = [-sx * cry, -cx * cry, (angle_y as f64).sin() as f32];
        let mut lights = Self {
            hdr,
            hdr_intensity: 1.0,
            sun_direction: dir,
            moon_direction: neg(dir),
            moon_axis: [0.0, 0.0, 1.0],
            light_direction: dir,
            dir_colour: [1.0; 4],
            dir_ambient_colour: [0.5, 0.5, 0.5, 1.0],
            natural_ambient_up: [0.0; 4],
            natural_ambient_down: [0.0; 4],
            artificial_ambient_up: [0.0; 4],
            artificial_ambient_down: [0.0; 4],
        };
        if let (true, Some(p)) = (hdr, params) {
            lights.dir_colour = [p.sky_hdr, p.sky_hdr, p.sky_hdr, 1.0];
            let a = 0.5 * p.sky_hdr * 0.35;
            lights.dir_ambient_colour = [a, a, a, 1.0];
            lights.hdr_intensity = p.sky_hdr.max(1.0);
        }
        lights
    }
}

/// The constants the sky dome, sun and moon are drawn with (`SkydomeShader.UpdateSkyLocals`).
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct SkyParams {
    pub azimuth_east_color: [f32; 4],
    pub azimuth_west_color: [f32; 4],
    pub azimuth_transition_color: [f32; 3],
    pub azimuth_transition_position: f32,
    pub zenith_color: [f32; 4],
    pub zenith_transition_color: [f32; 4],
    pub zenith_constants: [f32; 4],
    pub sky_plane_color: [f32; 4],
    pub hdr_intensity: f32,
    pub sun_color: [f32; 3],
    /// Sun colour scaled by the sun's HDR value, with that value as the fourth component.
    pub sun_color_hdr: [f32; 4],
    /// Disc colour with the disc size.
    pub sun_disc_color_hdr: [f32; 4],
    /// Mie phase times scatter, mie scatter, influence size, mie intensity multiplier.
    pub sun_constants: [f32; 4],
    /// Direction the sun is looked at: the negative of the direction towards it.
    pub sun_direction: [f32; 3],
    pub starfield_intensity: f32,
    pub moon_direction: [f32; 3],
    pub moon_intensity: f32,
    pub moon_color: [f32; 3],
}

impl SkyParams {
    /// `hdr` is the renderer's HDR switch: without it the sky and sun brightness are capped.
    pub fn new(p: &TimeCycleParams, lights: &GlobalLights, hdr: bool) -> Self {
        let sky_hdr = if hdr { p.sky_hdr } else { p.sky_hdr.min(1.8) };
        let sun_hdr = if hdr { p.sky_sun_hdr } else { p.sky_sun_hdr.min(4.8) };
        let sun = p.sky_sun_col;
        let mie = p.sky_sun_mie;
        let disc = p.sky_sun_disc_col;
        let t = p.sky_azimuth_transition_col;
        Self {
            azimuth_east_color: p.sky_azimuth_east_col,
            azimuth_west_color: p.sky_azimuth_west_col,
            azimuth_transition_color: [t[0], t[1], t[2]],
            azimuth_transition_position: p.sky_azimuth_transition,
            zenith_color: p.sky_zenith_col,
            zenith_transition_color: p.sky_zenith_transition_col,
            zenith_constants: p.sky_zenith_transition,
            sky_plane_color: p.sky_plane,
            hdr_intensity: sky_hdr,
            sun_color: sun,
            sun_color_hdr: [sun[0] * sun_hdr, sun[1] * sun_hdr, sun[2] * sun_hdr, sun_hdr],
            sun_disc_color_hdr: [disc[0], disc[1], disc[2], p.sky_sun_disc_size],
            sun_constants: [mie[0] * mie[1], mie[1], 0.00003 * p.sky_sun_influence_radius, mie[2]],
            sun_direction: neg(lights.sun_direction),
            // brighter stars than the data says, which the original tool found looked better
            starfield_intensity: p.sky_stars_iten * 5.0,
            moon_direction: p.moon_direction,
            moon_intensity: p.sky_moon_iten,
            moon_color: p.sky_moon_col,
        }
    }
}
