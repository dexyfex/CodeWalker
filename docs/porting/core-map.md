# CodeWalker.Core porting map (what remains)

> **The C# sources are no longer in the working tree.** They are in git history at
> commit `c45ea83` (the upstream tip this port started from). Read a file with
> `git show c45ea83:CodeWalker.Core/GameFiles/RpfFile.cs`, or browse the whole tree with
> `git worktree add ../codewalker-csharp c45ea83`. Paths below refer to that commit.

Read-only map of `CodeWalker.Core/GameFiles/` (C#) against the Rust port in `crates/cw-core`. Paths are relative to `CodeWalker.Core/GameFiles/` unless shown otherwise. Line numbers come from `grep -n` / `sed -n`. **Verification status:** all binary offsets come from the C# source. The repo has no sample `.ytd`, `.ydr`, `.ymap`, `.ytyp` or `.rpf` files, so nothing was checked against game data. Items marked *unverified* need a test file first.

## 0. Already in Rust (skip)

| Need | Where in `crates/cw-core/src` |
|---|---|
| RPF7 index, names, AES, keys | `rpf.rs`, `keys.rs`, `crypto.rs` |
| Resource flags to segment size | `rpf.rs` `page_flags_size` (174), `resource_version` (189) |
| RSC7 header strip and inflate | `rpf.rs` `extract_resource` (433): returns `sys_flags`, `gfx_flags`, `version`, decompressed `data` |
| JenkHash | `hash.rs` `joaat`, `joaat_lower` |
| Extension to kind | `filekind.rs` (names only, no parsers) |

`extract_resource` returns the system segment followed by the graphics segment. Section 1 covers the rest: splitting that buffer and the typed block reader.

## 1. Resource system (RSC7 payload to typed blocks)

### 1.1 Constants

| Item | Value | Source |
|---|---|---|
| Header | 16 B: `RSC7` (0x37435352 LE) @0, version u32 @4, sysFlags u32 @8, gfxFlags u32 @12 | `RpfFile.cs` `CreateResourceFileEntry` (712) |
| Payload | raw DEFLATE from byte 16 (no zlib header) | `ResourceBuilder.cs` `Decompress` (674) |
| System base / graphics base | 0x50000000 / 0x60000000 | `Resources/ResourceData.cs` (45-46) |
| Segment size | base = 0x200 << (flags & 0xF), times multipliers: bit27 1, bit26 2, bit25 4, bit24 8, bits17-23 16, bits11-16 32, bits7-10 64, bits5-6 128, bit4 256. The page table and `BASE_SIZE` 0x2000 (`ResourceBuilder.cs` 14) are writer-only | `RpfFile.cs` `RpfResourcePageFlags.Size` (2800) |
| Version | `((sysFlags >> 28) << 4) \| (gfxFlags >> 28)`: ytd 13 (0x0D), ydr 165 (0xA5) | `RpfFile.cs` `GetVersionFromFlags` (2605) |

### 1.2 Pointer resolution

```
split:   sysSize = page_flags_size(sys_flags)
         system   = data[0 .. sysSize]
         graphics = data[sysSize .. sysSize + page_flags_size(gfx_flags)]

resolve(addr: u64):
    addr == 0                              -> null
    (addr & 0x50000000) == 0x50000000      -> (system,   addr & 0x0FFFFFFF)
    (addr & 0x60000000) == 0x60000000      -> (graphics, addr & 0x0FFFFFFF)
    otherwise                              -> error

read(seg, off, n): bytes seg[off .. off+n], zero-filled past the end (no error)
```

CodeWalker masks with `~0x50000000` / `~0x60000000` (`ResourceData.cs` 127-170), the same as `& 0x0FFFFFFF` while offsets stay below 256 MB. The system bit is tested first. Resources are little-endian.

### 1.3 Reader primitives (`Resources/ResourceData.cs`)

- `ReadBlockAt<T>(ptr)`: save position, seek, `ReadBlock<T>`, restore. `ptr == 0` gives null (221).
- `ReadBlock<T>()`: reads at the current position. Blocks are cached per address. A cache hit advances the position by `BlockLength` (172-188), so `BlockLength` must equal the bytes `Read` consumes.
- Arrays: `ReadUintsAt`, `ReadUshortsAt`, `ReadUlongsAt`, `ReadFloatsAt`, `ReadStructsAt<T>`, `ReadBytesAt` (252-392). Strings: `ReadStringAt`, UTF-8 NUL-terminated (`Utils/Data.cs` 206).
- 16-byte list headers (u64 ptr, u16 count, u16 capacity, u32 pad): `ResourceSimpleList64_uint` (`ResourceBaseTypes.cs` 1148) holds u32 items; `ResourcePointerList64<T>` (1562) holds `ReadBlockAt(ptr[i])` over a u64[capacity] array. `ResourcePointerListHeader` (1734) has the same shape.

### 1.4 Pitfalls

- `reader.IsGen9` is a global setting (`RpfManager.cs` 35). A file with a legacy version is forced back to legacy: YTD 13 (`YtdFile.cs` ~44-58), YDR 165 (`YdrFile.cs` ~44-53). Port legacy first. Gen9 is the `if (reader.IsGen9)` arm.
- The reader tolerates over-reads. The legacy texture path depends on that (2.4).

## 2. Texture dictionary (.ytd)

`YtdFile.cs` (148 lines) loads `TextureDictionary` (`Resources/Texture.cs`, 1402 lines). Version 13 legacy, 5 gen9. Root block at virtual 0x50000000.

### 2.1 TextureDictionary (64 B)

| Off | Field |
|---|---|
| 0x00 | ResourceFileBase (VFT, unk, pages ptr) |
| 0x20 | `TextureNameHashes` (ResourceSimpleList64_uint): JenkHash(lowercase name) per texture |
| 0x30 | `Textures` (ResourcePointerList64<Texture>): pointers to Texture blocks |

`hashes[i]` pairs with `textures[i]` (`BuildDict`; `Lookup(hash)`).

### 2.2 Texture (legacy, 144 B = TextureBase 0x00-0x4F + Texture 0x50-0x8F)

| Off | Type | Field |
|---|---|---|
| 0x28 | u64 | NamePointer: UTF-8 name, no extension. `NameHash = JenkHash(lower(name))` |
| 0x40 | u32 | UsageData: usage = `& 0x1F` (TextureUsage), flags = `>> 5` |
| 0x50 / 0x52 | u16 | Width / Height |
| 0x54 / 0x56 | u16 | Depth (1) / Stride (byte row pitch, see 2.4) |
| 0x58 | u32 | Format (2.3) |
| 0x5D | u8 | Levels (mip count) |
| 0x70 | u64 | DataPointer, graphics segment |

Source: `Texture.cs` 450-484 (TextureBase legacy), 917-943 (Texture legacy). Other fields are unknown.

### 2.3 Formats (`Texture.cs` 1125; DXGI mapping `DDSIO.cs` `GetDXGIFormat` 503)

| Name | Value | DXGI |
|---|---|---|
| D3DFMT_A8R8G8B8 / X8R8G8B8 | 21 / 22 | B8G8R8A8_UNORM / B8G8R8X8_UNORM |
| D3DFMT_A1R5G5B5 | 25 | B5G5R5A1_UNORM |
| D3DFMT_A8 | 28 | A8_UNORM |
| D3DFMT_A8B8G8R8 | 32 | R8G8B8A8_UNORM |
| D3DFMT_L8 | 50 | R8_UNORM |
| D3DFMT_DXT1 / DXT3 / DXT5 | FourCC "DXT1" / "DXT3" / "DXT5" | BC1 / BC2 / BC3 UNORM |
| D3DFMT_ATI1 / ATI2 | FourCC "ATI1" / "ATI2" | BC4 / BC5 UNORM |
| D3DFMT_BC7 | FourCC "BC7 " (0x20374342) | BC7_UNORM |

### 2.4 Pixel data

- `TextureData` (`Texture.cs`, class near 1072) is a graphics block at DataPointer. Mips are back to back, mip 0 first.
- Exact size per mip (`DDSIO.DXTex.ComputePitch`, `DDSIO.cs` 674): BC1/BC4 rowPitch = max(1,(w+3)/4) x 8, slice = rowPitch x max(1,(h+3)/4). BC2/3/5/6H/7 use x 16 per block. Uncompressed: rowPitch = w x bytes per pixel (A8, L8: 1; A1R5G5B5: 2; 32-bit: 4), slice = rowPitch x h. Mip i is max(1, w>>i) by max(1, h>>i). Total = sum of slices over Levels.
- **Do not port the legacy size loop.** `TextureData.Read` (`Texture.cs` 1103) uses `Stride * Height` for mip 0, then divides by 4 per mip. Stride is a byte row pitch, so for BC formats this reads about 4x the real size. C# survives it through zero-fill. *Unverified against real files.*
- `DDSIO.GetMipmapImages` (`DDSIO.cs` 538) already uses the exact sizes; use it as the reference. `GetDDSFile` (214) writes a DirectXTex-style header (`_EncodeDDSHeader`, ~1192), then the mips. BC7 header path *unverified*.
- Minimum for previews: Name (0x28), Width (0x50), Height (0x52), Format (0x58), Levels (0x5D), DataPointer (0x70). Gen9 (128 B texture, `TextureFormatG9` at `Texture.cs` 1144): skip for v1.

## 3. Meta, PSO and RBF; .ymap and .ytyp

### 3.1 Container detection

| Start | Container | Used by |
|---|---|---|
| `RSC7` + META root | Resource META, `ReadBlock<Meta>()` at 0x50000000 | ymap (`YmapFile.cs` 81, v2), ytyp (`YtypFile.cs` 177, v2), ymt, ymf |
| `PSIN` (BE 0x5053494E) | PSO sections PSIN, PMAP, PSCH, STRF, STRS, STRE (`MetaTypes/Pso.cs` 209-260) | non-RSC7 ymap, ytyp, ymt, cut, peds, vehicles |
| `RBF` + version byte (mask 0xFFFFFF00 == 0x52424600) | RBF (`Pso.cs` 344, `Rbf.cs` 219) | non-RSC7 files. PSO sections are big-endian (`Pso.cs` 216) |

### 3.2 META root (`MetaTypes/Meta.cs`, class `Meta` 18, 112 B after the ResourceFileBase)

| Off | Field |
|---|---|
| 0x10 | i32 `Unknown_10h` = 0x50524430 ("PRD0"), written by CodeWalker (18-40) |
| 0x1C | i32 RootBlockIndex (1-based) |
| 0x20 / 0x28 / 0x30 / 0x38 / 0x40 | i64 pointers: StructureInfos, EnumInfos, DataBlocks, Name, EncryptedStrings |
| 0x48 / 0x4A / 0x4C | i16 counts of the three arrays above |

Data block `MetaDataBlock` (`Meta.cs` 531, 16 B): u32 StructureNameHash (MetaName), i32 DataLength, i64 DataPointer. Bytes = `ReadBytesAt(DataPointer, DataLength)`. Block ids are 1-based (`GetBlock(id)` uses `id - 1`, 218).

### 3.3 Pointers inside META

- `MetaPOINTER` (8 B, `Meta.cs` 1101): `blockId = ptr & 0xFFF` (1-based, 0 = null), `offset = (ptr >> 12) & 0xFFFFF`.
- `Array_*` (16 B, `Meta.cs` 655 on): u64 Pointer (same encoding), u16 Count1, u16 Count2, u32 Unk1. Inline arrays (`Array_Structure`, `Array_uint`, `Array_Vector3`) need block `blockId-1` to carry the element type name, and may continue into the next blocks (`MetaTypes/MetaTypes.cs` `ConvertDataArray`, 1552). `Array_StructurePointer` points into a POINTER block of 8-byte MetaPOINTERs (`GetPointerArray`, 1684).
- Built-in block types are small codes, not hashes (`MetaTypeName`, `MetaTypes.cs` 2065): POINTER 0x07, STRING 0x10, BYTE 0x11, USHORT 0x13, UINT 0x15, FLOAT 0x21, VECTOR4 0x33, HASH 0x4A.

### 3.4 Naming trap

`MetaName` is the JenkHash of the exact-case type name. `CMapData` gives 3545841574, which is `MetaName.CMapData` (`MetaTypes/MetaNames.cs` 1033). An independent Jenkins computation agrees. The `Key:NNN` comments in `MetaTypes.cs` (CMapData `Key:3448101671`) are different numbers. Do not use them as lookup keys.

### 3.5 Struct layouts

The typed structs in `MetaTypes/MetaTypes.cs` carry byte offsets in `//NN` comments. Read them at fixed offsets. The generic structure-info interpreter (`Meta.cs` 231-420) is not needed for reading.

| Struct (`MetaTypes.cs` line) | Size | Fields needed (offset) |
|---|---|---|
| CMapData (2723) | 512 | name@8 (MetaHash), parent@12, flags@16 (bit0 scripted), streaming extents @32/@48, entities extents @64/@80, entities@96 (Array_StructurePointer to CEntityDef), containerLods@112, boxOccluders@128, occludeModels@144, physicsDictionaries@160 (Array_uint), instancedData@176, timeCycleModifiers@224, carGenerators@240, block@440 |
| CEntityDef (2766) | 128 | archetypeName@8, flags@12 (bit3 = LodInParentYmap), guid@16, position Vec3@32, rotation Vec4 x,y,z,w @48, scaleXY@64, scaleZ@68, parentIndex i32@72, lodDist f32@76, childLodDist@80, lodLevel enum@84, numChildren@88, priorityLevel@92, extensions@96, AO@112/116, tintValue@120 |
| CMloInstanceDef (2425) | 160 | CEntityDef@0, groupId@128, floorId@132, defaultEntitySets (Array_uint)@136, numExitPortals@152, MLOInstflags@156 |
| CMapTypes (2296) | 80 | extensions@8, archetypes (Array_StructurePointer)@24, name@40, dependencies (Array_uint)@48, compositeEntityTypes@64 |
| CBaseArchetypeDef (2313) | 144 | lodDist@8, flags@12, bbMin@32, bbMax@48, bsCentre@64, bsRadius@80, hdTextureDist@84, name@88, textureDictionary@92, clipDictionary@96, drawableDictionary@100, physicsDictionary@104, assetType enum@108, assetName@112, extensions@120 |
| CTimeArchetypeDef (2387) | 160 | base 144 + timeFlags@144 |
| CMloArchetypeDef (2412) | 240 | base 144 + mloFlags@144, entities@152, rooms@168 (CMloRoomDef, 112 B), portals@184, entitySets@200 (CMloEntitySet, 48 B), timeCycleModifiers@216 |

Enums: `rage__eLodType` HD 0, LOD 1, SLOD1 2, SLOD2 3, SLOD3 4, ORPHANHD 5, SLOD4 6. `rage__ePriorityLevel` REQUIRED 0, OPTIONAL_HIGH 1, OPTIONAL_MEDIUM 2, OPTIONAL_LOW 3. `eAssetType` UNINIT 0, FRAGMENT 1, DRAWABLE 2, DRAWABLEDICTIONARY 3, ASSETLESS 4.

### 3.6 Entity placement and LOD (`YmapFile.cs`)

- Entities = `CMapData.entities` as a CEntityDef array (`EnsureEntities`, 260-291). The array index is what `parentIndex` refers to.
- Position = Vec3@32. Rotation = quaternion (x, y, z, w) @48. Scale = (scaleXY, scaleXY, scaleZ) (1743). lodDist @76 is the draw distance (1454).
- LOD tree (315-330): for entity `d` with `parentIndex = p`, `d` is a root if `p` is out of range, or `d.flags` bit 3 is set, or `parent.lodLevel <= d.lodLevel`. Exception: an ORPHANHD parent does not make a non-ORPHANHD child a root. Otherwise `d` is a child of `p`.
- `CMapData.flags` bit 0 = IsScripted (45).

### 3.7 .ytyp (`YtypFile.cs`)

`Meta` to `CMapTypes` via `MetaName.CMapTypes` (216). The `archetypes` pointer array gives blocks named CBaseArchetypeDef, CTimeArchetypeDef or CMloArchetypeDef (237-255), read at `block.Data + offset`. GameFileCache key: `Hash = assetName` if non-zero, else `name` (`MetaTypes/Archetype.cs` 53-54).

Line counts: `Meta.cs` 1230, `MetaTypes.cs` 6593 (read 1480-1810, 2296-2840), `MetaNames.cs` 20688 (generated, lookup only), `Archetype.cs` 1054, `Pso.cs` 1087, `PsoTypes.cs` 16524 (generated, defer), `Rbf.cs` 442, `YmapFile.cs` 3666, `YtypFile.cs` 404. Writers and XML (`MetaBuilder`, `PsoBuilder`, `MetaXml`, `XmlMeta`) are skipped.

## 4. Drawable and fragment geometry (.ydr, .ydd, .yft)

Versions: YDR 165 legacy (159 gen9), YDD 165 / 159, YFT 162 / 171. Roots: `Drawable` (`Resources/Drawable.cs` 6680, 208 B), `DrawableDictionary` (7027), `FragType` (`Resources/Frag.cs` 41).

### 4.1 Chain (legacy offsets)

| Block (`Drawable.cs` line) | Size | Key fields |
|---|---|---|
| DrawableBase (5953, Read 6065) | 168 | ShaderGroupPtr@0x10, SkeletonPtr@0x18, BoundingCenter@0x20, BoundingSphereRadius@0x2C, BoundingBoxMin@0x30, BoundingBoxMax@0x40, DrawableModels High/Med/Low/VLow ptrs @0x50/58/60/68, LodDist f32 x4 @0x70, RenderMaskFlags x4 @0x80, JointsPtr@0x90, DrawableModelsPtr@0xA0 |
| ShaderGroup (14) | 64 | TextureDictionaryPtr@0x08 (embedded TXD, may be 0; same as section 2), ShadersPtr@0x10 (ShaderFX pointers), ShadersCount1 u16@0x18 |
| ShaderFX (178) | 48 | ParametersPtr@0x00, Name MetaHash@0x08, ParameterCount u8@0x10, RenderBucket@0x11, FileName MetaHash@0x18 (.sps) |
| ShaderParameter (537) | 16 | DataType u8@0 (0 texture, 1 Vec4, N = N x Vec4), DataPointer u64@8. Texture: a TextureBase (80 B, name at 0x28) |
| DrawableModelsBlock (3062) | varies | at DrawableModelsPtr, or the High pointer if that is 0. Per LOD: a 16-byte list header (3099-3140), then u64[] of DrawableModel pointers |
| DrawableModel (3290, Read 3410) | 48 | GeometriesPtr@0x08 (u64[]), GeometriesCount1 u16@0x10, BoundsPtr@0x18, ShaderMappingPtr@0x20 (u16[]), SkeletonBinding@0x28, RenderMaskFlags u16@0x2C, GeometriesCount3 u16@0x2E |
| DrawableGeometry (3665, Read 3721) | 152 | VertexBufferPtr@0x18, IndexBufferPtr@0x38, IndicesCount u32@0x58, TrianglesCount u32@0x5C, VerticesCount u16@0x60, BoneIdsPtr@0x68, VertexStride u16@0x70, BoneIdsCount u16@0x72, VertexDataPtr@0x78 |
| VertexBuffer (3971, Read 4014) | 128 | VertexStride u16@0x08, Flags u16@0x0A, DataPointer1 (VertexData)@0x10, VertexCount u32@0x18, DataPointer2@0x20, InfoPtr (VertexDeclaration)@0x30 |
| VertexDeclaration (4930, Read 4944) | 16 | Flags u32@0, Stride u16@4, Count u8@7, Types u64@8 |
| VertexData (4392, Read 4423) | count x stride | raw bytes at DataPointer1 (primary stream Data1, else Data2) |
| IndexBuffer (5446, Read 5485) | 96 | IndicesCount u32@0x08, IndicesPointer u64@0x10: u16[IndicesCount] |

Links: `ShaderID = ShaderMapping[i]` (`Drawable.cs` 3436); material = `ShaderGroup.Shaders[ShaderID]` (6458). Bounds: `BoundsData[i+1]` when there is more than one geometry, else `BoundsData[0]`. LOD choice: DrawableModelsPtr when non-zero, else High.

### 4.2 Vertex layout rules

- Flags bit k (k = 0..15) set means slot k exists. Semantic: 0 Position, 1 BlendWeights, 2 BlendIndices, 3 Normal, 4 Colour0, 5 Colour1, 6-13 TexCoord0-7, 14 Tangent, 15 Binormal (`Resources/VertexType.cs` `VertexSemantics`, 86).
- Component type of slot k = `(Types >> 4k) & 0xF` (`GetComponentType`, 5018). Slot offset = sum of sizes of the set slots below k (`GetComponentOffset`, 5024); stride = sum over all set slots. Codes (`VertexType.cs` 10): 1 Half2 (4 B), 2 Float (4), 3 Half4 (8), 5 Float2 (8), 6 Float3 (12), 7 Float4 (16), 8 UByte4 (4), 9 Colour (4), 10 RGBA8SNorm (4). Codes 0, 4, 11-15 have size 0 in the table: unknown.
- Known declarations (`VertexDeclarationTypes`, `VertexType.cs` 72): GTAV1 0x7755555555996996 (most YDRs), GTAV2 0x030000000199A006 and GTAV3 0x0300000001996006 (YFT), GTAV4 0x7655555555996996 (glass). `VertexType` (106) is the Flags word of named layouts, e.g. Default = 89 = bits 0, 3, 4, 6 (PNCT). Decode with the bit rules.
- Colour channel order and index topology are *unverified*. Triangle list is assumed (`TrianglesCount = IndicesCount / 3` in `Utils/FbxConverter.cs` 622).

### 4.3 Minimal .ydr path, YDD, YFT

1. Split the RSC7 payload (section 1). 2. `ReadBlock<Drawable>` at 0x50000000 (`YdrFile.Load`). 3. ShaderGroup, then ShaderFX list. 4. LOD 0 models: each geometry's VertexBuffer Data1 with its VertexDeclaration, plus IndexBuffer. 5. Textures: `ShaderGroup.TextureDictionary` if set, otherwise the archetype YTD and parent chain (section 6).
- Texture slot meaning is not in the file. `Utils/FbxConverter.cs` (713-726) assumes slot 0 = DiffuseSampler, slot 1 = BumpSampler (`ShaderParams.cs` 15).
- YDD: `DrawableDictionary` (7027), `HashesPointer` at 0x20 holds u32[] hashes (7088), then the drawable list. YFT: `FragType` (`Frag.cs` 41) embeds `FragDrawable` (`Frag.cs` 94); skip physics for previews.

Line counts: `Drawable.cs` 7190 (read 14-180, 537-600, 3062-3720, 3971-4960, 5446-5560, 5953-6200, 7027-7100), `VertexType.cs` 195, `YdrFile.cs` 153, `YddFile.cs` 182, `YftFile.cs` 153, `Frag.cs` 4471 (root only).

## 5. Other formats (one row each)

| Ext | Root class (file, lines) | Container and version | Notes |
|---|---|---|---|
| ybn | Bounds (`Resources/Bounds.cs` 5488; `YbnFile.cs` 210) | RSC7 v43 | BoundsType (`Bounds.cs` 101): Sphere 0, Capsule 1, Box 3, Geometry 4, BVH 8, Composite 10, Disc 12, Cylinder 13, Cloth 15 |
| ynd | NodeDictionary (`Node.cs` 572; `YndFile.cs` 1913) | RSC7 v1 | path nodes and links |
| ynv | NavMesh (`Nav.cs` 1144; `YnvFile.cs` 1406) | RSC7 v2 | sectors, polygons |
| ycd | ClipDictionary (`Clip.cs` 4466; `YcdFile.cs` 355) | RSC7 | animation, property maps |
| yed | ExpressionDictionary (`Expression.cs` 1650; `YedFile.cs` 125) | RSC7 | face expressions |
| ypt | ParticleEffectsList (`Particle.cs` 6166; `YptFile.cs` 243) | RSC7 v68 (gen9 71) | |
| yld | ClothDictionary (`Clothes.cs` 2643; `YldFile.cs` 133) | RSC7 | |
| yvr / ywr | VehicleRecordList (`VehicleRecord.cs` 275) / WaypointRecordList (`WaypointRecord.cs` 173) | RSC7 | `YvrFile.cs` 110, `YwrFile.cs` 111 |
| ymt / ymf | Meta (`YmtFile.cs` 324) / Meta manifest (`YmfFile.cs` 189) | RSC7 v2, or PSO/RBF | ymf holds HDTxdAssetBindings |
| yfd | FrameFilterDictionary (`Filter.cs` 454; `YfdFile.cs` 97) | RSC7 | |
| ypdb | `YpdbFile.cs` 411 | unverified | purpose unverified |
| rel | `RelFile.cs` 24909 (mostly tables) | NOT RSC7: u32 type, u32 len, data, u32 nameTableLen, u32 nameCount, u32 offsets[], NUL names, u32 indexCount (106-160) | audio; names go to JenkIndex |
| awc | `AwcFile.cs` 3169 | binary; `RpfFile.cs` ~1698 special-cases `.awc` | layout *unverified* |
| gxt2 | `Gxt2File.cs` 260 | binary text table (hash to text) | names, see 6 |
| cut | `CutFile.cs` 1274 | PSO/RBF (`rage__cutfCutsceneFile2`) | |
| ysc | none in CodeWalker.Core | `.ysc` entries flagged encrypted (`RpfFile.cs` 231); extraction skips 16 B, then AES (~419-440) | scripts, no parser |
| nametable | none standalone | `.nametable` RPF entries are NUL-separated strings (`RpfManager.cs` 447-470) | feed JenkIndex |
| dat: cache | `CacheDatFile.cs` 1074 | `gta5_cache_y.dat`: text version, then sections fwMapDataStore, CInteriorProxy, BoundsStoreItem | map hierarchy |
| dat: heightmap, water | `HeightmapFile.cs` 362 (`HMAP`); `WatermapFile.cs` 550 (`WMAP`) | binary grids | `heightmap*.dat`, `waterheight*.dat` |
| dat: lights, dlc | `DistantLightsFile.cs` 326; `DlcContentFile.cs` 431; `DlcSetupFile.cs` 83 | | |
| mrf / fxc | `MrfFile.cs` 5613 (`MoVE`); `FxcFile.cs` 1896 (compiled shaders) | | skip for an explorer |
| gtxd | `GtxdFile.cs` 160 | PSO/RBF meta | TXD parent map (6) |
| game tables | `PedsFile.cs` 347, `PedFile.cs` 124, `VehiclesFile.cs` 401, `CarColsFile.cs` 1278, `CarModColsFile.cs` 211, `CarVariationsFile.cs` 261, `VehicleLayoutsFile.cs` 37, `JPsoFile.cs` 48, `Stats.cs` 84 | PSO/RBF/XML meta | |

## 6. GameFileCache (`GameFileCache.cs`, 5981 lines)

Init order (`Init` 172, `InitGlobal` 283, `InitDlc` 293): `InitGlobalDicts` 964, `InitDlcList` 331, `InitMapDicts` 1010, `InitManifestDicts` 1060, `InitGtxds` 1121, `InitMapCaches` 1208, `InitArchetypeDicts` 1277, `InitStringDicts` 1385, `InitVehicles` 1480, `InitPeds` 1599, `InitAudio` 1795.

1. **Name to entry maps** (`YdrDict`, `YddDict`, `YtdDict`, `YftDict`, `YcdDict`, `YedDict`, `YbnDict`, `YmapDict`, `YnvDict`, `Gxt2Dict`). Key = `ShortNameHash` = JenkHash(lowercase name without last extension) (`RpfFile.cs` `GetShortNameLower`, ~2189).
2. **Archetype map** `archetypeDict[uint]`, built from every `.ytyp` (`AddYtypToDictionary`, ~1331-1360). Key = `Archetype.Hash`. `GetArchetype` 2062. `projectArchetypes` override for the project window. YMAP access: `GetYmap` 2196, `GetYmapEntry` 2430.
3. **LOD tree**: within a ymap by `parentIndex` (`YmapFile.cs` 315-330). Across ymaps by `YmapHierarchyDict`, built from `MapDataStoreNode` entries in `gta5_cache_y.dat` (`InitMapCaches`, 1208-1235; `GetMapNode` 2071).
5. **Texture lookup**: `textureLookup` maps texture name hash to YTD entry. Filled only from YTDs already loaded (`AddTextureLookups`, 2603); init loads just mapdetail and vehshare. `TryGetTextureDictForTexture` 2616.
6. **Parent TXD chain**: `textureParents` from `TxdRelationships` of `gtxd.ymt`, `gtxd.meta`, `mph4_gtxd.ymt`, `vehicles.meta` (`InitGtxds`, 1121-1190). `TryFindTextureInParent(tex, txd)` (2654) walks it when the archetype YTD misses.
7. **Lazy load and cache**: `GetYtd` 2157, `GetYdr` 2079 and others check `projectFiles`, then `mainCache` (`Cache<GameFileCacheKey, GameFile>`, default 2 GB, line 149), else create the file and queue an async load. `ContentThreadProc` 2502 does `MaxItemsPerLoop = 1` per tick (line 19). Callers must handle `Loaded == false`. `hdtexturelookup` (HD texture swap) comes from each YMF's `HDTxdAssetBindings` (`InitManifestDicts`, 1060-1118).
8. **DLC and mods**: `update/update.rpf/common/data/dlclist.xml` (`InitDlcList`, 331-360); `setup2.xml` patch map `DlcPatchedPaths` (`InitDlc`, ~380-445); per-DLC `<dlc>_<hash>_cache_y.dat` (637); `EnableMods` via `RpfManager.ModRpfDict` / `ModEntryDict`, `SetModsEnabled` 1950.
9. **Side tables**: `VehiclesInitDict` (vehicles.meta), `PedsInitDict` and `Ped{Drawable,Texture,Cloth}Dicts`, `AudioXxxDict` from REL files (`InitAudio`), `TimeCycleModsDict`.
10. **Strings**: `JenkIndex` (`Utils/Jenk.cs` 199) is the global hash-to-text map. `RpfManager.BuildBaseJenkIndex` (353) fills it from RPF and entry names, `.nametable` strings (447-470), "00".."99", and `strings.txt` (504). GXT2 text: `x64b.rpf\data\lang\american_rel.rpf\global.gxt2` (`InitStringDicts`); with `DoFullStringIndex`, all `*.gxt2` and `statssetup.xml`.

Files the cache depends on: `CodeWalker.Core/strings.txt` (422 lines, embedded as `CodeWalker.Files/strings.txt`, `CodeWalker.Core.csproj` 21; `//` lines are comments; gen9 shader names); `gta5_cache_y.dat` under `common.rpf/data` or `update/update.rpf/common/data` (`InitMapCaches`, 1251-1259); `dlclist.xml`, `setup2.xml`, `gtxd.ymt` / `gtxd.meta`, `vehicles.meta`, `*.ymf`, `heightmap*.dat`, `waterheight*.dat`. No "allnames" file exists in this repo (grep: 0 hits).

**Unverified:** all binary offsets (no sample assets); texture `Stride` values; BC7 DDS header; colour channel order; triangle-list topology; YDD offsets past 0x20; YPDB purpose.
