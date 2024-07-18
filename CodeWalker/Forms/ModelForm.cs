using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Rendering;
using CodeWalker.Utils;
using CodeWalker.World;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class ModelForm : Form, DXForm
    {
        public Form Form { get { return this; } } //for DXForm/DXManager use

        private Renderer Renderer = null;


        volatile bool formopen = false;
        //volatile bool running = false;
        volatile bool pauserendering = false;
        //volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera;
        Timecycle timecycle;
        Weather weather;
        Clouds clouds;

        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;
        bool MouseInvert = Settings.Default.MouseInvert;



        Vector3 prevworldpos = new Vector3(0, 0, 0); //also the start pos

        Entity camEntity = new Entity();

        //bool iseditmode = false;


        bool initedOk = false;


        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                UpdateFormTitle();
            }
        }
        public string FilePath { get; set; }

        YdrFile Ydr = null;
        YddFile Ydd = null;
        YftFile Yft = null;
        YbnFile Ybn = null;
        YptFile Ypt = null;
        YnvFile Ynv = null;



        InputManager Input = new InputManager();


        bool toolsPanelResizing = false;
        int toolsPanelResizeStartX = 0;
        int toolsPanelResizeStartLeft = 0;
        int toolsPanelResizeStartRight = 0;

        Dictionary<DrawableBase, bool> DrawableDrawFlags = new Dictionary<DrawableBase, bool>();


        bool enableGrid = false;
        float gridSize = 1.0f;
        int gridCount = 40;
        List<VertexTypePC> gridVerts = new List<VertexTypePC>();
        object gridSyncRoot = new object();

        GameFileCache gameFileCache = null;
        Archetype currentArchetype = null;
        bool updateArchetypeStatus = true;


        ModelMatForm materialForm = null;
        bool modelModified = false;

        TransformWidget Widget = new TransformWidget();
        TransformWidget GrabbedWidget = null;
        ModelLightForm lightForm = null;
        bool editingLights = false;
        public LightAttributes selectedLight = null;
        public bool showLightGizmos = true;
        public Skeleton Skeleton = null;

        ExploreForm exploreForm = null;
        RpfFileEntry rpfFileEntry = null;


        bool animsInited = false;
        YcdFile Ycd = null;
        ClipMapEntry AnimClip = null;

        MetaHash ModelHash;
        Archetype ModelArchetype = null;
        bool EnableRootMotion = false;



        public ModelForm(ExploreForm ExpForm = null)
        {
            InitializeComponent();

            exploreForm = ExpForm;

            gameFileCache = ExpForm?.GetFileCache();

            Renderer = new Renderer(this, gameFileCache);
            camera = Renderer.camera;
            timecycle = Renderer.timecycle;
            weather = Renderer.weather;
            clouds = Renderer.clouds;

            initedOk = Renderer.Init();

            Renderer.controllightdir = !Settings.Default.Skydome;
            Renderer.rendercollisionmeshes = false;
            Renderer.renderclouds = false;
            Renderer.rendermoon = false;
            Renderer.renderskeletons = false;
            Renderer.renderfragwindows = false;
            Renderer.SelectionFlagsTestAll = true;

            //var timeofday = 13.6f;
            //Renderer.SetTimeOfDay(timeofday);
            //TimeOfDayTrackBar.Value = (int)(timeofday * 60.0f);
            //UpdateTimeOfDayLabel();
        }

        private void Init()
        {
            //called from ModelForm_Load

            if (!initedOk)
            {
                Close();
                return;
            }


            MouseWheel += ModelForm_MouseWheel;

            if (!GTAFolder.UpdateGTAFolder(true))
            {
                Close();
                return;
            }

            Widget.Position = new Vector3(0f, 0f, 0f);
            Widget.Rotation = Quaternion.Identity;
            Widget.Scale = Vector3.One;
            Widget.SnapAngleDegrees = 0;
            Widget.Visible = false;
            Widget.OnPositionChange += Widget_OnPositionChange;
            Widget.OnRotationChange += Widget_OnRotationChange;
            Widget.OnScaleChange += Widget_OnScaleChange;

            ShaderParamNames[] texsamplers = RenderableGeometry.GetTextureSamplerList();
            foreach (var texsampler in texsamplers)
            {
                TextureSamplerComboBox.Items.Add(texsampler);
            }
            //TextureSamplerComboBox.SelectedIndex = 0;//LoadSettings will do this..


            UpdateGridVerts();
            GridSizeComboBox.SelectedIndex = 1;
            GridCountComboBox.SelectedIndex = 1;



            Input.Init();


            Renderer.Start();
        }


        public void InitScene(Device device)
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;

            try
            {
                Renderer.DeviceCreated(device, width, height);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading shaders!\n" + ex.ToString());
                return;
            }

            //shaders.hdrLumBlendSpeed = 1000.0f;


            camera.FollowEntity = camEntity;
            camera.FollowEntity.Position = prevworldpos;
            camera.FollowEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);
            camera.TargetDistance = 2.0f;
            camera.CurrentDistance = 2.0f;
            camera.TargetRotation.Y = 0.2f;
            camera.CurrentRotation.Y = 0.2f;
            camera.TargetRotation.X = 0.5f * (float)Math.PI;
            camera.CurrentRotation.X = 0.5f * (float)Math.PI;

            Renderer.shaders.deferred = false; //no point using this here yet


            LoadSettings();


            formopen = true;
            new Thread(new ThreadStart(ContentThread)).Start();

            frametimer.Start();
        }
        public void CleanupScene()
        {
            formopen = false;

            Renderer.DeviceDestroyed();

            //int count = 0;
            //while (running && (count < 5000)) //wait for the content thread to exit gracefully
            //{
            //    Thread.Sleep(1);
            //    count++;
            //}
        }
        public void BuffersResized(int w, int h)
        {
            Renderer.BuffersResized(w, h);
        }
        public void RenderScene(DeviceContext context)
        {
            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (pauserendering) return;

            if (!Monitor.TryEnter(Renderer.RenderSyncRoot, 50))
            { return; } //couldn't get a lock, try again next time

            UpdateControlInputs(elapsed);



            Renderer.Update(elapsed, MouseLastPoint.X, MouseLastPoint.Y);

            UpdateWidgets();

            Renderer.BeginRender(context);

            Renderer.RenderSkyAndClouds();


            RenderSingleItem();


            RenderGrid(context);

            RenderLightSelection();

            Renderer.RenderQueued();

            Renderer.RenderSelectionGeometry(MapSelectionMode.Entity);

            Renderer.RenderFinalPass();

            RenderWidgets();

            Renderer.EndRender();

            Monitor.Exit(Renderer.RenderSyncRoot);
        }
        public bool ConfirmQuit()
        {
            return true;
        }


        private void ContentThread()
        {
            //main content loading thread.
            //running = true;

            //UpdateStatus("Scanning...");

            //try
            //{
            //    GTA5Keys.LoadFromPath(Settings.Default.GTAFolder); //now loads from magic
            //}
            //catch
            //{
            //    MessageBox.Show("Keys not found! This shouldn't happen.");
            //    Close();
            //    return;
            //}

            //gameFileCache.Init(UpdateStatus, LogError);

            ////UpdateDlcListComboBox(gameFileCache.DlcNameList);
            ////EnableCacheDependentUI();
            ////LoadWorld();


            //initialised = true;

            ////EnableDLCModsUI();

            UpdateStatus("Ready");


            while (formopen && !IsDisposed) //main asset loop
            {

                if ((gameFileCache != null) && (gameFileCache.IsInited))
                {
                    if (!timecycle.Inited)
                    {
                        //UpdateStatus("Loading timecycles...");
                        timecycle.Init(gameFileCache, UpdateStatus);
                        timecycle.SetTime(Renderer.timeofday);
                        //UpdateStatus("Timecycles loaded.");
                    }
                    if (!animsInited)
                    {
                        InitAnimation();
                        animsInited = true;
                    }
                    if (Renderer.renderskydome)
                    {
                        if (!weather.Inited)
                        {
                            //UpdateStatus("Loading weather...");
                            weather.Init(gameFileCache, UpdateStatus, timecycle);
                            //UpdateStatus("Weather loaded.");
                        }
                        //if (!clouds.Inited)
                        //{
                        //    UpdateStatus("Loading clouds...");
                        //    clouds.Init(gameFileCache, UpdateStatus, weather);
                        //    UpdateStatus("Clouds loaded.");
                        //}
                    }
                }



                //if ((gameFileCache != null) && (gameFileCache.IsInited))
                //{
                //    gameFileCache.ContentThreadProc();
                //}

                bool rcItemsPending = Renderer.ContentThreadProc();

                if (!(rcItemsPending)) //gameFileCache.ItemsStillPending || 
                {
                    Thread.Sleep(1); //sleep if there's nothing to do
                }
            }

            //gameFileCache.Clear();

            //running = false;
        }





        private void InitAnimation()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { InitAnimation(); }));
            }
            else
            {
                ClipComboBox.Items.Clear();
                ClipDictComboBox.Items.Clear();
                var ycds = gameFileCache.YcdDict.Values.ToList();
                ycds.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                ClipDictComboBox.AutoCompleteCustomSource.Clear();
                List<string> ycdlist = new List<string>();
                foreach (var ycde in ycds)
                {
                    ycdlist.Add(ycde.GetShortName());
                }
                ClipDictComboBox.AutoCompleteCustomSource.AddRange(ycdlist.ToArray());
                ClipDictComboBox.Text = "";

                TrySelectClipDict();

            }
        }





        private void LoadSettings()
        {
            var s = Settings.Default;
            //WindowState = s.WindowMaximized ? FormWindowState.Maximized : WindowState;
            //FullScreenCheckBox.Checked = s.FullScreen;
            WireframeCheckBox.Checked = s.Wireframe;
            HDRRenderingCheckBox.Checked = s.HDR;
            ShadowsCheckBox.Checked = s.Shadows;
            SkydomeCheckBox.Checked = s.Skydome;
            RenderModeComboBox.SelectedIndex = Math.Max(RenderModeComboBox.FindString(s.RenderMode), 0);
            TextureSamplerComboBox.SelectedIndex = Math.Max(TextureSamplerComboBox.FindString(s.RenderTextureSampler), 0);
            TextureCoordsComboBox.SelectedIndex = Math.Max(TextureCoordsComboBox.FindString(s.RenderTextureSamplerCoord), 0);
            AnisotropicFilteringCheckBox.Checked = s.AnisotropicFiltering;
            //ErrorConsoleCheckBox.Checked = s.ShowErrorConsole;
            //StatusBarCheckBox.Checked = s.ShowStatusBar;
        }





        private void MoveCameraToView(Vector3 pos, float rad)
        {
            //move the camera to a default place where the given sphere is fully visible.

            rad = Math.Max(0.01f, rad);

            camera.FollowEntity.Position = pos;
            camera.TargetDistance = rad * 1.6f;
            camera.CurrentDistance = rad * 1.6f;

            camera.UpdateProj = true;
        }



        private Archetype TryGetArchetype(uint hash)
        {
            if ((gameFileCache == null) || (!gameFileCache.IsInited)) return null;

            var arch = gameFileCache.GetArchetype(hash);

            if ((arch != null) && (arch != currentArchetype) && (updateArchetypeStatus))
            {
                UpdateStatus("Archetype: " + arch.Name.ToString());
                currentArchetype = arch;
                updateArchetypeStatus = false;
            }

            return arch;
        }







        private void UpdateGridVerts()
        {
            lock (gridSyncRoot)
            {
                gridVerts.Clear();

                float s = gridSize * gridCount * 0.5f;
                uint cblack = (uint)Color.Black.ToRgba();
                uint cgray = (uint)Color.DimGray.ToRgba();
                uint cred = (uint)Color.DarkRed.ToRgba();
                uint cgrn = (uint)Color.DarkGreen.ToRgba();
                int interval = 10;

                for (int i = 0; i <= gridCount; i++)
                {
                    float o = (gridSize * i) - s;
                    if ((i % interval) != 0)
                    {
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, -s, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, s, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(-s, o, 0), Colour = cgray });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(s, o, 0), Colour = cgray });
                    }
                }
                for (int i = 0; i <= gridCount; i++) //draw main lines last, so they are on top
                {
                    float o = (gridSize * i) - s;
                    if ((i % interval) == 0)
                    {
                        var cx = (o == 0) ? cred : cblack;
                        var cy = (o == 0) ? cgrn : cblack;
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, -s, 0), Colour = cy });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(o, s, 0), Colour = cy });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(-s, o, 0), Colour = cx });
                        gridVerts.Add(new VertexTypePC() { Position = new Vector3(s, o, 0), Colour = cx });
                    }
                }

            }
        }

        private void RenderGrid(DeviceContext context)
        {
            if (!enableGrid) return;

            lock (gridSyncRoot)
            {
                if (gridVerts.Count > 0)
                {
                    Renderer.RenderLines(gridVerts);
                }
            }
        }

        public void SetCameraPosition(Vector3 p, float distance = 2.0f)
        {
            Renderer.camera.FollowEntity.Position = p;
            camera.TargetDistance = distance;
        }

        private void RenderLightSelection()
        {
            if (editingLights)
            {
                if (selectedLight != null)
                {
                    if (showLightGizmos)
                    {
                        Bone bone = null;
                        Skeleton?.BonesMap?.TryGetValue(selectedLight.BoneId, out bone);
                        Renderer.RenderSelectionDrawableLight(selectedLight, bone);
                    }
                }
            }
        }

        private void RenderWidgets()
        {
            if (Widget.Visible)
            {
                Renderer.RenderTransformWidget(Widget);
            }
        }
        private void UpdateWidgets()
        {
            Widget.Update(camera);
        }
        public void SetWidgetTransform(Vector3 p, Quaternion q, Vector3 s)
        {
            Widget.Position = p;
            Widget.Rotation = q;
            Widget.Scale = s;
        }
        public void SetWidgetMode(WidgetMode mode)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Widget.Mode = mode;
            }

            ToolbarMoveButton.Checked = (mode == WidgetMode.Position);
            ToolbarRotateButton.Checked = (mode == WidgetMode.Rotation);
            ToolbarScaleButton.Checked = (mode == WidgetMode.Scale);

            lightForm?.SetWidgetModeUI(mode);
        }
        private void Widget_OnPositionChange(Vector3 newpos, Vector3 oldpos)
        {
            //called during UpdateWidgets()
            if (newpos == oldpos) return;
            if (selectedLight == null || lightForm == null || !editingLights) return;

            Bone bone = null;
            Skeleton?.BonesMap?.TryGetValue(selectedLight.BoneId, out bone);
            if (bone != null)
            {
                var xforminv = Matrix.Invert(bone.AbsTransform);
                newpos = xforminv.Multiply(newpos);
            }

            selectedLight.Position = newpos;
            selectedLight.UpdateRenderable = true;
        }
        private void Widget_OnRotationChange(Quaternion newrot, Quaternion oldrot)
        {
            //called during UpdateWidgets()
            if (newrot == oldrot) return;
            if (selectedLight == null || lightForm == null || !editingLights) return;
            selectedLight.Orientation = newrot;
            selectedLight.UpdateRenderable = true;
        }
        private void Widget_OnScaleChange(Vector3 newscale, Vector3 oldscale)
        {
            //called during UpdateWidgets()
            if (newscale == oldscale) return;
            if (selectedLight == null || lightForm == null || !editingLights) return;
            if (selectedLight.Type == LightType.Capsule)
            {
                selectedLight.Falloff = newscale.X;
                selectedLight.Extent = new Vector3(newscale.Z, newscale.Z, newscale.Z);
            }
            else if (selectedLight.Type == LightType.Spot)
            {
                selectedLight.Falloff = newscale.Z;
                selectedLight.ConeInnerAngle = newscale.Y;
                selectedLight.ConeOuterAngle = newscale.X;
            }
            else
            {
                selectedLight.Falloff = newscale.Z;
            }
            selectedLight.UpdateRenderable = true;
        }

        private void SetRotationSnapping(float degrees)
        {
            Widget.SnapAngleDegrees = degrees;
            var cval = (float)SnapAngleUpDown.Value;
            if (cval != degrees)
            {
                SnapAngleUpDown.Value = (decimal)degrees;
            }

        }

        private void RenderSingleItem()
        {
            if (AnimClip != null)
            {
                AnimClip.EnableRootMotion = EnableRootMotion;
            }


            if (Ydr != null)
            {
                if (Ydr.Loaded)
                {
                    if (ModelArchetype == null) ModelArchetype = TryGetArchetype(ModelHash);

                    Renderer.RenderDrawable(Ydr.Drawable, ModelArchetype, null, ModelHash, null, null, AnimClip);
                }
            }
            else if (Ydd != null)
            {
                //render selected drawable(s)...
                if (Ydd.Loaded)
                {
                    foreach (var kvp in Ydd.Dict)
                    {
                        if (!DrawableDrawFlags.ContainsKey(kvp.Value))//only render if it's checked...
                        {
                            var arch = TryGetArchetype(kvp.Key);

                            Renderer.RenderDrawable(kvp.Value, arch, null, Ydd.RpfFileEntry.ShortNameHash, null, null, AnimClip);
                        }
                    }
                }
            }
            else if (Ypt != null)
            {
                if ((Ypt.Loaded) && (Ypt.DrawableDict != null))
                {
                    foreach (var kvp in Ypt.DrawableDict)
                    {
                        if (!DrawableDrawFlags.ContainsKey(kvp.Value))//only render if it's checked...
                        {
                            if (ModelArchetype == null) ModelArchetype = TryGetArchetype(kvp.Key);

                            Renderer.RenderDrawable(kvp.Value, ModelArchetype, null, kvp.Key, null, null, AnimClip);
                        }
                    }
                }
            }
            else if (Yft != null)
            {
                if (Yft.Loaded)
                {
                    if (Yft.Fragment != null)
                    {
                        var f = Yft.Fragment;

                        if (ModelArchetype == null) ModelArchetype = TryGetArchetype(ModelHash);

                        Renderer.RenderFragment(ModelArchetype, null, f, ModelHash, AnimClip);
                    }
                }
            }
            else if (Ybn != null)
            {
                if (Ybn.Loaded)
                {
                    Renderer.RenderCollisionMesh(Ybn.Bounds, null);
                }
            }
            else if (Ynv != null)
            {
                if (Ynv.Loaded)
                {
                    Renderer.RenderNavMesh(Ynv);
                }
            }


        }








        public void LoadModel(YdrFile ydr)
        {
            if (ydr == null) return;

            FileName = ydr.Name;
            Ydr = ydr;
            rpfFileEntry = Ydr.RpfFileEntry;
            ModelHash = Ydr.RpfFileEntry?.ShortNameHash ?? 0;
            if (ModelHash != 0)
            {
                ModelArchetype = TryGetArchetype(ModelHash);
            }

            if (ydr.Drawable != null)
            {
                var cen = ydr.Drawable.BoundingCenter;
                var rad = ydr.Drawable.BoundingSphereRadius;
                if (ModelArchetype != null)
                {
                    cen = ModelArchetype.BSCenter;
                    rad = ModelArchetype.BSRadius;
                }

                MoveCameraToView(cen, rad);

                Skeleton = ydr.Drawable.Skeleton;
            }

            if(ydr.Drawable?.LightAttributes.data_items.Length > 0)
            {
                DeferredShadingCheckBox.Checked = true;
            }

            UpdateModelsUI(ydr.Drawable);
        }
        public void LoadModels(YddFile ydd)
        {
            if (ydd == null) return;

            FileName = ydd.Name;
            Ydd = ydd;
            rpfFileEntry = Ydd.RpfFileEntry;

            if (Ydd.Drawables != null)
            {
                float maxrad = 0.01f;
                foreach (var d in Ydd.Drawables)
                {
                    maxrad = Math.Max(maxrad, d.BoundingSphereRadius);

                    if (d.Skeleton != null)
                    {
                        Skeleton = d.Skeleton;
                    }
                }
                MoveCameraToView(Vector3.Zero, maxrad);
            }

            foreach(var draw in ydd.Drawables)
            {
                if (draw?.LightAttributes.data_items.Length > 0)
                {
                    DeferredShadingCheckBox.Checked = true;
                    break;
                }
            }

            UpdateModelsUI(ydd.Dict);

            DetailsPropertyGrid.SelectedObject = ydd;
        }
        public void LoadModel(YftFile yft)
        {
            if (yft == null) return;

            FileName = yft.Name;
            Yft = yft;
            rpfFileEntry = Yft.RpfFileEntry;
            ModelHash = Yft.RpfFileEntry?.ShortNameHash ?? 0;
            var namelower = Yft.RpfFileEntry?.GetShortNameLower();
            if (namelower?.EndsWith("_hi") ?? false)
            {
                ModelHash = JenkHash.GenHash(namelower.Substring(0, namelower.Length - 3));
            }
            if (ModelHash != 0)
            {
                ModelArchetype = TryGetArchetype(ModelHash);
            }


            var dr = yft.Fragment?.Drawable;
            if (dr != null)
            {
                var cen = dr.BoundingCenter;
                var rad = dr.BoundingSphereRadius;
                if (ModelArchetype != null)
                {
                    cen = ModelArchetype.BSCenter;
                    rad = ModelArchetype.BSRadius;
                }

                MoveCameraToView(cen, rad);

                Skeleton = dr.Skeleton;
            }

            if (yft.Fragment?.LightAttributes.data_items.Length > 0)
            {
                DeferredShadingCheckBox.Checked = true;
            }

            UpdateModelsUI(yft.Fragment?.Drawable, yft.Fragment);
        }
        public void LoadModel(YbnFile ybn)
        {
            if (ybn == null) return;

            FileName = ybn.Name;
            Ybn = ybn;
            rpfFileEntry = Ybn.RpfFileEntry;

            if (Ybn.Bounds != null)
            {
                MoveCameraToView(Ybn.Bounds.SphereCenter, Ybn.Bounds.SphereRadius);
            }

            UpdateBoundsUI(ybn);
        }
        public void LoadParticles(YptFile ypt)
        {
            if (ypt == null) return;

            FileName = ypt.Name;
            Ypt = ypt;
            rpfFileEntry = Ypt.RpfFileEntry;

            if (ypt.DrawableDict != null)
            {
                float maxrad = 0.01f;
                foreach (var d in ypt.DrawableDict.Values)
                {
                    maxrad = Math.Max(maxrad, d.BoundingSphereRadius);
                }
                MoveCameraToView(Vector3.Zero, maxrad);
            }

            UpdateModelsUI(ypt.DrawableDict);

            DetailsPropertyGrid.SelectedObject = ypt;//.PtfxList;
        }
        public void LoadNavmesh(YnvFile ynv)
        {
            if (ynv == null) return;

            FileName = ynv.Name;
            Ynv = ynv;
            rpfFileEntry = Ynv.RpfFileEntry;

            if (ynv.Nav.SectorTree != null)
            {
                var st = ynv.Nav.SectorTree;
                var cen = (st.AABBMin + st.AABBMax).XYZ() * 0.5f;
                var rad = (st.AABBMax - st.AABBMin).XYZ().Length() * 0.5f;
                MoveCameraToView(cen, rad);
            }

            UpdateNavmeshUI(ynv);
        }


        private void TrySelectClipDict()
        {
            if (ModelArchetype != null)
            {
                var str = ModelArchetype.ClipDict.ToCleanString();
                ClipDictComboBox.Text = str;
            }
        }





        private void UpdateFormTitle()
        {
            Text = fileName + (modelModified ? "*" : "") + " - CodeWalker by dexyfex";
        }



        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }

        private void LogError(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { LogError(text); }));
                }
                else
                {
                    ConsoleTextBox.AppendText(text + "\r\n");
                }
            }
            catch { }
        }







        private void LoadClipDict(string name)
        {
            if (gameFileCache == null) return;
            if (!gameFileCache.IsInited) return;//what to do here? wait for it..?

            var ycdhash = JenkHash.GenHash(name.ToLowerInvariant());
            var ycd = gameFileCache.GetYcd(ycdhash);
            while ((ycd != null) && (!ycd.Loaded))
            {
                Thread.Sleep(1);//kinda hacky
                ycd = gameFileCache.GetYcd(ycdhash);
            }

            Ycd = ycd;

            ClipComboBox.Items.Clear();
            ClipComboBox.Items.Add("");

            if (ycd?.ClipMapEntries == null)
            {
                ClipComboBox.SelectedIndex = 0;
                AnimClip = null;
                return;
            }

            List<string> items = new List<string>();
            foreach (var cme in ycd.ClipMapEntries)
            {
                if (cme.Clip != null)
                {
                    items.Add(cme.Clip.ShortName);
                }
            }

            items.Sort();
            foreach (var item in items)
            {
                ClipComboBox.Items.Add(item);
            }
        }

        private void SelectClip(string name)
        {
            MetaHash cliphash = JenkHash.GenHash(name);
            ClipMapEntry cme = null;
            Ycd?.ClipMap?.TryGetValue(cliphash, out cme);
            AnimClip = cme;
        }






        private void UpdateTimeOfDayLabel()
        {
            int v = TimeOfDayTrackBar.Value;
            float fh = v / 60.0f;
            int ih = (int)fh;
            int im = v - (ih * 60);
            if (ih == 24) ih = 0;
            TimeOfDayLabel.Text = string.Format("{0:00}:{1:00}", ih, im);
        }




        private void UpdateControlInputs(float elapsed)
        {
            if (elapsed > 0.1f) elapsed = 0.1f;

            var s = Settings.Default;

            float moveSpeed = 2.0f;


            Input.Update();

            if (Input.xbenable)
            {
                //if (ControllerButtonJustPressed(GamepadButtonFlags.Start))
                //{
                //    SetControlMode(ControlMode == WorldControlMode.Free ? WorldControlMode.Ped : WorldControlMode.Free);
                //}
            }



            if (Input.ShiftPressed)
            {
                moveSpeed *= 5.0f;
            }
            if (Input.CtrlPressed)
            {
                moveSpeed *= 0.2f;
            }

            Vector3 movevec = Input.KeyboardMoveVec(false);

            if (Input.xbenable)
            {
                movevec.X += Input.xblx;
                movevec.Z -= Input.xbly;
                moveSpeed *= (1.0f + (Math.Min(Math.Max(Input.xblt, 0.0f), 1.0f) * 15.0f)); //boost with left trigger
                if (Input.ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.LeftShoulder))
                {
                    moveSpeed *= 5.0f;
                }
            }


            //if (MapViewEnabled == true)
            //{
            //    movevec *= elapsed * 100.0f * Math.Min(camera.OrthographicTargetSize * 0.01f, 30.0f);
            //    float mapviewscale = 1.0f / camera.Height;
            //    float fdx = MapViewDragX * mapviewscale;
            //    float fdy = MapViewDragY * mapviewscale;
            //    movevec.X -= fdx * camera.OrthographicSize;
            //    movevec.Y += fdy * camera.OrthographicSize;
            //}
            //else
            {
                //normal movement
                movevec *= elapsed * moveSpeed * Math.Min(camera.TargetDistance, 50.0f);
            }


            Vector3 movewvec = camera.ViewInvQuaternion.Multiply(movevec);
            camEntity.Position += movewvec;

            //MapViewDragX = 0;
            //MapViewDragY = 0;




            if (Input.xbenable)
            {
                camera.ControllerRotate(Input.xbrx, Input.xbry, elapsed);

                float zoom = 0.0f;
                float zoomspd = s.XInputZoomSpeed;
                float zoomamt = zoomspd * elapsed;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadUp)) zoom += zoomamt;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadDown)) zoom -= zoomamt;

                camera.ControllerZoom(zoom);

            }



        }




        private void UpdateModelsUI(DrawableBase drawable, object detailsObject = null)
        {
            DetailsPropertyGrid.SelectedObject = detailsObject ?? drawable;

            DrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = false;
            TexturesTreeView.Nodes.Clear();
            if (drawable != null)
            {
                AddDrawableModelsTreeNodes(drawable.DrawableModels?.High, "High Detail", true);
                AddDrawableModelsTreeNodes(drawable.DrawableModels?.Med, "Medium Detail", false);
                AddDrawableModelsTreeNodes(drawable.DrawableModels?.Low, "Low Detail", false);
                AddDrawableModelsTreeNodes(drawable.DrawableModels?.VLow, "Very Low Detail", false);
                //AddDrawableModelsTreeNodes(drawable.DrawableModels?.Extra, "X Detail", false);

                var fdrawable = drawable as FragDrawable;
                if (fdrawable != null)
                {
                    var plod1 = fdrawable.OwnerFragment?.PhysicsLODGroup?.PhysicsLOD1;
                    if ((plod1 != null) && (plod1.Children?.data_items != null))
                    {
                        foreach (var child in plod1.Children.data_items)
                        {
                            var cdrwbl = child.Drawable1;
                            if ((cdrwbl != null) && (cdrwbl.AllModels?.Length > 0))
                            {
                                if (cdrwbl.Owner is FragDrawable) continue; //it's a copied drawable... eg a wheel

                                var dname = child.GroupName;
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModels?.High, dname + " - High Detail", true);
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModels?.Med, dname + " - Medium Detail", false);
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModels?.Low, dname + " - Low Detail", false);
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModels?.VLow, dname + " - Very Low Detail", false);
                            }
                        }
                    }

                    var fdarr = fdrawable.OwnerFragment?.DrawableArray?.data_items;
                    if (fdarr != null)
                    {
                        var fdnames = fdrawable.OwnerFragment?.DrawableArrayNames?.data_items;
                        for (int i = 0; i < fdarr.Length; i++)
                        {
                            var arrd = fdarr[i];
                            if ((arrd != null) && (arrd.AllModels?.Length > 0))
                            {
                                var dname = ((fdnames != null) && (i < fdnames.Length)) ? fdnames[i]?.Value : arrd.Name;
                                if (string.IsNullOrEmpty(dname)) dname = "(No name)";
                                AddDrawableModelsTreeNodes(arrd.DrawableModels?.High, dname + " - High Detail", false);
                                AddDrawableModelsTreeNodes(arrd.DrawableModels?.Med, dname + " - Medium Detail", false);
                                AddDrawableModelsTreeNodes(arrd.DrawableModels?.Low, dname + " - Low Detail", false);
                                AddDrawableModelsTreeNodes(arrd.DrawableModels?.VLow, dname + " - Very Low Detail", false);
                            }
                        }
                    }

                }
            }
        }
        private void UpdateModelsUI(Dictionary<uint, Drawable> dict)
        {
            //DetailsPropertyGrid.SelectedObject = dict; //this won't look good...

            DrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = true;
            TexturesTreeView.Nodes.Clear();

            bool check = true;
            if (dict != null)
            {
                List<KeyValuePair<uint, Drawable>> items = new List<KeyValuePair<uint, Drawable>>();
                foreach (var kvp in dict)
                {
                    items.Add(kvp);
                }
                items.Sort((a, b) => { return a.Value?.Name?.CompareTo(b.Value?.Name ?? "") ?? 0; });
                foreach (var kvp in items)
                {
                    AddDrawableTreeNode(kvp.Value, kvp.Key, check);
                    check = false;
                }
            }

            ToolsPanel.Visible = true; //show the panel by default for dictionaries...
        }
        private void UpdateModelsUI(Dictionary<uint, DrawableBase> dict)
        {
            //DetailsPropertyGrid.SelectedObject = dict; //this won't look good...

            DrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = true;
            TexturesTreeView.Nodes.Clear();

            bool check = true;
            if (dict != null)
            {
                List<KeyValuePair<uint, DrawableBase>> items = new List<KeyValuePair<uint, DrawableBase>>();
                foreach (var kvp in dict)
                {
                    items.Add(kvp);
                }
                items.Sort((a, b) => { return ((MetaHash)a.Key).ToCleanString().CompareTo(((MetaHash)b.Key).ToCleanString()); });
                foreach (var kvp in items)
                {
                    AddDrawableTreeNode(kvp.Value, kvp.Key, check);
                    check = false;
                }
            }

            ToolsPanel.Visible = true; //show the panel by default for dictionaries...
        }
        private void UpdateBoundsUI(YbnFile bounds)
        {
            DetailsPropertyGrid.SelectedObject = bounds;
        }
        private void UpdateNavmeshUI(YnvFile ynv)
        {
            DetailsPropertyGrid.SelectedObject = ynv;
        }


        private void AddDrawableTreeNode(DrawableBase drawable, uint hash, bool check)
        {
            MetaHash mhash = new MetaHash(hash);
            
            var dnode = ModelsTreeView.Nodes.Add(mhash.ToString());
            dnode.Tag = drawable;
            dnode.Checked = check;

            AddDrawableModelsTreeNodes(drawable.DrawableModels?.High, "High Detail", true, dnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModels?.Med, "Medium Detail", false, dnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModels?.Low, "Low Detail", false, dnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModels?.VLow, "Very Low Detail", false, dnode);
            //AddDrawableModelsTreeNodes(drawable.DrawableModels?.Extra, "X Detail", false, dnode);

        }
        private void AddDrawableModelsTreeNodes(DrawableModel[] models, string prefix, bool check, TreeNode parentDrawableNode = null)
        {
            if (models == null) return;

            for (int mi = 0; mi < models.Length; mi++)
            {
                var tnc = (parentDrawableNode != null) ? parentDrawableNode.Nodes : ModelsTreeView.Nodes;

                var model = models[mi];
                string mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = tnc.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;
                mnode.Checked = check;

                var tmnode = TexturesTreeView.Nodes.Add(mprefix + " " + model.ToString());
                tmnode.Tag = model;

                if (!check)
                {
                    Renderer.SelectionModelDrawFlags[model] = false;
                }

                if (model.Geometries == null) continue;

                foreach (var geom in model.Geometries)
                {
                    var gname = geom.ToString();
                    var gnode = mnode.Nodes.Add(gname);
                    gnode.Tag = geom;
                    gnode.Checked = true;// check;

                    var tgnode = tmnode.Nodes.Add(gname);
                    tgnode.Tag = geom;

                    if ((geom.Shader != null) && (geom.Shader.ParametersList != null) && (geom.Shader.ParametersList.Hashes != null))
                    {
                        var pl = geom.Shader.ParametersList;
                        var h = pl.Hashes;
                        var p = pl.Parameters;
                        for (int ip = 0; ip < h.Length; ip++)
                        {
                            var hash = pl.Hashes[ip];
                            var parm = pl.Parameters[ip];
                            var tex = parm.Data as TextureBase;
                            if (tex != null)
                            {
                                var t = tex as Texture;
                                var tstr = tex.Name.Trim();
                                if (t != null)
                                {
                                    tstr = string.Format("{0} ({1}x{2}, embedded)", tex.Name, t.Width, t.Height);
                                }
                                var tnode = tgnode.Nodes.Add(hash.ToString().Trim() + ": " + tstr);
                                tnode.Tag = tex;
                            }
                        }
                        tgnode.Expand();
                    }

                }

                mnode.Expand();
                tmnode.Expand();
            }
        }
        private void UpdateSelectionDrawFlags(TreeNode node)
        {
            //update the selection draw flags depending on tag and checked/unchecked
            var drwbl = node.Tag as DrawableBase;
            var model = node.Tag as DrawableModel;
            var geom = node.Tag as DrawableGeometry;
            bool rem = node.Checked;
            lock (Renderer.RenderSyncRoot)
            {
                if (drwbl != null)
                {
                    if (rem)
                    {
                        if (DrawableDrawFlags.ContainsKey(drwbl))
                        {
                            DrawableDrawFlags.Remove(drwbl);
                        }
                    }
                    else
                    {
                        DrawableDrawFlags[drwbl] = false;
                    }
                }
                if (model != null)
                {
                    if (rem)
                    {
                        if (Renderer.SelectionModelDrawFlags.ContainsKey(model))
                        {
                            Renderer.SelectionModelDrawFlags.Remove(model);
                        }
                    }
                    else
                    {
                        Renderer.SelectionModelDrawFlags[model] = false;
                    }
                }
                if (geom != null)
                {
                    if (rem)
                    {
                        if (Renderer.SelectionGeometryDrawFlags.ContainsKey(geom))
                        {
                            Renderer.SelectionGeometryDrawFlags.Remove(geom);
                        }
                    }
                    else
                    {
                        Renderer.SelectionGeometryDrawFlags[geom] = false;
                    }
                }
                updateArchetypeStatus = true;
            }
        }



        private void UpdateEmbeddedTextures(DrawableBase dwbl)
        {
            if (dwbl == null) return;

            var sg = dwbl.ShaderGroup;
            var td = sg?.TextureDictionary;
            var sd = sg?.Shaders?.data_items;

            if (td == null) return;
            if (sd == null) return;

            var updated = false;
            foreach (var s in sd)
            {
                if (s?.ParametersList == null) continue;
                foreach (var p in s.ParametersList.Parameters)
                {
                    if (p.Data is TextureBase tex)
                    {
                        var tex2 = td.Lookup(tex.NameHash);
                        if ((tex2 != null) && (tex != tex2))
                        {
                            p.Data = tex2;//swap the parameter out for the new embedded texture
                            updated = true;
                        }
                    }
                }
            }

            if (!updated) return;

            foreach (var model in dwbl.AllModels)
            {
                if (model?.Geometries == null) continue;
                foreach (var geom in model.Geometries)
                {
                    geom.UpdateRenderableParameters = true;
                }
            }
        }
        public void UpdateEmbeddedTextures()
        {
            if (Ydr != null)
            {
                if (Ydr.Loaded)
                {
                    UpdateEmbeddedTextures(Ydr.Drawable);
                }
            }
            else if (Ydd != null)
            {
                if (Ydd.Loaded)
                {
                    foreach (var kvp in Ydd.Dict)
                    {
                        UpdateEmbeddedTextures(kvp.Value);
                    }
                }
            }
            else if (Ypt != null)
            {
                if ((Ypt.Loaded) && (Ypt.DrawableDict != null))
                {
                    foreach (var kvp in Ypt.DrawableDict)
                    {
                        UpdateEmbeddedTextures(kvp.Value);
                    }
                }
            }
            else if (Yft != null)
            {
                if (Yft.Loaded)
                {
                    if (Yft.Fragment != null)
                    {
                        var f = Yft.Fragment;

                        UpdateEmbeddedTextures(f.Drawable);
                        UpdateEmbeddedTextures(f.DrawableCloth);

                        if (f.DrawableArray?.data_items != null)
                        {
                            foreach (var d in f.DrawableArray.data_items)
                            {
                                UpdateEmbeddedTextures(d);
                            }
                        }

                        var c = f.PhysicsLODGroup?.PhysicsLOD1?.Children?.data_items;
                        if (c != null)
                        {
                            foreach (var child in c)
                            {
                                if (child != null)
                                {
                                    UpdateEmbeddedTextures(child.Drawable1);
                                    UpdateEmbeddedTextures(child.Drawable2);
                                }
                            }
                        }

                    }
                }
            }
        }




        private void ShowMaterialEditor()
        {
            DrawableBase drawable = null;
            Dictionary<uint, Drawable> dict = null;


            if ((Ydr != null) && (Ydr.Loaded))
            {
                drawable = Ydr.Drawable;
            }
            else if ((Ydd != null) && (Ydd.Loaded))
            {
                dict = Ydd.Dict;
            }
            else if ((Yft != null) && (Yft.Loaded))
            {
                drawable = Yft.Fragment?.Drawable;
            }
            else if ((Ypt != null) && (Ypt.Loaded))
            {
                //dict = Ypt.DrawableDict;
            }
            else
            {
                MessageBox.Show("Material editor not supported for the current file.");
                return;
            }

            if (materialForm == null)
            {
                materialForm = new ModelMatForm(this);

                if (drawable != null)
                {
                    materialForm.LoadModel(drawable);
                }
                else if (dict != null)
                {
                    materialForm.LoadModels(dict);
                }

                materialForm.Show(this);
            }
            else
            {
                if (materialForm.WindowState == FormWindowState.Minimized)
                {
                    materialForm.WindowState = FormWindowState.Normal;
                }
                materialForm.Focus();
            }
        }

        private void ShowTextureEditor()
        {
            TextureDictionary td = null;

            if ((Ydr != null) && (Ydr.Loaded))
            {
                td = Ydr.Drawable?.ShaderGroup?.TextureDictionary;
            }
            else if ((Yft != null) && (Yft.Loaded))
            {
                td = Yft.Fragment?.Drawable?.ShaderGroup?.TextureDictionary;
            }
            else if ((Ypt != null) && (Ypt.Loaded))
            {
                td = Ypt?.PtfxList?.TextureDictionary;
            }

            if (td != null)
            {
                YtdForm f = new YtdForm(null, this);
                f.Show(this);
                f.LoadTexDict(td, fileName);
            }
            else
            {
                MessageBox.Show("Couldn't find embedded texture dict.");
            }
        }

        private void ShowLightEditor()
        {
            DrawableBase drawable = null;
            Dictionary<uint, Drawable> dict = null;

            if ((Ydr != null) && (Ydr.Loaded))
            {
                drawable = Ydr.Drawable;
            }
            else if ((Ydd != null) && (Ydd.Loaded))
            {
                dict = Ydd.Dict;
            }
            else if ((Yft != null) && (Yft.Loaded))
            {
                drawable = Yft.Fragment?.Drawable;
            }
            else if ((Ypt != null) && (Ypt.Loaded))
            {
                //dict = Ypt.DrawableDict;
            }
            else
            {
                MessageBox.Show("Light editor not supported for the current file.");
            }

            if (lightForm == null)
            {
                lightForm = new ModelLightForm(this);

                if (drawable != null)
                {
                    lightForm.LoadModel(drawable);
                }
                else if (dict != null)
                {
                    lightForm.LoadModels(dict);
                }

                editingLights = true;
                Widget.Visible = true;
                lightForm.Show(this);
            }
            else
            {
                if (lightForm.WindowState == FormWindowState.Minimized)
                {
                    lightForm.WindowState = FormWindowState.Normal;
                }
                lightForm.Focus();
            }
            DeferredShadingCheckBox.Checked = true; //make sure we can see the lights we're editing (maybe this is bad for potatoes but meh)
        }



        public void OnLightFormClosed()
        {
            lightForm = null;
            editingLights = false;
            selectedLight = null;
            Widget.Visible = false;
        }

        public void OnMaterialFormClosed()
        {
            materialForm = null;
        }


        public void OnModelModified()
        {
            modelModified = true;
            UpdateFormTitle();
        }





        private void Save(bool saveAs = false)
        {
            var editMode = exploreForm?.EditMode ?? false;

            if (string.IsNullOrEmpty(FilePath))
            {
                if (!editMode) saveAs = true;
                if (rpfFileEntry == null) saveAs = true;
            }
            else
            {
                if ((FilePath.ToLowerInvariant().StartsWith(GTAFolder.CurrentGTAFolder.ToLowerInvariant()))) saveAs = true;
                if (!File.Exists(FilePath)) saveAs = true;
            }

            var fn = FilePath;
            if (saveAs)
            {
                if (!string.IsNullOrEmpty(fn))
                {
                    var dir = new FileInfo(fn).DirectoryName;
                    if (!Directory.Exists(dir)) dir = "";
                    SaveFileDialog.InitialDirectory = dir;
                }
                SaveFileDialog.FileName = FileName;

                var fileExt = Path.GetExtension(FileName);
                if ((fileExt.Length > 1) && fileExt.StartsWith("."))
                {
                    fileExt = fileExt.Substring(1);
                }
                SaveFileDialog.Filter = fileExt.ToUpperInvariant() + " files|*." + fileExt + "|All files|*.*";

                if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;
                fn = SaveFileDialog.FileName;
                FilePath = fn;
            }



            byte[] fileBytes = null;

#if !DEBUG
            try
            {
#endif
            if (Ydr != null)
            {
                fileBytes = Ydr.Save();
            }
            else if (Ydd != null)
            {
                fileBytes = Ydd.Save();
            }
            else if (Yft != null)
            {
                fileBytes = Yft.Save();
            }
            else if (Ybn != null)
            {
                fileBytes = Ybn.Save();
            }
            else if (Ypt != null)
            {
                fileBytes = Ypt.Save();
            }
            else if (Ynv != null)
            {
                fileBytes = Ynv.Save();
            }
#if !DEBUG
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error saving file!\n" + ex.ToString());
                return;
            }
#endif
            if (fileBytes == null)
            {
                MessageBox.Show("Error saving file!\n fileBytes was null!");
                return;
            }


            var rpfSave = editMode && (rpfFileEntry?.Parent != null) && !saveAs;

            if (rpfSave)
            {
                if (!rpfFileEntry.Path.ToLowerInvariant().StartsWith("mods"))
                {
                    if (MessageBox.Show("This file is NOT located in the mods folder - Are you SURE you want to save this file?\r\nWARNING: This could cause permanent damage to your game!!!", "WARNING: Are you sure about this?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;//that was a close one
                    }
                }

                try
                {
                    if (!(exploreForm?.EnsureRpfValidEncryption(rpfFileEntry.File) ?? false)) return;

                    var newentry = RpfFile.CreateFile(rpfFileEntry.Parent, rpfFileEntry.Name, fileBytes);
                    if (newentry != rpfFileEntry)
                    { }
                    rpfFileEntry = newentry;

                    exploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...

                    StatusLabel.Text = rpfFileEntry.Name + " saved successfully at " + DateTime.Now.ToString();

                    //victory!
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file to RPF! The RPF archive may be corrupted...\r\n" + ex.ToString(), "Really Bad Error");
                }

            }
            else
            {
                if (string.IsNullOrEmpty(fn))
                {
                    fn = rpfFileEntry?.Path;
                }

                try
                {
                    File.WriteAllBytes(fn, fileBytes);

                    fileName = Path.GetFileName(fn);

                    exploreForm?.RefreshMainListViewInvoke(); //update the file details in explorer...

                    StatusLabel.Text = fileName + " saved successfully at " + DateTime.Now.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing file to disk!\n" + ex.ToString());
                    return;
                }
            }


            modelModified = false;
            UpdateFormTitle();

        }





        private void SaveAllTextures(bool includeEmbedded)
        {
            if (gameFileCache == null)
            {
                MessageBox.Show("This operation requires GameFileCache to continue. This shouldn't happen!");
                return;
            }

            if (FolderBrowserDialog.ShowDialogNew() != DialogResult.OK) return;
            string folderpath = FolderBrowserDialog.SelectedPath;
            if (!folderpath.EndsWith("\\")) folderpath += "\\";


            var tryGetTextureFromYtd = new Func<uint, YtdFile, Texture>((texHash, ytd) => 
            {
                if (ytd == null) return null;
                int tries = 0;
                while (!ytd.Loaded && (tries < 500)) //wait upto ~5 sec
                {
                    Thread.Sleep(10);
                    tries++;
                }
                if (ytd.Loaded)
                {
                    return ytd.TextureDict?.Lookup(texHash);
                }
                return null;
            });
            var tryGetTexture = new Func<uint, uint, Texture>((texHash, txdHash) =>
            {
                if (txdHash != 0)
                {
                    var ytd = gameFileCache.GetYtd(txdHash);
                    var tex = tryGetTextureFromYtd(texHash, ytd);
                    return tex;
                }
                return null;
            });

            var textures = new HashSet<Texture>();
            var texturesMissing = new HashSet<string>();
            var collectTextures = new Action<DrawableBase>((d) => 
            {
                if (includeEmbedded)
                {
                    if (d?.ShaderGroup?.TextureDictionary?.Textures?.data_items != null)
                    {
                        foreach (var tex in d.ShaderGroup.TextureDictionary.Textures.data_items)
                        {
                            textures.Add(tex);
                        }
                    }
                    if ((d?.Owner is YptFile ypt) && (ypt.PtfxList?.TextureDictionary?.Textures?.data_items != null))
                    {
                        foreach (var tex in ypt.PtfxList.TextureDictionary.Textures.data_items)
                        {
                            textures.Add(tex);
                        }
                        return; //ypt's apparently only use embedded textures...
                    }
                }

                if (d?.ShaderGroup?.Shaders?.data_items == null) return;

                var archhash = 0u;
                if (d is Drawable dwbl)
                {
                    var dname = dwbl.Name.ToLowerInvariant();
                    dname = dname.Replace(".#dr", "").Replace(".#dd", "");
                    archhash = JenkHash.GenHash(dname);
                }
                else if (d is FragDrawable fdbl)
                {
                    var yft = fdbl.Owner as YftFile;
                    var fraghash = (MetaHash)(yft?.RpfFileEntry?.ShortNameHash ?? 0);
                    archhash = fraghash;
                }
                var arch = gameFileCache.GetArchetype(archhash);
                if (arch == null)
                {
                    arch = currentArchetype;
                }

                var txdHash = (arch != null) ? arch.TextureDict.Hash : archhash;
                if ((txdHash == 0) && (archhash == 0))
                { }

                foreach (var s in d.ShaderGroup.Shaders.data_items)
                {
                    if (s?.ParametersList?.Parameters == null) continue;
                    foreach (var p in s.ParametersList.Parameters)
                    {
                        var t = p.Data as TextureBase;
                        if (t == null) continue;
                        var tex = t as Texture;
                        if (tex != null)
                        {
                            if (includeEmbedded)
                            {
                                textures.Add(tex);//probably redundant
                            }
                        }
                        else
                        {
                            var texhash = t.NameHash;
                            tex = tryGetTexture(texhash, txdHash);
                            if (tex == null)
                            {
                                var ptxdhash = gameFileCache.TryGetParentYtdHash(txdHash);
                                while ((ptxdhash != 0) && (tex == null))
                                {
                                    tex = tryGetTexture(texhash, ptxdhash);
                                    if (tex == null)
                                    {
                                        ptxdhash = gameFileCache.TryGetParentYtdHash(ptxdhash);
                                    }
                                }
                                if (tex == null)
                                {
                                    var ytd = gameFileCache.TryGetTextureDictForTexture(texhash);
                                    tex = tryGetTextureFromYtd(texhash, ytd);
                                }
                                if (tex == null)
                                {
                                    texturesMissing.Add(t.Name);
                                }
                            }
                            if (tex != null)
                            {
                                textures.Add(tex);
                            }
                        }
                    }
                }
            });

            if (Ydr != null)
            {
                collectTextures(Ydr.Drawable);
            }
            if (Ydd?.Drawables != null)
            {
                foreach (var d in Ydd.Drawables)
                {
                    collectTextures(d);
                }
            }
            if (Yft?.Fragment != null)
            {
                var f = Yft.Fragment;
                collectTextures(f.Drawable);
                collectTextures(f.DrawableCloth);
                if (f.DrawableArray?.data_items != null)
                {
                    foreach (var d in f.DrawableArray.data_items)
                    {
                        collectTextures(d);
                    }
                }
                if (f.Cloths?.data_items != null)
                {
                    foreach (var c in f.Cloths.data_items)
                    {
                        collectTextures(c.Drawable);
                    }
                }
                var fc = f.PhysicsLODGroup?.PhysicsLOD1?.Children?.data_items;
                if (fc != null)
                {
                    foreach (var fcc in fc)
                    {
                        collectTextures(fcc.Drawable1);
                        collectTextures(fcc.Drawable2);
                    }
                }
            }
            if (Ypt?.DrawableDict != null)
            {
                foreach (var d in Ypt.DrawableDict.Values)
                {
                    collectTextures(d);
                }
            }

            var errordds = new List<string>();
            var successcount = 0;
            foreach (var tex in textures)
            {
                try
                {
                    string fpath = folderpath + tex.Name + ".dds";
                    byte[] dds = DDSIO.GetDDSFile(tex);
                    File.WriteAllBytes(fpath, dds);
                    successcount++;
                }
                catch
                {
                    errordds.Add(tex.Name ?? "???");
                }
            }

            var sb = new StringBuilder();
            if (successcount > 0)
            {
                sb.AppendLine(successcount.ToString() + " textures successfully exported.");
            }
            if (texturesMissing.Count > 0)
            {
                sb.AppendLine(texturesMissing.Count.ToString() + " textures weren't found!");
            }
            if (errordds.Count > 0)
            {
                sb.AppendLine(errordds.Count.ToString() + " textures couldn't be converted to .dds!");
            }
            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
            }
            else
            {
                MessageBox.Show("No textures were found to export.");
            }

        }









        private void ModelForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void ModelForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (ActiveControl is NumericUpDown)
            {
                ActiveControl = null;
            }
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = true; break;
                case MouseButtons.Right: MouseRButtonDown = true; break;
            }

            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;

            if (MouseLButtonDown)
            {
                if (Widget.IsUnderMouse && !Input.kbmoving)
                {
                    GrabbedWidget = Widget;
                    GrabbedWidget.IsDragging = true;
                }
            }
            else
            {
                if (GrabbedWidget != null)
                {
                    GrabbedWidget.IsDragging = false;
                    GrabbedWidget = null;
                }
            }


            if (MouseRButtonDown)
            {
                //SelectMousedItem();
            }

            MouseX = e.X; //to stop jumps happening on mousedown, sometimes the last MouseMove event was somewhere else... (eg after clicked a menu)
            MouseY = e.Y;
        }

        private void ModelForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = false; break;
                case MouseButtons.Right: MouseRButtonDown = false; break;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (GrabbedWidget != null)
                {
                    GrabbedWidget.IsDragging = false;
                    //GrabbedWidget.Position = SelectedItem.WidgetPosition;//in case of any snapping, make sure widget is in correct position at the end
                    GrabbedWidget = null;
                    lightForm.UpdateUI(); //do this so position and direction textboxes are updated after a drag
                }
            }
            //lock (MouseControlSyncRoot)
            //{
            //    MouseControlButtons &= ~e.Button;
            //}
        }

        private void ModelForm_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - MouseX;
            int dy = e.Y - MouseY;

            if (MouseInvert)
            {
                dy = -dy;
            }

            if (MouseLButtonDown)
            {
                if (GrabbedWidget == null)
                {
                        camera.MouseRotate(dx, dy);
                }
            }
            if (MouseRButtonDown)
            {
                if (Renderer.controllightdir)
                {
                    Renderer.lightdirx += (dx * camera.Sensitivity);
                    Renderer.lightdiry += (dy * camera.Sensitivity);
                }
                else if (Renderer.controltimeofday)
                {
                    float tod = Renderer.timeofday;
                    tod += (dx - dy) / 30.0f;
                    while (tod >= 24.0f) tod -= 24.0f;
                    while (tod < 0.0f) tod += 24.0f;
                    timecycle.SetTime(tod);
                    Renderer.timeofday = tod;

                    float fv = tod * 60.0f;
                    TimeOfDayTrackBar.Value = (int)fv;
                    UpdateTimeOfDayLabel();
                }
            }

            MouseX = e.X;
            MouseY = e.Y;
            MouseLastPoint = e.Location;



        }

        private void ModelForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                //if (ControlMode == WorldControlMode.Free)
                //{
                    camera.MouseZoom(e.Delta);
                //}
                //else
                //{
                //    lock (MouseControlSyncRoot)
                //    {
                //        MouseControlWheel += e.Delta;
                //    }
                //}
            }

        }

        private void ModelForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (ActiveControl is TextBox)
            {
                var tb = ActiveControl as TextBox;
                if (!tb.ReadOnly) return; //don't move the camera when typing!
            }
            if (ActiveControl is ComboBox)
            {
                var cb = ActiveControl as ComboBox;
                if (cb.DropDownStyle != ComboBoxStyle.DropDownList) return; //nontypable combobox
            }

            bool enablemove = true;// (!iseditmode) || (MouseLButtonDown && (GrabbedMarker == null) && (GrabbedWidget == null));

            Input.KeyDown(e, enablemove);

            var k = e.KeyCode;
            var kb = Input.keyBindings;
            bool ctrl = Input.CtrlPressed;
            bool shift = Input.ShiftPressed;


            if (!ctrl)
            {
                if (k == kb.MoveSlowerZoomIn)
                {
                    camera.MouseZoom(1);
                }
                if (k == kb.MoveFasterZoomOut)
                {
                    camera.MouseZoom(-1);
                }
            }


            if (!Input.kbmoving) //don't trigger further actions if moving.
            {
                if (!ctrl)
                {
                    //switch widget modes and spaces.
                    //if ((k == keyBindings.ExitEditMode))
                    //{
                    //    if (Widget.Mode == WidgetMode.Default) ToggleWidgetSpace();
                    //    else SetWidgetMode("Default");
                    //}
                    //if ((k == keyBindings.EditPosition))// && !enablemove)
                    //{
                    //    if (Widget.Mode == WidgetMode.Position) ToggleWidgetSpace();
                    //    else SetWidgetMode("Position");
                    //}
                    //if ((k == keyBindings.EditRotation))// && !enablemove)
                    //{
                    //    if (Widget.Mode == WidgetMode.Rotation) ToggleWidgetSpace();
                    //    else SetWidgetMode("Rotation");
                    //}
                    //if ((k == keyBindings.EditScale))// && !enablemove)
                    //{
                    //    if (Widget.Mode == WidgetMode.Scale) ToggleWidgetSpace();
                    //    else SetWidgetMode("Scale");
                    //}
                    //if (k == keyBindings.ToggleMouseSelect)
                    //{
                    //    SetMouseSelect(!MouseSelectEnabled);
                    //}
                    //if (k == keyBindings.ToggleToolbar)
                    //{
                    //    ToggleToolbar();
                    //}
                    //if (k == Keys.P)
                    //{
                    //    //TEMPORARY!
                    //    SetControlMode((ControlMode == WorldControlMode.Free) ? WorldControlMode.Ped : WorldControlMode.Free);
                    //}
                }
                else
                {
                    //switch (k)
                    //{
                    //    case Keys.N:
                    //        New();
                    //        break;
                    //    case Keys.O:
                    //        Open();
                    //        break;
                    //    case Keys.S:
                    //        if (shift) SaveAll();
                    //        else Save();
                    //        break;
                    //    case Keys.Z:
                    //        Undo();
                    //        break;
                    //    case Keys.Y:
                    //        Redo();
                    //        break;
                    //    case Keys.C:
                    //        CopyItem();
                    //        break;
                    //    case Keys.V:
                    //        PasteItem();
                    //        break;
                    //    case Keys.U:
                    //        ToolsPanelShowButton.Visible = !ToolsPanelShowButton.Visible;
                    //        break;
                    //}
                }
                //if (k == Keys.Escape) //temporary? panic get cursor back
                //{
                //    if (ControlMode != WorldControlMode.Free) SetControlMode(WorldControlMode.Free);
                //}
            }

            //if (ControlMode != WorldControlMode.Free)
            //{
            //    e.Handled = true;
            //}
        }

        private void ModelForm_KeyUp(object sender, KeyEventArgs e)
        {
            Input.KeyUp(e);

            if (ActiveControl is TextBox)
            {
                var tb = ActiveControl as TextBox;
                if (!tb.ReadOnly) return; //don't move the camera when typing!
            }
            if (ActiveControl is ComboBox)
            {
                var cb = ActiveControl as ComboBox;
                if (cb.DropDownStyle != ComboBoxStyle.DropDownList) return; //non-typable combobox
            }

            //if (ControlMode != WorldControlMode.Free)
            //{
            //    e.Handled = true;
            //}
        }

        private void ModelForm_Deactivate(object sender, EventArgs e)
        {
            //try not to lock keyboard movement if the form loses focus.
            Input.KeyboardStop();
        }

        private void StatsUpdateTimer_Tick(object sender, EventArgs e)
        {
            StatsLabel.Text = Renderer.GetStatusText();

            if (Renderer.timerunning)
            {
                float fv = Renderer.timeofday * 60.0f;
                //TimeOfDayTrackBar.Value = (int)fv;
                UpdateTimeOfDayLabel();
            }

            //CameraPositionTextBox.Text = FloatUtil.GetVector3String(camera.Position, "0.##");
        }

        private void ToolsPanelShowButton_Click(object sender, EventArgs e)
        {
            ToolsPanel.Visible = true;
        }

        private void ToolsPanelHideButton_Click(object sender, EventArgs e)
        {
            ToolsPanel.Visible = false;
        }

        private void ToolsDragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                toolsPanelResizing = true;
                toolsPanelResizeStartX = e.X + ToolsPanel.Left + ToolsDragPanel.Left;
                toolsPanelResizeStartLeft = ToolsPanel.Left;
                toolsPanelResizeStartRight = ToolsPanel.Right;
            }
        }

        private void ToolsDragPanel_MouseUp(object sender, MouseEventArgs e)
        {
            toolsPanelResizing = false;
        }

        private void ToolsDragPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (toolsPanelResizing)
            {
                int rx = e.X + ToolsPanel.Left + ToolsDragPanel.Left;
                int dx = rx - toolsPanelResizeStartX;
                ToolsPanel.Width = toolsPanelResizeStartRight - toolsPanelResizeStartLeft + dx;
            }
        }

        private void ModelsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                UpdateSelectionDrawFlags(e.Node);
            }
        }

        private void ModelsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                e.Node.Checked = !e.Node.Checked;
                //UpdateSelectionDrawFlags(e.Node);
            }
        }

        private void ModelsTreeView_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //stops annoying ding sound...
        }

        private void HDRRenderingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.shaders.hdr = HDRRenderingCheckBox.Checked;
            }
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.shaders.shadows = ShadowsCheckBox.Checked;
            }
        }

        private void SkydomeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderskydome = SkydomeCheckBox.Checked;
            //Renderer.controllightdir = !Renderer.renderskydome;
        }

        private void ControlLightDirCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.controllightdir = ControlLightDirCheckBox.Checked;
        }

        private void TimeOfDayTrackBar_Scroll(object sender, EventArgs e)
        {
            int v = TimeOfDayTrackBar.Value;
            float fh = v / 60.0f;
            UpdateTimeOfDayLabel();
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.timeofday = fh;
                timecycle.SetTime(Renderer.timeofday);
            }
        }

        private void ShowCollisionMeshesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendercollisionmeshes = ShowCollisionMeshesCheckBox.Checked;
            Renderer.rendercollisionmeshlayerdrawable = ShowCollisionMeshesCheckBox.Checked;
        }

        private void WireframeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.wireframe = WireframeCheckBox.Checked;
        }

        private void AnisotropicFilteringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
        }

        private void HDTexturesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderhdtextures = HDTexturesCheckBox.Checked;
        }

        private void RenderModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextureSamplerComboBox.Enabled = false;
            TextureCoordsComboBox.Enabled = false;
            switch (RenderModeComboBox.Text)
            {
                default:
                case "Default":
                    Renderer.shaders.RenderMode = WorldRenderMode.Default;
                    break;
                case "Single texture":
                    Renderer.shaders.RenderMode = WorldRenderMode.SingleTexture;
                    TextureSamplerComboBox.Enabled = true;
                    TextureCoordsComboBox.Enabled = true;
                    break;
                case "Vertex normals":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexNormals;
                    break;
                case "Vertex tangents":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexTangents;
                    break;
                case "Vertex colour 1":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.shaders.RenderVertexColourIndex = 1;
                    break;
                case "Vertex colour 2":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.shaders.RenderVertexColourIndex = 2;
                    break;
                case "Vertex colour 3":
                    Renderer.shaders.RenderMode = WorldRenderMode.VertexColour;
                    Renderer.shaders.RenderVertexColourIndex = 3;
                    break;
                case "Texture coord 1":
                    Renderer.shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.shaders.RenderTextureCoordIndex = 1;
                    break;
                case "Texture coord 2":
                    Renderer.shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.shaders.RenderTextureCoordIndex = 2;
                    break;
                case "Texture coord 3":
                    Renderer.shaders.RenderMode = WorldRenderMode.TextureCoord;
                    Renderer.shaders.RenderTextureCoordIndex = 3;
                    break;
            }
        }

        private void TextureSamplerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextureSamplerComboBox.SelectedItem is ShaderParamNames)
            {
                Renderer.shaders.RenderTextureSampler = (ShaderParamNames)TextureSamplerComboBox.SelectedItem;
            }
        }

        private void TextureCoordsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TextureCoordsComboBox.Text)
            {
                default:
                case "Texture coord 1":
                    Renderer.shaders.RenderTextureSamplerCoord = 1;
                    break;
                case "Texture coord 2":
                    Renderer.shaders.RenderTextureSamplerCoord = 2;
                    break;
                case "Texture coord 3":
                    Renderer.shaders.RenderTextureSamplerCoord = 3;
                    break;
            }
        }

        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableGrid = GridCheckBox.Checked;
        }

        private void GridSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            float newgs;
            float.TryParse(GridSizeComboBox.Text, out newgs);
            if (newgs != gridSize)
            {
                gridSize = newgs;
                UpdateGridVerts();
            }
        }

        private void GridCountComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int newgc;
            int.TryParse(GridCountComboBox.Text, out newgc);
            if (newgc != gridCount)
            {
                gridCount = newgc;
                UpdateGridVerts();
            }
        }

        private void SkeletonsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderskeletons = SkeletonsCheckBox.Checked;
        }

        private void FragGlassCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderfragwindows = FragGlassCheckBox.Checked;
        }

        private void ErrorConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConsolePanel.Visible = ErrorConsoleCheckBox.Checked;
        }

        private void StatusBarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StatusStrip.Visible = StatusBarCheckBox.Checked;
        }

        private void SaveAllTexturesButton_Click(object sender, EventArgs e)
        {
            SaveAllTextures(true);
        }

        private void SaveSharedTexturesButton_Click(object sender, EventArgs e)
        {
            SaveAllTextures(false);
        }

        private void SaveButton_ButtonClick(object sender, EventArgs e)
        {
            Save();
        }

        private void SaveMenuButton_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void SaveAsMenuButton_Click(object sender, EventArgs e)
        {
            Save(true);
        }

        private void SaveAllTexturesMenuButton_Click(object sender, EventArgs e)
        {
            SaveAllTextures(true);
        }

        private void SaveSharedTexturesMenuButton_Click(object sender, EventArgs e)
        {
            SaveAllTextures(false);
        }

        private void ClipDictComboBox_TextChanged(object sender, EventArgs e)
        {
            LoadClipDict(ClipDictComboBox.Text);
        }

        private void ClipComboBox_TextChanged(object sender, EventArgs e)
        {
            SelectClip(ClipComboBox.Text);
        }

        private void EnableRootMotionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            EnableRootMotion = EnableRootMotionCheckBox.Checked;
        }

        private void DeferredShadingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.deferred = DeferredShadingCheckBox.Checked;
        }

        private void HDLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderlights = HDLightsCheckBox.Checked;
        }

        private void ToolbarMaterialEditorButton_Click(object sender, EventArgs e)
        {
            ShowMaterialEditor();
        }

        private void ToolbarTextureEditorButton_Click(object sender, EventArgs e)
        {
            ShowTextureEditor();
        }

        private void ToolbarLightEditorButton_Click(object sender, EventArgs e)
        {
            ShowLightEditor();
        }

        private void ToolbarMoveButton_Click(object sender, EventArgs e)
        {
            SetWidgetMode(ToolbarMoveButton.Checked ? WidgetMode.Default : WidgetMode.Position);
        }

        private void ToolbarRotateButton_Click(object sender, EventArgs e)
        {
            SetWidgetMode(ToolbarRotateButton.Checked ? WidgetMode.Default : WidgetMode.Rotation);
        }

        private void ToolbarScaleButton_Click(object sender, EventArgs e)
        {
            SetWidgetMode(ToolbarScaleButton.Checked ? WidgetMode.Default : WidgetMode.Scale);
        }

        private void OptionsShowOutlinesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            showLightGizmos = OptionsShowOutlinesCheckBox.Checked;
        }

        private void SnapAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (Widget != null)
            {
                SetRotationSnapping((float)SnapAngleUpDown.Value);
            }
        }
    }
}
