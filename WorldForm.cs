using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using SharpDX;
using SharpDX.XInput;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using CodeWalker.World;
using CodeWalker.Project;
using CodeWalker.Rendering;
using CodeWalker.GameFiles;
using CodeWalker.Properties;

namespace CodeWalker
{
    public partial class WorldForm : Form, DXForm
    {
        public Form Form { get { return this; } } //for DXForm/DXManager use

        private Renderer Renderer = null;
        public object RenderSyncRoot { get { return Renderer.RenderSyncRoot; } }

        volatile bool formopen = false;
        volatile bool running = false;
        volatile bool pauserendering = false;
        volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Space space = new Space();
        Camera camera;
        Timecycle timecycle;
        Weather weather;
        Clouds clouds;
        Water water = new Water();
        Trains trains = new Trains();
        Scenarios scenarios = new Scenarios();
        PopZones popzones = new PopZones();
        AudioZones audiozones = new AudioZones();


        bool MouseLButtonDown = false;
        bool MouseRButtonDown = false;
        int MouseX;
        int MouseY;
        System.Drawing.Point MouseDownPoint;
        System.Drawing.Point MouseLastPoint;

        bool rendermaps = false;
        bool renderworld = false;
        int startupviewmode = 0; //0=world, 1=ymap, 2=model
        string modelname = "dt1_tc_dufo_core";//"dt1_11_fount_decal";//
        string[] ymaplist;

        Vector3 prevworldpos = new Vector3(0, 0, 100); //also the start pos


        public GameFileCache GameFileCache { get { return gameFileCache; } }
        GameFileCache gameFileCache = GameFileCacheFactory.Create();


        WorldControlMode ControlMode = WorldControlMode.Free;

        object MouseControlSyncRoot = new object();
        int MouseControlX = 0;
        int MouseControlY = 0;
        int MouseControlWheel = 0;
        MouseButtons MouseControlButtons = MouseButtons.None;
        MouseButtons MouseControlButtonsPrev = MouseButtons.None;
        bool MouseInvert = Settings.Default.MouseInvert;

        bool ControlFireToggle = false;

        Entity camEntity = new Entity();
        PedEntity pedEntity = new PedEntity();


        bool iseditmode = false;


        List<MapIcon> Icons;
        MapIcon MarkerIcon = null;
        MapIcon LocatorIcon = null;
        MapMarker LocatorMarker = null;
        MapMarker GrabbedMarker = null;
        MapMarker SelectedMarker = null;
        MapMarker MousedMarker = null;
        List<MapMarker> Markers = new List<MapMarker>();
        List<MapMarker> SortedMarkers = new List<MapMarker>();
        List<MapMarker> MarkerBatch = new List<MapMarker>();
        bool RenderLocator = false;
        object markersyncroot = new object();
        object markersortedsyncroot = new object();







        bool rendercollisionmeshes = Settings.Default.ShowCollisionMeshes;
        List<BoundsStoreItem> collisionitems = new List<BoundsStoreItem>();
        int collisionmeshrange = Settings.Default.CollisionMeshRange;
        bool[] collisionmeshlayers = { true, true, true };

        Dictionary<MetaHash, YmapFile> renderworldVisibleYmapDict = new Dictionary<MetaHash, YmapFile>();

        bool worldymaptimefilter = true;
        bool worldymapweatherfilter = true;

        bool renderpathbounds = true;
        bool renderpaths = false;
        List<YndFile> renderpathynds = new List<YndFile>();

        bool renderwaterquads = true;
        List<WaterQuad> renderwaterquadlist = new List<WaterQuad>();

        bool rendertraintracks = false;
        List<TrainTrack> rendertraintracklist = new List<TrainTrack>();

        bool rendernavmeshes = false;
        List<YnvFile> rendernavmeshynvs = new List<YnvFile>();

        bool renderscenariobounds = false;
        bool renderscenarios = false;
        List<YmtFile> renderscenariolist = new List<YmtFile>();

        bool renderpopzones = false;

        bool renderaudiozones = false;
        bool renderaudioouterbounds = true;


        bool MapViewEnabled = false;
        int MapViewDragX = 0;
        int MapViewDragY = 0;


        bool MouseSelectEnabled = false;
        bool ShowSelectionBounds = true;
        bool SelectByGeometry = false; //select by geometry needs more work 
        MapSelection CurMouseHit = new MapSelection();
        MapSelection LastMouseHit = new MapSelection();
        MapSelection PrevMouseHit = new MapSelection();

        bool MouseRayCollisionEnabled = true;
        bool MouseRayCollisionVisible = true;
        SpaceRayIntersectResult MouseRayCollision = new SpaceRayIntersectResult();

        string SelectionModeStr = "Entity";
        MapSelectionMode SelectionMode = MapSelectionMode.Entity;
        MapSelection SelectedItem;
        List<MapSelection> SelectedItems = new List<MapSelection>();
        WorldInfoForm InfoForm = null;


        TransformWidget Widget = new TransformWidget();
        TransformWidget GrabbedWidget = null;
        bool ShowWidget = true;


        ProjectForm ProjectForm = null;

        Stack<UndoStep> UndoSteps = new Stack<UndoStep>();
        Stack<UndoStep> RedoSteps = new Stack<UndoStep>();
        Vector3 UndoStartPosition;
        Quaternion UndoStartRotation;
        Vector3 UndoStartScale;

        WorldSnapMode SnapMode = WorldSnapMode.None;
        WorldSnapMode SnapModePrev = WorldSnapMode.Ground;//also the default snap mode
        float SnapGridSize = 1.0f;

        YmapEntityDef CopiedEntity = null;
        YmapCarGen CopiedCarGen = null;
        YndNode CopiedPathNode = null;
        YnvPoly CopiedNavPoly = null;
        TrainTrackNode CopiedTrainNode = null;
        ScenarioNode CopiedScenarioNode = null;

        public bool EditEntityPivot { get; set; } = false;

        SettingsForm SettingsForm = null;

        WorldSearchForm SearchForm = null;


        InputManager Input = new InputManager();



        bool toolspanelexpanded = false;
        int toolspanellastwidth;
        bool toolsPanelResizing = false;
        int toolsPanelResizeStartX = 0;
        int toolsPanelResizeStartLeft = 0;
        int toolsPanelResizeStartRight = 0;

        bool initedOk = false;


        public WorldForm()
        {
            InitializeComponent();

            Renderer = new Renderer(this, gameFileCache);
            camera = Renderer.camera;
            timecycle = Renderer.timecycle;
            weather = Renderer.weather;
            clouds = Renderer.clouds;

            initedOk = Renderer.Init();
        }


        private void Init()
        {
            //called from WorldForm_Load

            if (!initedOk)
            {
                Close();
                return;
            }


            MouseWheel += WorldForm_MouseWheel;

            if (!GTAFolder.UpdateGTAFolder(true))
            {
                Close();
                return;
            }

            Widget.Position = new Vector3(1.0f, 10.0f, 100.0f);
            Widget.Rotation = Quaternion.Identity;
            Widget.Scale = Vector3.One;
            Widget.Visible = false;
            Widget.OnPositionChange += Widget_OnPositionChange;
            Widget.OnRotationChange += Widget_OnRotationChange;
            Widget.OnScaleChange += Widget_OnScaleChange;

            ymaplist = YmapsTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            ViewModeComboBox.SelectedIndex = startupviewmode;
            BoundsStyleComboBox.SelectedIndex = 0; //LoadSettings will handle this

            SelectionModeComboBox.SelectedIndex = 0; //Entity mode
            ShowSelectedExtensionTab(false);

            toolspanellastwidth = ToolsPanel.Width * 2; //default expanded size


            Icons = new List<MapIcon>();
            AddIcon("Google Marker", "icon_google_marker_64x64.png", 64, 64, 11.0f, 40.0f, 1.0f);
            AddIcon("Glokon Marker", "icon_glokon_normal_32x32.png", 32, 32, 11.0f, 32.0f, 1.0f);
            AddIcon("Glokon Debug", "icon_glokon_debug_32x32.png", 32, 32, 11.5f, 32.0f, 1.0f);
            MarkerIcon = Icons[1];
            LocatorIcon = Icons[2];
            foreach (MapIcon icon in Icons)
            {
                MarkerStyleComboBox.Items.Add(icon);
                LocatorStyleComboBox.Items.Add(icon);
            }
            MarkerStyleComboBox.SelectedItem = MarkerIcon; //LoadSettings will handle this
            LocatorStyleComboBox.SelectedItem = LocatorIcon;
            LocatorMarker = new MapMarker();
            LocatorMarker.Icon = LocatorIcon;
            LocatorMarker.IsMovable = true;
            //AddDefaultMarkers(); //some POI to start with

            MetaName[] texsamplers = RenderableGeometry.GetTextureSamplerList();
            foreach (var texsampler in texsamplers)
            {
                TextureSamplerComboBox.Items.Add(texsampler);
            }
            //TextureSamplerComboBox.SelectedIndex = 0; //LoadSettings will handle this
            //RenderModeComboBox.SelectedIndex = 0; //Default

            WorldMaxLodComboBox.SelectedIndex = 0;//should this be a setting?

            WeatherComboBox.SelectedIndex = 0;//show "<Loading...>" until weather types are loaded

            CameraModeComboBox.SelectedIndex = 0; //"Perspective"

            DlcLevelComboBox.SelectedIndex = 0; //show "<Loading...>" until DLC list is loaded

            UpdateToolbarShortcutsText();


            Input.Init();


            Renderer.Start();
        }



        private MapIcon AddIcon(string name, string filename, int texw, int texh, float centerx, float centery, float scale)
        {
            string filepath = "icons\\" + filename;
            try
            {
                MapIcon mi = new MapIcon(name, filepath, texw, texh, centerx, centery, scale);
                Icons.Add(mi);
                return mi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load map icon " + filepath + " for " + name + "!\n\n" + ex.ToString());
            }
            return null;
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

            if (Icons != null)
            {
                foreach (MapIcon icon in Icons)
                {
                    icon.LoadTexture(device, LogError);
                }
            }

            camera.FollowEntity = camEntity;
            camEntity.Position = (startupviewmode!=2) ? prevworldpos : Vector3.Zero;
            camEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);

            space.AddPersistentEntity(pedEntity);


            LoadSettings();


            formopen = true;
            new Thread(new ThreadStart(ContentThread)).Start();

            frametimer.Start();
        }
        public void CleanupScene()
        {
            formopen = false;

            Renderer.DeviceDestroyed();

            if (Icons != null)
            {
                foreach (MapIcon icon in Icons)
                {
                    icon.UnloadTexture();
                }
            }

            int count = 0;
            while (running && (count < 5000)) //wait for the content thread to exit gracefully
            {
                Thread.Sleep(1);
                count++;
            }
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

            space.Update(elapsed);



            Renderer.Update(elapsed, MouseLastPoint.X, MouseLastPoint.Y);



            UpdateWidgets();

            BeginMouseHitTest();




            Renderer.BeginRender(context);

            Renderer.RenderSkyAndClouds();

            Renderer.SelectedDrawable = SelectedItem.Drawable;

            if (renderworld)
            {
                RenderWorld();
            }
            else if (rendermaps)
            {
                RenderYmaps();
            }
            else
            {
                RenderSingleItem();
            }

            UpdateMouseHitsFromRenderer();

            RenderSelection();

            Renderer.RenderQueued();

            Renderer.RenderBounds(SelectionMode);

            Renderer.RenderSelectionGeometry(SelectionMode);

            RenderMoused();

            Renderer.RenderFinalPass();

            RenderMarkers();

            RenderWidgets();

            Renderer.EndRender();

            Monitor.Exit(Renderer.RenderSyncRoot);

            UpdateMarkerSelectionPanelInvoke();
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

            float moveSpeed = 50.0f;


            Input.Update(elapsed);

            if (Input.xbenable)
            {
                if (Input.ControllerButtonJustPressed(GamepadButtonFlags.Start))
                {
                    SetControlMode(ControlMode == WorldControlMode.Free ? WorldControlMode.Ped : WorldControlMode.Free);
                }
            }


            if (ControlMode == WorldControlMode.Free)
            {
                if (Input.ShiftPressed)
                {
                    moveSpeed *= 5.0f;
                }
                if (Input.CtrlPressed)
                {
                    moveSpeed *= 0.2f;
                }

                Vector3 movevec = Input.KeyboardMoveVec(MapViewEnabled);

                if (MapViewEnabled)
                {
                    movevec *= elapsed * moveSpeed * Math.Min(camera.OrthographicTargetSize * 0.01f, 50.0f);

                    float mapviewscale = 1.0f / camera.Height;
                    float fdx = MapViewDragX * mapviewscale;
                    float fdy = MapViewDragY * mapviewscale;
                    movevec.X -= fdx * camera.OrthographicSize;
                    movevec.Y += fdy * camera.OrthographicSize;

                }
                else
                {
                    //normal movement
                    movevec *= elapsed * moveSpeed * Math.Min(camera.TargetDistance, 20.0f);
                }


                Vector3 movewvec = camera.ViewInvQuaternion.Multiply(movevec);
                camEntity.Position += movewvec;

                MapViewDragX = 0;
                MapViewDragY = 0;




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
                        //Input.xbcontrolvelocity = 0.0f;
                        if (Math.Abs(Input.xbcontrolvelocity) < 0.001f) Input.xbcontrolvelocity = 0.0f;
                    }

                    camEntity.Velocity = newdir * Input.xbcontrolvelocity;
                    camEntity.Position += camEntity.Velocity * elapsed;


                    //fire!
                    if (Input.ControllerButtonJustPressed(GamepadButtonFlags.LeftShoulder))
                    {
                        SpawnTestEntity(true);
                    }

                }

            }
            else
            {
                //"play" mode

                int mcx, mcy, mcw;
                MouseButtons mcb, mcbp;
                bool mlb = false, mrb = false;
                bool mlbjustpressed = false, mrbjustpressed = false;
                lock (MouseControlSyncRoot)
                {
                    mcx = MouseControlX;
                    mcy = MouseControlY;
                    mcw = MouseControlWheel;
                    mcb = MouseControlButtons;
                    mcbp = MouseControlButtonsPrev;
                    mlb = ((mcb & MouseButtons.Left) > 0);
                    mrb = ((mcb & MouseButtons.Right) > 0);
                    mlbjustpressed = mlb && ((mcbp & MouseButtons.Left) == 0);
                    mrbjustpressed = mrb && ((mcbp & MouseButtons.Right) == 0);
                    MouseControlX = 0;
                    MouseControlY = 0;
                    MouseControlWheel = 0;
                    MouseControlButtonsPrev = MouseControlButtons;
                    //MouseControlButtons = MouseButtons.None;
                }


                camera.MouseRotate(mcx, mcy);

                if (Input.xbenable)
                {
                    camera.ControllerRotate(Input.xbrx, Input.xbry);
                }



                Vector2 movecontrol = new Vector2(Input.xbmainaxes.X, Input.xbmainaxes.Y); //(L stick)
                if (Input.kbmovelft) movecontrol.X -= 1.0f;
                if (Input.kbmovergt) movecontrol.X += 1.0f;
                if (Input.kbmovefwd) movecontrol.Y += 1.0f;
                if (Input.kbmovebck) movecontrol.Y -= 1.0f;
                movecontrol.X = Math.Min(movecontrol.X, 1.0f);
                movecontrol.X = Math.Max(movecontrol.X, -1.0f);
                movecontrol.Y = Math.Min(movecontrol.Y, 1.0f);
                movecontrol.Y = Math.Max(movecontrol.Y, -1.0f);

                Vector3 fwd = camera.ViewDirection;
                Vector3 fwdxy = Vector3.Normalize(new Vector3(fwd.X, fwd.Y, 0));
                Vector3 lftxy = Vector3.Normalize(Vector3.Cross(fwd, Vector3.UnitZ));
                Vector3 move = lftxy * movecontrol.X + fwdxy * movecontrol.Y;
                Vector2 movexy = new Vector2(move.X, move.Y);


                pedEntity.ControlMovement = movexy;
                pedEntity.ControlJump = Input.kbjump || Input.ControllerButtonPressed(GamepadButtonFlags.X);
                pedEntity.ControlBoost = Input.ShiftPressed || Input.ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.LeftShoulder);


                //Vector3 pedfwd = pedEntity.Orientation.Multiply(Vector3.UnitZ);






                bool fire = mlb || (Input.xbtrigs.Y > 0);
                if (fire && !ControlFireToggle)
                {
                    SpawnTestEntity(true);
                }
                ControlFireToggle = fire;


            }


        }




        private void RenderWorld()
        {
            //start point for world view mode rendering.
            //also used for the water, paths, collisions, nav mesh, and the project window items.

            renderworldVisibleYmapDict.Clear();


            int hour = worldymaptimefilter ? (int)Renderer.timeofday : -1;
            MetaHash weathertype = worldymapweatherfilter ? ((weather.CurrentWeatherType != null) ? weather.CurrentWeatherType.NameHash : new MetaHash(0)) : new MetaHash(0);

            IEnumerable<Entity> spaceEnts = null;

            if (renderworld)
            {
                space.GetVisibleYmaps(camera, hour, weathertype, renderworldVisibleYmapDict);

                spaceEnts = space.TemporaryEntities;
            }

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleYmaps(camera, renderworldVisibleYmapDict);
            }


            Renderer.RenderWorld(renderworldVisibleYmapDict, spaceEnts);


            foreach (var ymap in Renderer.VisibleYmaps)
            {
                UpdateMouseHits(ymap);
            }



            if (renderwaterquads || (SelectionMode == MapSelectionMode.WaterQuad))
            {
                RenderWorldWaterQuads();
            }
            if (rendercollisionmeshes || (SelectionMode == MapSelectionMode.Collision))
            {
                RenderWorldCollisionMeshes();
            }
            if (renderpaths || (SelectionMode == MapSelectionMode.Path))
            {
                RenderWorldPaths();
            }
            if (rendertraintracks || (SelectionMode == MapSelectionMode.TrainTrack))
            {
                RenderWorldTrainTracks();
            }
            if (rendernavmeshes || (SelectionMode == MapSelectionMode.NavMesh))
            {
                RenderWorldNavMeshes();
            }
            if (renderscenarios || (SelectionMode == MapSelectionMode.Scenario))
            {
                RenderWorldScenarios();
            }
            if (renderpopzones || (SelectionMode == MapSelectionMode.PopZone))
            {
                RenderWorldPopZones();
            }
            if (renderaudiozones || (SelectionMode == MapSelectionMode.Audio))
            {
                RenderWorldAudioZones();
            }

        }

        private void RenderWorldCollisionMeshes()
        {
            //enqueue collision meshes for rendering - from the world grid

            collisionitems.Clear();
            space.GetVisibleBounds(camera, collisionmeshrange, collisionmeshlayers, collisionitems);

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleCollisionMeshes(camera, collisionitems);
            }

            foreach (var item in collisionitems)
            {
                YbnFile ybn = gameFileCache.GetYbn(item.Name);
                if ((ybn != null) && (ybn.Loaded))
                {
                    Renderer.RenderCollisionMesh(ybn.Bounds, null);
                }
            }

        }

        private void RenderWorldWaterQuads()
        {
            renderwaterquadlist.Clear();

            water.GetVisibleQuads(camera, renderwaterquadlist);

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleWaterQuads(camera, renderwaterquadlist);
            }

            Renderer.RenderWaterQuads(renderwaterquadlist);

            UpdateMouseHits(renderwaterquadlist);

        }

        private void RenderWorldPaths()
        {
            renderpathynds.Clear();

            space.GetVisibleYnds(camera, renderpathynds);

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleYnds(camera, renderpathynds);
            }

            Renderer.RenderPaths(renderpathynds);

            UpdateMouseHits(renderpathynds);
        }

        private void RenderWorldTrainTracks()
        {
            if (!trains.Inited) return;

            rendertraintracklist.Clear();
            rendertraintracklist.AddRange(trains.TrainTracks);

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleTrainTracks(camera, rendertraintracklist);
            }

            Renderer.RenderTrainTracks(rendertraintracklist);

            UpdateMouseHits(rendertraintracklist);
        }

        private void RenderWorldNavMeshes()
        {

            rendernavmeshynvs.Clear();
            space.GetVisibleYnvs(camera, rendernavmeshynvs);

            if (ProjectForm != null)
            {
                //ProjectForm.GetVisibleYnvs(camera, rendernavmeshynvs);
            }

            Renderer.RenderNavMeshes(rendernavmeshynvs);

            UpdateMouseHits(rendernavmeshynvs);


        }

        private void RenderWorldScenarios()
        {
            if (!scenarios.Inited) return;

            renderscenariolist.Clear();
            renderscenariolist.AddRange(scenarios.ScenarioRegions);

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleScenarios(camera, renderscenariolist);
            }

            Renderer.RenderScenarios(renderscenariolist);

            UpdateMouseHits(renderscenariolist);
        }

        private void RenderWorldPopZones()
        {
            if (!popzones.Inited) return;

            //renderpopzonelist.Clear();
            //renderpopzonelist.AddRange(popzones.Groups.Values);

            if (ProjectForm != null)
            {
                //ProjectForm.GetVisiblePopZones(camera, renderpopzonelist);
            }

            Renderer.RenderPopZones(popzones);
        }

        private void RenderWorldAudioZones()
        {
            if (!audiozones.Inited) return;

            //renderaudzonelist.Clear();
            //renderaudzonelist.AddRange(audzones.Zones.Values);

            if (ProjectForm != null)
            {
                //ProjectForm.GetVisibleAudioZones(camera, renderaudzonelist);
            }


            //RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(audiozones);
            //if ((rnd != null) && (rnd.IsLoaded))
            //{
            //    shaders.Enqueue(rnd);
            //}


            BoundingBox bbox = new BoundingBox();
            BoundingSphere bsph = new BoundingSphere();
            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;
            float hitdist = float.MaxValue;

            MapBox lastHitOuterBox = new MapBox();
            MapSphere lastHitOuterSphere = new MapSphere();
            MapBox mb = new MapBox();
            MapSphere ms = new MapSphere();

            for (int i = 0; i < audiozones.AllItems.Count; i++)
            {
                var placement = audiozones.AllItems[i];
                switch (placement.Shape)
                {
                    case Dat151ZoneShape.Box:
                    case Dat151ZoneShape.Line:

                        mb.CamRelPos = placement.InnerPos - camera.Position;
                        mb.BBMin = placement.InnerMin;
                        mb.BBMax = placement.InnerMax;
                        mb.Orientation = placement.InnerOri;
                        mb.Scale = Vector3.One;
                        Renderer.HilightBoxes.Add(mb);

                        if (renderaudioouterbounds)
                        {
                            mb.CamRelPos = placement.OuterPos - camera.Position;
                            mb.BBMin = placement.OuterMin;
                            mb.BBMax = placement.OuterMax;
                            mb.Orientation = placement.OuterOri;
                            mb.Scale = Vector3.One;
                            Renderer.BoundingBoxes.Add(mb);
                        }

                        Vector3 hbcamrel = (placement.Position - camera.Position);
                        Ray mraytrn = new Ray();
                        mraytrn.Position = placement.OrientationInv.Multiply(camera.MouseRay.Position - hbcamrel);
                        mraytrn.Direction = placement.OrientationInv.Multiply(mray.Direction);
                        bbox.Minimum = placement.HitboxMin;
                        bbox.Maximum = placement.HitboxMax;
                        if (mraytrn.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                        {
                            CurMouseHit.Audio = placement;
                            CurMouseHit.HitDist = hitdist;
                            CurMouseHit.CamRel = hbcamrel;
                            CurMouseHit.AABB = bbox;
                            lastHitOuterBox = mb; //highlight the outer box
                        }
                        break;
                    case Dat151ZoneShape.Sphere:

                        if ((placement.InnerPos != Vector3.Zero) && (placement.OuterPos != Vector3.Zero))
                        {
                            ms.CamRelPos = placement.InnerPos - camera.Position;
                            ms.Radius = placement.InnerRad;
                            Renderer.HilightSpheres.Add(ms);

                            if (renderaudioouterbounds)
                            {
                                ms.CamRelPos = placement.OuterPos - camera.Position;
                                ms.Radius = placement.OuterRad;
                                Renderer.BoundingSpheres.Add(ms);
                            }

                            bsph.Center = placement.Position;
                            bsph.Radius = placement.HitSphereRad;
                            if (mray.Intersects(ref bsph, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                            {
                                CurMouseHit.Audio = placement;
                                CurMouseHit.HitDist = hitdist;
                                CurMouseHit.CamRel = placement.Position - camera.Position;
                                CurMouseHit.AABB = new BoundingBox(); //no box here
                                CurMouseHit.BSphere = bsph;
                                lastHitOuterSphere = ms; //highlight the outer sphere
                            }
                        }
                        else
                        { }


                        break;
                    default:
                        break;//shouldn't get here
                }
            }

            if (CurMouseHit.Audio != null)
            {
                //hilight the outer bounds of moused item
                switch (CurMouseHit.Audio.Shape)
                {
                    case Dat151ZoneShape.Box:
                    case Dat151ZoneShape.Line:
                        Renderer.HilightBoxes.Add(lastHitOuterBox);
                        break;
                    case Dat151ZoneShape.Sphere:
                        Renderer.HilightSpheres.Add(lastHitOuterSphere);
                        break;
                }
            }


        }



        private void RenderSingleItem()
        {
            //start point for model view mode rendering

            uint hash = 0;// JenkHash.GenHash(modelname);
            if (!uint.TryParse(modelname, out hash)) //try use a hash directly
            {
                hash = JenkHash.GenHash(modelname);
            }
            Archetype arche = gameFileCache.GetArchetype(hash);

            Archetype selarch = null;
            DrawableBase seldrwbl = null;
            YmapEntityDef selent = null;

            if (arche != null)
            {
                Renderer.RenderArchetype(arche, null);

                selarch = arche;
            }
            else
            {
                YmapFile ymap = gameFileCache.GetYmap(hash);
                if (ymap != null)
                {
                    Renderer.RenderYmap(ymap);
                }
                else
                {
                    //not a ymap... see if it's a ydr or yft
                    YdrFile ydr = gameFileCache.GetYdr(hash);
                    if (ydr != null)
                    {
                        if (ydr.Loaded)
                        {
                            Renderer.RenderDrawable(ydr.Drawable, null, null, hash);

                            seldrwbl = ydr.Drawable;
                        }
                    }
                    else
                    {
                        YftFile yft = gameFileCache.GetYft(hash);
                        if (yft != null)
                        {
                            if (yft.Loaded)
                            {
                                if (yft.Fragment != null)
                                {
                                    var f = yft.Fragment;

                                    Renderer.RenderFragment(null, null, f, hash);

                                    seldrwbl = f.Drawable;
                                }
                            }
                        }
                        else
                        {
                            //TODO: collision bounds single model...
                            //YbnFile ybn = gameFileCache.GetYbn(hash);
                        }
                    }

                }
            }

            if ((selarch != null) && (seldrwbl == null))
            {
                seldrwbl = gameFileCache.TryGetDrawable(selarch);
            }

            //select this item for viewing by the UI...
            if ((SelectedItem.Archetype != selarch) || (SelectedItem.Drawable != seldrwbl) || (SelectedItem.EntityDef != selent))
            {
                SelectedItem.Clear();
                SelectedItem.Archetype = selarch;
                SelectedItem.Drawable = seldrwbl;
                SelectedItem.EntityDef = selent;
                UpdateSelectionUI(false);
            }

        }


        private void RenderYmaps()
        {
            //start point for ymap view mode rendering

            foreach (string lod in ymaplist)
            {
                uint hash = JenkHash.GenHash(lod);
                YmapFile ymap = gameFileCache.GetYmap(hash);
                Renderer.RenderYmap(ymap);

                UpdateMouseHits(ymap);
            }
        }






        private void RenderMoused()
        {
            //immediately render the bounding box of the currently moused entity.

            if (!MouseSelectEnabled)
            { return; }

            PrevMouseHit = LastMouseHit;
            LastMouseHit = CurMouseHit;

            bool change = (LastMouseHit.EntityDef != PrevMouseHit.EntityDef);
            if (SelectByGeometry)
            {
                change = change || (LastMouseHit.Geometry != PrevMouseHit.Geometry);
            }
            switch (SelectionMode)
            {
                case MapSelectionMode.EntityExtension:
                    change = change || (LastMouseHit.EntityExtension != PrevMouseHit.EntityExtension);
                    break;
                case MapSelectionMode.ArchetypeExtension:
                    change = change || (LastMouseHit.ArchetypeExtension != PrevMouseHit.ArchetypeExtension);
                    break;
                case MapSelectionMode.TimeCycleModifier:
                    change = change || (LastMouseHit.TimeCycleModifier != PrevMouseHit.TimeCycleModifier);
                    break;
                case MapSelectionMode.CarGenerator:
                    change = change || (LastMouseHit.CarGenerator != PrevMouseHit.CarGenerator);
                    break;
                case MapSelectionMode.MloInstance:
                    change = change || (LastMouseHit.MloEntityDef != PrevMouseHit.MloEntityDef);
                    break;
                case MapSelectionMode.DistantLodLights:
                    change = change || (LastMouseHit.DistantLodLights != PrevMouseHit.DistantLodLights);
                    break;
                case MapSelectionMode.Grass:
                    change = change || (LastMouseHit.GrassBatch != PrevMouseHit.GrassBatch);
                    break;
                case MapSelectionMode.WaterQuad:
                    change = change || (LastMouseHit.WaterQuad != PrevMouseHit.WaterQuad);
                    break;
                case MapSelectionMode.Collision:
                    change = change || (LastMouseHit.CollisionBounds != PrevMouseHit.CollisionBounds);
                    break;
                case MapSelectionMode.NavMesh:
                    change = change || (LastMouseHit.NavPoly != PrevMouseHit.NavPoly);
                    break;
                case MapSelectionMode.Path:
                    change = change || (LastMouseHit.PathNode != PrevMouseHit.PathNode);
                    break;
                case MapSelectionMode.TrainTrack:
                    change = change || (LastMouseHit.TrainTrackNode != PrevMouseHit.TrainTrackNode);
                    break;
                case MapSelectionMode.Scenario:
                    change = change || (LastMouseHit.ScenarioNode != PrevMouseHit.ScenarioNode);
                    break;
                case MapSelectionMode.Audio:
                    change = change || (LastMouseHit.Audio != PrevMouseHit.Audio);
                    break;
            }


            if (change)
            {
                string text = LastMouseHit.GetFullNameString(string.Empty);
                UpdateMousedLabel(text);
            }

            if(!CurMouseHit.HasHit)
            { return; }


            if (SelectionMode == MapSelectionMode.NavMesh)
            {
                return;//navmesh mode isn't needing a selection box..
            }


            bool clip = Renderer.renderboundsclip;

            BoundsShaderMode mode = BoundsShaderMode.Box;
            float bsphrad = CurMouseHit.BSphere.Radius;
            Vector3 bbmin = CurMouseHit.AABB.Minimum;
            Vector3 bbmax = CurMouseHit.AABB.Maximum;
            Vector3 camrel = CurMouseHit.CamRel;
            Vector3 scale = Vector3.One;
            Quaternion ori = Quaternion.Identity;
            bool ext = (CurMouseHit.ArchetypeExtension != null) || (CurMouseHit.EntityExtension != null) || (CurMouseHit.CollisionBounds != null);
            if (CurMouseHit.EntityDef != null)
            {
                scale = ext ? Vector3.One : CurMouseHit.EntityDef.Scale;
                ori = CurMouseHit.EntityDef.Orientation;
            }
            if (CurMouseHit.Archetype != null)
            {
                bbmin = CurMouseHit.Archetype.BBMin;
                bbmax = CurMouseHit.Archetype.BBMax;
            }
            if ((CurMouseHit.Geometry != null) || ext)
            {
                bbmin = CurMouseHit.AABB.Minimum; //override archetype AABB..
                bbmax = CurMouseHit.AABB.Maximum;
            }
            if (CurMouseHit.CarGenerator != null)
            {
                ori = CurMouseHit.CarGenerator.Orientation;
            }
            if (CurMouseHit.MloEntityDef != null)
            {
                scale = Vector3.One;
                clip = false;
            }
            if (CurMouseHit.WaterQuad != null)
            {
                clip = false;
            }
            if (CurMouseHit.ScenarioNode != null)
            {
                var sp = CurMouseHit.ScenarioNode.MyPoint;
                if (sp == null) sp = CurMouseHit.ScenarioNode.ClusterMyPoint;
                if (sp != null) //orientate the moused box for the correct scenario point direction...
                {
                    ori = sp.Orientation;
                }
            }
            if (CurMouseHit.Audio != null)
            {
                ori = CurMouseHit.Audio.Orientation;
                if (CurMouseHit.Audio.Shape == Dat151ZoneShape.Sphere)
                {
                    mode = BoundsShaderMode.Sphere;
                }
            }


            Renderer.RenderMouseHit(mode, clip, ref camrel, ref bbmin, ref bbmax, ref scale, ref ori, bsphrad);
        }

        private void RenderSelection()
        {
            if (SelectedItem.MultipleSelection)
            {
                for (int i = 0; i < SelectedItems.Count; i++)
                {
                    var item = SelectedItems[i];
                    RenderSelection(ref item);
                }
            }
            else
            {
                RenderSelection(ref SelectedItem);
            }
        }
        private void RenderSelection(ref MapSelection selectionItem)
        {
            //immediately render the bounding box of the current selection. also, arrows.

            const uint cgrn = 4278255360;// (uint)new Color4(0.0f, 1.0f, 0.0f, 1.0f).ToRgba();
            const uint cblu = 4294901760;// (uint)new Color4(0.0f, 0.0f, 1.0f, 1.0f).ToRgba();

            if (MouseRayCollisionEnabled && MouseRayCollisionVisible)
            {
                if (MouseRayCollision.Hit)
                {
                    var arup = GetPerpVec(MouseRayCollision.Normal);
                    Renderer.RenderSelectionArrowOutline(MouseRayCollision.Position, MouseRayCollision.Normal, arup, Quaternion.Identity, 2.0f, 0.15f, cgrn);
                }
            }

            if (!ShowSelectionBounds)
            { return; }

            if (!selectionItem.HasValue)
            { return; }



            BoundsShaderMode mode = BoundsShaderMode.Box;
            float bsphrad = selectionItem.BSphere.Radius;
            Vector3 bbmin = selectionItem.AABB.Minimum;
            Vector3 bbmax = selectionItem.AABB.Maximum;
            Vector3 camrel = -camera.Position;
            Vector3 scale = Vector3.One;
            Quaternion ori = Quaternion.Identity;


            var arch = selectionItem.Archetype;
            var ent = selectionItem.EntityDef;
            if (selectionItem.Archetype != null)
            {
                bbmin = selectionItem.Archetype.BBMin;
                bbmax = selectionItem.Archetype.BBMax;
            }
            if (selectionItem.EntityDef != null)
            {
                camrel = ent.Position - camera.Position;
                scale = ent.Scale;
                ori = ent.Orientation;

                if (EditEntityPivot)
                {
                    Renderer.RenderSelectionEntityPivot(ent);
                }
            }
            if (selectionItem.CarGenerator != null)
            {
                var cg = selectionItem.CarGenerator;
                camrel = cg.Position - camera.Position;
                ori = cg.Orientation;
                bbmin = cg.BBMin;
                bbmax = cg.BBMax;
                float arrowlen = cg._CCarGen.perpendicularLength;
                float arrowrad = arrowlen * 0.066f;
                Renderer.RenderSelectionArrowOutline(cg.Position, Vector3.UnitX, Vector3.UnitY, ori, arrowlen, arrowrad, cgrn);

                Quaternion cgtrn = Quaternion.RotationAxis(Vector3.UnitZ, (float)Math.PI * -0.5f); //car fragments currently need to be rotated 90 deg right...
                Quaternion cgori = Quaternion.Multiply(ori, cgtrn);

                Renderer.RenderCar(cg.Position, cgori, cg._CCarGen.carModel, cg._CCarGen.popGroup);
            }
            if (selectionItem.PathNode != null)
            {
                camrel = selectionItem.PathNode.Position - camera.Position;
            }
            if (selectionItem.TrainTrackNode != null)
            {
                camrel = selectionItem.TrainTrackNode.Position - camera.Position;
            }
            if (selectionItem.ScenarioNode != null)
            {
                camrel = selectionItem.ScenarioNode.Position - camera.Position;

                var sn = selectionItem.ScenarioNode;

                //render direction arrow for ScenarioPoint
                ori = sn.Orientation;
                float arrowlen = 2.0f;
                float arrowrad = 0.25f;
                Renderer.RenderSelectionArrowOutline(sn.Position, Vector3.UnitY, Vector3.UnitZ, ori, arrowlen, arrowrad, cgrn);

                MCScenarioPoint vpoint = sn.MyPoint ?? sn.ClusterMyPoint;
                if ((vpoint != null) && (vpoint?.Type?.IsVehicle ?? false))
                {
                    var vhash = vpoint.ModelSet?.NameHash ?? vpoint.Type?.VehicleModelSetHash ?? 0;
                    if ((vhash == 0) && (sn.ChainingNode?.Chain?.Edges != null) && (sn.ChainingNode.Chain.Edges.Length > 0))
                    {
                        var fedge = sn.ChainingNode.Chain.Edges[0]; //for chain nodes, show the first node's model...
                        var fnode = fedge?.NodeFrom?.ScenarioNode;
                        if (fnode != null)
                        {
                            vpoint = fnode.MyPoint ?? fnode.ClusterMyPoint;
                            vhash = vpoint.ModelSet?.NameHash ?? vpoint.Type?.VehicleModelSetHash ?? 0;
                        }
                    }

                    Renderer.RenderCar(sn.Position, sn.Orientation, 0, vhash, true);
                }

            }
            if (selectionItem.ScenarioEdge != null)
            {
                //render scenario edge arrow
                var se = selectionItem.ScenarioEdge;
                var sn1 = se.NodeFrom;
                var sn2 = se.NodeTo;
                if ((sn1 != null) && (sn2 != null))
                {
                    var dirp = sn2.Position - sn1.Position;
                    float dl = dirp.Length();
                    Vector3 dir = dirp * (1.0f / dl);
                    Vector3 dup = Vector3.UnitZ;
                    var aori = Quaternion.Invert(Quaternion.RotationLookAtRH(dir, dup));
                    float arrowrad = 0.25f;
                    float arrowlen = Math.Max(dl - arrowrad*5.0f, 0);
                    Renderer.RenderSelectionArrowOutline(sn1.Position, -Vector3.UnitZ, Vector3.UnitY, aori, arrowlen, arrowrad, cblu);
                }
            }
            if (selectionItem.MloEntityDef != null)
            {
                bbmin = selectionItem.AABB.Minimum;
                bbmax = selectionItem.AABB.Maximum;
            }
            if ((selectionItem.GrassBatch != null) || (selectionItem.ArchetypeExtension != null) || (selectionItem.EntityExtension != null) || (selectionItem.CollisionBounds != null))
            {
                bbmin = selectionItem.AABB.Minimum;
                bbmax = selectionItem.AABB.Maximum;
                scale = Vector3.One;
            }
            if (selectionItem.NavPoly != null)
            {
                Renderer.RenderSelectionNavPoly(selectionItem.NavPoly);
                //return;//don't render a selection box for nav mesh?
            }

            if (selectionItem.Audio != null)
            {
                var au = selectionItem.Audio;
                camrel = au.Position - camera.Position;
                ori = au.Orientation;
                bbmin = au.HitboxMin;
                bbmax = au.HitboxMax;

                if (selectionItem.Audio.Shape == Dat151ZoneShape.Sphere)
                {
                    mode = BoundsShaderMode.Sphere;
                    MapSphere wsph = new MapSphere();
                    wsph.CamRelPos = au.OuterPos - camera.Position;
                    wsph.Radius = au.OuterRad;
                    Renderer.WhiteSpheres.Add(wsph);
                }
                else
                {
                    MapBox wbox = new MapBox();
                    wbox.CamRelPos = au.OuterPos - camera.Position;
                    wbox.BBMin = au.OuterMin;
                    wbox.BBMax = au.OuterMax;
                    wbox.Orientation = au.OuterOri;
                    wbox.Scale = scale;
                    Renderer.WhiteBoxes.Add(wbox);
                }
            }


            if (mode == BoundsShaderMode.Box)
            {
                MapBox box = new MapBox();
                box.CamRelPos = camrel;
                box.BBMin = bbmin;
                box.BBMax = bbmax;
                box.Orientation = ori;
                box.Scale = scale;
                Renderer.SelectionBoxes.Add(box);
            }
            else if (mode == BoundsShaderMode.Sphere)
            {
                MapSphere sph = new MapSphere();
                sph.CamRelPos = camrel;
                sph.Radius = bsphrad;
                Renderer.SelectionSpheres.Add(sph);
            }

        }



        private void RenderMarkers()
        {
            //immediately render all the current markers.

            lock (markersyncroot) //should only cause delays if markers moved/updated
            {
                foreach (var marker in Markers)
                {
                    marker.CamRelPos = marker.WorldPos - camera.Position;
                    marker.Distance = marker.CamRelPos.Length();
                    marker.ScreenPos = camera.ViewProjMatrix.MultiplyW(marker.CamRelPos);
                }

                lock (markersortedsyncroot) //stop collisions with mouse testing
                {
                    SortedMarkers.Clear();
                    SortedMarkers.AddRange(Markers);
                    if (RenderLocator)
                    {
                        LocatorMarker.CamRelPos = LocatorMarker.WorldPos - camera.Position;
                        LocatorMarker.Distance = LocatorMarker.CamRelPos.Length();
                        LocatorMarker.ScreenPos = camera.ViewProjMatrix.MultiplyW(LocatorMarker.CamRelPos);
                        SortedMarkers.Add(LocatorMarker);
                    }
                    SortedMarkers.Sort((m1, m2) => m2.Distance.CompareTo(m1.Distance));
                }

                MarkerBatch.Clear();
                MarkerBatch.AddRange(SortedMarkers);
            }

            Renderer.RenderMarkers(MarkerBatch);
        }


        private void RenderWidgets()
        {
            if (!ShowWidget) return;

            Renderer.RenderTransformWidget(Widget);
        }
        private void UpdateWidgets()
        {
            if (!ShowWidget) return;

            Widget.Update(camera);
        }



        private Vector3 GetGroundPoint(Vector3 p)
        {
            float uplimit = 3.0f;
            float downlimit = 20.0f;
            Ray ray = new Ray(p, new Vector3(0, 0, -1.0f));
            ray.Position.Z += 0.1f;
            SpaceRayIntersectResult hit = space.RayIntersect(ray, downlimit);
            if (hit.Hit)
            {
                return hit.Position;
            }
            ray.Position.Z += uplimit;
            hit = space.RayIntersect(ray, downlimit);
            if (hit.Hit)
            {
                return hit.Position;
            }
            return p;
        }
        private Vector3 SnapPosition(Vector3 p)
        {
            Vector3 gpos = (p / SnapGridSize).Round() * SnapGridSize;
            switch (SnapMode)
            {
                case WorldSnapMode.Grid:
                    p = gpos;
                    break;
                case WorldSnapMode.Ground:
                    p = GetGroundPoint(p);
                    break;
                case WorldSnapMode.Hybrid:
                    p = GetGroundPoint(gpos);
                    break;
            }
            return p;
        }


        private void Widget_OnPositionChange(Vector3 newpos, Vector3 oldpos)
        {
            //called during UpdateWidgets()

            newpos = SnapPosition(newpos);

            if (newpos == oldpos) return;

            if (SelectedItem.MultipleSelection)
            {
                if (EditEntityPivot)
                {
                }
                else
                {
                    var dpos = newpos - SelectedItem.MultipleSelectionCenter;// oldpos;
                    if (dpos == Vector3.Zero) return; //nothing moved.. (probably due to snap)
                    for (int i = 0; i < SelectedItems.Count; i++)
                    {
                        var refpos = SelectedItems[i].WidgetPosition;
                        SelectedItems[i].SetPosition(refpos + dpos, false);
                    }
                    SelectedItem.MultipleSelectionCenter = newpos;
                }
            }
            else
            {
                SelectedItem.SetPosition(newpos, EditEntityPivot);                
            }
            if (ProjectForm != null)
            {
                ProjectForm.OnWorldSelectionModified(SelectedItem, SelectedItems);
            }
        }
        private void Widget_OnRotationChange(Quaternion newrot, Quaternion oldrot)
        {
            //called during UpdateWidgets()
            if (newrot == oldrot) return;

            if (SelectedItem.MultipleSelection)
            {
                if (EditEntityPivot)
                {
                }
                else
                {
                }
            }
            else
            {
                SelectedItem.SetRotation(newrot, oldrot, EditEntityPivot);
            }
            if (ProjectForm != null)
            {
                ProjectForm.OnWorldSelectionModified(SelectedItem, SelectedItems);
            }
        }
        private void Widget_OnScaleChange(Vector3 newscale, Vector3 oldscale)
        {
            //called during UpdateWidgets()
            if (newscale == oldscale) return;

            if (SelectedItem.MultipleSelection)
            {
                if (EditEntityPivot)
                {//editing pivot scale is sort of meaningless..
                }
                else
                {
                }
            }
            else
            {
                SelectedItem.SetScale(newscale, oldscale, EditEntityPivot);
            }
            if (ProjectForm != null)
            {
                ProjectForm.OnWorldSelectionModified(SelectedItem, SelectedItems);
            }
        }

        public void SetWidgetPosition(Vector3 pos, bool enableUndo = false)
        {
            if (enableUndo)
            {
                SetWidgetMode("Position");
                MarkUndoStart(Widget);
            }

            Widget.Position = pos;

            if (enableUndo)
            {
                MarkUndoEnd(Widget);
            }
        }
        public void SetWidgetRotation(Quaternion q, bool enableUndo = false)
        {
            if (enableUndo)
            {
                SetWidgetMode("Rotation");
                MarkUndoStart(Widget);
            }

            Widget.Rotation = q;

            if (enableUndo)
            {
                MarkUndoEnd(Widget);
            }
        }
        public void SetWidgetScale(Vector3 s, bool enableUndo = false)
        {
            if (enableUndo)
            {
                SetWidgetMode("Scale");
                MarkUndoStart(Widget);
            }

            Widget.Scale = s;

            if (enableUndo)
            {
                MarkUndoEnd(Widget);
            }
        }


        public void UpdatePathYndGraphics(YndFile ynd, bool fullupdate)
        {
            if (fullupdate)
            {
                ynd.UpdateAllNodePositions();
                ynd.BuildBVH();

                space.BuildYndData(ynd);
            }
            else
            {
                space.BuildYndVerts(ynd);
            }
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.Invalidate(ynd);
            }
        }
        public void UpdatePathNodeGraphics(YndNode pathnode, bool fullupdate)
        {
            if (pathnode == null) return;
            pathnode.Ynd.UpdateBvhForNode(pathnode);
            UpdatePathYndGraphics(pathnode.Ynd, fullupdate);
        }
        public YndNode GetPathNodeFromSpace(ushort areaid, ushort nodeid)
        {
            return space.NodeGrid.GetYndNode(areaid, nodeid);
        }

        public void UpdateNavYnvGraphics(YnvFile ynv, bool fullupdate)//TODO!
        {
        }
        public void UpdateNavPolyGraphics(YnvPoly poly, bool fullupdate)//TODO!
        {
        }

        public void UpdateTrainTrackGraphics(TrainTrack tt, bool fullupdate)
        {
            tt.BuildVertices();
            tt.BuildBVH();
            //if (fullupdate)
            //{
            //    //space.BuildYndData(ynd);
            //}
            //else
            //{
            //    //space.BuildYndVerts(ynd);
            //}
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.Invalidate(tt);
            }
        }
        public void UpdateTrainTrackNodeGraphics(TrainTrackNode node, bool fullupdate)
        {
            if (node == null) return;
            node.Track.UpdateBvhForNode(node);
            UpdateTrainTrackGraphics(node.Track, fullupdate);
        }

        public void UpdateScenarioGraphics(YmtFile ymt, bool fullupdate)
        {
            var scenario = ymt.ScenarioRegion;
            if (scenario == null) return;

            scenario.BuildBVH();

            scenario.BuildVertices();

            lock (Renderer.RenderSyncRoot)
            {
                Renderer.Invalidate(scenario);
            }
        }


        public Vector3 GetCameraPosition()
        {
            //currently used by ProjectForm when creating entities
            lock (Renderer.RenderSyncRoot)
            {
                return camera.Position;
            }
        }
        public Vector3 GetCameraViewDir()
        {
            //currently used by ProjectForm when creating entities
            lock (Renderer.RenderSyncRoot)
            {
                return camera.ViewDirection;
            }
        }

        public void SetCameraSensitivity(float sensitivity, float smoothing)
        {
            camera.Sensitivity = sensitivity;
            camera.Smoothness = smoothing;
        }
        public void SetMouseInverted(bool invert)
        {
            MouseInvert = invert;
        }

        public void SetKeyBindings(KeyBindings kb)
        {
            Input.keyBindings = kb.Copy();
            UpdateToolbarShortcutsText();
        }
        private void UpdateToolbarShortcutsText()
        {
            var kb = Input.keyBindings;
            ToolbarSelectButton.ToolTipText = string.Format("Select objects / Exit edit mode ({0}, {1})", kb.ToggleMouseSelect, kb.ExitEditMode);
            ToolbarMoveButton.ToolTipText = string.Format("Move ({0})", kb.EditPosition);
            ToolbarRotateButton.ToolTipText = string.Format("Rotate ({0})", kb.EditRotation);
            ToolbarScaleButton.ToolTipText = string.Format("Scale ({0})", kb.EditScale);
            ShowToolbarCheckBox.Text = string.Format("Show Toolbar ({0})", kb.ToggleToolbar);
        }


        private MapBox GetExtensionBox(Vector3 camrel, MetaWrapper ext)
        {
            MapBox b = new MapBox();
            Vector3 pos = Vector3.Zero;
            float size = 0.5f;
            if (ext is MCExtensionDefLightEffect)
            {
                var le = ext as MCExtensionDefLightEffect;
                pos = le.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefSpawnPointOverride)
            {
                var spo = ext as MCExtensionDefSpawnPointOverride;
                pos = spo.Data.offsetPosition;
                size = spo.Data.Radius;
            }
            else if (ext is MCExtensionDefDoor)
            {
                var door = ext as MCExtensionDefDoor;
                pos = door.Data.offsetPosition;
            }
            else if (ext is Mrage__phVerletClothCustomBounds)
            {
                var cb = ext as Mrage__phVerletClothCustomBounds;
                if ((cb.CollisionData != null) && (cb.CollisionData.Length > 0))
                {
                    pos = cb.CollisionData[0].Data.Position;
                }
            }
            else if (ext is MCExtensionDefParticleEffect)
            {
                var pe = ext as MCExtensionDefParticleEffect;
                pos = pe.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefAudioCollisionSettings)
            {
                var acs = ext as MCExtensionDefAudioCollisionSettings;
                pos = acs.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefAudioEmitter)
            {
                var ae = ext as MCExtensionDefAudioEmitter;
                pos = ae.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefSpawnPoint)
            {
                var sp = ext as MCExtensionDefSpawnPoint;
                pos = sp.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefExplosionEffect)
            {
                var ee = ext as MCExtensionDefExplosionEffect;
                pos = ee.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefLadder)
            {
                var ld = ext as MCExtensionDefLadder;
                pos = ld.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefBuoyancy)
            {
                var bu = ext as MCExtensionDefBuoyancy;
                pos = bu.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefExpression)
            {
                var exp = ext as MCExtensionDefExpression;
                pos = exp.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefLightShaft)
            {
                var ls = ext as MCExtensionDefLightShaft;
                pos = ls.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefWindDisturbance)
            {
                var wd = ext as MCExtensionDefWindDisturbance;
                pos = wd.Data.offsetPosition;
            }
            else if (ext is MCExtensionDefProcObject)
            {
                var po = ext as MCExtensionDefProcObject;
                pos = po.Data.offsetPosition;
            }


            b.BBMin = pos - size;
            b.BBMax = pos + size;
            b.CamRelPos = camrel;

            return b;
        }


        public static Vector3 GetPerpVec(Vector3 n)
        {
            //make a vector perpendicular to the given one
            float nx = Math.Abs(n.X);
            float ny = Math.Abs(n.Y);
            float nz = Math.Abs(n.Z);
            if ((nx < ny) && (nx < nz))
            {
                return Vector3.Cross(n, Vector3.Right);
            }
            else if (ny < nz)
            {
                return Vector3.Cross(n, Vector3.Up);
            }
            else
            {
                return Vector3.Cross(n, Vector3.ForwardLH);
            }
        }


        private void SpawnTestEntity(bool cameraCenter = false)
        {
            if (!space.Inited) return;

            Vector3 dir = (cameraCenter ? camera.ViewDirection : camera.MouseRay.Direction);
            Vector3 ofs = (cameraCenter ? Vector3.Zero : camera.MouseRay.Position);
            Vector3 pos = ofs + camera.Position + (dir * 1.5f);
            Vector3 vel = dir * 50.0f; //m/s

            var hash = JenkHash.GenHash("prop_alien_egg_01");
            var arch = GameFileCache.GetArchetype(hash);

            if (arch == null) return;

            CEntityDef cent = new CEntityDef();
            cent.archetypeName = hash;
            cent.rotation = new Vector4(0, 0, 0, 1);
            cent.scaleXY = 1.0f;
            cent.scaleZ = 1.0f;
            cent.flags = 1572872;
            cent.parentIndex = -1;
            cent.lodDist = 200.0f;
            cent.lodLevel = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
            cent.priorityLevel = Unk_648413703.PRI_REQUIRED;
            cent.ambientOcclusionMultiplier = 255;
            cent.artificialAmbientOcclusion = 255;
            cent.position = pos;

            YmapEntityDef ent = new YmapEntityDef(null, 0, ref cent);

            ent.SetArchetype(arch);


            Entity e = new Entity();
            e.Position = pos;
            e.Velocity = vel;
            e.Mass = 10.0f;
            e.Momentum = vel * e.Mass;
            e.EntityDef = ent;
            e.Radius = arch.BSRadius * 0.7f;
            e.EnableCollisions = true;
            e.Enabled = true;

            lock (Renderer.RenderSyncRoot)
            {
                space.AddTemporaryEntity(e);
            }
        }



        public void SetControlMode(WorldControlMode mode)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => { SetControlMode(mode); }));
                }
                catch
                { }
                return;
            }

            if (mode == ControlMode) return;

            bool wasfree = (ControlMode == WorldControlMode.Free);
            bool isfree = (mode == WorldControlMode.Free);

            if (isfree && !wasfree)
            {
                camEntity.Position = pedEntity.Position;

                pedEntity.Enabled = false;

                Renderer.timerunning = false;

                camera.SetFollowEntity(camEntity);
                camera.TargetDistance = 1.0f; //default?
                camera.Smoothness = Settings.Default.CameraSmoothing;

                Cursor.Show();
            }
            else if (!isfree && wasfree)
            {
                pedEntity.Position = camEntity.Position;
                pedEntity.Velocity = Vector3.Zero;
                pedEntity.Enabled = true;

                Renderer.timerunning = true;

                camera.SetFollowEntity(pedEntity.CameraEntity);
                camera.TargetDistance = 0.01f; //1cm
                camera.Smoothness = 20.0f;

                //center the mouse in the window
                System.Drawing.Point centerp = new System.Drawing.Point(ClientSize.Width / 2, ClientSize.Height / 2);
                MouseLastPoint = centerp;
                MouseX = centerp.X;
                MouseY = centerp.Y;
                Cursor.Position = PointToScreen(centerp);
                Cursor.Hide();
            }




            ControlMode = mode;

        }






        private void BeginMouseHitTest()
        {
            //reset variables for beginning the mouse hit test
            CurMouseHit.Clear();


            MouseRayCollisionEnabled = Input.CtrlPressed; //temporary...!
            if (MouseRayCollisionEnabled)
            {
                if (space.Inited && space.Grid != null)
                {
                    Ray mray = new Ray();
                    mray.Position = camera.MouseRay.Position + camera.Position;
                    mray.Direction = camera.MouseRay.Direction;
                    MouseRayCollision = space.RayIntersect(mray);
                }
            }


            Renderer.RenderedDrawablesListEnable =
                ((SelectionMode == MapSelectionMode.Entity) && MouseSelectEnabled) ||
                (SelectionMode == MapSelectionMode.EntityExtension) ||
                (SelectionMode == MapSelectionMode.ArchetypeExtension);

            Renderer.RenderedBoundCompsListEnable = (SelectionMode == MapSelectionMode.Collision);


        }
        private void UpdateMouseHitsFromRenderer()
        {
            foreach (var rd in Renderer.RenderedDrawables)
            {
                UpdateMouseHits(rd.Drawable, rd.Archetype, rd.Entity);
            }
            foreach (var rb in Renderer.RenderedBoundComps)
            {
                UpdateMouseHits(rb.BoundComp, rb.Entity);
            }
        }
        private void UpdateMouseHits(DrawableBase drawable, Archetype arche, YmapEntityDef entity)
        {
            //if ((SelectionMode == MapSelectionMode.Entity) && !MouseSelectEnabled) return; //performance improvement when not selecting entities...

            //test the selected entity/archetype for mouse hit.
            
            //first test the bounding sphere for mouse hit..
            Quaternion orinv;
            Ray mraytrn;
            float hitdist = 0.0f;
            int geometryIndex = 0;
            DrawableGeometry geometry = null;
            BoundingBox geometryAABB = new BoundingBox();
            BoundingSphere bsph = new BoundingSphere();
            BoundingBox bbox = new BoundingBox();
            BoundingBox gbbox = new BoundingBox();
            Quaternion orientation = Quaternion.Identity;
            Vector3 scale = Vector3.One;
            Vector3 camrel = -camera.Position;
            if (entity != null)
            {
                orientation = entity.Orientation;
                scale = entity.Scale;
                camrel += entity.Position;
            }
            if (arche != null)
            {
                bsph.Center = camrel + orientation.Multiply(arche.BSCenter);//could use entity.BSCenter
                bsph.Radius = arche.BSRadius;
                bbox.Minimum = arche.BBMin * scale;
                bbox.Maximum = arche.BBMax * scale;
            }
            else
            {
                bsph.Center = camrel + drawable.BoundingCenter;
                bsph.Radius = drawable.BoundingSphereRadius;
                bbox.Minimum = drawable.BoundingBoxMin.XYZ() * scale;
                bbox.Maximum = drawable.BoundingBoxMax.XYZ() * scale;
            }
            bool mousespherehit = camera.MouseRay.Intersects(ref bsph);



            if ((SelectionMode == MapSelectionMode.EntityExtension) || (SelectionMode == MapSelectionMode.ArchetypeExtension))
            {
                //transform the mouse ray into the entity space.
                orinv = Quaternion.Invert(orientation);
                mraytrn = new Ray();
                mraytrn.Position = orinv.Multiply(camera.MouseRay.Position-camrel);
                mraytrn.Direction = orinv.Multiply(camera.MouseRay.Direction);

                if (SelectionMode == MapSelectionMode.EntityExtension)
                {
                    if ((entity != null) && (entity.Extensions != null))
                    {
                        for (int i = 0; i < entity.Extensions.Length; i++)
                        {
                            var extension = entity.Extensions[i];
                            MapBox mb = GetExtensionBox(camrel, extension);
                            mb.Orientation = orientation;
                            mb.Scale = Vector3.One;// scale;
                            mb.BBMin *= scale;
                            mb.BBMax *= scale;
                            Renderer.BoundingBoxes.Add(mb);

                            bbox.Minimum = mb.BBMin; //TODO: refactor this!
                            bbox.Maximum = mb.BBMax;
                            if (mraytrn.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                            {
                                CurMouseHit.EntityDef = entity;
                                CurMouseHit.Archetype = arche;
                                CurMouseHit.EntityExtension = extension;
                                CurMouseHit.HitDist = hitdist;
                                CurMouseHit.CamRel = mb.CamRelPos;
                                CurMouseHit.AABB = bbox;
                            }
                        }
                    }
                    return; //only test extensions when in select extension mode...
                }
                if (SelectionMode == MapSelectionMode.ArchetypeExtension)
                {
                    if ((arche != null) && (arche.Extensions != null))
                    {
                        for (int i = 0; i < arche.Extensions.Length; i++)
                        {
                            var extension = arche.Extensions[i];
                            MapBox mb = GetExtensionBox(camrel, extension);
                            mb.Orientation = orientation;
                            mb.Scale = Vector3.One;// scale;
                            mb.BBMin *= scale;
                            mb.BBMax *= scale;
                            Renderer.BoundingBoxes.Add(mb);

                            bbox.Minimum = mb.BBMin; //TODO: refactor this!
                            bbox.Maximum = mb.BBMax;
                            if (mraytrn.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                            {
                                CurMouseHit.EntityDef = entity;
                                CurMouseHit.Archetype = arche;
                                CurMouseHit.ArchetypeExtension = extension;
                                CurMouseHit.HitDist = hitdist;
                                CurMouseHit.CamRel = mb.CamRelPos;
                                CurMouseHit.AABB = bbox;
                            }
                        }
                    }
                    return; //only test extensions when in select extension mode...
                }

            }




            if (!mousespherehit)
            { return; } //no sphere hit, so no entity hit.



            bool usegeomboxes = SelectByGeometry;
            var dmodels = drawable.DrawableModelsHigh;
            if((dmodels==null)||(dmodels.data_items==null))
            { usegeomboxes = false; }
            if (usegeomboxes)
            {
                for (int i = 0; i < dmodels.data_items.Length; i++)
                {
                    var m = dmodels.data_items[i];
                    if (m.BoundsData == null)
                    { usegeomboxes = false; break; }
                }
            }



            //transform the mouse ray into the entity space.
            orinv = Quaternion.Invert(orientation);
            mraytrn = new Ray();
            mraytrn.Position = orinv.Multiply(camera.MouseRay.Position-camrel);
            mraytrn.Direction = orinv.Multiply(camera.MouseRay.Direction);
            hitdist = 0.0f;


            if (usegeomboxes)
            {
                //geometry bounding boxes version
                float ghitdist = float.MaxValue;
                for (int i = 0; i < dmodels.data_items.Length; i++)
                {
                    var m = dmodels.data_items[i];
                    int gbbcount = m.BoundsData.Length;
                    for (int j = 0; j < gbbcount; j++) //first box seems to be whole model
                    {
                        var gbox = m.BoundsData[j];
                        gbbox.Minimum = gbox.Min.XYZ();
                        gbbox.Maximum = gbox.Max.XYZ();
                        bbox.Minimum = gbbox.Minimum * scale;
                        bbox.Maximum = gbbox.Maximum * scale;
                        bool usehit = false;
                        if (mraytrn.Intersects(ref bbox, out hitdist))
                        {
                            if ((j == 0) && (gbbcount > 1)) continue;//ignore a model hit
                            //bool firsthit = (mousehit.EntityDef == null);
                            if (hitdist > 0.0f) //firsthit || //ignore when inside the box
                            {
                                bool nearer = ((hitdist < CurMouseHit.HitDist) && (hitdist < ghitdist));
                                bool radsm = true;
                                if (CurMouseHit.Geometry != null)
                                {
                                    var b1 = (gbbox.Maximum - gbbox.Minimum) * scale;
                                    var b2 = (CurMouseHit.AABB.Maximum - CurMouseHit.AABB.Minimum) * scale;
                                    float r1 = b1.Length() * 0.5f;
                                    float r2 = b2.Length() * 0.5f;
                                    radsm = (r1 < (r2));// * 0.5f));
                                }
                                if ((nearer&&radsm) || radsm) usehit = true;
                            }
                        }
                        else if (j == 0) //no hit on model box
                        {
                            break; //don't try this model's geometries
                        }
                        if (usehit)
                        {
                            int gind = (j > 0) ? j - 1 : 0;
                            ghitdist = hitdist;
                            geometry = m.Geometries[gind];
                            geometryAABB = gbbox;
                            geometryIndex = gind;
                        }
                    }
                }
                if (geometry == null)
                {
                    return; //no geometry hit.
                }
                hitdist = ghitdist;
            }
            else
            {
                //archetype/drawable bounding boxes version
                bool outerhit = false;
                if (mraytrn.Intersects(ref bbox, out hitdist)) //test primary box
                {
                    bool firsthit = (CurMouseHit.EntityDef == null);
                    if (firsthit || (hitdist > 0.0f)) //ignore when inside the box..
                    {
                        bool nearer = (hitdist < CurMouseHit.HitDist);  //closer than the last..
                        bool radsm = true;
                        if ((CurMouseHit.Archetype != null) && (arche != null)) //compare hit archetype sizes...
                        {
                            //var b1 = (arche.BBMax - arche.BBMin) * scale;
                            //var b2 = (mousehit.Archetype.BBMax - mousehit.Archetype.BBMin) * scale;
                            float r1 = arche.BSRadius;
                            float r2 = CurMouseHit.Archetype.BSRadius;
                            radsm = (r1 <= (r2));// * 0.5f)); //prefer selecting smaller things
                        }
                        if ((nearer&&radsm) || radsm)
                        {
                            outerhit = true;
                        }
                    }
                }
                if (!outerhit)
                { return; } //no hit.
            }




            CurMouseHit.HitDist = (hitdist > 0.0f) ? hitdist : CurMouseHit.HitDist;
            CurMouseHit.EntityDef = entity;
            CurMouseHit.Archetype = arche;
            CurMouseHit.Drawable = drawable;
            CurMouseHit.Geometry = geometry;
            CurMouseHit.AABB = geometryAABB;
            CurMouseHit.GeometryIndex = geometryIndex;
            CurMouseHit.CamRel = camrel;




            //go through geometries...? need to use skeleton?
            //if (drawable.DrawableModelsHigh == null)
            //{ return; }
            //if (drawable.DrawableModelsHigh.data_items == null)
            //{ return; }
            //for (int i = 0; i < drawable.DrawableModelsHigh.data_items.Length; i++)
            //{
            //    var model = drawable.DrawableModelsHigh.data_items[i];
            //    if ((model.Geometries == null) || (model.Geometries.data_items == null))
            //    { continue; }
            //    if ((model.Unknown_18h_Data == null))
            //    { continue; }
            //    int boffset = 0;
            //    if ((model.Unknown_18h_Data.Length > model.Geometries.data_items.Length))
            //    { boffset = 1; }
            //    for (int j = 0; j < model.Geometries.data_items.Length; j++)
            //    {
            //        var geom = model.Geometries.data_items[j];
            //        var gbox = model.Unknown_18h_Data[j + boffset];
            //        bbox.Minimum = gbox.AABB_Max.XYZ();
            //        bbox.Maximum = gbox.AABB_Min.XYZ();
            //        if (mraytrn.Intersects(ref bbox, out hitdist)) //test geom box
            //        {
            //            bool firsthit = (mousehit.EntityDef == null);
            //            if (firsthit || (hitdist > 0.0f)) //ignore when inside the box..
            //            {
            //                bool nearer = (hitdist < mousehit.HitDist);  //closer than the last..
            //                if (nearer)
            //                {
            //                    mousehit.HitDist = (hitdist > 0.0f) ? hitdist : mousehit.HitDist;
            //                    mousehit.EntityDef = entity;
            //                    mousehit.Archetype = arche;
            //                    mousehit.Drawable = drawable;
            //                    mousehit.CamRel = camrel;
            //                }
            //            }
            //        }
            //    }
            //}


            //Bounds b = null;
            //var dd = drawable as Drawable;
            //if (dd != null)
            //{
            //    b = dd.Bound;
            //}
            //else
            //{
            //    var fd = drawable as FragDrawable;
            //    if (fd != null)
            //    {
            //        b = fd.Bound;
            //    }
            //}
            //if (b == null)
            //{ return; }
            //else
            //{ }


        }
        private void UpdateMouseHits(RenderableBoundComposite rndbc, YmapEntityDef entity)
        {
            if (SelectionMode != MapSelectionMode.Collision) return;

            var position = entity?.Position ?? Vector3.Zero;
            var orientation = entity?.Orientation ?? Quaternion.Identity;
            var scale = entity?.Scale ?? Vector3.One;

            var camrel = position - camera.Position;



            BoundingBox bbox = new BoundingBox();
            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;
            float hitdist = float.MaxValue;
            Quaternion orinv = Quaternion.Invert(orientation);
            Ray mraytrn = new Ray();
            mraytrn.Position = orinv.Multiply(camera.MouseRay.Position - camrel);
            mraytrn.Direction = orinv.Multiply(mray.Direction);

            MapBox mb = new MapBox();
            mb.CamRelPos = camrel;// rbginst.Inst.CamRel;
            mb.Orientation = orientation;
            mb.Scale = scale;

            foreach (var geom in rndbc.Geometries)
            {
                if (geom == null) continue;

                mb.BBMin = geom.BoundGeom.BoundingBoxMin;
                mb.BBMax = geom.BoundGeom.BoundingBoxMax;

                var cent = camrel + (mb.BBMin + mb.BBMax) * 0.5f;
                if (cent.Length() > Renderer.renderboundsmaxdist) continue;

                Renderer.BoundingBoxes.Add(mb);

                bbox.Minimum = mb.BBMin * scale;
                bbox.Maximum = mb.BBMax * scale;
                if (mraytrn.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                {
                    CurMouseHit.CollisionBounds = geom.BoundGeom;
                    CurMouseHit.EntityDef = entity;
                    CurMouseHit.Archetype = entity?.Archetype;
                    CurMouseHit.HitDist = hitdist;
                    CurMouseHit.CamRel = camrel;
                    CurMouseHit.AABB = bbox;
                }
            }

        }
        private void UpdateMouseHits(YmapFile ymap)
        {
            //find mouse hits for things like time cycle mods and car generators in ymaps.

            BoundingBox bbox = new BoundingBox();
            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;
            float hitdist = float.MaxValue;

            float dmax = Renderer.renderboundsmaxdist;

            if ((SelectionMode == MapSelectionMode.TimeCycleModifier) && (ymap.TimeCycleModifiers != null))
            {
                for (int i = 0; i < ymap.TimeCycleModifiers.Length; i++)
                {
                    var tcm = ymap.TimeCycleModifiers[i];
                    if ((((tcm.BBMin + tcm.BBMax) * 0.5f) - camera.Position).Length() > dmax) continue;

                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = tcm.BBMin;
                    mb.BBMax = tcm.BBMax;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);

                    bbox.Minimum = mb.BBMin;
                    bbox.Maximum = mb.BBMax;
                    if (mray.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                    {
                        CurMouseHit.TimeCycleModifier = tcm;
                        CurMouseHit.HitDist = hitdist;
                        CurMouseHit.CamRel = mb.CamRelPos;
                        CurMouseHit.AABB = bbox;
                    }
                }
            }
            if ((SelectionMode == MapSelectionMode.CarGenerator) && (ymap.CarGenerators != null))
            {
                for (int i = 0; i < ymap.CarGenerators.Length; i++)
                {
                    var cg = ymap.CarGenerators[i];
                    MapBox mb = new MapBox();
                    mb.CamRelPos = cg.Position - camera.Position;
                    mb.BBMin = cg.BBMin;
                    mb.BBMax = cg.BBMax;
                    mb.Orientation = cg.Orientation;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);

                    Quaternion orinv = Quaternion.Invert(cg.Orientation);
                    Ray mraytrn = new Ray();
                    mraytrn.Position = orinv.Multiply(camera.MouseRay.Position - mb.CamRelPos);
                    mraytrn.Direction = orinv.Multiply(mray.Direction);
                    bbox.Minimum = mb.BBMin;
                    bbox.Maximum = mb.BBMax;
                    if (mraytrn.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                    {
                        CurMouseHit.CarGenerator = cg;
                        CurMouseHit.HitDist = hitdist;
                        CurMouseHit.CamRel = mb.CamRelPos;
                        CurMouseHit.AABB = bbox;
                    }
                }
                if (SelectedItem.CarGenerator != null)
                {
                }

            }
            if ((SelectionMode == MapSelectionMode.MloInstance) && (ymap.MloEntities != null))
            {
                for (int i = 0; i < ymap.MloEntities.Length; i++)
                {
                    var ent = ymap.MloEntities[i];
                    MapBox mb = new MapBox();
                    mb.CamRelPos = ent.Position - camera.Position;
                    mb.BBMin = /*ent?.BBMin ??*/ new Vector3(-1.5f);
                    mb.BBMax = /*ent?.BBMax ??*/ new Vector3(1.5f);
                    mb.Orientation = ent?.Orientation ?? Quaternion.Identity;
                    mb.Scale = /*ent?.Scale ??*/ Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);

                    Quaternion orinv = Quaternion.Invert(mb.Orientation);
                    Ray mraytrn = new Ray();
                    mraytrn.Position = orinv.Multiply(camera.MouseRay.Position - mb.CamRelPos);
                    mraytrn.Direction = orinv.Multiply(mray.Direction);
                    bbox.Minimum = mb.BBMin;
                    bbox.Maximum = mb.BBMax;
                    if (mraytrn.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                    {
                        CurMouseHit.MloEntityDef = ent;
                        CurMouseHit.EntityDef = ent;
                        CurMouseHit.HitDist = hitdist;
                        CurMouseHit.CamRel = mb.CamRelPos;
                        CurMouseHit.AABB = new BoundingBox(mb.BBMin, mb.BBMax);
                    }
                }
            }
            if ((SelectionMode == MapSelectionMode.Grass) && (ymap.GrassInstanceBatches != null))
            {
                for (int i = 0; i < ymap.GrassInstanceBatches.Length; i++)
                {
                    var gb = ymap.GrassInstanceBatches[i];
                    if ((gb.Position - camera.Position).Length() > dmax) continue;

                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = gb.AABBMin;
                    mb.BBMax = gb.AABBMax;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);

                    bbox.Minimum = mb.BBMin;
                    bbox.Maximum = mb.BBMax;
                    if (mray.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                    {
                        CurMouseHit.GrassBatch = gb;
                        CurMouseHit.HitDist = hitdist;
                        CurMouseHit.CamRel = mb.CamRelPos;
                        CurMouseHit.AABB = bbox;
                    }
                }
            }
            if ((SelectionMode == MapSelectionMode.DistantLodLights) && (ymap.DistantLODLights != null))
            {
                var dll = ymap.DistantLODLights;
                if ((((dll.BBMin + dll.BBMax) * 0.5f) - camera.Position).Length() <= dmax)
                {
                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = dll.BBMin;
                    mb.BBMax = dll.BBMax;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);

                    bbox.Minimum = mb.BBMin;
                    bbox.Maximum = mb.BBMax;
                    if (mray.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                    {
                        CurMouseHit.DistantLodLights = dll;
                        CurMouseHit.HitDist = hitdist;
                        CurMouseHit.CamRel = mb.CamRelPos;
                        CurMouseHit.AABB = bbox;
                    }
                }
            }


        }
        private void UpdateMouseHits(List<WaterQuad> waterquads)
        {
            if (SelectionMode != MapSelectionMode.WaterQuad) return;

            BoundingBox bbox = new BoundingBox();
            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;
            float hitdist = float.MaxValue;


            foreach (var quad in waterquads)
            {
                MapBox mb = new MapBox();
                mb.CamRelPos = -camera.Position;
                mb.BBMin = new Vector3(quad.minX, quad.minY, quad.z);
                mb.BBMax = new Vector3(quad.maxX, quad.maxY, quad.z);
                mb.Orientation = Quaternion.Identity;
                mb.Scale = Vector3.One;
                Renderer.BoundingBoxes.Add(mb);

                bbox.Minimum = mb.BBMin;
                bbox.Maximum = mb.BBMax;
                if (mray.Intersects(ref bbox, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                {
                    CurMouseHit.WaterQuad = quad;
                    CurMouseHit.HitDist = hitdist;
                    CurMouseHit.CamRel = mb.CamRelPos;
                    CurMouseHit.AABB = bbox;
                }
            }
        }
        private void UpdateMouseHits(List<YnvFile> ynvs)
        {
            if (SelectionMode != MapSelectionMode.NavMesh) return;

            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;

            foreach (var ynv in ynvs)
            {
                if (renderpathbounds)
                {
                    if (ynv.Nav == null) continue;
                    if (ynv.Nav.SectorTree == null) continue;

                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = ynv.Nav.SectorTree.AABBMin.XYZ();
                    mb.BBMax = ynv.Nav.SectorTree.AABBMax.XYZ();
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);
                }

                if ((ynv.Nav != null) && (ynv.Vertices != null) && (ynv.Indices != null) && (ynv.Polys != null))
                {
                    UpdateMouseHits(ynv, ynv.Nav.SectorTree, ynv.Nav.SectorTree, ref mray);
                }
            }

            if ((CurMouseHit.NavPoly != null) && MouseSelectEnabled)
            {
                var colour = Color4.White;
                var colourval = (uint)colour.ToRgba();
                var poly = CurMouseHit.NavPoly;
                var ynv = poly.Ynv;
                var ic = poly._RawData.IndexCount;
                var startid = poly._RawData.IndexID;
                var endid = startid + ic;
                var lastid = endid - 1;
                var vc = ynv.Vertices.Count;
                var startind = ynv.Indices[startid];
                
                ////draw poly outline
                VertexTypePC v = new VertexTypePC();
                v.Colour = colourval;
                VertexTypePC v0 = new VertexTypePC();
                for (int id = startid; id < endid; id++)
                {
                    var ind = ynv.Indices[id];
                    if (ind >= vc)
                    { continue; }

                    v.Position = ynv.Vertices[ind];
                    Renderer.SelectionLineVerts.Add(v);
                    if (id == startid)
                    {
                        v0 = v;
                    }
                    else
                    {
                        Renderer.SelectionLineVerts.Add(v);
                    }
                    if (id == lastid)
                    {
                        Renderer.SelectionLineVerts.Add(v0);
                    }
                }


                ////draw poly triangles
                //VertexTypePC v0 = new VertexTypePC();
                //VertexTypePC v1 = new VertexTypePC();
                //VertexTypePC v2 = new VertexTypePC();
                //v0.Position = ynv.Vertices[startind];
                //v0.Colour = colourval;
                //v1.Colour = colourval;
                //v2.Colour = colourval;
                //int tricount = ic - 2;
                //for (int t = 0; t < tricount; t++)
                //{
                //    int tid = startid + t;
                //    int ind1 = ynv.Indices[tid + 1];
                //    int ind2 = ynv.Indices[tid + 2];
                //    if ((ind1 >= vc) || (ind2 >= vc))
                //    { continue; }
                //    v1.Position = ynv.Vertices[ind1];
                //    v2.Position = ynv.Vertices[ind2];
                //    Renderer.SelectionTriVerts.Add(v0);
                //    Renderer.SelectionTriVerts.Add(v1);
                //    Renderer.SelectionTriVerts.Add(v2);
                //    Renderer.SelectionTriVerts.Add(v0);
                //    Renderer.SelectionTriVerts.Add(v2);
                //    Renderer.SelectionTriVerts.Add(v1);
                //}

            }

        }
        private void UpdateMouseHits(YnvFile ynv, NavMeshSector navsector, NavMeshSector rootsec, ref Ray mray)
        {
            if (navsector == null) return;

            float hitdist = float.MaxValue;

            BoundingBox bbox = new BoundingBox();
            bbox.Minimum = navsector.AABBMin.XYZ();
            bbox.Maximum = navsector.AABBMax.XYZ();

            if (rootsec != null) //apparently the Z values are incorrect :(
            {
                bbox.Minimum.Z = rootsec.AABBMin.Z;
                bbox.Maximum.Z = rootsec.AABBMax.Z;
            }

            float fhd;
            if (mray.Intersects(ref bbox, out fhd)) //ray intersects this node... check children for hits!
            {
                ////test vis
                //MapBox mb = new MapBox();
                //mb.CamRelPos = -camera.Position;
                //mb.BBMin = bbox.Minimum;
                //mb.BBMax = bbox.Maximum;
                //mb.Orientation = Quaternion.Identity;
                //mb.Scale = Vector3.One;
                //BoundingBoxes.Add(mb);


                if (navsector.SubTree1 != null)
                {
                    UpdateMouseHits(ynv, navsector.SubTree1, rootsec, ref mray);
                }
                if (navsector.SubTree2 != null)
                {
                    UpdateMouseHits(ynv, navsector.SubTree2, rootsec, ref mray);
                }
                if (navsector.SubTree3 != null)
                {
                    UpdateMouseHits(ynv, navsector.SubTree3, rootsec, ref mray);
                }
                if (navsector.SubTree4 != null)
                {
                    UpdateMouseHits(ynv, navsector.SubTree4, rootsec, ref mray);
                }
                if ((navsector.Data != null) && (navsector.Data.PolyIDs != null))
                {
                    BoundingBox cbox = new BoundingBox();
                    cbox.Minimum = bbox.Minimum - camera.Position;
                    cbox.Maximum = bbox.Maximum - camera.Position;

                    var polys = ynv.Polys;
                    var polyids = navsector.Data.PolyIDs;
                    for (int i = 0; i < polyids.Length; i++)
                    {
                        var polyid = polyids[i];
                        if (polyid >= polys.Count)
                        { continue; }

                        var poly = polys[polyid];
                        var ic = poly._RawData.IndexCount;
                        var startid = poly._RawData.IndexID;
                        var endid = startid + ic;
                        if (startid >= ynv.Indices.Count)
                        { continue; }
                        if (endid > ynv.Indices.Count)
                        { continue; }

                        var vc = ynv.Vertices.Count;
                        var startind = ynv.Indices[startid];
                        if (startind >= vc)
                        { continue; }

                        Vector3 p0 = ynv.Vertices[startind];

                        //test triangles for the poly.
                        int tricount = ic - 2;
                        for (int t = 0; t < tricount; t++)
                        {
                            int tid = startid + t;
                            int ind1 = ynv.Indices[tid + 1];
                            int ind2 = ynv.Indices[tid + 2];
                            if ((ind1 >= vc) || (ind2 >= vc))
                            { continue; }

                            Vector3 p1 = ynv.Vertices[ind1];
                            Vector3 p2 = ynv.Vertices[ind2];

                            if (mray.Intersects(ref p0, ref p1, ref p2, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                            {
                                var cellaabb = poly._RawData.CellAABB;
                                CurMouseHit.NavPoly = poly;
                                CurMouseHit.HitDist = hitdist;
                                CurMouseHit.AABB = new BoundingBox(cellaabb.Min, cellaabb.Max);
                                break;//no need to test further tris in this poly
                            }
                        }
                    }
                }
            }
        }
        private void UpdateMouseHits(List<YndFile> ynds)
        {
            if (SelectionMode != MapSelectionMode.Path) return;

            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;

            foreach (var ynd in ynds)
            {
                if (renderpathbounds)
                {
                    float minz = (ynd.BVH != null) ? ynd.BVH.Box.Minimum.Z : 0.0f;
                    float maxz = (ynd.BVH != null) ? ynd.BVH.Box.Maximum.Z : 0.0f;
                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = new Vector3(ynd.BBMin.X, ynd.BBMin.Y, minz);
                    mb.BBMax = new Vector3(ynd.BBMax.X, ynd.BBMax.Y, maxz);
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);
                }

                if (ynd.BVH != null)
                {
                    UpdateMouseHits(ynd.BVH, ref mray);
                }
            }


            if (SelectedItem.PathNode != null)
            {
                float linkrad = 0.25f;

                var n = SelectedItem.PathNode;
                if (n.Links != null)
                {
                    foreach (var ln in n.Links)
                    {
                        if (ln.Node2 == null) continue;//invalid links can hit here...
                        Vector3 dv = n.Position - ln.Node2.Position;
                        float dl = dv.Length();
                        Vector3 dir = dv * (1.0f / dl);
                        Vector3 dup = Vector3.UnitZ;
                        MapBox mb = new MapBox();

                        int lanestot = ln.LaneCountForward + ln.LaneCountBackward;
                        float lanewidth = n.IsPedNode ? 0.5f : 5.5f;
                        float inner = ln.LaneOffset * lanewidth;// 0.0f;
                        float outer = inner + Math.Max(lanewidth * ln.LaneCountForward, 0.5f);
                        float totwidth = lanestot * lanewidth;
                        float halfwidth = totwidth * 0.5f;
                        if (ln.LaneCountBackward == 0)
                        {
                            inner -= halfwidth;
                            outer -= halfwidth;
                        }
                        if (ln.LaneCountForward == 0)
                        {
                            inner += halfwidth;
                            outer += halfwidth;
                        }


                        mb.CamRelPos = n.Position - camera.Position;
                        mb.BBMin = new Vector3(-linkrad - outer, -linkrad, 0.0f);
                        mb.BBMax = new Vector3(linkrad - inner, linkrad, dl);
                        mb.Orientation = Quaternion.Invert(Quaternion.RotationLookAtRH(dir, dup));
                        mb.Scale = Vector3.One;
                        if (ln == SelectedItem.PathLink)
                        {
                            Renderer.HilightBoxes.Add(mb);
                        }
                        else
                        {
                            Renderer.BoundingBoxes.Add(mb);
                        }
                    }
                }
            }

        }
        private void UpdateMouseHits(List<TrainTrack> tracks)
        {
            if (SelectionMode != MapSelectionMode.TrainTrack) return;

            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;

            foreach (var track in tracks)
            {
                if (renderpathbounds)
                {
                    //MapBox mb = new MapBox();
                    //mb.CamRelPos = -camera.Position;
                    //mb.BBMin = track.BVH?.Box.Minimum ?? Vector3.Zero;
                    //mb.BBMax = track.BVH?.Box.Maximum ?? Vector3.Zero;
                    //mb.Orientation = Quaternion.Identity;
                    //mb.Scale = Vector3.One;
                    //BoundingBoxes.Add(mb);
                }

                if (track.BVH != null)
                {
                    UpdateMouseHits(track.BVH, ref mray);
                }
            }


            if (SelectedItem.TrainTrackNode != null)
            {
                float linkrad = 0.25f;
                var n = SelectedItem.TrainTrackNode;
                if (n.Links != null)
                {
                    foreach (var ln in n.Links)
                    {
                        if (ln == null) continue;
                        Vector3 dv = n.Position - ln.Position;
                        float dl = dv.Length();
                        Vector3 dir = dv * (1.0f / dl);
                        Vector3 dup = Vector3.UnitZ;
                        MapBox mb = new MapBox();
                        mb.CamRelPos = n.Position - camera.Position;
                        mb.BBMin = new Vector3(-linkrad, -linkrad, 0.0f);
                        mb.BBMax = new Vector3(linkrad, linkrad, dl);
                        mb.Orientation = Quaternion.Invert(Quaternion.RotationLookAtRH(dir, dup));
                        mb.Scale = Vector3.One;
                        Renderer.BoundingBoxes.Add(mb);
                    }
                }
            }

        }
        private void UpdateMouseHits(List<YmtFile> scenarios)
        {
            if (SelectionMode != MapSelectionMode.Scenario) return;

            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;

            foreach (var scenario in scenarios)
            {
                var sr = scenario.ScenarioRegion;
                if (sr == null) continue;

                if (renderscenariobounds)
                {
                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = sr?.BVH?.Box.Minimum ?? Vector3.Zero;
                    mb.BBMax = sr?.BVH?.Box.Maximum ?? Vector3.Zero;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    Renderer.BoundingBoxes.Add(mb);
                }

                if (sr.BVH != null)
                {
                    UpdateMouseHits(sr.BVH, ref mray);
                }
            }


            if (SelectedItem.ScenarioNode != null) //move this stuff to renderselection..?
            {
                var n = SelectedItem.ScenarioNode;
                var nc = n.ChainingNode?.Chain;
                var ncl = n.Cluster;


                //float linkrad = 0.25f;
                //if (n.Links != null)
                //{
                //    foreach (var ln in n.Links)
                //    {
                //        if (ln == null) continue;
                //        Vector3 dv = n.Position - ln.Position;
                //        float dl = dv.Length();
                //        Vector3 dir = dv * (1.0f / dl);
                //        Vector3 dup = Vector3.UnitZ;
                //        MapBox mb = new MapBox();
                //        mb.CamRelPos = n.Position - camera.Position;
                //        mb.BBMin = new Vector3(-linkrad, -linkrad, 0.0f);
                //        mb.BBMax = new Vector3(linkrad, linkrad, dl);
                //        mb.Orientation = Quaternion.Invert(Quaternion.RotationLookAtRH(dir, dup));
                //        mb.Scale = Vector3.One;
                //        BoundingBoxes.Add(mb);
                //    }
                //}

                var sr = SelectedItem.ScenarioNode.Ymt.ScenarioRegion;
                //if (renderscenariobounds)
                {
                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = sr?.BVH?.Box.Minimum ?? Vector3.Zero;
                    mb.BBMax = sr?.BVH?.Box.Maximum ?? Vector3.Zero;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    if (renderscenariobounds)
                    {
                        Renderer.HilightBoxes.Add(mb);
                    }
                    else
                    {
                        Renderer.BoundingBoxes.Add(mb);
                    }
                }


                if (ncl != null)
                {

                    //hilight the cluster itself
                    MapBox mb = new MapBox();
                    mb.Scale = Vector3.One;
                    mb.BBMin = new Vector3(-0.5f);
                    mb.BBMax = new Vector3(0.5f);
                    mb.CamRelPos = ncl.Position - camera.Position;
                    mb.Orientation = Quaternion.Identity;
                    Renderer.HilightBoxes.Add(mb);


                    //show boxes for points in the cluster
                    if ((ncl.Points != null) && (ncl.Points.MyPoints != null))
                    {
                        foreach (var clpoint in ncl.Points.MyPoints)
                        {
                            if (clpoint == n.ClusterMyPoint) continue; //don't highlight the selected node...
                            mb = new MapBox();
                            mb.Scale = Vector3.One;
                            mb.BBMin = new Vector3(-0.5f);
                            mb.BBMax = new Vector3(0.5f);
                            mb.CamRelPos = clpoint.Position - camera.Position;
                            mb.Orientation = clpoint.Orientation;
                            Renderer.BoundingBoxes.Add(mb);
                        }
                    }
                }



            }




        }
        private void UpdateMouseHits(PathBVHNode pathbvhnode, ref Ray mray)
        {
            float nrad = 0.5f;
            float hitdist = float.MaxValue;

            BoundingSphere bsph = new BoundingSphere();
            bsph.Radius = nrad;

            BoundingBox bbox = new BoundingBox();
            bbox.Minimum = pathbvhnode.Box.Minimum - nrad;
            bbox.Maximum = pathbvhnode.Box.Maximum + nrad;

            BoundingBox nbox = new BoundingBox();
            nbox.Minimum = new Vector3(-nrad);
            nbox.Maximum = new Vector3(nrad);

            float fhd;
            if (mray.Intersects(ref bbox, out fhd)) //ray intersects this node... check children for hits!
            {
                if ((pathbvhnode.Node1 != null) && (pathbvhnode.Node2 != null)) //node is split. recurse
                {
                    UpdateMouseHits(pathbvhnode.Node1, ref mray);
                    UpdateMouseHits(pathbvhnode.Node2, ref mray);
                }
                else if (pathbvhnode.Nodes != null) //leaf node. test contaned pathnodes
                {
                    foreach (var n in pathbvhnode.Nodes)
                    {
                        bsph.Center = n.Position;
                        if (mray.Intersects(ref bsph, out hitdist) && (hitdist < CurMouseHit.HitDist) && (hitdist > 0))
                        {
                            CurMouseHit.PathNode = n as YndNode;
                            CurMouseHit.TrainTrackNode = n as TrainTrackNode;
                            CurMouseHit.ScenarioNode = n as ScenarioNode;
                            CurMouseHit.HitDist = hitdist;
                            CurMouseHit.CamRel = (n.Position - camera.Position);
                            CurMouseHit.AABB = nbox;
                        }
                    }
                }
            }
        }

        public void SelectItem(MapSelection? mhit = null, bool addSelection = false)
        {
            var mhitv = mhit.HasValue ? mhit.Value : new MapSelection();
            if (mhit != null)
            {
                if ((mhitv.Archetype == null) && (mhitv.EntityDef != null))
                {
                    mhitv.Archetype = mhitv.EntityDef.Archetype; //use the entity archetype if no archetype given
                }
                if (mhitv.GrassBatch != null)
                {
                    mhitv.Archetype = mhitv.GrassBatch.Archetype;
                }
            }
            if ((mhitv.Archetype != null) && (mhitv.Drawable == null))
            {
                mhitv.Drawable = gameFileCache.TryGetDrawable(mhitv.Archetype); //no drawable given.. try to get it from the cache.. if it's not there, drawable info won't display...
            }


            bool change = false;
            if (mhit != null)
            {
                change = SelectedItem.CheckForChanges(mhitv); 
            }
            else
            {
                change = SelectedItem.CheckForChanges();
            }

            if (addSelection)
            {
                if (SelectedItem.MultipleSelection)
                {
                    if (mhitv.HasValue) //incoming selection isn't empty...
                    {
                        //search the list for a match, remove it if already there, otherwise add it.
                        bool found = false;
                        foreach (var item in SelectedItems)
                        {
                            if (!item.CheckForChanges(mhitv))
                            {
                                SelectedItems.Remove(item);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            if (SelectedItems.Count == 1)
                            {
                                mhitv = SelectedItems[0];
                                SelectedItems.Clear();
                            }
                            else if (SelectedItems.Count <= 0)
                            {
                                mhitv.Clear();
                                SelectedItems.Clear();//this shouldn't really happen..
                            }
                        }
                        else
                        {
                            mhitv.MultipleSelection = false;
                            SelectedItems.Add(mhitv);
                        }
                        change = true;
                    }
                    else //empty incoming value... do nothing?
                    {
                        return;
                    }
                }
                else //current selection is single item, or empty
                {
                    if (change) //incoming selection item is different from the current one
                    {
                        if (mhitv.HasValue) //incoming selection isn't empty, add it to the list
                        {
                            if (SelectedItem.HasValue) //add the existing item to the selection list, if it's not empty
                            {
                                SelectedItem.MultipleSelection = false;
                                SelectedItems.Add(SelectedItem);
                                mhitv.MultipleSelection = false;
                                SelectedItems.Add(mhitv);
                                SelectedItem.MultipleSelection = true;
                            }
                        }
                        else //empty incoming value... do nothing?
                        {
                            return;
                        }
                    }
                    else //same thing was selected a 2nd time, just clear the selection.
                    {
                        SelectedItem.Clear();
                        SelectedItems.Clear();
                        mhit = null; //dont's wants to selects it agains!
                        change = true;
                    }
                }

                if (SelectedItems.Count > 1)
                {
                    //iterate the selected items, and calculate the selection position
                    var center = Vector3.Zero;
                    foreach (var item in SelectedItems)
                    {
                        center += item.WidgetPosition;
                    }
                    if (SelectedItems.Count > 0)
                    {
                        center *= (1.0f / SelectedItems.Count);
                    }

                    mhitv.Clear();
                    mhitv.MultipleSelection = true;
                    mhitv.MultipleSelectionCenter = center;
                }
            }
            else
            {
                if (SelectedItem.MultipleSelection)
                {
                    change = true;
                    SelectedItem.MultipleSelection = false;
                    SelectedItem.Clear();
                }
                SelectedItems.Clear();
            }

            if (!change)
            {
                if (mhit.HasValue)
                {
                    //make sure the path link gets changed (sub-selection!)
                    lock (Renderer.RenderSyncRoot)
                    {
                        SelectedItem.PathLink = mhitv.PathLink;
                        SelectedItem.ScenarioEdge = mhitv.ScenarioEdge;
                    }
                }
                return;
            }

            lock (Renderer.RenderSyncRoot) //drawflags is used when rendering.. need that lock
            {
                if (mhit.HasValue)
                {
                    SelectedItem = mhitv;
                }
                else
                {
                    SelectedItem.Clear();
                }

                if (change)
                {
                    UpdateSelectionUI(true);

                    Widget.Visible = SelectedItem.CanShowWidget;
                    if (Widget.Visible)
                    {
                        Widget.Position = SelectedItem.WidgetPosition;
                        Widget.Rotation = SelectedItem.WidgetRotation;
                        Widget.RotationWidget.EnableAxes = SelectedItem.WidgetRotationAxes;
                        Widget.Scale = SelectedItem.WidgetScale;
                    }
                }
            }
            if (change && (ProjectForm != null))
            {
                ProjectForm.OnWorldSelectionChanged(SelectedItem);
            }
        }
        public void SelectMulti(MapSelection[] items)
        {
            SelectItem(null);
            if (items != null)
            {
                foreach (var item in items)
                {
                    SelectItem(item, true);
                }
            }
        }
        public void SelectEntity(YmapEntityDef entity)
        {
            if (entity == null)
            {
                SelectItem(null);
            }
            else
            {
                MapSelection ms = new MapSelection();
                ms.EntityDef = entity;
                ms.Archetype = entity?.Archetype;
                ms.AABB = new BoundingBox(entity.BBMin, entity.BBMax);
                SelectItem(ms);
            }
        }
        public void SelectCarGen(YmapCarGen cargen)
        {
            if (cargen == null)
            {
                SelectItem(null);
            }
            else
            {
                MapSelection ms = new MapSelection();
                ms.CarGenerator = cargen;
                ms.AABB = new BoundingBox(cargen.BBMin, cargen.BBMax);
                SelectItem(ms);
            }
        }
        public void SelectNavPoly(YnvPoly poly)
        {
            if (poly == null)
            {
                SelectItem(null);
            }
            else
            {
                var sect = poly.Ynv?.Nav?.SectorTree;

                MapSelection ms = new MapSelection();
                ms.NavPoly = poly;

                var cellaabb = poly._RawData.CellAABB;
                ms.AABB = new BoundingBox(cellaabb.Min, cellaabb.Max);
                //if (sect != null)
                //{
                //    ms.AABB = new BoundingBox(sect.AABBMin.XYZ(), sect.AABBMax.XYZ());
                //}
                SelectItem(ms);
            }
        }
        public void SelectPathNode(YndNode node)
        {
            if (node == null)
            {
                SelectItem(null);
            }
            else
            {
                float nrad = 0.5f;

                MapSelection ms = new MapSelection();
                ms.PathNode = node;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
                SelectItem(ms);
            }
        }
        public void SelectPathLink(YndLink link)
        {
            var node = link?.Node1;
            if (node == null)
            {
                SelectItem(null);
            }
            else
            {
                float nrad = 0.5f;

                MapSelection ms = new MapSelection();
                ms.PathNode = node;
                ms.PathLink = link;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
                SelectItem(ms);
            }
        }
        public void SelectTrainTrackNode(TrainTrackNode node)
        {
            if (node == null)
            {
                SelectItem(null);
            }
            else
            {
                float nrad = 0.5f;

                MapSelection ms = new MapSelection();
                ms.TrainTrackNode = node;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
                SelectItem(ms);
            }
        }
        public void SelectScenarioNode(ScenarioNode node)
        {
            if (node == null)
            {
                SelectItem(null);
            }
            else
            {
                float nrad = 0.5f;

                MapSelection ms = new MapSelection();
                ms.ScenarioNode = node;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
                SelectItem(ms);
            }
        }
        public void SelectScenarioEdge(ScenarioNode node, MCScenarioChainingEdge edge)
        {
            if (node == null)
            {
                SelectItem(null);
            }
            else
            {
                float nrad = 0.5f;

                MapSelection ms = new MapSelection();
                ms.ScenarioNode = node;
                ms.ScenarioEdge = edge;
                ms.AABB = new BoundingBox(new Vector3(-nrad), new Vector3(nrad));
                SelectItem(ms);
            }
        }
        public void SelectAudio(AudioPlacement audio)
        {
            if (audio == null)
            {
                SelectItem(null);
            }
            else
            {
                MapSelection ms = new MapSelection();
                ms.Audio = audio;
                ms.AABB = new BoundingBox(audio.HitboxMin, audio.HitboxMax);
                ms.BSphere = new BoundingSphere(audio.Position, audio.HitSphereRad);
                SelectItem(ms);
            }
        }
        private void SelectMousedItem()
        {
            //when clicked, select the currently moused item and update the selection info UI

            if (!MouseSelectEnabled)
            { return; }

            SelectItem(LastMouseHit, Input.CtrlPressed);
        }
        private void UpdateSelectionUI(bool wait)
        {
            try
            {
                if (InvokeRequired)
                {
                    if (wait)
                    {
                        Invoke(new Action(() => { UpdateSelectionUI(wait); }));
                    }
                    else
                    {
                        BeginInvoke(new Action(() => { UpdateSelectionUI(wait); }));
                    }
                }
                else
                {
                    SetSelectionUI(SelectedItem);

                    if (InfoForm != null)
                    {
                        InfoForm.SetSelection(SelectedItem, SelectedItems);
                    }
                }
            }
            catch { }
        }
        private void SetSelectionUI(MapSelection item)
        {
            SelectionNameTextBox.Text = item.GetNameString("Nothing selected");
            //SelEntityPropertyGrid.SelectedObject = item.EntityDef;
            SelArchetypePropertyGrid.SelectedObject = item.Archetype;
            SelDrawablePropertyGrid.SelectedObject = item.Drawable;

            Renderer.SelectionModelDrawFlags.Clear();
            Renderer.SelectionGeometryDrawFlags.Clear();
            SelDrawableModelsTreeView.Nodes.Clear();
            SelDrawableTexturesTreeView.Nodes.Clear();
            if (item.Drawable != null)
            {
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsHigh, "High Detail", true);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsMedium, "Medium Detail", false);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsLow, "Low Detail", false);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsVeryLow, "Very Low Detail", false);
                //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModelsX, "X Detail", false);
            }


            YmapFile ymap = null;
            YnvFile ynv = null;
            YndFile ynd = null;
            TrainTrack traintr = null;
            YmtFile scenario = null;
            ToolbarCopyButton.Enabled = false;
            ToolbarDeleteItemButton.Enabled = false;
            ToolbarDeleteItemButton.Text = "Delete";

            if (item.MultipleSelection)
            {
                SelectionEntityTabPage.Text = "Multiple items";
                SelEntityPropertyGrid.SelectedObject = SelectedItems.ToArray();
            }
            else if (item.TimeCycleModifier != null)
            {
                SelectionEntityTabPage.Text = "TCMod";
                SelEntityPropertyGrid.SelectedObject = item.TimeCycleModifier;
            }
            else if (item.CarGenerator != null)
            {
                SelectionEntityTabPage.Text = "CarGen";
                SelEntityPropertyGrid.SelectedObject = item.CarGenerator;
                ymap = item.CarGenerator.Ymap;
                ToolbarCopyButton.Enabled = true;
                ToolbarDeleteItemButton.Enabled = true;
                ToolbarDeleteItemButton.Text = "Delete car generator";
            }
            else if (item.DistantLodLights != null)
            {
                SelectionEntityTabPage.Text = "DistLodLight";
                SelEntityPropertyGrid.SelectedObject = item.DistantLodLights;
            }
            else if (item.GrassBatch != null)
            {
                SelectionEntityTabPage.Text = "Grass";
                SelEntityPropertyGrid.SelectedObject = item.GrassBatch;
            }
            else if (item.WaterQuad != null)
            {
                SelectionEntityTabPage.Text = "WaterQuad";
                SelEntityPropertyGrid.SelectedObject = item.WaterQuad;
            }
            else if (item.PathNode != null)
            {
                SelectionEntityTabPage.Text = "PathNode";
                SelEntityPropertyGrid.SelectedObject = item.PathNode;
                ynd = item.PathNode.Ynd;
                ToolbarCopyButton.Enabled = true;
                ToolbarDeleteItemButton.Enabled = true;
                ToolbarDeleteItemButton.Text = "Delete path node";
            }
            else if (item.NavPoly != null)
            {
                SelectionEntityTabPage.Text = "NavPoly";
                SelEntityPropertyGrid.SelectedObject = item.NavPoly;
                ynv = item.NavPoly.Ynv;
                //ToolbarCopyButton.Enabled = true;
                //ToolbarDeleteItemButton.Enabled = true;
                //ToolbarDeleteItemButton.Text = "Delete nav poly";
            }
            else if (item.TrainTrackNode != null)
            {
                SelectionEntityTabPage.Text = "TrainNode";
                SelEntityPropertyGrid.SelectedObject = item.TrainTrackNode;
                traintr = item.TrainTrackNode.Track;
                ToolbarCopyButton.Enabled = true;
                ToolbarDeleteItemButton.Enabled = true;
                ToolbarDeleteItemButton.Text = "Delete train track node";
            }
            else if (item.ScenarioNode != null)
            {
                SelectionEntityTabPage.Text = item.ScenarioNode.ShortTypeName;
                SelEntityPropertyGrid.SelectedObject = item.ScenarioNode;
                scenario = item.ScenarioNode.Ymt;
                ToolbarCopyButton.Enabled = true;
                ToolbarDeleteItemButton.Enabled = true;
                ToolbarDeleteItemButton.Text = "Delete scenario point";
            }
            else if (item.Audio != null)
            {
                SelectionEntityTabPage.Text = item.Audio.ShortTypeName;
                SelEntityPropertyGrid.SelectedObject = item.Audio;
            }
            else
            {
                SelectionEntityTabPage.Text = "Entity";
                SelEntityPropertyGrid.SelectedObject = item.EntityDef;
                if (item.EntityDef != null)
                {
                    ymap = item.EntityDef?.Ymap;
                    ToolbarCopyButton.Enabled = true;
                    ToolbarDeleteItemButton.Enabled = true;
                    ToolbarDeleteItemButton.Text = "Delete entity";
                }
            }


            if (item.EntityExtension != null)
            {
                SelExtensionPropertyGrid.SelectedObject = item.EntityExtension;
                ShowSelectedExtensionTab(true);
            }
            else if (item.ArchetypeExtension != null)
            {
                SelExtensionPropertyGrid.SelectedObject = item.ArchetypeExtension;
                ShowSelectedExtensionTab(true);
            }
            else if (item.CollisionBounds != null)
            {
                SelExtensionPropertyGrid.SelectedObject = item.CollisionBounds;
                ShowSelectedExtensionTab(true, "Coll");
            }
            else
            {
                SelExtensionPropertyGrid.SelectedObject = null;
                ShowSelectedExtensionTab(false);
            }


            //var ent = SelectedItem.EntityDef;
            //ToolbarDeleteEntityButton.Enabled = false;
            ////ToolbarAddEntityButton.Enabled = false;
            //ToolbarCopyButton.Enabled = (ent != null);
            //if (ent != null)
            //{
            //    ToolbarDeleteEntityButton.Enabled = true;
            //    //ToolbarAddEntityButton.Enabled = true;
            //    //if (ProjectForm != null)
            //    //{
            //    //    ToolbarDeleteEntityButton.Enabled = ProjectForm.IsCurrentEntity(ent);
            //    //}
            //}
            //bool enableymapui = (ent != null) && (ent.Ymap != null);
            //var ymap = ent?.Ymap;

            bool enableymapui = (ymap != null);

            EnableYmapUI(enableymapui, (ymap != null) ? ymap.Name : "");

            if (ynd != null)
            {
                EnableYndUI(true, ynd.Name);
            }
            if (ynv != null)
            {
                EnableYnvUI(true, ynv.Name);
            }
            if (traintr != null)
            {
                EnableTrainsUI(true, traintr.Name);
            }
            if (scenario != null)
            {
                EnableScenarioUI(true, scenario.Name);
            }

        }
        private void ShowSelectedExtensionTab(bool show, string text = "Ext")
        {
            SelectionExtensionTabPage.Text = text;
            if (show)
            {
                if (!SelectionTabControl.TabPages.Contains(SelectionExtensionTabPage))
                {
                    SelectionTabControl.TabPages.Add(SelectionExtensionTabPage);
                    SelectionTabControl.SelectedTab = SelectionExtensionTabPage;
                }
            }
            else
            {
                if (SelectionTabControl.TabPages.Contains(SelectionExtensionTabPage))
                {
                    SelectionTabControl.TabPages.Remove(SelectionExtensionTabPage);
                }
            }
        }
        private void AddSelectionDrawableModelsTreeNodes(ResourcePointerList64<DrawableModel> models, string prefix, bool check)
        {
            if (models == null) return;
            if (models.data_items == null) return;

            for (int mi = 0; mi < models.data_items.Length; mi++)
            {
                var model = models.data_items[mi];
                string mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = SelDrawableModelsTreeView.Nodes.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;
                mnode.Checked = check;

                var tmnode = SelDrawableTexturesTreeView.Nodes.Add(mprefix + " " + model.ToString());
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
            var model = node.Tag as DrawableModel;
            var geom = node.Tag as DrawableGeometry;
            bool rem = node.Checked;

            Renderer.UpdateSelectionDrawFlags(model, geom, rem);
        }
        public void SyncSelDrawableModelsTreeNode(TreeNode node)
        {
            //called by the info form when a selection treeview node is checked/unchecked.
            foreach (TreeNode mnode in SelDrawableModelsTreeView.Nodes)
            {
                if (mnode.Tag == node.Tag)
                {
                    if (mnode.Checked != node.Checked)
                    {
                        mnode.Checked = node.Checked;
                    }
                }
                foreach (TreeNode gnode in mnode.Nodes)
                {
                    if (gnode.Tag == node.Tag)
                    {
                        if (gnode.Checked != node.Checked)
                        {
                            gnode.Checked = node.Checked;
                        }
                    }
                }
            }
        }


        private void ShowInfoForm()
        {
            if (InfoForm == null)
            {
                InfoForm = new WorldInfoForm(this);
                InfoForm.SetSelection(SelectedItem, SelectedItems);
                InfoForm.SetSelectionMode(SelectionModeStr, MouseSelectEnabled);
                InfoForm.Show(this);
            }
            else
            {
                if (InfoForm.WindowState == FormWindowState.Minimized)
                {
                    InfoForm.WindowState = FormWindowState.Normal;
                }
                InfoForm.Focus();
            }
            ToolbarInfoWindowButton.Checked = true;
        }
        public void OnInfoFormSelectionModeChanged(string mode, bool enableSelect)
        {
            //called by the WorldInfoForm
            SetSelectionMode(mode);
            SetMouseSelect(enableSelect);
        }
        public void OnInfoFormClosed()
        {
            //called by the WorldInfoForm when it's closed.
            InfoForm = null;
            ToolbarInfoWindowButton.Checked = false;
        }

        private void ShowProjectForm()
        {
            if (ProjectForm == null)
            {
                ProjectForm = new ProjectForm(this);
                ProjectForm.Show(this);
            }
            else
            {
                if (ProjectForm.WindowState == FormWindowState.Minimized)
                {
                    ProjectForm.WindowState = FormWindowState.Normal;
                }
                ProjectForm.Focus();
            }
            ToolbarProjectWindowButton.Checked = true;
        }
        public void OnProjectFormClosed()
        {
            ProjectForm = null;
            ToolbarProjectWindowButton.Checked = false;
        }

        private void ShowSearchForm()
        {
            if (SearchForm == null)
            {
                SearchForm = new WorldSearchForm(this);
                SearchForm.Show(this);
            }
            else
            {
                if (SearchForm.WindowState == FormWindowState.Minimized)
                {
                    SearchForm.WindowState = FormWindowState.Normal;
                }
                SearchForm.Focus();
            }
            //ToolbarSearchWindowButton.Checked = true;
        }
        public void OnSearchFormClosed()
        {
            SearchForm = null;
            //ToolbarSearchWindowButton.Checked = false;
        }

        public void ShowModel(string name)
        {
            ViewModeComboBox.Text = "Model view";
            ModelComboBox.Text = name;
            modelname = name;
        }
        public void GoToEntity(YmapEntityDef entity)
        {
            if (entity == null) return;
            ViewModeComboBox.Text = "World view";
            GoToPosition(entity.Position);
            SelectEntity(entity);
        }


        private void LoadWorld()
        {

            UpdateStatus("Loading timecycles...");
            timecycle.Init(gameFileCache, UpdateStatus);
            timecycle.SetTime(Renderer.timeofday);

            UpdateStatus("Loading materials...");
            BoundsMaterialTypes.Init(gameFileCache);

            UpdateStatus("Loading weather...");
            weather.Init(gameFileCache, UpdateStatus, timecycle);
            UpdateWeatherTypesComboBox(weather);

            UpdateStatus("Loading clouds...");
            clouds.Init(gameFileCache, UpdateStatus, weather);
            UpdateCloudTypesComboBox(clouds);

            UpdateStatus("Loading water...");
            water.Init(gameFileCache, UpdateStatus);

            UpdateStatus("Loading trains...");
            trains.Init(gameFileCache, UpdateStatus);

            UpdateStatus("Loading scenarios...");
            scenarios.Init(gameFileCache, UpdateStatus, timecycle);

            UpdateStatus("Loading popzones...");
            popzones.Init(gameFileCache, UpdateStatus);

            UpdateStatus("Loading audio zones...");
            audiozones.Init(gameFileCache, UpdateStatus);

            UpdateStatus("Loading world...");
            space.Init(gameFileCache, UpdateStatus);

            UpdateStatus("World loaded");

        }



        private void SetDlcLevel(string dlc, bool enable)
        {
            if (!initialised) return;
            Cursor = Cursors.WaitCursor;
            Task.Run(() =>
            {
                lock (Renderer.RenderSyncRoot)
                {
                    if (gameFileCache.SetDlcLevel(dlc, enable))
                    {
                        LoadWorld();
                    }
                }
                Invoke(new Action(()=> {
                    Cursor = Cursors.Default;
                }));
            });
        }

        private void SetModsEnabled(bool enable)
        {
            if (!initialised) return;
            Cursor = Cursors.WaitCursor;
            Task.Run(() =>
            {
                lock (Renderer.RenderSyncRoot)
                {
                    if (gameFileCache.SetModsEnabled(enable))
                    {
                        UpdateDlcListComboBox(gameFileCache.DlcNameList);

                        LoadWorld();
                    }
                }
                Invoke(new Action(() => {
                    Cursor = Cursors.Default;
                }));
            });
        }


        private void ContentThread()
        {
            //main content loading thread.
            running = true;

            UpdateStatus("Scanning...");

            try
            {
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);

                //save the key for later if it's not saved already. not really ideal to have this in this thread
                if (string.IsNullOrEmpty(Settings.Default.Key) && (GTA5Keys.PC_AES_KEY != null))
                {
                    Settings.Default.Key = Convert.ToBase64String(GTA5Keys.PC_AES_KEY);
                    Settings.Default.Save();
                }
            }
            catch
            {
                MessageBox.Show("Keys not found! This shouldn't happen.");
                Close();
                return;
            }

            gameFileCache.Init(UpdateStatus, LogError);

            UpdateDlcListComboBox(gameFileCache.DlcNameList);

            EnableCacheDependentUI();



            LoadWorld();



            initialised = true;

            EnableDLCModsUI();


            while (formopen && !IsDisposed) //main asset loop
            {
                bool fcItemsPending = gameFileCache.ContentThreadProc();

                bool rcItemsPending = Renderer.ContentThreadProc();

                if (!(fcItemsPending || rcItemsPending))
                {
                    Thread.Sleep(1); //sleep if there's nothing to do
                }
            }

            gameFileCache.Clear();

            running = false;
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
        private void UpdateMousedLabel(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateMousedLabel(text); }));
                }
                else
                {
                    MousedLabel.Text = text;
                }
            }
            catch { }
        }
        private void UpdateWeatherTypesComboBox(Weather weather)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateWeatherTypesComboBox(weather); }));
                }
                else
                {

                    //MessageBox.Show("sky_hdr: " + weather.GetDynamicValue("sky_hdr").ToString() + "\n" +
                    //                "Timecycle index: " + weather.Timecycle.CurrentSampleIndex + "\n" +
                    //                "Timecycle blend: " + weather.Timecycle.CurrentSampleBlend + "\n");

                    WeatherComboBox.Items.Clear();
                    foreach (string wt in weather.WeatherTypes.Keys)
                    {
                        WeatherComboBox.Items.Add(wt);
                    }
                    WeatherComboBox.SelectedIndex = 0;
                    WeatherRegionComboBox.SelectedIndex = 0;
                }
            }
            catch { }
        }
        private void UpdateCloudTypesComboBox(Clouds clouds)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateCloudTypesComboBox(clouds); }));
                }
                else
                {
                    CloudsComboBox.Items.Clear();
                    foreach (var frag in clouds.HatManager.CloudHatFrags)
                    {
                        CloudsComboBox.Items.Add(frag.Name);
                    }
                    CloudsComboBox.SelectedIndex = Math.Max(CloudsComboBox.FindString(Renderer.individualcloudfrag), 0);


                    CloudParamComboBox.Items.Clear();
                    foreach (var setting in clouds.AnimSettings.Values)
                    {
                        CloudParamComboBox.Items.Add(setting);
                    }
                    CloudParamComboBox.SelectedIndex = 0;
                }
            }
            catch { }
        }
        private void UpdateDlcListComboBox(List<string> dlcnames)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateDlcListComboBox(dlcnames); }));
                }
                else
                {
                    DlcLevelComboBox.Items.Clear();
                    foreach (var dlcname in dlcnames)
                    {
                        DlcLevelComboBox.Items.Add(dlcname);
                    }
                    if (string.IsNullOrEmpty(gameFileCache.SelectedDlc))
                    {
                        DlcLevelComboBox.SelectedIndex = dlcnames.Count - 1;
                    }
                    else
                    {
                        int idx = DlcLevelComboBox.FindString(gameFileCache.SelectedDlc);
                        DlcLevelComboBox.SelectedIndex = (idx > 0) ? idx : (dlcnames.Count - 1);
                    }
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
                    //MessageBox.Show(text);
                }
            }
            catch { }
        }




        private void UpdateMarkerSelectionPanelInvoke()
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateMarkerSelectionPanel(); }));
                }
                else
                {
                    UpdateMarkerSelectionPanel();
                }
            }
            catch { }
        }
        private void UpdateMarkerSelectionPanel()
        {
            if (!SelectedMarkerPanel.Visible) return;
            if (SelectedMarker == null)
            {
                SelectedMarkerPanel.Visible = false;
                return;
            }

            int ox = -90; //screen offset from actual marker world pos
            int oy = -76;

            float spx = ((SelectedMarker.ScreenPos.X * 0.5f) + 0.5f) * camera.Width;
            float spy = ((SelectedMarker.ScreenPos.Y * -0.5f) + 0.5f) * camera.Height;

            int px = (int)Math.Round(spx, MidpointRounding.AwayFromZero) + ox;
            int py = (int)Math.Round(spy, MidpointRounding.AwayFromZero) + oy;

            int sx = SelectedMarkerPanel.Width;
            int sy = SelectedMarkerPanel.Height;

            SelectedMarkerPanel.SetBounds(px, py, sx, sy);
        }
        private void ShowMarkerSelectionInfo(MapMarker marker)
        {
            SelectedMarkerNameTextBox.Text = SelectedMarker.Name;
            SelectedMarkerPositionTextBox.Text = SelectedMarker.Get3DWorldPosString();
            UpdateMarkerSelectionPanel();
            SelectedMarkerPanel.Visible = true;
        }
        private void HideMarkerSelectionInfo()
        {
            SelectedMarkerPanel.Visible = false;
        }

        private MapMarker FindMousedMarker()
        {
            lock (markersortedsyncroot)
            {
                float mx = MouseLastPoint.X;
                float my = MouseLastPoint.Y;

                if (ShowLocatorCheckBox.Checked)
                {
                    if (IsMarkerUnderPoint(LocatorMarker, mx, my))
                    {
                        return LocatorMarker;
                    }
                }

                //search backwards through the render markers (front to back)
                for (int i = SortedMarkers.Count - 1; i >= 0; i--)
                {
                    MapMarker m = SortedMarkers[i];
                    if (IsMarkerUnderPoint(m, mx, my))
                    {
                        return m;
                    }
                }
            }
            return null;
        }
        private bool IsMarkerUnderPoint(MapMarker marker, float x, float y)
        {
            if (marker.ScreenPos.Z <= 0.0f) return false; //behind the camera...
            float dx = x - ((marker.ScreenPos.X * 0.5f) + 0.5f) * camera.Width;
            float dy = y - ((marker.ScreenPos.Y * -0.5f) + 0.5f) * camera.Height;
            float mcx = marker.Icon.Center.X;
            float mcy = marker.Icon.Center.Y;
            bool bx = ((dx >= -mcx) && (dx <= mcx));
            bool by = ((dy <= 0.0f) && (dy >= -mcy));
            return (bx && by);
        }

        private void GoToMarker(MapMarker m)
        {
            //////adjust the target to account for the main panel...
            ////Vector3 view = m.TexturePos;
            ////view.X += ((float)(MainPanel.Width + 4) * 0.5f) / CurrentZoom;
            ////TargetViewCenter = view;

            camera.FollowEntity.Position = m.WorldPos;

        }
        public void GoToPosition(Vector3 p)
        {
            camera.FollowEntity.Position = p;
        }

        private MapMarker AddMarker(Vector3 pos, string name, bool addtotxtbox = false)
        {
            string str = pos.X.ToString() + ", " + pos.Y.ToString() + ", " + pos.Z.ToString();
            if (!string.IsNullOrEmpty(name))
            {
                str += ", " + name;
            }
            if (addtotxtbox)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(MultiFindTextBox.Text);
                if ((sb.Length > 0) && (!MultiFindTextBox.Text.EndsWith("\n")))
                {
                    sb.AppendLine();
                }
                sb.AppendLine(str);
                MultiFindTextBox.Text = sb.ToString();
            }

            return AddMarker(str);
        }
        private MapMarker AddMarker(string markerstr)
        {
            lock (markersyncroot)
            {
                MapMarker m = new MapMarker();
                m.Parse(markerstr.Trim());
                m.Icon = MarkerIcon;

                Markers.Add(m);

                //ListViewItem lvi = new ListViewItem(new string[] { m.Name, m.WorldPos.X.ToString(), m.WorldPos.Y.ToString(), m.WorldPos.Z.ToString() });
                //lvi.Tag = m;
                //MarkersListView.Items.Add(lvi);

                return m;
            }
        }
        private void AddDefaultMarkers()
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("1972.606, 3817.044, 0.0, Trevor Bed");
            //sb.AppendLine("94.5723, -1290.082, 0.0, Strip Club Bed");
            //sb.AppendLine("-1151.746, -1518.136, 0.0, Trevor City Bed");
            //sb.AppendLine("-1154.11, -2715.203, 0.0, Flight School");
            //sb.AppendLine("-1370.625, 56.1227, 52.82404, Golf");
            //sb.AppendLine("-1109.213, 4914.744, 0.0, Altruist Cult");
            //sb.AppendLine("-1633.087, 4736.784, 0.0, Deal Gone Wrong");
            sb.AppendLine("-2052, 3237, 1449.036, Zancudo UFO");
            sb.AppendLine("2490, 3777, 2400, Hippy UFO");
            sb.AppendLine("2577.396, 3301.573, 52.52076, Sand glyph");
            sb.AppendLine("-804.8452, 176.4936, 75.40561, bh1_48_michaels");
            sb.AppendLine("-5.757423, 529.674, 171.1747, ch2_05c_b1");
            sb.AppendLine("1971.208, 3818.237, 33.46632, cs4_10_trailer003b");
            sb.AppendLine("760.4618, 7392.803, -126.0774, cs1_09_sea_ufo");
            sb.AppendLine("501.4398, 5603.96, 795.9738, cs1_10_redeye");
            sb.AppendLine("51.3909, 5957.7568, 209.614, cs1_10_clue_moon02");
            sb.AppendLine("400.7087, 5714.5645, 605.0978, cs1_10_clue_rain01");
            sb.AppendLine("703.442, 6329.8936, 76.4973, cs1_10_clue_rain02");
            sb.AppendLine("228.7844, 5370.585, 577.2613, cs1_10_clue_moon01");
            sb.AppendLine("366.4871, 5518.0742, 704.3185, cs1_10_clue_mountain01");
            sb.AppendLine("41.64376, -779.9391, 832.4024, hw1_22_shipint");
            sb.AppendLine("-1255.392, 6795.764, -181.9927, cs1_08_sea_base");
            sb.AppendLine("4285.036, 2967.639, -184.1908, cs5_1_sea_hatch");
            sb.AppendLine("3041.498, 5584.321, 196.4748, cs2_08_generic02");
            sb.AppendLine("3406.483, 5498.655, 23.50577, cs2_08_generic01a");
            sb.AppendLine("1507.081, 6565.075, 8.681923, cs1_09_props_elec_spider1");
            sb.AppendLine("455.7852, 5586.104, 779.4382, cs1_10_elec_spider_spline052b");
            sb.AppendLine("3861.661, -4959.252, 91.49448, plg_01_nico_new");
            sb.AppendLine("-1689.308, 2174.457, 107.2592, ch1_09b_vinesleaf_28");
            sb.AppendLine("440.8488, 5810.079, 563.4703, Cock face");
            sb.AppendLine("-3955.667, -4675.212, -1274.563, Interesting...");
            sb.AppendLine("4512.627, 2623.241, 2500, Interesting...");
            sb.AppendLine("228.6058, -992.0537, -100, v_garagel");

            MultiFindTextBox.Text = sb.ToString();
            string[] lines = MultiFindTextBox.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                AddMarker(line);
            }

        }






        private void LoadSettings()
        {
            var s = Settings.Default;
            WindowState = s.WindowMaximized ? FormWindowState.Maximized : WindowState;
            FullScreenCheckBox.Checked = s.FullScreen;
            WireframeCheckBox.Checked = s.Wireframe;
            HDRRenderingCheckBox.Checked = s.HDR;
            ShadowsCheckBox.Checked = s.Shadows;
            SkydomeCheckBox.Checked = s.Skydome;
            GrassCheckBox.Checked = s.Grass;
            TimedEntitiesCheckBox.Checked = s.ShowTimedEntities;
            CollisionMeshesCheckBox.Checked = s.ShowCollisionMeshes;
            CollisionMeshRangeTrackBar.Value = s.CollisionMeshRange;
            DynamicLODCheckBox.Checked = s.DynamicLOD;
            DetailTrackBar.Value = s.DetailDist;
            WaitForChildrenCheckBox.Checked = s.WaitForChildren;
            RenderModeComboBox.SelectedIndex = Math.Max(RenderModeComboBox.FindString(s.RenderMode), 0);
            TextureSamplerComboBox.SelectedIndex = Math.Max(TextureSamplerComboBox.FindString(s.RenderTextureSampler), 0);
            TextureCoordsComboBox.SelectedIndex = Math.Max(TextureCoordsComboBox.FindString(s.RenderTextureSamplerCoord), 0);
            MarkerStyleComboBox.SelectedIndex = Math.Max(MarkerStyleComboBox.FindString(s.MarkerStyle), 0);
            LocatorStyleComboBox.SelectedIndex = Math.Max(LocatorStyleComboBox.FindString(s.LocatorStyle), 0);
            MarkerDepthClipCheckBox.Checked = s.MarkerDepthClip;
            AnisotropicFilteringCheckBox.Checked = s.AnisotropicFiltering;
            BoundsStyleComboBox.SelectedIndex = Math.Max(BoundsStyleComboBox.FindString(s.BoundsStyle), 0);
            BoundsDepthClipCheckBox.Checked = s.BoundsDepthClip;
            BoundsRangeTrackBar.Value = s.BoundsRange;
            ErrorConsoleCheckBox.Checked = s.ShowErrorConsole;
            StatusBarCheckBox.Checked = s.ShowStatusBar;

            EnableModsCheckBox.Checked = s.EnableMods;
            DlcLevelComboBox.Text = s.DLC;
            gameFileCache.SelectedDlc = s.DLC;
            EnableDlcCheckBox.Checked = !string.IsNullOrEmpty(s.DLC);
        }
        private void SaveSettings()
        {
            var s = Settings.Default;
            s.WindowMaximized = (WindowState == FormWindowState.Maximized);
            s.FullScreen = FullScreenCheckBox.Checked;
            s.Wireframe = WireframeCheckBox.Checked;
            s.HDR = HDRRenderingCheckBox.Checked;
            s.Shadows = ShadowsCheckBox.Checked;
            s.Skydome = SkydomeCheckBox.Checked;
            s.Grass = GrassCheckBox.Checked;
            s.ShowTimedEntities = TimedEntitiesCheckBox.Checked;
            s.ShowCollisionMeshes = CollisionMeshesCheckBox.Checked;
            s.CollisionMeshRange = CollisionMeshRangeTrackBar.Value;
            s.DynamicLOD = DynamicLODCheckBox.Checked;
            s.DetailDist = DetailTrackBar.Value;
            s.WaitForChildren = WaitForChildrenCheckBox.Checked;
            s.RenderMode = RenderModeComboBox.Text;
            s.RenderTextureSampler = TextureSamplerComboBox.Text;
            s.RenderTextureSamplerCoord = TextureCoordsComboBox.Text;
            s.MarkerStyle = MarkerStyleComboBox.Text;
            s.LocatorStyle = LocatorStyleComboBox.Text;
            s.MarkerDepthClip = MarkerDepthClipCheckBox.Checked;
            s.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
            s.BoundsStyle = BoundsStyleComboBox.Text;
            s.BoundsDepthClip = BoundsDepthClipCheckBox.Checked;
            s.BoundsRange = BoundsRangeTrackBar.Value;
            s.ShowErrorConsole = ErrorConsoleCheckBox.Checked;
            s.ShowStatusBar = StatusBarCheckBox.Checked;

            //additional settings from gamefilecache...
            s.EnableMods = gameFileCache.EnableMods;
            s.DLC = gameFileCache.EnableDlc ? gameFileCache.SelectedDlc : "";

            s.Save();
        }

        private void ShowSettingsForm(string tab = "")
        {
            if (SettingsForm == null)
            {
                SettingsForm = new SettingsForm(this);
                SettingsForm.Show(this);
            }
            else
            {
                if (SettingsForm.WindowState == FormWindowState.Minimized)
                {
                    SettingsForm.WindowState = FormWindowState.Normal;
                }
                SettingsForm.Focus();
            }
            if (!string.IsNullOrEmpty(tab))
            {
                SettingsForm.SelectTab(tab);
            }
        }
        public void OnSettingsFormClosed()
        {
            //called by the SettingsForm when it's closed.
            SettingsForm = null;
        }





        private void MarkUndoStart(Widget w)
        {
            bool canundo = false;
            if (SelectedItem.MultipleSelection) canundo = true;
            if (SelectedItem.EntityDef != null) canundo = true;
            if (SelectedItem.CarGenerator != null) canundo = true;
            if (SelectedItem.PathNode != null) canundo = true;
            //if (SelectedItem.NavPoly != null) hasval = true;
            if (SelectedItem.TrainTrackNode != null) canundo = true;
            if (SelectedItem.ScenarioNode != null) canundo = true;
            if (SelectedItem.Audio != null) canundo = true;
            if (!canundo) return;
            if (Widget is TransformWidget)
            {
                UndoStartPosition = Widget.Position;
                UndoStartRotation = Widget.Rotation;
                UndoStartScale = Widget.Scale;
            }
        }
        private void MarkUndoEnd(Widget w)
        {
            bool canundo = false;
            if (SelectedItem.MultipleSelection) canundo = true;
            if (SelectedItem.EntityDef != null) canundo = true;
            if (SelectedItem.CarGenerator != null) canundo = true;
            if (SelectedItem.PathNode != null) canundo = true;
            //if (SelectedItem.NavPoly != null) hasval = true;
            if (SelectedItem.TrainTrackNode != null) canundo = true;
            if (SelectedItem.ScenarioNode != null) canundo = true;
            if (SelectedItem.Audio != null) canundo = true;
            if (!canundo) return;
            var ent = SelectedItem.EntityDef;
            var cargen = SelectedItem.CarGenerator;
            var pathnode = SelectedItem.PathNode;
            var trainnode = SelectedItem.TrainTrackNode;
            var scenarionode = SelectedItem.ScenarioNode;
            var audio = SelectedItem.Audio;
            TransformWidget tw = Widget as TransformWidget;
            UndoStep s = null;
            if (tw != null)
            {
                if (SelectedItem.MultipleSelection)
                {
                    switch (tw.Mode)
                    {
                        case WidgetMode.Position: s = new MultiPositionUndoStep(SelectedItem, SelectedItems.ToArray(), UndoStartPosition, this); break;
                    }
                }
                else if (ent != null)
                {
                    if (EditEntityPivot)
                    {
                        switch (tw.Mode)
                        {
                            case WidgetMode.Position: s = new EntityPivotPositionUndoStep(ent, UndoStartPosition); break;
                            case WidgetMode.Rotation: s = new EntityPivotRotationUndoStep(ent, UndoStartRotation); break;
                        }
                    }
                    else
                    {
                        switch (tw.Mode)
                        {
                            case WidgetMode.Position: s = new EntityPositionUndoStep(ent, UndoStartPosition); break;
                            case WidgetMode.Rotation: s = new EntityRotationUndoStep(ent, UndoStartRotation); break;
                            case WidgetMode.Scale: s = new EntityScaleUndoStep(ent, UndoStartScale); break;
                        }
                    }
                }
                else if (cargen != null)
                {
                    switch (tw.Mode)
                    {
                        case WidgetMode.Position: s = new CarGenPositionUndoStep(cargen, UndoStartPosition); break;
                        case WidgetMode.Rotation: s = new CarGenRotationUndoStep(cargen, UndoStartRotation); break;
                        case WidgetMode.Scale: s = new CarGenScaleUndoStep(cargen, UndoStartScale); break;
                    }
                }
                else if (pathnode != null)
                {
                    switch (tw.Mode)
                    {
                        case WidgetMode.Position: s = new PathNodePositionUndoStep(pathnode, UndoStartPosition, this); break;
                    }
                }
                else if (trainnode != null)
                {
                    switch (tw.Mode)
                    {
                        case WidgetMode.Position: s = new TrainTrackNodePositionUndoStep(trainnode, UndoStartPosition, this); break;
                    }
                }
                else if (scenarionode != null)
                {
                    switch (tw.Mode)
                    {
                        case WidgetMode.Position: s = new ScenarioNodePositionUndoStep(scenarionode, UndoStartPosition, this); break;
                        case WidgetMode.Rotation: s = new ScenarioNodeRotationUndoStep(scenarionode, UndoStartRotation, this); break;
                    }
                }
                else if (audio != null)
                {
                    switch (tw.Mode)
                    {
                        case WidgetMode.Position: s = new AudioPositionUndoStep(audio, UndoStartPosition); break;
                        case WidgetMode.Rotation: s = new AudioRotationUndoStep(audio, UndoStartRotation); break;
                    }
                }
            }
            if (s != null)
            {
                RedoSteps.Clear();
                UndoSteps.Push(s);
                UpdateUndoUI();
            }
        }
        private void Undo()
        {
            if (UndoSteps.Count == 0) return;
            var s = UndoSteps.Pop();
            RedoSteps.Push(s);

            s.Undo(this, ref SelectedItem);

            if (ProjectForm != null)
            {
                ProjectForm.OnWorldSelectionModified(SelectedItem, SelectedItems);
            }

            UpdateUndoUI();
        }
        private void Redo()
        {
            if (RedoSteps.Count == 0) return;
            var s = RedoSteps.Pop();
            UndoSteps.Push(s);

            s.Redo(this, ref SelectedItem);

            if (ProjectForm != null)
            {
                ProjectForm.OnWorldSelectionModified(SelectedItem, SelectedItems);
            }

            UpdateUndoUI();
        }
        private void UpdateUndoUI()
        {
            ToolbarUndoButton.DropDownItems.Clear();
            ToolbarRedoButton.DropDownItems.Clear();
            int i = 0;
            foreach (var step in UndoSteps)
            {
                var button = ToolbarUndoButton.DropDownItems.Add(step.ToString());
                button.Tag = step;
                button.Click += ToolbarUndoListButton_Click;
                i++;
                if (i >= 10) break;
            }
            i = 0;
            foreach (var step in RedoSteps)
            {
                var button = ToolbarRedoButton.DropDownItems.Add(step.ToString());
                button.Tag = step;
                button.Click += ToolbarRedoListButton_Click;
                i++;
                if (i >= 10) break;
            }
            ToolbarUndoButton.Enabled = (UndoSteps.Count > 0);
            ToolbarRedoButton.Enabled = (RedoSteps.Count > 0);
        }



        private void EnableCacheDependentUI()
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { EnableCacheDependentUI(); }));
                }
                else
                {
                    ToolbarNewButton.Enabled = true;
                    ToolbarOpenButton.Enabled = true;
                    ToolbarProjectWindowButton.Enabled = true;
                    ToolsMenuProjectWindow.Enabled = true;
                    ToolsMenuBinarySearch.Enabled = true;
                    ToolsMenuJenkInd.Enabled = true;
                }
            }
            catch { }
        }
        private void EnableDLCModsUI()
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { EnableDLCModsUI(); }));
                }
                else
                {
                    EnableDlcCheckBox.Enabled = true;
                    EnableModsCheckBox.Enabled = true;
                    DlcLevelComboBox.Enabled = true;
                }
            }
            catch { }
        }


        public void SetCurrentSaveItem(string filename)
        {
            bool enable = !string.IsNullOrEmpty(filename);
            ToolbarSaveButton.ToolTipText = enable ? ("Save " + filename) : "Save";
            ToolbarSaveButton.Enabled = enable;
            ToolbarSaveAllButton.Enabled = enable;
        }
        public void EnableYmapUI(bool enable, string filename)
        {
            string type = "entity";
            switch (SelectionMode)
            {
                case MapSelectionMode.CarGenerator: type = "car generator"; break;
            }

            ToolbarAddItemButton.ToolTipText = "Add " + type + (enable ? (" to " + filename) : "");
            ToolbarAddItemButton.Enabled = enable;
            //ToolbarDeleteEntityButton.Enabled = enable;
            ToolbarPasteButton.Enabled = (CopiedEntity != null) && enable;
        }
        public void EnableYndUI(bool enable, string filename)
        {
            string type = "node";
            switch (SelectionMode)
            {
                case MapSelectionMode.Path: type = "node"; break;
            }

            if (enable) //only do something if a ynd is selected - EnableYmapUI will handle the no selection case.. 
            {
                ToolbarAddItemButton.ToolTipText = "Add " + type + (enable ? (" to " + filename) : "");
                ToolbarAddItemButton.Enabled = enable;
                //ToolbarDeleteEntityButton.Enabled = enable;
                ToolbarPasteButton.Enabled = (CopiedPathNode != null) && enable;
            }
        }
        public void EnableYnvUI(bool enable, string filename)
        {
            string type = "polygon";
            switch (SelectionMode)
            {
                case MapSelectionMode.NavMesh: type = "polygon"; break;
            }

            if (enable) //only do something if a ynv is selected - EnableYmapUI will handle the no selection case.. 
            {
                ToolbarAddItemButton.ToolTipText = "Add " + type + (enable ? (" to " + filename) : "");
                ToolbarAddItemButton.Enabled = enable;
                //ToolbarDeleteEntityButton.Enabled = enable;
                ToolbarPasteButton.Enabled = (CopiedNavPoly != null) && enable;
            }
        }
        public void EnableTrainsUI(bool enable, string filename)
        {
            string type = "node";
            switch (SelectionMode)
            {
                case MapSelectionMode.TrainTrack: type = "node"; break;
            }

            if (enable) //only do something if a track is selected - EnableYmapUI will handle the no selection case.. 
            {
                ToolbarAddItemButton.ToolTipText = "Add " + type + (enable ? (" to " + filename) : "");
                ToolbarAddItemButton.Enabled = enable;
                //ToolbarDeleteEntityButton.Enabled = enable;
                ToolbarPasteButton.Enabled = false;// (CopiedTrainNode != null) && enable;
            }
        }
        public void EnableScenarioUI(bool enable, string filename)
        {
            string type = "scenario point";
            switch (SelectionMode)
            {
                case MapSelectionMode.Scenario: type = "scenario point"; break;
            }

            if (enable) //only do something if a scenario is selected - EnableYmapUI will handle the no selection case.. 
            {
                ToolbarAddItemButton.ToolTipText = "Add " + type + (enable ? (" to " + filename) : "");
                ToolbarAddItemButton.Enabled = enable;
                //ToolbarDeleteEntityButton.Enabled = enable;
                ToolbarPasteButton.Enabled = (CopiedScenarioNode != null) && enable;
            }
        }


        private void New()
        {
            ShowProjectForm();

            if (ProjectForm.IsProjectLoaded)
            {
                ProjectForm.NewYmap();
            }
            else
            {
                ProjectForm.NewProject();
            }
        }
        private void NewProject()
        {
            ShowProjectForm();
            ProjectForm.NewProject();
        }
        private void NewYmap()
        {
            ShowProjectForm();
            ProjectForm.NewYmap();
        }
        private void NewYnd()
        {
            ShowProjectForm();
            ProjectForm.NewYnd();
        }
        private void NewYnv()
        {
            ShowProjectForm();
            ProjectForm.NewYnv();
        }
        private void NewTrainTrack()
        {
            ShowProjectForm();
            ProjectForm.NewTrainTrack();
        }
        private void NewScenario()
        {
            ShowProjectForm();
            ProjectForm.NewScenario();
        }
        private void Open()
        {
            ShowProjectForm();

            if (ProjectForm.IsProjectLoaded)
            {
                ProjectForm.OpenYmap();
            }
            else
            {
                ProjectForm.OpenProject();
            }
        }
        private void OpenProject()
        {
            ShowProjectForm();
            ProjectForm.OpenProject();
        }
        private void OpenYmap()
        {
            ShowProjectForm();
            ProjectForm.OpenYmap();
        }
        private void OpenYnd()
        {
            ShowProjectForm();
            ProjectForm.OpenYnd();
        }
        private void OpenYnv()
        {
            ShowProjectForm();
            ProjectForm.OpenYnv();
        }
        private void OpenTrainTrack()
        {
            ShowProjectForm();
            ProjectForm.OpenTrainTrack();
        }
        private void OpenScenario()
        {
            ShowProjectForm();
            ProjectForm.OpenScenario();
        }
        private void Save()
        {
            if (ProjectForm == null) return;
            ProjectForm.Save();
        }
        private void SaveAll()
        {
            if (ProjectForm == null) return;
            ProjectForm.SaveAll();
        }


        private void AddItem()
        {
            switch (SelectionMode)
            {
                case MapSelectionMode.Entity: AddEntity(); break;
                case MapSelectionMode.CarGenerator: AddCarGen(); break;
                case MapSelectionMode.Path: AddPathNode(); break;
                case MapSelectionMode.NavMesh: AddNavPoly(); break;
                case MapSelectionMode.TrainTrack: AddTrainNode(); break;
                case MapSelectionMode.Scenario: AddScenarioNode(); break;
            }
        }
        private void DeleteItem()
        {
            if (SelectedItem.EntityDef != null) DeleteEntity();
            else if (SelectedItem.CarGenerator != null) DeleteCarGen();
            else if (SelectedItem.PathNode != null) DeletePathNode();
            else if (SelectedItem.NavPoly != null) DeleteNavPoly();
            else if (SelectedItem.TrainTrackNode != null) DeleteTrainNode();
            else if (SelectedItem.ScenarioNode != null) DeleteScenarioNode();
        }
        private void CopyItem()
        {
            if (SelectedItem.EntityDef != null) CopyEntity();
            else if (SelectedItem.CarGenerator != null) CopyCarGen();
            else if (SelectedItem.PathNode != null) CopyPathNode();
            else if (SelectedItem.NavPoly != null) CopyNavPoly();
            else if (SelectedItem.TrainTrackNode != null) CopyTrainNode();
            else if (SelectedItem.ScenarioNode != null) CopyScenarioNode();
        }
        private void PasteItem()
        {
            if (CopiedEntity != null) PasteEntity();
            else if (CopiedCarGen != null) PasteCarGen();
            else if (CopiedPathNode != null) PastePathNode();
            else if (CopiedNavPoly != null) PasteNavPoly();
            else if (CopiedTrainNode != null) PasteTrainNode();
            else if (CopiedScenarioNode != null) PasteScenarioNode();
        }
        private void CloneItem()
        {
            if (SelectedItem.EntityDef != null) CloneEntity();
            else if (SelectedItem.CarGenerator != null) CloneCarGen();
            else if (SelectedItem.PathNode != null) ClonePathNode();
            else if (SelectedItem.NavPoly != null) CloneNavPoly();
            else if (SelectedItem.TrainTrackNode != null) CloneTrainNode();
            else if (SelectedItem.ScenarioNode != null) CloneScenarioNode();
        }

        private void AddEntity()
        {
            if (ProjectForm == null) return;
            ProjectForm.NewEntity();
        }
        private void DeleteEntity()
        {
            var ent = SelectedItem.EntityDef;
            if (ent == null) return;

            if ((ProjectForm != null) && (ProjectForm.IsCurrentEntity(ent)))
            {
                if (!ProjectForm.DeleteEntity())
                {
                    //MessageBox.Show("Unable to delete this entity from the current project. Make sure the entity's ymap exists in the current project.");
                }
                else
                {
                    SelectItem(null);
                }
            }
            else
            {
                //project not open, or entity not selected there, just remove the entity from the ymap...
                var ymap = ent.Ymap;
                if (ymap == null)
                {
                    MessageBox.Show("Sorry, deleting interior entities is not currently supported.");
                }
                else if (!ymap.RemoveEntity(ent))
                {
                    MessageBox.Show("Unable to remove entity.");
                }
                else
                {
                    SelectItem(null);
                }
            }
        }
        private void CopyEntity()
        {
            CopiedEntity = SelectedItem.EntityDef;
            ToolbarPasteButton.Enabled = (CopiedEntity != null) && ToolbarAddItemButton.Enabled;
        }
        private void PasteEntity()
        {
            if (CopiedEntity == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewEntity(CopiedEntity);
        }
        private void CloneEntity()
        {
            if (SelectedItem.EntityDef == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewEntity(SelectedItem.EntityDef, true);
        }

        private void AddCarGen()
        {
            if (ProjectForm == null) return;
            ProjectForm.NewCarGen();
        }
        private void DeleteCarGen()
        {
            var cargen = SelectedItem.CarGenerator;
            if (cargen == null) return;

            if ((ProjectForm != null) && (ProjectForm.IsCurrentCarGen(cargen)))
            {
                if (!ProjectForm.DeleteCarGen())
                {
                    //MessageBox.Show("Unable to delete this car generator from the current project. Make sure the car generator's ymap exists in the current project.");
                }
                else
                {
                    SelectItem(null);
                }
            }
            else
            {
                //project not open, or cargen not selected there, just remove the cargen from the ymap...
                var ymap = cargen.Ymap;
                if (!ymap.RemoveCarGen(cargen))
                {
                    MessageBox.Show("Unable to remove car generator.");
                }
                else
                {
                    SelectItem(null);
                }
            }
        }
        private void CopyCarGen()
        {
            CopiedCarGen = SelectedItem.CarGenerator;
            ToolbarPasteButton.Enabled = (CopiedCarGen != null) && ToolbarAddItemButton.Enabled;
        }
        private void PasteCarGen()
        {
            if (CopiedCarGen == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewCarGen(CopiedCarGen);
        }
        private void CloneCarGen()
        {
            if (SelectedItem.CarGenerator == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewCarGen(SelectedItem.CarGenerator, true);
        }

        private void AddPathNode()
        {
            if (ProjectForm == null) return;
            ProjectForm.NewPathNode();
        }
        private void DeletePathNode()
        {
            var pathnode = SelectedItem.PathNode;
            if (pathnode == null) return;

            if ((ProjectForm != null) && (ProjectForm.IsCurrentPathNode(pathnode)))
            {
                if (!ProjectForm.DeletePathNode())
                {
                    //MessageBox.Show("Unable to delete this path node from the current project. Make sure the path node's ynd exists in the current project.");
                }
                else
                {
                    SelectItem(null);
                }
            }
            else
            {
                //project not open, or cargen not selected there, just remove the cargen from the ymap...
                var ynd = pathnode.Ynd;
                if (!ynd.RemoveNode(pathnode))
                {
                    MessageBox.Show("Unable to remove path node.");
                }
                else
                {
                    UpdatePathNodeGraphics(pathnode, false);
                    SelectItem(null);
                }
            }
        }
        private void CopyPathNode()
        {
            CopiedPathNode = SelectedItem.PathNode;
            ToolbarPasteButton.Enabled = (CopiedPathNode != null) && ToolbarAddItemButton.Enabled;
        }
        private void PastePathNode()
        {
            if (CopiedPathNode == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewPathNode(CopiedPathNode);
        }
        private void ClonePathNode()
        {
            if (SelectedItem.PathNode == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewPathNode(SelectedItem.PathNode, true);
        }

        private void AddNavPoly()
        {
            if (ProjectForm == null) return;
            ProjectForm.NewNavPoly();
        }
        private void DeleteNavPoly()
        {
            var navpoly = SelectedItem.NavPoly;
            if (navpoly == null) return;

            if ((ProjectForm != null) && (ProjectForm.IsCurrentNavPoly(navpoly)))
            {
                if (!ProjectForm.DeleteNavPoly())
                {
                    //MessageBox.Show("Unable to delete this nav poly from the current project. Make sure the nav poly's ynv exists in the current project.");
                }
                else
                {
                    SelectItem(null);
                }
            }
            else
            {
                //project not open, or nav poly not selected there, just remove the poly from the ynv...
                var ynv = navpoly.Ynv;
                if (!ynv.RemovePoly(navpoly))
                {
                    MessageBox.Show("Unable to remove nav poly. NavMesh editing TODO!");
                }
                else
                {
                    UpdateNavPolyGraphics(navpoly, false);
                    SelectItem(null);
                }
            }
        }
        private void CopyNavPoly()
        {
            CopiedNavPoly = SelectedItem.NavPoly;
            ToolbarPasteButton.Enabled = (CopiedNavPoly != null) && ToolbarAddItemButton.Enabled;
        }
        private void PasteNavPoly()
        {
            if (CopiedNavPoly == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewNavPoly(CopiedNavPoly);
        }
        private void CloneNavPoly()
        {
            if (SelectedItem.NavPoly == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewNavPoly(SelectedItem.NavPoly, true);
        }

        private void AddTrainNode()
        {
            if (ProjectForm == null) return;
            ProjectForm.NewTrainNode();
        }
        private void DeleteTrainNode()
        {
            var trainnode = SelectedItem.TrainTrackNode;
            if (trainnode == null) return;

            if ((ProjectForm != null) && (ProjectForm.IsCurrentTrainNode(trainnode)))
            {
                if (!ProjectForm.DeleteTrainNode())
                {
                    //MessageBox.Show("Unable to delete this train track node from the current project. Make sure the path train track file exists in the current project.");
                }
                else
                {
                    SelectItem(null);
                }
            }
            else
            {
                //project not open, or train node not selected there, just remove the node from the train track...
                var track = trainnode.Track;
                if (!track.RemoveNode(trainnode))
                {
                    MessageBox.Show("Unable to remove train track node.");
                }
                else
                {
                    UpdateTrainTrackNodeGraphics(trainnode, false);
                    SelectItem(null);
                }
            }
        }
        private void CopyTrainNode()
        {
            CopiedTrainNode = SelectedItem.TrainTrackNode;
            ToolbarPasteButton.Enabled = (CopiedTrainNode != null) && ToolbarAddItemButton.Enabled;
        }
        private void PasteTrainNode()
        {
            if (CopiedTrainNode == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewTrainNode(CopiedTrainNode);
        }
        private void CloneTrainNode()
        {
            if (SelectedItem.TrainTrackNode == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewTrainNode(SelectedItem.TrainTrackNode, true);
        }

        private void AddScenarioNode()
        {
            if (ProjectForm == null) return;
            ProjectForm.NewScenarioNode();
        }
        private void DeleteScenarioNode()
        {
            var scenariopt = SelectedItem.ScenarioNode;
            if (scenariopt == null) return;

            if ((ProjectForm != null) && (ProjectForm.IsCurrentScenarioNode(scenariopt)))
            {
                if (!ProjectForm.DeleteScenarioNode())
                {
                    //MessageBox.Show("Unable to delete this scenario point from the current project. Make sure the scenario file exists in the current project.");
                }
                else
                {
                    SelectItem(null);
                }
            }
            else
            {
                //project not open, or scenario point not selected there, just remove the point from the region...
                var region = scenariopt.Region.Ymt.ScenarioRegion;
                if (!region.RemoveNode(scenariopt))
                {
                    MessageBox.Show("Unable to remove scenario point.");
                }
                else
                {
                    UpdateScenarioGraphics(scenariopt.Ymt, false);
                    SelectItem(null);
                }
            }
        }
        private void CopyScenarioNode()
        {
            CopiedScenarioNode = SelectedItem.ScenarioNode;
            ToolbarPasteButton.Enabled = (CopiedScenarioNode != null) && ToolbarAddItemButton.Enabled;
        }
        private void PasteScenarioNode()
        {
            if (CopiedScenarioNode == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewScenarioNode(CopiedScenarioNode);
        }
        private void CloneScenarioNode()
        {
            if (SelectedItem.ScenarioNode == null) return;
            if (ProjectForm == null) return;
            ProjectForm.NewScenarioNode(SelectedItem.ScenarioNode, true);
        }


        private void SetMouseSelect(bool enable)
        {
            MouseSelectEnabled = enable;
            MouseSelectCheckBox.Checked = enable;
            ToolbarSelectButton.Checked = enable;

            if (InfoForm != null)
            {
                InfoForm.SetSelectionMode(SelectionModeStr, MouseSelectEnabled);
            }
        }

        private void SetWidgetMode(string mode)
        {
            ToolbarMoveButton.Checked = false;
            ToolbarRotateButton.Checked = false;
            ToolbarScaleButton.Checked = false;

            lock (Renderer.RenderSyncRoot)
            {
                switch (mode)
                {
                    case "Default":
                        Widget.Mode = WidgetMode.Default;
                        iseditmode = false;
                        break;
                    case "Position":
                        Widget.Mode = WidgetMode.Position;
                        iseditmode = true;
                        ToolbarMoveButton.Checked = true;
                        break;
                    case "Rotation":
                        Widget.Mode = WidgetMode.Rotation;
                        iseditmode = true;
                        ToolbarRotateButton.Checked = true;
                        break;
                    case "Scale":
                        Widget.Mode = WidgetMode.Scale;
                        iseditmode = true;
                        ToolbarScaleButton.Checked = true;
                        break;
                }
            }
        }

        private void SetWidgetSpace(string space)
        {
            foreach (var child in ToolbarTransformSpaceButton.DropDownItems)
            {
                var childi = child as ToolStripMenuItem;
                if (childi != null)
                {
                    childi.Checked = false;
                }
            }

            lock (Renderer.RenderSyncRoot)
            {
                switch (space)
                {
                    case "World space":
                        Widget.ObjectSpace = false;
                        ToolbarTransformSpaceButton.Image = ToolbarWorldSpaceButton.Image;
                        ToolbarWorldSpaceButton.Checked = true;
                        break;
                    case "Object space":
                        Widget.ObjectSpace = true;
                        ToolbarTransformSpaceButton.Image = ToolbarObjectSpaceButton.Image;
                        ToolbarObjectSpaceButton.Checked = true;
                        break;
                }
            }
        }

        private void ToggleWidgetSpace()
        {
            SetWidgetSpace(Widget.ObjectSpace ? "World space" : "Object space");
        }



        private void SetFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        private void SetBoundsMode(string modestr)
        {
            BoundsShaderMode mode = BoundsShaderMode.None;
            switch (modestr)
            {
                case "Boxes":
                    mode = BoundsShaderMode.Box;
                    break;
                case "Spheres":
                    mode = BoundsShaderMode.Sphere;
                    break;
            }
            Renderer.boundsmode = mode;
        }



        private void SetSelectionMode(string modestr)
        {

            foreach (var child in ToolbarSelectButton.DropDownItems)
            {
                var childi = child as ToolStripMenuItem;
                if (childi != null)
                {
                    childi.Checked = false;
                }
            }

            MapSelectionMode mode = MapSelectionMode.Entity;
            switch (modestr)
            {
                default:
                case "Entity":
                    mode = MapSelectionMode.Entity;
                    ToolbarSelectEntityButton.Checked = true;
                    break;
                case "Entity Extension":
                    mode = MapSelectionMode.EntityExtension;
                    ToolbarSelectEntityExtensionButton.Checked = true;
                    break;
                case "Archetype Extension":
                    mode = MapSelectionMode.ArchetypeExtension;
                    ToolbarSelectArchetypeExtensionButton.Checked = true;
                    break;
                case "Time Cycle Modifier":
                    mode = MapSelectionMode.TimeCycleModifier;
                    ToolbarSelectTimeCycleModifierButton.Checked = true;
                    break;
                case "Car Generator":
                    mode = MapSelectionMode.CarGenerator;
                    ToolbarSelectCarGeneratorButton.Checked = true;
                    break;
                case "Grass":
                    mode = MapSelectionMode.Grass;
                    ToolbarSelectGrassButton.Checked = true;
                    break;
                case "Water Quad":
                    mode = MapSelectionMode.WaterQuad;
                    ToolbarSelectWaterQuadButton.Checked = true;
                    break;
                case "Collision":
                    mode = MapSelectionMode.Collision;
                    ToolbarSelectCollisionButton.Checked = true;
                    break;
                case "Nav Mesh":
                    mode = MapSelectionMode.NavMesh;
                    ToolbarSelectNavMeshButton.Checked = true;
                    break;
                case "Path":
                    mode = MapSelectionMode.Path;
                    ToolbarSelectPathButton.Checked = true;
                    break;
                case "Train Track":
                    mode = MapSelectionMode.TrainTrack;
                    ToolbarSelectTrainTrackButton.Checked = true;
                    break;
                case "Distant Lod Lights":
                    mode = MapSelectionMode.DistantLodLights;
                    ToolbarSelectDistantLodLightsButton.Checked = true;
                    break;
                case "Mlo Instance":
                    mode = MapSelectionMode.MloInstance;
                    ToolbarSelectMloInstanceButton.Checked = true;
                    break;
                case "Scenario":
                    mode = MapSelectionMode.Scenario;
                    ToolbarSelectScenarioButton.Checked = true;
                    break;
                case "Audio":
                    mode = MapSelectionMode.Audio;
                    ToolbarSelectAudioButton.Checked = true;
                    break;

            }
            SelectionMode = mode;
            SelectionModeStr = modestr;

            if (SelectionModeComboBox.Text != modestr)
            {
                SelectionModeComboBox.Text = modestr;
            }

            if (InfoForm != null)
            {
                InfoForm.SetSelectionMode(modestr, MouseSelectEnabled);
            }
        }


        private void SetSnapMode(WorldSnapMode mode)
        {
            foreach (var child in ToolbarSnapButton.DropDownItems)
            {
                var childi = child as ToolStripMenuItem;
                if (childi != null)
                {
                    childi.Checked = false;
                }
            }

            ToolbarSnapButton.Checked = (mode != WorldSnapMode.None);

            ToolStripMenuItem selItem = null;

            switch (mode)
            {
                case WorldSnapMode.Ground:
                    selItem = ToolbarSnapToGroundButton;
                    break;
                case WorldSnapMode.Grid:
                    selItem = ToolbarSnapToGridButton;
                    break;
                case WorldSnapMode.Hybrid:
                    selItem = ToolbarSnapToGroundGridButton;
                    break;
            }

            if (selItem != null)
            {
                selItem.Checked = true;
                ToolbarSnapButton.Image = selItem.Image;
                ToolbarSnapButton.Text = selItem.Text;
                ToolbarSnapButton.ToolTipText = selItem.ToolTipText;
            }

            if (mode != WorldSnapMode.None)
            {
                SnapModePrev = mode;
            }
            SnapMode = mode;
        }


        private void SetCameraMode(string modestr)
        {
            foreach (var child in ToolbarCameraModeButton.DropDownItems)
            {
                var childi = child as ToolStripMenuItem;
                if (childi != null)
                {
                    childi.Checked = false;
                }
            }


            Renderer.SetCameraMode(modestr);

            switch (modestr)
            {
                case "Perspective":
                    MapViewEnabled = false;
                    ToolbarCameraModeButton.Image = ToolbarCameraPerspectiveButton.Image;
                    ToolbarCameraPerspectiveButton.Checked = true;
                    break;
                case "Orthographic":
                    MapViewEnabled = false;
                    ToolbarCameraModeButton.Image = ToolbarCameraOrthographicButton.Image;
                    ToolbarCameraOrthographicButton.Checked = true;
                    break;
                case "2D Map":
                    MapViewEnabled = true;
                    ToolbarCameraModeButton.Image = ToolbarCameraMapViewButton.Image;
                    ToolbarCameraMapViewButton.Checked = true;
                    break;
            }

            FieldOfViewTrackBar.Enabled = !MapViewEnabled;
            MapViewDetailTrackBar.Enabled = MapViewEnabled;


            if (CameraModeComboBox.Text != modestr)
            {
                CameraModeComboBox.Text = modestr;
            }


        }

        private void ToggleCameraMode()
        {
            SetCameraMode(MapViewEnabled ? "Perspective" : "2D Map");
        }


        private void ToggleToolbar()
        {
            ToolbarPanel.Visible = !ToolbarPanel.Visible;
            ShowToolbarCheckBox.Checked = ToolbarPanel.Visible;
        }


        private void StatsUpdateTimer_Tick(object sender, EventArgs e)
        {

            StatsLabel.Text = Renderer.GetStatusText();

            if (Renderer.timerunning)
            {
                float fv = Renderer.timeofday * 60.0f;
                TimeOfDayTrackBar.Value = (int)fv;
                UpdateTimeOfDayLabel();
            }

            CameraPositionTextBox.Text = FloatUtil.GetVector3String(camera.Position, "0.##");
        }

        private void WorldForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void WorldForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (ProjectForm != null)
            //{
            //    if (MessageBox.Show("Are you sure you want to quit CodeWalker?", "Confirm quit", MessageBoxButtons.YesNo) != DialogResult.Yes)
            //    {
            //        e.Cancel = true; //unfortunately this doesn't catch the event early enough! :(
            //    }
            //}
        }

        private void WorldForm_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = true; break;
                case MouseButtons.Right: MouseRButtonDown = true; break;
            }

            MouseDownPoint = e.Location;
            MouseLastPoint = MouseDownPoint;

            if (ControlMode == WorldControlMode.Free)
            {
                if (MouseLButtonDown)
                {
                    if (MousedMarker != null)
                    {
                        if (MousedMarker.IsMovable)
                        {
                            GrabbedMarker = MousedMarker;
                        }
                        else
                        {
                            SelectedMarker = MousedMarker;
                            ShowMarkerSelectionInfo(SelectedMarker);
                        }
                        if (GrabbedWidget != null)
                        {
                            GrabbedWidget.IsDragging = false;
                            GrabbedWidget = null;
                        }
                    }
                    else
                    {
                        if (ShowWidget && Widget.IsUnderMouse)
                        {
                            GrabbedWidget = Widget;
                            GrabbedWidget.IsDragging = true;
                            if (Input.ShiftPressed)
                            {
                                CloneItem();
                            }
                            MarkUndoStart(GrabbedWidget);
                        }
                        else
                        {
                            if (GrabbedWidget != null)
                            {
                                GrabbedWidget.IsDragging = false;
                                GrabbedWidget = null;
                            }

                            if (Input.CtrlPressed)
                            {
                                SpawnTestEntity();
                            }

                        }
                        GrabbedMarker = null;
                    }
                }

                if (MouseRButtonDown)
                {
                    SelectMousedItem();
                }
            }
            else
            {
                lock (MouseControlSyncRoot)
                {
                    MouseControlButtons |= e.Button;
                }
            }

            MouseX = e.X; //to stop jumps happening on mousedown, sometimes the last MouseMove event was somewhere else... (eg after clicked a menu)
            MouseY = e.Y;
        }

        private void WorldForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left: MouseLButtonDown = false; break;
                case MouseButtons.Right: MouseRButtonDown = false; break;
            }

            lock (MouseControlSyncRoot)
            {
                MouseControlButtons &= ~e.Button;
            }



            if (e.Button == MouseButtons.Left)
            {
                GrabbedMarker = null;
                if (GrabbedWidget != null)
                {
                    MarkUndoEnd(GrabbedWidget);
                    GrabbedWidget.IsDragging = false;
                    GrabbedWidget.Position = SelectedItem.WidgetPosition;//in case of any snapping, make sure widget is in correct position at the end
                    GrabbedWidget = null;
                }
                if ((e.Location == MouseDownPoint) && (MousedMarker == null))
                {
                    //was clicked. but not on a marker... deselect and hide the panel
                    SelectedMarker = null;
                    HideMarkerSelectionInfo();
                }
            }

        }

        private void WorldForm_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - MouseX;
            int dy = e.Y - MouseY;

            if (MouseInvert)
            {
                dy = -dy;
            }

            if (ControlMode == WorldControlMode.Free)
            {
                if (MouseLButtonDown)
                {
                    if (GrabbedMarker == null)
                    {
                        if (GrabbedWidget == null)
                        {
                            if (MapViewEnabled == false)
                            {
                                camera.MouseRotate(dx, dy);
                            }
                            else
                            {
                                //need to move the camera entity XY with mouse in mapview mode...
                                MapViewDragX += dx;
                                MapViewDragY += dy;
                            }
                        }
                        else
                        {
                            //grabbed widget will move itself in Update() when IsDragging==true
                        }
                    }
                    else
                    {
                        //move the grabbed marker...
                        //float uptx = (CurrentMap != null) ? CurrentMap.UnitsPerTexelX : 1.0f;
                        //float upty = (CurrentMap != null) ? CurrentMap.UnitsPerTexelY : 1.0f;
                        //Vector3 wpos = GrabbedMarker.WorldPos;
                        //wpos.X += dx * uptx;
                        //wpos.Y += dy * upty;
                        //GrabbedMarker.WorldPos = wpos;
                        //UpdateMarkerTexturePos(GrabbedMarker);
                        //if (GrabbedMarker == LocatorMarker)
                        //{
                        //    LocateTextBox.Text = LocatorMarker.ToString();
                        //    WorldCoordTextBox.Text = LocatorMarker.Get2DWorldPosString();
                        //    TextureCoordTextBox.Text = LocatorMarker.Get2DTexturePosString();
                        //}
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
            else
            {
                lock (MouseControlSyncRoot)
                {
                    MouseControlX += dx;
                    MouseControlY += dy;
                    //MouseControlButtons = e.Button;
                }
                var newpos = PointToScreen(MouseLastPoint);
                if (Cursor.Position != newpos)
                {
                    Cursor.Position = newpos;
                    return;
                }
            }



            MousedMarker = FindMousedMarker();

            if (Cursor != Cursors.WaitCursor)
            {
                if (MousedMarker != null)
                {
                    if (MousedMarker.IsMovable)
                    {
                        Cursor = Cursors.SizeAll;
                    }
                    else
                    {
                        Cursor = Cursors.Hand;
                    }
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void WorldForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (ControlMode == WorldControlMode.Free)
                {
                    camera.MouseZoom(e.Delta);
                }
                else
                {
                    lock (MouseControlSyncRoot)
                    {
                        MouseControlWheel += e.Delta;
                    }
                }
            }

        }

        private void WorldForm_KeyDown(object sender, KeyEventArgs e)
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

            bool enablemove = (!iseditmode) || (MouseLButtonDown && (GrabbedMarker == null) && (GrabbedWidget == null));

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
                    if ((k == kb.ExitEditMode))
                    {
                        if (Widget.Mode == WidgetMode.Default) ToggleWidgetSpace();
                        else SetWidgetMode("Default");
                    }
                    if ((k == kb.EditPosition))// && !enablemove)
                    {
                        if (Widget.Mode == WidgetMode.Position) ToggleWidgetSpace();
                        else SetWidgetMode("Position");
                    }
                    if ((k == kb.EditRotation))// && !enablemove)
                    {
                        if (Widget.Mode == WidgetMode.Rotation) ToggleWidgetSpace();
                        else SetWidgetMode("Rotation");
                    }
                    if ((k == kb.EditScale))// && !enablemove)
                    {
                        if (Widget.Mode == WidgetMode.Scale) ToggleWidgetSpace();
                        else SetWidgetMode("Scale");
                    }
                    if (k == kb.ToggleMouseSelect)
                    {
                        SetMouseSelect(!MouseSelectEnabled);
                    }
                    if (k == kb.ToggleToolbar)
                    {
                        ToggleToolbar();
                    }
                    if (k == kb.FirstPerson)
                    {
                        SetControlMode((ControlMode == WorldControlMode.Free) ? WorldControlMode.Ped : WorldControlMode.Free);
                    }
                    if (k == Keys.Delete)
                    {
                        DeleteItem();
                    }
                }
                else
                {
                    switch (k)
                    {
                        case Keys.N:
                            New();
                            break;
                        case Keys.O:
                            Open();
                            break;
                        case Keys.S:
                            if (shift) SaveAll();
                            else Save();
                            break;
                        case Keys.Z:
                            Undo();
                            break;
                        case Keys.Y:
                            Redo();
                            break;
                        case Keys.C:
                            CopyItem();
                            break;
                        case Keys.V:
                            PasteItem();
                            break;
                        case Keys.U:
                            ToolsPanelShowButton.Visible = !ToolsPanelShowButton.Visible;
                            break;
                    }
                }
                if (k == Keys.Escape) //temporary? panic get cursor back when in first person mode
                {
                    if (ControlMode != WorldControlMode.Free) SetControlMode(WorldControlMode.Free);
                }
            }

            if (ControlMode != WorldControlMode.Free)
            {
                e.Handled = true;
            }
        }

        private void WorldForm_KeyUp(object sender, KeyEventArgs e)
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

            if (ControlMode != WorldControlMode.Free)
            {
                e.Handled = true;
            }
        }

        private void WorldForm_Deactivate(object sender, EventArgs e)
        {
            //try not to lock keyboard movement if the form loses focus.
            Input.KeyboardStop();
        }

        private void ViewModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool prevmodel = !(rendermaps || renderworld);
            string mode = (string)ViewModeComboBox.SelectedItem;
            switch (mode)
            {
                case "World view":
                    rendermaps = false;
                    renderworld = true;
                    ViewTabControl.SelectedTab = ViewWorldTabPage;
                    break;
                case "Ymap view":
                    rendermaps = true;
                    renderworld = false;
                    ViewTabControl.SelectedTab = ViewYmapsTabPage;
                    break;
                case "Model view":
                    rendermaps = false;
                    renderworld = false;
                    ViewTabControl.SelectedTab = ViewModelTabPage;
                    break;
            }

            if ((camera == null) || (camera.FollowEntity == null)) return;
            if (rendermaps || renderworld)
            {
                if (prevmodel) //only change location if the last mode was model mode
                {
                    camera.FollowEntity.Position = prevworldpos;
                }
            }
            else
            {
                prevworldpos = camera.FollowEntity.Position;
                camera.FollowEntity.Position = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        private void ModelComboBox_TextUpdate(object sender, EventArgs e)
        {
            modelname = ModelComboBox.Text;
        }

        private void ModelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            modelname = ModelComboBox.Text;
        }

        private void YmapsTextBox_TextChanged(object sender, EventArgs e)
        {
            ymaplist = YmapsTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void ToolsPanelHideButton_Click(object sender, EventArgs e)
        {
            ToolsPanel.Visible = false;
            ToolsPanelShowButton.Focus();
        }

        private void ToolsPanelShowButton_Click(object sender, EventArgs e)
        {
            ToolsPanel.Visible = true;
            ToolsPanelHideButton.Focus();
        }

        private void WireframeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.wireframe = WireframeCheckBox.Checked;
        }

        private void GrassCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendergrass = GrassCheckBox.Checked;
        }

        private void TimedEntitiesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendertimedents = TimedEntitiesCheckBox.Checked;
        }

        private void TimedEntitiesAlwaysOnCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendertimedentsalways = TimedEntitiesAlwaysOnCheckBox.Checked;
        }

        private void InteriorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderinteriors = InteriorsCheckBox.Checked;
        }

        private void WaterQuadsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderwaterquads = WaterQuadsCheckBox.Checked;
        }

        private void ProxiesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderproxies = ProxiesCheckBox.Checked;
        }

        private void PathsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderpaths = PathsCheckBox.Checked;
        }

        private void PathBoundsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderpathbounds = PathBoundsCheckBox.Checked;
        }

        private void TrainPathsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendertraintracks = TrainPathsCheckBox.Checked;
        }

        private void NavMeshesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendernavmeshes = NavMeshesCheckBox.Checked;
        }

        private void PathsDepthClipCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.PathsDepthClip = PathsDepthClipCheckBox.Checked;
        }

        private void ErrorConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConsolePanel.Visible = ErrorConsoleCheckBox.Checked;
        }

        private void DynamicLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.usedynamiclod = DynamicLODCheckBox.Checked;
            ShowYmapChildrenCheckBox.Enabled = !Renderer.usedynamiclod;
        }

        private void DetailTrackBar_Scroll(object sender, EventArgs e)
        {
            Renderer.lodthreshold = 50.0f / (0.1f + (float)DetailTrackBar.Value);
        }

        private void WaitForChildrenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.waitforchildrentoload = WaitForChildrenCheckBox.Checked;
        }

        private void ReloadShadersButton_Click(object sender, EventArgs e)
        {
            if (Renderer.Device == null) return; //can't do this with no device

            Cursor = Cursors.WaitCursor;
            pauserendering = true;

            lock (Renderer.RenderSyncRoot)
            {
                try
                {
                    Renderer.ReloadShaders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading shaders!\n" + ex.ToString());
                    return;
                }
            }

            pauserendering = false;
            Cursor = Cursors.Default;
        }

        private void MarkerStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapIcon icon = MarkerStyleComboBox.SelectedItem as MapIcon;
            if (icon != MarkerIcon)
            {
                MarkerIcon = icon;
                foreach (MapMarker m in Markers)
                {
                    m.Icon = icon;
                }
            }
        }

        private void LocatorStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapIcon icon = LocatorStyleComboBox.SelectedItem as MapIcon;
            if (icon != LocatorIcon)
            {
                LocatorIcon = icon;
                LocatorMarker.Icon = icon;
            }
        }

        private void ShowLocatorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RenderLocator = ShowLocatorCheckBox.Checked;
        }

        private void LocateTextBox_TextChanged(object sender, EventArgs e)
        {
            if (GrabbedMarker == LocatorMarker) return; //don't try to update the marker if it's being dragged
            if (LocatorMarker == null) return; //this shouldn't happen, but anyway

            LocatorMarker.Parse(LocateTextBox.Text);

            //UpdateMarkerTexturePos(LocatorMarker);
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            GoToMarker(LocatorMarker);
        }

        private void AddMarkersButton_Click(object sender, EventArgs e)
        {
            string[] lines = MultiFindTextBox.Text.Split('\n');
            foreach (string line in lines)
            {
                AddMarker(line);
            }
        }

        private void ClearMarkersButton_Click(object sender, EventArgs e)
        {
            MultiFindTextBox.Text = string.Empty;
            Markers.Clear();
        }

        private void ResetMarkersButton_Click(object sender, EventArgs e)
        {
            Markers.Clear();
            AddDefaultMarkers();
        }

        private void AddCurrentPositonMarkerButton_Click(object sender, EventArgs e)
        {
            AddMarker(camera.Position, "Marker", true);
        }

        private void AddSelectionMarkerButton_Click(object sender, EventArgs e)
        {
            if (SelectedItem.EntityDef == null)
            { return; }

            Vector3 pos = SelectedItem.EntityDef.Position;
            string name = SelectedItem.EntityDef.CEntityDef.archetypeName.ToString();
            var marker = AddMarker(pos, name, true);
            SelectedMarker = marker;
            ShowMarkerSelectionInfo(SelectedMarker);
        }

        private void MarkerDepthClipCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.markerdepthclip = MarkerDepthClipCheckBox.Checked;
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.shaders.shadows = ShadowsCheckBox.Checked;
            }
        }

        private void SkydomeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderskydome = SkydomeCheckBox.Checked;
        }

        private void BoundsStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = BoundsStyleComboBox.SelectedItem;
            var strval = val as string;
            SetBoundsMode(strval);
        }

        private void BoundsDepthClipCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderboundsclip = BoundsDepthClipCheckBox.Checked;
        }

        private void BoundsRangeTrackBar_Scroll(object sender, EventArgs e)
        {
            float fv = BoundsRangeTrackBar.Value;
            Renderer.renderboundsmaxdist = fv * fv;
        }

        private void MouseSelectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetMouseSelect(MouseSelectCheckBox.Checked);
        }

        private void SelectionBoundsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowSelectionBounds = SelectionBoundsCheckBox.Checked;
        }

        private void PopZonesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderpopzones = PopZonesCheckBox.Checked;
        }

        private void SkeletonsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderskeletons = SkeletonsCheckBox.Checked;
        }

        private void AudioOuterBoundsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderaudioouterbounds = AudioOuterBoundsCheckBox.Checked;
        }

        private void ToolsPanelExpandButton_Click(object sender, EventArgs e)
        {
            toolspanelexpanded = !toolspanelexpanded;

            int oldwidth = ToolsPanel.Width;
            if (toolspanelexpanded)
            {
                ToolsPanelExpandButton.Text = ">>";
            }
            else
            {
                ToolsPanelExpandButton.Text = "<<";
            }
            ToolsPanel.Width = toolspanellastwidth; //or extended width
            ToolsPanel.Left -= (toolspanellastwidth - oldwidth);
            toolspanellastwidth = oldwidth;
        }

        private void ToolsDragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                toolsPanelResizing = true;
                toolsPanelResizeStartX = e.X + ToolsPanel.Left;
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
                int rx = e.X + ToolsPanel.Left;
                int dx = rx - toolsPanelResizeStartX;
                ToolsPanel.Left = toolsPanelResizeStartLeft + dx;
                ToolsPanel.Width = toolsPanelResizeStartRight - toolsPanelResizeStartLeft - dx;
            }
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetFullscreen(FullScreenCheckBox.Checked);
        }

        private void ControlSettingsButton_Click(object sender, EventArgs e)
        {
            ShowSettingsForm("Controls");
        }

        private void AdvancedSettingsButton_Click(object sender, EventArgs e)
        {
            ShowSettingsForm("Advanced");
        }

        private void ReloadSettingsButton_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really quit?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Close();
            }
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            AboutForm f = new AboutForm();
            f.Show(this);
        }

        private void ToolsButton_Click(object sender, EventArgs e)
        {
            ToolsMenu.Show(ToolsButton, 0, ToolsButton.Height);
        }

        private void ToolsMenuRPFBrowser_Click(object sender, EventArgs e)
        {
            BrowseForm f = new BrowseForm();
            f.Show(this);
        }

        private void ToolsMenuRPFExplorer_Click(object sender, EventArgs e)
        {
            ExploreForm f = new ExploreForm();
            f.Show(this);
        }

        private void ToolsMenuSelectionInfo_Click(object sender, EventArgs e)
        {
            ShowInfoForm();
        }

        private void ToolsMenuProjectWindow_Click(object sender, EventArgs e)
        {
            ShowProjectForm();
        }

        private void ToolsMenuWorldSearch_Click(object sender, EventArgs e)
        {
            ShowSearchForm();
        }

        private void ToolsMenuBinarySearch_Click(object sender, EventArgs e)
        {
            BinarySearchForm f = new BinarySearchForm(gameFileCache);
            f.Show(this);
        }

        private void ToolsMenuJenkGen_Click(object sender, EventArgs e)
        {
            JenkGenForm f = new JenkGenForm();
            f.Show(this);
        }

        private void ToolsMenuJenkInd_Click(object sender, EventArgs e)
        {
            JenkIndForm f = new JenkIndForm(gameFileCache);
            f.Show(this);
        }

        private void ToolsMenuExtractScripts_Click(object sender, EventArgs e)
        {
            ExtractScriptsForm f = new ExtractScriptsForm();
            f.Show(this);
        }

        private void ToolsMenuExtractTextures_Click(object sender, EventArgs e)
        {
            ExtractTexForm f = new ExtractTexForm();
            f.Show(this);
        }

        private void ToolsMenuExtractRawFiles_Click(object sender, EventArgs e)
        {
            ExtractRawForm f = new ExtractRawForm();
            f.Show(this);
        }

        private void ToolsMenuExtractShaders_Click(object sender, EventArgs e)
        {
            ExtractShadersForm f = new ExtractShadersForm();
            f.Show(this);
        }

        private void ToolsMenuOptions_Click(object sender, EventArgs e)
        {
            ShowSettingsForm();
        }

        private void StatusBarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            StatusStrip.Visible = StatusBarCheckBox.Checked;
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

        private void CollisionMeshesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendercollisionmeshes = CollisionMeshesCheckBox.Checked;
            Renderer.rendercollisionmeshes = rendercollisionmeshes;
        }

        private void CollisionMeshRangeTrackBar_Scroll(object sender, EventArgs e)
        {
            collisionmeshrange = CollisionMeshRangeTrackBar.Value;
        }

        private void CollisionMeshLayer0CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            collisionmeshlayers[0] = CollisionMeshLayer0CheckBox.Checked;
        }

        private void CollisionMeshLayer1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            collisionmeshlayers[1] = CollisionMeshLayer1CheckBox.Checked;
        }

        private void CollisionMeshLayer2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            collisionmeshlayers[2] = CollisionMeshLayer2CheckBox.Checked;
        }

        private void CollisionMeshLayerDrawableCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendercollisionmeshlayerdrawable = CollisionMeshLayerDrawableCheckBox.Checked;
        }

        private void ControlLightDirectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.controllightdir = ControlLightDirectionCheckBox.Checked;
            if (Renderer.controllightdir)
            {
                ControlTimeOfDayCheckBox.Checked = false;
            }
        }

        private void ControlTimeOfDayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.controltimeofday = ControlTimeOfDayCheckBox.Checked;
            if (Renderer.controltimeofday)
            {
                ControlLightDirectionCheckBox.Checked = false;
            }
        }

        private void ShowYmapChildrenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderchildents = ShowYmapChildrenCheckBox.Checked;
        }

        private void HDRRenderingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.shaders.hdr = HDRRenderingCheckBox.Checked;
            }
        }

        private void AnisotropicFilteringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.shaders.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
        }

        private void WorldMaxLodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (WorldMaxLodComboBox.Text)
            {
                default:
                case "ORPHANHD":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
                    break;
                case "HD":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_HD;
                    break;
                case "LOD":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_LOD;
                    break;
                case "SLOD1":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD1;
                    break;
                case "SLOD2":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD2;
                    break;
                case "SLOD3":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD3;
                    break;
                case "SLOD4":
                    Renderer.renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD4;
                    break;
            }
        }

        private void WorldLodDistTrackBar_Scroll(object sender, EventArgs e)
        {
            float loddist = ((float)WorldLodDistTrackBar.Value) * 0.1f;
            Renderer.renderworldLodDistMult = loddist;
            WorldLodDistLabel.Text = loddist.ToString("0.0");
        }

        private void WorldDetailDistTrackBar_Scroll(object sender, EventArgs e)
        {
            float detdist = ((float)WorldDetailDistTrackBar.Value) * 0.1f;
            Renderer.renderworldDetailDistMult = detdist;
            WorldDetailDistLabel.Text = detdist.ToString("0.0");
        }

        private void WorldScriptedYmapsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.ShowScriptedYmaps = WorldScriptedYmapsCheckBox.Checked;
        }

        private void WorldYmapTimeFilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            worldymaptimefilter = WorldYmapTimeFilterCheckBox.Checked;
        }

        private void WorldYmapWeatherFilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            worldymapweatherfilter = WorldYmapWeatherFilterCheckBox.Checked;
        }

        private void EnableModsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!initialised) return;
            if (ProjectForm != null)
            {
                MessageBox.Show("Please close the Project Window before enabling or disabling mods.");
                return;
            }
            
            SetModsEnabled(EnableModsCheckBox.Checked);
        }

        private void EnableDlcCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!initialised) return;
            if (ProjectForm != null)
            {
                MessageBox.Show("Please close the Project Window before enabling or disabling DLC.");
                return;
            }

            SetDlcLevel(DlcLevelComboBox.Text, EnableDlcCheckBox.Checked);
        }

        private void DlcLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!initialised) return;
            if (ProjectForm != null)
            {
                MessageBox.Show("Please close the Project Window before changing the DLC level.");
                return;
            }

            SetDlcLevel(DlcLevelComboBox.Text, EnableDlcCheckBox.Checked);
        }

        private void TimeOfDayTrackBar_Scroll(object sender, EventArgs e)
        {
            int v = TimeOfDayTrackBar.Value;
            float hour = v / 60.0f;
            UpdateTimeOfDayLabel();
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.SetTimeOfDay(hour);
            }
        }

        private void WeatherComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Renderer.SetWeatherType(WeatherComboBox.Text);
        }

        private void WeatherRegionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            weather.Region = WeatherRegionComboBox.Text;
        }

        private void CloudsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (!Monitor.TryEnter(rendersyncroot, 50))
            //{ return; } //couldn't get a lock...
            Renderer.individualcloudfrag = CloudsComboBox.Text;
            //Monitor.Exit(rendersyncroot);
        }

        private void DistantLODLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderdistlodlights = DistantLODLightsCheckBox.Checked;
        }

        private void NaturalAmbientLightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.rendernaturalambientlight = NaturalAmbientLightCheckBox.Checked;
        }

        private void ArtificialAmbientLightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Renderer.renderartificialambientlight = ArtificialAmbientLightCheckBox.Checked;
        }

        private void TimeStartStopButton_Click(object sender, EventArgs e)
        {
            Renderer.timerunning = !Renderer.timerunning;
            TimeStartStopButton.Text = Renderer.timerunning ? "Stop" : "Start";
        }

        private void TimeSpeedTrackBar_Scroll(object sender, EventArgs e)
        {
            float tv = TimeSpeedTrackBar.Value * 0.01f;
            //when tv=0,   speed=0 min/sec
            //when tv=0.5, speed=0.5 min/sec
            //when tv=1,  speed=128 min/sec

            Renderer.timespeed = 128.0f * tv * tv * tv * tv * tv * tv * tv * tv;

            TimeSpeedLabel.Text = Renderer.timespeed.ToString("0.###") + " min/sec";
        }

        private void CameraModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCameraMode(CameraModeComboBox.Text);
        }

        private void MapViewDetailTrackBar_Scroll(object sender, EventArgs e)
        {
            float det = ((float)MapViewDetailTrackBar.Value) * 0.1f;
            MapViewDetailLabel.Text = det.ToString("0.0#");
            lock (Renderer.RenderSyncRoot)
            {
                Renderer.MapViewDetail = det;
            }
        }

        private void FieldOfViewTrackBar_Scroll(object sender, EventArgs e)
        {
            float fov = FieldOfViewTrackBar.Value * 0.01f;
            FieldOfViewLabel.Text = fov.ToString("0.0#");
            lock (Renderer.RenderSyncRoot)
            {
                camera.FieldOfView = fov;
                camera.UpdateProj = true;
            }
        }

        private void CloudParamComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CloudAnimSetting setting = CloudParamComboBox.SelectedItem as CloudAnimSetting;
            if (setting != null)
            {
                float rng = setting.MaxValue - setting.MinValue;
                float cval = (setting.CurrentValue - setting.MinValue) / rng;
                int ival = (int)(cval * 200.0f);
                ival = Math.Min(Math.Max(ival, 0), 200);
                CloudParamTrackBar.Value = ival;
            }
        }

        private void CloudParamTrackBar_Scroll(object sender, EventArgs e)
        {
            CloudAnimSetting setting = CloudParamComboBox.SelectedItem as CloudAnimSetting;
            if (setting != null)
            {
                float rng = setting.MaxValue - setting.MinValue;
                float fval = CloudParamTrackBar.Value / 200.0f;
                float cval = (fval * rng) + setting.MinValue;
                setting.CurrentValue = cval;
            }
        }

        private void SelDrawableModelsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                UpdateSelectionDrawFlags(e.Node);

                if (InfoForm != null)
                {
                    InfoForm.SyncSelDrawableModelsTreeNode(e.Node);
                }
            }
        }

        private void SelDrawableModelsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                e.Node.Checked = !e.Node.Checked;
                //UpdateSelectionDrawFlags(e.Node);
            }
        }

        private void SelDrawableModelsTreeView_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //stops annoying ding sound...
        }

        private void SelectionWidgetCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowWidget = SelectionWidgetCheckBox.Checked;
        }

        private void ShowToolbarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ToolbarPanel.Visible = ShowToolbarCheckBox.Checked;
        }

        private void ToolbarNewButton_ButtonClick(object sender, EventArgs e)
        {
            New();
        }

        private void ToolbarNewProjectButton_Click(object sender, EventArgs e)
        {
            NewProject();
        }

        private void ToolbarNewYmapButton_Click(object sender, EventArgs e)
        {
            NewYmap();
        }

        private void ToolbarNewYndButton_Click(object sender, EventArgs e)
        {
            NewYnd();
        }

        private void ToolbarNewTrainsButton_Click(object sender, EventArgs e)
        {
            NewTrainTrack();
        }

        private void ToolbarNewScenarioButton_Click(object sender, EventArgs e)
        {
            NewScenario();
        }

        private void ToolbarOpenButton_ButtonClick(object sender, EventArgs e)
        {
            Open();
        }

        private void ToolbarOpenProjectButton_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void ToolbarOpenYmapButton_Click(object sender, EventArgs e)
        {
            OpenYmap();
        }

        private void ToolbarOpenYndButton_Click(object sender, EventArgs e)
        {
            OpenYnd();
        }

        private void ToolbarOpenTrainsButton_Click(object sender, EventArgs e)
        {
            OpenTrainTrack();
        }

        private void ToolbarOpenScenarioButton_Click(object sender, EventArgs e)
        {
            OpenScenario();
        }

        private void ToolbarSaveButton_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void ToolbarSaveAllButton_Click(object sender, EventArgs e)
        {
            SaveAll();
        }

        private void ToolbarSelectButton_ButtonClick(object sender, EventArgs e)
        {
            SetMouseSelect(!ToolbarSelectButton.Checked);
            SetWidgetMode("Default");
        }

        private void ToolbarSelectEntityButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Entity");
            SetMouseSelect(true);
        }

        private void ToolbarSelectEntityExtensionButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Entity Extension");
            SetMouseSelect(true);
        }

        private void ToolbarSelectArchetypeExtensionButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Archetype Extension");
            SetMouseSelect(true);
        }

        private void ToolbarSelectTimeCycleModifierButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Time Cycle Modifier");
            SetMouseSelect(true);
        }

        private void ToolbarSelectCarGeneratorButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Car Generator");
            SetMouseSelect(true);
        }

        private void ToolbarSelectGrassButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Grass");
            SetMouseSelect(true);
        }

        private void ToolbarSelectWaterQuadButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Water Quad");
            SetMouseSelect(true);
        }

        private void ToolbarSelectCollisionButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Collision");
            SetMouseSelect(true);
        }

        private void ToolbarSelectNavMeshButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Nav Mesh");
            SetMouseSelect(true);
        }

        private void ToolbarSelectPathButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Path");
            SetMouseSelect(true);
        }

        private void ToolbarSelectTrainTrackButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Train Track");
            SetMouseSelect(true);
        }

        private void ToolbarSelectDistantLodLightsButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Distant Lod Lights");
            SetMouseSelect(true);
        }

        private void ToolbarSelectMloInstanceButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Mlo Instance");
            SetMouseSelect(true);
        }

        private void ToolbarSelectScenarioButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Scenario");
            SetMouseSelect(true);
        }

        private void ToolbarSelectAudioButton_Click(object sender, EventArgs e)
        {
            SetSelectionMode("Audio");
            SetMouseSelect(true);
        }

        private void ToolbarMoveButton_Click(object sender, EventArgs e)
        {
            SetWidgetMode(ToolbarMoveButton.Checked ? "Default" : "Position");
        }

        private void ToolbarRotateButton_Click(object sender, EventArgs e)
        {
            SetWidgetMode(ToolbarRotateButton.Checked ? "Default" : "Rotation");
        }

        private void ToolbarScaleButton_Click(object sender, EventArgs e)
        {
            SetWidgetMode(ToolbarScaleButton.Checked ? "Default" : "Scale");
        }

        private void ToolbarTransformSpaceButton_ButtonClick(object sender, EventArgs e)
        {
            SetWidgetSpace(Widget.ObjectSpace ? "World space" : "Object space");
        }

        private void ToolbarObjectSpaceButton_Click(object sender, EventArgs e)
        {
            SetWidgetSpace("Object space");
        }

        private void ToolbarWorldSpaceButton_Click(object sender, EventArgs e)
        {
            SetWidgetSpace("World space");
        }

        private void ToolbarSnapButton_ButtonClick(object sender, EventArgs e)
        {
            if (SnapMode == WorldSnapMode.None)
            {
                SetSnapMode(SnapModePrev);
            }
            else
            {
                SetSnapMode(WorldSnapMode.None);
            }
        }

        private void ToolbarSnapToGroundButton_Click(object sender, EventArgs e)
        {
            SetSnapMode(WorldSnapMode.Ground);
        }

        private void ToolbarSnapToGridButton_Click(object sender, EventArgs e)
        {
            SetSnapMode(WorldSnapMode.Grid);
        }

        private void ToolbarSnapToGroundGridButton_Click(object sender, EventArgs e)
        {
            SetSnapMode(WorldSnapMode.Hybrid);
        }

        private void ToolbarUndoButton_ButtonClick(object sender, EventArgs e)
        {
            Undo();
        }

        private void ToolbarUndoListButton_Click(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            if (tsi == null) return;
            var step = tsi.Tag as UndoStep;
            if (step == null) return;
            if (UndoSteps.Count == 0) return;
            var cstep = UndoSteps.Peek();
            while (cstep != null)
            {
                Undo();
                if (cstep == step) break;
                if (UndoSteps.Count == 0) break;
                cstep = UndoSteps.Peek();
            }
        }

        private void ToolbarRedoButton_ButtonClick(object sender, EventArgs e)
        {
            Redo();
        }

        private void ToolbarRedoListButton_Click(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            if (tsi == null) return;
            var step = tsi.Tag as UndoStep;
            if (step == null) return;
            if (RedoSteps.Count == 0) return;
            var cstep = RedoSteps.Peek();
            while (cstep != null)
            {
                Redo();
                if (cstep == step) break;
                if (RedoSteps.Count == 0) break;
                cstep = RedoSteps.Peek();
            }
        }

        private void ToolbarInfoWindowButton_Click(object sender, EventArgs e)
        {
            ShowInfoForm();
        }

        private void ToolbarProjectWindowButton_Click(object sender, EventArgs e)
        {
            ShowProjectForm();
        }

        private void ToolbarAddItemButton_Click(object sender, EventArgs e)
        {
            AddItem();
        }

        private void ToolbarDeleteItemButton_Click(object sender, EventArgs e)
        {
            DeleteItem();
        }

        private void ToolbarCopyButton_Click(object sender, EventArgs e)
        {
            CopyItem();
        }

        private void ToolbarPasteButton_Click(object sender, EventArgs e)
        {
            PasteItem();
        }

        private void ToolbarCameraModeButton_ButtonClick(object sender, EventArgs e)
        {
            ToggleCameraMode();
        }

        private void ToolbarCameraPerspectiveButton_Click(object sender, EventArgs e)
        {
            SetCameraMode("Perspective");
        }

        private void ToolbarCameraMapViewButton_Click(object sender, EventArgs e)
        {
            SetCameraMode("2D Map");
        }

        private void ToolbarCameraOrthographicButton_Click(object sender, EventArgs e)
        {
            SetCameraMode("Orthographic");
        }

        private void SelectionModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectionMode(SelectionModeComboBox.Text);
        }

        private void SelectionModeComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void ViewModeComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void WorldMaxLodComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void DlcLevelComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CameraModeComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void RenderModeComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TextureSamplerComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TextureCoordsComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void MarkerStyleComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void LocatorStyleComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void BoundsStyleComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void WeatherComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void WeatherRegionComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CloudsComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CloudParamComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void SnapGridSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            SnapGridSize = (float)SnapGridSizeUpDown.Value;
        }
    }


    public enum WorldControlMode
    {
        Free = 0,
        Ped = 1,
        Car = 2,
        Heli = 3,
        Plane = 4,
        Jetpack = 10,
    }

    public enum WorldSnapMode
    {
        None = 0,
        Grid = 1,
        Ground = 2,
        Hybrid = 3,
    }

}
