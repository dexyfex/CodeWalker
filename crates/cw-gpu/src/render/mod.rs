//! The 3-D renderer: meshes, materials, instancing, cameras and the passes that turn a draw list
//! into a picture. See "Renderer" in `docs/ARCHITECTURE.md` for how a caller draws a frame.

pub mod camera;
pub mod math;

pub use camera::{Camera, FlyCamera, FlyInput, OrbitCamera};
pub use math::{
    Aabb, Containment, Frustum, Plane, WorldRay, perspective_reversed_z, ray_aabb, ray_sphere, select_lod,
};
