# CodeWalker app map (C# to Rust port)

> **The C# sources are no longer in the working tree.** They are in git history at
> commit `c45ea83` (the upstream tip this port started from). Read a file with
> `git show c45ea83:CodeWalker.Core/GameFiles/RpfFile.cs`, or browse the whole tree with
> `git worktree add ../codewalker-csharp c45ea83`. Paths below refer to that commit.

Read-only map of the C# sources under `/home/user/CodeWalkerFork`. Line counts are `wc -l` of `.cs` files; "+D" means the matching `*.Designer.cs` is counted separately. Items marked (unverified) were not read in full.

## 0. Solution layout

| Path | Role | .cs lines |
|---|---|---|
| `CodeWalker/` | Main exe (net48 WinExe): World, RPF Explorer, Vehicle and Ped viewers, Menu, Project editor, Tools | 137,234 (incl. Designers) |
| `CodeWalker.Core/` | Game-file formats, GameFileCache, RPF I/O, audio, meta/PSO, mods overlay | 178,009 (not mapped) |
| `CodeWalker.WinForms/` | WinForms helpers (PropertyGrid, TextBox fixes, STNodeEditor) | 10,597 (not mapped) |
| `CodeWalker.ModManager/` | Mod staging tool exe | 1,835 |
| `CodeWalker.RPFExplorer/`, `.Vehicles/`, `.Peds/` | Launcher exes; short Program.cs, each refs CodeWalker.csproj | 50 / 36 / 38 |
| `CodeWalker.Gen9Converter/`, `.ErrorReport/` | Gen9 converter launcher; error-report form | 33 / 251 (not mapped) |
| `Shared/` | EmbeddedAssemblies.cs, UserConfig.cs, SelfContained.targets | 178 |
| `CodeWalker.Shaders/`, `Shaders/` | 90 `.hlsl` (+ `.hlsli`, vcxproj) and their 90 compiled `.cso` | |
| `crates/cw-core`, `crates/cw-cmd` | Existing Rust port, 3,749 `.rs` lines (not inspected) | |

NuGet deps: SharpDX 4.2.0 (D3D11, D2D1, D3DCompiler, Mathematics, XAudio2, XInput), DockPanelSuite.ThemeVS2015 3.0.6, FCTB 2.16.24. `Shared/SelfContained.targets` embeds copy-local DLLs as resources, loaded by `EmbeddedAssemblies.cs`.

## 1. Entry point and modes

`CodeWalker/Program.cs` (186 lines). `Main` runs `EmbeddedAssemblies.Install()`, `UserConfig.Repair()`, then `Run(args)`. `Run` scans all args case-insensitively, then dispatches in this order (Program.cs:88-113):

| Trigger | Form (file, lines) |
|---|---|
| `menu` | MenuForm (`MenuForm.cs` 145 + 233 D) |
| `explorer`, or ANY arg that is an existing file or directory | ExploreForm (`ExploreForm.cs` 5,181 + 1,511 D); paths become StartupPaths |
| `project` | Project.ProjectForm (`Project/ProjectForm.cs` 9,934 + 1,921 D) |
| `vehicles` | VehicleForm (`VehicleForm.cs` 1,427 + 925 D) |
| `peds` | PedsForm (`PedsForm.cs` 1,632 + 1,200 D) |
| no mode arg | WorldForm (`WorldForm.cs` 8,037 + 3,981 D), the default |

Precedence: `menu` beats everything. Any path argument forces the Explorer even when `vehicles`, `peds` or `project` is also given (Program.cs:92 is tested first).

Other launch logic:
- GTA folder check runs before dispatch (Program.cs:79). Release builds wrap dispatch in try/catch with a MessageBox (Program.cs:84-121).
- `EnsureJumpList()` (Program.cs:125-184) registers Jump List entries once (World, RPF Explorer, Vehicle, Ped; flag `Settings.JumpListInitialised`).
- Sibling exes (RPFExplorer, Vehicles, Peds) run their form directly and skip dispatch. They validate the GTA folder in the form itself (`UpdateGTAFolder(true)` at VehicleForm.cs:247, PedsForm.cs:297, ExploreForm.cs:186).
- ModManager is a separate exe (`CodeWalker.ModManager/Program.cs`, 32 lines), no args.
- Cross-launch is one-way: WorldForm's Tools menu opens an ExploreForm (WorldForm.cs:7040). MenuForm opens WorldForm on a new thread (MenuForm.cs:94). The Explorer never opens WorldForm.

## 2. RPF Explorer

Main file: `CodeWalker/ExploreForm.cs`, not under `Forms/` as the brief assumed. Viewers are in `Forms/`, tools in `Tools/`.

### 2.1 Menus and bars
- **File**: Configure Game, Open Folder, Exit.
- **Edit**: View, View Hex, Export XML, Extract Raw, Extract All, Import FBX, Import XML, Import Raw, Copy, Copy Path, Copy File List, Paste, Rename, Delete, Select All.
- **View**: Large/Small Icons, List, Details; Theme Blue / Light / Dark (ExploreForm.cs:79, persisted).
- **Tools**: Binary Search, Audio Explorer, Old RPF Browser (`Tools/BrowseForm.cs` 1,288), JenkGen, JenkInd, Asset Converter.
- **Options**: Start in Edit mode (persisted); Start in Folder (Default / Set to Default / Set to Current Folder).
- Toolbar: Back, Forward, Up (each with a history [List]); address box (Enter or Go); Refresh All; Edit mode toggle (shows a "changes are saved automatically" warning); Global Search (Enter); Filter (live as you type); Copy to mods folder (edit mode only, see 2.5).
- Status bar: one StatusLabel (ExploreForm.cs:432).
- Extract scripts, textures, raw, shaders and keys are NOT in the Explorer. They exist only in MenuForm and WorldForm's Tools menu.

### 2.2 Tree, list, keys and drag-drop
- Left tree: GTA root folders and RPF archives as nested nodes, scanned in the background (ScanRpfFiles 778, RefreshMainTreeView 734). Extra root folders persist in `RPFExplorerExtraFolders`. Context menu: Copy Path, Show in Windows Explorer (file-system nodes only), Expand, Collapse, Collapse All, Close Folder (extra roots only).
- Right list: Name, Type, Size, Attributes, Path columns, click-to-sort, virtual list, label-edit in edit mode.
- Keys (ExploreForm.cs:3983-4025): Ctrl+P view, Ctrl+H hex, Ctrl+S export XML, Ctrl+E extract raw, Ctrl+Shift+E extract all, Insert import raw, Shift+Insert import XML, Ctrl+C copy, Ctrl+Shift+C copy path, Ctrl+V paste, F2 rename, Shift+Delete delete, Ctrl+A select all.
- Drag out: selection is written to a temp folder and dragged out. Files over 100 MB are refused (ExploreForm.cs:4078).
- Drag in, edit mode: files are imported. Shift forces raw. `.xml` imports as XML (4215-4247). Outside edit mode, a drop opens the path (OpenPath, 3665). A dropped non-RPF file opens its viewer directly.

### 2.3 Context menus (gated, ShowListContextMenu 2093-2158)
- Item: View, View Hex, Export XML (XML-convertible types only), Extract Raw, Extract Uncompressed, Extract All; New > Folder, RPF Archive, YTD File; Import Raw / FBX / XML; Copy, Copy Path, Copy File List, Paste; Open File Location (search results); Rename; Delete; Defragment Archive; Select All.
- Create, import, paste, rename, delete need edit mode.

### 2.4 Action methods (ExploreForm.cs line numbers)
- View 1631, ViewHex 1770, ViewSelected 2377, ViewSelectedHex 2390. Export XML 2403. Extract Raw 2484, Extract Uncompressed 2556, Extract All 2626.
- New Folder 2664, New RPF Archive 2754 (encryption choice), New YTD 2808. Import FBX 2844 (`Tools/ImportFbxForm.cs`, 133), Import XML 2953, Import Raw 3076, folder-structure import 3142.
- Copy 3221, Copy Path 3240, Copy File List 3275, Rename 3287, Delete 3364, Paste 3707.
- Global Search 1386, Filter 1442, search results 1479. Show in Windows Explorer 3559, Open File Location 3570, Open Folder 3602. History: GoUp 563, GoBack 569, GoForward 584.

### 2.5 Edit mode, mods copy, encryption
- Edit mode writes straight into RPF files. No undo (EnableEditMode, 2163).
- Copy to mods folder (CopyToModsFolder, 2207-2287) copies the whole top-level `.rpf` containing the current folder to `<GTA>\mods\<same relative path>`. It asks before overwriting. Only for RPFs inside the GTA folder.
- EnsureRpfValidEncryption (2307) can switch an archive to OPEN encryption. It warns that the game then needs OpenIV.asi or OpenRPF.asi.
- Defragment (DefragmentSelected, 3457) previews the size first.

### 2.6 Missing
- No "open in World view" or "open in model viewer" action. ExploreForm.cs has no WorldForm reference.
- No mod enable or disable, only copy-to-mods. No undo.

## 3. Per-file-type viewers and editors

Routing: `InitFileTypes` (ExploreForm.cs:265-326) maps extension to `FileTypeAction`; `View()` (1631-1768) picks the form; handlers are at 1803-2025. Anything not listed falls back to hex.

| Extension(s) | Opens (file, lines) | Shows / edits |
|---|---|---|
| `.ydr` `.ydd` `.yft` `.ybn` `.ypt` `.ynv` | ModelForm (`Forms/ModelForm.cs` 2,704 + 1,163 D) | 3D viewer; material editor (`ModelMatForm.cs` 303); light editor (`ModelLightForm.cs` 1,187 + 1,536 D); saves each type via `.Save()` (ModelForm.cs:1682-1750) |
| `.ytd` | YtdForm (`Forms/YtdForm.cs` 728 + 604 D) | Texture dictionary: add, replace, remove, save YTD and textures |
| `.ymap` `.ytyp` `.ynd` `.cut` `.ymt` `.ymf` `.pso` `.yed` `.yld` `.yfd` `.mrf` `.ypdb` `cache_y.dat` `heightmap*.dat` | MetaForm (`Forms/MetaForm.cs` 601 + 370 D) | "Meta Editor", Edit and View menus marked [WIP!]. Not CutsceneForm or Project panels |
| `.awc` | AwcForm (`Forms/AwcForm.cs` 456 + 410 D) | Audio player; export .wav |
| `.rel` | RelForm (`Forms/RelForm.cs` 812 + 713 D) | Audio REL: search, synth play, copy XML; toolbar New/Open/Save (scope unverified) |
| `.ycd` | YcdForm (`Forms/YcdForm.cs` 254 + 219 D) | Clip dictionary inspector; no save seen |
| `.fxc` | FxcForm (`Forms/FxcForm.cs` 191 + 378 D) | Shader and technique list; no save seen |
| `.ywr` | YwrForm (`Forms/YwrForm.cs` 132 + 165 D) | Waypoint table; export, copy |
| `.yvr` | YvrForm (`Forms/YvrForm.cs` 159 + 249 D) | Vehicle record table; export, copy |
| `.xml` `.meta` | XmlForm (`Forms/XmlForm.cs` 357 + 296 D) | XML editor with Save |
| `.txt` `.log` `.ini` `.vdf` `.sps` `.ugc` `.ide` `.ipl` `.gxt2` `.nametable` | TextForm (`Forms/TextForm.cs` 368 + 296 D) | Text editor with Save; `.gxt2` shown as plain text |
| `.dat` (distantlights, distantlights_hd) | GenericForm (`Forms/GenericForm.cs` 58) | "File Inspector" |
| everything else (`.rpf` `.png` `.jpg` `.dds` `.gif` `.bik` `.gfx` `.ysc` `.dll` `.exe` `.asi`) | HexForm (`Forms/HexForm.cs` 155) | Hex dump; images not previewed |

Export XML is offered for types whose `InitFileType` call sets xmlConvertible (ymt, pso, ynd, ynv, yvr, ywr, fxc, yed, yld, yfd, yft, ydr, ydd, cut, ymf, ytd, mrf, ycd, ypt, ybn, ytyp, ymap, awc, rel, ypdb).

Write-back: TextForm, XmlForm, YtdForm, ModelForm, MetaForm (WIP), RelForm (unverified). Read-only: AwcForm, YcdForm, FxcForm, YwrForm, YvrForm, GenericForm, HexForm.

## 4. CodeWalker.ModManager (9 .cs files, 1,835 lines)

What it is: a staging cache for mod files. It never installs into the game folder.

| File | Lines | Role |
|---|---|---|
| `Program.cs` | 32 | Entry point |
| `ModManagerForm.cs` (+365 D) | 537 | Main UI, install and uninstall logic |
| `Mod.cs` | 353 | Mod, ModFile, ModType, ModStatus; BeginInstall, CompleteInstall, Load; mod.cwmm |
| `SelectFolderForm.cs` (+155 D) | 157 | GTA folder and edition picker; registry autodetect |
| `SettingsFile.cs` | 73 | Settings: GameFolder, IsGen9, AESKey |
| `SimpleKvpFile.cs` | 100 | Line key/value file (spaces to `<SPACE>`, newlines to `<NEWLINE>`) |
| `Properties/Resources.Designer.cs` | | Resources only |

UI: Game button, "Install new mod...", drag-drop anywhere, installed list (Name, Status; orange = Pending, green = Ready), selected-mod panel (file list, Move up, Move down, Uninstall), Clean and "Build mods folder" buttons.

Data: Mod has Name, LocalPath, SourcePath, Type (Loose, DLC, OIV, ASI, ASILoader, ModLoader), Status (Pending, Ready), LoadOrder (default -1), Files. Per mod, `mod.cwmm` is key/value (Name, Icon, Source, Type, Status, Order, Files, File0..n); `log.cwmm` is an append-only log.

Install:
- Accepted: `dlc.rpf` (name = parent folder); `*.oiv` (zip extracted to cache; `uninstall*.oiv` rejected); `*.asi` (name = file name; copies all top-level files of its folder, Mod.cs:95-99).
- Loose files are rejected by `CanInstallFile`, so the Loose type is unreachable from the UI.
- Folder drop: recursive, refused above 100 files, keeps only supported types. Duplicate names are refused.
- Target: `<exe dir>\ModCache\<GTAVLegacy|GTAVEnhanced>\<ModName>\`, overwriting same-named files, then writes mod.cwmm.
- Uninstall: confirm, then delete the cache folder.

Enable and disable: not implemented. Status is never set to Ready. LoadOrder is assigned (max+1) and sorted but never applied. Move up and Move down are not wired. Clean and Build mods folder are visible with no click handler (ModManagerForm.Designer.cs:88-89, 171-172).

Conventions and gaps:
- `<exe dir>\ModCache\<edition>\`: the only folder touched. Nothing in the repo reads it.
- `<GTA>\mods\`: the main app's overlay (CodeWalker.Core RpfFile.cs:85-113, GameFileCache.cs:920-950). ModManager never writes there.
- OpenIV: registry source for autodetect only (SelectFolderForm.cs:76). No overlay handling.
- Backups: none. AES key: scanned from the game exe on first run and saved in plain text in the ini.
- A cache folder without mod.cwmm loads with Name = null and is hidden (ModManagerForm.cs:186) but stays on disk.

## 5. World view

| File | Lines (+D) | Role |
|---|---|---|
| `WorldForm.cs` | 8,037 + 3,981 D | Render loop, input, toolbox, toolbar, loading |
| `World/MapSelection.cs` | 1,645 | Mouse hit and selection model |
| `World/Widget.cs` | 956 | Move, rotate, scale gizmo |
| `World/CutsceneForm.cs` | 1,575 + 269 D | Cutscene viewer |
| `World/WorldInfoForm.cs` | 600 + 675 D | Selection info window |
| `World/WorldSearchForm.cs` | 535 + 681 D | World search; calls GoToEntity |
| `World/SettingsForm.cs`, `StatisticsForm.cs` | 325 + 886 D; 45 + 245 D | Advanced settings; statistics |
| `Project/ProjectForm.cs` | 9,934 + 1,921 D | Project window (also the `project` mode) |
| `Project/ProjectFile.cs`, `UndoStep.cs`, `ProjectExplorerPanel.cs` | 1,271; 1,786; 2,842 | Project model; undo steps; project tree |
| `Project/MenyooXml.cs` | 212 | Menyoo-format XML (unverified) |

Features:
- Camera: free fly, first-person Ped mode, Perspective, Map View and Orthographic modes, FOV, clip planes, sensitivity, mouse invert, XInput gamepad (up to 4 controllers).
- Selection: left-click, or right-click when MouseSelect is on. 18 toolbar modes: Entity, Entity Extension, Archetype Extension, Time Cycle Modifier, Car Generator, Grass, Water Quad (calm, wave), Collision, Nav Mesh, Traffic Path, Train Track, Lod Lights, Interior Instance, Scenario, Audio, Occlusion.
- Widgets: move, rotate, scale; object or world space; snap to ground, grid, or both; grid size; rotation snap Off, 1, 2, 5, 10, 45, 90, Custom.
- Toolbar: New (Project, Ymap, Ytyp, Ybn, Ynd, Trains, Scenario); Open (Project, Files, Folder); Save, Save All; Add and Delete entity; Copy, Paste; Undo and Redo with history lists; Selection info; Project window.
- Undo and redo: MarkUndoStart / MarkUndoEnd, Undo, Redo (WorldForm), UndoStep classes.
- Toolbox tabs (collapsible): View (World, Ymaps, Model), Markers (add, go to, locate, multi-find), Selection (Entity, Archetype, Drawable with Info, Models, Textures; Ext), Options (General, Render, Helpers, Lighting).
- Render toggles: deferred shading, HDR, shadow cascades, skydome, grass, LOD lights, HD textures, anisotropic filtering, render mode, wireframe.
- Tools menu: Configure Game, RPF Browser, RPF Explorer, Selection info, Project window, Cutscene viewer, Audio explorer, World search, Binary search, JenkGen, JenkInd, Extract scripts, textures, raw, shaders, Options. Controls... opens KeyBindForm (62 lines).

Project panels (`Project/Panels`: 42 files, 19,880 lines, no designers):
- ymap: EditYmapPanel 563, EditYmapEntityPanel 799, EditYmapCarGenPanel 421, EditYmapLodLightPanel 521, EditYmapGrassPanel 286, EditYmapOccludeModelPanel 245, EditYmapBoxOccluderPanel 162.
- ytyp: EditYtypPanel 63, EditYtypArchetypePanel 465, EditYtypMloRoomPanel 278, EditYtypMloPortalPanel 270, EditYtypMloEntSetPanel 198.
- ybn: EditYbnPanel 77, EditYbnBoundsPanel 617, EditYbnBoundPolyPanel 624, EditYbnBoundVertexPanel 114.
- ynd: EditYndPanel 166, EditYndNodePanel 1,469. ynv: EditYnvPanel 263, EditYnvPolyPanel 288, EditYnvPortalPanel 311, EditYnvPointPanel 150, GenerateNavMeshPanel 2,148.
- scenario: EditScenarioNodePanel 2,399, EditScenarioYmtPanel 119. trains: EditTrainTrackPanel 95, EditTrainNodePanel 139.
- audio: EditAudioFilePanel 103, EditAudioInteriorPanel 215, EditAudioInteriorRoomPanel 436, EditAudioAmbientZonePanel 681, EditAudioAmbientRulePanel 527, EditAudioStaticEmitterPanel 613, two list panels (150 each).
- lods and grass: GenerateLODLightsPanel 316, DeleteGrassPanel 72. project: EditProjectPanel 51, EditProjectManifestPanel 319, EditMultiPanel 133, WelcomePanel 22, ProjectExplorerPanel.
- Water: no water panel. Water quads are selectable only in the World view (edit support unverified).

## 6. Rendering layer

`CodeWalker/Rendering/`: 20,615 lines of non-designer code.

| Class (file, lines) | Purpose |
|---|---|
| Renderer (`Renderer.cs`, 4,296) | Frame driver (278-336); world, ymap, LOD, grass, car gens (1853-2803); sky, clouds, water, paths, nav, scenarios (1565-1839); gizmos and selection overlays (645-1360) |
| Renderable, RenderableInst (`Renderable.cs`, 2,501) | Per-drawable GPU instance: models, geometry, lights, skeleton, textures |
| RenderableCache (`RenderableCache.cs`, 454) | GPU cache for drawables and textures with eviction |
| ShaderManager (`ShaderManager.cs`, 1,450) | One instance per shader family; creates PostProcessor (HDR) and DeferredScene on demand (296-322) |
| VertexTypes (`VertexTypes.cs`, 1,044) | GTA vertex layouts as structs; unknown layouts get names like VertexType89 |
| DXManager (`DirectX/DXManager.cs`, 365) | SharpDX D3D11 device and swap chain; feature levels 11.0, 10.1, 10.0 (line 67) |
| DXUtility (467), DXForm (29) | D3D helpers; interface a WinForm implements to host the render target |
| Shader (`Shaders/Shader.cs`, 30) | Abstract shader wrapper base |
| BasicShader (`Shaders/BasicShader.cs`, 1,077) | Main lit textured entity shader; many vertex variants |
| TerrainShader 658, WaterShader 569, SkydomeShader 410, CloudsShader 357, CableShader 357, PathShader 274, TreesLodShader 326, DistantLightsShader 146, BoundsShader 239, MarkerShader 166, WidgetShader 617 | Terrain, water, sky with sun and moon, clouds, power cables, traffic paths, tree LOD, distant lights, bounds wireframes, markers, gizmo |
| ShadowShader (445) and Utils/Shadowmap.cs (969) | Cascaded shadow depth pass and shadow map setup |
| DeferredScene (640) | Deferred lighting passes, final pass, SSAA |
| PostProcessor (707) | HDR: compute bloom and luminance reduction, final pass |
| Utils/GpuBuffers.cs (593), RenderTargetSwitch.cs (86) | Constant and dynamic buffer helpers; render target switching |
| Utils/Unit*.cs (cube, sphere, cylinder, capsule, cone, disc, quad; 93 to 308 each) | Generated primitive meshes for debug, selection, light cones |

Shaders: 90 compiled `Shaders/*.cso`, every one referenced by a loader.

| Group | Count | Binaries (loader) |
|---|---|---|
| Basic | 26 | BasicVS_P\*N\*C\*T\* (24 VS, incl. Box, Sphere, Capsule, Cylinder), BasicPS, BasicPS_Deferred (BasicShader) |
| Terrain | 9 | TerrainVS_PNCCT, PNCCTT, PNCCTX, PNCCTTX, PNCCTTTX, PNCTTX, PNCTTTX; TerrainPS, TerrainPS_Deferred |
| Deferred and post | 18 | DirLight, LodLights, Light (VS, PS, PS_MS each); PPFinalPassVS/PS; PPSSAAPS; PPCopyPixelsPS; PPBloomFilterBPHCS, PPBloomFilterVCS, PPLumBlendCS, PPReduceTo0DCS, PPReduceTo1DCS |
| Skydome, Water, Path, Shadow | 6, 6, 5, 3 | SkydomeVS/PS, SkySunVS/PS, SkyMoonVS/PS; WaterVS_PT, PCT, PNCT, PNCTX, WaterPS, WaterPS_Deferred; PathVS, PathPS, PathDynVS, PathBoxVS, PathBoxPS; ShadowVS, ShadowVS_Skin, ShadowPS |
| Bounds, Cable, TreesLod, Clouds, DistantLights, Marker, Widget | 3, 3, 3, 2, 2, 2, 2 | BoundingBoxVS, BoundingSphereVS, BoundsPS; CableVS, CablePS, CablePS_Deferred; TreesLodVS, PS, PS_Deferred; CloudsVS/PS; DistantLightsVS/PS; MarkerVS/PS; WidgetVS/PS |

Notes:
- Vertex suffixes: P position, N normal, C colour, T texcoord set, B blend weights (skinning). X appears to mark tangents (seen on PNCTTX and PBBNCCTTX). The rest is inferred.
- All shaders are compiled as shader model 4.0, compute included (CodeWalker.Shaders.vcxproj:79-91). The device asks for feature levels 11.0 down to 10.0.
- Shaders are embedded in CodeWalker.Core (csproj line 23). `PathUtil.ReadAllBytes` reads the embedded copy first, then disk (`CodeWalker.Core/Utils/Utils.cs:32`).

Minimum set for a basic textured-model and map-entity viewer:
- Code: DXManager, DXUtility, DXForm, Shader base, BasicShader, ShaderManager (Basic only), VertexTypes (PNCT, PNCTT, PNCCT, PNCTTX, PBBNCT), Renderable, RenderableCache, GpuBuffers, a Renderer subset (BeginRender, EndRender, RenderWorld, RenderYmap, RenderYmapLOD, RenderFragment), and CodeWalker/Utils/TextureLoader.cs (216).
- Binaries: BasicVS_PNCT, BasicVS_PNCTT, BasicVS_PNCCT, BasicVS_PBBNCT (skinned), BasicPS. Add the X variants when a model needs tangents.
- Defer: bounds and marker overlays, Widget, Shadow, Skydome, Water, Terrain, DeferredScene, PostProcessor, TreesLod, DistantLights, Cable, Path, Clouds.
- Unverified: how Renderer picks forward versus deferred, since Renderer.cs does not reference DeferredScene or PostProcessor directly.

## 7. Settings and GTA folder

GTA folder auto-detect (`CodeWalker/Utils/GTAFolder.cs:139-174`), in order:
1. The saved folder, if still valid.
2. Steam: HKLM `Software\Rockstar Games\GTAV`, value `InstallFolderSteam` (trailing `\GTAV` stripped).
3. Retail: HKLM `Software\Rockstar Games\Grand Theft Auto V`, value `InstallFolder`.
4. OpenIV: HKCU `Software\NewTechnologyStudio\OpenIV.exe\BrowseForFolder`, value `game_path_Five_pc`.

The first match is offered in a Yes/No prompt (GTAFolder.cs:77-85). No Epic Games lookup exists anywhere in the repo (grep for Epic, EpicGames and Launcher gives no hits).

Validation (GTAFolder.cs:25-59): the folder exists and has `gta5.exe` (Legacy) or `gta5_enhanced.exe` (Enhanced). The edition is inferred from `gta5_enhanced.exe` (GTAFolder.cs:20-23).

Startup (Program.cs:79): `UpdateGTAFolder(RememberGTAFolder)`. The default is True, so a valid saved folder skips the dialog. Otherwise: auto-detect prompt, then SelectFolderForm (`Utils/SelectFolderForm.cs`), with Retry on invalid. Sibling exes always pass `true`, so the remember flag only affects the main exe.

Persistence:
- `Properties/Settings.settings`: 79 user-scoped keys in the .NET user.config. `Shared/UserConfig.cs` patches the config's section groups on each start.
- Key settings: GTAFolder, GTAGen9, RememberGTAFolder, Key (AES key in base64, filled by the GTA5Keys scan and saved by GTAFolder.UpdateSettings, GTAFolder.cs:192), KeyBindings, XInput settings, MouseInvert, camera settings, RenderMode, ExcludeFolders, EnableMods, DLC, Region, RPFExplorer* (SelectedFolder, ExtraFolders, StartInEditMode, StartFolder), ExplorerWindowTheme, ProjectWindowTheme, JumpListInitialised, WindowMaximized, StartPosition, SavePosition.
- ModManager keeps its own copy in `CodeWalker.ModManager.ini` next to the exe: GameFolder, IsGen9, AESKey. It only saves if the file exists (SettingsFile.cs:36, SimpleKvpFile.cs:67). The shipped ini is a 3-byte UTF-8 BOM and works as a marker.

Key handling:
- Controls... opens KeyBindForm (`Utils/KeyBindForm.cs`, 62).
- 16 rebindable actions in `KeyBindings` (`Utils/InputUtils.cs:245-330`): MoveForward, MoveBackward, MoveLeft, MoveRight, MoveUp, MoveDown, MoveSlowerZoomIn, MoveFasterZoomOut, ToggleMouseSelect, ToggleToolbar, ExitEditMode, EditPosition, EditRotation, EditScale, Jump, FirstPerson.
- Hard-coded in WorldForm.KeyDown (WorldForm.cs:6480-6570): Delete; J and K for path linking; Ctrl+N, O, S (Shift = SaveAll), Z, Y, C, V, U; Esc returns to free control.
- Gamepad: SharpDX.XInput, controllers 1 to 4 (InputUtils.cs:55-58).

## 8. Unverified and not inspected

- CodeWalker.Core beyond the overlay and mods code.
- Bodies of Project/Panels and ProjectForm (names and sizes only). Tools/ dialogs (constructors only).
- Renderer's forward versus deferred selection; TextureLoader's exact use; vertex-format letters other than X.
- Epic Games detection: absent from the repo, not tested against an Epic install.
