//! Built-in commands. Each module registers its own set.

use crate::engine::Engine;

mod files;
mod game;
mod mods;
mod rpf;
mod util;

pub fn register_all(e: &mut Engine) {
    files::register(e);
    game::register(e);
    mods::register(e);
    rpf::register(e);
    util::register(e);
}

pub(crate) mod args {
    use serde::de::DeserializeOwned;
    use serde_json::Value;

    use crate::error::CmdError;

    pub fn parse<T: DeserializeOwned>(v: Value) -> Result<T, CmdError> {
        serde_json::from_value(v).map_err(|e| CmdError::invalid(e.to_string()))
    }
}
