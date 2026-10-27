//! The map-editing model. It knows nothing about windows or GPUs, so the UI,
//! the CLI, the JSON channel and MCP all edit the same [`World`].

mod history;
mod scene;

pub use glam::{Quat, Vec3};
pub use history::{Edit, Error, NewEntity, World};
pub use scene::{Entity, EntityId, Scene, Transform};
