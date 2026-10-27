//! Map data (`.ymap`) as commands: read one, or put its entities in the scene.

use cw_core::ymap::{Ymap, YmapEntity, parse_ymap};
use cw_world::{NewEntity, Quat, Transform, Vec3};
use serde::Deserialize;
use serde_json::{Value, json};

use super::{
    args::parse,
    source::{self, Loaded, Source},
};
use crate::{
    engine::{Ctx, Engine, Event},
    error::{CmdError, CmdResult},
    spec::{CommandSpec, ParamKind as K, ParamSpec},
};

fn with_source(spec: CommandSpec) -> CommandSpec {
    spec.param(ParamSpec::new("path", K::String, "entry path of a .ymap inside the open archives"))
        .param(ParamSpec::new("file", K::Path, "a .ymap file on disk (instead of `path`)"))
}

pub fn register(e: &mut Engine) {
    e.register(
        with_source(CommandSpec::new(
            "ymap.info",
            "map",
            "Summarise a .ymap: extents, entities per LOD level, and a page of its entities",
        ))
        .param(ParamSpec::new("offset", K::Integer, "first entity of the page (default 0)"))
        .param(ParamSpec::new("limit", K::Integer, "entities in the page (default 20, max 1000)")),
        info,
    );
    e.register(
        with_source(CommandSpec::new(
            "world.import",
            "map",
            "Place the entities of a .ymap in the scene, as one undo step",
        ))
        .param(ParamSpec::new(
            "lod",
            K::String,
            "`detailed` (HD and orphan HD, the default) or `all` levels of detail",
        ))
        .param(ParamSpec::new(
            "replace",
            K::Boolean,
            "swap the scene for this map instead of adding to it (undo brings the old scene back)",
        ))
        .mutating(),
        import,
    );
}

/// Reads and parses the map the arguments name; also says where it came from.
fn load_map(ctx: &Ctx, src: &Source) -> Result<(Ymap, String), CmdError> {
    let origin = match (&src.path, &src.file) {
        (Some(p), _) => p.clone(),
        (None, Some(f)) => f.display().to_string(),
        _ => String::new(),
    };
    match source::read(ctx, src)? {
        (Loaded::Resource(res), _) => Ok((parse_ymap(&res)?, origin)),
        (Loaded::Plain(data), name) if data.starts_with(b"PSIN") => Err(CmdError::invalid(format!(
            "{name} is a PSO map (Enhanced Edition); only the legacy resource format is read so far"
        ))),
        (Loaded::Plain(_), name) => {
            Err(CmdError::invalid(format!("{name} is not a map resource (no RSC7 header)")))
        }
    }
}

fn hex(v: u32) -> String {
    format!("0x{v:08X}")
}

/// How to show a model: its name when an open archive has a file called that, else its hash.
struct Models(Option<std::sync::Arc<cw_core::RpfTree>>);

impl Models {
    fn of(ctx: &Ctx) -> Self {
        Models(ctx.session().tree.clone())
    }

    fn name(&self, hash: u32) -> Option<&str> {
        self.0.as_ref()?.model_names().get(&hash).map(String::as_str)
    }

    fn label(&self, hash: u32) -> String {
        self.name(hash).map_or_else(|| hex(hash), str::to_string)
    }
}

fn lods_json(map: &Ymap) -> Value {
    map.lod_counts().into_iter().map(|(l, n)| json!({ "level": l.label(), "count": n })).collect()
}

fn info(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        #[serde(flatten)]
        src: Source,
        offset: Option<usize>,
        limit: Option<usize>,
    }
    let a: A = parse(args)?;
    let (map, origin) = load_map(ctx, &a.src)?;
    let (offset, limit) = (a.offset.unwrap_or(0), a.limit.unwrap_or(20).min(1000));
    let models = Models::of(ctx);
    let page: Vec<Value> = map
        .entities
        .iter()
        .enumerate()
        .skip(offset)
        .take(limit)
        .map(|(i, e)| {
            json!({
                "index": i, "archetype": hex(e.archetype), "model": models.name(e.archetype), "guid": e.guid,
                "x": e.position[0], "y": e.position[1], "z": e.position[2],
                "scale_xy": e.scale_xy, "scale_z": e.scale_z,
                "lod": e.lod.label(), "parent": e.parent_index, "interior": e.mlo,
                "lod_distance": e.lod_dist,
            })
        })
        .collect();
    let extents = |(lo, hi): ([f32; 3], [f32; 3])| json!({ "min": lo, "max": hi });
    Ok(json!({
        "source": origin,
        "name": hex(map.name), "parent": hex(map.parent), "flags": map.flags, "scripted": map.flags & 1 != 0,
        "streaming_extents": extents(map.streaming_extents),
        "entity_extents": extents(map.entity_extents),
        "entities": map.entities.len(),
        "interiors": map.entities.iter().filter(|e| e.mlo).count(),
        "lod_roots": (0..map.entities.len()).filter(|&i| map.is_lod_root(i)).count(),
        "lod": lods_json(&map),
        "page": { "offset": offset, "items": page },
    }))
}

/// Where and how an entity of the file stands in the scene.
fn transform_of(e: &YmapEntity) -> Transform {
    let [x, y, z, w] = e.orientation();
    let q = Quat::from_xyzw(x, y, z, w);
    let rotation = if q.is_finite() && q.length() > 1e-6 { q.normalize() } else { Quat::IDENTITY };
    Transform { position: Vec3::from(e.position), rotation, scale: Vec3::from(e.scale()) }
}

fn import(ctx: &Ctx, args: Value) -> CmdResult {
    #[derive(Deserialize)]
    struct A {
        #[serde(flatten)]
        src: Source,
        lod: Option<String>,
        replace: Option<bool>,
    }
    let a: A = parse(args)?;
    let all = match a.lod.as_deref().map(str::to_ascii_lowercase).as_deref() {
        None | Some("detailed") => false,
        Some("all") => true,
        Some(other) => return Err(CmdError::invalid(format!("unknown lod `{other}`; use detailed or all"))),
    };
    let (map, origin) = load_map(ctx, &a.src)?;
    let models = Models::of(ctx);

    let wanted: Vec<&YmapEntity> = map.entities.iter().filter(|e| all || e.lod.is_detailed()).collect();
    let left_out_by_lod = map.entities.len() - wanted.len();
    let items_named = wanted.iter().filter(|e| models.name(e.archetype).is_some()).count();
    let items: Vec<NewEntity> = wanted
        .into_iter()
        .map(|e| NewEntity {
            archetype: models.label(e.archetype),
            archetype_hash: e.archetype,
            transform: transform_of(e),
            source: origin.clone(),
        })
        .collect();

    let mut session = ctx.session_mut();
    let (ids, unplaceable) = if a.replace.unwrap_or(false) {
        session.world.replace_all(&origin, items)?
    } else {
        session.world.add_many(items)?
    };
    let scene_total = session.world.scene.entities.len();
    drop(session);
    ctx.emit(Event::WorldChanged);
    Ok(json!({
        "source": origin,
        "name": hex(map.name),
        "entities_in_file": map.entities.len(),
        "imported": ids.len(),
        "left_out": { "by_lod": left_out_by_lod, "unplaceable": unplaceable },
        "lod": lods_json(&map),
        "named": items_named,
        "scene_entities": scene_total,
        "first_id": ids.first(),
    }))
}
