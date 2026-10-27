# Plan: from RPF explorer to the full CodeWalker

This is the work plan and the hand-off sheet. It is written so that any engineer or AI can stop at any
point and the next one can continue without asking anyone: every stage says what it is for, what has to
exist when it is done, how to prove it, and which files it touches. [`ROADMAP.md`](ROADMAP.md) says what
exists in the product; this file says what to do next and in which order.

**The goal** (from the user): the Rust port of CodeWalker, one executable, that does what the original
does, faster and extensible:

- an **RPF explorer** that previews files simply, and opens a model (`.yft`, `.ydr`, `.ydd`) in its **own
  tab**, a renderer-only view with the options of the original model viewer (textures, LOD, wireframe,
  material list, ...), also from the command line / "Open with";
- a **World** that loads and renders the GTA V map like the original (models, textures, LOD streaming,
  terrain, **sky, clouds, water and post-processing from the first version**), lets you move around, edit
  entities and keep the changes in **projects**;
- every action is a command (window, CLI, JSON channel, MCP server), GPU work is wgpu (Vulkan, Metal,
  DX12, GL), no web view.

## How to take over (read this first)

1. Read `CLAUDE.md` (commit rules), then [`ARCHITECTURE.md`](ARCHITECTURE.md), [`DESIGN.md`](DESIGN.md) and the
   porting notes in [`porting/`](porting/). The original C# source is **not** in the tree: it is in git
   history at commit `c45ea83` (`git show c45ea83:CodeWalker.Core/GameFiles/Resources/Drawable.cs`,
   `git ls-tree -r c45ea83 --name-only | grep -i water`). It is the only ground truth for byte layouts.
2. Find the first stage below whose status is not **done**. Open the work-log at the bottom to see what the
   last session did and what it left half-finished. `git log --oneline` and `git status` tell the rest.
3. Run `scripts/check.sh` before you start (it must be green) and before every commit.
4. Work in small steps; commit each one (conventional commits, one line, no co-author trailers); keep the
   tree compiling between commits.
5. **When you stop, update this file**: set the status of what you touched, tick the acceptance boxes you
   proved, and add a line to the work-log. A stage is only *done* when its acceptance list is complete and
   `scripts/check.sh` passes on the whole workspace.

Rules that never change:

- **A command first.** Logic lives in `cw-core` / `cw-world` / `cw-cmd` and is exposed through a
  `CommandSpec`; the window only calls commands. See "Adding a command" in `ARCHITECTURE.md`.
- **Hostile files are normal.** Every length, count and pointer is bounded by the data it came from; a bad
  file is an error, never a panic or a giant allocation. Every reader has damage tests.
- **Heavy work stays off the UI thread** (see the thread table in `ARCHITECTURE.md`).
- **Honesty about data.** There was never a real GTA V file in the development environment. Every format
  was ported from the C# and tested against files this project writes itself (builders, the `fixture`
  example). Nothing may be described as "verified" until `codewalker game.check` passes on a real
  installation (stage 9). Say so in docs and reports.
- **No web view, no Electron.** GPU = wgpu. Tests that need a GPU use the software adapter and skip when
  there is none.

Tooling notes: screenshots under a virtual display (`Xvfb :99`, `DISPLAY=:99 WGPU_BACKEND=gl`, the
`CODEWALKER_*` hooks in `ARCHITECTURE.md`; xdotool needs `windowfocus`); `cargo run -p cw-core --example
fixture -- DIR` writes a small game-shaped folder; a side-by-side `cargo +1.99.1` toolchain matches CI's
clippy; the build directory is large, so use `CARGO_INCREMENTAL=0` and no extra worktrees on a small disk.

## Status

| # | Stage | Status |
|---|---|---|
| 0 | Foundation: explorer, textures, maps as boxes, command surface, CI | done |
| 1 | Window shell: long paths, no Mods tab, document tabs | done (re-run `scripts/check.sh` once stages 2 and 3 are clippy-clean) |
| 2 | Data layer: models, archetypes, environment | in progress |
| 3 | GPU renderer: meshes, sky, clouds, water, post-processing, shadows | in progress |
| 4 | Model viewer tab | not started |
| 5 | World data: archetype and map index, streaming | not started |
| 6 | World view: rendering the map, time and weather | not started |
| 7 | World editing | not started |
| 8 | Projects and saving | not started |
| 9 | Real-data validation, performance, hardening | not started |

Dependencies: 1 is independent. 2 and 3 are independent of each other and of 1. 4 needs 1, 2a, 3. 5 needs
2a, 2b. 6 needs 3, 4, 5 and 2c. 7 needs 6. 8 needs 7. 9 runs after 4 (models) and again after 6 and 8.

---

## Stage 0: foundation (done)

RPF7 reader (nested, `OPEN`/`AES`/`NG` decryption, parallel scan, hostile-file budgets), explorer with
previews, textures (`.ytd`, `.dds`: mips, channels, save as DDS/PNG, copy-on-write tiles, filters on all
cores), `.ymap` reader and `world.import` (entities as boxes, named after model files), box scene editor
with undo, mods overlay commands, CLI / JSON / MCP front ends, `game.check`, CI (Windows tests, release
build artifact). Details: `ARCHITECTURE.md`, `ROADMAP.md`.

Known leftovers from the second review (small, do them when convenient): a console wrapper binary
`codewalker-cli` for Windows (the GUI exe has no console); `--file=x` (with `=`) is not recognised by the CLI's
"needs the game folder" check; texture export arguments should be captured when the button is clicked, not
when the file dialog closes; the scene list clones archetype strings every frame.

## Stage 1: window shell

**Why:** the path bar overflows on long archive paths and the Mods tab is not wanted; later stages need a
tab strip that can host one tab per opened model.

**Deliver**
- Breadcrumb that never overlaps the filter box at any width: keeps the first and the last crumbs, folds the
  middle into a `…` menu, truncates a single long crumb with a tooltip. Pure width function with tests.
- The Mods tab removed from the window (the `mods.*` commands and the "copy to mods" action stay).
- Tab strip with fixed tabs (Explorer, World) and dynamic closable document tabs (used by stage 4).

**Accept**
- [x] Screenshots at 900, 1100, 1600 and 2200 px with a deeply nested path show no overlap or clipping.
- [x] Unit tests for the breadcrumb fit function and the tab model (open, close, activate, fixed tabs cannot close).
- [x] Shortcut-uniqueness test green (Alt+1 Explorer, Alt+2 World).

**Files:** `cw-app/src/ui/*`, `app.rs`, `actions.rs`, README/ARCHITECTURE/DESIGN text about tabs.

## Stage 2: data layer (no rendering, no UI beyond commands)

### 2a. Models: `.ydr`, `.ydd`, `.yft`

**Why:** nothing can be drawn until a drawable is parsed into plain, renderer-friendly data.

**Deliver** (`cw-core::drawable`, builders for tests, commands in `cw-cmd`)
- `Drawable` with LOD models (high/medium/low/very low + switch distances), geometries with decoded vertex
  streams (positions, normals, tangents, up to 8 UV sets, colours, blend data), indices, bounds, shader
  group (shader name, texture/vector/float parameters), embedded texture dictionary, optional skeleton.
- `.ydd` dictionaries and `.yft` fragments (the drawables that are drawn; physics only as needed).
- `model.info`, `model.export` (OBJ + MTL + PNG), `game.check` sampling of the new readers, fixture with a
  textured drawable.

**Accept**
- [ ] Build -> parse round trips and a hand-assembled byte-level fixture per tricky layout.
- [ ] Damage tests (random bit flips, out-of-range pointers, giant counts) prove no panic and bounded memory.
- [ ] `codewalker model.info` / `model.export` work on the fixture; the OBJ opens in an external viewer.
- [ ] Docs list what is unverified against real files.

### 2b. Archetypes: `.ytyp`

**Deliver:** `cw-core::ytyp` (base / time / MLO archetype definitions: name hash, asset name, drawable and
texture dictionaries, flags, LOD distance, bounds; MLO rooms, portals and entities), `build_ytyp`, `ytyp.info`,
`game.check` sampling.
**Accept:** round trip + damage tests; the fixture ymap's entity resolves to the fixture archetype.

### 2c. Environment: time cycle, weather, clouds, water

**Why:** sky, clouds, water and post-processing must be driven by time of day and weather like the game.

**Deliver** (`cw-core::env`): parsers for the time cycle (+ modifiers), weather types, cloud settings and
water quads (`water.xml`, watermap if readable); `Timecycle::evaluate(hour, weather blend)`; sun/moon
direction and moon phase by time of day; `timecycle.eval`, `weather.list`, `water.info`, `clouds.info`;
`game.check` sampling; fixture files at the standard paths.
**Accept:** numeric tests of the interpolation (midnight wrap, weather blend 0 / 0.5 / 1, sun elevation by
hour); parsers have damage tests; docs say which files are plain XML and which are binary containers.

## Stage 3: GPU renderer (`cw-gpu`)

**Why:** one renderer serves the model viewer (one model, orbit camera) and the world (tens of thousands
of instances, fly camera). It takes plain buffers and parameter structs, never game types.

**Deliver, in this order (each step committed with tests and a demo image)**
1. Core: colour + depth target (reversed-Z, 20 km far plane), mesh upload with sub-meshes, materials (base
   colour with mips, optional normal/spec, alpha opaque/cutout/blend, double sided, vertex tint, unlit),
   instancing and draw lists, camera-relative rendering for +-8000 m worlds, `OrbitCamera` / `FlyCamera`,
   frustum culling and LOD-by-distance helpers, picking, line/box/grid/gizmo overlays, wireframe, stats,
   handle-based resource lifetime with byte sizes for LRU eviction.
2. HDR + post-processing: RGBA16F scene target, auto-exposure, bright-pass + separable bloom, tone-map
   and final pass, anti-aliasing (MSAA / supersample / FXAA), `PostSettings`.
3. Sky: dome gradient, sun, moon, stars, horizon haze, driven by `Environment`.
4. Clouds: animated, sun-lit layer with `CloudSettings`, procedural noise by default.
5. Water: `WaterQuad` list, animated normals, Fresnel sky reflection, depth tint, river flow, foam.
6. Cascaded sun shadow maps with PCF.
7. Dynamic lights (point/spot) for street lights and lit windows.

**Accept**
- [ ] Software-adapter tests: lit textured cube read back, depth order, instancing count, alpha cutout,
      culling counts, picking, precision at x = 7000 m, wireframe, resize, each post/sky/water/shadow pass.
- [ ] `cargo run -p cw-gpu --example mesh_demo -- out.png` shows textured, cutout and wireframe meshes, a
      10,000-instance field, day sky, dusk with bloom, clouds, water with reflection and shadows.
- [ ] Every pass can be switched off (the viewer runs a cheaper profile).

**Not measured here:** no real GPU was available; performance numbers must be taken on real hardware.

## Stage 4: model viewer tab

**Why:** in the explorer a model only gets a basic preview; opening it gives a dedicated, renderer-only tab
like the original `ModelForm` (read `CodeWalker/Forms/ModelForm*.cs` in the C# for the exact option list).

**Deliver**
- Conversion `Drawable` -> renderer meshes/materials, textures from the embedded dictionary and from `.ytd`
  files found next to it / by the archetype's texture dictionary (parent texture dictionaries too).
- A document tab per model (several at once, closable): orbit/pan/zoom, frame, reset; options as in the
  original: render mode (textured / solid / wireframe), LOD level, show textures on/off, show bounds, show
  skeleton, material and geometry list with shader and parameters, texture list with preview, statistics,
  lighting and time-of-day for the preview, export.
- Opening: double click and context menu in the explorer, palette action, **`codewalker file.yft`** (and the
  OS "Open with" on a loose file, which passes the path as an argument), which opens only that tab.
- The explorer preview of a model stays basic (summary and an "Open in viewer" button).
- Commands: opening and configuring a viewer, and **`model.render`**: render a model to a PNG headlessly
  (lod, size, camera angles, wireframe, textures) so scripts and MCP clients can look at a model.

**Accept**
- [ ] The fixture drawable renders textured in a tab (screenshot), switches LOD/wireframe/textures, lists
      its materials, and exports.
- [ ] `codewalker file.ydr` opens the viewer tab directly; `codewalker model.render` writes a PNG.
- [ ] Opening ten models and closing them returns GPU memory (no leak); a damaged model shows an error
      in its tab, never a crash.

## Stage 5: world data (index and streaming; no drawing)

**Why:** the world cannot be drawn until every entity can be resolved to a model and its textures, and the
right files are loaded as the camera moves.

**Deliver** (`cw-core` / `cw-cmd`, with commands)
- Archetype index: all `.ytyp` of the base game, `update` and DLC packs with correct override order; hash
  -> archetype, and the lookup of its `.ydr` / `.ydd` / `.yft` and `.ytd` (the C# `GameFileCache` rules,
  including texture-dictionary parents and `_hi` variants).
- Map index: every `.ymap` with its streaming extents and parent, a spatial index (the original's
  `MapDataStore` quadtree), LOD parent/child links, interior (MLO) instances.
- Streaming core: an async, bounded cache (LRU, byte budget) for ymaps, drawables and textures with
  priorities by distance, cancellation of requests that are no longer needed; no UI thread work.
- Commands: `world.index` (build/report), `world.archetype` (lookup by name/hash), `world.view` (what is
  loaded around a point), progress events.

**Accept:** fixture game folder with two ymaps, an archetype and a model streams in and out as a point moves;
cache budget respected; update/DLC override order tested; damage tests on every new reader entry point.

## Stage 6: world view

**Deliver:** the World tab renders the map: fly camera, entities drawn with their models and textures,
LOD selection by distance (`lodDist`, LOD parent hiding), culling, time of day and weather controls feeding
the stage 2c data into the stage 3 renderer (sky, clouds, water quads, bloom/exposure, sun shadows),
terrain and props, LOD lights, stats overlay (draws, instances, streaming queue, memory), selection
highlight. Frame time stays interactive on a world with tens of thousands of instances.

**Accept:** fixture world screenshot with models, sky, clouds, water and bloom; a streaming stress test
(synthetic 50,000 entities) keeps the UI thread free; toggles for each effect; `world.render` command for a
headless screenshot at a position, time and weather.

## Stage 7: world editing

**Deliver:** picking by click, box select, translate/rotate/scale gizmos with snapping, entity inspector
(archetype, position, rotation, scale, flags, LOD distance, parent), add entity from an archetype browser,
duplicate/delete, multi-select, undo/redo for all of it (extend `cw-world` history), ymap properties.
All edits are commands.

**Accept:** edit flows tested through the command surface; gizmo math unit-tested; screenshots of the flows.

## Stage 8: projects and saving

**Deliver:** a project (list of ymap/ytyp files and edits; JSON, plus read/write of the original `.cwproj`
XML if its schema is in the C#); writing edited `.ymap` files (`build_ymap` exists), new `.ytyp`; export to
the `mods/` overlay or a folder; an RPF writer for repacking (without encryption first; NG/AES encryption
needs the game's inverse tables, see `ARCHITECTURE.md`); unsaved-changes handling.

**Accept:** edit -> save -> reopen round trip; the saved `.ymap` re-parses to the same entities; export
into `mods/` is atomic and contained.

## Stage 9: validation and hardening

Run `codewalker game.check` on a real GTA V installation and fix what it reports, in this order: key
derivation and `magic.dat`, NG archives, `.ytd`, `.ymap`, then models, archetypes, environment files.
Profile on real hardware (frame time, streaming hitches, memory), add an independent review pass over every
new subsystem (security of readers, thread safety, leaks), update the "verified" claims in the docs only for
what a real file proved.

## Later (not scheduled)

`.ybn` collision, `.ynd` paths, navmeshes, `.ymt` scenarios, trains, cut-scenes, vehicle and ped viewers,
audio, Enhanced Edition (gen9) and PSO/RBF containers, a plugin surface, packaging (macOS bundle, Windows
installer).

## Work log (append one line per session: date, what, outcome, what is left)

- 2026-10: stage 0 complete; second review applied (hostile texture sizes, ymap rotation and LOD counts,
  undoable replace, `game.check` empties, GPU-limit guard for big textures). Stage 1, 2a, 2b (with 2c) and 3
  started in parallel by separate sessions; see the commit history for what each landed.
- 2026-10 (stage 1): Mods tab removed; the window now has a tab strip (`tabs.rs`, `ui/tabs.rs`) over the Explorer's
  bar, whose row is planned from measured widths (`layout.rs`: breadcrumb fold, filter and scope squeeze, tab
  shrink and overflow) and checked by screenshots at 900-2200 px under Xvfb. No document tab is opened by anything
  yet (stage 4 calls `Tabs::open`; the `expect(dead_code)` on it and on `DocKind::Model` marks the spot). `cw-app`
  passes fmt, clippy (1.95 and 1.99.1, `--no-deps`) and tests; workspace clippy was red only in `cw-core` / `cw-gpu`
  files other stages were still writing, so run `scripts/check.sh` again before calling the stage closed.
