using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D;

namespace CodeWalker.Rendering
{
    public class GpuVarsBuffer<T> where T:struct //shader vars buffer helper!
    {
        public int Size;
        public Buffer Buffer;
        public T Vars;
        public bool Flag;//for external use
        public GpuVarsBuffer(Device device)
        {
            Size = System.Runtime.InteropServices.Marshal.SizeOf<T>();// (sizeof(T));
            Buffer = new Buffer(device, Size, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);// //DXUtility::CreateShaderVarsBuffer(Size, name);
        }
        public void Dispose()
        {
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }
        public void Update(DeviceContext context)
        {
            try
            {
                var dataBox = context.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Utilities.Write(dataBox.DataPointer, ref Vars);
                context.UnmapSubresource(Buffer, 0);
            }
            catch { } //not much we can do about this except ignore it..
        }
        public void SetVSCBuffer(DeviceContext context, int slot)
        {
            context.VertexShader.SetConstantBuffer(slot, Buffer);
        }
        public void SetPSCBuffer(DeviceContext context, int slot)
        {
            context.PixelShader.SetConstantBuffer(slot, Buffer);
        }
    }

    public class GpuSBuffer<T> where T : struct //for static struct data as resource view
    {
        public int StructSize;
        public int StructCount;
        public int BufferSize;
        public Buffer Buffer;
        public ShaderResourceView SRV;
        public GpuSBuffer(Device device, T[] data)
        {
            StructCount = data.Length;
            StructSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();// (sizeof(T));
            BufferSize = StructCount * StructSize;
            //var ds = new DataStream(new DataPointer()
            Buffer = Buffer.Create<T>(device, BindFlags.ShaderResource, data, BufferSize, ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, StructSize);
            //Buffer = new Buffer(device, BufferSize, ResourceUsage.Default, BindFlags.ShaderResource, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, StructSize);
            SRV = DXUtility.CreateShaderResourceView(device, Buffer, SharpDX.DXGI.Format.Unknown, SharpDX.Direct3D.ShaderResourceViewDimension.Buffer, 0, 0, StructCount, 0);
        }
        public void Dispose()
        {
            if (SRV != null)
            {
                SRV.Dispose();
                SRV = null;
            }
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }
    }


    public class GpuCBuffer<T> where T : struct //Dynamic GPU buffer of items updated by CPU
    {
        public int StructSize;
        public int StructCount;
        public int BufferSize;
        public int CurrentCount;
        public Buffer Buffer;
        public ShaderResourceView SRV;
        public List<T> Data;
        public T[] DataArray;

        public GpuCBuffer(Device device, int count)
        {
            StructCount = count;
            StructSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();// (sizeof(T));
            BufferSize = StructCount * StructSize;
            //Buffer = Buffer.Create<T>(device, BindFlags.ShaderResource, null, BufferSize, ResourceUsage.Dynamic, CpuAccessFlags.Write, ResourceOptionFlags.BufferStructured, StructSize);
            Buffer = new Buffer(device, BufferSize, ResourceUsage.Dynamic, BindFlags.ShaderResource, CpuAccessFlags.Write, ResourceOptionFlags.BufferStructured, StructSize);
            SRV = DXUtility.CreateShaderResourceView(device, Buffer, SharpDX.DXGI.Format.Unknown, SharpDX.Direct3D.ShaderResourceViewDimension.Buffer, 0, 0, StructCount, 0);
            Data = new List<T>(count);
            DataArray = new T[count];
        }
        public void Dispose()
        {
            if (SRV != null)
            {
                SRV.Dispose();
                SRV = null;
            }
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }

        public void Clear()
        {
            Data.Clear();
            CurrentCount = 0;
        }
        public bool Add(T item)
        {
            if (CurrentCount < StructCount)
            {
                Data.Add(item);
                CurrentCount++;
                return true;
            }
            return false;
        }

        public void Update(DeviceContext context)
        {
            for (int i = 0; i < CurrentCount; i++)
            {
                DataArray[i] = Data[i];
            }
            var dataBox = context.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Utilities.Write(dataBox.DataPointer, DataArray, 0, CurrentCount);
            context.UnmapSubresource(Buffer, 0);
        }

        public void SetVSResource(DeviceContext context, int slot)
        {
            context.VertexShader.SetShaderResource(slot, SRV);
        }
        public void SetPSResource(DeviceContext context, int slot)
        {
            context.PixelShader.SetShaderResource(slot, SRV);
        }

    }


    public class GpuBuffer<T> where T : struct //Dynamic GPU buffer of items updated by compute shader
    {
        public int StructSize;
        public int StructCount;
        public int ItemTotalSize;
        public int ItemCount;
        public int Size;
        public Buffer Buffer;
        public ShaderResourceView SRV;
        public UnorderedAccessView UAV;

        public GpuBuffer(Device device, int itemSize, int itemCount)
        {
            StructSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();// sizeof(T)),
            StructCount = itemCount * itemSize;
            ItemTotalSize = itemSize * StructSize;
            ItemCount = itemCount;
            Size = StructSize * itemSize * itemCount;
            Buffer = DXUtility.CreateBuffer(device, Size, ResourceUsage.Default, BindFlags.ShaderResource | BindFlags.UnorderedAccess, 0, ResourceOptionFlags.BufferStructured, StructSize);
            SRV = DXUtility.CreateShaderResourceView(device, Buffer, Format.Unknown, ShaderResourceViewDimension.Buffer, 0, 0, StructCount, 0);
            UAV = DXUtility.CreateUnorderedAccessView(device, Buffer, Format.Unknown, UnorderedAccessViewDimension.Buffer, 0, StructCount, 0, 0);
        }
        public void Dispose()
        {
            if (UAV != null)
            {
                UAV.Dispose();
                UAV = null;
            }
            if (SRV != null)
            {
                SRV.Dispose();
                SRV = null;
            }
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }
    }


    public class GpuTexture //texture and render targets (depth, MS).
    {
        public Texture2D Texture;
        public Texture2D TextureMS;
        public Texture2D Depth;
        public Texture2D DepthMS;
        public RenderTargetView RTV;
        public DepthStencilView DSV;
        public RenderTargetView MSRTV;
        public DepthStencilView MSDSV;
        public ShaderResourceView SRV;
        public int VramUsage;
        public bool Multisampled;
        public bool UseDepth;

        public void Init(Device device, int w, int h, Format f, int sc, int sq, bool depth, Format df)
        {
            VramUsage = 0;
            Multisampled = (sc > 1);
            UseDepth = depth;
            ResourceUsage u = ResourceUsage.Default;
            BindFlags b = BindFlags.RenderTarget | BindFlags.ShaderResource;
            RenderTargetViewDimension rtvd = RenderTargetViewDimension.Texture2D;
            ShaderResourceViewDimension srvd = ShaderResourceViewDimension.Texture2D;// D3D11_SRV_DIMENSION_TEXTURE2D;
            int fs = DXUtility.ElementSize(f);
            int wh = w * h;
            BindFlags db = BindFlags.DepthStencil;// D3D11_BIND_DEPTH_STENCIL;
            DepthStencilViewDimension dsvd = DepthStencilViewDimension.Texture2D;

            Texture = DXUtility.CreateTexture2D(device, w, h, 1, 1, f, 1, 0, u, b, 0, 0);
            RTV = DXUtility.CreateRenderTargetView(device, Texture, f, rtvd, 0, 0, 0);
            SRV = DXUtility.CreateShaderResourceView(device, Texture, f, srvd, 1, 0, 0, 0);
            VramUsage += (wh * fs);

            if (Multisampled)
            {
                b = BindFlags.RenderTarget;
                rtvd = RenderTargetViewDimension.Texture2DMultisampled;
                dsvd = DepthStencilViewDimension.Texture2DMultisampled;

                TextureMS = DXUtility.CreateTexture2D(device, w, h, 1, 1, f, sc, sq, u, b, 0, 0);
                MSRTV = DXUtility.CreateRenderTargetView(device, TextureMS, f, rtvd, 0, 0, 0);
                VramUsage += (wh * fs);

                if (depth)
                {
                    DepthMS = DXUtility.CreateTexture2D(device, w, h, 1, 1, df, sc, sq, u, db, 0, 0);
                    MSDSV = DXUtility.CreateDepthStencilView(device, DepthMS, df, dsvd);
                    VramUsage += (wh * DXUtility.ElementSize(df));
                }
            }
            else
            {
                if (depth)
                {
                    Depth = DXUtility.CreateTexture2D(device, w, h, 1, 1, df, sc, sq, u, db, 0, 0);
                    DSV = DXUtility.CreateDepthStencilView(device, Depth, df, dsvd);
                    VramUsage += (wh * DXUtility.ElementSize(df));
                }
            }
        }
        public void Dispose()
        {
            if (SRV != null)
            {
                SRV.Dispose();
                SRV = null;
            }
            if (MSDSV != null)
            {
                MSDSV.Dispose();
                MSDSV = null;
            }
            if (MSRTV != null)
            {
                MSRTV.Dispose();
                MSRTV = null;
            }
            if (DSV != null)
            {
                DSV.Dispose();
                DSV = null;
            }
            if (RTV != null)
            {
                RTV.Dispose();
                RTV = null;
            }
            if (DepthMS != null)
            {
                DepthMS.Dispose();
                DepthMS = null;
            }
            if (Depth != null)
            {
                Depth.Dispose();
                Depth = null;
            }
            if (TextureMS != null)
            {
                TextureMS.Dispose();
                TextureMS = null;
            }
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }
        public GpuTexture(Device device, int w, int h, Format f, int sc, int sq, bool depth, Format df)
        {
            Init(device, w, h, f, sc, sq, depth, df);
        }
        public GpuTexture(Device device, int w, int h, Format f, int sc, int sq)
        {
            Init(device, w, h, f, sc, sq, false, Format.Unknown);
        }
        public GpuTexture(Device device, int w, int h, Format f)
        {
            Init(device, w, h, f, 1, 0, false, Format.Unknown);
        }

        public void Clear(DeviceContext context, Color4 colour)
        {
            if (Multisampled)
            {
                context.ClearRenderTargetView(MSRTV, colour);
                if (UseDepth)
                {
                    context.ClearDepthStencilView(MSDSV, DepthStencilClearFlags.Depth, 1.0f, 0);
                }
            }
            else
            {
                context.ClearRenderTargetView(RTV, colour);
                if (UseDepth)
                {
                    context.ClearDepthStencilView(DSV, DepthStencilClearFlags.Depth, 1.0f, 0);
                }
            }
        }

        public void ClearDepth(DeviceContext context)
        {
            if (!UseDepth) return;
            if (Multisampled)
            {
                context.ClearDepthStencilView(MSDSV, DepthStencilClearFlags.Depth, 1.0f, 0);
            }
            else
            {
                context.ClearDepthStencilView(DSV, DepthStencilClearFlags.Depth, 1.0f, 0);
            }
        }

        public void SetRenderTarget(DeviceContext context)
        {
            if (Multisampled)
            {
                context.OutputMerger.SetRenderTargets(UseDepth ? MSDSV : null, MSRTV);
            }
            else
            {
                context.OutputMerger.SetRenderTargets(UseDepth ? DSV : null, RTV);
            }
        }

    }


}
