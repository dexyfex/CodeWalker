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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = SharpDX.Color;

namespace CodeWalker.Peds
{
    public partial class PedsForm : Form, DXForm
    {
        public Form Form { get { return this; } } //for DXForm/DXManager use

        public Renderer Renderer = null;
        public object RenderSyncRoot { get { return Renderer.RenderSyncRoot; } }

        volatile bool formopen = false;
        volatile bool running = false;
        volatile bool pauserendering = false;
        //volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera;
        Timecycle timecycle;
        Weather weather;
        Clouds clouds;

        Entity camEntity = new Entity();


        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;


        public GameFileCache GameFileCache { get; } = GameFileCacheFactory.Create();


        InputManager Input = new InputManager();


        bool initedOk = false;



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



        string SelectedPedName = string.Empty;
        MetaHash SelectedPedHash = 0;//ped name hash
        CPedModelInfo__InitData SelectedPedInit = null; //ped init data
        YddFile SelectedPedYdd = null; //ped drawables
        YftFile SelectedPedYft = null; //ped skeleton YFT
        PedFile SelectedPedYmt = null; //ped variation info

        Drawable SelectedHead = null;
        Drawable SelectedBerd = null;
        Drawable SelectedHair = null;
        Drawable SelectedUppr = null;
        Drawable SelectedLowr = null;
        Drawable SelectedHand = null;
        Drawable SelectedFeet = null;
        Drawable SelectedTeef = null;
        Drawable SelectedAccs = null;
        Drawable SelectedTask = null;
        Drawable SelectedDecl = null;
        Drawable SelectedJbib = null;







        public PedsForm()
        {
            InitializeComponent();

            Renderer = new Renderer(this, GameFileCache);
            camera = Renderer.camera;
            timecycle = Renderer.timecycle;
            weather = Renderer.weather;
            clouds = Renderer.clouds;

            initedOk = Renderer.Init();

            Renderer.controllightdir = !Settings.Default.Skydome;
            Renderer.rendercollisionmeshes = false;
            Renderer.renderclouds = false;
            //Renderer.renderclouds = true;
            //Renderer.individualcloudfrag = "Contrails";
            Renderer.rendermoon = false;
            Renderer.renderskeletons = true;
            Renderer.SelectionFlagsTestAll = true;

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


            camera.FollowEntity = camEntity;
            camera.FollowEntity.Position = Vector3.Zero;// prevworldpos;
            camera.FollowEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);
            camera.TargetDistance = 2.0f;
            camera.CurrentDistance = 2.0f;
            camera.TargetRotation.Y = 0.2f;
            camera.CurrentRotation.Y = 0.2f;
            camera.TargetRotation.X = 0.5f * (float)Math.PI;
            camera.CurrentRotation.X = 0.5f * (float)Math.PI;


            LoadSettings();


            formopen = true;
            new Thread(new ThreadStart(ContentThread)).Start();

            frametimer.Start();

        }
        public void CleanupScene()
        {
            formopen = false;

            Renderer.DeviceDestroyed();

            int count = 0;
            while (running && (count < 5000)) //wait for the content thread to exit gracefully
            {
                Thread.Sleep(1);
                count++;
            }
        }
        public void RenderScene(DeviceContext context)
        {
            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            frametimer.Restart();

            if (pauserendering) return;

            if (!Monitor.TryEnter(Renderer.RenderSyncRoot, 50))
            { return; } //couldn't get a lock, try again next time

            UpdateControlInputs(elapsed);
            //space.Update(elapsed);


            Renderer.Update(elapsed, MouseLastPoint.X, MouseLastPoint.Y);



            //UpdateWidgets();
            //BeginMouseHitTest();




            Renderer.BeginRender(context);

            Renderer.RenderSkyAndClouds();

            Renderer.SelectedDrawable = null;// SelectedItem.Drawable;


            RenderPed();

            //UpdateMouseHitsFromRenderer();
            //RenderSelection();


            RenderGrid(context);


            Renderer.RenderQueued();

            //Renderer.RenderBounds(MapSelectionMode.Entity);

            //Renderer.RenderSelectionGeometry(MapSelectionMode.Entity);

            //RenderMoused();

            Renderer.RenderFinalPass();

            //RenderMarkers();
            //RenderWidgets();

            Renderer.EndRender();

            Monitor.Exit(Renderer.RenderSyncRoot);

            //UpdateMarkerSelectionPanelInvoke();
        }
        public void BuffersResized(int w, int h)
        {
            Renderer.BuffersResized(w, h);
        }





        private void Init()
        {
            //called from PedForm_Load

            if (!initedOk)
            {
                Close();
                return;
            }


            MouseWheel += PedsForm_MouseWheel;

            if (!GTAFolder.UpdateGTAFolder(true))
            {
                Close();
                return;
            }



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


        private void ContentThread()
        {
            //main content loading thread.
            running = true;

            UpdateStatus("Scanning...");

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
            }
            catch
            {
                MessageBox.Show("Keys not found! This shouldn't happen.");
                Close();
                return;
            }

            GameFileCache.EnableDlc = true;
            GameFileCache.LoadPeds = true;
            GameFileCache.LoadArchetypes = false;//to speed things up a little
            GameFileCache.BuildExtendedJenkIndex = false;//to speed things up a little
            GameFileCache.DoFullStringIndex = true;//to get all global text from DLC...
            GameFileCache.Init(UpdateStatus, LogError);

            //UpdateDlcListComboBox(gameFileCache.DlcNameList);

            //EnableCacheDependentUI();

            UpdateGlobalPedsUI();


            LoadWorld();



            //initialised = true;

            //EnableDLCModsUI();

            //UpdateStatus("Ready");


            Task.Run(() => {
                while (formopen && !IsDisposed) //renderer content loop
                {
                    bool rcItemsPending = Renderer.ContentThreadProc();

                    if (!rcItemsPending)
                    {
                        Thread.Sleep(1); //sleep if there's nothing to do
                    }
                }
            });

            while (formopen && !IsDisposed) //main asset loop
            {
                bool fcItemsPending = GameFileCache.ContentThreadProc();

                if (!fcItemsPending)
                {
                    Thread.Sleep(1); //sleep if there's nothing to do
                }
            }

            GameFileCache.Clear();

            running = false;
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



        private void LoadWorld()
        {
            UpdateStatus("Loading timecycles...");
            timecycle.Init(GameFileCache, UpdateStatus);
            timecycle.SetTime(Renderer.timeofday);

            UpdateStatus("Loading materials...");
            BoundsMaterialTypes.Init(GameFileCache);

            UpdateStatus("Loading weather...");
            weather.Init(GameFileCache, UpdateStatus, timecycle);
            //UpdateWeatherTypesComboBox(weather);

            UpdateStatus("Loading clouds...");
            clouds.Init(GameFileCache, UpdateStatus, weather);
            //UpdateCloudTypesComboBox(clouds);

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
                    //TODO: error logging..
                    ConsoleTextBox.AppendText(text + "\r\n");
                    //StatusLabel.Text = text;
                    //MessageBox.Show(text);
                }
            }
            catch { }
        }




        private void UpdateMousePosition(MouseEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
            MouseLastPoint = e.Location;
        }

        private void RotateCam(int dx, int dy)
        {
            camera.MouseRotate(dx, dy);
        }

        private void MoveCameraToView(Vector3 pos, float rad)
        {
            //move the camera to a default place where the given sphere is fully visible.

            rad = Math.Max(0.01f, rad*0.1f);

            camera.FollowEntity.Position = pos;
            camera.TargetDistance = rad * 1.6f;
            camera.CurrentDistance = rad * 1.6f;

            camera.ZFar = Math.Min(rad * 200.0f, 12000.0f);
            camera.ZNear = Math.Min(camera.ZFar * 5e-5f, 0.5f);
            camera.UpdateProj = true;

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
                //updateArchetypeStatus = true;
            }
        }


        private void UpdateGlobalPedsUI()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { UpdateGlobalPedsUI(); }));
            }
            else
            {
                PedNameComboBox.Items.Clear();

                var peds = GameFileCache.PedsInitDict.Values.ToList();
                peds.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                foreach (var ped in peds)
                {
                    PedNameComboBox.Items.Add(ped.Name);
                }
                if (peds.Count > 0)
                {
                    PedNameComboBox.SelectedIndex = 0;
                }

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

                                var dname = child.GroupNameHash.ToString();
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsHigh, dname + " - High Detail", true);
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsMedium, dname + " - Medium Detail", false);
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsLow, dname + " - Low Detail", false);
                                AddDrawableModelsTreeNodes(cdrwbl.DrawableModelsVeryLow, dname + " - Very Low Detail", false);
                            }
                        }
                    }
                }

            }
        }



        public void LoadPed()
        {
            var pedname = PedNameComboBox.Text;
            var pednamel = pedname.ToLowerInvariant();
            MetaHash pedhash = JenkHash.GenHash(pednamel);

            SelectedPedName = string.Empty;
            SelectedPedHash = 0;
            SelectedPedInit = null;
            SelectedPedYdd = null;
            SelectedPedYft = null;
            SelectedPedYmt = null;
            ClearCombo(CompHeadComboBox); SelectedHead = null;
            ClearCombo(CompBerdComboBox); SelectedBerd = null;
            ClearCombo(CompHairComboBox); SelectedHair = null;
            ClearCombo(CompUpprComboBox); SelectedUppr = null;
            ClearCombo(CompLowrComboBox); SelectedLowr = null;
            ClearCombo(CompHandComboBox); SelectedHand = null;
            ClearCombo(CompFeetComboBox); SelectedFeet = null;
            ClearCombo(CompTeefComboBox); SelectedTeef = null;
            ClearCombo(CompAccsComboBox); SelectedAccs = null;
            ClearCombo(CompTaskComboBox); SelectedTask = null;
            ClearCombo(CompDeclComboBox); SelectedDecl = null;
            ClearCombo(CompJbibComboBox); SelectedJbib = null;

            CPedModelInfo__InitData initdata = null;
            if (!GameFileCache.PedsInitDict.TryGetValue(pedhash, out initdata)) return;
            

            bool pedchange = SelectedPedHash != pedhash;
            SelectedPedName = pedname;
            SelectedPedHash = pedhash;
            SelectedPedInit = initdata;
            SelectedPedYdd = GameFileCache.GetYdd(pedhash);
            SelectedPedYft = GameFileCache.GetYft(pedhash);
            GameFileCache.PedVariationsDict?.TryGetValue(pedhash, out SelectedPedYmt);

            while ((SelectedPedYdd != null) && (!SelectedPedYdd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                SelectedPedYdd = GameFileCache.GetYdd(SelectedPedHash);
            }
            while ((SelectedPedYft != null) && (!SelectedPedYft.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                SelectedPedYft = GameFileCache.GetYft(SelectedPedHash);
            }

            LoadModel(SelectedPedYft, pedchange);


            var vi = SelectedPedYmt?.VariationInfo;
            if (vi != null)
            {
                PopulateCompCombo(CompHeadComboBox, vi.GetVariations(0));
                PopulateCompCombo(CompBerdComboBox, vi.GetVariations(1));
                PopulateCompCombo(CompHairComboBox, vi.GetVariations(2));
                PopulateCompCombo(CompUpprComboBox, vi.GetVariations(3));
                PopulateCompCombo(CompLowrComboBox, vi.GetVariations(4));
                PopulateCompCombo(CompHandComboBox, vi.GetVariations(5));
                PopulateCompCombo(CompFeetComboBox, vi.GetVariations(6));
                PopulateCompCombo(CompTeefComboBox, vi.GetVariations(7));
                PopulateCompCombo(CompAccsComboBox, vi.GetVariations(8));
                PopulateCompCombo(CompTaskComboBox, vi.GetVariations(9));
                PopulateCompCombo(CompDeclComboBox, vi.GetVariations(10));
                PopulateCompCombo(CompJbibComboBox, vi.GetVariations(11));
            }



        }

        public void LoadModel(YftFile yft, bool movecamera = true)
        {
            if (yft == null) return;

            //FileName = yft.Name;
            //Yft = yft;

            var dr = yft.Fragment?.Drawable;
            if (movecamera && (dr != null))
            {
                MoveCameraToView(dr.BoundingCenter, dr.BoundingSphereRadius);
            }

            UpdateModelsUI(yft.Fragment.Drawable);
        }



        private void ClearCombo(ComboBox c)
        {
            c.Items.Clear();
            c.Text = string.Empty;
        }
        private void PopulateCompCombo(ComboBox c, MUnk_3538495220 vars)
        {
            if (vars?.Variations == null) return;
            foreach (var item in vars.Variations)
            {
                c.Items.Add(item.GetDrawableName());
            }
            if (vars.Variations.Length > 0)
            {
                c.SelectedIndex = 0;
            }
        }



        private Drawable GetComponentDrawable(string name)
        {
            var namel = name.ToLowerInvariant();
            MetaHash hash = JenkHash.GenHash(namel);
            Drawable d;

            if (SelectedPedYdd?.Dict != null)
            {
                if (SelectedPedYdd.Dict.TryGetValue(hash, out d)) return d;
            }

            Dictionary<MetaHash, RpfFileEntry> peddict = null;
            if (GameFileCache.PedDrawableDicts.TryGetValue(SelectedPedHash, out peddict))
            {
                RpfFileEntry file = null;
                if (peddict.TryGetValue(hash, out file))
                {
                    var ydd = GameFileCache.GetFileUncached<YddFile>(file);
                    while ((ydd != null) && (!ydd.Loaded))
                    {
                        Thread.Sleep(20);//kinda hacky
                        GameFileCache.TryLoadEnqueue(ydd);
                    }
                    if (ydd?.Drawables?.Length > 0)
                    {
                        return ydd.Drawables[0];//should only be one in this dict
                    }
                }
            }

            return null;
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







        private void RenderPed()
        {

            YftFile yft = SelectedPedYft;// GameFileCache.GetYft(SelectedModelHash);
            if (yft != null)
            {
                if (yft.Loaded)
                {
                    if (yft.Fragment != null)
                    {
                        var f = yft.Fragment;

                        var txdhash = 0u;// SelectedVehicleHash;// yft.RpfFileEntry?.ShortNameHash ?? 0;
                        //var namelower = yft.RpfFileEntry?.GetShortNameLower();

                        Archetype arch = null;// TryGetArchetype(hash);

                        Renderer.RenderFragment(arch, null, f, txdhash);

                        //seldrwbl = f.Drawable;
                    }
                }
            }

        }










        private void PedsForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void PedsForm_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = true; break;
                case MouseButtons.Right: MouseRButtonDown = true; break;
            }

            if (!ToolsPanelShowButton.Focused)
            {
                ToolsPanelShowButton.Focus(); //make sure no textboxes etc are focused!
            }

            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;

            if (MouseLButtonDown)
            {
            }

            if (MouseRButtonDown)
            {
                //SelectMousedItem();
            }

            MouseX = e.X; //to stop jumps happening on mousedown, sometimes the last MouseMove event was somewhere else... (eg after clicked a menu)
            MouseY = e.Y;
        }

        private void PedsForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = false; break;
                case MouseButtons.Right: MouseRButtonDown = false; break;
            }



            if (e.Button == MouseButtons.Left)
            {
            }
        }

        private void PedsForm_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - MouseX;
            int dy = e.Y - MouseY;

            //if (MouseInvert)
            //{
            //    dy = -dy;
            //}

            //if (ControlMode == WorldControlMode.Free && !ControlBrushEnabled)
            {
                if (MouseLButtonDown)
                {
                    RotateCam(dx, dy);
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

                UpdateMousePosition(e);

            }



        }

        private void PedsForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                camera.MouseZoom(e.Delta);
            }
        }

        private void PedsForm_KeyDown(object sender, KeyEventArgs e)
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

                }
                else
                {
                    //switch (k)
                    //{
                    //    //case Keys.N:
                    //    //    New();
                    //    //    break;
                    //    //case Keys.O:
                    //    //    Open();
                    //    //    break;
                    //    //case Keys.S:
                    //    //    if (shift) SaveAll();
                    //    //    else Save();
                    //    //    break;
                    //    //case Keys.Z:
                    //    //    Undo();
                    //    //    break;
                    //    //case Keys.Y:
                    //    //    Redo();
                    //    //    break;
                    //    //case Keys.C:
                    //    //    CopyItem();
                    //    //    break;
                    //    //case Keys.V:
                    //    //    PasteItem();
                    //    //    break;
                    //    //case Keys.U:
                    //    //    ToolsPanelShowButton.Visible = !ToolsPanelShowButton.Visible;
                    //    //    break;
                    //}
                }
            }

            //if (ControlMode != WorldControlMode.Free || ControlBrushEnabled)
            //{
            //    e.Handled = true;
            //}
        }

        private void PedsForm_KeyUp(object sender, KeyEventArgs e)
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

        private void PedsForm_Deactivate(object sender, EventArgs e)
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

        private void StatusBarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StatusStrip.Visible = StatusBarCheckBox.Checked;
        }

        private void ErrorConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConsolePanel.Visible = ErrorConsoleCheckBox.Checked;
        }

        private void TextureViewerButton_Click(object sender, EventArgs e)
        {
            //TextureDictionary td = null;

            //if ((Ydr != null) && (Ydr.Loaded))
            //{
            //    td = Ydr.Drawable?.ShaderGroup?.TextureDictionary;
            //}
            //else if ((Yft != null) && (Yft.Loaded))
            //{
            //    td = Yft.Fragment?.Drawable?.ShaderGroup?.TextureDictionary;
            //}

            //if (td != null)
            //{
            //    YtdForm f = new YtdForm();
            //    f.Show();
            //    f.LoadTexDict(td, fileName);
            //    //f.LoadYtd(ytd);
            //}
            //else
            //{
            //    MessageBox.Show("Couldn't find embedded texture dict.");
            //}
        }





        private void PedNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!GameFileCache.IsInited) return;

            LoadPed();
        }

        private void CompHeadComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedHead = GetComponentDrawable(CompHeadComboBox.Text);
        }

        private void CompBerdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedBerd = GetComponentDrawable(CompBerdComboBox.Text);
        }

        private void CompHairComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedHair = GetComponentDrawable(CompHairComboBox.Text);
        }

        private void CompUpprComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedUppr = GetComponentDrawable(CompUpprComboBox.Text);
        }

        private void CompLowrComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedLowr = GetComponentDrawable(CompLowrComboBox.Text);
        }

        private void CompHandComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedHand = GetComponentDrawable(CompHandComboBox.Text);
        }

        private void CompFeetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedFeet = GetComponentDrawable(CompFeetComboBox.Text);
        }

        private void CompTeefComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTeef = GetComponentDrawable(CompTeefComboBox.Text);
        }

        private void CompAccsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedAccs = GetComponentDrawable(CompAccsComboBox.Text);
        }

        private void CompTaskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTask = GetComponentDrawable(CompTaskComboBox.Text);
        }

        private void CompDeclComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedDecl = GetComponentDrawable(CompDeclComboBox.Text);
        }

        private void CompJbibComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedJbib = GetComponentDrawable(CompJbibComboBox.Text);
        }



    }
}
