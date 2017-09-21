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
    public class UnitCube
    {
        private Buffer VertexBuffer { get; set; }
        private Buffer IndexBuffer { get; set; }
        private InputLayout InputLayout { get; set; }
        private VertexBufferBinding vbbinding;
        private bool issigned;
        private bool islines;
        private bool isnormals;
        private int indexcount;

        public UnitCube(Device device, byte[] vsbytes, bool signed, bool lines, bool normals)
        {
            issigned = signed;
            islines = lines;
            isnormals = normals;

            if (normals)
            {
                InputLayout = new InputLayout(device, vsbytes, new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                });
            }
            else
            {
                InputLayout = new InputLayout(device, vsbytes, new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                });
            }


            if (signed)
            {
                if (normals)
                {
                    VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                    {
                        //position (x4), normal (x3)
                        //-Z face
                        -1.0f, -1.0f, -1.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,       1.0f, -1.0f, -1.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,
                        -1.0f,  1.0f, -1.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,       1.0f,  1.0f, -1.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,
                        //+Z face
                        -1.0f, -1.0f,  1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,        1.0f, -1.0f,  1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,
                        -1.0f,  1.0f,  1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,        1.0f,  1.0f,  1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,
                        //-Y face
                        -1.0f, -1.0f, -1.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,       1.0f, -1.0f, -1.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,
                        -1.0f, -1.0f,  1.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,       1.0f, -1.0f,  1.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,
                        //+Y face
                        -1.0f,  1.0f, -1.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,        1.0f,  1.0f, -1.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,
                        -1.0f,  1.0f,  1.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,        1.0f,  1.0f,  1.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,
                        //-X face
                        -1.0f, -1.0f, -1.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,      -1.0f,  1.0f, -1.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,
                        -1.0f, -1.0f,  1.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,      -1.0f,  1.0f,  1.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,
                        //+X face
                         1.0f, -1.0f, -1.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,        1.0f,  1.0f, -1.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,
                         1.0f, -1.0f,  1.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,        1.0f,  1.0f,  1.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,
                    });
                    vbbinding = new VertexBufferBinding(VertexBuffer, 32, 0);
                }
                else
                {
                    VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                    {
                        //position (x4)
                        -1.0f, -1.0f, -1.0f, 1.0f,       1.0f, -1.0f, -1.0f, 1.0f,
                        -1.0f,  1.0f, -1.0f, 1.0f,       1.0f,  1.0f, -1.0f, 1.0f,
                        -1.0f, -1.0f,  1.0f, 1.0f,       1.0f, -1.0f,  1.0f, 1.0f,
                        -1.0f,  1.0f,  1.0f, 1.0f,       1.0f,  1.0f,  1.0f, 1.0f,
                    });
                    vbbinding = new VertexBufferBinding(VertexBuffer, 16, 0);
                }
            }
            else
            {
                if (normals)
                {
                    VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                    {
                        //position (x4), normal (x3)
                        //-Z face
                        0.0f, 0.0f, 0.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,      1.0f, 0.0f, 0.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,
                        0.0f, 1.0f, 0.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,      1.0f, 1.0f, 0.0f, 1.0f,  0.0f,0.0f,-1.0f,0.0f,
                        //+Z face
                        0.0f, 0.0f, 1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,       1.0f, 0.0f, 1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,
                        0.0f, 1.0f, 1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,       1.0f, 1.0f, 1.0f, 1.0f,  0.0f,0.0f,1.0f,0.0f,
                        //-Y face
                        0.0f, 0.0f, 0.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,      1.0f, 0.0f, 0.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,
                        0.0f, 0.0f, 1.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,      1.0f, 0.0f, 1.0f, 1.0f,  0.0f,-1.0f,0.0f,0.0f,
                        //+Y face
                        0.0f, 1.0f, 0.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,       1.0f, 1.0f, 0.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,
                        0.0f, 1.0f, 1.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,       1.0f, 1.0f, 1.0f, 1.0f,  0.0f,1.0f,0.0f,0.0f,
                        //-X face
                        0.0f, 0.0f, 0.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,      0.0f, 1.0f, 0.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,
                        0.0f, 0.0f, 1.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,      0.0f, 1.0f, 1.0f, 1.0f,  -1.0f,0.0f,0.0f,0.0f,
                        //+X face
                        1.0f, 0.0f, 0.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,       1.0f, 1.0f, 0.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,
                        1.0f, 0.0f, 1.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,       1.0f, 1.0f, 1.0f, 1.0f,  1.0f,0.0f,0.0f,0.0f,
                    });
                    vbbinding = new VertexBufferBinding(VertexBuffer, 32, 0);
                }
                else
                {
                    VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                    {
                        //position (x4)
                        0.0f, 0.0f, 0.0f, 1.0f,       1.0f, 0.0f, 0.0f, 1.0f,
                        0.0f, 1.0f, 0.0f, 1.0f,       1.0f, 1.0f, 0.0f, 1.0f,
                        0.0f, 0.0f, 1.0f, 1.0f,       1.0f, 0.0f, 1.0f, 1.0f,
                        0.0f, 1.0f, 1.0f, 1.0f,       1.0f, 1.0f, 1.0f, 1.0f,
                    });
                    vbbinding = new VertexBufferBinding(VertexBuffer, 16, 0);
                }
            }
            if (lines)
            {
                IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, new uint[]
                {
                    0,1,1,3,3,2,2,0,
                    4,5,5,7,7,6,6,4,
                    0,4,1,5,3,7,2,6,
                });
                indexcount = 24;
            }
            else
            {
                if (normals)
                {
                    IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, new uint[]
                    {
                        0,2,1,1,2,3,
                        4,5,6,5,7,6,
                        8,9,10,9,11,10,
                        12,14,13,13,14,15,
                        16,18,17,17,18,19,
                        20,21,22,21,23,22
                    });
                    indexcount = 36;
                }
                else
                {
                    IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, new uint[]
                    {
                        0,2,1,1,2,3,
                        4,5,6,5,7,6,
                        //todo: other faces
                    });
                    indexcount = 12; //36..
                }
           }

        }


        public void Draw(DeviceContext context)
        {
            context.InputAssembler.InputLayout = InputLayout;

            if (islines)
            {
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            }
            else
            {
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            }
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            context.DrawIndexed(indexcount, 0, 0);
        }

        public void DrawInstanced(DeviceContext context, int count)
        {
            context.InputAssembler.InputLayout = InputLayout;

            if (islines)
            {
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            }
            else
            {
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            }
            context.InputAssembler.SetVertexBuffers(0, vbbinding);
            context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

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
