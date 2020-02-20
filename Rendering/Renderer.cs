﻿using CodeWalker.GameFiles;
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
        public RenderableCache RenderableCache { get { return renderableCache; } }

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


        private RenderLodManager LodManager = new RenderLodManager();

        private List<YmapEntityDef> renderworldentities = new List<YmapEntityDef>(); //used when rendering world view.
        private List<RenderableEntity> renderworldrenderables = new List<RenderableEntity>();
        private Dictionary<Archetype, Renderable> ArchetypeRenderables = new Dictionary<Archetype, Renderable>();
        private Dictionary<YmapEntityDef, Renderable> RequiredParents = new Dictionary<YmapEntityDef, Renderable>();
        private List<YmapEntityDef> RenderEntities = new List<YmapEntityDef>();

        public Dictionary<uint, YmapEntityDef> HideEntities = new Dictionary<uint, YmapEntityDef>();//dictionary of entities to hide, for cutscenes to use 

        public bool ShowScriptedYmaps = true;
        public List<YmapFile> VisibleYmaps = new List<YmapFile>();

        public rage__eLodType renderworldMaxLOD = rage__eLodType.LODTYPES_DEPTH_ORPHANHD;
        public float renderworldLodDistMult = 1.0f;
        public float renderworldDetailDistMult = 1.0f;

        public bool rendertimedents = Settings.Default.ShowTimedEntities;
        public bool rendertimedentsalways = false;
        public bool renderinteriors = true;
        public bool renderproxies = false;
        public bool renderchildents = false;//when rendering single ymap, render root only or not...
        public bool renderentities = true;
        public bool rendergrass = true;
        public bool renderlights = true; //render individual drawable lights
        public bool renderlodlights = true; //render LOD lights from ymaps
        public bool renderdistlodlights = true; //render distant lod lights (coronas)
        public bool rendercars = false;

        public bool rendercollisionmeshes = Settings.Default.ShowCollisionMeshes;
        public bool rendercollisionmeshlayerdrawable = true;

        public bool renderskeletons = false;
        private List<RenderSkeletonItem> renderskeletonlist = new List<RenderSkeletonItem>();
        private List<VertexTypePC> skeletonLineVerts = new List<VertexTypePC>();

        public bool renderhdtextures = true;

        public bool swaphemisphere = false;//can be used to get better lighting in model viewers


        public MapSelectionMode SelectionMode = MapSelectionMode.Entity; //to assist in rendering embedded collisions properly...


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
        public Dictionary<DrawableBase, bool> SelectionDrawableDrawFlags = new Dictionary<DrawableBase, bool>();
        public Dictionary<DrawableModel, bool> SelectionModelDrawFlags = new Dictionary<DrawableModel, bool>();
        public Dictionary<DrawableGeometry, bool> SelectionGeometryDrawFlags = new Dictionary<DrawableGeometry, bool>();
        public bool SelectionFlagsTestAll = false; //to test all renderables for draw flags; for model form




        public List<RenderedDrawable> RenderedDrawables = new List<RenderedDrawable>(); //queued here for later hit tests...
        public List<RenderedBoundComposite> RenderedBoundComps = new List<RenderedBoundComposite>();
        public bool RenderedDrawablesListEnable = false; //whether or not to add rendered drawables to the list
        public bool RenderedBoundCompsListEnable = false; //whether or not to add rendered bound comps to the list


        private List<YtdFile> tryGetRenderableSDtxds = new List<YtdFile>();
        private List<YtdFile> tryGetRenderableHDtxds = new List<YtdFile>();






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

            HideEntities.Clear();
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



        public void Invalidate(Bounds bounds)
        {
            renderableCache.Invalidate(bounds);
        }
        public void Invalidate(BasePathData path)
        {
            renderableCache.Invalidate(path);
        }
        public void Invalidate(YmapGrassInstanceBatch batch)
        {
            renderableCache.Invalidate(batch);
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

                if (swaphemisphere)
                {
                    sundir.Y = -sundir.Y;
                }

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

        public void RenderBrushRadiusOutline(Vector3 position, Vector3 dir, Vector3 up, float radius, uint col)
        {
            const int Reso = 36;
            const float MaxDeg = 360f;
            const float DegToRad = 0.0174533f;
            const float Ang = DegToRad * MaxDeg / Reso;

            var axis = Vector3.Cross(dir, up);
            var c = new VertexTypePC[Reso];

            for (var i = 0; i < Reso; i++)
            {
                var rDir = Quaternion.RotationAxis(dir, i * Ang).Multiply(axis);
                c[i].Position = position + (rDir * radius);
                c[i].Colour = col;
            }

            for (var i = 0; i < c.Length; i++)
            {
                SelectionLineVerts.Add(c[i]);
                SelectionLineVerts.Add(c[(i + 1) % c.Length]);
            }

            SelectionLineVerts.Add(new VertexTypePC{Colour = col, Position = position});
            SelectionLineVerts.Add(new VertexTypePC { Colour = col, Position = position + dir * 2f});
        }

        public void RenderMouseHit(BoundsShaderMode mode, ref Vector3 camrel, ref Vector3 bbmin, ref Vector3 bbmax, ref Vector3 scale, ref Quaternion ori, float bsphrad)
        {
            if (mode == BoundsShaderMode.Box)
            {
                var wbox = new MapBox();
                wbox.CamRelPos = camrel;
                wbox.BBMin = bbmin;
                wbox.BBMax = bbmax;
                wbox.Scale = scale;
                wbox.Orientation = ori;
                WhiteBoxes.Add(wbox);
            }
            else if (mode == BoundsShaderMode.Sphere)
            {
                var wsph = new MapSphere();
                wsph.CamRelPos = camrel;
                wsph.Radius = bsphrad;
                WhiteSpheres.Add(wsph);
            }
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

        public void RenderSelectionCircle(Vector3 position, float radius, uint col)
        {
            const int Reso = 36;
            const float MaxDeg = 360f;
            const float DegToRad = 0.0174533f;
            const float Ang = DegToRad * MaxDeg / Reso;

            var dir = Vector3.Normalize(position - camera.Position);
            var up = Vector3.Normalize(dir.GetPerpVec());
            var axis = Vector3.Cross(dir, up);
            var c = new VertexTypePC[Reso];

            for (var i = 0; i < Reso; i++)
            {
                var rDir = Quaternion.RotationAxis(dir, i * Ang).Multiply(axis);
                c[i].Position = position + (rDir * radius);
                c[i].Colour = col;
            }

            for (var i = 0; i < c.Length; i++)
            {
                SelectionLineVerts.Add(c[i]);
                SelectionLineVerts.Add(c[(i + 1) % c.Length]);
            }
        }

        public void RenderSelectionBox(Vector3 p1, Vector3 p2, Vector3 a2, Vector3 a3, uint col)
        {
            VertexTypePC v = new VertexTypePC();
            v.Colour = col;
            var c1 = p1 - a2 - a3;
            var c2 = p1 - a2 + a3;
            var c3 = p1 + a2 + a3;
            var c4 = p1 + a2 - a3;
            var c5 = p2 - a2 - a3;
            var c6 = p2 - a2 + a3;
            var c7 = p2 + a2 + a3;
            var c8 = p2 + a2 - a3;
            v.Position = c1; SelectionLineVerts.Add(v);
            v.Position = c2; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c3; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c4; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c1; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c5; SelectionLineVerts.Add(v);
            v.Position = c2; SelectionLineVerts.Add(v);
            v.Position = c6; SelectionLineVerts.Add(v);
            v.Position = c3; SelectionLineVerts.Add(v);
            v.Position = c7; SelectionLineVerts.Add(v);
            v.Position = c4; SelectionLineVerts.Add(v);
            v.Position = c8; SelectionLineVerts.Add(v);
            v.Position = c5; SelectionLineVerts.Add(v);
            v.Position = c6; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c7; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c8; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
            v.Position = c5; SelectionLineVerts.Add(v);
        }

        public void RenderSelectionNavPoly(YnvPoly poly)
        {
            ////draw poly triangles
            var pcolour = new Color4(1.0f, 1.0f, 1.0f, 0.2f);
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

        public void RenderSelectionNavPolyOutline(YnvPoly poly, uint colourval)
        {
            //var colourval = (uint)colour.ToRgba();
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
            //    Renderer.SelectionTriVerts.Add(v0);
            //    Renderer.SelectionTriVerts.Add(v1);
            //    Renderer.SelectionTriVerts.Add(v2);
            //    Renderer.SelectionTriVerts.Add(v0);
            //    Renderer.SelectionTriVerts.Add(v2);
            //    Renderer.SelectionTriVerts.Add(v1);
            //}
        }

        public void RenderSelectionCollisionPolyOutline(BoundPolygon poly, uint colourval, YmapEntityDef entity)
        {
            var bgeom = poly?.Owner;
            if (bgeom == null) return;

            VertexTypePC v = new VertexTypePC();
            v.Colour = colourval;

            var ori = Quaternion.Identity;
            var pos = Vector3.Zero;
            var sca = Vector3.One;
            if (entity != null)
            {
                ori = entity.Orientation;
                pos = entity.Position;
                sca = entity.Scale;
            }

            if (poly is BoundPolygonTriangle ptri)
            {
                var p1 = pos + (ori.Multiply(ptri.Vertex1) * sca);
                var p2 = pos + (ori.Multiply(ptri.Vertex2) * sca);
                var p3 = pos + (ori.Multiply(ptri.Vertex3) * sca);
                v.Position = p1; SelectionLineVerts.Add(v);
                v.Position = p2; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
                v.Position = p3; SelectionLineVerts.Add(v); SelectionLineVerts.Add(v);
                v.Position = p1; SelectionLineVerts.Add(v);
            }
            else if (poly is BoundPolygonSphere psph)
            {
                var p1 = pos + (ori.Multiply(psph.Position) * sca);
                RenderSelectionCircle(p1, psph.sphereRadius * 1.03f, colourval);//enlarge the circle to make it more visible..
            }
            else if (poly is BoundPolygonCapsule pcap)
            {
                var p1 = pos + (ori.Multiply(pcap.Vertex1) * sca);
                var p2 = pos + (ori.Multiply(pcap.Vertex2) * sca);
                var a1 = Vector3.Normalize(p2 - p1);
                var a2 = Vector3.Normalize(a1.GetPerpVec());
                var a3 = Vector3.Normalize(Vector3.Cross(a1, a2));
                a1 *= pcap.capsuleRadius;
                a2 *= pcap.capsuleRadius;
                a3 *= pcap.capsuleRadius;
                RenderSelectionBox(p1 - a1, p2 + a1, a2, a3, colourval);
            }
            else if (poly is BoundPolygonBox pbox)
            {
                var p1 = pos + (ori.Multiply(pbox.Vertex1) * sca);
                var p2 = pos + (ori.Multiply(pbox.Vertex2) * sca);
                var p3 = pos + (ori.Multiply(pbox.Vertex3) * sca);
                var p4 = pos + (ori.Multiply(pbox.Vertex4) * sca);
                var p5 = (p1 + p2) * 0.5f;
                var p6 = (p3 + p4) * 0.5f;
                var a1 = (p6 - p5);
                var a2 = (p3 - (p1 + a1)) * 0.5f;
                var a3 = (p4 - (p1 + a1)) * 0.5f;
                RenderSelectionBox(p5, p6, a2, a3, colourval);
            }
            else if (poly is BoundPolygonCylinder pcyl)
            {
                var p1 = pos + (ori.Multiply(pcyl.Vertex1) * sca);
                var p2 = pos + (ori.Multiply(pcyl.Vertex2) * sca);
                var a1 = Vector3.Normalize(p2 - p1);
                var a2 = Vector3.Normalize(a1.GetPerpVec());
                var a3 = Vector3.Normalize(Vector3.Cross(a1, a2));
                a2 *= pcyl.cylinderRadius;
                a3 *= pcyl.cylinderRadius;
                RenderSelectionBox(p1, p2, a2, a3, colourval);
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
                shader.SetColourVars(context, new Vector4(coloursel, 1));
                for (int i = 0; i < SelectionBoxes.Count; i++)
                {
                    MapBox mb = SelectionBoxes[i];
                    shader.SetBoxVars(context, mb.CamRelPos, mb.BBMin, mb.BBMax, mb.Orientation, mb.Scale);
                    shader.DrawBox(context);
                }
                shader.SetColourVars(context, new Vector4(colourwht, 1));
                for (int i = 0; i < WhiteBoxes.Count; i++)
                {
                    MapBox mb = WhiteBoxes[i];
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
                shader.SetColourVars(context, new Vector4(coloursel, 1));
                for (int i = 0; i < SelectionSpheres.Count; i++)
                {
                    MapSphere ms = SelectionSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
                shader.SetColourVars(context, new Vector4(colourwht, 1));
                for (int i = 0; i < WhiteSpheres.Count; i++)
                {
                    MapSphere ms = WhiteSpheres[i];
                    shader.SetSphereVars(context, ms.CamRelPos, ms.Radius);
                    shader.DrawSphere(context);
                }
                shader.UnbindResources(context);
            }


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

            const uint cred = 4278190335;// (uint)new Color4(1.0f, 0.0f, 0.0f, 1.0f).ToRgba();
            const uint cgrn = 4278255360;// (uint)new Color4(0.0f, 1.0f, 0.0f, 1.0f).ToRgba();
            const uint cblu = 4294901760;// (uint)new Color4(0.0f, 0.0f, 1.0f, 1.0f).ToRgba();
            VertexTypePC vr = new VertexTypePC();
            VertexTypePC vg = new VertexTypePC();
            VertexTypePC vb = new VertexTypePC();
            vr.Colour = cred;
            vg.Colour = cgrn;
            vb.Colour = cblu;

            foreach (var item in renderskeletonlist)
            {
                YmapEntityDef entity = item.Entity;
                DrawableBase drawable = item.Renderable.Key;
                Skeleton skeleton = drawable?.Skeleton;
                if (skeleton == null) continue;

                Vector3 campos = camera.Position - (entity?.Position ?? Vector3.Zero);

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

                    if (xforms != null)//how to use xforms? bind pose?
                    {
                        var xform = (i < xforms.Length) ? xforms[i] : Matrix.Identity;
                        var pxform = ((pind >= 0) && (pind < xforms.Length)) ? xforms[pind] : Matrix.Identity;
                    }
                    else
                    {
                    }


                    //draw line from bone's position to parent position...
                    Vector3 lbeg = Vector3.Zero;
                    Vector3 lend = bone.AnimTranslation;// bone.Rotation.Multiply();

                    float starsize = (bone.AnimTransform.TranslationVector-campos).Length() * 0.011f;
                    Vector3[] starverts0 = { Vector3.UnitX * starsize, Vector3.UnitY * starsize, Vector3.UnitZ * starsize };
                    Vector3[] starverts1 = { Vector3.UnitX * -starsize, Vector3.UnitY * -starsize, Vector3.UnitZ * -starsize };
                    for (int j = 0; j < 3; j++) starverts0[j] = bone.AnimTransform.MultiplyW(starverts0[j]);
                    for (int j = 0; j < 3; j++) starverts1[j] = bone.AnimTransform.MultiplyW(starverts1[j]);

                    if (pbone != null)
                    {
                        lbeg = pbone.AnimTransform.MultiplyW(lbeg);
                        lend = pbone.AnimTransform.MultiplyW(lend);
                    }

                    if (entity != null)
                    {
                        lbeg = entity.Position + entity.Orientation.Multiply(lbeg * entity.Scale);
                        lend = entity.Position + entity.Orientation.Multiply(lend * entity.Scale);

                        for (int j = 0; j < 3; j++) starverts0[j] = entity.Position + entity.Orientation.Multiply(starverts0[j] * entity.Scale);
                        for (int j = 0; j < 3; j++) starverts1[j] = entity.Position + entity.Orientation.Multiply(starverts1[j] * entity.Scale);
                    }

                    vr.Position = starverts0[0]; skeletonLineVerts.Add(vr);
                    vr.Position = starverts1[0]; skeletonLineVerts.Add(vr);
                    vg.Position = starverts0[1]; skeletonLineVerts.Add(vg);
                    vg.Position = starverts1[1]; skeletonLineVerts.Add(vg);
                    vb.Position = starverts0[2]; skeletonLineVerts.Add(vb);
                    vb.Position = starverts1[2]; skeletonLineVerts.Add(vb);


                    if (pbone != null) //don't draw the origin to root bone line
                    {
                        vg.Position = lbeg;
                        vb.Position = lend;
                        skeletonLineVerts.Add(vg);
                        skeletonLineVerts.Add(vb);
                    }

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
                rinst.CastShadow = false;
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
                    rinst.CastShadow = false;
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

        public void RenderBasePath(BasePathData basepath)
        {
            RenderablePathBatch rnd = renderableCache.GetRenderablePathBatch(basepath);
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
            ArchetypeRenderables.Clear();
            RequiredParents.Clear();
            RenderEntities.Clear();

            foreach (var ymap in renderworldVisibleYmapDict.Values)
            {
                if (!RenderWorldYmapIsVisible(ymap)) continue;
                VisibleYmaps.Add(ymap);
            }
            RenderWorldAdjustMapViewCamera();


            LodManager.MaxLOD = renderworldMaxLOD;
            LodManager.LodDistMult = renderworldDetailDistMult;
            LodManager.MapViewEnabled = MapViewEnabled;
            LodManager.MapViewDist = camera.OrthographicSize / MapViewDetail;
            LodManager.ShowScriptedYmaps = ShowScriptedYmaps;
            LodManager.Update(renderworldVisibleYmapDict, camera, currentElapsedTime);



            var ents = LodManager.GetVisibleLeaves();

            for (int i = 0; i < ents.Count; i++)
            {
                var ent = ents[i];

                if (!RenderIsEntityFinalRender(ent))
                { continue; }

                if (ent.IsMlo)
                {
                    if (renderinteriors && (ent.MloInstance != null) && !MapViewEnabled) //render Mlo child entities...
                    {
                        renderworldentities.Add(ent);//collisions rendering needs this
                        RenderWorldAddInteriorEntities(ent);
                    }
                }
                else
                {
                    var rndbl = GetArchetypeRenderable(ent.Archetype);
                    ent.LodManagerRenderable = rndbl;
                    if (rndbl != null)
                    {
                        RenderEntities.Add(ent);
                    }

                    var pent = ent.Parent;
                    if (waitforchildrentoload && (pent != null))
                    {
                        if (!RequiredParents.ContainsKey(pent))
                        {
                            bool allok = true;
                            var pcnode = pent.LodManagerChildren?.First;
                            while (pcnode != null)
                            {
                                var pcent = pcnode.Value;
                                var pcrndbl = (pcent == ent) ? rndbl : GetArchetypeRenderable(pcent.Archetype);
                                pcent.LodManagerRenderable = pcrndbl;
                                pcnode = pcnode.Next;
                                allok = allok && (pcrndbl != null);
                            }
                            if (!allok)
                            {
                                rndbl = GetArchetypeRenderable(pent.Archetype);
                                pent.LodManagerRenderable = rndbl;
                                if (rndbl != null)
                                {
                                    RenderEntities.Add(pent);
                                }
                            }
                            RequiredParents[pent] = rndbl;
                        }
                    }


                }
            }





            if (spaceEnts != null)
            {
                foreach (var ae in spaceEnts) //used by active space entities (eg "bullets")
                {
                    if (ae.EntityDef == null) continue; //nothing to render...
                    ae.EntityDef.Distance = (ae.EntityDef.Position - camera.Position).Length();
                    renderworldentities.Add(ae.EntityDef);
                }
            }



            if (renderentities)
            {
                for (int i = 0; i < renderworldentities.Count; i++)
                {
                    var ent = renderworldentities[i];
                    var rndbl = GetArchetypeRenderable(ent.Archetype);
                    ent.LodManagerRenderable = rndbl;
                    if (rndbl != null)
                    {
                        RenderEntities.Add(ent);
                    }
                }

                for (int i = 0; i < RenderEntities.Count; i++)
                {
                    var ent = RenderEntities[i];

                    var rndbl = ent.LodManagerRenderable as Renderable;

                    if (rndbl != null)
                    {
                        var rent = new RenderableEntity();
                        rent.Entity = ent;
                        rent.Renderable = rndbl;

                        if (HideEntities.ContainsKey(ent.EntityHash)) continue; //don't render hidden entities!

                        RenderArchetype(ent.Archetype, ent, rent.Renderable, false);
                    }
                }
            }



            for (int i = 0; i < ents.Count; i++) //make sure to remove the renderable references to avoid hogging memory
            {
                var ent = ents[i];
                ent.LodManagerRenderable = null;
            }
            foreach (var ent in RequiredParents.Keys)
            {
                var pcnode = ent.LodManagerChildren?.First;
                while (pcnode != null)//maybe can improve performance of this
                {
                    var pcent = pcnode.Value;
                    pcent.LodManagerRenderable = null;
                    pcnode = pcnode.Next;
                }
            }


            RenderWorldYmapExtras();
        }

        public void RenderWorld_Orig(Dictionary<MetaHash, YmapFile> renderworldVisibleYmapDict, IEnumerable<Entity> spaceEnts)
        {
            renderworldentities.Clear();
            renderworldrenderables.Clear();
            VisibleYmaps.Clear();


            foreach (var ymap in renderworldVisibleYmapDict.Values)
            {
                if (!RenderWorldYmapIsVisible(ymap)) continue;
                VisibleYmaps.Add(ymap);
            }

            RenderWorldAdjustMapViewCamera();



            for (int y = 0; y < VisibleYmaps.Count; y++)
            {
                var ymap = VisibleYmaps[y];
                YmapFile pymap = ymap.Parent;
                if ((pymap == null) && (ymap._CMapData.parent != 0))
                {
                    renderworldVisibleYmapDict.TryGetValue(ymap._CMapData.parent, out pymap);
                    ymap.Parent = pymap;
                }
                if (ymap.RootEntities != null)
                {
                    for (int i = 0; i < ymap.RootEntities.Length; i++)
                    {
                        var ent = ymap.RootEntities[i];
                        int pind = ent._CEntityDef.parentIndex;
                        if (pind >= 0) //connect root entities to parents if they have them..
                        {
                            YmapEntityDef p = null;
                            if ((pymap != null) && (pymap.AllEntities != null))
                            {
                                if ((pind < pymap.AllEntities.Length))
                                {
                                    p = pymap.AllEntities[pind];
                                    ent.Parent = p;
                                    ent.ParentName = p._CEntityDef.archetypeName;
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




            if (spaceEnts != null)
            {
                foreach (var ae in spaceEnts) //used by active space entities (eg "bullets")
                {
                    if (ae.EntityDef == null) continue; //nothing to render...
                    RenderWorldCalcEntityVisibility(ae.EntityDef);
                    renderworldentities.Add(ae.EntityDef);
                }
            }


            if (renderentities)
            {
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
                }
                for (int i = 0; i < renderworldrenderables.Count; i++)
                {
                    var rent = renderworldrenderables[i];
                    var ent = rent.Entity;
                    var arch = ent.Archetype;

                    if (HideEntities.ContainsKey(ent.EntityHash)) continue; //don't render hidden entities!

                    RenderArchetype(arch, ent, rent.Renderable, false);
                }
            }

            RenderWorldYmapExtras();
        }
        private void RenderWorldCalcEntityVisibility(YmapEntityDef ent)
        {
            float dist = (ent.Position - camera.Position).Length();

            if (MapViewEnabled)
            {
                dist = camera.OrthographicSize / MapViewDetail;
            }


            var loddist = ent._CEntityDef.lodDist;
            var cloddist = ent._CEntityDef.childLodDist;

            if (loddist <= 0.0f)//usually -1 or -2
            {
                if (ent.Archetype != null)
                {
                    loddist = ent.Archetype.LodDist * renderworldLodDistMult;
                }
            }
            else if (ent._CEntityDef.lodLevel == rage__eLodType.LODTYPES_DEPTH_ORPHANHD)
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


            ent.Distance = dist;
            ent.IsVisible = (dist <= loddist);
            ent.ChildrenVisible = (dist <= cloddist) && (ent._CEntityDef.numChildren > 0);



            if (renderworldMaxLOD != rage__eLodType.LODTYPES_DEPTH_ORPHANHD)
            {
                if ((ent._CEntityDef.lodLevel == rage__eLodType.LODTYPES_DEPTH_ORPHANHD) ||
                    (ent._CEntityDef.lodLevel < renderworldMaxLOD))
                {
                    ent.IsVisible = false;
                    ent.ChildrenVisible = false;
                }
                if (ent._CEntityDef.lodLevel == renderworldMaxLOD)
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


                    if (!camera.ViewFrustum.ContainsAABBNoClip(ref ent.BBCenter, ref ent.BBExtent))
                    {
                        return;
                    }


                    renderworldentities.Add(ent);


                    if (renderinteriors && ent.IsMlo && (ent.MloInstance != null)) //render Mlo child entities...
                    {
                        RenderWorldAddInteriorEntities(ent);
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

        private bool RenderWorldYmapIsVisible(YmapFile ymap)
        {
            if (!ShowScriptedYmaps)
            {
                if ((ymap._CMapData.flags & 1) > 0)
                    return false;
            }

            if (!ymap.HasChanged)//don't cull edited project ymaps, because extents may not have been updated!
            {
                var eemin = ymap._CMapData.entitiesExtentsMin;
                var eemax = ymap._CMapData.entitiesExtentsMax;
                bool visible = false;
                if (MapViewEnabled)//don't do front clipping in 2D mode
                {
                    visible = camera.ViewFrustum.ContainsAABBNoFrontClipNoOpt(ref eemin, ref eemax);
                }
                else
                {
                    visible = camera.ViewFrustum.ContainsAABBNoClipNoOpt(ref eemin, ref eemax);
                }
                if (!visible)
                {
                    return false;
                }
            }

            return true;
        }
        private void RenderWorldAddInteriorEntities(YmapEntityDef ent)
        {
            if (ent.MloInstance.Entities != null)
            {
                for (int j = 0; j < ent.MloInstance.Entities.Length; j++)
                {
                    var intent = ent.MloInstance.Entities[j];
                    if (intent.Archetype == null) continue; //missing archetype...
                    if (!RenderIsEntityFinalRender(intent)) continue; //proxy or something..

                    intent.IsVisible = true;

                    if (!camera.ViewFrustum.ContainsAABBNoClip(ref intent.BBCenter, ref intent.BBExtent))
                    {
                        continue; //frustum cull interior ents
                    }

                    renderworldentities.Add(intent);
                }
            }
            if (ent.MloInstance.EntitySets != null)
            {
                for (int e = 0; e < ent.MloInstance.EntitySets.Length; e++)
                {
                    var entityset = ent.MloInstance.EntitySets[e];
                    if (!entityset.VisibleOrForced) continue;

                    var entities = entityset.Entities;
                    if (entities == null) continue;
                    for (int i = 0; i < entities.Count; i++)
                    {
                        var intent = entities[i];
                        if (intent.Archetype == null) continue; //missing archetype...
                        if (!RenderIsEntityFinalRender(intent)) continue; //proxy or something..

                        intent.IsVisible = true;

                        if (!camera.ViewFrustum.ContainsAABBNoClip(ref intent.BBCenter, ref intent.BBExtent))
                        {
                            continue; //frustum cull interior ents
                        }

                        renderworldentities.Add(intent);

                    }
                }
            }
        }
        private void RenderWorldAdjustMapViewCamera()
        {
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
                        for (int i = 0; i < ymap.AllEntities.Length; i++)//this is bad
                        {
                            var ent = ymap.AllEntities[i];
                            if ((ent.Position.Z < 1000.0f) && (ent.BSRadius < 500.0f))
                            {
                                if ((ent.BBMax.X > cvwmin) && (ent.BBMin.X < cvwmax) && (ent.BBMax.Y > cvhmin) && (ent.BBMin.Y < cvhmax))
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
                camera.ViewFrustum.Position = camera.Position;
            }
        }
        private void RenderWorldYmapExtras()
        {
            if (renderinteriors && (rendercollisionmeshes || (SelectionMode == MapSelectionMode.Collision)))
            {
                for (int i = 0; i < renderworldentities.Count; i++)
                {
                    var ent = renderworldentities[i];
                    if (ent.IsMlo)
                    {
                        RenderInteriorCollisionMesh(ent);
                    }
                }
            }
            if (rendercars)
            {
                for (int y = 0; y < VisibleYmaps.Count; y++)
                {
                    var ymap = VisibleYmaps[y];
                    if (ymap.CarGenerators != null)
                    {
                        RenderYmapCarGenerators(ymap);
                    }
                }
            }
            if (rendergrass && (renderworldMaxLOD == rage__eLodType.LODTYPES_DEPTH_ORPHANHD)) //hide grass with orphans
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
            if (renderlodlights && shaders.deferred)
            {
                for (int y = 0; y < VisibleYmaps.Count; y++)
                {
                    var ymap = VisibleYmaps[y];
                    if (ymap.LODLights != null)
                    {
                        RenderYmapLODLights(ymap);
                    }
                }
            }
        }




        private bool RenderIsEntityFinalRender(YmapEntityDef ent)
        {
            var arch = ent.Archetype;
            if (arch == null) return false;

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

            if ((model.RenderMaskFlags & 1) == 0) //smallest bit is proxy/"final render" bit? seems to work...
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




        private Renderable GetArchetypeRenderable(Archetype arch)
        {
            if (arch == null) return null;

            Renderable rndbl = null;
            if (!ArchetypeRenderables.TryGetValue(arch, out rndbl))
            {
                var drawable = gameFileCache.TryGetDrawable(arch);
                rndbl = TryGetRenderable(arch, drawable);
                ArchetypeRenderables[arch] = rndbl;
            }
            if ((rndbl != null) && rndbl.IsLoaded && (rndbl.AllTexturesLoaded || !waitforchildrentoload))
            {
                return rndbl;
            }
            return null;
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

            if (rendercars && ymap.CarGenerators != null)
            {
                RenderYmapCarGenerators(ymap);
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
                    entity.Distance = dist;
                    float rad = arch.BSRadius;
                    float loddist = entity._CEntityDef.lodDist;
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
                            if (rendercollisionmeshes || (SelectionMode == MapSelectionMode.Collision))
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

                instb.CamRel = instb.Position - camera.Position;//to gracefully handle batch size changes

                RenderableInstanceBatchInst binst = new RenderableInstanceBatchInst();
                binst.Batch = instb;
                binst.Renderable = rndbl;

                shaders.Enqueue(ref binst);

            }

        }



        private void RenderYmapLODLights(YmapFile ymap)
        {
            if (ymap.LODLights == null) return;
            if (ymap.Parent?.DistantLODLights == null) return; //need to get lodlights positions from parent (distlodlights)

            RenderableLODLights lights = renderableCache.GetRenderableLODLights(ymap);
            if (!lights.IsLoaded) return;

            shaders.Enqueue(lights);

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
                graphicsytd.TextureDict.Dict.TryGetValue(texhash, out lighttex);
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

        private void RenderYmapCarGenerators(YmapFile ymap)
        {

            if (ymap.CarGenerators == null) return;

            var maxdist = 200 * renderworldDetailDistMult;
            var maxdist2 = maxdist * maxdist;

            for (int i = 0; i < ymap.CarGenerators.Length; i++)
            {
                var cg = ymap.CarGenerators[i];

                var bscent = cg.Position - camera.Position;
                float bsrad = cg._CCarGen.perpendicularLength;
                if (bscent.LengthSquared() > maxdist2) continue; //don't render distant cars..
                if (!camera.ViewFrustum.ContainsSphereNoClipNoOpt(ref bscent, bsrad))
                {
                    continue; //frustum cull cars...
                }


                Quaternion cgtrn = Quaternion.RotationAxis(Vector3.UnitZ, (float)Math.PI * -0.5f); //car fragments currently need to be rotated 90 deg right...
                Quaternion cgori = Quaternion.Multiply(cg.Orientation, cgtrn);

                RenderCar(cg.Position, cgori, cg._CCarGen.carModel, cg._CCarGen.popGroup);
            }

        }





        public bool RenderFragment(Archetype arch, YmapEntityDef ent, FragType f, uint txdhash = 0, ClipMapEntry animClip = null)
        {

            RenderDrawable(f.Drawable, arch, ent, txdhash, null, null, animClip);

            if (f.Drawable2 != null) //cloth
            {
                RenderDrawable(f.Drawable2, arch, ent, txdhash, null, null, animClip);
            }

            //vehicle wheels...
            if ((f.PhysicsLODGroup != null) && (f.PhysicsLODGroup.PhysicsLOD1 != null))
            {
                var pl1 = f.PhysicsLODGroup.PhysicsLOD1;
                //var groupnames = pl1?.GroupNames?.data_items;
                var groups = pl1?.Groups?.data_items;

                FragDrawable wheel_f = null;
                FragDrawable wheel_r = null;

                if (pl1.Children?.data_items != null)
                {
                    for (int i = 0; i < pl1.Children.data_items.Length; i++)
                    {
                        var pch = pl1.Children.data_items[i];

                        //var groupname = pch.GroupNameHash;
                        //if ((pl1.Groups?.data_items != null) && (i < pl1.Groups.data_items.Length))
                        //{
                        //    //var group = pl1.Groups.data_items[i];
                        //}

                        if ((pch.Drawable1 != null) && (pch.Drawable1.AllModels.Length != 0))
                        {

                            switch (pch.BoneTag)
                            {
                                case 27922: //wheel_lf
                                case 26418: //wheel_rf
                                    wheel_f = pch.Drawable1;
                                    break;
                                case 29921: //wheel_lm1
                                case 29922: //wheel_lm2
                                case 29923: //wheel_lm3
                                case 27902: //wheel_lr
                                case 5857:  //wheel_rm1
                                case 5858:  //wheel_rm2
                                case 5859:  //wheel_rm3
                                case 26398: //wheel_rr
                                    wheel_r = pch.Drawable1;
                                    break;
                                default:

                                    RenderDrawable(pch.Drawable1, arch, ent, txdhash, null, null, animClip);

                                    break;
                            }

                        }
                        else
                        { }
                        if ((pch.Drawable2 != null) && (pch.Drawable2.AllModels.Length != 0))
                        {
                            RenderDrawable(pch.Drawable2, arch, ent, txdhash, null, null, animClip);
                        }
                        else
                        { }
                    }

                    if ((wheel_f != null) || (wheel_r != null))
                    {
                        for (int i = 0; i < pl1.Children.data_items.Length; i++)
                        {
                            var pch = pl1.Children.data_items[i];
                            FragDrawable dwbl = pch.Drawable1;
                            FragDrawable dwblcopy = null;
                            switch (pch.BoneTag)
                            {
                                case 27922: //wheel_lf
                                case 26418: //wheel_rf
                                    dwblcopy = wheel_f != null ? wheel_f : wheel_r;
                                    break;
                                case 29921: //wheel_lm1
                                case 29922: //wheel_lm2
                                case 29923: //wheel_lm3
                                case 27902: //wheel_lr
                                case 5857:  //wheel_rm1
                                case 5858:  //wheel_rm2
                                case 5859:  //wheel_rm3
                                case 26398: //wheel_rr
                                    dwblcopy = wheel_r != null ? wheel_r : wheel_f;
                                    break;
                                default:
                                    break;
                            }
                            //switch (pch.GroupNameHash)
                            //{
                            //    case 3311608449: //wheel_lf
                            //    case 1705452237: //wheel_lm1
                            //    case 1415282742: //wheel_lm2
                            //    case 3392433122: //wheel_lm3
                            //    case 133671269:  //wheel_rf
                            //    case 2908525601: //wheel_rm1
                            //    case 2835549038: //wheel_rm2
                            //    case 4148013026: //wheel_rm3
                            //        dwblcopy = wheel_f != null ? wheel_f : wheel_r;
                            //        break;
                            //    case 1695736278: //wheel_lr
                            //    case 1670111368: //wheel_rr
                            //        dwblcopy = wheel_r != null ? wheel_r : wheel_f;
                            //        break;
                            //    default:
                            //        break;
                            //}

                            if (dwblcopy != null)
                            {
                                if (dwbl != null)
                                {
                                    if ((dwbl != dwblcopy) && (dwbl.AllModels.Length == 0))
                                    {
                                        dwbl.Owner = dwblcopy;
                                        dwbl.AllModels = dwblcopy.AllModels; //hopefully this is all that's need to render, otherwise drawable is actually getting edited!
                                        //dwbl.DrawableModelsHigh = dwblcopy.DrawableModelsHigh;
                                        //dwbl.DrawableModelsMedium = dwblcopy.DrawableModelsMedium;
                                        //dwbl.DrawableModelsLow = dwblcopy.DrawableModelsLow;
                                        //dwbl.DrawableModelsVeryLow = dwblcopy.DrawableModelsVeryLow;
                                        //dwbl.VertexDecls = dwblcopy.VertexDecls;
                                    }

                                    RenderDrawable(dwbl, arch, ent, txdhash /*, null, null, animClip*/);

                                }
                                else
                                { }
                            }
                            else
                            { }
                        }
                    }

                }
            }

            return true;
        }

        public bool RenderArchetype(Archetype arche, YmapEntityDef entity, Renderable rndbl = null, bool cull = true, ClipMapEntry animClip = null)
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
                if (animClip != null)
                {
                    rndbl.ClipMapEntry = animClip;
                    rndbl.ClipDict = animClip.Clip?.Ycd;
                    rndbl.HasAnims = true;
                }


                res = RenderRenderable(rndbl, arche, entity);


                //fragments have extra drawables! need to render those too... TODO: handle fragments properly...
                FragDrawable fd = rndbl.Key as FragDrawable;
                if (fd != null)
                {
                    var frag = fd.OwnerFragment;
                    if ((frag != null) && (frag.Drawable2 != null)) //cloth...
                    {
                        rndbl = TryGetRenderable(arche, frag.Drawable2);
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

        public bool RenderDrawable(DrawableBase drawable, Archetype arche, YmapEntityDef entity, uint txdHash = 0, TextureDictionary txdExtra = null, Texture diffOverride = null, ClipMapEntry animClip = null, ClothInstance cloth = null)
        {
            //enqueue a single drawable for rendering.

            if (drawable == null)
                return false;

            Renderable rndbl = TryGetRenderable(arche, drawable, txdHash, txdExtra, diffOverride);
            if (rndbl == null)
                return false;

            if (animClip != null)
            {
                rndbl.ClipMapEntry = animClip;
                rndbl.ClipDict = animClip.Clip?.Ycd;
                rndbl.HasAnims = true;
            }
            else if ((arche == null) && (rndbl.ClipMapEntry != null))
            {
                rndbl.ClipMapEntry = null;
                rndbl.ClipDict = null;
                rndbl.HasAnims = false;
                rndbl.ResetBoneTransforms();
            }

            rndbl.Cloth = cloth;


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
            float distance = 0;// (camrel + bscen).Length();
            bool interiorent = false;
            bool castshadow = true;

            if (entity != null)
            {
                position = entity.Position;
                scale = entity.Scale;
                orientation = entity.Orientation;
                tintPaletteIndex = entity._CEntityDef.tintValue;
                bbmin = entity.BBMin;
                bbmax = entity.BBMax;
                bscen = entity.BSCenter;
                camrel += position;
                distance = entity.Distance;
                castshadow = (entity.MloParent == null);//don't cast sun/moon shadows if this is an interior entity - optimisation!
                interiorent = (entity.MloParent != null);
            }
            else
            {
                distance = (camrel + bscen).Length();
            }


            //bool usehdtxd = renderhdtextures && ((dist - bsrad) <= arche._BaseArchetypeDef.hdTextureDist);
            //var usehdtxd = false;
            //if ((arch != null) && (renderhdtextures))
            //{
            //    usehdtxd = ((ent.Distance - arch.BSRadius) <= arch._BaseArchetypeDef.hdTextureDist);
            //}



            if (rndbl.HasAnims)
            {
                rndbl.UpdateAnims(currentRealTime);
            }
            if (rndbl.Cloth != null)
            {
                rndbl.Cloth.Update(currentRealTime);
            }



            if ((rendercollisionmeshes || (SelectionMode == MapSelectionMode.Collision)) && rendercollisionmeshlayerdrawable)
            {
                if ((entity == null) || ((entity._CEntityDef.flags & 4) == 0)) //skip if entity embedded collisions disabled
                {
                    Drawable sdrawable = rndbl.Key as Drawable;
                    if ((sdrawable != null) && (sdrawable.Bound != null))
                    {
                        RenderCollisionMesh(sdrawable.Bound, entity);
                    }
                    FragDrawable fdrawable = rndbl.Key as FragDrawable;
                    if (fdrawable != null)
                    {
                        if (fdrawable.Bound != null)
                        {
                            RenderCollisionMesh(fdrawable.Bound, entity);
                        }
                        var fbound = fdrawable.OwnerFragment?.PhysicsLODGroup?.PhysicsLOD1?.Bound;
                        if (fbound != null)
                        {
                            RenderCollisionMesh(fbound, entity);//TODO: these probably have extra transforms..!
                        }
                    }
                }
            }
            if (renderskeletons && rndbl.HasSkeleton)
            {
                RenderSkeleton(rndbl, entity);
            }


            if (renderlights && shaders.deferred && (rndbl.Lights != null) && interiorent)//only interior ents making lights! todo: fix LOD lights
            {
                var linst = new RenderableLightInst();
                for (int i = 0; i < rndbl.Lights.Length; i++)
                {
                    linst.EntityPosition = position;
                    linst.EntityRotation = orientation;
                    linst.Light = rndbl.Lights[i];
                    shaders.Enqueue(ref linst);
                }
            }


            bool retval = true;// false;
            if ((rndbl.AllTexturesLoaded || !waitforchildrentoload))
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
                rginst.Inst.CastShadow = castshadow;


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
                        var dgeom = geom.DrawableGeom;

                        if (dgeom.UpdateRenderableParameters) //when edited by material editor
                        {
                            geom.Init(dgeom);
                            dgeom.UpdateRenderableParameters = false;
                        }

                        if (isselected)
                        {
                            if (geom.disableRendering || SelectionGeometryDrawFlags.ContainsKey(dgeom))
                            { continue; } //filter out geometries in selected item that aren't flagged for drawing.
                        }
                        else
                        {
                            if (geom.disableRendering)
                            { continue; } //filter out certain geometries like certain hair parts that shouldn't render by default
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
                    float minz = caryft.Fragment.PhysicsLODGroup?.PhysicsLOD1?.Bound?.BoxMin.Z ?? 0.0f;
                    pos.Z -= minz;
                }

                SelectedCarGenEntity.SetPosition(pos);
                SelectedCarGenEntity.SetOrientation(ori);

                RenderFragment(null, SelectedCarGenEntity, caryft.Fragment, carhash);
            }
        }

        public void RenderVehicle(Vehicle vehicle, ClipMapEntry animClip = null)
        {

            YftFile yft = vehicle.Yft;
            if ((yft != null) && (yft.Loaded) && (yft.Fragment != null))
            {
                var f = yft.Fragment;
                var txdhash = vehicle.NameHash;

                RenderFragment(null, vehicle.RenderEntity, f, txdhash, animClip);

            }

        }

        public void RenderWeapon(Weapon weapon, ClipMapEntry animClip = null)
        {
            if (weapon?.Drawable != null)
            {
                var d = weapon.Drawable;
                var txdhash = weapon.NameHash;
                RenderDrawable(d, null, weapon.RenderEntity, txdhash, null, null, animClip);
            }
        }



        public void RenderPed(Ped ped)
        {

            YftFile yft = ped.Yft;// GameFileCache.GetYft(SelectedModelHash);
            if (yft != null)
            {
                if (yft.Loaded)
                {
                    if (yft.Fragment != null)
                    {
                        //var f = yft.Fragment;
                        //var txdhash = 0u;// SelectedVehicleHash;// yft.RpfFileEntry?.ShortNameHash ?? 0;
                        //var namelower = yft.RpfFileEntry?.GetShortNameLower();
                        //Archetype arch = null;// TryGetArchetype(hash);
                        //Renderer.RenderFragment(arch, null, f, txdhash);
                        //seldrwbl = f.Drawable;
                    }
                }


                var vi = ped.Ymt?.VariationInfo;
                if (vi != null)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        RenderPedComponent(ped, i);
                    }
                }

            }

        }

        private void RenderPedComponent(Ped ped, int i)
        {
            //var compData = ped.Ymt?.VariationInfo?.GetComponentData(i);
            var drawable = ped.Drawables[i];
            var texture = ped.Textures[i];
            var cloth = ped.Clothes[i];

            //if (compData == null) return;
            if (drawable == null) return;

            var td = ped.Ytd?.TextureDict;
            var ac = ped.AnimClip;
            if (ac != null)
            {
                ac.EnableRootMotion = ped.EnableRootMotion;
            }

            var skel = ped.Skeleton;
            if (skel != null)
            {
                if (drawable.Skeleton == null)
                {
                    drawable.Skeleton = skel;//force the drawable to use this skeleton.
                }
                else if (drawable.Skeleton != skel)
                {
                    var dskel = drawable.Skeleton; //put the bones of the fragment into the drawable. drawable's bones in this case seem messed up!
                    for (int b = 0; b < skel.Bones.Count; b++)
                    {
                        var srcbone = skel.Bones[b];
                        var dstbone = srcbone;
                        if (dskel.BonesMap.TryGetValue(srcbone.Tag, out dstbone))
                        {
                            if (srcbone == dstbone) break; //bone reassignment already done!
                            dskel.Bones[dstbone.Index] = srcbone;
                            dskel.BonesMap[srcbone.Tag] = srcbone;
                        }
                    }
                }
            }


            bool drawFlag = true;
            if (!SelectionDrawableDrawFlags.TryGetValue(drawable, out drawFlag))
            { drawFlag = true; }

            if (drawFlag)
            {
                RenderDrawable(drawable, null, ped.RenderEntity, 0, td, texture, ac, cloth);
            }


        }


        public void RenderHideEntity(YmapEntityDef ent)
        {
            var hash = ent?.EntityHash ?? 0;
            if (hash == 0) return;

            HideEntities[hash] = ent;
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

            RenderableBoundComposite rndbc = renderableCache.GetRenderableBoundComp(bounds);
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
                    rbginst.Inst.Position = position + orientation.Multiply(geom.CenterGeom * scale);
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







        private Renderable TryGetRenderable(Archetype arche, DrawableBase drawable, uint txdHash = 0, TextureDictionary txdExtra = null, Texture diffOverride = null)
        {
            if (drawable == null) return null;
            //BUG: only last texdict used!! needs to cache textures per archetype........
            //(but is it possible to have the same drawable with different archetypes?)
            MetaHash texDict = txdHash;
            //uint texDictOrig = txdHash;
            uint clipDict = 0;

            if (arche != null)
            {
                texDict = arche.TextureDict.Hash;
                clipDict = arche.ClipDict.Hash;

                //texDictOrig = texDict;
                //if (hdtxd)
                //{
                //    texDict = gameFileCache.TryGetHDTextureHash(texDict);
                //}
            }


            Renderable rndbl = renderableCache.GetRenderable(drawable);
            if (rndbl == null) return null;

            if ((clipDict != 0) && (rndbl.ClipDict == null))
            {
                YcdFile ycd = gameFileCache.GetYcd(clipDict);
                if ((ycd != null) && (ycd.Loaded))
                {
                    rndbl.ClipDict = ycd;
                    MetaHash ahash = arche.Hash;
                    if (ycd.ClipMap.TryGetValue(ahash, out rndbl.ClipMapEntry)) rndbl.HasAnims = true;

                    foreach (var model in rndbl.HDModels)
                    {
                        if (model == null) continue;
                        foreach (var geom in model.Geometries)
                        {
                            if (geom == null) continue;
                            if (geom.globalAnimUVEnable)
                            {
                                uint cmeindex = geom.DrawableGeom.ShaderID + 1u;
                                MetaHash cmehash = ahash + cmeindex; //this goes to at least uv5! (from uv0) - see hw1_09.ycd
                                if (ycd.ClipMap.TryGetValue(cmehash, out geom.ClipMapEntryUV)) rndbl.HasAnims = true;
                            }
                        }
                    }
                }
            }


            var extraTexDict = (drawable.Owner as YptFile)?.PtfxList?.TextureDictionary;
            if (extraTexDict == null) extraTexDict = txdExtra;

            bool cacheSD = (rndbl.SDtxds == null);
            bool cacheHD = (renderhdtextures && (rndbl.HDtxds == null));
            if (cacheSD || cacheHD)
            {
                //cache the txd hierarchies for this renderable
                tryGetRenderableSDtxds.Clear();
                tryGetRenderableHDtxds.Clear();
                if (cacheHD && (arche != null)) //try get HD txd for the asset
                {
                    MetaHash hdtxd = gameFileCache.TryGetHDTextureHash(arche._BaseArchetypeDef.assetName);
                    if (hdtxd != arche._BaseArchetypeDef.assetName)
                    {
                        var asshdytd = gameFileCache.GetYtd(hdtxd);
                        if (asshdytd != null)
                        {
                            tryGetRenderableHDtxds.Add(asshdytd);
                        }
                    }
                }
                if (texDict != 0)
                {
                    if (cacheSD)
                    {
                        var txdytd = gameFileCache.GetYtd(texDict);
                        if (txdytd != null)
                        {
                            tryGetRenderableSDtxds.Add(txdytd);
                        }
                    }
                    if (cacheHD)
                    {
                        MetaHash hdtxd = gameFileCache.TryGetHDTextureHash(texDict);
                        if (hdtxd != texDict)
                        {
                            var txdhdytd = gameFileCache.GetYtd(hdtxd);
                            if (txdhdytd != null)
                            {
                                tryGetRenderableHDtxds.Add(txdhdytd);
                            }
                        }
                    }
                    MetaHash ptxdname = gameFileCache.TryGetParentYtdHash(texDict);
                    while (ptxdname != 0) //look for parent HD txds
                    {
                        if (cacheSD)
                        {
                            var pytd = gameFileCache.GetYtd(ptxdname);
                            if (pytd != null)
                            {
                                tryGetRenderableSDtxds.Add(pytd);
                            }
                        }
                        if (cacheHD)
                        {
                            MetaHash phdtxdname = gameFileCache.TryGetHDTextureHash(ptxdname);
                            if (phdtxdname != ptxdname)
                            {
                                var phdytd = gameFileCache.GetYtd(phdtxdname);
                                if (phdytd != null)
                                {
                                    tryGetRenderableHDtxds.Add(phdytd);
                                }
                            }
                        }
                        ptxdname = gameFileCache.TryGetParentYtdHash(ptxdname);
                    }
                }
                if (cacheSD) rndbl.SDtxds = tryGetRenderableSDtxds.ToArray();
                if (cacheHD) rndbl.HDtxds = tryGetRenderableHDtxds.ToArray();
            }





            bool alltexsloaded = true;


            for (int mi = 0; mi < rndbl.AllModels.Length; mi++)
            {
                var model = rndbl.AllModels[mi];

                if (!RenderIsModelFinalRender(model) && !renderproxies)
                {
                    continue; //filter out reflection proxy models...
                }


                foreach (var geom in model.Geometries)
                {
                    if (geom.Textures != null)
                    {
                        for (int i = 0; i < geom.Textures.Length; i++)
                        {
                            if (diffOverride != null)
                            {
                                var texParamHash = (i < geom.TextureParamHashes?.Length) ? geom.TextureParamHashes[i] : 0;
                                if (texParamHash == ShaderParamNames.DiffuseSampler)
                                {
                                    geom.Textures[i] = diffOverride;
                                }
                            }

                            var tex = geom.Textures[i];
                            var ttex = tex as Texture;
                            Texture dtex = null;
                            RenderableTexture rdtex = null;
                            if ((tex != null) && (ttex == null))
                            {
                                //TextureRef means this RenderableTexture needs to be loaded from texture dict...
                                if (extraTexDict != null) //for ypt files, first try the embedded tex dict..
                                {
                                    dtex = extraTexDict.Lookup(tex.NameHash);
                                }

                                if (dtex == null) //else //if (texDict != 0)
                                {
                                    bool waitingforload = false;
                                    if (rndbl.SDtxds != null)
                                    {
                                        //check the SD texture hierarchy
                                        for (int j = 0; j < rndbl.SDtxds.Length; j++)
                                        {
                                            var txd = rndbl.SDtxds[j];
                                            if (txd.Loaded)
                                            {
                                                dtex = txd.TextureDict?.Lookup(tex.NameHash);
                                            }
                                            else
                                            {
                                                txd = gameFileCache.GetYtd(txd.Key.Hash);//keep trying to load it - sometimes resuests can get lost (!)
                                                waitingforload = true;
                                            }
                                            if (dtex != null) break;
                                        }

                                        if (waitingforload)
                                        {
                                            alltexsloaded = false;
                                        }
                                    }

                                    if ((dtex == null) && (!waitingforload))
                                    {
                                        //not present in dictionary... check already loaded texture dicts... (maybe resident?)
                                        var ytd2 = gameFileCache.TryGetTextureDictForTexture(tex.NameHash);
                                        if (ytd2 != null)
                                        {
                                            if (ytd2.Loaded)
                                            {
                                                if (ytd2.TextureDict != null)
                                                {
                                                    dtex = ytd2.TextureDict.Lookup(tex.NameHash);
                                                }
                                            }
                                            else
                                            {
                                                alltexsloaded = false;
                                            }
                                        }

                                        //else { } //couldn't find texture dict?

                                        if ((dtex == null) && (ytd2 == null))// rndbl.SDtxds.Length == 0)//texture not found..
                                        {
                                            if (drawable.ShaderGroup.TextureDictionary != null)//check any embedded texdict
                                            {
                                                dtex = drawable.ShaderGroup.TextureDictionary.Lookup(tex.NameHash);
                                                if (dtex != null)
                                                { } //this shouldn't really happen as embedded textures should already be loaded! (not as TextureRef)
                                            }
                                        }
                                    }
                                }

                                if (dtex != null)
                                {
                                    geom.Textures[i] = dtex; //cache it for next time to avoid the lookup...
                                    ttex = dtex;
                                }
                            }


                            if (ttex != null) //ensure renderable texture
                            {
                                rdtex = renderableCache.GetRenderableTexture(ttex);
                            }

                            //if ((rdtex != null) && (rdtex.IsLoaded == false))
                            //{
                            //    alltexsloaded = false;
                            //}


                            geom.RenderableTextures[i] = rdtex;



                            RenderableTexture rhdtex = null;
                            if (renderhdtextures)
                            {
                                Texture hdtex = geom.TexturesHD[i];
                                if (hdtex == null)
                                {
                                    //look for a replacement HD texture...
                                    if (rndbl.HDtxds != null)
                                    {
                                        for (int j = 0; j < rndbl.HDtxds.Length; j++)
                                        {
                                            var txd = rndbl.HDtxds[j];
                                            if (txd.Loaded)
                                            {
                                                hdtex = txd.TextureDict?.Lookup(tex.NameHash);
                                            }
                                            else
                                            {
                                                txd = gameFileCache.GetYtd(txd.Key.Hash);//keep trying to load it - sometimes resuests can get lost (!)
                                            }
                                            if (hdtex != null) break;
                                        }
                                    }
                                    if (hdtex != null)
                                    {
                                        geom.TexturesHD[i] = hdtex;
                                    }
                                }
                                if (hdtex != null)
                                {
                                    rhdtex = renderableCache.GetRenderableTexture(hdtex);
                                }
                            }
                            geom.RenderableTexturesHD[i] = rhdtex;

                        }
                    }
                }
            }


            rndbl.AllTexturesLoaded = alltexsloaded;


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

























    public class RenderLodManager
    {
        public rage__eLodType MaxLOD = rage__eLodType.LODTYPES_DEPTH_ORPHANHD;
        public float LodDistMult = 1.0f;
        public bool MapViewEnabled = false;
        public float MapViewDist = 1.0f;
        public bool ShowScriptedYmaps = true;

        public Camera Camera = null;
        public Vector3 Position = Vector3.Zero;

        public Dictionary<MetaHash, YmapFile> CurrentYmaps = new Dictionary<MetaHash, YmapFile>();
        private List<MetaHash> RemoveYmaps = new List<MetaHash>();
        public Dictionary<YmapEntityDef, YmapEntityDef> RootEntities = new Dictionary<YmapEntityDef, YmapEntityDef>();
        public List<YmapEntityDef> VisibleLeaves = new List<YmapEntityDef>();

        public void Update(Dictionary<MetaHash, YmapFile> ymaps, Camera camera, float elapsed)
        {
            Camera = camera;
            Position = camera.Position;

            foreach (var kvp in ymaps)
            {
                var ymap = kvp.Value;
                var pymap = ymap.Parent;
                if (ymap._CMapData.parent != 0) //ensure parent references on ymaps
                {
                    ymaps.TryGetValue(ymap._CMapData.parent, out pymap);
                    if (pymap == null) //skip adding ymaps until parents are available
                    { continue; }
                    if (ymap.Parent != pymap)
                    {
                        ymap.Parent = pymap;
                        if (ymap.RootEntities != null) //parent changed or first set, make sure to link entities hierarchy
                        {
                            for (int i = 0; i < ymap.RootEntities.Length; i++)
                            {
                                var ent = ymap.RootEntities[i];
                                int pind = ent._CEntityDef.parentIndex;
                                if (pind >= 0) //connect root entities to parents if they have them..
                                {
                                    YmapEntityDef p = null;
                                    if ((pymap != null) && (pymap.AllEntities != null))
                                    {
                                        if ((pind < pymap.AllEntities.Length))
                                        {
                                            p = pymap.AllEntities[pind];
                                            ent.Parent = p;
                                            ent.ParentName = p._CEntityDef.archetypeName;
                                        }
                                    }
                                    else
                                    { }//should only happen if parent ymap not loaded yet...
                                }
                            }
                        }
                    }
                }
            }

            RemoveYmaps.Clear();
            foreach (var kvp in CurrentYmaps)
            {
                YmapFile ymap = null;
                if (!ymaps.TryGetValue(kvp.Key, out ymap) || (ymap != kvp.Value) || (ymap.IsScripted && !ShowScriptedYmaps) || (ymap.LodManagerUpdate))
                {
                    RemoveYmaps.Add(kvp.Key);
                }
            }
            foreach (var remYmap in RemoveYmaps)
            {
                var ymap = CurrentYmaps[remYmap];
                CurrentYmaps.Remove(remYmap);
                var remEnts = ymap.LodManagerOldEntities ?? ymap.AllEntities;
                if (remEnts != null)    // remove this ymap's entities from the tree.....
                {
                    for (int i = 0; i < remEnts.Length; i++)
                    {
                        var ent = remEnts[i];
                        RootEntities.Remove(ent);
                        ent.LodManagerChildren?.Clear();
                        ent.LodManagerChildren = null;
                        ent.LodManagerRenderable = null;
                        if ((ent.Parent != null) && (ent.Parent.Ymap != ymap))
                        {
                            ent.Parent.LodManagerRemoveChild(ent);
                        }
                    }
                }
                ymap.LodManagerUpdate = false;
                ymap.LodManagerOldEntities = null;
            }
            foreach (var kvp in ymaps)
            {
                var ymap = kvp.Value;
                if (ymap.IsScripted && !ShowScriptedYmaps)
                { continue; }
                if ((ymap._CMapData.parent != 0) && (ymap.Parent == null)) //skip adding ymaps until parents are available
                { continue; }
                if (!CurrentYmaps.ContainsKey(kvp.Key))
                {
                    CurrentYmaps.Add(kvp.Key, kvp.Value);
                    if (ymap.AllEntities != null)    // add this ymap's entities to the tree...
                    {
                        for (int i = 0; i < ymap.AllEntities.Length; i++)
                        {
                            var ent = ymap.AllEntities[i];
                            if (ent.Parent != null)
                            {
                                ent.Parent.LodManagerAddChild(ent);
                            }
                            else
                            {
                                RootEntities[ent] = ent;
                            }
                        }
                    }
                }
            }


        }

        public List<YmapEntityDef> GetVisibleLeaves()
        {
            VisibleLeaves.Clear();
            foreach (var kvp in RootEntities)
            {
                var ent = kvp.Key;
                if (EntityVisibleAtMaxLodLevel(ent))
                {
                    ent.Distance = MapViewEnabled ? MapViewDist : (ent.Position - Position).Length();
                    if (ent.Distance <= (ent.LodDist * LodDistMult))
                    {
                        RecurseAddVisibleLeaves(ent);
                    }
                }
            }
            return VisibleLeaves;
        }
        private void RecurseAddVisibleLeaves(YmapEntityDef ent)
        {
            var clist = GetEntityChildren(ent);
            if (clist != null)
            {
                var cnode = clist.First;
                while (cnode != null)
                {
                    RecurseAddVisibleLeaves(cnode.Value);
                    cnode = cnode.Next;
                }
            }
            else
            {
                if (EntityVisible(ent))
                {
                    VisibleLeaves.Add(ent);
                }
            }
        }



        private LinkedList<YmapEntityDef> GetEntityChildren(YmapEntityDef ent)
        {
            //get the children list for this entity, if all the hcildren are available, and they are within range
            if (!EntityChildrenVisibleAtMaxLodLevel(ent)) return null;
            var clist = ent.LodManagerChildren;
            if ((clist != null) && (clist.Count >= ent._CEntityDef.numChildren))
            {
                if (ent.Parent != null)//already calculated root entities distance
                {
                    ent.Distance = MapViewEnabled ? MapViewDist : (ent.Position - Position).Length();
                }
                if (ent.Distance <= (ent.ChildLodDist * LodDistMult))
                {
                    return clist;
                }
                else
                {
                    var cnode = clist.First;
                    while (cnode != null)
                    {
                        var child = cnode.Value;
                        child.Distance = MapViewEnabled ? MapViewDist : (child.Position - Position).Length();
                        if (child.Distance <= (child.LodDist * LodDistMult))
                        {
                            return clist;
                        }
                        cnode = cnode.Next;
                    }
                }
            }
            return null;
        }

        private bool EntityVisible(YmapEntityDef ent)
        {
            if (MapViewEnabled)
            {
                return Camera.ViewFrustum.ContainsAABBNoFrontClipNoOpt(ref ent.BBMin, ref ent.BBMax);
            }
            else
            {
                return Camera.ViewFrustum.ContainsAABBNoClip(ref ent.BBCenter, ref ent.BBExtent);
            }
        }
        private bool EntityVisibleAtMaxLodLevel(YmapEntityDef ent)
        {
            if (MaxLOD != rage__eLodType.LODTYPES_DEPTH_ORPHANHD)
            {
                if ((ent._CEntityDef.lodLevel == rage__eLodType.LODTYPES_DEPTH_ORPHANHD) ||
                    (ent._CEntityDef.lodLevel < MaxLOD))
                {
                    return false;
                }
            }
            return true;
        }
        private bool EntityChildrenVisibleAtMaxLodLevel(YmapEntityDef ent)
        {
            if (MaxLOD != rage__eLodType.LODTYPES_DEPTH_ORPHANHD)
            {
                if ((ent._CEntityDef.lodLevel == rage__eLodType.LODTYPES_DEPTH_ORPHANHD) ||
                    (ent._CEntityDef.lodLevel <= MaxLOD))
                {
                    return false;
                }
            }
            return true;
        }


    }











}
