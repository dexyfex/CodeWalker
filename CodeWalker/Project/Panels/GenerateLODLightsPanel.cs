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
            //var space = ProjectForm?.WorldForm?.Space;
            //if (space == null) return;
            var gameFileCache = ProjectForm?.WorldForm?.GameFileCache;
            if (gameFileCache == null) return;

            var path = ProjectForm.CurrentProjectFile.GetFullFilePath("lodlights") + "\\";

            GenerateButton.Enabled = false;


            List<YmapFile> projectYmaps = ProjectForm.CurrentProjectFile.YmapFiles;

            var pname = NameTextBox.Text;

            Task.Run(() =>
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
                        if (dwbl != null)
                        {
                            var fphys = (dwbl as FragDrawable)?.OwnerFragmentPhys;

                            ent.EnsureLights(dwbl);
                            var elights = ent.Lights;
                            if (elights != null)
                            {

                                for (int li = 0; li<elights.Length;li++)
                                {
                                    var elight = elights[li];
                                    var la = elight.Attributes;
                                    //transform this light with the entity position and orientation
                                    //generate lights data from it!


                                    //gotta transform the light position by the given bone! annoying
                                    Bone bone = null;
                                    Matrix xform = Matrix.Identity;
                                    int boneidx = 0;
                                    var skeleton = dwbl.Skeleton;
                                    if (skeleton?.Bones?.Items != null)
                                    {
                                        for (int j = 0; j < skeleton.Bones.Items.Length; j++)
                                        {
                                            var tbone = skeleton.Bones.Items[j];
                                            if (tbone.Tag == la.BoneId)
                                            {
                                                boneidx = j;
                                                bone = tbone;
                                                break;
                                            }
                                        }
                                        if (bone != null)
                                        {
                                            var modeltransforms = skeleton.Transformations;
                                            var fragtransforms = fphys?.OwnerFragPhysLod?.FragTransforms?.Matrices;
                                            var fragtransformid = fphys?.OwnerFragPhysIndex ?? 0;
                                            var fragoffset = new Vector4(fphys?.OwnerFragPhysLod.Unknown_30h ?? Vector3.Zero, 0.0f);

                                            if ((fragtransforms != null) && (fragtransformid < fragtransforms.Length))
                                            {
                                                xform = fragtransforms[fragtransformid];
                                                xform.Row4 += fragoffset;
                                            }
                                            else
                                            {
                                                //when using the skeleton's matrices, they need to be transformed by parent
                                                xform = modeltransforms[boneidx];
                                                xform.Column4 = Vector4.UnitW;
                                                //xform = Matrix.Identity;
                                                short[] pinds = skeleton.ParentIndices;
                                                short parentind = ((pinds != null) && (boneidx < pinds.Length)) ? pinds[boneidx] : (short)-1;
                                                while ((parentind >= 0) && (parentind < pinds.Length))
                                                {
                                                    Matrix ptrans = (parentind < modeltransforms.Length) ? modeltransforms[parentind] : Matrix.Identity;
                                                    ptrans.Column4 = Vector4.UnitW;
                                                    xform = Matrix.Multiply(ptrans, xform);
                                                    parentind = ((pinds != null) && (parentind < pinds.Length)) ? pinds[parentind] : (short)-1;
                                                }
                                            }
                                        }
                                    }



                                    Vector3 lpos = la.Position;
                                    Vector3 ldir = la.Direction;
                                    Vector3 bpos = xform.Multiply(lpos);
                                    Vector3 bdir = xform.MultiplyRot(ldir);
                                    Vector3 epos = ent.Orientation.Multiply(bpos) + ent.Position;
                                    Vector3 edir = ent.Orientation.Multiply(bdir);

                                    uint r = la.ColorR;
                                    uint g = la.ColorG;
                                    uint b = la.ColorB;
                                    uint i = (byte)Math.Min(la.Intensity*4, 255);
                                    uint c = (i << 24) + (r << 16) + (g << 8) + b;
                                    uint h = elight.Hash;

                                    if (ent._CEntityDef.guid == 91259075)
                                    { } //h = 2324437992?     should be:19112537
                                    if (ent._CEntityDef.guid == 889043351)
                                    { } //h = 422028630 ?     should be:4267224866



                                    //any other way to know if it's a streetlight?
                                    //var name = ent.Archetype.Name;
                                    var flags = la.Flags;
                                    bool isStreetLight = (((flags >> 10) & 1u) == 1);// (name != null) && (name.Contains("street") || name.Contains("traffic"));
                                    isStreetLight = false; //TODO: fix this!


                                    //@Calcium:
                                    //1 = point
                                    //2 = spot
                                    //4 = capsule
                                    uint type = (uint)la.Type;
                                    uint unk = isStreetLight ? 1u : 0;//2 bits - isStreetLight low bit, unk high bit
                                    uint t = la.TimeFlags | (type << 26) | (unk << 24);

                                    var inner = (byte)Math.Round(la.ConeInnerAngle * 1.4117647f);
                                    var outer = (byte)Math.Round(la.ConeOuterAngle * 1.4117647f);
                                    if (type == 4)
                                    {
                                        outer = (byte)Math.Max(Math.Max(la.Extent.X, la.Extent.Y), la.Extent.Z);
                                    }


                                    var light = new Light();
                                    light.position = new MetaVECTOR3(epos);
                                    light.colour = c;
                                    light.direction = new MetaVECTOR3(edir);
                                    light.falloff = la.Falloff;
                                    light.falloffExponent = la.FalloffExponent;
                                    light.timeAndStateFlags = t;
                                    light.hash = h;
                                    light.coneInnerAngle = inner;
                                    light.coneOuterAngleOrCapExt = outer;
                                    light.coronaIntensity = (byte)(la.CoronaIntensity * 6);
                                    light.isStreetLight = isStreetLight;
                                    lights.Add(light);

                                }
                            }
                        }
                    }
                }


                if (lights.Count == 0)
                {
                    MessageBox.Show("No lights found in project!");
                    return;
                }



                //final lights should be sorted by isStreetLight (1 first!) and then hash
                lights.Sort((a, b) => 
                {
                    if (a.isStreetLight != b.isStreetLight) return b.isStreetLight.CompareTo(a.isStreetLight);
                    return a.hash.CompareTo(b.hash);
                });



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



                UpdateStatus("Creating new ymap files...");

                var lodymap = new YmapFile();
                var distymap = new YmapFile();
                var ll = new YmapLODLights();
                var dl = new YmapDistantLODLights();
                var cdl = new CDistantLODLight();
                distymap.DistantLODLights = dl;
                lodymap.LODLights = ll;
                lodymap.Parent = distymap;
                cdl.category = 1;//0=small, 1=med, 2=large
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


                var lodname = pname + "_lodlights";
                var distname = pname + "_distantlights";
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

                UpdateStatus("Adding new ymap files to project...");

                ProjectForm.Invoke((MethodInvoker)delegate
                {
                    ProjectForm.AddYmapToProject(lodymap);
                    ProjectForm.AddYmapToProject(distymap);
                });

                var stats = "";
                UpdateStatus("Process complete. " + stats);
                GenerateComplete();

            });
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
