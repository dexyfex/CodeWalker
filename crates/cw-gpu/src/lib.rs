//! GPU side of the map viewport. The scene is drawn into an offscreen target
//! (with its own depth buffer and MSAA) and the UI composes that texture, so
//! the renderer never depends on how the host's render pass is set up.

pub mod camera;
pub mod pick;
mod renderer;

pub use camera::Camera;
pub use renderer::{BoxInstance, Globals, GpuScene, LineVertex, Renderer, Target, grid_spacing};
