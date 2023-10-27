/*
	Copyright(c) 2015 Neodymium

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

//DDSIO: stolen and translated




// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License

//DecompressDXT: ripped from MonoGame - todo: reinterpret to remove stupid license



//additional dds importing code modified from: https://gist.github.com/spazzarama/2710d020d1d615cde20c607711655167
// DDSTextureLoader Ported to C# by Justin Stenning, March 2017
//--------------------------------------------------------------------------------------
// File: DDSTextureLoader.cpp
//
// Functions for loading a DDS texture and creating a Direct3D runtime resource for it
//
// Note these functions are useful as a light-weight runtime loader for DDS files. For
// a full-featured DDS file reader, writer, and texture processing pipeline see
// the 'Texconv' sample and the 'DirectXTex' library.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
// http://go.microsoft.com/fwlink/?LinkId=248929
//--------------------------------------------------------------------------------------





using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Utils
{
    public class AssertionFailedException : Exception
    {
        public AssertionFailedException() : base("Assertion failed")
        {
        }

        public AssertionFailedException(string message) : base(message)
        {
        }

        public AssertionFailedException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public static class DDSIO
    {


        public static byte[] GetPixels(Texture texture, int mip)
        {
            //dexyfex version
            var format = GetDXGIFormat(texture.Format);
            ImageStruct img = GetImageStruct(texture, format);
            Image[] images = GetMipmapImages(img, format);
            TexMetadata meta = GetImageMetadata(img, format);

            if (meta.dimension != TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D)
                throw new Exception("Can only GetPixels from Texture2D.");


            var i0 = images[Math.Min(Math.Max(mip, 0), images.Length - 1)];

            int ddsRowPitch, ddsSlicePitch;
            DXTex.ComputePitch(meta.format, i0.width, i0.height, out ddsRowPitch, out ddsSlicePitch, 0);

            if (i0.slicePitch == ddsSlicePitch)
            { }
            else
            { }

            int w = i0.width;
            int h = i0.height;
            int imglen = i0.slicePitch;// h * i0.rowPitch;
            byte[] imgdata = new byte[imglen];
            byte[] px = null;// = new byte[w * h * 4];

            if (i0.pixels + imglen > img.Data.Length)
            { throw new Exception("Mipmap not in texture!"); }

            Buffer.BlockCopy(img.Data, i0.pixels, imgdata, 0, imglen);

            bool swaprb = true;

            switch (format)
            {
                // compressed
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM: // TextureFormat.D3DFMT_DXT1
                    px = DecompressDxt1(imgdata, w, h);
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM: // TextureFormat.D3DFMT_DXT3
                    px = DecompressDxt3(imgdata, w, h);
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM: // TextureFormat.D3DFMT_DXT5
                    px = DecompressDxt5(imgdata, w, h);
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM: // TextureFormat.D3DFMT_ATI1
                    px = DecompressBC4(imgdata, w, h);
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM: // TextureFormat.D3DFMT_ATI2
                    px = DecompressBC5(imgdata, w, h);
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM: // TextureFormat.D3DFMT_BC7
                    //BC7 TODO!!
                    break;

                // uncompressed
                case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM: // TextureFormat.D3DFMT_A1R5G5B5
                    px = ConvertBGR5A1ToRGBA8(imgdata, w, h); //needs testing
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:       // TextureFormat.D3DFMT_A8
                    px = ConvertA8ToRGBA8(imgdata, w, h);
                    swaprb = false;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM: // TextureFormat.D3DFMT_A8B8G8R8
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS:
                    px = imgdata;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:       // TextureFormat.D3DFMT_L8
                    px = ConvertR8ToRGBA8(imgdata, w, h);
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM: // TextureFormat.D3DFMT_A8R8G8B8
                    px = imgdata;
                    swaprb = false;
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM: // TextureFormat.D3DFMT_X8R8G8B8
                    px = imgdata;
                    swaprb = false;
                    break;
                default:
                    break; //shouldn't get here...
            }

            if (swaprb && (px != null))
            {
                int pxc = px.Length / 4;// w * h;
                for (int i = 0; i < pxc; i++)
                {
                    int ib = i * 4;
                    byte p2 = px[ib + 2];
                    px[ib + 2] = px[ib];
                    px[ib] = p2;
                }
            }


            return px;
        }



        public static byte[] GetDDSFile(Texture texture)
        {
            var format = GetDXGIFormat(texture.Format);
            ImageStruct img = GetImageStruct(texture, format);
            Image[] images = GetMipmapImages(img, format);
            TexMetadata meta = GetImageMetadata(img, format);




            using MemoryStream ms = new MemoryStream(texture.Data.FullData.Length + 128);
            using BinaryWriter bw = new BinaryWriter(ms);

            int nimages = img.MipMapLevels;

            // Create DDS Header
            int required;
            if (!DXTex._EncodeDDSHeader(meta, 0, bw, out required))
            {
                throw new Exception("Couldn't make DDS header");
            }

            // Write images
            switch (meta.dimension)
            {
                case TEX_DIMENSION.TEX_DIMENSION_TEXTURE1D:
                case TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D:
                    {
                        int index = 0;
                        for (int item = 0; item < meta.arraySize; ++item)
                        {
                            for (int level = 0; level < meta.mipLevels; ++level, ++index)
                            {
                                if (index >= nimages)
                                    throw new Exception("Tried to write mip out of range");
                                if (images[index].rowPitch <= 0)
                                    throw new Exception("Invalid row pitch.");
                                if (images[index].slicePitch <= 0)
                                    throw new Exception("Invalid slice pitch.");
                                //if (images[index].pixels)
                                //    return E_POINTER;

                                int ddsRowPitch, ddsSlicePitch;
                                DXTex.ComputePitch(meta.format, images[index].width, images[index].height, out ddsRowPitch, out ddsSlicePitch, 0);// CP_FLAGS.CP_FLAGS_NONE);

                                if (images[index].slicePitch == ddsSlicePitch)
                                {
                                    int lengt = ddsSlicePitch;
                                    if (images[index].pixels + ddsSlicePitch > img.Data.Length)
                                    {
                                        lengt = img.Data.Length - images[index].pixels;
                                        if (lengt <= 0)
                                        {
                                            //throw new Exception("Not enough data to read...");
                                        }
                                    }
                                    if (lengt > 0)
                                    {
                                        bw.Write(img.Data, images[index].pixels, lengt);
                                    }
                                }
                                else
                                {
                                    int rowPitch = images[index].rowPitch;
                                    if (rowPitch < ddsRowPitch)
                                    {
                                        // DDS uses 1-byte alignment, so if this is happening then the input pitch isn't actually a full line of data
                                        throw new Exception("Input pitch isn't a full line of data");
                                    }

                                    int sPtr = images[index].pixels;

                                    int lines = DXTex.ComputeScanlines(meta.format, images[index].height);
                                    for (int j = 0; j < lines; ++j)
                                    {
                                        bw.Write(img.Data, sPtr, ddsRowPitch);
                                        sPtr += rowPitch;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case TEX_DIMENSION.TEX_DIMENSION_TEXTURE3D:
                    {
                        if (meta.arraySize != 1)
                            throw new Exception("Texture3D must have arraySize == 1"); //return null;// E_FAIL;

                        int d = meta.depth;

                        int index = 0;
                        for (int level = 0; level < meta.mipLevels; ++level)
                        {
                            for (int slice = 0; slice < d; ++slice, ++index)
                            {
                                if (index >= nimages)
                                    throw new Exception("Tried to write mip out of range");
                                if (images[index].rowPitch <= 0)
                                    throw new Exception("Invalid row pitch.");
                                if (images[index].slicePitch <= 0)
                                    throw new Exception("Invalid slice pitch.");
                                //if (!images[index].pixels)
                                //    return E_POINTER;

                                int ddsRowPitch, ddsSlicePitch;
                                DXTex.ComputePitch(meta.format, images[index].width, images[index].height, out ddsRowPitch, out ddsSlicePitch, 0);// CP_FLAGS_NONE);

                                if (images[index].slicePitch == ddsSlicePitch)
                                {
                                    bw.Write(img.Data, images[index].pixels, ddsSlicePitch);
                                }
                                else
                                {
                                    int rowPitch = images[index].rowPitch;
                                    if (rowPitch < ddsRowPitch)
                                    {
                                        // DDS uses 1-byte alignment, so if this is happening then the input pitch isn't actually a full line of data
                                        throw new Exception("Input pitch isn't a full line of data");
                                    }

                                    int sPtr = images[index].pixels;

                                    int lines = DXTex.ComputeScanlines(meta.format, images[index].height);
                                    for (int j = 0; j < lines; ++j)
                                    {
                                        bw.Write(img.Data, sPtr, ddsRowPitch);
                                        sPtr += rowPitch;
                                    }
                                }
                            }

                            if (d > 1)
                                d >>= 1;
                        }
                    }
                    break;

                default:
                    throw new Exception("Unsupported texture dimension");
            }

            byte[] buff = ms.GetBuffer();

            byte[] outbuf = new byte[ms.Length]; //need to copy to the right size buffer for File.WriteAllBytes().
            Array.Copy(buff, outbuf, outbuf.Length);

            return outbuf;
        }


        public static Texture GetTexture(byte[] ddsfile)
        {
            var ms = new MemoryStream(ddsfile);
            var br = new BinaryReader(ms);

            var header = new DDS_HEADER();
            var header10 = new DDS_HEADER_DXT10();
            var useheader10 = false;

            if (!DXTex._ReadDDSHeader(br, out header, out header10, out useheader10))
            { return null; }

            var width = header.dwWidth;
            var height = header.dwHeight;
            var depth = header.dwDepth;
            var mipCount = header.dwMipMapCount;
            if (mipCount == 0)
            {
                mipCount = 1;
            }

            TEX_DIMENSION resDim = 0;
            uint arraySize = 1;
            DXGI_FORMAT format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
            bool isCubeMap = false;

            if (useheader10)
            {
                arraySize = header10.arraySize;
                format = header10.dxgiFormat;
                resDim = (TEX_DIMENSION)header10.resourceDimension;

                if (arraySize == 0) throw new Exception("ArraySize was 0! This isn't supported...");
                switch (format)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_AI44:
                    case DXGI_FORMAT.DXGI_FORMAT_IA44:
                    case DXGI_FORMAT.DXGI_FORMAT_P8:
                    case DXGI_FORMAT.DXGI_FORMAT_A8P8:
                        throw new NotSupportedException(string.Format("{0} DXGI format is not supported", format.ToString()));
                    default:
                        if (DXTex.BitsPerPixel(format) == 0)
                            throw new NotSupportedException(string.Format("{0} DXGI format is not supported", format.ToString()));
                        break;
                }
                switch (resDim)
                {
                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE1D:
                        if ((header.dwFlags & DXTex.DDS_HEIGHT) > 0 && height != 1) throw new NotSupportedException("1D texture's height wasn't 1!");
                        height = depth = 1; // D3DX writes 1D textures with a fixed Height of 1
                        break;
                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D:
                        if ((header10.miscFlag & (uint)TEX_MISC_FLAG.TEX_MISC_TEXTURECUBE) > 0) { arraySize *= 6; isCubeMap = true; }
                        depth = 1;
                        break;
                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE3D:
                        if ((header.dwFlags & DXTex.DDS_HEADER_FLAGS_VOLUME) == 0) throw new ArgumentException("3D texture without volume flag!");
                        if (arraySize > 1) throw new ArgumentException("3D texture with ArraySize > 1!");
                        break;
                    default: throw new ArgumentException("Unknown resource dimension!");
                }
            }
            else
            {
                format = header.ddspf.GetDXGIFormat();

                if (format == DXGI_FORMAT.DXGI_FORMAT_UNKNOWN) throw new ArgumentException("Unknown DDS format.");
                if ((header.dwFlags & DXTex.DDS_HEADER_FLAGS_VOLUME) > 0)
                {
                    resDim = TEX_DIMENSION.TEX_DIMENSION_TEXTURE3D;
                }
                else
                {
                    if ((header.dwCaps2 & DXTex.DDS_CUBEMAP) > 0)
                    {
                        if ((header.dwCaps2 & DXTex.DDS_CUBEMAP_ALLFACES) != DXTex.DDS_CUBEMAP_ALLFACES) throw new ArgumentException("Not all faces in cubemap exist!");//requires all 6 faces
                        arraySize = 6;
                        isCubeMap = true;
                    }
                    depth = 1;
                    resDim = TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D;
                    // Note there's no way for a legacy Direct3D 9 DDS to express a '1D' texture
                }
                if (DXTex.BitsPerPixel(format) == 0) throw new Exception(string.Format("{0} DXGI format is not supported", format.ToString()));
            }

            if (isCubeMap)
            { }

            DXTex.ComputePitch(format, (int)width, (int)height, out int rowPitch, out int slicePitch, 0);
            var stride = slicePitch / height;
            var scanlines = DXTex.ComputeScanlines(format, (int)height);

            var brem = ms.Length - ms.Position;
            var ddsdata = br.ReadBytes((int)brem);

            var tex = new Texture();
            tex.Width = (ushort)width;
            tex.Height = (ushort)height;
            tex.Depth = (ushort)depth;
            tex.Levels = (byte)mipCount;
            tex.Format = GetTextureFormat(format);
            tex.Stride = (ushort)stride;
            tex.Data = new TextureData();
            tex.Data.FullData = ddsdata;

            //tex.Unknown_43h = (byte)header.ddspf.dwSize;

            return tex;
        }




        private static TextureFormat GetTextureFormat(DXGI_FORMAT f)
        {
            var format = (TextureFormat)0;
            switch (f)
            {
                // compressed
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM: format = TextureFormat.D3DFMT_DXT1; break;
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM: format = TextureFormat.D3DFMT_DXT3; break;
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM: format = TextureFormat.D3DFMT_DXT5; break;
                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM: format = TextureFormat.D3DFMT_ATI1; break;
                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM: format = TextureFormat.D3DFMT_ATI2; break;
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM: format = TextureFormat.D3DFMT_BC7; break;

                // uncompressed
                case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM: format = TextureFormat.D3DFMT_A1R5G5B5; break;
                case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM: format = TextureFormat.D3DFMT_A8; break;
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM: format = TextureFormat.D3DFMT_A8B8G8R8; break;
                case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM: format = TextureFormat.D3DFMT_L8; break;
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM: format = TextureFormat.D3DFMT_A8R8G8B8; break;
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM: format = TextureFormat.D3DFMT_X8R8G8B8; break;
            }
            return format;
        }

        private static DXGI_FORMAT GetDXGIFormat(TextureFormat f)
        {
            var format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
            switch (f)
            {
                // compressed
                case TextureFormat.D3DFMT_DXT1: format = DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM; break;
                case TextureFormat.D3DFMT_DXT3: format = DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM; break;
                case TextureFormat.D3DFMT_DXT5: format = DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM; break;
                case TextureFormat.D3DFMT_ATI1: format = DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM; break;
                case TextureFormat.D3DFMT_ATI2: format = DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM; break;
                case TextureFormat.D3DFMT_BC7: format = DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM; break;

                // uncompressed
                case TextureFormat.D3DFMT_A1R5G5B5: format = DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM; break;
                case TextureFormat.D3DFMT_A8: format = DXGI_FORMAT.DXGI_FORMAT_A8_UNORM; break;
                case TextureFormat.D3DFMT_A8B8G8R8: format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM; break;
                case TextureFormat.D3DFMT_L8: format = DXGI_FORMAT.DXGI_FORMAT_R8_UNORM; break;
                case TextureFormat.D3DFMT_A8R8G8B8: format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM; break;
                case TextureFormat.D3DFMT_X8R8G8B8: format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM; break;
            }
            return format;
        }

        private static ImageStruct GetImageStruct(Texture texture, DXGI_FORMAT format)
        {
            ImageStruct img = new ImageStruct();
            img.Data = texture.Data.FullData;
            img.Width = texture.Width;
            img.Height = texture.Height;
            img.MipMapLevels = texture.Levels;
            img.Format = (int)format;
            return img;
        }

        private static Image[] GetMipmapImages(ImageStruct img, DXGI_FORMAT format)
        {
            Image[] images = new Image[img.MipMapLevels];

            int buf = 0;
            int div = 1;
            int add = 0;
            for (int i = 0; i < img.MipMapLevels; i++)
            {
                images[i].width = Math.Max(img.Width / div, 1);
                images[i].height = Math.Max(img.Height / div, 1);
                images[i].format = format; //(DXGI_FORMAT)img.Format;
                images[i].pixels = buf + add;

                DXTex.ComputePitch(images[i].format, images[i].width, images[i].height, out images[i].rowPitch, out images[i].slicePitch, 0);

                add += images[i].slicePitch;
                div *= 2;
            }

            return images;
        }

        private static TexMetadata GetImageMetadata(ImageStruct img, DXGI_FORMAT format)
        {
            TexMetadata meta = new TexMetadata();
            meta.width = img.Width;
            meta.height = img.Height;
            meta.depth = 1;
            meta.arraySize = 1; // ???
            meta.mipLevels = img.MipMapLevels;
            meta.miscFlags = 0; // ???
            meta.miscFlags2 = 0; // ???
            meta.format = format; //(DXGI_FORMAT)img.Format;
            meta.dimension = TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D;
            return meta;
        }


        public struct TexMetadata
        {
            public int width;
            public int height;     // Should be 1 for 1D textures
            public int depth;      // Should be 1 for 1D or 2D textures
            public int arraySize;  // For cubemap, this is a multiple of 6
            public int mipLevels;
            public uint miscFlags;
            public uint miscFlags2;
            public DXGI_FORMAT format;
            public TEX_DIMENSION dimension;

            // Returns size_t(-1) to indicate an out-of-range error
            public int ComputeIndex(int mip, int item, int slice)
            {
                if (mip >= mipLevels)
                    return -1;

                switch (dimension)
                {
                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE1D:
                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D:
                        if (slice > 0)
                            return -1;

                        if (item >= arraySize)
                            return -1;

                        return (item * (mipLevels) + mip);

                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE3D:
                        if (item > 0)
                        {
                            // No support for arrays of volumes
                            return -1;
                        }
                        else
                        {
                            int index = 0;
                            int d = depth;

                            for (int level = 0; level < mip; ++level)
                            {
                                index += d;
                                if (d > 1)
                                    d >>= 1;
                            }

                            if (slice >= d)
                                return -1;

                            index += slice;

                            return index;
                        }
                    //break;

                    default:
                        return -1;
                }
            }

            // Helper for miscFlags
            public bool IsCubemap()
            {
                return (miscFlags & (uint)TEX_MISC_FLAG.TEX_MISC_TEXTURECUBE) != 0;
            }

            public bool IsPMAlpha()
            {
                return ((miscFlags2 & (uint)TEX_MISC_FLAG.TEX_MISC2_ALPHA_MODE_MASK) == (uint)TEX_ALPHA_MODE.TEX_ALPHA_MODE_PREMULTIPLIED);
            }
            // Helpers for miscFlags2
            //public void SetAlphaMode(TEX_ALPHA_MODE mode)
            //{
            //    miscFlags2 = (miscFlags2 & ~TEX_MISC2_ALPHA_MODE_MASK) | static_cast<uint32_t>(mode);
            //}

            // Helper for dimension
            public bool IsVolumemap()
            {
                return (dimension == TEX_DIMENSION.TEX_DIMENSION_TEXTURE3D);
            }
        }

        public struct Image
        {
            public int width;
            public int height;
            public DXGI_FORMAT format;
            public int rowPitch;
            public int slicePitch;
            public int pixels; //offset into data array...
        };

        public static class DXTex
        {
            public static void ComputePitch(DXGI_FORMAT fmt, int width, int height, out int rowPitch, out int slicePitch, uint flags)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                        assert(IsCompressed(fmt));
                        {
                            int nbw = Math.Max(1, (width + 3) / 4);
                            int nbh = Math.Max(1, (height + 3) / 4);
                            rowPitch = nbw * 8;

                            slicePitch = rowPitch * nbh;
                        }
                        break;

                    case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                        assert(IsCompressed(fmt));
                        {
                            int nbw = Math.Max(1, (width + 3) / 4);
                            int nbh = Math.Max(1, (height + 3) / 4);
                            rowPitch = nbw * 16;

                            slicePitch = rowPitch * nbh;
                        }
                        break;

                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_YUY2:
                        assert(IsPacked(fmt));
                        rowPitch = ((width + 1) >> 1) * 4;
                        slicePitch = rowPitch * height;
                        break;

                    case DXGI_FORMAT.DXGI_FORMAT_Y210:
                    case DXGI_FORMAT.DXGI_FORMAT_Y216:
                        assert(IsPacked(fmt));
                        rowPitch = ((width + 1) >> 1) * 8;
                        slicePitch = rowPitch * height;
                        break;

                    case DXGI_FORMAT.DXGI_FORMAT_NV12:
                    case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                        assert(IsPlanar(fmt));
                        rowPitch = ((width + 1) >> 1) * 2;
                        slicePitch = rowPitch * (height + ((height + 1) >> 1));
                        break;

                    case DXGI_FORMAT.DXGI_FORMAT_P010:
                    case DXGI_FORMAT.DXGI_FORMAT_P016:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT:
                        assert(IsPlanar(fmt));
                        rowPitch = ((width + 1) >> 1) * 4;
                        slicePitch = rowPitch * (height + ((height + 1) >> 1));
                        break;

                    case DXGI_FORMAT.DXGI_FORMAT_NV11:
                        assert(IsPlanar(fmt));
                        rowPitch = ((width + 3) >> 2) * 4;
                        slicePitch = rowPitch * height * 2;
                        break;

                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_P208:
                        assert(IsPlanar(fmt));
                        rowPitch = ((width + 1) >> 1) * 2;
                        slicePitch = rowPitch * height * 2;
                        break;

                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V208:
                        assert(IsPlanar(fmt));
                        rowPitch = width;
                        slicePitch = rowPitch * (height + (((height + 1) >> 1) * 2));
                        break;

                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V408:
                        assert(IsPlanar(fmt));
                        rowPitch = width;
                        slicePitch = rowPitch * (height + ((height >> 1) * 4));
                        break;

                    default:
                        assert(IsValid(fmt), $"{fmt} is not a valid texture format");
                        assert(!IsCompressed(fmt) && !IsPacked(fmt) && !IsPlanar(fmt));
                        {

                            int bpp;

                            if ((flags & (uint)CP_FLAGS.CP_FLAGS_24BPP) != 0)
                                bpp = 24;
                            else if ((flags & (uint)CP_FLAGS.CP_FLAGS_16BPP) != 0)
                                bpp = 16;
                            else if ((flags & (uint)CP_FLAGS.CP_FLAGS_8BPP) != 0)
                                bpp = 8;
                            else
                                bpp = BitsPerPixel(fmt);

                            if ((flags & (uint)(CP_FLAGS.CP_FLAGS_LEGACY_DWORD | CP_FLAGS.CP_FLAGS_PARAGRAPH | CP_FLAGS.CP_FLAGS_YMM | CP_FLAGS.CP_FLAGS_ZMM | CP_FLAGS.CP_FLAGS_PAGE4K)) != 0)
                            {
                                if ((flags & (uint)CP_FLAGS.CP_FLAGS_PAGE4K) != 0)
                                {
                                    rowPitch = ((width * bpp + 32767) / 32768) * 4096;
                                    slicePitch = rowPitch * height;
                                }
                                else if ((flags & (uint)CP_FLAGS.CP_FLAGS_ZMM) != 0)
                                {
                                    rowPitch = ((width * bpp + 511) / 512) * 64;
                                    slicePitch = rowPitch * height;
                                }
                                else if ((flags & (uint)CP_FLAGS.CP_FLAGS_YMM) != 0)
                                {
                                    rowPitch = ((width * bpp + 255) / 256) * 32;
                                    slicePitch = rowPitch * height;
                                }
                                else if ((flags & (uint)CP_FLAGS.CP_FLAGS_PARAGRAPH) != 0)
                                {
                                    rowPitch = ((width * bpp + 127) / 128) * 16;
                                    slicePitch = rowPitch * height;
                                }
                                else // DWORD alignment
                                {
                                    // Special computation for some incorrectly created DDS files based on
                                    // legacy DirectDraw assumptions about pitch alignment
                                    rowPitch = ((width * bpp + 31) / 32) * 4;// sizeof(uint32_t);
                                    slicePitch = rowPitch * height;
                                }
                            }
                            else
                            {
                                // Default byte alignment
                                rowPitch = (width * bpp + 7) / 8;
                                slicePitch = rowPitch * height;
                            }
                        }
                        break;
                }
            }

            public static bool IsCompressed(DXGI_FORMAT fmt)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                        return true;

                    default:
                        return false;
                }
            }

            public static bool IsPlanar(DXGI_FORMAT fmt)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_NV12:      // 4:2:0 8-bit
                    case DXGI_FORMAT.DXGI_FORMAT_P010:      // 4:2:0 10-bit
                    case DXGI_FORMAT.DXGI_FORMAT_P016:      // 4:2:0 16-bit
                    case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:// 4:2:0 8-bit
                    case DXGI_FORMAT.DXGI_FORMAT_NV11:      // 4:1:1 8-bit

                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_P208: // 4:2:2 8-bit
                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V208: // 4:4:0 8-bit
                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V408: // 4:4:4 8-bit
                                                             // These are JPEG Hardware decode formats (DXGI 1.4)

                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT:
                        // These are Xbox One platform specific types
                        return true;

                    default:
                        return false;
                }

            }

            public static bool IsPacked(DXGI_FORMAT fmt)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_YUY2: // 4:2:2 8-bit
                    case DXGI_FORMAT.DXGI_FORMAT_Y210: // 4:2:2 10-bit
                    case DXGI_FORMAT.DXGI_FORMAT_Y216: // 4:2:2 16-bit
                        return true;

                    default:
                        return false;
                }
            }

            public static void assert(bool b, string message = null)
            {
                if (!b)
                {
                    if (message is null)
                    {
                        throw new AssertionFailedException();
                    } else
                    {
                        throw new AssertionFailedException(message);
                    }
                }
            }



            public static bool IsValid(DXGI_FORMAT fmt)
            {
                return (((int)fmt) >= 1 && ((int)fmt) <= 190);
            }
            public static bool IsPalettized(DXGI_FORMAT fmt)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_AI44:
                    case DXGI_FORMAT.DXGI_FORMAT_IA44:
                    case DXGI_FORMAT.DXGI_FORMAT_P8:
                    case DXGI_FORMAT.DXGI_FORMAT_A8P8:
                        return true;

                    default:
                        return false;
                }
            }


            public static int BitsPerPixel(DXGI_FORMAT fmt)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT:
                        return 128;

                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_SINT:
                        return 96;

                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32G8X24_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_X32_TYPELESS_G8X24_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_Y416:
                    case DXGI_FORMAT.DXGI_FORMAT_Y210:
                    case DXGI_FORMAT.DXGI_FORMAT_Y216:
                        return 64;

                    case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R32_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R24G8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_X24_TYPELESS_G8_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_AYUV:
                    case DXGI_FORMAT.DXGI_FORMAT_Y410:
                    case DXGI_FORMAT.DXGI_FORMAT_YUY2:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R10G10B10_7E3_A2_FLOAT:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R10G10B10_6E4_A2_FLOAT:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R10G10B10_SNORM_A2_UNORM:
                        return 32;

                    case DXGI_FORMAT.DXGI_FORMAT_P010:
                    case DXGI_FORMAT.DXGI_FORMAT_P016:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT:
                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V408:
                        return 24;

                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT:
                    case DXGI_FORMAT.DXGI_FORMAT_D16_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R16_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_A8P8:
                    case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM:
                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_P208:
                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V208:
                        return 16;

                    case DXGI_FORMAT.DXGI_FORMAT_NV12:
                    case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                    case DXGI_FORMAT.DXGI_FORMAT_NV11:
                        return 12;

                    case DXGI_FORMAT.DXGI_FORMAT_R8_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R8_UINT:
                    case DXGI_FORMAT.DXGI_FORMAT_R8_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_R8_SINT:
                    case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_AI44:
                    case DXGI_FORMAT.DXGI_FORMAT_IA44:
                    case DXGI_FORMAT.DXGI_FORMAT_P8:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R4G4_UNORM:
                        return 8;

                    case DXGI_FORMAT.DXGI_FORMAT_R1_UNORM:
                        return 1;

                    case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                        return 4;

                    case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                        return 8;

                    default:
                        return 0;
                }
            }



            public static int ComputeScanlines(DXGI_FORMAT fmt, int height)
            {
                switch (fmt)
                {
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                    case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                        assert(IsCompressed(fmt));
                        return Math.Max(1, (height + 3) / 4);

                    case DXGI_FORMAT.DXGI_FORMAT_NV11:
                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_P208:
                        assert(IsPlanar(fmt));
                        return height * 2;

                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V208:
                        assert(IsPlanar(fmt));
                        return height + (((height + 1) >> 1) * 2);

                    case DXGI_FORMAT.WIN10_DXGI_FORMAT_V408:
                        assert(IsPlanar(fmt));
                        return height + ((height >> 1) * 4);

                    case DXGI_FORMAT.DXGI_FORMAT_NV12:
                    case DXGI_FORMAT.DXGI_FORMAT_P010:
                    case DXGI_FORMAT.DXGI_FORMAT_P016:
                    case DXGI_FORMAT.DXGI_FORMAT_420_OPAQUE:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS:
                    case DXGI_FORMAT.XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT:
                        assert(IsPlanar(fmt));
                        return height + ((height + 1) >> 1);

                    default:
                        assert(IsValid(fmt));
                        assert(!IsCompressed(fmt) && !IsPlanar(fmt));
                        return height;
                }
            }





            public static uint DDS_HEADER_FLAGS_TEXTURE = 0x00001007; // DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT 
            public static uint DDS_HEADER_FLAGS_MIPMAP = 0x00020000;  // DDSD_MIPMAPCOUNT
            public static uint DDS_HEADER_FLAGS_VOLUME = 0x00800000;  // DDSD_DEPTH
            public static uint DDS_HEADER_FLAGS_PITCH = 0x00000008; // DDSD_PITCH
            public static uint DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000;  // DDSD_LINEARSIZE

            public static uint DDS_HEIGHT = 0x00000002; // DDSD_HEIGHT
            public static uint DDS_WIDTH = 0x00000004; // DDSD_WIDTH

            public static uint DDS_SURFACE_FLAGS_TEXTURE = 0x00001000; // DDSCAPS_TEXTURE
            public static uint DDS_SURFACE_FLAGS_MIPMAP = 0x00400008; // DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
            public static uint DDS_SURFACE_FLAGS_CUBEMAP = 0x00000008; // DDSCAPS_COMPLEX

            public static uint DDS_CUBEMAP_POSITIVEX = 0x00000600; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
            public static uint DDS_CUBEMAP_NEGATIVEX = 0x00000a00; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
            public static uint DDS_CUBEMAP_POSITIVEY = 0x00001200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
            public static uint DDS_CUBEMAP_NEGATIVEY = 0x00002200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
            public static uint DDS_CUBEMAP_POSITIVEZ = 0x00004200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
            public static uint DDS_CUBEMAP_NEGATIVEZ = 0x00008200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ

            public static uint DDS_CUBEMAP_ALLFACES = (DDS_CUBEMAP_POSITIVEX | DDS_CUBEMAP_NEGATIVEX |
                               DDS_CUBEMAP_POSITIVEY | DDS_CUBEMAP_NEGATIVEY |
                               DDS_CUBEMAP_POSITIVEZ | DDS_CUBEMAP_NEGATIVEZ);

            public static uint DDS_CUBEMAP = 0x00000200; // DDSCAPS2_CUBEMAP

            public static uint DDS_FLAGS_VOLUME = 0x00200000; // DDSCAPS2_VOLUME

            public static uint DDS_MAGIC = 0x20534444; // "DDS "



            //-------------------------------------------------------------------------------------
            // Encodes DDS file header (magic value, header, optional DX10 extended header)
            //-------------------------------------------------------------------------------------
            public static bool _EncodeDDSHeader(TexMetadata metadata, uint flags, BinaryWriter bw, out int required)
            {
                //const uint MAX_HEADER_SIZE = sizeof(uint) + 31*4/*sizeof(DDS_HEADER)*/ + 5*4/*sizeof(DDS_HEADER_DXT10)*/;
                //byte header[MAX_HEADER_SIZE];

                required = 0;
                if (!IsValid(metadata.format))
                    return false;

                if (IsPalettized(metadata.format))
                    return false;// HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED);

                if (metadata.arraySize > 1)
                {
                    if ((metadata.arraySize != 6) || (metadata.dimension != TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D) || !(metadata.IsCubemap()))
                    {
                        // Texture1D arrays, Texture2D arrays, and Cubemap arrays must be stored using 'DX10' extended header
                        flags |= (uint)DDS_FLAGS.DDS_FLAGS_FORCE_DX10_EXT;
                    }
                }

                if ((flags & (uint)DDS_FLAGS.DDS_FLAGS_FORCE_DX10_EXT_MISC2)!=0)
                {
                    flags |= (uint)DDS_FLAGS.DDS_FLAGS_FORCE_DX10_EXT;
                }

                DDS_PIXELFORMAT ddpf = new DDS_PIXELFORMAT(0);
                if (!((flags & (uint)DDS_FLAGS.DDS_FLAGS_FORCE_DX10_EXT)!=0))
                {
                    switch (metadata.format)
                    {
                        case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_A8B8G8R8; break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_G16R16; break;
                        case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_A8L8; break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_L16; break;
                        case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_L8; break;
                        case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_A8; break;
                        case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_R8G8_B8G8; break;
                        case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_G8R8_G8B8; break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_DXT1; break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM: ddpf = metadata.IsPMAlpha() ? (DDS_PIXELFORMAT.DDSPF_DXT2) : (DDS_PIXELFORMAT.DDSPF_DXT3); break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM: ddpf = metadata.IsPMAlpha() ? (DDS_PIXELFORMAT.DDSPF_DXT4) : (DDS_PIXELFORMAT.DDSPF_DXT5); break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_BC4_UNORM; break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM: ddpf = DDS_PIXELFORMAT.DDSPF_BC4_SNORM; break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_BC5_UNORM; break;
                        case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM: ddpf = DDS_PIXELFORMAT.DDSPF_BC5_SNORM; break;
                        case DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_R5G6B5; break;
                        case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_A1R5G5B5; break;
                        case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_A8R8G8B8; break; // DXGI 1.1
                        case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_X8R8G8B8; break; // DXGI 1.1
                        case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM: ddpf = DDS_PIXELFORMAT.DDSPF_A4R4G4B4; break; // DXGI 1.2
                        case DXGI_FORMAT.DXGI_FORMAT_YUY2: ddpf = DDS_PIXELFORMAT.DDSPF_YUY2; break; // DXGI 1.2

                        // Legacy D3DX formats using D3DFMT enum value as FourCC
                        case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 116;  // D3DFMT_A32B32G32R32F
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 113;  // D3DFMT_A16B16G16R16F
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 36;  // D3DFMT_A16B16G16R16
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 110;  // D3DFMT_Q16W16V16U16
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 115;  // D3DFMT_G32R32F
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 112;  // D3DFMT_G16R16F
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 114;  // D3DFMT_R32F
                            break;
                        case DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT:
                            ddpf.dwSize = 32/*sizeof(DDS_PIXELFORMAT)*/; ddpf.dwFlags = DDS_PIXELFORMAT.DDS_FOURCC; ddpf.dwFourCC = 111;  // D3DFMT_R16F
                            break;
                    }
                }

                int sizeofddsheader = 31 * 4;
                int sizeofddsheader10 = 5 * 4;

                required = 4/*sizeof(uint32_t)*/ + sizeofddsheader/*sizeof(DDS_HEADER)*/;

                if (ddpf.dwSize == 0)
                    required += sizeofddsheader10;// sizeof(DDS_HEADER_DXT10);

                //if (!pDestination)
                //    return S_OK;

                //if (maxsize < required)
                //    return E_NOT_SUFFICIENT_BUFFER;

                //*reinterpret_cast<uint32_t*>(pDestination) = DDS_MAGIC;
                //bw.Write(DDS_MAGIC);

                var header = new DDS_HEADER();
                //auto header = reinterpret_cast<DDS_HEADER*>(reinterpret_cast<uint8_t*>(pDestination) + sizeof(uint32_t));
                //assert(header);

                //memset(header, 0, sizeof(DDS_HEADER));
                header.dwSize = (uint)sizeofddsheader;// sizeof(DDS_HEADER);
                header.dwFlags = DDS_HEADER_FLAGS_TEXTURE;
                header.dwCaps = DDS_SURFACE_FLAGS_TEXTURE;

                if (metadata.mipLevels > 0)
                {
                    header.dwFlags |= DDS_HEADER_FLAGS_MIPMAP;

//# ifdef _M_X64
//                    if (metadata.mipLevels > 0xFFFFFFFF)
//                        return E_INVALIDARG;
//#endif

                    header.dwMipMapCount = (uint)(metadata.mipLevels);

                    if (header.dwMipMapCount > 1)
                        header.dwCaps |= DDS_SURFACE_FLAGS_MIPMAP;
                }

                switch (metadata.dimension)
                {
                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE1D:
//# ifdef _M_X64
//                        if (metadata.width > 0xFFFFFFFF)
//                            return E_INVALIDARG;
//#endif

                        header.dwWidth = (uint)(metadata.width);
                        header.dwHeight = header.dwDepth = 1;
                        break;

                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE2D:
//# ifdef _M_X64
//                        if (metadata.height > 0xFFFFFFFF
//                             || metadata.width > 0xFFFFFFFF)
//                            return E_INVALIDARG;
//#endif

                        header.dwHeight = (uint)(metadata.height);
                        header.dwWidth = (uint)(metadata.width);
                        header.dwDepth = 1;

                        if (metadata.IsCubemap())
                        {
                            header.dwCaps |= DDS_SURFACE_FLAGS_CUBEMAP;
                            header.dwCaps2 |= DDS_CUBEMAP_ALLFACES;
                        }
                        break;

                    case TEX_DIMENSION.TEX_DIMENSION_TEXTURE3D:
//# ifdef _M_X64
//                        if (metadata.height > 0xFFFFFFFF
//                             || metadata.width > 0xFFFFFFFF
//                             || metadata.depth > 0xFFFFFFFF)
//                            return E_INVALIDARG;
//#endif

                        header.dwFlags |= DDS_HEADER_FLAGS_VOLUME;
                        header.dwCaps2 |= DDS_FLAGS_VOLUME;
                        header.dwHeight = (uint)(metadata.height);
                        header.dwWidth = (uint)(metadata.width);
                        header.dwDepth = (uint)(metadata.depth);
                        break;

                    default:
                        return false;// E_FAIL;
                }

                int rowPitch, slicePitch;
                ComputePitch(metadata.format, metadata.width, metadata.height, out rowPitch, out slicePitch, 0);// (uint)CP_FLAGS.CP_FLAGS_NONE);

//# ifdef _M_X64
//                if (slicePitch > 0xFFFFFFFF
//                     || rowPitch > 0xFFFFFFFF)
//                    return E_FAIL;
//#endif

                if (IsCompressed(metadata.format))
                {
                    header.dwFlags |= DDS_HEADER_FLAGS_LINEARSIZE;
                    header.dwPitchOrLinearSize = (uint)(slicePitch);
                }
                else
                {
                    header.dwFlags |= DDS_HEADER_FLAGS_PITCH;
                    header.dwPitchOrLinearSize = (uint)(rowPitch);
                }

                var ext = new DDS_HEADER_DXT10();
                if (ddpf.dwSize == 0)
                {
                    header.ddspf = DDS_PIXELFORMAT.DDSPF_DX10;
                    //memcpy_s(&header->ddspf, sizeof(header->ddspf), &DDSPF_DX10, sizeof(DDS_PIXELFORMAT) );

                    //auto ext = reinterpret_cast<DDS_HEADER_DXT10*>(reinterpret_cast<uint8_t*>(header) + sizeof(DDS_HEADER));
                    //assert(ext);
                    //var ext = new DDS_HEADER_DXT10();

                    //memset(ext, 0, sizeof(DDS_HEADER_DXT10));
                    ext.dxgiFormat = metadata.format;
                    ext.resourceDimension = (uint)metadata.dimension;

//# ifdef _M_X64
//                    if (metadata.arraySize > 0xFFFFFFFF)
//                        return E_INVALIDARG;
//#endif

                    //static_assert(TEX_MISC_TEXTURECUBE == DDS_RESOURCE_MISC_TEXTURECUBE, "DDS header mismatch");

                    ext.miscFlag = metadata.miscFlags & ~((uint)TEX_MISC_FLAG.TEX_MISC_TEXTURECUBE);

                    if ((metadata.miscFlags & (uint)TEX_MISC_FLAG.TEX_MISC_TEXTURECUBE)!=0)
                    {
                        ext.miscFlag |= (uint)TEX_MISC_FLAG.TEX_MISC_TEXTURECUBE;
                        assert((metadata.arraySize % 6) == 0);
                        ext.arraySize = (uint)(metadata.arraySize / 6);
                    }
                    else
                    {
                        ext.arraySize = (uint)(metadata.arraySize);
                    }

                    //static_assert(TEX_MISC2_ALPHA_MODE_MASK == DDS_MISC_FLAGS2_ALPHA_MODE_MASK, "DDS header mismatch");
                    //static_assert(TEX_ALPHA_MODE_UNKNOWN == DDS_ALPHA_MODE_UNKNOWN, "DDS header mismatch");
                    //static_assert(TEX_ALPHA_MODE_STRAIGHT == DDS_ALPHA_MODE_STRAIGHT, "DDS header mismatch");
                    //static_assert(TEX_ALPHA_MODE_PREMULTIPLIED == DDS_ALPHA_MODE_PREMULTIPLIED, "DDS header mismatch");
                    //static_assert(TEX_ALPHA_MODE_OPAQUE == DDS_ALPHA_MODE_OPAQUE, "DDS header mismatch");
                    //static_assert(TEX_ALPHA_MODE_CUSTOM == DDS_ALPHA_MODE_CUSTOM, "DDS header mismatch");

                    if ((flags & (uint)DDS_FLAGS.DDS_FLAGS_FORCE_DX10_EXT_MISC2)!=0)
                    {
                        // This was formerly 'reserved'. D3DX10 and D3DX11 will fail if this value is anything other than 0
                        ext.miscFlags2 = metadata.miscFlags2;
                    }
                }
                else
                {
                    header.ddspf = ddpf;
                    //memcpy_s(&header.ddspf, sizeof(header.ddspf), &ddpf, sizeof(ddpf) );
                }


                //magic write
                bw.Write(DDS_MAGIC);

                //write header
                bw.Write(header.dwSize);
                bw.Write(header.dwFlags);
                bw.Write(header.dwHeight);
                bw.Write(header.dwWidth);
                bw.Write(header.dwPitchOrLinearSize);
                bw.Write(header.dwDepth); // only if DDS_HEADER_FLAGS_VOLUME is set in dwFlags
                bw.Write(header.dwMipMapCount);

                //public uint dwReserved1[11]; //x11
                for (int i = 0; i < 11; i++) bw.Write(0);

                //bw.Write(header.ddspf);
                bw.Write(header.ddspf.dwSize);
                bw.Write(header.ddspf.dwFlags);
                bw.Write(header.ddspf.dwFourCC);
                bw.Write(header.ddspf.dwRGBBitCount);
                bw.Write(header.ddspf.dwRBitMask);
                bw.Write(header.ddspf.dwGBitMask);
                bw.Write(header.ddspf.dwBBitMask);
                bw.Write(header.ddspf.dwABitMask);

                bw.Write(header.dwCaps);
                bw.Write(header.dwCaps2);
                bw.Write(header.dwCaps3);
                bw.Write(header.dwCaps4);
                bw.Write(header.dwReserved2);


                if (ddpf.dwSize == 0)
                {
                    //write ext
                    bw.Write((uint)ext.dxgiFormat);
                    bw.Write(ext.resourceDimension);
                    bw.Write(ext.miscFlag); // see DDS_RESOURCE_MISC_FLAG
                    bw.Write(ext.arraySize);
                    bw.Write(ext.miscFlags2); // see DDS_MISC_FLAGS2
                }

                return true; //S_OK
            }


            public static bool _ReadDDSHeader(BinaryReader br, out DDS_HEADER header, out DDS_HEADER_DXT10 header10, out bool useheader10)
            {
                var magic = br.ReadUInt32();
                if (magic != DDS_MAGIC) throw new Exception("Invalid DDS magic!");

                //var header = new DDS_HEADER();
                //var header10 = new DDS_HEADER_DXT10();
                int sizeofddsheader = 31 * 4;
                int sizeofddspixelformat = 8 * 4;

                header.dwSize = br.ReadUInt32();
                header.dwFlags = br.ReadUInt32();
                header.dwHeight = br.ReadUInt32();
                header.dwWidth = br.ReadUInt32();
                header.dwPitchOrLinearSize = br.ReadUInt32();
                header.dwDepth = br.ReadUInt32(); // only if DDS_HEADER_FLAGS_VOLUME is set in dwFlags
                header.dwMipMapCount = br.ReadUInt32();

                //public uint dwReserved1[11]; //x11
                for (int i = 0; i < 11; i++) br.ReadUInt32();

                header.ddspf.dwSize = br.ReadUInt32();
                header.ddspf.dwFlags = br.ReadUInt32();
                header.ddspf.dwFourCC = br.ReadUInt32();
                header.ddspf.dwRGBBitCount = br.ReadUInt32();
                header.ddspf.dwRBitMask = br.ReadUInt32();
                header.ddspf.dwGBitMask = br.ReadUInt32();
                header.ddspf.dwBBitMask = br.ReadUInt32();
                header.ddspf.dwABitMask = br.ReadUInt32();

                header.dwCaps = br.ReadUInt32();
                header.dwCaps2 = br.ReadUInt32();
                header.dwCaps3 = br.ReadUInt32();
                header.dwCaps4 = br.ReadUInt32();
                header.dwReserved2 = br.ReadUInt32();



                if(((DDS_PIXELFORMAT.DDS_FOURCC & header.ddspf.dwFlags) > 0) && 
                    (DDS_PIXELFORMAT.MAKEFOURCC('D', 'X', '1', '0') == header.ddspf.dwFourCC))
                {
                    header10.dxgiFormat = (DXGI_FORMAT)br.ReadUInt32();
                    header10.resourceDimension = br.ReadUInt32();
                    header10.miscFlag = br.ReadUInt32(); // see DDS_RESOURCE_MISC_FLAG
                    header10.arraySize = br.ReadUInt32();
                    header10.miscFlags2 = br.ReadUInt32(); // see DDS_MISC_FLAGS2
                    useheader10 = true;
                }
                else
                {
                    header10 = new DDS_HEADER_DXT10();
                    useheader10 = false;
                }

                if (header.dwSize != sizeofddsheader || header.ddspf.dwSize != sizeofddspixelformat)
                {
                    throw new Exception("Invalid DDS header size");
                }


                return true;
            }


        }

        public enum CP_FLAGS : uint
        {
            CP_FLAGS_NONE = 0x0,      // Normal operation
            CP_FLAGS_LEGACY_DWORD = 0x1,      // Assume pitch is DWORD aligned instead of BYTE aligned
            CP_FLAGS_PARAGRAPH = 0x2,      // Assume pitch is 16-byte aligned instead of BYTE aligned
            CP_FLAGS_YMM = 0x4,      // Assume pitch is 32-byte aligned instead of BYTE aligned
            CP_FLAGS_ZMM = 0x8,      // Assume pitch is 64-byte aligned instead of BYTE aligned
            CP_FLAGS_PAGE4K = 0x200,    // Assume pitch is 4096-byte aligned instead of BYTE aligned
            CP_FLAGS_24BPP = 0x10000,  // Override with a legacy 24 bits-per-pixel format size
            CP_FLAGS_16BPP = 0x20000,  // Override with a legacy 16 bits-per-pixel format size
            CP_FLAGS_8BPP = 0x40000,  // Override with a legacy 8 bits-per-pixel format size
        }


        public class ImageStruct
        {
            public int Width;
            public int Height;
            //property int Stride;
            public int Format;
            public int MipMapLevels;
            public byte[] Data;

            int GetRowPitch()
            {
                int rowPitch;
                int slicePitch;
                DXTex.ComputePitch((DXGI_FORMAT)Format, Width, Height, out rowPitch, out slicePitch, 0);
                return rowPitch;
            }

            int GetSlicePitch()
            {
                int rowPitch;
                int slicePitch;
                DXTex.ComputePitch((DXGI_FORMAT)Format, Width, Height, out rowPitch, out slicePitch, 0);
                return slicePitch;
            }


        }

        public enum DXGI_FORMAT : uint
        {
            DXGI_FORMAT_UNKNOWN = 0,
            DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
            DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
            DXGI_FORMAT_R32G32B32A32_UINT = 3,
            DXGI_FORMAT_R32G32B32A32_SINT = 4,
            DXGI_FORMAT_R32G32B32_TYPELESS = 5,
            DXGI_FORMAT_R32G32B32_FLOAT = 6,
            DXGI_FORMAT_R32G32B32_UINT = 7,
            DXGI_FORMAT_R32G32B32_SINT = 8,
            DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
            DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
            DXGI_FORMAT_R16G16B16A16_UNORM = 11,
            DXGI_FORMAT_R16G16B16A16_UINT = 12,
            DXGI_FORMAT_R16G16B16A16_SNORM = 13,
            DXGI_FORMAT_R16G16B16A16_SINT = 14,
            DXGI_FORMAT_R32G32_TYPELESS = 15,
            DXGI_FORMAT_R32G32_FLOAT = 16,
            DXGI_FORMAT_R32G32_UINT = 17,
            DXGI_FORMAT_R32G32_SINT = 18,
            DXGI_FORMAT_R32G8X24_TYPELESS = 19,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
            DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
            DXGI_FORMAT_R10G10B10A2_UNORM = 24,
            DXGI_FORMAT_R10G10B10A2_UINT = 25,
            DXGI_FORMAT_R11G11B10_FLOAT = 26,
            DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
            DXGI_FORMAT_R8G8B8A8_UNORM = 28,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
            DXGI_FORMAT_R8G8B8A8_UINT = 30,
            DXGI_FORMAT_R8G8B8A8_SNORM = 31,
            DXGI_FORMAT_R8G8B8A8_SINT = 32,
            DXGI_FORMAT_R16G16_TYPELESS = 33,
            DXGI_FORMAT_R16G16_FLOAT = 34,
            DXGI_FORMAT_R16G16_UNORM = 35,
            DXGI_FORMAT_R16G16_UINT = 36,
            DXGI_FORMAT_R16G16_SNORM = 37,
            DXGI_FORMAT_R16G16_SINT = 38,
            DXGI_FORMAT_R32_TYPELESS = 39,
            DXGI_FORMAT_D32_FLOAT = 40,
            DXGI_FORMAT_R32_FLOAT = 41,
            DXGI_FORMAT_R32_UINT = 42,
            DXGI_FORMAT_R32_SINT = 43,
            DXGI_FORMAT_R24G8_TYPELESS = 44,
            DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
            DXGI_FORMAT_R8G8_TYPELESS = 48,
            DXGI_FORMAT_R8G8_UNORM = 49,
            DXGI_FORMAT_R8G8_UINT = 50,
            DXGI_FORMAT_R8G8_SNORM = 51,
            DXGI_FORMAT_R8G8_SINT = 52,
            DXGI_FORMAT_R16_TYPELESS = 53,
            DXGI_FORMAT_R16_FLOAT = 54,
            DXGI_FORMAT_D16_UNORM = 55,
            DXGI_FORMAT_R16_UNORM = 56,
            DXGI_FORMAT_R16_UINT = 57,
            DXGI_FORMAT_R16_SNORM = 58,
            DXGI_FORMAT_R16_SINT = 59,
            DXGI_FORMAT_R8_TYPELESS = 60,
            DXGI_FORMAT_R8_UNORM = 61,
            DXGI_FORMAT_R8_UINT = 62,
            DXGI_FORMAT_R8_SNORM = 63,
            DXGI_FORMAT_R8_SINT = 64,
            DXGI_FORMAT_A8_UNORM = 65,
            DXGI_FORMAT_R1_UNORM = 66,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
            DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
            DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
            DXGI_FORMAT_BC1_TYPELESS = 70,
            DXGI_FORMAT_BC1_UNORM = 71,
            DXGI_FORMAT_BC1_UNORM_SRGB = 72,
            DXGI_FORMAT_BC2_TYPELESS = 73,
            DXGI_FORMAT_BC2_UNORM = 74,
            DXGI_FORMAT_BC2_UNORM_SRGB = 75,
            DXGI_FORMAT_BC3_TYPELESS = 76,
            DXGI_FORMAT_BC3_UNORM = 77,
            DXGI_FORMAT_BC3_UNORM_SRGB = 78,
            DXGI_FORMAT_BC4_TYPELESS = 79,
            DXGI_FORMAT_BC4_UNORM = 80,
            DXGI_FORMAT_BC4_SNORM = 81,
            DXGI_FORMAT_BC5_TYPELESS = 82,
            DXGI_FORMAT_BC5_UNORM = 83,
            DXGI_FORMAT_BC5_SNORM = 84,
            DXGI_FORMAT_B5G6R5_UNORM = 85,
            DXGI_FORMAT_B5G5R5A1_UNORM = 86,
            DXGI_FORMAT_B8G8R8A8_UNORM = 87,
            DXGI_FORMAT_B8G8R8X8_UNORM = 88,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
            DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
            DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
            DXGI_FORMAT_BC6H_TYPELESS = 94,
            DXGI_FORMAT_BC6H_UF16 = 95,
            DXGI_FORMAT_BC6H_SF16 = 96,
            DXGI_FORMAT_BC7_TYPELESS = 97,
            DXGI_FORMAT_BC7_UNORM = 98,
            DXGI_FORMAT_BC7_UNORM_SRGB = 99,
            DXGI_FORMAT_AYUV = 100,
            DXGI_FORMAT_Y410 = 101,
            DXGI_FORMAT_Y416 = 102,
            DXGI_FORMAT_NV12 = 103,
            DXGI_FORMAT_P010 = 104,
            DXGI_FORMAT_P016 = 105,
            DXGI_FORMAT_420_OPAQUE = 106,
            DXGI_FORMAT_YUY2 = 107,
            DXGI_FORMAT_Y210 = 108,
            DXGI_FORMAT_Y216 = 109,
            DXGI_FORMAT_NV11 = 110,
            DXGI_FORMAT_AI44 = 111,
            DXGI_FORMAT_IA44 = 112,
            DXGI_FORMAT_P8 = 113,
            DXGI_FORMAT_A8P8 = 114,
            DXGI_FORMAT_B4G4R4A4_UNORM = 115,
            DXGI_FORMAT_FORCE_UINT = 0xffffffff,


            XBOX_DXGI_FORMAT_R10G10B10_7E3_A2_FLOAT = 116,
            XBOX_DXGI_FORMAT_R10G10B10_6E4_A2_FLOAT = 117,
            XBOX_DXGI_FORMAT_D16_UNORM_S8_UINT = 118,
            XBOX_DXGI_FORMAT_R16_UNORM_X8_TYPELESS = 119,
            XBOX_DXGI_FORMAT_X16_TYPELESS_G8_UINT = 120,

            WIN10_DXGI_FORMAT_P208 = 130,
            WIN10_DXGI_FORMAT_V208 = 131,
            WIN10_DXGI_FORMAT_V408 = 132,

            XBOX_DXGI_FORMAT_R10G10B10_SNORM_A2_UNORM = 189,
            XBOX_DXGI_FORMAT_R4G4_UNORM = 190,


        }

        public enum TEX_DIMENSION : uint
        { // Subset here matches D3D10_RESOURCE_DIMENSION and D3D11_RESOURCE_DIMENSION
            TEX_DIMENSION_TEXTURE1D = 2,
            TEX_DIMENSION_TEXTURE2D = 3,
            TEX_DIMENSION_TEXTURE3D = 4,
        }

        public enum TEX_MISC_FLAG : long // Subset here matches D3D10_RESOURCE_MISC_FLAG and D3D11_RESOURCE_MISC_FLAG
        {
            TEX_MISC_TEXTURECUBE = 0x4L,
            TEX_MISC2_ALPHA_MODE_MASK = 0x7L,
        }

        public enum TEX_ALPHA_MODE    // Matches DDS_ALPHA_MODE, encoded in MISC_FLAGS2
        {
            TEX_ALPHA_MODE_UNKNOWN = 0,
            TEX_ALPHA_MODE_STRAIGHT = 1,
            TEX_ALPHA_MODE_PREMULTIPLIED = 2,
            TEX_ALPHA_MODE_OPAQUE = 3,
            TEX_ALPHA_MODE_CUSTOM = 4,
        }


        public struct DDS_HEADER
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwHeight;
            public uint dwWidth;
            public uint dwPitchOrLinearSize;
            public uint dwDepth; // only if DDS_HEADER_FLAGS_VOLUME is set in dwFlags
            public uint dwMipMapCount;
            //public uint dwReserved1[11]; //x11
            public DDS_PIXELFORMAT ddspf;
            public uint dwCaps;
            public uint dwCaps2;
            public uint dwCaps3;
            public uint dwCaps4;
            public uint dwReserved2;
        };

        public struct DDS_HEADER_DXT10
        {
            public DXGI_FORMAT dxgiFormat;
            public uint resourceDimension;
            public uint miscFlag; // see DDS_RESOURCE_MISC_FLAG
            public uint arraySize;
            public uint miscFlags2; // see DDS_MISC_FLAGS2
        };

        public struct DDS_PIXELFORMAT
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwFourCC;
            public uint dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;

            public DDS_PIXELFORMAT(uint val)
            {
                dwSize = val;
                dwFlags = val;
                dwFourCC = val;
                dwRGBBitCount = val;
                dwRBitMask = val;
                dwGBitMask = val;
                dwBBitMask = val;
                dwABitMask = val;
            }
            public DDS_PIXELFORMAT(uint size, uint flags, uint fourcc, uint rgbbitcount, uint rmask, uint gmask, uint bmask, uint amask)
            {
                dwSize = size;
                dwFlags = flags;
                dwFourCC = fourcc;
                dwRGBBitCount = rgbbitcount;
                dwRBitMask = rmask;
                dwGBitMask = gmask;
                dwBBitMask = bmask;
                dwABitMask = amask;
            }


            public static uint MAKEFOURCC(char ch0, char ch1, char ch2, char ch3)
            {
                return
                    ((uint)(byte)(ch0) | ((uint)(byte)(ch1) << 8) |
                    ((uint)(byte)(ch2) << 16) | ((uint)(byte)(ch3) << 24));
            }
            public static uint DDS_FOURCC = 0x00000004; // DDPF_FOURCC
            public static uint DDS_RGB = 0x00000040;  // DDPF_RGB
            public static uint DDS_RGBA = 0x00000041;  // DDPF_RGB | DDPF_ALPHAPIXELS
            public static uint DDS_LUMINANCE = 0x00020000; // DDPF_LUMINANCE
            public static uint DDS_LUMINANCEA = 0x00020001;  // DDPF_LUMINANCE | DDPF_ALPHAPIXELS
            public static uint DDS_ALPHA = 0x00000002; // DDPF_ALPHA
            public static uint DDS_PAL8 = 0x00000020;  // DDPF_PALETTEINDEXED8


            public static DDS_PIXELFORMAT DDSPF_DXT1 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('D','X','T','1'), 0, 0, 0, 0, 0 );
            public static DDS_PIXELFORMAT DDSPF_DXT2 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_DXT3 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('D','X','T','3'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_DXT4 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('D','X','T','4'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_DXT5 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('D','X','T','5'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_BC4_UNORM = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('B','C','4','U'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_BC4_SNORM = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('B','C','4','S'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_BC5_UNORM = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('B','C','5','U'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_BC5_SNORM = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('B','C','5','S'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_R8G8_B8G8 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('R','G','B','G'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_G8R8_G8B8 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('G','R','G','B'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_YUY2 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('Y','U','Y','2'), 0, 0, 0, 0, 0);
            public static DDS_PIXELFORMAT DDSPF_A8R8G8B8 = new DDS_PIXELFORMAT(32, DDS_RGBA, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);
            public static DDS_PIXELFORMAT DDSPF_X8R8G8B8 = new DDS_PIXELFORMAT(32, DDS_RGB,  0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);
            public static DDS_PIXELFORMAT DDSPF_A8B8G8R8 = new DDS_PIXELFORMAT(32, DDS_RGBA, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000);
            public static DDS_PIXELFORMAT DDSPF_X8B8G8R8 = new DDS_PIXELFORMAT(32, DDS_RGB,  0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000);
            public static DDS_PIXELFORMAT DDSPF_G16R16 = new DDS_PIXELFORMAT(32, DDS_RGB,  0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000);
            public static DDS_PIXELFORMAT DDSPF_R5G6B5 = new DDS_PIXELFORMAT(32, DDS_RGB, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000);
            public static DDS_PIXELFORMAT DDSPF_A1R5G5B5 = new DDS_PIXELFORMAT(32, DDS_RGBA, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000);
            public static DDS_PIXELFORMAT DDSPF_A4R4G4B4 = new DDS_PIXELFORMAT(32, DDS_RGBA, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000);
            public static DDS_PIXELFORMAT DDSPF_R8G8B8 = new DDS_PIXELFORMAT(32, DDS_RGB, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);
            public static DDS_PIXELFORMAT DDSPF_L8 = new DDS_PIXELFORMAT(32, DDS_LUMINANCE, 0,  8, 0xff, 0x00, 0x00, 0x00);
            public static DDS_PIXELFORMAT DDSPF_L16 = new DDS_PIXELFORMAT(32, DDS_LUMINANCE, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);
            public static DDS_PIXELFORMAT DDSPF_A8L8 = new DDS_PIXELFORMAT(32, DDS_LUMINANCEA, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);
            public static DDS_PIXELFORMAT DDSPF_A8 = new DDS_PIXELFORMAT(32, DDS_ALPHA, 0, 8, 0x00, 0x00, 0x00, 0xff);

            // D3DFMT_A2R10G10B10/D3DFMT_A2B10G10R10 should be written using DX10 extension to avoid D3DX 10:10:10:2 reversal issue

            // This indicates the DDS_HEADER_DXT10 extension is present (the format is in dxgiFormat)
            public static DDS_PIXELFORMAT DDSPF_DX10 = new DDS_PIXELFORMAT(32, DDS_FOURCC, MAKEFOURCC('D','X','1','0'), 0, 0, 0, 0, 0);





            private bool ISBITMASK(uint r, uint g, uint b, uint a)
            {
                return ((dwRBitMask == r) && (dwGBitMask == g) && (dwBBitMask == b) && (dwABitMask == a));
            }

            public DXGI_FORMAT GetDXGIFormat()
            {

                if ((dwFlags & DDS_RGB) > 0)
                {
                    // Note that sRGB formats are written using the "DX10" extended header

                    switch (dwRGBBitCount)
                    {
                        case 32:
                            if (ISBITMASK(0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;
                            }

                            if (ISBITMASK(0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;
                            }

                            if (ISBITMASK(0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM;
                            }

                            // No DXGI format maps to ISBITMASK(0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000) aka D3DFMT_X8B8G8R8

                            // Note that many common DDS reader/writers (including D3DX) swap the
                            // the RED/BLUE masks for 10:10:10:2 formats. We assumme
                            // below that the 'backwards' header mask is being used since it is most
                            // likely written by D3DX. The more robust solution is to use the 'DX10'
                            // header extension and specify the DXGI_FORMAT_R10G10B10A2_UNORM format directly

                            // For 'correct' writers, this should be 0x000003ff, 0x000ffc00, 0x3ff00000 for RGB data
                            if (ISBITMASK(0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM;
                            }

                            // No DXGI format maps to ISBITMASK(0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000) aka D3DFMT_A2R10G10B10

                            if (ISBITMASK(0x0000ffff, 0xffff0000, 0x00000000, 0x00000000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM;
                            }

                            if (ISBITMASK(0xffffffff, 0x00000000, 0x00000000, 0x00000000))
                            {
                                // Only 32-bit color channel format in D3D9 was R32F
                                return DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT; // D3DX writes this out as a FourCC of 114
                            }
                            break;

                        case 24:
                            // No 24bpp DXGI formats aka D3DFMT_R8G8B8
                            break;

                        case 16:
                            if (ISBITMASK(0x7c00, 0x03e0, 0x001f, 0x8000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM;
                            }
                            if (ISBITMASK(0xf800, 0x07e0, 0x001f, 0x0000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM;
                            }

                            // No DXGI format maps to ISBITMASK(0x7c00, 0x03e0, 0x001f, 0x0000) aka D3DFMT_X1R5G5B5
                            if (ISBITMASK(0x0f00, 0x00f0, 0x000f, 0xf000))
                            {
                                return DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM;
                            }

                            // No DXGI format maps to ISBITMASK(0x0f00, 0x00f0, 0x000f, 0x0000) aka D3DFMT_X4R4G4B4

                            // No 3:3:2, 3:3:2:8, or paletted DXGI formats aka D3DFMT_A8R3G3B2, D3DFMT_R3G3B2, D3DFMT_P8, D3DFMT_A8P8, etc.
                            break;
                    }
                }
                else if ((dwFlags & DDS_LUMINANCE) > 0)
                {
                    if (8 == dwRGBBitCount)
                    {
                        if (ISBITMASK(0x000000ff, 0x00000000, 0x00000000, 0x00000000))
                        {
                            return DXGI_FORMAT.DXGI_FORMAT_R8_UNORM; // D3DX10/11 writes this out as DX10 extension
                        }

                        // No DXGI format maps to ISBITMASK(0x0f, 0x00, 0x00, 0xf0) aka D3DFMT_A4L4
                    }

                    if (16 == dwRGBBitCount)
                    {
                        if (ISBITMASK(0x0000ffff, 0x00000000, 0x00000000, 0x00000000))
                        {
                            return DXGI_FORMAT.DXGI_FORMAT_R16_UNORM; // D3DX10/11 writes this out as DX10 extension
                        }
                        if (ISBITMASK(0x000000ff, 0x00000000, 0x00000000, 0x0000ff00))
                        {
                            return DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM; // D3DX10/11 writes this out as DX10 extension
                        }
                    }
                }
                else if ((dwFlags & DDS_ALPHA) > 0)
                {
                    if (8 == dwRGBBitCount)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_A8_UNORM;
                    }
                }
                else if ((dwFlags & DDS_FOURCC) > 0)
                {
                    if (MAKEFOURCC('D', 'X', 'T', '1') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM;
                    }
                    if (MAKEFOURCC('D', 'X', 'T', '3') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM;
                    }
                    if (MAKEFOURCC('D', 'X', 'T', '5') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM;
                    }

                    // While pre-mulitplied alpha isn't directly supported by the DXGI formats,
                    // they are basically the same as these BC formats so they can be mapped
                    if (MAKEFOURCC('D', 'X', 'T', '2') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM;
                    }
                    if (MAKEFOURCC('D', 'X', 'T', '4') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM;
                    }

                    if (MAKEFOURCC('A', 'T', 'I', '1') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM;
                    }
                    if (MAKEFOURCC('B', 'C', '4', 'U') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM;
                    }
                    if (MAKEFOURCC('B', 'C', '4', 'S') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM;
                    }

                    if (MAKEFOURCC('A', 'T', 'I', '2') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM;
                    }
                    if (MAKEFOURCC('B', 'C', '5', 'U') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM;
                    }
                    if (MAKEFOURCC('B', 'C', '5', 'S') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM;
                    }

                    // BC6H and BC7 are written using the "DX10" extended header

                    if (MAKEFOURCC('R', 'G', 'B', 'G') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM;
                    }
                    if (MAKEFOURCC('G', 'R', 'G', 'B') == dwFourCC)
                    {
                        return DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM;
                    }

                    // Check for D3DFORMAT enums being set here
                    switch (dwFourCC)
                    {
                        case 36: // D3DFMT_A16B16G16R16
                            return DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM;

                        case 110: // D3DFMT_Q16W16V16U16
                            return DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM;

                        case 111: // D3DFMT_R16F
                            return DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT;

                        case 112: // D3DFMT_G16R16F
                            return DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT;

                        case 113: // D3DFMT_A16B16G16R16F
                            return DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT;

                        case 114: // D3DFMT_R32F
                            return DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT;

                        case 115: // D3DFMT_G32R32F
                            return DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT;

                        case 116: // D3DFMT_A32B32G32R32F
                            return DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT;
                    }
                }

                return DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
            }


        };

        public enum DDS_FLAGS : uint
        {
            DDS_FLAGS_NONE = 0x0,

            DDS_FLAGS_LEGACY_DWORD = 0x1,
            // Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)

            DDS_FLAGS_NO_LEGACY_EXPANSION = 0x2,
            // Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8) 

            DDS_FLAGS_NO_R10B10G10A2_FIXUP = 0x4,
            // Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks

            DDS_FLAGS_FORCE_RGB = 0x8,
            // Convert DXGI 1.1 BGR formats to DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats

            DDS_FLAGS_NO_16BPP = 0x10,
            // Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats

            DDS_FLAGS_EXPAND_LUMINANCE = 0x20,
            // When loading legacy luminance formats expand replicating the color channels rather than leaving them packed (L8, L16, A8L8)

            DDS_FLAGS_FORCE_DX10_EXT = 0x10000,
            // Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)

            DDS_FLAGS_FORCE_DX10_EXT_MISC2 = 0x20000,
            // DDS_FLAGS_FORCE_DX10_EXT including miscFlags2 information (result may not be compatible with D3DX10 or D3DX11)
        };





















        internal static byte[] DecompressDxt1(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressDxt1(imageStream, width, height);
        }

        internal static byte[] DecompressDxt1(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt1Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressDxt1Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    if (c0 > c1)
                    {
                        switch (index)
                        {
                            case 0:
                                r = r0;
                                g = g0;
                                b = b0;
                                break;
                            case 1:
                                r = r1;
                                g = g1;
                                b = b1;
                                break;
                            case 2:
                                r = (byte)((2 * r0 + r1) / 3);
                                g = (byte)((2 * g0 + g1) / 3);
                                b = (byte)((2 * b0 + b1) / 3);
                                break;
                            case 3:
                                r = (byte)((r0 + 2 * r1) / 3);
                                g = (byte)((g0 + 2 * g1) / 3);
                                b = (byte)((b0 + 2 * b1) / 3);
                                break;
                        }
                    }
                    else
                    {
                        switch (index)
                        {
                            case 0:
                                r = r0;
                                g = g0;
                                b = b0;
                                break;
                            case 1:
                                r = r1;
                                g = g1;
                                b = b1;
                                break;
                            case 2:
                                r = (byte)((r0 + r1) / 2);
                                g = (byte)((g0 + g1) / 2);
                                b = (byte)((b0 + b1) / 2);
                                break;
                            case 3:
                                r = 0;
                                g = 0;
                                b = 0;
                                a = 0;
                                break;
                        }
                    }

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }


        internal static byte[] DecompressDxt3(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressDxt3(imageStream, width, height);
        }

        internal static byte[] DecompressDxt3(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt3Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressDxt3Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte a0 = imageReader.ReadByte();
            byte a1 = imageReader.ReadByte();
            byte a2 = imageReader.ReadByte();
            byte a3 = imageReader.ReadByte();
            byte a4 = imageReader.ReadByte();
            byte a5 = imageReader.ReadByte();
            byte a6 = imageReader.ReadByte();
            byte a7 = imageReader.ReadByte();

            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            int alphaIndex = 0;
            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 0;

                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    switch (alphaIndex)
                    {
                        case 0:
                            a = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4));
                            break;
                        case 1:
                            a = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4));
                            break;
                        case 2:
                            a = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4));
                            break;
                        case 3:
                            a = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4));
                            break;
                        case 4:
                            a = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4));
                            break;
                        case 5:
                            a = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4));
                            break;
                        case 6:
                            a = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4));
                            break;
                        case 7:
                            a = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4));
                            break;
                        case 8:
                            a = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4));
                            break;
                        case 9:
                            a = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4));
                            break;
                        case 10:
                            a = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4));
                            break;
                        case 11:
                            a = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4));
                            break;
                        case 12:
                            a = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4));
                            break;
                        case 13:
                            a = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4));
                            break;
                        case 14:
                            a = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4));
                            break;
                        case 15:
                            a = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4));
                            break;
                    }
                    ++alphaIndex;

                    switch (index)
                    {
                        case 0:
                            r = r0;
                            g = g0;
                            b = b0;
                            break;
                        case 1:
                            r = r1;
                            g = g1;
                            b = b1;
                            break;
                        case 2:
                            r = (byte)((2 * r0 + r1) / 3);
                            g = (byte)((2 * g0 + g1) / 3);
                            b = (byte)((2 * b0 + b1) / 3);
                            break;
                        case 3:
                            r = (byte)((r0 + 2 * r1) / 3);
                            g = (byte)((g0 + 2 * g1) / 3);
                            b = (byte)((b0 + 2 * b1) / 3);
                            break;
                    }

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }


        internal static byte[] DecompressDxt5(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressDxt5(imageStream, width, height);
        }

        internal static byte[] DecompressDxt5(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt5Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte alpha0 = imageReader.ReadByte();
            byte alpha1 = imageReader.ReadByte();

            ulong alphaMask = (ulong)imageReader.ReadByte();
            alphaMask += (ulong)imageReader.ReadByte() << 8;
            alphaMask += (ulong)imageReader.ReadByte() << 16;
            alphaMask += (ulong)imageReader.ReadByte() << 24;
            alphaMask += (ulong)imageReader.ReadByte() << 32;
            alphaMask += (ulong)imageReader.ReadByte() << 40;

            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    uint alphaIndex = (uint)((alphaMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (alphaIndex == 0)
                    {
                        a = alpha0;
                    }
                    else if (alphaIndex == 1)
                    {
                        a = alpha1;
                    }
                    else if (alpha0 > alpha1)
                    {
                        a = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7);
                    }
                    else if (alphaIndex == 6)
                    {
                        a = 0;
                    }
                    else if (alphaIndex == 7)
                    {
                        a = 0xff;
                    }
                    else
                    {
                        a = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5);
                    }

                    switch (index)
                    {
                        case 0:
                            r = r0;
                            g = g0;
                            b = b0;
                            break;
                        case 1:
                            r = r1;
                            g = g1;
                            b = b1;
                            break;
                        case 2:
                            r = (byte)((2 * r0 + r1) / 3);
                            g = (byte)((2 * g0 + g1) / 3);
                            b = (byte)((2 * b0 + b1) / 3);
                            break;
                        case 3:
                            r = (byte)((r0 + 2 * r1) / 3);
                            g = (byte)((g0 + 2 * g1) / 3);
                            b = (byte)((b0 + 2 * b1) / 3);
                            break;
                    }

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }

        private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
        {
            int temp;
            temp = (color >> 11) * 255 + 16;
            r = (byte)((temp / 32 + temp) / 32);
            temp = ((color & 0x07E0) >> 5) * 255 + 32;
            g = (byte)((temp / 64 + temp) / 64);
            temp = (color & 0x001F) * 255 + 16;
            b = (byte)((temp / 32 + temp) / 32);
        }

        private static void ConvertBgra5551ToRgba8(ushort color, out byte r, out byte g, out byte b, out byte a)
        {
            //added by dexy
            int temp;
            temp = ((color >> 11) & 0x1F) * 255 + 16;
            r = (byte)((temp / 32 + temp) / 32);
            temp = ((color >> 6) & 0x1F) * 255 + 32;
            g = (byte)((temp / 32 + temp) / 32);
            temp = ((color >> 1) & 0x1F) * 255 + 16;
            b = (byte)((temp / 32 + temp) / 32);
            a = (byte)((color & 1) * 255);
        }




        //these ones added by dexy - based on DecompressDxt

        internal static byte[] DecompressBC4(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressBC4(imageStream, width, height);
        }

        internal static byte[] DecompressBC4(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressBC4Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressBC4Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte r0 = imageReader.ReadByte();
            byte r1 = imageReader.ReadByte();

            ulong rMask = (ulong)imageReader.ReadByte();
            rMask += (ulong)imageReader.ReadByte() << 8;
            rMask += (ulong)imageReader.ReadByte() << 16;
            rMask += (ulong)imageReader.ReadByte() << 24;
            rMask += (ulong)imageReader.ReadByte() << 32;
            rMask += (ulong)imageReader.ReadByte() << 40;

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 255;

                    uint rIndex = (uint)((rMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (rIndex == 0)
                    {
                        r = r0;
                    }
                    else if (rIndex == 1)
                    {
                        r = r1;
                    }
                    else if (r0 > r1)
                    {
                        r = (byte)(((8 - rIndex) * r0 + (rIndex - 1) * r1) / 7);
                    }
                    else if (rIndex == 6)
                    {
                        r = 0;
                    }
                    else if (rIndex == 7)
                    {
                        r = 0xff;
                    }
                    else
                    {
                        r = (byte)(((6 - rIndex) * r0 + (rIndex - 1) * r1) / 5);
                    }

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = 0;
                        imageData[offset + 2] = 0;
                        imageData[offset + 3] = 255;
                    }
                }
            }
        }


        internal static byte[] DecompressBC5(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                return DecompressBC5(imageStream, width, height);
        }

        internal static byte[] DecompressBC5(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressBC5Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressBC5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte r0 = imageReader.ReadByte();
            byte r1 = imageReader.ReadByte();

            ulong rMask = (ulong)imageReader.ReadByte();
            rMask += (ulong)imageReader.ReadByte() << 8;
            rMask += (ulong)imageReader.ReadByte() << 16;
            rMask += (ulong)imageReader.ReadByte() << 24;
            rMask += (ulong)imageReader.ReadByte() << 32;
            rMask += (ulong)imageReader.ReadByte() << 40;

            byte g0 = imageReader.ReadByte();
            byte g1 = imageReader.ReadByte();

            ulong gMask = (ulong)imageReader.ReadByte();
            gMask += (ulong)imageReader.ReadByte() << 8;
            gMask += (ulong)imageReader.ReadByte() << 16;
            gMask += (ulong)imageReader.ReadByte() << 24;
            gMask += (ulong)imageReader.ReadByte() << 32;
            gMask += (ulong)imageReader.ReadByte() << 40;

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 255;
                    uint rIndex = (uint)((rMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (rIndex == 0)
                    {
                        r = r0;
                    }
                    else if (rIndex == 1)
                    {
                        r = r1;
                    }
                    else if (r0 > r1)
                    {
                        r = (byte)(((8 - rIndex) * r0 + (rIndex - 1) * r1) / 7);
                    }
                    else if (rIndex == 6)
                    {
                        r = 0;
                    }
                    else if (rIndex == 7)
                    {
                        r = 0xff;
                    }
                    else
                    {
                        r = (byte)(((6 - rIndex) * r0 + (rIndex - 1) * r1) / 5);
                    }


                    byte g = 255;
                    uint gIndex = (uint)((gMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (gIndex == 0)
                    {
                        r = r0;
                    }
                    else if (gIndex == 1)
                    {
                        r = r1;
                    }
                    else if (r0 > r1)
                    {
                        r = (byte)(((8 - gIndex) * r0 + (gIndex - 1) * r1) / 7);
                    }
                    else if (gIndex == 6)
                    {
                        r = 0;
                    }
                    else if (gIndex == 7)
                    {
                        r = 0xff;
                    }
                    else
                    {
                        r = (byte)(((6 - gIndex) * r0 + (gIndex - 1) * r1) / 5);
                    }


                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = 0;
                        imageData[offset + 3] = 255;
                    }
                }
            }
        }



        private static byte[] ConvertA8ToRGBA8(byte[] imgdata, int w, int h)
        {
            var px = new byte[w * h * 4];
            var pxmax = px.Length - 3;
            for (int i = 0; i < imgdata.Length; i++)
            {
                byte av = imgdata[i];
                int pxi = i * 4;
                if (pxi < pxmax)
                {
                    px[pxi] = av;
                    px[pxi + 1] = av;
                    px[pxi + 2] = av;
                    px[pxi + 3] = 255;
                }
            }
            return px;
        }
        private static byte[] ConvertR8ToRGBA8(byte[] imgdata, int w, int h)
        {
            var px = new byte[w * h * 4];
            var pxmax = px.Length - 3;
            for (int i = 0; i < imgdata.Length; i++)
            {
                byte av = imgdata[i];
                int pxi = i * 4;
                if (pxi < pxmax)
                {
                    px[pxi] = av;
                    px[pxi + 1] = 0;
                    px[pxi + 2] = 0;
                    px[pxi + 3] = 255;
                }
            }
            return px;
        }
        private static byte[] ConvertBGR5A1ToRGBA8(byte[] imgdata, int w, int h)
        {
            var px = new byte[w * h * 4];
            var pxmax = px.Length - 3;
            for (int i = 0; i < imgdata.Length; i+=2)
            {
                byte v0 = imgdata[i];
                byte v1 = imgdata[i + 1];
                ushort vu = (ushort)(v1 * 255 + v0);
                int pxi = i * 4;
                if (pxi < pxmax)
                {
                    ConvertBgra5551ToRgba8(vu, out px[pxi], out px[pxi + 1], out px[pxi + 2], out px[pxi + 3]);
                }
            }
            return px;
        }

    }
}
