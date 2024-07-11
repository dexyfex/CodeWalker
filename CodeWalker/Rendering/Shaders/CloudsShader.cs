using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX.Direct3D11;
using System.IO;
using SharpDX;

namespace CodeWalker.Rendering
{
    public struct CloudsShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Matrix ViewInv;
    }
    public struct CloudsShaderVSEntityVars
    {
        public Vector4 CamRel;
        public Quaternion Orientation;
        public Vector4 Scale;
    }
    public struct CloudsShaderVSModelVars
    {
        public Matrix Transform;
    }
    public struct CloudsShaderPSSceneVars
    {
        public Vector4 LightDirection;
        public uint EnableHDR;
        public uint Pad0;
        public uint Pad1;
        public uint Pad2;
    }
    public struct CloudsShaderCloudsLocals
    {
        public Vector3 gSkyColor;                  // Offset:    0 Size:    12 [unused]
        public float gPad00;
        public Vector3 gEastMinusWestColor;        // Offset:   16 Size:    12 [unused]
        public float gPad01;
        public Vector3 gWestColor;                 // Offset:   32 Size:    12 [unused]
        public float gPad02;
        public Vector3 gSunDirection;              // Offset:   48 Size:    12
        public float gPad03;
        public Vector3 gSunColor;                  // Offset:   64 Size:    12
        public float gPad04;
        public Vector3 gCloudColor;                // Offset:   80 Size:    12 [unused]
        public float gPad05;
        public Vector3 gAmbientColor;              // Offset:   96 Size:    12 [unused]
        public float gPad06;
        public Vector3 gBounceColor;               // Offset:  112 Size:    12 [unused]
        public float gPad07;
        public Vector4 gDensityShiftScale;         // Offset:  128 Size:    16 [unused]
        public Vector4 gScatterG_GSquared_PhaseMult_Scale;// Offset:  144 Size:    16
        public Vector4 gPiercingLightPower_Strength_NormalStrength_Thickness;// Offset:  160 Size:    16
        public Vector3 gScaleDiffuseFillAmbient;   // Offset:  176 Size:    12 [unused]
        public float gPad08;
        public Vector3 gWrapLighting_MSAARef;      // Offset:  192 Size:    12 [unused]
        public float gPad09;
        public Vector4 gNearFarQMult;              // Offset:  208 Size:    16 [unused]
        public Vector3 gAnimCombine;               // Offset:  224 Size:    12 [unused]
        public float gPad10;
        public Vector3 gAnimSculpt;                // Offset:  240 Size:    12 [unused]
        public float gPad11;
        public Vector3 gAnimBlendWeights;          // Offset:  256 Size:    12 [unused]
        public float gPad12;
        public Vector4 gUVOffsetArr0;               // Offset:  272 Size:    32
        public Vector4 gUVOffsetArr1;               // Offset:  272 Size:    32
        public Matrix gCloudViewProj; // Offset:  304 Size:    64
        public Vector4 gCameraPos;                 // Offset:  368 Size:    16
        public Vector2 gUVOffset1;                 // Offset:  384 Size:     8
        public Vector2 gUVOffset2;                 // Offset:  392 Size:     8
        public Vector2 gUVOffset3;                 // Offset:  400 Size:     8
        public Vector2 gRescaleUV1;                // Offset:  408 Size:     8
        public Vector2 gRescaleUV2;                // Offset:  416 Size:     8
        public Vector2 gRescaleUV3;                // Offset:  424 Size:     8
        public float gSoftParticleRange;          // Offset:  432 Size:     4 [unused]
        public float gEnvMapAlphaScale;           // Offset:  436 Size:     4 [unused]
        public Vector2 cloudLayerAnimScale1;       // Offset:  440 Size:     8
        public Vector2 cloudLayerAnimScale2;       // Offset:  448 Size:     8
        public Vector2 cloudLayerAnimScale3;       // Offset:  456 Size:     8
    };


    public class CloudsShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader vs;
        PixelShader ps;

        GpuVarsBuffer<CloudsShaderCloudsLocals> CloudsLocalVars;
        GpuVarsBuffer<CloudsShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<CloudsShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<CloudsShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<CloudsShaderPSSceneVars> PSSceneVars;

        SamplerState texsampler;
        SamplerState texsampleranis;
        InputLayout layout;

        public bool EnableHDR { get; set; }
        public bool AnisotropicFilter = false;

        public CloudsShader(Device device)
        {
            byte[] vsbytes = PathUtil.ReadAllBytes("Shaders\\CloudsVS.cso");
            byte[] psbytes = PathUtil.ReadAllBytes("Shaders\\CloudsPS.cso");

            vs = new VertexShader(device, vsbytes);
            ps = new PixelShader(device, psbytes);

            CloudsLocalVars = new GpuVarsBuffer<CloudsShaderCloudsLocals>(device);
            VSSceneVars = new GpuVarsBuffer<CloudsShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<CloudsShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<CloudsShaderVSModelVars>(device);
            PSSceneVars = new GpuVarsBuffer<CloudsShaderPSSceneVars>(device);

            layout = new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.DefaultEx));

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
        }

        public void UpdateCloudsLocals(Clouds clouds, ShaderGlobalLights lights)
        {

            CloudsLocalVars.Vars.gSunDirection = lights.CurrentSunDir;
            CloudsLocalVars.Vars.gSunColor = ((Vector4)lights.Params.LightDirColour).XYZ();
            CloudsLocalVars.Vars.gUVOffset1 = clouds.AnimOverrides.UVOffset1;
            CloudsLocalVars.Vars.gUVOffset2 = clouds.AnimOverrides.UVOffset2;
            CloudsLocalVars.Vars.gUVOffset3 = clouds.AnimOverrides.UVOffset3;
            CloudsLocalVars.Vars.gRescaleUV1 = clouds.AnimOverrides.RescaleUV1;
            CloudsLocalVars.Vars.gRescaleUV2 = clouds.AnimOverrides.RescaleUV2;
            CloudsLocalVars.Vars.gRescaleUV3 = clouds.AnimOverrides.RescaleUV3;
            CloudsLocalVars.Vars.cloudLayerAnimScale1 = clouds.AnimOverrides.cloudLayerAnimScale1;
            CloudsLocalVars.Vars.cloudLayerAnimScale2 = clouds.AnimOverrides.cloudLayerAnimScale2;
            CloudsLocalVars.Vars.cloudLayerAnimScale3 = clouds.AnimOverrides.cloudLayerAnimScale3;
            CloudsLocalVars.Vars.gUVOffsetArr0 = Vector4.Zero;
            CloudsLocalVars.Vars.gUVOffsetArr1 = Vector4.Zero;


        }

        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(vs);
            context.PixelShader.Set(ps);
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            CloudsLocalVars.Update(context);
            CloudsLocalVars.SetVSCBuffer(context, 0);
            CloudsLocalVars.SetPSCBuffer(context, 0);

            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Vars.ViewInv = Matrix.Transpose(camera.ViewInvMatrix);
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 1);

            PSSceneVars.Vars.LightDirection = new Vector4(lights.Params.LightDir, 0.0f);
            PSSceneVars.Vars.EnableHDR = EnableHDR ? 1u : 0u;
            PSSceneVars.Update(context);
            PSSceneVars.SetPSCBuffer(context, 1);
        }

        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
            VSEntityVars.Vars.CamRel = new Vector4(rend.CamRel, 0.0f);
            VSEntityVars.Vars.Orientation = rend.Orientation;
            VSEntityVars.Vars.Scale = new Vector4(rend.Scale, 1.0f);
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);
        }

        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
            //currently not used..
            //if (!model.UseTransform) return;
            //VSModelVars.Vars.Transform = Matrix.Transpose(model.Transform);
            //VSModelVars.Update(context);
            //VSModelVars.SetVSCBuffer(context, 3);
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {

            switch (geom.DrawableGeom.Shader.FileName.Hash)
            {
                case 4103916155://{clouds_animsoft.sps}
                case 1097000161://{clouds_altitude.sps}
                case 1481470665://{clouds_soft.sps}
                case 2184108982://{clouds_fast.sps}
                case 4192928948://{clouds_anim.sps}
                    break;
                default:
                    break;
            }

            RenderableTexture DensitySampler = null;
            RenderableTexture NormalSampler = null;
            RenderableTexture DetailDensitySampler = null;
            RenderableTexture DetailNormalSampler = null;
            RenderableTexture DetailDensity2Sampler = null;
            RenderableTexture DetailNormal2Sampler = null;
            RenderableTexture DepthMapTexSampler = null;


            if ((geom.RenderableTextures != null) && (geom.RenderableTextures.Length > 0))
            {
                for (int i = 0; i < geom.RenderableTextures.Length; i++)
                {
                    var itex = geom.RenderableTextures[i];
                    var ihash = geom.TextureParamHashes[i];
                    switch (ihash)
                    {
                        case ShaderParamNames.DensitySampler:
                            DensitySampler = itex;
                            break;
                        case ShaderParamNames.normalSampler:
                            NormalSampler = itex;
                            break;
                        case ShaderParamNames.DetailDensitySampler:
                            DetailDensitySampler = itex;
                            break;
                        case ShaderParamNames.DetailNormalSampler:
                            DetailNormalSampler = itex;
                            break;
                        case ShaderParamNames.DetailDensity2Sampler:
                            DetailDensity2Sampler = itex;
                            break;
                        case ShaderParamNames.DetailNormal2Sampler:
                            DetailNormal2Sampler = itex;
                            break;
                        case ShaderParamNames.DepthMapTexSampler:
                            DepthMapTexSampler = itex;
                            break;
                        default:
                            break;
                    }
                }
            }

            bool usedens = ((DensitySampler != null) && (DensitySampler.ShaderResourceView != null));
            bool usenorm = ((NormalSampler != null) && (NormalSampler.ShaderResourceView != null));
            bool usedden = ((DetailDensitySampler != null) && (DetailDensitySampler.ShaderResourceView != null));
            bool usednrm = ((DetailNormalSampler != null) && (DetailNormalSampler.ShaderResourceView != null));
            bool useddn2 = ((DetailDensity2Sampler != null) && (DetailDensity2Sampler.ShaderResourceView != null));
            bool usednm2 = ((DetailNormal2Sampler != null) && (DetailNormal2Sampler.ShaderResourceView != null));
            bool usedept = ((DepthMapTexSampler != null) && (DepthMapTexSampler.ShaderResourceView != null));

            if (usedens)
            {
                DensitySampler.SetPSResource(context, 0);
            }
            if (usenorm)
            {
                NormalSampler.SetPSResource(context, 1);
            }
            if (usedden)
            {
                DetailDensitySampler.SetPSResource(context, 2);
            }
            if (usednrm)
            {
                DetailNormalSampler.SetPSResource(context, 3);
            }
            if (useddn2)
            {
                DetailDensity2Sampler.SetPSResource(context, 4);
            }
            if (usednm2)
            {
                DetailNormal2Sampler.SetPSResource(context, 5);
            }
            if (usedept)
            {
                DepthMapTexSampler.SetPSResource(context, 6);
            }

            context.PixelShader.SetSampler(0, AnisotropicFilter ? texsampleranis : texsampler);


        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            if (type != VertexType.DefaultEx)
            {
                return false;
            }
            context.InputAssembler.InputLayout = layout;
            return true;
        }

        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null);
            context.PixelShader.SetConstantBuffer(1, null);
            context.VertexShader.SetConstantBuffer(2, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;


            texsampler.Dispose();
            texsampleranis.Dispose();

            layout.Dispose();

            CloudsLocalVars.Dispose();
            VSSceneVars.Dispose();
            VSEntityVars.Dispose();
            VSModelVars.Dispose();
            PSSceneVars.Dispose();

            ps.Dispose();
            vs.Dispose();
        }
    }
}
