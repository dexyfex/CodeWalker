//! Application state and the glue between the engine, background threads and
//! the views in `ui/`.

use std::{
    path::PathBuf,
    sync::{
        Arc,
        mpsc::{self, Receiver, Sender},
    },
    thread,
};

use cw_cmd::{Engine, Event};
use cw_core::EntryRef;
use eframe::egui::{self, Key, Modifiers, TextureHandle};
use serde_json::{Value, json};

use crate::{
    actions::Action,
    format,
    model::{Explorer, Item},
    preview::{self, Loaded, Source},
    ui::{
        palette::Palette,
        toasts::{Toast, ToastKind},
    },
};

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Mode {
    Explorer,
    Mods,
}

#[derive(Clone, Debug, PartialEq, Eq)]
pub struct ModRow {
    pub path: String,
    pub size: u64,
    pub enabled: bool,
    pub archive: bool,
    pub overrides_game: bool,
}

#[derive(Default)]
pub struct ModsState {
    pub loaded: bool,
    pub dir: Option<PathBuf>,
    pub dir_exists: bool,
    pub rows: Vec<ModRow>,
    pub confirm_remove: Option<String>,
}

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum ViewMode {
    Auto,
    Hex,
    Info,
}

#[derive(Clone, PartialEq, Eq, Debug)]
pub enum Target {
    Entry(EntryRef),
    Disk(PathBuf),
}

pub enum PState {
    Idle,
    Loading,
    Ready(Box<Loaded>),
    Failed(String),
}

pub struct PreviewSlot {
    pub target: Option<Target>,
    pub generation: u64,
    pub state: PState,
    pub view: ViewMode,
    pub tex: Option<(TextureHandle, (u32, u32))>,
}

#[derive(Clone, Copy, PartialEq, Eq, Debug)]
pub enum Pick {
    GameFolder,
    Files,
    ExtractTo,
    InstallFiles,
    InstallFolder,
}

pub enum Msg {
    Cmd { cmd: &'static str, result: Result<Value, cw_cmd::CmdError> },
    Preview { generation: u64, result: Result<Box<Loaded>, String> },
    Search { generation: u64, query: String, total: usize, items: Vec<EntryRef> },
    Picked(Pick, Vec<PathBuf>),
}

pub struct App {
    pub engine: Arc<Engine>,
    events: Receiver<Event>,
    tx: Sender<Msg>,
    rx: Receiver<Msg>,
    pub ctx: egui::Context,

    pub mode: Mode,
    pub mods: ModsState,
    pub ex: Explorer,
    pub loose: Vec<PathBuf>,
    pub preview: PreviewSlot,
    pub keys_loaded: bool,
    pub candidates: Vec<cw_cmd::gamefolder::Candidate>,

    pub busy: Option<String>,
    pub progress: Option<String>,
    pub toasts: Vec<Toast>,
    pub palette: Option<Palette>,

    pub search_text: String,
    pub search_everywhere: bool,
    search_generation: u64,
    pub focus_search: bool,
    pub tree_offset: f32,
    pub list_offset: f32,
    pub tree_scroll_to: Option<usize>,
    pub list_scroll_to: Option<usize>,
    last_current: Option<crate::model::NodeId>,
    pub quit: bool,
    pub config_game_folder: Option<PathBuf>,
    /// Set by a context menu: the selection is applied on the next frame.
    pub pending_mods_copy: bool,
    shot: Option<Shot>,
    script: Option<Script>,
}

/// Developer aid for screenshots: applied once, when the first tree arrives.
/// `CODEWALKER_SELECT=path`, `CODEWALKER_VIEW=hex|info`, `CODEWALKER_PALETTE=1`,
/// `CODEWALKER_SEARCH=text`, `CODEWALKER_SEARCH_ALL=1`.
struct Script {
    select: Option<String>,
    view: ViewMode,
    palette: bool,
    search: Option<String>,
    everywhere: bool,
    mods: bool,
}

impl Script {
    fn from_env() -> Option<Self> {
        let var = |k: &str| std::env::var(k).ok();
        let s = Self {
            select: var("CODEWALKER_SELECT"),
            view: match var("CODEWALKER_VIEW").as_deref() {
                Some("hex") => ViewMode::Hex,
                Some("info") => ViewMode::Info,
                _ => ViewMode::Auto,
            },
            palette: var("CODEWALKER_PALETTE").is_some(),
            search: var("CODEWALKER_SEARCH"),
            everywhere: var("CODEWALKER_SEARCH_ALL").is_some(),
            mods: var("CODEWALKER_MODE").as_deref() == Some("mods"),
        };
        (s.select.is_some() || s.palette || s.search.is_some() || s.mods || s.view != ViewMode::Auto)
            .then_some(s)
    }
}

/// Developer aid: `CODEWALKER_SCREENSHOT=out.png` saves a frame and exits.
struct Shot {
    path: PathBuf,
    after_frames: u32,
    frame: u32,
    requested: bool,
    done: Arc<std::sync::atomic::AtomicBool>,
}

impl App {
    pub fn new(cc: &eframe::CreationContext<'_>, engine: Arc<Engine>, startup: Vec<PathBuf>) -> Self {
        crate::theme::install(&cc.egui_ctx);
        let (tx, rx) = mpsc::channel();
        let events = engine.subscribe();
        let config_game_folder = engine.session().config.game_folder.clone();
        let mut app = Self {
            events,
            tx,
            rx,
            ctx: cc.egui_ctx.clone(),
            mode: Mode::Explorer,
            mods: ModsState::default(),
            ex: Explorer::default(),
            loose: Vec::new(),
            preview: PreviewSlot {
                target: None,
                generation: 0,
                state: PState::Idle,
                view: ViewMode::Auto,
                tex: None,
            },
            keys_loaded: false,
            candidates: Vec::new(),
            busy: None,
            progress: None,
            toasts: Vec::new(),
            palette: None,
            search_text: String::new(),
            search_everywhere: false,
            search_generation: 0,
            focus_search: false,
            tree_offset: 0.0,
            list_offset: 0.0,
            tree_scroll_to: None,
            list_scroll_to: None,
            last_current: None,
            quit: false,
            config_game_folder,
            pending_mods_copy: false,
            script: Script::from_env(),
            shot: std::env::var_os("CODEWALKER_SCREENSHOT").map(|p| Shot {
                path: PathBuf::from(p),
                after_frames: std::env::var("CODEWALKER_SCREENSHOT_FRAMES")
                    .ok()
                    .and_then(|v| v.parse().ok())
                    .unwrap_or(30),
                frame: 0,
                requested: false,
                done: Arc::new(std::sync::atomic::AtomicBool::new(false)),
            }),
            engine,
        };

        if startup.is_empty() {
            match app.config_game_folder.clone().filter(|p| p.is_dir()) {
                Some(folder) => app.run("Scanning game files…", "game.open", json!({ "folder": folder })),
                None => app.detect(),
            }
        } else {
            for p in startup {
                app.open_path(p);
            }
        }
        app
    }

    // --------------------------------------------------------- background

    fn spawn(&self, work: impl FnOnce() -> Msg + Send + 'static) {
        let (tx, ctx) = (self.tx.clone(), self.ctx.clone());
        thread::spawn(move || {
            let _ = tx.send(work());
            ctx.request_repaint();
        });
    }

    /// Runs an engine command on a worker thread; the result arrives as `Msg::Cmd`.
    pub fn run(&mut self, what: &str, cmd: &'static str, args: Value) {
        self.busy = Some(what.to_string());
        let engine = self.engine.clone();
        self.spawn(move || Msg::Cmd { cmd, result: engine.execute(cmd, args) });
    }

    fn detect(&mut self) {
        let engine = self.engine.clone();
        self.spawn(move || Msg::Cmd {
            cmd: "game.detect",
            result: engine.execute("game.detect", Value::Null),
        });
    }

    pub fn pick(&self, what: Pick) {
        self.spawn(move || {
            let dlg = rfd::FileDialog::new();
            let paths = match what {
                Pick::GameFolder | Pick::ExtractTo | Pick::InstallFolder => {
                    dlg.pick_folder().into_iter().collect()
                }
                Pick::Files | Pick::InstallFiles => dlg.pick_files().unwrap_or_default(),
            };
            Msg::Picked(what, paths)
        });
    }

    /// Opens a dropped/argument path: folders and `.rpf` archives join the
    /// tree, anything else is previewed as a loose file.
    pub fn open_path(&mut self, p: PathBuf) {
        let is_archive = p.extension().is_some_and(|e| e.eq_ignore_ascii_case("rpf"));
        if p.is_dir() {
            if cw_cmd::gamefolder::validate(&p).is_some() {
                self.run("Scanning game files…", "game.open", json!({ "folder": p }));
            } else {
                let add = !self.ex.is_empty();
                self.run("Scanning archives…", "rpf.open", json!({ "path": p, "add": add }));
            }
        } else if is_archive {
            let add = !self.ex.is_empty();
            self.run("Opening archive…", "rpf.open", json!({ "path": p, "add": add }));
        } else {
            self.run("Opening file…", "file.open", json!({ "path": p }));
        }
    }

    pub fn toast(&mut self, text: impl Into<String>, kind: ToastKind) {
        self.toasts.push(Toast::new(text.into(), kind, self.ctx.input(|i| i.time)));
    }

    // ------------------------------------------------------------- pumping

    /// Applies everything the background threads and the engine produced.
    fn pump(&mut self) {
        while let Ok(ev) = self.events.try_recv() {
            match ev {
                Event::TreeChanged => {
                    let tree = self.engine.session().tree.clone();
                    self.ex.set_tree(tree);
                    self.tree_scroll_to = None;
                    self.apply_script();
                }
                Event::FilesChanged => self.loose = self.engine.session().files.clone(),
                Event::KeysChanged => self.keys_loaded = self.engine.session().keys.is_some(),
                Event::Progress { message, .. } => self.progress = Some(message),
                Event::ConfigChanged | Event::Log { .. } => {}
            }
        }
        while let Ok(msg) = self.rx.try_recv() {
            match msg {
                Msg::Cmd { cmd, result } => self.finish_command(cmd, result),
                Msg::Preview { generation, result } => {
                    if generation == self.preview.generation {
                        match result {
                            Ok(l) => {
                                self.preview.tex = l
                                    .image
                                    .as_ref()
                                    .map(|d| (preview::upload(&self.ctx, "preview", d), d.original));
                                self.preview.state = PState::Ready(l);
                            }
                            Err(e) => self.preview.state = PState::Failed(e),
                        }
                    }
                }
                Msg::Search { generation, query, total, items } => {
                    if generation == self.search_generation {
                        self.ex.show_search(query, total, items);
                    }
                }
                Msg::Picked(what, paths) => self.picked(what, paths),
            }
        }
    }

    fn picked(&mut self, what: Pick, paths: Vec<PathBuf>) {
        if paths.is_empty() {
            return;
        }
        match what {
            Pick::GameFolder => {
                let p = paths[0].clone();
                if cw_cmd::gamefolder::validate(&p).is_none() {
                    self.toast(
                        "That folder has no GTA5.exe, so it may not be a GTA V install. Scanning it anyway.",
                        ToastKind::Warning,
                    );
                }
                self.run("Scanning game files…", "game.open", json!({ "folder": p }));
            }
            Pick::Files => {
                for p in paths {
                    self.open_path(p);
                }
            }
            Pick::InstallFiles | Pick::InstallFolder => {
                for source in paths {
                    self.run("Installing…", "mods.install", json!({ "source": source }));
                }
            }
            Pick::ExtractTo => {
                let Some(item) = self.ex.selected.clone() else { return };
                let path = self.ex.item_path(&item);
                self.run("Extracting…", "rpf.extract", json!({ "path": path, "dest": paths[0] }));
            }
        }
    }

    fn finish_command(&mut self, cmd: &'static str, result: Result<Value, cw_cmd::CmdError>) {
        self.busy = None;
        self.progress = None;
        let v = match result {
            Ok(v) => v,
            Err(e) => {
                self.toast(e.message, ToastKind::Error);
                return;
            }
        };
        match cmd {
            "game.open" | "rpf.open" => {
                let archives = v["archives"].as_u64().unwrap_or(0);
                let errors = v["error_count"].as_u64().unwrap_or(0);
                let mut msg = format!(
                    "Opened {} · {}",
                    format::plural(archives, "archive", "archives"),
                    format::plural(v["entries"].as_u64().unwrap_or(0), "entry", "entries")
                );
                if errors > 0 {
                    msg.push_str(&format!(
                        " · {} could not be read",
                        format::plural(errors, "archive", "archives")
                    ));
                }
                self.toast(
                    msg,
                    if errors > 0 && archives == 0 {
                        ToastKind::Error
                    } else if errors > 0 {
                        ToastKind::Warning
                    } else {
                        ToastKind::Success
                    },
                );
                if let Some(w) = v["key_warning"].as_str() {
                    self.toast(format!("Keys: {w}"), ToastKind::Warning);
                }
                self.keys_loaded = self.engine.session().keys.is_some();
                self.config_game_folder = self.engine.session().config.game_folder.clone();
                if self.mode == Mode::Mods {
                    self.refresh_mods();
                }
            }
            "game.detect" => {
                self.candidates = serde_json::from_value::<Vec<Value>>(v)
                    .unwrap_or_default()
                    .into_iter()
                    .filter_map(|c| {
                        Some(cw_cmd::gamefolder::Candidate {
                            path: PathBuf::from(c["path"].as_str()?),
                            source: match c["source"].as_str()? {
                                "Steam" => "Steam",
                                "Epic Games" => "Epic Games",
                                "OpenIV" => "OpenIV",
                                _ => "Rockstar launcher",
                            },
                            enhanced: c["enhanced"].as_bool().unwrap_or(false),
                        })
                    })
                    .collect();
            }
            "mods.list" => {
                self.mods.loaded = true;
                self.mods.dir = v["mods_dir"].as_str().map(PathBuf::from);
                self.mods.dir_exists = v["exists"].as_bool().unwrap_or(false);
                self.mods.rows = v["files"]
                    .as_array()
                    .map(|a| {
                        a.iter()
                            .map(|f| ModRow {
                                path: f["path"].as_str().unwrap_or_default().to_string(),
                                size: f["size"].as_u64().unwrap_or(0),
                                enabled: f["enabled"].as_bool().unwrap_or(true),
                                archive: f["archive"].as_bool().unwrap_or(false),
                                overrides_game: f["overrides_game"].as_bool().unwrap_or(false),
                            })
                            .collect()
                    })
                    .unwrap_or_default();
            }
            "mods.copy" => {
                self.toast(
                    format!("Copied {} to the mods folder", v["copied"].as_str().unwrap_or("archive")),
                    ToastKind::Success,
                );
                self.refresh_mods();
            }
            "mods.install" => {
                self.toast(
                    format!(
                        "Installed {}",
                        format::plural(v["installed"].as_u64().unwrap_or(0), "file", "files")
                    ),
                    ToastKind::Success,
                );
                self.refresh_mods();
            }
            "mods.enable" | "mods.disable" | "mods.remove" => self.refresh_mods(),
            "rpf.extract" => {
                let files = v["files"].as_u64().unwrap_or(0);
                let failed = v["failed"].as_u64().unwrap_or(0);
                let mut msg = format!(
                    "Extracted {} ({})",
                    format::plural(files, "file", "files"),
                    format::size(v["bytes"].as_u64().unwrap_or(0))
                );
                if failed > 0 {
                    msg.push_str(&format!(" · {failed} failed"));
                }
                self.toast(msg, if failed > 0 { ToastKind::Warning } else { ToastKind::Success });
            }
            _ => {}
        }
    }

    fn apply_script(&mut self) {
        let Some(s) = self.script.take() else { return };
        if let (Some(p), Some(t)) = (&s.select, self.ex.tree.clone())
            && let Some(r) = t.find(p)
        {
            self.ex.reveal(r);
        }
        self.preview.view = s.view;
        if s.mods {
            self.mode = Mode::Mods;
        }
        if s.palette {
            self.palette = Some(Palette::default());
        }
        if let Some(q) = s.search {
            self.search_everywhere = s.everywhere;
            self.search_text = q;
            self.search_changed();
        }
    }

    // ----------------------------------------------------------- preview

    /// Chooses a loose file for preview.
    pub fn select_loose(&mut self, p: PathBuf) {
        self.ex.selected = None;
        self.request_preview(Some(Target::Disk(p)));
    }

    fn request_preview(&mut self, target: Option<Target>) {
        if target == self.preview.target {
            return;
        }
        self.preview.target = target.clone();
        self.preview.generation += 1;
        self.preview.tex = None;
        let generation = self.preview.generation;
        let Some(target) = target else {
            self.preview.state = PState::Idle;
            return;
        };
        let tree = self.ex.tree.clone();
        // folders and archives are summarised on the spot, not loaded
        if let (Target::Entry(r), Some(t)) = (&target, &tree) {
            let e = t.entry(*r);
            if e.is_dir() || e.child_archive.is_some() {
                self.preview.state = PState::Idle;
                return;
            }
        }
        self.preview.state = PState::Loading;
        let src = match target {
            Target::Entry(r) => Source::Entry(r),
            Target::Disk(p) => Source::Disk(p),
        };
        self.spawn(move || Msg::Preview {
            generation,
            result: preview::load(tree.as_deref(), &src).map(Box::new),
        });
    }

    fn sync_preview(&mut self) {
        let want = match &self.ex.selected {
            Some(Item::Entry(r)) => Some(Target::Entry(*r)),
            Some(Item::Folder(_)) => None,
            None => match &self.preview.target {
                Some(Target::Disk(p)) if self.loose.contains(p) => Some(Target::Disk(p.clone())),
                _ => None,
            },
        };
        self.request_preview(want);
    }

    // ------------------------------------------------------------ search

    /// Called when the search box text or scope changes.
    pub fn search_changed(&mut self) {
        let q = self.search_text.trim().to_string();
        if !self.search_everywhere {
            self.ex.clear_search();
            self.ex.set_filter(q);
            return;
        }
        self.ex.set_filter(String::new());
        self.search_generation += 1;
        if q.chars().count() < 2 {
            self.ex.clear_search();
            return;
        }
        let (generation, tree) = (self.search_generation, self.ex.tree.clone());
        self.spawn(move || {
            let items = tree.map(|t| t.search_all(&q)).unwrap_or_default();
            let total = items.len();
            Msg::Search { generation, query: q, total, items: items.into_iter().take(5000).collect() }
        });
    }

    pub fn clear_search(&mut self) {
        self.search_text.clear();
        self.search_generation += 1;
        self.ex.clear_search();
        self.ex.set_filter(String::new());
    }

    // ----------------------------------------------------------- actions

    pub fn enabled(&self, a: Action) -> bool {
        let has_sel = self.ex.selected.is_some();
        match a {
            Action::ReloadGame => self.config_game_folder.is_some(),
            Action::Back => self.ex.can_back(),
            Action::Forward => self.ex.can_forward(),
            Action::Up => self.ex.parent_of(&self.ex.current).is_some(),
            Action::ExtractSelected => has_sel,
            Action::CopyPath => has_sel || matches!(self.preview.target, Some(Target::Disk(_))),
            Action::CopyToMods => self.mods_copy_source().is_some(),
            Action::ShowExplorer => self.mode != Mode::Explorer,
            Action::ShowMods => self.mode != Mode::Mods,
            Action::SearchEverywhere | Action::FocusSearch => !self.ex.is_empty(),
            Action::ShowPreview | Action::ShowHex | Action::ShowInfo => self.preview.target.is_some(),
            _ => true,
        }
    }

    pub fn perform(&mut self, a: Action) {
        if !self.enabled(a) {
            return;
        }
        match a {
            Action::OpenGameFolder => self.pick(Pick::GameFolder),
            Action::OpenFiles => self.pick(Pick::Files),
            Action::ReloadGame => {
                if let Some(f) = self.config_game_folder.clone() {
                    self.run("Scanning game files…", "game.open", json!({ "folder": f }));
                }
            }
            Action::Back => self.ex.back(),
            Action::Forward => self.ex.forward(),
            Action::Up => self.ex.up(),
            Action::FocusSearch => {
                self.search_everywhere = false;
                self.focus_search = true;
            }
            Action::SearchEverywhere => {
                self.search_everywhere = true;
                self.focus_search = true;
            }
            Action::ShowPreview => self.preview.view = ViewMode::Auto,
            Action::ShowHex => self.preview.view = ViewMode::Hex,
            Action::ShowInfo => self.preview.view = ViewMode::Info,
            Action::ExtractSelected => {
                if self.ex.selected.is_some() {
                    self.pick(Pick::ExtractTo);
                }
            }
            Action::CopyPath => {
                let text = match (&self.ex.selected, &self.preview.target) {
                    (Some(item), _) => Some(self.ex.item_path(item)),
                    (None, Some(Target::Disk(p))) => Some(p.display().to_string()),
                    _ => None,
                };
                if let Some(t) = text {
                    self.ctx.copy_text(t);
                    self.toast("Path copied", ToastKind::Success);
                }
            }
            Action::CopyToMods => {
                if let Some(path) = self.mods_copy_source() {
                    self.run("Copying to mods…", "mods.copy", json!({ "path": path }));
                }
            }
            Action::ShowExplorer => self.mode = Mode::Explorer,
            Action::ShowMods => {
                self.mode = Mode::Mods;
                self.refresh_mods();
            }
            Action::Quit => self.quit = true,
        }
    }

    /// The selected archive entry, when it lives in an archive from the game folder.
    pub fn mods_copy_source(&self) -> Option<String> {
        self.config_game_folder.as_ref()?;
        match self.ex.selected.as_ref()? {
            Item::Entry(r) => {
                let t = self.ex.tree.as_ref()?;
                let mut id = r.archive;
                while let Some(p) = t.archive(id).parent {
                    id = p;
                }
                let top = t.archive(id);
                let game = self.config_game_folder.as_ref()?;
                game.join(&top.path).exists().then(|| t.entry(*r).path.clone())
            }
            Item::Folder(_) => None,
        }
    }

    pub fn refresh_mods(&mut self) {
        if self.config_game_folder.is_some() {
            self.run("Reading mods folder…", "mods.list", json!({}));
        } else {
            self.mods.loaded = true;
        }
    }

    pub fn open_folder(&mut self, path: &std::path::Path) {
        let cmd = if cfg!(windows) {
            "explorer"
        } else if cfg!(target_os = "macos") {
            "open"
        } else {
            "xdg-open"
        };
        if std::process::Command::new(cmd).arg(path).spawn().is_err() {
            self.toast(format!("Could not open {}", path.display()), ToastKind::Error);
        }
    }

    fn shortcuts(&mut self, ctx: &egui::Context) {
        let typing = ctx.egui_wants_keyboard_input();
        let mut act = None;
        ctx.input_mut(|i| {
            let cmd_shift = Modifiers::COMMAND | Modifiers::SHIFT;
            if i.consume_key(Modifiers::COMMAND, Key::K) {
                act = Some(None); // palette toggle
            } else if i.consume_key(cmd_shift, Key::O) {
                act = Some(Some(Action::OpenGameFolder));
            } else if i.consume_key(Modifiers::COMMAND, Key::O) {
                act = Some(Some(Action::OpenFiles));
            } else if i.consume_key(cmd_shift, Key::F) {
                act = Some(Some(Action::SearchEverywhere));
            } else if i.consume_key(Modifiers::COMMAND, Key::F) {
                act = Some(Some(Action::FocusSearch));
            } else if i.consume_key(cmd_shift, Key::C) {
                act = Some(Some(Action::CopyPath));
            } else if i.consume_key(Modifiers::COMMAND, Key::E) {
                act = Some(Some(Action::ExtractSelected));
            } else if i.consume_key(Modifiers::COMMAND, Key::M) {
                act = Some(Some(Action::CopyToMods));
            } else if i.consume_key(Modifiers::ALT, Key::Num1) {
                act = Some(Some(Action::ShowExplorer));
            } else if i.consume_key(Modifiers::ALT, Key::Num2) {
                act = Some(Some(Action::ShowMods));
            } else if i.consume_key(Modifiers::COMMAND, Key::Q) {
                act = Some(Some(Action::Quit));
            } else if i.consume_key(Modifiers::COMMAND, Key::Num1) {
                act = Some(Some(Action::ShowPreview));
            } else if i.consume_key(Modifiers::COMMAND, Key::Num2) {
                act = Some(Some(Action::ShowHex));
            } else if i.consume_key(Modifiers::COMMAND, Key::Num3) {
                act = Some(Some(Action::ShowInfo));
            } else if i.consume_key(Modifiers::ALT, Key::ArrowLeft) {
                act = Some(Some(Action::Back));
            } else if i.consume_key(Modifiers::ALT, Key::ArrowRight) {
                act = Some(Some(Action::Forward));
            } else if i.consume_key(Modifiers::NONE, Key::F5) {
                act = Some(Some(Action::ReloadGame));
            } else if !typing && self.palette.is_none() && i.consume_key(Modifiers::NONE, Key::Backspace) {
                act = Some(Some(Action::Up));
            }
        });
        match act {
            Some(None) => self.palette = if self.palette.is_some() { None } else { Some(Palette::default()) },
            Some(Some(a)) => self.perform(a),
            None => {}
        }
    }

    fn handle_drops(&mut self, ctx: &egui::Context) {
        let dropped: Vec<PathBuf> =
            ctx.input(|i| i.raw.dropped_files.iter().map(|f| f.path().to_path_buf()).collect());
        for p in dropped {
            self.open_path(p);
        }
    }

    /// Scrolls the tree/list to keep the location and the keyboard selection visible.
    fn track_scroll_targets(&mut self) {
        if self.last_current.as_ref() != Some(&self.ex.current) {
            self.last_current = Some(self.ex.current.clone());
            self.tree_scroll_to = self.ex.rows.iter().position(|r| r.node == self.ex.current);
            self.list_scroll_to = Some(0);
        }
    }
}

impl App {
    fn screenshot(&mut self, ctx: &egui::Context) {
        use std::sync::atomic::Ordering;
        let Some(s) = self.shot.as_mut() else { return };
        s.frame += 1;
        ctx.request_repaint();
        if s.done.load(Ordering::SeqCst) {
            self.quit = true;
        } else if s.frame >= s.after_frames && self.busy.is_none() && !s.requested {
            s.requested = true;
            let (path, done) = (s.path.clone(), s.done.clone());
            ctx.request_screenshot(move |img| {
                let [w, h] = img.size;
                if let Err(e) =
                    image::save_buffer(&path, img.as_raw(), w as u32, h as u32, image::ColorType::Rgba8)
                {
                    eprintln!("screenshot failed: {e}");
                }
                done.store(true, Ordering::SeqCst);
            });
        }
    }
}

impl eframe::App for App {
    fn logic(&mut self, ctx: &egui::Context, _frame: &mut eframe::Frame) {
        self.pump();
        self.handle_drops(ctx);
        self.shortcuts(ctx);
        if std::mem::take(&mut self.pending_mods_copy) {
            self.perform(Action::CopyToMods);
        }
        self.sync_preview();
        self.track_scroll_targets();
        let t = ctx.input(|i| i.time);
        self.toasts.retain(|x| !x.expired(t));
        if self.busy.is_some() || !self.toasts.is_empty() {
            ctx.request_repaint_after(std::time::Duration::from_millis(100));
        }
        self.screenshot(ctx);
        if self.quit {
            ctx.send_viewport_cmd(egui::ViewportCommand::Close);
        }
    }

    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        self.show(ui);
    }

    fn clear_color(&self, _visuals: &egui::Visuals) -> [f32; 4] {
        crate::theme::GROUND.to_normalized_gamma_f32()
    }
}
