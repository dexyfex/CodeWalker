using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Rendering;
using CodeWalker.World;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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


        bool enableGrid = true;
        float gridSize = 1.0f;
        int gridCount = 40;
        List<VertexTypePC> gridVerts = new List<VertexTypePC>();
        object gridSyncRoot = new object();

        GameFileCache gameFileCache = null;
        Archetype currentArchetype = null;
        bool updateArchetypeStatus = true;


        public ModelForm(ExploreForm ExpForm = null)
        {
            InitializeComponent();


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
            Renderer.renderskeletons = true;
            Renderer.SelectionFlagsTestAll = true;
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

            MetaName[] texsamplers = RenderableGeometry.GetTextureSamplerList();
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


            Renderer.BeginRender(context);

            Renderer.RenderSkyAndClouds();


            RenderSingleItem();


            RenderGrid(context);


            Renderer.RenderQueued();


            Renderer.RenderFinalPass();


            Renderer.EndRender();

            Monitor.Exit(Renderer.RenderSyncRoot);
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
                uint cblack = (uint)SharpDX.Color.Black.ToRgba();
                uint cgray = (uint)SharpDX.Color.DimGray.ToRgba();
                uint cred = (uint)SharpDX.Color.DarkRed.ToRgba();
                uint cgrn = (uint)SharpDX.Color.DarkGreen.ToRgba();
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







        private void RenderSingleItem()
        {

            uint hash = 0;
            Archetype arch = null;

            if (Ydr != null)
            {
                if (Ydr.Loaded)
                {
                    hash = Ydr?.RpfFileEntry?.ShortNameHash ?? 0;
                    arch = TryGetArchetype(hash);

                    Renderer.RenderDrawable(Ydr.Drawable, arch, null, hash);
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
                            arch = TryGetArchetype(kvp.Key);

                            Renderer.RenderDrawable(kvp.Value, arch, null, Ydd.RpfFileEntry.ShortNameHash);
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
                            arch = TryGetArchetype(kvp.Key);

                            Renderer.RenderDrawable(kvp.Value, arch, null, kvp.Key);
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

                        hash = Yft?.RpfFileEntry?.ShortNameHash ?? 0;
                        arch = TryGetArchetype(hash);

                        Renderer.RenderFragment(arch, null, f, hash);
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
            else if (Ypt != null)
            {
                if (Ypt.Loaded)
                {
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

            UpdateModelsUI(ydr.Drawable);
        }
        public void LoadModels(YddFile ydd)
        {
            if (ydd == null) return;

            FileName = ydd.Name;
            Ydd = ydd;

            UpdateModelsUI(ydd.Dict);

            DetailsPropertyGrid.SelectedObject = ydd;
        }
        public void LoadModel(YftFile yft)
        {
            if (yft == null) return;

            FileName = yft.Name;
            Yft = yft;

            UpdateModelsUI(yft.Fragment.Drawable);
        }
        public void LoadModel(YbnFile ybn)
        {
            if (ybn == null) return;

            FileName = ybn.Name;
            Ybn = ybn;

            UpdateBoundsUI(ybn.Bounds);
        }
        public void LoadParticles(YptFile ypt)
        {
            if (ypt == null) return;

            FileName = ypt.Name;
            Ypt = ypt;

            UpdateModelsUI(ypt.DrawableDict);

            DetailsPropertyGrid.SelectedObject = ypt;//.PtfxList;
        }
        public void LoadNavmesh(YnvFile ynv)
        {
            if (ynv == null) return;

            FileName = ynv.Name;
            Ynv = ynv;

            //UpdateModelsUI(ypt.Particles.Drawable);
        }








        private void UpdateFormTitle()
        {
            Text = fileName + " - CodeWalker by dexyfex";
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

            float moveSpeed = 10.0f;


            Input.Update(elapsed);

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
                camera.ControllerRotate(Input.xblx + Input.xbrx, Input.xbly + Input.xbry);

                float zoom = 0.0f;
                float zoomspd = s.XInputZoomSpeed;
                float zoomamt = zoomspd * elapsed;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadUp)) zoom += zoomamt;
                if (Input.ControllerButtonPressed(GamepadButtonFlags.DPadDown)) zoom -= zoomamt;

                camera.ControllerZoom(zoom);

                float acc = 0.0f;
                float accspd = s.XInputMoveSpeed;//actually accel speed...
                acc += Input.xbrt * accspd;
                acc -= Input.xblt * accspd;

                Vector3 newdir = camera.ViewDirection; //maybe use the "vehicle" direction...?
                Input.xbcontrolvelocity += (acc * elapsed);

                if (Input.ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder)) //handbrake...
                {
                    Input.xbcontrolvelocity *= Math.Max(0.75f - elapsed, 0);//not ideal for low fps...
                                                                        //xbcontrolvelocity = 0.0f;
                    if (Math.Abs(Input.xbcontrolvelocity) < 0.001f) Input.xbcontrolvelocity = 0.0f;
                }

                camEntity.Velocity = newdir * Input.xbcontrolvelocity;
                camEntity.Position += camEntity.Velocity * elapsed;


                //fire!
                //if (ControllerButtonJustPressed(GamepadButtonFlags.LeftShoulder))
                //{
                //    SpawnTestEntity(true);
                //}

            }



        }




        private void UpdateModelsUI(DrawableBase drawable)
        {
            DetailsPropertyGrid.SelectedObject = drawable;

            DrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = false;
            TexturesTreeView.Nodes.Clear();
            if (drawable != null)
            {
                AddDrawableModelsTreeNodes(drawable.DrawableModelsHigh, "High Detail", true);
                AddDrawableModelsTreeNodes(drawable.DrawableModelsMedium, "Medium Detail", false);
                AddDrawableModelsTreeNodes(drawable.DrawableModelsLow, "Low Detail", false);
                AddDrawableModelsTreeNodes(drawable.DrawableModelsVeryLow, "Very Low Detail", false);
                //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsX, "X Detail", false);
            }
        }
        private void UpdateModelsUI(FragType frag)
        {
            DetailsPropertyGrid.SelectedObject = frag;

            var drawable = frag.Drawable;

            DrawableDrawFlags.Clear();
            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            ModelsTreeView.Nodes.Clear();
            ModelsTreeView.ShowRootLines = false;
            TexturesTreeView.Nodes.Clear();
            if (drawable != null)
            {
                AddDrawableModelsTreeNodes(drawable.DrawableModelsHigh, "High Detail", true);
                AddDrawableModelsTreeNodes(drawable.DrawableModelsMedium, "Medium Detail", false);
                AddDrawableModelsTreeNodes(drawable.DrawableModelsLow, "Low Detail", false);
                AddDrawableModelsTreeNodes(drawable.DrawableModelsVeryLow, "Very Low Detail", false);
                //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsX, "X Detail", false);
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
                foreach (var kvp in dict)
                {
                    AddDrawableTreeNode(kvp.Value, kvp.Key, check);
                    check = false;
                }
            }

            ToolsPanel.Visible = true; //show the panel by default for dictionaries...
        }
        private void UpdateBoundsUI(Bounds bounds)
        {
            DetailsPropertyGrid.SelectedObject = bounds;
        }
        private void UpdateNavmeshUI(YnvFile ynv)
        {
            DetailsPropertyGrid.SelectedObject = ynv.Nav;
        }


        private void AddDrawableTreeNode(DrawableBase drawable, uint hash, bool check)
        {
            MetaHash mhash = new MetaHash(hash);
            
            var dnode = ModelsTreeView.Nodes.Add(mhash.ToString());
            dnode.Tag = drawable;
            dnode.Checked = check;

            AddDrawableModelsTreeNodes(drawable.DrawableModelsHigh, "High Detail", true, dnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModelsMedium, "Medium Detail", false, dnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModelsLow, "Low Detail", false, dnode);
            AddDrawableModelsTreeNodes(drawable.DrawableModelsVeryLow, "Very Low Detail", false, dnode);
            //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsX, "X Detail", false, dnode);

        }
        private void AddDrawableModelsTreeNodes(ResourcePointerList64<DrawableModel> models, string prefix, bool check, TreeNode parentDrawableNode = null)
        {
            if (models == null) return;
            if (models.data_items == null) return;

            for (int mi = 0; mi < models.data_items.Length; mi++)
            {
                var tnc = (parentDrawableNode != null) ? parentDrawableNode.Nodes : ModelsTreeView.Nodes;

                var model = models.data_items[mi];
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

                if ((model.Geometries == null) || (model.Geometries.data_items == null)) continue;

                foreach (var geom in model.Geometries.data_items)
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








        private void ModelForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void ModelForm_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = true; break;
                case MouseButtons.Right: MouseRButtonDown = true; break;
            }

            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;


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
                camera.MouseRotate(dx, dy);
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
            if (TextureSamplerComboBox.SelectedItem is MetaName)
            {
                Renderer.shaders.RenderTextureSampler = (MetaName)TextureSamplerComboBox.SelectedItem;
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

        private void ErrorConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConsolePanel.Visible = ErrorConsoleCheckBox.Checked;
        }

        private void StatusBarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StatusStrip.Visible = StatusBarCheckBox.Checked;
        }
    }
}
