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

    public struct PathShaderVSSceneVars
    {
        public Matrix ViewProj;
        public Vector4 CameraPos;
        public Vector4 LightColour;
    }


    public class PathShader : Shader, IDisposable
    {
        bool disposed = false;

        VertexShader boxvs;
        PixelShader boxps;
        VertexShader dynvs;
        VertexShader vs;
        PixelShader ps;

        GpuVarsBuffer<PathShaderVSSceneVars> VSSceneVars;

        InputLayout layout;

        UnitCube cube;

        bool UseDynamicVerts = false;
        GpuCBuffer<VertexTypePC> vertices; //for selection polys/lines use


        public PathShader(Device device)
        {
            byte[] boxvsbytes = PathUtil.ReadAllBytes("Shaders\\PathBoxVS.cso");
            byte[] boxpsbytes = PathUtil.ReadAllBytes("Shaders\\PathBoxPS.cso");
            byte[] dynvsbytes = PathUtil.ReadAllBytes("Shaders\\PathDynVS.cso");
            byte[] vsbytes = PathUtil.ReadAllBytes("Shaders\\PathVS.cso");
            byte[] psbytes = PathUtil.ReadAllBytes("Shaders\\PathPS.cso");


            boxvs = new VertexShader(device, boxvsbytes);
            boxps = new PixelShader(device, boxpsbytes);
            dynvs = new VertexShader(device, dynvsbytes);
            vs = new VertexShader(device, vsbytes);
            ps = new PixelShader(device, psbytes);

            VSSceneVars = new GpuVarsBuffer<PathShaderVSSceneVars>(device);

            layout = new InputLayout(device, vsbytes, VertexTypeGTAV.GetLayout(VertexType.PC));

            cube = new UnitCube(device, boxvsbytes, true, false, true);

            vertices = new GpuCBuffer<VertexTypePC>(device, 1000); //should be more than needed....
        }


        public override void SetShader(DeviceContext context)
        {
            if (UseDynamicVerts)
            {
                context.VertexShader.Set(dynvs);
                //context.InputAssembler.SetVertexBuffers(0, null);
                context.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            }
            else
            {
                context.VertexShader.Set(vs);
            }
            context.PixelShader.Set(ps);
        }

        public override bool SetInputLayout(DeviceContext context, VertexType type)
        {
            if (UseDynamicVerts)
            {
                context.InputAssembler.InputLayout = null;
            }
            else
            {
                context.InputAssembler.InputLayout = layout;
            }
            return true;
        }

        public override void SetSceneVars(DeviceContext context, Camera camera, Shadowmap shadowmap, ShaderGlobalLights lights)
        {
            VSSceneVars.Vars.ViewProj = Matrix.Transpose(camera.ViewProjMatrix);
            VSSceneVars.Vars.CameraPos = new Vector4(camera.Position, 0.0f);
            VSSceneVars.Vars.LightColour = new Vector4(1.0f, 1.0f, 1.0f, lights.HdrIntensity * 2.0f);
            VSSceneVars.Update(context);
            VSSceneVars.SetVSCBuffer(context, 0);
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


        public void RenderBatches(DeviceContext context, List<RenderablePathBatch> batches, Camera camera, ShaderGlobalLights lights, Vector3? excludeNodePosition = null)
        {
            UseDynamicVerts = false;
            SetShader(context);
            SetInputLayout(context, VertexType.PC);
            SetSceneVars(context, camera, null, lights);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.InputAssembler.SetIndexBuffer(null, Format.R16_UInt, 0);
            for (int i = 0; i < batches.Count; i++)
            {
                var pbatch = batches[i];

                if (pbatch.TriangleVertexBuffer == null) continue;
                if (pbatch.TriangleVertexCount == 0) continue;

                context.InputAssembler.SetVertexBuffers(0, pbatch.TriangleVBBinding);
                context.Draw(pbatch.TriangleVertexCount, 0);
            }

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            context.InputAssembler.SetIndexBuffer(null, Format.R16_UInt, 0);
            for (int i = 0; i < batches.Count; i++)
            {
                var pbatch = batches[i];

                if (pbatch.PathVertexBuffer == null) continue;
                if (pbatch.PathVertexCount == 0) continue;

                context.InputAssembler.SetVertexBuffers(0, pbatch.PathVBBinding);
                context.Draw(pbatch.PathVertexCount, 0);
            }


            context.VertexShader.Set(boxvs);
            context.PixelShader.Set(boxps);

            VSSceneVars.SetVSCBuffer(context, 0);

            foreach (var batch in batches)
            {
                if (batch.NodeBuffer == null) continue;
                if (batch.Nodes == null) continue;

                int nodeCount = batch.Nodes.Length;

                // If we need to exclude a specific node, filter it out
                if (excludeNodePosition.HasValue && nodeCount > 0)
                {
                    var excludePos = excludeNodePosition.Value;
                    var filteredNodes = new List<Vector4>();

                    foreach (var node in batch.Nodes)
                    {
                        // Check if this node matches the position to exclude (with small epsilon for float comparison)
                        var nodePos = new Vector3(node.X, node.Y, node.Z);
                        float distSq = (nodePos - excludePos).LengthSquared();
                        if (distSq > 0.01f) // If not the selected node, include it
                        {
                            filteredNodes.Add(node);
                        }
                    }

                    // Only render if we have nodes left after filtering
                    if (filteredNodes.Count > 0)
                    {
                        // Create temporary buffer with filtered nodes
                        var tempBuffer = new GpuSBuffer<Vector4>(context.Device, filteredNodes.ToArray());
                        context.VertexShader.SetShaderResource(0, tempBuffer.SRV);
                        cube.DrawInstanced(context, filteredNodes.Count);
                        tempBuffer.Dispose();
                    }
                }
                else
                {
                    // No filtering needed, render all nodes
                    context.VertexShader.SetShaderResource(0, batch.NodeBuffer.SRV);
                    cube.DrawInstanced(context, nodeCount);
                }
            }

            UnbindResources(context);
        }

        public void RenderTriangles(DeviceContext context, List<VertexTypePC> verts, Camera camera, ShaderGlobalLights lights)
        {
            UseDynamicVerts = true;
            SetShader(context);
            SetInputLayout(context, VertexType.PC);
            SetSceneVars(context, camera, null, lights);

            int drawn = 0;
            int tricount = verts.Count / 3;
            int maxcount = vertices.StructCount / 3;
            while (drawn < tricount)
            {
                vertices.Clear();

                int offset = drawn*3;
                int bcount = Math.Min(tricount - drawn, maxcount);
                for (int i = 0; i < bcount; i++)
                {
                    int t = offset + (i * 3);
                    vertices.Add(verts[t + 0]);
                    vertices.Add(verts[t + 1]);
                    vertices.Add(verts[t + 2]);
                }
                drawn += bcount;

                vertices.Update(context);
                vertices.SetVSResource(context, 0);

                context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                context.Draw(vertices.CurrentCount, 0);
            }

        }

        public void RenderLines(DeviceContext context, List<VertexTypePC> verts, Camera camera, ShaderGlobalLights lights)
        {
            UseDynamicVerts = true;
            SetShader(context);
            SetInputLayout(context, VertexType.PC);
            SetSceneVars(context, camera, null, lights);

            int drawn = 0;
            int linecount = verts.Count / 2;
            int maxcount = vertices.StructCount / 2;
            while (drawn < linecount)
            {
                vertices.Clear();

                int offset = drawn * 2;
                int bcount = Math.Min(linecount - drawn, maxcount);
                for (int i = 0; i < bcount; i++)
                {
                    int t = offset + (i * 2);
                    vertices.Add(verts[t + 0]);
                    vertices.Add(verts[t + 1]);
                }
                drawn += bcount;

                vertices.Update(context);
                vertices.SetVSResource(context, 0);

                context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
                context.Draw(vertices.CurrentCount, 0);
            }
        }




        public override void UnbindResources(DeviceContext context)
        {
            context.VertexShader.SetConstantBuffer(0, null);
            context.VertexShader.SetShaderResource(0, null);
            context.VertexShader.Set(null);
            context.PixelShader.Set(null);
        }



        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            VSSceneVars.Dispose();

            vertices.Dispose();

            layout.Dispose();
            cube.Dispose();

            ps.Dispose();
            vs.Dispose();
            dynvs.Dispose();
            boxvs.Dispose();
            boxps.Dispose();
        }

    }

}
