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
    public class UnitQuad
    {
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }

        private VertexBufferBinding vbbinding;

        public UnitQuad(Device device, bool invert = false)
        {
            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new[]
            {
                //position (x4), texture (x2)
                -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, 1.0f,       1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                -1.0f,  1.0f, 0.0f, 1.0f, 0.0f, 0.0f,       1.0f,  1.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                //new Vector4(-1.0f, -1.0f, 0.0f, 1.0f), new Vector4(1.0f, -1.0f, 0.0f, 1.0f),
                //new Vector4(-1.0f,  1.0f, 0.0f, 1.0f), new Vector4(1.0f,  1.0f, 0.0f, 1.0f),
                //new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.5f, -0.5f, 0.5f, 1.0f),
                //new Vector4(-0.5f,  0.5f, 0.5f, 1.0f), new Vector4(0.5f,  0.5f, 0.5f, 1.0f),
            });
            if (invert)
            {
                IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, new[]
                {
                    0u,1u,2u,1u,3u,2u
                });
            }
            else
            {
                IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, new[]
                {
                    0u,2u,1u,1u,2u,3u
                });
            }

            vbbinding = new VertexBufferBinding(VertexBuffer, 24, 0);
        }


        public void Draw(DeviceContext context)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexed(6, 0, 0);
        }
        public void DrawInstanced(DeviceContext context, int instcount)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            context.DrawIndexedInstanced(6, instcount, 0, 0, 0);
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
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
            };
        }

    }
}
