using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.World;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Rendering
{
    public class ShaderManager
    {
        private DXManager DXMan;

        private int GeometryCount;
        public int RenderedGeometries;

        private int VerticesCount;
        public int RenderedVeritices;

        private Device Device;

        public bool wireframe = Settings.Default.Wireframe;
        RasterizerState rsSolid;
        RasterizerState rsWireframe;
        RasterizerState rsSolidDblSided;
        RasterizerState rsWireframeDblSided;
        BlendState bsDefault;
        BlendState bsAlpha;
        BlendState bsAdd;
        DepthStencilState dsEnabled;
        DepthStencilState dsDisableAll;
        DepthStencilState dsDisableComp;
        DepthStencilState dsDisableWrite;
        DepthStencilState dsDisableWriteRev;



        public DeferredScene DefScene { get; set; }
        public PostProcessor HDR { get; set; }
        public BasicShader Basic { get; set; }
        public CableShader Cable { get; set; }
        public WaterShader Water { get; set; }
        public TerrainShader Terrain { get; set; }
        public TreesLodShader TreesLod { get; set; }
        public SkydomeShader Skydome { get; set; }
        public CloudsShader Clouds { get; set; }
        public MarkerShader Marker { get; set; }
        public BoundsShader Bounds { get; set; }
        public ShadowShader Shadow { get; set; }
        public DistantLightsShader DistLights { get; set; }
        public PathShader Paths { get; set; }
        public WidgetShader Widgets { get; set; }

        public bool shadows = Settings.Default.Shadows;
        public Shadowmap Shadowmap { get; set; }
        List<RenderableGeometryInst> shadowcasters = new List<RenderableGeometryInst>();
        List<RenderableGeometryInst> shadowbatch = new List<RenderableGeometryInst>();
        List<ShaderBatch> shadowbatches = new List<ShaderBatch>();
        int shadowcastercount = 0; //total casters rendered

        public bool deferred = Settings.Default.Deferred;
        public bool hdr = Settings.Default.HDR;
        public float hdrLumBlendSpeed = 2.0f;
        int Width;
        int Height;

        private bool disposed = false;


        public List<ShaderRenderBucket> RenderBuckets = new List<ShaderRenderBucket>();
        public List<RenderableBoundGeometryInst> RenderBoundGeoms = new List<RenderableBoundGeometryInst>();
        public List<RenderableInstanceBatchInst> RenderInstBatches = new List<RenderableInstanceBatchInst>();
        public List<RenderableLightInst> RenderLights = new List<RenderableLightInst>();
        public List<RenderableLODLights> RenderLODLights = new List<RenderableLODLights>();
        public List<RenderableDistantLODLights> RenderDistLODLights = new List<RenderableDistantLODLights>();
        public List<RenderablePathBatch> RenderPathBatches = new List<RenderablePathBatch>();
        public List<RenderableWaterQuad> RenderWaterQuads = new List<RenderableWaterQuad>();

        public bool AnisotropicFiltering = true;
        public WorldRenderMode RenderMode = WorldRenderMode.Default;
        public int RenderVertexColourIndex = 1;
        public int RenderTextureCoordIndex = 1;
        public int RenderTextureSamplerCoord = 1;
        public ShaderParamNames RenderTextureSampler = ShaderParamNames.DiffuseSampler;
        public double CurrentRealTime = 0;
        public float CurrentElapsedTime = 0;

        private Camera Camera;
        public ShaderGlobalLights GlobalLights = new ShaderGlobalLights();
        public bool PathsDepthClip = true;//false;//

        private GameFileCache GameFileCache;
        private RenderableCache RenderableCache;


        public long TotalGraphicsMemoryUse
        {
            get
            {
                long u = 0;
                if (DefScene != null)
                {
                    u += DefScene.VramUsage;
                }
                if (HDR != null)
                {
                    u += HDR.VramUsage;
                }
                if (Shadowmap != null)
                {
                    u += Shadowmap.VramUsage;
                }
                return u;
            }
        }


        public ShaderManager(Device device, DXManager dxman)
        {
            Device = device;
            DXMan = dxman;

            //HDR = new PostProcessor(dxman);
            Basic = new BasicShader(device);
            Cable = new CableShader(device);
            Water = new WaterShader(device);
            Terrain = new TerrainShader(device);
            TreesLod = new TreesLodShader(device);
            Skydome = new SkydomeShader(device);
            Clouds = new CloudsShader(device);
            Marker = new MarkerShader(device);
            Bounds = new BoundsShader(device);
            Shadow = new ShadowShader(device);
            DistLights = new DistantLightsShader(device);
            Paths = new PathShader(device);
            Widgets = new WidgetShader(device);


            RasterizerStateDescription rsd = new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f,
            };
            rsSolid = new RasterizerState(device, rsd);
            rsd.FillMode = FillMode.Wireframe;
            rsWireframe = new RasterizerState(device, rsd);
            rsd.CullMode = CullMode.None;
            rsWireframeDblSided = new RasterizerState(device, rsd);
            rsd.FillMode = FillMode.Solid;
            rsSolidDblSided = new RasterizerState(device, rsd);
            rsd.CullMode = CullMode.Back;


            BlendStateDescription bsd = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,//true,
                IndependentBlendEnable = false,
            };
            bsd.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            bsd.RenderTarget[0].BlendOperation = BlendOperation.Add;
            bsd.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            bsd.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            bsd.RenderTarget[0].IsBlendEnabled = true;
            bsd.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            bsd.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            bsd.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            bsd.RenderTarget[1] = bsd.RenderTarget[0];
            bsd.RenderTarget[2] = bsd.RenderTarget[0];
            bsd.RenderTarget[3] = bsd.RenderTarget[0];
            bsDefault = new BlendState(device, bsd);

            bsd.AlphaToCoverageEnable = true;
            bsAlpha = new BlendState(device, bsd);

            bsd.AlphaToCoverageEnable = false;
            bsd.RenderTarget[0].DestinationBlend = BlendOption.One;
            bsAdd = new BlendState(device, bsd);

            DepthStencilStateDescription dsd = new DepthStencilStateDescription()
            {
                BackFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.GreaterEqual,
                    DepthFailOperation = StencilOperation.Zero,
                    FailOperation = StencilOperation.Zero,
                    PassOperation = StencilOperation.Zero,
                },
                DepthComparison = Comparison.GreaterEqual,
                DepthWriteMask = DepthWriteMask.All,
                FrontFace = new DepthStencilOperationDescription()
                {
                    Comparison = Comparison.GreaterEqual,
                    DepthFailOperation = StencilOperation.Zero,
                    FailOperation = StencilOperation.Zero,
                    PassOperation = StencilOperation.Zero
                },
                IsDepthEnabled = true,
                
                IsStencilEnabled = false,
                StencilReadMask = 0,
                StencilWriteMask = 0
            };
            dsEnabled = new DepthStencilState(device, dsd);
            dsd.DepthWriteMask = DepthWriteMask.Zero;
            dsDisableWrite = new DepthStencilState(device, dsd);
            dsd.DepthComparison = Comparison.LessEqual;
            dsDisableWriteRev = new DepthStencilState(device, dsd);
            dsd.DepthComparison = Comparison.Always;
            dsDisableComp = new DepthStencilState(device, dsd);
            dsd.IsDepthEnabled = false;
            dsDisableAll = new DepthStencilState(device, dsd);
        }



        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            dsEnabled.Dispose();
            dsDisableWriteRev.Dispose();
            dsDisableWrite.Dispose();
            dsDisableComp.Dispose();
            dsDisableAll.Dispose();
            bsDefault.Dispose();
            bsAlpha.Dispose();
            bsAdd.Dispose();
            rsSolid.Dispose();
            rsWireframe.Dispose();
            rsSolidDblSided.Dispose();
            rsWireframeDblSided.Dispose();

            Widgets.Dispose();
            Paths.Dispose();
            DistLights.Dispose();
            Shadow.Dispose();
            Bounds.Dispose();
            Marker.Dispose();
            Terrain.Dispose();
            TreesLod.Dispose();
            Skydome.Dispose();
            Clouds.Dispose();
            Basic.Dispose();
            Cable.Dispose();
            Water.Dispose();


            if (DefScene != null)
            {
                DefScene.Dispose();
                DefScene = null;
            }

            if (HDR != null)
            {
                HDR.Dispose();
                HDR = null;
            }

            if (Shadowmap != null)
            {
                Shadowmap.Dispose();
                Shadowmap = null;
            }
        }


        public void SetGlobalLightParams(ShaderGlobalLights lights)
        {
            GlobalLights.CurrentSunDir = lights.CurrentSunDir;
            GlobalLights.CurrentMoonDir = lights.CurrentMoonDir;
            GlobalLights.HdrEnabled = lights.HdrEnabled;
            GlobalLights.HdrIntensity = lights.HdrIntensity;
            GlobalLights.SpecularEnabled = lights.SpecularEnabled;
            GlobalLights.Weather = lights.Weather;
            GlobalLights.Params = lights.Params;
        }

        public void BeginFrame(DeviceContext context, double currentRealTime, float elapsedTime)
        {
            if (disposed) return;

            CurrentRealTime = currentRealTime;
            CurrentElapsedTime = elapsedTime;

            shadowcasters.Clear();
            if (shadows && (Shadowmap == null))
            {
                Shadowmap = new Shadowmap(Device);
            }
            if (!shadows && (Shadowmap != null))
            {
                Shadowmap.Dispose();
                Shadowmap = null;
            }
            if (hdr && (HDR == null))
            {
                HDR = new PostProcessor(DXMan);
                HDR.OnWindowResize(DXMan);
                HDR.LumBlendSpeed = hdrLumBlendSpeed;
            }
            if (!hdr && (HDR != null))
            {
                HDR.Dispose();
                HDR = null;
            }
            if (deferred && (DefScene == null))
            {
                DefScene = new DeferredScene(DXMan);
                DefScene.OnWindowResize(DXMan);
            }
            if (!deferred && (DefScene != null))
            {
                DefScene.Dispose();
                DefScene = null;
            }


            foreach (var bucket in RenderBuckets)
            {
                bucket.Clear();
            }

            RenderBoundGeoms.Clear();
            RenderInstBatches.Clear();
            RenderLights.Clear();
            RenderLODLights.Clear();
            RenderDistLODLights.Clear();
            RenderPathBatches.Clear();
            RenderWaterQuads.Clear();


            if (DefScene != null)
            {
                DefScene.Clear(context);
                DefScene.ClearDepth(context);
            }
            if (HDR != null)
            {
                HDR.Clear(context);
                HDR.ClearDepth(context);
            }

            if (DefScene != null)
            {
                DefScene.SetSceneColour(context);
            }
            else if (HDR != null)
            {
                HDR.SetPrimary(context); //for rendering some things before shadowmaps... (eg sky)
            }
            else
            {
                DXMan.SetDefaultRenderTarget(context);
            }

            Skydome.EnableHDR = hdr;
            Clouds.EnableHDR = hdr;

            Basic.Deferred = deferred;
            Cable.Deferred = deferred;
            Water.Deferred = deferred;
            Terrain.Deferred = deferred;
            TreesLod.Deferred = deferred;
        }


        public void EnsureShaderTextures(GameFileCache gameFileCache, RenderableCache renderableCache)
        {
            if (!gameFileCache.IsInited) return;

            GameFileCache = gameFileCache;
            RenderableCache = renderableCache;

            uint graphics = 3154743001; //JenkHash.GenHash("graphics");
            uint waterbump = 2826194296; //JenkHash.GenHash("waterbump");
            uint waterbump2 = 209023451; //JenkHash.GenHash("waterbump2");
            uint waterfog = 4047019542;
            Water.waterbump = EnsureTexture(graphics, waterbump);
            Water.waterbump2 = EnsureTexture(graphics, waterbump2);
            Water.waterfog = EnsureTexture(graphics, waterfog);



        }

        private RenderableTexture EnsureTexture(uint texDict, uint texName)
        {
            YtdFile ytd = GameFileCache.GetYtd(texDict);
            if ((ytd != null) && (ytd.Loaded) && (ytd.TextureDict != null))
            {
                var dtex = ytd.TextureDict.Lookup(texName);
                return RenderableCache.GetRenderableTexture(dtex);
            }
            return null;
        }

        public void RenderQueued(DeviceContext context, Camera camera, Vector4 wind)
        {
            GeometryCount = 0;
            VerticesCount = 0;
            Camera = camera;


            SetShaderRenderModeParams();

            Basic.WindVector = wind;
            Shadow.WindVector = wind;
            Water.CurrentRealTime = CurrentRealTime;
            Water.CurrentElapsedTime = CurrentElapsedTime;

            shadowbatches.Clear();
            for (int i = 0; i < RenderBuckets.Count; i++)
            {
                var bucket = RenderBuckets[i];

                //sort the bucket's batches based on the shader file hashes
                bucket.GroupBatches();

                if (shadows)
                {
                    shadowbatches.AddRange(bucket.BasicBatches);
                    shadowbatches.AddRange(bucket.TerrainBatches);
                    shadowbatches.AddRange(bucket.CableBatches);
                    shadowbatches.AddRange(bucket.CutoutBatches);
                    shadowbatches.AddRange(bucket.ClothBatches);
                }
            }


            if (shadows)
            {
                RenderShadowmap(context);
            }


            if (DefScene != null)
            {
                DefScene.SetGBuffers(context);
            }
            else if (HDR != null)
            {
                HDR.SetPrimary(context);
            }
            else
            {
                DXMan.SetDefaultRenderTarget(context);
            }

            for (int i = 0; i < RenderBuckets.Count; i++) //"solid" objects pass
            {
                var bucket = RenderBuckets[i];

                context.OutputMerger.BlendState = bsDefault;
                context.OutputMerger.DepthStencilState = dsEnabled;
                context.Rasterizer.State = wireframe ? rsWireframe : rsSolid;

                if (bucket.TerrainBatches.Count > 0)
                {
                    RenderGeometryBatches(context, bucket.TerrainBatches, Terrain);
                }
                if (bucket.BasicBatches.Count > 0)
                {
                    RenderGeometryBatches(context, bucket.BasicBatches, Basic);
                }
                if (bucket.TreesLodBatches.Count > 0)
                {
                    context.Rasterizer.State = wireframe ? rsWireframeDblSided : rsSolidDblSided;
                    RenderGeometryBatches(context, bucket.TreesLodBatches, TreesLod);
                }
                if (bucket.CutoutBatches.Count > 0)
                {
                    context.Rasterizer.State = wireframe ? rsWireframeDblSided : rsSolidDblSided;
                    RenderGeometryBatches(context, bucket.CutoutBatches, Basic);
                }
                if (bucket.ClothBatches.Count > 0)
                {
                    context.Rasterizer.State = wireframe ? rsWireframeDblSided : rsSolidDblSided;
                    RenderGeometryBatches(context, bucket.ClothBatches, Basic);
                }
                if (bucket.CableBatches.Count > 0)
                {
                    context.Rasterizer.State = wireframe ? rsWireframe : rsSolid;
                    RenderGeometryBatches(context, bucket.CableBatches, Cable);
                }
                if (bucket.DecalBatches.Count > 0)
                {
                    context.Rasterizer.State = wireframe ? rsWireframe : rsSolid;
                    context.OutputMerger.DepthStencilState = dsDisableWrite;
                    Basic.DecalMode = true;
                    RenderGeometryBatches(context, bucket.DecalBatches, Basic);
                    Basic.DecalMode = false;
                    context.OutputMerger.DepthStencilState = dsEnabled;
                }
            }


            if (RenderInstBatches.Count > 0) //grass pass
            {
                context.Rasterizer.State = wireframe ? rsWireframeDblSided : rsSolidDblSided;
                context.OutputMerger.BlendState = bsAlpha; //alpha to coverage for grass...
                Basic.DecalMode = true;
                Basic.AlphaScale = 7.0f; //instanced grass alpha scale...
                Basic.SetShader(context);
                Basic.SetSceneVars(context, Camera, Shadowmap, GlobalLights);
                for (int i = 0; i < RenderInstBatches.Count; i++)
                {
                    RenderInstancedBatch(context, RenderInstBatches[i]);
                }
                Basic.UnbindResources(context);
                Basic.AlphaScale = 1.0f;
                Basic.DecalMode = false;
                context.OutputMerger.BlendState = bsDefault;
            }




            context.OutputMerger.BlendState = bsDefault;
            context.Rasterizer.State = wireframe ? rsWireframeDblSided : rsSolidDblSided;
            context.OutputMerger.DepthStencilState = dsEnabled;
            if (RenderWaterQuads.Count > 0) //render water quads
            {
                Water.SetShader(context);
                Water.SetSceneVars(context, Camera, Shadowmap, GlobalLights);
                for (int i = 0; i < RenderWaterQuads.Count; i++)
                {
                    Water.RenderWaterQuad(context, RenderWaterQuads[i]);
                }
                Water.UnbindResources(context);
            }
            for (int i = 0; i < RenderBuckets.Count; i++) //main water geoms pass
            {
                var bucket = RenderBuckets[i];
                if (bucket.WaterBatches.Count > 0)
                {
                    RenderGeometryBatches(context, bucket.WaterBatches, Water);
                }
            }

            context.OutputMerger.DepthStencilState = dsDisableWrite;
            for (int i = 0; i < RenderBuckets.Count; i++) //water decals pass
            {
                var bucket = RenderBuckets[i];
                if (bucket.Water2Batches.Count > 0)
                {
                    RenderGeometryBatches(context, bucket.Water2Batches, Water);
                }
            }



            //TODO: needs second gbuffer pass?
            Basic.DecalMode = true;
            for (int i = 0; i < RenderBuckets.Count; i++) //alphablended and glass pass
            {
                var bucket = RenderBuckets[i];

                if (bucket.AlphaBatches.Count > 0)
                {
                    //context.OutputMerger.BlendState = bsAlpha;
                    RenderGeometryBatches(context, bucket.AlphaBatches, Basic);
                }
                if (bucket.GlassBatches.Count > 0)
                {
                    RenderGeometryBatches(context, bucket.GlassBatches, Basic);
                }
            }
            Basic.DecalMode = false;
            context.OutputMerger.DepthStencilState = dsEnabled;





            if (DefScene != null)
            {
                context.Rasterizer.State = rsSolid;

                DefScene.SetSceneColour(context);

                DefScene.RenderLights(context, camera, Shadowmap, GlobalLights);

                if (RenderLODLights.Count > 0) //LOD lights pass
                {
                    context.OutputMerger.BlendState = bsAdd; //additive blend for lights...
                    context.OutputMerger.DepthStencilState = dsDisableWriteRev;//only render parts behind or at surface
                    DefScene.RenderLights(context, camera, RenderLODLights);
                }

                if (RenderLights.Count > 0)
                {
                    context.OutputMerger.BlendState = bsAdd; //additive blend for lights...
                    context.OutputMerger.DepthStencilState = dsDisableWriteRev;//only render parts behind or at surface
                    DefScene.RenderLights(context, camera, RenderLights);
                }
            }

            Basic.Deferred = false;





            if (RenderDistLODLights.Count > 0) //distant LOD lights pass
            {
                context.Rasterizer.State = rsSolidDblSided;
                context.OutputMerger.BlendState = bsAdd; //additive blend for distant lod lights...
                context.OutputMerger.DepthStencilState = dsDisableWrite;
                DistLights.SetShader(context);
                DistLights.SetSceneVars(context, Camera, Shadowmap, GlobalLights);
                for (int i = 0; i < RenderDistLODLights.Count; i++)
                {
                    DistLights.RenderBatch(context, RenderDistLODLights[i]);
                }
                DistLights.UnbindResources(context);
                context.OutputMerger.BlendState = bsDefault;
                context.OutputMerger.DepthStencilState = dsEnabled;
            }



            if (RenderBoundGeoms.Count > 0) //collision meshes pass
            {
                if (DefScene != null)
                {
                    DefScene.ClearDepth(context);
                }
                else
                {
                    ClearDepth(context); //draw over everything else
                }

                context.OutputMerger.BlendState = bsDefault;
                context.OutputMerger.DepthStencilState = dsEnabled;
                context.Rasterizer.State = wireframe ? rsWireframe : rsSolid;

                RenderBoundGeometryBatch(context, RenderBoundGeoms);
            }


            if (RenderPathBatches.Count > 0) //paths pass
            {
                //ClearDepth(context); //draw over everything else

                context.OutputMerger.BlendState = bsDefault;
                context.OutputMerger.DepthStencilState = PathsDepthClip ? dsDisableWrite : dsDisableAll;// dsEnabled; //
                context.Rasterizer.State = rsSolid;

                Paths.RenderBatches(context, RenderPathBatches, camera, GlobalLights);
            }


            context.OutputMerger.BlendState = bsDefault;


            RenderedGeometries = GeometryCount;
            RenderedVeritices = VerticesCount;
        }

        public void RenderFinalPass(DeviceContext context)
        {
            context.Rasterizer.State = rsSolid;
            context.OutputMerger.BlendState = bsDefault;
            context.OutputMerger.DepthStencilState = dsDisableAll;

            if (HDR != null)
            {
                if ((DefScene?.SSAASampleCount ?? 1) > 1)
                {
                    HDR.SetPrimary(context);
                    DefScene.SSAAPass(context);
                }

                HDR.Render(DXMan, CurrentElapsedTime, DefScene);
            }
            else if (DefScene != null)
            {
                DXMan.SetDefaultRenderTarget(context);
                DefScene.SSAAPass(context);
            }

            Basic.Deferred = deferred;
        }

        private void RenderShadowmap(DeviceContext context)
        {
            context.OutputMerger.BlendState = bsDefault;
            context.OutputMerger.DepthStencilState = dsEnabled;
            context.Rasterizer.State = rsSolid;

            float maxdist = Shadowmap.maxShadowDistance;// 3000.0f;// cascade.IntervalFar * 5.0f;

            //find the casters within range
            shadowcasters.Clear();
            shadowcastercount = 0;
            for (int b = 0; b < shadowbatches.Count; b++)
            {
                //shadowcasters.AddRange(shadowbatches[b].Geometries);
                var sbgeoms = shadowbatches[b].Geometries;
                for (int g = 0; g < sbgeoms.Count; g++)
                {
                    var sbgeom = sbgeoms[g];
                    if (sbgeom.Inst.CastShadow)
                    {
                        float idist = sbgeom.Inst.Distance - sbgeom.Inst.Radius;
                        if (idist <= maxdist)
                        {
                            shadowcasters.Add(sbgeom);
                        }
                    }
                }
            }

            //render cascades
            Shadowmap.BeginUpdate(context, Camera, GlobalLights.Params.LightDir, shadowcasters);
            for (int i = 0; i < Shadowmap.CascadeCount; i++)
            {
                var cascade = Shadowmap.Cascades[i];

                Shadowmap.BeginDepthRender(context, i);

                float worldtocascade = cascade.WorldUnitsToCascadeUnits * 2.0f;
                float minrad = cascade.WorldUnitsPerTexel * 5.0f;

                shadowbatch.Clear();
                for (int c = 0; c < shadowcasters.Count; c++)
                {
                    //if the caster overlaps the cascade, draw it
                    var caster = shadowcasters[c];
                    if (caster.Inst.Radius <= minrad) continue; //don't render little things
                    Vector3 iscenepos = caster.Inst.Position - Shadowmap.SceneOrigin;
                    Vector3 ilightpos = cascade.Matrix.Multiply(iscenepos + caster.Inst.BSCenter);
                    float ilightradf = caster.Inst.Radius * worldtocascade;
                    float ilightminx = ilightpos.X - ilightradf;
                    float ilightmaxx = ilightpos.X + ilightradf;
                    float ilightminy = ilightpos.Y - ilightradf;
                    float ilightmaxy = ilightpos.Y + ilightradf;
                    if ((ilightmaxx > -1.0f) && (ilightminx < 1.0f) && (ilightmaxy > -1.0f) && (ilightminy < 1.0f))
                    {
                        shadowcastercount++;
                        caster.Inst.CamRel = iscenepos;
                        shadowbatch.Add(caster);
                    }
                }

                Shadow.SetShader(context);
                Shadow.SetSceneVars(context, cascade.Matrix);
                RenderGeometryBatch(context, shadowbatch, Shadow);

            }
            Shadowmap.EndUpdate(context);
            Shadow.UnbindResources(context);

            context.OutputMerger.BlendState = bsDefault;
            context.OutputMerger.DepthStencilState = dsEnabled;
            context.Rasterizer.State = rsSolid;
        }


        private void RenderGeometryBatches(DeviceContext context, List<ShaderBatch> batches, Shader shader)
        {
            shader.SetShader(context);
            shader.SetSceneVars(context, Camera, Shadowmap, GlobalLights);
            foreach (var batch in batches)
            {
                RenderGeometryBatch(context, batch.Geometries, shader);
            }
            shader.UnbindResources(context);
        }
        private void RenderGeometryBatch(DeviceContext context, List<RenderableGeometryInst> batch, Shader shader)
        {
            GeometryCount += batch.Count;

            RenderableModel model = null;

            VertexType vtyp = 0;
            bool vtypok = false;
            for (int i = 0; i < batch.Count; i++)
            {
                var geom = batch[i];
                var gmodel = geom.Geom.Owner;
                shader.SetEntityVars(context, ref geom.Inst);

                VerticesCount += geom.Geom.VertexCount;

                if (gmodel != model)
                {
                    model = gmodel;
                    shader.SetModelVars(context, model);
                }
                if (geom.Geom.VertexType != vtyp)
                {
                    vtyp = geom.Geom.VertexType;
                    if (!shader.SetInputLayout(context, vtyp))
                    {
                        //vertex type not supported...
                        vtypok = false;
                    }
                    else
                    {
                        vtypok = true;
                    }
                }
                if (vtypok)
                {

                    shader.SetGeomVars(context, geom.Geom);
                    geom.Geom.Render(context);

                }
            }

        }

        private void RenderBoundGeometryBatch(DeviceContext context, List<RenderableBoundGeometryInst> batch)
        {
            Basic.RenderMode = WorldRenderMode.VertexColour;
            Basic.SetShader(context);
            Basic.SetSceneVars(context, Camera, /*Shadowmap*/ null, GlobalLights);//should this be using shadows??
            Basic.SetInputLayout(context, VertexType.Default);

            GeometryCount += batch.Count;

            foreach (var geom in batch)
            {
                VerticesCount += geom.Geom.VertexCount;
                Basic.RenderBoundGeom(context, geom);
            }

            Basic.UnbindResources(context);
        }

        private void RenderInstancedBatch(DeviceContext context, RenderableInstanceBatchInst batch)
        {
            Basic.SetInstanceVars(context, batch.Batch);

            if (batch.Renderable.HDModels.Length > 1)
            { }

            foreach (var model in batch.Renderable.HDModels)
            {
                if (model.Geometries.Length > 1)
                { }

                Basic.SetModelVars(context, model);
                foreach (var geom in model.Geometries)
                {
                    if (Basic.SetInputLayout(context, geom.VertexType))
                    {
                        Basic.SetGeomVars(context, geom);
                        geom.RenderInstanced(context, batch.Batch.InstanceCount);
                    }
                }
            }
        }



        public void Enqueue(ref RenderableGeometryInst geom)
        {
            var shader = geom.Geom.DrawableGeom.Shader;

            var b = (shader!=null) ? shader.RenderBucket : 0; //rage render bucket?

            var bucket = EnsureRenderBucket(b);

            ShaderBatch batch = null;
            ShaderKey key = new ShaderKey();
            key.ShaderName = (shader!=null) ? shader.Name : new MetaHash(0);
            key.ShaderFile = (shader!=null) ? shader.FileName : new MetaHash(0);

            if (!bucket.Batches.TryGetValue(key, out batch))
            {
                batch = new ShaderBatch(key);
                bucket.Batches.Add(key, batch);
            }

            batch.Geometries.Add(geom);
        }
        public void Enqueue(ref RenderableLightInst light)
        {
            RenderLights.Add(light);
        }
        public void Enqueue(ref RenderableBoundGeometryInst geom)
        {
            RenderBoundGeoms.Add(geom);
        }
        public void Enqueue(ref RenderableInstanceBatchInst batch)
        {
            RenderInstBatches.Add(batch);
        }
        public void Enqueue(RenderableLODLights lights)
        {
            RenderLODLights.Add(lights);
        }
        public void Enqueue(RenderableDistantLODLights lights)
        {
            RenderDistLODLights.Add(lights);
        }
        public void Enqueue(RenderablePathBatch paths)
        {
            RenderPathBatches.Add(paths);
        }
        public void Enqueue(RenderableWaterQuad waterquad)
        {
            RenderWaterQuads.Add(waterquad);
        }


        public ShaderRenderBucket EnsureRenderBucket(int index)
        {
            ShaderRenderBucket bucket = null;
            while (index >= RenderBuckets.Count)
            {
                RenderBuckets.Add(new ShaderRenderBucket(RenderBuckets.Count));
            }
            if (index < RenderBuckets.Count)
            {
                bucket = RenderBuckets[index];
            }
            return bucket;
        }


        private void SetShaderRenderModeParams()
        {
            Basic.RenderMode = RenderMode;
            Basic.RenderVertexColourIndex = RenderVertexColourIndex;
            Basic.RenderTextureCoordIndex = RenderTextureCoordIndex;
            Basic.RenderTextureSamplerCoord = RenderTextureSamplerCoord;
            Basic.RenderTextureSampler = RenderTextureSampler;
            Basic.AnisotropicFilter = AnisotropicFiltering;
            Clouds.AnisotropicFilter = AnisotropicFiltering;
            Water.RenderMode = RenderMode;
            Water.RenderVertexColourIndex = RenderVertexColourIndex;
            Water.RenderTextureCoordIndex = RenderTextureCoordIndex;
            Water.RenderTextureSamplerCoord = RenderTextureSamplerCoord;
            Water.RenderTextureSampler = RenderTextureSampler;
            Water.AnisotropicFilter = AnisotropicFiltering;
            Terrain.RenderMode = RenderMode;
            Terrain.RenderVertexColourIndex = RenderVertexColourIndex;
            Terrain.RenderTextureCoordIndex = RenderTextureCoordIndex;
            Terrain.RenderTextureSamplerCoord = RenderTextureSamplerCoord;
            Terrain.RenderTextureSampler = RenderTextureSampler;
            Terrain.AnisotropicFilter = AnisotropicFiltering;
        }



        public void ClearDepth(DeviceContext context, bool firstpass = true)
        {
            if ((HDR != null) && firstpass)
            {
                HDR.ClearDepth(context);
            }
            else
            {
                DXMan.ClearDepth(context);
            }
        }
        public void SetRasterizerMode(DeviceContext context, RasterizerMode mode)
        {
            switch (mode)
            {
                default:
                case RasterizerMode.Solid:
                    context.Rasterizer.State = rsSolid;
                    break;
                case RasterizerMode.Wireframe:
                    context.Rasterizer.State = rsWireframe;
                    break;
                case RasterizerMode.SolidDblSided:
                    context.Rasterizer.State = rsSolidDblSided;
                    break;
                case RasterizerMode.WireframeDblSided:
                    context.Rasterizer.State = rsWireframeDblSided;
                    break;
            }
        }
        public void SetDepthStencilMode(DeviceContext context, DepthStencilMode mode)
        {
            switch (mode)
            {
                default:
                case DepthStencilMode.Enabled:
                    context.OutputMerger.DepthStencilState = dsEnabled;
                    break;
                case DepthStencilMode.DisableWrite:
                    context.OutputMerger.DepthStencilState = dsDisableWrite;
                    break;
                case DepthStencilMode.DisableComp:
                    context.OutputMerger.DepthStencilState = dsDisableComp;
                    break;
                case DepthStencilMode.DisableAll:
                    context.OutputMerger.DepthStencilState = dsDisableAll;
                    break;
            }
        }
        public void SetDefaultBlendState(DeviceContext context)
        {
            context.OutputMerger.BlendState = bsDefault;
        }
        public void SetAlphaBlendState(DeviceContext context)
        {
            context.OutputMerger.BlendState = bsAlpha;
        }

        public void OnWindowResize(int w, int h)
        {
            Width = w;
            Height = h;
            if (DefScene != null)
            {
                DefScene.OnWindowResize(DXMan);
            }
            if (HDR != null)
            {
                HDR.OnWindowResize(DXMan);
            }
        }
    }

    public enum RasterizerMode
    {
        Solid = 1,
        Wireframe = 2,
        SolidDblSided = 3,
        WireframeDblSided = 4,
    }
    public enum DepthStencilMode
    {
        Enabled = 1,
        DisableWrite = 2,
        DisableComp = 3,
        DisableAll = 4,
    }


    public struct ShaderKey
    {
        public MetaHash ShaderName;
        public MetaHash ShaderFile;

        public override string ToString()
        {
            return ShaderFile.ToString() + ": " + ShaderName.ToString();
        }

        public override int GetHashCode()
        {
            return ShaderName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is not ShaderKey shaderKey) return false;
            return shaderKey.ShaderName == ShaderName && shaderKey.ShaderFile == ShaderFile;
        }
    }
    public class ShaderRenderBucket
    {
        public int Index;
        public Dictionary<ShaderKey, ShaderBatch> Batches = new Dictionary<ShaderKey, ShaderBatch>();

        public List<ShaderBatch> BasicBatches = new List<ShaderBatch>();
        public List<ShaderBatch> DecalBatches = new List<ShaderBatch>();
        public List<ShaderBatch> WaterBatches = new List<ShaderBatch>();
        public List<ShaderBatch> Water2Batches = new List<ShaderBatch>();
        public List<ShaderBatch> AlphaBatches = new List<ShaderBatch>();
        public List<ShaderBatch> GlassBatches = new List<ShaderBatch>();
        public List<ShaderBatch> CutoutBatches = new List<ShaderBatch>();
        public List<ShaderBatch> TerrainBatches = new List<ShaderBatch>();
        public List<ShaderBatch> TreesLodBatches = new List<ShaderBatch>();
        public List<ShaderBatch> CableBatches = new List<ShaderBatch>();
        public List<ShaderBatch> ClothBatches = new List<ShaderBatch>();
        public List<ShaderBatch> VehicleBatches = new List<ShaderBatch>();


        public ShaderRenderBucket(int index)
        {
            Index = index;
        }
        public void Clear()
        {
            foreach (var batch in Batches.Values)
            {
                batch.Clear();
            }
        }

        public void GroupBatches()
        {
            BasicBatches.Clear();
            DecalBatches.Clear();
            WaterBatches.Clear();
            Water2Batches.Clear();
            AlphaBatches.Clear();
            GlassBatches.Clear();
            CutoutBatches.Clear();
            TerrainBatches.Clear();
            TreesLodBatches.Clear();
            CableBatches.Clear();
            ClothBatches.Clear();
            VehicleBatches.Clear();

            foreach (var kvp in Batches.Where(p => p.Value.Geometries.Count > 0).OrderBy(p => p.Value.Geometries.Average(p => p.Inst.Distance)))
            {
                if (kvp.Value.Geometries.Count == 0) continue;

                List<ShaderBatch> b = null;
                switch (kvp.Key.ShaderFile.Hash)
                {
                    #region default batches
                    case 0://default
                    case 413996436://{default.sps}
                    case 1581835696://{default_um.sps}
                    case 200682448://{default_tnt.sps}
                    case 1891750326://{default_spec.sps}
                    case 3080824371://{default_detail.sps}
                    case 3939256279://{default_terrain_wet.sps}
                    case 2948410525://{normal.sps}
                    case 3326705511://{normal_um.sps}
                    case 1891558834://{normal_pxm.sps}
                    case 3348158737://{normal_pxm_tnt.sps}
                    case 346521853://{normal_spec.sps}
                    case 1092618307://{normal_spec_dpm.sps}
                    case 505619005://{normal_spec_detail.sps}
                    case 10880875://{normal_spec_detail_tnt.sps}
                    case 3376941572://{normal_spec_detail_dpm_tnt.sps}
                    case 1315370658://{normal_spec_detail_dpm.sps}
                    case 4246136089://{normal_spec_detail_dpm_vertdecal_tnt.sps}
                    case 810202407://{normal_spec_tnt.sps}
                    case 2121232575://{normal_spec_tnt_pxm.sps}
                    case 1489449972://{normal_spec_pxm.sps}
                    case 1117835835://{normal_spec_pxm_tnt.sps}
                    case 3085209681://{normal_spec_um.sps}
                    case 1332909972://{normal_spec_emissive.sps}
                    case 3424303599://{normal_spec_reflect.sps}
                    case 2072061694://{normal_spec_reflect_emissivenight.sps}
                    case 3564197994://{normal_spec_cubemap_reflect.sps}
                    case 1808536605://{normal_detail.sps}
                    case 557170358://{normal_detail_dpm.sps}
                    case 3420295952://{normal_diffspec.sps}
                    case 559317884://{normal_diffspec_detail_dpm_tnt.sps}
                    case 2735427639://{normal_diffspec_detail_dpm.sps}
                    case 1845339284://{normal_diffspec_detail.sps}
                    case 601788410://{normal_diffspec_detail_tnt.sps}
                    case 3756536307://{normal_diffspec_tnt.sps}
                    case 2988770526://{normal_tnt.sps}
                    case 1728634142://{normal_tnt_pxm.sps}
                    case 3362038760://{normal_terrain_wet.sps}
                    case 1685791092://{normal_reflect.sps}
                    case 2626821864://{normal_cubemap_reflect.sps}
                    case 2635608835://{emissive.sps}
                    case 443538781://{emissive_clip.sps}
                    case 2049580179://{emissive_speclum.sps}
                    case 1193295596://{emissive_tnt.sps}
                    case 1434302180://{emissivenight.sps}
                    case 1897917258://{emissivenight_geomnightonly.sps}
                    case 140448747://{emissivestrong.sps}
                    case 2247429097://{spec.sps}
                    case 1396339721://{spec_const.sps}
                    case 689627308://{spec_tnt.sps}
                    case 873285799://{spec_reflect.sps}
                    case 3634763545://{reflect.sps}
                    case 1658580369://{mirror_default.sps}
                    case 129155404://{mirror_crack.sps}
                    case 2067933913://{parallax.sps}
                    case 1084945164://{parallax_specmap.sps}
                    case 3052800384://{blend_2lyr.sps}
                    case 154074948://{spec_twiddle_tnt.sps}
                    case 14185869://{weapon_normal_spec_tnt.sps}
                        b = BasicBatches;
                        break;
                    #endregion
                    #region cloth batches
                    case 1106229739://{cloth_default.sps}
                    case 1800418130://{cloth_normal_spec.sps}
                    case 476099326://{cloth_spec_cutout.sps}
                    case 170694389://{cloth_normal_spec_cutout.sps}
                    case 38543133://{cloth_normal_spec_tnt.sps}
                        b = ClothBatches;
                        break;
                    #endregion
                    #region cutout batches
                    case 1530399584://{cutout.sps}
                    case 3190732435://{cutout_um.sps}
                    case 3959636627://{cutout_tnt.sps}
                    case 2219447268://{cutout_fence.sps}
                    case 3091995132://{cutout_fence_normal.sps}
                    case 3187789425://{cutout_hard.sps}
                    case 3339370144://{cutout_spec_tnt.sps}
                    case 1264076685://{normal_cutout.sps}
                    case 46387092://{normal_cutout_tnt.sps}
                    case 748520668://{normal_cutout_um.sps}
                    case 807996366://{normal_spec_cutout.sps}
                    case 3300978494://{normal_spec_cutout_tnt.sps}
                    case 582493193://{trees_lod.sps} //not billboarded..
                    case 2322653400://{trees.sps}
                    case 3334613197://{trees_tnt.sps}
                    case 3192134330://{trees_normal.sps}
                    case 1224713457://{trees_normal_spec.sps}
                    case 1229591973://{trees_normal_spec_tnt.sps}
                    case 4265705004://{trees_normal_diffspec.sps}
                    case 2245870123://{trees_normal_diffspec_tnt.sps}
                        b = CutoutBatches;
                        break;
                    #endregion
                    #region alpha batches
                    case 2021887493://{normal_spec_alpha.sps}
                    case 1086592620://{normal_spec_reflect_alpha.sps}
                    case 1436689415://{normal_spec_reflect_emissivenight_alpha.sps}
                    case 4237650253://{normal_spec_screendooralpha.sps}
                    case 1564459451://{normal_alpha.sps}
                    case 763839200://{normal_reflect_alpha.sps}
                    case 179247185://{emissive_alpha.sps}
                    case 1314864030://{emissive_alpha_tnt.sps}
                    case 1478174766://{emissive_additive_alpha.sps}
                    case 3733846327://{emissivenight_alpha.sps}
                    case 3174327089://{emissivestrong_alpha.sps}
                    case 257450439://{spec_alpha.sps}
                    case 2116642565://{spec_reflect_alpha.sps}
                    case 298255408://{alpha.sps}
                    case 181295180://{reflect_alpha.sps}
                    case 1896243360://{normal_screendooralpha.sps}
                    case 3724703640://{spec_screendooralpha.sps}
                    case 2161953435://{cloth_spec_alpha.sps} //alpha cloth is ok in alpha batch for now...
                    case 3203310712://{cloth_normal_spec_alpha.sps}
                        b = AlphaBatches;
                        break;
                    #endregion
                    #region water batches
                    case 1529202445://{water_river.sps}
                    case 4064804434://{water_riverlod.sps}
                    case 2871265627://{water_riverocean.sps}
                    case 1507348828://{water_rivershallow.sps}
                    case 3945561843://{water_fountain.sps}
                    case 4234404348://{water_shallow.sps}
                        b = WaterBatches;
                        break;
                    case 1077877097://{water_poolenv.sps}
                    case 3053856997://{water_riverfoam.sps}
                    case 3066724854://{water_terrainfoam.sps}
                    case 1471966282://{water_decal.sps} (should this be in decal batch?)
                        b = Water2Batches;
                        break;
                    #endregion
                    #region glass batches
                    case 3928756789://{glass.sps}
                    case 4018753208://{glass_pv.sps}
                    case 2800545026://{glass_pv_env.sps}
                    case 1263059426://{glass_env.sps}
                    case 3398951093://{glass_spec.sps}
                    case 1520288031://{glass_reflect.sps}
                    case 3924045432://{glass_emissive.sps}
                    case 837003310://{glass_emissivenight.sps}
                    case 485710087://{glass_emissivenight_alpha.sps}
                    case 1359281054://{glass_breakable.sps}
                    case 4237090538://{glass_breakable_screendooralpha.sps}
                    case 430314084://{glass_displacement.sps}
                    case 2866652360://{glass_normal_spec_reflect.sps}
                    case 2055615352://{glass_emissive_alpha.sps}
                        b = GlassBatches;
                        break;
                    #endregion
                    #region decal batches
                    case 3140040342://{decal.sps}
                    case 1093522222://{decal_tnt.sps}
                    case 3948167519://{decal_glue.sps}
                    case 3880384844://{decal_spec_only.sps}
                    case 341123999://{decal_normal_only.sps}
                    case 2918136469://{decal_emissive_only.sps}
                    case 2698880237://{decal_emissivenight_only.sps}
                    case 600733812://{decal_amb_only.sps}
                    case 1145906525://{normal_decal.sps}
                    case 1342302630://{normal_decal_pxm.sps}
                    case 2269053854://{normal_decal_pxm_tnt.sps}
                    case 108580378://{normal_decal_tnt.sps}
                    case 2189252961://{normal_spec_decal.sps}
                    case 992110385://{normal_spec_decal_detail.sps}
                    case 941334042://{normal_spec_decal_nopuddle.sps}
                    case 2739041469://{normal_spec_decal_tnt.sps}
                    case 3606424483://{normal_spec_decal_pxm.sps}
                    case 2842248626://{spec_decal.sps}
                    case 3717415672://{spec_reflect_decal.sps}
                    case 2457676400://{reflect_decal.sps}
                    case 2655725442://{decal_dirt.sps}
                    case 2706821972://{mirror_decal.sps}
                    case 1851110504://{distance_map.sps}
                        b = DecalBatches;
                        break;
                    #endregion
                    #region trees batches
                    case 4113118754://{trees_lod2.sps}
                        b = TreesLodBatches;
                        break;
                    #endregion
                    #region terrain batches
                    case 2131453483://{terrain_cb_w_4lyr.sps}
                    case 404312839://{terrain_cb_w_4lyr_lod.sps}
                    case 1196319128://{terrain_cb_w_4lyr_spec.sps}
                    case 1539365454://{terrain_cb_w_4lyr_spec_pxm.sps}
                    case 2415127391://{terrain_cb_w_4lyr_pxm_spm.sps}
                    case 4024079163://{terrain_cb_w_4lyr_pxm.sps}
                    case 1437909171://{terrain_cb_w_4lyr_cm_pxm.sps}
                    case 1185891401://{terrain_cb_w_4lyr_cm_tnt.sps} //golf
                    case 3254053796://{terrain_cb_w_4lyr_cm_pxm_tnt.sps} //golf
                    case 1567099655://{terrain_cb_w_4lyr_cm.sps}
                    case 3908107877://{terrain_cb_w_4lyr_2tex.sps}
                    case 341330913://{terrain_cb_w_4lyr_2tex_blend.sps}
                    case 673057355://{terrain_cb_w_4lyr_2tex_blend_lod.sps}
                    case 1046220944://{terrain_cb_w_4lyr_2tex_blend_pxm.sps}
                    case 2344506399://{terrain_cb_w_4lyr_2tex_blend_pxm_spm.sps}
                    case 3444846649://{terrain_cb_w_4lyr_2tex_pxm.sps}
                    case 3369161623://{terrain_cb_4lyr.sps}
                    case 1003015712://{terrain_cb_w_4lyr_spec_int_pxm.sps}
                    case 1927591706://{terrain_cb_w_4lyr_spec_int.sps}
                    case 3550605441://{terrain_cb_4lyr_lod.sps} //TODO! (used in custom stuff...)
                        b = TerrainBatches;
                        break;
                    #endregion
                    #region cable batches
                    case 3854885487://{cable.sps}
                        b = CableBatches;
                        break;
                    #endregion
                    #region vehicle batches
                    case 753908000://{vehicle_basic.sps}
                    case 1107511867://{vehicle_generic.sps}
                    case 3274951810://{vehicle_paint1.sps}
                    case 1009159769://{vehicle_paint2.sps}
                    case 2045642561://{vehicle_paint3.sps}
                    case 1534086746://{vehicle_paint3_enveff.sps}
                    case 4262329590://{vehicle_paint4.sps}
                    case 60950417://{vehicle_paint4_enveff.sps}
                    case 249472155://{vehicle_paint5_enveff.sps}
                    case 354168229://{vehicle_paint6_enveff.sps}
                    case 617726044://{vehicle_mesh.sps}
                    case 1138799003://{vehicle_mesh2_enveff.sps}
                    case 2162256878://{vehicle_tire.sps}
                    case 1337209217://{vehicle_tire_emissive.sps}
                    case 3106021319://{vehicle_interior.sps}
                    case 2837548125://{vehicle_interior2.sps}
                    case 2094873540://{vehicle_shuts.sps}
                    case 2405328911://{vehicle_licenseplate.sps}
                    case 2226589567://{vehicle_lights.sps}
                    case 364912658://{vehicle_lightsemissive.sps}
                    case 3030872505://{vehicle_emissive_opaque.sps}
                    case 1930196358://{vehicle_emissive_alpha.sps}
                    case 146667297://{vehicle_badges.sps}
                    case 4162395624://{vehicle_dash_emissive.sps}
                    case 254152173://{vehicle_dash_emissive_opaque.sps}
                    case 3355845283://{vehicle_detail2.sps}
                    case 4097152976://{vehicle_track.sps}
                    case 4268056926://{vehicle_track2.sps}
                    case 3631243954://{vehicle_blurredrotor.sps}
                    case 457610770://{vehicle_nosplash.sps}
                    case 3621563260://{vehicle_nowater.sps}
                    case 430888562://{vehicle_paint8.sps}
                    case 4118002252://{vehicle_paint9.sps}
                    case 158342452://{vehicle_detail.sps}
                    case 482429992://{vehicle_track_emissive.sps}
                        b = BasicBatches;
                        break;
                    case 1041778472://{vehicle_decal.sps}
                    case 1462664157://{vehicle_decal2.sps}
                    case 15603050://{vehicle_blurredrotor_emissive.sps}
                        b = DecalBatches;
                        break;
                    case 3096299666://{vehicle_vehglass.sps}
                    case 588619930://{vehicle_vehglass_inner.sps}
                        b = GlassBatches;
                        break;
                    case 3986926894://{vehicle_cloth.sps}
                        b = ClothBatches;
                        break;
                    case 2617558500://{vehicle_cutout.sps}
                        b = CutoutBatches;
                        break;
                    #endregion
                    #region TODO/unused batches
                    case 3333227093://{grass_fur.sps}
                    case 4256676773://{grass_fur_mask.sps}
                        break;//todo: grass_fur

                    case 83630553://{cpv_only.sps}
                    case 1238547107://{decal_shadow_only.sps}
                    case 1695474112://{trees_shadow_proxy.sps}
                        break; //should add this to a shadowproxies group.. but currently don't draw.
                    #endregion
                    #region TODO/clouds batches
                    case 4103916155://{clouds_animsoft.sps}
                    case 1097000161://{clouds_altitude.sps}
                    case 1481470665://{clouds_soft.sps}
                    case 2184108982://{clouds_fast.sps}
                    case 4192928948://{clouds_anim.sps}
                        b = BasicBatches; //todo: correct batched cloud rendering...
                        break;
                    #endregion

                    default:
                        b = BasicBatches;
                        break;
                }

                if (b != null)
                {
                    b.Add(kvp.Value);
                }

            }
        }
    }
    public class ShaderBatch
    {
        public ShaderKey Key;
        public List<RenderableGeometryInst> Geometries = new List<RenderableGeometryInst>();

        public ShaderBatch(ShaderKey key)
        {
            Key = key;
        }

        public void Clear()
        {
            Geometries.Clear();
        }

    }


    public class ShaderGlobalLights
    {
        public Weather Weather;
        public ShaderGlobalLightParams Params;
        public Vector3 CurrentSunDir;
        public Vector3 CurrentMoonDir;
        public Vector3 MoonAxis;
        public bool HdrEnabled;
        public float HdrIntensity;
        public bool SpecularEnabled;
    }
    public struct ShaderGlobalLightParams
    {
        public Vector3 LightDir;
        public float LightHdr; //global intensity
        public Color4 LightDirColour;
        public Color4 LightDirAmbColour;
        public Color4 LightNaturalAmbUp;
        public Color4 LightNaturalAmbDown;
        public Color4 LightArtificialAmbUp;
        public Color4 LightArtificialAmbDown;
    }


}
