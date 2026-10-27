# CodeWalker (Rust)

A Rust port of [CodeWalker](https://github.com/dexyfex/CodeWalker) by dexyfex: one native executable to explore
GTA V `.rpf` archives, preview files, manage mods and place things on the map. No Electron, no webview, no .NET.

The window is [egui](https://github.com/emilk/egui) on top of [wgpu](https://wgpu.rs) (Vulkan, Metal, DX12,
OpenGL). Every action is a *command*, so everything the UI does can also be driven from the command line, a
JSON control channel or an MCP server.

> **Status: early.** It has been tested against archives built by the test suite, **not against real GTA V
> files** (none were available while writing it). Byte layouts come from the original C# source. Treat each
> format as unverified until you have opened real game data with it, and please report what breaks.

## What works

| Area | State |
|---|---|
| RPF7 archives | Open, nested archives, `OPEN` / `AES` / `NG` encryption, resources (`RSC7`), search, extract. Keys are derived from your own `GTA5.exe`. Scans run in parallel on memory-mapped files and are bounded against hostile input. |
| Explorer | Folder/archive tree, sortable list, text / hex / info previews, filter or search-everywhere, command palette (`Ctrl+K`, `Cmd+K` on macOS), open loose files, drag and drop. |
| Mods | `<game>/mods/` overlay: copy an archive from the game, install files or folders, enable / disable (a `.disabled` suffix, the file is kept), remove. Writes are atomic and cannot leave the `mods` folder. |
| Textures | `.ytd` and `.dds` preview: every texture of a dictionary with thumbnails, mips, channel isolation (R, G, B, A, RGB), invert, zoom, pan and a pixel readout. Pictures are copy-on-write tiles, filtered on all cores, uploaded tile by tile and composed on the GPU. `texture.list` and `texture.export` (DDS as stored, or a filtered PNG) expose the same pipeline to the command line. Tested on generated textures only. |
| Maps | `.ymap` files: a summary in the preview, and "Place in map editor" / `world.import` puts their detailed entities in the scene (one undo step). Entities are named after model files in the archives when one matches their hash. Legacy resource maps only. |
| Map editor | A scene drawn in a wgpu viewport: add, move, rotate, scale and remove entities with a gizmo, picking, undo / redo, save / load as JSON. Entities are **boxes**; models are not loaded yet. |
| Control | CLI, newline-delimited JSON channel (stdio or authenticated loopback TCP), MCP server on stdio. |

## What does not exist yet

- `.ytyp` archetypes, and any model (`.ydr`, `.ydd`, `.yft`) or collision (`.ybn`) viewing: the map editor shows boxes.
- Gen9 (Enhanced Edition) texture formats.
- Writing to archives (`NG` encryption, edit-mode RPF), project files, `.ynd` / `.ymt` editing, audio, vehicle and ped viewers.

[`docs/ROADMAP.md`](docs/ROADMAP.md) has the order of work, [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) how it fits together,
and `docs/porting/` what remains of the C# tool, with the file and line to read for each item.

## Check it against your game

Nothing here has been run on a real installation yet. This reads a sample of your archives with every
reader and says what worked and what did not, with the reason and example paths:

```sh
codewalker --game "D:\Games\GTAV" game.check
```

If it reports failures, that output is exactly what is needed to fix them.

## Build and run

Needs Rust 1.95 or newer (edition 2024; egui sets the floor). On Linux, eframe needs the usual windowing and GL
libraries (X11 or Wayland, `libxkbcommon`, `libGL` / `libEGL`).

```sh
cargo run --release                     # the app; pass folders, .rpf files or loose files to open them
cargo test --workspace
```

No game files? `cargo run -p cw-core --example fixture -- /tmp/fake-gta` writes a small GTA-shaped folder of real
(unencrypted) archives, and `cargo run --release -- /tmp/fake-gta` opens it.

On first start the app looks for your GTA V install (Steam, Epic, Rockstar, OpenIV) or asks for the folder.
The Legacy / Enhanced edition is taken from the executable found in that folder.

## Commands

Everything is `codewalker COMMAND --option value`; `codewalker help` lists them and `codewalker help COMMAND`
describes one. Options are generated from each command's parameters.

```sh
codewalker --game "D:\Games\GTAV" rpf.search --query adder
codewalker rpf.list --path x64a.rpf              # opens the saved game folder first
codewalker mods.copy --path x64/audio/sfx/resident.rpf
codewalker hash.joaat --text adder
codewalker texture.export --path x64a.rpf/textures/vehshare.ytd --index 0 --dest out --format png --channels a --invert
```

A control channel, one JSON request per line:

```sh
codewalker serve --stdio
# -> {"id":1,"cmd":"rpf.list","args":{"path":"x64a.rpf"}}
# <- {"id":1,"ok":true,"result":{...}}
codewalker serve --listen 127.0.0.1:7777   # prints a token; the first line must be {"auth":"<token>"}
```

And an MCP server for AI tools: `codewalker mcp` (tools are the commands with dots turned into underscores,
with read-only / destructive hints).

## Layout

| Crate | Role |
|---|---|
| `cw-core` | RPF7, keys and crypto, resources, textures, tiled images and their filters, hashes. No UI, no I/O beyond files. |
| `cw-cmd` | The command engine and its CLI / JSON / MCP front-ends, game folder detection, mods, map import. |
| `cw-world` | The editable scene and its undo history. |
| `cw-gpu` | wgpu renderer, camera, picking, tile-wise texture upload and the image compositor. |
| `cw-app` | The `codewalker` executable: egui UI over the engine. |

Design rules for the interface are in [`docs/DESIGN.md`](docs/DESIGN.md).

## Credits and licence

This is a port of CodeWalker, written by dexyfex and contributors; their work is the reference for every file
format here. [`Notice.txt`](Notice.txt) carries the copyright notices that came with it. The original C# source
is in git history at commit `c45ea83`.

The upstream tree contains no licence file, so **this repository has none either, and the terms are not
settled.** Confirm them with the original author before distributing binaries.

`.claude/skills/` holds the design skills used while building the interface, each with its own licence.
