using System;
using System.IO;
using System.Collections.Generic;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class MrfFile : GameFile, PackedFile
    {
        public byte[] RawFileData { get; set; }
        public uint Magic { get; set; } = 0x45566F4D; // 'MoVE'
        public uint Version { get; set; } = 2;
        public uint HeaderUnk1 { get; set; } = 0;
        public uint HeaderUnk2 { get; set; } = 0;
        public uint HeaderUnk3 { get; set; } = 0;
        public uint DataLength { get; set; }
        public uint FlagsCount { get; set; }
        public uint Unk1_Count { get; set; }
        public uint Unk2_Count { get; set; }
        public uint Unk3_Count { get; set; }

        public MrfStructHeaderUnk1[] Unk1_Items { get; set; }
        public MrfStructHeaderUnk2[] Unk2_Items { get; set; }
        public MrfStructHeaderUnk3[] Unk3_Items { get; set; }
        public byte[] FlagsItems { get; set; }

        public MrfNode[] AllNodes { get; set; }
        public MrfNode RootNode { get; set; }

        public MrfFile() : base(null, GameFileType.Mrf)
        {
        }

        public MrfFile(RpfFileEntry entry) : base(entry, GameFileType.Mrf)
        {
            RpfFileEntry = entry;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RawFileData = data;
            if (entry != null)
            {
                RpfFileEntry = entry;
                Name = entry.Name;
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                DataReader r = new DataReader(ms, Endianess.LittleEndian);

                Read(r);
            };
        }

        public byte[] Save()
        {
            MemoryStream s = new MemoryStream();
            DataWriter w = new DataWriter(s);

            Write(w);

            var buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            return buf;
        }

        private void Write(DataWriter w)
        {
            if (Magic != 0x45566F4D || Version != 2 || HeaderUnk1 != 0 || HeaderUnk2 != 0)
                throw new Exception("Failed to write MRF, header is invalid!");

            w.Write(Magic);
            w.Write(Version);
            w.Write(HeaderUnk1);
            w.Write(HeaderUnk2);
            w.Write(HeaderUnk3);
            w.Write(DataLength);
            w.Write(FlagsCount);

            // Unused in final game
            w.Write(Unk1_Count);

            if (Unk1_Count > 0)
            {
                foreach (var entry in Unk1_Items)
                {
                    w.Write(entry.Size);
                    w.Write(entry.Bytes);
                }
            }

            w.Write(Unk2_Count);

            if (Unk2_Count > 0)
            {
                foreach (var entry in Unk2_Items)
                {
                    w.Write(entry.Unk1);
                    w.Write(entry.Unk2);
                }
            }

            w.Write(Unk3_Count);

            if (Unk3_Count > 0)
            {
                foreach (var entry in Unk3_Items)
                {
                    w.Write(entry.Unk1);
                    w.Write(entry.Unk2);
                }
            }

            if (AllNodes != null)
            {
                foreach (var node in AllNodes)
                    node.Write(w);
            }

            for (int i = 0; i < FlagsCount; i++)
                w.Write(FlagsItems[i]);
        }

        private void Read(DataReader r)
        {
            Magic = r.ReadUInt32(); // Should be 'MoVE'
            Version = r.ReadUInt32(); // GTA5 = 2, RDR3 = 11
            HeaderUnk1 = r.ReadUInt32(); // Should be 0
            HeaderUnk2 = r.ReadUInt32();
            HeaderUnk3 = r.ReadUInt32(); // Should be 0
            DataLength = r.ReadUInt32();
            FlagsCount = r.ReadUInt32();

            if (Magic != 0x45566F4D || Version != 2 || HeaderUnk1 != 0 || HeaderUnk2 != 0)
                throw new Exception("Failed to read MRF, header is invalid!");

            // Unused in final game
            Unk1_Count = r.ReadUInt32();
            if (Unk1_Count > 0)
            {
                Unk1_Items = new MrfStructHeaderUnk1[Unk1_Count];

                for (int i = 0; i < Unk1_Count; i++)
                    Unk1_Items[i] = new MrfStructHeaderUnk1(r);
            }

            Unk2_Count = r.ReadUInt32();
            if (Unk2_Count > 0)
            {
                Unk2_Items = new MrfStructHeaderUnk2[Unk2_Count];

                for (int i = 0; i < Unk2_Count; i++)
                    Unk2_Items[i] = new MrfStructHeaderUnk2(r);
            }

            Unk3_Count = r.ReadUInt32();
            if (Unk3_Count > 0)
            {
                Unk3_Items = new MrfStructHeaderUnk3[Unk3_Count];

                for (int i = 0; i < Unk3_Count; i++)
                    Unk3_Items[i] = new MrfStructHeaderUnk3(r);
            }

            var nodes = new List<MrfNode>();

            while (true)
            {
                var index = nodes.Count;
                var offset = (int)r.Position;

                var node = ReadNode(r);
                
                if (node == null) break;

                node.FileIndex = index;
                node.FileOffset = offset;
                node.FileDataSize = ((int)r.Position) - offset;

                nodes.Add(node);
            }

            AllNodes = nodes.ToArray();

            if (FlagsCount != 0)
            {
                FlagsItems = new byte[FlagsCount];

                for (int i = 0; i < FlagsCount; i++)
                    FlagsItems[i] = r.ReadByte();
            }


            BuildNodeHierarchy();


            if (r.Position != r.Length)
                throw new Exception($"Failed to read MRF ({r.Position} / {r.Length})");
        }

        private MrfNode ReadNode(DataReader r)
        {
            var startPos = r.Position;
            var nodeType = (MrfNodeType)r.ReadUInt16();
            r.Position = startPos;

            if (nodeType <= MrfNodeType.None || nodeType >= MrfNodeType.Max)
            {
                if (r.Position != r.Length)//should only be at EOF
                { }
                return null;
            }

            var node = CreateNode(nodeType);

            node.Read(r);

            return node;
        }

        private MrfNode CreateNode(MrfNodeType infoType)
        {
            switch (infoType)
            {
                case MrfNodeType.StateMachineClass:
                    return new MrfNodeStateMachineClass();
                case MrfNodeType.Tail:
                    return new MrfNodeTail();
                case MrfNodeType.InlinedStateMachine:
                    return new MrfNodeInlinedStateMachine();
                case MrfNodeType.Unk4:
                    return new MrfNodeUnk4();
                case MrfNodeType.Blend:
                    return new MrfNodeBlend();
                case MrfNodeType.AddSubtract:
                    return new MrfNodeAddSubstract();
                case MrfNodeType.Filter:
                    return new MrfNodeFilter();
                case MrfNodeType.Unk8:
                    return new MrfNodeUnk8();
                case MrfNodeType.Frame:
                    return new MrfNodeFrame();
                case MrfNodeType.Unk10:
                    return new MrfNodeUnk10();
                case MrfNodeType.BlendN:
                    return new MrfNodeBlendN();
                case MrfNodeType.Clip:
                    return new MrfNodeClip();
                case MrfNodeType.Unk17:
                    return new MrfNodeUnk17();
                case MrfNodeType.Unk18:
                    return new MrfNodeUnk18();
                case MrfNodeType.Expression:
                    return new MrfNodeExpression();
                case MrfNodeType.Unk20:
                    return new MrfNodeUnk20();
                case MrfNodeType.Proxy:
                    return new MrfNodeProxy();
                case MrfNodeType.AddN:
                    return new MrfNodeAddN();
                case MrfNodeType.Identity:
                    return new MrfNodeIdentity();
                case MrfNodeType.Unk24:
                    return new MrfNodeUnk24();
                case MrfNodeType.Unk25:
                    return new MrfNodeUnk25();
                case MrfNodeType.MergeN:
                    return new MrfNodeMergeN();
                case MrfNodeType.State:
                    return new MrfNodeState();
                case MrfNodeType.Invalid:
                    return new MrfNodeInvalid();
                case MrfNodeType.Unk29:
                    return new MrfNodeUnk29();
                case MrfNodeType.SubNetworkClass:
                    return new MrfNodeSubNetworkClass();
                case MrfNodeType.Unk31:
                    return new MrfNodeUnk31();
            }

            throw new Exception($"A handler for ({infoType}) mrf node type is not valid");
        }


        private int BuildNodeHierarchy(MrfNode node = null, int index = 0)
        {
            if (AllNodes == null) return 0;
            if (AllNodes.Length <= index) return 0;

            if (node == null)
            {
                var rlist = new List<MrfNode>();
                for (int i = 0; i < AllNodes.Length; i++)
                {
                    var rnode = AllNodes[i];
                    rlist.Add(rnode);
                    i += BuildNodeHierarchy(rnode, i);
                }

                var smlist = new List<MrfNodeStateMachineClass>();
                var imlist = new List<MrfNodeInlinedStateMachine>();
                var snlist = new List<MrfNodeState>();
                foreach (var n in AllNodes)
                {
                    if (n is MrfNodeStateMachineClass sm) 
                    { 
                        smlist.Add(sm);
                        if (sm.ChildNodes != null) for (int i = 0; i < sm.ChildNodes.Length; i++) if (sm.ChildNodes[i].NodeIndex != i)
                                { }//sanity check - don't get here
                    }
                    if (n is MrfNodeInlinedStateMachine im)
                    {
                        imlist.Add(im);
                        if (im.ChildNodes != null) for (int i = 0; i < im.ChildNodes.Length; i++) if (im.ChildNodes[i].NodeIndex != i)
                                { }//sanity check - don't get here
                    }
                    if (n is MrfNodeState sn)
                    {
                        snlist.Add(sn);
                        if (sn.ChildNodes != null) for (int i = 0; i < sn.ChildNodes.Length; i++) if (sn.ChildNodes[i].NodeIndex != i)
                                { }//sanity check - don't get here
                    }
                }

                if (rlist.Count > 0)
                {
                    RootNode = rlist[0];
                }
                if (rlist.Count != 1)
                { }//sanity check - don't get here
                if (AllNodes.Length > 1000)
                { }
                return 0;
            }

            if (node is MrfNodeStateBase snode)
            {
                var c = 0;
                var clist = new List<MrfNode>();
                int ccount = snode.StateChildCount;
                if (ccount == 0)
                {
                    return 0;
                }
                for (int i = 0; i <= ccount; i++)
                {
                    if ((i == ccount) && (node is MrfNodeState))
                    { break; }
                    var cind = index + c + i + 1;
                    if (cind >= AllNodes.Length)
                    {
                        if (i != ccount)
                        { }//don't get here (tried to continue past the end of the array!)
                        break; 
                    }
                    var cnode = AllNodes[cind];
                    if (cnode is MrfNodeTail)
                    {
                        i--;
                        c++;
                        if (clist.Count > 0)
                        {
                            var prevnode = clist[clist.Count - 1];
                            if (prevnode is MrfNodeStateBase sprevnode)
                            {
                                if (sprevnode.TailNode != null)
                                { }//don't get here (tail node was already assigned?!?)
                                sprevnode.TailNode = cnode;
                                if (clist.Count == ccount)
                                { break; }//list is full, don't continue
                            }
                            else
                            { }//don't get here (previous node isn't a state??)
                        }
                        else
                        { }//don't get here (can't have tail without a previous node!)
                        continue;
                    }
                    else if (clist.Count == ccount)
                    { break; }//this node isn't a tail, but the list is already full, so it must belong to another node
                    if (cnode.NodeIndex != i)
                    { break; }//don't get here (node index mismatch!)
                    clist.Add(cnode);
                    c += BuildNodeHierarchy(cnode, cind);//recurse...
                }
                if (clist.Count != ccount)
                { }//don't get here (sanity check)
                snode.ChildNodes = clist.ToArray();
                return c + clist.Count;
            }

            return 0;
        }
    
    }



    // Unused node indexes by GTAV: 11, 12, 14, 16
    // Exist in GTAV but not used in MRFs: 4, 8, 10, 17, 21, 22, 28, 29, 31, 32
    public enum MrfNodeType : ushort
    {
        None = 0,
        StateMachineClass = 1,
        Tail = 2,
        InlinedStateMachine = 3,
        Unk4 = 4,
        Blend = 5,
        AddSubtract = 6,
        Filter = 7,
        Unk8 = 8,
        Frame = 9,
        Unk10 = 10,
        BlendN = 13,
        Clip = 15,
        Unk17 = 17,
        Unk18 = 18,
        Expression = 19,
        Unk20 = 20,
        Proxy = 21,
        AddN = 22,
        Identity = 23,
        Unk24 = 24,
        Unk25 = 25,
        MergeN = 26,
        State = 27,
        Invalid = 28,
        Unk29 = 29,
        SubNetworkClass = 30,
        Unk31 = 31,
        Max = 32
    }

    #region mrf node abstractions
    
    [TC(typeof(EXP))] public abstract class MrfNode
    {
        public MrfNodeType NodeType { get; set; }
        public ushort NodeIndex { get; set; } //index in the parent node

        public int FileIndex { get; set; } //index in the file
        public int FileOffset { get; set; } //offset in the file
        public int FileDataSize { get; set; } //number of bytes read from the file (this node only)

        public virtual void Read(DataReader r)
        {
            NodeType = (MrfNodeType)r.ReadUInt16();
            NodeIndex = r.ReadUInt16();
        }

        public virtual void Write(DataWriter w)
        {
            w.Write((ushort)NodeType);
            w.Write(NodeIndex);
        }

        public override string ToString()
        {
            return /* FileIndex.ToString() + ":" + FileOffset.ToString() + "+" + FileDataSize.ToString() + ": " +  */
                NodeType.ToString() + " - " + NodeIndex.ToString();
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeNameFlagsBase : MrfNode
    {
        public MetaHash NameHash { get; set; }
        public uint Flags { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);
            NameHash = new MetaHash(r.ReadUInt32());
            Flags = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(NameHash);
            w.Write(Flags);
        }

        public override string ToString()
        {
            return base.ToString() + " - " + NameHash.ToString() + " - " + Flags.ToString();
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeStateBase : MrfNode
    {
        public MetaHash NameHash { get; set; } // Used as an identifier for transitions
        public uint StateByteCount { get; set; }
        public uint StateUnk3 { get; set; }
        public byte StateUnk4 { get; set; }
        public byte StateUnk5 { get; set; }
        public byte StateChildCount { get; set; }
        public byte StateSectionCount { get; set; }
        public uint StateUnk8 { get; set; }
        public uint StateUnk9 { get; set; }
        public uint StateFlags { get; set; }

        public MrfNode[] ChildNodes { get; set; }
        public MrfNode TailNode { get; set; }


        public override void Read(DataReader r)
        {
            base.Read(r);
            NameHash = new MetaHash(r.ReadUInt32());
            StateByteCount = r.ReadUInt32();
            StateUnk3 = r.ReadUInt32();
            StateUnk4 = r.ReadByte();
            StateUnk5 = r.ReadByte();
            StateChildCount = r.ReadByte();
            StateSectionCount = r.ReadByte();
            StateUnk8 = r.ReadUInt32();
            StateUnk9 = r.ReadUInt32();
            StateFlags = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(NameHash);
            w.Write(StateByteCount);
            w.Write(StateUnk3);
            w.Write(StateUnk4);
            w.Write(StateUnk5);
            w.Write(StateChildCount);
            w.Write(StateSectionCount);
            w.Write(StateUnk8);
            w.Write(StateUnk9);
            w.Write(StateFlags);
        }

        public override string ToString()
        {
            return base.ToString() + " - " + NameHash.ToString()
                + " - BC:" + StateByteCount.ToString()
                + " - CC:" + StateChildCount.ToString()
                + " - SC:" + StateSectionCount.ToString()
                + " - " + StateUnk3.ToString()
                + " - " + StateUnk4.ToString()
                + " - " + StateUnk5.ToString()
                + " - " + StateUnk8.ToString()
                + " - " + StateUnk9.ToString()
                + " - F:" + StateFlags.ToString();
        }
    }



    [TC(typeof(EXP))] public abstract class MrfNodeValueBase : MrfNode
    {
        public MetaHash Value { get; set; }//maybe not an actual hash

        public override void Read(DataReader r)
        {
            base.Read(r);
            Value = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(Value);
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeBlendAddSubtractBase : MrfNodeNameFlagsBase
    {
        public int Unk1 { get; set; }
        public int Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public MetaHash Unk4 { get; set; }
        public MetaHash Unk5 { get; set; }
        public MetaHash Unk6 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadInt32();
            Unk2 = r.ReadInt32();

            if ((Flags & 0x180000) == 0x80000)
                Unk3 = r.ReadUInt32();

            if ((Flags & 3) != 0)
                Unk4 = new MetaHash(r.ReadUInt32());

            switch ((Flags >> 2) & 3)
            {
                case 1:
                    Unk5 = new MetaHash(r.ReadUInt32());
                    Unk6 = new MetaHash(r.ReadUInt32());
                    break;
                case 2:
                    Unk6 = new MetaHash(r.ReadUInt32());
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);
            w.Write(Unk2);

            if ((Flags & 0x180000) == 0x80000)
                w.Write(Unk3);

            if ((Flags & 3) != 0)
                w.Write(Unk4);

            switch ((Flags >> 2) & 3)
            {
                case 1:
                    w.Write(Unk5);
                    w.Write(Unk6);
                    break;
                case 2:
                    w.Write(Unk6);
                    break;
            }
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeFilterUnkBase : MrfNodeNameFlagsBase
    {
        public uint Unk1 { get; set; }
        public MetaHash Unk2 { get; set; }
        public MetaHash Unk3 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();

            switch (Flags & 3)
            {
                case 1:
                    Unk2 = new MetaHash(r.ReadUInt32()); // Filter Frame dict hash
                    Unk3 = new MetaHash(r.ReadUInt32()); // Filter Frame name hash
                    break;
                case 2:
                    Unk3 = new MetaHash(r.ReadUInt32());
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);

            switch (Flags & 3)
            {
                case 1:
                    w.Write(Unk2);
                    w.Write(Unk3);
                    break;
                case 2:
                    w.Write(Unk3);
                    break;
            }
        }
    }

    [TC(typeof(EXP))] public abstract class MrfNodeNegativeBase : MrfNodeNameFlagsBase
    {
        public uint Unk1 { get; set; }
        public byte[] Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public int[] Unk7 { get; set; }
        public MrfStructNegativeDataUnk7[] Unk7_Items { get; set; }
        public uint[] Unk8 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            var unkTypeFlag1 = Flags & 3;
            var unkTypeFlag2 = (Flags >> 2) & 3;
            var unk7Count = Flags >> 26;

            if ((Flags & 0x180000) == 0x80000)
                Unk1 = r.ReadUInt32();

            if (unkTypeFlag1 == 1)
                Unk2 = r.ReadBytes(76); // Unused?
            else if (unkTypeFlag1 == 2)
                Unk3 = r.ReadUInt32();

            if (unkTypeFlag2 == 1)
            {
                Unk4 = r.ReadUInt32();
                Unk5 = r.ReadUInt32();
            }
            else if (unkTypeFlag2 == 2)
                Unk6 = r.ReadUInt32();

            if (unk7Count != 0)
            {
                Unk7 = new int[unk7Count];

                for (int i = 0; i < unk7Count; i++)
                    Unk7[i] = r.ReadInt32();
            }

            var unk8Count = (((2 * unk7Count) | 7) + 1) >> 3;

            if (unk8Count != 0)
            {
                Unk8 = new uint[unk8Count];

                for (int i = 0; i < unk8Count; i++)
                    Unk8[i] = r.ReadUInt32();
            }

            if (unk7Count == 0)
                return;

            Unk7_Items = new MrfStructNegativeDataUnk7[unk7Count];
            int iteration = 0;

            for (int i = 0; i < unk7Count; i++)
            {
                var unk8Value = Unk8[iteration >> 3];
                var unk7Flag = unk8Value >> (4 * (iteration & 7));
                var unkTypeFlag3 = (unk7Flag >> 4) & 3;

                var item = new MrfStructNegativeDataUnk7();

                if ((unk7Flag & 3) != 0)
                    item.Unk1 = r.ReadUInt32();

                if (unkTypeFlag3 == 1)
                {
                    item.Unk2 = r.ReadUInt32();
                    item.Unk3 = r.ReadUInt32();
                }
                else if (unkTypeFlag3 == 2)
                    item.Unk4 = r.ReadUInt32();

                Unk7_Items[i] = item;

                iteration += 2;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            var unkTypeFlag1 = Flags & 3;
            var unkTypeFlag2 = (Flags >> 2) & 3;
            var unk7Count = Flags >> 26;

            if ((Flags & 0x180000) == 0x80000)
                w.Write(Unk1);

            if (unkTypeFlag1 == 1)
                w.Write(Unk2);
            else if (unkTypeFlag1 == 2)
                w.Write(Unk3);

            if (unkTypeFlag2 == 1)
            {
                w.Write(Unk4);
                w.Write(Unk5);
            }
            else if (unkTypeFlag2 == 2)
                w.Write(Unk6);

            if (unk7Count > 0)
            {
                foreach (var value in Unk7)
                    w.Write(value);
            }

            var unk8Count = (((2 * unk7Count) | 7) + 1) >> 3;

            if (unk8Count > 0)
            {
                foreach (var value in Unk8)
                    w.Write(value);
            }

            if (unk7Count == 0)
                return;

            int iteration = 0;

            foreach (var item in Unk7_Items)
            {
                var unk8Value = Unk8[iteration >> 3];
                var unk7Flag = unk8Value >> (4 * (iteration & 7));
                var unkTypeFlag3 = (unk7Flag >> 4) & 3;

                if ((unk7Flag & 3) != 0)
                    w.Write(item.Unk1);

                if (unkTypeFlag3 == 1)
                {
                    w.Write(item.Unk2);
                    w.Write(item.Unk3);
                }
                else if (unkTypeFlag3 == 2)
                    w.Write(item.Unk4);

                iteration += 2;
            }
        }
    }
    


    #endregion

    #region mrf node structs

    public abstract class MrfStruct
    {
        public abstract void Write(DataWriter w);
    }

    [TC(typeof(EXP))] public class MrfStructHeaderUnk1 : MrfStruct
    {
        public uint Size { get; set; }
        public byte[] Bytes { get; set; }

        public MrfStructHeaderUnk1(DataReader r)
        {
            Size = r.ReadUInt32();
            Bytes = r.ReadBytes((int)Size);
        }

        public override void Write(DataWriter w)
        {
            w.Write(Size);
            w.Write(Bytes);
        }

        public override string ToString()
        {
            return Size.ToString() + " bytes";
        }
    }

    [TC(typeof(EXP))] public class MrfStructHeaderUnk2 : MrfStruct
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public MrfStructHeaderUnk2(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructHeaderUnk3 : MrfStruct
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public MrfStructHeaderUnk3(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateMainSection : MrfStruct
    {
        //maybe Transition ..?

        public uint Unk1 { get; set; }
        public int Unk2 { get; set; }
        public float Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }
        public uint Unk8 { get; set; }
        public MrfStructStateCondition[] Conditions { get; set; }

        public MrfStructStateMainSection(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadInt32();
            Unk3 = r.ReadSingle();
            Unk4 = r.ReadUInt32();
            Unk5 = r.ReadUInt32();
            Unk6 = r.ReadUInt32();

            uint flags = Unk1 & 0xFFFBFFFF;
            var numconds = (flags >> 20) & 0xF;

            if (numconds > 0)
            {
                Conditions = new MrfStructStateCondition[numconds];
                for (int i = 0; i < numconds; i++)
                {
                    Conditions[i] = new MrfStructStateCondition(r);
                }
            }

            if ((flags & 0x40000000) != 0)
            {
                Unk7 = r.ReadUInt32();
                Unk8 = r.ReadUInt32();
            }
            else
            {
                Unk7 = 0;
                Unk8 = 0;
            }
        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(Unk6);

            // FIXME: might be broken if changed without flags, see "numconds"
            if (Conditions != null)
                for (int i = 0; i < Conditions.Length; i++)
                    Conditions[i].Write(w);

            // FIXME: might be broken if changed without flags
            uint flags = Unk1 & 0xFFFBFFFF;

            if ((flags & 0x40000000) != 0)
            {
                w.Write(Unk7);
                w.Write(Unk8);
            }
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString() + " - " + FloatUtil.ToString(Unk3)
                + " - " + Unk4.ToString()
                + " - " + Unk5.ToString()
                + " - " + Unk6.ToString()
                + " - " + Unk7.ToString()
                + " - " + Unk8.ToString()
                + " - " + (Conditions?.Length ?? 0).ToString() + " conditions";
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateCondition : MrfStruct
    {
        public short UnkType { get; }
        public short UnkIndex { get; }
        public uint Unk1_1 { get; }
        public uint Unk1_2 { get; }
        public uint Unk1_3 { get; }
        public uint Unk2_1 { get; }
        public uint Unk3_1 { get; }
        public uint Unk3_2 { get; }

        public MrfStructStateCondition(DataReader r)
        {
            Unk1_1 = 0;
            Unk1_2 = 0;
            Unk1_3 = 0;
            Unk2_1 = 0;
            Unk3_1 = 0;
            Unk3_2 = 0;

            UnkType = r.ReadInt16();
            UnkIndex = r.ReadInt16();

            switch (UnkType)
            {
                case 0:
                case 1:
                    {
                        Unk1_1 = r.ReadUInt32();
                        Unk1_2 = r.ReadUInt32();
                        Unk1_3 = r.ReadUInt32();
                        break;
                    }
                case 9:
                case 10:
                    {
                        Unk2_1 = r.ReadUInt32();
                        break;
                    }
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 11:
                case 12:
                    {
                        Unk3_1 = r.ReadUInt32();
                        Unk3_2 = r.ReadUInt32();
                        break;
                    }
            }
        }

        public override void Write(DataWriter w)
        {
            w.Write(UnkType);
            w.Write(UnkIndex);

            // FIXME: might be broken if changed outside
            switch (UnkType)
            {
                case 0:
                case 1:
                    {
                        w.Write(Unk1_1);
                        w.Write(Unk1_2);
                        w.Write(Unk1_3);
                        break;
                    }
                case 9:
                case 10:
                    {
                        w.Write(Unk2_1);
                        break;
                    }
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 11:
                case 12:
                    {
                        w.Write(Unk3_1);
                        w.Write(Unk3_2);
                        break;
                    }
            }
        }

        public override string ToString()
        {
            return UnkType.ToString() + " - " + UnkIndex.ToString()
                + " - " + Unk1_1.ToString()
                + " - " + Unk1_2.ToString()
                + " - " + Unk1_3.ToString()
                + " - " + Unk2_1.ToString()
                + " - " + Unk3_1.ToString()
                + " - " + Unk3_2.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateVariable : MrfStruct
    {
        public MetaHash VariableName { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public uint Unk4 { get; }

        public MrfStructStateVariable(DataReader r)
        {
            VariableName = new MetaHash(r.ReadUInt32());
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            w.Write(VariableName);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
        }

        public override string ToString()
        {
            return VariableName.ToString() + " - " + Unk2.ToString() + " - " + Unk3.ToString() + " - " + Unk4.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateEvent : MrfStruct
    {
        public ushort Unk1 { get; }
        public ushort Unk2 { get; }
        public MetaHash NameHash { get; }

        public MrfStructStateEvent(DataReader r)
        {
            Unk1 = r.ReadUInt16();
            Unk2 = r.ReadUInt16();
            NameHash = new MetaHash(r.ReadUInt32());
        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(NameHash);
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString() + " - " + NameHash.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateUnk6 : MrfStruct
    {
        public MetaHash Unk1 { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public uint Unk4 { get; }

        public MrfStructStateUnk6(DataReader r)
        {
            Unk1 = new MetaHash(r.ReadUInt32());
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString() + " - " + Unk3.ToString() + " - " + Unk4.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateMachineState : MrfStruct
    {
        public MetaHash StateName { get; }
        public uint UnkValue { get; }

        public MrfStructStateMachineState(DataReader r)
        {
            StateName = new MetaHash(r.ReadUInt32());
            UnkValue = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            w.Write(StateName);
            w.Write(UnkValue);
        }

        public override string ToString()
        {
            return StateName.ToString() + " - " + UnkValue.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateSignalVariable : MrfStruct
    {
        public float Value { get; }//always 0 - probably float?
        public float Default { get; }
        public float RangeMin { get; }
        public float RangeMax { get; }

        public MrfStructStateSignalVariable(DataReader r)
        {
            Value = r.ReadSingle();
            Default = r.ReadSingle();
            RangeMin = r.ReadSingle();
            RangeMax = r.ReadSingle();
        }

        public override void Write(DataWriter w)
        {
            w.Write(Value);
            w.Write(Default);
            w.Write(RangeMin);
            w.Write(RangeMax);
        }

        public override string ToString()
        {
            return Value.ToString() + " - " + Default.ToString() + " - (" + RangeMin.ToString() + " - " + RangeMax.ToString() + ")";
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateSignalData : MrfStruct
    {
        public uint Type { get; set; }      //0, 2, 4, 5
        public uint NameHash { get; set; }  //only for Type==2, always 0 otherwise
        public uint Unk1 { get; set; }      //only for Type==5, always 4
        public float Unk2 { get; set; }     //only for Type==5, values: 0.0, 0.95, 0.8, 1.4, 1.8, 2.2, 0.2, 0.5  - maybe min value? always less than Unk3 when nonzero
        public float Unk3 { get; set; }     //only for Type==5, values: 1.0, 1.4, 1.8, 2.2, 2.95, 0.8, 0.5       - maybe max value?
        public uint ItemCount { get; set; } //only for Type==5
        public uint Unk4 { get; set; }      //only for Type==5, always 4

        public MrfStructStateSignalVariable[] Items { get; set; }

        public MrfStructStateSignalData(DataReader r)
        {
            Type = r.ReadUInt32();
            if (Type == 5)
            {
                Unk1 = r.ReadUInt32();
                Unk2 = r.ReadSingle();
                Unk3 = r.ReadSingle();
                ItemCount = r.ReadUInt32();
                Unk4 = r.ReadUInt32();
                Items = new MrfStructStateSignalVariable[ItemCount];
                for (int i = 0; i < ItemCount; i++)
                {
                    Items[i] = new MrfStructStateSignalVariable(r);
                }
            }
            else
            {
                NameHash = r.ReadUInt32();
            }
        }

        public override void Write(DataWriter w)
        {
            w.Write(Type);
            if (Type == 5)
            {
                w.Write(Unk1);
                w.Write(Unk2);
                w.Write(Unk3);
                w.Write(ItemCount);
                w.Write(Unk4);

                // FIXME: might be broken if changed outside
                foreach (var item in Items)
                    item.Write(w);
            }
            else
            {
                w.Write(NameHash);
            }

        }

        public override string ToString()
        {
            return Type.ToString() + " - " + NameHash.ToString() + " - " + 
                Unk1.ToString() + " - " + Unk2.ToString() + " - " + FloatUtil.ToString(Unk3) + " - " + Unk4.ToString() + " - C:" + ItemCount.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfStructStateSignal : MrfStruct
    {
        public ushort Unk1 { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }//Items.Length * 8
        public ushort Unk4 { get; }
        public MrfStructStateSignalData[] Items { get; }

        public MrfStructStateSignal(DataReader r)
        {
            Unk1 = r.ReadUInt16();
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt16();

            var itemsList = new List<MrfStructStateSignalData>();

            while (true)
            {
                var item = new MrfStructStateSignalData(r);
                itemsList.Add(item);
                if (item.Type == 0)
                {
                    break;
                }
            }

            Items = itemsList.ToArray();

        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);

            foreach (var item in Items)
                item.Write(w);
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString() + " - " + Unk3.ToString() + " - " + Unk4.ToString() + " - " +
                (Items?.Length ?? 0).ToString() + " items";
        }
    }

    
    // FIXME: most likely broken

    [TC(typeof(EXP))] public class MrfStructNegativeDataUnk7 : MrfStruct
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
        }

        public override string ToString()
        {
            return Unk1.ToString() + " - " + Unk2.ToString() + " - " + Unk3.ToString() + " - " + Unk4.ToString();
        }
    }
    
    #endregion

    #region mrf node classes
    
    [TC(typeof(EXP))] public class MrfNodeStateMachineClass : MrfNodeStateBase
    {
        // rage__mvNodeStateMachineClass (1)

        public MrfStructStateMachineState[] States { get; set; }
        public MrfStructStateMainSection[] Sections { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);


            if (StateChildCount > 0)
            {
                States = new MrfStructStateMachineState[StateChildCount];
                for (int i = 0; i < StateChildCount; i++)
                    States[i] = new MrfStructStateMachineState(r);
            }

            if (StateSectionCount > 0)
            {
                Sections = new MrfStructStateMainSection[StateSectionCount];
                for (int i = 0; i < StateSectionCount; i++)
                    Sections[i] = new MrfStructStateMainSection(r);
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if (States != null)
                foreach (var state in States)
                    state.Write(w);

            if (Sections != null)
                foreach(var section in Sections)
                    section.Write(w);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeTail : MrfNodeValueBase
    {
        // rage__mvNodeTail (2)

    }

    [TC(typeof(EXP))] public class MrfNodeInlinedStateMachine : MrfNodeStateBase
    {
        // rage__mvNodeInlinedStateMachine (3)

        public int Unk { get; set; } //usually(always?) negative, seems to be a byte offset maybe to parent node
        public MrfStructStateMachineState[] States { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk = r.ReadInt32();

            if (StateChildCount > 0)
            {
                States = new MrfStructStateMachineState[StateChildCount];
                for (int i = 0; i < StateChildCount; i++)
                    States[i] = new MrfStructStateMachineState(r);
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk);

            if (States != null)
                foreach (var item in States)
                    item.Write(w);
        }

        public override string ToString()
        {
            return base.ToString() + " - " + Unk.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeUnk4 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (4) not used in final game

        public uint Unk1 { get; set; }
        public uint Unk2_Count { get; set; }
        public byte[] Unk2_Bytes { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (Flags & 3)
            {
                case 1:
                    {
                        Unk2_Count = r.ReadUInt32();
                        Unk2_Bytes = r.ReadBytes((int)Unk2_Count);
                        break;
                    }
                case 2:
                    Unk1 = r.ReadUInt32();
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                Unk3 = r.ReadUInt32();

            if (((Flags >> 4) & 3) != 0)
                Unk4 = r.ReadUInt32();

            if (((Flags >> 6) & 3) != 0)
                Unk5 = r.ReadUInt32();

            if (((Flags >> 8) & 3) != 0)
                Unk6 = r.ReadUInt32();

            if (((Flags >> 10) & 3) != 0)
                Unk7 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (Flags & 3)
            {
                case 1:
                    {
                        w.Write(Unk2_Count);
                        w.Write(Unk2_Bytes);
                        break;
                    }
                case 2:
                    w.Write(Unk1);
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                w.Write(Unk3);

            if (((Flags >> 4) & 3) != 0)
                w.Write(Unk4);

            if (((Flags >> 6) & 3) != 0)
                w.Write(Unk5);

            if (((Flags >> 8) & 3) != 0)
                w.Write(Unk6);

            if (((Flags >> 10) & 3) != 0)
                w.Write(Unk7);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeBlend : MrfNodeBlendAddSubtractBase
    {
        // rage__mvNodeBlend (5)

    }

    [TC(typeof(EXP))] public class MrfNodeAddSubstract : MrfNodeBlendAddSubtractBase
    {
        // rage__mvNodeAddSubtract (6)

    }

    [TC(typeof(EXP))] public class MrfNodeFilter : MrfNodeFilterUnkBase
    {
        // rage__mvNodeFilter (7)

    }

    [TC(typeof(EXP))] public class MrfNodeUnk8 : MrfNodeFilterUnkBase
    {
        // rage__mvNode* (8)

    }

    [TC(typeof(EXP))] public class MrfNodeFrame : MrfNode
    {
        // rage__mvNodeFrame (9)

        public uint Unk1 { get; set; }
        public uint Flags { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();
            Flags = r.ReadUInt32();

            if ((Flags & 3) != 0)
                Unk2 = r.ReadUInt32();

            if ((Flags & 0x30) != 0)
                Unk3 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);
            w.Write(Flags);

            if ((Flags & 3) != 0)
                w.Write(Unk2);

            if ((Flags & 0x30) != 0)
                w.Write(Unk3);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeUnk10 : MrfNodeValueBase
    {
        // rage__mvNode* (10)

    }

    [TC(typeof(EXP))] public class MrfNodeBlendN : MrfNodeNegativeBase
    {
        // rage__mvNodeBlendN (13)

    }

    [TC(typeof(EXP))] public class MrfNodeClip : MrfNodeNameFlagsBase
    {
        // rage__mvNodeClip (15)

        //eg "Clip_XXX" (lowercase hash, XXX number eg 012)
        //?- delta supplement
        //?- delta
        //?- looped
        //?- phase
        //?- rate


        public MetaHash VariableName { get; set; }
        public uint Unk2 { get; set; }
        public MetaHash DictName { get; set; }
        public MetaHash ClipName { get; set; }
        public MetaHash Unk5 { get; set; }
        public MetaHash Unk6 { get; set; }
        public uint Unk7 { get; set; }
        public uint Unk8 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            switch (Flags & 3)
            {
                case 1:
                    {
                        Unk2 = r.ReadUInt32();
                        DictName = new MetaHash(r.ReadUInt32());

                        if (Unk2 != 3)
                            ClipName = new MetaHash(r.ReadUInt32());

                        break;
                    }
                case 2:
                    VariableName = new MetaHash(r.ReadUInt32());
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                Unk5 = new MetaHash(r.ReadUInt32());

            if (((Flags >> 4) & 3) != 0)
                Unk6 = new MetaHash(r.ReadUInt32());

            if (((Flags >> 6) & 3) != 0)
                Unk7 = r.ReadUInt32();

            if (((Flags >> 6) & 3) != 0)
                Unk8 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            switch (Flags & 3)
            {
                case 1:
                    {
                        w.Write(Unk2);
                        w.Write(DictName);

                        if (Unk2 != 3)
                            w.Write(ClipName);

                        break;
                    }
                case 2:
                    w.Write(VariableName);
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                w.Write(Unk5);

            if (((Flags >> 4) & 3) != 0)
                w.Write(Unk6);

            if (((Flags >> 6) & 3) != 0)
                w.Write(Unk7);

            if (((Flags >> 6) & 3) != 0)
                w.Write(Unk8);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeUnk17 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (17)

        public uint Unk1_Count { get; set; }
        public byte[] Unk1_Bytes { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint[] Unk6 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if ((Flags & 3) == 1)
            {
                Unk1_Count = r.ReadUInt32();
                Unk1_Bytes = r.ReadBytes((int)Unk1_Count);
            }
            else if ((Flags & 3) == 2)
                Unk2 = r.ReadUInt32();

            if (((Flags >> 2) & 3) != 0)
                Unk3 = r.ReadUInt32();

            if (((Flags >> 4) & 3) != 0)
                Unk4 = r.ReadUInt32();

            if ((Flags >> 6) != 0)
                Unk5 = r.ReadUInt32();

            var flags = Flags >> 19;
            var unk6Count = (Flags >> 10) & 0xF;

            Unk6 = new uint[unk6Count];

            for (int i = 0; i < unk6Count; i++)
            {
                if ((flags & 3) != 0)
                    Unk6[i] = r.ReadUInt32();

                flags >>= 2;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if ((Flags & 3) == 1)
            {
                w.Write(Unk1_Count);
                w.Write(Unk1_Bytes);
            }
            else if ((Flags & 3) == 2)
                w.Write(Unk2);

            if (((Flags >> 2) & 3) != 0)
                w.Write(Unk3);

            if (((Flags >> 4) & 3) != 0)
                w.Write(Unk4);

            if ((Flags >> 6) != 0)
                w.Write(Unk5);

            var unk6Count = (Flags >> 10) & 0xF;

            if (unk6Count > 0)
            {
                foreach (var value in Unk6)
                    w.Write(value);
            }
        }

    }

    [TC(typeof(EXP))] public class MrfNodeUnk18 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (18)

        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();

            if ((Flags & 3) != 0)
                Unk2 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);

            if ((Flags & 3) != 0)
                w.Write(Unk2);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeExpression : MrfNodeNameFlagsBase
    {
        // rage__mvNodeExpression (19)

        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public MetaHash ExpressionDict { get; set; }
        public MetaHash ExpressionName { get; set; }
        public MetaHash VariableName { get; set; }
        public uint Unk6 { get; set; }
        public uint[][] Unk7 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();

            switch (Flags & 3)
            {
                case 1:
                    ExpressionDict = new MetaHash(r.ReadUInt32());
                    ExpressionName = new MetaHash(r.ReadUInt32());
                    break;
                case 2:
                    VariableName = new MetaHash(r.ReadUInt32());
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                Unk6 = r.ReadUInt32();

            var unk7Count = (Flags >> 28);

            if (unk7Count == 0)
                return;

            var unkHeaderFlag = (Flags >> 4) & 0xFFFFFF;
            var offset = 0;

            Unk7 = new uint[unk7Count][];

            for (int i = 0; i < unk7Count; i++)
            {
                var unkTypeFlag = (unkHeaderFlag >> offset) & 3;

                var value1 = r.ReadUInt32();
                uint value2 = 0;

                if (unkTypeFlag == 2 || unkTypeFlag == 1)
                    value2 = r.ReadUInt32();

                Unk7[i] = new uint[2] { value1, value2 };

                offset += 2;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);

            switch (Flags & 3)
            {
                case 1:
                    w.Write(ExpressionDict);
                    w.Write(ExpressionName);
                    break;
                case 2:
                    w.Write(VariableName);
                    break;
            }

            if (((Flags >> 2) & 3) != 0)
                w.Write(Unk6);

            var unk7Count = (Flags >> 28);

            if (unk7Count == 0)
                return;

            var unkHeaderFlag = (Flags >> 4) & 0xFFFFFF;
            var offset = 0;

            foreach (var item in Unk7)
            {
                var unkTypeFlag = (unkHeaderFlag >> offset) & 3;

                w.Write(item[0]);

                if (unkTypeFlag == 2 || unkTypeFlag == 1)
                    w.Write(item[1]);

                offset += 2;
            }
        }

    }

    [TC(typeof(EXP))] public class MrfNodeUnk20 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (20)

        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();

            if ((Flags & 3) != 0)
                Unk2 = r.ReadUInt32();
   
            if ((Flags & 0x30) != 0)
                Unk3 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);

            if ((Flags & 3) != 0)
                w.Write(Unk2);

            if ((Flags & 0x30) != 0)
                w.Write(Unk3);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeProxy : MrfNodeNameFlagsBase
    {
        // rage__mvNodeProxy (21)

        //?- node
    }

    [TC(typeof(EXP))] public class MrfNodeAddN : MrfNodeNegativeBase
    {
        // rage__mvNodeAddN (22)

    }

    [TC(typeof(EXP))] public class MrfNodeIdentity : MrfNodeValueBase
    {
        // rage__mvNodeIdentity (23)

    }

    [TC(typeof(EXP))] public class MrfNodeUnk24 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (24)

        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public MetaHash Unk4 { get; set; }
        public MetaHash Unk5 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();

            if ((Flags & 0x180000) == 0x80000)
                Unk3 = r.ReadUInt32();

            switch (Flags & 3)
            {
                case 1:
                    Unk4 = new MetaHash(r.ReadUInt32());
                    Unk5 = new MetaHash(r.ReadUInt32());
                    break;
                case 2:
                    Unk5 = new MetaHash(r.ReadUInt32());
                    break;
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);
            w.Write(Unk2);

            if ((Flags & 0x180000) == 0x80000)
                w.Write(Unk3);

            switch (Flags & 3)
            {
                case 1:
                    w.Write(Unk4);
                    w.Write(Unk5);
                    break;
                case 2:
                    w.Write(Unk5);
                    break;
            }
        }

    }

    [TC(typeof(EXP))] public class MrfNodeUnk25 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (25)

        public uint Unk1 { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            if ((Flags & 3) != 0)
                Unk1 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            if ((Flags & 3) != 0)
                w.Write(Unk1);
        }

    }

    [TC(typeof(EXP))] public class MrfNodeMergeN : MrfNodeNegativeBase
    {
        // rage__mvNodeMergeN (26)

        //?- filter
        //?- input filter
        //?- weight
    }

    [TC(typeof(EXP))] public class MrfNodeState : MrfNodeStateBase
    {
        // rage__mvNodeState (27)

        public uint Unk1 { get; set; }
        public uint VariablesCount { get; set; }
        public uint Unk3 { get; set; }
        public uint EventsCount { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6Count { get; set; }
        public uint Unk7 { get; set; }
        public uint SignalsCount { get; set; }

        public MrfStructStateMainSection[] Sections { get; set; }
        public MrfStructStateVariable[] Variables { get; set; }
        public MrfStructStateEvent[] Events { get; set; }
        public MrfStructStateUnk6[] Unk6Items { get; set; }
        public MrfStructStateSignal[] Signals { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();
            VariablesCount = r.ReadUInt32();
            Unk3 = r.ReadUInt32();
            EventsCount = r.ReadUInt32();
            Unk5 = r.ReadUInt32();
            Unk6Count = r.ReadUInt32();
            Unk7 = r.ReadUInt32();
            SignalsCount = r.ReadUInt32();


            if (StateSectionCount > 0)
            {
                Sections = new MrfStructStateMainSection[StateSectionCount];
                for (int i = 0; i < StateSectionCount; i++)
                    Sections[i] = new MrfStructStateMainSection(r);
            }

            if (VariablesCount > 0)
            {
                Variables = new MrfStructStateVariable[VariablesCount];
                for (int i = 0; i < VariablesCount; i++)
                    Variables[i] = new MrfStructStateVariable(r);
            }

            if (EventsCount > 0)
            {
                Events = new MrfStructStateEvent[EventsCount];
                for (int i = 0; i < EventsCount; i++)
                    Events[i] = new MrfStructStateEvent(r);
            }

            if (Unk6Count > 0)
            {
                Unk6Items = new MrfStructStateUnk6[Unk6Count];
                for (int i = 0; i < Unk6Count; i++)
                    Unk6Items[i] = new MrfStructStateUnk6(r);
            }

            if (SignalsCount > 0)
            {
                Signals = new MrfStructStateSignal[SignalsCount];
                for (int i = 0; i < SignalsCount; i++)
                    Signals[i] = new MrfStructStateSignal(r);
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);
            w.Write(VariablesCount);
            w.Write(Unk3);
            w.Write(EventsCount);
            w.Write(Unk5);
            w.Write(Unk6Count);
            w.Write(Unk7);
            w.Write(SignalsCount);

            if (Sections != null)
                foreach (var section in Sections)
                    section.Write(w);

            if (Variables != null)
                foreach (var item in Variables)
                    item.Write(w);

            if (Events != null)
                foreach (var item in Events)
                    item.Write(w);

            if (Unk6Items != null)
                foreach (var item in Unk6Items)
                    item.Write(w);

            if (Signals != null)
                foreach (var item in Signals)
                    item.Write(w);
        }

        public override string ToString()
        {
            return base.ToString() + " --- " + Unk1.ToString() + " - " + Unk3.ToString() + " - " + Unk5.ToString() + " - " + Unk7.ToString();
        }
    }

    [TC(typeof(EXP))] public class MrfNodeInvalid : MrfNodeValueBase
    {
        // rage__mvNodeInvalid (28)

    }

    [TC(typeof(EXP))] public class MrfNodeUnk29 : MrfNodeFilterUnkBase
    {
        // rage__mvNode* (29)

    }

    [TC(typeof(EXP))] public class MrfNodeSubNetworkClass : MrfNodeNameFlagsBase
    {
        // rage__mvNodeSubNetworkClass (30)

    }

    [TC(typeof(EXP))] public class MrfNodeUnk31 : MrfNodeNameFlagsBase
    {
        // rage__mvNode* (31)

        public uint Unk1 { get; set; }
        public uint Unk2_Count { get; set; }
        public uint[][] Unk2_Items { get; set; }
        public uint Unk3_Count { get; set; }
        public uint[][] Unk3_Items { get; set; }
        public uint Unk4_Count { get; set; }
        public uint[][] Unk4_Items { get; set; }
        public uint Unk5_Count { get; set; }
        public uint[][] Unk5_Items { get; set; }

        public override void Read(DataReader r)
        {
            base.Read(r);

            Unk1 = r.ReadUInt32();
            Unk2_Count = r.ReadUInt32();
            Unk3_Count = r.ReadUInt32();
            Unk4_Count = r.ReadUInt32();
            Unk5_Count = r.ReadUInt32();

            if (Unk3_Count > 0)
            {
                Unk3_Items = new uint[Unk3_Count][];

                for (int i = 0; i < Unk3_Count; i++)
                {
                    var value1 = r.ReadUInt32();
                    var value2 = r.ReadUInt32();
                    Unk3_Items[i] = new uint[] { value1, value2 };
                }
            }

            if (Unk4_Count > 0)
            {
                Unk4_Items = new uint[Unk4_Count][];

                for (int i = 0; i < Unk4_Count; i++)
                {
                    var value1 = r.ReadUInt32();
                    var value2 = r.ReadUInt32();
                    Unk4_Items[i] = new uint[] { value1, value2 };
                }
            }

            if (Unk5_Count > 0)
            {
                Unk5_Items = new uint[Unk5_Count][];

                for (int i = 0; i < Unk5_Count; i++)
                {
                    var value1 = r.ReadUInt32();
                    var value2 = r.ReadUInt32();
                    Unk5_Items[i] = new uint[] { value1, value2 };
                }
            }

            if (Unk2_Count > 0)
            {
                Unk2_Items = new uint[Unk2_Count][];

                for (int i = 0; i < Unk2_Count; i++)
                {
                    var value1 = r.ReadUInt32();
                    var value2 = r.ReadUInt32();
                    var value3 = r.ReadUInt32();
                    Unk2_Items[i] = new uint[] { value1, value2, value3 };
                }
            }
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);

            w.Write(Unk1);
            w.Write(Unk2_Count);
            w.Write(Unk3_Count);
            w.Write(Unk4_Count);
            w.Write(Unk5_Count);

            if (Unk3_Count > 0)
            {
                foreach (var entry in Unk3_Items)
                {
                    w.Write(entry[0]);
                    w.Write(entry[1]);
                }
            }

            if (Unk4_Count > 0)
            {
                foreach (var entry in Unk4_Items)
                {
                    w.Write(entry[0]);
                    w.Write(entry[1]);
                }
            }

            if (Unk5_Count > 0)
            {
                foreach (var entry in Unk5_Items)
                {
                    w.Write(entry[0]);
                    w.Write(entry[1]);
                }
            }

            if (Unk2_Count > 0)
            {
                foreach (var entry in Unk2_Items)
                {
                    w.Write(entry[0]);
                    w.Write(entry[1]);
                    w.Write(entry[2]);
                }
            }
        }

    }
    
    #endregion

}
