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
        public uint SampleCount;//for MSAA
        public float SampleMult;//for MSAA
    }
    public struct DeferredLightInstVars
    {
        public Vector3 InstPosition;
        public float InstIntensity;
        public Vector3 InstColour;
        public float InstFalloff;
        public Vector3 InstDirection;
        public float InstFalloffExponent;
        public Vector3 InstTangentX;
        public float InstConeInnerAngle;
        public Vector3 InstTangentY;
        public float InstConeOuterAngle;
        public Vector3 InstCapsuleExtent;
        public uint InstType;
        public Vector3 InstCullingPlaneNormal;
        public float InstCullingPlaneOffset;
    }

    public struct DeferredSSAAPSVars
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

        VertexShader DirLightVS;
        PixelShader DirLightPS;
        PixelShader DirLightMSPS;
        VertexShader LodLightVS;
        PixelShader LodLightPS;
        PixelShader LodLightMSPS;
        VertexShader LightVS;
        PixelShader LightPS;
        PixelShader LightMSPS;
        UnitCone LightCone;
        UnitSphere LightSphere;
        UnitCapsule LightCapsule;
        UnitQuad LightQuad;
        InputLayout LightQuadLayout;


        GpuVarsBuffer<DeferredLightVSVars> LightVSVars;
        GpuVarsBuffer<DeferredLightPSVars> LightPSVars;
        GpuVarsBuffer<DeferredLightInstVars> LightInstVars;



        VertexShader FinalVS;
        PixelShader SSAAPS;
        GpuVarsBuffer<DeferredSSAAPSVars> SSAAPSVars;
        public int SSAASampleCount = 1;


        public int MSAASampleCount = 4;




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

            byte[] bDirLightVS = File.ReadAllBytes("Shaders\\DirLightVS.cso");
            byte[] bDirLightPS = File.ReadAllBytes("Shaders\\DirLightPS.cso");
            byte[] bDirLightMSPS = File.ReadAllBytes("Shaders\\DirLightPS_MS.cso");
            byte[] bLodLightVS = File.ReadAllBytes("Shaders\\LodLightsVS.cso");
            byte[] bLodLightPS = File.ReadAllBytes("Shaders\\LodLightsPS.cso");
            byte[] bLodLightMSPS = File.ReadAllBytes("Shaders\\LodLightsPS_MS.cso");
            byte[] bLightVS = File.ReadAllBytes("Shaders\\LightVS.cso");
            byte[] bLightPS = File.ReadAllBytes("Shaders\\LightPS.cso");
            byte[] bLightMSPS = File.ReadAllBytes("Shaders\\LightPS_MS.cso");
            byte[] bFinalVS = File.ReadAllBytes("Shaders\\PPFinalPassVS.cso");
            byte[] bSSAAPS = File.ReadAllBytes("Shaders\\PPSSAAPS.cso");

            DirLightVS = new VertexShader(device, bDirLightVS);
            DirLightPS = new PixelShader(device, bDirLightPS);
            LodLightVS = new VertexShader(device, bLodLightVS);
            LodLightPS = new PixelShader(device, bLodLightPS);
            LightVS = new VertexShader(device, bLightVS);
            LightPS = new PixelShader(device, bLightPS);

            try
            {
                //error could happen here if the device isn't supporting feature level 10.1
                DirLightMSPS = new PixelShader(device, bDirLightMSPS);
                LodLightMSPS = new PixelShader(device, bLodLightMSPS);
                LightMSPS = new PixelShader(device, bLightMSPS);
            }
            catch
            {
                MSAASampleCount = 1; //can't do MSAA without at least 10.1 support
            }


            LightCone = new UnitCone(device, bLodLightVS, 4, false);
            LightSphere = new UnitSphere(device, bLodLightVS, 4, true);
            LightCapsule = new UnitCapsule(device, bLodLightVS, 4, false);
            LightQuad = new UnitQuad(device, true);
            LightQuadLayout = new InputLayout(device, bDirLightVS, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
            });

            LightVSVars = new GpuVarsBuffer<DeferredLightVSVars>(device);
            LightPSVars = new GpuVarsBuffer<DeferredLightPSVars>(device);
            LightInstVars = new GpuVarsBuffer<DeferredLightInstVars>(device);


            FinalVS = new VertexShader(device, bFinalVS);
            SSAAPS = new PixelShader(device, bSSAAPS);

            SSAAPSVars = new GpuVarsBuffer<DeferredSSAAPSVars>(device);

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
            if (LightInstVars != null)
            {
                LightInstVars.Dispose();
                LightInstVars = null;
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
            if (DirLightPS != null)
            {
                DirLightPS.Dispose();
                DirLightPS = null;
            }
            if (DirLightMSPS != null)
            {
                DirLightMSPS.Dispose();
                DirLightMSPS = null;
            }
            if (DirLightVS != null)
            {
                DirLightVS.Dispose();
                DirLightVS = null;
            }
            if (LodLightPS != null)
            {
                LodLightPS.Dispose();
                LodLightPS = null;
            }
            if (LodLightMSPS != null)
            {
                LodLightMSPS.Dispose();
                LodLightMSPS = null;
            }
            if (LodLightVS != null)
            {
                LodLightVS.Dispose();
                LodLightVS = null;
            }
            if (LightPS != null)
            {
                LightPS.Dispose();
                LightPS = null;
            }
            if (LightMSPS != null)
            {
                LightMSPS.Dispose();
                LightMSPS = null;
            }
            if (LightVS != null)
            {
                LightVS.Dispose();
                LightVS = null;
            }
            if (SSAAPSVars != null)
            {
                SSAAPSVars.Dispose();
                SSAAPSVars = null;
            }
            if (SSAAPS != null)
            {
                SSAAPS.Dispose();
                SSAAPS = null;
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



            int uw = Width = dxman.backbuffer.Description.Width * SSAASampleCount;
            int uh = Height = dxman.backbuffer.Description.Height * SSAASampleCount;
            Viewport = new ViewportF();
            Viewport.Width = (float)uw;
            Viewport.Height = (float)uh;
            Viewport.MinDepth = 0.0f;
            Viewport.MaxDepth = 1.0f;
            Viewport.X = 0.0f;
            Viewport.Y = 0.0f;


            GBuffers = new GpuMultiTexture(device, uw, uh, 4, Format.R8G8B8A8_UNorm, true, Format.D32_Float, MSAASampleCount);
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

            //first full-screen directional light pass, for sun/moon
            //discard pixels where scene depth is 0, since nothing was rendered there
            //blend mode: overwrite

            var ps = (MSAASampleCount > 1) ? DirLightMSPS : DirLightPS;

            context.VertexShader.Set(DirLightVS);
            context.PixelShader.Set(ps);

            LightVSVars.Vars.ViewProj = Matrix.Identity;
            LightVSVars.Vars.CameraPos = Vector4.Zero;
            LightVSVars.Vars.LightType = 0;
            LightVSVars.Vars.IsLOD = 0;
            LightVSVars.Vars.Pad0 = 0;
            LightVSVars.Vars.Pad1 = 0;
            LightVSVars.Update(context);
            LightVSVars.SetVSCBuffer(context, 0);

            LightPSVars.Vars.GlobalLights = globalLights.Params;
            LightPSVars.Vars.ViewProjInv = Matrix.Transpose(camera.ViewProjInvMatrix);
            LightPSVars.Vars.CameraPos = Vector4.Zero;
            LightPSVars.Vars.EnableShadows = (globalShadows != null) ? 1u : 0u;
            LightPSVars.Vars.RenderMode = 0;
            LightPSVars.Vars.RenderModeIndex = 1;
            LightPSVars.Vars.RenderSamplerCoord = 0;
            LightPSVars.Vars.LightType = 0;
            LightPSVars.Vars.IsLOD = 0;
            LightPSVars.Vars.SampleCount = (uint)MSAASampleCount;
            LightPSVars.Vars.SampleMult = 1.0f / MSAASampleCount;
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

            var ps = (MSAASampleCount > 1) ? LodLightMSPS : LodLightPS;

            context.VertexShader.Set(LodLightVS);
            context.PixelShader.Set(ps);

            LightVSVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            LightVSVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            LightVSVars.Vars.LightType = 0;
            LightVSVars.Vars.IsLOD = 0;
            LightVSVars.Vars.Pad0 = 0;
            LightVSVars.Vars.Pad1 = 0;

            LightPSVars.Vars.ViewProjInv = Matrix.Transpose(camera.ViewProjInvMatrix);
            LightPSVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            LightPSVars.Vars.EnableShadows = 0;
            LightPSVars.Vars.RenderMode = 0;
            LightPSVars.Vars.RenderModeIndex = 1;
            LightPSVars.Vars.RenderSamplerCoord = 0;
            LightPSVars.Vars.LightType = 0;
            LightPSVars.Vars.IsLOD = 0;
            LightPSVars.Vars.SampleCount = (uint)MSAASampleCount;
            LightPSVars.Vars.SampleMult = 1.0f / MSAASampleCount;

            context.PixelShader.SetShaderResources(0, GBuffers.DepthSRV);
            context.PixelShader.SetShaderResources(2, GBuffers.SRVs);


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

        public void RenderLights(DeviceContext context, Camera camera, List<RenderableLightInst> lights)
        {
            //instanced rendering of all other lights, using appropriate shapes
            //blend mode: additive


            var ps = (MSAASampleCount > 1) ? LightMSPS : LightPS;

            context.VertexShader.Set(LightVS);
            context.PixelShader.Set(ps);

            LightVSVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            LightVSVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            LightVSVars.Vars.LightType = 0;
            LightVSVars.Vars.IsLOD = 0;
            LightVSVars.Vars.Pad0 = 0;
            LightVSVars.Vars.Pad1 = 0;
            LightVSVars.Update(context);
            LightVSVars.SetVSCBuffer(context, 0);

            LightPSVars.Vars.ViewProjInv = Matrix.Transpose(camera.ViewProjInvMatrix);
            LightPSVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            LightPSVars.Vars.EnableShadows = 0;
            LightPSVars.Vars.RenderMode = 0;
            LightPSVars.Vars.RenderModeIndex = 1;
            LightPSVars.Vars.RenderSamplerCoord = 0;
            LightPSVars.Vars.LightType = 0;
            LightPSVars.Vars.IsLOD = 0;
            LightPSVars.Vars.SampleCount = (uint)MSAASampleCount;
            LightPSVars.Vars.SampleMult = 1.0f / MSAASampleCount;
            LightPSVars.Update(context);
            LightPSVars.SetPSCBuffer(context, 0);

            context.PixelShader.SetShaderResources(0, GBuffers.DepthSRV);
            context.PixelShader.SetShaderResources(2, GBuffers.SRVs);


            for (int i = 0; i < lights.Count; i++)
            {
                var li = lights[i];
                var rl = li.Light;

                LightInstVars.Vars.InstPosition = li.EntityPosition + li.EntityRotation.Multiply(rl.Position) - camera.Position;
                LightInstVars.Vars.InstDirection = li.EntityRotation.Multiply(rl.Direction);
                LightInstVars.Vars.InstTangentX = li.EntityRotation.Multiply(rl.TangentX);
                LightInstVars.Vars.InstTangentY = li.EntityRotation.Multiply(rl.TangentY);
                LightInstVars.Vars.InstCapsuleExtent = li.EntityRotation.Multiply(rl.CapsuleExtent);
                LightInstVars.Vars.InstCullingPlaneNormal = li.EntityRotation.Multiply(rl.CullingPlaneNormal);
                LightInstVars.Vars.InstColour = rl.Colour;
                LightInstVars.Vars.InstIntensity = rl.Intensity;
                LightInstVars.Vars.InstFalloff = rl.Falloff;
                LightInstVars.Vars.InstFalloffExponent = rl.FalloffExponent;
                LightInstVars.Vars.InstConeInnerAngle = rl.ConeInnerAngle;
                LightInstVars.Vars.InstConeOuterAngle = rl.ConeOuterAngle;
                LightInstVars.Vars.InstType = (uint)rl.Type;
                LightInstVars.Vars.InstCullingPlaneOffset = rl.CullingPlaneOffset;
                LightInstVars.Update(context);
                LightInstVars.SetVSCBuffer(context, 1);
                LightInstVars.SetPSCBuffer(context, 2);

                switch (rl.Type)
                {
                    case LightType.Point:
                        LightSphere.Draw(context);
                        break;
                    case LightType.Spot:
                        LightCone.Draw(context);
                        break;
                    case LightType.Capsule:
                        LightCapsule.Draw(context);
                        break;
                    default:
                        break;
                }

            }


            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.PixelShader.SetShaderResources(0, null, null, null);
            context.PixelShader.SetSamplers(0, null, null);
        }


        public void SSAAPass(DeviceContext context)
        {
            //do antialiasing from SceneColour

            context.VertexShader.Set(FinalVS);
            context.PixelShader.Set(SSAAPS);

            context.PixelShader.SetShaderResources(0, SceneColour.SRV);
            context.PixelShader.SetSamplers(0, SampleStatePoint);

            SSAAPSVars.Vars.SampleCount = (uint)SSAASampleCount;
            SSAAPSVars.Vars.SampleMult = 1.0f / (SSAASampleCount * SSAASampleCount);
            SSAAPSVars.Vars.TexelSizeX = 1.0f / Width;
            SSAAPSVars.Vars.TexelSizeY = 1.0f / Height;
            SSAAPSVars.Update(context);
            SSAAPSVars.SetPSCBuffer(context, 0);

            context.InputAssembler.InputLayout = LightQuadLayout;
            LightQuad.Draw(context);

            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
            context.PixelShader.SetShaderResources(0, null, null, null);
            context.PixelShader.SetSamplers(0, null, null);

        }


    }
}
