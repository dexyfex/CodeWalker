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
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace CodeWalker.Rendering
{

    public struct DeferredLightVSVars
    {
        public Matrix ViewProj;
        public Vector4 CameraPos;
        public uint LightType; //0=directional, 1=Point, 2=Spot, 4=Capsule
        public uint IsLOD; //useful or not?
        public uint Pad0;
        public uint Pad1;
    }
    public struct DeferredLightPSVars
    {
        public ShaderGlobalLightParams GlobalLights;
        public Matrix ViewProjInv;
        public Vector4 CameraPos;
        public uint EnableShadows;
        public uint RenderMode;//0=default, 1=normals, 2=tangents, 3=colours, 4=texcoords, 5=diffuse, 6=normalmap, 7=spec, 8=direct
        public uint RenderModeIndex; //colour/texcoord index
        public uint RenderSamplerCoord; //which texcoord to use in single texture mode
        public uint LightType; //0=directional, 1=Point, 2=Spot, 4=Capsule
        public uint IsLOD; //useful or not?
        public uint Pad0;
        public uint Pad1;
    }

    public struct DeferredMSAAPSVars
    {
        public uint SampleCount;
        public float SampleMult;
        public float TexelSizeX;
        public float TexelSizeY;
    }


    public class DeferredScene
    {

        public GpuMultiTexture GBuffers; // diffuse, normals, specular, irradiance
        public GpuTexture SceneColour; //final scene colour buffer

        SamplerState SampleStatePoint;
        SamplerState SampleStateLinear;
        BlendState BlendState;
        long WindowSizeVramUsage = 0;
        int Width = 0;
        int Height = 0;
        ViewportF Viewport;

        VertexShader LightVS;
        PixelShader LightPS;
        UnitCone LightCone;
        UnitSphere LightSphere;
        UnitCapsule LightCapsule;
        UnitQuad LightQuad;
        InputLayout LightQuadLayout;


        GpuVarsBuffer<DeferredLightVSVars> LightVSVars;
        GpuVarsBuffer<DeferredLightPSVars> LightPSVars;


        int MSAASampleCount = 1;

        VertexShader FinalVS;
        PixelShader MSAAPS;

        GpuVarsBuffer<DeferredMSAAPSVars> MSAAPSVars;


        public long VramUsage
        {
            get
            {
                return WindowSizeVramUsage;
            }
        }



        public DeferredScene(DXManager dxman)
        {
            var device = dxman.device;

            byte[] bLightVS = File.ReadAllBytes("Shaders\\LightVS.cso");
            byte[] bLightPS = File.ReadAllBytes("Shaders\\LightPS.cso");
            byte[] bFinalVS = File.ReadAllBytes("Shaders\\PPFinalPassVS.cso");
            byte[] bMSAAPS = File.ReadAllBytes("Shaders\\PPMSAAPS.cso");

            LightVS = new VertexShader(device, bLightVS);
            LightPS = new PixelShader(device, bLightPS);
            LightCone = new UnitCone(device, bLightVS, 4, false);
            LightSphere = new UnitSphere(device, bLightVS, 4, true);
            LightCapsule = new UnitCapsule(device, bLightVS, 4, false);
            LightQuad = new UnitQuad(device, true);
            LightQuadLayout = new InputLayout(device, bLightVS, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
            });

            LightVSVars = new GpuVarsBuffer<DeferredLightVSVars>(device);
            LightPSVars = new GpuVarsBuffer<DeferredLightPSVars>(device);


            FinalVS = new VertexShader(device, bFinalVS);
            MSAAPS = new PixelShader(device, bMSAAPS);

            MSAAPSVars = new GpuVarsBuffer<DeferredMSAAPSVars>(device);

            TextureAddressMode a = TextureAddressMode.Clamp;
            Color4 b = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            Comparison c = Comparison.Always;
            SampleStatePoint = DXUtility.CreateSamplerState(device, a, b, c, Filter.MinMagMipPoint, 0, 1.0f, 1.0f, 0.0f);
            SampleStateLinear = DXUtility.CreateSamplerState(device, a, b, c, Filter.MinMagMipLinear, 0, 1.0f, 1.0f, 0.0f);

            BlendState = DXUtility.CreateBlendState(device, false, BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, BlendOption.One, BlendOption.Zero, ColorWriteMaskFlags.All);

        }
        public void Dispose()
        {
            DisposeBuffers();

            if (BlendState != null)
            {
                BlendState.Dispose();
                BlendState = null;
            }
            if (SampleStateLinear != null)
            {
                SampleStateLinear.Dispose();
                SampleStateLinear = null;
            }
            if (SampleStatePoint != null)
            {
                SampleStatePoint.Dispose();
                SampleStatePoint = null;
            }
            if (LightVSVars != null)
            {
                LightVSVars.Dispose();
                LightVSVars = null;
            }
            if (LightPSVars != null)
            {
                LightPSVars.Dispose();
                LightPSVars = null;
            }
            if (LightQuadLayout != null)
            {
                LightQuadLayout.Dispose();
                LightQuadLayout = null;
            }
            if (LightQuad != null)
            {
                LightQuad.Dispose();
                LightQuad = null;
            }
            if (LightCone != null)
            {
                LightCone.Dispose();
                LightCone = null;
            }
            if (LightSphere != null)
            {
                LightSphere.Dispose();
                LightSphere = null;
            }
            if (LightCapsule != null)
            {
                LightCapsule.Dispose();
                LightCapsule = null;
            }
            if (LightPS != null)
            {
                LightPS.Dispose();
                LightPS = null;
            }
            if (LightVS != null)
            {
                LightVS.Dispose();
                LightVS = null;
            }
            if (MSAAPSVars != null)
            {
                MSAAPSVars.Dispose();
                MSAAPSVars = null;
            }
            if (MSAAPS != null)
            {
                MSAAPS.Dispose();
                MSAAPS = null;
            }
            if (FinalVS != null)
            {
                FinalVS.Dispose();
                FinalVS = null;
            }
        }

        public void OnWindowResize(DXManager dxman)
        {
            DisposeBuffers();

            var device = dxman.device;



            int uw = Width = dxman.backbuffer.Description.Width * MSAASampleCount;
            int uh = Height = dxman.backbuffer.Description.Height * MSAASampleCount;
            Viewport = new ViewportF();
            Viewport.Width = (float)uw;
            Viewport.Height = (float)uh;
            Viewport.MinDepth = 0.0f;
            Viewport.MaxDepth = 1.0f;
            Viewport.X = 0.0f;
            Viewport.Y = 0.0f;


            GBuffers = new GpuMultiTexture(device, uw, uh, 4, Format.R8G8B8A8_UNorm, true, Format.D32_Float);
            WindowSizeVramUsage += GBuffers.VramUsage;

            SceneColour = new GpuTexture(device, uw, uh, Format.R32G32B32A32_Float, 1, 0, true, Format.D32_Float);
            WindowSizeVramUsage += SceneColour.VramUsage;
        }
        public void DisposeBuffers()
        {
            if (GBuffers != null)
            {
                GBuffers.Dispose();
                GBuffers = null;
            }
            if (SceneColour != null)
            {
                SceneColour.Dispose();
                SceneColour = null;
            }
            WindowSizeVramUsage = 0;
        }

        public void Clear(DeviceContext context)
        {
            GBuffers.Clear(context, new Color4(0.0f, 0.0f, 0.0f, 0.0f));
            SceneColour.Clear(context, new Color4(0.2f, 0.4f, 0.6f, 0.0f));
        }
        public void ClearDepth(DeviceContext context)
        {
            GBuffers.ClearDepth(context);
            SceneColour.ClearDepth(context);
        }
        public void SetGBuffers(DeviceContext context)
        {
            GBuffers.SetRenderTargets(context);
            context.Rasterizer.SetViewport(Viewport);
        }
        public void SetSceneColour(DeviceContext context)
        {
            SceneColour.SetRenderTarget(context);
            context.Rasterizer.SetViewport(Viewport);
        }

        public void RenderLights(DeviceContext context, Camera camera, Shadowmap globalShadows, ShaderGlobalLights globalLights)
        {
            uint rendermode = 0;
            uint rendermodeind = 1;

            //first full-screen directional light pass, for sun/moon
            //discard pixels where scene depth is 0, since nothing was rendered there
            //blend mode: overwrite

            context.VertexShader.Set(LightVS);
            context.PixelShader.Set(LightPS);

            LightVSVars.Vars.ViewProj = Matrix.Identity; //Matrix.Transpose(camera.ViewProjMatrix);
            LightVSVars.Vars.CameraPos = Vector4.Zero;   //new Vector4(camera.Position, 0.0f);
            LightVSVars.Vars.LightType = 0;
            LightVSVars.Vars.IsLOD = 0;
            LightVSVars.Vars.Pad0 = 0;
            LightVSVars.Vars.Pad1 = 0;
            LightVSVars.Update(context);
            LightVSVars.SetVSCBuffer(context, 0);

            LightPSVars.Vars.GlobalLights = globalLights.Params;
            LightPSVars.Vars.ViewProjInv = Matrix.Transpose(camera.ViewProjInvMatrix);
            LightPSVars.Vars.CameraPos = Vector4.Zero;   //new Vector4(camera.Position, 0.0f);
            LightPSVars.Vars.EnableShadows = (globalShadows != null) ? 1u : 0u;
            LightPSVars.Vars.RenderMode = rendermode;
            LightPSVars.Vars.RenderModeIndex = rendermodeind;
            LightPSVars.Vars.RenderSamplerCoord = 0;// (uint)RenderTextureSamplerCoord;
            LightPSVars.Vars.LightType = 0;
            LightPSVars.Vars.IsLOD = 0;
            LightPSVars.Vars.Pad0 = 0;
            LightPSVars.Vars.Pad1 = 0;
            LightPSVars.Update(context);
            LightPSVars.SetPSCBuffer(context, 0);

            context.PixelShader.SetShaderResources(0, GBuffers.DepthSRV);
            context.PixelShader.SetShaderResources(2, GBuffers.SRVs);

            if (globalShadows != null)
            {
                globalShadows.SetFinalRenderResources(context);
            }

            context.InputAssembler.InputLayout = LightQuadLayout;
            LightQuad.Draw(context);


            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.PixelShader.SetShaderResources(0, null, null, null);
            context.PixelShader.SetSamplers(0, null, null);
        }


        public void RenderLights(DeviceContext context, Camera camera, List<RenderableLODLights> lodlights)
        {
            //instanced rendering of all other lights, using appropriate shapes
            //blend mode: additive


            context.VertexShader.Set(LightVS);
            context.PixelShader.Set(LightPS);

            LightVSVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            LightVSVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            LightVSVars.Vars.LightType = 0;
            LightVSVars.Vars.IsLOD = 0;
            LightVSVars.Vars.Pad0 = 0;
            LightVSVars.Vars.Pad1 = 0;

            //LightPSVars.Vars.GlobalLights = globalLights.Params;
            LightPSVars.Vars.ViewProjInv = Matrix.Transpose(camera.ViewProjInvMatrix);
            LightPSVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            LightPSVars.Vars.EnableShadows = 0;// (globalShadows != null) ? 1u : 0u;
            LightPSVars.Vars.RenderMode = 0;// rendermode;
            LightPSVars.Vars.RenderModeIndex = 1;// rendermodeind;
            LightPSVars.Vars.RenderSamplerCoord = 0;// (uint)RenderTextureSamplerCoord;
            LightPSVars.Vars.LightType = 0;
            LightPSVars.Vars.IsLOD = 0;
            LightPSVars.Vars.Pad0 = 0;
            LightPSVars.Vars.Pad1 = 0;

            context.PixelShader.SetShaderResources(0, GBuffers.DepthSRV);
            context.PixelShader.SetShaderResources(2, GBuffers.SRVs);

            //if (globalShadows != null)
            //{
            //    globalShadows.SetFinalRenderResources(context);
            //}


            foreach (var rll in lodlights)
            {
                if (rll.PointsBuffer != null)
                {
                    context.VertexShader.SetShaderResources(0, rll.PointsBuffer.SRV);
                    context.PixelShader.SetShaderResources(6, rll.PointsBuffer.SRV);
                    LightVSVars.Vars.LightType = 1;
                    LightVSVars.Update(context);
                    LightVSVars.SetVSCBuffer(context, 0);
                    LightPSVars.Vars.LightType = 1;
                    LightPSVars.Update(context);
                    LightPSVars.SetPSCBuffer(context, 0);
                    LightSphere.DrawInstanced(context, rll.PointsBuffer.StructCount);
                }
                if (rll.SpotsBuffer != null)
                {
                    context.VertexShader.SetShaderResources(0, rll.SpotsBuffer.SRV);
                    context.PixelShader.SetShaderResources(6, rll.SpotsBuffer.SRV);
                    LightVSVars.Vars.LightType = 2;
                    LightVSVars.Update(context);
                    LightVSVars.SetVSCBuffer(context, 0);
                    LightPSVars.Vars.LightType = 2;
                    LightPSVars.Update(context);
                    LightPSVars.SetPSCBuffer(context, 0);
                    LightCone.DrawInstanced(context, rll.SpotsBuffer.StructCount);
                }
                if (rll.CapsBuffer != null)
                {
                    context.VertexShader.SetShaderResources(0, rll.CapsBuffer.SRV);
                    context.PixelShader.SetShaderResources(6, rll.CapsBuffer.SRV);
                    LightVSVars.Vars.LightType = 4;
                    LightVSVars.Update(context);
                    LightVSVars.SetVSCBuffer(context, 0);
                    LightPSVars.Vars.LightType = 4;
                    LightPSVars.Update(context);
                    LightPSVars.SetPSCBuffer(context, 0);
                    LightCapsule.DrawInstanced(context, rll.CapsBuffer.StructCount);
                }
            }


            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.PixelShader.SetShaderResources(0, null, null, null);
            context.PixelShader.SetSamplers(0, null, null);




        }




        public void FinalPass(DeviceContext context)
        {
            //do antialiasing from SceneColour into HDR primary

            context.VertexShader.Set(FinalVS);
            context.PixelShader.Set(MSAAPS);

            context.PixelShader.SetShaderResources(0, SceneColour.SRV);
            context.PixelShader.SetSamplers(0, SampleStatePoint);

            MSAAPSVars.Vars.SampleCount = (uint)MSAASampleCount;
            MSAAPSVars.Vars.SampleMult = 1.0f / (MSAASampleCount * MSAASampleCount);
            MSAAPSVars.Vars.TexelSizeX = 1.0f / Width;
            MSAAPSVars.Vars.TexelSizeY = 1.0f / Height;
            MSAAPSVars.Update(context);
            MSAAPSVars.SetPSCBuffer(context, 0);

            context.InputAssembler.InputLayout = LightQuadLayout;
            LightQuad.Draw(context);

            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.PixelShader.SetShaderResources(0, null, null, null);
            context.PixelShader.SetSamplers(0, null, null);

        }


    }
}
