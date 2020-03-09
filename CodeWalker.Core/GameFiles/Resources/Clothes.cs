using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/*
    Copyright(c) 2017 Neodymium
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


//ruthlessly stolen and brutally mangled


namespace CodeWalker.GameFiles
{

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothDictionary : ResourceFileBase
    {
        // pgBase
        // pgDictionaryBase
        // pgDictionary<characterCloth>
        public override long BlockLength => 0x40;

        // structure data
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h = 1; // 0x0000000000000001
        public ResourceSimpleList64_s<MetaHash> ClothNameHashes { get; set; }
        public ResourcePointerList64<CharacterCloth> Clothes { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.ClothNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.Clothes = reader.ReadBlock<ResourcePointerList64<CharacterCloth>>();

            if (Clothes?.data_items != null)
            {
                for (int i = 0; i < Clothes.data_items.Length; i++)
                {
                    var h = ((ClothNameHashes?.data_items != null) && (i < ClothNameHashes.data_items.Length)) ? ClothNameHashes.data_items[i] : 0;
                    if (Clothes.data_items[i] != null)
                    {
                        Clothes.data_items[i].NameHash = h;
                    }
                }
            }


            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 1)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.WriteBlock(this.ClothNameHashes);
            writer.WriteBlock(this.Clothes);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {

            if (Clothes?.data_items != null)
            {
                foreach (var c in Clothes.data_items)
                {
                    YldXml.OpenTag(sb, indent, "Item");
                    c.WriteXml(sb, indent + 1);
                    YldXml.CloseTag(sb, indent, "Item");
                }
            }

        }
        public void ReadXml(XmlNode node)
        {
            var clothes = new List<CharacterCloth>();
            var clothhashes = new List<MetaHash>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var c = new CharacterCloth();
                    c.ReadXml(inode);
                    clothes.Add(c);
                    clothhashes.Add(c.NameHash);
                }
            }

            ClothNameHashes = new ResourceSimpleList64_s<MetaHash>();
            ClothNameHashes.data_items = clothhashes.ToArray();
            Clothes = new ResourcePointerList64<CharacterCloth>();
            Clothes.data_items = clothes.ToArray();
            //BuildDict();
        }
        public static void WriteXmlNode(ClothDictionary d, StringBuilder sb, int indent, string name = "ClothDictionary")
        {
            if (d == null) return;
            if ((d.Clothes?.data_items == null) || (d.Clothes.data_items.Length == 0))
            {
                YldXml.SelfClosingTag(sb, indent, name);
            }
            else
            {
                YldXml.OpenTag(sb, indent, name);
                d.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, name);
            }
        }
        public static ClothDictionary ReadXmlNode(XmlNode node)
        {
            if (node == null) return null;
            var cd = new ClothDictionary();
            cd.ReadXml(node);
            return cd;
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, ClothNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Clothes)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothController : ResourceSystemBlock
    {
        // clothController
        public override long BlockLength => 0x80;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong BridgeSimGfxPointer { get; set; }
        public ulong MorphControllerPointer { get; set; }
        public ulong VerletCloth1Pointer { get; set; }
        public ulong VerletCloth2Pointer { get; set; }
        public ulong VerletCloth3Pointer { get; set; }
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public uint Type { get; set; }
        public uint Unknown_54h; // 0x00000000
        public PsoChar32 Name { get; set; }
        public float Unknown_78h { get; set; } // 0.0f, 1.0f
        public uint Unknown_7Ch; // 0x00000000

        // reference data
        public ClothBridgeSimGfx BridgeSimGfx { get; set; }
        public MorphController MorphController { get; set; }
        public VerletCloth VerletCloth1 { get; set; }
        public VerletCloth VerletCloth2 { get; set; }
        public VerletCloth VerletCloth3 { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.BridgeSimGfxPointer = reader.ReadUInt64();
            this.MorphControllerPointer = reader.ReadUInt64();
            this.VerletCloth1Pointer = reader.ReadUInt64();
            this.VerletCloth2Pointer = reader.ReadUInt64();
            this.VerletCloth3Pointer = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Type = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Name = reader.ReadStruct<PsoChar32>();
            this.Unknown_78h = reader.ReadSingle();
            this.Unknown_7Ch = reader.ReadUInt32();

            // read reference data
            this.BridgeSimGfx = reader.ReadBlockAt<ClothBridgeSimGfx>(this.BridgeSimGfxPointer);
            this.MorphController = reader.ReadBlockAt<MorphController>(this.MorphControllerPointer);
            this.VerletCloth1 = reader.ReadBlockAt<VerletCloth>(this.VerletCloth1Pointer);
            this.VerletCloth2 = reader.ReadBlockAt<VerletCloth>(this.VerletCloth2Pointer);
            this.VerletCloth3 = reader.ReadBlockAt<VerletCloth>(this.VerletCloth3Pointer);


            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit
            //if (Unknown_54h != 0)
            //{ }//no hit
            //if ((Unknown_78h != 0) && (Unknown_78h != 1.0f))
            //{ }//no hit
            //if (Unknown_7Ch != 0)
            //{ }//no hit


        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.BridgeSimGfxPointer = (ulong)(this.BridgeSimGfx != null ? this.BridgeSimGfx.FilePosition : 0);
            this.MorphControllerPointer = (ulong)(this.MorphController != null ? this.MorphController.FilePosition : 0);
            this.VerletCloth1Pointer = (ulong)(this.VerletCloth1 != null ? this.VerletCloth1.FilePosition : 0);
            this.VerletCloth2Pointer = (ulong)(this.VerletCloth2 != null ? this.VerletCloth2.FilePosition : 0);
            this.VerletCloth3Pointer = (ulong)(this.VerletCloth3 != null ? this.VerletCloth3.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.BridgeSimGfxPointer);
            writer.Write(this.MorphControllerPointer);
            writer.Write(this.VerletCloth1Pointer);
            writer.Write(this.VerletCloth2Pointer);
            writer.Write(this.VerletCloth3Pointer);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Type);
            writer.Write(this.Unknown_54h);
            writer.WriteStruct(this.Name);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
        public virtual void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.StringTag(sb, indent, "Name", YldXml.XmlEscape(Name.ToString()));
            YldXml.ValueTag(sb, indent, "Type", Type.ToString());
            YldXml.ValueTag(sb, indent, "Unknown78", FloatUtil.ToString(Unknown_78h));
            if (BridgeSimGfx != null)
            {
                YldXml.OpenTag(sb, indent, "BridgeSimGfx");
                BridgeSimGfx.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "BridgeSimGfx");
            }
            if (MorphController != null)
            {
                YldXml.OpenTag(sb, indent, "MorphController");
                MorphController.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "MorphController");
            }
            if (VerletCloth1 != null)
            {
                YldXml.OpenTag(sb, indent, "VerletCloth1");
                VerletCloth1.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "VerletCloth1");
            }
            if (VerletCloth2 != null)
            {
                YldXml.OpenTag(sb, indent, "VerletCloth2");
                VerletCloth2.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "VerletCloth2");
            }
            if (VerletCloth3 != null)
            {
                YldXml.OpenTag(sb, indent, "VerletCloth3");
                VerletCloth3.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "VerletCloth3");
            }
        }
        public virtual void ReadXml(XmlNode node)
        {
            Name = new PsoChar32(Xml.GetChildInnerText(node, "Name"));
            Type = Xml.GetChildUIntAttribute(node, "Type", "value");
            Unknown_78h = Xml.GetChildFloatAttribute(node, "Unknown78", "value");
            var bsnode = node.SelectSingleNode("BridgeSimGfx");
            if (bsnode != null)
            {
                BridgeSimGfx = new ClothBridgeSimGfx();
                BridgeSimGfx.ReadXml(bsnode);
            }
            var mcnode = node.SelectSingleNode("MorphController");
            if (mcnode != null)
            {
                MorphController = new MorphController();
                MorphController.ReadXml(mcnode);
            }
            var c1node = node.SelectSingleNode("VerletCloth1");
            if (c1node != null)
            {
                VerletCloth1 = new VerletCloth();
                VerletCloth1.ReadXml(c1node);
            }
            var c2node = node.SelectSingleNode("VerletCloth2");
            if (c2node != null)
            {
                VerletCloth2 = new VerletCloth();
                VerletCloth2.ReadXml(c2node);
            }
            var c3node = node.SelectSingleNode("VerletCloth3");
            if (c3node != null)
            {
                VerletCloth3 = new VerletCloth();
                VerletCloth3.ReadXml(c3node);
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (BridgeSimGfx != null) list.Add(BridgeSimGfx);
            if (MorphController != null) list.Add(MorphController);
            if (VerletCloth1 != null) list.Add(VerletCloth1);
            if (VerletCloth2 != null) list.Add(VerletCloth2);
            if (VerletCloth3 != null) list.Add(VerletCloth3);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothBridgeSimGfx : ResourceSystemBlock
    {
        // pgBase
        // clothBridgeSimGfx
        public override long BlockLength => 0x140;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public uint VertexCount { get; set; }
        public uint Unknown_14h { get; set; }
        public uint Unknown_18h { get; set; }
        public uint Unknown_1Ch; // 0x00000000
        public ResourceSimpleList64_float Unknown_20h { get; set; }
        public ResourceSimpleList64_float Unknown_30h { get; set; }
        public ResourceSimpleList64_float Unknown_40h { get; set; }
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong Unknown_58h; // 0x0000000000000000
        public ResourceSimpleList64_float Unknown_60h { get; set; }
        public ResourceSimpleList64_uint Unknown_70h { get; set; }
        public ResourceSimpleList64_uint Unknown_80h { get; set; }
        public ulong Unknown_90h; // 0x0000000000000000
        public ulong Unknown_98h; // 0x0000000000000000
        public ResourceSimpleList64_float Unknown_A0h { get; set; }
        public ResourceSimpleList64_uint Unknown_B0h { get; set; }
        public ResourceSimpleList64_uint Unknown_C0h { get; set; }
        public ulong Unknown_D0h; // 0x0000000000000000
        public ulong Unknown_D8h; // 0x0000000000000000
        public ResourceSimpleList64_ushort Unknown_E0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_F0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_100h { get; set; }
        public ulong Unknown_110h; // 0x0000000000000000
        public ulong Unknown_118h; // 0x0000000000000000
        public ulong Unknown_120h; // 0x0000000000000000
        public ResourceSimpleList64_uint Unknown_128h { get; set; }
        public ulong Unknown_138h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.VertexCount = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.Unknown_20h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_30h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_40h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_50h = reader.ReadUInt64();
            this.Unknown_58h = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_90h = reader.ReadUInt64();
            this.Unknown_98h = reader.ReadUInt64();
            this.Unknown_A0h = reader.ReadBlock<ResourceSimpleList64_float>();
            this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_C0h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_D0h = reader.ReadUInt64();
            this.Unknown_D8h = reader.ReadUInt64();
            this.Unknown_E0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_F0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_100h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_110h = reader.ReadUInt64();
            this.Unknown_118h = reader.ReadUInt64();
            this.Unknown_120h = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_138h = reader.ReadUInt64();


            //if ((Unknown_20h?.data_items!=null)&&(Unknown_20h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_30h?.data_items!=null)&&(Unknown_30h?.data_items?.Length != VertexCount))
            //{ }//no hit
            //if ((Unknown_40h?.data_items!=null)&&(Unknown_40h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_60h?.data_items!=null)&&(Unknown_60h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_70h?.data_items!=null)&&(Unknown_70h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_80h?.data_items!=null)&&(Unknown_80h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_A0h?.data_items!=null)&&(Unknown_A0h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_B0h?.data_items!=null)&&(Unknown_B0h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_C0h?.data_items!=null)&&(Unknown_C0h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_E0h?.data_items!=null)&&(Unknown_E0h?.data_items?.Length != VertexCount))
            //{ }//no hit
            //if ((Unknown_F0h?.data_items!=null)&&(Unknown_F0h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_100h?.data_items!=null)&&(Unknown_100h?.data_items?.Length != VertexCount))
            //{ }
            //if ((Unknown_128h?.data_items!=null)&&(Unknown_128h?.data_items?.Length != VertexCount))
            //{ }

            //if (Unknown_4h != 1)
            //{ }//no hit
            //switch (Unknown_14h)
            //{
            //    case 0:
            //    case 12://yft
            //    case 20://yft
            //    case 25://yft
            //    case 26://yft
            //    case 36://yft
            //    case 37://yft
            //    case 50://yft
            //    case 66://yft
            //    case 123://yft
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_18h)
            //{
            //    case 0:
            //    case 9://yft
            //    case 13://yft
            //    case 15://yft
            //    case 17://yft
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //if (Unknown_50h != 0)
            //{ }//no hit
            //if (Unknown_58h != 0)
            //{ }//no hit
            //if (Unknown_90h != 0)
            //{ }//no hit
            //if (Unknown_98h != 0)
            //{ }//no hit
            //if (Unknown_D0h != 0)
            //{ }//no hit
            //if (Unknown_D8h != 0)
            //{ }//no hit
            //if (Unknown_110h != 0)
            //{ }//no hit
            //if (Unknown_118h != 0)
            //{ }//no hit
            //if (Unknown_120h != 0)
            //{ }//no hit
            //if (Unknown_138h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.VertexCount);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.Unknown_20h);
            writer.WriteBlock(this.Unknown_30h);
            writer.WriteBlock(this.Unknown_40h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_58h);
            writer.WriteBlock(this.Unknown_60h);
            writer.WriteBlock(this.Unknown_70h);
            writer.WriteBlock(this.Unknown_80h);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_98h);
            writer.WriteBlock(this.Unknown_A0h);
            writer.WriteBlock(this.Unknown_B0h);
            writer.WriteBlock(this.Unknown_C0h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D8h);
            writer.WriteBlock(this.Unknown_E0h);
            writer.WriteBlock(this.Unknown_F0h);
            writer.WriteBlock(this.Unknown_100h);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_120h);
            writer.WriteBlock(this.Unknown_128h);
            writer.Write(this.Unknown_138h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.ValueTag(sb, indent, "VertexCount", VertexCount.ToString());
            YldXml.ValueTag(sb, indent, "Unknown14", Unknown_14h.ToString());
            YldXml.ValueTag(sb, indent, "Unknown18", Unknown_18h.ToString());
            if (Unknown_20h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_20h.data_items, indent, "Unknown20", "", FloatUtil.ToString);
            }
            if (Unknown_30h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_30h.data_items, indent, "Unknown30", "", FloatUtil.ToString);
            }
            if (Unknown_40h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_40h.data_items, indent, "Unknown40", "", FloatUtil.ToString);
            }
            if (Unknown_60h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_60h.data_items, indent, "Unknown60", "", FloatUtil.ToString);
            }
            if (Unknown_70h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_70h.data_items, indent, "Unknown70", "");
            }
            if (Unknown_80h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_80h.data_items, indent, "Unknown80", "");
            }
            if (Unknown_A0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_A0h.data_items, indent, "UnknownA0", "", FloatUtil.ToString);
            }
            if (Unknown_B0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_B0h.data_items, indent, "UnknownB0", "");
            }
            if (Unknown_C0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_C0h.data_items, indent, "UnknownC0", "");
            }
            if (Unknown_E0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_E0h.data_items, indent, "UnknownE0", "");
            }
            if (Unknown_F0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_F0h.data_items, indent, "UnknownF0", "");
            }
            if (Unknown_100h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_100h.data_items, indent, "Unknown100", "");
            }
            if (Unknown_128h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_128h.data_items, indent, "Unknown128", "");
            }
        }
        public void ReadXml(XmlNode node)
        {
            VertexCount = Xml.GetChildUIntAttribute(node, "VertexCount", "value");
            Unknown_14h = Xml.GetChildUIntAttribute(node, "Unknown14", "value");
            Unknown_18h = Xml.GetChildUIntAttribute(node, "Unknown18", "value");
            Unknown_20h = new ResourceSimpleList64_float();
            Unknown_20h.data_items = Xml.GetChildRawFloatArrayNullable(node, "Unknown20");
            Unknown_30h = new ResourceSimpleList64_float();
            Unknown_30h.data_items = Xml.GetChildRawFloatArrayNullable(node, "Unknown30");
            Unknown_40h = new ResourceSimpleList64_float();
            Unknown_40h.data_items = Xml.GetChildRawFloatArrayNullable(node, "Unknown40");
            Unknown_60h = new ResourceSimpleList64_float();
            Unknown_60h.data_items = Xml.GetChildRawFloatArrayNullable(node, "Unknown60");
            Unknown_70h = new ResourceSimpleList64_uint();
            Unknown_70h.data_items = Xml.GetChildRawUintArrayNullable(node, "Unknown70");
            Unknown_80h = new ResourceSimpleList64_uint();
            Unknown_80h.data_items = Xml.GetChildRawUintArrayNullable(node, "Unknown80");
            Unknown_A0h = new ResourceSimpleList64_float();
            Unknown_A0h.data_items = Xml.GetChildRawFloatArrayNullable(node, "UnknownA0");
            Unknown_B0h = new ResourceSimpleList64_uint();
            Unknown_B0h.data_items = Xml.GetChildRawUintArrayNullable(node, "UnknownB0");
            Unknown_C0h = new ResourceSimpleList64_uint();
            Unknown_C0h.data_items = Xml.GetChildRawUintArrayNullable(node, "UnknownC0");
            Unknown_E0h = new ResourceSimpleList64_ushort();
            Unknown_E0h.data_items = Xml.GetChildRawUshortArrayNullable(node, "UnknownE0");
            Unknown_F0h = new ResourceSimpleList64_ushort();
            Unknown_F0h.data_items = Xml.GetChildRawUshortArrayNullable(node, "UnknownF0");
            Unknown_100h = new ResourceSimpleList64_ushort();
            Unknown_100h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown100");
            Unknown_128h = new ResourceSimpleList64_uint();
            Unknown_128h.data_items = Xml.GetChildRawUintArrayNullable(node, "Unknown128");
        }


        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, Unknown_20h),
                new Tuple<long, IResourceBlock>(0x30, Unknown_30h),
                new Tuple<long, IResourceBlock>(0x40, Unknown_40h),
                new Tuple<long, IResourceBlock>(0x60, Unknown_60h),
                new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                new Tuple<long, IResourceBlock>(0xA0, Unknown_A0h),
                new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h),
                new Tuple<long, IResourceBlock>(0xF0, Unknown_F0h),
                new Tuple<long, IResourceBlock>(0x100, Unknown_100h),
                new Tuple<long, IResourceBlock>(0x128, Unknown_128h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ClothInstanceTuning : ResourceSystemBlock
    {
        // pgBase
        // clothInstanceTuning
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public float Unknown_10h { get; set; } // 3.14159274f, 4.7f
        public float Unknown_14h { get; set; } // 0.5235988f, 0.52f
        public ulong Unknown_18h; // 0x0000000000000000
        public float Unknown_20h { get; set; } // 0.0f, 4.0f
        public float Unknown_24h { get; set; } // 0.0f, 2.0f
        public float Unknown_28h { get; set; } // 0.0f, 2.0f, 8.0f
        public float Unknown_2Ch { get; set; } // 0.0f, 4.0f
        public uint Flags { get; set; } //flags
        public float Unknown_34h { get; set; } // -1.0f, 2.0f, 0.35f, 1.5f, 0.5f, 0.85f
        public float Unknown_38h { get; set; } // 0.0f, 0.1f, 0.0025f, 0.002f, 0.0015f, 0.0005f, 0.0001f
        public uint Unknown_3Ch { get; set; } // 0, 0x00204802   -wtf?

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadSingle();
            this.Unknown_14h = reader.ReadSingle();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadSingle();
            this.Unknown_24h = reader.ReadSingle();
            this.Unknown_28h = reader.ReadSingle();
            this.Unknown_2Ch = reader.ReadSingle();
            this.Flags = reader.ReadUInt32();
            this.Unknown_34h = reader.ReadSingle();
            this.Unknown_38h = reader.ReadSingle();
            this.Unknown_3Ch = reader.ReadUInt32();

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //switch (Unknown_10h)
            //{
            //    case 3.14159274f:
            //    case 4.7f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_14h)
            //{
            //    case 0.5235988f:
            //    case 0.52f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_20h)
            //{
            //    case 0:
            //    case 4.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_24h)
            //{
            //    case 0:
            //    case 2.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_28h)
            //{
            //    case 0:
            //    case 2.0f:
            //    case 8.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_2Ch)
            //{
            //    case 0:
            //    case 4.0f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_30h)//looks like flags
            //{
            //    case 0://rareish
            //    case 1:
            //    case 4:
            //    case 8:
            //    case 16:
            //    case 24:
            //    case 40:
            //    case 128:
            //    case 130:
            //    case 256:
            //    case 512:
            //    case 528:
            //    case 1280:
            //    case 1792:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_34h)
            //{
            //    case -1.0f:
            //    case 2.0f:
            //    case 0.35f:
            //    case 1.5f:
            //    case 0.5f:
            //    case 0.85f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_38h)
            //{
            //    case 0:
            //    case 0.1f:
            //    case 0.0025f:
            //    case 0.002f:
            //    case 0.0015f:
            //    case 0.0005f:
            //    case 0.0001f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_3Ch)
            //{
            //    case 0:
            //    case 0x00204802: //??? 
            //        break;
            //    default:
            //        break;
            //}


        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_24h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_2Ch);
            writer.Write(this.Flags);
            writer.Write(this.Unknown_34h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_3Ch);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.ValueTag(sb, indent, "Unknown10", FloatUtil.ToString(Unknown_10h));
            YldXml.ValueTag(sb, indent, "Unknown14", FloatUtil.ToString(Unknown_14h));
            YldXml.ValueTag(sb, indent, "Unknown20", FloatUtil.ToString(Unknown_20h));
            YldXml.ValueTag(sb, indent, "Unknown24", FloatUtil.ToString(Unknown_24h));
            YldXml.ValueTag(sb, indent, "Unknown28", FloatUtil.ToString(Unknown_28h));
            YldXml.ValueTag(sb, indent, "Unknown2C", FloatUtil.ToString(Unknown_2Ch));
            YldXml.ValueTag(sb, indent, "Flags", Flags.ToString());
            YldXml.ValueTag(sb, indent, "Unknown34", FloatUtil.ToString(Unknown_34h));
            YldXml.ValueTag(sb, indent, "Unknown38", FloatUtil.ToString(Unknown_38h));
            YldXml.ValueTag(sb, indent, "Unknown3C", Unknown_3Ch.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Unknown_10h = Xml.GetChildFloatAttribute(node, "Unknown10", "value");
            Unknown_14h = Xml.GetChildFloatAttribute(node, "Unknown14", "value");
            Unknown_20h = Xml.GetChildFloatAttribute(node, "Unknown20", "value");
            Unknown_24h = Xml.GetChildFloatAttribute(node, "Unknown24", "value");
            Unknown_28h = Xml.GetChildFloatAttribute(node, "Unknown28", "value");
            Unknown_2Ch = Xml.GetChildFloatAttribute(node, "Unknown2C", "value");
            Flags = Xml.GetChildUIntAttribute(node, "Flags", "value");
            Unknown_34h = Xml.GetChildFloatAttribute(node, "Unknown34", "value");
            Unknown_38h = Xml.GetChildFloatAttribute(node, "Unknown38", "value");
            Unknown_3Ch = Xml.GetChildUIntAttribute(node, "Unknown3C", "value");
        }

    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class VerletCloth : ResourceSystemBlock
    {
        // pgBase
        // phVerletCloth
        public override long BlockLength => 0x180;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong BoundPointer { get; set; }
        public ulong Unknown_20h; // 0x00000000
        public ulong Unknown_28h; // 0x00000000
        public Vector3 BBMin { get; set; }
        public uint Unknown_3Ch { get; set; }    // 0x00000000, 0x7f7fffff (yft only)
        public Vector3 BBMax { get; set; }
        public uint Unknown_4Ch { get; set; }    // 0x00000000, 0xff7fffff (yft only)
        public float Unknown_50h { get; set; } // 3.5424633f, 3.54442024f, 3.54442787f, 3.54441643f
        public uint Unknown_54h = 1; // 0x00000001
        public ulong Unknown_58h; // 0x0000000000000000
        public ulong Unknown_60h; // 0x0000000000000000
        public ulong Unknown_68h; // 0x0000000000000000
        public ResourceSimpleList64_s<Vector4> Vertices2 { get; set; } //vertex infos? original positions..?
        public ResourceSimpleList64_s<Vector4> Vertices { get; set; } //vertex positions  (in bind pose?)
        public ulong Unknown_90h; // 0x0000000000000000
        public ulong Unknown_98h; // 0x0000000000000000
        public ulong Unknown_A0h; // 0x0000000000000000
        public float Unknown_A8h { get; set; }      //9999, 100
        public float Unknown_ACh { get; set; }      //9999, 0
        public ulong Unknown_B0h; // 0x0000000000000000
        public ulong Unknown_B8h; // 0x0000000000000000
        public ulong Unknown_C0h; // 0x0000000000000000
        public ulong Unknown_C8h; // 0x0000000000000000
        public ulong Unknown_D0h; // 0x0000000000000000
        public ulong Unknown_D8h; // 0x0000000000000000
        public ulong Unknown_E0h; // 0x0000000000000000
        public uint Unknown_E8h { get; set; }       // ?
        public uint ConstraintCount { get; set; }       //constraints count 
        public uint VertexCount { get; set; }       //vertices count
        public uint Unknown_F4h; // 0x00000000
        public ushort Unknown_F8h = 3;       // 3 
        public ushort Unknown_FAh { get; set; } // 0, 2 (yft only)
        public uint Unknown_FCh; // 0x00000000
        public ResourceSimpleList64_s<Unknown_C_004> Constraints2 { get; set; }
        public ResourceSimpleList64_s<Unknown_C_004> Constraints { get; set; }
        public ulong Unknown_120h; // 0x0000000000000000
        public ulong Unknown_128h; // 0x0000000000000000
        public ulong BehaviorPointer { get; set; }
        public ulong Unknown_138h = 0x100000; // 0x0000000000100000
        public ulong Unknown_140h_Pointer { get; set; }
        public ushort Unknown_148h { get; set; }    // ?  min:1, max:31
        public ushort VertexCount2 { get; set; }    //also vertices count
        public uint Unknown_14Ch; // 0x00000000
        public ulong Unknown_150h; // 0x0000000000000000
        public float Unknown_158h { get; set; }
        public uint Unknown_15Ch; // 0x00000000
        public ulong Unknown_160h; // 0x0000000000000000
        public ulong Unknown_168h; // 0x0000000000000000
        public ulong Unknown_170h; // 0x0000000000000000
        public ulong Unknown_178h; // 0x0000000000000000

        // reference data
        public Bounds Bound { get; set; }
        public EnvClothVerletBehavior Behavior { get; set; }
        public Unknown_C_007 Unknown_140h_Data { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.BBMin = reader.ReadVector3();
            this.Unknown_3Ch = reader.ReadUInt32();
            this.BBMax = reader.ReadVector3();
            this.Unknown_4Ch = reader.ReadUInt32();
            this.Unknown_50h = reader.ReadSingle();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt64();
            this.Unknown_60h = reader.ReadUInt64();
            this.Unknown_68h = reader.ReadUInt64();
            this.Vertices2 = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Vertices = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_90h = reader.ReadUInt64();
            this.Unknown_98h = reader.ReadUInt64();
            this.Unknown_A0h = reader.ReadUInt64();
            this.Unknown_A8h = reader.ReadSingle();
            this.Unknown_ACh = reader.ReadSingle();
            this.Unknown_B0h = reader.ReadUInt64();
            this.Unknown_B8h = reader.ReadUInt64();
            this.Unknown_C0h = reader.ReadUInt64();
            this.Unknown_C8h = reader.ReadUInt64();
            this.Unknown_D0h = reader.ReadUInt64();
            this.Unknown_D8h = reader.ReadUInt64();
            this.Unknown_E0h = reader.ReadUInt64();
            this.Unknown_E8h = reader.ReadUInt32();
            this.ConstraintCount = reader.ReadUInt32();
            this.VertexCount = reader.ReadUInt32();
            this.Unknown_F4h = reader.ReadUInt32();
            this.Unknown_F8h = reader.ReadUInt16();
            this.Unknown_FAh = reader.ReadUInt16();
            this.Unknown_FCh = reader.ReadUInt32();
            this.Constraints2 = reader.ReadBlock<ResourceSimpleList64_s<Unknown_C_004>>();
            this.Constraints = reader.ReadBlock<ResourceSimpleList64_s<Unknown_C_004>>();
            this.Unknown_120h = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt64();
            this.BehaviorPointer = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt64();
            this.Unknown_140h_Pointer = reader.ReadUInt64();
            this.Unknown_148h = reader.ReadUInt16();
            this.VertexCount2 = reader.ReadUInt16();
            this.Unknown_14Ch = reader.ReadUInt32();
            this.Unknown_150h = reader.ReadUInt64();
            this.Unknown_158h = reader.ReadSingle();
            this.Unknown_15Ch = reader.ReadUInt32();
            this.Unknown_160h = reader.ReadUInt64();
            this.Unknown_168h = reader.ReadUInt64();
            this.Unknown_170h = reader.ReadUInt64();
            this.Unknown_178h = reader.ReadUInt64();

            // read reference data
            this.Bound = reader.ReadBlockAt<Bounds>(this.BoundPointer);
            this.Behavior = reader.ReadBlockAt<EnvClothVerletBehavior>(this.BehaviorPointer);
            this.Unknown_140h_Data = reader.ReadBlockAt<Unknown_C_007>(this.Unknown_140h_Pointer);

            if (Bound != null)
            {
                Bound.Owner = this;
            }

            //if (BBMin.X > BBMax.X)
            //{ }//no hit
            //if (BBMin.Y > BBMax.Y)
            //{ }//no hit
            //if (BBMin.Z > BBMax.Z)
            //{ }//no hit
            //if ((Vertices2?.data_items?.Length ?? (int)VertexCount) != VertexCount)
            //{ }//no hit
            //if ((Vertices?.data_items?.Length ?? 0) != VertexCount)
            //{ }//no hit
            //if ((Constraints2?.data_items?.Length ?? 0) != 0)//seems to be no field matching this
            //{ }
            //if ((Constraints?.data_items?.Length ?? 0) != ConstraintCount)
            //{ }//no hit
            //if (VertexCount2 != VertexCount)
            //{ }//no hit
            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //switch (Unknown_3Ch)
            //{
            //    case 0:
            //    case 0x7f7fffff: //yft only
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_4Ch)
            //{
            //    case 0:
            //    case 0xff7fffff: //yft only
            //        break;
            //    default:
            //        break;//no hit
            //}
            //switch (Unknown_50h)
            //{
            //    case 3.5424633f:
            //    case 3.54442024f:
            //    case 3.54442787f:
            //    case 3.54441643f:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_54h != 1)
            //{ }//no hit
            //if (Unknown_58h != 0)
            //{ }//no hit
            //if (Unknown_60h != 0)
            //{ }//no hit
            //if (Unknown_68h != 0)
            //{ }//no hit
            //if (Unknown_90h != 0)
            //{ }//no hit
            //if (Unknown_98h != 0)
            //{ }//no hit
            //if (Unknown_A0h != 0)
            //{ }//no hit
            //if (Unknown_B0h != 0)
            //{ }//no hit
            //if (Unknown_B8h != 0)
            //{ }//no hit
            //if (Unknown_C0h != 0)
            //{ }//no hit
            //if (Unknown_C8h != 0)
            //{ }//no hit
            //if (Unknown_D0h != 0)
            //{ }//no hit
            //if (Unknown_D8h != 0)
            //{ }//no hit
            //if (Unknown_E0h != 0)
            //{ }//no hit
            //switch (Unknown_E8h)
            //{
            //    case 2:
            //    case 4:
            //    case 8:
            //    case 10:
            //    case 16:
            //    case 18:
            //    case 19:
            //    case 20:
            //    case 21:
            //    case 23:
            //    case 24:
            //    case 25:
            //    case 26:
            //    case 27:
            //    case 28:
            //    case 29:
            //    case 30:
            //    case 31:
            //    case 33:
            //    case 34:
            //    case 35:
            //    case 37:
            //    case 38:
            //    case 39:
            //    case 41:
            //    case 45://yft
            //    case 50:
            //    case 54://yft
            //    case 56:
            //    case 62:
            //    case 64://yft
            //    case 73:
            //    case 75:
            //    case 78://yft
            //    case 94://yft
            //    case 103:
            //    case 116://yft
            //    case 128://yft
            //    case 132://yft
            //    case 142://yft
            //    case 159://yft
            //    case 173://yft
            //    case 175://yft
            //    case 176://yft
            //    case 177://yft
            //    case 202://yft
            //    case 230://yft
            //    case 233://yft
            //    case 287://yft
            //    case 389://yft
            //    case 456://yft
            //    case 566://yft
            //        break;
            //    default:
            //        break;//+more in yfts
            //}
            //if (Unknown_F4h != 0)
            //{ }//no hit
            //if (Unknown_F8h != 3)
            //{ }//no hit
            //switch (Unknown_FAh)
            //{
            //    case 0:
            //    case 2: //yft
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_FCh != 0)
            //{ }//no hit
            //if (Unknown_120h != 0)
            //{ }//no hit
            //if (Unknown_128h != 0)
            //{ }//no hit
            //if (Unknown_138h != 0x100000)
            //{ }//no hit
            //switch (Unknown_148h)
            //{
            //    case 1:
            //    case 2:
            //    case 3:
            //    case 4:
            //    case 5:
            //    case 6:
            //    case 7:
            //    case 8://ped cloth only up to 8
            //    case 9:
            //    case 10:
            //    case 11:
            //    case 12:
            //    case 13:
            //    case 14:
            //    case 15:
            //    case 16:
            //    case 17:
            //    case 18:
            //    case 19:
            //    case 20:
            //    case 21:
            //    case 22:
            //    case 23:
            //    case 24:
            //    case 25:
            //    case 26:
            //    case 27:
            //    case 28:
            //    case 30:
            //    case 31:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_14Ch != 0)
            //{ }//no hit
            //if (Unknown_150h != 0)
            //{ }//no hit
            //if (Unknown_15Ch != 0)
            //{ }//no hit
            //if (Unknown_160h != 0)
            //{ }//no hit
            //if (Unknown_168h != 0)
            //{ }//no hit
            //if (Unknown_170h != 0)
            //{ }//no hit
            //if (Unknown_178h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);
            this.BehaviorPointer = (ulong)(this.Behavior != null ? this.Behavior.FilePosition : 0);
            this.Unknown_140h_Pointer = (ulong)(this.Unknown_140h_Data != null ? this.Unknown_140h_Data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.BoundPointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.BBMin);
            writer.Write(this.Unknown_3Ch);
            writer.Write(this.BBMax);
            writer.Write(this.Unknown_4Ch);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_68h);
            writer.WriteBlock(this.Vertices2);
            writer.WriteBlock(this.Vertices);
            writer.Write(this.Unknown_90h);
            writer.Write(this.Unknown_98h);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_ACh);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_B8h);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C8h);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_E0h);
            writer.Write(this.Unknown_E8h);
            writer.Write(this.ConstraintCount);
            writer.Write(this.VertexCount);
            writer.Write(this.Unknown_F4h);
            writer.Write(this.Unknown_F8h);
            writer.Write(this.Unknown_FAh);
            writer.Write(this.Unknown_FCh);
            writer.WriteBlock(this.Constraints2);
            writer.WriteBlock(this.Constraints);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.BehaviorPointer);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_140h_Pointer);
            writer.Write(this.Unknown_148h);
            writer.Write(this.VertexCount2);
            writer.Write(this.Unknown_14Ch);
            writer.Write(this.Unknown_150h);
            writer.Write(this.Unknown_158h);
            writer.Write(this.Unknown_15Ch);
            writer.Write(this.Unknown_160h);
            writer.Write(this.Unknown_168h);
            writer.Write(this.Unknown_170h);
            writer.Write(this.Unknown_178h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.SelfClosingTag(sb, indent, "BBMin " + FloatUtil.GetVector3XmlString(BBMin));
            YldXml.SelfClosingTag(sb, indent, "BBMax " + FloatUtil.GetVector3XmlString(BBMax));
            YldXml.ValueTag(sb, indent, "Unknown3C", Unknown_3Ch.ToString());
            YldXml.ValueTag(sb, indent, "Unknown4C", Unknown_4Ch.ToString());
            YldXml.ValueTag(sb, indent, "Unknown50", FloatUtil.ToString(Unknown_50h));
            YldXml.ValueTag(sb, indent, "UnknownA8", FloatUtil.ToString(Unknown_A8h));
            YldXml.ValueTag(sb, indent, "UnknownAC", FloatUtil.ToString(Unknown_ACh));
            YldXml.ValueTag(sb, indent, "UnknownE8", Unknown_E8h.ToString());
            YldXml.ValueTag(sb, indent, "UnknownFA", Unknown_FAh.ToString());
            YldXml.ValueTag(sb, indent, "Unknown148", Unknown_148h.ToString());
            YldXml.ValueTag(sb, indent, "Unknown158", FloatUtil.ToString(Unknown_158h));
            if (Unknown_140h_Data != null)
            {
                YldXml.SelfClosingTag(sb, indent, "Unknown140");
            }
            if (Behavior != null)
            {
                YldXml.SelfClosingTag(sb, indent, "Behaviour");
            }
            if (Vertices?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Vertices.data_items, indent, "Vertices", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Vertices2?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Vertices2.data_items, indent, "Vertices2", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Constraints?.data_items != null)
            {
                YldXml.WriteItemArray(sb, Constraints.data_items, indent, "Constraints");
            }
            if (Constraints2?.data_items != null)
            {
                YldXml.WriteItemArray(sb, Constraints2.data_items, indent, "Constraints2");
            }
            if (Bound != null)
            {
                Bounds.WriteXmlNode(Bound, sb, indent);
            }
        }
        public void ReadXml(XmlNode node)
        {
            BBMin = Xml.GetChildVector3Attributes(node, "BBMin");
            BBMax = Xml.GetChildVector3Attributes(node, "BBMax");
            Unknown_3Ch = Xml.GetChildUIntAttribute(node, "Unknown3C", "value");
            Unknown_4Ch = Xml.GetChildUIntAttribute(node, "Unknown4C", "value");
            Unknown_50h = Xml.GetChildFloatAttribute(node, "Unknown50", "value");
            Unknown_A8h = Xml.GetChildFloatAttribute(node, "UnknownA8", "value");
            Unknown_ACh = Xml.GetChildFloatAttribute(node, "UnknownAC", "value");
            Unknown_E8h = Xml.GetChildUIntAttribute(node, "UnknownE8", "value");
            Unknown_FAh = (ushort)Xml.GetChildUIntAttribute(node, "UnknownFA", "value");
            Unknown_148h = (ushort)Xml.GetChildUIntAttribute(node, "Unknown148", "value");
            Unknown_158h = Xml.GetChildFloatAttribute(node, "Unknown158", "value");
            Vertices = new ResourceSimpleList64_s<Vector4>();
            Vertices.data_items = Xml.GetChildRawVector4ArrayNullable(node, "Vertices");
            Vertices2 = new ResourceSimpleList64_s<Vector4>();
            Vertices2.data_items = Xml.GetChildRawVector4ArrayNullable(node, "Vertices2");
            Constraints = new ResourceSimpleList64_s<Unknown_C_004>();
            Constraints.data_items = XmlMeta.ReadItemArray<Unknown_C_004>(node, "Constraints");
            Constraints2 = new ResourceSimpleList64_s<Unknown_C_004>();
            Constraints2.data_items = XmlMeta.ReadItemArray<Unknown_C_004>(node, "Constraints2");
            var unode = node.SelectSingleNode("Unknown140");
            if (unode != null)
            {
                Unknown_140h_Data = new Unknown_C_007();
            }
            var bhnode = node.SelectSingleNode("Behaviour");
            if (bhnode != null)
            {
                Behavior = new EnvClothVerletBehavior();
            }
            var bnode = node.SelectSingleNode("Bounds");
            if (bnode != null)
            {
                Bound = Bounds.ReadXmlNode(bnode, this);
            }

            VertexCount = VertexCount2 = (ushort)(Vertices?.data_items?.Length ?? 0);
            ConstraintCount = (ushort)(Constraints?.data_items.Length ?? 0);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Bound != null) list.Add(Bound);
            if (Behavior != null) list.Add(Behavior);
            if (Unknown_140h_Data != null) list.Add(Unknown_140h_Data);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x70, Vertices2),
                new Tuple<long, IResourceBlock>(0x80, Vertices),
                new Tuple<long, IResourceBlock>(0x100, Constraints2),
                new Tuple<long, IResourceBlock>(0x110, Constraints)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class EnvClothVerletBehavior : ResourceSystemBlock
    {
        // datBase
        // phInstBehavior
        // phClothVerletBehavior
        // phEnvClothVerletBehavior
        public override long BlockLength => 0x40;

        // structure data
        public ulong Unknown_0h; // 0x0000000000000000
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();


            //if (Unknown_0h != 0)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class EnvironmentCloth : ResourceSystemBlock
    {
        // pgBase
        // clothBase (TODO)
        // environmentCloth
        public override long BlockLength => 0x80;

        // structure data
        public uint VFT { get; set; } = 1080059352;
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong InstanceTuningPointer { get; set; }
        public ulong DrawablePointer { get; set; }
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong ControllerPointer { get; set; }
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public ulong Unknown_50h; // 0x0000000000000000
        public ulong Unknown_58h; // 0x0000000000000000
        public ulong UnknownPointer { get; set; }
        public ushort UnknownCount1 { get; set; }
        public ushort UnknownCount2 { get; set; }
        public uint Unknown_6Ch; // 0x00000000
        public ulong Unknown_70h; // 0x0000000000000000
        public uint Unknown_78h { get; set; } // 0, 16, 48
        public uint Unknown_7Ch; // 0x00000000

        // reference data
        public ClothInstanceTuning InstanceTuning { get; set; }
        public FragDrawable Drawable { get; set; }
        public ClothController Controller { get; set; }
        public uint[] UnknownData { get; set; }

        private ResourceSystemStructBlock<uint> UnknownDataBlock = null;

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.InstanceTuningPointer = reader.ReadUInt64();
            this.DrawablePointer = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.ControllerPointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadUInt64();
            this.Unknown_58h = reader.ReadUInt64();
            this.UnknownPointer = reader.ReadUInt64();
            this.UnknownCount1 = reader.ReadUInt16();
            this.UnknownCount2 = reader.ReadUInt16();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt64();
            this.Unknown_78h = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();

            // read reference data
            this.InstanceTuning = reader.ReadBlockAt<ClothInstanceTuning>(this.InstanceTuningPointer);
            this.Drawable = reader.ReadBlockAt<FragDrawable>(this.DrawablePointer);
            this.Controller = reader.ReadBlockAt<ClothController>(this.ControllerPointer);
            this.UnknownData = reader.ReadUintsAt(this.UnknownPointer, this.UnknownCount1);

            if (this.Drawable != null)
            {
                this.Drawable.OwnerCloth = this;
            }

            //if (UnknownCount1 != UnknownCount2)
            //{ }//no hit
            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit
            //if (Unknown_50h != 0)
            //{ }//no hit
            //if (Unknown_58h != 0)
            //{ }//no hit
            //if (Unknown_6Ch != 0)
            //{ }//no hit
            //if (Unknown_70h != 0)
            //{ }//no hit
            //if (Unknown_7Ch != 0)
            //{ }//no hit
            //switch (Unknown_78h)
            //{
            //    case 0:
            //    case 16:
            //    case 48:
            //        break;
            //    default:
            //        break;//no hit
            //}

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.InstanceTuningPointer = (ulong)(this.InstanceTuning != null ? this.InstanceTuning.FilePosition : 0);
            this.DrawablePointer = (ulong)(this.Drawable != null ? this.Drawable.FilePosition : 0);
            this.ControllerPointer = (ulong)(this.Controller != null ? this.Controller.FilePosition : 0);
            this.UnknownPointer = (ulong)(this.UnknownDataBlock != null ? this.UnknownDataBlock.FilePosition : 0);
            this.UnknownCount1 = (ushort)(this.UnknownDataBlock != null ? this.UnknownDataBlock.ItemCount : 0);
            this.UnknownCount2 = this.UnknownCount1;

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.InstanceTuningPointer);
            writer.Write(this.DrawablePointer);
            writer.Write(this.Unknown_20h);
            writer.Write(this.ControllerPointer);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.UnknownPointer);
            writer.Write(this.UnknownCount1);
            writer.Write(this.UnknownCount2);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_78h);
            writer.Write(this.Unknown_7Ch);
        }
        public void WriteXml(StringBuilder sb, int indent, string ddsfolder)
        {
            YldXml.ValueTag(sb, indent, "Unknown78", Unknown_78h.ToString());
            if (UnknownData != null)
            {
                YldXml.WriteRawArray(sb, UnknownData, indent, "UnknownData", "");
            }
            if (Controller != null)
            {
                YldXml.OpenTag(sb, indent, "Controller");
                Controller.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "Controller");
            }
            if (InstanceTuning != null)
            {
                YldXml.OpenTag(sb, indent, "InstanceTuning");
                InstanceTuning.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "InstanceTuning");
            }
            if (Drawable != null)
            {
                FragDrawable.WriteXmlNode(Drawable, sb, indent, ddsfolder, "Drawable");
            }
        }
        public void ReadXml(XmlNode node, string ddsfolder)
        {
            Unknown_78h = Xml.GetChildUIntAttribute(node, "Unknown78", "value");
            UnknownData = Xml.GetChildRawUintArrayNullable(node, "UnknownData");
            var cnode = node.SelectSingleNode("Controller");
            if (cnode != null)
            {
                Controller = new ClothController();
                Controller.ReadXml(cnode);
            }
            var tnode = node.SelectSingleNode("InstanceTuning");
            if (tnode != null)
            {
                InstanceTuning = new ClothInstanceTuning();
                InstanceTuning.ReadXml(tnode);
            }
            var dnode = node.SelectSingleNode("Drawable");
            if (dnode != null)
            {
                Drawable = FragDrawable.ReadXmlNode(dnode, ddsfolder);
            }

            UnknownCount1 = UnknownCount2 = (ushort)(UnknownData?.Length ?? 0);
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (InstanceTuning != null) list.Add(InstanceTuning);
            if (Drawable != null) list.Add(Drawable);
            if (Controller != null) list.Add(Controller);
            if (UnknownData != null)
            {
                UnknownDataBlock = new ResourceSystemStructBlock<uint>(UnknownData);
                list.Add(UnknownDataBlock);
            }
            return list.ToArray();
        }

    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class CharacterCloth : ResourceSystemBlock
    {
        // pgBase
        // clothBase (TODO)
        // characterCloth
        public override long BlockLength => 0xD0;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ResourceSimpleList64_s<Vector4> Unknown_10h { get; set; }
        public ulong ControllerPointer { get; set; }
        public ulong BoundPointer { get; set; }
        public ResourceSimpleList64_uint Unknown_30h { get; set; } //bone ids - maps to items in Bound.Children
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public Matrix Transform { get; set; }
        public ResourceSimpleList64_uint Unknown_90h { get; set; }
        public ulong Unknown_A0h; // 0x0000000000000000
        public ulong Unknown_A8h; // 0x0000000000000000
        public ulong Unknown_B0h; // 0x0000000000000000
        public ulong Unknown_B8h; // 0x0000000000000000
        public ulong Unknown_C0h = 1; // 0x0000000000000001
        public ulong Unknown_C8h; // 0x0000000000000000

        // reference data
        public CharacterClothController Controller { get; set; }
        public Bounds Bound { get; set; }

        public MetaHash NameHash { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.ControllerPointer = reader.ReadUInt64();
            this.BoundPointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Transform = reader.ReadMatrix();
            this.Unknown_90h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.Unknown_A0h = reader.ReadUInt64();
            this.Unknown_A8h = reader.ReadUInt64();
            this.Unknown_B0h = reader.ReadUInt64();
            this.Unknown_B8h = reader.ReadUInt64();
            this.Unknown_C0h = reader.ReadUInt64();
            this.Unknown_C8h = reader.ReadUInt64();

            // read reference data
            this.Controller = reader.ReadBlockAt<CharacterClothController>(this.ControllerPointer);
            this.Bound = reader.ReadBlockAt<Bounds>(this.BoundPointer);

            if (Bound != null)
            {
                Bound.Owner = this;
            }

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit
            //if (Unknown_A0h != 0)
            //{ }//no hit
            //if (Unknown_A8h != 0)
            //{ }//no hit
            //if (Unknown_B0h != 0)
            //{ }//no hit
            //if (Unknown_B8h != 0)
            //{ }//no hit
            //if (Unknown_C0h != 1)
            //{ }//no hit
            //if (Unknown_C8h != 0)
            //{ }//no hit


        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.ControllerPointer = (ulong)(this.Controller != null ? this.Controller.FilePosition : 0);
            this.BoundPointer = (ulong)(this.Bound != null ? this.Bound.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.WriteBlock(this.Unknown_10h);
            writer.Write(this.ControllerPointer);
            writer.Write(this.BoundPointer);
            writer.WriteBlock(this.Unknown_30h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.Write(this.Transform);
            writer.WriteBlock(this.Unknown_90h);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A8h);
            writer.Write(this.Unknown_B0h);
            writer.Write(this.Unknown_B8h);
            writer.Write(this.Unknown_C0h);
            writer.Write(this.Unknown_C8h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.StringTag(sb, indent, "Name", YldXml.HashString(NameHash));
            YldXml.WriteRawArray(sb, Transform.ToArray(), indent, "Transform", "", FloatUtil.ToString, 4);
            if (Unknown_10h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_10h.data_items, indent, "Unknown10", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Unknown_30h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_30h.data_items, indent, "Unknown30", "");
            }
            if (Unknown_90h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_90h.data_items, indent, "Unknown90", "");
            }
            if (Controller != null)
            {
                YldXml.OpenTag(sb, indent, "Controller");
                Controller.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "Controller");
            }
            if (Bound != null)
            {
                Bounds.WriteXmlNode(Bound, sb, indent);
            }
        }
        public void ReadXml(XmlNode node)
        {
            NameHash = XmlRel.GetHash(Xml.GetChildInnerText(node, "Name"));
            Transform = Xml.GetChildMatrix(node, "Transform");
            Unknown_10h = new ResourceSimpleList64_s<Vector4>();
            Unknown_10h.data_items = Xml.GetChildRawVector4ArrayNullable(node, "Unknown10");
            Unknown_30h = new ResourceSimpleList64_uint();
            Unknown_30h.data_items = Xml.GetChildRawUintArrayNullable(node, "Unknown30");
            Unknown_90h = new ResourceSimpleList64_uint();
            Unknown_90h.data_items = Xml.GetChildRawUintArrayNullable(node, "Unknown90");
            var cnode = node.SelectSingleNode("Controller");
            if (cnode != null)
            {
                Controller = new CharacterClothController();
                Controller.ReadXml(cnode);
            }
            var bnode = node.SelectSingleNode("Bounds");
            if (bnode != null)
            {
                Bound = Bounds.ReadXmlNode(bnode, this);
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Controller != null) list.Add(Controller);
            if (Bound != null) list.Add(Bound);
            return list.ToArray();
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x10, Unknown_10h),
                new Tuple<long, IResourceBlock>(0x30, Unknown_30h),
                new Tuple<long, IResourceBlock>(0x90, Unknown_90h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class CharacterClothController : ClothController
    {
        // characterClothController
        public override long BlockLength => 0xF0;

        // structure data      
        public ResourceSimpleList64_ushort Indices { get; set; }
        public ResourceSimpleList64_s<Vector4> Vertices { get; set; }
        public float Unknown_A0h { get; set; } = 0.04f; // 0x3D23D70A = 0.04f
        public uint Unknown_A4h; // 0x00000000
        public ulong Unknown_A8h; // 0x0000000000000000
        public ResourceSimpleList64_uint Unknown_B0h { get; set; }// related to BridgeSimGfx.Unknown_E0h? same count as boneids...  anchor verts..?
        public ResourceSimpleList64_s<CharClothBoneWeightsInds> BoneWeightsInds { get; set; }//bone weights / indices
        public ulong Unknown_D0h; // 0x0000000000000000
        public uint Unknown_D8h; // 0x00000000
        public float Unknown_DCh { get; set; } = 1.0f; // 0x3F800000 = 1.0f
        public ResourceSimpleList64_uint BoneIds { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data         
            this.Indices = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Vertices = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_A0h = reader.ReadSingle();
            this.Unknown_A4h = reader.ReadUInt32();
            this.Unknown_A8h = reader.ReadUInt64();
            this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64_uint>();
            this.BoneWeightsInds = reader.ReadBlock<ResourceSimpleList64_s<CharClothBoneWeightsInds>>();
            this.Unknown_D0h = reader.ReadUInt64();
            this.Unknown_D8h = reader.ReadUInt32();
            this.Unknown_DCh = reader.ReadSingle();
            this.BoneIds = reader.ReadBlock<ResourceSimpleList64_uint>();



            //if (Unknown_B0h?.data_items?.Length != BoneIds?.data_items?.Length)
            //{ }//no hit

            //if (BoneWeightsInds?.data_items?.Length != Vertices?.data_items?.Length)
            //{ }//2 hits here, where BoneWeightsInds only 1 less than vertex count

            //if (Unknown_A0h != 0.04f)
            //{ }//no hit
            //if (Unknown_A4h != 0)
            //{ }//no hit
            //if (Unknown_A8h != 0)
            //{ }//no hit
            //if (Unknown_D0h != 0)
            //{ }//no hit
            //if (Unknown_D8h != 0)
            //{ }//no hit
            //if (Unknown_DCh != 1.0f)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data           
            writer.WriteBlock(this.Indices);
            writer.WriteBlock(this.Vertices);
            writer.Write(this.Unknown_A0h);
            writer.Write(this.Unknown_A4h);
            writer.Write(this.Unknown_A8h);
            writer.WriteBlock(this.Unknown_B0h);
            writer.WriteBlock(this.BoneWeightsInds);
            writer.Write(this.Unknown_D0h);
            writer.Write(this.Unknown_D8h);
            writer.Write(this.Unknown_DCh);
            writer.WriteBlock(this.BoneIds);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            base.WriteXml(sb, indent);
            YldXml.ValueTag(sb, indent, "UnknownA0", FloatUtil.ToString(Unknown_A0h));
            YldXml.ValueTag(sb, indent, "UnknownDC", FloatUtil.ToString(Unknown_DCh));
            if (Vertices?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Vertices.data_items, indent, "Vertices", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Indices?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Indices.data_items, indent, "Indices", "");
            }
            if (Unknown_B0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_B0h.data_items, indent, "UnknownB0", "");
            }
            if (BoneIds?.data_items != null)
            {
                YldXml.WriteRawArray(sb, BoneIds.data_items, indent, "BoneIDs", "");
            }
            if (BoneWeightsInds?.data_items != null)
            {
                YldXml.WriteItemArray(sb, BoneWeightsInds.data_items, indent, "BoneWeightsIndices");
            }
        }
        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            Unknown_A0h = Xml.GetChildFloatAttribute(node, "UnknownA0", "value");
            Unknown_DCh = Xml.GetChildFloatAttribute(node, "UnknownDC", "value");
            Vertices = new ResourceSimpleList64_s<Vector4>();
            Vertices.data_items = Xml.GetChildRawVector4ArrayNullable(node, "Vertices");
            Indices = new ResourceSimpleList64_ushort();
            Indices.data_items = Xml.GetChildRawUshortArrayNullable(node, "Indices");
            Unknown_B0h = new ResourceSimpleList64_uint();
            Unknown_B0h.data_items = Xml.GetChildRawUintArrayNullable(node, "UnknownB0");
            BoneIds = new ResourceSimpleList64_uint();
            BoneIds.data_items = Xml.GetChildRawUintArrayNullable(node, "BoneIDs");
            BoneWeightsInds = new ResourceSimpleList64_s<CharClothBoneWeightsInds>();
            BoneWeightsInds.data_items = XmlMeta.ReadItemArray<CharClothBoneWeightsInds>(node, "BoneWeightsIndices");
        }


        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x80, Indices),
                new Tuple<long, IResourceBlock>(0x90, Vertices),
                new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                new Tuple<long, IResourceBlock>(0xC0, BoneWeightsInds),
                new Tuple<long, IResourceBlock>(0xE0, BoneIds)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class MorphController : ResourceSystemBlock
    {
        // pgBase
        // phMorphController
        public override long BlockLength => 0x40;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h = 1; // 0x00000001
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h_Pointer { get; set; }
        public ulong Unknown_20h_Pointer { get; set; }
        public ulong Unknown_28h_Pointer { get; set; }
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000

        // reference data
        public Unknown_C_006 Unknown_18h_Data { get; set; }
        public Unknown_C_006 Unknown_20h_Data { get; set; }
        public Unknown_C_006 Unknown_28h_Data { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.VFT = reader.ReadUInt32();
            this.Unknown_4h = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h_Pointer = reader.ReadUInt64();
            this.Unknown_20h_Pointer = reader.ReadUInt64();
            this.Unknown_28h_Pointer = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();

            // read reference data
            this.Unknown_18h_Data = reader.ReadBlockAt<Unknown_C_006>(this.Unknown_18h_Pointer);
            this.Unknown_20h_Data = reader.ReadBlockAt<Unknown_C_006>(this.Unknown_20h_Pointer);
            this.Unknown_28h_Data = reader.ReadBlockAt<Unknown_C_006>(this.Unknown_28h_Pointer);

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.Unknown_18h_Pointer = (ulong)(this.Unknown_18h_Data != null ? this.Unknown_18h_Data.FilePosition : 0);
            this.Unknown_20h_Pointer = (ulong)(this.Unknown_20h_Data != null ? this.Unknown_20h_Data.FilePosition : 0);
            this.Unknown_28h_Pointer = (ulong)(this.Unknown_28h_Data != null ? this.Unknown_28h_Data.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h_Pointer);
            writer.Write(this.Unknown_20h_Pointer);
            writer.Write(this.Unknown_28h_Pointer);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            if (Unknown_18h_Data != null)
            {
                YldXml.OpenTag(sb, indent, "Unknown18");
                Unknown_18h_Data.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "Unknown18");
            }
            if (Unknown_20h_Data != null)
            {
                YldXml.OpenTag(sb, indent, "Unknown20");
                Unknown_20h_Data.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "Unknown20");
            }
            if (Unknown_28h_Data != null)
            {
                YldXml.OpenTag(sb, indent, "Unknown28");
                Unknown_28h_Data.WriteXml(sb, indent + 1);
                YldXml.CloseTag(sb, indent, "Unknown28");
            }
        }
        public void ReadXml(XmlNode node)
        {
            var u18node = node.SelectSingleNode("Unknown18");
            if (u18node != null)
            {
                Unknown_18h_Data = new Unknown_C_006();
                Unknown_18h_Data.ReadXml(u18node);
            }
            var u20node = node.SelectSingleNode("Unknown20");
            if (u20node != null)
            {
                Unknown_20h_Data = new Unknown_C_006();
                Unknown_20h_Data.ReadXml(u20node);
            }
            var u28node = node.SelectSingleNode("Unknown28");
            if (u28node != null)
            {
                Unknown_28h_Data = new Unknown_C_006();
                Unknown_28h_Data.ReadXml(u28node);
            }
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Unknown_18h_Data != null) list.Add(Unknown_18h_Data);
            if (Unknown_20h_Data != null) list.Add(Unknown_20h_Data);
            if (Unknown_28h_Data != null) list.Add(Unknown_28h_Data);
            return list.ToArray();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct CharClothBoneWeightsInds : IMetaXmlItem
    {
        public Vector4 Weights { get; set; }
        public uint Index0 { get; set; }
        public uint Index1 { get; set; }
        public uint Index2 { get; set; }
        public uint Index3 { get; set; }


        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.SelfClosingTag(sb, indent, "Weights " + FloatUtil.GetVector4XmlString(Weights));
            YldXml.ValueTag(sb, indent, "Index0", Index0.ToString());
            YldXml.ValueTag(sb, indent, "Index1", Index1.ToString());
            YldXml.ValueTag(sb, indent, "Index2", Index2.ToString());
            YldXml.ValueTag(sb, indent, "Index3", Index3.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Weights = Xml.GetChildVector4Attributes(node, "Weights");
            Index0 = Xml.GetChildUIntAttribute(node, "Index0", "value");
            Index1 = Xml.GetChildUIntAttribute(node, "Index1", "value");
            Index2 = Xml.GetChildUIntAttribute(node, "Index2", "value");
            Index3 = Xml.GetChildUIntAttribute(node, "Index3", "value");
        }

        public override string ToString()
        {
            return Weights.ToString() + "   :   " + Index0.ToString() + ", " + Index1.ToString() + ", " + Index2.ToString() + ", " + Index3.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public struct Unknown_C_004 : IMetaXmlItem
    {
        public ushort Unknown_0h { get; set; }
        public ushort Unknown_2h { get; set; }
        public float Unknown_4h { get; set; }
        public float Unknown_8h { get; set; }
        public float Unknown_Ch { get; set; }


        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.ValueTag(sb, indent, "Unknown0", Unknown_0h.ToString());
            YldXml.ValueTag(sb, indent, "Unknown2", Unknown_2h.ToString());
            YldXml.ValueTag(sb, indent, "Unknown4", FloatUtil.ToString(Unknown_4h));
            YldXml.ValueTag(sb, indent, "Unknown8", FloatUtil.ToString(Unknown_8h));
            YldXml.ValueTag(sb, indent, "UnknownC", FloatUtil.ToString(Unknown_Ch));
        }
        public void ReadXml(XmlNode node)
        {
            Unknown_0h = (ushort)Xml.GetChildUIntAttribute(node, "Unknown0", "value");
            Unknown_2h = (ushort)Xml.GetChildUIntAttribute(node, "Unknown2", "value");
            Unknown_4h = Xml.GetChildFloatAttribute(node, "Unknown4", "value");
            Unknown_8h = Xml.GetChildFloatAttribute(node, "Unknown8", "value");
            Unknown_Ch = Xml.GetChildFloatAttribute(node, "UnknownC", "value");
        }

        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_2h.ToString() + ", " + Unknown_4h.ToString() + ", " + Unknown_8h.ToString() + ", " + Unknown_Ch.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_C_006 : ResourceSystemBlock
    {
        public override long BlockLength => 0x190;

        // structure data
        public ulong Unknown_0h; // 0x0000000000000000
        public ulong Unknown_8h; // 0x0000000000000000
        public ulong Unknown_10h; // 0x0000000000000000
        public ulong Unknown_18h; // 0x0000000000000000
        public ulong Unknown_20h; // 0x0000000000000000
        public ulong Unknown_28h; // 0x0000000000000000
        public ulong Unknown_30h; // 0x0000000000000000
        public ulong Unknown_38h; // 0x0000000000000000
        public ulong Unknown_40h; // 0x0000000000000000
        public ulong Unknown_48h; // 0x0000000000000000
        public ResourceSimpleList64_s<Vector4> Unknown_50h { get; set; }
        public ResourceSimpleList64_ushort Unknown_60h { get; set; }
        public ResourceSimpleList64_ushort Unknown_70h { get; set; }
        public ResourceSimpleList64_ushort Unknown_80h { get; set; }
        public ResourceSimpleList64_ushort Unknown_90h { get; set; }
        public ResourceSimpleList64_s<Vector4> Unknown_A0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_B0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_C0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_D0h { get; set; }
        public ResourceSimpleList64_ushort Unknown_E0h { get; set; }
        public ulong Unknown_F0h; // 0x0000000000000000
        public ulong Unknown_F8h; // 0x0000000000000000
        public ulong Unknown_100h; // 0x0000000000000000
        public ulong Unknown_108h; // 0x0000000000000000
        public ulong Unknown_110h; // 0x0000000000000000
        public ulong Unknown_118h; // 0x0000000000000000
        public ulong Unknown_120h; // 0x0000000000000000
        public ulong Unknown_128h; // 0x0000000000000000
        public ulong Unknown_130h; // 0x0000000000000000
        public ulong Unknown_138h; // 0x0000000000000000
        public ulong Unknown_140h; // 0x0000000000000000
        public ulong Unknown_148h; // 0x0000000000000000
        public ResourceSimpleList64_ushort Unknown_150h { get; set; }
        public ResourceSimpleList64_ushort Unknown_160h { get; set; }
        public ulong Unknown_170h; // 0x0000000000000000
        public ulong Unknown_178h; // 0x0000000000000000
        public uint Unknown_180h { get; set; }
        public uint Unknown_184h; // 0x00000000
        public ulong Unknown_188h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt64();
            this.Unknown_10h = reader.ReadUInt64();
            this.Unknown_18h = reader.ReadUInt64();
            this.Unknown_20h = reader.ReadUInt64();
            this.Unknown_28h = reader.ReadUInt64();
            this.Unknown_30h = reader.ReadUInt64();
            this.Unknown_38h = reader.ReadUInt64();
            this.Unknown_40h = reader.ReadUInt64();
            this.Unknown_48h = reader.ReadUInt64();
            this.Unknown_50h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_60h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_70h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_80h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_90h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_A0h = reader.ReadBlock<ResourceSimpleList64_s<Vector4>>();
            this.Unknown_B0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_C0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_D0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_E0h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_F0h = reader.ReadUInt64();
            this.Unknown_F8h = reader.ReadUInt64();
            this.Unknown_100h = reader.ReadUInt64();
            this.Unknown_108h = reader.ReadUInt64();
            this.Unknown_110h = reader.ReadUInt64();
            this.Unknown_118h = reader.ReadUInt64();
            this.Unknown_120h = reader.ReadUInt64();
            this.Unknown_128h = reader.ReadUInt64();
            this.Unknown_130h = reader.ReadUInt64();
            this.Unknown_138h = reader.ReadUInt64();
            this.Unknown_140h = reader.ReadUInt64();
            this.Unknown_148h = reader.ReadUInt64();
            this.Unknown_150h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_160h = reader.ReadBlock<ResourceSimpleList64_ushort>();
            this.Unknown_170h = reader.ReadUInt64();
            this.Unknown_178h = reader.ReadUInt64();
            this.Unknown_180h = reader.ReadUInt32();
            this.Unknown_184h = reader.ReadUInt32();
            this.Unknown_188h = reader.ReadUInt64();

            //if (Unknown_0h != 0)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_20h != 0)
            //{ }//no hit
            //if (Unknown_28h != 0)
            //{ }//no hit
            //if (Unknown_30h != 0)
            //{ }//no hit
            //if (Unknown_38h != 0)
            //{ }//no hit
            //if (Unknown_40h != 0)
            //{ }//no hit
            //if (Unknown_48h != 0)
            //{ }//no hit
            //if (Unknown_F0h != 0)
            //{ }//no hit
            //if (Unknown_F8h != 0)
            //{ }//no hit
            //if (Unknown_100h != 0)
            //{ }//no hit
            //if (Unknown_108h != 0)
            //{ }//no hit
            //if (Unknown_110h != 0)
            //{ }//no hit
            //if (Unknown_118h != 0)
            //{ }//no hit
            //if (Unknown_120h != 0)
            //{ }//no hit
            //if (Unknown_128h != 0)
            //{ }//no hit
            //if (Unknown_130h != 0)
            //{ }//no hit
            //if (Unknown_138h != 0)
            //{ }//no hit
            //if (Unknown_140h != 0)
            //{ }//no hit
            //if (Unknown_148h != 0)
            //{ }//no hit
            //if (Unknown_170h != 0)
            //{ }//no hit
            //if (Unknown_178h != 0)
            //{ }//no hit
            //if (Unknown_184h != 0)
            //{ }//no hit
            //if (Unknown_188h != 0)
            //{ }//no hit
            //switch (Unknown_180h)
            //{
            //    case 90:
            //    case 108:
            //    case 140:
            //    case 150:
            //    case 218:
            //    case 236:
            //    case 323:
            //    case 368:
            //    case 384:
            //    case 420:
            //    case 434:
            //    case 440:
            //    case 512:
            //    case 640:
            //    case 768:
            //    case 896:
            //    case 960:
            //    case 1152:
            //        break;
            //    default:
            //        break;//+more...
            //}
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_20h);
            writer.Write(this.Unknown_28h);
            writer.Write(this.Unknown_30h);
            writer.Write(this.Unknown_38h);
            writer.Write(this.Unknown_40h);
            writer.Write(this.Unknown_48h);
            writer.WriteBlock(this.Unknown_50h);
            writer.WriteBlock(this.Unknown_60h);
            writer.WriteBlock(this.Unknown_70h);
            writer.WriteBlock(this.Unknown_80h);
            writer.WriteBlock(this.Unknown_90h);
            writer.WriteBlock(this.Unknown_A0h);
            writer.WriteBlock(this.Unknown_B0h);
            writer.WriteBlock(this.Unknown_C0h);
            writer.WriteBlock(this.Unknown_D0h);
            writer.WriteBlock(this.Unknown_E0h);
            writer.Write(this.Unknown_F0h);
            writer.Write(this.Unknown_F8h);
            writer.Write(this.Unknown_100h);
            writer.Write(this.Unknown_108h);
            writer.Write(this.Unknown_110h);
            writer.Write(this.Unknown_118h);
            writer.Write(this.Unknown_120h);
            writer.Write(this.Unknown_128h);
            writer.Write(this.Unknown_130h);
            writer.Write(this.Unknown_138h);
            writer.Write(this.Unknown_140h);
            writer.Write(this.Unknown_148h);
            writer.WriteBlock(this.Unknown_150h);
            writer.WriteBlock(this.Unknown_160h);
            writer.Write(this.Unknown_170h);
            writer.Write(this.Unknown_178h);
            writer.Write(this.Unknown_180h);
            writer.Write(this.Unknown_184h);
            writer.Write(this.Unknown_188h);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YldXml.ValueTag(sb, indent, "Unknown180", FloatUtil.ToString(Unknown_180h));
            if (Unknown_50h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_50h.data_items, indent, "Unknown50", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Unknown_60h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_60h.data_items, indent, "Unknown60", "");
            }
            if (Unknown_70h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_70h.data_items, indent, "Unknown70", "");
            }
            if (Unknown_80h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_80h.data_items, indent, "Unknown80", "");
            }
            if (Unknown_90h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_90h.data_items, indent, "Unknown90", "");
            }
            if (Unknown_A0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_A0h.data_items, indent, "UnknownA0", "", v => FloatUtil.GetVector4String(v), 1);
            }
            if (Unknown_B0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_B0h.data_items, indent, "UnknownB0", "");
            }
            if (Unknown_C0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_C0h.data_items, indent, "UnknownC0", "");
            }
            if (Unknown_D0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_D0h.data_items, indent, "UnknownD0", "");
            }
            if (Unknown_E0h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_E0h.data_items, indent, "UnknownE0", "");
            }
            if (Unknown_150h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_150h.data_items, indent, "Unknown150", "");
            }
            if (Unknown_160h?.data_items != null)
            {
                YldXml.WriteRawArray(sb, Unknown_160h.data_items, indent, "Unknown160", "");
            }
        }
        public void ReadXml(XmlNode node)
        {
            Unknown_180h = Xml.GetChildUIntAttribute(node, "Unknown180", "value");
            Unknown_50h = new ResourceSimpleList64_s<Vector4>();
            Unknown_50h.data_items = Xml.GetChildRawVector4ArrayNullable(node, "Unknown50");
            Unknown_60h = new ResourceSimpleList64_ushort();
            Unknown_60h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown60");
            Unknown_70h = new ResourceSimpleList64_ushort();
            Unknown_70h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown70");
            Unknown_80h = new ResourceSimpleList64_ushort();
            Unknown_80h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown80");
            Unknown_90h = new ResourceSimpleList64_ushort();
            Unknown_90h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown90");
            Unknown_A0h = new ResourceSimpleList64_s<Vector4>();
            Unknown_A0h.data_items = Xml.GetChildRawVector4ArrayNullable(node, "UnknownA0");
            Unknown_B0h = new ResourceSimpleList64_ushort();
            Unknown_B0h.data_items = Xml.GetChildRawUshortArrayNullable(node, "UnknownB0");
            Unknown_C0h = new ResourceSimpleList64_ushort();
            Unknown_C0h.data_items = Xml.GetChildRawUshortArrayNullable(node, "UnknownC0");
            Unknown_D0h = new ResourceSimpleList64_ushort();
            Unknown_D0h.data_items = Xml.GetChildRawUshortArrayNullable(node, "UnknownD0");
            Unknown_E0h = new ResourceSimpleList64_ushort();
            Unknown_E0h.data_items = Xml.GetChildRawUshortArrayNullable(node, "UnknownE0");
            Unknown_150h = new ResourceSimpleList64_ushort();
            Unknown_150h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown150");
            Unknown_160h = new ResourceSimpleList64_ushort();
            Unknown_160h.data_items = Xml.GetChildRawUshortArrayNullable(node, "Unknown160");
        }

        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x50, Unknown_50h),
                new Tuple<long, IResourceBlock>(0x60, Unknown_60h),
                new Tuple<long, IResourceBlock>(0x70, Unknown_70h),
                new Tuple<long, IResourceBlock>(0x80, Unknown_80h),
                new Tuple<long, IResourceBlock>(0x90, Unknown_90h),
                new Tuple<long, IResourceBlock>(0xA0, Unknown_A0h),
                new Tuple<long, IResourceBlock>(0xB0, Unknown_B0h),
                new Tuple<long, IResourceBlock>(0xC0, Unknown_C0h),
                new Tuple<long, IResourceBlock>(0xD0, Unknown_D0h),
                new Tuple<long, IResourceBlock>(0xE0, Unknown_E0h),
                new Tuple<long, IResourceBlock>(0x150, Unknown_150h),
                new Tuple<long, IResourceBlock>(0x160, Unknown_160h)
            };
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class Unknown_C_007 : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public ulong Unknown_0h; // 0x0000000000000000
        public ulong Unknown_8h; // 0x0000000000000000

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.Unknown_0h = reader.ReadUInt64();
            this.Unknown_8h = reader.ReadUInt64();

            //if (Unknown_0h != 0)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // write structure data
            writer.Write(this.Unknown_0h);
            writer.Write(this.Unknown_8h);
        }


        public override string ToString()
        {
            return Unknown_0h.ToString() + ", " + Unknown_8h.ToString();
        }
    }















    public class ClothInstance
    {
        public CharacterCloth CharCloth { get; set; }
        public EnvironmentCloth EnvCloth { get; set; }
        public ClothController Controller { get; set; }

        public Bone[] Bones { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;
        public Vector4[] Vertices { get; set; }

        public Skeleton Skeleton { get; set; }


        double CurrentTime = 0.0;

        public void Init(CharacterCloth c, Skeleton s)
        {
            CharCloth = c;
            Skeleton = s;
            Init(c?.Controller);

            var cc = c.Controller;
            var verts = cc?.Vertices?.data_items;
            if (verts != null)
            {
                if (verts.Length >= Vertices?.Length)
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        Vertices[i] = verts[i];
                    }
                }
                else
                { }
            }

            var t = c.Transform;
            t.M44 = 1.0f;
            Transform = t;


            var boneIds = cc.BoneIds?.data_items;
            if ((boneIds != null) && (Skeleton != null))
            {
                Bones = new Bone[boneIds.Length];
                for (int i = 0; i < Bones.Length; i++)
                {
                    var boneid = (ushort)boneIds[i];
                    Bone bone = null;
                    Skeleton.BonesMap.TryGetValue(boneid, out bone);
                    Bones[i] = bone;
                }
            }



        }
        public void Init(EnvironmentCloth c, Skeleton s)
        {
            EnvCloth = c;
            Skeleton = s;
            Init(c?.Controller);
        }
        private void Init(ClothController cc)
        {
            Controller = cc;

            var bg = cc?.BridgeSimGfx;
            var vc = bg?.VertexCount ?? 0;
            if (vc > 0)
            {
                Vertices = new Vector4[vc];

            }


        }




        public void Update(double t)
        {
            if (Vertices == null) return;
            if (CurrentTime == t) return;

            var elapsed = (float)(t - CurrentTime);

            var charCont = CharCloth?.Controller;
            if (charCont != null)
            {
                Update(charCont, elapsed);
            }

            CurrentTime = t;
        }

        private void Update(CharacterClothController charCont, float elapsed)
        {
            if (Bones == null)
            { return; }

            var cv = charCont?.Vertices?.data_items;
            if (Vertices.Length > cv?.Length)
            { return; }

            var bounds = CharCloth.Bound as BoundComposite;
            var capsules = bounds?.Children?.data_items;
            var capsuleBoneIds = CharCloth.Unknown_30h?.data_items;
            if (capsules?.Length != capsuleBoneIds?.Length)
            { }

            var bsg = charCont.BridgeSimGfx;
            var vc1 = charCont.VerletCloth1;
            var unk00 = charCont.Unknown_B0h?.data_items;//anchor verts/constraints..? related to bsg.Unknown_E0h? same count as boneids
            var winds = charCont.BoneWeightsInds?.data_items;//bone weights/indices   - numverts
            var cons1 = vc1?.Constraints2?.data_items;//constraints? less than numverts
            var cons2 = vc1?.Constraints?.data_items;//constraints? more than numverts
            var unk0 = bsg?.Unknown_20h?.data_items;// 0-1 weights values for something?
            var unk1 = bsg?.Unknown_60h?.data_items;// cloth thickness?
            var unk2 = bsg?.Unknown_A0h?.data_items;//numverts zeroes?
            var unk3 = bsg?.Unknown_E0h?.data_items;//mapping? connections?
            var unk4 = bsg?.Unknown_128h?.data_items;//(boneids+1?) zeroes? has numverts capacity?


            for (int v = 0; v < Vertices.Length; v++)
            {
                //transform the vertices using the bone weights/indices. this should provide positions for anchored verts
                var ov = cv[v].XYZ();
                var wind = (v < winds?.Length) ? winds[v] : new CharClothBoneWeightsInds();
                var b0 = (wind.Index0 < Bones.Length) ? Bones[wind.Index0] : null;
                var b1 = (wind.Index1 < Bones.Length) ? Bones[wind.Index1] : null;
                var b2 = (wind.Index2 < Bones.Length) ? Bones[wind.Index2] : null;
                var b3 = (wind.Index3 < Bones.Length) ? Bones[wind.Index3] : null;
                var w0 = wind.Weights.X;
                var w1 = wind.Weights.Y;
                var w2 = wind.Weights.Z;
                var w3 = wind.Weights.W;
                var v0 = ((w0 > 0) ? b0 : null)?.SkinTransform.MultiplyW(ov) ?? ov;
                var v1 = ((w1 > 0) ? b1 : null)?.SkinTransform.MultiplyW(ov) ?? ov;
                var v2 = ((w2 > 0) ? b2 : null)?.SkinTransform.MultiplyW(ov) ?? ov;
                var v3 = ((w3 > 0) ? b3 : null)?.SkinTransform.MultiplyW(ov) ?? ov;
                var nv = v0*w0 + v1*w1 + v2*w2 + v3*w3;
                Vertices[v] = new Vector4(nv, 0);
            }


        }

    }





}
