//! The time of day: `time.xml`, which hour falls in which time-cycle sample, where the sun and moon
//! stand, and the evaluation of the weather's per-sample keyframes for one moment.
//!
//! Ported from `CodeWalker.Core/World/Timecycle.cs` (`Init`, `SetTime`, `IsNightTime`),
//! `CodeWalker.Core/World/Weather.cs` (`GetDynamicValue` and `WeatherValues.Update`) and
//! `CodeWalker/Rendering/Renderer.cs` (`UpdateGlobalLights` for the sun and moon).
//!
//! `time.xml` (`common.rpf/data/levels/gta5/time.xml`, plain XML as far as the C# reads it):
//!
//! ```xml
//! <time>
//!   <suninfo sun_roll="122" sun_yaw="0"/>
//!   <mooninfo moon_roll="-122" moon_wobble_freq="2" moon_wobble_amp="0.2" moon_wobble_offset="0.375"/>
//!   <sample name="MIDNIGHT" hour="0" duration="5" uw_tc_mod="..."/>  <!-- one per time-cycle sample -->
//!   <region name="GLOBAL"/>                                           <!-- one per region -->
//! </time>
//! ```
//!
//! A weather type keeps one number per sample for every parameter (see [`super::weather`]). The
//! samples are the keyframes of a day: `hour` is where one starts, `duration` how long it is held
//! unchanged, and the rest of the time up to the next sample's hour is a linear blend towards that
//! next sample. After the last sample the day wraps to the first.

use std::f32::consts::PI;

use super::{
    Result,
    weather::{CycleRegion, Weather},
    xml::{self, N},
};

/// One keyframe slot of the day.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct TimeSample {
    pub name: String,
    pub hour: f32,
    /// Hours the sample is held before the blend towards the next one starts.
    pub duration: f32,
    /// Time-cycle modifier used under water (a name in `timecycle_mods_*.xml`).
    pub uw_tc_mod: String,
}

/// `time.xml`.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct Timecycle {
    pub sun_roll: f32,
    pub sun_yaw: f32,
    pub moon_roll: f32,
    pub moon_wobble_freq: f32,
    pub moon_wobble_amp: f32,
    pub moon_wobble_offset: f32,
    pub samples: Vec<TimeSample>,
    pub regions: Vec<String>,
}

/// Where an hour falls: the sample it is in and how far it has moved on to the next one
/// (`Timecycle.CurrentSampleIndex`, `CurrentSampleBlend`, `NextSampleBlend`).
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct SamplePosition {
    /// The hour inside the day, `0 <= hour < 24`.
    pub hour: f32,
    pub index: usize,
    /// Weight of the sample at `index`; `1 - next_blend`.
    pub current_blend: f32,
    /// Weight of the sample after it.
    pub next_blend: f32,
}

impl Default for SamplePosition {
    /// What the C# holds before the first `SetTime`.
    fn default() -> Self {
        Self { hour: 0.0, index: 0, current_blend: 1.0, next_blend: 0.0 }
    }
}

/// The sun and the moon at an hour, as unit vectors in world axes (x east, y north, z up).
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct SunMoon {
    /// Direction from the viewer towards the sun.
    pub sun_direction: [f32; 3],
    pub moon_direction: [f32; 3],
    /// The axis the moon disc is oriented around.
    pub moon_axis: [f32; 3],
}

impl Timecycle {
    pub fn parse(bytes: &[u8]) -> Result<Timecycle> {
        xml::with_root(bytes, |root| Ok(Self::read(root)))
    }

    fn read(root: N) -> Timecycle {
        let (sun, moon) = (xml::child(root, "suninfo"), xml::child(root, "mooninfo"));
        let f = |n: Option<N>, name: &str| n.map_or(0.0, |n| xml::attr_f32(n, name));
        Timecycle {
            sun_roll: f(sun, "sun_roll"),
            sun_yaw: f(sun, "sun_yaw"),
            moon_roll: f(moon, "moon_roll"),
            moon_wobble_freq: f(moon, "moon_wobble_freq"),
            moon_wobble_amp: f(moon, "moon_wobble_amp"),
            moon_wobble_offset: f(moon, "moon_wobble_offset"),
            samples: xml::children(root, "sample")
                .into_iter()
                .map(|s| TimeSample {
                    name: xml::attr_string(s, "name"),
                    hour: xml::attr_f32(s, "hour"),
                    duration: xml::attr_f32(s, "duration"),
                    uw_tc_mod: xml::attr_string(s, "uw_tc_mod"),
                })
                .collect(),
            regions: xml::children(root, "region").into_iter().map(|r| xml::attr_string(r, "name")).collect(),
        }
    }

    /// A `time.xml` this module reads back to the same value; for tests and the fixture.
    pub fn to_xml(&self) -> String {
        let mut s = format!("{}<time version=\"1\">\n", xml::HEADER);
        s += &format!("  <suninfo sun_roll=\"{}\" sun_yaw=\"{}\"/>\n", self.sun_roll, self.sun_yaw);
        s += &format!(
            "  <mooninfo moon_roll=\"{}\" moon_wobble_freq=\"{}\" moon_wobble_amp=\"{}\" moon_wobble_offset=\"{}\"/>\n",
            self.moon_roll, self.moon_wobble_freq, self.moon_wobble_amp, self.moon_wobble_offset
        );
        for t in &self.samples {
            s += &format!(
                "  <sample name=\"{}\" hour=\"{}\" duration=\"{}\" uw_tc_mod=\"{}\"/>\n",
                xml::esc(&t.name),
                t.hour,
                t.duration,
                xml::esc(&t.uw_tc_mod)
            );
        }
        for r in &self.regions {
            s += &format!("  <region name=\"{}\"/>\n", xml::esc(r));
        }
        s + "</time>\n"
    }

    /// Which sample `hour` is in and the blend towards the next (`Timecycle.SetTime`). The hour wraps
    /// into the day; the C# only wraps hours of 24 and over, and leaves a negative hour as it is, which
    /// lands in the last sample with no blend. `None` when the file has no samples.
    pub fn locate(&self, hour: f32) -> Option<SamplePosition> {
        let h = wrap_hour(hour);
        let n = self.samples.len();
        for (i, cur) in self.samples.iter().enumerate() {
            let last = i + 1 >= n;
            let nxt = &self.samples[if last { 0 } else { i + 1 }];
            let nxt_hour = if last { nxt.hour + 24.0 } else { nxt.hour };
            if (h >= cur.hour && h < nxt_hour) || last {
                let range = (nxt_hour - cur.hour) - cur.duration;
                let rel = h - (cur.hour + cur.duration);
                let value = rel / range;
                // a sample as long as its own span (range 0) has nothing to blend: held until the end
                let blend = if value.is_finite() { value.clamp(0.0, 1.0) } else { 0.0 };
                return Some(SamplePosition {
                    hour: h,
                    index: i,
                    current_blend: 1.0 - blend,
                    next_blend: blend,
                });
            }
        }
        None
    }

    /// Night as the C# defines it for the distant lights (`Timecycle.IsNightTime`).
    pub fn is_night(hour: f32) -> bool {
        let h = wrap_hour(hour);
        !(6.0..=20.0).contains(&h)
    }

    /// Sun and moon for an hour of the day (`Renderer.UpdateGlobalLights`). The sun is up from 6:00 to
    /// 20:00 and crosses the sky as a half circle tilted by `sun_roll`; the moon makes its own, shorter
    /// pass tilted by `-moon_roll`. The C# has no phase for the moon and does not use the wobble values
    /// it reads, so neither does this.
    pub fn sun_moon(&self, hour: f32) -> SunMoon {
        let t = wrap_hour(hour);
        let sunroll = self.sun_roll * PI / 180.0;
        let moonroll = self.moon_roll * PI / 180.0;
        let dayval = 0.5 + (t - 6.0) / 14.0;
        let nightval = (if t > 12.0 { t - 7.0 } else { t + 17.0 }) / 9.0;
        let (daycyc, nightcyc) = (PI * dayval, PI * nightval);
        let sdir = [sin(daycyc), -cos(daycyc), 0.0];
        let mdir = [-sin(nightcyc), 0.0, -cos(nightcyc)];
        SunMoon {
            sun_direction: normalize(rotate_x(sdir, sunroll)),
            moon_direction: normalize(rotate_x(mdir, -moonroll)),
            moon_axis: normalize(rotate_x([0.0, 1.0, 0.0], -moonroll)),
        }
    }

    /// Every parameter for one moment: the sample the hour falls in, the weather's values for it, and a
    /// blend between two weather types (`Weather.GetDynamicValue` with `WeatherValues.Update`).
    pub fn evaluate(&self, weather: &Weather, q: &EvalQuery) -> TimeCycleParams {
        TimeCycleParams::sample(&self.sampler(weather, q))
    }

    /// One named entry of the cycle files mixed the same way [`evaluate`](Self::evaluate) mixes its own;
    /// for parameters the C# does not model (fog, post-processing, ...). Zero when the weather has none.
    pub fn evaluate_named(&self, weather: &Weather, q: &EvalQuery, name: &str) -> f32 {
        self.sampler(weather, q).value(name)
    }

    fn sampler<'a>(&self, weather: &'a Weather, q: &EvalQuery) -> Sampler<'a> {
        let current = weather.region_of(q.weather, q.region);
        // no second weather is the same as the first one, as in the C# between changes
        let next = q.next_weather.map_or(current, |n| weather.region_of(n, q.region));
        let blend = if q.blend.is_finite() { q.blend.clamp(0.0, 1.0) } else { 0.0 };
        Sampler { pos: self.locate(q.hour).unwrap_or_default(), current, next, blend }
    }
}

/// What to evaluate: an hour, the weather, and optionally a second weather being blended towards.
#[derive(Debug, Clone, Copy)]
pub struct EvalQuery<'a> {
    /// Hour of the day; any value, it wraps.
    pub hour: f32,
    /// A weather type name as in `weather.xml` (`EXTRASUNNY`, `RAIN`, ...).
    pub weather: &'a str,
    /// The weather changing in, if any.
    pub next_weather: Option<&'a str>,
    /// 0 is entirely `weather`, 1 entirely `next_weather` (`CurrentWeatherChangeBlend`).
    pub blend: f32,
    /// A region of the cycle files (`GLOBAL`, `URBAN`); a weather without it uses its first region.
    pub region: &'a str,
}

impl<'a> EvalQuery<'a> {
    pub fn new(hour: f32, weather: &'a str) -> Self {
        Self { hour, weather, next_weather: None, blend: 0.0, region: "GLOBAL" }
    }
}

/// The two regions being mixed, the sample position and the weather blend.
struct Sampler<'a> {
    pos: SamplePosition,
    current: Option<&'a CycleRegion>,
    next: Option<&'a CycleRegion>,
    blend: f32,
}

impl Sampler<'_> {
    /// `GetDynamicValue`. With both regions the two are mixed; with only the current one its value
    /// stands; a next region without a current one gives nothing, as in the C#.
    fn value(&self, name: &str) -> f32 {
        let at = |r: &CycleRegion| r.value_at(name, self.pos.index, self.pos.current_blend);
        match (self.current, self.next) {
            (Some(c), Some(n)) => at(c) * (1.0 - self.blend) + at(n) * self.blend,
            (Some(c), None) => at(c),
            _ => 0.0,
        }
    }

    fn rgb(&self, r: &str, g: &str, b: &str) -> [f32; 3] {
        [self.value(r), self.value(g), self.value(b)]
    }

    fn rgba(&self, r: &str, g: &str, b: &str, a: &str) -> [f32; 4] {
        [self.value(r), self.value(g), self.value(b), self.value(a)]
    }
}

/// Every value `WeatherValues.Update` computes, in the same grouping the C# gives them. Colours are
/// linear RGB with an intensity (or alpha) as the fourth component where the C# has one.
#[derive(Debug, Clone, Copy, Default, PartialEq)]
pub struct TimeCycleParams {
    pub sun_direction: [f32; 3],
    pub moon_direction: [f32; 3],
    pub sky_zenith_col: [f32; 4],
    pub sky_zenith_transition_col: [f32; 4],
    /// `blend_start`, `east_blend`, `west_blend`, `position` of the zenith transition.
    pub sky_zenith_transition: [f32; 4],
    pub sky_azimuth_east_col: [f32; 4],
    pub sky_azimuth_west_col: [f32; 4],
    pub sky_azimuth_transition_col: [f32; 4],
    pub sky_azimuth_transition: f32,
    pub sky_hdr: f32,
    pub sky_plane: [f32; 4],
    pub sky_sun_col: [f32; 3],
    pub sky_sun_disc_col: [f32; 3],
    pub sky_sun_disc_size: f32,
    pub sky_sun_hdr: f32,
    /// Mie phase, scatter and intensity multiplier.
    pub sky_sun_mie: [f32; 3],
    pub sky_sun_influence_radius: f32,
    pub sky_sun_scatter_inten: f32,
    pub sky_moon_col: [f32; 3],
    pub sky_moon_disc_size: f32,
    pub sky_moon_iten: f32,
    pub sky_moon_influence_radius: f32,
    pub sky_moon_scatter_inten: f32,
    pub sky_stars_iten: f32,
    /// Colour of the directional (sun or moon) light; the fourth value is its multiplier.
    pub light_dir_col: [f32; 4],
    pub light_dir_amb_col: [f32; 4],
    pub light_dir_amb_intensity_mult: f32,
    pub light_dir_amb_bounce: f32,
    pub light_natural_amb_down: [f32; 4],
    pub light_natural_amb_up: [f32; 4],
    pub light_natural_amb_up_intensity_mult: f32,
    pub light_artificial_int_down: [f32; 4],
    pub light_artificial_int_up: [f32; 4],
    pub light_artificial_ext_down: [f32; 4],
    pub light_artificial_ext_up: [f32; 4],
}

impl TimeCycleParams {
    /// The names are the element names of the cycle files (`WeatherValues.Update`).
    fn sample(s: &Sampler) -> Self {
        Self {
            sun_direction: s.rgb("sun_direction_x", "sun_direction_y", "sun_direction_z"),
            moon_direction: s.rgb("moon_direction_x", "moon_direction_y", "moon_direction_z"),
            sky_zenith_col: s.rgba(
                "sky_zenith_col_r",
                "sky_zenith_col_g",
                "sky_zenith_col_b",
                "sky_zenith_col_inten",
            ),
            sky_zenith_transition_col: s.rgba(
                "sky_zenith_transition_col_r",
                "sky_zenith_transition_col_g",
                "sky_zenith_transition_col_b",
                "sky_zenith_transition_col_inten",
            ),
            sky_zenith_transition: s.rgba(
                "sky_zenith_blend_start",
                "sky_zenith_transition_east_blend",
                "sky_zenith_transition_west_blend",
                "sky_zenith_transition_position",
            ),
            sky_azimuth_east_col: s.rgba(
                "sky_azimuth_east_col_r",
                "sky_azimuth_east_col_g",
                "sky_azimuth_east_col_b",
                "sky_azimuth_east_col_inten",
            ),
            sky_azimuth_west_col: s.rgba(
                "sky_azimuth_west_col_r",
                "sky_azimuth_west_col_g",
                "sky_azimuth_west_col_b",
                "sky_azimuth_west_col_inten",
            ),
            sky_azimuth_transition_col: s.rgba(
                "sky_azimuth_transition_col_r",
                "sky_azimuth_transition_col_g",
                "sky_azimuth_transition_col_b",
                "sky_azimuth_transition_col_inten",
            ),
            sky_azimuth_transition: s.value("sky_azimuth_transition_position"),
            sky_hdr: s.value("sky_hdr"),
            sky_plane: s.rgba("sky_plane_r", "sky_plane_g", "sky_plane_b", "sky_plane_inten"),
            sky_sun_col: s.rgb("sky_sun_col_r", "sky_sun_col_g", "sky_sun_col_b"),
            sky_sun_disc_col: s.rgb("sky_sun_disc_col_r", "sky_sun_disc_col_g", "sky_sun_disc_col_b"),
            sky_sun_disc_size: s.value("sky_sun_disc_size"),
            sky_sun_hdr: s.value("sky_sun_hdr"),
            sky_sun_mie: s.rgb("sky_sun_miephase", "sky_sun_miescatter", "sky_sun_mie_intensity_mult"),
            sky_sun_influence_radius: s.value("sky_sun_influence_radius"),
            sky_sun_scatter_inten: s.value("sky_sun_scatter_inten"),
            sky_moon_col: s.rgb("sky_moon_col_r", "sky_moon_col_g", "sky_moon_col_b"),
            sky_moon_disc_size: s.value("sky_moon_disc_size"),
            sky_moon_iten: s.value("sky_moon_iten"),
            sky_moon_influence_radius: s.value("sky_moon_influence_radius"),
            sky_moon_scatter_inten: s.value("sky_moon_scatter_inten"),
            sky_stars_iten: s.value("sky_stars_iten"),
            light_dir_col: s.rgba("light_dir_col_r", "light_dir_col_g", "light_dir_col_b", "light_dir_mult"),
            light_dir_amb_col: s.rgba(
                "light_directional_amb_col_r",
                "light_directional_amb_col_g",
                "light_directional_amb_col_b",
                "light_directional_amb_intensity",
            ),
            light_dir_amb_intensity_mult: s.value("light_directional_amb_intensity_mult"),
            light_dir_amb_bounce: s.value("light_directional_amb_bounce_enabled"),
            light_natural_amb_down: s.rgba(
                "light_natural_amb_down_col_r",
                "light_natural_amb_down_col_g",
                "light_natural_amb_down_col_b",
                "light_natural_amb_down_intensity",
            ),
            light_natural_amb_up: s.rgba(
                "light_natural_amb_up_col_r",
                "light_natural_amb_up_col_g",
                "light_natural_amb_up_col_b",
                "light_natural_amb_up_intensity",
            ),
            light_natural_amb_up_intensity_mult: s.value("light_natural_amb_up_intensity_mult"),
            light_artificial_int_down: s.rgba(
                "light_artificial_int_down_col_r",
                "light_artificial_int_down_col_g",
                "light_artificial_int_down_col_b",
                "light_artificial_int_down_intensity",
            ),
            light_artificial_int_up: s.rgba(
                "light_artificial_int_up_col_r",
                "light_artificial_int_up_col_g",
                "light_artificial_int_up_col_b",
                "light_artificial_int_up_intensity",
            ),
            light_artificial_ext_down: s.rgba(
                "light_artificial_ext_down_col_r",
                "light_artificial_ext_down_col_g",
                "light_artificial_ext_down_col_b",
                "light_artificial_ext_down_intensity",
            ),
            light_artificial_ext_up: s.rgba(
                "light_artificial_ext_up_col_r",
                "light_artificial_ext_up_col_g",
                "light_artificial_ext_up_col_b",
                "light_artificial_ext_up_intensity",
            ),
        }
    }
}

/// The hour inside `0..24`. Not a number counts as midnight.
pub fn wrap_hour(hour: f32) -> f32 {
    if !hour.is_finite() {
        return 0.0;
    }
    let h = hour.rem_euclid(24.0);
    // rem_euclid can round a tiny negative hour up to exactly 24
    if h >= 24.0 { 0.0 } else { h }
}

fn sin(a: f32) -> f32 {
    (a as f64).sin() as f32
}

fn cos(a: f32) -> f32 {
    (a as f64).cos() as f32
}

/// `saxis.Multiply(v)` for `Quaternion.RotationYawPitchRoll(0, angle, 0)`: a turn about the x axis.
fn rotate_x(v: [f32; 3], angle: f32) -> [f32; 3] {
    let (x, w) = (sin(angle * 0.5), cos(angle * 0.5));
    let (awxx, axxx) = (w * x * 2.0, x * x * 2.0);
    [v[0], v[1] * (1.0 - axxx) - v[2] * awxx, v[1] * awxx + v[2] * (1.0 - axxx)]
}

pub(super) fn normalize(v: [f32; 3]) -> [f32; 3] {
    let len = (v[0] * v[0] + v[1] * v[1] + v[2] * v[2]).sqrt();
    if len > 0.0 { [v[0] / len, v[1] / len, v[2] / len] } else { v }
}
