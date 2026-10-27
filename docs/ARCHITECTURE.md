# Architecture

One executable, five crates. Everything the window does is a command the engine runs, so the same
action is available from the UI, the command line, a JSON channel and an MCP server.

```
cw-core    archives, crypto, resources, textures, maps, tiled images      (no UI, no GPU)
cw-world   the editable scene and its undo history                        (cw-core)
cw-cmd     the engine, every command, CLI / JSON / MCP front ends, mods   (cw-core, cw-world)
cw-gpu     wgpu: viewport renderer, image compositor, tile upload         (cw-core)
cw-app     the `codewalker` executable: egui window over the engine       (all of the above)
```

`cw-core`, `cw-world` and `cw-cmd` never touch a window or a GPU, so the whole command surface runs
headless and is tested that way.

## One engine, four front ends

`Engine::execute(name, args)` takes a command name and a JSON object and returns JSON or a
`CmdError { code, message }`. Each command is registered with a `CommandSpec`: name, category,
summary, parameters (name, type, description, required) and two flags, `mutates` and `destructive`
(it writes or deletes files). Everything else is generated from that spec:

| Front end | How it uses the spec |
|---|---|
| Window | calls `execute` from a worker thread and reacts to `Event`s |
| CLI | `codewalker COMMAND --option value`; flags come from the parameters, so a new command has a CLI on day one |
| JSON channel | `{"id":1,"cmd":"rpf.list","args":{…}}` per line, on stdio or loopback TCP with a token, a handshake timeout and a connection cap |
| MCP | `tools/list` is the specs; `readOnlyHint` and `destructiveHint` come from the flags |

`commands.list` returns the specs, so a client can discover everything at run time.

**Adding a command.** Write `fn(&Ctx, Value) -> CmdResult`, register it with a `CommandSpec` in a
module under `crates/cw-cmd/src/commands/`, and add an end-to-end test in `tests/end_to_end.rs`.
Mark it `.mutating()` or `.destructive()` when it is. A command in the category `archive`,
`textures` or `map` makes the CLI open the saved game folder first. To reach it from the window add
an `Action` in `cw-app/src/actions.rs` (palette entry, optional shortcut) that runs the command.

State lives in a `Session`: settings, keys, the open `RpfTree`, loose files and the scene. The tree
is immutable and shared (`Arc`); opening more archives builds a new tree that shares the old
archives. Commands tell the outside world what changed with `Event`s (`TreeChanged`,
`WorldChanged`, …) on a channel anyone can subscribe to.

## Archives (`cw-core::rpf`)

- Files are memory-mapped (`Blob`); a game folder is scanned in parallel with rayon. Nested
  archives are scanned in place, and each is checked to lie inside its parent's data.
- Hostile files are expected. Scans have budgets (`ScanLimits`: archives and entries); a directory
  owns only the entries it claims, so a table cannot make a directory contain itself; inflating
  stops at the size the file declares (and at 512 MiB); addresses and lengths inside resources are
  checked against the resource before they are followed. Tests include random byte damage.
- Encryption: `OPEN`, `AES` and `NG` are read. NG tables and keys come from `magic.dat`, which is
  only readable with the AES key; the AES key is found by SHA-1 scanning the user's own executable.
  NG *encryption* is not implemented (the game's inverse tables need a solver), so archives cannot
  be written back encrypted. The test builder writes encrypted entries using synthetic tables that
  have an exact inverse.
- `model_names()` turns the file names in the archives into a dictionary for the hashes that map
  entities use to name their models.

## Images (`cw-core::image`, `cw-gpu`)

A picture is cut into 128×128 tiles, each an `Arc`'d buffer. Cloning an image, or filtering it,
shares every tile that did not change: a filter returns the tile it was given when it changes
nothing. Filters (channel isolation, invert) run one task per tile on all cores. `TileTexture`
keeps a GPU texture in sync by uploading only the tiles that differ from what it holds. The
compositor draws that texture into an offscreen target with pan, zoom and a checkerboard under
transparency; egui shows the target. The command line runs the same filters (`texture.export
--format png --channels g --invert`), so the window and the CLI cannot disagree.

## The window (`cw-app`)

egui on wgpu, no web view. The UI thread owns all UI state; everything slow happens elsewhere and
comes back as a `Msg`:

| Thread | Work | Policy |
|---|---|---|
| `commands` | engine commands started from the UI | one at a time, in the order asked |
| `latest-wins` (previews) | extracting and parsing the selected file | only the newest request runs |
| `latest-wins` (textures) | decoding one mip of one texture | only the newest request runs |
| short-lived | searches, file dialogs | per request, results carry a generation and stale ones are dropped |

Navigation state (`model.rs`) is plain data with no egui in it, so it is unit-tested; it keeps its
location, history and selection across a rescan by path, since entry indices belong to one scan.
Design rules and tokens are in [`DESIGN.md`](DESIGN.md).

The tabs (`tabs.rs`, also plain data) are Explorer and World, which stay, and closable document tabs
(one per opened model, `docs/PLAN.md` stage 4); closing the tab on show shows its neighbour. The
widths of the tab strip and of the Explorer's bar are worked out in `layout.rs`, which has no egui
either: the window measures its text, the functions there decide what fits, and every group of
controls is drawn in its own rect, so nothing can overlap at any window width.

## Mods (`cw-cmd::mods`)

`<game>/mods/` mirrors the game folder; a file there replaces the original. Disabling renames to
`*.disabled` and keeps the file. Copies are written to a temporary file and renamed into place;
every destination is checked against the nearest existing ancestor so a symlink cannot move a write
out of `mods/`. The window has no page for them: it copies the selected archive there ("Copy to mods
folder") and leaves listing, installing, enabling and removing to the `mods.*` commands.

## Maps (`cw-core::{meta, ymap}`, `cw-world`)

A `.ymap` is a META resource: typed data blocks joined by (block, offset) pointers. `meta.rs` reads
the container, `ymap.rs` reads `CMapData` and its `CEntityDef`s (and interior instances) and the
level-of-detail tree. `world.import` turns the detailed entities into scene entities in one undo
step. Models are not loaded, so the viewport draws each entity as a box at its position with its
scale. There are matching writers (`build_meta`, `build_ymap`) for tests and tools.

## Testing and developer aids

- About 220 tests: unit tests beside the code, end-to-end tests that drive the engine with JSON
  (`crates/cw-cmd/tests/`), GPU tests that run on a software adapter and skip themselves when
  there is none.
- `cargo run -p cw-core --example fixture -- DIR` writes a small game-shaped folder (real archives,
  drawable textures, a map) for trying everything without the game.
- `codewalker game.check` samples the open archives with every reader and reports what failed and
  why. It is how this code gets checked against real game files.
- Screenshots under a virtual display: `CODEWALKER_SCREENSHOT=out.png` (with `_AFTER` seconds or
  `_FRAMES`), plus `CODEWALKER_SELECT`, `_VIEW`, `_MODE`, `_SCENE`, `_SEARCH`, `_PALETTE`, `_TEX`
  to put the window in a state first. Set `WGPU_BACKEND=gl` for a software GL driver.

## What has and has not been verified

Verified by tests: the archive container (built and read back, including damage), AES and NG
decryption plumbing with synthetic keys, .NET `Random` values, the hash functions, the tile and
filter pipeline against a serial reference, the compositor against read-back pixels, the command
layer end to end, and the UI by screenshots.

Not verified: any byte layout against real game files. Offsets for `.ytd`, META and `.ymap` were
transcribed from the C# source and are exercised by files this project writes itself. Key
derivation from a real executable and `magic.dat` have never run on real input. Run
`codewalker game.check` on an installation to find out what actually works.
