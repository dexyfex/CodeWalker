﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.DXGI;

namespace CodeWalker.Rendering
{
    public class UnitCone
    {
        private Buffer VertexBuffer { get; set; }
        private Buffer IndexBuffer { get; set; }
        private InputLayout InputLayout { get; set; }
        private VertexBufferBinding vbbinding;
        private int indexcount;

        private struct SphTri
        {
            public int v1;
            public int v2;
            public int v3;
            public SphTri(int i1, int i2, int i3)
            {
                v1 = i1;
                v2 = i2;
                v3 = i3;
            }
        }

        public UnitCone(Device device, byte[] vsbytes, int detail, bool invert = false)
        {

            InputLayout = new InputLayout(device, vsbytes, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            });



            List<Vector4> verts = new List<Vector4>();
            Dictionary<Vector4, int> vdict = new Dictionary<Vector4, int>();
            List<SphTri> curtris = new List<SphTri>();

            verts.Add(new Vector4(0.0f, 0.0f, 0.0f, 0.0f));//top end (translated by VS!)
            verts.Add(new Vector4(0.0f, -1.0f, 0.0f, 0.0f));//top normal
            verts.Add(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));//bottom end
            verts.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));//bottom normal

            int nlons = detail * 4;
            int lastlon = nlons - 1;
            float latrng = 1.0f / (detail);
            float lonrng = 1.0f / (nlons);
            float twopi = (float)(2.0 * Math.PI);

            for (int lon = 0; lon < nlons; lon++)
            {
                float tlon = lon * lonrng;
                float rlon = tlon * twopi;
                float lonx = (float)Math.Sin(rlon);
                float lonz = (float)Math.Cos(rlon);

                verts.Add(new Vector4(lonx, 0.0f, lonz, 1.0f));//2
                verts.Add(new Vector4(lonx, 0.0f, lonz, 0.0f));//side normal
                verts.Add(new Vector4(lonx, 0.0f, lonz, 1.0f));//3
                verts.Add(new Vector4(0.0f, 1.0f, 0.0f, 0.0f));//bottom normal
            }

            for (int lon = 0; lon < nlons; lon++)
            {
                int i0 = 2 + lon * 2;
                int i1 = i0 + 2;

                if (lon == lastlon)
                {
                    i1 = 2;
                }

                curtris.Add(new SphTri(0, i0, i1)); //fill the cone
                curtris.Add(new SphTri(1, i1+1, i0+1)); //bottom cap triangles
            }



            List<uint> idata = new List<uint>();
            foreach (var tri in curtris)
            {
                idata.Add((uint)tri.v1);
                idata.Add((uint)(invert ? tri.v2 : tri.v3));
                idata.Add((uint)(invert ? tri.v3 : tri.v2));
            }


            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, verts.ToArray());
            vbbinding = new VertexBufferBinding(VertexBuffer, 32, 0);

            IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, idata.ToArray());
            indexcount = idata.Count;

        }


        public void Draw(DeviceContext context)
        {
            context.InputAssembler.InputLayout = InputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            context.DrawIndexed(indexcount, 0, 0);
        }

        public void DrawInstanced(DeviceContext context, int count)
        {
            context.InputAssembler.InputLayout = InputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            context.DrawIndexedInstanced(indexcount, count, 0, 0, 0);
        }


        public void Dispose()
        {
            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }
            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }
            if (InputLayout != null)
            {
                InputLayout.Dispose();
                InputLayout = null;
            }
        }

    }


}
