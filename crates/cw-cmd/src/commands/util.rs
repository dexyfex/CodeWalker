use cw_core::hash;
use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine, Event},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new("hash.joaat", "tools", "Jenkins one-at-a-time hash of a string")
            .param(ParamSpec::new("text", K::String, "text to hash").required())
            .param(ParamSpec::new("lower", K::Boolean, "lower-case the text first (game convention)")),
        joaat,
    );
    e.register(
        CommandSpec::new("game.detect", "game", "Find GTA V installations (Steam, Epic, Rockstar, OpenIV)"),
        detect,
    );
    e.register(
        CommandSpec::new("commands.list", "app", "Every available command with its parameters"),
        commands_list,
    );
    e.register(CommandSpec::new("config.get", "app", "Current settings"), config_get);
    e.register(
        CommandSpec::new("config.set", "app", "Change settings")
            .param(ParamSpec::new("game_folder", K::Path, "GTA V folder"))
            .param(ParamSpec::new("enhanced_edition", K::Boolean, "Enhanced Edition executable"))
            .param(ParamSpec::new("reduce_motion", K::Boolean, "minimise UI animation"))
            .mutating(),
        config_set,
    );
}

#[derive(Deserialize)]
struct HashArgs {
    text: String,
    lower: Option<bool>,
}

fn joaat(_: &Ctx, args: Value) -> CmdResult {
    let a: HashArgs = parse(args)?;
    let h = if a.lower.unwrap_or(false) { hash::joaat_lower(&a.text) } else { hash::joaat(&a.text) };
    Ok(json!({ "hash": h, "hex": format!("0x{h:08X}"), "signed": h as i32 }))
}

fn detect(_: &Ctx, _: Value) -> CmdResult {
    Ok(serde_json::to_value(crate::gamefolder::detect())?)
}

fn commands_list(ctx: &Ctx, _: Value) -> CmdResult {
    Ok(serde_json::to_value(ctx.specs())?)
}

fn config_get(ctx: &Ctx, _: Value) -> CmdResult {
    Ok(serde_json::to_value(&ctx.session().config)?)
}

fn config_set(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        game_folder: Option<std::path::PathBuf>,
        enhanced_edition: Option<bool>,
        reduce_motion: Option<bool>,
    }
    let a: A = parse(args)?;
    let mut s = ctx.session_mut();
    if let Some(f) = a.game_folder {
        if !f.is_dir() {
            return Err(CmdError::not_found(format!("{} is not a folder", f.display())));
        }
        s.config.game_folder = Some(f);
    }
    if let Some(v) = a.enhanced_edition {
        s.config.enhanced_edition = v;
    }
    if let Some(v) = a.reduce_motion {
        s.config.reduce_motion = v;
    }
    let v = serde_json::to_value(&s.config)?;
    drop(s);
    ctx.save_config()?;
    ctx.emit(Event::ConfigChanged);
    Ok(v)
}
