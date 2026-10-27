//! Map editing as commands. The UI's viewport, the CLI and MCP all go through these.

use std::path::PathBuf;

use cw_world::{Error as WorldError, Transform, Vec3, World};
use serde::Deserialize;
use serde_json::{Value, json};

use super::args::parse;
use crate::{
    engine::{Ctx, Engine, Event},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

impl From<WorldError> for CmdError {
    fn from(e: WorldError) -> Self {
        let code = match &e {
            WorldError::NoEntity(_) => "not_found",
            WorldError::BadTransform => "invalid_args",
            WorldError::Io(_) => "io",
            WorldError::Full => "bad_state",
        };
        CmdError::new(code, e.to_string())
    }
}

fn xyz(spec: CommandSpec, with_name: bool) -> CommandSpec {
    let mut s = spec;
    if with_name {
        s = s.param(ParamSpec::new("archetype", K::String, "model name, e.g. prop_bench_01").required());
    }
    for (n, d) in [
        ("x", "position x"),
        ("y", "position y"),
        ("z", "position z"),
        ("rx", "rotation about X, degrees"),
        ("ry", "rotation about Y, degrees"),
        ("rz", "rotation about Z (heading), degrees"),
        ("sx", "scale x"),
        ("sy", "scale y"),
        ("sz", "scale z"),
    ] {
        s = s.param(ParamSpec::new(n, K::Number, d));
    }
    s
}

pub fn register(e: &mut Engine) {
    e.register(
        CommandSpec::new("world.new", "world", "Start an empty scene")
            .param(ParamSpec::new("name", K::String, "scene name"))
            .mutating(),
        new_scene,
    );
    e.register(xyz(CommandSpec::new("world.add", "world", "Add an entity").mutating(), true), add);
    e.register(
        xyz(
            CommandSpec::new(
                "world.set",
                "world",
                "Change an entity's position, rotation or scale (omitted values stay)",
            )
            .param(ParamSpec::new("id", K::Integer, "entity id").required())
            .mutating(),
            false,
        ),
        set,
    );
    e.register(
        CommandSpec::new("world.rename", "world", "Change an entity's model")
            .param(ParamSpec::new("id", K::Integer, "entity id").required())
            .param(ParamSpec::new("archetype", K::String, "new model name").required())
            .mutating(),
        rename,
    );
    e.register(
        CommandSpec::new("world.remove", "world", "Delete an entity")
            .param(ParamSpec::new("id", K::Integer, "entity id").required())
            .mutating(),
        remove,
    );
    e.register(
        CommandSpec::new("world.select", "world", "Select an entity (no id clears the selection)")
            .param(ParamSpec::new("id", K::Integer, "entity id"))
            .param(ParamSpec::new("additive", K::Boolean, "add to the selection"))
            .mutating(),
        select,
    );
    e.register(
        CommandSpec::new("world.list", "world", "List entities").param(ParamSpec::new(
            "limit",
            K::Integer,
            "max rows (default 500)",
        )),
        list,
    );
    e.register(CommandSpec::new("world.stats", "world", "Scene summary and undo state"), stats);
    e.register(CommandSpec::new("world.undo", "world", "Undo the last edit").mutating(), undo);
    e.register(CommandSpec::new("world.redo", "world", "Redo the last undone edit").mutating(), redo);
    e.register(
        CommandSpec::new("world.save", "world", "Save the scene as JSON")
            .param(ParamSpec::new("path", K::Path, "file to write").required())
            .mutating(),
        save,
    );
    e.register(
        CommandSpec::new("world.load", "world", "Load a scene saved with world.save")
            .param(ParamSpec::new("path", K::Path, "file to read").required())
            .mutating(),
        load,
    );
}

fn with_world<R>(ctx: &Ctx, f: impl FnOnce(&mut World) -> Result<R, CmdError>) -> Result<R, CmdError> {
    let r = f(&mut ctx.session_mut().world);
    if r.is_ok() {
        ctx.emit(Event::WorldChanged);
    }
    r
}

#[derive(Deserialize, Default)]
struct Pose {
    x: Option<f32>,
    y: Option<f32>,
    z: Option<f32>,
    rx: Option<f32>,
    ry: Option<f32>,
    rz: Option<f32>,
    sx: Option<f32>,
    sy: Option<f32>,
    sz: Option<f32>,
}

impl Pose {
    /// Applies the given fields over `base`.
    fn apply(&self, mut t: Transform) -> Transform {
        let p = t.position;
        t.position = Vec3::new(self.x.unwrap_or(p.x), self.y.unwrap_or(p.y), self.z.unwrap_or(p.z));
        let s = t.scale;
        t.scale = Vec3::new(self.sx.unwrap_or(s.x), self.sy.unwrap_or(s.y), self.sz.unwrap_or(s.z));
        if self.rx.is_some() || self.ry.is_some() || self.rz.is_some() {
            let e = t.euler_degrees();
            t = t.with_euler_degrees(Vec3::new(
                self.rx.unwrap_or(e.x),
                self.ry.unwrap_or(e.y),
                self.rz.unwrap_or(e.z),
            ));
        }
        t
    }
}

fn entity_json(e: &cw_world::Entity, selected: bool) -> Value {
    let t = e.transform;
    let r = t.euler_degrees();
    json!({
        "id": e.id, "archetype": e.archetype, "hash": format!("0x{:08X}", e.archetype_hash),
        "x": t.position.x, "y": t.position.y, "z": t.position.z,
        "rx": r.x, "ry": r.y, "rz": r.z,
        "sx": t.scale.x, "sy": t.scale.y, "sz": t.scale.z,
        "source": e.source, "selected": selected,
    })
}

fn new_scene(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        name: Option<String>,
    }
    let a: A = parse(args)?;
    with_world(ctx, |w| {
        *w = World::new(a.name.as_deref().unwrap_or("Scratch scene"));
        Ok(json!({ "name": w.scene.name }))
    })
}

fn add(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        archetype: String,
        #[serde(flatten)]
        pose: Pose,
    }
    let a: A = parse(args)?;
    if a.archetype.trim().is_empty() {
        return Err(CmdError::invalid("archetype is empty"));
    }
    with_world(ctx, |w| {
        let id = w.add(a.archetype.trim(), a.pose.apply(Transform::default()))?;
        Ok(entity_json(w.scene.get(id).expect("just added"), false))
    })
}

fn set(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        id: u32,
        #[serde(flatten)]
        pose: Pose,
    }
    let a: A = parse(args)?;
    with_world(ctx, |w| {
        let cur = w.scene.get(a.id).ok_or(WorldError::NoEntity(a.id))?.transform;
        w.set_transform(a.id, a.pose.apply(cur))?;
        Ok(entity_json(w.scene.get(a.id).expect("exists"), w.scene.selection.contains(&a.id)))
    })
}

fn rename(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        id: u32,
        archetype: String,
    }
    let a: A = parse(args)?;
    if a.archetype.trim().is_empty() {
        return Err(CmdError::invalid("archetype is empty"));
    }
    with_world(ctx, |w| {
        w.rename(a.id, a.archetype.trim())?;
        Ok(entity_json(w.scene.get(a.id).expect("exists"), w.scene.selection.contains(&a.id)))
    })
}

fn remove(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        id: u32,
    }
    let a: A = parse(args)?;
    with_world(ctx, |w| {
        w.remove(a.id)?;
        Ok(json!({ "removed": a.id }))
    })
}

fn select(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        id: Option<u32>,
        additive: Option<bool>,
    }
    let a: A = parse(args)?;
    with_world(ctx, |w| {
        match a.id {
            Some(id) => {
                if w.scene.get(id).is_none() {
                    return Err(WorldError::NoEntity(id).into());
                }
                w.select([id], a.additive.unwrap_or(false));
            }
            None => w.clear_selection(),
        }
        Ok(json!({ "selection": w.scene.selection.iter().collect::<Vec<_>>() }))
    })
}

fn list(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        limit: Option<usize>,
    }
    let a: A = parse(args)?;
    let s = ctx.session();
    let sc = &s.world.scene;
    let rows: Vec<Value> = sc
        .entities
        .iter()
        .take(a.limit.unwrap_or(500))
        .map(|e| entity_json(e, sc.selection.contains(&e.id)))
        .collect();
    Ok(json!({ "total": sc.entities.len(), "entities": rows }))
}

fn stats(ctx: &Ctx, _: Value) -> CmdResult {
    let s = ctx.session();
    let w = &s.world;
    Ok(json!({
        "name": w.scene.name,
        "entities": w.scene.entities.len(),
        "selected": w.scene.selection.len(),
        "can_undo": w.can_undo(),
        "can_redo": w.can_redo(),
        "dirty": w.is_dirty(),
    }))
}

fn undo(ctx: &Ctx, _: Value) -> CmdResult {
    with_world(ctx, |w| Ok(json!({ "undone": w.undo(), "can_undo": w.can_undo(), "can_redo": w.can_redo() })))
}

fn redo(ctx: &Ctx, _: Value) -> CmdResult {
    with_world(ctx, |w| Ok(json!({ "redone": w.redo(), "can_undo": w.can_undo(), "can_redo": w.can_redo() })))
}

#[derive(Deserialize)]
struct PathArg {
    path: PathBuf,
}

fn save(ctx: &Ctx, args: Value) -> CmdResult {
    let a: PathArg = parse(args)?;
    with_world(ctx, |w| {
        w.save(&a.path)?;
        Ok(json!({ "saved": a.path, "entities": w.scene.entities.len() }))
    })
}

fn load(ctx: &Ctx, args: Value) -> CmdResult {
    let a: PathArg = parse(args)?;
    with_world(ctx, |w| {
        w.load(&a.path)?;
        Ok(json!({ "loaded": a.path, "entities": w.scene.entities.len() }))
    })
}
