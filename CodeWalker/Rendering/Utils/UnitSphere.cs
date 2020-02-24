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
    public class UnitSphere
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
            public SphTri(int i1,int i2, int i3)
            {
                v1 = i1;
                v2 = i2;
                v3 = i3;
            }
        }

        public UnitSphere(Device device, byte[] vsbytes, int detail, bool invert = false)
        {

            InputLayout = new InputLayout(device, vsbytes, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                //new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
            });



            List<Vector3> verts = new List<Vector3>();
            Dictionary<Vector3, int> vdict = new Dictionary<Vector3, int>();
            List<SphTri> curtris = new List<SphTri>();
            List<SphTri> nxttris = new List<SphTri>();

            verts.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            verts.Add(new Vector3(1.0f, 0.0f, 0.0f));
            verts.Add(new Vector3(0.0f, -1.0f, 0.0f));
            verts.Add(new Vector3(0.0f, 1.0f, 0.0f));
            verts.Add(new Vector3(0.0f, 0.0f, -1.0f));
            verts.Add(new Vector3(0.0f, 0.0f, 1.0f));
            curtris.Add(new SphTri(0, 4, 2));
            curtris.Add(new SphTri(4, 1, 2));
            curtris.Add(new SphTri(1, 5, 2));
            curtris.Add(new SphTri(5, 0, 2));
            curtris.Add(new SphTri(4, 0, 3));
            curtris.Add(new SphTri(1, 4, 3));
            curtris.Add(new SphTri(5, 1, 3));
            curtris.Add(new SphTri(0, 5, 3));

            for (int i = 0; i < verts.Count; i++)
            {
                vdict[verts[i]] = i;
            }


            for (int i = 0; i < detail; i++)
            {
                nxttris.Clear();
                foreach (var tri in curtris)
                {
                    Vector3 v1 = verts[tri.v1];
                    Vector3 v2 = verts[tri.v2];
                    Vector3 v3 = verts[tri.v3];
                    Vector3 s1 = Vector3.Normalize(v1 + v2);
                    Vector3 s2 = Vector3.Normalize(v2 + v3);
                    Vector3 s3 = Vector3.Normalize(v3 + v1);
                    int i1, i2, i3;
                    if (!vdict.TryGetValue(s1, out i1))
                    {
                        i1 = verts.Count;
                        verts.Add(s1);
                        vdict[s1] = i1;
                    }
                    if (!vdict.TryGetValue(s2, out i2))
                    {
                        i2 = verts.Count;
                        verts.Add(s2);
                        vdict[s2] = i2;
                    }
                    if (!vdict.TryGetValue(s3, out i3))
                    {
                        i3 = verts.Count;
                        verts.Add(s3);
                        vdict[s3] = i3;
                    }
                    nxttris.Add(new SphTri(tri.v1, i1, i3));
                    nxttris.Add(new SphTri(tri.v2, i2, i1));
                    nxttris.Add(new SphTri(tri.v3, i3, i2));
                    nxttris.Add(new SphTri(i1, i2, i3));
                }
                var cur = curtris;
                curtris = nxttris;
                nxttris = cur;
            }


            List<Vector4> vdata = new List<Vector4>();
            foreach (var vert in verts)
            {
                vdata.Add(new Vector4(vert, 1.0f));
            }

            List<uint> idata = new List<uint>();
            foreach (var tri in curtris)
            {
                idata.Add((uint)tri.v1);
                idata.Add((uint)(invert ? tri.v3 : tri.v2));
                idata.Add((uint)(invert ? tri.v2 : tri.v3));
            }


            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vdata.ToArray());
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
