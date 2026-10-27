//! The engine owns the application state and the command registry. Every
//! front-end (egui, CLI, JSON channel, MCP) calls [`Engine::execute`].

use std::{
    collections::BTreeMap,
    path::PathBuf,
    sync::{Arc, Mutex, RwLock, mpsc},
};

use cw_core::{GtaKeys, RpfTree};
use serde::{Deserialize, Serialize};
use serde_json::Value;

use crate::{
    error::{CmdError, CmdResult},
    spec::CommandSpec,
};

/// Persisted user settings.
#[derive(Debug, Clone, Default, Serialize, Deserialize)]
pub struct Config {
    pub game_folder: Option<PathBuf>,
    #[serde(default)]
    pub enhanced_edition: bool,
    #[serde(default)]
    pub recent: Vec<PathBuf>,
    #[serde(default)]
    pub reduce_motion: bool,
}

impl Config {
    pub fn path() -> Option<PathBuf> {
        dirs::config_dir().map(|d| d.join("CodeWalker").join("config.json"))
    }

    pub fn load() -> Self {
        Self::path()
            .and_then(|p| std::fs::read_to_string(p).ok())
            .and_then(|s| serde_json::from_str(&s).ok())
            .unwrap_or_default()
    }

    pub fn save(&self) -> std::io::Result<()> {
        let Some(path) = Self::path() else { return Ok(()) };
        if let Some(dir) = path.parent() {
            std::fs::create_dir_all(dir)?;
        }
        std::fs::write(path, serde_json::to_string_pretty(self).unwrap_or_default())
    }
}

#[derive(Default)]
pub struct Session {
    pub config: Config,
    pub keys: Option<Arc<GtaKeys>>,
    pub tree: Option<Arc<RpfTree>>,
    /// Folder that `tree` was scanned from, when it came from a game folder.
    pub root: Option<PathBuf>,
}

/// Something the UI (or any subscriber) may want to react to.
#[derive(Debug, Clone, Serialize)]
#[serde(tag = "event", rename_all = "snake_case")]
pub enum Event {
    Progress { task: String, message: String },
    TreeChanged,
    KeysChanged,
    ConfigChanged,
    Log { message: String },
}

pub struct Ctx<'a> {
    engine: &'a Engine,
}

impl Ctx<'_> {
    pub fn session(&self) -> std::sync::RwLockReadGuard<'_, Session> {
        self.engine.session.read().unwrap()
    }

    pub fn session_mut(&self) -> std::sync::RwLockWriteGuard<'_, Session> {
        self.engine.session.write().unwrap()
    }

    pub fn emit(&self, e: Event) {
        self.engine.emit(e);
    }

    /// Saves settings, unless the engine is ephemeral.
    pub fn save_config(&self) -> std::io::Result<()> {
        if self.engine.persist { self.session().config.save() } else { Ok(()) }
    }

    pub fn specs(&self) -> Vec<&CommandSpec> {
        self.engine.specs()
    }

    pub fn tree(&self) -> Result<Arc<RpfTree>, CmdError> {
        self.session()
            .tree
            .clone()
            .ok_or_else(|| CmdError::state("no archives are open; run `game.open` or `rpf.open` first"))
    }
}

type Handler = Box<dyn Fn(&Ctx, Value) -> CmdResult + Send + Sync>;

struct Registered {
    spec: CommandSpec,
    handler: Handler,
}

pub struct Engine {
    session: RwLock<Session>,
    commands: BTreeMap<&'static str, Registered>,
    subscribers: Mutex<Vec<mpsc::Sender<Event>>>,
    persist: bool,
}

impl Engine {
    /// Engine with every built-in command registered and the saved config loaded.
    pub fn new() -> Self {
        let mut e = Self::ephemeral();
        e.persist = true;
        e.session.write().unwrap().config = Config::load();
        e
    }

    /// Engine without commands or persisted config (tests, embedding).
    pub fn empty() -> Self {
        Self {
            session: RwLock::new(Session::default()),
            commands: BTreeMap::new(),
            subscribers: Mutex::new(Vec::new()),
            persist: false,
        }
    }

    /// Every built-in command and default settings; nothing is ever written
    /// to the user's config folder. For tests and embedding.
    pub fn ephemeral() -> Self {
        let mut e = Self::empty();
        crate::commands::register_all(&mut e);
        e
    }

    pub fn register<F>(&mut self, spec: CommandSpec, handler: F)
    where
        F: Fn(&Ctx, Value) -> CmdResult + Send + Sync + 'static,
    {
        self.commands.insert(spec.name, Registered { spec, handler: Box::new(handler) });
    }

    pub fn specs(&self) -> Vec<&CommandSpec> {
        self.commands.values().map(|r| &r.spec).collect()
    }

    pub fn spec(&self, name: &str) -> Option<&CommandSpec> {
        self.commands.get(name).map(|r| &r.spec)
    }

    pub fn execute(&self, name: &str, args: Value) -> CmdResult {
        let reg = self
            .commands
            .get(name)
            .ok_or_else(|| CmdError::new("unknown_command", format!("no command named `{name}`")))?;
        let args = if args.is_null() { Value::Object(Default::default()) } else { args };
        if !args.is_object() {
            return Err(CmdError::invalid("arguments must be a JSON object"));
        }
        check_required(&reg.spec, &args)?;
        (reg.handler)(&Ctx { engine: self }, args)
    }

    pub fn session(&self) -> std::sync::RwLockReadGuard<'_, Session> {
        self.session.read().unwrap()
    }

    pub fn subscribe(&self) -> mpsc::Receiver<Event> {
        let (tx, rx) = mpsc::channel();
        self.subscribers.lock().unwrap().push(tx);
        rx
    }

    fn emit(&self, e: Event) {
        self.subscribers.lock().unwrap().retain(|s| s.send(e.clone()).is_ok());
    }
}

impl Default for Engine {
    fn default() -> Self {
        Self::new()
    }
}

fn check_required(spec: &CommandSpec, args: &Value) -> Result<(), CmdError> {
    let obj = args.as_object().unwrap();
    for p in &spec.params {
        if p.required && !obj.contains_key(p.name) {
            return Err(CmdError::invalid(format!("missing required argument `{}`", p.name)));
        }
    }
    for k in obj.keys() {
        if !spec.params.iter().any(|p| p.name == k) {
            return Err(CmdError::invalid(format!("unknown argument `{k}` for `{}`", spec.name)));
        }
    }
    Ok(())
}
