//! Map data (`.ymap`): where the world's objects stand.
//!
//! A `.ymap` is a META resource ([`crate::meta`]) whose root is a `CMapData`. Its `entities` list
//! points at `CEntityDef` structures (and `CMloInstanceDef`s, which start with one): a model
//! name hash, a position, a rotation, a scale and a place in the level-of-detail tree.
//!
//! ```text
//! CMapData (512 bytes, block type = joaat("CMapData"))
//!   +0x08 u32 name hash   +0x0C u32 parent hash   +0x10 u32 flags (bit 0: scripted)
//!   +0x20 vec3 streaming extents min   +0x30 vec3 max   +0x40 vec3 entity extents min   +0x50 max
//!   +0x60 Array_StructurePointer entities
//! CEntityDef (128 bytes, block type = joaat("CEntityDef"))
//!   +0x08 u32 archetype hash   +0x0C u32 flags (bit 3: LOD parent is in another ymap)
//!   +0x10 u32 guid   +0x20 vec3 position   +0x30 quaternion x y z w   +0x40 f32 scale xy
//!   +0x44 f32 scale z   +0x48 i32 parent index   +0x4C f32 lod distance   +0x50 f32 child lod distance
//!   +0x54 u32 lod level   +0x58 u32 child count   +0x5C u32 priority
//! CMloInstanceDef (160 bytes, block type = joaat("CMloInstanceDef")): a CEntityDef, then interior data
//! ```
//!
//! The type names hash with [`crate::hash::joaat`] on the exact-case name (not the lower-cased
//! form used for file names); that matches the values in the C# `MetaNames`. Offsets are
//! transcribed from `MetaTypes.cs` and `YmapFile.cs`. Nothing here has been checked against real
//! game files; the layout is exercised by synthetic ones. Legacy RSC7 maps only: the Enhanced
//! Edition's `PSIN` maps are not read.

use crate::{
    error::{Error, Result},
    hash::joaat,
    meta::{Meta, MetaPointer, TYPE_POINTER, build_meta},
    rpf::ResourceData,
};

const CMAP_DATA: &str = "CMapData";
const CENTITY_DEF: &str = "CEntityDef";
const CMLO_INSTANCE_DEF: &str = "CMloInstanceDef";
const ENTITY_SIZE: usize = 128;
const MLO_SIZE: usize = 160;
const MAP_SIZE: usize = 512;
/// Entities per data block when writing: pointers carry a 20-bit offset into their block.
const PER_BLOCK: usize = 2048;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum LodLevel {
    Hd,
    Lod,
    Slod1,
    Slod2,
    Slod3,
    OrphanHd,
    Slod4,
    Other(u32),
}

impl LodLevel {
    pub fn from_raw(v: u32) -> Self {
        match v {
            0 => Self::Hd,
            1 => Self::Lod,
            2 => Self::Slod1,
            3 => Self::Slod2,
            4 => Self::Slod3,
            5 => Self::OrphanHd,
            6 => Self::Slod4,
            o => Self::Other(o),
        }
    }

    pub fn to_raw(self) -> u32 {
        match self {
            Self::Hd => 0,
            Self::Lod => 1,
            Self::Slod1 => 2,
            Self::Slod2 => 3,
            Self::Slod3 => 4,
            Self::OrphanHd => 5,
            Self::Slod4 => 6,
            Self::Other(o) => o,
        }
    }

    pub fn label(self) -> &'static str {
        match self {
            Self::Hd => "HD",
            Self::Lod => "LOD",
            Self::Slod1 => "SLOD1",
            Self::Slod2 => "SLOD2",
            Self::Slod3 => "SLOD3",
            Self::OrphanHd => "Orphan HD",
            Self::Slod4 => "SLOD4",
            Self::Other(_) => "Other",
        }
    }

    /// Detailed models you see up close, as opposed to the stand-ins for groups of them.
    pub fn is_detailed(self) -> bool {
        matches!(self, Self::Hd | Self::OrphanHd)
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct YmapEntity {
    /// Hash of the model's name (`joaat` of the lower-cased name).
    pub archetype: u32,
    pub flags: u32,
    pub guid: u32,
    pub position: [f32; 3],
    /// As stored: x, y, z, w. The orientation in the world is its inverse (an interior instance's
    /// is the stored one), see [`Self::orientation`].
    pub rotation: [f32; 4],
    pub scale_xy: f32,
    pub scale_z: f32,
    /// Index into the entity list of this file, or negative for none.
    pub parent_index: i32,
    pub lod_dist: f32,
    pub child_lod_dist: f32,
    pub lod: LodLevel,
    pub num_children: u32,
    pub priority: u32,
    /// An interior instance rather than a plain entity.
    pub mlo: bool,
}

impl Default for YmapEntity {
    fn default() -> Self {
        Self {
            archetype: 0,
            flags: 0,
            guid: 0,
            position: [0.0; 3],
            rotation: [0.0, 0.0, 0.0, 1.0],
            scale_xy: 1.0,
            scale_z: 1.0,
            parent_index: -1,
            lod_dist: 0.0,
            child_lod_dist: 0.0,
            lod: LodLevel::Hd,
            num_children: 0,
            priority: 0,
            mlo: false,
        }
    }
}

impl YmapEntity {
    /// The orientation to place the model with. A plain entity's is the stored quaternion
    /// inverted, unless it is the identity; an interior instance's is the stored one as it is
    /// (`YmapEntityDef` does both, and has the interior's inversion commented out). A unit
    /// quaternion's inverse is its conjugate. Never NaN: garbage becomes the identity.
    pub fn orientation(&self) -> [f32; 4] {
        const IDENTITY: [f32; 4] = [0.0, 0.0, 0.0, 1.0];
        let [x, y, z, w] = self.rotation;
        let len2 = x * x + y * y + z * z + w * w;
        if self.rotation == IDENTITY || !(len2.is_finite() && len2 > 1e-12) {
            return IDENTITY;
        }
        if self.mlo {
            let len = len2.sqrt();
            return [x / len, y / len, z / len, w / len];
        }
        [-x / len2, -y / len2, -z / len2, w / len2]
    }

    pub fn scale(&self) -> [f32; 3] {
        [self.scale_xy, self.scale_xy, self.scale_z]
    }

    /// Whether the LOD parent lives in another ymap (so `parent_index` means nothing here).
    pub fn lod_in_parent_ymap(&self) -> bool {
        self.flags & 8 != 0
    }
}

#[derive(Debug, Clone, Default, PartialEq)]
pub struct Ymap {
    pub name: u32,
    pub parent: u32,
    pub flags: u32,
    pub streaming_extents: ([f32; 3], [f32; 3]),
    pub entity_extents: ([f32; 3], [f32; 3]),
    pub entities: Vec<YmapEntity>,
}

impl Ymap {
    /// Whether entity `i` starts a LOD hierarchy in this file (it has no parent here, or its
    /// parent is not a coarser level). Same rule as `YmapFile.EnsureEntities`.
    pub fn is_lod_root(&self, i: usize) -> bool {
        let Some(d) = self.entities.get(i) else { return false };
        let parent = usize::try_from(d.parent_index).ok().and_then(|p| self.entities.get(p));
        match parent {
            None => true,
            Some(_) if d.lod_in_parent_ymap() => true,
            Some(p) => {
                p.lod.to_raw() <= d.lod.to_raw()
                    || (p.lod == LodLevel::OrphanHd && d.lod != LodLevel::OrphanHd)
            }
        }
    }

    /// How many entities sit at each LOD level, in level order. Values the game does not use
    /// share one row (`Other`, labelled with the first such value met), however many there are.
    pub fn lod_counts(&self) -> Vec<(LodLevel, usize)> {
        let mut known = [0usize; 7];
        let (mut other, mut other_count) = (None, 0usize);
        for e in &self.entities {
            match e.lod {
                LodLevel::Other(v) => {
                    other.get_or_insert(v);
                    other_count += 1;
                }
                l => known[l.to_raw() as usize] += 1,
            }
        }
        let mut counts: Vec<(LodLevel, usize)> = known
            .iter()
            .enumerate()
            .filter(|(_, n)| **n > 0)
            .map(|(i, n)| (LodLevel::from_raw(i as u32), *n))
            .collect();
        if let Some(v) = other {
            counts.push((LodLevel::Other(v), other_count));
        }
        counts
    }
}

fn f32_at(b: &[u8], at: usize) -> f32 {
    f32::from_le_bytes(b[at..at + 4].try_into().unwrap())
}

fn u32_at(b: &[u8], at: usize) -> u32 {
    u32::from_le_bytes(b[at..at + 4].try_into().unwrap())
}

fn vec3_at(b: &[u8], at: usize) -> [f32; 3] {
    [f32_at(b, at), f32_at(b, at + 4), f32_at(b, at + 8)]
}

/// Reads the `CEntityDef` at the start of `b` (at least 128 bytes).
fn entity_at(b: &[u8], mlo: bool) -> YmapEntity {
    YmapEntity {
        archetype: u32_at(b, 8),
        flags: u32_at(b, 12),
        guid: u32_at(b, 16),
        position: vec3_at(b, 32),
        rotation: [f32_at(b, 48), f32_at(b, 52), f32_at(b, 56), f32_at(b, 60)],
        scale_xy: f32_at(b, 64),
        scale_z: f32_at(b, 68),
        parent_index: u32_at(b, 72) as i32,
        lod_dist: f32_at(b, 76),
        child_lod_dist: f32_at(b, 80),
        lod: LodLevel::from_raw(u32_at(b, 84)),
        num_children: u32_at(b, 88),
        priority: u32_at(b, 92),
        mlo,
    }
}

pub fn parse_ymap(res: &ResourceData) -> Result<Ymap> {
    let meta = Meta::parse(res)?;
    let map = meta
        .first_of(joaat(CMAP_DATA))
        .filter(|b| b.data.len() >= 112)
        .ok_or_else(|| Error::InvalidRpf("not a map: no CMapData block".into()))?;
    let d = &map.data;

    // plain entities first, then interior instances: `parent_index` counts through both
    let pointers = meta.pointer_array(d, 96);
    let mut entities = Vec::with_capacity(pointers.len());
    for (type_name, size, mlo) in [(CENTITY_DEF, ENTITY_SIZE, false), (CMLO_INSTANCE_DEF, MLO_SIZE, true)] {
        let hash = joaat(type_name);
        entities.extend(
            pointers.iter().filter_map(|p| meta.structure(*p, hash, size)).map(|b| entity_at(b, mlo)),
        );
    }

    Ok(Ymap {
        name: u32_at(d, 8),
        parent: u32_at(d, 12),
        flags: u32_at(d, 16),
        streaming_extents: (vec3_at(d, 32), vec3_at(d, 48)),
        entity_extents: (vec3_at(d, 64), vec3_at(d, 80)),
        entities,
    })
}

fn put_vec3(b: &mut [u8], at: usize, v: [f32; 3]) {
    for (i, x) in v.iter().enumerate() {
        b[at + i * 4..at + i * 4 + 4].copy_from_slice(&x.to_le_bytes());
    }
}

fn write_entity(b: &mut [u8], e: &YmapEntity) {
    b[8..12].copy_from_slice(&e.archetype.to_le_bytes());
    b[12..16].copy_from_slice(&e.flags.to_le_bytes());
    b[16..20].copy_from_slice(&e.guid.to_le_bytes());
    put_vec3(b, 32, e.position);
    for (i, x) in e.rotation.iter().enumerate() {
        b[48 + i * 4..52 + i * 4].copy_from_slice(&x.to_le_bytes());
    }
    b[64..68].copy_from_slice(&e.scale_xy.to_le_bytes());
    b[68..72].copy_from_slice(&e.scale_z.to_le_bytes());
    b[72..76].copy_from_slice(&e.parent_index.to_le_bytes());
    b[76..80].copy_from_slice(&e.lod_dist.to_le_bytes());
    b[80..84].copy_from_slice(&e.child_lod_dist.to_le_bytes());
    b[84..88].copy_from_slice(&e.lod.to_raw().to_le_bytes());
    b[88..92].copy_from_slice(&e.num_children.to_le_bytes());
    b[92..96].copy_from_slice(&e.priority.to_le_bytes());
}

/// Writes a `.ymap` resource: the inverse of [`parse_ymap`]. Plain entities must come before
/// interior instances in `map.entities`, as they do after parsing. For tests and map tools.
///
/// # Panics
/// With more than 65,535 entities: the file format counts them in 16 bits.
pub fn build_ymap(map: &Ymap) -> ResourceData {
    assert!(
        map.entities.len() <= u16::MAX as usize,
        "a .ymap holds at most 65,535 entities, got {}",
        map.entities.len()
    );
    // blocks: 1 = CMapData, 2 = pointer list, 3.. = entity structures, PER_BLOCK to a block
    let mut blocks: Vec<(u32, Vec<u8>)> =
        vec![(joaat(CMAP_DATA), vec![0; MAP_SIZE]), (TYPE_POINTER, Vec::new())];
    let mut pointers = Vec::new();
    for mlo in [false, true] {
        let (hash, size) =
            if mlo { (joaat(CMLO_INSTANCE_DEF), MLO_SIZE) } else { (joaat(CENTITY_DEF), ENTITY_SIZE) };
        let group: Vec<&YmapEntity> = map.entities.iter().filter(|e| e.mlo == mlo).collect();
        for chunk in group.chunks(PER_BLOCK) {
            let block_id = blocks.len() as u32 + 1;
            let mut data = vec![0u8; chunk.len() * size];
            for (i, e) in chunk.iter().enumerate() {
                write_entity(&mut data[i * size..(i + 1) * size], e);
                pointers.push(MetaPointer { block: block_id, offset: (i * size) as u32 });
            }
            blocks.push((hash, data));
        }
    }
    blocks[1].1 = pointers.iter().flat_map(|p| p.to_raw().to_le_bytes()).collect();

    let d = &mut blocks[0].1;
    d[8..12].copy_from_slice(&map.name.to_le_bytes());
    d[12..16].copy_from_slice(&map.parent.to_le_bytes());
    d[16..20].copy_from_slice(&map.flags.to_le_bytes());
    put_vec3(d, 32, map.streaming_extents.0);
    put_vec3(d, 48, map.streaming_extents.1);
    put_vec3(d, 64, map.entity_extents.0);
    put_vec3(d, 80, map.entity_extents.1);
    d[96..104].copy_from_slice(&MetaPointer { block: 2, offset: 0 }.to_raw().to_le_bytes());
    let count = pointers.len() as u16;
    d[104..106].copy_from_slice(&count.to_le_bytes());
    d[106..108].copy_from_slice(&count.to_le_bytes());
    build_meta(1, &blocks)
}

#[cfg(test)]
mod tests;
