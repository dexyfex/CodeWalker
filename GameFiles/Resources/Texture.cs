using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{


    [TypeConverter(typeof(ExpandableObjectConverter))] public class TextureDictionary : ResourceFileBase
    {
        public override long BlockLength
        {
            get
            {
                return 64;
            }
        }

        // structure data
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64Ptr TextureNameHashesPtr { get; set; }
        public uint[] TextureNameHashes { get; set; }
        public ResourcePointerList64<Texture> Textures { get; set; }

        public Dictionary<uint, Texture> Dict { get; set; }
        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if ((Textures != null) && (Textures.data_items != null))
                {
                    foreach (var tex in Textures.data_items)
                    {
                        if (tex != null)
                        {
                            val += tex.MemoryUsage;
                        }
                    }
                }
                return val;
            }
        }

        public TextureDictionary()
        {
            //this.TextureNameHashes = new ResourceSimpleList64<uint_r>();
            this.Textures = new ResourcePointerList64<Texture>();
        }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.TextureNameHashesPtr = reader.ReadStruct<ResourceSimpleList64Ptr>();
            this.TextureNameHashes = reader.ReadUintsAt(this.TextureNameHashesPtr.EntriesPointer, this.TextureNameHashesPtr.EntriesCount);
            //this.TextureNameHashes = reader.ReadBlock<ResourceSimpleList64<uint_r>>();
            this.Textures = reader.ReadBlock<ResourcePointerList64<Texture>>();

            var dict = new Dictionary<uint, Texture>();
            if ((Textures != null) && (Textures.data_items != null) && (TextureNameHashes != null))
            {
                for (int i = 0; (i < Textures.data_items.Length) && (i < TextureNameHashes.Length); i++)
                {
                    var tex = Textures.data_items[i];
                    var hash = TextureNameHashes[i];
                    dict[hash] = tex;
                }
            }
            Dict = new Dictionary<uint, Texture>(dict);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            //writer.WriteBlock(this.TextureNameHashes); //TODO: fix!
            //writer.WriteBlock(this.Textures);
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                //new Tuple<long, IResourceBlock>(0x20, TextureNameHashes), //TODO: fix!
                new Tuple<long, IResourceBlock>(0x30, Textures)
            };
        }

        public Dictionary<uint, Texture> GetDictionary()
        {
            Dictionary<uint, Texture> td = new Dictionary<uint, Texture>();
            if ((Textures != null) && (Textures.data_items != null))
            {
                var texs = Textures.data_items;
                var hashes = TextureNameHashes;
                for (int i = 0; (i < texs.Length) && (i < hashes.Length); i++)
                {
                    td.Add(hashes[i], texs[i]);
                }
            }
            return td;
        }

        public Texture Lookup(uint hash)
        {
            Texture tex = null;
            if (Dict != null)
            {
                Dict.TryGetValue(hash, out tex);
            }
            return tex;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class TextureBase : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 64; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public uint Unknown_30h { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000

        // reference data
        public string Name { get; set; }
        public uint NameHash { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadUInt32();
            this.Unknown_24h = reader.ReadUInt32();
            this.NamePointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadStringAt( //BlockAt<string_r>(
                this.NamePointer // offset
            );

            if (!string.IsNullOrEmpty(Name))
            {
                NameHash = JenkHash.GenHash(Name.ToLower());
            }
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.NamePointer = (ulong)(this.Name != null ? this.Name.Position : 0); //TODO: fix

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.NamePointer);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Name != null) list.Add(Name); //TODO: fix
            return list.ToArray();
        }

        public override string ToString()
        {
            return "TextureBase: " + Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class Texture : TextureBase
    {
        public override long BlockLength
        {
            get { return 144; }
        }

        // structure data
        public uint Unknown_40h { get; set; }
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint Unknown_48h { get; set; }
        public uint Unknown_4Ch { get; set; } // 0x00000000
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public ushort Unknown_54h { get; set; } // 0x0001
        public ushort Stride { get; set; }
        public TextureFormat Format { get; set; }
        public byte Unknown_5Ch { get; set; } // 0x00
        public byte Levels { get; set; }
        public ushort Unknown_5Eh { get; set; } // 0x0000
        public uint Unknown_60h { get; set; } // 0x00000000
        public uint Unknown_64h { get; set; } // 0x00000000
        public uint Unknown_68h { get; set; } // 0x00000000
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public ulong DataPointer { get; set; }
        public uint Unknown_78h { get; set; } // 0x00000000
        public uint Unknown_7Ch { get; set; } // 0x00000000
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000

        // reference data
        public TextureData Data { get; set; }

        public long MemoryUsage
        {
            get
            {
                long val = 0;
                if (Data != null)
                {
                    val += Data.FullData.LongLength;
                }
                return val;
            }
        }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_40h = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.Unknown_48h = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Width = reader.ReadUInt16();
            this.Height = reader.ReadUInt16();
            this.Unknown_54h = reader.ReadUInt16();
            this.Stride = reader.ReadUInt16();
            this.Format = (TextureFormat)reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadByte();
            this.Levels = reader.ReadByte();
            this.Unknown_5Eh = reader.ReadUInt16();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.DataPointer = reader.ReadUInt64();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();

            // read reference data
            this.Data = reader.ReadBlockAt<TextureData>(
                this.DataPointer, // offset
                this.Format,
                this.Width,
                this.Height,
                this.Levels,
                this.Stride
            );
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            this.DataPointer = (ulong)this.Data.FilePosition;

            // write structure data
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_44h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Width);
            writer.Write(this.Height);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Stride);
            writer.Write((uint)this.Format);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Levels);
            writer.Write(this.Unknown_5Eh);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.DataPointer);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            list.Add(Data);
            return list.ToArray();
        }

        public override string ToString()
        {
            return "Texture: " + Width.ToString() + "x" + Height.ToString() + ": " + Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class TextureData : ResourceGraphicsBlock
    {
        public override long BlockLength
        {
            get
            {
                return FullData.Length;
            }
        }

        public byte[] FullData { get; set; }

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            uint format = Convert.ToUInt32(parameters[0]);
            int Width = Convert.ToInt32(parameters[1]);
            int Height = Convert.ToInt32(parameters[2]);
            int Levels = Convert.ToInt32(parameters[3]);
            int Stride = Convert.ToInt32(parameters[4]);

            int fullLength = 0;
            int length = Stride * Height;
            for (int i = 0; i < Levels; i++)
            {
                fullLength += length;
                length /= 4;
            }

            FullData = reader.ReadBytes(fullLength);
        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(FullData);
        }
    }


    public enum TextureFormat : uint
    {
        D3DFMT_A8R8G8B8 = 21,
        D3DFMT_A1R5G5B5 = 25,
        D3DFMT_A8 = 28,
        D3DFMT_A8B8G8R8 = 32,
        D3DFMT_L8 = 50,

        // fourCC
        D3DFMT_DXT1 = 0x31545844,
        D3DFMT_DXT3 = 0x33545844,
        D3DFMT_DXT5 = 0x35545844,
        D3DFMT_ATI1 = 0x31495441,
        D3DFMT_ATI2 = 0x32495441,
        D3DFMT_BC7 = 0x20374342,

        //UNKNOWN
    }

    public static class TextureFormats
    {
        public static SharpDX.DXGI.Format GetDXGIFormat(TextureFormat fmt)
        {
            SharpDX.DXGI.Format format = SharpDX.DXGI.Format.Unknown;
            switch (fmt)
            {
                // compressed
                case TextureFormat.D3DFMT_DXT1: format = SharpDX.DXGI.Format.BC1_UNorm; break;
                case TextureFormat.D3DFMT_DXT3: format = SharpDX.DXGI.Format.BC2_UNorm; break;
                case TextureFormat.D3DFMT_DXT5: format = SharpDX.DXGI.Format.BC3_UNorm; break;
                case TextureFormat.D3DFMT_ATI1: format = SharpDX.DXGI.Format.BC4_UNorm; break;
                case TextureFormat.D3DFMT_ATI2: format = SharpDX.DXGI.Format.BC5_UNorm; break;
                case TextureFormat.D3DFMT_BC7: format = SharpDX.DXGI.Format.BC7_UNorm; break;

                // uncompressed
                case TextureFormat.D3DFMT_A1R5G5B5: format = SharpDX.DXGI.Format.B5G5R5A1_UNorm; break;
                case TextureFormat.D3DFMT_A8: format = SharpDX.DXGI.Format.A8_UNorm; break;
                case TextureFormat.D3DFMT_A8B8G8R8: format = SharpDX.DXGI.Format.R8G8B8A8_UNorm; break;
                case TextureFormat.D3DFMT_L8: format = SharpDX.DXGI.Format.R8_UNorm; break;
                case TextureFormat.D3DFMT_A8R8G8B8: format = SharpDX.DXGI.Format.B8G8R8A8_UNorm; break;
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
