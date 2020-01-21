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
                var eemin = new Vector3(float.MaxValue);
                var eemax = new Vector3(float.MinValue);
                var semin = new Vector3(float.MaxValue);
                var semax = new Vector3(float.MinValue);
                //var rnd = new Random();

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
                            Drawable ddwbl = dwbl as Drawable;
                            FragDrawable fdwbl = dwbl as FragDrawable;
                            LightAttributes_s[] lightAttrs = null;
                            if (ddwbl != null)
                            {
                                lightAttrs = ddwbl.LightAttributes?.data_items;
                            }
                            else if (fdwbl != null)
                            {
                                lightAttrs = fdwbl.OwnerFragment?.LightAttributes?.data_items;
                            }
                            if (lightAttrs != null)
                            {
                                eemin = Vector3.Min(eemin, ent.BBMin);
                                eemax = Vector3.Max(eemax, ent.BBMax);
                                semin = Vector3.Min(semin, ent.BBMin - ent._CEntityDef.lodDist);
                                semax = Vector3.Max(semax, ent.BBMax + ent._CEntityDef.lodDist);


                                for (int li = 0; li<lightAttrs.Length;li++)
                                {
                                    var la = lightAttrs[li];
                                    //transform this light with the entity position and orientation
                                    //generate lights data from it!


                                    //gotta transform the light position by the given bone! annoying
                                    Bone bone = null;
                                    Matrix xform = Matrix.Identity;
                                    int boneidx = 0;
                                    var skeleton = dwbl.Skeleton;
                                    if (skeleton?.Bones?.Data != null)
                                    {
                                        for (int j = 0; j < skeleton.Bones.Data.Count; j++)
                                        {
                                            var tbone = skeleton.Bones.Data[j];
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
                                            var fragtransforms = fdwbl?.OwnerFragmentPhys?.OwnerFragPhysLod?.FragTransforms?.Matrices;
                                            var fragtransformid = fdwbl?.OwnerFragmentPhys?.OwnerFragPhysIndex ?? 0;
                                            var fragoffset = fdwbl?.OwnerFragmentPhys?.OwnerFragPhysLod.Unknown_30h ?? Vector4.Zero;
                                            fragoffset.W = 0.0f;

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


                                    uint h = GetLightHash(ent, li);// (uint)rnd.NextLong();

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

                                    var maxext = (byte)Math.Max(Math.Max(la.Extent.X, la.Extent.Y), la.Extent.Z);



                                    var light = new Light();
                                    light.position = new MetaVECTOR3(epos);
                                    light.colour = c;
                                    light.direction = new MetaVECTOR3(edir);
                                    light.falloff = la.Falloff;
                                    light.falloffExponent = la.FalloffExponent;
                                    light.timeAndStateFlags = t;
                                    light.hash = h;
                                    light.coneInnerAngle = (byte)la.ConeInnerAngle;
                                    light.coneOuterAngleOrCapExt = Math.Max((byte)la.ConeOuterAngle, maxext);
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
                cdl.category = 1;//0=small, 1=med, 2=large
                cdl.numStreetLights = numStreetLights;
                dl.CDistantLODLight = cdl;
                dl.positions = position.ToArray();
                dl.colours = colour.ToArray();
                ll.direction = direction.ToArray();
                ll.falloff = falloff.ToArray();
                ll.falloffExponent = falloffExponent.ToArray();
                ll.timeAndStateFlags = timeAndStateFlags.ToArray();
                ll.hash = hash.ToArray();
                ll.coneInnerAngle = coneInnerAngle.ToArray();
                ll.coneOuterAngleOrCapExt = coneOuterAngleOrCapExt.ToArray();
                ll.coronaIntensity = coronaIntensity.ToArray();


                lodymap._CMapData.flags = 0;
                distymap._CMapData.flags = 2;
                lodymap._CMapData.contentFlags = 128;
                distymap._CMapData.contentFlags = 256;

                lodymap._CMapData.entitiesExtentsMin = eemin;
                lodymap._CMapData.entitiesExtentsMax = eemax;
                lodymap._CMapData.streamingExtentsMin = semin - 1000f;
                lodymap._CMapData.streamingExtentsMax = semax + 1000f; //vanilla = ~1km
                distymap._CMapData.entitiesExtentsMin = eemin;
                distymap._CMapData.entitiesExtentsMax = eemax;
                distymap._CMapData.streamingExtentsMin = semin - 5000f; //make it huge
                distymap._CMapData.streamingExtentsMax = semax + 5000f; //vanilla = ~3km

                lodymap.LODLights = ll;
                distymap.DistantLODLights = dl;

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



        private uint GetLightHash(YmapEntityDef ent, int lightIndex)
        {
            unchecked
            {

                //var aabb1 = GetAABB(ent);
                var aabb = GetAABB2(ent);

                var hashData = new int[7];
                hashData[0] = (int)(aabb.Min.X * 10.0f);
                hashData[1] = (int)(aabb.Min.Y * 10.0f);
                hashData[2] = (int)(aabb.Min.Z * 10.0f);
                hashData[3] = (int)(aabb.Max.X * 10.0f);
                hashData[4] = (int)(aabb.Max.Y * 10.0f);
                hashData[5] = (int)(aabb.Max.Z * 10.0f);
                hashData[6] = lightIndex;


                int v3 = 7;
                int v4 = 0;//=hashData index
                int v5 = (int)0xDEADBEEF + 28;// -559038709;
                int v6 = (int)0xDEADBEEF + 28;
                int v7 = (int)0xDEADBEEF + 28;

                uint v8 = 2;
                do
                {
                    int v9 = hashData[v4 + 2] + v5;
                    int v10 = hashData[v4 + 1] + v6;
                    int v11 = hashData[v4 + 0] - v9;
                    int v13 = v10 + v9;
                    int v14 = (v7 + v11) ^ RotateLeft(v9, 4);
                    int v17 = v13 + v14;
                    int v18 = (v10 - v14) ^ RotateLeft(v14, 6);
                    int v21 = v17 + v18;
                    int v22 = (v13 - v18) ^ RotateLeft(v18, 8);
                    int v25 = v21 + v22;
                    int v26 = (v17 - v22) ^ RotateLeft(v22, 16);
                    int v29 = (v21 - v26) ^ RotateRight(v26, 13);
                    int v30 = v25 - v29;
                    v7 = v25 + v26;
                    v6 = v7 + v29;
                    v5 = v30 ^ RotateLeft(v29, 4);
                    v4 += 3;
                    v3 -= 3;
                    --v8;
                }
                while (v8 > 0);

                int v32 = v3 - 1; //should always be 0
                if (v32 != 0)
                { }

                int v50 = v7 + hashData[v4];
                int v34 = (v6 ^ v5) - RotateLeft(v6, 14);
                int v35 = (v34 ^ v50) - RotateLeft(v34, 11);
                int v36 = (v35 ^ v6) - RotateRight(v35, 7);
                int v37 = (v36 ^ v34) - RotateLeft(v36, 16);
                int v51 = (v35 ^ v37) - RotateLeft(v37, 4);
                int v38 = (v51 ^ v36) - RotateLeft(v51, 14);
                int v53 = (v38 ^ v37) - RotateRight(v38, 8);
                return (uint)v53;

            }
        }


        private AABB_s GetAABB(YmapEntityDef ent)
        {
            var arch = ent.Archetype;
            var ori = ent.Orientation;
            Vector3 bbmin = ent.Position - ent.BSRadius; //sphere
            Vector3 bbmax = ent.Position + ent.BSRadius;
            if (arch != null)
            {
                Vector3[] c = new Vector3[8];
                Vector3 abmin = arch.BBMin * ent.Scale; //entity box
                Vector3 abmax = arch.BBMax * ent.Scale;
                c[0] = abmin;
                c[1] = new Vector3(abmin.X, abmin.Y, abmax.Z);
                c[2] = new Vector3(abmin.X, abmax.Y, abmin.Z);
                c[3] = new Vector3(abmin.X, abmax.Y, abmax.Z);
                c[4] = new Vector3(abmax.X, abmin.Y, abmin.Z);
                c[5] = new Vector3(abmax.X, abmin.Y, abmax.Z);
                c[6] = new Vector3(abmax.X, abmax.Y, abmin.Z);
                c[7] = abmax;
                bbmin = new Vector3(float.MaxValue);
                bbmax = new Vector3(float.MinValue);
                for (int j = 0; j < 8; j++)
                {
                    Vector3 corn = ori.Multiply(c[j]) + ent.Position;
                    bbmin = Vector3.Min(bbmin, corn);
                    bbmax = Vector3.Max(bbmax, corn);
                }
            }
            AABB_s b = new AABB_s();
            b.Min = new Vector4(bbmin, 0f);
            b.Max = new Vector4(bbmax, 0f);
            return b;
        }
        private AABB_s GetAABB2(YmapEntityDef ent)
        {
            var arch = ent.Archetype;
            var ori = ent.Orientation;
            var pos = ent.Position;
            var sca = ent.Scale;
            var mat = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, sca, Vector3.Zero, ori, pos);
            var matabs = mat;
            matabs.Column1 = mat.Column1.Abs();
            matabs.Column2 = mat.Column2.Abs();
            matabs.Column3 = mat.Column3.Abs();
            matabs.Column4 = mat.Column4.Abs();
            Vector3 bbmin = pos - ent.BSRadius; //sphere
            Vector3 bbmax = pos + ent.BSRadius;
            if (arch != null)
            {
                var bbcenter = (arch.BBMax + arch.BBMin) * 0.5f;
                var bbextent = (arch.BBMax - arch.BBMin) * 0.5f;
                var ncenter = Vector3.TransformCoordinate(bbcenter, mat);
                var nextent = Vector3.TransformNormal(bbextent, matabs);
                bbmin = ncenter - nextent;
                bbmax = ncenter + nextent;
            }
            AABB_s b = new AABB_s();
            b.Min = new Vector4(bbmin, 0f);
            b.Max = new Vector4(bbmax, 0f);
            return b;
        }



        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
        private static uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
        private static int RotateLeft(int value, int count)
        {
            return (int)RotateLeft((uint)value, count);
        }
        private static int RotateRight(int value, int count)
        {
            return (int)RotateRight((uint)value, count);
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
