using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using SharpDX;
using SharpDX.Direct3D11;
using System.IO;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using CodeWalker.World;
using System.Diagnostics;

namespace CodeWalker.Rendering
{
    public struct DistantLightsShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Matrix ViewInv;
        public Vector3 CamPos;
        public float Pad0;
    }

    public class DistantLightsShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader lightsvs;
        PixelShader lightsps;

        InputLayout layout;

        SamplerState texsampler;

        UnitQuad quad;

        GpuVarsBuffer<DistantLightsShaderVSSceneVars> VSSceneVars;


        public DistantLightsShader(Device device)
        {
            string folder = ShaderManager.GetShaderFolder();
            byte[] vsbytes = File.ReadAllBytes(Path.Combine(folder, "DistantLightsVS.cso"));
            byte[] psbytes = File.ReadAllBytes(Path.Combine(folder, "DistantLightsPS.cso"));

            lightsvs = new VertexShader(device, vsbytes);
            lightsps = new PixelShader(device, psbytes);

            layout = new InputLayout(device, vsbytes, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
            });


            texsampler = new SamplerState(device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Transparent,
                ComparisonFunction = Comparison.Always,
                Filter = Filter.MinMagMipLinear,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0,
            });


            quad = new UnitQuad(device);

            VSSceneVars = new GpuVarsBuffer<DistantLightsShaderVSSceneVars>(device);

        }

        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(lightsvs);
            context.PixelShader.Set(lightsps);
            SetInputLayout(context, VertexType.PT);
        }
        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            context.InputAssembler.InputLayout = layout;
            return true;
        }
        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap? shadowmap, ShaderGlobalLights lights)
        {
            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Vars.ViewInv = Matrix.Transpose(camera.ViewInvMatrix);
            VSSceneVars.Vars.CamPos = camera.Position;
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);
        }
        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
        }
        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
        }
        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
        }
        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetShaderResource(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.PixelShader.SetSampler(0, null);
        }


        public void RenderBatch(DeviceContext context, RenderableDistantLODLights lights)
        {
            context.VertexShader.SetShaderResource(0, lights.InstanceBuffer.SRV);
            context.PixelShader.SetShaderResource(0, lights.Texture.ShaderResourceView);
            context.PixelShader.SetSampler(0, texsampler);

            quad.DrawInstanced(context, lights.InstanceCount);
        }


        public void Dispose()
        {
            if (disposed) return;

            VSSceneVars.Dispose();

            quad.Dispose();

            texsampler.Dispose();

            layout.Dispose();

            //VSSceneVars.Dispose();

            lightsps.Dispose();
            lightsvs.Dispose();

            disposed = true;
        }

    }
}
