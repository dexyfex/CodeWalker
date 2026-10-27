//! Weather: `weather.xml` and the per-type time-cycle files it names.
//!
//! Ported from `CodeWalker.Core/World/Weather.cs` (`Weather.Init`, `WeatherType.Init`,
//! `WeatherGpuFx`, `WeatherCycle`, `WeatherCycleKeyframeData/Region/DataEntry`).
//!
//! `weather.xml` (`update/update.rpf/common/data/levels/gta5/weather.xml`, else
//! `common.rpf/data/levels/gta5/weather.xml`; plain XML as far as the C# reads it) has three lists of
//! `Item`s: `WeatherGpuFx` (rain and snow particle settings), `WeatherTypes` and `WeatherCycles`. Each
//! scalar is a child element with a `value` attribute (`<Rain value="0.5"/>`), each name a child with
//! text (`<Name>RAIN</Name>`). A weather type's `TimeCycleFilename` (`common:/data/timecycle/w_rain.xml`)
//! is a second file holding the keyframes:
//!
//! ```xml
//! <timecycle>                                  <!-- the root's name is not read -->
//!   <cycle name="rain" regions="2">            <!-- one child; if there are several the last wins -->
//!     <region name="GLOBAL">
//!       <sky_zenith_col_r>0.1 0.1 0.2 ...</sky_zenith_col_r>   <!-- one number per time sample -->
//!     </region>
//!     <region name="URBAN"> ... </region>
//!   </cycle>
//! </timecycle>
//! ```

use std::collections::BTreeMap;

use super::{
    Result,
    xml::{self, N},
};
use crate::hash::joaat_lower;

/// The particle systems a weather type uses (`WeatherGpuFx`).
#[derive(Debug, Clone, PartialEq, Default)]
pub struct WeatherGpuFx {
    pub name: String,
    pub system_type: String,
    pub diffuse_name: String,
    pub distortion_texture: String,
    pub diffuse_splash_name: String,
    pub drive_type: String,
    pub wind_influence: f32,
    pub gravity: f32,
    pub emitter_settings_name: String,
    pub render_settings_name: String,
}

/// A step in the scripted weather sequence (`WeatherCycle`).
#[derive(Debug, Clone, PartialEq, Default)]
pub struct WeatherCycle {
    pub cycle_name: String,
    pub time_mult: f32,
}

/// Declares [`WeatherType`] with its numeric fields tied to the element each is read from, so the
/// parser, the writer and the listing used by commands cannot drift apart.
macro_rules! weather_type {
    ($($field:ident => $tag:literal,)*) => {
        /// One weather (`WeatherType`): the numbers the game and the water/rain effects use, and the
        /// time-cycle keyframes it names.
        #[derive(Debug, Clone, PartialEq, Default)]
        pub struct WeatherType {
            pub name: String,
            /// `joaat` of the lower-case name.
            pub name_hash: u32,
            $(pub $field: f32,)*
            pub old_setting_name: String,
            pub drop_setting_name: String,
            pub mist_setting_name: String,
            pub ground_setting_name: String,
            /// Where the keyframes are, as the game writes it (`common:/data/timecycle/w_rain.xml`).
            pub time_cycle_filename: String,
            /// Entry of `cloudkeyframes.xml` ([`super::clouds::CloudSettingsMap`]) this weather uses.
            pub cloud_settings_name: String,
            /// The keyframes, once [`Weather::load_cycles`] found the file.
            pub time_cycle: Option<CycleData>,
        }

        impl WeatherType {
            /// The numeric settings by their element names, in file order.
            pub fn values(&self) -> Vec<(&'static str, f32)> {
                vec![$(($tag, self.$field),)*]
            }

            fn read(node: N) -> WeatherType {
                let name = xml::child_string(node, "Name").trim().to_string();
                WeatherType {
                    name_hash: joaat_lower(&name),
                    name,
                    $($field: xml::value_f32(node, $tag),)*
                    old_setting_name: xml::child_string(node, "OldSettingName"),
                    drop_setting_name: xml::child_string(node, "DropSettingName"),
                    mist_setting_name: xml::child_string(node, "MistSettingName"),
                    ground_setting_name: xml::child_string(node, "GroundSettingName"),
                    time_cycle_filename: xml::child_string(node, "TimeCycleFilename").trim().to_string(),
                    cloud_settings_name: xml::child_string(node, "CloudSettingsName").trim().to_string(),
                    time_cycle: None,
                }
            }
        }
    };
}

weather_type! {
    sun => "Sun",
    cloud => "Cloud",
    wind_min => "WindMin",
    wind_max => "WindMax",
    rain => "Rain",
    snow => "Snow",
    snow_mist => "SnowMist",
    fog => "Fog",
    ripple_bumpiness => "RippleBumpiness",
    ripple_min_bumpiness => "RippleMinBumpiness",
    ripple_max_bumpiness => "RippleMaxBumpiness",
    ripple_bumpiness_wind_scale => "RippleBumpinessWindScale",
    ripple_scale => "RippleScale",
    ripple_speed => "RippleSpeed",
    ripple_velocity_transfer => "RippleVelocityTransfer",
    ocean_bumpiness => "OceanBumpiness",
    deep_ocean_scale => "DeepOceanScale",
    ocean_noise_min_amplitude => "OceanNoiseMinAmplitude",
    ocean_wave_amplitude => "OceanWaveAmplitude",
    shore_wave_amplitude => "ShoreWaveAmplitude",
    ocean_wave_wind_scale => "OceanWaveWindScale",
    shore_wave_wind_scale => "ShoreWaveWindScale",
    ocean_wave_min_amplitude => "OceanWaveMinAmplitude",
    shore_wave_min_amplitude => "ShoreWaveMinAmplitude",
    ocean_wave_max_amplitude => "OceanWaveMaxAmplitude",
    shore_wave_max_amplitude => "ShoreWaveMaxAmplitude",
    ocean_foam_intensity => "OceanFoamIntensity",
    ocean_foam_scale => "OceanFoamScale",
    ripple_disturb => "RippleDisturb",
    lightning => "Lightning",
    sandstorm => "Sandstorm",
}

impl WeatherType {
    /// The region the game would use for `name`: that region, or the first one the file lists
    /// (`WeatherType.GetRegion`).
    pub fn region(&self, name: &str) -> Option<&CycleRegion> {
        self.time_cycle.as_ref()?.region(name)
    }
}

/// `WeatherCycleKeyframeData`: the keyframes of one weather type, by region.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CycleData {
    pub name: String,
    /// The `regions` attribute; the regions actually present are in `regions`.
    pub region_count: i32,
    /// In file order; a region named twice keeps the later one in the earlier one's place.
    pub regions: Vec<CycleRegion>,
}

impl CycleData {
    /// The named region, else the first (`WeatherType.GetRegion`).
    pub fn region(&self, name: &str) -> Option<&CycleRegion> {
        self.regions.iter().find(|r| r.name == name).or_else(|| self.regions.first())
    }

    fn read(node: N) -> CycleData {
        let mut regions: Vec<CycleRegion> = Vec::new();
        for r in node.children().filter(|c| c.is_element()) {
            let region = CycleRegion::read(r);
            match regions.iter_mut().find(|x| x.name == region.name) {
                Some(slot) => *slot = region,
                None => regions.push(region),
            }
        }
        CycleData {
            name: xml::attr_string(node, "name"),
            region_count: xml::attr_i32(node, "regions"),
            regions,
        }
    }

    /// Reads one time-cycle file (the root's last child element is the cycle, as in the C#). `None` for
    /// a file with no element below its root.
    pub fn parse(bytes: &[u8]) -> Result<Option<CycleData>> {
        xml::with_root(bytes, |root| Ok(root.children().rfind(|c| c.is_element()).map(CycleData::read)))
    }

    /// A file [`parse`](Self::parse) reads back to the same value.
    pub fn to_xml(&self) -> String {
        let mut s = format!("{}<timecycle>\n", xml::HEADER);
        s += &format!("  <cycle name=\"{}\" regions=\"{}\">\n", xml::esc(&self.name), self.region_count);
        for r in &self.regions {
            s += &format!("    <region name=\"{}\">\n", xml::esc(&r.name));
            for (name, values) in &r.entries {
                let v: Vec<String> = values.iter().map(f32::to_string).collect();
                s += &format!("      <{name}>{}</{name}>\n", v.join(" "));
            }
            s += "    </region>\n";
        }
        s + "  </cycle>\n</timecycle>\n"
    }
}

/// `WeatherCycleKeyframeRegion`: every parameter with one value per time sample.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct CycleRegion {
    pub name: String,
    /// Element name to its values. The names are what [`super::TimeCycleParams`] documents.
    pub entries: BTreeMap<String, Vec<f32>>,
}

impl CycleRegion {
    fn read(node: N) -> CycleRegion {
        let mut entries = BTreeMap::new();
        for e in node.children().filter(|c| c.is_element()) {
            // an element without numbers still has one value (zero) in the C#
            let mut values: Vec<f32> =
                xml::inner_text(e).split_ascii_whitespace().map(xml::parse_f32).collect();
            if values.is_empty() {
                values.push(0.0);
            }
            entries.insert(e.tag_name().name().to_string(), values);
        }
        CycleRegion { name: xml::attr_string(node, "name"), entries }
    }

    /// The value of `name` between sample `sample` and the one after it
    /// (`WeatherCycleKeyframeRegion.GetCurrentValue`): `current_blend` is the weight of `sample`. A
    /// sample past the end of the list uses the last, the one after the last is the first, and an entry
    /// the region does not have is zero.
    pub fn value_at(&self, name: &str, sample: usize, current_blend: f32) -> f32 {
        let Some(values) = self.entries.get(name).filter(|v| !v.is_empty()) else {
            return 0.0;
        };
        let sample = sample.min(values.len() - 1);
        let next = if sample < values.len() - 1 { sample + 1 } else { 0 };
        values[sample] * current_blend + values[next] * (1.0 - current_blend)
    }
}

/// A keyframe file that could not be used for a weather type.
#[derive(Debug, Clone, PartialEq)]
pub struct CycleProblem {
    pub weather: String,
    pub file: String,
    pub reason: String,
}

/// `weather.xml`.
#[derive(Debug, Clone, PartialEq, Default)]
pub struct Weather {
    pub gpu_fx: Vec<WeatherGpuFx>,
    /// In file order; a name listed twice keeps the later entry in the earlier one's place, as a .NET
    /// dictionary does.
    pub types: Vec<WeatherType>,
    pub cycles: Vec<WeatherCycle>,
}

impl Weather {
    /// Reads the file. The keyframes the types name are separate files: give them to
    /// [`load_cycles`](Self::load_cycles).
    pub fn parse(bytes: &[u8]) -> Result<Weather> {
        xml::with_root(bytes, |root| {
            let mut w = Weather::default();
            for n in xml::items(root, "WeatherGpuFx") {
                let g = WeatherGpuFx {
                    name: xml::child_string(n, "Name"),
                    system_type: xml::child_string(n, "SystemType"),
                    diffuse_name: xml::child_string(n, "diffuseName"),
                    distortion_texture: xml::child_string(n, "distortionTexture"),
                    diffuse_splash_name: xml::child_string(n, "diffuseSplashName"),
                    drive_type: xml::child_string(n, "driveType"),
                    wind_influence: xml::value_f32(n, "windInfluence"),
                    gravity: xml::value_f32(n, "gravity"),
                    emitter_settings_name: xml::child_string(n, "emitterSettingsName"),
                    render_settings_name: xml::child_string(n, "renderSettingsName"),
                };
                match w.gpu_fx.iter_mut().find(|x| x.name == g.name) {
                    Some(slot) => *slot = g,
                    None => w.gpu_fx.push(g),
                }
            }
            for n in xml::items(root, "WeatherTypes") {
                let t = WeatherType::read(n);
                match w.types.iter_mut().find(|x| x.name == t.name) {
                    Some(slot) => *slot = t,
                    None => w.types.push(t),
                }
            }
            for n in xml::items(root, "WeatherCycles") {
                w.cycles.push(WeatherCycle {
                    cycle_name: xml::child_string(n, "CycleName"),
                    time_mult: xml::value_f32(n, "TimeMult"),
                });
            }
            Ok(w)
        })
    }

    /// Finds and reads each type's keyframe file. `read` gets the name as the game writes it
    /// (`TimeCycleFilename`) and returns the file's bytes. A type whose file is missing or unreadable
    /// keeps no keyframes and is reported; the others are unaffected.
    pub fn load_cycles(&mut self, read: &mut dyn FnMut(&str) -> Option<Vec<u8>>) -> Vec<CycleProblem> {
        let mut problems = Vec::new();
        for t in &mut self.types {
            if t.time_cycle_filename.is_empty() {
                continue;
            }
            let mut problem = |reason: String| {
                problems.push(CycleProblem {
                    weather: t.name.clone(),
                    file: t.time_cycle_filename.clone(),
                    reason,
                })
            };
            match read(&t.time_cycle_filename) {
                None => problem("file not found".into()),
                Some(bytes) => match CycleData::parse(&bytes) {
                    Ok(Some(data)) => t.time_cycle = Some(data),
                    Ok(None) => problem("the file holds no cycle".into()),
                    Err(e) => problem(e.to_string()),
                },
            }
        }
        problems
    }

    /// The type called `name`: an exact match, else one that differs only in case.
    pub fn find(&self, name: &str) -> Option<&WeatherType> {
        self.types
            .iter()
            .find(|t| t.name == name)
            .or_else(|| self.types.iter().find(|t| t.name.eq_ignore_ascii_case(name)))
    }

    /// The keyframes of weather `name` for `region` (see [`WeatherType::region`]).
    pub fn region_of(&self, name: &str, region: &str) -> Option<&CycleRegion> {
        self.find(name)?.region(region)
    }

    /// A `weather.xml` [`parse`](Self::parse) reads back to the same types, effects and cycles (the
    /// keyframes are separate files).
    pub fn to_xml(&self) -> String {
        let mut s = format!("{}<CWeatherInfoMgr>\n  <WeatherGpuFx>\n", xml::HEADER);
        for g in &self.gpu_fx {
            s += "    <Item>\n";
            for (tag, v) in [
                ("Name", &g.name),
                ("SystemType", &g.system_type),
                ("diffuseName", &g.diffuse_name),
                ("distortionTexture", &g.distortion_texture),
                ("diffuseSplashName", &g.diffuse_splash_name),
                ("driveType", &g.drive_type),
            ] {
                s += &format!("      <{tag}>{}</{tag}>\n", xml::esc(v));
            }
            s += &format!("      <windInfluence value=\"{}\"/>\n", g.wind_influence);
            s += &format!("      <gravity value=\"{}\"/>\n", g.gravity);
            s += &format!(
                "      <emitterSettingsName>{}</emitterSettingsName>\n",
                xml::esc(&g.emitter_settings_name)
            );
            s += &format!(
                "      <renderSettingsName>{}</renderSettingsName>\n",
                xml::esc(&g.render_settings_name)
            );
            s += "    </Item>\n";
        }
        s += "  </WeatherGpuFx>\n  <WeatherTypes>\n";
        for t in &self.types {
            s += &format!("    <Item>\n      <Name>{}</Name>\n", xml::esc(&t.name));
            for (tag, v) in t.values() {
                s += &format!("      <{tag} value=\"{v}\"/>\n");
            }
            for (tag, v) in [
                ("OldSettingName", &t.old_setting_name),
                ("DropSettingName", &t.drop_setting_name),
                ("MistSettingName", &t.mist_setting_name),
                ("GroundSettingName", &t.ground_setting_name),
                ("TimeCycleFilename", &t.time_cycle_filename),
                ("CloudSettingsName", &t.cloud_settings_name),
            ] {
                s += &format!("      <{tag}>{}</{tag}>\n", xml::esc(v));
            }
            s += "    </Item>\n";
        }
        s += "  </WeatherTypes>\n  <WeatherCycles>\n";
        for c in &self.cycles {
            s += &format!(
                "    <Item>\n      <CycleName>{}</CycleName>\n      <TimeMult value=\"{}\"/>\n    </Item>\n",
                xml::esc(&c.cycle_name),
                c.time_mult
            );
        }
        s + "  </WeatherCycles>\n</CWeatherInfoMgr>\n"
    }
}
