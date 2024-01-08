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
using System.Diagnostics;

namespace CodeWalker.Rendering
{

    public struct BoundingSphereVSSceneVars
    {
        public Matrix ViewProj;
        public Matrix ViewInv;
        public float SegmentCount;
        public float VertexCount;
        public float Pad1;
        public float Pad2;
    }
    public struct BoundingSphereVSSphereVars
    {
        public Vector3 Center;
        public float Radius;
    }
    public struct BoundingBoxVSSceneVars
    {
        public Matrix ViewProj;
    }
    public struct BoundingBoxVSBoxVars
    {
        public Quaternion Orientation;
        public Vector4 BBMin;
        public Vector4 BBRng; //max-min
        public Vector3 CamRel;
        public float Pad1;
        public Vector3 Scale;
        public float Pad2;
    }
    public struct BoundsPSColourVars
    {
        public Vector4 Colour;
    }


    public class BoundsShader : Shader, IDisposable
    {
        bool disposed = false;

        BoundsShaderMode mode = BoundsShaderMode.Sphere;
        VertexShader spherevs;
        VertexShader boxvs;
        PixelShader boundsps;

        GpuVarsBuffer<BoundingSphereVSSceneVars> VSSphereSceneVars;
        GpuVarsBuffer<BoundingSphereVSSphereVars> VSSphereVars;
        GpuVarsBuffer<BoundingBoxVSSceneVars> VSBoxSceneVars;
        GpuVarsBuffer<BoundingBoxVSBoxVars> VSBoxVars;
        GpuVarsBuffer<BoundsPSColourVars> PSColourVars;

        int SegmentCount = 64;
        int VertexCount = 65;
        UnitCube cube;

        public BoundsShader(Device device)
        {
            string folder = ShaderManager.GetShaderFolder();
            byte[] spherevsbytes = File.ReadAllBytes(Path.Combine(folder, "BoundingSphereVS.cso"));
            byte[] boxvsbytes = File.ReadAllBytes(Path.Combine(folder, "BoundingBoxVS.cso"));
            byte[] psbytes = File.ReadAllBytes(Path.Combine(folder, "BoundsPS.cso"));

            spherevs = new VertexShader(device, spherevsbytes);
            boxvs = new VertexShader(device, boxvsbytes);
            boundsps = new PixelShader(device, psbytes);

            VSSphereSceneVars = new GpuVarsBuffer<BoundingSphereVSSceneVars>(device);
            VSSphereVars = new GpuVarsBuffer<BoundingSphereVSSphereVars>(device);
            VSBoxSceneVars = new GpuVarsBuffer<BoundingBoxVSSceneVars>(device);
            VSBoxVars = new GpuVarsBuffer<BoundingBoxVSBoxVars>(device);
            PSColourVars = new GpuVarsBuffer<BoundsPSColourVars>(device);

            cube = new UnitCube(device, boxvsbytes, false, true, false);
        }


        public void SetMode(BoundsShaderMode m)
        {
            mode = m;
        }

        public override void SetShader(DeviceContext context)
        {
            switch (mode)
            {
                default:
                case BoundsShaderMode.Sphere:
                    context.VertexShader.Set(spherevs);
                    break;
                case BoundsShaderMode.Box:
                    context.VertexShader.Set(boxvs);
                    break;
            }
            context.PixelShader.Set(boundsps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            //switch (mode)
            //{
            //    default:
            //    case BoundsShaderMode.Sphere:
            //        context.InputAssembler.InputLayout = null;
            //        break;
            //    case BoundsShaderMode.Box:
            //        context.InputAssembler.InputLayout = cube.InputLayout;
            //        break;
            //}
            return true;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap? shadowmap, ShaderGlobalLights lights)
        {
            switch (mode)
            {
                default:
                case BoundsShaderMode.Sphere:
                    VSSphereSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
                    VSSphereSceneVars.Vars.ViewInv = Matrix.Transpose(camera.ViewInvMatrix);
                    VSSphereSceneVars.Vars.SegmentCount = (float)SegmentCount;
                    VSSphereSceneVars.Vars.VertexCount = (float)VertexCount;
                    VSSphereSceneVars.Update(context);
                    VSSphereSceneVars.SetVSCBuffer(context, 0);
                    break;
                case BoundsShaderMode.Box:
                    VSBoxSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
                    VSBoxSceneVars.Update(context);
                    VSBoxSceneVars.SetVSCBuffer(context, 0);
                    break;
            }
        }

        public void SetSphereVars(DeviceContext context, Vector3 center, float radius)
        {
            VSSphereVars.Vars.Center = center;
            VSSphereVars.Vars.Radius = radius;
            VSSphereVars.Update(context);
            VSSphereVars.SetVSCBuffer(context, 1);
        }

        public void SetBoxVars(DeviceContext context, Vector3 camrel, Vector3 bbmin, Vector3 bbmax, Quaternion orientation, Vector3 scale)
        {
            VSBoxVars.Vars.Orientation = orientation;
            VSBoxVars.Vars.BBMin = new Vector4(bbmin, 0.0f);
            VSBoxVars.Vars.BBRng = new Vector4(bbmax - bbmin, 0.0f);
            VSBoxVars.Vars.CamRel = camrel;
            VSBoxVars.Vars.Scale = scale;
            VSBoxVars.Update(context);
            VSBoxVars.SetVSCBuffer(context, 1);
        }

        public void SetColourVars(DeviceContext context, Vector4 colour)
        {
            PSColourVars.Vars.Colour = colour;
            PSColourVars.Update(context);
            PSColourVars.SetPSCBuffer(context, 0);
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
            context.PixelShader.SetConstantBuffer(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }


        public void DrawSphere(DeviceContext context)
        {
            context.InputAssembler.InputLayout = null;
            context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineStrip;
            context.Draw(VertexCount, 0);
        }

        public void DrawBox(DeviceContext context)
        {
            cube.Draw(context);
        }


        public void Dispose()
        {
            if (disposed) return;

            VSSphereSceneVars.Dispose();
            VSSphereVars.Dispose();
            VSBoxSceneVars.Dispose();
            VSBoxVars.Dispose();
            PSColourVars.Dispose();

            cube.Dispose();

            boundsps.Dispose();
            boxvs.Dispose();
            spherevs.Dispose();

            disposed = true;
        }

    }

    public enum BoundsShaderMode
    {
        None = 0,
        Sphere = 1,
        Box = 2,
    }

}
