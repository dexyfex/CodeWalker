using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;

namespace CodeWalker.GameFiles
{

    public static class TextureFormats
    {
        public static Format GetDXGIFormat(TextureFormat fmt)
        {
            Format format = Format.Unknown;
            switch (fmt)
            {
                // compressed
                case TextureFormat.D3DFMT_DXT1: format = Format.BC1_UNorm; break;
                case TextureFormat.D3DFMT_DXT3: format = Format.BC2_UNorm; break;
                case TextureFormat.D3DFMT_DXT5: format = Format.BC3_UNorm; break;
                case TextureFormat.D3DFMT_ATI1: format = Format.BC4_UNorm; break;
                case TextureFormat.D3DFMT_ATI2: format = Format.BC5_UNorm; break;
                case TextureFormat.D3DFMT_BC7: format = Format.BC7_UNorm; break;

                // uncompressed
                case TextureFormat.D3DFMT_A1R5G5B5: format = Format.B5G5R5A1_UNorm; break;
                case TextureFormat.D3DFMT_A8: format = Format.A8_UNorm; break;
                case TextureFormat.D3DFMT_A8B8G8R8: format = Format.R8G8B8A8_UNorm; break;
                case TextureFormat.D3DFMT_L8: format = Format.R8_UNorm; break;
                case TextureFormat.D3DFMT_A8R8G8B8: format = Format.B8G8R8A8_UNorm; break;
                case TextureFormat.D3DFMT_X8R8G8B8: format = Format.B8G8R8X8_UNorm; break;
            }
            return format;
        }

        public static int ByteSize(TextureFormat fmt)
        {
            switch (fmt)
            {
                // compressed
                case TextureFormat.D3DFMT_DXT1: return 4;// BC1_UNorm
                case TextureFormat.D3DFMT_DXT3: return 8;// BC2_UNorm
                case TextureFormat.D3DFMT_DXT5: return 8;// BC3_UNorm
                case TextureFormat.D3DFMT_ATI1: return 4;// BC4_UNorm
                case TextureFormat.D3DFMT_ATI2: return 8;// BC5_UNorm
                case TextureFormat.D3DFMT_BC7: return 8;// BC7_UNorm

                // uncompressed
                case TextureFormat.D3DFMT_A1R5G5B5: return 16;// B5G5R5A1_UNorm
                case TextureFormat.D3DFMT_A8: return 8;// A8_UNorm
                case TextureFormat.D3DFMT_A8B8G8R8: return 32;// R8G8B8A8_UNorm
                case TextureFormat.D3DFMT_L8: return 8;// R8_UNorm
                case TextureFormat.D3DFMT_A8R8G8B8: return 32;// B8G8R8A8_UNorm
                case TextureFormat.D3DFMT_X8R8G8B8: return 32;// B8G8R8X8_UNorm

                default: return 0;
            }
        }


        public static void ComputePitch(Format fmt, int width, int height, out int rowPitch, out int slicePitch, uint flags)
        {
            int nbw, nbh;
            switch (fmt)
            {
                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    nbw = Math.Max(1, (width + 3) / 4);
                    nbh = Math.Max(1, (height + 3) / 4);
                    rowPitch = nbw * 8;
                    slicePitch = rowPitch * nbh;
                    break;
                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.BC6H_Typeless:
                case Format.BC6H_Uf16:
                case Format.BC6H_Sf16:
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    nbw = Math.Max(1, (width + 3) / 4);
                    nbh = Math.Max(1, (height + 3) / 4);
                    rowPitch = nbw * 16;
                    slicePitch = rowPitch * nbh;
                    break;

                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                case Format.YUY2:
                    rowPitch = ((width + 1) >> 1) * 4;
                    slicePitch = rowPitch * height;
                    break;

                case Format.Y210:
                case Format.Y216:
                    rowPitch = ((width + 1) >> 1) * 8;
                    slicePitch = rowPitch * height;
                    break;

                case Format.NV12:
                case Format.Opaque420:
                    rowPitch = ((width + 1) >> 1) * 2;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1));
                    break;

                case Format.P010:
                case Format.P016:
                    //case Format.XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT:
                    //case Format.XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS:
                    //case Format.XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT:
                    rowPitch = ((width + 1) >> 1) * 4;
                    slicePitch = rowPitch * (height + ((height + 1) >> 1));
                    break;

                case Format.NV11:
                    rowPitch = ((width + 3) >> 2) * 4;
                    slicePitch = rowPitch * height * 2;
                    break;

                //case Format.WIN10_DXGI_FORMAT_P208:
                //    rowPitch = ((width + 1) >> 1) * 2;
                //    slicePitch = rowPitch * height * 2;
                //    break;

                //case Format.WIN10_DXGI_FORMAT_V208:
                //    rowPitch = width;
                //    slicePitch = rowPitch * (height + (((height + 1) >> 1) * 2));
                //    break;

                //case Format.WIN10_DXGI_FORMAT_V408:
                //    rowPitch = width;
                //    slicePitch = rowPitch * (height + ((height >> 1) * 4));
                //    break;

                default:
                    int bpp = FormatHelper.SizeOfInBytes(fmt) * 8;
                    // Default byte alignment
                    rowPitch = (width * bpp + 7) / 8;
                    slicePitch = rowPitch * height;
                    break;
            }
        }

    }

}
