using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class GenerateLODLightsPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }

        const float MAX_LODLIGHT_INTENSITY = 48.0f;
        const float MAX_LODLIGHT_CONE_ANGLE = 180.0f;
        const float MAX_LODLIGHT_CAPSULE_EXTENT = 140.0f;
        const float MAX_LODLIGHT_CORONA_INTENSITY = 32.0f;
        const uint LIGHTFLAG_DONT_USE_IN_CUTSCENE = (1u << 2);
        const uint LIGHTFLAG_VEHICLE = (1u << 3);
        const uint LIGHTFLAG_FX = (1u << 4);
        const uint LIGHTFLAG_ENABLE_BUZZING = (1u << 10);
        const uint LIGHTFLAG_FORCE_BUZZING = (1u << 11);
        const uint LIGHTFLAG_CORONA_ONLY = (1u << 15);
        const uint LIGHTFLAG_FAR_LOD_LIGHT = (1u << 22);
        const uint LIGHTFLAG_MOVING_LIGHT_SOURCE = (1u << 26);
        const uint LIGHTFLAG_FORCE_MEDIUM_LOD_LIGHT = (1u << 28);
        const uint LIGHTFLAG_CORONA_ONLY_LOD_LIGHT = (1u << 29);
        const int LIGHT_CATEGORY_SMALL = 0;
        const int LIGHT_CATEGORY_MEDIUM = 1;
        const int LIGHT_CATEGORY_LARGE = 2;

        const int MAX_LIGHTS_PER_CELL = 800;
        const int MIN_LIGHT_COUNT_TO_CONSOLIDATE = 100;

        static readonly string[] CategoryLabels = { "small", "medium", "large" };


        public GenerateLODLightsPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();

            if (ProjectForm?.WorldForm == null)
            {
                GenerateButton.Enabled = false;
                UpdateStatus("Unable to generate - World View not available!");
            }
        }


        public void SetProject(ProjectFile project)
        {
            CurrentProjectFile = project;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select output directory for LOD light ymaps";
                if (!string.IsNullOrEmpty(OutputPathTextBox.Text) && Directory.Exists(OutputPathTextBox.Text))
                {
                    dialog.SelectedPath = OutputPathTextBox.Text;
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    OutputPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void GenerateComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { GenerateComplete(); }));
                }
                else
                {
                    GenerateButton.Enabled = true;
                }
            }
            catch { }
        }


        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }

        private static byte PackU8(float val, float range)
        {
            return (byte)Math.Max(Math.Min(Math.Round(val * (255.0f / range)), 255), 0);
        }

        private static int GetLightCategory(LightAttributes la, float capsuleExtent)
        {
            uint flags = la.Flags;

            if ((flags & LIGHTFLAG_FAR_LOD_LIGHT) != 0)
            {
                return LIGHT_CATEGORY_LARGE;
            }

            float length = la.Falloff;
            if (la.Type == LightType.Capsule)
            {
                length = 2.0f * la.Falloff + capsuleExtent;
            }

            if ((flags & LIGHTFLAG_FORCE_MEDIUM_LOD_LIGHT) != 0 || (length >= 10.0f && la.Intensity >= 1.0f))
            {
                return LIGHT_CATEGORY_MEDIUM;
            }

            return LIGHT_CATEGORY_SMALL;
        }


        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var gameFileCache = ProjectForm?.WorldForm?.GameFileCache;
            if (gameFileCache == null) return;

            var outputDir = OutputPathTextBox.Text;
            if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            {
                MessageBox.Show("Please select a valid output directory.");
                return;
            }

            GenerateButton.Enabled = false;

            List<YmapFile> projectYmaps = ProjectForm.CurrentProjectFile.YmapFiles;

            var pname = NameTextBox.Text;

            Task.Run(async () =>
            {
                var lightsSmall = new List<Light>();
                var lightsMedium = new List<Light>();
                var lightsLarge = new List<Light>();

                // Collect all entities and deduplicate archetypes for batch loading
                var allEntities = new List<(YmapEntityDef ent, string entName)>();
                var uniqueArchetypes = new HashSet<uint>();

                foreach (var ymap in projectYmaps)
                {
                    if (ymap?.AllEntities == null) continue;
                    foreach (var ent in ymap.AllEntities)
                    {
                        if (ent.Archetype == null) continue;
                        var entName = ent.Archetype.Name?.ToString() ?? "";
                        if (entName.Contains("prop_dock_bouy")) continue;
                        allEntities.Add((ent, entName));
                        uniqueArchetypes.Add(ent.Archetype.Hash);
                    }
                }

                // Pre-request all unique drawables so they start loading in parallel
                UpdateStatus($"Requesting {uniqueArchetypes.Count} unique drawables...");
                var drawableCache = new Dictionary<uint, DrawableBase>();
                var pendingArchetypes = new HashSet<uint>();

                foreach (var (ent, _) in allEntities)
                {
                    var hash = ent.Archetype.Hash;
                    if (drawableCache.ContainsKey(hash) || pendingArchetypes.Contains(hash)) continue;

                    var (dwbl, waiting) = await gameFileCache.TryGetDrawableAsync(ent.Archetype);
                    if (dwbl != null)
                    {
                        drawableCache[hash] = dwbl;
                    }
                    else if (waiting)
                    {
                        pendingArchetypes.Add(hash);
                    }
                }

                // Wait for all pending drawables to finish loading (10s timeout per drawable)
                if (pendingArchetypes.Count > 0)
                {
                    UpdateStatus($"Waiting for {pendingArchetypes.Count} drawables to load...");
                    var archetypeLookup = new Dictionary<uint, Archetype>();
                    foreach (var (ent, _) in allEntities)
                    {
                        var hash = ent.Archetype.Hash;
                        if (pendingArchetypes.Contains(hash) && !archetypeLookup.ContainsKey(hash))
                        {
                            archetypeLookup[hash] = ent.Archetype;
                        }
                    }

                    var pendingTimers = new Dictionary<uint, System.Diagnostics.Stopwatch>();
                    foreach (var hash in pendingArchetypes)
                    {
                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        pendingTimers[hash] = sw;
                    }

                    while (pendingArchetypes.Count > 0)
                    {
                        await Task.Delay(3);
                        var resolved = new List<uint>();
                        foreach (var hash in pendingArchetypes)
                        {
                            var (dwbl, waiting) = await gameFileCache.TryGetDrawableAsync(archetypeLookup[hash]);
                            if (dwbl != null)
                            {
                                drawableCache[hash] = dwbl;
                                resolved.Add(hash);
                            }
                            else if (!waiting)
                            {
                                resolved.Add(hash);
                            }
                            else if (pendingTimers[hash].Elapsed.TotalMinutes >= 1.0)
                            {
                                UpdateStatus($"Timed out loading {hash} after 1 minute, skipping...");
                                resolved.Add(hash);
                            }
                        }
                        foreach (var hash in resolved)
                        {
                            pendingArchetypes.Remove(hash);
                            pendingTimers.Remove(hash);
                        }
                        if (pendingArchetypes.Count > 0)
                        {
                            UpdateStatus($"Waiting for {pendingArchetypes.Count} drawables to load...");
                        }
                    }
                }

                // Process all entities using cached drawables
                UpdateStatus($"Processing {allEntities.Count} entities...");
                foreach (var (ent, entName) in allEntities)
                {
                    if (!drawableCache.TryGetValue(ent.Archetype.Hash, out var dwbl)) continue;

                    ent.EnsureLights(dwbl);
                    var elights = ent.Lights;
                    if (elights == null) continue;

                    var archBB = new BoundingBox(ent.Archetype.BBMin, ent.Archetype.BBMax).Transform(ent.Position, ent.Orientation, ent.Scale);
                    var hashInts = new uint[7];
                    hashInts[0] = (uint)(int)(archBB.Minimum.X * 10.0f);
                    hashInts[1] = (uint)(int)(archBB.Minimum.Y * 10.0f);
                    hashInts[2] = (uint)(int)(archBB.Minimum.Z * 10.0f);
                    hashInts[3] = (uint)(int)(archBB.Maximum.X * 10.0f);
                    hashInts[4] = (uint)(int)(archBB.Maximum.Y * 10.0f);
                    hashInts[5] = (uint)(int)(archBB.Maximum.Z * 10.0f);
                    int exts = ent.Archetype.Extensions?.Length ?? 0;

                    bool isStreetLight = entName.Contains("streetlight") || entName.Contains("street_light") || entName.Contains("nylamp") || entName.Contains("nytraf");

                    for (int li = 0; li < elights.Length; li++)
                    {
                        var elight = elights[li];
                        var la = elight.Attributes;

                        if (la.LightFadeDistance > 0) continue;

                        uint flags = la.Flags;

                        // skip vehicle, FX, moving, and buzzing/flashing lights - these don't belong in LOD lights
                        if ((flags & (LIGHTFLAG_VEHICLE | LIGHTFLAG_FX | LIGHTFLAG_MOVING_LIGHT_SOURCE | LIGHTFLAG_ENABLE_BUZZING | LIGHTFLAG_FORCE_BUZZING)) != 0)
                            continue;

                        // skip flashing lights (Flashiness > 0 means the light flickers/flashes at runtime)
                        if (la.Flashiness > 0) continue;

                        bool isCoronaOnly = (flags & (LIGHTFLAG_CORONA_ONLY | LIGHTFLAG_CORONA_ONLY_LOD_LIGHT)) != 0;

                        // skip lights with zero intensity unless they are corona-only
                        if (la.Intensity <= 0 && !isCoronaOnly) continue;

                        // skip lights that are too small to matter as LOD lights (tiny falloff and low intensity)
                        if (la.Falloff < 2.0f && la.Intensity < 0.5f && !isCoronaOnly && (flags & LIGHTFLAG_FAR_LOD_LIGHT) == 0)
                            continue;

                        uint type = (uint)la.Type;
                        float capsuleExtent = la.Extent.X;

                        if (type == (uint)LightType.Capsule)
                        {
                            float minCapsuleExtent = MAX_LODLIGHT_CAPSULE_EXTENT / 255.0f;
                            if (capsuleExtent < minCapsuleExtent)
                            {
                                type = (uint)LightType.Point;
                            }
                        }

                        uint r = la.ColorR;
                        uint g = la.ColorG;
                        uint b = la.ColorB;
                        uint packedIntensity = PackU8(la.Intensity, MAX_LODLIGHT_INTENSITY);
                        uint colour = (packedIntensity << 24) + (r << 16) + (g << 8) + b;

                        byte inner = PackU8(la.ConeInnerAngle, MAX_LODLIGHT_CONE_ANGLE);
                        byte outer;
                        if (type == (uint)LightType.Capsule)
                        {
                            outer = PackU8(capsuleExtent, MAX_LODLIGHT_CAPSULE_EXTENT);
                        }
                        else
                        {
                            outer = PackU8(la.ConeOuterAngle, MAX_LODLIGHT_CONE_ANGLE);
                        }

                        byte packedCorona = 0;
                        if (la.CoronaSize >= 0.05f && la.CoronaIntensity > 0)
                        {
                            packedCorona = PackU8(la.CoronaIntensity, MAX_LODLIGHT_CORONA_INTENSITY);
                        }

                        // if TimeFlags is 0 the source light has no time restriction - default to always on
                        // in the LOD system 0 means "never visible" so we must set all 24 hour bits
                        uint rawTimeFlags = la.TimeFlags & 0x00FFFFFFu;
                        uint timeAndState = (rawTimeFlags != 0) ? rawTimeFlags : 0x00FFFFFFu;
                        if (isStreetLight)
                        {
                            timeAndState |= (1u << 24);
                        }
                        if (isCoronaOnly)
                        {
                            timeAndState |= (1u << 25);
                        }
                        timeAndState |= (type << 26);
                        if ((flags & LIGHTFLAG_DONT_USE_IN_CUTSCENE) != 0)
                        {
                            timeAndState |= (1u << 31);
                        }

                        var light = new Light();
                        light.position = new MetaVECTOR3(elight.Position);
                        light.colour = colour;
                        light.direction = new MetaVECTOR3(elight.Direction);
                        light.falloff = la.Falloff;
                        light.falloffExponent = la.FalloffExponent;
                        light.timeAndStateFlags = timeAndState;
                        hashInts[6] = (uint)(exts + li);
                        light.hash = YmapEntityDef.ComputeLightHash(hashInts);
                        light.coneInnerAngle = inner;
                        light.coneOuterAngleOrCapExt = outer;
                        light.coronaIntensity = packedCorona;
                        light.isStreetLight = isStreetLight;

                        int category = GetLightCategory(la, capsuleExtent);
                        switch (category)
                        {
                            case LIGHT_CATEGORY_LARGE:
                                lightsLarge.Add(light);
                                break;
                            case LIGHT_CATEGORY_MEDIUM:
                                lightsMedium.Add(light);
                                break;
                            default:
                                lightsSmall.Add(light);
                                break;
                        }
                    }
                }

                int totalLights = lightsSmall.Count + lightsMedium.Count + lightsLarge.Count;

                if (totalLights == 0)
                {
                    MessageBox.Show("No lights found in project!");
                    GenerateComplete();
                    return;
                }

                UpdateStatus($"Collected {totalLights} lights (S:{lightsSmall.Count} M:{lightsMedium.Count} L:{lightsLarge.Count}). Chopping into grid cells...");

                var categoryLights = new[] { lightsSmall, lightsMedium, lightsLarge };
                var allYmaps = new List<YmapFile>();

                for (int cat = 0; cat < 3; cat++)
                {
                    var lights = categoryLights[cat];
                    if (lights.Count == 0) continue;

                    var cells = ChopLightsIntoGrid(lights);

                    UpdateStatus($"Building {cells.Count} ymap pairs for {CategoryLabels[cat]} category...");

                    for (int ci = 0; ci < cells.Count; ci++)
                    {
                        var cell = cells[ci];

                        cell.Lights.Sort((a, b) =>
                        {
                            if (a.isStreetLight != b.isStreetLight) return b.isStreetLight.CompareTo(a.isStreetLight);
                            return a.hash.CompareTo(b.hash);
                        });

                        var (lodymap, distymap) = BuildYmapPair(cell.Lights, pname, cat, ci);
                        allYmaps.Add(lodymap);
                        allYmaps.Add(distymap);
                    }
                }

                UpdateStatus($"Saving {allYmaps.Count} ymaps to {outputDir}...");

                foreach (var ymap in allYmaps)
                {
                    var data = ymap.Save();
                    if (data != null)
                    {
                        var filePath = Path.Combine(outputDir, ymap.RpfFileEntry.Name);
                        File.WriteAllBytes(filePath, data);
                    }
                }

                int totalYmaps = allYmaps.Count / 2;
                UpdateStatus($"Process complete. {totalLights} lights (S:{lightsSmall.Count} M:{lightsMedium.Count} L:{lightsLarge.Count}) in {totalYmaps} ymap pairs saved to {outputDir}");
                GenerateComplete();
            });
        }


        #region Grid Chopping

        private class GridCell
        {
            public float StartX;
            public float StartY;
            public float Width;
            public float Height;
            public List<Light> Lights = new List<Light>();
        }

        private static List<GridCell> ChopLightsIntoGrid(List<Light> lights)
        {
            if (lights.Count == 0) return new List<GridCell>();

            var cell = new GridCell();
            cell.Lights.AddRange(lights);
            UpdateCellExtentsFromLights(cell);

            var cells = new List<GridCell> { cell };

            SubdivideOverpopulatedCells(cells);
            ConsolidateSparseCells(cells);
            MakeCellsSquareIsh(cells);

            cells.RemoveAll(c => c.Lights.Count == 0);

            return cells;
        }

        private static void UpdateCellExtentsFromLights(GridCell cell)
        {
            if (cell.Lights.Count == 0) return;
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = -float.MaxValue, maxY = -float.MaxValue;
            foreach (var light in cell.Lights)
            {
                float x = light.position.x;
                float y = light.position.y;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
            cell.StartX = minX;
            cell.StartY = minY;
            cell.Width = maxX - minX;
            cell.Height = maxY - minY;
        }

        private static void SubdivideOverpopulatedCells(List<GridCell> cells)
        {
            // Iterate including newly added cells
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.Lights.Count > MAX_LIGHTS_PER_CELL && cell.Width > 1.0f && cell.Height > 1.0f)
                {
                    var newCells = DivideCellVH(cell);
                    cells.AddRange(newCells);
                }
            }
            cells.RemoveAll(c => c.Lights.Count == 0);
        }

        private static List<GridCell> DivideCellVH(GridCell cell)
        {
            float halfW = cell.Width / 2.0f;
            float halfH = cell.Height / 2.0f;

            var tl = new GridCell { StartX = cell.StartX, StartY = cell.StartY, Width = halfW, Height = halfH };
            var tr = new GridCell { StartX = cell.StartX + halfW, StartY = cell.StartY, Width = halfW, Height = halfH };
            var bl = new GridCell { StartX = cell.StartX, StartY = cell.StartY + halfH, Width = halfW, Height = halfH };
            var br = new GridCell { StartX = cell.StartX + halfW, StartY = cell.StartY + halfH, Width = halfW, Height = halfH };

            var newCells = new List<GridCell> { tl, tr, bl, br };
            MoveLightsToClosestCell(cell, newCells);
            foreach (var nc in newCells) UpdateCellExtentsFromLights(nc);
            return newCells;
        }

        private static List<GridCell> DivideCellVByCount(GridCell cell, int count)
        {
            float newWidth = cell.Width / count;
            var newCells = new List<GridCell>();
            for (int i = 0; i < count; i++)
            {
                newCells.Add(new GridCell
                {
                    StartX = cell.StartX + newWidth * i,
                    StartY = cell.StartY,
                    Width = newWidth,
                    Height = cell.Height
                });
            }
            MoveLightsToClosestCell(cell, newCells);
            foreach (var nc in newCells) UpdateCellExtentsFromLights(nc);
            return newCells;
        }

        private static List<GridCell> DivideCellHByCount(GridCell cell, int count)
        {
            float newHeight = cell.Height / count;
            var newCells = new List<GridCell>();
            for (int i = 0; i < count; i++)
            {
                newCells.Add(new GridCell
                {
                    StartX = cell.StartX,
                    StartY = cell.StartY + newHeight * i,
                    Width = cell.Width,
                    Height = newHeight
                });
            }
            MoveLightsToClosestCell(cell, newCells);
            foreach (var nc in newCells) UpdateCellExtentsFromLights(nc);
            return newCells;
        }

        private static void MoveLightsToClosestCell(GridCell source, List<GridCell> targets)
        {
            foreach (var light in source.Lights)
            {
                float x = light.position.x;
                float y = light.position.y;
                float bestDist = float.MaxValue;
                GridCell bestCell = targets[0];
                foreach (var target in targets)
                {
                    float dist = DistToRectSq(x, y, target);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestCell = target;
                    }
                }
                bestCell.Lights.Add(light);
            }
            source.Lights.Clear();
        }

        private static float DistToRectSq(float x, float y, GridCell cell)
        {
            float dx = 0, dy = 0;
            float minX = cell.StartX;
            float maxX = cell.StartX + cell.Width;
            float minY = cell.StartY;
            float maxY = cell.StartY + cell.Height;
            if (x < minX) dx = x - minX;
            else if (x > maxX) dx = x - maxX;
            if (y < minY) dy = y - minY;
            else if (y > maxY) dy = y - maxY;
            return dx * dx + dy * dy;
        }

        private static void ConsolidateSparseCells(List<GridCell> cells)
        {
            int nCells = cells.Count;
            for (int i = 0; i < nCells; i++)
            {
                var cell = cells[i];
                int nLights = cell.Lights.Count;
                if (nLights > 0 && nLights < MIN_LIGHT_COUNT_TO_CONSOLIDATE)
                {
                    var otherCells = new List<GridCell>();
                    for (int k = 0; k < nCells; k++)
                    {
                        if (k != i && cells[k].Lights.Count > MIN_LIGHT_COUNT_TO_CONSOLIDATE)
                        {
                            otherCells.Add(cells[k]);
                        }
                    }
                    if (otherCells.Count > 0)
                    {
                        foreach (var light in cell.Lights)
                        {
                            float x = light.position.x;
                            float y = light.position.y;
                            float bestDist = float.MaxValue;
                            GridCell bestCell = otherCells[0];
                            foreach (var other in otherCells)
                            {
                                float dist = DistToRectSq(x, y, other);
                                if (dist < bestDist)
                                {
                                    bestDist = dist;
                                    bestCell = other;
                                }
                            }
                            bestCell.Lights.Add(light);
                        }
                        cell.Lights.Clear();
                        foreach (var other in otherCells) UpdateCellExtentsFromLights(other);
                    }
                }
            }
            cells.RemoveAll(c => c.Lights.Count == 0);
        }

        private static void MakeCellsSquareIsh(List<GridCell> cells)
        {
            int nCells = cells.Count;
            for (int i = 0; i < nCells; i++)
            {
                var cell = cells[i];
                if (cell.Lights.Count == 0 || cell.Height <= 0 || cell.Width <= 0) continue;

                float whRatio = cell.Width / cell.Height;
                if (whRatio > 2.0f)
                {
                    var newCells = DivideCellVByCount(cell, (int)whRatio);
                    cells.AddRange(newCells);
                }
                else if (whRatio < 0.5f)
                {
                    var newCells = DivideCellHByCount(cell, (int)(1.0f / whRatio));
                    cells.AddRange(newCells);
                }
            }
            cells.RemoveAll(c => c.Lights.Count == 0);
        }

        #endregion


        private (YmapFile lodymap, YmapFile distymap) BuildYmapPair(List<Light> lights, string pname, int category, int cellIndex)
        {
            var position = new List<MetaVECTOR3>();
            var colour = new List<uint>();
            var direction = new List<MetaVECTOR3>();
            var falloff = new List<float>();
            var falloffExponent = new List<float>();
            var timeAndStateFlags = new List<uint>();
            var hash = new List<uint>();
            var coneInnerAngle = new List<byte>();
            var coneOuterAngleOrCapExt = new List<byte>();
            var coronaIntensity = new List<byte>();
            ushort numStreetLights = 0;

            foreach (var light in lights)
            {
                position.Add(light.position);
                colour.Add(light.colour);
                direction.Add(light.direction);
                falloff.Add(light.falloff);
                falloffExponent.Add(light.falloffExponent);
                timeAndStateFlags.Add(light.timeAndStateFlags);
                hash.Add(light.hash);
                coneInnerAngle.Add(light.coneInnerAngle);
                coneOuterAngleOrCapExt.Add(light.coneOuterAngleOrCapExt);
                coronaIntensity.Add(light.coronaIntensity);
                if (light.isStreetLight) numStreetLights++;
            }

            string catLabel = CategoryLabels[category];

            var lodymap = new YmapFile();
            var distymap = new YmapFile();
            var ll = new YmapLODLights();
            var dl = new YmapDistantLODLights();
            var cdl = new CDistantLODLight();
            distymap.DistantLODLights = dl;
            lodymap.LODLights = ll;
            lodymap.Parent = distymap;
            cdl.category = (ushort)category;
            cdl.numStreetLights = numStreetLights;
            dl.CDistantLODLight = cdl;
            dl.positions = position.ToArray();
            dl.colours = colour.ToArray();
            dl.Ymap = distymap;
            dl.CalcBB();
            ll.direction = direction.ToArray();
            ll.falloff = falloff.ToArray();
            ll.falloffExponent = falloffExponent.ToArray();
            ll.timeAndStateFlags = timeAndStateFlags.ToArray();
            ll.hash = hash.ToArray();
            ll.coneInnerAngle = coneInnerAngle.ToArray();
            ll.coneOuterAngleOrCapExt = coneOuterAngleOrCapExt.ToArray();
            ll.coronaIntensity = coronaIntensity.ToArray();
            ll.Ymap = lodymap;
            ll.BuildLodLights(dl);
            ll.CalcBB();
            ll.BuildBVH();

            lodymap.CalcFlags();
            lodymap.CalcExtents();
            distymap.CalcFlags();
            distymap.CalcExtents();

            var lodname = $"{pname}_lodlights_{catLabel}{cellIndex:D3}";
            var distname = $"{pname}_distlodlights_{catLabel}{cellIndex:D3}";
            lodymap.Name = lodname;
            lodymap._CMapData.name = JenkHash.GenHash(lodname);
            lodymap.RpfFileEntry = new RpfResourceFileEntry();
            lodymap.RpfFileEntry.Name = lodname + ".ymap";
            lodymap.RpfFileEntry.NameLower = lodname + ".ymap";
            distymap.Name = distname;
            distymap._CMapData.name = JenkHash.GenHash(distname);
            distymap.RpfFileEntry = new RpfResourceFileEntry();
            distymap.RpfFileEntry.Name = distname + ".ymap";
            distymap.RpfFileEntry.NameLower = distname + ".ymap";

            lodymap._CMapData.parent = distymap._CMapData.name;
            lodymap.Loaded = true;
            distymap.Loaded = true;

            return (lodymap, distymap);
        }

        public class Light
        {
            public MetaVECTOR3 position { get; set; }
            public uint colour { get; set; }
            public MetaVECTOR3 direction { get; set; }
            public float falloff { get; set; }
            public float falloffExponent { get; set; }
            public uint timeAndStateFlags { get; set; }
            public uint hash { get; set; }
            public byte coneInnerAngle { get; set; }
            public byte coneOuterAngleOrCapExt { get; set; }
            public byte coronaIntensity { get; set; }
            public bool isStreetLight { get; set; }
        }
    }
}
