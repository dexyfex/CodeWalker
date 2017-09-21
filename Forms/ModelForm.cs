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
        DXManager dxman = new DXManager();
        public DXManager DXMan { get { return dxman; } }
        Device currentdevice;
        public Device Device { get { return currentdevice; } }
        object rendersyncroot = new object();
        public object RenderSyncRoot { get { return rendersyncroot; } }

        ShaderManager shaders;

        volatile bool formopen = false;
        //volatile bool running = false;
        volatile bool pauserendering = false;
        //volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera = new Camera();
        Timecycle timecycle = new Timecycle();
        Weather weather = new Weather();
        Clouds clouds = new Clouds();

        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;



        //public GameFileCache GameFileCache { get { return gameFileCache; } }
        //GameFileCache gameFileCache = new GameFileCache();
        RenderableCache renderableCache = new RenderableCache();


        Vector3 prevworldpos = new Vector3(0, 0, 0); //also the start pos

        Entity camEntity = new Entity();
        volatile bool kbmovefwd = false;
        volatile bool kbmovebck = false;
        volatile bool kbmovelft = false;
        volatile bool kbmovergt = false;
        volatile bool kbmoveup = false;
        volatile bool kbmovedn = false;
        volatile bool kbjump = false;

        KeyBindings keyBindings = new KeyBindings(Settings.Default.KeyBindings);
        //bool iseditmode = false;


        float timeofday = 12.0f;
        bool controltimeofday = true;
        bool timerunning = false;
        float timespeed = 0.5f;//min/sec
        //string weathertype = "";
        string individualcloudfrag = "contrails";
        Vector4 currentWindVec = Vector4.Zero;
        float currentWindTime = 0.0f;

        bool controllightdir = !Settings.Default.Skydome;//true; //if not, use timecycle
        float lightdirx = 2.25f;//radians // approx. light dir on map satellite view
        float lightdiry = 0.65f;//radians  - used for manual light placement
        bool renderskydome = Settings.Default.Skydome;

        bool rendernaturalambientlight = true;
        bool renderartificialambientlight = true;
        ShaderGlobalLights globalLights = new ShaderGlobalLights();

        double currentRealTime = 0;
        int framecount = 0;
        float fcelapsed = 0.0f;
        int fps = 0;

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

        bool waitforchildrentoload = true;
        bool rendercollisionmeshes = false;// Settings.Default.ShowCollisionMeshes;

        bool CtrlPressed = false;
        bool ShiftPressed = false;

        Controller xbcontroller = null;
        State xbcontrollerstate;
        State xbcontrollerstateprev;
        Vector4 xbmainaxes = Vector4.Zero;
        Vector4 xbmainaxesprev = Vector4.Zero;
        Vector2 xbtrigs = Vector2.Zero;
        Vector2 xbtrigsprev = Vector2.Zero;
        float xbcontrolvelocity = 0.0f;

        bool toolsPanelResizing = false;
        int toolsPanelResizeStartX = 0;
        int toolsPanelResizeStartLeft = 0;
        int toolsPanelResizeStartRight = 0;

        Dictionary<DrawableBase, bool> DrawableDrawFlags = new Dictionary<DrawableBase, bool>();
        Dictionary<DrawableModel, bool> ModelDrawFlags = new Dictionary<DrawableModel, bool>();
        Dictionary<DrawableGeometry, bool> GeometryDrawFlags = new Dictionary<DrawableGeometry, bool>();


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

            initedOk = dxman.Init(this, false);
        }

        private void Init()
        {
            //called from WorldForm_Load

            if (!initedOk)
            {
                Close();
                return;
            }


            MouseWheel += ModelForm_MouseWheel;

            string fldr = Settings.Default.GTAFolder;
            if (string.IsNullOrEmpty(fldr) || !Directory.Exists(fldr))
            {
                SelectFolderForm f = new SelectFolderForm();
                f.ShowDialog();
                if (f.Result == DialogResult.OK)
                {
                    fldr = f.SelectedFolder;
                }
                else
                {
                    Close();
                    return;
                }
            }
            if (!Directory.Exists(fldr))
            {
                MessageBox.Show("The specified folder does not exist:\n" + fldr);
                Close();
                return;
            }
            if (!File.Exists(fldr + "\\gta5.exe"))
            {
                MessageBox.Show("GTA5.exe not found in folder:\n" + fldr);
                Close();
                return;
            }

            Settings.Default.GTAFolder = fldr; //seems ok, save it for later




            MetaName[] texsamplers = RenderableGeometry.GetTextureSamplerList();
            foreach (var texsampler in texsamplers)
            {
                TextureSamplerComboBox.Items.Add(texsampler);
            }
            //TextureSamplerComboBox.SelectedIndex = 0;//LoadSettings will do this..


            UpdateGridVerts();
            GridSizeComboBox.SelectedIndex = 1;
            GridCountComboBox.SelectedIndex = 1;



            InitController();


            dxman.Start();
        }

        private void InitController()
        {
            xbcontroller = new Controller(UserIndex.One);
            if (!xbcontroller.IsConnected)
            {
                var controllers = new[] { new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
                foreach (var selectControler in controllers)
                {
                    if (selectControler.IsConnected)
                    {
                        xbcontroller = selectControler;
                        xbcontrollerstate = xbcontroller.GetState();
                        xbcontrollerstateprev = xbcontrollerstate;
                        break;
                    }
                }
            }
            else
            {
                xbcontrollerstate = xbcontroller.GetState();
                xbcontrollerstateprev = xbcontrollerstate;
            }

        }


        public void InitScene(Device device)
        {
            currentdevice = device;

            int width = ClientSize.Width;
            int height = ClientSize.Height;

            try
            {
                shaders = new ShaderManager(device, dxman);
                shaders.OnWindowResize(width, height); //init the buffers
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading shaders!\n" + ex.ToString());
                return;
            }

            //shaders.hdrLumBlendSpeed = 1000.0f;

            renderableCache.OnDeviceCreated(device);

            camera.OnWindowResize(width, height); //init the projection stuff
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

            renderableCache.OnDeviceDestroyed();

            shaders.Dispose();


            //int count = 0;
            //while (running && (count < 5000)) //wait for the content thread to exit gracefully
            //{
            //    Thread.Sleep(1);
            //    count++;
            //}

            currentdevice = null;
        }
        public void BuffersResized(int w, int h)
        {
            lock (rendersyncroot)
            {
                camera.OnWindowResize(w, h);
                shaders.OnWindowResize(w, h);
            }
        }
        public void RenderScene(DeviceContext context)
        {
            float elapsed = (float)frametimer.Elapsed.TotalSeconds;
            framecount++;
            fcelapsed += elapsed;
            if (fcelapsed >= 0.5f)
            {
                fps = framecount * 2;
                framecount = 0;
                fcelapsed -= 0.5f;
            }
            if (elapsed > 0.1f) elapsed = 0.1f;
            frametimer.Restart();

            currentRealTime += elapsed;

            if (pauserendering) return;

            if (!Monitor.TryEnter(rendersyncroot, 50))
            { return; } //couldn't get a lock, try again next time
            //Monitor.Enter(rendersyncroot);

            UpdateControlInputs(elapsed);

            UpdateTimeOfDay(elapsed);

            weather.Update(elapsed);

            //clouds.Update(elapsed);

            UpdateWindVector(elapsed);

            UpdateGlobalLights();

            camera.SetMousePosition(MouseLastPoint.X, MouseLastPoint.Y);

            camera.Update(elapsed);

            //UpdateWidgets();

            //HilightBoxes.Clear();
            //BoundingBoxes.Clear();
            //BoundingSpheres.Clear();
            //BeginMouseHitTest();


            dxman.ClearRenderTarget(context);

            shaders.BeginFrame(context, currentRealTime, elapsed);

            shaders.EnsureShaderTextures(gameFileCache, renderableCache);

            RenderSky(context);

            //RenderClouds(context);

            shaders.ClearDepth(context);

            //if (renderworld || rendermaps)
            //{
            //    RenderWorld();
            //    if (rendermaps)
            //    {
            //        RenderYmaps();
            //    }
            //}
            //else
            //{
            RenderSingleItem();
            //}

            RenderGrid(context);

            shaders.RenderQueued(context, camera, currentWindVec);

            //RenderBounds(context);

            //RenderSelection(context);

            //RenderMoused(context);

            //RenderSelectionGeometry(context);

            shaders.RenderFinalPass(context);

            //RenderMarkers(context);

            //RenderWidgets(context);

            renderableCache.RenderThreadSync();

            Monitor.Exit(rendersyncroot);

            //UpdateMarkerSelectionPanelInvoke();
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
                        timecycle.SetTime(timeofday);
                        //UpdateStatus("Timecycles loaded.");
                    }
                    if (renderskydome)
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

                renderableCache.ContentThreadProc();

                if (!(renderableCache.ItemsStillPending)) //gameFileCache.ItemsStillPending || 
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








        private void RenderSky(DeviceContext context)
        {
            if (!renderskydome) return;
            if (!weather.Inited) return;

            var shader = shaders.Skydome;
            shader.UpdateSkyLocals(weather, globalLights);




            DrawableBase skydomeydr = null;
            YddFile skydomeydd = gameFileCache.GetYdd(2640562617); //skydome hash
            if ((skydomeydd != null) && (skydomeydd.Loaded) && (skydomeydd.Dict != null))
            {
                skydomeydr = skydomeydd.Dict.Values.FirstOrDefault();
            }

            Texture starfield = null;
            //Texture moon = null;
            YtdFile skydomeytd = gameFileCache.GetYtd(2640562617); //skydome hash
            if ((skydomeytd != null) && (skydomeytd.Loaded) && (skydomeytd.TextureDict != null) && (skydomeytd.TextureDict.Dict != null))
            {
                skydomeytd.TextureDict.Dict.TryGetValue(1064311147, out starfield); //starfield hash
                //skydomeytd.TextureDict.Dict.TryGetValue(234339206, out moon); //moon-new hash
            }

            Renderable sdrnd = null;
            if (skydomeydr != null)
            {
                sdrnd = renderableCache.GetRenderable(skydomeydr);
            }

            RenderableTexture sftex = null;
            if (starfield != null)
            {
                sftex = renderableCache.GetRenderableTexture(starfield);
            }

            //RenderableTexture moontex = null;
            //if (moon != null)
            //{
            //    moontex = renderableCache.GetRenderableTexture(moon);
            //}

            if ((sdrnd != null) && (sdrnd.IsLoaded) && (sftex != null) && (sftex.IsLoaded))
            {
                shaders.SetDepthStencilMode(context, DepthStencilMode.DisableAll);
                shaders.SetRasterizerMode(context, RasterizerMode.Solid);

                RenderableInst rinst = new RenderableInst();
                rinst.Position = Vector3.Zero;
                rinst.CamRel = Vector3.Zero;
                rinst.Distance = 0.0f;
                rinst.BBMin = skydomeydr.BoundingBoxMin.XYZ();
                rinst.BBMax = skydomeydr.BoundingBoxMax.XYZ();
                rinst.BSCenter = Vector3.Zero;
                rinst.Radius = skydomeydr.BoundingSphereRadius;
                rinst.Orientation = Quaternion.Identity;
                rinst.Scale = Vector3.One;
                rinst.TintPaletteIndex = 0;
                rinst.Renderable = sdrnd;
                shader.SetShader(context);
                shader.SetInputLayout(context, VertexType.PTT);
                shader.SetSceneVars(context, camera, null, globalLights);
                shader.SetEntityVars(context, ref rinst);

                RenderableModel rmod = ((sdrnd.HDModels != null) && (sdrnd.HDModels.Length > 0)) ? sdrnd.HDModels[0] : null;
                RenderableGeometry rgeom = ((rmod != null) && (rmod.Geometries != null) && (rmod.Geometries.Length > 0)) ? rmod.Geometries[0] : null;

                if ((rgeom != null) && (rgeom.VertexType == VertexType.PTT))
                {
                    shader.SetModelVars(context, rmod);
                    shader.SetTextures(context, sftex);

                    rgeom.Render(context);
                }

                //shaders.SetRasterizerMode(context, RasterizerMode.SolidDblSided);
                //shaders.SetDepthStencilMode(context, DepthStencilMode.Enabled);
                shader.RenderSun(context, camera, weather, globalLights);


                //if ((moontex != null) && (moontex.IsLoaded))
                //{
                //    shader.RenderMoon(context, camera, weather, globalLights, moontex);
                //}



                shader.UnbindResources(context);
            }

        }

        private void RenderClouds(DeviceContext context)
        {
            if (!renderskydome) return;
            if (!weather.Inited) return;
            if (!clouds.Inited) return;


            var shader = shaders.Clouds;

            shaders.SetDepthStencilMode(context, DepthStencilMode.DisableAll);
            shaders.SetRasterizerMode(context, RasterizerMode.Solid);
            shaders.SetDefaultBlendState(context);
            //shaders.SetAlphaBlendState(context);

            shader.SetShader(context);
            shader.UpdateCloudsLocals(clouds, globalLights);
            shader.SetSceneVars(context, camera, null, globalLights);

            var vtype = (VertexType)0;

            if (!string.IsNullOrEmpty(individualcloudfrag))
            {
                //render one cloud fragment.

                CloudHatFrag frag = clouds.HatManager.FindFrag(individualcloudfrag);
                if (frag == null) return;

                for (int i = 0; i < frag.Layers.Length; i++)
                {
                    CloudHatFragLayer layer = frag.Layers[i];
                    uint dhash = JenkHash.GenHash(layer.Filename.ToLower());
                    Archetype arch = gameFileCache.GetArchetype(dhash);
                    if (arch == null)
                    { continue; }

                    if (Math.Max(camera.Position.Z, 0.0f) < layer.HeightTigger) continue;

                    var drw = TryGetDrawable(arch);
                    var rnd = TryGetRenderable(arch, drw);

                    if ((rnd == null) || (rnd.IsLoaded == false) || (rnd.AllTexturesLoaded == false))
                    { continue; }


                    RenderableInst rinst = new RenderableInst();
                    rinst.Position = frag.Position;// Vector3.Zero;
                    rinst.CamRel = Vector3.Zero;// - camera.Position;
                    rinst.Distance = rinst.CamRel.Length();
                    rinst.BBMin = arch.BBMin;
                    rinst.BBMax = arch.BBMax;
                    rinst.BSCenter = frag.Position;
                    rinst.Radius = arch.BSRadius;
                    rinst.Orientation = Quaternion.Identity;
                    rinst.Scale = frag.Scale;// Vector3.One;
                    rinst.TintPaletteIndex = 0;
                    rinst.Renderable = rnd;

                    shader.SetEntityVars(context, ref rinst);


                    for (int mi = 0; mi < rnd.HDModels.Length; mi++)
                    {
                        var model = rnd.HDModels[mi];

                        for (int gi = 0; gi < model.Geometries.Length; gi++)
                        {
                            var geom = model.Geometries[gi];

                            if (geom.VertexType != vtype)
                            {
                                vtype = geom.VertexType;
                                shader.SetInputLayout(context, vtype);
                            }

                            shader.SetGeomVars(context, geom);

                            geom.Render(context);
                        }
                    }

                }


            }



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
                    shaders.SetDepthStencilMode(context, DepthStencilMode.Enabled);
                    shaders.Paths.RenderLines(context, gridVerts, camera, shaders.GlobalLights);
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

                    RenderDrawable(Ydr.Drawable, arch, null, -camera.Position, hash);
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

                            RenderDrawable(kvp.Value, arch, null, -camera.Position, Ydd.RpfFileEntry.ShortNameHash);
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

                            RenderDrawable(kvp.Value, arch, null, -camera.Position, kvp.Key);
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

                        RenderDrawable(f.Drawable, arch, null, -camera.Position, hash);

                        if (f.Unknown_F8h_Data != null) //cloth
                        {
                            RenderDrawable(f.Unknown_F8h_Data, arch, null, -camera.Position, hash);
                        }
                        //vehicle wheels...
                        if ((f.PhysicsLODGroup != null) && (f.PhysicsLODGroup.PhysicsLOD1 != null))
                        {
                            var pl1 = f.PhysicsLODGroup.PhysicsLOD1;
                            if ((pl1.Children != null) && (pl1.Children.data_items != null))
                            {
                                for (int i = 0; i < pl1.Children.data_items.Length; i++)
                                {
                                    var pch = pl1.Children.data_items[i];
                                    if ((pch.Drawable1 != null) && (pch.Drawable1.AllModels.Length != 0))
                                    {
                                        //RenderDrawable(pch.Drawable1, null, null, -camera.Position, hash);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (Ybn != null)
            {
                if (Ybn.Loaded)
                {
                    RenderCollisionMesh(Ybn.Bounds, null);
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
                    RenderNavmesh(Ynv);
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

        private void UpdateTimeOfDay(float elapsed)
        {
            if (timerunning)
            {
                float helapsed = elapsed * timespeed / 60.0f;
                timeofday += helapsed;
                while (timeofday >= 24.0f) timeofday -= 24.0f;
                while (timeofday < 0.0f) timeofday += 24.0f;
                timecycle.SetTime(timeofday);
            }

        }

        private void UpdateGlobalLights()
        {
            Vector3 lightdir = Vector3.Zero;//will be updated before each frame from X and Y vars
            Color4 lightdircolour = Color4.White;
            Color4 lightdirambcolour = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            Color4 lightnaturalupcolour = new Color4(0.0f);
            Color4 lightnaturaldowncolour = new Color4(0.0f);
            Color4 lightartificialupcolour = new Color4(0.0f);
            Color4 lightartificialdowncolour = new Color4(0.0f);
            bool hdr = shaders.hdr;
            float hdrint = 1.0f;
            Vector3 sundir = Vector3.Up;
            Vector3 moondir = Vector3.Down;
            Vector3 moonax = Vector3.UnitZ;

            if (controllightdir || !timecycle.Inited)
            {
                float cryd = (float)Math.Cos(lightdiry);
                lightdir.X = -(float)Math.Sin(-lightdirx) * cryd;
                lightdir.Y = -(float)Math.Cos(-lightdirx) * cryd;
                lightdir.Z = (float)Math.Sin(lightdiry);
                lightdircolour = Color4.White;
                lightdirambcolour = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
                if (hdr && weather.Inited)
                {
                    lightdircolour *= weather.CurrentValues.skyHdr;
                    lightdircolour.Alpha = 1.0f;
                    lightdirambcolour *= weather.CurrentValues.skyHdr;
                    lightdirambcolour.Alpha = 1.0f;
                    hdrint = weather.CurrentValues.skyHdr;
                }
                sundir = lightdir;
                moondir = -lightdir;
                moonax = Vector3.Normalize(Vector3.UnitY);
            }
            else
            {
                float sunroll = timecycle.sun_roll * (float)Math.PI / 180.0f;  //122
                float moonroll = timecycle.moon_roll * (float)Math.PI / 180.0f;  //-122
                float moonwobamp = timecycle.moon_wobble_amp; //0.2
                float moonwobfreq = timecycle.moon_wobble_freq; //2
                float moonwoboffs = timecycle.moon_wobble_offset; //0.375
                float dayval = (0.5f + (timeofday - 6.0f) / 14.0f);
                float nightval = (((timeofday > 12.0f) ? (timeofday - 7.0f) : (timeofday + 17.0f)) / 9.0f);
                float daycyc = (float)Math.PI * dayval;
                float nightcyc = (float)Math.PI * nightval;
                Vector3 sdir = new Vector3((float)Math.Sin(daycyc), -(float)Math.Cos(daycyc), 0.0f);
                Vector3 mdir = new Vector3(-(float)Math.Sin(nightcyc), 0.0f, -(float)Math.Cos(nightcyc));
                Quaternion saxis = Quaternion.RotationYawPitchRoll(0.0f, sunroll, 0.0f);
                Quaternion maxis = Quaternion.RotationYawPitchRoll(0.0f, -moonroll, 0.0f);
                sundir = Vector3.Normalize(saxis.Multiply(sdir));
                moondir = Vector3.Normalize(maxis.Multiply(mdir));
                moonax = Vector3.Normalize(maxis.Multiply(Vector3.UnitY));
                //bool usemoon = false;

                lightdir = sundir;

                //if (lightdir.Z < -0.5f) lightdir.Z = -lightdir.Z; //make sure the lightsource is always above the horizon...

                if ((timeofday < 5.0f) || (timeofday > 21.0f))
                {
                    lightdir = moondir;
                    //usemoon = true;
                }

                if (lightdir.Z < 0)
                {
                    lightdir.Z = 0; //don't let the light source go below the horizon...
                }

                //lightdir = Vector3.Normalize(weather.CurrentValues.sunDirection);

                if (weather != null && weather.Inited)
                {
                    lightdircolour = (Color4)weather.CurrentValues.lightDirCol;
                    lightdirambcolour = (Color4)weather.CurrentValues.lightDirAmbCol;
                    lightnaturalupcolour = (Color4)weather.CurrentValues.lightNaturalAmbUp;
                    lightnaturaldowncolour = (Color4)weather.CurrentValues.lightNaturalAmbDown;
                    lightartificialupcolour = (Color4)weather.CurrentValues.lightArtificialExtUp;
                    lightartificialdowncolour = (Color4)weather.CurrentValues.lightArtificialExtDown;
                    float lamult = weather.CurrentValues.lightDirAmbIntensityMult;
                    float abounce = weather.CurrentValues.lightDirAmbBounce;
                    float minmult = hdr ? 0.1f : 0.5f;
                    lightdircolour *= Math.Max(lightdircolour.Alpha, minmult);
                    lightdirambcolour *= lightdirambcolour.Alpha * lamult; // 0.1f * lamult;

                    //if (usemoon)
                    //{
                    //    lightdircolour *= weather.CurrentValues.skyMoonIten;
                    //}


                    lightnaturalupcolour *= lightnaturalupcolour.Alpha * weather.CurrentValues.lightNaturalAmbUpIntensityMult;
                    lightnaturaldowncolour *= lightnaturaldowncolour.Alpha;
                    lightartificialupcolour *= lightartificialupcolour.Alpha;
                    lightartificialdowncolour *= lightartificialdowncolour.Alpha;

                    if (!hdr)
                    {
                        Color4 maxdirc = new Color4(1.0f);
                        Color4 maxambc = new Color4(0.5f);
                        lightdircolour = Color4.Min(lightdircolour, maxdirc);
                        lightdirambcolour = Color4.Min(lightdirambcolour, maxambc);
                        lightnaturalupcolour = Color4.Min(lightnaturalupcolour, maxambc);
                        lightnaturaldowncolour = Color4.Min(lightnaturaldowncolour, maxambc);
                        lightartificialupcolour = Color4.Min(lightartificialupcolour, maxambc);
                        lightartificialdowncolour = Color4.Min(lightartificialdowncolour, maxambc);
                    }
                    else
                    {
                        hdrint = weather.CurrentValues.skyHdr;//.lightDirCol.W;
                    }
                }


            }

            globalLights.Weather = weather;
            globalLights.HdrEnabled = hdr;
            globalLights.SpecularEnabled = true;// !MapViewEnabled;//disable specular for map view.
            globalLights.HdrIntensity = Math.Max(hdrint, 1.0f);
            globalLights.CurrentSunDir = sundir;
            globalLights.CurrentMoonDir = moondir;
            globalLights.MoonAxis = moonax;
            globalLights.Params.LightDir = lightdir;
            globalLights.Params.LightDirColour = lightdircolour;
            globalLights.Params.LightDirAmbColour = lightdirambcolour;
            globalLights.Params.LightNaturalAmbUp = rendernaturalambientlight ? lightnaturalupcolour : Color4.Black;
            globalLights.Params.LightNaturalAmbDown = rendernaturalambientlight ? lightnaturaldowncolour : Color4.Black;
            globalLights.Params.LightArtificialAmbUp = renderartificialambientlight ? lightartificialupcolour : Color4.Black;
            globalLights.Params.LightArtificialAmbDown = renderartificialambientlight ? lightartificialdowncolour : Color4.Black;


            if (shaders != null)
            {
                shaders.SetGlobalLightParams(globalLights);
            }

        }

        private void UpdateWindVector(float elapsed)
        {
            //wind still needs a lot of work.
            //currently just feed the wind vector with small oscillations...
            currentWindTime += elapsed;
            if (currentWindTime >= 200.0f) currentWindTime -= 200.0f;

            float dirval = (float)(currentWindTime * 0.01 * Math.PI);
            float dirval1 = (float)Math.Sin(currentWindTime * 0.100 * Math.PI) * 0.3f;
            float dirval2 = (float)(currentWindTime * 0.333 * Math.PI);
            float dirval3 = (float)(currentWindTime * 0.5 * Math.PI);
            float dirval4 = (float)Math.Sin(currentWindTime * 0.223 * Math.PI) * 0.4f;
            float dirval5 = (float)Math.Sin(currentWindTime * 0.4 * Math.PI) * 5.5f;

            currentWindVec.Z = (float)Math.Sin(dirval) * dirval1 + (float)Math.Cos(dirval2) * dirval4;
            currentWindVec.W = (float)Math.Cos(dirval) * dirval5 + (float)Math.Sin(dirval3) * dirval4;

            float strval = (float)(currentWindTime * 0.1 * Math.PI);
            float strval2 = (float)(currentWindTime * 0.825 * Math.PI);
            float strval3 = (float)(currentWindTime * 0.333 * Math.PI);
            float strval4 = (float)(currentWindTime * 0.666 * Math.PI);
            float strbase = 0.1f * ((float)Math.Sin(strval * 0.5));
            float strbase2 = 0.02f * ((float)Math.Sin(strval2 * 0.1));

            currentWindVec.X = (float)Math.Sin(strval) * strbase + ((float)Math.Cos(strval3) * strbase2);
            currentWindVec.Y = (float)Math.Cos(strval2) * strbase + ((float)Math.Sin(strval4 - strval3) * strbase2);
        }



        private void UpdateControlInputs(float elapsed)
        {
            var s = Settings.Default;

            float moveSpeed = 10.0f;

            bool xbenable = (xbcontroller != null) && (xbcontroller.IsConnected);
            float lx = 0, ly = 0, rx = 0, ry = 0, lt = 0, rt = 0; //input axes
            if (xbenable)
            {
                xbcontrollerstateprev = xbcontrollerstate;
                xbcontrollerstate = xbcontroller.GetState();
                xbmainaxesprev = xbmainaxes;
                xbtrigsprev = xbtrigs;
                xbmainaxes = ControllerMainAxes();
                xbtrigs = ControllerTriggers();
                lx = xbmainaxes.X;
                ly = xbmainaxes.Y;
                rx = xbmainaxes.Z;
                ry = xbmainaxes.W;
                lt = xbtrigs.X;
                rt = xbtrigs.Y;
                float lamt = s.XInputLThumbSensitivity * elapsed;
                float ramt = s.XInputRThumbSensitivity * elapsed;
                ly = s.XInputLThumbInvert ? ly : -ly;
                ry = s.XInputRThumbInvert ? ry : -ry;
                lx *= lamt;
                ly *= lamt;
                rx *= ramt;
                ry *= ramt;

                //if (ControllerButtonJustPressed(GamepadButtonFlags.Start))
                //{
                //    SetControlMode(ControlMode == WorldControlMode.Free ? WorldControlMode.Ped : WorldControlMode.Free);
                //}
            }



                Vector3 movevec = Vector3.Zero;

                //if (MapViewEnabled == true)
                //{
                //    if (kbmovefwd) movevec.Y += 1.0f;
                //    if (kbmovebck) movevec.Y -= 1.0f;
                //    if (kbmovelft) movevec.X -= 1.0f;
                //    if (kbmovergt) movevec.X += 1.0f;
                //    if (kbmoveup) movevec.Y += 1.0f;
                //    if (kbmovedn) movevec.Y -= 1.0f;
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
                    if (kbmovefwd) movevec.Z -= 1.0f;
                    if (kbmovebck) movevec.Z += 1.0f;
                    if (kbmovelft) movevec.X -= 1.0f;
                    if (kbmovergt) movevec.X += 1.0f;
                    if (kbmoveup) movevec.Y += 1.0f;
                    if (kbmovedn) movevec.Y -= 1.0f;
                    movevec *= elapsed * moveSpeed * Math.Min(camera.TargetDistance, 50.0f);
                }


                Vector3 movewvec = camera.ViewInvQuaternion.Multiply(movevec);
                camEntity.Position += movewvec;

                //MapViewDragX = 0;
                //MapViewDragY = 0;




                if (xbenable)
                {
                    camera.ControllerRotate(lx + rx, ly + ry);

                    float zoom = 0.0f;
                    float zoomspd = s.XInputZoomSpeed;
                    float zoomamt = zoomspd * elapsed;
                    if (ControllerButtonPressed(GamepadButtonFlags.DPadUp)) zoom += zoomamt;
                    if (ControllerButtonPressed(GamepadButtonFlags.DPadDown)) zoom -= zoomamt;

                    camera.ControllerZoom(zoom);

                    float acc = 0.0f;
                    float accspd = s.XInputMoveSpeed;//actually accel speed...
                    acc += rt * accspd;
                    acc -= lt * accspd;

                    Vector3 newdir = camera.ViewDirection; //maybe use the "vehicle" direction...?
                    xbcontrolvelocity += (acc * elapsed);

                    if (ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder)) //handbrake...
                    {
                        xbcontrolvelocity *= Math.Max(0.75f - elapsed, 0);//not ideal for low fps...
                                                                          //xbcontrolvelocity = 0.0f;
                        if (Math.Abs(xbcontrolvelocity) < 0.001f) xbcontrolvelocity = 0.0f;
                    }

                    camEntity.Velocity = newdir * xbcontrolvelocity;
                    camEntity.Position += camEntity.Velocity * elapsed;


                    //fire!
                    //if (ControllerButtonJustPressed(GamepadButtonFlags.LeftShoulder))
                    //{
                    //    SpawnTestEntity(true);
                    //}

                }



        }

        private Vector4 ControllerMainAxes()
        {
            var gp = xbcontrollerstate.Gamepad;
            var ldz = Gamepad.LeftThumbDeadZone;
            var rdz = Gamepad.RightThumbDeadZone;
            float ltnrng = -(short.MinValue + ldz);
            float ltprng = (short.MaxValue - ldz);
            float rtnrng = -(short.MinValue + rdz);
            float rtprng = (short.MaxValue - rdz);

            float lx = (gp.LeftThumbX < 0) ? Math.Min((gp.LeftThumbX + ldz) / ltnrng, 0) :
                       (gp.LeftThumbX > 0) ? Math.Max((gp.LeftThumbX - ldz) / ltprng, 0) : 0;
            float ly = (gp.LeftThumbY < 0) ? Math.Min((gp.LeftThumbY + ldz) / ltnrng, 0) :
                       (gp.LeftThumbY > 0) ? Math.Max((gp.LeftThumbY - ldz) / ltprng, 0) : 0;
            float rx = (gp.RightThumbX < 0) ? Math.Min((gp.RightThumbX + rdz) / rtnrng, 0) :
                       (gp.RightThumbX > 0) ? Math.Max((gp.RightThumbX - rdz) / rtprng, 0) : 0;
            float ry = (gp.RightThumbY < 0) ? Math.Min((gp.RightThumbY + rdz) / rtnrng, 0) :
                       (gp.RightThumbY > 0) ? Math.Max((gp.RightThumbY - rdz) / rtprng, 0) : 0;

            return new Vector4(lx, ly, rx, ry);
        }
        private Vector2 ControllerTriggers()
        {
            var gp = xbcontrollerstate.Gamepad;
            var tt = Gamepad.TriggerThreshold;
            float trng = byte.MaxValue - tt;
            float lt = Math.Max((gp.LeftTrigger - tt) / trng, 0);
            float rt = Math.Max((gp.RightTrigger - tt) / trng, 0);
            return new Vector2(lt, rt);
        }
        private bool ControllerButtonPressed(GamepadButtonFlags b)
        {
            return ((xbcontrollerstate.Gamepad.Buttons & b) != 0);
        }
        private bool ControllerButtonJustPressed(GamepadButtonFlags b)
        {
            return (((xbcontrollerstate.Gamepad.Buttons & b) != 0) && ((xbcontrollerstateprev.Gamepad.Buttons & b) == 0));
        }




        private void UpdateModelsUI(DrawableBase drawable)
        {
            DetailsPropertyGrid.SelectedObject = drawable;

            DrawableDrawFlags.Clear();
            ModelDrawFlags.Clear();
            GeometryDrawFlags.Clear();
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
            ModelDrawFlags.Clear();
            GeometryDrawFlags.Clear();
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
            ModelDrawFlags.Clear();
            GeometryDrawFlags.Clear();
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
                    ModelDrawFlags[model] = false;
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
            lock (rendersyncroot)
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
                        if (ModelDrawFlags.ContainsKey(model))
                        {
                            ModelDrawFlags.Remove(model);
                        }
                    }
                    else
                    {
                        ModelDrawFlags[model] = false;
                    }
                }
                if (geom != null)
                {
                    if (rem)
                    {
                        if (GeometryDrawFlags.ContainsKey(geom))
                        {
                            GeometryDrawFlags.Remove(geom);
                        }
                    }
                    else
                    {
                        GeometryDrawFlags[geom] = false;
                    }
                }
                updateArchetypeStatus = true;
            }
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

        private DrawableBase TryGetDrawable(Archetype arche)
        {
            if (arche == null) return null;
            if ((gameFileCache == null) || (!gameFileCache.IsInited)) return null;

            uint drawhash = arche.Hash;
            DrawableBase drawable = null;
            if ((arche.DrawableDict != 0))// && (arche.DrawableDict != arche.Hash))
            {
                //try get drawable from ydd...
                YddFile ydd = gameFileCache.GetYdd(arche.DrawableDict);
                if (ydd != null)
                {
                    if (ydd.Loaded && (ydd.Dict != null))
                    {
                        Drawable d;
                        ydd.Dict.TryGetValue(drawhash, out d); //can't out to base class?
                        drawable = d;
                        if (drawable == null)
                        {
                            return null; //drawable wasn't in dict!!
                        }
                    }
                    else
                    {
                        return null; //ydd not loaded yet, or has no dict
                    }
                }
                else
                {
                    //return null; //couldn't find drawable dict... quit now?
                }
            }
            if (drawable == null)
            {
                //try get drawable from ydr.
                YdrFile ydr = gameFileCache.GetYdr(drawhash);
                if (ydr != null)
                {
                    if (ydr.Loaded)
                    {
                        drawable = ydr.Drawable;
                    }
                }
                else
                {
                    YftFile yft = gameFileCache.GetYft(drawhash);
                    if (yft != null)
                    {
                        if (yft.Loaded)
                        {
                            if (yft.Fragment != null)
                            {
                                drawable = yft.Fragment.Drawable;
                            }
                        }
                    }
                }
            }

            return drawable;
        }

        private Renderable TryGetRenderable(Archetype arche, DrawableBase drawable, uint txdHash = 0)
        {
            if (drawable == null) return null;
            //BUG: only last texdict used!! needs to cache textures per archetype........
            //(but is it possible to have the same drawable with different archetypes?)
            uint texDict = (arche != null) ? arche.TextureDict.Hash : txdHash;
            uint clipDict = (arche != null) ? arche.ClipDict.Hash : 0;
            var yptTextDict = Ypt?.PtfxList?.TextureDictionary;

            Renderable rndbl = renderableCache.GetRenderable(drawable);
            if (rndbl == null) return null;


            var gfc = gameFileCache;
            if ((gfc != null) && (!gfc.IsInited))
            {
                gfc = null;
            }


            //if (clipDict != 0)
            //{
            //    YcdFile ycd = gameFileCache.GetYcd(clipDict);
            //    if ((ycd != null) && (ycd.Loaded))
            //    {
            //        ClipMapEntry cme;
            //        if (ycd.ClipMap.TryGetValue(arche.Hash, out cme))
            //        {
            //        }
            //        else
            //        { }
            //    }
            //}


            bool alltexsloaded = true;
            int missingtexcount = 0;
            for (int mi = 0; mi < rndbl.HDModels.Length; mi++)
            {
                var model = rndbl.HDModels[mi];

                //if (!RenderIsModelFinalRender(model) && !renderproxies)
                //{
                //    continue; //filter out reflection proxy models...
                //}


                foreach (var geom in model.Geometries)
                {
                    if (geom.Textures != null)
                    {
                        for (int i = 0; i < geom.Textures.Length; i++)
                        {
                            var tex = geom.Textures[i];
                            var ttex = tex as Texture;
                            RenderableTexture rdtex = null;
                            if ((ttex == null) && (tex != null))
                            {
                                //TextureRef means this RenderableTexture needs to be loaded from texture dict...
                                if (yptTextDict != null) //for ypt files, first try the embedded tex dict..
                                {
                                    var dtex = yptTextDict.Lookup(tex.NameHash);
                                    rdtex = renderableCache.GetRenderableTexture(dtex);
                                }
                                else if (texDict != 0)
                                {
                                    YtdFile ytd = gfc?.GetYtd(texDict);
                                    if ((ytd != null) && (ytd.Loaded) && (ytd.TextureDict != null))
                                    {
                                        var dtex = ytd.TextureDict.Lookup(tex.NameHash);
                                        if (dtex == null)
                                        {
                                            //not present in dictionary... check already loaded texture dicts...
                                            YtdFile ytd2 = gfc?.TryGetTextureDictForTexture(tex.NameHash);
                                            if ((ytd2 != null) && (ytd2.Loaded) && (ytd2.TextureDict != null))
                                            {
                                                dtex = ytd2.TextureDict.Lookup(tex.NameHash);
                                            }
                                            else
                                            {
                                                //couldn't find texture dict?
                                                //first try going through ytd hierarchy...
                                                dtex = gfc?.TryFindTextureInParent(tex.NameHash, texDict);


                                                //if (dtex == null)
                                                //{ //try for a texture dict with the same hash as the archetype?
                                                //    dtex = gameFileCache.TryFindTextureInParent(tex.TextureRef.NameHash, arche.Hash);
                                                //    if (dtex != null)
                                                //    { }
                                                //}
                                            }
                                        }
                                        if (dtex != null)
                                        {
                                            geom.Textures[i] = dtex; //cache it for next time to avoid the lookup...
                                            rdtex = renderableCache.GetRenderableTexture(dtex);
                                        }
                                        if (rdtex == null)
                                        { } //nothing to see here :(
                                    }
                                    else if ((ytd == null))
                                    {
                                        Texture dtex = null;
                                        if (drawable.ShaderGroup.TextureDictionary != null)
                                        {
                                            dtex = drawable.ShaderGroup.TextureDictionary.Lookup(tex.NameHash);
                                            if (dtex == null)
                                            {
                                                //dtex = drawable.ShaderGroup.TextureDictionary.Textures.data_items[0];
                                            }
                                        }
                                        if (dtex == null)
                                        {
                                            YtdFile ytd2 = gfc?.TryGetTextureDictForTexture(tex.NameHash);
                                            if ((ytd2 != null) && (ytd2.Loaded) && (ytd2.TextureDict != null))
                                            {
                                                dtex = ytd2.TextureDict.Lookup(tex.NameHash);
                                            }
                                            if (dtex == null)
                                            {
                                                dtex = gfc?.TryFindTextureInParent(tex.NameHash, texDict);
                                            }
                                        }
                                        rdtex = renderableCache.GetRenderableTexture(dtex);
                                        if (rdtex == null)
                                        { missingtexcount -= 2; } //(give extra chance..)  couldn't find the texture! :(
                                    }
                                    else if (ytd != null)
                                    {
                                        alltexsloaded = false;//ytd not loaded yet
                                        //missingtexcount++;
                                    }
                                }
                                else //no texdict specified, nothing to see here..
                                {
                                    YtdFile ytd2 = gfc?.TryGetTextureDictForTexture(tex.NameHash);
                                    if ((ytd2 != null) && (ytd2.Loaded) && (ytd2.TextureDict != null))
                                    {
                                        var dtex = ytd2.TextureDict.Lookup(tex.NameHash);
                                        rdtex = renderableCache.GetRenderableTexture(dtex);
                                    }
                                }
                            }
                            else if (ttex != null) //ensure embedded renderable texture
                            {
                                rdtex = renderableCache.GetRenderableTexture(ttex);
                            }
                            else if (tex == null)
                            { } //tex wasn't loaded? shouldn't happen..


                            geom.RenderableTextures[i] = rdtex;
                            if (rdtex != null)
                            {
                                if (!rdtex.IsLoaded)
                                {
                                    alltexsloaded = false;
                                    missingtexcount++;
                                }
                            }
                            else
                            {
                                //alltexsloaded = false;
                                missingtexcount++;
                            }


                        }
                    }
                }
            }

            rndbl.AllTexturesLoaded = alltexsloaded || (missingtexcount < 2);

            return rndbl;
        }



        private bool RenderDrawable(DrawableBase drawable, Archetype arche, YmapEntityDef entity, Vector3 camrel, uint txdHash = 0)
        {
            //enqueue a single drawable for rendering.

            if (drawable == null)
                return false;

            Renderable rndbl = TryGetRenderable(arche, drawable, txdHash);
            if (rndbl == null)
                return false;

            return RenderRenderable(rndbl, arche, entity, camrel);
        }


        private bool RenderRenderable(Renderable rndbl, Archetype arche, YmapEntityDef entity, Vector3 camrel)
        {
            //enqueue a single renderable for rendering.

            if (!rndbl.IsLoaded) return false;


            //if (((SelectionMode == MapSelectionMode.Entity) || (SelectionMode == MapSelectionMode.EntityExtension) || (SelectionMode == MapSelectionMode.ArchetypeExtension)))
            //{
            //    UpdateMouseHit(rndbl, arche, entity, camrel);
            //}

            bool isselected = true;// (rndbl.Key == SelectedItem.Drawable);

            Vector3 position = Vector3.Zero;
            Vector3 scale = Vector3.One;
            Quaternion orientation = Quaternion.Identity;
            uint tintPaletteIndex = 0;
            Vector3 bbmin = (arche != null) ? arche.BBMin : rndbl.Key.BoundingBoxMin.XYZ();
            Vector3 bbmax = (arche != null) ? arche.BBMax : rndbl.Key.BoundingBoxMax.XYZ();
            Vector3 bscen = (arche != null) ? arche.BSCenter : rndbl.Key.BoundingCenter;
            float radius = (arche != null) ? arche.BSRadius : rndbl.Key.BoundingSphereRadius;
            float distance = (camrel + bscen).Length();
            if (entity != null)
            {
                position = entity.Position;
                scale = entity.Scale;
                orientation = entity.Orientation;
                tintPaletteIndex = entity.CEntityDef.tintValue;
                bbmin = entity.BBMin;
                bbmax = entity.BBMax;
                bscen = entity.BSCenter;
            }


            if (rendercollisionmeshes)// && collisionmeshlayerdrawable)
            {
                Drawable sdrawable = rndbl.Key as Drawable;
                if ((sdrawable != null) && (sdrawable.Bound != null))
                {
                    RenderCollisionMesh(sdrawable.Bound, entity);
                }
            }


            bool retval = true;// false;
            if (rndbl.IsLoaded && (rndbl.AllTexturesLoaded || !waitforchildrentoload))
            {
                RenderableGeometryInst rginst = new RenderableGeometryInst();
                rginst.Inst.Renderable = rndbl;
                rginst.Inst.CamRel = camrel;
                rginst.Inst.Position = position;
                rginst.Inst.Scale = scale;
                rginst.Inst.Orientation = orientation;
                rginst.Inst.TintPaletteIndex = tintPaletteIndex;
                rginst.Inst.BBMin = bbmin;
                rginst.Inst.BBMax = bbmax;
                rginst.Inst.BSCenter = bscen;
                rginst.Inst.Radius = radius;
                rginst.Inst.Distance = distance;


                RenderableModel[] models = isselected ? rndbl.AllModels : rndbl.HDModels;

                for (int mi = 0; mi < models.Length; mi++)
                {
                    var model = models[mi];

                    if (isselected)
                    {
                        if (ModelDrawFlags.ContainsKey(model.DrawableModel))
                        { continue; } //filter out models in selected item that aren't flagged for drawing.
                    }

                    if (!RenderIsModelFinalRender(model))// && !renderproxies)
                    { continue; } //filter out reflection proxy models...

                    for (int gi = 0; gi < model.Geometries.Length; gi++)
                    {
                        var geom = model.Geometries[gi];

                        if (isselected)
                        {
                            if (GeometryDrawFlags.ContainsKey(geom.DrawableGeom))
                            { continue; } //filter out geometries in selected item that aren't flagged for drawing.
                        }

                        rginst.Geom = geom;

                        shaders.Enqueue(rginst);
                    }
                }
            }
            else
            {
                retval = false;
            }
            return retval;
        }


        private void RenderCollisionMesh(Bounds bounds, YmapEntityDef entity)
        {
            //enqueue a single collision mesh for rendering.

            Vector3 position;
            Vector3 scale;
            Quaternion orientation;
            if (entity != null)
            {
                position = entity.Position;
                scale = entity.Scale;
                orientation = entity.Orientation;
            }
            else
            {
                position = Vector3.Zero;
                scale = Vector3.One;
                orientation = Quaternion.Identity;
            }

            switch (bounds.Type)
            {
                case 10: //BoundComposite
                    BoundComposite boundcomp = bounds as BoundComposite;
                    if (boundcomp != null)
                    {
                        RenderableBoundComposite rndbc = renderableCache.GetRenderableBoundComp(boundcomp);
                        if (rndbc.IsLoaded)
                        {
                            RenderableBoundGeometryInst rbginst = new RenderableBoundGeometryInst();
                            rbginst.Inst.Renderable = rndbc;
                            rbginst.Inst.Orientation = orientation;
                            rbginst.Inst.Scale = scale;
                            foreach (var geom in rndbc.Geometries)
                            {
                                if (geom == null) continue;
                                rbginst.Geom = geom;
                                rbginst.Inst.Position = position + orientation.Multiply(geom.BoundGeom.CenterGeom * scale);
                                rbginst.Inst.CamRel = rbginst.Inst.Position - camera.Position;
                                shaders.Enqueue(rbginst);
                            }

                            //UpdateMouseHits(rndbc, entity);
                        }
                    }
                    else
                    { }
                    break;
                case 3: //BoundBox - found in drawables - TODO
                    BoundBox boundbox = bounds as BoundBox;
                    if (boundbox == null)
                    { }
                    break;
                case 0: //BoundSphere - found in drawables - TODO
                    BoundSphere boundsphere = bounds as BoundSphere;
                    if (boundsphere == null)
                    { }
                    break;
                default:
                    break;
            }
        }


        private void RenderNavmesh(YnvFile ynv)
        {
            RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(ynv);
            if ((rnd != null) && (rnd.IsLoaded))
            {
                shaders.Enqueue(rnd);
            }
        }



        private bool RenderIsModelFinalRender(RenderableModel model)
        {

            if ((model.Unk2Ch & 1) == 0) //smallest bit is proxy/"final render" bit? seems to work...
            {
                return false;// renderproxies;
            }
            return true;

            //switch (model.Unk2Ch)
            //{
            //    case 65784:  //0000010000000011111000  //reflection proxy?
            //    case 65788:  //0000010000000011111100
            //    case 131312: //0000100000000011110000  //reflection proxy?
            //    case 131320: //0000100000000011111000  //reflection proxy?
            //    case 131324: //0000100000000011111100  //shadow/reflection proxy?
            //    case 196834: //0000110000000011100010 //shadow proxy? (tree branches)
            //    case 196848: //0000110000000011110000  //reflection proxy?
            //    case 196856: //0000110000000011111000 //reflection proxy? hotel nr golf course
            //    case 262392: //0001000000000011111000  //reflection proxy?
            //    case 327932: //0001010000000011111100  //reflection proxy? (alamo/sandy shores)
            //    case 983268: //0011110000000011100100  //big reflection proxy?
            //    case 2293988://1000110000000011100100  //big reflection proxy?
            //                 //case 1442047://golf course water proxy, but other things also
            //                 //case 1114367://mike house water proxy, but other things also
            //        return renderproxies;
            //}
            //return true;
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

            if (MouseLButtonDown)
            {
                camera.MouseRotate(dx, dy);
            }
            if (MouseRButtonDown)
            {
                if (controllightdir)
                {
                    lightdirx += (dx * camera.Sensitivity);
                    lightdiry += (dy * camera.Sensitivity);
                }
                else if (controltimeofday)
                {
                    timeofday += (dx - dy) / 30.0f;
                    while (timeofday >= 24.0f) timeofday -= 24.0f;
                    while (timeofday < 0.0f) timeofday += 24.0f;
                    timecycle.SetTime(timeofday);

                    float fv = timeofday * 60.0f;
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

        private void StatsUpdateTimer_Tick(object sender, EventArgs e)
        {
            int rgc = (shaders != null) ? shaders.RenderedGeometries : 0;
            int crc = renderableCache.LoadedRenderableCount;
            int ctc = renderableCache.LoadedTextureCount;
            int tcrc = renderableCache.MemCachedRenderableCount;
            int tctc = renderableCache.MemCachedTextureCount;
            long vr = renderableCache.TotalGraphicsMemoryUse + (shaders != null ? shaders.TotalGraphicsMemoryUse : 0);
            string vram = TextUtil.GetBytesReadable(vr);
            //StatsLabel.Text = string.Format("Drawn: {0} geom, Loaded: {1}/{5} dr, {2}/{6} tx, Vram: {3}, Fps: {4}", rgc, crc, ctc, vram, fps, tcrc, tctc);
            StatsLabel.Text = string.Format("Drawn: {0} geom, Loaded: {1} dr, {2} tx, Vram: {3}, Fps: {4}", rgc, crc, ctc, vram, fps);

            if (timerunning)
            {
                float fv = timeofday * 60.0f;
                //TimeOfDayTrackBar.Value = (int)fv;
                UpdateTimeOfDayLabel();
            }

            //CameraPositionTextBox.Text = FloatUtil.GetVector3String(camera.Position, "0.##");
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

            var k = e.KeyCode;

            bool ctrl = (e.Modifiers & Keys.Control) > 0;
            bool shift = (e.Modifiers & Keys.Shift) > 0;
            CtrlPressed = ctrl;
            ShiftPressed = shift;

            bool enablemove = true;// (!iseditmode) || (MouseLButtonDown && (GrabbedMarker == null) && (GrabbedWidget == null));

            enablemove = enablemove && (!ctrl);

            //WASD move the camera entity...
            if (enablemove)
            {
                if (k == keyBindings.MoveForward) kbmovefwd = true;
                if (k == keyBindings.MoveBackward) kbmovebck = true;
                if (k == keyBindings.MoveLeft) kbmovelft = true;
                if (k == keyBindings.MoveRight) kbmovergt = true;
                if (k == keyBindings.MoveUp) kbmoveup = true;
                if (k == keyBindings.MoveDown) kbmovedn = true;
                if (k == keyBindings.Jump) kbjump = true;
            }

            bool moving = kbmovefwd || kbmovebck || kbmovelft || kbmovergt || kbmoveup || kbmovedn || kbjump;


            if (!ctrl)
            {
                if (k == keyBindings.MoveSlowerZoomIn)
                {
                    camera.MouseZoom(1);
                }
                if (k == keyBindings.MoveFasterZoomOut)
                {
                    camera.MouseZoom(-1);
                }
            }



            if (!moving) //don't trigger further actions if moving.
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
            bool ctrl = (e.Modifiers & Keys.Control) > 0;
            bool shift = (e.Modifiers & Keys.Shift) > 0;
            CtrlPressed = ctrl;
            ShiftPressed = shift;

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


            var k = e.KeyCode;
            if (k == keyBindings.MoveForward) kbmovefwd = false;
            if (k == keyBindings.MoveBackward) kbmovebck = false;
            if (k == keyBindings.MoveLeft) kbmovelft = false;
            if (k == keyBindings.MoveRight) kbmovergt = false;
            if (k == keyBindings.MoveUp) kbmoveup = false;
            if (k == keyBindings.MoveDown) kbmovedn = false;
            if (k == keyBindings.Jump) kbjump = false;


            //if (ControlMode != WorldControlMode.Free)
            //{
            //    e.Handled = true;
            //}
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
            lock (rendersyncroot)
            {
                shaders.hdr = HDRRenderingCheckBox.Checked;
            }
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (rendersyncroot)
            {
                shaders.shadows = ShadowsCheckBox.Checked;
            }
        }

        private void SkydomeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderskydome = SkydomeCheckBox.Checked;
            //controllightdir = !renderskydome;
        }

        private void ControlLightDirCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controllightdir = ControlLightDirCheckBox.Checked;
        }

        private void TimeOfDayTrackBar_Scroll(object sender, EventArgs e)
        {
            int v = TimeOfDayTrackBar.Value;
            float fh = v / 60.0f;
            UpdateTimeOfDayLabel();
            lock (rendersyncroot)
            {
                timeofday = fh;
                timecycle.SetTime(timeofday);
            }
        }

        private void ShowBoundsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendercollisionmeshes = ShowBoundsCheckBox.Checked;
        }

        private void WireframeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            shaders.wireframe = WireframeCheckBox.Checked;
        }

        private void AnisotropicFilteringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            shaders.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
        }

        private void RenderModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextureSamplerComboBox.Enabled = false;
            TextureCoordsComboBox.Enabled = false;
            switch (RenderModeComboBox.Text)
            {
                default:
                case "Default":
                    shaders.RenderMode = WorldRenderMode.Default;
                    break;
                case "Single texture":
                    shaders.RenderMode = WorldRenderMode.SingleTexture;
                    TextureSamplerComboBox.Enabled = true;
                    TextureCoordsComboBox.Enabled = true;
                    break;
                case "Vertex normals":
                    shaders.RenderMode = WorldRenderMode.VertexNormals;
                    break;
                case "Vertex tangents":
                    shaders.RenderMode = WorldRenderMode.VertexTangents;
                    break;
                case "Vertex colour 1":
                    shaders.RenderMode = WorldRenderMode.VertexColour;
                    shaders.RenderVertexColourIndex = 1;
                    break;
                case "Vertex colour 2":
                    shaders.RenderMode = WorldRenderMode.VertexColour;
                    shaders.RenderVertexColourIndex = 2;
                    break;
                case "Vertex colour 3":
                    shaders.RenderMode = WorldRenderMode.VertexColour;
                    shaders.RenderVertexColourIndex = 3;
                    break;
                case "Texture coord 1":
                    shaders.RenderMode = WorldRenderMode.TextureCoord;
                    shaders.RenderTextureCoordIndex = 1;
                    break;
                case "Texture coord 2":
                    shaders.RenderMode = WorldRenderMode.TextureCoord;
                    shaders.RenderTextureCoordIndex = 2;
                    break;
                case "Texture coord 3":
                    shaders.RenderMode = WorldRenderMode.TextureCoord;
                    shaders.RenderTextureCoordIndex = 3;
                    break;
            }
        }

        private void TextureSamplerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextureSamplerComboBox.SelectedItem is MetaName)
            {
                shaders.RenderTextureSampler = (MetaName)TextureSamplerComboBox.SelectedItem;
            }
        }

        private void TextureCoordsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TextureCoordsComboBox.Text)
            {
                default:
                case "Texture coord 1":
                    shaders.RenderTextureSamplerCoord = 1;
                    break;
                case "Texture coord 2":
                    shaders.RenderTextureSamplerCoord = 2;
                    break;
                case "Texture coord 3":
                    shaders.RenderTextureSamplerCoord = 3;
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
