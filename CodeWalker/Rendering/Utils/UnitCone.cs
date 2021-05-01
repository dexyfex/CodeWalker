using System;
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

        private struct Tri
        {
            public int v1;
            public int v2;
            public int v3;
            public Tri(int i1, int i2, int i3)
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
            List<Tri> curtris = new List<Tri>();

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

                curtris.Add(new Tri(0, i0, i1)); //fill the cone
                curtris.Add(new Tri(1, i1+1, i0+1)); //bottom cap triangles
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



    public class LightCone
    {
        private Buffer VertexBuffer { get; set; }
        private Buffer IndexBuffer { get; set; }
        private InputLayout InputLayout { get; set; }
        private VertexBufferBinding vbbinding;
        private int indexcount;

        private struct Tri
        {
            public Vector4 v1;
            public Vector4 v2;
            public Vector4 v3;
            public Tri(Vector4 i1, Vector4 i2, Vector4 i3)
            {
                v1 = i1;
                v2 = i2;
                v3 = i3;
            }
        }

        public LightCone(Device device, byte[] vsbytes, int detail)
        {

            InputLayout = new InputLayout(device, vsbytes, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
            });


            var tris = new List<Tri>();
            var newtris = new List<Tri>();
            var curtris = new List<Tri>();

            curtris.Clear();//"cone" triangles
            curtris.Add(new Tri(new Vector4(0, 0, 0, 0), new Vector4(-1, 0, 0, 1), new Vector4(0, -1, 0, 1)));
            curtris.Add(new Tri(new Vector4(0, 0, 0, 0), new Vector4(0, 1, 0, 1), new Vector4(-1, 0, 0, 1)));
            curtris.Add(new Tri(new Vector4(0, 0, 0, 0), new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1)));
            curtris.Add(new Tri(new Vector4(0, 0, 0, 0), new Vector4(0, -1, 0, 1), new Vector4(1, 0, 0, 1)));
            for (int i = 0; i < detail; i++)
            {
                foreach (var tri in curtris)
                {
                    var v1 = tri.v1;
                    var v2 = tri.v2;
                    var v3 = tri.v3;
                    var v4 = new Vector4(Vector3.Normalize((v2 + v3).XYZ() * 0.5f), 1);
                    newtris.Add(new Tri(v1, v2, v4));
                    newtris.Add(new Tri(v1, v4, v3));
                }
                curtris = newtris;
                newtris = new List<Tri>();
            }
            tris.AddRange(curtris);

            curtris.Clear();//hemisphere triangles
            curtris.Add(new Tri(new Vector4(0, 0, 1, 1), new Vector4(0, -1, 0, 1), new Vector4(-1, 0, 0, 1)));
            curtris.Add(new Tri(new Vector4(0, 0, 1, 1), new Vector4(-1, 0, 0, 1), new Vector4(0, 1, 0, 1)));
            curtris.Add(new Tri(new Vector4(0, 0, 1, 1), new Vector4(0, 1, 0, 1), new Vector4(1, 0, 0, 1)));
            curtris.Add(new Tri(new Vector4(0, 0, 1, 1), new Vector4(1, 0, 0, 1), new Vector4(0, -1, 0, 1)));
            for (int i = 0; i < detail; i++)
            {
                foreach (var tri in curtris)
                {
                    var v1 = tri.v1;
                    var v2 = tri.v2;
                    var v3 = tri.v3;
                    var v4 = new Vector4(Vector3.Normalize((v1 + v2).XYZ() * 0.5f), 1);
                    var v5 = new Vector4(Vector3.Normalize((v2 + v3).XYZ() * 0.5f), 1);
                    var v6 = new Vector4(Vector3.Normalize((v3 + v1).XYZ() * 0.5f), 1);
                    newtris.Add(new Tri(v1, v4, v6));
                    newtris.Add(new Tri(v4, v2, v5));
                    newtris.Add(new Tri(v4, v5, v6));
                    newtris.Add(new Tri(v6, v5, v3));
                }
                curtris = newtris;
                newtris = new List<Tri>();
            }
            tris.AddRange(curtris);




            var verts = new List<Vector4>();
            var vdict = new Dictionary<Vector4, int>();
            var idata = new List<uint>();
            uint addVert(Vector4 v)
            {
                if (vdict.TryGetValue(v, out int i)) return (uint)i;
                var n = verts.Count;
                verts.Add(v);
                vdict[v] = n;
                return (uint)n;
            }
            foreach (var tri in tris)
            {
                idata.Add(addVert(tri.v1));
                idata.Add(addVert(tri.v2));
                idata.Add(addVert(tri.v3));
            }



            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, verts.ToArray());
            vbbinding = new VertexBufferBinding(VertexBuffer, 16, 0);

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
