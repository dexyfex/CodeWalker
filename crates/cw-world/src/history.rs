use std::path::Path;

use cw_core::hash::joaat_lower;

use crate::scene::{Entity, EntityId, Scene, Transform};

/// One undoable step. Each carries what is needed to run it in both directions.
#[derive(Debug, Clone, PartialEq)]
pub enum Edit {
    Add {
        entity: Entity,
        index: usize,
    },
    Remove {
        entity: Entity,
        index: usize,
    },
    Transform {
        id: EntityId,
        before: Transform,
        after: Transform,
    },
    Rename {
        id: EntityId,
        before: String,
        after: String,
    },
    /// Several edits that undo and redo together (an import).
    Batch(Vec<Edit>),
    /// The whole scene swapped for another: its name and entities before and after.
    Replace {
        before: Box<(String, Vec<Entity>)>,
        after: Box<(String, Vec<Entity>)>,
    },
}

/// An entity to add as part of a batch.
#[derive(Debug, Clone, PartialEq)]
pub struct NewEntity {
    /// The model's name, or a stand-in such as its hash in hex when only the hash is known.
    pub archetype: String,
    /// What the game calls the model; set here because it cannot always be derived from `archetype`.
    pub archetype_hash: u32,
    pub transform: Transform,
    /// Where it came from, e.g. the `.ymap` path.
    pub source: String,
}

#[derive(Debug, thiserror::Error)]
pub enum WorldError {
    #[error("no entity with id {0}")]
    NoEntity(EntityId),
    #[error("the transform is not finite or has a zero scale")]
    BadTransform,
    #[error("the scene has no free entity ids left")]
    Full,
    #[error("{0}")]
    Io(String),
}

/// A scene plus its undo/redo stacks. Every mutation goes through here.
#[derive(Debug, Default)]
pub struct World {
    pub scene: Scene,
    undo: Vec<Edit>,
    redo: Vec<Edit>,
    /// A drag in progress: the entity and its transform when the drag began.
    gesture: Option<(EntityId, Transform)>,
    dirty: bool,
    rev: u64,
}

impl World {
    pub fn new(name: &str) -> Self {
        Self { scene: Scene::new(name), ..Default::default() }
    }

    /// Starts over with an empty scene called `name`; history is dropped. The revision still moves
    /// forward, so a view that remembers the old one rebuilds.
    pub fn reset(&mut self, name: &str) {
        let rev = self.rev + 1;
        *self = World { scene: Scene::new(name), rev, ..Default::default() };
    }

    pub fn is_dirty(&self) -> bool {
        self.dirty
    }

    /// Changes whenever anything a view might draw changes (entities, their
    /// transforms, the selection). Views compare it to skip rebuilding.
    pub fn revision(&self) -> u64 {
        self.rev
    }

    pub fn can_undo(&self) -> bool {
        !self.undo.is_empty()
    }

    pub fn can_redo(&self) -> bool {
        !self.redo.is_empty()
    }

    pub fn undo_depth(&self) -> usize {
        self.undo.len()
    }

    fn commit(&mut self, e: Edit) {
        self.undo.push(e);
        self.redo.clear();
        self.dirty = true;
        self.rev += 1;
    }

    // ------------------------------------------------------------- editing

    pub fn add(&mut self, archetype: &str, transform: Transform) -> Result<EntityId, WorldError> {
        if !transform.is_valid() {
            return Err(WorldError::BadTransform);
        }
        let id = self.scene.alloc_id().ok_or(WorldError::Full)?;
        let entity = Entity::new(id, archetype, transform);
        let index = self.scene.entities.len();
        self.scene.entities.push(entity.clone());
        self.commit(Edit::Add { entity, index });
        Ok(id)
    }

    /// Turns the placeable items into entities with fresh ids. Returns them and how many items were
    /// left out (an invalid transform). Nothing is used up if the ids do not fit.
    fn make_entities(&mut self, items: Vec<NewEntity>) -> Result<(Vec<Entity>, usize), WorldError> {
        let total = items.len();
        let items: Vec<NewEntity> = items.into_iter().filter(|i| i.transform.is_valid()).collect();
        let skipped = total - items.len();
        let ids = self.scene.alloc_ids(items.len()).ok_or(WorldError::Full)?;
        let entities = ids
            .zip(items)
            .map(|(id, item)| Entity {
                id,
                archetype: item.archetype,
                archetype_hash: item.archetype_hash,
                transform: item.transform,
                source: item.source,
            })
            .collect();
        Ok((entities, skipped))
    }

    /// Adds many entities as one undoable step. Entities with an invalid transform are left out;
    /// returns the new ids and how many were left out. Nothing changes if ids run out.
    pub fn add_many(&mut self, items: Vec<NewEntity>) -> Result<(Vec<EntityId>, usize), WorldError> {
        let (entities, skipped) = self.make_entities(items)?;
        let ids: Vec<EntityId> = entities.iter().map(|e| e.id).collect();
        if entities.is_empty() {
            return Ok((ids, skipped));
        }
        let start = self.scene.entities.len();
        let edits: Vec<Edit> = entities
            .iter()
            .enumerate()
            .map(|(i, e)| Edit::Add { entity: e.clone(), index: start + i })
            .collect();
        self.scene.entities.extend(entities);
        self.commit(Edit::Batch(edits));
        Ok((ids, skipped))
    }

    /// Swaps the whole scene for a new one called `name` holding `items`, as one undoable step:
    /// undo brings the old name and entities back. Ids keep counting up, so nothing old is reused.
    pub fn replace_all(
        &mut self,
        name: &str,
        items: Vec<NewEntity>,
    ) -> Result<(Vec<EntityId>, usize), WorldError> {
        self.end_gesture();
        let (entities, skipped) = self.make_entities(items)?;
        let ids: Vec<EntityId> = entities.iter().map(|e| e.id).collect();
        let before = (
            std::mem::replace(&mut self.scene.name, name.to_string()),
            std::mem::take(&mut self.scene.entities),
        );
        let after = (self.scene.name.clone(), entities.clone());
        self.scene.entities = entities;
        self.scene.selection.clear();
        self.commit(Edit::Replace { before: Box::new(before), after: Box::new(after) });
        Ok((ids, skipped))
    }

    pub fn remove(&mut self, id: EntityId) -> Result<(), WorldError> {
        let index = self.scene.entities.iter().position(|e| e.id == id).ok_or(WorldError::NoEntity(id))?;
        let entity = self.scene.entities.remove(index);
        self.scene.selection.remove(&id);
        self.commit(Edit::Remove { entity, index });
        Ok(())
    }

    /// Atomic transform change (one undo step). An invalid transform changes nothing.
    pub fn set_transform(&mut self, id: EntityId, t: Transform) -> Result<(), WorldError> {
        self.begin_gesture(id)?;
        if let Err(e) = self.update_gesture(t) {
            self.cancel_gesture();
            return Err(e);
        }
        self.end_gesture();
        Ok(())
    }

    /// Starts a drag; returns the transform to restore on cancel.
    pub fn begin_gesture(&mut self, id: EntityId) -> Result<Transform, WorldError> {
        self.end_gesture();
        let t = self.scene.get(id).ok_or(WorldError::NoEntity(id))?.transform;
        self.gesture = Some((id, t));
        Ok(t)
    }

    /// Moves the entity during a drag without growing the undo stack.
    pub fn update_gesture(&mut self, t: Transform) -> Result<(), WorldError> {
        let (id, _) = self.gesture.ok_or(WorldError::BadTransform)?;
        if !t.is_valid() {
            return Err(WorldError::BadTransform);
        }
        self.scene.get_mut(id).ok_or(WorldError::NoEntity(id))?.transform = t;
        self.rev += 1;
        Ok(())
    }

    /// Ends the drag, recording a single undo step when something changed.
    pub fn end_gesture(&mut self) {
        if let Some((id, before)) = self.gesture.take()
            && let Some(e) = self.scene.get(id)
            && e.transform != before
        {
            let after = e.transform;
            self.commit(Edit::Transform { id, before, after });
        }
    }

    pub fn cancel_gesture(&mut self) {
        if let Some((id, before)) = self.gesture.take()
            && let Some(e) = self.scene.get_mut(id)
        {
            e.transform = before;
            self.rev += 1;
        }
    }

    pub fn rename(&mut self, id: EntityId, archetype: &str) -> Result<(), WorldError> {
        let e = self.scene.get_mut(id).ok_or(WorldError::NoEntity(id))?;
        if e.archetype == archetype {
            return Ok(());
        }
        let before = std::mem::replace(&mut e.archetype, archetype.to_string());
        e.archetype_hash = joaat_lower(archetype);
        self.commit(Edit::Rename { id, before, after: archetype.to_string() });
        Ok(())
    }

    // ------------------------------------------------------------ history

    pub fn undo(&mut self) -> bool {
        self.end_gesture();
        let Some(e) = self.undo.pop() else { return false };
        self.apply(&e, true);
        self.redo.push(e);
        self.dirty = true;
        self.rev += 1;
        true
    }

    pub fn redo(&mut self) -> bool {
        self.end_gesture();
        let Some(e) = self.redo.pop() else { return false };
        self.apply(&e, false);
        self.undo.push(e);
        self.dirty = true;
        self.rev += 1;
        true
    }

    fn apply(&mut self, edit: &Edit, reverse: bool) {
        match (edit, reverse) {
            (Edit::Add { entity, index }, false) | (Edit::Remove { entity, index }, true) => {
                let at = (*index).min(self.scene.entities.len());
                self.scene.entities.insert(at, entity.clone());
            }
            (Edit::Add { entity, .. }, true) | (Edit::Remove { entity, .. }, false) => {
                self.scene.entities.retain(|e| e.id != entity.id);
                self.scene.selection.remove(&entity.id);
            }
            (Edit::Transform { id, before, after }, rev) => {
                if let Some(e) = self.scene.get_mut(*id) {
                    e.transform = if rev { *before } else { *after };
                }
            }
            (Edit::Batch(edits), false) => {
                for e in edits {
                    self.apply(e, false);
                }
            }
            (Edit::Batch(edits), true) => {
                // undoing a long run of additions one by one would rescan the scene each time
                let added: std::collections::HashSet<EntityId> = edits
                    .iter()
                    .filter_map(|e| if let Edit::Add { entity, .. } = e { Some(entity.id) } else { None })
                    .collect();
                if added.len() == edits.len() {
                    self.scene.entities.retain(|e| !added.contains(&e.id));
                    self.scene.selection.retain(|id| !added.contains(id));
                } else {
                    for e in edits.iter().rev() {
                        self.apply(e, true);
                    }
                }
            }
            (Edit::Replace { before, after }, rev) => {
                let (name, entities) = if rev { &**before } else { &**after };
                self.scene.name = name.clone();
                self.scene.entities = entities.clone();
                self.scene.selection.clear();
            }
            (Edit::Rename { id, before, after }, rev) => {
                if let Some(e) = self.scene.get_mut(*id) {
                    e.archetype = if rev { before.clone() } else { after.clone() };
                    e.archetype_hash = joaat_lower(&e.archetype);
                }
            }
        }
    }

    // ----------------------------------------------------------- selection

    pub fn select(&mut self, ids: impl IntoIterator<Item = EntityId>, additive: bool) {
        if !additive {
            self.scene.selection.clear();
        }
        let existing: Vec<EntityId> = ids.into_iter().filter(|i| self.scene.get(*i).is_some()).collect();
        self.scene.selection.extend(existing);
        self.rev += 1;
    }

    pub fn clear_selection(&mut self) {
        self.scene.selection.clear();
        self.rev += 1;
    }

    // --------------------------------------------------------- persistence

    pub fn to_json(&self) -> String {
        serde_json::to_string_pretty(&self.scene).unwrap_or_default()
    }

    /// Replaces the scene with one parsed from untrusted JSON. History restarts.
    pub fn load_json(&mut self, text: &str) -> Result<(), WorldError> {
        let mut scene: Scene = serde_json::from_str(text).map_err(|e| WorldError::Io(e.to_string()))?;
        scene.sanitize();
        let rev = self.rev + 1;
        *self = World { scene, rev, ..Default::default() };
        Ok(())
    }

    pub fn save(&mut self, path: &Path) -> Result<(), WorldError> {
        std::fs::write(path, self.to_json()).map_err(|e| WorldError::Io(e.to_string()))?;
        self.dirty = false;
        Ok(())
    }

    pub fn load(&mut self, path: &Path) -> Result<(), WorldError> {
        let text = std::fs::read_to_string(path).map_err(|e| WorldError::Io(e.to_string()))?;
        self.load_json(&text)
    }
}

pub use WorldError as Error;

#[cfg(test)]
mod tests;
