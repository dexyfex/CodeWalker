//! One executable: the GUI by default, plus the same engine from the command
//! line, a JSON control channel and an MCP server.

// The window opens without a console. The command-line modes get one back, see `console`.
#![cfg_attr(all(windows, not(debug_assertions)), windows_subsystem = "windows")]

mod actions;
mod app;
mod console;
mod format;
mod gpu;
mod icons;
mod latest;
mod layout;
mod model;
mod preview;
mod tabs;
mod texview;
mod theme;
mod ui;
mod world_view;

use std::{net::SocketAddr, path::PathBuf, process::ExitCode, sync::Arc};

use cw_cmd::{
    Engine,
    transport::{cli, json, mcp},
};
use eframe::egui;

const USAGE: &str = "\
CodeWalker — explore GTA V archives, preview files, manage mods and edit the map.

USAGE
  codewalker [PATH…]                     open the app (folders, .rpf archives or files to open)
  codewalker [--game DIR] COMMAND [--option value …]
                                         run one command and print its JSON result
  codewalker serve [--stdio | --listen 127.0.0.1:PORT]
                                         JSON control channel (one request per line)
  codewalker mcp                         MCP server on stdio
  codewalker help [COMMAND]              list commands or describe one

Commands that read archives open the saved game folder first unless --game DIR
or --open PATH is given.
";

fn main() -> ExitCode {
    let args: Vec<String> = std::env::args().skip(1).collect();
    let engine = Arc::new(Engine::new());
    if is_headless(&engine, &args) {
        console::attach();
    }

    match args.first().map(String::as_str) {
        Some("-h" | "--help") => {
            print!("{USAGE}");
            ExitCode::SUCCESS
        }
        Some("help") => {
            print!(
                "{}{}",
                if args.len() == 1 { USAGE } else { "" },
                cli::help(&engine, args.get(1).map(String::as_str))
            );
            ExitCode::SUCCESS
        }
        Some("mcp") => report(mcp::serve_stdio(&engine)),
        Some("serve") => serve(engine, &args[1..]),
        Some(_) if is_headless(&engine, &args) => run_command(&engine, &args),
        _ => gui(engine, args.into_iter().map(PathBuf::from).collect()),
    }
}

/// Whether these arguments run a command instead of opening the window.
fn is_headless(engine: &Engine, args: &[String]) -> bool {
    match args.first().map(String::as_str) {
        None => false,
        Some("-h" | "--help" | "help" | "mcp" | "serve") => true,
        Some(first) => first.starts_with("--") || engine.spec(first).is_some(),
    }
}

fn report(r: std::io::Result<()>) -> ExitCode {
    match r {
        Ok(()) => ExitCode::SUCCESS,
        Err(e) => {
            eprintln!("error: {e}");
            ExitCode::FAILURE
        }
    }
}

fn serve(engine: Arc<Engine>, args: &[String]) -> ExitCode {
    match args.first().map(String::as_str) {
        None | Some("--stdio") => report(json::serve_stdio(&engine)),
        Some("--listen") => {
            let Some(addr) = args.get(1).and_then(|a| a.parse::<SocketAddr>().ok()) else {
                eprintln!("error: --listen needs an address such as 127.0.0.1:7777");
                return ExitCode::FAILURE;
            };
            let token = json::random_token();
            eprintln!(
                "listening on {addr}\nfirst line of every connection must be: {{\"auth\":\"{token}\"}}"
            );
            report(json::serve_tcp(engine, addr, token))
        }
        Some(other) => {
            eprintln!("error: unknown option {other}\n\n{USAGE}");
            ExitCode::FAILURE
        }
    }
}

fn run_command(engine: &Engine, args: &[String]) -> ExitCode {
    let mut rest = args;
    let (mut game, mut open) = (None, None);
    while let Some(flag) = rest.first().filter(|a| matches!(a.as_str(), "--game" | "--open")) {
        let Some(value) = rest.get(1) else {
            eprintln!("error: {flag} needs a value");
            return ExitCode::FAILURE;
        };
        if flag == "--game" {
            game = Some(value.clone())
        } else {
            open = Some(value.clone())
        }
        rest = &rest[2..];
    }
    let Some(cmd) = rest.first() else {
        eprintln!("error: no command given\n\n{USAGE}");
        return ExitCode::FAILURE;
    };
    let Some(spec) = engine.spec(cmd).cloned() else {
        eprintln!("error: unknown command `{cmd}`; `codewalker help` lists them");
        return ExitCode::FAILURE;
    };
    let call = |name: &str, args: serde_json::Value| engine.execute(name, args);

    let needs_archives = spec.reads_archives() && !rest.iter().any(|a| a == "--file");
    let prepared = if let Some(g) = game {
        call("game.open", serde_json::json!({ "folder": g })).map(|_| ())
    } else if let Some(o) = open {
        call("rpf.open", serde_json::json!({ "path": o })).map(|_| ())
    } else if needs_archives && engine.session().config.game_folder.is_some() {
        call("game.open", serde_json::json!({})).map(|_| ())
    } else {
        Ok(())
    };
    if let Err(e) = prepared {
        eprintln!("error: {e}");
        return ExitCode::FAILURE;
    }

    let result = cli::parse_args(&spec, &rest[1..]).and_then(|a| call(cmd, a));
    match result {
        Ok(v) => {
            println!("{}", serde_json::to_string_pretty(&v).unwrap_or_default());
            ExitCode::SUCCESS
        }
        Err(e) => {
            eprintln!("error: {e}");
            ExitCode::FAILURE
        }
    }
}

fn window_icon() -> egui::IconData {
    let png = include_bytes!("../assets/icon.png");
    match image::load_from_memory(png) {
        Ok(img) => {
            let rgba = img.to_rgba8();
            egui::IconData { width: rgba.width(), height: rgba.height(), rgba: rgba.into_raw() }
        }
        Err(_) => egui::IconData::default(),
    }
}

fn gui(engine: Arc<Engine>, startup: Vec<PathBuf>) -> ExitCode {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_title("CodeWalker")
            .with_inner_size([1360.0, 860.0])
            .with_min_inner_size([960.0, 600.0])
            .with_drag_and_drop(true)
            .with_icon(window_icon()),
        ..Default::default()
    };
    let result = eframe::run_native(
        "CodeWalker",
        options,
        Box::new(move |cc| Ok(Box::new(app::App::new(cc, engine, startup)) as Box<dyn eframe::App>)),
    );
    match result {
        Ok(()) => ExitCode::SUCCESS,
        Err(e) => {
            eprintln!("error: could not start the window: {e}");
            ExitCode::FAILURE
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn args(a: &[&str]) -> Vec<String> {
        a.iter().map(|s| s.to_string()).collect()
    }

    #[test]
    fn paths_open_the_window_and_commands_do_not() {
        let e = Engine::ephemeral();
        assert!(!is_headless(&e, &args(&[])));
        assert!(!is_headless(&e, &args(&["/games/GTAV", "x64a.rpf"])), "paths to open");
        assert!(!is_headless(&e, &args(&["rpf"])), "a folder that merely starts like a command");
        for headless in [
            &["--help"][..],
            &["help"],
            &["mcp"],
            &["serve", "--stdio"],
            &["rpf.list"],
            &["--game", "D:/GTAV", "rpf.stats"],
            &["hash.joaat", "--text", "a"],
        ] {
            assert!(is_headless(&e, &args(headless)), "{headless:?}");
        }
    }

    #[test]
    fn the_embedded_icon_decodes() {
        let icon = window_icon();
        assert_eq!((icon.width, icon.height), (256, 256));
        assert_eq!(icon.rgba.len(), 256 * 256 * 4);
        assert!(icon.rgba.chunks(4).any(|p| p[3] == 255), "not blank");
    }
}
