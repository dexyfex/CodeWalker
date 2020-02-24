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
    public class UnitDisc
    {
        public int SegmentCount { get; set; }
        public int IndexCount { get; set; }
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }

        private VertexBufferBinding vbbinding;

        public UnitDisc(Device device, int segmentCount, bool invert = false)
        {
            SegmentCount = segmentCount;
            List<Vector3> verts = new List<Vector3>();
            List<uint> inds = new List<uint>();
            verts.Add(Vector3.Zero);
            float incr = (float)Math.PI * 2.0f / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                float a = incr * i;
                float px = (float)Math.Sin(a);
                float py = (float)Math.Cos(a);
                verts.Add(new Vector3(px, py, 0));
            }
            for (int i = 0; i < segmentCount; i++)
            {
                uint ci = (uint)((i == 0) ? segmentCount : i);
                uint ni = (uint)i + 1;
                inds.Add(0);
                inds.Add(invert ? ni : ci);
                inds.Add(invert ? ci : ni);
            }
            IndexCount = inds.Count;

            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, verts.ToArray());
            IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, inds.ToArray());
            vbbinding = new VertexBufferBinding(VertexBuffer, 12, 0);
        }


        public void Draw(DeviceContext context)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexed(IndexCount, 0, 0);
        }
        public void DrawInstanced(DeviceContext context, int instcount)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexedInstanced(IndexCount, instcount, 0, 0, 0);
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
        }

        public InputElement[] GetLayout()
        {
            return new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            };
        }


    }
}
