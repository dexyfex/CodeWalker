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

            var path = ProjectForm.CurrentProjectFile.GetFullFilePath("navmeshes") + "\\";

            GenerateButton.Enabled = false;


            List<YmapFile> projectYmaps = ProjectForm.CurrentProjectFile.YmapFiles;

            var pname = NameTextBox.Text;

            Task.Run(() =>
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
                var eemin = new Vector3(float.MaxValue);
                var eemax = new Vector3(float.MinValue);
                var semin = new Vector3(float.MaxValue);
                var semax = new Vector3(float.MinValue);

                foreach (var ymap in projectYmaps)
                {

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
                                lightAttrs = ddwbl.LightAttributes;
                            }
                            else if (fdwbl != null)
                            {
                                lightAttrs = fdwbl.OwnerFragment?.LightAttributes;
                            }
                            if (lightAttrs != null)
                            {
                                eemin = Vector3.Min(eemin, ent.BBMin);
                                eemax = Vector3.Max(eemax, ent.BBMax);
                                semin = Vector3.Min(semin, ent.BBMin - ent._CEntityDef.lodDist);
                                semax = Vector3.Max(semax, ent.BBMax + ent._CEntityDef.lodDist);


                                foreach (var la in lightAttrs)
                                {
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
                                            if (tbone.Id == la.BoneId)
                                            {
                                                boneidx = j;
                                                bone = tbone;
                                                break;
                                            }
                                        }
                                        if (bone != null)
                                        {
                                            var modeltransforms = skeleton.Transformations;
                                            var fragtransforms = fdwbl?.OwnerFragmentPhys?.OwnerFragPhysLod?.FragTransforms?.Data;
                                            var fragtransformid = fdwbl?.OwnerFragmentPhys?.OwnerFragPhysIndex ?? 0;
                                            var fragoffset = fdwbl?.OwnerFragmentPhys?.OwnerFragPhysLod.Unknown_30h ?? Vector4.Zero;
                                            fragoffset.W = 0.0f;

                                            if ((fragtransforms != null))// && (fragtransformid < fragtransforms.Length))
                                            {
                                                if (fragtransformid < fragtransforms.Length)
                                                {
                                                    xform = fragtransforms[fragtransformid];
                                                    xform.Row4 += fragoffset;
                                                }
                                            }
                                            else
                                            {
                                                //when using the skeleton's matrices, they need to be transformed by parent
                                                xform = modeltransforms[boneidx];
                                                xform.Column4 = Vector4.UnitW;
                                                //xform = Matrix.Identity;
                                                ushort[] pinds = skeleton.ParentIndices;
                                                ushort parentind = ((pinds != null) && (boneidx < pinds.Length)) ? pinds[boneidx] : (ushort)65535;
                                                while (parentind < pinds.Length)
                                                {
                                                    Matrix ptrans = (parentind < modeltransforms.Length) ? modeltransforms[parentind] : Matrix.Identity;
                                                    ptrans.Column4 = Vector4.UnitW;
                                                    xform = Matrix.Multiply(ptrans, xform);
                                                    parentind = ((pinds != null) && (parentind < pinds.Length)) ? pinds[parentind] : (ushort)65535;
                                                }
                                            }
                                        }
                                    }



                                    Vector3 lpos = new Vector3(la.PositionX, la.PositionY, la.PositionZ);
                                    Vector3 ldir = new Vector3(la.DirectionX, la.DirectionY, la.DirectionZ);
                                    Vector3 bpos = xform.Multiply(lpos);
                                    Vector3 epos = ent.Orientation.Multiply(bpos) + ent.Position;
                                    Vector3 edir = ent.Orientation.Multiply(ldir);

                                    uint r = la.ColorR;
                                    uint g = la.ColorG;
                                    uint b = la.ColorB;
                                    uint i = (byte)Math.Min(la.Intensity*4, 255);
                                    uint c = (i << 24) + (r << 16) + (g << 8) + b;

                                    uint h = 0; //TODO: what hash to use???

                                    uint t = la.TimeFlags;

                                    var maxext = (byte)Math.Max(Math.Max(la.ExtentX, la.ExtentY), la.ExtentZ);
                                    

                                    position.Add(new MetaVECTOR3(epos));
                                    colour.Add(c);
                                    direction.Add(new MetaVECTOR3(edir));
                                    falloff.Add(la.Falloff);
                                    falloffExponent.Add(la.FalloffExponent);
                                    timeAndStateFlags.Add(t);
                                    hash.Add(h);
                                    coneInnerAngle.Add((byte)la.ConeInnerAngle);
                                    coneOuterAngleOrCapExt.Add(Math.Max((byte)la.ConeOuterAngle, maxext));
                                    coronaIntensity.Add((byte)la.CoronaIntensity);


                                }
                            }
                        }
                    }
                }


                if (position.Count == 0)
                {
                    MessageBox.Show("No lights found in project!");
                    return;
                }


                var lodymap = new YmapFile();
                var distymap = new YmapFile();
                var ll = new YmapLODLights();
                var dl = new YmapDistantLODLights();
                var cdl = new CDistantLODLight();
                cdl.category = 1;
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
                lodymap._CMapData.streamingExtentsMax = semax + 1000f;
                distymap._CMapData.entitiesExtentsMin = eemin;
                distymap._CMapData.entitiesExtentsMax = eemax;
                distymap._CMapData.streamingExtentsMin = semin - 5000f; //make it huge
                distymap._CMapData.streamingExtentsMax = semax + 5000f;

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
    }
}
