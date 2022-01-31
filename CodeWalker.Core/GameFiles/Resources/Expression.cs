using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
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


//ruthlessly stolen


namespace CodeWalker.GameFiles
{

    [TC(typeof(EXP))] public class ExpressionDictionary : ResourceFileBase
    {
        // pgDictionaryBase
        // pgDictionary<crExpressions>
        public override long BlockLength => 0x40;

        // structure data
        public uint Unknown_10h { get; set; } = 0;
        public uint Unknown_14h { get; set; } = 0;
        public uint Unknown_18h { get; set; } = 1;
        public uint Unknown_1Ch { get; set; } = 0;
        public ResourceSimpleList64_s<MetaHash> ExpressionNameHashes { get; set; }
        public ResourcePointerList64<Expression> Expressions { get; set; }

        
        public Dictionary<MetaHash, Expression> ExprMap { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadUInt32();
            this.Unknown_14h = reader.ReadUInt32();
            this.Unknown_18h = reader.ReadUInt32();
            this.Unknown_1Ch = reader.ReadUInt32();
            this.ExpressionNameHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.Expressions = reader.ReadBlock<ResourcePointerList64<Expression>>();

            BuildMap();

        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.ExpressionNameHashes);
            writer.WriteBlock(this.Expressions);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {

            if (Expressions?.data_items != null)
            {
                foreach (var e in Expressions.data_items)
                {
                    YedXml.OpenTag(sb, indent, "Item");
                    e.WriteXml(sb, indent + 1);
                    YedXml.CloseTag(sb, indent, "Item");
                }
            }

        }
        public void ReadXml(XmlNode node)
        {
            var expressions = new List<Expression>();
            var expressionhashes = new List<MetaHash>();

            var inodes = node.SelectNodes("Item");
            if (inodes != null)
            {
                foreach (XmlNode inode in inodes)
                {
                    var e = new Expression();
                    e.ReadXml(inode);
                    expressions.Add(e);
                    expressionhashes.Add(e.NameHash);
                }
            }

            ExpressionNameHashes = new ResourceSimpleList64_s<MetaHash>();
            ExpressionNameHashes.data_items = expressionhashes.ToArray();
            Expressions = new ResourcePointerList64<Expression>();
            Expressions.data_items = expressions.ToArray();
            
            BuildMap();
        }
        public static void WriteXmlNode(ExpressionDictionary d, StringBuilder sb, int indent, string name = "ExpressionDictionary")
        {
            if (d == null) return;
            if ((d.Expressions?.data_items == null) || (d.Expressions.data_items.Length == 0))
            {
                YedXml.SelfClosingTag(sb, indent, name);
            }
            else
            {
                YedXml.OpenTag(sb, indent, name);
                d.WriteXml(sb, indent + 1);
                YedXml.CloseTag(sb, indent, name);
            }
        }
        public static ExpressionDictionary ReadXmlNode(XmlNode node)
        {
            if (node == null) return null;
            var ed = new ExpressionDictionary();
            ed.ReadXml(node);
            return ed;
        }


        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            return list.ToArray();
        }
        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, ExpressionNameHashes),
                new Tuple<long, IResourceBlock>(0x30, Expressions)
            };
        }


        public void BuildMap()
        {
            ExprMap = new Dictionary<MetaHash, Expression>();

            if ((Expressions?.data_items != null) && (ExpressionNameHashes?.data_items != null))
            {
                var exprs = Expressions.data_items;
                var names = ExpressionNameHashes.data_items;

                for (int i = 0; i < exprs.Length; i++)
                {
                    var expr = exprs[i];
                    var name = (i < names.Length) ? names[i] : (MetaHash)JenkHash.GenHash(expr?.GetShortName() ?? "");
                    expr.NameHash = name;
                    ExprMap[name] = expr;
                }
            }

        }

    }



    [TC(typeof(EXP))] public class Expression : ResourceSystemBlock
    {
        // pgBase
        // crExpressions
        public override long BlockLength => 0x90;

        // structure data
        public uint VFT { get; set; }
        public uint Unknown_4h { get; set; } = 1;
        public uint Unknown_8h { get; set; } // 0x00000000
        public uint Unknown_Ch { get; set; } // 0x00000000
        public uint Unknown_10h { get; set; } // 0x00000000
        public uint Unknown_14h { get; set; } // 0x00000000
        public uint Unknown_18h { get; set; } // 0x00000000
        public uint Unknown_1Ch { get; set; } // 0x00000000
        public ResourcePointerList64<ExpressionStream> Streams { get; set; }
        public ResourceSimpleList64_s<ExpressionBoneTrack> BoneTracks { get; set; } // bone tags / animation tracks
        public ResourceSimpleList64<ExpressionJiggleBlock> JiggleData { get; set; } //compiled list of jiggles data from all jiggle Stream nodes
        public ResourceSimpleList64_s<MetaHash> ItemHashes { get; set; }  // maybe variables?  only for: faceinit.expr, independent_mover.expr
        public ulong NamePointer { get; set; }
        public ushort NameLength { get; set; } // name len
        public ushort NameCapacity { get; set; } // name len+1
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } = 1;
        public MetaHash Unknown_74h { get; set; } // seems to be a hash?
        public uint MaxStreamSize { get; set; } // max length of any item in Streams
        public uint Unknown_7Ch { get; set; } // 3 or 2
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }

        public Dictionary<ExpressionBoneTrack, ExpressionBoneTrack> BoneTracksDict { get; set; }


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
            this.Streams = reader.ReadBlock<ResourcePointerList64<ExpressionStream>>();
            this.BoneTracks = reader.ReadBlock<ResourceSimpleList64_s<ExpressionBoneTrack>>();
            this.JiggleData = reader.ReadBlock<ResourceSimpleList64<ExpressionJiggleBlock>>();
            this.ItemHashes = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.NamePointer = reader.ReadUInt64();
            this.NameLength = reader.ReadUInt16();
            this.NameCapacity = reader.ReadUInt16();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt32();
            this.Unknown_74h = reader.ReadUInt32();
            this.MaxStreamSize = reader.ReadUInt32();
            this.Unknown_7Ch = reader.ReadUInt32();
            this.Unknown_80h = reader.ReadUInt32();
            this.Unknown_84h = reader.ReadUInt32();
            this.Unknown_88h = reader.ReadUInt32();
            this.Unknown_8Ch = reader.ReadUInt32();

            // read reference data
            this.Name = reader.ReadBlockAt<string_r>(
                this.NamePointer // offset
            );

            JenkIndex.Ensure(GetShortName());


            BuildBoneTracksDict();



            #region testing

            //if (Streams?.data_items != null)
            //{ }

            ////if ((this.JiggleData?.data_items?.Length ?? 0) > 0)
            //{
            //    var cnt1 = JiggleData?.data_items?.Length ?? 0;
            //    var cnt2 = 0;
            //    foreach (var stream in Streams.data_items)
            //    {
            //        foreach (var node in stream.Items)
            //        {
            //            if (node is ExpressionNodeJiggle jnode)
            //            {
            //                var trackrot = BoneTracks.data_items[jnode.BoneTrackRot];
            //                var trackpos = BoneTracks.data_items[jnode.BoneTrackPos];
            //                var jd1 = jnode.JiggleData;
            //                var jd2 = JiggleData.data_items[cnt2].JiggleData;
            //                if (!jd1.Compare(jd2))
            //                { }//no hit
            //                if (jd2.BoneTag != trackrot.BoneTag)
            //                { }//no hit
            //                cnt2++;
            //            }
            //        }
            //    }
            //    if (cnt1 != cnt2)
            //    { }//no hit
            //}

            //long tlen = 0;
            //if (Streams?.data_items != null) foreach (var item in Streams.data_items) tlen = Math.Max(tlen, item.BlockLength);
            //if (MaxStreamSize != tlen)
            //{ }//no hit

            //if (Unknown_4h != 1)
            //{ }//no hit
            //if (Unknown_8h != 0)
            //{ }//no hit
            //if (Unknown_Ch != 0)
            //{ }//no hit
            //if (Unknown_10h != 0)
            //{ }//no hit
            //if (Unknown_14h != 0)
            //{ }//no hit
            //if (Unknown_18h != 0)
            //{ }//no hit
            //if (Unknown_1Ch != 0)
            //{ }//no hit
            //if (NameLength != (Name?.Value?.Length ?? 0))
            //{ }//no hit
            //if (NameCapacity != (NameLength + 1))
            //{ }//no hit
            //if (Unknown_6Ch != 0)
            //{ }//no hit
            //if (Unknown_70h != 1)
            //{ }//no hit
            //switch (Unknown_74h)
            //{
            //    default:
            //        break;
            //}
            //switch (Unknown_7Ch)
            //{
            //    case 3:
            //    case 2:
            //        break;
            //    default:
            //        break;//no hit
            //}
            //if (Unknown_80h != 0)
            //{ }//no hit
            //if (Unknown_84h != 0)
            //{ }//no hit
            //if (Unknown_88h != 0)
            //{ }//no hit
            //if (Unknown_8Ch != 0)
            //{ }//no hit
            #endregion
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.NamePointer = (ulong)(this.Name != null ? this.Name.FilePosition : 0);

            // write structure data
            writer.Write(this.VFT);
            writer.Write(this.Unknown_4h);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.Unknown_1Ch);
            writer.WriteBlock(this.Streams);
            writer.WriteBlock(this.BoneTracks);
            writer.WriteBlock(this.JiggleData);
            writer.WriteBlock(this.ItemHashes);
            writer.Write(this.NamePointer);
            writer.Write(this.NameLength);
            writer.Write(this.NameCapacity);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Unknown_74h);
            writer.Write(this.MaxStreamSize);
            writer.Write(this.Unknown_7Ch);
            writer.Write(this.Unknown_80h);
            writer.Write(this.Unknown_84h);
            writer.Write(this.Unknown_88h);
            writer.Write(this.Unknown_8Ch);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.StringTag(sb, indent, "Name", Name?.Value ?? "");
            YedXml.StringTag(sb, indent, "Unk74", YedXml.HashString(Unknown_74h));
            YedXml.ValueTag(sb, indent, "Unk7C", Unknown_7Ch.ToString());

            if ((BoneTracks?.data_items?.Length ?? 0) > 0)
            {
                YedXml.WriteItemArray(sb, BoneTracks.data_items, indent, "BoneTracks");
            }

            if ((Streams?.data_items?.Length ?? 0) > 0)
            {
                YedXml.WriteItemArray(sb, Streams.data_items, indent, "Streams");
            }

        }
        public void ReadXml(XmlNode node)
        {
            Name = new string_r();
            Name.Value = Xml.GetChildInnerText(node, "Name");
            NameLength = (ushort)Name.Value.Length;
            NameCapacity = (ushort)(NameLength + 1);
            NameHash = JenkHash.GenHash(GetShortName());
            Unknown_74h = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Unk74"));
            Unknown_7Ch = Xml.GetChildUIntAttribute(node, "Unk7C", "value");

            BoneTracks = new ResourceSimpleList64_s<ExpressionBoneTrack>();
            BoneTracks.data_items = XmlMeta.ReadItemArray<ExpressionBoneTrack>(node, "BoneTracks");

            Streams = new ResourcePointerList64<ExpressionStream>();
            Streams.data_items = XmlMeta.ReadItemArray<ExpressionStream>(node, "Streams");

            BuildBoneTracksDict();
            BuildJiggleData();
            UpdateItemHashes();
            UpdateStreamBuffers();
        }

        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (Name != null) list.Add(Name);
            return list.ToArray();
        }
        public override Tuple<long, IResourceBlock>[] GetParts()
        {
            return new Tuple<long, IResourceBlock>[] {
                new Tuple<long, IResourceBlock>(0x20, Streams),
                new Tuple<long, IResourceBlock>(0x30, BoneTracks),
                new Tuple<long, IResourceBlock>(0x40, JiggleData),
                new Tuple<long, IResourceBlock>(0x50, ItemHashes)
            };
        }


        public void BuildBoneTracksDict()
        {
            BoneTracksDict = new Dictionary<ExpressionBoneTrack, ExpressionBoneTrack>();

            if (BoneTracks?.data_items == null) return;

            var mapto = new ExpressionBoneTrack();
            for(int i=0; i< BoneTracks.data_items.Length;i++)
            {
                var bt = BoneTracks.data_items[i];
                if ((bt.Flags & 128) == 0)
                {
                    mapto = bt;
                }
                else if (bt.BoneTag != 0)
                {
                    bt.Flags &= 0x7F;
                    BoneTracksDict[bt] = mapto;
                }
            }

        }

        public void BuildJiggleData()
        {
            var jiggles = new List<ExpressionJiggleBlock>();
            if (Streams?.data_items != null)
            {
                foreach (var stream in Streams.data_items)
                {
                    foreach (var node in stream.Items)
                    {
                        if (node is ExpressionNodeJiggle jnode)
                        {
                            var jiggle = new ExpressionJiggleBlock();
                            jiggle.JiggleData = jnode.JiggleData.Clone();
                            jiggles.Add(jiggle);
                        }
                    }
                }
            }
            JiggleData = new ResourceSimpleList64<ExpressionJiggleBlock>();
            JiggleData.data_items = jiggles.ToArray();
        }

        public void UpdateItemHashes()
        {
            var dict = new Dictionary<MetaHash, int>();
            if (Streams?.data_items != null)
            {
                foreach (var stream in Streams.data_items)
                {
                    foreach (var item in stream.Items)
                    {
                        if (item is ExpressionNodeS3 s3)
                        {
                            dict[s3.Hash] = 0;
                        }
                    }
                }
            }
            var list = dict.Keys.ToList();
            list.Sort((a, b) => a.Hash.CompareTo(b.Hash));
            for (int i = 0; i < list.Count; i++)
            {
                dict[list[i]] = i;
            }
            if (Streams?.data_items != null)
            {
                foreach (var stream in Streams.data_items)
                {
                    foreach (var item in stream.Items)
                    {
                        if (item is ExpressionNodeS3 s3)
                        {
                            var index = dict[s3.Hash];
                            s3.HashIndex = (byte)index;
                        }
                    }
                }
            }

            ItemHashes = new ResourceSimpleList64_s<MetaHash>();
            ItemHashes.data_items = list.ToArray();

        }

        public void UpdateStreamBuffers()
        {
            MaxStreamSize = 0;
            if (Streams?.data_items != null)
            {
                foreach (var item in Streams.data_items)
                {
                    //item.FlattenHierarchy();
                    item.WriteItems(); //makes sure the data buffers are updated to the correct length!
                    MaxStreamSize = Math.Max(MaxStreamSize, (uint)item.BlockLength);
                }
            }
        }


        public string GetShortName()
        {
            return Path.GetFileNameWithoutExtension(Name?.Value ?? "").ToLowerInvariant();
        }


        public override string ToString()
        {
            return (Name?.ToString() ?? base.ToString()) + "   " + Unknown_74h.ToString();
        }
    }



    [TC(typeof(EXP))] public class ExpressionStream : ResourceSystemBlock, IMetaXmlItem
    {
        public override long BlockLength
        {
            get { return 16 + Data1.Length + Data2.Length + Data3.Length; }
        }

        // structure data
        public MetaHash NameHash { get; set; }//presumably name hash
        public uint Data1Length { get; set; }
        public uint Data2Length { get; set; }
        public ushort Data3Length { get; set; }
        public ushort Unk0E { get; set; }//what is this? possibly max hierarchy depth
        public byte[] Data1 { get; set; }
        public byte[] Data2 { get; set; }
        public byte[] Data3 { get; set; }


        public ExpressionNodeBase[] Items { get; set; }
        public ExpressionNodeBase[] Roots { get; set; }



        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.NameHash = reader.ReadUInt32();
            this.Data1Length = reader.ReadUInt32();
            this.Data2Length = reader.ReadUInt32();
            this.Data3Length = reader.ReadUInt16();
            this.Unk0E = reader.ReadUInt16();
            this.Data1 = reader.ReadBytes((int)Data1Length);
            this.Data2 = reader.ReadBytes((int)Data2Length);
            this.Data3 = reader.ReadBytes((int)Data3Length);


            ReadItems();
            BuildHierarchy();




            #region testing
            //FlattenHierarchy();
            //WriteItems();
            //switch (Unk1)
            //{
            //    case 2:
            //    case 8:
            //    case 4:
            //    case 6:
            //    case 59:
            //    case 5:
            //    case 12:
            //    case 58:
            //    case 20:
            //    case 10:
            //    case 9:
            //    case 1:
            //    case 50:
            //    case 14:
            //    case 19:
            //    case 7:
            //    case 25:
            //    case 15:
            //    case 13:
            //    case 28:
            //    case 17:
            //    case 22:
            //    case 26:
            //    case 18:
            //    case 21:
            //    case 23:
            //    case 11:
            //    case 24:
            //    case 27:
            //    case 30:
            //    case 16:
            //    case 377:
            //    case 31:
            //    case 125:
            //    case 32:
            //    case 34:
            //    case 52:
            //    case 51:
            //    case 345:
            //    case 399:
            //        break;
            //    default:
            //        break;//no hit
            //}
            #endregion
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {

            //FlattenHierarchy();
            //WriteItems();//should already be done by Expression.UpdateStreamBuffers

            // write structure data
            writer.Write(this.NameHash);
            writer.Write(this.Data1Length);
            writer.Write(this.Data2Length);
            writer.Write(this.Data3Length);
            writer.Write(this.Unk0E);
            writer.Write(this.Data1);
            writer.Write(this.Data2);
            writer.Write(this.Data3);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.StringTag(sb, indent, "Name", YedXml.HashString(NameHash));
            YedXml.ValueTag(sb, indent, "Unk0E", Unk0E.ToString());

            //TODO: use hierarchy!
            YedXml.OpenTag(sb, indent, "Instructions");
            var cind = indent + 1;
            var cind2 = cind + 1;
            foreach (var item in Items)
            {
                if (item is ExpressionNodeEmpty)
                {
                    YedXml.SelfClosingTag(sb, cind, "Item type=\"" + item.Type.ToString() + "\"");
                }
                else
                {
                    YedXml.OpenTag(sb, cind, "Item type=\"" + item.Type.ToString() + "\"");
                    item.WriteXml(sb, cind2);
                    YedXml.CloseTag(sb, cind, "Item");
                }
            }
            YedXml.CloseTag(sb, indent, "Instructions");


        }
        public void ReadXml(XmlNode node)
        {
            NameHash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            Unk0E = (ushort)Xml.GetChildUIntAttribute(node, "Unk0E", "value");

            var items = new List<ExpressionNodeBase>();
            var instnode = node.SelectSingleNode("Instructions");
            if (instnode != null)
            {
                var inodes = instnode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    foreach (XmlNode inode in inodes)
                    {
                        if (Enum.TryParse<ExpressionNodeType>(Xml.GetStringAttribute(inode, "type"), out var type))
                        {
                            var item = CreateItem(type);
                            item.Type = type;
                            item.ReadXml(inode);
                            items.Add(item);
                        }
                    }
                }
            }
            Items = items.ToArray();
            BuildHierarchy();

        }




        public void ReadItems()
        {
            var s1 = new MemoryStream(Data1);
            var s2 = new MemoryStream(Data2);
            var s3 = new MemoryStream(Data3);
            var r1 = new DataReader(s1);
            var r2 = new DataReader(s2);
            var r3 = new DataReader(s3);

            var items = new List<ExpressionNodeBase>();
            while (s3.Position < s3.Length)
            {
                var type = (ExpressionNodeType)r3.ReadByte();
                if (type == ExpressionNodeType.None)
                {
                    if (s3.Position != s3.Length)
                    { }//no hit
                    break;
                }
                var item = CreateItem(type);
                item.Type = type;
                item.Read(r1, r2);
                items.Add(item);
            }

            if ((r1.Length - r1.Position) != 0)
            { }//no hit
            if ((r2.Length - r2.Position) != 0)
            { }//no hit
            if ((r3.Length - r3.Position) != 0)
            { }//no hit

            Items = items.ToArray();
        }

        public void BuildHierarchy()
        {
            if (Items == null) return;

            var stack = new Stack<ExpressionNodeBase>();

            for (int i = 0; i < Items.Length; i++)
            {
                var item = Items[i];

                switch (item.Type)
                {
                    case ExpressionNodeType.Unk01: break;
                    case ExpressionNodeType.Unk03: break;
                    case ExpressionNodeType.Unk04: break;
                    case ExpressionNodeType.Unk05: break;
                    case ExpressionNodeType.Unk06: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk07: break;
                    case ExpressionNodeType.Unk09: break;
                    case ExpressionNodeType.Unk0A: break;
                    case ExpressionNodeType.Unk0B: break;
                    case ExpressionNodeType.Jiggle: break;
                    case ExpressionNodeType.Unk10: item.Children = new[] { stack.Pop() }; break; //####### maybe not
                    case ExpressionNodeType.Unk11: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk1B: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk1D: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk1E: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk1F: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk20: break;//first in list
                    case ExpressionNodeType.Unk21: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk22: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk23: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk26: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk27: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk28: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk2A: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk2B: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk2C: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk2D: item.Children = new[] { stack.Pop(), stack.Pop() }; break;//4 maybe?
                    case ExpressionNodeType.Unk2E: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk2F: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk30: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk31: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk32: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk33: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk35: item.Children = new[] { stack.Pop(), stack.Pop() }; break;//can't be more than 2
                    case ExpressionNodeType.Unk36: item.Children = new[] { stack.Pop(), stack.Pop() }; break; //can't be more than 2
                    case ExpressionNodeType.Unk37: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk38: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk39: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk3A: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk3B: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break; //####### maybe not                       
                    case ExpressionNodeType.Unk3C: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk3D: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk3E: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk3F: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk40: item.Children = new[] { stack.Pop(), stack.Pop() }; break;
                    case ExpressionNodeType.Unk42: break;//first in list
                    case ExpressionNodeType.Unk43: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk44: break;
                    case ExpressionNodeType.Unk45: break;
                    case ExpressionNodeType.Unk46: item.Children = new[] { stack.Pop() }; break;
                    case ExpressionNodeType.Unk49: item.Children = new[] { stack.Pop(), stack.Pop(), stack.Pop() }; break;
                    default: break;//no hit
                }

                stack.Push(item);
            }


            Roots = stack.Reverse().ToArray();


            foreach (var item in Items) //probably using the stack caused child arrays to be reversed, so reverse them back again
            {
                if ((item.Children?.Length ?? 0) > 1)
                {
                    Array.Reverse(item.Children);
                }
            }


        }

        public void FlattenHierarchy(ExpressionNodeBase node = null, List<ExpressionNodeBase> list = null)
        {
            if (node == null)
            {
                if (Roots == null) return;

                list = new List<ExpressionNodeBase>();

                foreach (var root in Roots)
                {
                    FlattenHierarchy(root, list);
                }

                Items = list.ToArray();
            }
            else
            {
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        FlattenHierarchy(child, list);
                    }
                }
                list.Add(node);
            }
        }

        public void WriteItems()
        {
            var s1 = new MemoryStream();
            var s2 = new MemoryStream();
            var s3 = new MemoryStream();
            var w1 = new DataWriter(s1);
            var w2 = new DataWriter(s2);
            var w3 = new DataWriter(s3);

            foreach (var item in Items)
            {
                w3.Write((byte)item.Type);
                item.Write(w1, w2);
            }
            w3.Write((byte)0);

            var b1 = new byte[s1.Length];
            var b2 = new byte[s2.Length];
            var b3 = new byte[s3.Length];
            s1.Position = 0;
            s2.Position = 0;
            s3.Position = 0;
            s1.Read(b1, 0, b1.Length);
            s2.Read(b2, 0, b2.Length);
            s3.Read(b3, 0, b3.Length);

            Data1 = b1;
            Data2 = b2;
            Data3 = b3;
            Data1Length = (uint)Data1.Length;
            Data2Length = (uint)Data2.Length;
            Data3Length = (ushort)Data3.Length;
        }




        public static ExpressionNodeBase CreateItem(ExpressionNodeType type)
        {
            switch (type)
            {
                case ExpressionNodeType.Unk01:
                case ExpressionNodeType.Unk03:
                case ExpressionNodeType.Unk04:
                case ExpressionNodeType.Unk10:
                case ExpressionNodeType.Unk11:
                case ExpressionNodeType.Unk1B:
                case ExpressionNodeType.Unk1D:
                case ExpressionNodeType.Unk1E:
                case ExpressionNodeType.Unk1F:
                case ExpressionNodeType.Unk21:
                case ExpressionNodeType.Unk22:
                case ExpressionNodeType.Unk2E:
                case ExpressionNodeType.Unk2F:
                case ExpressionNodeType.Unk30:
                case ExpressionNodeType.Unk31:
                case ExpressionNodeType.Unk32:
                case ExpressionNodeType.Unk33:
                case ExpressionNodeType.Unk35:
                case ExpressionNodeType.Unk36:
                case ExpressionNodeType.Unk37:
                case ExpressionNodeType.Unk38:
                case ExpressionNodeType.Unk39:
                case ExpressionNodeType.Unk3A:
                case ExpressionNodeType.Unk3B:
                case ExpressionNodeType.Unk3C:
                case ExpressionNodeType.Unk3D:
                case ExpressionNodeType.Unk3F:
                case ExpressionNodeType.Unk40:
                case ExpressionNodeType.Unk46:
                case ExpressionNodeType.Unk49:
                    return new ExpressionNodeEmpty();

                case ExpressionNodeType.Unk44:
                case ExpressionNodeType.Unk45:
                    return new ExpressionNodeS1();

                case ExpressionNodeType.Unk06:
                case ExpressionNodeType.Unk07:
                case ExpressionNodeType.Unk09:
                case ExpressionNodeType.Unk0A:
                case ExpressionNodeType.Unk20:
                case ExpressionNodeType.Unk23:
                case ExpressionNodeType.Unk26:
                case ExpressionNodeType.Unk27: 
                case ExpressionNodeType.Unk28:
                case ExpressionNodeType.Unk2A:
                    return new ExpressionNodeS2();

                case ExpressionNodeType.Unk42:
                case ExpressionNodeType.Unk43:
                    return new ExpressionNodeS3();

                case ExpressionNodeType.Unk2B:
                case ExpressionNodeType.Unk2C:
                case ExpressionNodeType.Unk2D:
                    return new ExpressionNode2B();


                case ExpressionNodeType.Unk05: return new ExpressionNode05();
                case ExpressionNodeType.Unk0B: return new ExpressionNode0B();
                case ExpressionNodeType.Jiggle: return new ExpressionNodeJiggle();
                case ExpressionNodeType.Unk3E: return new ExpressionNode3E();

            }
            return new ExpressionNodeBase();
        }


        public override string ToString()
        {
            return NameHash.ToString() + " (" + (Items?.Length??0).ToString() + " items, " + (Roots?.Length??0).ToString() + " roots)";
        }

    }


    public enum ExpressionNodeType : byte
    {
        None = 0,
        Unk01 = 0x01,
        Unk03 = 0x03,
        Unk04 = 0x04,
        Unk05 = 0x05,
        Unk06 = 0x06,
        Unk07 = 0x07,
        Unk09 = 0x09,
        Unk0A = 0x0A,
        Unk0B = 0x0B,
        Jiggle = 0x0E,
        Unk10 = 0x10,
        Unk11 = 0x11,
        Unk1B = 0x1B,
        Unk1D = 0x1D,
        Unk1E = 0x1E,
        Unk1F = 0x1F,
        Unk20 = 0x20,
        Unk21 = 0x21,
        Unk22 = 0x22,
        Unk23 = 0x23,
        Unk26 = 0x26,
        Unk27 = 0x27,
        Unk28 = 0x28,
        Unk2A = 0x2A,
        Unk2B = 0x2B,
        Unk2C = 0x2C,
        Unk2D = 0x2D,
        Unk2E = 0x2E,
        Unk2F = 0x2F,
        Unk30 = 0x30,
        Unk31 = 0x31,
        Unk32 = 0x32,
        Unk33 = 0x33,
        Unk35 = 0x35,
        Unk36 = 0x36,
        Unk37 = 0x37,
        Unk38 = 0x38,
        Unk39 = 0x39,
        Unk3A = 0x3A,
        Unk3B = 0x3B,
        Unk3C = 0x3C,
        Unk3D = 0x3D,
        Unk3E = 0x3E,
        Unk3F = 0x3F,
        Unk40 = 0x40,
        Unk42 = 0x42,
        Unk43 = 0x43,
        Unk44 = 0x44,
        Unk45 = 0x45,
        Unk46 = 0x46,
        Unk49 = 0x49,
    }


    [TC(typeof(EXP))] public class ExpressionNodeBase
    {
    
        /*
    possible type names:
crExprInstrBone
crExprInstrRBFs
crExprInstrSpring
crExprInstrConstraint
crExprInstrLinear
crExprInstrSpringChain
crExprInstrCurve
crExprInstrFrame
crExprInstrLookAt
crExprInstrVariable
        */


        public ExpressionNodeType Type { get; set; }

        public ExpressionNodeBase[] Children { get; set; }

        public virtual void Read(DataReader r1, DataReader r2)
        { }
        public virtual void Write(DataWriter w1, DataWriter w2)
        { }
        public virtual void WriteXml(StringBuilder sb, int indent)
        { }
        public virtual void ReadXml(XmlNode node)
        { }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionNodeEmpty : ExpressionNodeBase
    { }
    [TC(typeof(EXP))] public class ExpressionNodeS1 : ExpressionNodeBase
    {
        [TC(typeof(EXP))] public class ItemStruct : IMetaXmlItem
        {
            public ushort BoneTrack { get; set; }
            public ushort Unk2 { get; set; }
            public float[] Values { get; set; }

            public void Read(DataReader r)
            {
                BoneTrack = r.ReadUInt16();
                Unk2 = r.ReadUInt16();

                switch (Unk2)
                {
                    case 0:
                    case 4:
                    case 8:
                        break;
                    default:
                        break;//no hit
                }
            }
            public void Write(DataWriter w)
            {
                w.Write(BoneTrack);
                w.Write(Unk2);
            }
            public void WriteXml(StringBuilder sb, int indent)
            {
                YedXml.ValueTag(sb, indent, "BoneTrack", BoneTrack.ToString());
                YedXml.ValueTag(sb, indent, "Unk2", Unk2.ToString());
                YedXml.WriteRawArray(sb, Values, indent, "Values", "", FloatUtil.ToString, Values.Length);
            }
            public void ReadXml(XmlNode node)
            {
                BoneTrack = (ushort)Xml.GetChildUIntAttribute(node, "BoneTrack", "value");
                Unk2 = (ushort)Xml.GetChildUIntAttribute(node, "Unk2", "value");
                Values = Xml.GetChildRawFloatArray(node, "Values");
            }

            public override string ToString()
            {
                var str = BoneTrack.ToString() + ", " + Unk2.ToString();
                if (Values != null)
                {
                    str += "   -   ";
                    for (int i = 0; i < Values.Length; i++)
                    {
                        if (i != 0) str += ", ";
                        str += Values[i].ToString();
                    }
                }
                return str;
            }
        }

        public uint Length { get; set; }//updated automatically
        public uint ItemCount { get; set; }//updated automatically
        public uint ItemType { get; set; }
        public uint Unk1 { get; set; } // 0x00000000
        public ItemStruct[] Items { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Length = r1.ReadUInt32();
            ItemCount = r1.ReadUInt32();
            ItemType = r1.ReadUInt32();
            Unk1 = r1.ReadUInt32();

            var valcnt = (ItemType - 1) * 9 + 6;
            var hlen = ItemCount * 4 + 16;
            var dlen = Length - hlen;
            var tlen = ItemCount * valcnt * 4;
            if (tlen != dlen)
            { }//no hitting


            Items = new ItemStruct[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                var item1 = new ItemStruct();
                item1.Read(r1);
                Items[i] = item1;
                Items[i].Values = new float[valcnt];
            }
            for (int n = 0; n < valcnt; n++)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    Items[i].Values[n] = r1.ReadSingle();
                }
            }


            switch (ItemCount)
            {
                case 0:
                case 4:
                case 8:
                case 12:
                case 20:
                case 24:
                case 48:
                case 52:
                case 76:
                case 32:
                case 16:
                case 40:
                case 44:
                case 60:
                case 72:
                case 56:
                case 28:
                case 68:
                    break;
                default:
                    break;//no hit
            }
            switch (ItemType)
            {
                case 1: break;
                case 2: break;
                case 3: break;
                case 4: break;
                default:
                    break;//no hit
            }
            if (Unk1 != 0)
            { }//no hit
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            ItemCount = (uint)(Items?.Length ?? 0);
            ItemType = Math.Max(ItemType, 1);
            var valcnt = (ItemType - 1) * 9 + 6;
            var hlen = ItemCount * 4 + 16;
            var tlen = ItemCount * valcnt * 4;
            Length = hlen + tlen;

            w1.Write(Length);
            w1.Write(ItemCount);
            w1.Write(ItemType);
            w1.Write(Unk1);

            for (int i = 0; i < ItemCount; i++)
            {
                Items[i].Write(w1);
            }
            for (int n = 0; n < valcnt; n++)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    var vals = Items[i].Values;
                    w1.Write((n < (vals?.Length ?? 0)) ? vals[n] : 0.0f);
                }
            }

        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "ItemType", ItemType.ToString());
            YedXml.WriteItemArray(sb, Items, indent, "Items");
        }
        public override void ReadXml(XmlNode node)
        {
            ItemType = Xml.GetChildUIntAttribute(node, "ItemType", "value");
            Items = XmlMeta.ReadItemArray<ItemStruct>(node, "Items");
        }

        public override string ToString()
        {
            var str = base.ToString() + "   -   " + Length.ToString() + ", " + ItemCount.ToString() + ", " + ItemType.ToString();
            if (Items != null)
            {
                str += " (S1)   -   " + Items.Length.ToString() + " items";
            }
            return str;
        }
    }
    [TC(typeof(EXP))] public class ExpressionNodeS2 : ExpressionNodeBase
    {
        public ushort BoneTrack { get; set; } //index of the BoneTag in the Expression.BoneTracks array
        public ushort BoneTag { get; set; }
        public byte Unk01 { get; set; }
        public byte Unk02 { get; set; }
        public byte Unk03 { get; set; }
        public byte Unk04 { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            BoneTrack = r2.ReadUInt16();
            BoneTag = r2.ReadUInt16();
            Unk01 = r2.ReadByte();
            Unk02 = r2.ReadByte();
            Unk03 = r2.ReadByte();
            Unk04 = r2.ReadByte();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(BoneTrack);
            w2.Write(BoneTag);
            w2.Write(Unk01);
            w2.Write(Unk02);
            w2.Write(Unk03);
            w2.Write(Unk04);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "BoneTrack", BoneTrack.ToString());
            YedXml.ValueTag(sb, indent, "BoneTag", BoneTag.ToString());
            YedXml.ValueTag(sb, indent, "Unk01", Unk01.ToString());
            YedXml.ValueTag(sb, indent, "Unk02", Unk02.ToString());
            YedXml.ValueTag(sb, indent, "Unk03", Unk03.ToString());
            YedXml.ValueTag(sb, indent, "Unk04", Unk04.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            BoneTrack = (ushort)Xml.GetChildUIntAttribute(node, "BoneTrack", "value");
            BoneTag = (ushort)Xml.GetChildUIntAttribute(node, "BoneTag", "value");
            Unk01 = (byte)Xml.GetChildUIntAttribute(node, "Unk01", "value");
            Unk02 = (byte)Xml.GetChildUIntAttribute(node, "Unk02", "value");
            Unk03 = (byte)Xml.GetChildUIntAttribute(node, "Unk03", "value");
            Unk04 = (byte)Xml.GetChildUIntAttribute(node, "Unk04", "value");
        }

        public override string ToString()
        {
            return base.ToString() + " (S2)   -   Track:" + BoneTrack.ToString() + ", Tag:" + BoneTag.ToString() + ", " + Unk01.ToString() + ", " + Unk02.ToString() + ", " + Unk03.ToString() + ", " + Unk04.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionNodeS3 : ExpressionNodeBase
    {
        public MetaHash Hash { get; set; }
        public byte HashIndex { get; set; } //index of the hash in the Expression.ItemHashes array (autoupdated - don't need in XML)
        public byte Unk03 { get; set; }//0
        public byte Unk04 { get; set; }//0
        public byte Unk05 { get; set; }//0

        public override void Read(DataReader r1, DataReader r2)
        {
            Hash = r2.ReadUInt32();
            HashIndex = r2.ReadByte();
            Unk03 = r2.ReadByte();
            Unk04 = r2.ReadByte();
            Unk05 = r2.ReadByte();

            if (Unk03 != 0)
            { }
            if (Unk04 != 0)
            { }
            if (Unk05 != 0)
            { }
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(Hash);
            w2.Write(HashIndex);
            w2.Write(Unk03);
            w2.Write(Unk04);
            w2.Write(Unk05);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.StringTag(sb, indent, "Hash", YedXml.HashString(Hash));
        }
        public override void ReadXml(XmlNode node)
        {
            Hash = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Hash"));
        }

        public override string ToString()
        {
            return base.ToString() + " (S3)   -   Hash:" + Hash.ToString() + ", " + HashIndex.ToString() + ", " + Unk03.ToString() + ", " + Unk04.ToString() + ", " + Unk05.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionNode2B : ExpressionNodeBase
    {
        public uint Unk01 { get; set; }
        public uint Unk02 { get; set; }
        public uint Unk03 { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Unk01 = r2.ReadUInt32();
            Unk02 = r2.ReadUInt32();
            Unk03 = r2.ReadUInt32();

            switch (Unk01)//flags?
            {
                case 0:
                case 128:
                case 464:
                case 352:
                case 240:
                case 16:
                    break;
                default:
                    break;//no hit
            }
            switch (Unk02)//some index?
            {
                case 24:
                case 12:
                case 0:
                case 8:
                case 60:
                case 48:
                case 20:
                case 44:
                case 16:
                case 52:
                case 28:
                case 4:
                case 88:
                case 64:
                case 128:
                case 104:
                case 68:
                case 80:
                case 92:
                case 84:
                case 56:
                case 108:
                case 116:
                case 72:
                case 32:
                case 40:
                case 512:
                case 36:
                case 100:
                case 140:
                    break;
                default:
                    break;//no hit
            }
            switch (Unk03)//some index?
            {
                case 5:
                case 4:
                case 2:
                case 3:
                case 6:
                case 17:
                case 16:
                case 15:
                case 14:
                case 12:
                case 7:
                case 19:
                case 34:
                case 26:
                case 22:
                case 18:
                case 11:
                case 20:
                case 8:
                case 30:
                case 9:
                case 10:
                case 24:
                case 77:
                case 38:
                    break;
                default:
                    break;//no hit
            }

        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(Unk01);
            w2.Write(Unk02);
            w2.Write(Unk03);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "Unk01", Unk01.ToString());
            YedXml.ValueTag(sb, indent, "Unk02", Unk02.ToString());
            YedXml.ValueTag(sb, indent, "Unk03", Unk03.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            Unk01 = Xml.GetChildUIntAttribute(node, "Unk01", "value");
            Unk02 = Xml.GetChildUIntAttribute(node, "Unk02", "value");
            Unk03 = Xml.GetChildUIntAttribute(node, "Unk03", "value");
        }

        public override string ToString()
        {
            return base.ToString() + " (S2B)   -   " + Unk01.ToString() + ", " + Unk02.ToString() + ", " + Unk03.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionNode05 : ExpressionNodeBase
    {
        public float Unk01 { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Unk01 = r2.ReadSingle();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(Unk01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "Unk01", FloatUtil.ToString(Unk01));
        }
        public override void ReadXml(XmlNode node)
        {
            Unk01 = Xml.GetChildFloatAttribute(node, "Unk01", "value");
        }

        public override string ToString()
        {
            return base.ToString() + " (S05)   -   " + Unk01.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionNode0B : ExpressionNodeBase
    {
        public Vector4 Unk01 { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Unk01 = r1.ReadVector4();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w1.Write(Unk01);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.SelfClosingTag(sb, indent, "Unk01 " + FloatUtil.GetVector4XmlString(Unk01));
        }
        public override void ReadXml(XmlNode node)
        {
            Unk01 = Xml.GetChildVector4Attributes(node, "Unk01");
        }

        public override string ToString()
        {
            var str = base.ToString() + " (S0B)   -   " + Unk01.ToString();
            return str;
        }
    }
    [TC(typeof(EXP))] public class ExpressionNodeJiggle : ExpressionNodeBase
    {
        public ExpressionJiggleData JiggleData { get; set; }
        public uint BoneTrackRot { get; set; }
        public uint BoneTrackPos { get; set; }
        public uint UnkUint13 { get; set; }//0
        public uint UnkUint14 { get; set; }//0

        public override void Read(DataReader r1, DataReader r2)
        {
            JiggleData = new ExpressionJiggleData();
            JiggleData.Read(r1);
            BoneTrackRot = r1.ReadUInt32();
            BoneTrackPos = r1.ReadUInt32();
            UnkUint13 = r1.ReadUInt32();
            UnkUint14 = r1.ReadUInt32();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            if (JiggleData == null) JiggleData = new ExpressionJiggleData();
            JiggleData.Write(w1);
            w1.Write(BoneTrackRot);
            w1.Write(BoneTrackPos);
            w1.Write(UnkUint13);
            w1.Write(UnkUint14);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            if (JiggleData == null) JiggleData = new ExpressionJiggleData();
            JiggleData.WriteXml(sb, indent);
            YedXml.ValueTag(sb, indent, "BoneTrackRot", BoneTrackRot.ToString());
            YedXml.ValueTag(sb, indent, "BoneTrackPos", BoneTrackPos.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            JiggleData = new ExpressionJiggleData();
            JiggleData.ReadXml(node);
            BoneTrackRot = Xml.GetChildUIntAttribute(node, "BoneTrackRot", "value");
            BoneTrackPos = Xml.GetChildUIntAttribute(node, "BoneTrackPos", "value");
        }

        public override string ToString()
        {
            return base.ToString() + "   -   " + BoneTrackRot.ToString() + ", " + BoneTrackPos.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionNode3E : ExpressionNodeBase
    {
        public Vector4 Unk01 { get; set; }
        public uint Unk02 { get; set; } // 0, 1, 2
        public uint Unk03 { get; set; } // 0, 2
        public uint Unk04 { get; set; } // 0, 2
        public uint Unk05 { get; set; } // 0x00000000

        public override void Read(DataReader r1, DataReader r2)
        {
            Unk01 = r1.ReadVector4();
            Unk02 = r1.ReadUInt32();
            Unk03 = r1.ReadUInt32();
            Unk04 = r1.ReadUInt32();
            Unk05 = r1.ReadUInt32();

            switch (Unk02)
            {
                case 0:
                case 2:
                case 1:
                    break;
                default:
                    break;//no hit
            }
            switch (Unk03)
            {
                case 2:
                case 0:
                    break;
                default:
                    break;//no hit
            }
            switch (Unk04)
            {
                case 2:
                case 0:
                    break;
                default:
                    break;//no hit
            }
            switch (Unk05)
            {
                case 0:
                    break;
                default:
                    break;//no hit
            }
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w1.Write(Unk01);
            w1.Write(Unk02);
            w1.Write(Unk03);
            w1.Write(Unk04);
            w1.Write(Unk05);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.SelfClosingTag(sb, indent, "Unk01 " + FloatUtil.GetVector4XmlString(Unk01));
            YedXml.ValueTag(sb, indent, "Unk02", Unk02.ToString());
            YedXml.ValueTag(sb, indent, "Unk03", Unk03.ToString());
            YedXml.ValueTag(sb, indent, "Unk04", Unk04.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            Unk01 = Xml.GetChildVector4Attributes(node, "Unk01");
            Unk02 = Xml.GetChildUIntAttribute(node, "Unk02", "value");
            Unk03 = Xml.GetChildUIntAttribute(node, "Unk03", "value");
            Unk04 = Xml.GetChildUIntAttribute(node, "Unk04", "value");
        }

        public override string ToString()
        {
            return base.ToString() + " (S3E)   -   " + Unk01.ToString() + "   -   " + Unk02.ToString() + ", " + Unk03.ToString() + ", " + Unk04.ToString() + ", " + Unk05.ToString();
        }
    }




    [TC(typeof(EXP))] public class ExpressionJiggleBlock : ResourceSystemBlock
    {
        public override long BlockLength => 0xA0;

        public ExpressionJiggleData JiggleData { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            JiggleData = new ExpressionJiggleData();
            JiggleData.Read(reader);
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (JiggleData == null) JiggleData = new ExpressionJiggleData();
            JiggleData.Write(writer);
        }

        public override string ToString()
        {
            return JiggleData?.ToString() ?? base.ToString();
        }
    }



    [TC(typeof(EXP))] public class ExpressionJiggleData
    {
        public Vector3 Vector01 { get; set; }
        public ushort Ushort01a { get; set; }
        public ushort Ushort01b { get; set; }
        public Vector3 Vector02 { get; set; }
        public ushort Ushort02a { get; set; }
        public ushort Ushort02b { get; set; }
        public Vector3 Vector03 { get; set; }
        public ushort Ushort03a { get; set; }
        public ushort Ushort03b { get; set; }
        public Vector3 Vector04 { get; set; }
        public ushort Ushort04a { get; set; }
        public ushort Ushort04b { get; set; }
        public Vector3 Vector05 { get; set; }
        public ushort Ushort05a { get; set; }
        public ushort Ushort05b { get; set; }
        public Vector3 Vector06 { get; set; }
        public ushort Ushort06a { get; set; }
        public ushort Ushort06b { get; set; }
        public Vector3 Vector07 { get; set; }
        public ushort Ushort07a { get; set; }
        public ushort Ushort07b { get; set; }
        public Vector3 Vector08 { get; set; }
        public ushort Ushort08a { get; set; }
        public ushort Ushort08b { get; set; }
        public Vector3 Vector09 { get; set; }
        public ushort Ushort09a { get; set; }
        public ushort Ushort09b { get; set; }
        public Vector3 Gravity { get; set; }
        public ushort BoneTag { get; set; }
        public ushort Ushort10b { get; set; }//0

        public void Read(DataReader r)
        {
            Vector01 = r.ReadVector3();
            Ushort01a = r.ReadUInt16();
            Ushort01b = r.ReadUInt16();
            Vector02 = r.ReadVector3();
            Ushort02a = r.ReadUInt16();
            Ushort02b = r.ReadUInt16();
            Vector03 = r.ReadVector3();
            Ushort03a = r.ReadUInt16();
            Ushort03b = r.ReadUInt16();
            Vector04 = r.ReadVector3();
            Ushort04a = r.ReadUInt16();
            Ushort04b = r.ReadUInt16();
            Vector05 = r.ReadVector3();
            Ushort05a = r.ReadUInt16();
            Ushort05b = r.ReadUInt16();
            Vector06 = r.ReadVector3();
            Ushort06a = r.ReadUInt16();
            Ushort06b = r.ReadUInt16();
            Vector07 = r.ReadVector3();
            Ushort07a = r.ReadUInt16();
            Ushort07b = r.ReadUInt16();
            Vector08 = r.ReadVector3();
            Ushort08a = r.ReadUInt16();
            Ushort08b = r.ReadUInt16();
            Vector09 = r.ReadVector3();
            Ushort09a = r.ReadUInt16();
            Ushort09b = r.ReadUInt16();
            Gravity = r.ReadVector3();
            BoneTag = r.ReadUInt16();
            Ushort10b = r.ReadUInt16();

            //if (Ushort10b != 0)
            //{ }//no hit
        }
        public void Write(DataWriter w)
        {
            w.Write(Vector01);
            w.Write(Ushort01a);
            w.Write(Ushort01b);
            w.Write(Vector02);
            w.Write(Ushort02a);
            w.Write(Ushort02b);
            w.Write(Vector03);
            w.Write(Ushort03a);
            w.Write(Ushort03b);
            w.Write(Vector04);
            w.Write(Ushort04a);
            w.Write(Ushort04b);
            w.Write(Vector05);
            w.Write(Ushort05a);
            w.Write(Ushort05b);
            w.Write(Vector06);
            w.Write(Ushort06a);
            w.Write(Ushort06b);
            w.Write(Vector07);
            w.Write(Ushort07a);
            w.Write(Ushort07b);
            w.Write(Vector08);
            w.Write(Ushort08a);
            w.Write(Ushort08b);
            w.Write(Vector09);
            w.Write(Ushort09a);
            w.Write(Ushort09b);
            w.Write(Gravity);
            w.Write(BoneTag);
            w.Write(Ushort10b);
        }
        public void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.SelfClosingTag(sb, indent, "Vector01 " + FloatUtil.GetVector3XmlString(Vector01));
            YedXml.SelfClosingTag(sb, indent, "Vector02 " + FloatUtil.GetVector3XmlString(Vector02));
            YedXml.SelfClosingTag(sb, indent, "Vector03 " + FloatUtil.GetVector3XmlString(Vector03));
            YedXml.SelfClosingTag(sb, indent, "Vector04 " + FloatUtil.GetVector3XmlString(Vector04));
            YedXml.SelfClosingTag(sb, indent, "Vector05 " + FloatUtil.GetVector3XmlString(Vector05));
            YedXml.SelfClosingTag(sb, indent, "Vector06 " + FloatUtil.GetVector3XmlString(Vector06));
            YedXml.SelfClosingTag(sb, indent, "Vector07 " + FloatUtil.GetVector3XmlString(Vector07));
            YedXml.SelfClosingTag(sb, indent, "Vector08 " + FloatUtil.GetVector3XmlString(Vector08));
            YedXml.SelfClosingTag(sb, indent, "Vector09 " + FloatUtil.GetVector3XmlString(Vector09));
            YedXml.SelfClosingTag(sb, indent, "Gravity " + FloatUtil.GetVector3XmlString(Gravity));
            YedXml.ValueTag(sb, indent, "Ushort01a", Ushort01a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort01b", Ushort01b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort02a", Ushort02a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort02b", Ushort02b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort03a", Ushort03a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort03b", Ushort03b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort04a", Ushort04a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort04b", Ushort04b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort05a", Ushort05a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort05b", Ushort05b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort06a", Ushort06a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort06b", Ushort06b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort07a", Ushort07a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort07b", Ushort07b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort08a", Ushort08a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort08b", Ushort08b.ToString());
            YedXml.ValueTag(sb, indent, "Ushort09a", Ushort09a.ToString());
            YedXml.ValueTag(sb, indent, "Ushort09b", Ushort09b.ToString());
            YedXml.ValueTag(sb, indent, "BoneTag", BoneTag.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            Vector01 = Xml.GetChildVector3Attributes(node, "Vector01");
            Vector02 = Xml.GetChildVector3Attributes(node, "Vector02");
            Vector03 = Xml.GetChildVector3Attributes(node, "Vector03");
            Vector04 = Xml.GetChildVector3Attributes(node, "Vector04");
            Vector05 = Xml.GetChildVector3Attributes(node, "Vector05");
            Vector06 = Xml.GetChildVector3Attributes(node, "Vector06");
            Vector07 = Xml.GetChildVector3Attributes(node, "Vector07");
            Vector08 = Xml.GetChildVector3Attributes(node, "Vector08");
            Vector09 = Xml.GetChildVector3Attributes(node, "Vector09");
            Gravity = Xml.GetChildVector3Attributes(node, "Gravity");
            Ushort01a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort01a", "value");
            Ushort01b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort01b", "value");
            Ushort02a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort02a", "value");
            Ushort02b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort02b", "value");
            Ushort03a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort03a", "value");
            Ushort03b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort03b", "value");
            Ushort04a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort04a", "value");
            Ushort04b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort04b", "value");
            Ushort05a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort05a", "value");
            Ushort05b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort05b", "value");
            Ushort06a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort06a", "value");
            Ushort06b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort06b", "value");
            Ushort07a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort07a", "value");
            Ushort07b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort07b", "value");
            Ushort08a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort08a", "value");
            Ushort08b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort08b", "value");
            Ushort09a = (ushort)Xml.GetChildUIntAttribute(node, "Ushort09a", "value");
            Ushort09b = (ushort)Xml.GetChildUIntAttribute(node, "Ushort09b", "value");
            BoneTag = (ushort)Xml.GetChildUIntAttribute(node, "BoneTag", "value");
        }

        public bool Compare(ExpressionJiggleData o)
        {
            if (o.Vector01 !=  Vector01) return false;
            if (o.Ushort01a != Ushort01a) return false;
            if (o.Ushort01b != Ushort01b) return false;
            if (o.Vector02 !=  Vector02) return false;
            if (o.Ushort02a != Ushort02a) return false;
            if (o.Ushort02b != Ushort02b) return false;
            if (o.Vector03 !=  Vector03) return false;
            if (o.Ushort03a != Ushort03a) return false;
            if (o.Ushort03b != Ushort03b) return false;
            if (o.Vector04 !=  Vector04) return false;
            if (o.Ushort04a != Ushort04a) return false;
            if (o.Ushort04b != Ushort04b) return false;
            if (o.Vector05 !=  Vector05) return false;
            if (o.Ushort05a != Ushort05a) return false;
            if (o.Ushort05b != Ushort05b) return false;
            if (o.Vector06 !=  Vector06) return false;
            if (o.Ushort06a != Ushort06a) return false;
            if (o.Ushort06b != Ushort06b) return false;
            if (o.Vector07 !=  Vector07) return false;
            if (o.Ushort07a != Ushort07a) return false;
            if (o.Ushort07b != Ushort07b) return false;
            if (o.Vector08 !=  Vector08) return false;
            if (o.Ushort08a != Ushort08a) return false;
            if (o.Ushort08b != Ushort08b) return false;
            if (o.Vector09 !=  Vector09) return false;
            if (o.Ushort09a != Ushort09a) return false;
            if (o.Ushort09b != Ushort09b) return false;
            if (o.Gravity != Gravity) return false;
            if (o.BoneTag != BoneTag) return false;
            if (o.Ushort10b != Ushort10b) return false;
            return true;
        }
        public ExpressionJiggleData Clone()
        {
            var n = new ExpressionJiggleData();
            n.Vector01 =  Vector01;
            n.Ushort01a = Ushort01a;
            n.Ushort01b = Ushort01b;
            n.Vector02 =  Vector02;
            n.Ushort02a = Ushort02a;
            n.Ushort02b = Ushort02b;
            n.Vector03 =  Vector03;
            n.Ushort03a = Ushort03a;
            n.Ushort03b = Ushort03b;
            n.Vector04 =  Vector04;
            n.Ushort04a = Ushort04a;
            n.Ushort04b = Ushort04b;
            n.Vector05 =  Vector05;
            n.Ushort05a = Ushort05a;
            n.Ushort05b = Ushort05b;
            n.Vector06 =  Vector06;
            n.Ushort06a = Ushort06a;
            n.Ushort06b = Ushort06b;
            n.Vector07 =  Vector07;
            n.Ushort07a = Ushort07a;
            n.Ushort07b = Ushort07b;
            n.Vector08 =  Vector08;
            n.Ushort08a = Ushort08a;
            n.Ushort08b = Ushort08b;
            n.Vector09 =  Vector09;
            n.Ushort09a = Ushort09a;
            n.Ushort09b = Ushort09b;
            n.Gravity =  Gravity;
            n.BoneTag = BoneTag;
            n.Ushort10b = Ushort10b;
            return n;
        }

        public override string ToString()
        {
            return BoneTag.ToString();
        }
    }


    [TC(typeof(EXP))] public struct ExpressionBoneTrack : IMetaXmlItem
    {
        public ushort BoneTag { get; set; }
        public byte Track { get; set; }
        public byte Flags { get; set; }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "BoneTag", BoneTag.ToString());
            YedXml.ValueTag(sb, indent, "Track", Track.ToString());
            YedXml.ValueTag(sb, indent, "Flags", Flags.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            BoneTag = (ushort)Xml.GetChildUIntAttribute(node, "BoneTag", "value");
            Track = (byte)Xml.GetChildUIntAttribute(node, "Track", "value");
            Flags = (byte)Xml.GetChildUIntAttribute(node, "Flags", "value");
        }

        public override string ToString()
        {
            return BoneTag.ToString() + ", " + Track.ToString() + ", " + Flags.ToString();
        }
    }


}
