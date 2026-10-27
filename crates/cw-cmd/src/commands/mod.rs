//! Built-in commands. Each module registers its own set.

use crate::engine::Engine;

mod check;
mod files;
mod game;
mod map;
mod mods;
mod rpf;
mod source;
mod texture;
mod util;
mod world;

pub fn register_all(e: &mut Engine) {
    check::register(e);
    files::register(e);
    game::register(e);
    map::register(e);
    mods::register(e);
    texture::register(e);
    rpf::register(e);
    util::register(e);
    world::register(e);
}

pub(crate) mod args {
    use serde::de::DeserializeOwned;
    use serde_json::Value;

    use crate::error::CmdError;

    pub fn parse<T: DeserializeOwned>(v: Value) -> Result<T, CmdError> {
        serde_json::from_value(v).map_err(|e| CmdError::invalid(e.to_string()))
    }
}
