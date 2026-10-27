//! Command layer of CodeWalker. The UI, the CLI, the JSON control channel and
//! the MCP server are thin shells over [`Engine::execute`].

pub mod commands;
pub mod engine;
pub mod error;
pub mod spec;
pub mod transport;

pub use engine::{Config, Ctx, Engine, Event, Session};
pub use error::{CmdError, CmdResult};
pub use spec::{CommandSpec, ParamKind, ParamSpec};
