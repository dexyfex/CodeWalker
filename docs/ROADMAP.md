# Roadmap

What exists, what is next, and what is deliberately not planned. Everything here is about the Rust
port; the C# tool it comes from is far larger (see [`porting/`](porting/) for a map of what is left,
with the C# file and line for each item).

**The one caveat that applies to every row below:** no real GTA V file has been available while
building this. Formats were ported from the C# source and tested against files this project
generated. Treat a format as unverified until `codewalker game.check` passes on a real installation.

## Done

| Area | What works |
|---|---|
| Archives | RPF7 open / nested / search / extract; `OPEN`, `AES` and `NG` reading; resources; parallel scan on memory-mapped files; hostile-file budgets |
| Explorer | tree, list, sorting, filter and search-everywhere, command palette, text / hex / info previews, loose files, drag and drop |
| Textures | `.ytd` and `.dds`: thumbnails, mips, channel isolation, invert, zoom and pan, pixel readout, save as DDS or filtered PNG |
| Mods | the `mods/` overlay: copy, install, enable / disable, remove, atomic and contained writes (commands; the window only copies an archive there) |
| Maps | `.ymap` summary and `world.import` of the detailed entities as boxes, named after model files when possible |
| Map editor | scene of boxes, move / rotate / scale with a gizmo, picking, undo / redo, save / load JSON |
| Control | CLI, JSON channel (stdio, authenticated loopback TCP), MCP server, `commands.list` |
| Project | CI on three platforms, tagged release builds, app icon, no console flash on Windows |

## Next

In this order, because each step needs the one before it:

1. **Run it on a real installation.** `codewalker game.check`, then fix whatever it reports: key
   derivation, `magic.dat`, NG archives, `.ytd` and `.ymap` layouts. Nothing below is worth much
   until this passes.
2. **Archetypes (`.ytyp`).** Real bounding boxes and names for the entity boxes; the META reader is
   already there, `core-map.md` section 3.7 has the offsets.
3. **Models (`.ydr`, `.ydd`, `.yft`).** Parse drawables (`core-map.md` section 4), upload meshes,
   bind textures from the `.ytd`, draw them in the viewport instead of boxes.
4. **A world that streams.** A spatial index over all map files, level-of-detail selection by
   camera distance, culling, so a whole map area can be open at once.
5. **Writing back.** An RPF writer for edit mode, `.ymap` export, and NG encryption (needs the
   game's inverse tables, which CodeWalker derives with a Gauss solver).

## Later

- Enhanced Edition (gen9) textures and models; PSO and RBF containers (non-RSC7 maps and metadata).
- `.ynd` (traffic paths), `.ymt` (scenarios), trains, cut-scenes, project files (`.cwproj`).
- Collision (`.ybn`), navmeshes, terrain and water, vehicle and ped viewers, audio.
- Packaging: a macOS app bundle with signing, a Windows installer, a Linux package.
- A plugin surface on top of the command engine (the commands already are the API).

## Not planned

- A web view, Electron or any runtime besides the executable itself.
- Shipping game data or keys. The AES key is found in the user's own executable. The only thing
  bundled for key derivation is `magic.dat`, the encrypted blob the original tool embeds; it is
  unreadable without that key.
