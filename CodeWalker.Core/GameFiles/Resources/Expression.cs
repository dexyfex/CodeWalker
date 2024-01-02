﻿using SharpDX;
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
        public ResourceSimpleList64_s<ExpressionTrack> Tracks { get; set; } // bone tags / animation tracks
        public ResourceSimpleList64<ExpressionSpringDescriptionBlock> Springs { get; set; } //compiled list of spring data from all DefineSpring Stream instructions
        public ResourceSimpleList64_s<MetaHash> Variables { get; set; }
        public ulong NamePointer { get; set; }
        public ushort NameLength { get; set; } // name len
        public ushort NameCapacity { get; set; } // name len+1
        public uint Unknown_6Ch { get; set; } // 0x00000000
        public uint Unknown_70h { get; set; } = 1;
        public uint Signature { get; set; }
        public uint MaxStreamSize { get; set; } // max length of any item in Streams
        public uint Unknown_7Ch { get; set; } // 3 or 2
        public uint Unknown_80h { get; set; } // 0x00000000
        public uint Unknown_84h { get; set; } // 0x00000000
        public uint Unknown_88h { get; set; } // 0x00000000
        public uint Unknown_8Ch { get; set; } // 0x00000000

        // reference data
        public string_r Name { get; set; }
        public MetaHash NameHash { get; set; }

        public Dictionary<ExpressionTrack, ExpressionTrack> BoneTracksDict { get; set; }


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
            this.Tracks = reader.ReadBlock<ResourceSimpleList64_s<ExpressionTrack>>();
            this.Springs = reader.ReadBlock<ResourceSimpleList64<ExpressionSpringDescriptionBlock>>();
            this.Variables = reader.ReadBlock<ResourceSimpleList64_s<MetaHash>>();
            this.NamePointer = reader.ReadUInt64();
            this.NameLength = reader.ReadUInt16();
            this.NameCapacity = reader.ReadUInt16();
            this.Unknown_6Ch = reader.ReadUInt32();
            this.Unknown_70h = reader.ReadUInt32();
            this.Signature = reader.ReadUInt32();
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
            writer.WriteBlock(this.Tracks);
            writer.WriteBlock(this.Springs);
            writer.WriteBlock(this.Variables);
            writer.Write(this.NamePointer);
            writer.Write(this.NameLength);
            writer.Write(this.NameCapacity);
            writer.Write(this.Unknown_6Ch);
            writer.Write(this.Unknown_70h);
            writer.Write(this.Signature);
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
            YedXml.ValueTag(sb, indent, "Signature", Signature.ToString()); // TODO: calculate signature
            YedXml.ValueTag(sb, indent, "Unk7C", Unknown_7Ch.ToString());

            if ((Tracks?.data_items?.Length ?? 0) > 0)
            {
                YedXml.WriteItemArray(sb, Tracks.data_items, indent, "Tracks");
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
            Signature = Xml.GetChildUIntAttribute(node, "Signature");// TODO: calculate signature
            Unknown_7Ch = Xml.GetChildUIntAttribute(node, "Unk7C");

            Tracks = new ResourceSimpleList64_s<ExpressionTrack>();
            Tracks.data_items = XmlMeta.ReadItemArray<ExpressionTrack>(node, "Tracks");

            Streams = new ResourcePointerList64<ExpressionStream>();
            Streams.data_items = XmlMeta.ReadItemArray<ExpressionStream>(node, "Streams");

            BuildBoneTracksDict();
            BuildSpringsList();
            UpdateVariables();
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
                new Tuple<long, IResourceBlock>(0x30, Tracks),
                new Tuple<long, IResourceBlock>(0x40, Springs),
                new Tuple<long, IResourceBlock>(0x50, Variables)
            };
        }


        public void BuildBoneTracksDict()
        {
            BoneTracksDict = new Dictionary<ExpressionTrack, ExpressionTrack>();

            if (Tracks?.data_items == null) return;

            var mapto = new ExpressionTrack();
            for(int i=0; i< Tracks.data_items.Length;i++)
            {
                var bt = Tracks.data_items[i];
                if ((bt.Flags & 128) == 0)
                {
                    mapto = bt;
                }
                else if (bt.BoneId != 0)
                {
                    bt.Flags &= 0x7F;
                    BoneTracksDict[bt] = mapto;
                }
            }

        }

        public void BuildSpringsList()
        {
            var springs = new List<ExpressionSpringDescriptionBlock>();
            if (Streams?.data_items != null)
            {
                foreach (var stream in Streams.data_items)
                {
                    foreach (var node in stream.Instructions)
                    {
                        if (node is ExpressionInstrSpring instr)
                        {
                            var spring = new ExpressionSpringDescriptionBlock();
                            spring.Spring = instr.SpringDescription.Clone();
                            springs.Add(spring);
                        }
                    }
                }
            }
            Springs = new ResourceSimpleList64<ExpressionSpringDescriptionBlock>();
            Springs.data_items = springs.ToArray();
        }

        public void UpdateVariables()
        {
            var dict = new Dictionary<MetaHash, uint>();
            if (Streams?.data_items != null)
            {
                foreach (var stream in Streams.data_items)
                {
                    foreach (var instr in stream.Instructions)
                    {
                        if (instr is ExpressionInstrVariable instrVar)
                        {
                            dict[instrVar.Variable] = 0;
                        }
                    }
                }
            }
            var list = dict.Keys.ToList();
            list.Sort((a, b) => a.Hash.CompareTo(b.Hash));
            for (int i = 0; i < list.Count; i++)
            {
                dict[list[i]] = (uint)i;
            }
            if (Streams?.data_items != null)
            {
                foreach (var stream in Streams.data_items)
                {
                    foreach (var item in stream.Instructions)
                    {
                        if (item is ExpressionInstrVariable s3)
                        {
                            var index = dict[s3.Variable];
                            s3.VariableIndex = index;
                        }
                    }
                }
            }

            Variables = new ResourceSimpleList64_s<MetaHash>();
            Variables.data_items = list.ToArray();

        }

        public void UpdateStreamBuffers()
        {
            MaxStreamSize = 0;
            if (Streams?.data_items != null)
            {
                foreach (var item in Streams.data_items)
                {
                    item.WriteInstructions(); //makes sure the data buffers are updated to the correct length!
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
            return Name?.ToString() ?? base.ToString();
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


        public ExpressionInstrBase[] Instructions { get; set; }



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


            ReadInstructions();
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
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

            YedXml.OpenTag(sb, indent, "Instructions");
            var cind = indent + 1;
            var cind2 = cind + 1;
            foreach (var item in Instructions)
            {
                if (item is ExpressionInstrEmpty)
                {
                    YedXml.SelfClosingTag(sb, cind, "Item type=\"" + item.Type + "\"");
                }
                else
                {
                    YedXml.OpenTag(sb, cind, "Item type=\"" + item.Type + "\"");
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

            var items = new List<ExpressionInstrBase>();
            var instnode = node.SelectSingleNode("Instructions");
            if (instnode != null)
            {
                var inodes = instnode.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    foreach (XmlNode inode in inodes)
                    {
                        if (Enum.TryParse<ExpressionInstrType>(Xml.GetStringAttribute(inode, "type"), out var type))
                        {
                            var item = CreateInstruction(type);
                            item.Type = type;
                            item.ReadXml(inode);
                            items.Add(item);
                        }
                    }
                }
            }
            Instructions = items.ToArray();
        }

        public void ReadInstructions()
        {
            var s1 = new MemoryStream(Data1);
            var s2 = new MemoryStream(Data2);
            var s3 = new MemoryStream(Data3);
            var r1 = new DataReader(s1);
            var r2 = new DataReader(s2);
            var r3 = new DataReader(s3);

            var unresolvedJumps = new Dictionary<(long, long, long), List<ExpressionInstrJump>>();
            var instructions = new List<ExpressionInstrBase>();
            while (s3.Position < s3.Length)
            {
                var loc = (s1.Position, s2.Position, s3.Position);

                var type = (ExpressionInstrType)r3.ReadByte();
                if (type == ExpressionInstrType.None)
                {
                    if (s3.Position != s3.Length)
                    { }//no hit
                    break;
                }
                var instr = CreateInstruction(type);
                instr.Type = type;
                instr.Read(r1, r2);
                var instrIndex = (uint)instructions.Count;
                instructions.Add(instr);

                // resolve jumps by converting data byte offsets to instruction indices offsets
                // this is to make it easier to edit the XML
                if (unresolvedJumps.TryGetValue(loc, out var jumps))
                {
                    foreach (var jump in jumps)
                    {
                        // relative offset between jump and target instructions
                        jump.InstructionOffset = instrIndex - jump.InstructionOffset;
                    }
                    unresolvedJumps.Remove(loc);
                }

                if (instr is ExpressionInstrJump j)
                {
                    j.InstructionOffset = instrIndex;
                    var targetLoc = (s1.Position + j.Data1Offset, s2.Position + j.Data2Offset, s3.Position + j.Data3Offset);
                    if (!unresolvedJumps.TryGetValue(targetLoc, out jumps))
                    {
                        jumps = new List<ExpressionInstrJump>();
                        unresolvedJumps[targetLoc] = jumps;
                    }
                    jumps.Add(j);
                }
            }

            // special case to resolve jumps to the end of the instructions list since we don't store the finish marker instruction
            var endLoc = (s1.Position, s2.Position, s3.Position - 1);
            if (unresolvedJumps.TryGetValue(endLoc, out var endJumps))
            {
                foreach (var jump in endJumps)
                {
                    jump.InstructionOffset = (uint)instructions.Count - jump.InstructionOffset;
                }
                unresolvedJumps.Remove(endLoc);
            }


            if ((r1.Length - r1.Position) != 0)
            { }//no hit
            if ((r2.Length - r2.Position) != 0)
            { }//no hit
            if ((r3.Length - r3.Position) != 0) 
            { }//no hit

            // there should be no unresolved jumps at this point
            if (unresolvedJumps.Count != 0)
            { }//no hit

            Instructions = instructions.ToArray();
        }

        public void WriteInstructions()
        {
            var s1 = new MemoryStream();
            var s2 = new MemoryStream();
            var s3 = new MemoryStream();
            var w1 = new DataWriter(s1);
            var w2 = new DataWriter(s2);
            var w3 = new DataWriter(s3);

            foreach (var instr in Instructions)
            {
                w3.Write((byte)instr.Type);
                instr.Write(w1, w2);
            }
            w3.Write((byte)0);

            Data1 = s1.ToArray();
            Data2 = s2.ToArray();
            Data3 = s3.ToArray();
            Data1Length = (uint)Data1.Length;
            Data2Length = (uint)Data2.Length;
            Data3Length = (ushort)Data3.Length;
        }


        public static ExpressionInstrBase CreateInstruction(ExpressionInstrType type)
        {
            switch (type)
            {
                case ExpressionInstrType.Pop:
                case ExpressionInstrType.Dup:
                case ExpressionInstrType.Push0:
                case ExpressionInstrType.Push1:
                case ExpressionInstrType.VectorAbs:
                case ExpressionInstrType.VectorNeg:
                case ExpressionInstrType.VectorRcp:
                case ExpressionInstrType.VectorSqrt:
                case ExpressionInstrType.VectorNeg3:
                case ExpressionInstrType.VectorSquare:
                case ExpressionInstrType.VectorDeg2Rad:
                case ExpressionInstrType.VectorRad2Deg:
                case ExpressionInstrType.VectorSaturate:
                case ExpressionInstrType.FromEuler:
                case ExpressionInstrType.ToEuler:
                case ExpressionInstrType.VectorAdd:
                case ExpressionInstrType.VectorSub:
                case ExpressionInstrType.VectorMul:
                case ExpressionInstrType.VectorMin:
                case ExpressionInstrType.VectorMax:
                case ExpressionInstrType.QuatMul:
                case ExpressionInstrType.VectorGreaterThan:
                case ExpressionInstrType.VectorLessThan:
                case ExpressionInstrType.VectorGreaterEqual:
                case ExpressionInstrType.VectorLessEqual:
                case ExpressionInstrType.VectorClamp:
                case ExpressionInstrType.VectorLerp:
                case ExpressionInstrType.VectorMad:
                case ExpressionInstrType.QuatSlerp:
                case ExpressionInstrType.ToVector:
                case ExpressionInstrType.PushTime:
                case ExpressionInstrType.VectorTransform:
                case ExpressionInstrType.PushDeltaTime:
                case ExpressionInstrType.VectorEqual:
                case ExpressionInstrType.VectorNotEqual:
                    return new ExpressionInstrEmpty();

                case ExpressionInstrType.BlendVector:
                case ExpressionInstrType.BlendQuaternion:
                    return new ExpressionInstrBlend();

                case ExpressionInstrType.TrackGet:
                case ExpressionInstrType.TrackGetComp:
                case ExpressionInstrType.TrackGetOffset:
                case ExpressionInstrType.TrackGetOffsetComp:
                case ExpressionInstrType.TrackGetBoneTransform:
                case ExpressionInstrType.TrackValid:
                case ExpressionInstrType.Unk23:
                case ExpressionInstrType.TrackSet:
                case ExpressionInstrType.TrackSetComp:
                case ExpressionInstrType.TrackSetOffset:
                case ExpressionInstrType.TrackSetOffsetComp:
                case ExpressionInstrType.TrackSetBoneTransform:
                    return new ExpressionInstrBone();

                case ExpressionInstrType.GetVariable:
                case ExpressionInstrType.SetVariable:
                    return new ExpressionInstrVariable();

                case ExpressionInstrType.Jump:
                case ExpressionInstrType.JumpIfTrue:
                case ExpressionInstrType.JumpIfFalse:
                    return new ExpressionInstrJump();


                case ExpressionInstrType.PushFloat: return new ExpressionInstrFloat();
                case ExpressionInstrType.PushVector: return new ExpressionInstrVector();
                case ExpressionInstrType.DefineSpring: return new ExpressionInstrSpring();
                case ExpressionInstrType.LookAt: return new ExpressionInstrLookAt();

                default: throw new Exception("Unknown instruction type");
            }
        }


        public override string ToString()
        {
            return NameHash + " (" + (Instructions?.Length??0) + " instructions)";
        }

    }


    public enum ExpressionInstrType : byte
    {
        None = 0,
        Pop = 0x01,
        Dup = 0x02,
        Push0 = 0x03,
        Push1 = 0x04,
        PushFloat = 0x05,
        TrackGet = 0x06,
        TrackGetComp = 0x07,
        TrackGetOffset = 0x08,
        TrackGetOffsetComp = 0x09,
        TrackGetBoneTransform = 0x0A,
        PushVector = 0x0B,
        DefineSpring = 0x0E,
        VectorAbs = 0x0F,
        VectorNeg = 0x10,
        VectorRcp = 0x11,
        VectorSqrt = 0x12,
        VectorNeg3 = 0x1B,
        VectorSquare = 0x1C,
        VectorDeg2Rad = 0x1D,
        VectorRad2Deg = 0x1E,
        VectorSaturate = 0x1F,
        TrackValid = 0x20,
        FromEuler = 0x21,
        ToEuler = 0x22,
        Unk23 = 0x23,
        TrackSet = 0x26,
        TrackSetComp = 0x27,
        TrackSetOffset = 0x28,
        TrackSetOffsetComp = 0x29,
        TrackSetBoneTransform = 0x2A,
        Jump = 0x2B,
        JumpIfTrue = 0x2C,
        JumpIfFalse = 0x2D,
        VectorAdd = 0x2E,
        VectorSub = 0x2F,
        VectorMul = 0x30,
        VectorMin = 0x31,
        VectorMax = 0x32,
        QuatMul = 0x33,
        VectorGreaterThan = 0x35,
        VectorLessThan = 0x36,
        VectorGreaterEqual = 0x37,
        VectorLessEqual = 0x38,
        VectorClamp = 0x39,
        VectorLerp = 0x3A,
        VectorMad = 0x3B,
        QuatSlerp = 0x3C,
        ToVector = 0x3D,
        LookAt = 0x3E,
        PushTime = 0x3F,
        VectorTransform = 0x40,
        GetVariable = 0x42,
        SetVariable = 0x43,
        BlendVector = 0x44,
        BlendQuaternion = 0x45,
        PushDeltaTime = 0x46,
        VectorEqual = 0x48,
        VectorNotEqual = 0x49,
    }


    [TC(typeof(EXP))] public abstract class ExpressionInstrBase
    {
        public ExpressionInstrType Type { get; set; }

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
    [TC(typeof(EXP))] public class ExpressionInstrEmpty : ExpressionInstrBase
    { }
    [TC(typeof(EXP))] public class ExpressionInstrBlend : ExpressionInstrBase
    {
        [TC(typeof(EXP))] public class SourceComponent : IMetaXmlItem
        {
            public float[] Weights { get; set; }
            public float[] Offsets { get; set; }
            public float[] Thresholds { get; set; }

            public SourceComponent(uint numSourceWeights)
            {
                Weights = new float[numSourceWeights];
                Offsets = new float[numSourceWeights];
                Thresholds = new float[numSourceWeights - 1];
            }

            public void WriteXml(StringBuilder sb, int indent)
            {
                YedXml.WriteRawArray(sb, Weights, indent, "Weights", "", FloatUtil.ToString, 32);
                YedXml.WriteRawArray(sb, Offsets, indent, "Offsets", "", FloatUtil.ToString, 32);
                YedXml.WriteRawArray(sb, Thresholds, indent, "Thresholds", "", FloatUtil.ToString, 32);
            }

            public void ReadXml(XmlNode node)
            {
                throw new NotImplementedException();
            }
        }

        [TC(typeof(EXP))] public class Source : IMetaXmlItem
        {
            public ushort TrackIndex { get; set; }
            public ushort ComponentOffset { get; set; }
            public SourceComponent X { get; set; }
            public SourceComponent Y { get; set; }
            public SourceComponent Z { get; set; }

            public void WriteXml(StringBuilder sb, int indent)
            {
                YedXml.ValueTag(sb, indent, "TrackIndex", TrackIndex.ToString());
                YedXml.ValueTag(sb, indent, "ComponentIndex", (ComponentOffset / 4).ToString());
                YedXml.OpenTag(sb, indent, "X");
                X.WriteXml(sb, indent + 1);
                YedXml.CloseTag(sb, indent, "X");
                YedXml.OpenTag(sb, indent, "Y");
                Y.WriteXml(sb, indent + 1);
                YedXml.CloseTag(sb, indent, "Y");
                YedXml.OpenTag(sb, indent, "Z");
                Z.WriteXml(sb, indent + 1);
                YedXml.CloseTag(sb, indent, "Z");
            }

            public void ReadXml(XmlNode node)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return "TODO"; // TODO
            }
        }

        public uint ByteLength { get; set; } //updated automatically
        public uint SourceCount { get; set; } //updated automatically
        public uint NumSourceWeights { get; set; }
        public uint Unk1 { get; set; } // 0x00000000
        public Source[] Sources { get; set; }
        public Vector4[] Values { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            ByteLength = r1.ReadUInt32();
            SourceCount = r1.ReadUInt32();
            NumSourceWeights = r1.ReadUInt32();
            Unk1 = r1.ReadUInt32();

            Sources = new Source[SourceCount];
            for (int i = 0; i < SourceCount; i++)
            {
                var s = new Source();
                s.TrackIndex = r1.ReadUInt16();
                s.ComponentOffset = r1.ReadUInt16();
                s.X = new SourceComponent(NumSourceWeights);
                s.Y = new SourceComponent(NumSourceWeights);
                s.Z = new SourceComponent(NumSourceWeights);
                Sources[i] = s;
            }

            // TODO: rename
            var numIters = SourceCount / 4;
            var numSubpasses = NumSourceWeights - 1;
            var numValues = (numIters * 6) + (numIters * numSubpasses * 9);
            Values = new Vector4[numValues];
            for (int i = 0; i < numValues; i++)
            {
                Values[i] = r1.ReadVector4();
            }



            switch (SourceCount)
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
                    break; //no hit
            }

            switch (NumSourceWeights)
            {
                case 1: break;
                case 2: break;
                case 3: break;
                case 4: break;
                default:
                    break; //no hit
            }

            if (Unk1 != 0)
            {
            } //no hit


            // organize data into more human-readable layout
            // the file layout is optimized for vectorized operations
            for (int i = 0; i < (Sources.Length / 4); i++)
            {
                var it = i * 4;
                int v = i * 6 + (i * (int)(NumSourceWeights - 1) * 9);
                for (int k = 0; k < 4; k++)
                {
                    Sources[it + k].X.Weights[0] = Values[v + 0][k];
                    Sources[it + k].Y.Weights[0] = Values[v + 1][k];
                    Sources[it + k].Z.Weights[0] = Values[v + 2][k];
                    Sources[it + k].X.Offsets[0] = Values[v + 3][k];
                    Sources[it + k].Y.Offsets[0] = Values[v + 4][k];
                    Sources[it + k].Z.Offsets[0] = Values[v + 5][k];

                    for (int n = 1; n < NumSourceWeights; n++)
                    {
                        int b = v + 6 + (n - 1) * 9;
                        Sources[it + k].X.Thresholds[n - 1] = Values[b + 0][k];
                        Sources[it + k].Y.Thresholds[n - 1] = Values[b + 1][k];
                        Sources[it + k].Z.Thresholds[n - 1] = Values[b + 2][k];
                        Sources[it + k].X.Weights[n] = Values[b + 3][k];
                        Sources[it + k].Y.Weights[n] = Values[b + 4][k];
                        Sources[it + k].Z.Weights[n] = Values[b + 5][k];
                        Sources[it + k].X.Offsets[n] = Values[b + 6][k];
                        Sources[it + k].Y.Offsets[n] = Values[b + 7][k];
                        Sources[it + k].Z.Offsets[n] = Values[b + 8][k];
                    }
                }
            }
        }

        public override void Write(DataWriter w1, DataWriter w2)
        {
            // TODO
            SourceCount = (uint)(Sources?.Length ?? 0);
            NumSourceWeights = Math.Max(NumSourceWeights, 1);
            var valcnt = (NumSourceWeights - 1) * 9 + 6;
            var hlen = SourceCount * 4 + 16;
            var tlen = SourceCount * valcnt * 4;
            ByteLength = hlen + tlen;

            w1.Write(ByteLength);
            w1.Write(SourceCount);
            w1.Write(NumSourceWeights);
            w1.Write(Unk1);

            // TODO
            for (int i = 0; i < SourceCount; i++)
            {
                // Sources[i].Write(w1);
            }
            // TODO
            // for (int n = 0; n < valcnt; n++)
            // {
            //     for (int i = 0; i < ItemCount; i++)
            //     {
            //         var vals = Items[i].Values;
            //         w1.Write((n < (vals?.Length ?? 0)) ? vals[n] : 0.0f);
            //     }
            // }
        }

        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "NumSourceWeights", NumSourceWeights.ToString());
            YedXml.WriteItemArray(sb, Sources, indent, "Sources");
        }

        public override void ReadXml(XmlNode node)
        {
            NumSourceWeights = Xml.GetChildUIntAttribute(node, "NumSourceWeights");
            Sources = XmlMeta.ReadItemArray<Source>(node, "Sources");
        }

        public override string ToString()
        {
            return base.ToString() + "  -  " + SourceCount + ", " + NumSourceWeights;
        }
    }

    [TC(typeof(EXP))] public class ExpressionInstrBone : ExpressionInstrBase
    {
        public ushort TrackIndex { get; set; } //index of the BoneTag in the Expression.BoneTracks array
        public ushort BoneId { get; set; }
        public byte Track { get; set; }
        public byte Format { get; set; }
        public byte ComponentIndex { get; set; }
        public bool UseDefaults { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            TrackIndex = r2.ReadUInt16();
            BoneId = r2.ReadUInt16();
            Track = r2.ReadByte();
            Format = r2.ReadByte();
            ComponentIndex = r2.ReadByte();
            UseDefaults = r2.ReadByte() != 0;
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(TrackIndex);
            w2.Write(BoneId);
            w2.Write(Track);
            w2.Write(Format);
            w2.Write(ComponentIndex);
            w2.Write(UseDefaults ? (byte)1 : (byte)0);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "TrackIndex", TrackIndex.ToString());
            YedXml.ValueTag(sb, indent, "BoneId", BoneId.ToString());
            YedXml.ValueTag(sb, indent, "Track", Track.ToString());
            YedXml.ValueTag(sb, indent, "Format", Format.ToString());
            YedXml.ValueTag(sb, indent, "ComponentIndex", ComponentIndex.ToString());
            YedXml.ValueTag(sb, indent, "UseDefaults", UseDefaults.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            TrackIndex = (ushort)Xml.GetChildUIntAttribute(node, "BoneTrack", "value");
            BoneId = (ushort)Xml.GetChildUIntAttribute(node, "BoneId", "value");
            Track = (byte)Xml.GetChildUIntAttribute(node, "Track", "value");
            Format = (byte)Xml.GetChildUIntAttribute(node, "Format", "value");
            ComponentIndex = (byte)Xml.GetChildUIntAttribute(node, "ComponentIndex", "value");
            UseDefaults = Xml.GetChildBoolAttribute(node, "UseDefaults", "value");
        }

        public override string ToString()
        {
            return base.ToString() + "  -  TrackIndex:" + TrackIndex + ", BoneId:" + BoneId + ", Track: " + Track + ", Format: " + Format + ", ComponentIndex: " + ComponentIndex + ", UseDefaults: " + UseDefaults;
        }
    }
    [TC(typeof(EXP))] public class ExpressionInstrVariable : ExpressionInstrBase
    {
        public MetaHash Variable { get; set; }
        public uint VariableIndex { get; set; } //index of the hash in the Expression.Variables array (autoupdated - don't need in XML)

        public override void Read(DataReader r1, DataReader r2)
        {
            Variable = r2.ReadUInt32();
            VariableIndex = r2.ReadUInt32();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(Variable);
            w2.Write(VariableIndex);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.StringTag(sb, indent, "Variable", YedXml.HashString(Variable));
        }
        public override void ReadXml(XmlNode node)
        {
            Variable = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Variable"));
        }

        public override string ToString()
        {
            return base.ToString() + "  -  Variable:" + Variable + "  [" + VariableIndex + "]";
        }
    }
    [TC(typeof(EXP))] public class ExpressionInstrJump : ExpressionInstrBase
    {
        // note: unsigned so can only jump forwards
        public uint Data1Offset { get; set; }
        public uint Data2Offset { get; set; }
        public uint Data3Offset { get; set; }

        public uint InstructionOffset { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Data1Offset = r2.ReadUInt32();
            Data2Offset = r2.ReadUInt32();
            Data3Offset = r2.ReadUInt32();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(Data1Offset);
            w2.Write(Data2Offset);
            w2.Write(Data3Offset);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "InstructionOffset", InstructionOffset.ToString());
            // YedXml.ValueTag(sb, indent, "Data1Offset", Data1Offset.ToString());
            // YedXml.ValueTag(sb, indent, "Data2Offset", Data2Offset.ToString());
            // YedXml.ValueTag(sb, indent, "Data3Offset", Data3Offset.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            // TODO: calculate data offsets
            InstructionOffset = Xml.GetChildUIntAttribute(node, "InstructionOffset");
            // Data1Offset = Xml.GetChildUIntAttribute(node, "Data1Offset");
            // Data2Offset = Xml.GetChildUIntAttribute(node, "Data2Offset");
            // Data3Offset = Xml.GetChildUIntAttribute(node, "Data3Offset");
        }

        public override string ToString()
        {
            return base.ToString() + "  -  " + Data1Offset + ", " + Data2Offset + ", " + Data3Offset;
        }
    }
    [TC(typeof(EXP))] public class ExpressionInstrFloat : ExpressionInstrBase
    {
        public float Value { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Value = r2.ReadSingle();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w2.Write(Value);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "Value", FloatUtil.ToString(Value));
        }
        public override void ReadXml(XmlNode node)
        {
            Value = Xml.GetChildFloatAttribute(node, "Value");
        }

        public override string ToString()
        {
            return base.ToString() + "  -  " + Value.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionInstrVector : ExpressionInstrBase
    {
        public Vector4 Value { get; set; }

        public override void Read(DataReader r1, DataReader r2)
        {
            Value = r1.ReadVector4();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            w1.Write(Value);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.SelfClosingTag(sb, indent, "Value " + FloatUtil.GetVector4XmlString(Value));
        }
        public override void ReadXml(XmlNode node)
        {
            Value = Xml.GetChildVector4Attributes(node, "Value");
        }

        public override string ToString()
        {
            return base.ToString() + "  -  " + Value;
        }
    }
    [TC(typeof(EXP))] public class ExpressionInstrSpring : ExpressionInstrBase
    {
        public ExpressionSpringDescription SpringDescription { get; set; }
        public uint BoneTrackRot { get; set; }
        public uint BoneTrackPos { get; set; }
        public uint UnkUint13 { get; set; }//0
        public uint UnkUint14 { get; set; }//0

        public override void Read(DataReader r1, DataReader r2)
        {
            SpringDescription = new ExpressionSpringDescription();
            SpringDescription.Read(r1);
            BoneTrackRot = r1.ReadUInt32();
            BoneTrackPos = r1.ReadUInt32();
            UnkUint13 = r1.ReadUInt32();
            UnkUint14 = r1.ReadUInt32();
        }
        public override void Write(DataWriter w1, DataWriter w2)
        {
            if (SpringDescription == null) SpringDescription = new ExpressionSpringDescription();
            SpringDescription.Write(w1);
            w1.Write(BoneTrackRot);
            w1.Write(BoneTrackPos);
            w1.Write(UnkUint13);
            w1.Write(UnkUint14);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            if (SpringDescription == null) SpringDescription = new ExpressionSpringDescription();
            SpringDescription.WriteXml(sb, indent);
            YedXml.ValueTag(sb, indent, "BoneTrackRot", BoneTrackRot.ToString());
            YedXml.ValueTag(sb, indent, "BoneTrackPos", BoneTrackPos.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            SpringDescription = new ExpressionSpringDescription();
            SpringDescription.ReadXml(node);
            BoneTrackRot = Xml.GetChildUIntAttribute(node, "BoneTrackRot", "value");
            BoneTrackPos = Xml.GetChildUIntAttribute(node, "BoneTrackPos", "value");
        }

        public override string ToString()
        {
            return base.ToString() + "   -   " + BoneTrackRot.ToString() + ", " + BoneTrackPos.ToString();
        }
    }
    [TC(typeof(EXP))] public class ExpressionInstrLookAt : ExpressionInstrBase
    {
        public enum Axis : uint
        {
            PositiveX = 0, // ( 1.0,  0.0,  0.0)
            PositiveY = 1, // ( 0.0,  1.0,  0.0)
            PositiveZ = 2, // ( 0.0,  0.0,  1.0)
            NegativeX = 3, // (-1.0,  0.0,  0.0)
            NegativeY = 4, // ( 0.0, -1.0,  0.0)
            NegativeZ = 5, // ( 0.0,  0.0, -1.0)
        }
        
        public Vector4 Offset { get; set; }
        public Axis LookAtAxis { get; set; } // 0, 1, 2
        public Axis UpAxis { get; set; } // 0, 2
        public Axis Origin { get; set; } // 0, 2
        public uint Unk05 { get; set; } // 0x00000000

        public override void Read(DataReader r1, DataReader r2)
        {
            Offset = r1.ReadVector4();
            LookAtAxis = (Axis)r1.ReadUInt32();
            UpAxis = (Axis)r1.ReadUInt32();
            Origin = (Axis)r1.ReadUInt32();
            Unk05 = r1.ReadUInt32();

            switch ((uint)LookAtAxis)
            {
                case 0:
                case 2:
                case 1:
                    break;
                default:
                    break;//no hit
            }
            switch ((uint)UpAxis)
            {
                case 2:
                case 0:
                    break;
                default:
                    break;//no hit
            }
            switch ((uint)Origin)
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
            w1.Write(Offset);
            w1.Write((uint)LookAtAxis);
            w1.Write((uint)UpAxis);
            w1.Write((uint)Origin);
            w1.Write(Unk05);
        }
        public override void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.SelfClosingTag(sb, indent, "Offset " + FloatUtil.GetVector4XmlString(Offset));
            YedXml.StringTag(sb, indent, "LookAtAxis", LookAtAxis.ToString());
            YedXml.StringTag(sb, indent, "UpAxis", UpAxis.ToString());
            YedXml.StringTag(sb, indent, "Origin", Origin.ToString());
        }
        public override void ReadXml(XmlNode node)
        {
            Offset = Xml.GetChildVector4Attributes(node, "Offset");
            LookAtAxis = Xml.GetChildEnumInnerText<Axis>(node, "LookAtAxis");
            UpAxis = Xml.GetChildEnumInnerText<Axis>(node, "UpAxis");
            Origin = Xml.GetChildEnumInnerText<Axis>(node, "Origin");
        }

        public override string ToString()
        {
            return base.ToString() + "  -  " + Offset + "   -   " + LookAtAxis + ", " + UpAxis + ", " + Origin;
        }
    }




    [TC(typeof(EXP))] public class ExpressionSpringDescriptionBlock : ResourceSystemBlock
    {
        public override long BlockLength => 0xA0;

        public ExpressionSpringDescription Spring { get; set; }

        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Spring = new ExpressionSpringDescription();
            Spring.Read(reader);
        }
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            if (Spring == null) Spring = new ExpressionSpringDescription();
            Spring.Write(writer);
        }

        public override string ToString()
        {
            return Spring?.ToString() ?? base.ToString();
        }
    }



    [TC(typeof(EXP))] public class ExpressionSpringDescription
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
        public ushort BoneId { get; set; }
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
            BoneId = r.ReadUInt16();
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
            w.Write(BoneId);
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
            YedXml.ValueTag(sb, indent, "BoneTag", BoneId.ToString());
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
            BoneId = (ushort)Xml.GetChildUIntAttribute(node, "BoneTag", "value");
        }

        public bool Compare(ExpressionSpringDescription o)
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
            if (o.BoneId != BoneId) return false;
            if (o.Ushort10b != Ushort10b) return false;
            return true;
        }
        public ExpressionSpringDescription Clone()
        {
            var n = new ExpressionSpringDescription();
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
            n.BoneId = BoneId;
            n.Ushort10b = Ushort10b;
            return n;
        }

        public override string ToString()
        {
            return BoneId.ToString();
        }
    }


    [TC(typeof(EXP))] public struct ExpressionTrack : IMetaXmlItem
    {
        public ushort BoneId { get; set; }
        public byte Track { get; set; }
        public byte Flags { get; set; }

        public byte Format => (byte)(Flags & 0x7F); // VECTOR3 = 0, QUATERNION = 1, FLOAT = 2
        public bool UnkFlag => (Flags & 0x80) != 0;

        public void WriteXml(StringBuilder sb, int indent)
        {
            YedXml.ValueTag(sb, indent, "BoneId", BoneId.ToString());
            YedXml.ValueTag(sb, indent, "Track", Track.ToString());
            YedXml.ValueTag(sb, indent, "Format", Format.ToString());
            YedXml.ValueTag(sb, indent, "UnkFlag", UnkFlag.ToString());
        }
        public void ReadXml(XmlNode node)
        {
            BoneId = (ushort)Xml.GetChildUIntAttribute(node, "BoneId");
            Track = (byte)Xml.GetChildUIntAttribute(node, "Track");
            var format = (byte)Xml.GetChildUIntAttribute(node, "Format");
            var unkFlag = Xml.GetChildBoolAttribute(node, "UnkFlag");
            Flags = (byte)((format & 0x7F) | (unkFlag ? 0x80 : 0));
        }

        public override string ToString()
        {
            return BoneId + ", " + Track + ", " + Format + ", " + UnkFlag;
        }
    }


}
