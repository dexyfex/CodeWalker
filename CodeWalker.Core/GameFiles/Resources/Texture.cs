using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        public uint Unknown_18h { get; set; } = 1; // 0x00000001
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourceSimpleList64_uint TextureNameHashes { get; set; }
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
        { }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.TextureNameHashes = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Textures = reader.ReadBlock<ResourcePointerList64<Texture>>();

            BuildDict();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);


            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.TextureNameHashes);
            writer.WriteBlock(this.Textures);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {

            if (Textures?.data_items != null)
            {
                foreach (var tex in Textures.data_items)
                {
                    YtdXml.OpenTag(sb, indent, "Item");
                    tex.WriteXml(sb, indent + 1, ddsfolder);
                    YtdXml.CloseTag(sb, indent, "Item");
                }
            }

        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            var textures = new List<Texture>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var tex = new Texture();
                    tex.ReadXml(inode, ddsfolder);
                    textures.Add(tex);
                }
            }

            BuildFromTextureList(textures);
        }
        public static void WriteXmlNode(TextureDictionary d, StringBuilder sb, int indent, string ddsfolder, string name = "TextureDictionary")
        {
            if (d == null) return;
            if ((d.Textures?.data_items == null) || (d.Textures.data_items.Length == 0))
            {
                YtdXml.SelfClosingTag(sb, indent, name);
            }
            else
            {
                YtdXml.OpenTag(sb, indent, name);
                d.WriteXml(sb, indent + 1, ddsfolder);
                YtdXml.CloseTag(sb, indent, name);
            }
        }
        public static TextureDictionary ReadXmlNode(XmlNode node, string ddsfolder)
        {
            if (node == null) return null;
            var td = new TextureDictionary();
            td.ReadXml(node, ddsfolder);
            return td;
        }


        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, TextureNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Textures)
            };
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

        private void BuildDict()
        {
            var dict = new Dictionary<uint, Texture>();
            if ((Textures?.data_items != null) && (TextureNameHashes?.data_items != null))
            {
                for (int i = 0; (i < Textures.data_items.Length) && (i < TextureNameHashes.data_items.Length); i++)
                {
                    var tex = Textures.data_items[i];
                    var hash = TextureNameHashes.data_items[i];
                    dict[hash] = tex;
                }
            }
            Dict = dict;
        }

        public void BuildFromTextureList(List<Texture> textures)
        {
            textures.Sort((a, b) => a.NameHash.CompareTo(b.NameHash));
            
            var texturehashes = new List<uint>();
            foreach (var tex in textures)
            {
                texturehashes.Add(tex.NameHash);
            }

            TextureNameHashes = new ResourceSimpleList64_uint();
            TextureNameHashes.data_items = texturehashes.ToArray();
            Textures = new ResourcePointerList64<Texture>();
            Textures.data_items = textures.ToArray();
            BuildDict();
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class TextureBase : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 80; }
        }

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } = 1; // 0x00000001
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public uint Unknown_20h { get; set; } // 0x00000000
        public uint Unknown_24h { get; set; } // 0x00000000
        public ulong NamePointer { get; set; }
        public ushort Unknown_30h { get; set; } = 1;
        public ushort Unknown_32h { get; set; }
        public uint Unknown_34h { get; set; } // 0x00000000
        public uint Unknown_38h { get; set; } // 0x00000000
        public uint Unknown_3Ch { get; set; } // 0x00000000
        public uint UsageData { get; set; }
        public uint Unknown_44h { get; set; } // 0x00000000
        public uint ExtraFlags { get; set; } // 0, 1
        public uint Unknown_4Ch { get; set; } // 0x00000000

        // reference data
        public string Name { get; set; }
        public uint NameHash { get; set; }

        private string_r NameBlock = null;



        public TextureUsage Usage
        {
            get
            {
                return (TextureUsage)(UsageData & 0x1F);
            }
            set
            {
                UsageData = (UsageData & 0xFFFFFFE0) + (((uint)value) & 0x1F);
            }
        }
        public TextureUsageFlags UsageFlags
        {
            get
            {
                return (TextureUsageFlags)(UsageData >> 5);
            }
            set
            {
                UsageData = (UsageData & 0x1F) + (((uint)value) << 5);
            }
        }



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
            this.Unknown_30h = reader.ReadUInt16();
            this.Unknown_32h = reader.ReadUInt16();
            this.Unknown_34h = reader.ReadUInt32();
            this.Unknown_38h = reader.ReadUInt32();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.UsageData = reader.ReadUInt32();
            this.Unknown_44h = reader.ReadUInt32();
            this.ExtraFlags = reader.ReadUInt32();
            this.Unknown_4Ch = reader.ReadUInt32();



            // read reference data
            this.Name = reader.ReadStringAt( //BlockAt<string_r>(
                this.NamePointer // offset
            );

            if (!string.IsNullOrEmpty(Name))
            {
                NameHash = JenkHash.GenHash(Name.ToLowerInvariant());
            }

            //switch (Unknown_32h)
            //{
            //    case 0x20:
            //    case 0x28:
            //    case 0x30:
            //    case 0x38:
            //    case 0x40:
            //    case 0x48:
            //    case 0x80:
            //    case 0x90:
            //    case 0x2://base/shaderparam
            //        break;
            //    default:
            //        break;//no hit
            //}

            //switch (Usage)
            //{
            //    case TextureUsage.UNKNOWN:// = 0,
            //    case TextureUsage.DEFAULT:// = 1,
            //    case TextureUsage.TERRAIN:// = 2,
            //    case TextureUsage.CLOUDDENSITY:// = 3,
            //    case TextureUsage.CLOUDNORMAL:// = 4,
            //    case TextureUsage.CABLE:// = 5,
            //    case TextureUsage.FENCE:// = 6,
            //    case TextureUsage.SCRIPT:// = 8,
            //    case TextureUsage.WATERFLOW:// = 9,
            //    case TextureUsage.WATERFOAM:// = 10,
            //    case TextureUsage.WATERFOG:// = 11,
            //    case TextureUsage.WATEROCEAN:// = 12,
            //    case TextureUsage.FOAMOPACITY:// = 14,
            //    case TextureUsage.DIFFUSEMIPSHARPEN:// = 16,
            //    case TextureUsage.DIFFUSEDARK:// = 18,
            //    case TextureUsage.DIFFUSEALPHAOPAQUE:// = 19,
            //    case TextureUsage.DIFFUSE:// = 20,
            //    case TextureUsage.DETAIL:// = 21,
            //    case TextureUsage.NORMAL:// = 22,
            //    case TextureUsage.SPECULAR:// = 23,
            //    case TextureUsage.EMISSIVE:// = 24,
            //    case TextureUsage.TINTPALETTE:// = 25,
            //    case TextureUsage.SKIPPROCESSING:// = 26,
            //        break;
            //    case TextureUsage.ENVEFF:// = 7, //unused by V
            //    case TextureUsage.WATER:// = 13, //unused by V
            //    case TextureUsage.FOAM:// = 15,  //unused by V
            //    case TextureUsage.DIFFUSEDETAIL:// = 17, //unused by V
            //    case TextureUsage.DONOTOPTIMIZE:// = 27, //unused by V
            //    case TextureUsage.TEST:// = 28,  //unused by V
            //    case TextureUsage.COUNT:// = 29, //unused by V
            //        break;//no hit
            //    default:
            //        break;//no hit
            //}

            //var uf = UsageFlags;
            //if ((uf & TextureUsageFlags.EMBEDDEDSCRIPTRT) > 0) // .ydr embedded script_rt textures, only 3 uses
            //{ }
            //if ((uf & TextureUsageFlags.UNK19) > 0)
            //{ }//no hit
            //if ((uf & TextureUsageFlags.UNK20) > 0)
            //{ }//no hit
            //if ((uf & TextureUsageFlags.UNK21) > 0)
            //{ }//no hit
            //if ((uf & TextureUsageFlags.UNK24) == 0)//wtf isthis? only 0 on special resident(?) textures and some reused ones
            //{ }

            //if (!(this is Texture))
            //{
            //    if (Unknown_32h != 0x2)//base/shaderparam
            //    { }//no hit
            //    if (UsageData != 0)
            //    { }//no hit
            //    if (Unknown_44h != 0)
            //    { }//no hit
            //    if (ExtraFlags != 0)
            //    { }//no hit
            //    if (Unknown_4Ch != 0)
            //    { }//no hit
            //}

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.NameBlock != null ? this.NameBlock.FilePosition : 0);

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
            writer.Write(this.Unknown_32h);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.UsageData);
            writer.Write(this.Unknown_44h);
            writer.Write(this.ExtraFlags);
            writer.Write(this.Unknown_4Ch);
        }
        public virtual void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YtdXml.StringTag(sb, indent, "Name", YtdXml.XmlEscape(Name));
            YtdXml.ValueTag(sb, indent, "Unk32", Unknown_32h.ToString());
            YtdXml.StringTag(sb, indent, "Usage", Usage.ToString());
            YtdXml.StringTag(sb, indent, "UsageFlags", UsageFlags.ToString());
            YtdXml.ValueTag(sb, indent, "ExtraFlags", ExtraFlags.ToString());
        }
        public virtual void ReadXml(XmlNode node, string ddsfolder)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameHash = JenkHash.GenHash(Name?.ToLowerInvariant());
            Unknown_32h = (ushort)Xml.GetChildUIntAttribute(node, "Unk32", "value");
            Usage = Xml.GetChildEnumInnerText<TextureUsage>(node, "Usage");
            UsageFlags = Xml.GetChildEnumInnerText<TextureUsageFlags>(node, "UsageFlags");
            ExtraFlags = Xml.GetChildUIntAttribute(node, "ExtraFlags", "value");
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (!string.IsNullOrEmpty(Name))
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
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
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public ushort Depth { get; set; } = 1;  //is depth > 1 supported?
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

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Width = reader.ReadUInt16();
            this.Height = reader.ReadUInt16();
            this.Depth = reader.ReadUInt16();
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
            this.Data = reader.ReadBlockAt<TextureData>(this.DataPointer, this.Format, this.Width, this.Height, this.Levels, this.Stride);

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            this.DataPointer = (ulong)this.Data.FilePosition;

            // write structure data
            writer.Write(this.Width);
            writer.Write(this.Height);
            writer.Write(this.Depth);
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
        public override void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            base.WriteXml(sb, indent, ddsfolder);
            YtdXml.ValueTag(sb, indent, "Width", Width.ToString());
            YtdXml.ValueTag(sb, indent, "Height", Height.ToString());
            YtdXml.ValueTag(sb, indent, "MipLevels", Levels.ToString());
            YtdXml.StringTag(sb, indent, "Format", Format.ToString());
            YtdXml.StringTag(sb, indent, "FileName", YtdXml.XmlEscape((Name ?? "null") + ".dds"));

            try
            {
                if (!string.IsNullOrEmpty(ddsfolder))
                {
                    if (!Directory.Exists(ddsfolder))
                    {
                        Directory.CreateDirectory(ddsfolder);
                    }
                    var filepath = Path.Combine(ddsfolder, (Name ?? "null") + ".dds");
                    var dds = DDSIO.GetDDSFile(this);
                    File.WriteAllBytes(filepath, dds);
                }
            }
            catch { }
        }
        public override void ReadXml(XmlNode node, string ddsfolder)
        {
            base.ReadXml(node, ddsfolder);
            Width = (ushort)Xml.GetChildUIntAttribute(node, "Width", "value");
            Height = (ushort)Xml.GetChildUIntAttribute(node, "Height", "value");
            Levels = (byte)Xml.GetChildUIntAttribute(node, "MipLevels", "value");
            Format = Xml.GetChildEnumInnerText<TextureFormat>(node, "Format");
            var filename = Xml.GetChildInnerText(node, "FileName");


            if ((!string.IsNullOrEmpty(filename)) && (!string.IsNullOrEmpty(ddsfolder)))
            {
                var filepath = Path.Combine(ddsfolder, filename);
                if (File.Exists(filepath))
                {
                    try
                    {
                        var dds = File.ReadAllBytes(filepath);
                        var tex = DDSIO.GetTexture(dds);
                        if (tex != null)
                        {
                            Data = tex.Data;
                            Width = tex.Width;
                            Height = tex.Height;
                            Depth = tex.Depth;
                            Levels = tex.Levels;
                            Format = tex.Format;
                            Stride = tex.Stride;
                        }
                    }
                    catch
                    {
                        throw new Exception("Texture file format not supported:\n" + filepath);
                    }
                }
                else
                {
                    throw new Exception("Texture file not found:\n" + filepath);
                }
            }


        }

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
        D3DFMT_X8R8G8B8 = 22,
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

    public enum TextureUsage : byte
    {
        UNKNOWN = 0,
        DEFAULT = 1,
        TERRAIN = 2,
        CLOUDDENSITY = 3,
        CLOUDNORMAL = 4,
        CABLE = 5,
        FENCE = 6,
        ENVEFF = 7, //unused by V
        SCRIPT = 8,
        WATERFLOW = 9,
        WATERFOAM = 10,
        WATERFOG = 11,
        WATEROCEAN = 12,
        WATER = 13, //unused by V
        FOAMOPACITY = 14,
        FOAM = 15,  //unused by V
        DIFFUSEMIPSHARPEN = 16,
        DIFFUSEDETAIL = 17, //unused by V
        DIFFUSEDARK = 18,
        DIFFUSEALPHAOPAQUE = 19,
        DIFFUSE = 20,
        DETAIL = 21,
        NORMAL = 22,
        SPECULAR = 23,
        EMISSIVE = 24,
        TINTPALETTE = 25,
        SKIPPROCESSING = 26,
        DONOTOPTIMIZE = 27, //unused by V
        TEST = 28,  //unused by V
        COUNT = 29, //unused by V
    }
    [Flags]
    public enum TextureUsageFlags : uint
    {
        NOT_HALF = 1,
        HD_SPLIT = (1 << 1),
        X2 = (1 << 2),
        X4 = (1 << 3),
        Y4 = (1 << 4),
        X8 = (1 << 5),
        X16 = (1 << 6),
        X32 = (1 << 7),
        X64 = (1 << 8),
        Y64 = (1 << 9),
        X128 = (1 << 10),
        X256 = (1 << 11),
        X512 = (1 << 12),
        Y512 = (1 << 13),
        X1024 = (1 << 14),//wtf is all this?
        Y1024 = (1 << 15),
        X2048 = (1 << 16),
        Y2048 = (1 << 17),
        EMBEDDEDSCRIPTRT = (1 << 18),
        UNK19 = (1 << 19),  //unused by V
        UNK20 = (1 << 20),  //unused by V
        UNK21 = (1 << 21),  //unused by V
        FLAG_FULL = (1 << 22),
        MAPS_HALF = (1 << 23),
        UNK24 = (1 << 24),//used by almost everything...
    }


}
