use std::collections::BTreeSet;

use cw_core::hash::joaat_lower;
use glam::{EulerRot, Mat4, Quat, Vec3};
use serde::{Deserialize, Serialize};

pub type EntityId = u32;

#[derive(Debug, Clone, Copy, PartialEq, Serialize, Deserialize)]
pub struct Transform {
    pub position: Vec3,
    pub rotation: Quat,
    pub scale: Vec3,
}

impl Default for Transform {
    fn default() -> Self {
        Self { position: Vec3::ZERO, rotation: Quat::IDENTITY, scale: Vec3::ONE }
    }
}

impl Transform {
    pub fn matrix(&self) -> Mat4 {
        Mat4::from_scale_rotation_translation(self.scale, self.rotation, self.position)
    }

    /// Rotation as Z-Y-X Euler angles in degrees (yaw about Z, as GTA does).
    pub fn euler_degrees(&self) -> Vec3 {
        let (z, y, x) = self.rotation.to_euler(EulerRot::ZYX);
        Vec3::new(x.to_degrees(), y.to_degrees(), z.to_degrees())
    }

    pub fn with_euler_degrees(mut self, e: Vec3) -> Self {
        self.rotation = Quat::from_euler(EulerRot::ZYX, e.z.to_radians(), e.y.to_radians(), e.x.to_radians());
        self
    }

    /// Finite and non-degenerate: safe to render and to invert.
    pub fn is_valid(&self) -> bool {
        self.position.is_finite()
            && self.rotation.is_finite()
            && self.scale.is_finite()
            && self.scale.abs().min_element() > 1e-6
            && (self.rotation.length() - 1.0).abs() < 1e-3
    }
}

#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct Entity {
    pub id: EntityId,
    /// Archetype (model) name, e.g. `prop_bench_01`.
    pub archetype: String,
    /// `joaat` of the lower-cased archetype name: how the game refers to it.
    pub archetype_hash: u32,
    pub transform: Transform,
    /// Where it came from: a `.ymap` path, or empty for entities created here.
    #[serde(default)]
    pub source: String,
}

impl Entity {
    pub fn new(id: EntityId, archetype: &str, transform: Transform) -> Self {
        Self {
            id,
            archetype: archetype.to_string(),
            archetype_hash: joaat_lower(archetype),
            transform,
            source: String::new(),
        }
    }
}

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
pub struct Scene {
    pub name: String,
    pub entities: Vec<Entity>,
    pub(crate) next_id: EntityId,
    #[serde(skip)]
    pub selection: BTreeSet<EntityId>,
}

impl Scene {
    pub fn new(name: &str) -> Self {
        Self { name: name.to_string(), next_id: 1, ..Default::default() }
    }

    pub fn get(&self, id: EntityId) -> Option<&Entity> {
        self.entities.iter().find(|e| e.id == id)
    }

    pub(crate) fn get_mut(&mut self, id: EntityId) -> Option<&mut Entity> {
        self.entities.iter_mut().find(|e| e.id == id)
    }

    /// A fresh id, or `None` when the id space is exhausted (only reachable
    /// with a hostile file that already uses `u32::MAX`).
    pub fn alloc_id(&mut self) -> Option<EntityId> {
        let id = self.next_id.max(1);
        if id == EntityId::MAX || self.get(id).is_some() {
            return None;
        }
        self.next_id = id + 1;
        Some(id)
    }

    /// Axis-aligned bounds of the entity origins, `None` when empty.
    pub fn bounds(&self) -> Option<(Vec3, Vec3)> {
        let mut it = self.entities.iter().map(|e| e.transform.position);
        let first = it.next()?;
        Some(it.fold((first, first), |(lo, hi), p| (lo.min(p), hi.max(p))))
    }

    /// Repairs invariants after loading untrusted data.
    pub(crate) fn sanitize(&mut self) {
        self.entities.retain(|e| e.transform.is_valid());
        let mut seen = BTreeSet::new();
        self.entities.retain(|e| seen.insert(e.id));
        let highest = self.entities.iter().map(|e| e.id).max().unwrap_or(0);
        self.next_id = highest.max(self.next_id).saturating_add(1);
        self.selection.clear();
    }
}
