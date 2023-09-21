using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX.Direct3D11;
using SharpDX;
using System.IO;
using System.Diagnostics;

namespace CodeWalker.Rendering
{
    public struct WaterShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Vector4 WaterVector;
        public float ScaledTime;
        public float ScnPad0;
        public float ScnPad1;
        public float ScnPad2;
    }
    public struct WaterShaderVSEntityVars
    {
        public Vector4 CamRel;
        public Quaternion Orientation;
        public Vector3 Scale;
        public uint EntPad0;
    }
    public struct WaterShaderVSGeomVars
    {
        public Vector4 WaterParams;
        public uint EnableFlow;
        public uint ShaderMode;
        public uint GeoPad1;
        public uint GeoPad2;
        public float RippleSpeed;
        public float GeoPad3;
        public float GeoPad4;
        public float GeoPad5;
    }
    public struct WaterShaderPSSceneVars
    {
        public ShaderGlobalLightParams GlobalLights;
        public uint EnableShadows;
        public uint RenderMode;//0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
        public uint RenderModeIndex; //colour/texcoord index
        public uint RenderSamplerCoord; //which texcoord to use in single texture mode
        public uint EnableWaterbumps;//if the waterbump textures are ready..
        public uint EnableFogtex; //if the fog texture is ready
        public uint ScnPad1;
        public uint ScnPad2;
        public Vector4 gFlowParams;
        public Vector4 CameraPos;
        public Vector4 WaterFogParams; //xy = base location, zw = inverse size
    }
    public struct WaterShaderPSGeomVars
    {
        public uint EnableTexture;
        public uint EnableBumpMap;
        public uint EnableFoamMap;
        public uint ShaderMode;
        public float SpecularIntensity;
        public float SpecularFalloff;
        public float GeoPad1;
        public float GeoPad2;
        public float WaveOffset; //for terrainfoam
        public float WaterHeight; //for terrainfoam
        public float WaveMovement; //for terrainfoam
        public float HeightOpacity; //for terrainfoam
        public float RippleSpeed;
        public float RippleScale;
        public float RippleBumpiness;
        public float GeoPad3;
    }


    public class WaterShader : Shader
    {
        bool disposed = false;

        VertexShader vspt;
        VertexShader vspct;
        VertexShader vspnct;
        VertexShader vspnctx;
        PixelShader ps;
        PixelShader psdef;

        GpuVarsBuffer<WaterShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<WaterShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<WaterShaderVSGeomVars> VSGeomVars;
        GpuVarsBuffer<WaterShaderPSSceneVars> PSSceneVars;
        GpuVarsBuffer<WaterShaderPSGeomVars> PSGeomVars;

        SamplerState texsampler;
        SamplerState texsampleranis;
        SamplerState texsamplerflow;

        private Dictionary<VertexType, InputLayout> layouts = new Dictionary<VertexType, InputLayout>();


        public bool AnisotropicFilter = false;
        public WorldRenderMode RenderMode = WorldRenderMode.Default;
        public int RenderVertexColourIndex = 1;
        public int RenderTextureCoordIndex = 1;
        public int RenderTextureSamplerCoord = 1;
        public ShaderParamNames RenderTextureSampler = ShaderParamNames.DiffuseSampler;
        public double CurrentRealTime = 0;
        public float CurrentElapsedTime = 0;
        public bool SpecularEnable = true;
        public bool Deferred = false;


        public RenderableTexture waterbump { get; set; }
        public RenderableTexture waterbump2 { get; set; }
        public RenderableTexture waterfog { get; set; }


        //check dt1_21_reflproxy and dt1_05_reflproxy


        public WaterShader(Device device)
        {
            string folder = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Shaders");
            byte[] vsptbytes = File.ReadAllBytes(Path.Combine(folder, "WaterVS_PT.cso"));
            byte[] vspctbytes = File.ReadAllBytes(Path.Combine(folder, "WaterVS_PCT.cso"));
            byte[] vspnctbytes = File.ReadAllBytes(Path.Combine(folder, "WaterVS_PNCT.cso"));
            byte[] vspnctxbytes = File.ReadAllBytes(Path.Combine(folder, "WaterVS_PNCTX.cso"));
            byte[] psbytes = File.ReadAllBytes(Path.Combine(folder, "WaterPS.cso"));
            byte[] psdefbytes = File.ReadAllBytes(Path.Combine(folder, "WaterPS_Deferred.cso"));


            vspt = new VertexShader(device, vsptbytes);
            vspct = new VertexShader(device, vspctbytes);
            vspnct = new VertexShader(device, vspnctbytes);
            vspnctx = new VertexShader(device, vspnctxbytes);
            ps = new PixelShader(device, psbytes);
            psdef = new PixelShader(device, psdefbytes);

            VSSceneVars = new GpuVarsBuffer<WaterShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<WaterShaderVSEntityVars>(device);
            VSGeomVars = new GpuVarsBuffer<WaterShaderVSGeomVars>(device);
            PSSceneVars = new GpuVarsBuffer<WaterShaderPSSceneVars>(device);
            PSGeomVars = new GpuVarsBuffer<WaterShaderPSGeomVars>(device);

            layouts.Add(VertexType.PT, new InputLayout(device, vsptbytes, VertexTypeGTAV.GetLayout(VertexType.PT)));
            layouts.Add(VertexType.PCT, new InputLayout(device, vspctbytes, VertexTypeGTAV.GetLayout(VertexType.PCT)));
            layouts.Add(VertexType.Default, new InputLayout(device, vspnctbytes, VertexTypeGTAV.GetLayout(VertexType.Default)));
            layouts.Add(VertexType.DefaultEx, new InputLayout(device, vspnctxbytes, VertexTypeGTAV.GetLayout(VertexType.DefaultEx)));

            texsampler = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.MinMagMipLinear,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });
            texsampleranis = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.Anisotropic,
                MaximumAnisotropy = 8,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });
            texsamplerflow = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.White,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.MinMagMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });

        }


        private void SetVertexShader(DeviceContext context, VertexType type)
        {
            VertexShader vs = vspnct;
            switch (type)
            {
                case VertexType.PT:
                    vs = vspt;
                    break;
                case VertexType.PCT:
                    vs = vspct;
                    break;
                case VertexType.Default:
                    vs = vspnct;
                    break;
                case VertexType.DefaultEx:
                    vs = vspnctx;
                    break;

                default:
                    break;

            }
            context.VertexShader.Set(vs);
        }

        public override void SetShader(DeviceContext context)
        {
            context.PixelShader.Set(Deferred ? psdef : ps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            InputLayout l;
            if (layouts.TryGetValue(type, out l))
            {
                SetVertexShader(context, type);
                context.InputAssembler.InputLayout = l;
                return true;
            }
            return false;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            uint rendermode = 0;
            uint rendermodeind = 1;

            SpecularEnable = lights.SpecularEnabled;

            switch (RenderMode)
            {
                case WorldRenderMode.VertexNormals:
                    rendermode = 1;
                    break;
                case WorldRenderMode.VertexTangents:
                    rendermode = 2;
                    break;
                case WorldRenderMode.VertexColour:
                    rendermode = 3;
                    rendermodeind = (uint)RenderVertexColourIndex;
                    break;
                case WorldRenderMode.TextureCoord:
                    rendermode = 4;
                    rendermodeind = (uint)RenderTextureCoordIndex;
                    break;
                case WorldRenderMode.SingleTexture:
                    rendermode = 8;//direct mode
                    break;
            }


            float phspd = 4.0f;
            float phspdi = 1.0f / phspd;
            float phspdh = phspd * 0.5f;
            float t = (float)(CurrentRealTime - (Math.Floor(CurrentRealTime * 0.001) * 1000.0))*1.0f;
            float ta = t * 2.0f;
            float tb = ta + (phspdh);
            float t1 = (ta * phspdi - (float)Math.Floor(ta * phspdi)) * phspd;
            float t2 = (tb * phspdi - (float)Math.Floor(tb * phspdi)) * phspd;
            float s1 = ((t1 < phspdh) ? t1 : phspd - t1) * phspdi * 1.0f;
            float s2 = ((t2 < phspdh) ? t2 : phspd - t2) * phspdi * 1.0f;
            //float s1 = ((float)Math.Cos(t1 * phspdi * Math.PI * 2) + 1.0f) * 0.5f;
            //float s2 = ((float)Math.Cos(t2 * phspdi * Math.PI * 2) + 1.0f) * 0.5f;

            float gFlowX = t1*0.5f;
            float gFlowY = t2*0.5f;
            float gFlowZ = s1;
            float gFlowW = s2;


            Vector2 fogtexMin = new Vector2(-4000.0f, -4000.0f); //aka water quads min/max
            Vector2 fogtexMax = new Vector2(4500.0f, 8000.0f);
            Vector2 fogtexInv = 1.0f / (fogtexMax - fogtexMin);


            bool usewaterbumps = (waterbump != null) && (waterbump.ShaderResourceView != null) && (waterbump2 != null) && (waterbump2.ShaderResourceView != null);
            bool usefogtex = (waterfog != null) && (waterfog.ShaderResourceView != null);

            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Vars.WaterVector = Vector4.Zero;
            VSSceneVars.Vars.ScaledTime = t * 0.1f;
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);

            PSSceneVars.Vars.GlobalLights = lights.Params;
            PSSceneVars.Vars.EnableShadows = (shadowmap != null) ? 1u : 0u;
            PSSceneVars.Vars.RenderMode = rendermode;
            PSSceneVars.Vars.RenderModeIndex = rendermodeind;
            PSSceneVars.Vars.RenderSamplerCoord = (uint)RenderTextureSamplerCoord;
            PSSceneVars.Vars.EnableWaterbumps = usewaterbumps ? 1u : 0u;
            PSSceneVars.Vars.EnableFogtex = usefogtex ? 1u : 0u;
            PSSceneVars.Vars.gFlowParams = new Vector4(gFlowX, gFlowY, gFlowZ, gFlowW);
            PSSceneVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            PSSceneVars.Vars.WaterFogParams = new Vector4(fogtexMin, fogtexInv.X, fogtexInv.Y);
            PSSceneVars.Update(context);
            PSSceneVars.SetPSCBuffer(context, 0);

            if (shadowmap != null)
            {
                shadowmap.SetFinalRenderResources(context);
            }
            if (usewaterbumps)
            {
                context.PixelShader.SetShaderResource(4, waterbump.ShaderResourceView);
                context.PixelShader.SetShaderResource(5, waterbump2.ShaderResourceView);
            }
            if (usefogtex)
            {
                context.PixelShader.SetShaderResource(6, waterfog.ShaderResourceView);
            }

        }

        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
            VSEntityVars.Vars.CamRel = new Vector4(rend.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = rend.Orientation;
            VSEntityVars.Vars.Scale = rend.Scale;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);
        }

        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            RenderableTexture texture = null;
            RenderableTexture bumptex = null;
            RenderableTexture flowtex = null;
            RenderableTexture foamtex = null;
            RenderableTexture fogtex = null;

            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
            {
                if (RenderMode == WorldRenderMode.Default)
                {
                    for (int i = 0; i < geom.RenderableTextures.Length; i++)
                    {
                        var itex = geom.RenderableTextures[i];
                        var ihash = geom.TextureParamHashes[i];
                        if (itex == null) continue;
                        switch (ihash)
                        {
                            case ShaderParamNames.DiffuseSampler:
                                texture = itex;
                                break;
                            case ShaderParamNames.BumpSampler:
                                bumptex = itex;
                                break;
                            case ShaderParamNames.FlowSampler:
                                flowtex = itex;
                                break;
                            case ShaderParamNames.FoamSampler:
                                foamtex = itex;
                                break;
                            case ShaderParamNames.FogSampler:
                                fogtex = itex;
                                break;
                            default:
                                if (texture == null) texture = itex;
                                break;
                        }
                    }
                }
                else if (RenderMode == WorldRenderMode.SingleTexture)
                {
                    for (int i = 0; i < geom.RenderableTextures.Length; i++)
                    {
                        var itex = geom.RenderableTextures[i];
                        var ihash = geom.TextureParamHashes[i];
                        if (ihash == RenderTextureSampler)
                        {
                            texture = itex;
                            break;
                        }
                    }
                }
            }


            bool usediff = ((texture != null) && (texture.ShaderResourceView != null));
            bool usebump = ((bumptex != null) && (bumptex.ShaderResourceView != null));
            bool useflow = ((flowtex != null) && (flowtex.ShaderResourceView != null));
            bool usefoam = ((foamtex != null) && (foamtex.ShaderResourceView != null));
            bool usefog = ((fogtex != null) && (fogtex.ShaderResourceView != null));

            uint shaderMode = 0;
            var shaderFile = geom.DrawableGeom.Shader.FileName;
            switch (shaderFile.Hash)
            {
                case 1529202445://{water_river.sps}
                case 4064804434://{water_riverlod.sps}
                case 2871265627://{water_riverocean.sps}
                case 1507348828://{water_rivershallow.sps}
                case 3945561843://{water_fountain.sps}
                case 4234404348://{water_shallow.sps}
                case 1077877097://{water_poolenv.sps}
                    break;
                case 1471966282://{water_decal.sps} (should this be in decal batch?)
                case 3053856997://{water_riverfoam.sps}
                    shaderMode = 1;
                    break;
                case 3066724854://{water_terrainfoam.sps}
                    shaderMode = 2;
                    break;
                default:
                    break;
            }


            PSGeomVars.Vars.EnableTexture = usediff ? 1u : 0u;
            PSGeomVars.Vars.EnableBumpMap = usebump ? 1u : 0u;
            PSGeomVars.Vars.EnableFoamMap = usefoam ? 1u : 0u;
            PSGeomVars.Vars.ShaderMode = shaderMode;
            PSGeomVars.Vars.SpecularIntensity = SpecularEnable ? 1.0f : 0.0f;// geom.specularIntensityMult;
            PSGeomVars.Vars.SpecularFalloff = 1.0f;// geom.specularFalloffMult;
            PSGeomVars.Vars.WaveOffset = geom.WaveOffset; //for terrainfoam
            PSGeomVars.Vars.WaterHeight = geom.WaterHeight; //for terrainfoam
            PSGeomVars.Vars.WaveMovement = geom.WaveMovement; //for terrainfoam
            PSGeomVars.Vars.HeightOpacity = geom.HeightOpacity; //for terrainfoam
            PSGeomVars.Vars.RippleSpeed = geom.RippleSpeed;
            PSGeomVars.Vars.RippleScale = geom.RippleScale;
            PSGeomVars.Vars.RippleBumpiness = geom.RippleBumpiness;
            PSGeomVars.Update(context);
            PSGeomVars.SetPSCBuffer(context, 2);

            VSGeomVars.Vars.EnableFlow = useflow ? 1u : 0u;
            VSGeomVars.Vars.ShaderMode = shaderMode;
            VSGeomVars.Vars.WaterParams = Vector4.Zero;
            VSGeomVars.Vars.RippleSpeed = geom.RippleSpeed;
            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 3);

            context.VertexShader.SetSampler(0, texsamplerflow);
            context.PixelShader.SetSampler(0, AnisotropicFilter ? texsampleranis : texsampler);
            if (usediff)
            {
                texture.SetPSResource(context, 0);
            }
            if (usebump)
            {
                bumptex.SetPSResource(context, 2);
            }
            if (usefoam)
            {
                foamtex.SetPSResource(context, 3);
            }
            if (useflow)
            {
                flowtex.SetVSResource(context, 0);
            }
        }


        public void RenderWaterQuad(DeviceContext context, RenderableWaterQuad quad)
        {
            SetInputLayout(context, VertexType.PCT);

            VSEntityVars.Vars.CamRel = new Vector4(quad.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = Quaternion.Identity;
            VSEntityVars.Vars.Scale = Vector3.One;
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);


            PSGeomVars.Vars.EnableTexture = 0;
            PSGeomVars.Vars.EnableBumpMap = 0;
            PSGeomVars.Vars.EnableFoamMap = 0;
            PSGeomVars.Vars.ShaderMode = 0; //normal water render
            PSGeomVars.Vars.SpecularIntensity = SpecularEnable ? 4.2f : 0.0f;// geom.specularIntensityMult;
            PSGeomVars.Vars.SpecularFalloff = 1600.0f;// geom.specularFalloffMult;
            PSGeomVars.Vars.WaveOffset = 0.0f;// geom.WaveOffset; //for terrainfoam
            PSGeomVars.Vars.WaterHeight = 0.0f;// geom.WaterHeight; //for terrainfoam
            PSGeomVars.Vars.WaveMovement = 0.0f;// geom.WaveMovement; //for terrainfoam
            PSGeomVars.Vars.HeightOpacity = 0.0f;// geom.HeightOpacity; //for terrainfoam
            PSGeomVars.Vars.RippleSpeed = 2.1f;// geom.RippleSpeed;
            PSGeomVars.Vars.RippleScale = 0.02f;// geom.RippleScale;
            PSGeomVars.Vars.RippleBumpiness = 4.5f;// geom.RippleBumpiness;
            PSGeomVars.Update(context);
            PSGeomVars.SetPSCBuffer(context, 2);

            VSGeomVars.Vars.EnableFlow = 0; //no flow for water quads...
            VSGeomVars.Vars.ShaderMode = 0; //normal water
            VSGeomVars.Vars.WaterParams = Vector4.Zero;
            VSGeomVars.Vars.RippleSpeed = 1.0f;// geom.RippleSpeed;
            VSGeomVars.Update(context);
            VSGeomVars.SetVSCBuffer(context, 3);

            context.VertexShader.SetSampler(0, texsamplerflow);
            context.PixelShader.SetSampler(0, AnisotropicFilter ? texsampleranis : texsampler);

            quad.Render(context);
        }


        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null); //shadowmap
            context.PixelShader.SetConstantBuffer(1, null); //shadowmap
            context.PixelShader.SetShaderResource(1, null);//shadowmap
            context.PixelShader.SetSampler(1, null); //shadowmap
            context.VertexShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(3, null);
            context.PixelShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(4, null);
            context.VertexShader.SetConstantBuffer(5, null);
            context.VertexShader.SetConstantBuffer(6, null);
            context.VertexShader.SetSampler(0, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.PixelShader.SetShaderResource(2, null);
            context.PixelShader.SetShaderResource(3, null);
            context.PixelShader.SetShaderResource(4, null);
            context.PixelShader.SetShaderResource(5, null);
            context.VertexShader.SetShaderResource(0, null);
            context.VertexShader.SetShaderResource(1, null);
            context.VertexShader.SetShaderResource(2, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }

        public void Dispose()
        {
            if (disposed) return;

            texsampler.Dispose();
            texsampleranis.Dispose();
            texsamplerflow.Dispose();

            foreach (InputLayout layout in layouts.Values)
            {
                layout.Dispose();
            }
            layouts.Clear();

            VSSceneVars.Dispose();
            VSEntityVars.Dispose();
            VSGeomVars.Dispose();
            PSSceneVars.Dispose();
            PSGeomVars.Dispose();

            ps.Dispose();
            psdef.Dispose();
            vspt.Dispose();
            vspct.Dispose();
            vspnct.Dispose();
            vspnctx.Dispose();

            disposed = true;

        }
    }
}
