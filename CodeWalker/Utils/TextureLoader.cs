using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Utils
{
    public class TextureLoader
    {
        /// <summary>
        /// Loads a bitmap using WIC.
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static BitmapSource LoadBitmap(ImagingFactory2 factory, string filename)
        {
            var bitmapDecoder = new BitmapDecoder(
                factory,
                filename,
                DecodeOptions.CacheOnDemand
                );

            var formatConverter = new FormatConverter(factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }

        /// <summary>
        /// Creates a <see cref="SharpDX.Direct3D11.Texture2D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
        /// </summary>
        /// <param name="device">The Direct3D11 device</param>
        /// <param name="bitmapSource">The WIC bitmap source</param>
        /// <returns>A Texture2D</returns>
        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmap(Device device, SharpDX.WIC.BitmapSource bitmapSource)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * 4;
            using (var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true))
            {
                // Copy the content of the WIC to the buffer
                bitmapSource.CopyPixels(stride, buffer);
                return new Texture2D(device, new Texture2DDescription()
                {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                }, new SharpDX.DataRectangle(buffer.DataPointer, stride));
            }
        }



        //create a bitmap from a texture2D. DOES NOT WORK
        public static System.Drawing.Bitmap GetTextureBitmap(Device device, Texture2D tex, int mipSlice)
        {
            int w = tex.Description.Width;
            int h = tex.Description.Height;

            var textureCopy = new Texture2D(device, new Texture2DDescription
            {
                Width = w,
                Height = h,
                MipLevels = tex.Description.MipLevels,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });
            DataStream dataStream;// = new DataStream(8 * tex.Description.Width * tex.Description.Height, true, true);

            DeviceContext context = device.ImmediateContext;

            //context.CopyResource(tex, textureCopy);
            context.CopySubresourceRegion(tex, mipSlice, null, textureCopy, 0);
            

            var dataBox = context.MapSubresource(
                textureCopy,
                mipSlice,
                0,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out dataStream);

            //int bytesize = w * h * 4;
            //byte[] pixels = new byte[bytesize];
            //dataStream.Read(pixels, 0, bytesize);
            //dataStream.Position = 0;

            var dataRectangle = new DataRectangle
            {
                DataPointer = dataStream.DataPointer,
                Pitch = dataBox.RowPitch
            };
            

            ImagingFactory wicf = new ImagingFactory();

            var b = new SharpDX.WIC.Bitmap(wicf, w, h, SharpDX.WIC.PixelFormat.Format32bppBGRA, dataRectangle);


            var s = new MemoryStream();
            using (var bitmapEncoder = new PngBitmapEncoder(wicf, s))
            {
                using (var bitmapFrameEncode = new BitmapFrameEncode(bitmapEncoder))
                {
                    bitmapFrameEncode.Initialize();
                    bitmapFrameEncode.SetSize(b.Size.Width, b.Size.Height);
                    var pixelFormat = PixelFormat.FormatDontCare;
                    bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
                    bitmapFrameEncode.WriteSource(b);
                    bitmapFrameEncode.Commit();
                    bitmapEncoder.Commit();
                }
            }

            context.UnmapSubresource(textureCopy, 0);
            textureCopy.Dispose();
            b.Dispose();



            s.Position = 0;
            var bmp = new System.Drawing.Bitmap(s);



            //Palette pal = new Palette(wf);
            //b.CopyPalette(pal);

            //byte[] pixels = new byte[w * h * 4];
            //b.CopyPixels(pixels, w * 4);
            //GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            //var hptr = handle.AddrOfPinnedObject();
            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(w, h, w * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, hptr);
            //handle.Free();

            //System.Threading.Thread.Sleep(1000);

            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            //System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            //  new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
            //  System.Drawing.Imaging.ImageLockMode.WriteOnly,
            //  System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            ////b.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            //b.CopyPixels(data.Stride, data.Scan0, data.Height * data.Stride);
            //bmp.UnlockBits(data);


            var c1 = bmp.GetPixel(10, 2);


            //dataStream.Dispose();


            return bmp;
        }


    }
}
