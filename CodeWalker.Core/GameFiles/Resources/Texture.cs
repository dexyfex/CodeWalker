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
        public override long BlockLength => 64;

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


        public void EnsureGen9()
        {
            FileVFT = 0;
            FileUnknown = 1;

            //make sure textures all have SRVs and are appropriately formatted for gen9
            var texs = Textures?.data_items;
            if (texs == null) return;
            foreach (var tex in texs)
            {
                if (tex == null) continue;
                tex.EnsureGen9();
            }
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class TextureBase : ResourceSystemBlock
    {
        public override long BlockLength => 80;
        public override long BlockLength_Gen9 => 80;

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

        //Texture subclass structure data - moved here for gen9 compatibility
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


        //gen9 extra structure data
        public uint G9_BlockCount { get; set; }
        public uint G9_BlockStride { get; set; }
        public uint G9_Flags { get; set; }
        public TextureDimensionG9 G9_Dimension { get; set; } = TextureDimensionG9.Texture2D;
        public TextureFormatG9 G9_Format { get; set; }
        public TextureTileModeG9 G9_TileMode { get; set; } = TextureTileModeG9.Auto;
        public byte G9_AntiAliasType { get; set; } //0
        public byte G9_Unknown_23h { get; set; }
        public byte G9_Unknown_25h { get; set; }
        public ushort G9_UsageCount { get; set; } = 1;
        public ulong G9_SRVPointer { get; set; }
        public uint G9_UsageData { get; set; }
        public ulong G9_Unknown_48h { get; set; }


        // reference data
        public string Name { get; set; }
        public uint NameHash { get; set; }

        private string_r NameBlock = null;

        public TextureData Data { get; set; }

        public ShaderResourceViewG9 G9_SRV { get; set; }//make sure this is null if saving legacy version!



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
            if (reader.IsGen9)
            {

                VFT = reader.ReadUInt32();
                Unknown_4h = reader.ReadUInt32();
                G9_BlockCount = reader.ReadUInt32();
                G9_BlockStride = reader.ReadUInt32();
                G9_Flags = reader.ReadUInt32();
                Unknown_14h = reader.ReadUInt32();
                Width = reader.ReadUInt16();                    // rage::sga::ImageParams 24
                Height = reader.ReadUInt16();
                Depth = reader.ReadUInt16();
                G9_Dimension = (TextureDimensionG9)reader.ReadByte();
                G9_Format = (TextureFormatG9)reader.ReadByte();
                G9_TileMode = (TextureTileModeG9)reader.ReadByte();
                G9_AntiAliasType = reader.ReadByte();
                Levels = reader.ReadByte();
                G9_Unknown_23h = reader.ReadByte();
                Unknown_24h = reader.ReadByte();
                G9_Unknown_25h = reader.ReadByte();
                G9_UsageCount = reader.ReadUInt16();
                NamePointer = reader.ReadUInt64();
                G9_SRVPointer = reader.ReadUInt64();
                DataPointer = reader.ReadUInt64();
                G9_UsageData = reader.ReadUInt32();
                Unknown_44h = reader.ReadUInt32();//2 (or 0 for shader param)
                G9_Unknown_48h = reader.ReadUInt64();

                Format = GetLegacyFormat(G9_Format);
                Stride = CalculateStride();
                Usage = (TextureUsage)(G9_UsageData & 0x1F);

                Data = reader.ReadBlockAt<TextureData>(DataPointer, CalcDataSize());
                G9_SRV = reader.ReadBlockAt<ShaderResourceViewG9>(G9_SRVPointer);

                Name = reader.ReadStringAt(NamePointer);
                if (!string.IsNullOrEmpty(Name))
                {
                    NameHash = JenkHash.GenHash(Name.ToLowerInvariant());
                }

                switch (G9_Flags)
                {
                    case 0x00260208:
                    case 0x00260228:
                    case 0x00260000:
                        break;
                    default:
                        break;
                }
                if (Unknown_14h != 0)
                { }
                switch (G9_Dimension)
                {
                    case TextureDimensionG9.Texture2D:
                    case TextureDimensionG9.Texture3D:
                        break;
                    default:
                        break;
                }
                if (G9_TileMode != TextureTileModeG9.Auto)
                { }
                if (G9_AntiAliasType != 0)
                { }
                switch (G9_Unknown_23h)
                {
                    case 0x28:
                    case 0x2a:
                    case 0:
                        break;
                    default:
                        break;
                }
                if (Unknown_24h != 0)
                { }
                if (G9_Unknown_25h != 0)
                { }
                if (G9_UsageCount != 1)
                { }
                switch (Usage)
                {
                    case TextureUsage.DETAIL:
                    case TextureUsage.NORMAL:
                    case TextureUsage.DIFFUSE:
                    case TextureUsage.SPECULAR:
                    case TextureUsage.DEFAULT:
                    case TextureUsage.SKIPPROCESSING:
                    case TextureUsage.UNKNOWN:
                    case TextureUsage.WATEROCEAN:
                    case TextureUsage.CLOUDNORMAL:
                    case TextureUsage.CLOUDDENSITY:
                    case TextureUsage.TERRAIN:
                    case TextureUsage.CABLE:
                    case TextureUsage.FENCE:
                    case TextureUsage.SCRIPT:
                    case TextureUsage.DIFFUSEDARK:
                    case TextureUsage.DIFFUSEALPHAOPAQUE:
                    case TextureUsage.TINTPALETTE:
                    case TextureUsage.FOAMOPACITY:
                    case TextureUsage.WATERFLOW:
                    case TextureUsage.WATERFOG:
                    case TextureUsage.EMISSIVE:
                    case TextureUsage.WATERFOAM:
                    case TextureUsage.DIFFUSEMIPSHARPEN:
                        break;
                    default:
                        break;
                }
                switch (G9_UsageData >> 5)//usage flags??
                {
                    case 0x00040010:
                    case 0x00014010:
                    case 0x00020010:
                    case 0x00018010:
                    case 0x00010010:
                    case 0x00040000:
                    case 0x00048010:
                    case 0x0001c010:
                    case 0x00010000:
                    case 0x00024010:
                    case 0x00040013:
                    case 0x00040011:
                    case 0x00040012:
                    case 0x00010012:
                    case 0x00010013:
                    case 0x00014012:
                    case 0x00040014:
                    case 0x00010011:
                    case 0://(shader params)
                        break;
                    default:
                        break;
                }
                switch (Unknown_44h)
                {
                    case 2:
                    case 0://(shader params)
                        break;
                    default:
                        break;
                }
                if (G9_Unknown_48h != 0)
                { }

            }
            else
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
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (writer.IsGen9)
            {
                NamePointer = (ulong)(NameBlock != null ? NameBlock.FilePosition : 0);
                DataPointer = (ulong)(Data != null ? Data.FilePosition : 0);
                G9_SRVPointer = (ulong)(G9_SRV != null ? G9_SRV.FilePosition : 0);
                if (G9_Format == 0) G9_Format = GetEnhancedFormat(Format);
                if (G9_Dimension == 0) G9_Dimension = TextureDimensionG9.Texture2D;//TODO?
                if (G9_TileMode == 0) G9_TileMode = TextureTileModeG9.Auto;//TODO?
                G9_BlockCount = GetBlockCount(G9_Format, Width, Height, Depth, Levels, G9_Flags, G9_BlockCount);
                G9_BlockStride = GetBlockStride(G9_Format);
                G9_UsageData = (G9_UsageData & 0xFFFFFFE0) + (((uint)Usage) & 0x1F);
                //G9_Flags = ... TODO??


                writer.Write(VFT);
                writer.Write(Unknown_4h);
                writer.Write(G9_BlockCount);
                writer.Write(G9_BlockStride);
                writer.Write(G9_Flags);
                writer.Write(Unknown_14h);
                writer.Write(Width);                    // rage::sga::ImageParams 24
                writer.Write(Height);
                writer.Write(Depth);
                writer.Write((byte)G9_Dimension);
                writer.Write((byte)G9_Format);
                writer.Write((byte)G9_TileMode);
                writer.Write(G9_AntiAliasType);
                writer.Write(Levels);
                writer.Write(G9_Unknown_23h);
                writer.Write((byte)Unknown_24h);
                writer.Write(G9_Unknown_25h);
                writer.Write(G9_UsageCount);
                writer.Write(NamePointer);
                writer.Write(G9_SRVPointer);
                writer.Write(DataPointer);
                writer.Write(G9_UsageData);
                writer.Write(Unknown_44h);
                writer.Write(G9_Unknown_48h);

            }
            else
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



        public void EnsureGen9()
        {
            VFT = 0;
            Unknown_4h = 1;

            var istex = this is Texture;

            Unknown_44h = istex ? 2 : 0u;

            if (G9_Flags == 0)
            {
                G9_Flags = 0x00260208;//TODO...
                if (Name?.ToLowerInvariant()?.StartsWith("script_rt_") ?? false)
                {
                    G9_Flags = 0x00260228;
                }
            }
            if ((G9_Unknown_23h == 0) && istex)
            {
                G9_Unknown_23h = 0x28;//TODO...
            }

            if (G9_SRV == null)
            {
                G9_SRV = new ShaderResourceViewG9();
                G9_SRV.Dimension = ShaderResourceViewDimensionG9.Texture2D;
                if (Depth > 1)
                {
                    G9_SRV.Dimension = ShaderResourceViewDimensionG9.Texture2DArray;
                    //TODO: handle Texture3D!
                }
            }
        }


        public int CalcDataSize()
        {
            if (Format == 0) return 0;
            var dxgifmt = DDSIO.GetDXGIFormat(Format);
            int div = 1;
            int len = 0;
            bool compressed = DDSIO.DXTex.IsCompressed(dxgifmt);
            int minimumLengthPerMip = compressed ? (IsBC1Based(dxgifmt) ? 8 : 16) : DDSIO.DXTex.BitsPerPixel(dxgifmt) / 8;
            for (int i = 0; i < Levels; i++)
            {
                // Width or Height may reach 1 before the last mip level, half of 1 would be 0.5 truncated to 0.
                // A texture can't have a dimension of 0, so we need to floor at 1.
                var width = Math.Max(1, Width / div);
                var height = Math.Max(1, Height / div);
                DDSIO.DXTex.ComputePitch(dxgifmt, width, height, out var rowPitch, out var slicePitch, 0);
                len += Math.Max(minimumLengthPerMip, slicePitch);
                div *= 2;
            }
            
            return len * Depth;
        }

        private bool IsBC1Based(DDSIO.DXGI_FORMAT format)
        {
            return format == DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS || format == DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM || format == DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB;
        }
        
        public ushort CalculateStride()
        {
            if (Format == 0) return 0;
            var dxgifmt = DDSIO.GetDXGIFormat(Format);
            DDSIO.DXTex.ComputePitch(dxgifmt, Width, Height, out var rowPitch, out var slicePitch, 0);
            return (ushort)rowPitch;
        }
        public TextureFormat GetLegacyFormat(TextureFormatG9 format)
        {
            switch (format)
            {
                case TextureFormatG9.UNKNOWN: return 0;
                case TextureFormatG9.R8G8B8A8_UNORM: return TextureFormat.D3DFMT_A8B8G8R8;
                case TextureFormatG9.B8G8R8A8_UNORM: return TextureFormat.D3DFMT_A8R8G8B8;
                case TextureFormatG9.A8_UNORM: return TextureFormat.D3DFMT_A8;
                case TextureFormatG9.R8_UNORM: return TextureFormat.D3DFMT_L8;
                case TextureFormatG9.B5G5R5A1_UNORM: return TextureFormat.D3DFMT_A1R5G5B5;
                case TextureFormatG9.BC1_UNORM: return TextureFormat.D3DFMT_DXT1;
                case TextureFormatG9.BC2_UNORM: return TextureFormat.D3DFMT_DXT3;
                case TextureFormatG9.BC3_UNORM: return TextureFormat.D3DFMT_DXT5;
                case TextureFormatG9.BC4_UNORM: return TextureFormat.D3DFMT_ATI1;
                case TextureFormatG9.BC5_UNORM: return TextureFormat.D3DFMT_ATI2;
                case TextureFormatG9.BC7_UNORM: return TextureFormat.D3DFMT_BC7;
                case TextureFormatG9.BC7_UNORM_SRGB: return TextureFormat.D3DFMT_BC7;//TODO
                case TextureFormatG9.BC3_UNORM_SRGB: return TextureFormat.D3DFMT_DXT5;//TODO
                case TextureFormatG9.R16_UNORM: return TextureFormat.D3DFMT_A8;//TODO
                default: return TextureFormat.D3DFMT_A8R8G8B8;
            }
        }
        public TextureFormatG9 GetEnhancedFormat(TextureFormat format)
        {
            switch (format)
            {
                case (TextureFormat)0: return TextureFormatG9.UNKNOWN;
                case TextureFormat.D3DFMT_A8B8G8R8: return TextureFormatG9.R8G8B8A8_UNORM;
                case TextureFormat.D3DFMT_A8R8G8B8: return TextureFormatG9.B8G8R8A8_UNORM;
                case TextureFormat.D3DFMT_A8: return TextureFormatG9.A8_UNORM;
                case TextureFormat.D3DFMT_L8: return TextureFormatG9.R8_UNORM;
                case TextureFormat.D3DFMT_A1R5G5B5: return TextureFormatG9.B5G5R5A1_UNORM;
                case TextureFormat.D3DFMT_DXT1: return TextureFormatG9.BC1_UNORM;
                case TextureFormat.D3DFMT_DXT3: return TextureFormatG9.BC2_UNORM;
                case TextureFormat.D3DFMT_DXT5: return TextureFormatG9.BC3_UNORM;
                case TextureFormat.D3DFMT_ATI1: return TextureFormatG9.BC4_UNORM;
                case TextureFormat.D3DFMT_ATI2: return TextureFormatG9.BC5_UNORM;
                case TextureFormat.D3DFMT_BC7: return TextureFormatG9.BC7_UNORM;
                //case TextureFormat.D3DFMT_BC7: return TextureFormatG9.BC7_UNORM_SRGB;//TODO
                //case TextureFormat.D3DFMT_DXT5: return TextureFormatG9.BC3_UNORM_SRGB;//TODO
                //case TextureFormat.D3DFMT_A8: return TextureFormatG9.R16_UNORM;//TODO
                default: return TextureFormatG9.B8G8R8A8_UNORM;
            }
        }

        public static uint GetBlockStride(TextureFormatG9 f)
        {
            //pixel size for uncompressed formats, block size for compressed
            switch (f)
            {
                default: return 8;
                case TextureFormatG9.UNKNOWN: return 0;
                case TextureFormatG9.BC1_UNORM: return 8;
                case TextureFormatG9.BC2_UNORM: return 16;
                case TextureFormatG9.BC3_UNORM: return 16;
                case TextureFormatG9.BC4_UNORM: return 8;
                case TextureFormatG9.BC5_UNORM: return 16;
                case TextureFormatG9.BC6H_UF16: return 16;
                case TextureFormatG9.BC7_UNORM: return 16;
                case TextureFormatG9.BC1_UNORM_SRGB: return 8;
                case TextureFormatG9.BC2_UNORM_SRGB: return 16;
                case TextureFormatG9.BC3_UNORM_SRGB: return 16;
                case TextureFormatG9.BC7_UNORM_SRGB: return 16;
                case TextureFormatG9.R8G8B8A8_UNORM: return 4;
                case TextureFormatG9.B8G8R8A8_UNORM: return 4;
                case TextureFormatG9.R8G8B8A8_UNORM_SRGB: return 4;
                case TextureFormatG9.B8G8R8A8_UNORM_SRGB: return 4;
                case TextureFormatG9.B5G5R5A1_UNORM: return 2;
                case TextureFormatG9.R10G10B10A2_UNORM: return 4;
                case TextureFormatG9.R16G16B16A16_UNORM: return 8;
                case TextureFormatG9.R16G16B16A16_FLOAT: return 8;
                case TextureFormatG9.R16_UNORM: return 2;
                case TextureFormatG9.R16_FLOAT: return 2;
                case TextureFormatG9.R8_UNORM: return 1;
                case TextureFormatG9.A8_UNORM: return 1;
                case TextureFormatG9.R32_FLOAT: return 4;
                case TextureFormatG9.R32G32B32A32_FLOAT: return 16;
                case TextureFormatG9.R11G11B10_FLOAT: return 4;
            }
        }
        public static uint GetBlockPixelCount(TextureFormatG9 f)
        {
            switch (f)
            {
                default:
                    return 1;
                case TextureFormatG9.BC1_UNORM:
                case TextureFormatG9.BC2_UNORM:
                case TextureFormatG9.BC3_UNORM:
                case TextureFormatG9.BC4_UNORM:
                case TextureFormatG9.BC5_UNORM:
                case TextureFormatG9.BC6H_UF16:
                case TextureFormatG9.BC7_UNORM:
                case TextureFormatG9.BC1_UNORM_SRGB:
                case TextureFormatG9.BC2_UNORM_SRGB:
                case TextureFormatG9.BC3_UNORM_SRGB:
                case TextureFormatG9.BC7_UNORM_SRGB:
                    return 4;
            }
        }
        public static uint GetBlockCount(TextureFormatG9 f, uint width, uint height, uint depth, uint mips, uint flags, uint oldval = 0)
        {
            if (f == TextureFormatG9.UNKNOWN) return 0;

            var bs = GetBlockStride(f);
            var bp = GetBlockPixelCount(f);
            var bw = (uint)width;
            var bh = (uint)height;
            var bd = (uint)depth;
            var bm = (uint)mips;

            var align = 1u;// (bs == 1) ? 16u : 8u;
            if (mips > 1)
            {
                bw = 1; while (bw < width) bw *= 2;
                bh = 1; while (bh < height) bh *= 2;
                bd = 1; while (bd < depth) bd *= 2;
            }

            var bc = 0u;
            for (int i = 0; i < mips; i++)
            {
                var bx = Math.Max(1, (bw + bp - 1) / bp);
                var by = Math.Max(1, (bh + bp - 1) / bp);
                bx += (align - (bx % align)) % align;
                by += (align - (by % align)) % align;
                bc += bx * by * bd;
                bw /= 2;
                bh /= 2;
            }


            if (bc != oldval)
            {
                if (f != TextureFormatG9.A8_UNORM)
                {
                    if (bp == 1)
                    { }
                    if (bd == 1)
                    { }
                }
                else
                { }
            }

            return bc;
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
        public override long BlockLength => 144;
        public override long BlockLength_Gen9 => 128;


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
            
            if (reader.IsGen9)
            {
                var unk50 = reader.ReadUInt64();//0
                var srv58 = reader.ReadUInt64();//SRV embedded at offset 88 (base+8)
                var srv60 = reader.ReadUInt64();
                var srv68 = reader.ReadUInt64();
                var srv70 = reader.ReadUInt64();
                var unk78 = reader.ReadUInt64();//0
            }
            else
            {

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
                
                // Ignore stride loaded from file as it may be incorrect, especially if texture is ATI2 and the file was
                // previously saved in OpenIV, DDS documentation recommends recalculating this anyway
                DDSIO.DXTex.ComputePitch(DDSIO.GetDXGIFormat(Format), this.Width, this.Height, out int rowPitch, out int slicePitch, 0);
                this.Stride = (ushort)rowPitch;
                
                // read reference data
                this.Data = reader.ReadBlockAt<TextureData>(this.DataPointer, CalcDataSize()); 
                //this.Format, this.Width, this.Height, this.Levels, this.Stride);

            }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            if (writer.IsGen9)
            {
                writer.Write(0UL);
                writer.WriteBlock(G9_SRV);//SRV embedded at offset 88 (base+8)
                writer.Write(0UL);
            }
            else
            {
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
        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            if (G9_SRV != null)//G9 only
            {
                return new Tuple<long, IResourceBlock>[] {
                    new Tuple<long, IResourceBlock>(88, G9_SRV),
                };
            }
            return base.GetParts();
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
            FullData = reader.ReadBytes((int)parameters[0]);
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
    public enum TextureFormatG9 : uint //actually rage::sga::BufferFormat, something like DXGI_FORMAT, also used in drawables
    {
        UNKNOWN = 0x0,
        R32G32B32A32_TYPELESS = 0x1,
        R32G32B32A32_FLOAT = 0x2,
        R32G32B32A32_UINT = 0x3,
        R32G32B32A32_SINT = 0x4,
        R32G32B32_TYPELESS = 0x5,
        R32G32B32_FLOAT = 0x6,
        R32G32B32_UINT = 0x7,
        R32G32B32_SINT = 0x8,
        R16G16B16A16_TYPELESS = 0x9,
        R16G16B16A16_FLOAT = 0xA,
        R16G16B16A16_UNORM = 0xB,
        R16G16B16A16_UINT = 0xC,
        R16G16B16A16_SNORM = 0xD,
        R16G16B16A16_SINT = 0xE,
        R32G32_TYPELESS = 0xF,
        R32G32_FLOAT = 0x10,
        R32G32_UINT = 0x11,
        R32G32_SINT = 0x12,
        D32_FLOAT_S8X24_UINT = 0x14,
        B10G10R10A2_UNORM = 0x15,
        R10G10B10A2_SNORM = 0x16,
        R10G10B10A2_TYPELESS = 0x17,
        R10G10B10A2_UNORM = 0x18,
        R10G10B10A2_UINT = 0x19,
        R11G11B10_FLOAT = 0x1A,
        R8G8B8A8_TYPELESS = 0x1B,
        R8G8B8A8_UNORM = 0x1C,
        R8G8B8A8_UNORM_SRGB = 0x1D,
        R8G8B8A8_UINT = 0x1E,
        R8G8B8A8_SNORM = 0x1F,
        R8G8B8A8_SINT = 0x20,
        R16G16_TYPELESS = 0x21,
        R16G16_FLOAT = 0x22,
        R16G16_UNORM = 0x23,
        R16G16_UINT = 0x24,
        R16G16_SNORM = 0x25,
        R16G16_SINT = 0x26,
        R32_TYPELESS = 0x27,
        D32_FLOAT = 0x28,
        R32_FLOAT = 0x29,
        R32_UINT = 0x2A,
        R32_SINT = 0x2B,
        R8G8_TYPELESS = 0x30,
        R8G8_UNORM = 0x31,
        R8G8_UINT = 0x32,
        R8G8_SNORM = 0x33,
        R8G8_SINT = 0x34,
        R16_TYPELESS = 0x35,
        R16_FLOAT = 0x36,
        D16_UNORM = 0x37,
        R16_UNORM = 0x38,
        R16_UINT = 0x39,
        R16_SNORM = 0x3A,
        R16_SINT = 0x3B,
        R8_TYPELESS = 0x3C,
        R8_UNORM = 0x3D,
        R8_UINT = 0x3E,
        R8_SNORM = 0x3F,
        R8_SINT = 0x40,
        A8_UNORM = 0x41,
        R9G9B9E5_SHAREDEXP = 0x43,
        BC1_TYPELESS = 0x46,
        BC1_UNORM = 0x47,
        BC1_UNORM_SRGB = 0x48,
        BC2_TYPELESS = 0x49,
        BC2_UNORM = 0x4A,
        BC2_UNORM_SRGB = 0x4B,
        BC3_TYPELESS = 0x4C,
        BC3_UNORM = 0x4D,
        BC3_UNORM_SRGB = 0x4E,
        BC4_TYPELESS = 0x4F,
        BC4_UNORM = 0x50,
        BC4_SNORM = 0x51,
        BC5_TYPELESS = 0x52,
        BC5_UNORM = 0x53,
        BC5_SNORM = 0x54,
        B5G6R5_UNORM = 0x55,
        B5G5R5A1_UNORM = 0x56,
        B8G8R8A8_UNORM = 0x57,
        B8G8R8A8_TYPELESS = 0x5A,
        B8G8R8A8_UNORM_SRGB = 0x5B,
        BC6H_TYPELESS = 0x5E,
        BC6H_UF16 = 0x5F,
        BC6H_SF16 = 0x60,
        BC7_TYPELESS = 0x61,
        BC7_UNORM = 0x62,
        BC7_UNORM_SRGB = 0x63,
        NV12 = 0x67,
        B4G4R4A4_UNORM = 0x73,
        D16_UNORM_S8_UINT = 0x76,
        R16_UNORM_X8_TYPELESS = 0x77,
        X16_TYPELESS_G8_UINT = 0x78,
        ETC1 = 0x79,
        ETC1_SRGB = 0x7A,
        ETC1A = 0x7B,
        ETC1A_SRGB = 0x7C,
        R4G4_UNORM = 0x7F,
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

    public enum TextureDimensionG9 : byte
    {
        Texture2D = 1,
        TextureCube = 2,
        Texture3D = 3,
    }

    public enum TextureTileModeG9 : byte
    {
        Depth = 4,
        Linear = 8,
        Display = 10,
        Standard = 13,
        RenderTarget = 14,
        VolumeStandard = 19,
        VolumeRenderTarget = 20,
        Auto = 255,
    }



    public enum ShaderResourceViewDimensionG9 : ushort //probably actually a uint
    {
        Texture2D = 0x41,//0x401
        Texture2DArray = 0x61,//0x601
        TextureCube = 0x82,//0x802
        Texture3D = 0xa3,//0xa03
        Buffer = 0x14,//0x104
    }
    public class ShaderResourceViewG9 : ResourceSystemBlock
    {
        public override long BlockLength => 32;//64
        public ulong VFT { get; set; } = 0x00000001406b77d8;
        public ulong Unknown_08h { get; set; }
        public ShaderResourceViewDimensionG9 Dimension { get; set; }
        public ushort Unknown_12h { get; set; } = 0xFFFF;
        public uint Unknown_14h { get; set; } = 0xFFFFFFFF;
        public ulong Unknown_18h { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            VFT = reader.ReadUInt64();//runtime ptr?
            Unknown_08h = reader.ReadUInt64();
            Dimension = (ShaderResourceViewDimensionG9)reader.ReadUInt16();//0x41
            Unknown_12h = reader.ReadUInt16();
            Unknown_14h = reader.ReadUInt32();
            Unknown_18h = reader.ReadUInt64();

            switch (VFT)
            {
                case 0x00000001406b77d8:
                case 0x0000000140695e58:
                case 0x000000014070f830:
                case 0x00000001406b9308:
                case 0x0000000140729b58:
                case 0x0000000140703378:
                case 0x0000000140704670:
                case 0x00000001407096f0:
                case 0x00000001406900a8:
                case 0x00000001406b7358:
                    break;
                default:
                    break;
            }

            switch (Dimension)
            {
                case ShaderResourceViewDimensionG9.Texture2D:
                case ShaderResourceViewDimensionG9.Texture2DArray:
                case ShaderResourceViewDimensionG9.Texture3D:
                    break;
                case ShaderResourceViewDimensionG9.Buffer:
                    break;
                default:
                    break;
            }
            if (Unknown_08h != 0)
            { }
            if (Unknown_18h != 0)
            { }
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            writer.Write(VFT);
            writer.Write(Unknown_08h);
            writer.Write((ushort)Dimension);
            writer.Write(Unknown_12h);
            writer.Write(Unknown_14h);
            writer.Write(Unknown_18h);
        }
    }

}
