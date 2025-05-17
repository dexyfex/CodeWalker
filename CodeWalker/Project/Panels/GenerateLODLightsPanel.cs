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
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class GenerateLODLightsPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }


        public GenerateLODLightsPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();

            if (ProjectForm?.WorldForm == null)
            {
                //could happen in some other startup mode - world form is required for this..
                GenerateButton.Enabled = false;
                UpdateStatus("Unable to generate - World View not available!");
            }
        }


        public void SetProject(ProjectFile project)
        {
            CurrentProjectFile = project;

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



        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var gameFileCache = ProjectForm?.WorldForm?.GameFileCache;
            if (gameFileCache == null) return;

            var path = ProjectForm.CurrentProjectFile.GetFullFilePath("lodlights") + "\\";
            GenerateButton.Enabled = false;

            var projectYmaps = ProjectForm.CurrentProjectFile.YmapFiles;
            var pname = NameTextBox.Text;

            Task.Run( () =>
            {
                var lights = new List<Light>();

                foreach (var ymap in projectYmaps)
                {
                    if (ymap?.AllEntities == null) continue;

                    foreach (var ent in ymap.AllEntities)
                    {
                        if (ent.Archetype == null) continue;

                        bool waiting = false;
                        var dwbl = gameFileCache.TryGetDrawable(ent.Archetype, out waiting);
                        while (waiting)
                        {
                            dwbl = gameFileCache.TryGetDrawable(ent.Archetype, out waiting);
                            UpdateStatus("Waiting for " + ent.Archetype.AssetName + " to load...");
                            Thread.Sleep(20);
                        }

                        UpdateStatus("Adding lights from " + ent.Archetype.Name + "...");
                        if (dwbl == null) continue;

                        ent.EnsureLights(dwbl);
                        var elights = ent.Lights;
                        if (elights == null) continue;

                        foreach (var elight in elights)
                        {
                            var la = elight.Attributes;

                            uint r = la.ColorR;
                            uint g = la.ColorG;
                            uint b = la.ColorB;
                            uint i = (byte)MathUtil.Clamp(Math.Round(la.Intensity * 5.3125f), 0, 255);
                            uint c = (i << 24) + (r << 16) + (g << 8) + b;
                            uint h = elight.Hash;

                            uint type = (uint)la.Type;
                            bool isStreetLight = ((la.Flags >> 10) & 1u) == 1;
                            if (ent.Archetype.Name != null)
                            {
                                if (ent.Archetype.Name.Contains("street") || ent.Archetype.Name.Contains("traffic") || ent.Archetype.Name.Contains("street_light") || ent.Archetype.Name.Contains("nylamp") || ent.Archetype.Name.Contains("nytrafflite"))
                                {
                                    isStreetLight = true;
                                }
                                else
                                {
                                    isStreetLight = false;
                                }
                            }
                            uint IsStreetLight = isStreetLight ? 1u : 0;
                            uint TimeAndStateFlags = la.TimeFlags | (type << 26) | (IsStreetLight << 24);

                            var inner = (byte)Math.Round(la.ConeInnerAngle * 1.4117647f);
                            var outer = (byte)Math.Round(type == 4 ? la.Extent.X * 1.82f : la.ConeOuterAngle * 1.4117647f);


                            var light = new Light();
                            light.position = new MetaVECTOR3(elight.Position);
                            light.colour = c;
                            light.direction = new MetaVECTOR3(elight.Direction);
                            light.falloff = la.Falloff;
                            light.falloffExponent = la.FalloffExponent;
                            light.timeAndStateFlags = TimeAndStateFlags;
                            light.hash = h;
                            light.coneInnerAngle = inner;
                            light.coneOuterAngleOrCapExt = outer;
                            if (la.CoronaSize != 0)
                            {
                                light.coronaIntensity = (byte)(la.CoronaIntensity * 6);
                            }
                            light.isStreetLight = isStreetLight;
                            light.Category = CalculateLightCategory((byte)la.Type, la.Falloff, la.Intensity, la.Extent.X, (LightFlags)la.Flags);

                            lights.Add(light);
                        }
                    }
                }

                if (lights.Count == 0)
                {
                    MessageBox.Show("No lights found in project!");
                    return;
                }

                lights.Sort((a, b) =>
                {
                    if (a.isStreetLight != b.isStreetLight) return b.isStreetLight.CompareTo(a.isStreetLight);
                    return a.hash.CompareTo(b.hash);
                });

                UpdateStatus("Creating new ymap files by category...");

                int chunkSize = 2500;

                for (int category = 0; category <= 2; category++)
                {
                    var categoryLights = lights.Where(l => l.Category == category).ToList();
                    if (categoryLights.Count == 0) continue;

                    int totalChunks = (int)Math.Ceiling(categoryLights.Count / (float)chunkSize);

                    for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
                    {
                        var chunk = categoryLights.Skip(chunkIndex * chunkSize).Take(chunkSize).ToList();

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

                        foreach (var light in chunk)
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

                        var lodymap = new YmapFile();
                        var distymap = new YmapFile();
                        var ll = new YmapLODLights();
                        var dl = new YmapDistantLODLights();
                        var cdl = new CDistantLODLight
                        {
                            category = (byte)category,
                            numStreetLights = numStreetLights
                        };

                        distymap.DistantLODLights = dl;
                        dl.CDistantLODLight = cdl;
                        dl.positions = position.ToArray();
                        dl.colours = colour.ToArray();
                        dl.Ymap = distymap;
                        dl.CalcBB();

                        lodymap.LODLights = ll;
                        lodymap.Parent = distymap;
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

                        string suffix = category == 0 ? "_small" : category == 1 ? "_medium" : "_large";
                        string chunkSuffix = $"{chunkIndex:D2}";

                        var lodname = pname + "_lodlights" + suffix + chunkSuffix;
                        var distname = pname + "_distantlights" + suffix + chunkSuffix;

                        lodymap.Name = lodname;
                        lodymap._CMapData.name = JenkHash.GenHash(lodname);
                        lodymap.RpfFileEntry = new RpfResourceFileEntry { Name = lodname + ".ymap", NameLower = lodname + ".ymap" };
                        distymap.Name = distname;
                        distymap._CMapData.name = JenkHash.GenHash(distname);
                        distymap.RpfFileEntry = new RpfResourceFileEntry { Name = distname + ".ymap", NameLower = distname + ".ymap" };
                        lodymap._CMapData.parent = distymap._CMapData.name;
                        lodymap.Loaded = true;
                        distymap.Loaded = true;

                        ProjectForm.Invoke((MethodInvoker)(() =>
                        {
                            ProjectForm.AddYmapToProject(lodymap);
                            ProjectForm.AddYmapToProject(distymap);
                        }));
                    }
                }

                UpdateStatus("Process complete.");
                GenerateComplete();
            });
        }

        private int CalculateLightCategory(byte lightType, float falloff, float intensity, float capsuleExtentX, LightFlags lightAttrFlags)
        {
            if ((lightAttrFlags & LightFlags.FarLodLight) != 0)
                return 2; // LARGE

            float length = falloff;
            if (lightType == 4) // CAPSULE
                length = (2.0f * falloff + capsuleExtentX);

            if ((lightAttrFlags & LightFlags.ForceMediumLodLight) != 0 || (length >= 10.0f && intensity >= 1.0f))
                return 1; // MEDIUM

            return 0; // SMALL
        }

        public enum LightFlags : uint
        {
            None = 0,
            InteriorOnly = 1 << 0,
            ExteriorOnly = 1 << 1,
            DontUseInCutscene = 1 << 2,
            Vehicle = 1 << 3,
            FX = 1 << 4,
            TextureProjection = 1 << 5,
            CastShadows = 1 << 6,
            CastStaticGeomShadows = 1 << 7,
            CastDynamicGeomShadows = 1 << 8,
            CalcFromSun = 1 << 9,
            EnableBuzzing = 1 << 10,
            ForceBuzzing = 1 << 11,
            DrawVolume = 1 << 12,
            NoSpecular = 1 << 13,
            BothInteriorAndExterior = 1 << 14,
            CoronaOnly = 1 << 15,
            NotInReflection = 1 << 16,
            OnlyInReflection = 1 << 17,
            UseCullPlane = 1 << 18,
            UseVolumeOuterColour = 1 << 19,
            CastHigherResShadows = 1 << 20,
            CastOnlyLowResShadows = 1 << 21,
            FarLodLight = 1 << 22,
            DontLightAlpha = 1 << 23,
            CastShadowsIfPossible = 1 << 24,
            Cutscene = 1 << 25,
            MovingLightSource = 1 << 26,
            UseVehicleTwin = 1 << 27,
            ForceMediumLodLight = 1 << 28,
            CoronaOnlyLodLight = 1 << 29,
            DelayRender = 1 << 30,
            AlreadyTestedForOcclusion = 1u << 31
        }

        public static class MathUtil
        {
            public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
            {
                if (value.CompareTo(min) < 0) return min;
                else if (value.CompareTo(max) > 0) return max;
                else return value;
            }
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
            public int Category { get; set; }
        }
    }
}
