using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.World;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.Rendering
{
    public class Renderer
    {
        private DXForm Form;
        private GameFileCache gameFileCache;
        private RenderableCache renderableCache;

        private DXManager dxman = new DXManager();
        public DXManager DXMan { get { return dxman; } }
        private Device currentdevice;
        public Device Device { get { return currentdevice; } }
        private object rendersyncroot = new object();
        public object RenderSyncRoot { get { return rendersyncroot; } }

        public ShaderManager shaders;

        public Camera camera;

        private double currentRealTime = 0;
        private float currentElapsedTime = 0;
        private int framecount = 0;
        private float fcelapsed = 0.0f;
        private int fps = 0;

        private DeviceContext context;


        public float timeofday = 12.0f;
        public bool controltimeofday = true;
        public bool timerunning = false;
        public float timespeed = 0.5f;//min/sec
        public string weathertype = "";
        public string individualcloudfrag = "contrails";

        private Vector4 currentWindVec = Vector4.Zero;
        private float currentWindTime = 0.0f;


        public bool usedynamiclod = Settings.Default.DynamicLOD; //for ymap view
        public float lodthreshold = 50.0f / (0.1f + (float)Settings.Default.DetailDist); //to match formula for the DetailTrackBar value
        public bool waitforchildrentoload = true;


        public bool controllightdir = false; //if not, use timecycle
        public float lightdirx = 2.25f;//radians // approx. light dir on map satellite view
        public float lightdiry = 0.65f;//radians  - used for manual light placement
        public bool renderskydome = Settings.Default.Skydome;
        public bool renderclouds = true;
        public bool rendermoon = true;


        public Timecycle timecycle = new Timecycle();
        public Weather weather = new Weather();
        public Clouds clouds = new Clouds();



        private ShaderGlobalLights globalLights = new ShaderGlobalLights();
        public bool rendernaturalambientlight = true;
        public bool renderartificialambientlight = true;


        public bool MapViewEnabled = false;
        public float MapViewDetail = 1.0f;


        private UnitQuad markerquad = null;
        public bool markerdepthclip = Settings.Default.MarkerDepthClip;



        private List<YmapEntityDef> renderworldentities = new List<YmapEntityDef>(); //used when rendering world view.
        private List<RenderableEntity> renderworldrenderables = new List<RenderableEntity>();


        public bool ShowScriptedYmaps = true;
        public List<YmapFile> VisibleYmaps = new List<YmapFile>();

        public Unk_1264241711 renderworldMaxLOD = Unk_1264241711.LODTYPES_DEPTH_ORPHANHD;
        public float renderworldLodDistMult = 1.0f;
        public float renderworldDetailDistMult = 1.0f;

        public bool rendertimedents = Settings.Default.ShowTimedEntities;
        public bool rendertimedentsalways = false;
        public bool renderinteriors = true;
        public bool renderproxies = false;
        public bool renderchildents = false;//when rendering single ymap, render root only or not...

        public bool rendergrass = true;
        public bool renderdistlodlights = true;

        public bool rendercollisionmeshes = Settings.Default.ShowCollisionMeshes;
        public bool rendercollisionmeshlayerdrawable = true;

        public bool renderskeletons = false;
        private List<RenderSkeletonItem> renderskeletonlist = new List<RenderSkeletonItem>();
        private List<VertexTypePC> skeletonLineVerts = new List<VertexTypePC>();


        public BoundsShaderMode boundsmode = BoundsShaderMode.None;
        public bool renderboundsclip = Settings.Default.BoundsDepthClip;
        public float renderboundsmaxrad = 20000.0f;
        public float renderboundsmaxdist = 10000.0f;
        public List<MapBox> BoundingBoxes = new List<MapBox>();
        public List<MapSphere> BoundingSpheres = new List<MapSphere>();
        public List<MapSphere> HilightSpheres = new List<MapSphere>();
        public List<MapBox> HilightBoxes = new List<MapBox>();
        public List<MapBox> SelectionBoxes = new List<MapBox>();
        public List<MapBox> WhiteBoxes = new List<MapBox>();
        public List<MapSphere> SelectionSpheres = new List<MapSphere>();
        public List<MapSphere> WhiteSpheres = new List<MapSphere>();
        public List<VertexTypePC> SelectionLineVerts = new List<VertexTypePC>();
        public List<VertexTypePC> SelectionTriVerts = new List<VertexTypePC>();


        private YmapEntityDef SelectedCarGenEntity = new YmapEntityDef(); //placeholder entity object for drawing cars

        public DrawableBase SelectedDrawable = null;
        public Dictionary<DrawableModel, bool> SelectionModelDrawFlags = new Dictionary<DrawableModel, bool>();
        public Dictionary<DrawableGeometry, bool> SelectionGeometryDrawFlags = new Dictionary<DrawableGeometry, bool>();
        public bool SelectionFlagsTestAll = false; //to test all renderables for draw flags; for model form




        public List<RenderedDrawable> RenderedDrawables = new List<RenderedDrawable>(); //queued here for later hit tests...
        public List<RenderedBoundComposite> RenderedBoundComps = new List<RenderedBoundComposite>();
        public bool RenderedDrawablesListEnable = false; //whether or not to add rendered drawables to the list
        public bool RenderedBoundCompsListEnable = false; //whether or not to add rendered bound comps to the list








        public Renderer(DXForm form, GameFileCache cache)
        {
            Form = form;
            gameFileCache = cache;
            if (gameFileCache == null)
            {
                gameFileCache = GameFileCacheFactory.Create();
            }
            renderableCache = new RenderableCache();

            var s = Settings.Default;
            camera = new Camera(s.CameraSmoothing, s.CameraSensitivity, s.CameraFieldOfView);
        }


        public bool Init()
        {
            return dxman.Init(Form, false);
        }

        public void Start()
        {
            dxman.Start();
        }

        public void DeviceCreated(Device device, int width, int height)
        {
            currentdevice = device;

            shaders = new ShaderManager(device, dxman);
            shaders.OnWindowResize(width, height); //init the buffers

            renderableCache.OnDeviceCreated(device);

            camera.OnWindowResize(width, height); //init the projection stuff


            markerquad = new UnitQuad(device);

        }

        public void DeviceDestroyed()
        {
            renderableCache.OnDeviceDestroyed();

            markerquad.Dispose();

            shaders.Dispose();

            currentdevice = null;
        }

        public void BuffersResized(int width, int height)
        {
            lock (rendersyncroot)
            {
                camera.OnWindowResize(width, height);
                shaders.OnWindowResize(width, height);
            }
        }

        public void ReloadShaders()
        {
            if (shaders != null)
            {
                shaders.Dispose();
            }
            shaders = new ShaderManager(currentdevice, dxman);
        }


        public void Update(float elapsed, int mouseX, int mouseY)
        {
            framecount++;
            fcelapsed += elapsed;
            if (fcelapsed >= 0.5f)
            {
                fps = framecount * 2;
                framecount = 0;
                fcelapsed -= 0.5f;
            }
            if (elapsed > 0.1f) elapsed = 0.1f;
            currentRealTime += elapsed;
            currentElapsedTime = elapsed;



            UpdateTimeOfDay(elapsed);


            weather.Update(elapsed);

            clouds.Update(elapsed);


            UpdateWindVector(elapsed);

            UpdateGlobalLights();


            camera.SetMousePosition(mouseX, mouseY);

            camera.Update(elapsed);
        }


        public void BeginRender(DeviceContext ctx)
        {
            context = ctx;

            dxman.ClearRenderTarget(context);

            shaders.BeginFrame(context, currentRealTime, currentElapsedTime);

            shaders.EnsureShaderTextures(gameFileCache, renderableCache);




            SelectionLineVerts.Clear();
            SelectionTriVerts.Clear();
            WhiteBoxes.Clear();
            WhiteSpheres.Clear();
            SelectionBoxes.Clear();
            SelectionSpheres.Clear();
            HilightBoxes.Clear();
            HilightSpheres.Clear();
            BoundingBoxes.Clear();
            BoundingSpheres.Clear();

            RenderedDrawables.Clear();
            RenderedBoundComps.Clear();

            renderskeletonlist.Clear();
        }

        public void RenderSkyAndClouds()
        {
            RenderSky();

            RenderClouds();

            shaders.ClearDepth(context);
        }

        public void RenderQueued()
        {
            shaders.RenderQueued(context, camera, currentWindVec);

            RenderSkeletons();
        }

        public void RenderFinalPass()
        {
            shaders.RenderFinalPass(context);
        }

        public void EndRender()
        {
            renderableCache.RenderThreadSync();
        }

        public bool ContentThreadProc()
        {
            bool rcItemsPending = renderableCache.ContentThreadProc();

            return rcItemsPending;
        }



        public void SetTimeOfDay(float hour)
        {
            timeofday = hour;
            timecycle.SetTime(timeofday);
        }

        public void SetWeatherType(string name)
        {
            if (!Monitor.TryEnter(rendersyncroot, 50))
            { return; } //couldn't get a lock...
            weathertype = name;
            weather.SetNextWeather(weathertype);
            Monitor.Exit(rendersyncroot);
        }

        public void SetCameraMode(string modestr)
        {
            lock (rendersyncroot)
            {
                switch (modestr)
                {
                    case "Perspective":
                        camera.IsOrthographic = false;
                        MapViewEnabled = false;
                        camera.UpdateProj = true;
                        break;
                    case "Orthographic":
                        camera.IsOrthographic = true;
                        MapViewEnabled = false;
                        break;
                    case "2D Map":
                        camera.IsOrthographic = true;
                        MapViewEnabled = true;
                        break;
                }
                camera.IsMapView = MapViewEnabled;
            }
        }


        public string GetStatusText()
        {
            int rgc = (shaders != null) ? shaders.RenderedGeometries : 0;
            int crc = renderableCache.LoadedRenderableCount;
            int ctc = renderableCache.LoadedTextureCount;
            int tcrc = renderableCache.MemCachedRenderableCount;
            int tctc = renderableCache.MemCachedTextureCount;
            long vr = renderableCache.TotalGraphicsMemoryUse + (shaders != null ? shaders.TotalGraphicsMemoryUse : 0);
            string vram = TextUtil.GetBytesReadable(vr);
            //StatsLabel.Text = string.Format("Drawn: {0} geom, Loaded: {1}/{5} dr, {2}/{6} tx, Vram: {3}, Fps: {4}", rgc, crc, ctc, vram, fps, tcrc, tctc);
            return string.Format("Drawn: {0} geom, Loaded: {1} dr, {2} tx, Vram: {3}, Fps: {4}", rgc, crc, ctc, vram, fps);
        }



        public void Invalidate(BasePathData path)
        {
            //used to update path graphics.
            renderableCache.Invalidate(path);
        }


        public void UpdateSelectionDrawFlags(DrawableModel model, DrawableGeometry geom, bool rem)
        {
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







        public void RenderMarkers(List<MapMarker> batch)
        {

            shaders.SetRasterizerMode(context, RasterizerMode.SolidDblSided); //hmm they are backwards
            shaders.SetDepthStencilMode(context, markerdepthclip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);
            shaders.SetDefaultBlendState(context);

            var shader = shaders.Marker;
            shader.SetShader(context);
            shader.SetInputLayout(context, VertexType.Default);
            shader.SetSceneVars(context, camera, null, globalLights);

            MapIcon icon = null;
            foreach (var marker in batch)
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



        public void RenderTransformWidget(TransformWidget widget)
        {
            var dsmode = DepthStencilMode.Enabled;
            if (widget.Mode == WidgetMode.Rotation)
            {
                dsmode = DepthStencilMode.DisableAll;
            }

            shaders.SetRasterizerMode(context, RasterizerMode.SolidDblSided);
            shaders.SetDepthStencilMode(context, dsmode);
            shaders.SetDefaultBlendState(context);
            shaders.ClearDepth(context, false);

            var shader = shaders.Widgets;

            widget.Render(context, camera, shader);
        }




        public void RenderSelectionEntityPivot(YmapEntityDef ent)
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

        public void RenderSelectionArrowOutline(Vector3 pos, Vector3 dir, Vector3 up, Quaternion ori, float len, float rad, uint colour)
        {
            Vector3 ax = Vector3.Cross(dir, up);
            Vector3 sx = ax * rad;
            Vector3 sy = up * rad;
            Vector3 sz = dir * len;
            VertexTypePC[] c = new VertexTypePC[8];
            Vector3 d0 = -sx - sy;
            Vector3 d1 = -sx + sy;
            Vector3 d2 = +sx - sy;
            Vector3 d3 = +sx + sy;
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

        public void RenderSelectionNavPoly(YnvPoly poly)
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

        public void RenderSelectionGeometry(MapSelectionMode mode)
        {

            bool clip = true;
            switch (mode)
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




            Vector3 coloursel = new Vector3(0, 1, 0) * globalLights.HdrIntensity * 5.0f;
            Vector3 colourwht = new Vector3(1, 1, 1) * globalLights.HdrIntensity * 10.0f;
            var shader = shaders.Bounds;

            if ((WhiteBoxes.Count > 0) || (SelectionBoxes.Count > 0))
            {
                shader.SetMode(BoundsShaderMode.Box);
                shader.SetShader(context);
                shader.SetInputLayout(context, VertexType.Default);
                shader.SetSceneVars(context, camera, null, globalLights);
                shader.SetColourVars(context, new Vector4(colourwht, 1));
                for (int i = 0; i < WhiteBoxes.Count; i++)
                {
                    MapBox mb = WhiteBoxes[i];
                    shader.SetBoxVars(context, mb.CamRelPos, mb.BBMin, mb.BBMax, mb.Orientation, mb.Scale);
                    shader.DrawBox(context);
                }
                shader.SetColourVars(context, new Vector4(coloursel, 1));
                for (int i = 0; i < SelectionBoxes.Count; i++)
                {
                    MapBox mb = SelectionBoxes[i];
                    shader.SetBoxVars(context, mb.CamRelPos, mb.BBMin, mb.BBMax, mb.Orientation, mb.Scale);
                    shader.DrawBox(context);
                }
                shader.UnbindResources(context);
            }

            if ((WhiteSpheres.Count > 0) || (SelectionSpheres.Count > 0))
            {
                shader.SetMode(BoundsShaderMode.Sphere);
                shader.SetShader(context);
                shader.SetInputLayout(context, VertexType.Default);
                shader.SetSceneVars(context, camera, null, globalLights);
                shader.SetColourVars(context, new Vector4(colourwht, 1));
                for (int i = 0; i < WhiteSpheres.Count; i++)
                {
                    MapSphere ms = WhiteSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
                shader.SetColourVars(context, new Vector4(coloursel, 1));
                for (int i = 0; i < SelectionSpheres.Count; i++)
                {
                    MapSphere ms = SelectionSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
                shader.UnbindResources(context);
            }


        }

        public void RenderMouseHit(BoundsShaderMode mode, bool clip, ref Vector3 camrel, ref Vector3 bbmin, ref Vector3 bbmax, ref Vector3 scale, ref Quaternion ori, float bsphrad)
        {
            Vector3 colour = new Vector3(1, 1, 1);
            colour *= globalLights.HdrIntensity * 5.0f;

            shaders.SetDepthStencilMode(context, clip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);

            //render moused object box.
            var shader = shaders.Bounds;
            shader.SetMode(mode);
            shader.SetShader(context);
            shader.SetInputLayout(context, VertexType.Default);
            shader.SetSceneVars(context, camera, null, globalLights);
            shader.SetColourVars(context, new Vector4(colour, 1)); //white box

            if (mode == BoundsShaderMode.Box)
            {
                shader.SetBoxVars(context, camrel, bbmin, bbmax, ori, scale);
                shader.DrawBox(context);
            }
            else if (mode == BoundsShaderMode.Sphere)
            {
                shader.SetSphereVars(context, camrel, bsphrad);
                shader.DrawSphere(context);
            }

            shader.UnbindResources(context);
        }


        public void RenderBounds(MapSelectionMode mode)
        {
            //immediately render the entity bounding boxes/spheres - depending on boundsmode

            bool clip = renderboundsclip;

            switch (mode)
            {
                case MapSelectionMode.WaterQuad:
                case MapSelectionMode.MloInstance:
                    clip = false;
                    break;
            }


            Vector3 colour = new Vector3(0, 0, 1) * globalLights.HdrIntensity;
            Vector3 colourhi = new Vector3(0, 1, 1) * globalLights.HdrIntensity;

            shaders.SetDepthStencilMode(context, clip ? DepthStencilMode.Enabled : DepthStencilMode.DisableAll);
            var shader = shaders.Bounds;

            if ((BoundingBoxes.Count > 0) || (HilightBoxes.Count > 0))
            {
                shader.SetMode(BoundsShaderMode.Box);
                shader.SetShader(context);
                shader.SetInputLayout(context, VertexType.Default);
                shader.SetSceneVars(context, camera, null, globalLights);
                shader.SetColourVars(context, new Vector4(colour, 1));

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

            if ((BoundingSpheres.Count > 0) || (HilightSpheres.Count > 0))
            {
                shader.SetMode(BoundsShaderMode.Sphere);
                shader.SetShader(context);
                shader.SetInputLayout(context, VertexType.Default);
                shader.SetSceneVars(context, camera, null, globalLights);
                shader.SetColourVars(context, new Vector4(colour, 1));

                for (int i = 0; i < BoundingSpheres.Count; i++)
                {
                    MapSphere ms = BoundingSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
                shader.SetColourVars(context, new Vector4(colourhi, 1));
                for (int i = 0; i < HilightSpheres.Count; i++)
                {
                    MapSphere ms = HilightSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
            }



            shader.UnbindResources(context);
        }








        private void RenderSkeleton(Renderable renderable, YmapEntityDef entity)
        {
            RenderSkeletonItem item = new RenderSkeletonItem();
            item.Renderable = renderable;
            item.Entity = entity;
            renderskeletonlist.Add(item);
        }

        private void RenderSkeletons()
        {

            skeletonLineVerts.Clear();

            const uint cgrn = 4278255360;// (uint)new Color4(0.0f, 1.0f, 0.0f, 1.0f).ToRgba();
            const uint cblu = 4294901760;// (uint)new Color4(0.0f, 0.0f, 1.0f, 1.0f).ToRgba();
            VertexTypePC v1 = new VertexTypePC();
            VertexTypePC v2 = new VertexTypePC();
            v1.Colour = cgrn;
            v2.Colour = cblu;

            foreach (var item in renderskeletonlist)
            {
                YmapEntityDef entity = item.Entity;
                DrawableBase drawable = item.Renderable.Key;
                Skeleton skeleton = drawable?.Skeleton;
                if (skeleton == null) continue;

                var pinds = skeleton.ParentIndices;
                var bones = skeleton.Bones;
                if ((pinds == null) || (bones == null)) continue;
                var xforms = skeleton.Transformations;

                int cnt = Math.Min(pinds.Length, bones.Count);
                for (int i = 0; i < cnt; i++)
                {
                    var pind = pinds[i];
                    var bone = bones[i];
                    var pbone = bone.Parent;
                    if (pbone == null) continue; //nothing to draw for the root bone

                    if (xforms != null)//how to use xforms? bind pose?
                    {
                        var xform = (i < xforms.Length) ? xforms[i] : Matrix.Identity;
                        var pxform = (pind < xforms.Length) ? xforms[pind] : Matrix.Identity;
                    }
                    else
                    {
                    }


                    //draw line from bone's position to parent position...
                    Vector3 lbeg = Vector3.Zero;
                    Vector3 lend = bone.Translation;// bone.Rotation.Multiply();
                    while (pbone != null)
                    {
                        lbeg = pbone.Rotation.Multiply(lbeg) + pbone.Translation;
                        lend = pbone.Rotation.Multiply(lend) + pbone.Translation;
                        pbone = pbone.Parent;
                    }


                    if (entity != null)
                    {
                        lbeg = entity.Position + entity.Orientation.Multiply(lbeg * entity.Scale);
                        lend = entity.Position + entity.Orientation.Multiply(lend * entity.Scale);
                    }

                    v1.Position = lbeg;
                    v2.Position = lend;
                    skeletonLineVerts.Add(v1);
                    skeletonLineVerts.Add(v2);

                }


            }





            if (skeletonLineVerts.Count > 0)
            {
                RenderLines(skeletonLineVerts, DepthStencilMode.DisableAll);
            }

        }



        public void RenderLines(List<VertexTypePC> linelist, DepthStencilMode dsmode = DepthStencilMode.Enabled)
        {
            shaders.SetDepthStencilMode(context, dsmode);
            shaders.Paths.RenderLines(context, linelist, camera, shaders.GlobalLights);
        }






        private void RenderSky()
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

                if (rendermoon)
                {
                    skydomeytd.TextureDict.Dict.TryGetValue(234339206, out moon); //moon-new hash
                }
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


                if ((rendermoon) && (moontex != null) && (moontex.IsLoaded))
                {
                    shader.RenderMoon(context, camera, weather, globalLights, moontex);
                }



                shader.UnbindResources(context);
            }

        }

        private void RenderClouds()
        {
            if (MapViewEnabled) return;
            if (!renderclouds) return;
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
                    uint dhash = JenkHash.GenHash(layer.Filename.ToLowerInvariant());
                    Archetype arch = gameFileCache.GetArchetype(dhash);
                    if (arch == null)
                    { continue; }

                    if (Math.Max(camera.Position.Z, 0.0f) < layer.HeightTigger) continue;

                    var drw = gameFileCache.TryGetDrawable(arch);
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



        public void RenderWaterQuads(List<WaterQuad> waterquads)
        {
            foreach (var quad in waterquads)
            {
                RenderableWaterQuad rquad = renderableCache.GetRenderableWaterQuad(quad);
                if ((rquad != null) && (rquad.IsLoaded))
                {
                    rquad.CamRel = -camera.Position;
                    shaders.Enqueue(rquad);
                }
            }
        }

        public void RenderPaths(List<YndFile> ynds)
        {
            foreach (var ynd in ynds)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(ynd);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }
        }

        public void RenderTrainTracks(List<TrainTrack> tracks)
        {
            foreach (var track in tracks)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(track);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }
        }

        public void RenderNavMeshes(List<YnvFile> ynvs)
        {
            foreach (var ynv in ynvs)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(ynv);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }
        }

        public void RenderNavMesh(YnvFile ynv)
        {
            RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(ynv);
            if ((rnd != null) && (rnd.IsLoaded))
            {
                shaders.Enqueue(rnd);
            }
        }

        public void RenderScenarios(List<YmtFile> ymts)
        {
            foreach (var scenario in ymts)
            {
                RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(scenario.ScenarioRegion);
                if ((rnd != null) && (rnd.IsLoaded))
                {
                    shaders.Enqueue(rnd);
                }
            }
        }

        public void RenderPopZones(PopZones zones)
        {
            RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(zones);
            if ((rnd != null) && (rnd.IsLoaded))
            {
                shaders.Enqueue(rnd);
            }
        }






        public void RenderWorld(Dictionary<MetaHash, YmapFile> renderworldVisibleYmapDict, IEnumerable<Entity> spaceEnts)
        {

            renderworldentities.Clear();
            renderworldrenderables.Clear();

            VisibleYmaps.Clear();
            foreach (var ymap in renderworldVisibleYmapDict.Values)
            {
                if (!RenderWorldYmapIsVisible(ymap)) continue;
                VisibleYmaps.Add(ymap);
            }

            if (spaceEnts != null)
            {
                foreach (var ae in spaceEnts) //used by active space entities (eg "bullets")
                {
                    if (ae.EntityDef == null) continue; //nothing to render...
                    RenderWorldCalcEntityVisibility(ae.EntityDef);
                    renderworldentities.Add(ae.EntityDef);
                }
            }

            if (MapViewEnabled)
            {
                //find the max Z value for positioning camera in map view, to help shadows
                //float minZ = float.MaxValue;
                float maxZ = float.MinValue;
                float cvwidth = camera.OrthographicSize * camera.AspectRatio * 0.5f;
                float cvheight = camera.OrthographicSize * 0.5f;
                float cvwmin = camera.Position.X - cvwidth; //TODO:make all these vars global...
                float cvwmax = camera.Position.X + cvwidth;
                float cvhmin = camera.Position.Y - cvheight;
                float cvhmax = camera.Position.Y + cvheight;

                for (int y = 0; y < VisibleYmaps.Count; y++)
                {
                    var ymap = VisibleYmaps[y];
                    if (ymap.AllEntities != null)
                    {
                        for (int i = 0; i < ymap.AllEntities.Length; i++)
                        {
                            var ent = ymap.AllEntities[i];
                            if ((ent.Position.Z < 1000.0f) && (ent.BSRadius < 500.0f))
                            {
                                float r = ent.BSRadius;
                                if (((ent.Position.X + r) > cvwmin) && ((ent.Position.X - r) < cvwmax) && ((ent.Position.Y + r) > cvhmin) && ((ent.Position.Y - r) < cvhmax))
                                {
                                    //minZ = Math.Min(minZ, ent.BBMin.Z);
                                    maxZ = Math.Max(maxZ, ent.BBMax.Z + 50.0f);//add some bias to avoid clipping things...
                                }
                            }
                        }
                    }
                }

                //move the camera closer to the geometry, to help shadows in map view.
                if (maxZ == float.MinValue) maxZ = 1000.0f;
                camera.Position.Z = Math.Min(maxZ, 1000.0f);
            }


            for (int y = 0; y < VisibleYmaps.Count; y++)
            {
                var ymap = VisibleYmaps[y];
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
                                    ent.ParentName = p.CEntityDef.archetypeName;
                                }
                            }
                            else
                            { }//should only happen if parent ymap not loaded yet...
                        }
                        RenderWorldRecurseCalcEntityVisibility(ent);
                    }
                }
            }

            for (int y = 0; y < VisibleYmaps.Count; y++)
            {
                var ymap = VisibleYmaps[y];
                if (ymap.RootEntities != null)
                {
                    for (int i = 0; i < ymap.RootEntities.Length; i++)
                    {
                        var ent = ymap.RootEntities[i];
                        RenderWorldRecurseAddEntities(ent);
                    }
                }
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
                var drawable = gameFileCache.TryGetDrawable(arch);
                Renderable rndbl = TryGetRenderable(arch, drawable);
                if ((rndbl != null) && rndbl.IsLoaded && (rndbl.AllTexturesLoaded || !waitforchildrentoload))
                {
                    RenderableEntity rent = new RenderableEntity();
                    rent.Entity = ent;
                    rent.Renderable = rndbl;
                    renderworldrenderables.Add(rent);
                }
                else if (waitforchildrentoload)
                {
                    //todo: render parent if children loading.......
                }

                if (ent.IsMlo && rendercollisionmeshes && renderinteriors)
                {
                    RenderInteriorCollisionMesh(ent);
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
                for (int y = 0; y < VisibleYmaps.Count; y++)
                {
                    var ymap = VisibleYmaps[y];
                    if (ymap.GrassInstanceBatches != null)
                    {
                        RenderYmapGrass(ymap);
                    }
                }
            }
            if (renderdistlodlights && timecycle.IsNightTime)
            {
                for (int y = 0; y < VisibleYmaps.Count; y++)
                {
                    var ymap = VisibleYmaps[y];
                    if (ymap.DistantLODLights != null)
                    {
                        RenderYmapDistantLODLights(ymap);
                    }
                }
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
        private void RenderWorldCalcEntityVisibility(YmapEntityDef ent)
        {
            float dist = (ent.Position - camera.Position).Length();

            if (MapViewEnabled)
            {
                dist = camera.OrthographicSize / MapViewDetail;
            }


            var loddist = ent.CEntityDef.lodDist;
            var cloddist = ent.CEntityDef.childLodDist;

            if (loddist <= 0.0f)//usually -1 or -2
            {
                if (ent.Archetype != null)
                {
                    loddist = ent.Archetype.LodDist * renderworldLodDistMult;
                }
            }
            else if (ent.CEntityDef.lodLevel == Unk_1264241711.LODTYPES_DEPTH_ORPHANHD)
            {
                loddist *= renderworldDetailDistMult * 1.5f; //orphan view dist adjustment...
            }
            else
            {
                loddist *= renderworldLodDistMult;
            }


            if (cloddist <= 0)
            {
                if (ent.Archetype != null)
                {
                    cloddist = ent.Archetype.LodDist * renderworldLodDistMult;
                }
            }
            else
            {
                cloddist *= renderworldLodDistMult;
            }


            ent.IsVisible = (dist <= loddist);
            ent.ChildrenVisible = (dist <= cloddist) && (ent.CEntityDef.numChildren > 0);



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
        }
        private void RenderWorldRecurseCalcEntityVisibility(YmapEntityDef ent)
        {
            RenderWorldCalcEntityVisibility(ent);
            if (ent.ChildrenVisible)
            {
                if (ent.Children != null)
                {
                    for (int i = 0; i < ent.Children.Length; i++)
                    {
                        var child = ent.Children[i];
                        if (child.Ymap == ent.Ymap)
                        {
                            RenderWorldRecurseCalcEntityVisibility(child);
                        }
                    }
                }
            }
        }
        private void RenderWorldRecurseAddEntities(YmapEntityDef ent)
        {
            bool hide = ent.ChildrenVisible;
            bool force = (ent.Parent != null) && ent.Parent.ChildrenVisible && !hide;
            if (force || (ent.IsVisible && !hide))
            {
                if (ent.Archetype != null)
                {
                    if (!RenderIsEntityFinalRender(ent)) return;

                    var bscent = ent.Position + ent.BSCenter - camera.Position;
                    float bsrad = ent.BSRadius;
                    if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref bscent, bsrad))
                    {
                        return; //frustum cull
                    }


                    renderworldentities.Add(ent);


                    if (renderinteriors && ent.IsMlo) //render Mlo child entities...
                    {
                        if ((ent.MloInstance != null) && (ent.MloInstance.Entities != null))
                        {
                            for (int j = 0; j < ent.MloInstance.Entities.Length; j++)
                            {
                                var intent = ent.MloInstance.Entities[j];
                                if (intent.Archetype == null) continue; //missing archetype...
                                if (!RenderIsEntityFinalRender(intent)) continue; //proxy or something..

                                intent.IsVisible = true;

                                var iebscent = intent.Position + intent.BSCenter - camera.Position;
                                float iebsrad = intent.BSRadius;
                                if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref iebscent, iebsrad))
                                {
                                    continue; //frustum cull interior ents
                                }

                                renderworldentities.Add(intent);
                            }
                        }
                    }

                }
            }
            if (ent.IsVisible && ent.ChildrenVisible && (ent.Children != null))
            {
                for (int i = 0; i < ent.Children.Length; i++)
                {
                    var child = ent.Children[i];
                    if (child.Ymap == ent.Ymap)
                    {
                        RenderWorldRecurseAddEntities(ent.Children[i]);
                    }
                }
            }
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








        public void RenderYmap(YmapFile ymap)
        {
            if (ymap == null) return;
            if (!ymap.Loaded) return;

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
                    Vector3 camrel = entity.Position - camera.Position;
                    float dist = (camrel + entity.BSCenter).Length();
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
                var drbl = gameFileCache.TryGetDrawable(arch);
                var rndbl = TryGetRenderable(arch, drbl);
                var instb = renderableCache.GetRenderableInstanceBatch(batch);
                if (rndbl == null) continue; //no renderable
                if (!(rndbl.IsLoaded && (rndbl.AllTexturesLoaded || !waitforchildrentoload))) continue; //not loaded yet
                if ((instb == null) || !instb.IsLoaded) continue;

                RenderableInstanceBatchInst binst = new RenderableInstanceBatchInst();
                binst.Batch = instb;
                binst.Renderable = rndbl;

                shaders.Enqueue(ref binst);

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






        public bool RenderFragment(Archetype arch, YmapEntityDef ent, FragType f, uint txdhash = 0)
        {

            RenderDrawable(f.Drawable, arch, ent, txdhash);

            if (f.Unknown_F8h_Data != null) //cloth
            {
                RenderDrawable(f.Unknown_F8h_Data, arch, ent, txdhash);
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
                            RenderDrawable(pch.Drawable1, arch, ent, txdhash);
                        }
                    }
                }
            }

            return true;
        }

        public bool RenderArchetype(Archetype arche, YmapEntityDef entity, Renderable rndbl = null, bool cull = true)
        {
            //enqueue a single archetype for rendering.

            if (arche == null) return false;

            Vector3 entpos = (entity != null) ? entity.Position : Vector3.Zero;
            Vector3 camrel = entpos - camera.Position;

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
                var drawable = gameFileCache.TryGetDrawable(arche);
                rndbl = TryGetRenderable(arche, drawable);
            }

            if (rndbl != null)
            {
                res = RenderRenderable(rndbl, arche, entity);


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
                            bool res2 = RenderRenderable(rndbl, arche, entity);
                            res = res || res2;
                        }
                    }
                }
            }


            return res;
        }

        public bool RenderDrawable(DrawableBase drawable, Archetype arche, YmapEntityDef entity, uint txdHash = 0)
        {
            //enqueue a single drawable for rendering.

            if (drawable == null)
                return false;

            Renderable rndbl = TryGetRenderable(arche, drawable, txdHash);
            if (rndbl == null)
                return false;

            return RenderRenderable(rndbl, arche, entity);
        }

        private bool RenderRenderable(Renderable rndbl, Archetype arche, YmapEntityDef entity)
        {
            //enqueue a single renderable for rendering.

            if (!rndbl.IsLoaded) return false;


            if (RenderedDrawablesListEnable) //for later hit tests
            {
                var rd = new RenderedDrawable();
                rd.Drawable = rndbl.Key;
                rd.Archetype = arche;
                rd.Entity = entity;
                RenderedDrawables.Add(rd);
            }

            bool isselected = SelectionFlagsTestAll || (rndbl.Key == SelectedDrawable);

            Vector3 camrel = -camera.Position;
            Vector3 position = Vector3.Zero;
            Vector3 scale = Vector3.One;
            Quaternion orientation = Quaternion.Identity;
            uint tintPaletteIndex = 0;
            Vector3 bbmin = (arche != null) ? arche.BBMin : rndbl.Key.BoundingBoxMin.XYZ();
            Vector3 bbmax = (arche != null) ? arche.BBMax : rndbl.Key.BoundingBoxMax.XYZ();
            Vector3 bscen = (arche != null) ? arche.BSCenter : rndbl.Key.BoundingCenter;
            float radius = (arche != null) ? arche.BSRadius : rndbl.Key.BoundingSphereRadius;
            if (entity != null)
            {
                position = entity.Position;
                scale = entity.Scale;
                orientation = entity.Orientation;
                tintPaletteIndex = entity.CEntityDef.tintValue;
                bbmin = entity.BBMin;
                bbmax = entity.BBMax;
                bscen = entity.BSCenter;
                camrel += position;
            }
            float distance = (camrel + bscen).Length();


            if (rendercollisionmeshes && rendercollisionmeshlayerdrawable)
            {
                Drawable sdrawable = rndbl.Key as Drawable;
                if ((sdrawable != null) && (sdrawable.Bound != null))
                {
                    RenderCollisionMesh(sdrawable.Bound, entity);
                }
            }
            if (renderskeletons && rndbl.HasSkeleton)
            {
                RenderSkeleton(rndbl, entity);
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

                        shaders.Enqueue(ref rginst);
                    }
                }
            }
            else
            {
                retval = false;
            }
            return retval;
        }




        public void RenderCar(Vector3 pos, Quaternion ori, MetaHash modelHash, MetaHash modelSetHash, bool valign = false)
        {

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
                if (valign)
                {
                    float minz = caryft.Fragment.PhysicsLODGroup?.PhysicsLOD1?.Bound?.BoundingBoxMin.Z ?? 0.0f;
                    pos.Z -= minz;
                }

                SelectedCarGenEntity.SetPosition(pos);
                SelectedCarGenEntity.SetOrientation(ori);

                RenderFragment(null, SelectedCarGenEntity, caryft.Fragment, carhash);
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

        public void RenderCollisionMesh(Bounds bounds, YmapEntityDef entity)
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
                                shaders.Enqueue(ref rbginst);
                            }

                            if (RenderedBoundCompsListEnable) //for later hit tests
                            {
                                var rb = new RenderedBoundComposite();
                                rb.BoundComp = rndbc;
                                rb.Entity = entity;
                                RenderedBoundComps.Add(rb);
                            }
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


            var yptTexDict = (drawable.Owner as YptFile)?.PtfxList?.TextureDictionary;

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
                                if (yptTexDict != null) //for ypt files, first try the embedded tex dict..
                                {
                                    var dtex = yptTexDict.Lookup(tex.NameHash);
                                    rdtex = renderableCache.GetRenderableTexture(dtex);
                                }
                                else if (texDict != 0)
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




    }


    public struct RenderedDrawable
    {
        public DrawableBase Drawable;
        public Archetype Archetype;
        public YmapEntityDef Entity;
    }
    public struct RenderedBoundComposite
    {
        public RenderableBoundComposite BoundComp;
        public YmapEntityDef Entity;
    }

    public struct RenderSkeletonItem
    {
        public Renderable Renderable;
        public YmapEntityDef Entity;
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





}
