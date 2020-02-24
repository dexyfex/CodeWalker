using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Resource = SharpDX.Direct3D11.Resource;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D;

namespace CodeWalker.Rendering
{
    public static class DXUtility
    {

        public static Buffer CreateBuffer(Device device, int size, ResourceUsage usage, BindFlags bindFlags, CpuAccessFlags cpuAccessFlags, ResourceOptionFlags miscFlags, int structByteStride)
        {
            BufferDescription desc = new BufferDescription();
            desc.SizeInBytes = size;
	        desc.Usage = usage;
	        desc.BindFlags = bindFlags;
            desc.CpuAccessFlags = cpuAccessFlags;
            desc.OptionFlags = miscFlags;
            desc.StructureByteStride = structByteStride;

            Buffer b = new Buffer(device, desc);

            //D3D11_SUBRESOURCE_DATA srd;
            //srd.pSysMem = data;
            //ComPtr<ID3D11Buffer> b;

            //Try(DXManager::GetDevice()->CreateBuffer(&desc, data != nullptr? &srd : nullptr, &b), name);
            //DXManager::AddVramUsage(size);
            return b;
        }


        public static Texture2D CreateTexture2D(Device device, int width, int height, int mipLevels, int arraySize, Format format, int sampleCount, int sampleQuality, ResourceUsage usage, BindFlags bindFlags, CpuAccessFlags cpuAccessFlags, ResourceOptionFlags miscFlags)
        {
            Texture2DDescription td = new Texture2DDescription();
            td.Width = width;
            td.Height = height;
            td.MipLevels = mipLevels;
            td.ArraySize = arraySize;
            td.Format = format;
            td.SampleDescription = new SampleDescription(sampleCount, sampleQuality);
            td.Usage = usage;
            td.BindFlags = bindFlags;
            td.CpuAccessFlags = cpuAccessFlags;
            td.OptionFlags = miscFlags;
            Texture2D t = new Texture2D(device, td);
            return t;
        }
        //static ComPtr<ID3D11Texture2D> CreateTexture2D(UINT width, UINT height, UINT mipLevels, UINT arraySize, DXGI_FORMAT format, UINT sampleCount, UINT sampleQuality, D3D11_USAGE usage, UINT bindFlags, UINT cpuAccessFlags, UINT miscFlags, const void* data, const string& name)
        //{
        //    D3D11_TEXTURE2D_DESC td;
        //    td.Width = width;
        //    td.Height = height;
        //    td.MipLevels = mipLevels;
        //    td.ArraySize = arraySize;
        //    td.Format = format;
        //    td.SampleDesc.Count = sampleCount;
        //    td.SampleDesc.Quality = sampleQuality;
        //    td.Usage = usage;
        //    td.BindFlags = bindFlags;
        //    td.CPUAccessFlags = cpuAccessFlags;
        //    td.MiscFlags = miscFlags;
        //    D3D11_SUBRESOURCE_DATA srd;
        //    srd.pSysMem = data;
        //    srd.SysMemPitch = 1;
        //    srd.SysMemSlicePitch = 0;
        //    ComPtr<ID3D11Texture2D> t;
        //    Try(DXManager::GetDevice()->CreateTexture2D(&td, data != nullptr ? &srd : nullptr, &t), name);
        //    DXManager::AddVramUsage(ElementSize(format) * width * height * arraySize);
        //    return t;
        //}


        public static SamplerState CreateSamplerState(Device device, TextureAddressMode addressU, TextureAddressMode addressV, TextureAddressMode addressW, RawColor4 border, Comparison comparisonFunc, Filter filter, int maxAnisotropy, float maxLOD, float minLOD, float mipLODBias)
        {
            SamplerStateDescription smpDesc = new SamplerStateDescription();
            smpDesc.AddressU = addressU;
            smpDesc.AddressV = addressV;
            smpDesc.AddressW = addressW;
            smpDesc.BorderColor = border;
            smpDesc.ComparisonFunction = comparisonFunc;
            smpDesc.Filter = filter;
            smpDesc.MaximumAnisotropy = maxAnisotropy;
            smpDesc.MaximumLod = maxLOD;
            smpDesc.MinimumLod = minLOD;
            smpDesc.MipLodBias = mipLODBias;
            SamplerState smp = new SamplerState(device, smpDesc);
            return smp;
        }
        public static SamplerState CreateSamplerState(Device device, TextureAddressMode address, RawColor4 border, Comparison comparisonFunc, Filter filter, int maxAnisotropy, float maxLOD, float minLOD, float mipLODBias)
        {
            return CreateSamplerState(device, address, address, address, border, comparisonFunc, filter, maxAnisotropy, maxLOD, minLOD, mipLODBias);
        }

        public static ShaderResourceView CreateShaderResourceView(Device device, Resource resource, Format format, ShaderResourceViewDimension viewDimension, int mipLevels, int mostDetailedMip, int arraySize, int firstArraySlice)
        {
            ShaderResourceViewDescription srvd = new ShaderResourceViewDescription();
            srvd.Format = format;
            srvd.Dimension = viewDimension;
            switch (viewDimension)
            {
                case ShaderResourceViewDimension.Buffer://  D3D11_SRV_DIMENSION_BUFFER:
                    srvd.Buffer.ElementOffset = mipLevels;
                    srvd.Buffer.ElementWidth = ElementSize(format);
                    srvd.Buffer.ElementCount = arraySize;
                    srvd.Buffer.FirstElement = firstArraySlice;
                    break;
                case ShaderResourceViewDimension.Texture2D:// D3D11_SRV_DIMENSION_TEXTURE2D:
                    srvd.Texture2D.MipLevels = mipLevels;
                    srvd.Texture2D.MostDetailedMip = mostDetailedMip;
                    break;
                case ShaderResourceViewDimension.Texture2DArray:// D3D11_SRV_DIMENSION_TEXTURE2DARRAY:
                    srvd.Texture2DArray.MipLevels = mipLevels;
                    srvd.Texture2DArray.MostDetailedMip = mostDetailedMip;
                    srvd.Texture2DArray.ArraySize = arraySize;
                    srvd.Texture2DArray.FirstArraySlice = firstArraySlice;
                    break;
                case ShaderResourceViewDimension.TextureCube:// D3D11_SRV_DIMENSION_TEXTURECUBE:
                    srvd.TextureCube.MipLevels = mipLevels;
                    srvd.TextureCube.MostDetailedMip = mostDetailedMip;
                    break;
                case ShaderResourceViewDimension.Texture3D:// D3D11_SRV_DIMENSION_TEXTURE3D:
                    srvd.Texture3D.MipLevels = mipLevels;
                    srvd.Texture3D.MostDetailedMip = mostDetailedMip;
                    break;
                case ShaderResourceViewDimension.Texture2DMultisampled:
                case ShaderResourceViewDimension.Texture2DMultisampledArray:
                    //nothing to do here
                    break;
                default:
                    throw new Exception(); //not implemented....
            }
            ShaderResourceView srv = new ShaderResourceView(device, resource, srvd);
            return srv;
        }

        public static UnorderedAccessView CreateUnorderedAccessView(Device device, Resource resource, Format format, UnorderedAccessViewDimension viewDimension, int firstElement, int numElements, UnorderedAccessViewBufferFlags flags, int mipSlice)
        {
            UnorderedAccessViewDescription uavd = new UnorderedAccessViewDescription();
            uavd.Format = format;
            uavd.Dimension = viewDimension;

            switch(viewDimension) 
            {
            case UnorderedAccessViewDimension.Texture1D:
                uavd.Texture1D.MipSlice = mipSlice;
                break;
            case UnorderedAccessViewDimension.Texture1DArray:
                uavd.Texture1DArray.MipSlice = mipSlice;
                uavd.Texture1DArray.ArraySize = numElements;
                uavd.Texture1DArray.FirstArraySlice = firstElement;
                break;
            case UnorderedAccessViewDimension.Texture2D:
                uavd.Texture2D.MipSlice = mipSlice;
                break;
            case UnorderedAccessViewDimension.Texture2DArray:
                uavd.Texture2DArray.MipSlice = mipSlice;
                uavd.Texture2DArray.ArraySize = numElements;
                uavd.Texture2DArray.FirstArraySlice = firstElement;
                break;
            case UnorderedAccessViewDimension.Texture3D:
                uavd.Texture3D.MipSlice = mipSlice;
                uavd.Texture3D.WSize = numElements;
                uavd.Texture3D.FirstWSlice = firstElement;
                break;
            case UnorderedAccessViewDimension.Buffer:
                uavd.Buffer.ElementCount = numElements;
                uavd.Buffer.FirstElement = firstElement;
                uavd.Buffer.Flags = flags;
                break;
            case UnorderedAccessViewDimension.Unknown:
            default:
                return null;
            }
            var uav = new UnorderedAccessView(device, resource, uavd);
            return uav;
        }


        public static RenderTargetView CreateRenderTargetView(Device device, Resource renderTarget, Format format, RenderTargetViewDimension viewDimension, int mipSlice, int arraySize, int firstArraySlice)
        {
            RenderTargetView rtv;
            RenderTargetViewDescription rtvd = new RenderTargetViewDescription();
            rtvd.Format = format;
            rtvd.Dimension = viewDimension;
            switch(viewDimension) 
            {
                case RenderTargetViewDimension.Buffer:// D3D11_RTV_DIMENSION_BUFFER:
                    rtvd.Buffer.ElementOffset = mipSlice;
                    rtvd.Buffer.ElementWidth = arraySize* ElementSize(format);// arraySize; //assume square buffer... is this the width?
                    rtvd.Buffer.FirstElement = 0*ElementSize(format);//firstArraySlice;
                    rtvd.Buffer.ElementCount = arraySize;//*arraySize*ElementSize(format); //does this represent the height??
                    break;
                case RenderTargetViewDimension.Texture2D:// D3D11_RTV_DIMENSION_TEXTURE2D:
                    rtvd.Texture2D.MipSlice = mipSlice;
                    break;
                case RenderTargetViewDimension.Texture2DArray:// D3D11_RTV_DIMENSION_TEXTURE2DARRAY:
                    rtvd.Texture2DArray.MipSlice = mipSlice;
                    rtvd.Texture2DArray.ArraySize = arraySize;
                    rtvd.Texture2DArray.FirstArraySlice = firstArraySlice;
                    break;
                case RenderTargetViewDimension.Texture2DMultisampled:// D3D11_RTV_DIMENSION_TEXTURE2DMS:
                    break;
                case RenderTargetViewDimension.Texture2DMultisampledArray:// D3D11_RTV_DIMENSION_TEXTURE2DMSARRAY:
                    rtvd.Texture2DMSArray.ArraySize = arraySize;
                    rtvd.Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    break;
                case RenderTargetViewDimension.Texture3D:// D3D11_RTV_DIMENSION_TEXTURE3D:
                    rtvd.Texture3D.MipSlice = mipSlice;
                    rtvd.Texture3D.DepthSliceCount = arraySize;
                    rtvd.Texture3D.FirstDepthSlice = firstArraySlice;
                    break;
            }
            rtv = new RenderTargetView(device, renderTarget, rtvd);
            return rtv;
        }


        public static DepthStencilView CreateDepthStencilView(Device device, Texture2D depthStencil, Format format, DepthStencilViewDimension viewDimension)
        {
            DepthStencilViewDescription dsvd = new DepthStencilViewDescription();
            dsvd.Format = format;
            dsvd.Flags = 0;
            dsvd.Dimension = viewDimension;
            dsvd.Texture2D.MipSlice = 0;
            DepthStencilView dsv = new DepthStencilView(device, depthStencil, dsvd);
            return dsv;
        }
        public static DepthStencilView CreateDepthStencilView(Device device, Texture2D depthStencil, Format format, int arraySlice)
        {
            DepthStencilViewDescription dsvd = new DepthStencilViewDescription();
            dsvd.Format = format;
            dsvd.Flags = 0;
            dsvd.Dimension = DepthStencilViewDimension.Texture2DArray;// D3D11_DSV_DIMENSION_TEXTURE2DARRAY;
            dsvd.Texture2DArray.ArraySize = 1;
            dsvd.Texture2DArray.FirstArraySlice = arraySlice;
            dsvd.Texture2DArray.MipSlice = 0;
            DepthStencilView dsv = new DepthStencilView(device, depthStencil, dsvd);
            return dsv;
        }


        public static DepthStencilState CreateDepthStencilState(Device device, bool depthEnable, DepthWriteMask writeMask, Comparison func, bool stencilEnable, byte stencilReadMask, byte stencilWriteMask, DepthStencilOperationDescription frontFace, DepthStencilOperationDescription backFace)
        {
            DepthStencilStateDescription dsd;
            dsd.IsDepthEnabled = depthEnable;
            dsd.DepthWriteMask = writeMask;
            dsd.DepthComparison = func;
            dsd.IsStencilEnabled = stencilEnable;
            dsd.StencilReadMask = stencilReadMask;
            dsd.StencilWriteMask = stencilWriteMask;
            dsd.FrontFace = frontFace;
            dsd.BackFace = backFace;
            DepthStencilState s = new DepthStencilState(device, dsd);
            return s;
        }
        public static DepthStencilState CreateDepthStencilState(Device device, bool depthEnable, DepthWriteMask writeMask)
        {
            DepthStencilOperationDescription frontFace = new DepthStencilOperationDescription();
            DepthStencilOperationDescription backFace = new DepthStencilOperationDescription();
            bool stencil = false;//depthEnable;
            byte rm = 0;
            byte wm = 0;
            if (stencil)
            {
                rm = 0xFF;
                wm = 0xFF;
                // Stencil operations if pixel is front-facing
                frontFace.FailOperation = StencilOperation.Keep;// D3D11_STENCIL_OP_KEEP;
                frontFace.DepthFailOperation = StencilOperation.Increment;// D3D11_STENCIL_OP_INCR;
                frontFace.PassOperation = StencilOperation.Keep;// D3D11_STENCIL_OP_KEEP;
                frontFace.Comparison = Comparison.Always;// D3D11_COMPARISON_ALWAYS;

                // Stencil operations if pixel is back-facing
                backFace.FailOperation = StencilOperation.Keep;// D3D11_STENCIL_OP_KEEP;
                backFace.DepthFailOperation = StencilOperation.Decrement;// D3D11_STENCIL_OP_DECR;
                backFace.PassOperation = StencilOperation.Keep;// D3D11_STENCIL_OP_KEEP;
                backFace.Comparison = Comparison.Always;// D3D11_COMPARISON_ALWAYS;
            }

            return CreateDepthStencilState(device, depthEnable, writeMask, Comparison.LessEqual, stencil, rm, wm, frontFace, backFace);
        }


        public static RasterizerState CreateRasterizerState(Device device, FillMode fillMode, CullMode cullMode, bool depthClipEnable, bool scissorEnable, bool multisampleEnable, int depthBias, float depthBiasClamp, float slopeScaledDepthBias)
        {
            RasterizerStateDescription drd = new RasterizerStateDescription();
            drd.FillMode = fillMode; //D3D11_FILL_MODE FillMode;
            drd.CullMode = cullMode;//D3D11_CULL_MODE CullMode;
            drd.IsFrontCounterClockwise = false; //BOOL FrontCounterClockwise;
            drd.DepthBias = depthBias; //INT DepthBias;
            drd.DepthBiasClamp = depthBiasClamp;//FLOAT DepthBiasClamp;
            drd.SlopeScaledDepthBias = slopeScaledDepthBias;//FLOAT SlopeScaledDepthBias;
            drd.IsDepthClipEnabled = depthClipEnable;//BOOL DepthClipEnable;
            drd.IsScissorEnabled = scissorEnable;//BOOL ScissorEnable;
            drd.IsMultisampleEnabled = multisampleEnable;//BOOL MultisampleEnable;
            drd.IsAntialiasedLineEnabled = false;//BOOL AntialiasedLineEnable;        
            RasterizerState rs = new RasterizerState(device, drd);
            return rs;
        }
        public static RasterizerState CreateRasterizerState(Device device, FillMode fillMode, CullMode cullMode, bool depthClipEnable, bool scissorEnable, bool multisampleEnable)
        {
            return CreateRasterizerState(device, fillMode, cullMode, depthClipEnable, scissorEnable, multisampleEnable, 0, 0.0f, 0.0f);
        }
        public static RasterizerState CreateRasterizerState(Device device, FillMode fillMode, CullMode cullMode, bool depthClipEnable, bool multisampleEnable)
        {
            return CreateRasterizerState(device, fillMode, cullMode, depthClipEnable, false, multisampleEnable);
        }
        public static RasterizerState CreateRasterizerState(Device device, bool depthClipEnable, bool multisampleEnable)
        {
            return CreateRasterizerState(device, FillMode.Solid, CullMode.Back, depthClipEnable, false, multisampleEnable);
        }

        public static BlendState CreateBlendState(Device device, bool blendEnable, BlendOperation op, BlendOption src, BlendOption dst, BlendOperation opAlpha, BlendOption srcAlpha, BlendOption dstAlpha, ColorWriteMaskFlags writeMask)
        {
            BlendStateDescription bsd = new BlendStateDescription();
            //ZeroMemory(&bsd, sizeof(bsd));
            bsd.RenderTarget[0].IsBlendEnabled = blendEnable;
            bsd.RenderTarget[0].BlendOperation = op;
            bsd.RenderTarget[0].SourceBlend = src;
            bsd.RenderTarget[0].DestinationBlend = dst;
            bsd.RenderTarget[0].AlphaBlendOperation = opAlpha;
            bsd.RenderTarget[0].SourceAlphaBlend = srcAlpha;
            bsd.RenderTarget[0].DestinationAlphaBlend = dstAlpha;
            bsd.RenderTarget[0].RenderTargetWriteMask = writeMask;
            BlendState bs = new BlendState(device, bsd);
            return bs;
        }


        public static int ElementSize(Format format)
        {
            //FormatHelper.SizeOfInBytes?
            switch (format)
            {
                case Format.R32G32B32A32_Typeless:
                case Format.R32G32B32A32_Float:
                case Format.R32G32B32A32_UInt:
                case Format.R32G32B32A32_SInt:
                    return 16;

                case Format.R32G32B32_Typeless:
                case Format.R32G32B32_Float:
                case Format.R32G32B32_UInt:
                case Format.R32G32B32_SInt:
                    return 12;

                case Format.R16G16B16A16_Typeless:
                case Format.R16G16B16A16_Float:
                case Format.R16G16B16A16_UNorm:
                case Format.R16G16B16A16_UInt:
                case Format.R16G16B16A16_SNorm:
                case Format.R16G16B16A16_SInt:
                case Format.R32G32_Typeless:
                case Format.R32G32_Float:
                case Format.R32G32_UInt:
                case Format.R32G32_SInt:
                case Format.R32G8X24_Typeless:
                case Format.D32_Float_S8X24_UInt:
                case Format.R32_Float_X8X24_Typeless:
                case Format.X32_Typeless_G8X24_UInt:
                    return 8;

                case Format.R10G10B10A2_Typeless:
                case Format.R10G10B10A2_UNorm:
                case Format.R10G10B10A2_UInt:
                case Format.R11G11B10_Float:
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRgb:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_SNorm:
                case Format.R8G8B8A8_SInt:
                case Format.R16G16_Typeless:
                case Format.R16G16_Float:
                case Format.R16G16_UNorm:
                case Format.R16G16_UInt:
                case Format.R16G16_SNorm:
                case Format.R16G16_SInt:
                case Format.R32_Typeless:
                case Format.D32_Float:
                case Format.R32_Float:
                case Format.R32_UInt:
                case Format.R32_SInt:
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                case Format.R24_UNorm_X8_Typeless:
                case Format.X24_Typeless_G8_UInt:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8X8_UNorm:
                    return 4;

                case Format.R8G8_Typeless:
                case Format.R8G8_UNorm:
                case Format.R8G8_UInt:
                case Format.R8G8_SNorm:
                case Format.R8G8_SInt:
                case Format.R16_Typeless:
                case Format.R16_Float:
                case Format.D16_UNorm:
                case Format.R16_UNorm:
                case Format.R16_UInt:
                case Format.R16_SNorm:
                case Format.R16_SInt:
                case Format.B5G6R5_UNorm:
                case Format.B5G5R5A1_UNorm:
                    return 2;

                case Format.R8_Typeless:
                case Format.R8_UNorm:
                case Format.R8_UInt:
                case Format.R8_SNorm:
                case Format.R8_SInt:
                case Format.A8_UNorm:
                    return 1;

                // Compressed format; http://msdn2.microsoft.com/en-us/library/bb694531(VS.85).aspx
                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                    return 16;

                // Compressed format; http://msdn2.microsoft.com/en-us/library/bb694531(VS.85).aspx
                case Format.R1_UNorm:
                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    return 8;

                // Compressed format; http://msdn2.microsoft.com/en-us/library/bb694531(VS.85).aspx
                case Format.R9G9B9E5_Sharedexp:
                    return 4;

                // These are compressed, but bit-size information is unclear.
                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                    return 4;

                case Format.Unknown:
                default:
                    return 0;
            }
        }




    }
}
