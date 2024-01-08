using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D11;
using System.IO;
using CodeWalker.GameFiles;
using CodeWalker.World;
using System.Diagnostics;

namespace CodeWalker.Rendering
{

    public struct SkydomeShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Matrix ViewInv;
    }
    public struct SkydomeShaderVSEntityVars
    {
        public Vector4 CamRel;
        public Quaternion Orientation;
    }
    public struct SkydomeShaderVSModelVars
    {
        public Matrix Transform;
    }
    public struct SkydomeShaderPSSceneVars
    {
        public Vector4 LightDirection;
        public uint EnableHDR;
        public uint Pad0;
        public uint Pad1;
        public uint Pad2;
    }

    public struct SkydomeShaderVSSunMoonVars
    {
        public Matrix ViewProj;
        public Matrix ViewInv;
        public Vector4 CamRel;
        public Vector2 Size;
        public Vector2 Offset;
    }
    public struct SkydomeShaderPSSunMoonVars
    {
        public Vector4 Colour;
    }


    public struct SkydomeShaderSkySystemLocals
    {
        public Vector4 azimuthEastColor;           // Offset:    0 Size:    12 
        public Vector4 azimuthWestColor;           // Offset:   16 Size:    12 
        public Vector3 azimuthTransitionColor;     // Offset:   32 Size:    12 
        public float azimuthTransitionPosition;    // Offset:   44 Size:     4 
        public Vector4 zenithColor;                // Offset:   48 Size:    12 
        public Vector4 zenithTransitionColor;      // Offset:   64 Size:    12 
        public Vector4 zenithConstants;            // Offset:   80 Size:    16 
        public Vector4 skyPlaneColor;              // Offset:   96 Size:    16 [unused]
        public Vector4 skyPlaneParams;             // Offset:  112 Size:    16 [unused]
        public float hdrIntensity;                 // Offset:  128 Size:     4 
        public Vector3 sunColor;                   // Offset:  132 Size:    12
        public Vector4 sunColorHdr;                // Offset:  144 Size:    12
        public Vector4 sunDiscColorHdr;            // Offset:  160 Size:    12 [unused]
        public Vector4 sunConstants;               // Offset:  176 Size:    16
        public Vector4 sunDirection;               // Offset:  192 Size:    12
        public Vector4 sunPosition;                // Offset:  208 Size:    12 [unused]
        public Vector4 cloudBaseMinusMidColour;    // Offset:  224 Size:    12
        public Vector4 cloudMidColour;             // Offset:  240 Size:    12
        public Vector4 cloudShadowMinusBaseColourTimesShadowStrength;// Offset:  256 Size:    12
        public Vector4 cloudDetailConstants;       // Offset:  272 Size:    16
        public Vector4 cloudConstants1;            // Offset:  288 Size:    16
        public Vector4 cloudConstants2;            // Offset:  304 Size:    16
        public Vector4 smallCloudConstants;        // Offset:  320 Size:    16
        public Vector4 smallCloudColorHdr;         // Offset:  336 Size:    12
        public Vector4 effectsConstants;           // Offset:  352 Size:    16
        public float horizonLevel;                 // Offset:  368 Size:     4 
        public Vector3 speedConstants;             // Offset:  372 Size:    12 
        public float starfieldIntensity;           // Offset:  384 Size:     4
        public Vector3 moonDirection;              // Offset:  388 Size:    12
        public Vector3 moonPosition;               // Offset:  400 Size:    12 [unused]
        public float moonIntensity;                // Offset:  412 Size:     4 [unused]
        public Vector4 lunarCycle;                 // Offset:  416 Size:    12
        public Vector3 moonColor;                  // Offset:  432 Size:    12
        public float noiseFrequency;               // Offset:  444 Size:     4 [unused]
        public float noiseScale;                   // Offset:  448 Size:     4 [unused]
        public float noiseThreshold;               // Offset:  452 Size:     4 [unused]
        public float noiseSoftness;                // Offset:  456 Size:     4 [unused]
        public float noiseDensityOffset;           // Offset:  460 Size:     4 [unused]
        public Vector4 noisePhase;                 // Offset:  464 Size:     8 [unused]
    }


    public class SkydomeShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader skyvs;
        PixelShader skyps;
        VertexShader sunvs;
        PixelShader sunps;
        VertexShader moonvs;
        PixelShader moonps;

        GpuVarsBuffer<SkydomeShaderSkySystemLocals> SkyLocalVars;
        GpuVarsBuffer<SkydomeShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<SkydomeShaderVSEntityVars> VSEntityVars;
        GpuVarsBuffer<SkydomeShaderVSModelVars> VSModelVars;
        GpuVarsBuffer<SkydomeShaderPSSceneVars> PSSceneVars;

        GpuVarsBuffer<SkydomeShaderVSSunMoonVars> VSSunMoonVars;
        GpuVarsBuffer<SkydomeShaderPSSunMoonVars> PSSunMoonVars;


        SamplerState texsampler;

        InputLayout skylayout;
        InputLayout sunlayout;
        InputLayout moonlayout;

        UnitDisc sundisc;
        UnitQuad moonquad;

        public bool EnableHDR { get; set; }


        public SkydomeShader(Device device)
        {
            string folder = ShaderManager.GetShaderFolder();
            byte[] skyvsbytes = File.ReadAllBytes(Path.Combine(folder, "SkydomeVS.cso"));
            byte[] skypsbytes = File.ReadAllBytes(Path.Combine(folder, "SkydomePS.cso"));
            byte[] sunvsbytes = File.ReadAllBytes(Path.Combine(folder, "SkySunVS.cso"));
            byte[] sunpsbytes = File.ReadAllBytes(Path.Combine(folder, "SkySunPS.cso"));
            byte[] moonvsbytes = File.ReadAllBytes(Path.Combine(folder, "SkyMoonVS.cso"));
            byte[] moonpsbytes = File.ReadAllBytes(Path.Combine(folder, "SkyMoonPS.cso"));

            skyvs = new VertexShader(device, skyvsbytes);
            skyps = new PixelShader(device, skypsbytes);
            sunvs = new VertexShader(device, sunvsbytes);
            sunps = new PixelShader(device, sunpsbytes);
            moonvs = new VertexShader(device, moonvsbytes);
            moonps = new PixelShader(device, moonpsbytes);

            SkyLocalVars = new GpuVarsBuffer<SkydomeShaderSkySystemLocals>(device);
            VSSceneVars = new GpuVarsBuffer<SkydomeShaderVSSceneVars>(device);
            VSEntityVars = new GpuVarsBuffer<SkydomeShaderVSEntityVars>(device);
            VSModelVars = new GpuVarsBuffer<SkydomeShaderVSModelVars>(device);
            PSSceneVars = new GpuVarsBuffer<SkydomeShaderPSSceneVars>(device);

            VSSunMoonVars = new GpuVarsBuffer<SkydomeShaderVSSunMoonVars>(device);
            PSSunMoonVars = new GpuVarsBuffer<SkydomeShaderPSSunMoonVars>(device);

            sundisc = new UnitDisc(device, 30, true);
            sunlayout = new InputLayout(device, sunvsbytes, sundisc.GetLayout());
            skylayout = new InputLayout(device, skyvsbytes, VertexTypeGTAV.GetLayout(VertexType.PTT));

            moonquad = new UnitQuad(device, true);
            moonlayout = new InputLayout(device, moonvsbytes, moonquad.GetLayout());

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

        }



        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(skyvs);
            context.PixelShader.Set(skyps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            if (type != VertexType.PTT)
            {
                return false;
            }
            context.InputAssembler.InputLayout = skylayout;
            return true;
        }

        public void UpdateSkyLocals(Weather weather, ShaderGlobalLights lights)
        {
            var wv = weather.CurrentValues;

            float skyhdr = EnableHDR ? wv.skyHdr : Math.Min(wv.skyHdr, 1.8f);
            float sunhdr = EnableHDR ? wv.skySunHdr : Math.Min(wv.skySunHdr, 4.8f); //NO HDR - turn it down!!!
            float scathdr = EnableHDR ? wv.skySunScatterInten : Math.Min(wv.skySunScatterInten, 1.8f);
            Vector3 suncolhdr = wv.skySunCol * sunhdr;
            Vector4 azecol = wv.skyAzimuthEastCol;
            Vector4 azwcol = wv.skyAzimuthWestCol;
            Vector4 aztcol = wv.skyAzimuthTransitionCol;
            Vector4 zencol = wv.skyZenithCol;
            Vector4 zetcol = wv.skyZenithTransitionCol;
            Vector4 plncol = wv.skyPlane;

            SkyLocalVars.Vars.azimuthEastColor = azecol;
            SkyLocalVars.Vars.azimuthWestColor = azwcol;
            SkyLocalVars.Vars.azimuthTransitionColor = aztcol.XYZ();
            SkyLocalVars.Vars.azimuthTransitionPosition = wv.skyAzimuthTransition;
            SkyLocalVars.Vars.zenithColor = zencol;
            SkyLocalVars.Vars.zenithTransitionColor = zetcol;
            SkyLocalVars.Vars.zenithConstants = wv.skyZenithTransition;
            SkyLocalVars.Vars.skyPlaneColor = plncol;
            SkyLocalVars.Vars.skyPlaneParams = Vector4.Zero;
            SkyLocalVars.Vars.hdrIntensity = skyhdr;
            SkyLocalVars.Vars.sunColor = wv.skySunCol;
            SkyLocalVars.Vars.sunColorHdr = new Vector4(suncolhdr, sunhdr);
            SkyLocalVars.Vars.sunDiscColorHdr = new Vector4(wv.skySunDiscCol, wv.skySunDiscSize);
            //SkyLocalVars.Vars.sunConstants = new Vector4(wv.skySunMie, scathdr);
            //SkyLocalVars.Vars.sunConstants.X = wv.skySunMie.X; //mie phase
            //SkyLocalVars.Vars.sunConstants.Y = wv.skySunMie.Y; //mie scatter
            //SkyLocalVars.Vars.sunConstants.Z = 0.5f / wv.skySunInfluenceRadius;// * 0.01f; // 0.0025f;  //mie size/"range"
            //SkyLocalVars.Vars.sunConstants.W = wv.skySunMie.Z;// * scathdr; //mie hdr intensity
            SkyLocalVars.Vars.sunConstants.X = wv.skySunMie.X * wv.skySunMie.Y; //mie phase
            SkyLocalVars.Vars.sunConstants.Y = wv.skySunMie.Y;// 1.0f /wv.skySunMie.Y;// 1.7f;// wv.skySunMie.Y; //mie scatter
            SkyLocalVars.Vars.sunConstants.Z = 0.00003f * wv.skySunInfluenceRadius;// * wv.skySunMie.X;// wv.skySunMie.Z;///720.0f;// * 0.01f; // 0.0025f;  //mie size/"range"
            SkyLocalVars.Vars.sunConstants.W = wv.skySunMie.Z;// * scathdr; //mie intensity multiplier
            SkyLocalVars.Vars.sunDirection = new Vector4(-lights.CurrentSunDir, 0);// wv.sunDirection, 0);
            SkyLocalVars.Vars.sunPosition = new Vector4(-lights.CurrentSunDir, 0); //not used
            SkyLocalVars.Vars.cloudBaseMinusMidColour = Vector4.Zero;
            SkyLocalVars.Vars.cloudMidColour = Vector4.Zero;
            SkyLocalVars.Vars.cloudShadowMinusBaseColourTimesShadowStrength = Vector4.Zero;
            SkyLocalVars.Vars.cloudDetailConstants = Vector4.Zero;
            SkyLocalVars.Vars.cloudConstants1 = Vector4.Zero;
            SkyLocalVars.Vars.cloudConstants2 = Vector4.Zero;
            SkyLocalVars.Vars.smallCloudConstants = Vector4.Zero;
            SkyLocalVars.Vars.smallCloudColorHdr = Vector4.Zero;
            SkyLocalVars.Vars.effectsConstants = Vector4.Zero;
            SkyLocalVars.Vars.horizonLevel = 0;
            SkyLocalVars.Vars.speedConstants = Vector3.Zero;
            SkyLocalVars.Vars.starfieldIntensity = wv.skyStarsIten * 5.0f; //makes stars brighter....
            SkyLocalVars.Vars.moonDirection = wv.moonDirection;
            SkyLocalVars.Vars.moonPosition = Vector3.Zero;//need to update this?
            SkyLocalVars.Vars.moonIntensity = wv.skyMoonIten;
            SkyLocalVars.Vars.lunarCycle = Vector4.Zero;
            SkyLocalVars.Vars.moonColor = wv.skyMoonCol;
            SkyLocalVars.Vars.noiseFrequency = 0;
            SkyLocalVars.Vars.noiseScale = 0;
            SkyLocalVars.Vars.noiseThreshold = 0;
            SkyLocalVars.Vars.noiseSoftness = 0;
            SkyLocalVars.Vars.noiseDensityOffset = 0;
            SkyLocalVars.Vars.noisePhase = Vector4.Zero;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap? shadowmap, ShaderGlobalLights lights)
        {
            SkyLocalVars.Update(context);
            SkyLocalVars.SetVSCBuffer(context, 0);
            SkyLocalVars.SetPSCBuffer(context, 0);

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
            VSEntityVars.Update(context);
            VSEntityVars.SetVSCBuffer(context, 2);
        }

        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
            if (!model.UseTransform)
                return;
            VSModelVars.Vars.Transform = Matrix.Transpose(model.Transform);
            VSModelVars.Update(context);
            VSModelVars.SetVSCBuffer(context, 3);
        }

        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            //don't use this version
        }

        public void SetTextures(DeviceContext context, RenderableTexture starfield)
        {
            if (starfield != null)
            {
                context.PixelShader.SetSampler(0, texsampler);
                starfield.SetPSResource(context, 0);
            }
        }

        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null);
            context.PixelShader.SetConstantBuffer(1, null);
            context.VertexShader.SetConstantBuffer(2, null);
            context.VertexShader.SetConstantBuffer(3, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }



        public void RenderSun(DeviceContext context, Camera camera, Weather weather, ShaderGlobalLights lights)
        {

            context.VertexShader.Set(sunvs);
            context.PixelShader.Set(sunps);

            VSSunMoonVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSunMoonVars.Vars.ViewInv = Matrix.Transpose(camera.ViewInvMatrix);
            VSSunMoonVars.Vars.CamRel = new Vector4(lights.CurrentSunDir, 0);
            VSSunMoonVars.Vars.Size = new Vector2(weather.CurrentValues.skySunDiscSize * 0.008f);
            VSSunMoonVars.Vars.Offset = Vector2.Zero;
            VSSunMoonVars.Update(context);
            VSSunMoonVars.SetVSCBuffer(context, 0);

            PSSunMoonVars.Vars.Colour = new Vector4(weather.CurrentValues.skySunDiscCol * weather.CurrentValues.skySunHdr, 1);
            PSSunMoonVars.Update(context);
            PSSunMoonVars.SetPSCBuffer(context, 0);

            context.InputAssembler.InputLayout = sunlayout;
            sundisc.Draw(context);
        }

        public void RenderMoon(DeviceContext context, Camera camera, Weather weather, ShaderGlobalLights lights, RenderableTexture moontex)
        {
            context.VertexShader.Set(moonvs);
            context.PixelShader.Set(moonps);


            Quaternion ori = Quaternion.Invert(Quaternion.LookAtRH(Vector3.Zero, lights.CurrentMoonDir, lights.MoonAxis));
            Matrix omat = ori.ToMatrix();


            VSSunMoonVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSunMoonVars.Vars.ViewInv = Matrix.Transpose(omat);// camera.ViewInvMatrix);
            VSSunMoonVars.Vars.CamRel = new Vector4(lights.CurrentMoonDir, 0);
            VSSunMoonVars.Vars.Size = new Vector2(weather.CurrentValues.skyMoonDiscSize * 0.008f);
            VSSunMoonVars.Vars.Offset = Vector2.Zero;
            VSSunMoonVars.Update(context);
            VSSunMoonVars.SetVSCBuffer(context, 0);

            PSSunMoonVars.Vars.Colour = new Vector4(weather.CurrentValues.skyMoonCol * weather.CurrentValues.skyMoonIten, weather.CurrentValues.skyMoonIten);
            PSSunMoonVars.Update(context);
            PSSunMoonVars.SetPSCBuffer(context, 0);

            context.PixelShader.SetSampler(0, texsampler);
            moontex.SetPSResource(context, 0);

            context.InputAssembler.InputLayout = moonlayout;
            moonquad.Draw(context);
        }




        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            texsampler.Dispose();

            sundisc.Dispose();
            moonquad.Dispose();

            skylayout.Dispose();
            sunlayout.Dispose();
            moonlayout.Dispose();

            VSSunMoonVars.Dispose();
            PSSunMoonVars.Dispose();

            SkyLocalVars.Dispose();
            VSSceneVars.Dispose();
            VSEntityVars.Dispose();
            VSModelVars.Dispose();
            PSSceneVars.Dispose();

            skyps.Dispose();
            skyvs.Dispose();
            sunps.Dispose();
            sunvs.Dispose();
            moonps.Dispose();
            moonvs.Dispose();
        }
    }
}
