//! The environment: the data that makes the world look like GTA V at a given time of day and weather.
//!
//! The renderer takes plain parameter structs; this module reads the game's files and computes them.
//!
//! | Module | Game file(s) (`common.rpf` is the base, `update/update.rpf/common` overrides it) | Encoding |
//! |---|---|---|
//! | [`timecycle`] | `data/levels/gta5/time.xml` | plain XML |
//! | [`mods`] | `data/timecycle/timecycle_mods_1..4.xml` (and the same in DLC packs) | plain XML |
//! | [`clouds`] | `data/clouds.xml`, `data/cloudkeyframes.xml` | plain XML |
//! | [`weather`] | `data/levels/gta5/weather.xml`, and the keyframe file each weather names (`data/timecycle/w_*.xml`) | plain XML |
//!
//! "Plain XML" means the C# reads the file with `RpfManager.GetFileXml`, which treats the bytes as UTF-8
//! text; every such file must therefore not be one of the game's binary containers (RBF, PSO, RSC7), and
//! these readers say so when one is. Nothing here has been checked against a real installation: the
//! layouts come from the C# source and are exercised by files this project writes itself.

pub mod clouds;
pub mod lighting;
pub mod mods;
pub mod timecycle;
pub mod weather;
mod xml;

#[cfg(test)]
mod tests;

pub use clouds::{CloudHatFrag, CloudHatLayer, CloudHatManager, CloudSettingsItem, CloudSettingsMap, Clouds};
pub use lighting::{GlobalLights, SkyParams};
pub use mods::{TimecycleMod, TimecycleMods};
pub use timecycle::{EvalQuery, SamplePosition, SunMoon, TimeCycleParams, TimeSample, Timecycle};
pub use weather::{CycleData, CycleProblem, CycleRegion, Weather, WeatherCycle, WeatherGpuFx, WeatherType};

/// Why an environment file could not be read.
#[derive(Debug, thiserror::Error)]
pub enum EnvError {
    #[error("the file is empty")]
    Empty,
    #[error("the file is {0} bytes, which is too large for this kind of file")]
    TooLarge(usize),
    #[error("this is a {0} file, not plain XML; only plain XML is read")]
    Binary(&'static str),
    #[error("not valid XML: {0}")]
    Xml(String),
    #[error("{0}")]
    Invalid(String),
}

pub type Result<T> = std::result::Result<T, EnvError>;
