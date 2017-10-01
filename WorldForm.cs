using CodeWalker.GameFiles;
using CodeWalker.Properties;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using CodeWalker.Rendering;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using DriverType = SharpDX.Direct3D.DriverType;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using CodeWalker.World;
using System.Diagnostics;
using SharpDX;
using CodeWalker.Utils;
using System.Globalization;
using CodeWalker.Project;
using System.Collections.Specialized;
using SharpDX.XInput;

namespace CodeWalker
{
    public partial class WorldForm : Form, DXForm
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
        volatile bool running = false;
        volatile bool pauserendering = false;
        volatile bool initialised = false;

        Stopwatch frametimer = new Stopwatch();
        Camera camera = new Camera();
        Space space = new Space();
        Timecycle timecycle = new Timecycle();
        Weather weather = new Weather();
        Clouds clouds = new Clouds();
        Water water = new Water();
        Trains trains = new Trains();
        Scenarios scenarios = new Scenarios();
        PopZones popzones = new PopZones();


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
        bool rendertimedents = Settings.Default.ShowTimedEntities;
        bool rendertimedentsalways = false;
        bool renderinteriors = true;
        bool renderproxies = false;
        bool renderchildents = false;
        Vector3 prevworldpos = new Vector3(0, 0, 100); //also the start pos

        bool usedynamiclod = Settings.Default.DynamicLOD;
        float lodthreshold = 50.0f / (0.1f + (float)Settings.Default.DetailDist); //to match formula for the DetailTrackBar value
        bool waitforchildrentoload = true;

        public GameFileCache GameFileCache { get { return gameFileCache; } }
        GameFileCache gameFileCache = new GameFileCache();
        RenderableCache renderableCache = new RenderableCache();


        WorldControlMode ControlMode = WorldControlMode.Free;

        object MouseControlSyncRoot = new object();
        int MouseControlX = 0;
        int MouseControlY = 0;
        int MouseControlWheel = 0;
        MouseButtons MouseControlButtons = MouseButtons.None;
        MouseButtons MouseControlButtonsPrev = MouseButtons.None;

        bool ControlFireToggle = false;

        Entity camEntity = new Entity();
        PedEntity pedEntity = new PedEntity();
        volatile bool kbmovefwd = false;
        volatile bool kbmovebck = false;
        volatile bool kbmovelft = false;
        volatile bool kbmovergt = false;
        volatile bool kbmoveup = false;
        volatile bool kbmovedn = false;
        volatile bool kbjump = false;

        KeyBindings keyBindings = new KeyBindings(Settings.Default.KeyBindings);
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
        bool markerdepthclip = Settings.Default.MarkerDepthClip;
        object markersyncroot = new object();
        object markersortedsyncroot = new object();
        UnitQuad markerquad = null;

        BoundsShaderMode boundsmode = BoundsShaderMode.None;
        bool renderboundsclip = Settings.Default.BoundsDepthClip;
        float renderboundsmaxrad = 20000.0f;
        float renderboundsmaxdist = 10000.0f;
        List<MapBox> BoundingBoxes = new List<MapBox>();
        List<MapSphere> BoundingSpheres = new List<MapSphere>();
        List<MapBox> HilightBoxes = new List<MapBox>();
        List<MapBox> SelectionBoxes = new List<MapBox>();


        bool controllightdir = false; //if not, use timecycle
        float lightdirx = 2.25f;//radians // approx. light dir on map satellite view
        float lightdiry = 0.65f;//radians  - used for manual light placement
        bool renderskydome = Settings.Default.Skydome;

        bool rendercollisionmeshes = Settings.Default.ShowCollisionMeshes;
        List<BoundsStoreItem> collisionitems = new List<BoundsStoreItem>();
        int collisionmeshrange = Settings.Default.CollisionMeshRange;
        bool[] collisionmeshlayers = { true, true, true };
        bool collisionmeshlayerdrawable = true;

        List<YmapEntityDef> renderworldentities = new List<YmapEntityDef>();
        List<RenderableEntity> renderworldrenderables = new List<RenderableEntity>();
        Dictionary<MetaHash, YmapFile> renderworldVisibleYmapDict = new Dictionary<MetaHash, YmapFile>();
        Dictionary<uint, bool> renderworldHideFlags = new Dictionary<uint, bool>();
        Unk_1264241711 renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
        float renderworldLodDistMult = 1.0f;
        float renderworldDetailDistMult = 1.0f;

        bool worldymaptimefilter = true;
        bool worldymapweatherfilter = true;

        bool rendergrass = true;
        bool renderdistlodlights = true;
        bool rendernaturalambientlight = true;
        bool renderartificialambientlight = true;
        ShaderGlobalLights globalLights = new ShaderGlobalLights();

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


        float timeofday = 12.0f;
        bool controltimeofday = true;
        bool timerunning = false;
        float timespeed = 0.5f;//min/sec
        string weathertype = "";
        string individualcloudfrag = "contrails";

        Vector4 currentWindVec = Vector4.Zero;
        float currentWindTime = 0.0f;

        bool MapViewEnabled = false;
        float MapViewDetail = 1.0f;
        int MapViewDragX = 0;
        int MapViewDragY = 0;

        bool ShowScriptedYmaps = true;

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
        Dictionary<DrawableModel, bool> SelectionModelDrawFlags = new Dictionary<DrawableModel, bool>();
        Dictionary<DrawableGeometry, bool> SelectionGeometryDrawFlags = new Dictionary<DrawableGeometry, bool>();
        List<VertexTypePC> SelectionLineVerts = new List<VertexTypePC>();
        List<VertexTypePC> SelectionTriVerts = new List<VertexTypePC>();

        YmapEntityDef SelectedCarGenEntity = new YmapEntityDef();


        TransformWidget Widget = new TransformWidget();
        TransformWidget GrabbedWidget = null;
        bool ShowWidget = true;

        bool CtrlPressed = false;
        bool ShiftPressed = false;

        ProjectForm ProjectForm = null;

        Stack<UndoStep> UndoSteps = new Stack<UndoStep>();
        Stack<UndoStep> RedoSteps = new Stack<UndoStep>();
        Vector3 UndoStartPosition;
        Quaternion UndoStartRotation;
        Vector3 UndoStartScale;

        YmapEntityDef CopiedEntity = null;
        YmapCarGen CopiedCarGen = null;
        YndNode CopiedPathNode = null;
        YnvPoly CopiedNavPoly = null;
        TrainTrackNode CopiedTrainNode = null;
        ScenarioNode CopiedScenarioNode = null;

        public bool EditEntityPivot { get; set; } = false;

        SettingsForm SettingsForm = null;

        WorldSearchForm SearchForm = null;

        Controller xbcontroller = null;
        State xbcontrollerstate;
        State xbcontrollerstateprev;
        Vector4 xbmainaxes = Vector4.Zero;
        Vector4 xbmainaxesprev = Vector4.Zero;
        Vector2 xbtrigs = Vector2.Zero;
        Vector2 xbtrigsprev = Vector2.Zero;
        float xbcontrolvelocity = 0.0f;



        bool toolspanelexpanded = false;
        int toolspanellastwidth;
        bool toolsPanelResizing = false;
        int toolsPanelResizeStartX = 0;
        int toolsPanelResizeStartLeft = 0;
        int toolsPanelResizeStartRight = 0;

        double currentRealTime = 0;
        int framecount = 0;
        float fcelapsed = 0.0f;
        int fps = 0;

        bool initedOk = false;


        public WorldForm()
        {
            InitializeComponent();

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


            MouseWheel += WorldForm_MouseWheel;

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
                    //MessageBox.Show("No GTAV folder was chosen. CodeWalker will now exit.");
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


            if (Icons != null)
            {
                foreach (MapIcon icon in Icons)
                {
                    icon.LoadTexture(device, LogError);
                }
            }

            markerquad = new UnitQuad(device);

            renderableCache.OnDeviceCreated(device);

            camera.OnWindowResize(width, height); //init the projection stuff
            camera.FollowEntity = camEntity;
            camera.FollowEntity.Position = (startupviewmode!=2) ? prevworldpos : Vector3.Zero;// new Vector3(0.0f, 0.0f, 100.0f);
            camera.FollowEntity.Orientation = Quaternion.LookAtLH(Vector3.Zero, Vector3.Up, Vector3.ForwardLH);

            space.AddPersistentEntity(pedEntity);


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


            if (Icons != null)
            {
                foreach (MapIcon icon in Icons)
                {
                    icon.UnloadTexture();
                }
            }

            markerquad.Dispose();

            int count = 0;
            while (running && (count < 5000)) //wait for the content thread to exit gracefully
            {
                Thread.Sleep(1);
                count++;
            }

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

            clouds.Update(elapsed);

            UpdateWindVector(elapsed);

            UpdateGlobalLights();

            space.Update(elapsed);

            camera.SetMousePosition(MouseLastPoint.X, MouseLastPoint.Y);

            camera.Update(elapsed);

            UpdateWidgets();

            SelectionBoxes.Clear();
            HilightBoxes.Clear();
            BoundingBoxes.Clear();
            BoundingSpheres.Clear();
            BeginMouseHitTest();


            dxman.ClearRenderTarget(context);

            shaders.BeginFrame(context, currentRealTime, elapsed);

            shaders.EnsureShaderTextures(gameFileCache, renderableCache);

            RenderSky(context);

            RenderClouds(context);

            shaders.ClearDepth(context);

            if (renderworld || rendermaps)
            {
                RenderWorld();

                if (rendermaps)
                {
                    RenderYmaps();
                }
            }
            else
            {
                RenderSingleItem();
            }

            RenderSelection();

            shaders.RenderQueued(context, camera, currentWindVec);

            RenderBounds(context);

            RenderSelectionGeometry(context);

            RenderMoused(context);

            shaders.RenderFinalPass(context);

            RenderMarkers(context);

            RenderWidgets(context);

            renderableCache.RenderThreadSync();

            Monitor.Exit(rendersyncroot);

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

            if (controllightdir)
            {
                float cryd = (float)Math.Cos(lightdiry);
                lightdir.X = -(float)Math.Sin(-lightdirx) * cryd;
                lightdir.Y = -(float)Math.Cos(-lightdirx) * cryd;
                lightdir.Z = (float)Math.Sin(lightdiry);
                lightdircolour = Color4.White;
                lightdirambcolour = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
                if (hdr && (weather != null) && (weather.Inited))
                {
                    lightdircolour *= weather.CurrentValues.skyHdr;
                    lightdircolour.Alpha = 1.0f;
                    lightdirambcolour *= weather.CurrentValues.skyHdr;
                    lightdirambcolour.Alpha = 1.0f;
                    hdrint = weather.CurrentValues.skyHdr;
                }
                sundir = lightdir;
                moondir = -lightdir;
            }
            else
            {
                float sunroll = timecycle.sun_roll * (float)Math.PI / 180.0f;  //122
                float moonroll = timecycle.moon_roll * (float)Math.PI / 180.0f;  //-122
                float moonwobamp = timecycle.moon_wobble_amp; //0.2
                float moonwobfreq = timecycle.moon_wobble_freq; //2
                float moonwoboffs = timecycle.moon_wobble_offset; //0.375
                float dayval = (0.5f + (timeofday - 6.0f) / 14.0f);
                float nightval = (((timeofday>12.0f)?(timeofday - 7.0f):(timeofday+17.0f)) / 9.0f);
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
            globalLights.SpecularEnabled = !MapViewEnabled;//disable specular for map view.
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

            float moveSpeed = 50.0f;

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

                if (ControllerButtonJustPressed(GamepadButtonFlags.Start))
                {
                    SetControlMode(ControlMode == WorldControlMode.Free ? WorldControlMode.Ped : WorldControlMode.Free);
                }
            }


            if (ControlMode == WorldControlMode.Free)
            {

                Vector3 movevec = Vector3.Zero;

                if (MapViewEnabled == true)
                {
                    if (kbmovefwd) movevec.Y += 1.0f;
                    if (kbmovebck) movevec.Y -= 1.0f;
                    if (kbmovelft) movevec.X -= 1.0f;
                    if (kbmovergt) movevec.X += 1.0f;
                    if (kbmoveup) movevec.Y += 1.0f;
                    if (kbmovedn) movevec.Y -= 1.0f;
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
                    if (kbmovefwd) movevec.Z -= 1.0f;
                    if (kbmovebck) movevec.Z += 1.0f;
                    if (kbmovelft) movevec.X -= 1.0f;
                    if (kbmovergt) movevec.X += 1.0f;
                    if (kbmoveup) movevec.Y += 1.0f;
                    if (kbmovedn) movevec.Y -= 1.0f;
                    movevec *= elapsed * moveSpeed * Math.Min(camera.TargetDistance, 20.0f);
                }


                Vector3 movewvec = camera.ViewInvQuaternion.Multiply(movevec);
                camEntity.Position += movewvec;

                MapViewDragX = 0;
                MapViewDragY = 0;




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
                    if (ControllerButtonJustPressed(GamepadButtonFlags.LeftShoulder))
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

                if (xbenable)
                {
                    camera.ControllerRotate(rx, ry);
                }



                Vector2 movecontrol = new Vector2(xbmainaxes.X, xbmainaxes.Y); //(L stick)
                if (kbmovelft) movecontrol.X -= 1.0f;
                if (kbmovergt) movecontrol.X += 1.0f;
                if (kbmovefwd) movecontrol.Y += 1.0f;
                if (kbmovebck) movecontrol.Y -= 1.0f;
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
                pedEntity.ControlJump = kbjump || ControllerButtonPressed(GamepadButtonFlags.X);
                pedEntity.ControlBoost = ShiftPressed || ControllerButtonPressed(GamepadButtonFlags.A | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.LeftShoulder);


                //Vector3 pedfwd = pedEntity.Orientation.Multiply(Vector3.UnitZ);






                bool fire = mlb || (xbtrigs.Y > 0);
                if (fire && !ControlFireToggle)
                {
                    SpawnTestEntity(true);
                }
                ControlFireToggle = fire;


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



        private DrawableBase TryGetDrawable(Archetype arche)
        {
            if (arche == null) return null;
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

            Renderable rndbl = renderableCache.GetRenderable(drawable);
            if (rndbl == null) return null;

            if (clipDict != 0)
            {
                YcdFile ycd = gameFileCache.GetYcd(clipDict);
                if ((ycd != null) && (ycd.Loaded))
                {
                    MetaHash ahash = arche.Hash;
                    MetaHash ahashuv1 = ahash + 1;
                    MetaHash ahashuv2 = ahash + 2;
                    ClipMapEntry cme, cmeuv1, cmeuv2; //this goes to at least uv5! (from uv0) - see hw1_09.ycd
                    bool found = false;
                    if (ycd.ClipMap.TryGetValue(ahash, out cme))
                    {
                        found = true;
                    }
                    if (ycd.ClipMap.TryGetValue(ahashuv1, out cmeuv1))
                    {
                        found = true;
                    }
                    if (ycd.ClipMap.TryGetValue(ahashuv2, out cmeuv2))
                    {
                        found = true;
                    }
                    if (!found)
                    {
                    }
                }
            }


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
                                if (texDict != 0)
                                {
                                    YtdFile ytd = gameFileCache.GetYtd(texDict);
                                    if ((ytd != null) && (ytd.Loaded) && (ytd.TextureDict != null))
                                    {
                                        var dtex = ytd.TextureDict.Lookup(tex.NameHash);
                                        if (dtex == null)
                                        {
                                            //not present in dictionary... check already loaded texture dicts...
                                            var ytd2 = gameFileCache.TryGetTextureDictForTexture(tex.NameHash);
                                            if ((ytd2 != null) && (ytd2.Loaded) && (ytd2.TextureDict != null))
                                            {
                                                dtex = ytd2.TextureDict.Lookup(tex.NameHash);
                                            }
                                            else
                                            {
                                                //couldn't find texture dict?
                                                //first try going through ytd hierarchy...
                                                dtex = gameFileCache.TryFindTextureInParent(tex.NameHash, texDict);


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
                                            var ytd2 = gameFileCache.TryGetTextureDictForTexture(tex.NameHash);
                                            if ((ytd2 != null) && (ytd2.Loaded) && (ytd2.TextureDict != null))
                                            {
                                                dtex = ytd2.TextureDict.Lookup(tex.NameHash);
                                            }
                                            if (dtex == null)
                                            {
                                                dtex = gameFileCache.TryFindTextureInParent(tex.NameHash, texDict);
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
                                    var ytd2 = gameFileCache.TryGetTextureDictForTexture(tex.NameHash);
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

        private bool LodDistTest(ref Vector3 p, ref Vector3 min, ref Vector3 max, float lodDist)
        {
            //for AABB only! for oriented BBs p must be transformed first...
            if (p.X < (min.X - lodDist)) return false;
            if (p.X > (max.X + lodDist)) return false;
            if (p.Y < (min.Y - lodDist)) return false;
            if (p.Y > (max.Y + lodDist)) return false;
            if (p.Z < (min.Z - lodDist)) return false;
            if (p.Z > (max.Z + lodDist)) return false;
            return true;
        }
        private bool LodDistTest(Vector3 p, Vector3 min, Vector3 max, float lodDist)
        {
            //for AABB only! for oriented BBs p must be transformed first...
            if (p.X < (min.X - lodDist)) return false;
            if (p.X > (max.X + lodDist)) return false;
            if (p.Y < (min.Y - lodDist)) return false;
            if (p.Y > (max.Y + lodDist)) return false;
            if (p.Z < (min.Z - lodDist)) return false;
            if (p.Z > (max.Z + lodDist)) return false;
            return true;
        }



        private void RenderSky(DeviceContext context)
        {
            if (MapViewEnabled) return;
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
            Texture moon = null;
            YtdFile skydomeytd = gameFileCache.GetYtd(2640562617); //skydome hash
            if ((skydomeytd != null) && (skydomeytd.Loaded) && (skydomeytd.TextureDict != null) && (skydomeytd.TextureDict.Dict != null))
            {
                skydomeytd.TextureDict.Dict.TryGetValue(1064311147, out starfield); //starfield hash
                skydomeytd.TextureDict.Dict.TryGetValue(234339206, out moon); //moon-new hash
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

            RenderableTexture moontex = null;
            if (moon != null)
            {
                moontex = renderableCache.GetRenderableTexture(moon);
            }

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


                if ((moontex != null) && (moontex.IsLoaded))
                {
                    shader.RenderMoon(context, camera, weather, globalLights, moontex);
                }



                shader.UnbindResources(context);
            }

        }

        private void RenderClouds(DeviceContext context)
        {
            if (MapViewEnabled) return;
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


        private void RenderWorld()
        {
            //start point for world view mode rendering.
            //also used for the water, paths, collisions, nav mesh, and the project window items.

            renderworldentities.Clear();
            renderworldrenderables.Clear();
            renderworldVisibleYmapDict.Clear();
            renderworldHideFlags.Clear();


            int hour = worldymaptimefilter ? (int)timeofday : -1;
            MetaHash weathertype = worldymapweatherfilter ? ((weather.CurrentWeatherType != null) ? weather.CurrentWeatherType.NameHash : new MetaHash(0)) : new MetaHash(0);

            if (renderworld)
            {
                space.GetVisibleYmaps(camera, hour, weathertype, renderworldVisibleYmapDict);

                foreach (var ae in space.TemporaryEntities)
                {
                    if (ae.EntityDef == null) continue; //nothing to render...
                    RenderWorldCalcEntityVisibility(camera, ae.EntityDef);
                    renderworldentities.Add(ae.EntityDef);
                }
            }

            if (ProjectForm != null)
            {
                ProjectForm.GetVisibleYmaps(camera, renderworldVisibleYmapDict);
            }

            //float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            float cvwidth = camera.OrthographicSize * camera.AspectRatio*0.5f;
            float cvheight = camera.OrthographicSize*0.5f;
            float cvwmin = camera.Position.X - cvwidth;
            float cvwmax = camera.Position.X + cvwidth;
            float cvhmin = camera.Position.Y - cvheight;
            float cvhmax = camera.Position.Y + cvheight;

            foreach (var ymap in renderworldVisibleYmapDict.Values)
            {
                if (!RenderWorldYmapIsVisible(ymap)) continue;
                if (ymap.AllEntities != null)
                {
                    for (int i = 0; i < ymap.AllEntities.Length; i++)
                    {
                        var ent = ymap.AllEntities[i];
                        ent.LargestChildLodDist = 0;
                        ent.ChildrenLoading = false;
                        ent.Rendered = false;
                        ent.ChildRendered = false;
                        if (MapViewEnabled)
                        {
                            //find the max Z value for positioning camera in map view, to help shadows
                            if ((ent.Position.Z<1000.0f)&&(ent.BSRadius < 500.0f))
                            {
                                float r = ent.BSRadius;
                                if (((ent.Position.X + r) > cvwmin) && ((ent.Position.X - r) < cvwmax) && ((ent.Position.Y + r) > cvhmin) && ((ent.Position.Y - r) < cvhmax))
                                {
                                    //minZ = Math.Min(minZ, ent.BBMin.Z);
                                    maxZ = Math.Max(maxZ, ent.BBMax.Z);
                                }
                            }
                        }
                    }
                }
            }

            if (MapViewEnabled)
            {
                //move the camera closer to the geometry, to help shadows in map view.
                if (maxZ == float.MinValue) maxZ = 1000.0f;
                camera.Position.Z = Math.Min(maxZ, 1000.0f); 
            }


            foreach (var ymap in renderworldVisibleYmapDict.Values)
            {
                if (!RenderWorldYmapIsVisible(ymap)) continue;
                if (ymap.RootEntities != null)
                {
                    YmapFile pymap;
                    renderworldVisibleYmapDict.TryGetValue(ymap.CMapData.parent, out pymap);
                    for (int i = 0; i < ymap.RootEntities.Length; i++)
                    {
                        var ent = ymap.RootEntities[i];
                        int pind = ent.CEntityDef.parentIndex;
                        if (pind >= 0) //connect root entities to parents if they have them..
                        {
                            YmapEntityDef p = null;
                            if ((pymap != null) && (pymap.AllEntities != null))
                            {
                                if ((pind < pymap.AllEntities.Length))
                                {
                                    p = pymap.AllEntities[pind];
                                    ent.Parent = p;
                                    ent.ParentGuid = p.CEntityDef.guid;
                                    ent.ParentName = p.CEntityDef.archetypeName;
                                }
                            }
                            else
                            { }//should only happen if parent ymap not loaded yet...
                        }
                        RenderWorldRecurseCalcEntityVisibility(camera, ent);
                    }
                }
            }

            foreach (var ymap in renderworldVisibleYmapDict.Values)
            {
                if (!RenderWorldYmapIsVisible(ymap)) continue;
                if (ymap.RootEntities != null)
                {
                    for (int i = 0; i < ymap.RootEntities.Length; i++)
                    {
                        var ent = ymap.RootEntities[i];
                        RenderWorldRecurseAddEntities(ent, renderworldentities);
                    }
                }

                UpdateMouseHits(ymap);
            }


            //go through the render list, and try ensure renderables and textures for all.
            //if an entity is not fully loaded, set a flag for its parent, then traverse to root
            //until found an entity that is fully loaded.
            //on a second loop, build a final render list based on the flags.

            for (int i = 0; i < renderworldentities.Count; i++)
            {
                var ent = renderworldentities[i];
                var arch = ent.Archetype;
                var pent = ent.Parent;

                if (renderinteriors && ent.IsMlo) //render Mlo child entities...
                {
                    if ((ent.MloInstance != null) && (ent.MloInstance.Entities != null))
                    {
                        for (int j = 0; j < ent.MloInstance.Entities.Length; j++)
                        {
                            var intent = ent.MloInstance.Entities[j];
                            var intarch = intent.Archetype;
                            if (intarch == null) continue; //missing archetype...

                            if (!RenderIsEntityFinalRender(intent)) continue; //proxy or something..

                            intent.CamRel = intent.Position - camera.Position;
                            intent.Distance = intent.CamRel.Length();
                            intent.IsVisible = true;

                            var bscent = intent.CamRel + intent.BSCenter;
                            float bsrad = intent.BSRadius;
                            if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref bscent, bsrad))
                            {
                                continue; //frustum cull interior ents
                            }
                            var intdrbl = TryGetDrawable(intarch);
                            var intrndbl = TryGetRenderable(intarch, intdrbl);
                            if (intrndbl == null) continue; //no renderable
                            if (!(intrndbl.IsLoaded && (intrndbl.AllTexturesLoaded || !waitforchildrentoload))) continue; //not loaded yet

                            RenderableEntity intrent = new RenderableEntity();
                            intrent.Entity = intent;
                            intrent.Renderable = intrndbl;
                            renderworldrenderables.Add(intrent);
                        }
                    }
                    if (rendercollisionmeshes)
                    {
                        RenderInteriorCollisionMesh(ent);
                    }
                }

                ent.Rendered = true;
                var drawable = TryGetDrawable(arch);
                Renderable rndbl = TryGetRenderable(arch, drawable);
                if ((rndbl != null) && rndbl.IsLoaded && (rndbl.AllTexturesLoaded || !waitforchildrentoload))
                {
                    RenderableEntity rent = new RenderableEntity();
                    rent.Entity = ent;
                    rent.Renderable = rndbl;
                    renderworldrenderables.Add(rent);

                    if (pent != null)
                    {
                        pent.ChildRendered = true;
                    }
                }
                else if (waitforchildrentoload)
                {
                    //todo: render parent if children loading.......
                }
            }

            for (int i = 0; i < renderworldrenderables.Count; i++)
            {
                var ent = renderworldrenderables[i].Entity;
                if (ent.ChildRendered && !ent.ChildrenLoading)
                {
                    ent.ChildrenRendered = true;
                }
            }

            for (int i = 0; i < renderworldrenderables.Count; i++)
            {
                var rent = renderworldrenderables[i];
                var ent = rent.Entity;
                var arch = ent.Archetype;

                RenderArchetype(arch, ent, rent.Renderable, false);
            }



            if (rendergrass)
            {
                foreach (var ymap in renderworldVisibleYmapDict.Values)
                {
                    if (ymap.GrassInstanceBatches != null)
                    {
                        RenderYmapGrass(ymap);
                    }
                }
            }
            if (renderdistlodlights && timecycle.IsNightTime)
            {
                foreach (var ymap in renderworldVisibleYmapDict.Values)
                {
                    if (ymap.DistantLODLights != null)
                    {
                        RenderYmapDistantLODLights(ymap);
                    }
                }
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
        }
        private bool RenderWorldYmapIsVisible(YmapFile ymap)
        {
            if (!ShowScriptedYmaps)
            {
                if ((ymap._CMapData.flags & 1) > 0)
                    return false;
            }

            return true;
        }
        private void RenderWorldCalcEntityVisibility(Camera cam, YmapEntityDef ent)
        {
            ent.CamRel = ent.Position - cam.Position;
            ent.Distance = ent.CamRel.Length();
            float distval = ent.Distance;

            if (MapViewEnabled)
            {
                distval = cam.OrthographicSize / MapViewDetail;
            }


            var loddist = ent.CEntityDef.lodDist;
            var cloddist = ent.CEntityDef.childLodDist;

            var loddistmultdef = renderworldLodDistMult * 1.0f;
            var loddistmultorph = renderworldDetailDistMult * 1.5f;
            var loddistmultarch = renderworldLodDistMult * 1.0f;

            if (loddist <= 0.0f)//usually -1 or -2
            {
                if (ent.Archetype != null)
                {
                    loddist = ent.Archetype.LodDist * loddistmultarch;
                }
            }
            else if (ent.CEntityDef.lodLevel == Unk_1264241711.LODTYPES_DEPTH_ORPHANHD)
            {
                loddist *= loddistmultorph; //orphan view dist adjustment...
            }
            else
            {
                loddist *= loddistmultdef;
            }


            if (cloddist <= 0)
            {
                if (ent.Archetype != null)
                {
                    cloddist = ent.Archetype.LodDist * loddistmultarch;
                    //cloddist = ent.Archetype.BSRadius * 50.0f;
                }
            }
            else
            {
                cloddist *= loddistmultdef;
            }
            if (cloddist == 0)
            {
                //cloddist = loddist;//always try to show children, based on their loddist
            }


            ent.IsVisible = (distval <= loddist);
            ent.ChildrenVisible = (distval <= cloddist) && (ent.CEntityDef.numChildren > 0);
            ent.ChildrenLoading = false;

            if ((ent.Parent != null) && (ent.CEntityDef.lodLevel != Unk_1264241711.LODTYPES_DEPTH_ORPHANHD))
            {
                if (ent.Parent.CEntityDef.childLodDist == 0.0f)
                {
                    ent.Parent.LargestChildLodDist = Math.Max(ent.Parent.LargestChildLodDist, loddist);
                }
            }

            if (renderworldMaxLOD != Unk_1264241711.LODTYPES_DEPTH_ORPHANHD)
            {
                if ((ent.CEntityDef.lodLevel == Unk_1264241711.LODTYPES_DEPTH_ORPHANHD) ||
                    (ent.CEntityDef.lodLevel < renderworldMaxLOD))
                {
                    ent.IsVisible = false;
                    ent.ChildrenVisible = false;
                }
                if (ent.CEntityDef.lodLevel == renderworldMaxLOD)
                {
                    ent.ChildrenVisible = false;
                }
            }

            if (!ent.IsVisible)
            {
                ent.ChildrenRendered = false;
            }
        }
        private void RenderWorldRecurseCalcEntityVisibility(Camera cam, YmapEntityDef ent)
        {
            RenderWorldCalcEntityVisibility(cam, ent);
            if (ent.ChildrenVisible)
            {
                if (ent.Children != null)
                {
                    for (int i = 0; i < ent.Children.Length; i++)
                    {
                        var child = ent.Children[i];
                        if (child.Ymap == ent.Ymap)
                        {
                            RenderWorldRecurseCalcEntityVisibility(cam, child);
                        }
                    }
                }
            }
        }
        private void RenderWorldRecurseAddEntities(YmapEntityDef ent, List<YmapEntityDef> res)
        {
            //bool useclod = false; //(ent.CEntityDef.childLodDist == 0.0f);
            //bool hide = useclod ? (ent.AnyChildVisible /*&& !ent.AnyChildInvisible*/) : ent.ChildrenVisible;
            //bool force = !useclod && (ent.Parent != null) && ent.Parent.ChildrenVisible && !hide;
            bool hide = ent.ChildrenVisible;
            bool force = (ent.Parent != null) && ent.Parent.ChildrenVisible && !hide;
            if (force || (ent.IsVisible && !hide))
            {
                if (ent.Archetype != null)
                {
                    if (!RenderIsEntityFinalRender(ent)) return;

                    var bscent = ent.CamRel + ent.BSCenter;
                    float bsrad = ent.BSRadius;
                    if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref bscent, bsrad))
                    {
                        return; //frustum cull
                    }


                    res.Add(ent);
                }
                else
                { }
            }
            if (ent.IsVisible && ent.ChildrenVisible && (ent.Children != null))
            {
                for (int i = 0; i < ent.Children.Length; i++)
                {
                    var child = ent.Children[i];
                    if (child.Ymap == ent.Ymap)
                    {
                        RenderWorldRecurseAddEntities(ent.Children[i], res);
                    }
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

            foreach (var quad in renderwaterquadlist)
            {
                RenderableWaterQuad rquad = renderableCache.GetRenderableWaterQuad(quad);
                if ((rquad != null) && (rquad.IsLoaded))
                {
                    rquad.CamRel = -camera.Position;
                    shaders.Enqueue(rquad);
                }
            }

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

            foreach (var ynd in renderpathynds)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(ynd);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }

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

            foreach (var track in rendertraintracklist)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(track);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }

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


            foreach (var ynv in rendernavmeshynvs)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(ynv);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }

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

            foreach (var scenario in renderscenariolist)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(scenario.ScenarioRegion);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }

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


            RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(popzones);
            if ((rnd != null) && (rnd.IsLoaded))
            {
                shaders.Enqueue(rnd);
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
                RenderArchetype(arche, null);

                selarch = arche;
            }
            else
            {
                YmapFile ymap = gameFileCache.GetYmap(hash);
                if (ymap != null)
                {
                    RenderYmap(ymap);
                }
                else
                {
                    //not a ymap... see if it's a ydr or yft
                    YdrFile ydr = gameFileCache.GetYdr(hash);
                    if (ydr != null)
                    {
                        if (ydr.Loaded)
                        {
                            RenderDrawable(ydr.Drawable, null, null, -camera.Position, hash);

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

                                    RenderFragment(null, null, f, hash);

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
                seldrwbl = TryGetDrawable(selarch);
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
                RenderYmap(ymap);
            }
        }
        private void RenderYmap(YmapFile ymap)
        {
            if (ymap == null) return;
            if (!ymap.Loaded) return;

            UpdateMouseHits(ymap);

            if ((ymap.AllEntities != null) && (ymap.RootEntities != null))
            {
                if (usedynamiclod)
                {
                    for (int i = 0; i < ymap.RootEntities.Length; i++)
                    {
                        RenderYmapLOD(ymap.RootEntities[i].Ymap, ymap.RootEntities[i]);
                    }
                }
                else
                {
                    var ents = renderchildents ? ymap.AllEntities : ymap.RootEntities;
                    for (int i = 0; i < ents.Length; i++)
                    {
                        var ent = ents[i];
                        if (renderchildents && ent.Children != null) continue;
                        //if (rootent.CEntityDef.parentIndex == -1) continue;
                        Archetype arch = ent.Archetype;
                        if (arch != null)
                        {
                            bool timed = (arch.Type == MetaName.CTimeArchetypeDef);
                            if (!timed || (rendertimedents && (rendertimedentsalways || arch.IsActive(timeofday))))
                            {
                                ent.CamRel = ent.Position - camera.Position;
                                RenderArchetype(arch, ent);
                            }
                        }
                        else
                        {
                            //couldn't find archetype...
                        }
                    }
                }
            }

            if (rendergrass && (ymap.GrassInstanceBatches != null))
            {
                RenderYmapGrass(ymap);
            }
            if (renderdistlodlights && timecycle.IsNightTime && (ymap.DistantLODLights != null))
            {
                RenderYmapDistantLODLights(ymap);
            }

        }
        private bool RenderYmapLOD(YmapFile ymap, YmapEntityDef entity)
        {
            if (!ymap.Loaded) return false;

            ymap.EnsureChildYmaps(gameFileCache);

            Archetype arch = entity.Archetype;
            if (arch != null)
            {
                bool timed = (arch.Type == MetaName.CTimeArchetypeDef);
                if (!timed || (rendertimedents && (rendertimedentsalways || arch.IsActive(timeofday))))
                {
                    bool usechild = false;
                    entity.CamRel = entity.Position - camera.Position;
                    float dist = (entity.CamRel + entity.BSCenter).Length();
                    float rad = arch.BSRadius;
                    float loddist = entity.CEntityDef.lodDist;
                    if (loddist < 1.0f)
                    {
                        loddist = 200.0f;
                    }
                    float mindist = Math.Max(dist - rad, 1.0f) * lodthreshold;
                    if (mindist < loddist)
                    {
                        //recurse...
                        var children = entity.ChildrenMerged;
                        if ((children != null))
                        {
                            usechild = true;
                            for (int i = 0; i < children.Length; i++)
                            {
                                var childe = children[i];
                                if (!RenderYmapLOD(childe.Ymap, childe))
                                {
                                    if (waitforchildrentoload)
                                    {
                                        usechild = false; //might cause some overlapping, but should reduce things disappearing
                                    }
                                }
                            }
                        }
                        if (!entity.ChildrenRendered)
                        {
                            entity.ChildrenRendered = usechild;
                        }
                    }
                    else
                    {
                        entity.ChildrenRendered = false;
                    }
                    if (!usechild && !entity.ChildrenRendered)
                    {



                        if (renderinteriors && entity.IsMlo) //render Mlo child entities...
                        {
                            if ((entity.MloInstance != null) && (entity.MloInstance.Entities != null))
                            {
                                for (int j = 0; j < entity.MloInstance.Entities.Length; j++)
                                {
                                    var intent = entity.MloInstance.Entities[j];
                                    var intarch = intent.Archetype;
                                    if (intarch == null) continue; //missing archetype...

                                    if (!RenderIsEntityFinalRender(intent)) continue; //proxy or something..

                                    intent.CamRel = intent.Position - camera.Position;
                                    intent.Distance = intent.CamRel.Length();
                                    intent.IsVisible = true;

                                    RenderArchetype(intarch, intent);
                                }
                            }
                            if (rendercollisionmeshes)
                            {
                                RenderInteriorCollisionMesh(entity);
                            }
                        }




                        return RenderArchetype(arch, entity);
                    }
                    return true;
                }

            }
            return false;
        }


        private void RenderYmapGrass(YmapFile ymap)
        {
            //enqueue ymap grass instance batches for rendering

            if (ymap.GrassInstanceBatches == null) return;

            foreach (var batch in ymap.GrassInstanceBatches)
            {
                batch.CamRel = batch.Position - camera.Position;
                //batch.Distance = batch.CamRel.Length();

                float lodDist = batch.Batch.lodDist * renderworldDetailDistMult;//maybe add grass dist mult
                //if (batch.Distance > lodDist) continue; //too far away..

                lodDist *= 0.75f; //reduce it just a bit to improve performance... remove this later

                float cx = camera.Position.X;
                float cy = camera.Position.Y;
                float cz = camera.Position.Z;
                if (cx < (batch.AABBMin.X - lodDist)) continue;
                if (cx > (batch.AABBMax.X + lodDist)) continue;
                if (cy < (batch.AABBMin.Y - lodDist)) continue;
                if (cy > (batch.AABBMax.Y + lodDist)) continue;
                if (cz < (batch.AABBMin.Z - lodDist)) continue;
                if (cz > (batch.AABBMax.Z + lodDist)) continue;


                var bscent = batch.CamRel;
                float bsrad = batch.Radius;
                if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref bscent, bsrad))
                {
                    continue; //frustum cull grass batches...
                }

                var arch = batch.Archetype;
                var drbl = TryGetDrawable(arch);
                var rndbl = TryGetRenderable(arch, drbl);
                var instb = renderableCache.GetRenderableInstanceBatch(batch);
                if (rndbl == null) continue; //no renderable
                if (!(rndbl.IsLoaded && (rndbl.AllTexturesLoaded || !waitforchildrentoload))) continue; //not loaded yet
                if ((instb == null) || !instb.IsLoaded) continue;

                RenderableInstanceBatchInst binst = new RenderableInstanceBatchInst();
                binst.Batch = instb;
                binst.Renderable = rndbl;

                shaders.Enqueue(binst);

            }

        }

        private void RenderYmapDistantLODLights(YmapFile ymap)
        {
            //enqueue ymap DistantLODLights instance batch for rendering

            if (ymap.DistantLODLights == null) return;

            switch (ymap.DistantLODLights.CDistantLODLight.category)
            {
                case 0: //distlodlights_small009.ymap
                case 1: //distlodlights_medium000.ymap
                case 2: //distlodlights_large000.ymap
                    break;
                default:
                    break;
            }

            RenderableDistantLODLights lights = renderableCache.GetRenderableDistantLODLights(ymap.DistantLODLights);
            if (!lights.IsLoaded) return;


            uint ytdhash = 3154743001; //"graphics"
            uint texhash = 2236244673; //"distant_light"
            YtdFile graphicsytd = gameFileCache.GetYtd(ytdhash);
            Texture lighttex = null;
            if ((graphicsytd != null) && (graphicsytd.Loaded) && (graphicsytd.TextureDict != null) && (graphicsytd.TextureDict.Dict != null))
            {
                graphicsytd.TextureDict.Dict.TryGetValue(texhash, out lighttex); //starfield hash
            }

            if (lighttex == null) return;
            RenderableTexture lightrtex = null;
            if (lighttex != null)
            {
                lightrtex = renderableCache.GetRenderableTexture(lighttex);
            }
            if (lightrtex == null) return;
            if (!lightrtex.IsLoaded) return;

            lights.Texture = lightrtex;

            shaders.Enqueue(lights);
        }




        private bool RenderIsEntityFinalRender(YmapEntityDef ent)
        {
            var arch = ent.Archetype;
            bool isshadowproxy = false;
            bool isreflproxy = false;
            uint archflags = arch._BaseArchetypeDef.flags;
            if (arch.Type == MetaName.CTimeArchetypeDef)
            {
                if (!(rendertimedents && (rendertimedentsalways || arch.IsActive(timeofday)))) return false;
                //archflags = arch._BaseArchetypeDef.flags;
            }
            //else if (arch.Type == MetaName.CMloArchetypeDef)
            //{
            //    archflags = arch._BaseArchetypeDef.flags;
            //}
            ////switch (archflags)
            ////{
            ////    //case 8192:  //8192: is YTYP no shadow rendering  - CP
            ////    case 2048:      //000000000000000000100000000000  shadow proxies...
            ////    case 536872960: //100000000000000000100000000000    tunnel refl/shadow prox?
            ////        isshadowproxy = true; break;
            ////}
            if ((archflags & 2048) > 0)
            {
                isshadowproxy = true;
            }

            //if ((ent.CEntityDef.flags & 1572864) == 1572864)
            //{
            //    isreflproxy = true;
            //}

            switch (ent._CEntityDef.flags)
            {
                case 135790592: //001000000110000000000000000000    prewater proxy (golf course)
                case 135790593: //001000000110000000000000000001    water refl proxy? (mike house)
                case 672661504: //101000000110000000000000000000    vb_ca_prop_tree_reflprox_2
                case 536870912: //100000000000000000000000000000    vb_05_emissive_mirroronly
                case 35127296:  //000010000110000000000000000000    tunnel refl proxy?
                case 39321602:  //000010010110000000000000000010    mlo reflection?
                    isreflproxy = true; break;
                //nonproxy is:  //000000000110000000000000001000   (1572872)
                //              //000000000110000000000000000000
            }
            if (isshadowproxy || isreflproxy)
            {
                return renderproxies; //filter out proxy entities...
            }
            return true;
        }
        private bool RenderIsModelFinalRender(RenderableModel model)
        {

            if ((model.Unk2Ch & 1) == 0) //smallest bit is proxy/"final render" bit? seems to work...
            {
                return renderproxies;
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




        private bool RenderFragment(Archetype arch, YmapEntityDef ent, FragType f, uint txdhash=0)
        {
            var pos = ent?.Position ?? Vector3.Zero;

            RenderDrawable(f.Drawable, arch, ent, pos-camera.Position, txdhash);

            if (f.Unknown_F8h_Data != null) //cloth
            {
                RenderDrawable(f.Unknown_F8h_Data, arch, ent, pos-camera.Position, txdhash);
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
                            //RenderDrawable(pch.Drawable1, arch, ent, -camera.Position, hash);
                        }
                    }
                }
            }

            return true;
        }

        private bool RenderArchetype(Archetype arche, YmapEntityDef entity, Renderable rndbl = null, bool cull = true)
        {
            //enqueue a single archetype for rendering.

            if (arche == null) return false;

            Vector3 camrel = (entity != null) ? entity.CamRel : -camera.Position;

            Quaternion orientation = Quaternion.Identity;
            Vector3 scale = Vector3.One;
            Vector3 bscent = camrel;
            if (entity != null)
            {
                orientation = entity.Orientation;
                scale = entity.Scale;
                bscent += entity.BSCenter;
            }
            else
            {
                bscent += arche.BSCenter;
            }

            float bsrad = arche.BSRadius;// * scale;
            if (cull)
            {
                if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref bscent, bsrad))
                {
                    return true; //culled - not visible; don't render, but pretend we did for LOD purposes..
                }
            }

            float dist = bscent.Length();

            if (boundsmode == BoundsShaderMode.Sphere)
            {
                if ((bsrad < renderboundsmaxrad) && (dist < renderboundsmaxdist))
                {
                    MapSphere ms = new MapSphere();
                    ms.CamRelPos = bscent;
                    ms.Radius = bsrad;
                    BoundingSpheres.Add(ms);
                }
            }
            if (boundsmode == BoundsShaderMode.Box)
            {
                if ((dist < renderboundsmaxdist))
                {
                    MapBox mb = new MapBox();
                    mb.CamRelPos = camrel;
                    mb.BBMin = arche.BBMin;
                    mb.BBMax = arche.BBMax;
                    mb.Orientation = orientation;
                    mb.Scale = scale;
                    BoundingBoxes.Add(mb);
                }
            }



            bool res = false;
            if (rndbl == null)
            {
                var drawable = TryGetDrawable(arche);
                rndbl = TryGetRenderable(arche, drawable);
            }

            if (rndbl != null)
            {
                res = RenderRenderable(rndbl, arche, entity, camrel);


                //fragments have extra drawables! need to render those too... TODO: handle fragments properly...
                FragDrawable fd = rndbl.Key as FragDrawable;
                if (fd != null)
                {
                    var frag = fd.OwnerFragment;
                    if ((frag != null) && (frag.Unknown_F8h_Data != null)) //cloth...
                    {
                        rndbl = TryGetRenderable(arche, frag.Unknown_F8h_Data);
                        if (rndbl != null)
                        {
                            bool res2 = RenderRenderable(rndbl, arche, entity, camrel);
                            res = res || res2;
                        }
                    }
                }
            }


            return res;
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


            if (((SelectionMode == MapSelectionMode.Entity) || (SelectionMode == MapSelectionMode.EntityExtension) || (SelectionMode == MapSelectionMode.ArchetypeExtension)))
            {
                UpdateMouseHit(rndbl, arche, entity, camrel);
            }

            bool isselected = (rndbl.Key == SelectedItem.Drawable);

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


            if (rendercollisionmeshes && collisionmeshlayerdrawable)
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
                        if (SelectionModelDrawFlags.ContainsKey(model.DrawableModel))
                        { continue; } //filter out models in selected item that aren't flagged for drawing.
                    }

                    if (!RenderIsModelFinalRender(model) && !renderproxies)
                    { continue; } //filter out reflection proxy models...

                    for (int gi = 0; gi < model.Geometries.Length; gi++)
                    {
                        var geom = model.Geometries[gi];

                        if (isselected)
                        {
                            if (SelectionGeometryDrawFlags.ContainsKey(geom.DrawableGeom))
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




        private void RenderCar(Vector3 pos, Quaternion ori, MetaHash modelHash, MetaHash modelSetHash)
        {
            SelectedCarGenEntity.SetPosition(pos);
            SelectedCarGenEntity.SetOrientation(ori);

            uint carhash = modelHash;
            if ((carhash == 0) && (modelSetHash != 0))
            {
                //find the pop group... and choose a vehicle..
                var stypes = Scenarios.ScenarioTypes;
                if (stypes != null)
                {
                    var modelset = stypes.GetVehicleModelSet(modelSetHash);
                    if ((modelset != null) && (modelset.Models != null) && (modelset.Models.Length > 0))
                    {
                        carhash = JenkHash.GenHash(modelset.Models[0].NameLower);
                    }
                }
            }
            if (carhash == 0) carhash = 418536135; //"infernus"

            YftFile caryft = gameFileCache.GetYft(carhash);
            if ((caryft != null) && (caryft.Loaded) && (caryft.Fragment != null))
            {
                RenderFragment(null, SelectedCarGenEntity, caryft.Fragment, carhash);
            }
        }





        private void RenderWorldCollisionMeshes()
        {
            //enqueue collision meshes for rendering - from the world grid

            collisionitems.Clear();
            space.GetVisibleBounds(camera, collisionmeshrange, collisionmeshlayers, collisionitems);

            foreach (var item in collisionitems)
            {
                YbnFile ybn = gameFileCache.GetYbn(item.Name);
                if ((ybn != null) && (ybn.Loaded))
                {
                    RenderCollisionMesh(ybn.Bounds, null);
                }
            }

        }

        private void RenderInteriorCollisionMesh(YmapEntityDef mlo)
        {
            //enqueue interior collison meshes for rendering.

            if (mlo.Archetype == null) return;
            var hash = mlo.Archetype.Hash;
            YbnFile ybn = gameFileCache.GetYbn(hash);
            if ((ybn != null) && (ybn.Loaded))
            {
                RenderCollisionMesh(ybn.Bounds, mlo);
            }
            if (ybn == null)
            { }
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

                            UpdateMouseHits(rndbc, entity);
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



        private void RenderBounds(DeviceContext context)
        {
            //immediately render the entity bounding boxes/spheres - depending on boundsmode


            //////rendering grass instance batch bounding boxes...
            ////shaders.SetDepthStencilMode(context, renderboundsclip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);
            ////var shader = shaders.Bounds;
            ////shader.SetMode(BoundsShaderMode.Box);
            ////shader.SetShader(context);
            ////shader.SetInputLayout(context, VertexType.Default);
            ////shader.SetSceneVars(context, camera);
            ////shader.SetColourVars(context, new Vector4(0, 0, 1, 1));
            ////for (int i = 0; i < shaders.RenderInstBatches.Count; i++)
            ////{
            ////    var b = shaders.RenderInstBatches[i];
            ////    var bpos = b.Batch.GrassInstanceBatch.Position;
            ////    var camrel = bpos - camera.Position;
            ////    var bbmin = b.Batch.GrassInstanceBatch.Batch.BatchAABB.min.XYZ() - bpos;
            ////    var bbmax = b.Batch.GrassInstanceBatch.Batch.BatchAABB.max.XYZ() - bpos;
            ////    shader.SetBoxVars(context, camrel, bbmin, bbmax, Quaternion.Identity, Vector3.One);
            ////    shader.DrawBox(context);
            ////}
            ////shader.UnbindResources(context);




            var mode = boundsmode; //try avoid multithreading issues
            bool clip = renderboundsclip;

            switch (SelectionMode)
            {
                case MapSelectionMode.EntityExtension:
                case MapSelectionMode.ArchetypeExtension:
                case MapSelectionMode.TimeCycleModifier:
                case MapSelectionMode.CarGenerator:
                case MapSelectionMode.DistantLodLights:
                case MapSelectionMode.Grass:
                case MapSelectionMode.Collision:
                case MapSelectionMode.NavMesh:
                case MapSelectionMode.Path:
                case MapSelectionMode.TrainTrack:
                case MapSelectionMode.Scenario:
                    mode = BoundsShaderMode.Box;
                    break;
                case MapSelectionMode.WaterQuad:
                case MapSelectionMode.MloInstance:
                    mode = BoundsShaderMode.Box;
                    clip = false;
                    break;
            }


            if (mode == BoundsShaderMode.None)
            { return; }

            Vector3 colour = new Vector3(0, 0, 1) * globalLights.HdrIntensity;
            Vector3 colourhi = new Vector3(0, 1, 1) * globalLights.HdrIntensity;

            shaders.SetDepthStencilMode(context, clip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);
            var shader = shaders.Bounds;
            shader.SetMode(mode);
            shader.SetShader(context);
            shader.SetInputLayout(context, VertexType.Default);
            shader.SetSceneVars(context, camera, null, globalLights);
            shader.SetColourVars(context, new Vector4(colour, 1));


            if (mode == BoundsShaderMode.Box)
            {
                for (int i = 0; i < BoundingBoxes.Count; i++)
                {
                    MapBox mb = BoundingBoxes[i];
                    shader.SetBoxVars(context, mb.CamRelPos, mb.BBMin, mb.BBMax, mb.Orientation, mb.Scale);
                    shader.DrawBox(context);
                }
                shader.SetColourVars(context, new Vector4(colourhi, 1));
                for (int i = 0; i < HilightBoxes.Count; i++)
                {
                    MapBox mb = HilightBoxes[i];
                    shader.SetBoxVars(context, mb.CamRelPos, mb.BBMin, mb.BBMax, mb.Orientation, mb.Scale);
                    shader.DrawBox(context);
                }
            }
            else if (mode == BoundsShaderMode.Sphere)
            {
                for (int i = 0; i < BoundingSpheres.Count; i++)
                {
                    MapSphere ms = BoundingSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
            }


            shader.UnbindResources(context);
        }

        private void RenderMoused(DeviceContext context)
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


            Vector3 colour = new Vector3(1, 1, 1);
            colour *= globalLights.HdrIntensity * 5.0f;

            bool clip = renderboundsclip;

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


            shaders.SetDepthStencilMode(context, clip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);

            //render moused object box.
            var shader = shaders.Bounds;
            shader.SetMode(BoundsShaderMode.Box);
            shader.SetShader(context);
            shader.SetInputLayout(context, VertexType.Default);
            shader.SetSceneVars(context, camera, null, globalLights);
            shader.SetColourVars(context, new Vector4(colour, 1)); //white box

            shader.SetBoxVars(context, camrel, bbmin, bbmax, ori, scale);
            shader.DrawBox(context);

            shader.UnbindResources(context);

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
                    RenderSelectionArrowOutline(MouseRayCollision.Position, MouseRayCollision.Normal, arup, Quaternion.Identity, 2.0f, 0.15f, cgrn);
                }
            }

            if (!ShowSelectionBounds)
            { return; }

            if (!selectionItem.HasValue)
            { return; }


            Vector3 colour = new Vector3(0, 1, 0);
            colour *= globalLights.HdrIntensity * 5.0f;


            bool clip = renderboundsclip;


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
                    RenderSelectionEntityPivot(ent);
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
                RenderSelectionArrowOutline(cg.Position, Vector3.UnitX, Vector3.UnitY, ori, arrowlen, arrowrad, cgrn);

                Quaternion cgtrn = Quaternion.RotationAxis(Vector3.UnitZ, (float)Math.PI * -0.5f); //car fragments currently need to be rotated 90 deg right...
                Quaternion cgori = Quaternion.Multiply(ori, cgtrn);

                RenderCar(cg.Position, cgori, cg._CCarGen.carModel, cg._CCarGen.popGroup);
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
                RenderSelectionArrowOutline(sn.Position, Vector3.UnitY, Vector3.UnitZ, ori, arrowlen, arrowrad, cgrn);

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

                    RenderCar(sn.Position, sn.Orientation, 0, vhash);
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
                    RenderSelectionArrowOutline(sn1.Position, -Vector3.UnitZ, Vector3.UnitY, aori, arrowlen, arrowrad, cblu);
                }
            }
            if (selectionItem.MloEntityDef != null)
            {
                bbmin = selectionItem.AABB.Minimum;
                bbmax = selectionItem.AABB.Maximum;
                clip = false;
            }
            if ((selectionItem.GrassBatch != null) || (selectionItem.ArchetypeExtension != null) || (selectionItem.EntityExtension != null) || (selectionItem.CollisionBounds != null))
            {
                bbmin = selectionItem.AABB.Minimum;
                bbmax = selectionItem.AABB.Maximum;
                scale = Vector3.One;
            }
            if (selectionItem.WaterQuad != null)
            {
                clip = false;
            }
            if (selectionItem.NavPoly != null)
            {
                RenderSelectionNavPoly(selectionItem.NavPoly);

                //return;//don't render a selection box for nav mesh
                //clip = false;
            }




            MapBox box = new MapBox();
            box.CamRelPos = camrel;
            box.BBMin = bbmin;
            box.BBMax = bbmax;
            box.Orientation = ori;
            box.Scale = scale;
            SelectionBoxes.Add(box);

        }

        private void RenderSelectionEntityPivot(YmapEntityDef ent)
        {
            uint cred = (uint)new Color4(1.0f, 0.0f, 0.0f, 1.0f).ToRgba();
            uint cgrn = (uint)new Color4(0.0f, 1.0f, 0.0f, 1.0f).ToRgba();
            uint cblu = (uint)new Color4(0.0f, 0.0f, 1.0f, 1.0f).ToRgba();

            var pos = ent.WidgetPosition;
            var ori = ent.WidgetOrientation;

            float pxsize = 120.0f;
            float sssize = pxsize / camera.Height;
            float dist = (pos - camera.Position).Length();
            float size = sssize * dist;
            if (camera.IsMapView || camera.IsOrthographic)
            {
                size = sssize * camera.OrthographicSize;
            }
            float rad = size * 0.066f;

            RenderSelectionArrowOutline(pos, Vector3.UnitX, Vector3.UnitY, ori, size, rad, cred);
            RenderSelectionArrowOutline(pos, Vector3.UnitY, Vector3.UnitX, ori, size, rad, cgrn);
            RenderSelectionArrowOutline(pos, Vector3.UnitZ, Vector3.UnitY, ori, size, rad, cblu);

        }

        private void RenderSelectionArrowOutline(Vector3 pos, Vector3 dir, Vector3 up, Quaternion ori, float len, float rad, uint colour)
        {
            Vector3 ax = Vector3.Cross(dir, up);
            Vector3 sx = ax * rad;
            Vector3 sy = up * rad;
            Vector3 sz = dir * len;
            VertexTypePC[] c = new VertexTypePC[8];
            Vector3 d0 = - sx - sy;
            Vector3 d1 = - sx + sy;
            Vector3 d2 = + sx - sy;
            Vector3 d3 = + sx + sy;
            c[0].Position = d0;
            c[1].Position = d1;
            c[2].Position = d2;
            c[3].Position = d3;
            c[4].Position = d0 + sz;
            c[5].Position = d1 + sz;
            c[6].Position = d2 + sz;
            c[7].Position = d3 + sz;
            for (int i = 0; i < 8; i++)
            {
                c[i].Colour = colour;
                c[i].Position = pos + ori.Multiply(c[i].Position);
            }

            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[1]);
            SelectionLineVerts.Add(c[1]);
            SelectionLineVerts.Add(c[3]);
            SelectionLineVerts.Add(c[3]);
            SelectionLineVerts.Add(c[2]);
            SelectionLineVerts.Add(c[2]);
            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[4]);
            SelectionLineVerts.Add(c[5]);
            SelectionLineVerts.Add(c[5]);
            SelectionLineVerts.Add(c[7]);
            SelectionLineVerts.Add(c[7]);
            SelectionLineVerts.Add(c[6]);
            SelectionLineVerts.Add(c[6]);
            SelectionLineVerts.Add(c[4]);
            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[4]);
            SelectionLineVerts.Add(c[1]);
            SelectionLineVerts.Add(c[5]);
            SelectionLineVerts.Add(c[2]);
            SelectionLineVerts.Add(c[6]);
            SelectionLineVerts.Add(c[3]);
            SelectionLineVerts.Add(c[7]);

            c[0].Position = pos + ori.Multiply(dir * (len + rad * 5.0f));
            c[4].Position += ori.Multiply(d0);
            c[5].Position += ori.Multiply(d1);
            c[6].Position += ori.Multiply(d2);
            c[7].Position += ori.Multiply(d3);
            SelectionLineVerts.Add(c[4]);
            SelectionLineVerts.Add(c[5]);
            SelectionLineVerts.Add(c[5]);
            SelectionLineVerts.Add(c[7]);
            SelectionLineVerts.Add(c[7]);
            SelectionLineVerts.Add(c[6]);
            SelectionLineVerts.Add(c[6]);
            SelectionLineVerts.Add(c[4]);
            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[4]);
            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[5]);
            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[6]);
            SelectionLineVerts.Add(c[0]);
            SelectionLineVerts.Add(c[7]);


        }

        private void RenderSelectionNavPoly(YnvPoly poly)
        {
            ////draw poly triangles
            var pcolour = new Color4(0.6f, 0.95f, 0.6f, 1.0f);
            var colourval = (uint)pcolour.ToRgba();
            var ynv = poly.Ynv;
            var ic = poly._RawData.IndexCount;
            var startid = poly._RawData.IndexID;
            var endid = startid + ic;
            var lastid = endid - 1;
            var vc = ynv.Vertices.Count;
            var startind = ynv.Indices[startid];
            VertexTypePC v0 = new VertexTypePC();
            VertexTypePC v1 = new VertexTypePC();
            VertexTypePC v2 = new VertexTypePC();
            v0.Position = ynv.Vertices[startind];
            v0.Colour = colourval;
            v1.Colour = colourval;
            v2.Colour = colourval;
            int tricount = ic - 2;
            for (int t = 0; t < tricount; t++)
            {
                int tid = startid + t;
                int ind1 = ynv.Indices[tid + 1];
                int ind2 = ynv.Indices[tid + 2];
                if ((ind1 >= vc) || (ind2 >= vc))
                { continue; }
                v1.Position = ynv.Vertices[ind1];
                v2.Position = ynv.Vertices[ind2];
                SelectionTriVerts.Add(v0);
                SelectionTriVerts.Add(v1);
                SelectionTriVerts.Add(v2);
                SelectionTriVerts.Add(v0);
                SelectionTriVerts.Add(v2);
                SelectionTriVerts.Add(v1);
            }
        }

        private void RenderSelectionGeometry(DeviceContext context)
        {

            bool clip = true;
            switch (SelectionMode)
            {
                case MapSelectionMode.NavMesh:
                case MapSelectionMode.WaterQuad:
                case MapSelectionMode.MloInstance:
                    clip = false;
                    break;
            }


            shaders.SetDepthStencilMode(context, clip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);

            var pshader = shaders.Paths;
            if (SelectionTriVerts.Count > 0)
            {
                pshader.RenderTriangles(context, SelectionTriVerts, camera, shaders.GlobalLights);
            }
            if (SelectionLineVerts.Count > 0)
            {
                pshader.RenderLines(context, SelectionLineVerts, camera, shaders.GlobalLights);
            }




            if (SelectionBoxes.Count > 0)
            {
                Vector3 coloursel = new Vector3(0, 1, 0) * globalLights.HdrIntensity * 5.0f;
                var shader = shaders.Bounds;
                shader.SetMode(BoundsShaderMode.Box);
                shader.SetShader(context);
                shader.SetInputLayout(context, VertexType.Default);
                shader.SetSceneVars(context, camera, null, globalLights);
                shader.SetColourVars(context, new Vector4(coloursel, 1));
                for (int i = 0; i < SelectionBoxes.Count; i++)
                {
                    MapBox mb = SelectionBoxes[i];
                    shader.SetBoxVars(context, mb.CamRelPos, mb.BBMin, mb.BBMax, mb.Orientation, mb.Scale);
                    shader.DrawBox(context);
                }
                shader.UnbindResources(context);
            }

        }


        private void RenderMarkers(DeviceContext context)
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

            shaders.SetRasterizerMode(context, RasterizerMode.SolidDblSided); //hmm they are backwards
            shaders.SetDepthStencilMode(context, markerdepthclip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);
            shaders.SetDefaultBlendState(context);

            var shader = shaders.Marker;
            shader.SetShader(context);
            shader.SetInputLayout(context, VertexType.Default);
            shader.SetSceneVars(context, camera, null, globalLights);

            MapIcon icon = null;
            foreach (var marker in MarkerBatch)
            {
                icon = marker.Icon;
                Vector2 texs = new Vector2(icon.TexWidth, icon.TexHeight);
                Vector2 size = texs * marker.Distance;
                Vector2 offset = (new Vector2(texs.X, -texs.Y) - new Vector2(icon.Center.X, -icon.Center.Y) * 2.0f) * marker.Distance;
                shader.SetMarkerVars(context, marker.CamRelPos, size, offset);
                shader.SetTexture(context, icon.TexView);
                markerquad.Draw(context);
            }

            shader.UnbindResources(context);
        }


        private void RenderWidgets(DeviceContext context)
        {
            if (!ShowWidget) return;


            var dsmode = DepthStencilMode.Enabled;
            if (Widget.Mode == WidgetMode.Rotation)
            {
                dsmode = DepthStencilMode.DisableAll;
            }

            shaders.SetRasterizerMode(context, RasterizerMode.SolidDblSided);
            shaders.SetDepthStencilMode(context, dsmode);
            shaders.SetDefaultBlendState(context);
            shaders.ClearDepth(context, false);

            var shader = shaders.Widgets;

            Widget.Render(context, camera, shader);

        }
        private void UpdateWidgets()
        {
            if (!ShowWidget) return;

            Widget.Update(camera);
        }

        private void Widget_OnPositionChange(Vector3 newpos, Vector3 oldpos)
        {
            //called during UpdateWidgets()
            if (newpos == oldpos) return;

            if (SelectedItem.MultipleSelection)
            {
                if (EditEntityPivot)
                {
                }
                else
                {
                    var dpos = newpos - oldpos;
                    for (int i = 0; i < SelectedItems.Count; i++)
                    {
                        var refpos = SelectedItems[i].WidgetPosition;
                        SelectedItems[i].SetPosition(refpos + dpos, refpos, false);
                    }
                    SelectedItem.MultipleSelectionCenter = newpos;
                }
            }
            else
            {
                SelectedItem.SetPosition(newpos, oldpos, EditEntityPivot);                
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
            lock (rendersyncroot)
            {
                renderableCache.Invalidate(ynd);
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
            lock (rendersyncroot)
            {
                renderableCache.Invalidate(tt);
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

            lock (rendersyncroot)
            {
                renderableCache.Invalidate(scenario);
            }
        }


        public Vector3 GetCameraPosition()
        {
            //currently used by ProjectForm when creating entities
            lock (rendersyncroot)
            {
                return camera.Position;
            }
        }
        public Vector3 GetCameraViewDir()
        {
            //currently used by ProjectForm when creating entities
            lock (rendersyncroot)
            {
                return camera.ViewDirection;
            }
        }

        public void SetCameraSensitivity(float sensitivity, float smoothing)
        {
            camera.Sensitivity = sensitivity;
            camera.Smoothness = smoothing;
        }

        public void SetKeyBindings(KeyBindings kb)
        {
            keyBindings = kb.Copy();
            UpdateToolbarShortcutsText();
        }
        private void UpdateToolbarShortcutsText()
        {
            ToolbarSelectButton.ToolTipText = string.Format("Select objects / Exit edit mode ({0}, {1})", keyBindings.ToggleMouseSelect, keyBindings.ExitEditMode);
            ToolbarMoveButton.ToolTipText = string.Format("Move ({0})", keyBindings.EditPosition);
            ToolbarRotateButton.ToolTipText = string.Format("Rotate ({0})", keyBindings.EditRotation);
            ToolbarScaleButton.ToolTipText = string.Format("Scale ({0})", keyBindings.EditScale);
            ShowToolbarCheckBox.Text = string.Format("Show Toolbar ({0})", keyBindings.ToggleToolbar);
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

            lock (rendersyncroot)
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

                timerunning = false;

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

                timerunning = true;

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

            SelectionLineVerts.Clear();
            SelectionTriVerts.Clear();


            MouseRayCollisionEnabled = CtrlPressed; //temporary...!
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

        }
        private void UpdateMouseHit(Renderable rndbl, Archetype arche, YmapEntityDef entity, Vector3 camrel)
        {
            if ((SelectionMode == MapSelectionMode.Entity) && !MouseSelectEnabled) return; //performance improvement when not selecting entities...

            //test the selected entity/archetype for mouse hit.
            
            //first test the bounding sphere for mouse hit..
            Quaternion orinv;
            Ray mraytrn;
            float hitdist = 0.0f;
            var drawable = rndbl.Key;
            int geometryIndex = 0;
            DrawableGeometry geometry = null;
            BoundingBox geometryAABB = new BoundingBox();
            BoundingSphere bsph = new BoundingSphere();
            BoundingBox bbox = new BoundingBox();
            BoundingBox gbbox = new BoundingBox();
            Quaternion orientation = Quaternion.Identity;
            Vector3 scale = Vector3.One;
            if (entity != null)
            {
                orientation = entity.Orientation;
                scale = entity.Scale;
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
                            BoundingBoxes.Add(mb);

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
                            BoundingBoxes.Add(mb);

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
                    if (m.Unknown_18h_Data == null)
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
                    int gbbcount = m.Unknown_18h_Data.Length;
                    for (int j = 0; j < gbbcount; j++) //first box seems to be whole model
                    {
                        var gbox = m.Unknown_18h_Data[j];
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
        private void UpdateMouseHits(YmapFile ymap)
        {
            //find mouse hits for things like time cycle mods and car generators in ymaps.

            BoundingBox bbox = new BoundingBox();
            Ray mray = new Ray();
            mray.Position = camera.MouseRay.Position + camera.Position;
            mray.Direction = camera.MouseRay.Direction;
            float hitdist = float.MaxValue;

            if ((SelectionMode == MapSelectionMode.TimeCycleModifier) && (ymap.TimeCycleModifiers != null))
            {
                for (int i = 0; i < ymap.TimeCycleModifiers.Length; i++)
                {
                    var tcm = ymap.TimeCycleModifiers[i];
                    if ((((tcm.BBMin + tcm.BBMax) * 0.5f) - camera.Position).Length() > renderboundsmaxdist) continue;
                    //if (!LodDistTest(camera.Position, tcm.BBMin, tcm.BBMax, renderboundsmaxdist)) continue;

                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = tcm.BBMin;
                    mb.BBMax = tcm.BBMax;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    BoundingBoxes.Add(mb);

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
                    BoundingBoxes.Add(mb);

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
                    BoundingBoxes.Add(mb);

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
                    if ((gb.Position - camera.Position).Length() > renderboundsmaxdist) continue;
                    //if (!LodDistTest(camera.Position, gb.AABBMin, gb.AABBMax, renderboundsmaxdist)) continue;

                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = gb.AABBMin;
                    mb.BBMax = gb.AABBMax;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    BoundingBoxes.Add(mb);

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
                if ((((dll.BBMin + dll.BBMax) * 0.5f) - camera.Position).Length() <= renderboundsmaxdist)
                //if (LodDistTest(camera.Position, dll.BBMin, dll.BBMax, renderboundsmaxdist))
                {
                    MapBox mb = new MapBox();
                    mb.CamRelPos = -camera.Position;
                    mb.BBMin = dll.BBMin;
                    mb.BBMax = dll.BBMax;
                    mb.Orientation = Quaternion.Identity;
                    mb.Scale = Vector3.One;
                    BoundingBoxes.Add(mb);

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
                BoundingBoxes.Add(mb);

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
                if (cent.Length() > renderboundsmaxdist) continue;

                BoundingBoxes.Add(mb);

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
                    BoundingBoxes.Add(mb);
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
                    SelectionLineVerts.Add(v);
                    if (id == startid)
                    {
                        v0 = v;
                    }
                    else
                    {
                        SelectionLineVerts.Add(v);
                    }
                    if (id == lastid)
                    {
                        SelectionLineVerts.Add(v0);
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
                //    SelectionTriVerts.Add(v0);
                //    SelectionTriVerts.Add(v1);
                //    SelectionTriVerts.Add(v2);
                //    SelectionTriVerts.Add(v0);
                //    SelectionTriVerts.Add(v2);
                //    SelectionTriVerts.Add(v1);
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
                    BoundingBoxes.Add(mb);
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
                            HilightBoxes.Add(mb);
                        }
                        else
                        {
                            BoundingBoxes.Add(mb);
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
                        BoundingBoxes.Add(mb);
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
                    BoundingBoxes.Add(mb);
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
                        HilightBoxes.Add(mb);
                    }
                    else
                    {
                        BoundingBoxes.Add(mb);
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
                    HilightBoxes.Add(mb);


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
                            BoundingBoxes.Add(mb);
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
                mhitv.Drawable = TryGetDrawable(mhitv.Archetype); //no drawable given.. try to get it from the cache.. if it's not there, drawable info won't display...
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
                    lock (rendersyncroot)
                    {
                        SelectedItem.PathLink = mhitv.PathLink;
                        SelectedItem.ScenarioEdge = mhitv.ScenarioEdge;
                    }
                }
                return;
            }

            lock (rendersyncroot) //drawflags is used when rendering.. need that lock
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
        private void SelectMousedItem()
        {
            //when clicked, select the currently moused item and update the selection info UI

            if (!MouseSelectEnabled)
            { return; }

            SelectItem(LastMouseHit, CtrlPressed);
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

            SelectionModelDrawFlags.Clear();
            SelectionGeometryDrawFlags.Clear();
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
                    SelectionModelDrawFlags[model] = false;
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
            lock (rendersyncroot)
            {
                if (model != null)
                {
                    if (rem)
                    {
                        if (SelectionModelDrawFlags.ContainsKey(model))
                        {
                            SelectionModelDrawFlags.Remove(model);
                        }
                    }
                    else
                    {
                        SelectionModelDrawFlags[model] = false;
                    }
                }
                if (geom != null)
                {
                    if (rem)
                    {
                        if (SelectionGeometryDrawFlags.ContainsKey(geom))
                        {
                            SelectionGeometryDrawFlags.Remove(geom);
                        }
                    }
                    else
                    {
                        SelectionGeometryDrawFlags[geom] = false;
                    }
                }
            }
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
            timecycle.SetTime(timeofday);

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
                lock (rendersyncroot)
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
                lock (rendersyncroot)
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
                GTA5Keys.LoadFromPath(Settings.Default.GTAFolder); //now loads from magic
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
                gameFileCache.ContentThreadProc();

                renderableCache.ContentThreadProc();

                if (!(gameFileCache.ItemsStillPending || renderableCache.ItemsStillPending))
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
                    CloudsComboBox.SelectedIndex = Math.Max(CloudsComboBox.FindString(individualcloudfrag), 0);


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
            if (!canundo) return;
            var ent = SelectedItem.EntityDef;
            var cargen = SelectedItem.CarGenerator;
            var pathnode = SelectedItem.PathNode;
            var trainnode = SelectedItem.TrainTrackNode;
            var scenarionode = SelectedItem.ScenarioNode;
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

            lock (rendersyncroot)
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

            lock (rendersyncroot)
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
            boundsmode = mode;
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

            lock (rendersyncroot)
            {
                switch (modestr)
                {
                    case "Perspective":
                        camera.IsOrthographic = false;
                        MapViewEnabled = false;
                        camera.UpdateProj = true;
                        ToolbarCameraModeButton.Image = ToolbarCameraPerspectiveButton.Image;
                        ToolbarCameraPerspectiveButton.Checked = true;
                        break;
                    case "Orthographic":
                        camera.IsOrthographic = true;
                        MapViewEnabled = false;
                        ToolbarCameraModeButton.Image = ToolbarCameraOrthographicButton.Image;
                        ToolbarCameraOrthographicButton.Checked = true;
                        break;
                    case "2D Map":
                        camera.IsOrthographic = true;
                        MapViewEnabled = true;
                        ToolbarCameraModeButton.Image = ToolbarCameraMapViewButton.Image;
                        ToolbarCameraMapViewButton.Checked = true;
                        break;
                }
                camera.IsMapView = MapViewEnabled;
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
                            if (ShiftPressed)
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

                            if (CtrlPressed)
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

            var k = e.KeyCode;

            bool ctrl = (e.Modifiers & Keys.Control) > 0;
            bool shift = (e.Modifiers & Keys.Shift) > 0;
            CtrlPressed = ctrl;
            ShiftPressed = shift;

            bool enablemove = (!iseditmode) || (MouseLButtonDown && (GrabbedMarker == null) && (GrabbedWidget == null));

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
                    if ((k == keyBindings.ExitEditMode))
                    {
                        if (Widget.Mode == WidgetMode.Default) ToggleWidgetSpace();
                        else SetWidgetMode("Default");
                    }
                    if ((k == keyBindings.EditPosition))// && !enablemove)
                    {
                        if (Widget.Mode == WidgetMode.Position) ToggleWidgetSpace();
                        else SetWidgetMode("Position");
                    }
                    if ((k == keyBindings.EditRotation))// && !enablemove)
                    {
                        if (Widget.Mode == WidgetMode.Rotation) ToggleWidgetSpace();
                        else SetWidgetMode("Rotation");
                    }
                    if ((k == keyBindings.EditScale))// && !enablemove)
                    {
                        if (Widget.Mode == WidgetMode.Scale) ToggleWidgetSpace();
                        else SetWidgetMode("Scale");
                    }
                    if (k == keyBindings.ToggleMouseSelect)
                    {
                        SetMouseSelect(!MouseSelectEnabled);
                    }
                    if (k == keyBindings.ToggleToolbar)
                    {
                        ToggleToolbar();
                    }
                    if (k == keyBindings.FirstPerson)
                    {
                        SetControlMode((ControlMode == WorldControlMode.Free) ? WorldControlMode.Ped : WorldControlMode.Free);
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


            if (ControlMode != WorldControlMode.Free)
            {
                e.Handled = true;
            }
        }

        private void WorldForm_Deactivate(object sender, EventArgs e)
        {
            //try not to lock keyboard movement if the form loses focus.
            kbmovefwd = false;
            kbmovebck = false;
            kbmovelft = false;
            kbmovergt = false;
            kbmoveup = false;
            kbmovedn = false;
            kbjump = false;
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
            shaders.wireframe = WireframeCheckBox.Checked;
        }

        private void GrassCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendergrass = GrassCheckBox.Checked;
        }

        private void TimedEntitiesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (rendersyncroot)
            {
                rendertimedents = TimedEntitiesCheckBox.Checked;
            }
        }

        private void TimedEntitiesAlwaysOnCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendertimedentsalways = TimedEntitiesAlwaysOnCheckBox.Checked;
        }

        private void InteriorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderinteriors = InteriorsCheckBox.Checked;
        }

        private void WaterQuadsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderwaterquads = WaterQuadsCheckBox.Checked;
        }

        private void ProxiesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (rendersyncroot)
            {
                renderproxies = ProxiesCheckBox.Checked;
            }
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
            shaders.PathsDepthClip = PathsDepthClipCheckBox.Checked;
        }

        private void ErrorConsoleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConsolePanel.Visible = ErrorConsoleCheckBox.Checked;
        }

        private void DynamicLODCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            usedynamiclod = DynamicLODCheckBox.Checked;
            ShowYmapChildrenCheckBox.Enabled = !usedynamiclod;
        }

        private void DetailTrackBar_Scroll(object sender, EventArgs e)
        {
            lodthreshold = 50.0f / (0.1f + (float)DetailTrackBar.Value);
        }

        private void WaitForChildrenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            waitforchildrentoload = WaitForChildrenCheckBox.Checked;
        }

        private void ReloadShadersButton_Click(object sender, EventArgs e)
        {
            if (currentdevice == null) return; //can't do this with no device

            Cursor = Cursors.WaitCursor;
            pauserendering = true;

            lock (rendersyncroot)
            {
                try
                {
                    if (shaders != null)
                    {
                        shaders.Dispose();
                    }
                    shaders = new ShaderManager(currentdevice, dxman);
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
            markerdepthclip = MarkerDepthClipCheckBox.Checked;
        }

        private void ShadowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (rendersyncroot)
            {
                shaders.shadows = ShadowsCheckBox.Checked;
            }
        }

        private void SkydomeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            renderskydome = SkydomeCheckBox.Checked;
        }

        private void BoundsStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = BoundsStyleComboBox.SelectedItem;
            var strval = val as string;
            SetBoundsMode(strval);
        }

        private void BoundsDepthClipCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderboundsclip = BoundsDepthClipCheckBox.Checked;
        }

        private void BoundsRangeTrackBar_Scroll(object sender, EventArgs e)
        {
            float fv = BoundsRangeTrackBar.Value;
            renderboundsmaxdist = fv * fv;
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
            BinarySearchForm f = new BinarySearchForm();
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

        private void CollisionMeshesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendercollisionmeshes = CollisionMeshesCheckBox.Checked;
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
            collisionmeshlayerdrawable = CollisionMeshLayerDrawableCheckBox.Checked;
        }

        private void ControlLightDirectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controllightdir = ControlLightDirectionCheckBox.Checked;
            if (controllightdir)
            {
                ControlTimeOfDayCheckBox.Checked = false;
            }
        }

        private void ControlTimeOfDayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controltimeofday = ControlTimeOfDayCheckBox.Checked;
            if (controltimeofday)
            {
                ControlLightDirectionCheckBox.Checked = false;
            }
        }

        private void ShowYmapChildrenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderchildents = ShowYmapChildrenCheckBox.Checked;
        }

        private void HDRRenderingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (rendersyncroot)
            {
                shaders.hdr = HDRRenderingCheckBox.Checked;
            }
        }

        private void AnisotropicFilteringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            shaders.AnisotropicFiltering = AnisotropicFilteringCheckBox.Checked;
        }

        private void WorldMaxLodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (WorldMaxLodComboBox.Text)
            {
                default:
                case "ORPHANHD":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
                    break;
                case "HD":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_HD;
                    break;
                case "LOD":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_LOD;
                    break;
                case "SLOD1":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD1;
                    break;
                case "SLOD2":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD2;
                    break;
                case "SLOD3":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD3;
                    break;
                case "SLOD4":
                    renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_SLOD4;
                    break;
            }
        }

        private void WorldLodDistTrackBar_Scroll(object sender, EventArgs e)
        {
            float loddist = ((float)WorldLodDistTrackBar.Value) * 0.1f;
            renderworldLodDistMult = loddist;
            WorldLodDistLabel.Text = loddist.ToString("0.0");
        }

        private void WorldDetailDistTrackBar_Scroll(object sender, EventArgs e)
        {
            float detdist = ((float)WorldDetailDistTrackBar.Value) * 0.1f;
            renderworldDetailDistMult = detdist;
            WorldDetailDistLabel.Text = detdist.ToString("0.0");
        }

        private void WorldScriptedYmapsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowScriptedYmaps = WorldScriptedYmapsCheckBox.Checked;
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
            float fh = v / 60.0f;
            UpdateTimeOfDayLabel();
            lock (rendersyncroot)
            {
                timeofday = fh;
                timecycle.SetTime(timeofday);
            }
        }

        private void WeatherComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Monitor.TryEnter(rendersyncroot, 50))
            { return; } //couldn't get a lock...
            weathertype = WeatherComboBox.Text;
            weather.SetNextWeather(weathertype);
            Monitor.Exit(rendersyncroot);
        }

        private void WeatherRegionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            weather.Region = WeatherRegionComboBox.Text;
        }

        private void CloudsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (!Monitor.TryEnter(rendersyncroot, 50))
            //{ return; } //couldn't get a lock...
            individualcloudfrag = CloudsComboBox.Text;
            //Monitor.Exit(rendersyncroot);
        }

        private void DistantLODLightsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderdistlodlights = DistantLODLightsCheckBox.Checked;
        }

        private void NaturalAmbientLightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rendernaturalambientlight = NaturalAmbientLightCheckBox.Checked;
        }

        private void ArtificialAmbientLightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            renderartificialambientlight = ArtificialAmbientLightCheckBox.Checked;
        }

        private void TimeStartStopButton_Click(object sender, EventArgs e)
        {
            timerunning = !timerunning;
            TimeStartStopButton.Text = timerunning ? "Stop" : "Start";
        }

        private void TimeSpeedTrackBar_Scroll(object sender, EventArgs e)
        {
            float tv = TimeSpeedTrackBar.Value * 0.01f;
            //when tv=0,   speed=0 min/sec
            //when tv=0.5, speed=0.5 min/sec
            //when tv=1,  speed=128 min/sec

            timespeed = 128.0f * tv * tv * tv * tv * tv * tv * tv * tv;

            TimeSpeedLabel.Text = timespeed.ToString("0.###") + " min/sec";
        }

        private void CameraModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCameraMode(CameraModeComboBox.Text);
        }

        private void MapViewDetailTrackBar_Scroll(object sender, EventArgs e)
        {
            float det = ((float)MapViewDetailTrackBar.Value) * 0.1f;
            MapViewDetailLabel.Text = det.ToString("0.0#");
            lock (rendersyncroot)
            {
                MapViewDetail = det;
            }
        }

        private void FieldOfViewTrackBar_Scroll(object sender, EventArgs e)
        {
            float fov = FieldOfViewTrackBar.Value * 0.01f;
            FieldOfViewLabel.Text = fov.ToString("0.0#");
            lock (rendersyncroot)
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
    }


    public enum WorldRenderMode
    {
        Default = 0,
        SingleTexture = 1,
        VertexNormals = 2,
        VertexTangents = 3,
        VertexColour = 4,
        TextureCoord = 5,
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


    public class MapIcon
    {
        public string Name { get; set; }
        public string Filepath { get; set; }
        public Texture2D Tex { get; set; }
        public ShaderResourceView TexView { get; set; }
        public Vector3 Center { get; set; } //in image pixels
        public float Scale { get; set; } //screen pixels per icon pixel
        public int TexWidth { get; set; }
        public int TexHeight { get; set; }

        public MapIcon(string name, string filepath, int texw, int texh, float centerx, float centery, float scale)
        {
            Name = name;
            Filepath = filepath;
            TexWidth = texw;
            TexHeight = texh;
            Center = new Vector3(centerx, centery, 0.0f);
            Scale = scale;

            if (!File.Exists(filepath))
            {
                throw new Exception("File not found.");
            }
        }

        public void LoadTexture(Device device, Action<string> errorAction)
        {
            try
            {
                if (device != null)
                {
                    Tex = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), Filepath));
                    TexView = new ShaderResourceView(device, Tex);
                }
            }
            catch (Exception ex)
            {
                errorAction("Could not load map icon " + Filepath + " for " + Name + "!\n\n" + ex.ToString());
            }
        }

        public void UnloadTexture()
        {
            if (TexView != null)
            {
                TexView.Dispose();
                TexView = null;
            }
            if (Tex != null)
            {
                Tex.Dispose();
                Tex = null;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MapMarker
    {
        public MapIcon Icon { get; set; }
        public Vector3 WorldPos { get; set; } //actual world pos
        public Vector3 CamRelPos { get; set; } //updated per frame
        public Vector3 ScreenPos { get; set; } //position on screen (updated per frame)
        public string Name { get; set; }
        public List<string> Properties { get; set; } //additional data
        public bool IsMovable { get; set; }
        public float Distance { get; set; } //length of CamRelPos, updated per frame

        public void Parse(string s)
        {
            Vector3 p = new Vector3(0.0f);
            string[] ss = s.Split(',');
            if (ss.Length > 1)
            {
                FloatUtil.TryParse(ss[0].Trim(), out p.X);
                FloatUtil.TryParse(ss[1].Trim(), out p.Y);
            }
            if (ss.Length > 2)
            {
                FloatUtil.TryParse(ss[2].Trim(), out p.Z);
            }
            if (ss.Length > 3)
            {
                Name = ss[3].Trim();
            }
            else
            {
                Name = string.Empty;
            }
            for (int i = 4; i < ss.Length; i++)
            {
                if (Properties == null) Properties = new List<string>();
                Properties.Add(ss[i].Trim());
            }
            WorldPos = p;
        }

        public override string ToString()
        {
            string cstr = Get3DWorldPosString();
            if (!string.IsNullOrEmpty(Name))
            {
                cstr += ", " + Name;
                if (Properties != null)
                {
                    foreach (string prop in Properties)
                    {
                        cstr += ", " + prop;
                    }
                }
            }
            return cstr;
        }

        public string Get2DWorldPosString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", WorldPos.X, WorldPos.Y);
        }
        public string Get3DWorldPosString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", WorldPos.X, WorldPos.Y, WorldPos.Z);
        }


    }

    public struct MapSphere
    {
        public Vector3 CamRelPos { get; set; }
        public float Radius { get; set; }
    }
    public struct MapBox
    {
        public Vector3 CamRelPos { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 Scale { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct MapSelection
    {
        public YmapEntityDef EntityDef { get; set; }
        public Archetype Archetype { get; set; }
        public DrawableBase Drawable { get; set; }
        public DrawableGeometry Geometry { get; set; }
        public MetaWrapper EntityExtension { get; set; }
        public MetaWrapper ArchetypeExtension { get; set; }
        public YmapTimeCycleModifier TimeCycleModifier { get; set; }
        public YmapCarGen CarGenerator { get; set; }
        public YmapGrassInstanceBatch GrassBatch { get; set; }
        public YmapDistantLODLights DistantLodLights { get; set; }
        public YmapEntityDef MloEntityDef { get; set; }
        public WaterQuad WaterQuad { get; set; }
        public Bounds CollisionBounds { get; set; }
        public YnvPoly NavPoly { get; set; }
        public YndNode PathNode { get; set; }
        public YndLink PathLink { get; set; }
        public TrainTrackNode TrainTrackNode { get; set; }
        public ScenarioNode ScenarioNode { get; set; }
        public MCScenarioChainingEdge ScenarioEdge { get; set; }

        public bool MultipleSelection { get; set; }
        public Vector3 MultipleSelectionCenter { get; set; }

        public BoundingBox AABB { get; set; }
        public int GeometryIndex { get; set; }
        public Vector3 CamRel { get; set; }
        public float HitDist { get; set; }


        public bool HasValue
        {
            get
            {
                return (EntityDef != null) ||
                    (Archetype != null) ||
                    (Drawable != null) ||
                    (Geometry != null) ||
                    (EntityExtension != null) ||
                    (ArchetypeExtension != null) ||
                    (TimeCycleModifier != null) ||
                    (CarGenerator != null) ||
                    (GrassBatch != null) ||
                    (WaterQuad != null) ||
                    (CollisionBounds != null) ||
                    (NavPoly != null) ||
                    (PathNode != null) ||
                    (TrainTrackNode != null) ||
                    (DistantLodLights != null) ||
                    (MloEntityDef != null) ||
                    (ScenarioNode != null);
            }
        }

        public bool HasHit
        {
            get { return (HitDist != float.MaxValue); }
        }


        public bool CheckForChanges(MapSelection mhit)
        {
            return (EntityDef != mhit.EntityDef)
                || (Archetype != mhit.Archetype)
                || (Drawable != mhit.Drawable)
                || (TimeCycleModifier != mhit.TimeCycleModifier)
                || (ArchetypeExtension != mhit.ArchetypeExtension)
                || (EntityExtension != mhit.EntityExtension)
                || (CarGenerator != mhit.CarGenerator)
                || (MloEntityDef != mhit.MloEntityDef)
                || (DistantLodLights != mhit.DistantLodLights)
                || (GrassBatch != mhit.GrassBatch)
                || (WaterQuad != mhit.WaterQuad)
                || (CollisionBounds != mhit.CollisionBounds)
                || (NavPoly != mhit.NavPoly)
                || (PathNode != mhit.PathNode)
                || (TrainTrackNode != mhit.TrainTrackNode)
                || (ScenarioNode != mhit.ScenarioNode);
        }
        public bool CheckForChanges()
        {
            return (EntityDef != null)
                || (Archetype != null)
                || (Drawable != null)
                || (TimeCycleModifier != null)
                || (ArchetypeExtension != null)
                || (EntityExtension != null)
                || (CarGenerator != null)
                || (MloEntityDef != null)
                || (DistantLodLights != null)
                || (GrassBatch != null)
                || (WaterQuad != null)
                || (CollisionBounds != null)
                || (NavPoly != null)
                || (PathNode != null)
                || (PathLink != null)
                || (TrainTrackNode != null)
                || (ScenarioNode != null);
        }


        public void Clear()
        {
            EntityDef = null;
            Archetype = null;
            Drawable = null;
            Geometry = null;
            EntityExtension = null;
            ArchetypeExtension = null;
            TimeCycleModifier = null;
            CarGenerator = null;
            GrassBatch = null;
            WaterQuad = null;
            CollisionBounds = null;
            NavPoly = null;
            PathNode = null;
            PathLink = null;
            TrainTrackNode = null;
            DistantLodLights = null;
            MloEntityDef = null;
            ScenarioNode = null;
            ScenarioEdge = null;
            MultipleSelection = false;
            AABB = new BoundingBox();
            GeometryIndex = 0;
            CamRel = new Vector3();
            HitDist = float.MaxValue;
        }

        public string GetNameString(string defval)
        {
            string name = defval;
            if (MultipleSelection)
            {
                name = "Multiple items";
            }
            else if (EntityDef != null)
            {
                name = EntityDef.CEntityDef.archetypeName.ToString();
            }
            else if (Archetype != null)
            {
                name = Archetype.Hash.ToString();
            }
            else if (TimeCycleModifier != null)
            {
                name = TimeCycleModifier.CTimeCycleModifier.name.ToString();
            }
            else if (CarGenerator != null)
            {
                name = CarGenerator.CCarGen.carModel.ToString();
            }
            else if (DistantLodLights != null)
            {
                name = DistantLodLights.Ymap?.Name ?? "";
            }
            else if (CollisionBounds != null)
            {
                name = CollisionBounds.GetName();
            }
            if (EntityExtension != null)
            {
                name = EntityExtension.Name;
            }
            if (ArchetypeExtension != null)
            {
                name = ArchetypeExtension.Name;
            }
            if (WaterQuad != null)
            {
                name = "WaterQuad " + WaterQuad.ToString();
            }
            if (NavPoly != null)
            {
                name = "NavPoly " + NavPoly.ToString();
            }
            if (PathNode != null)
            {
                name = "PathNode " + PathNode.AreaID.ToString() + "." + PathNode.NodeID.ToString(); //+ FloatUtil.GetVector3String(PathNode.Position);
            }
            if (TrainTrackNode != null)
            {
                name = "TrainTrackNode " + FloatUtil.GetVector3String(TrainTrackNode.Position);
            }
            if (ScenarioNode != null)
            {
                name = ScenarioNode.ToString();
            }
            return name;
        }

        public string GetFullNameString(string defval)
        {
            string name = defval;
            if (MultipleSelection)
            {
                name = "Multiple items";
            }
            else if (EntityDef != null)
            {
                name = EntityDef.CEntityDef.archetypeName.ToString();
            }
            else if (Archetype != null)
            {
                name = Archetype.Hash.ToString();
            }
            else if (CollisionBounds != null)
            {
                name = CollisionBounds.GetName();
            }
            if (Geometry != null)
            {
                name += " (" + GeometryIndex.ToString() + ")";
            }
            if (TimeCycleModifier != null)
            {
                name = TimeCycleModifier.CTimeCycleModifier.name.ToString();
            }
            if (CarGenerator != null)
            {
                name = CarGenerator.NameString();
            }
            if (EntityExtension != null)
            {
                name += ": " + EntityExtension.Name;
            }
            if (ArchetypeExtension != null)
            {
                name += ": " + ArchetypeExtension.Name;
            }
            if (WaterQuad != null)
            {
                name = "WaterQuad " + WaterQuad.ToString();
            }
            if (NavPoly != null)
            {
                name = "NavPoly " + NavPoly.ToString();
            }
            if (PathNode != null)
            {
                name = "PathNode " + PathNode.AreaID.ToString() + "." + PathNode.NodeID.ToString();// + FloatUtil.GetVector3String(PathNode.Position);
            }
            if (TrainTrackNode != null)
            {
                name = "TrainTrackNode " + FloatUtil.GetVector3String(TrainTrackNode.Position);
            }
            if (ScenarioNode != null)
            {
                name = ScenarioNode.ToString();
            }
            return name;
        }



        public bool CanShowWidget
        {
            get
            {
                bool res = false;

                if (MultipleSelection)
                {
                    res = true;
                }
                else if (EntityDef != null)
                {
                    res = true;
                }
                else if (CarGenerator != null)
                {
                    res = true;
                }
                else if (NavPoly != null)
                {
                    res = true;
                }
                else if (PathNode != null)
                {
                    res = true;
                }
                else if (TrainTrackNode != null)
                {
                    res = true;
                }
                else if (ScenarioNode != null)
                {
                    res = true;
                }

                return res;
            }
        }
        public Vector3 WidgetPosition
        {
            get
            {
                if (MultipleSelection)
                {
                    return MultipleSelectionCenter;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetPosition;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Position;
                }
                else if (NavPoly != null)
                {
                    return NavPoly.Position;
                }
                else if (PathNode != null)
                {
                    return PathNode.Position;
                }
                else if (TrainTrackNode != null)
                {
                    return TrainTrackNode.Position;
                }
                else if (ScenarioNode != null)
                {
                    return ScenarioNode.Position;
                }
                return Vector3.Zero;
            }
        }
        public Quaternion WidgetRotation
        {
            get
            {
                if (MultipleSelection)
                {
                    return Quaternion.Identity;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.WidgetOrientation;
                }
                else if (CarGenerator != null)
                {
                    return CarGenerator.Orientation;
                }
                else if (NavPoly != null)
                {
                    return Quaternion.Identity;
                }
                else if (PathNode != null)
                {
                    return Quaternion.Identity;
                }
                else if (TrainTrackNode != null)
                {
                    return Quaternion.Identity;
                }
                else if (ScenarioNode != null)
                {
                    return ScenarioNode.Orientation;
                }
                return Quaternion.Identity;
            }
        }
        public WidgetAxis WidgetRotationAxes
        {
            get
            {
                if (MultipleSelection)
                {
                    return WidgetAxis.XYZ;
                }
                else if (EntityDef != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (CarGenerator != null)
                {
                    return WidgetAxis.Z;
                }
                else if (NavPoly != null)
                {
                    return WidgetAxis.XYZ;
                }
                else if (PathNode != null)
                {
                    return WidgetAxis.None;
                }
                else if (TrainTrackNode != null)
                {
                    return WidgetAxis.None;
                }
                else if (ScenarioNode != null)
                {
                    return WidgetAxis.Z;
                }
                return WidgetAxis.None;
            }
        }
        public Vector3 WidgetScale
        {
            get
            {
                if (MultipleSelection)
                {
                    return Vector3.One;
                }
                else if (EntityDef != null)
                {
                    return EntityDef.Scale;
                }
                else if (CarGenerator != null)
                {
                    return new Vector3(CarGenerator.CCarGen.perpendicularLength);
                }
                else if (NavPoly != null)
                {
                    return Vector3.One;
                }
                else if (PathNode != null)
                {
                    return Vector3.One;
                }
                else if (TrainTrackNode != null)
                {
                    return Vector3.One;
                }
                else if (ScenarioNode != null)
                {
                    return Vector3.One;
                }
                return Vector3.One;
            }
        }




        public void SetPosition(Vector3 newpos, Vector3 oldpos, bool editPivot)
        {
            if (MultipleSelection)
            {
                //don't do anything here for multiselection
            }
            else if (EntityDef != null)
            {
                if (editPivot)
                {
                    EntityDef.SetPivotPositionFromWidget(newpos);
                }
                else
                {
                    EntityDef.SetPositionFromWidget(newpos);
                }
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetPosition(newpos);
            }
            else if (PathNode != null)
            {
                PathNode.SetPosition(newpos);
            }
            else if (NavPoly != null)
            {
                //NavPoly.SetPosition(newpos);

                //if (projectForm != null)
                //{
                //    projectForm.OnWorldNavPolyModified(NavPoly);
                //}
            }
            else if (TrainTrackNode != null)
            {
                TrainTrackNode.SetPosition(newpos);
            }
            else if (ScenarioNode != null)
            {
                ScenarioNode.SetPosition(newpos);
            }

        }
        public void SetRotation(Quaternion newrot, Quaternion oldrot, bool editPivot)
        {
            if (EntityDef != null)
            {
                if (editPivot)
                {
                    EntityDef.SetPivotOrientationFromWidget(newrot);
                }
                else
                {
                    EntityDef.SetOrientationFromWidget(newrot);
                }
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetOrientation(newrot);
            }
            else if (ScenarioNode != null)
            {
                ScenarioNode.SetOrientation(newrot);
            }

        }
        public void SetScale(Vector3 newscale, Vector3 oldscale, bool editPivot)
        {
            if (EntityDef != null)
            {
                EntityDef.SetScale(newscale);
            }
            else if (CarGenerator != null)
            {
                CarGenerator.SetScale(newscale);
                AABB = new BoundingBox(CarGenerator.BBMin, CarGenerator.BBMax);
            }
        }


        public override string ToString()
        {
            return GetFullNameString("[Empty]");
        }
    }

    public enum MapSelectionMode
    {
        None = 0,
        Entity = 1,
        EntityExtension = 2,
        ArchetypeExtension = 3,
        TimeCycleModifier = 4,
        CarGenerator = 5,
        Grass = 6,
        WaterQuad = 7,
        Collision = 8,
        NavMesh = 9,
        Path = 10,
        TrainTrack = 11,
        DistantLodLights = 12,
        MloInstance = 13,
        Scenario = 14,
        PopZone = 15,
    }



    public class KeyBindings
    {
        public Keys MoveForward = Keys.W;
        public Keys MoveBackward = Keys.S;
        public Keys MoveLeft = Keys.A;
        public Keys MoveRight = Keys.D;
        public Keys MoveUp = Keys.R;
        public Keys MoveDown = Keys.F;
        public Keys MoveSlowerZoomIn = Keys.Z;
        public Keys MoveFasterZoomOut = Keys.X;
        public Keys ToggleMouseSelect = Keys.C;
        public Keys ToggleToolbar = Keys.T;
        public Keys ExitEditMode = Keys.Q;
        public Keys EditPosition = Keys.W;
        public Keys EditRotation = Keys.E;
        public Keys EditScale = Keys.R;
        public Keys Jump = Keys.Space; //for control mode
        public Keys FirstPerson = Keys.P;

        public KeyBindings(StringCollection sc)
        {
            foreach (string s in sc)
            {
                string[] parts = s.Split(':');
                if (parts.Length == 2)
                {
                    string sval = parts[1].Trim();
                    Keys k = (Keys)Enum.Parse(typeof(Keys), sval);
                    SetBinding(parts[0], k);
                }
            }
        }

        public void SetBinding(string name, Keys k)
        {
            switch (name)
            {
                case "Move Forwards": MoveForward = k; break;
                case "Move Backwards": MoveBackward = k; break;
                case "Move Left": MoveLeft = k; break;
                case "Move Right": MoveRight = k; break;
                case "Move Up": MoveUp = k; break;
                case "Move Down": MoveDown = k; break;
                case "Move Slower / Zoom In": MoveSlowerZoomIn = k; break;
                case "Move Faster / Zoom Out": MoveFasterZoomOut = k; break;
                case "Toggle Mouse Select": ToggleMouseSelect = k; break;
                case "Toggle Toolbar": ToggleToolbar = k; break;
                case "Exit Edit Mode": ExitEditMode = k; break;
                case "Edit Position": EditPosition = k; break;
                case "Edit Rotation": EditRotation = k; break;
                case "Edit Scale": EditScale = k; break;
                case "First Person Mode": FirstPerson = k; break;
            }
        }

        public StringCollection GetSetting()
        {
            StringCollection sc = new StringCollection();
            sc.Add(GetSettingItem("Move Forwards", MoveForward));
            sc.Add(GetSettingItem("Move Backwards", MoveBackward));
            sc.Add(GetSettingItem("Move Left", MoveLeft));
            sc.Add(GetSettingItem("Move Right", MoveRight));
            sc.Add(GetSettingItem("Move Up", MoveUp));
            sc.Add(GetSettingItem("Move Down", MoveDown));
            sc.Add(GetSettingItem("Move Slower / Zoom In", MoveSlowerZoomIn));
            sc.Add(GetSettingItem("Move Faster / Zoom Out", MoveFasterZoomOut));
            sc.Add(GetSettingItem("Toggle Mouse Select", ToggleMouseSelect));
            sc.Add(GetSettingItem("Toggle Toolbar", ToggleToolbar));
            sc.Add(GetSettingItem("Exit Edit Mode", ExitEditMode));
            sc.Add(GetSettingItem("Edit Position", EditPosition));
            sc.Add(GetSettingItem("Edit Rotation", EditRotation));
            sc.Add(GetSettingItem("Edit Scale", EditScale));
            sc.Add(GetSettingItem("First Person Mode", FirstPerson));
            return sc;
        }

        private string GetSettingItem(string name, Keys val)
        {
            return name + ": " + val.ToString();
        }

        public KeyBindings Copy()
        {
            return (KeyBindings)MemberwiseClone();
        }

    }



}
