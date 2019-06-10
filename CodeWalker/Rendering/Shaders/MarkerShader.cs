using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using CodeWalker.GameFiles;
using System.IO;
using SharpDX.DXGI;
using CodeWalker.World;

namespace CodeWalker.Rendering
{

    public struct MarkerShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Matrix ViewInv;
        public Vector4 ScreenScale; //xy = 1/wh
    }
    public struct MarkerShaderVSMarkerVars
    {
        public Vector4 CamRel;
        public Vector2 Size;
        public Vector2 Offset;
    }

    public class MarkerShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader markervs;
        PixelShader markerps;

        GpuVarsBuffer<MarkerShaderVSSceneVars> VSSceneVars;
        GpuVarsBuffer<MarkerShaderVSMarkerVars> VSMarkerVars;

        SamplerState texsampler;

        InputLayout layout;


        public MarkerShader(Device device)
        {
            byte[] vsbytes = File.ReadAllBytes("Shaders\\MarkerVS.cso");
            byte[] psbytes = File.ReadAllBytes("Shaders\\MarkerPS.cso");

            markervs = new VertexShader(device, vsbytes);
            markerps = new PixelShader(device, psbytes);

            VSSceneVars = new GpuVarsBuffer<MarkerShaderVSSceneVars>(device);
            VSMarkerVars = new GpuVarsBuffer<MarkerShaderVSMarkerVars>(device);

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

        }




        public override void SetShader(DeviceContext context)
        {
            context.VertexShader.Set(markervs);
            context.PixelShader.Set(markerps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            context.InputAssembler.InputLayout = layout;
            return true;
        }


        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Vars.ViewInv = Matrix.Transpose(camera.ViewInvMatrix);
            VSSceneVars.Vars.ScreenScale = new Vector4(0.5f / camera.Width, 0.5f / camera.Height, camera.Width, camera.Height);
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);
        }

        public void SetMarkerVars(DeviceContext context, Vector3 camrel, Vector2 size, Vector2 offset)
        {
            VSMarkerVars.Vars.CamRel = new Vector4(camrel, 0.0f);
            VSMarkerVars.Vars.Size = size;
            VSMarkerVars.Vars.Offset = offset;
            VSMarkerVars.Update(context);
            VSMarkerVars.SetVSCBuffer(context, 1);
        }

        public void SetTexture(DeviceContext context, ShaderResourceView srv)
        {
            context.PixelShader.SetSampler(0, texsampler);
            context.PixelShader.SetShaderResource(0, srv);
        }

        public override void SetEntityVars(DeviceContext context, ref RenderableInst rend)
        {
            //don't use this one
        }
        public override void SetModelVars(DeviceContext context, RenderableModel model)
        {
            //don't use this
        }
        public override void SetGeomVars(DeviceContext context, RenderableGeometry geom)
        {
            //don't use this
        }

        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetConstantBuffer(1, null);
            context.PixelShader.SetSampler(0, null);
            context.PixelShader.SetShaderResource(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }


        public void Dispose()
        {
            if (disposed) return;

            if (texsampler != null)
            {
                texsampler.Dispose();
                texsampler = null;
            }

            layout.Dispose();

            VSSceneVars.Dispose();
            VSMarkerVars.Dispose();

            markerps.Dispose();
            markervs.Dispose();

            disposed = true;
        }

    }
}
