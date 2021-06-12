using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    // Unused by GTAV: 11, 12, 14, 16
    public enum MrfNodeInfoType : ushort
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
    [TC(typeof(EXP))]
    public abstract class MrfNodeInfoBase
    {
        public abstract void Parse(DataReader r);

        public abstract long CalculateSize(DataReader r);

        public abstract MrfNodeInfoType GetInfoType();
    }

    // Not real classes, just abstractions for sharing some parsers.

    [TC(typeof(EXP))]
    public abstract class MrfNodeEightBytesBase : MrfNodeInfoBase
    {
        public MrfHeaderNodeInfo Header { get; set; }
        public uint Value { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNodeInfo(r);
            Value = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            return 8;
        }
    }

    [TC(typeof(EXP))]
    public abstract class MrfNodeBlendAddSubtractBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public MetaHash Unk5 { get; set; }
        public uint Unk6 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();

            if ((Header.Flags & 0x180000) == 0x80000)
                Unk3 = r.ReadUInt32();

            if ((Header.Flags & 3) != 0)
                Unk4 = r.ReadUInt32();

            var unkTypeFlag = (Header.Flags >> 2) & 3;

            // FIXME: optimized switch-case?
            if (unkTypeFlag != 2)
            {
                if (unkTypeFlag != 1)
                    return;

                Unk5 = new MetaHash(r.ReadUInt32());
            }

            Unk6 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 20;

            r.Position += 8;
            var headerFlag = r.ReadUInt32();
            r.Position = startPos;

            var unkTypeFlag = (headerFlag >> 2) & 3;

            if ((headerFlag & 0x180000) == 0x80000)
                result = 24;

            if ((headerFlag & 3) != 0)
                result += 4;

            if (unkTypeFlag == 1)
                result += 8;
            else if (unkTypeFlag == 2)
                result += 4;

            return result;
        }
    }

    [TC(typeof(EXP))]
    public abstract class MrfNodeFilterUnkBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public MetaHash Unk2 { get; set; }
        public uint Unk3 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();

            var unkTypeFlag = Header.Flags & 3;

            // FIXME: optimized switch-case?
            if (unkTypeFlag != 2)
            {
                if (unkTypeFlag != 1)
                    return;

                Unk2 = new MetaHash(r.ReadUInt32());
            }

            Unk3 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 8;
            var headerFlag = r.ReadUInt32();
            r.Position = startPos;

            var unkTypeFlag = (headerFlag & 3);

            if (unkTypeFlag == 2)
                return 20;

            if (unkTypeFlag == 1)
                return 24;

            return 16;
        }
    }

    [TC(typeof(EXP))]
    public abstract class MrfNodeHeaderOnlyBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
        }

        public override long CalculateSize(DataReader r)
        {
            return 12;
        }
    }

    [TC(typeof(EXP))]
    public abstract class MrfNodeNegativeBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public byte[] Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint[] Unk7 { get; set; }
        public MrfStructNegativeInfoDataUnk7[] Unk7_Items { get; set; }
        public uint[] Unk8 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);

            var unkTypeFlag1 = Header.Flags & 3;
            var unkTypeFlag2 = (Header.Flags >> 2) & 3;
            var unk7Count = Header.Flags >> 26;

            if ((Header.Flags & 0x180000) == 0x80000)
                Unk1 = r.ReadUInt32();

            if (unkTypeFlag1 == 1)
                Unk2 = r.ReadBytes(76);
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
                Unk7 = new uint[unk7Count];

                for (int i = 0; i < unk7Count; i++)
                    Unk7[i] = r.ReadUInt32();
            }

            var bufferBasePos = r.Position;
            var unk8Count = (((2 * unk7Count) | 7) + 1) >> 3;

            if (unk8Count != 0)
            {
                Unk8 = new uint[unk8Count];

                for (int i = 0; i < unk8Count; i++)
                    Unk8[i] = r.ReadUInt32();
            }

            if (unk7Count == 0)
                return;

            Unk7_Items = new MrfStructNegativeInfoDataUnk7[unk7Count];
            var iteration = 0;

            for (int i = 0; i < unk7Count; i++)
            {
                var bufferPos = r.Position;
                var readPos = bufferBasePos + 4 * (iteration >> 3);

                r.Position = readPos;
                var unkInt = r.ReadUInt32() >> (4 * (iteration & 7));
                r.Position = bufferPos;

                var item = new MrfStructNegativeInfoDataUnk7();
                var unkTypeFlag3 = (unkInt >> 4) & 3;

                if ((unkInt & 3) != 0)
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

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            var baseSize = 12;

            r.Position += 8;
            var flags = r.ReadUInt32();
            r.Position = startPos;

            var unkTypeFlag = (flags >> 2) & 3;
            var unkCount = flags >> 26;

            if ((flags & 0x180000) == 0x80000)
                baseSize += 4;

            if ((flags & 3) == 2)
                baseSize = 4;

            if (unkTypeFlag == 2)
                baseSize += 4;
            else if (unkTypeFlag == 1)
                baseSize += 8;

            long result = baseSize + 4 * unkCount + 4 * ((((2 * unkCount) | 7) + 1) >> 3);

            if (unkCount == 0)
                return result;

            var iteration = 0;
            var lastPos = r.Position + (baseSize + 4 * unkCount);

            for (int i = 0; i < unkCount; i++)
            {
                var readPos = lastPos + 4 * (iteration >> 3);
                r.Position = readPos;
                var unkInt = r.ReadUInt32();
                r.Position = startPos;

                var v9 = unkInt >> (4 * (iteration & 7));
                var v10 = (v9 >> 4) & 3;

                if ((v9 & 3) != 0)
                    result += 4;

                if (v10 == 2)
                    result += 4;
                else if (v10 == 1)
                    result += 8;

                iteration += 2;
            }

            r.Position = startPos;

            return result;
        }
    }
    #endregion

    #region mrf node structs
    [TC(typeof(EXP))]
    public struct MrfStructHeaderUnk1
    {
        public uint Size { get; set; }
        public byte[] Bytes { get; set; }

        public MrfStructHeaderUnk1(DataReader r)
        {
            Size = r.ReadUInt32();
            Bytes = r.ReadBytes((int)Size);
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructHeaderUnk2
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public MrfStructHeaderUnk2(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructHeaderUnk3
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public MrfStructHeaderUnk3(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateMainSection
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }
        public uint Unk8 { get; set; }
        public List<MrfStructStateLoopSection> Items { get; set; }

        public MrfStructStateMainSection(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();
            Unk3 = r.ReadUInt32();
            Unk4 = r.ReadUInt32();
            Unk5 = r.ReadUInt32();
            Unk6 = r.ReadUInt32();

            uint flags = Unk1 & 0xFFFBFFFF;
            var iterations = (flags >> 20) & 0xF;

            Items = new List<MrfStructStateLoopSection>();

            // FIXME: for-loop?
            while (iterations != 0)
            {
                Items.Add(new MrfStructStateLoopSection(r));

                if (--iterations == 0)
                    break;
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
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateLoopSection
    {
        public short UnkType { get; }
        public short UnkIndex { get; }
        public uint Unk1_1 { get; }
        public uint Unk1_2 { get; }
        public uint Unk1_3 { get; }
        public uint Unk2_1 { get; }
        public uint Unk3_1 { get; }
        public uint Unk3_2 { get; }

        public MrfStructStateLoopSection(DataReader r)
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
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateSection
    {
        public MrfStructStateMainSection[] Sections { get; set; }

        public MrfStructStateSection(DataReader r, uint count)
        {
            Sections = new MrfStructStateMainSection[(int)count];

            // FIXME: for-loop?
            while (true)
            {
                Sections[(int)count - 1] = new MrfStructStateMainSection(r);

                if (--count == 0)
                    return;
            }
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateInfoUnk2
    {
        public uint Unk1 { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public uint Unk4 { get; }

        public MrfStructStateInfoUnk2(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt32();
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateInfoEvent
    {
        public ushort Unk1 { get; }
        public ushort Unk2 { get; }
        public uint HashName { get; }

        public MrfStructStateInfoEvent(DataReader r)
        {
            Unk1 = r.ReadUInt16();
            Unk2 = r.ReadUInt16();
            HashName = r.ReadUInt32();
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateInfoUnk6
    {
        public uint Unk1 { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public uint Unk4 { get; }

        public MrfStructStateInfoUnk6(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt32();
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateInfoSignalDataUnk3
    {
        public uint UnkValue;
        public uint UnkDefault;
        public ulong UnkRange;

        public MrfStructStateInfoSignalDataUnk3(DataReader r)
        {
            UnkValue = r.ReadUInt32();
            UnkDefault = r.ReadUInt32();
            UnkRange = r.ReadUInt64(); // 2 merged 32 bit values?
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateInfoSignalData
    {
        public uint UnkType { get; }
        public uint NameHash { get; }
        public uint Unk1 { get; }
        public uint Unk2 { get; }
        public uint Unk3_Count { get; }
        public uint Unk4 { get; }

        public MrfStructStateInfoSignalDataUnk3[] Unk3_Items { get; }

        public MrfStructStateInfoSignalData(DataReader r)
        {
            Unk1 = 0;
            Unk2 = 0;
            Unk3_Count = 0;
            Unk3_Items = null;
            Unk4 = 0;

            UnkType = r.ReadUInt32();
            NameHash = r.ReadUInt32();

            if (UnkType != 5)
                return;

            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32(); // Default value too?
            Unk3_Count = r.ReadUInt32();
            Unk3_Items = new MrfStructStateInfoSignalDataUnk3[Unk3_Count];
            Unk4 = r.ReadUInt32();

            for (int i = 0; i < Unk3_Count; i++)
                Unk3_Items[i] = new MrfStructStateInfoSignalDataUnk3(r);
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructStateInfoSignal
    {
        public ushort Header1 { get; }
        public ushort Header2 { get; }
        public ushort Header3 { get; }
        public ushort Header4 { get; }
        public List<MrfStructStateInfoSignalData> Items { get; }

        public MrfStructStateInfoSignal(DataReader r)
        {
            Items = new List<MrfStructStateInfoSignalData>();

            Header1 = r.ReadUInt16();
            Header2 = r.ReadUInt16();
            Header3 = r.ReadUInt16();
            Header4 = r.ReadUInt16();

            uint shouldContinue;

            // FIXME: those loops looks weird
            do
            {
                while (true)
                {
                    var data = new MrfStructStateInfoSignalData(r);
                    Items.Add(data);

                    shouldContinue = data.UnkType;

                    if (data.UnkType != 5)
                        break;
                }
            }
            while (shouldContinue != 0);
        }
    }

    [TC(typeof(EXP))]
    public struct MrfStructNegativeInfoDataUnk7
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }

        public MrfStructNegativeInfoDataUnk7(DataReader r)
        {
            Unk1 = 0;
            Unk2 = 0;
            Unk3 = 0;
            Unk4 = 0;
        }
    }
    #endregion

    #region mrf node headers
    [TC(typeof(EXP))]
    public class MrfHeaderNodeInfo
    {
        public MrfNodeInfoType NodeInfoType { get; set; }

        public ushort NodeInfoUnk { get; set; }

        public static long Length { get; set; }

        public MrfHeaderNodeInfo(DataReader r)
        {
            Length = 2 + 2;
            NodeInfoType = (MrfNodeInfoType)r.ReadUInt16();
            NodeInfoUnk = r.ReadUInt16();
        }
    }

    [TC(typeof(EXP))]
    public class MrfHeaderNameFlag : MrfHeaderNodeInfo
    {
        public MetaHash NameHash { get; set; }
        public uint Flags { get; set; }

        public MrfHeaderNameFlag(DataReader r) : base(r)
        {
            Length = 4 + 4 + 4;
            NameHash = new MetaHash(r.ReadUInt32());
            Flags = r.ReadUInt32();
        }
    }

    [TC(typeof(EXP))]
    public class MrfHeaderStateMachine : MrfHeaderNodeInfo
    {
        public MetaHash StateName { get; set; } // Used as an identifier for transitions
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public byte Unk6 { get; set; }
        public byte Unk7 { get; set; }
        public uint Unk8 { get; set; }
        public uint Unk9 { get; set; }
        public uint Unk10 { get; set; }

        public MrfHeaderStateMachine(DataReader r) : base(r)
        {
            StateName = new MetaHash(r.ReadUInt32());
            Unk2 = r.ReadUInt32();
            Unk3 = r.ReadUInt32();
            Unk4 = r.ReadByte();
            Unk5 = r.ReadByte();
            Unk6 = r.ReadByte();
            Unk7 = r.ReadByte();
            Unk8 = r.ReadUInt32();
            Unk9 = r.ReadUInt32();
            Unk10 = r.ReadUInt32();
        }
    }
    #endregion

    #region mrf node classes
    // rage__mvNodeStateMachineClass (1)
    [TC(typeof(EXP))]
    public class MrfNodeStateMachineClassInfo : MrfNodeInfoBase
    {
        public MrfHeaderStateMachine Header { get; set; }
        public int[][] Items { get; set; }
        public MrfStructStateSection Header_Unk7_Data { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderStateMachine(r);
            Items = new int[Header.Unk6][];

            for (int i = 0; i < Header.Unk6; i++)
            {
                var unk1 = r.ReadInt32();
                var unk2 = r.ReadInt32();

                Items[i] = new int[] { unk1, unk2 };
            }

            if (Header.Unk7 != 0)
            {
                Header_Unk7_Data = new MrfStructStateSection(r, Header.Unk7);
            }
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 0;

            {
                r.Position += 18;
                var unk1 = r.ReadByte();
                r.Position = startPos;

                result += 8 * unk1 + 32;
            }

            {
                r.Position += 19;
                var optimizedCount = r.ReadByte();
                r.Position = startPos;

                if (optimizedCount != 0)
                {
                    r.Position += 28;
                    var nextSectionOff = r.Position + r.ReadUInt32();

                    for (int i = 0; i < optimizedCount; i++)
                    {
                        r.Position = nextSectionOff;
                        var sectionSize = (r.ReadUInt32() >> 4) & 0x3FFF;
                        nextSectionOff += sectionSize;
                        result += sectionSize;
                    }

                }

                r.Position = startPos;
            }

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.StateMachineClass;
        }
    }

    // rage__mvNodeTail (2)
    [TC(typeof(EXP))]
    public class MrfNodeTailInfo : MrfNodeEightBytesBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Tail;
    }

    // rage__mvNodeInlinedStateMachine (3)
    [TC(typeof(EXP))]
    public class MrfNodeInlinedStateMachineInfo : MrfNodeInfoBase
    {
        public MrfHeaderStateMachine Header { get; set; }
        public uint Unk { get; set; }
        public int[][] Items { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderStateMachine(r);
            Items = new int[Header.Unk6][];

            Unk = r.ReadUInt32();

            for (int i = 0; i < Header.Unk6; i++)
            {
                var unk1 = r.ReadInt32();
                var unk2 = r.ReadInt32();

                Items[i] = new int[] { unk1, unk2 };
            }
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 18;
            var length = r.ReadByte();
            r.Position = startPos;

            return 8 * length + 36;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.InlinedStateMachine;
        }
    }

    // rage__mvNode* (4)
    [TC(typeof(EXP))]
    public class MrfNodeUnk4Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2_Count { get; set; }
        public byte[] Unk2_Bytes { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);

            switch (Header.Flags & 3)
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

            if (((Header.Flags >> 2) & 3) != 0)
                Unk3 = r.ReadUInt32();

            if (((Header.Flags >> 4) & 3) != 0)
                Unk4 = r.ReadUInt32();

            if (((Header.Flags >> 6) & 3) != 0)
                Unk5 = r.ReadUInt32();

            if (((Header.Flags >> 8) & 3) != 0)
                Unk6 = r.ReadUInt32();

            if (((Header.Flags >> 10) & 3) != 0)
                Unk7 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 12;

            r.Position += 8;
            var headerFlags = r.ReadUInt32();
            r.Position = startPos;

            switch (headerFlags & 3)
            {
                case 1:
                    {
                        r.Position += 12;
                        var length = r.ReadUInt32();
                        r.Position = startPos;

                        result = length + 16;
                        break;
                    }
                case 2:
                    result = 16;
                    break;
            }

            if (((headerFlags >> 2) & 3) != 0)
                result += 4;

            if (((headerFlags >> 4) & 3) != 0)
                result += 4;

            if (((headerFlags >> 6) & 3) != 0)
                result += 4;

            if (((headerFlags >> 8) & 3) != 0)
                result += 4;

            if (((headerFlags >> 10) & 3) != 0)
                result += 4;

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk4;
        }
    }

    // rage__mvNodeBlend (5)
    [TC(typeof(EXP))]
    public class MrfNodeBlendInfo : MrfNodeBlendAddSubtractBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Blend;
    }

    // rage__mvNodeAddSubtract (6)
    [TC(typeof(EXP))]
    public class MrfNodeAddSubstractInfo : MrfNodeBlendAddSubtractBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.AddSubtract;
    }

    // rage__mvNodeFilter (7)
    [TC(typeof(EXP))]
    public class MrfNodeFilterInfo : MrfNodeFilterUnkBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Filter;
    }

    // rage__mvNode* (8)
    [TC(typeof(EXP))]
    public class MrfNodeUnk8Info : MrfNodeFilterUnkBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Unk8;
    }

    // rage__mvNodeFrame (9)
    [TC(typeof(EXP))]
    public class MrfNodeFrameInfo : MrfNodeInfoBase
    {
        public MrfHeaderNodeInfo Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Flags { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNodeInfo(r);

            Unk1 = r.ReadUInt32();
            Flags = r.ReadUInt32();

            if ((Flags & 3) != 0)
                Unk2 = r.ReadUInt32();

            if ((Flags & 0x30) != 0)
                Unk3 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 12;

            r.Position += 8;
            var flags = r.ReadUInt32();
            r.Position = startPos;

            if ((flags & 3) != 0)
                result = 16;

            if ((flags & 0x30) != 0)
                result += 4;

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Frame;
        }
    }

    // rage__mvNode* (10)
    [TC(typeof(EXP))]
    public class MrfNodeUnk10Info : MrfNodeEightBytesBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Tail;
    }

    // rage__mvNodeBlendN (13)
    [TC(typeof(EXP))]
    public class MrfNodeBlendNInfo : MrfNodeNegativeBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.BlendN;
    }

    // rage__mvNodeClip (15)
    [TC(typeof(EXP))]
    public class MrfNodeClipInfo : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public MetaHash DictName { get; set; }
        public MetaHash ClipName { get; set; }
        public float Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }
        public uint Unk8 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);

            switch (Header.Flags & 3)
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
                    Unk1 = r.ReadUInt32();
                    break;
            }

            if (((Header.Flags >> 2) & 3) != 0)
                Unk5 = r.ReadSingle();

            if (((Header.Flags >> 4) & 3) != 0)
                Unk6 = r.ReadUInt32();

            if (((Header.Flags >> 6) & 3) != 0)
                Unk7 = r.ReadUInt32();

            if (((Header.Flags >> 6) & 3) != 0)
                Unk8 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 4;

            r.Position += 8;
            var headerFlags = r.ReadUInt32();
            r.Position = startPos;

            switch (headerFlags & 3)
            {
                case 1:
                    {
                        r.Position += 12;
                        var unk4 = r.ReadUInt32();
                        r.Position = startPos;

                        result = (unk4 != 3) ? 24 : 20;
                        break;
                    }
                case 2:
                    result = 16;
                    break;
            }

            if (((headerFlags >> 2) & 3) != 0)
                result += 4;

            if (((headerFlags >> 4) & 3) != 0)
                result += 4;

            if (((headerFlags >> 6) & 3) != 0)
                result += 4;

            if (((headerFlags >> 8) & 3) != 0)
                result += 4;

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Clip;
        }
    }

    // rage__mvNode* (17)
    [TC(typeof(EXP))]
    public class MrfNodeUnk17Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1_Count { get; set; }
        public byte[] Unk1_Bytes { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint[] Unk6 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);

            if ((Header.Flags & 3) == 1)
            {
                Unk1_Count = r.ReadUInt32();
                Unk1_Bytes = r.ReadBytes((int)Unk1_Count);
            }
            else if ((Header.Flags & 3) == 2)
                Unk2 = r.ReadUInt32();

            if (((Header.Flags >> 2) & 3) != 0)
                Unk3 = r.ReadUInt32();

            if (((Header.Flags >> 4) & 3) != 0)
                Unk4 = r.ReadUInt32();

            if ((Header.Flags >> 6) != 0)
                Unk5 = r.ReadUInt32();

            var flags = Header.Flags >> 19;
            var unk6Count = (Header.Flags >> 10) & 0xF;

            Unk6 = new uint[unk6Count];

            for (int i = 0; i < unk6Count; i++)
            {
                if ((flags & 3) != 0)
                    Unk6[i] = r.ReadUInt32();

                flags >>= 2;
            }
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 12;

            r.Position += 8;
            var headerFlags = r.ReadUInt32();
            r.Position = startPos;

            if ((headerFlags & 3) == 1)
            {
                r.Position += 12;
                var length = r.ReadUInt32();
                r.Position = startPos;

                result = length + 16;
            }
            else if ((headerFlags & 3) == 2)
                result = 16;

            if (((headerFlags >> 2) & 3) != 0)
                result += 4;

            if (((headerFlags >> 4) & 3) != 0)
                result += 4;

            if ((headerFlags >> 6) != 0)
                result += 4;

            var unk6Count = (headerFlags >> 10) & 0xF;

            if (unk6Count == 0)
                return result;

            var flags = headerFlags >> 19;

            for (int i = 0; i < unk6Count; i++)
            {
                if ((flags & 3) != 0)
                    result += 4;

                flags >>= 2;
            }

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk17;
        }
    }

    // rage__mvNode* (18)
    [TC(typeof(EXP))]
    public class MrfNodeUnk18Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();

            if ((Header.Flags & 3) != 0)
                Unk2 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 8;
            var headerFlags = r.ReadUInt32();
            r.Position = startPos;

            return ((headerFlags & 3) != 0) ? 20 : 16;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk18;
        }
    }

    // rage__mvNodeExpression (19)
    [TC(typeof(EXP))]
    public class MrfNodeExpressionInfo : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public MetaHash Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public float Unk6 { get; set; }
        public uint[][] Unk7 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();

            switch (Header.Flags & 3)
            {
                case 1:
                    Unk3 = new MetaHash(r.ReadUInt32());
                    Unk4 = r.ReadUInt32();
                    break;
                case 2:
                    Unk5 = r.ReadUInt32();
                    break;
            }

            if (((Header.Flags >> 2) & 3) != 0)
                Unk6 = r.ReadSingle();

            var unk7Count = (Header.Flags >> 28);

            if (unk7Count == 0)
                return;

            var unkOffset = (Header.Flags >> 4) & 0xFFFFFF;
            var offset = 0;

            Unk7 = new uint[unk7Count][];

            for (int i = 0; i < unk7Count; i++)
            {
                var unkFlag = (unkOffset >> offset) & 3;

                var value1 = r.ReadUInt32();
                uint value2 = 0;

                if (unkFlag == 2 || unkFlag == 1)
                    value2 = r.ReadUInt32();

                Unk7[i] = new uint[2] { value1, value2 };

                offset += 2;
            }
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 16;

            r.Position += 8;
            var headerFlag = r.ReadUInt32();
            r.Position = startPos;

            switch (headerFlag & 3)
            {
                case 1:
                    result = 24;
                    break;
                case 2:
                    result = 2;
                    break;
            }

            if (((headerFlag >> 2) & 3) != 0)
                result += 4;

            var unk7Count = (headerFlag >> 28);

            if (unk7Count == 0)
                return result;

            var unkOffset = (headerFlag >> 4) & 0xFFFFFF;
            var offset = 0;

            for (int i = 0; i < unk7Count; i++)
            {
                result += 4;
                var unkFlag = (unkOffset >> offset) & 3;

                if (unkFlag == 2 || unkFlag == 1)
                    result += 4;

                offset += 2;
            }

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Expression;
        }
    }

    // rage__mvNode* (20)
    [TC(typeof(EXP))]
    public class MrfNodeUnk20Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();

            if ((Header.Flags & 3) != 0)
                Unk2 = r.ReadUInt32();
   
            if ((Header.Flags & 0x30) != 0)
                Unk3 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 16;

            r.Position += 8;
            var flags = r.ReadUInt32();
            r.Position = startPos;

            if ((flags & 3) != 0)
                result = 20;

            if (((flags & 3) & 0x30) != 0)
                result += 4;

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk20;
        }
    }

    // rage__mvNodeProxy (21)
    [TC(typeof(EXP))]
    public class MrfNodeProxyInfo : MrfNodeHeaderOnlyBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Proxy;
    }

    // rage__mvNodeAddN (22)
    [TC(typeof(EXP))]
    public class MrfNodeAddNInfo : MrfNodeNegativeBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.AddN;
    }

    // rage__mvNodeIdentity (23)
    [TC(typeof(EXP))]
    public class MrfNodeIdentityInfo : MrfNodeEightBytesBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Identity;
    }

    // rage__mvNode* (24)
    [TC(typeof(EXP))]
    public class MrfNodeUnk24Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public MetaHash Unk4 { get; set; }
        public uint Unk5 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk2 = r.ReadUInt32();
            Unk3 = r.ReadUInt32();

            if ((Header.Flags & 0x180000) == 0x80000)
                Unk3 = r.ReadUInt32();

            var unkTypeFlag = (Header.Flags & 3);

            // FIXME: optimized switch-case?
            if (unkTypeFlag != 2)
            {
                if (unkTypeFlag != 1)
                    return;

                Unk4 = new MetaHash(r.ReadUInt32());
            }

            Unk5 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 20;

            r.Position += 8;
            var headerFlags = r.ReadUInt32();
            r.Position = startPos;

            if ((headerFlags & 0x180000) == 0x80000)
                result = 24;

            switch (headerFlags & 3)
            {
                case 1:
                    result += 8;
                    break;
                case 2:
                    result += 4;
                    break;
            }

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk24;
        }
    }

    // rage__mvNode* (25)
    [TC(typeof(EXP))]
    public class MrfNodeUnk25Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);

            if ((Header.Flags & 3) != 0)
                Unk1 = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 8;
            var headerFlag = r.ReadUInt32();
            r.Position = startPos;

            return ((headerFlag & 3) != 0) ? 16 : 12;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk25;
        }
    }

    // rage__mvNodeMergeN (26)
    [TC(typeof(EXP))]
    public class MrfNodeMergeNInfo : MrfNodeNegativeBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.MergeN;
    }

    // rage__mvNodeState (27)
    [TC(typeof(EXP))]
    public class MrfNodeStateInfo : MrfNodeInfoBase
    {
        public MrfHeaderStateMachine Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2_Count { get; set; }
        public uint Unk3 { get; set; }
        public uint EventsCount { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6_Count { get; set; }
        public uint Unk7 { get; set; }
        public uint SignalsCount { get; set; }

        public MrfStructStateSection Header_Unk7_Data { get; set; }
        public MrfStructStateInfoUnk2[] Unk2_Items { get; set; }
        public MrfStructStateInfoEvent[] EventsItems { get; set; }
        public MrfStructStateInfoUnk6[] Unk6_Items { get; set; }
        public MrfStructStateInfoSignal[] SignalsItems { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderStateMachine(r);

            Unk1 = r.ReadUInt32();
            Unk2_Count = r.ReadUInt32();
            Unk3 = r.ReadUInt32();
            EventsCount = r.ReadUInt32();
            Unk5 = r.ReadUInt32();
            Unk6_Count = r.ReadUInt32();
            Unk7 = r.ReadUInt32();
            SignalsCount = r.ReadUInt32();

            Unk2_Items = new MrfStructStateInfoUnk2[Unk2_Count];
            EventsItems = new MrfStructStateInfoEvent[EventsCount];
            Unk6_Items = new MrfStructStateInfoUnk6[Unk6_Count];
            SignalsItems = new MrfStructStateInfoSignal[SignalsCount];

            if (Header.Unk7 != 0)
                Header_Unk7_Data = new MrfStructStateSection(r, Header.Unk7);

            for (int i = 0; i < Unk2_Count; i++)
                Unk2_Items[i] = new MrfStructStateInfoUnk2(r);

            for (int i = 0; i < EventsCount; i++)
                EventsItems[i] = new MrfStructStateInfoEvent(r);

            for (int i = 0; i < Unk6_Count; i++)
                Unk6_Items[i] = new MrfStructStateInfoUnk6(r);

            if (SignalsCount != 0)
            {
                for (int i = 0; i < SignalsCount; i++)
                    SignalsItems[i] = new MrfStructStateInfoSignal(r);
            }
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;
            long result = 64;

            {
                r.Position += 19;
                var optimizedCount = r.ReadByte();
                r.Position = startPos;

                if (optimizedCount != 0)
                {
                    r.Position += 28;
                    var nextSectionOff = r.Position + r.ReadUInt32();

                    for (int i = 0; i < optimizedCount; i++)
                    {
                        r.Position = nextSectionOff;
                        var sectionSize = (r.ReadUInt32() >> 4) & 0x3FFF;
                        nextSectionOff += sectionSize;
                        result += sectionSize;
                    }
                }

                r.Position = startPos;
            }

            {
                r.Position += 36;
                var unk12 = r.ReadUInt32();
                r.Position = startPos;

                r.Position += 52;
                var unk14 = r.ReadUInt32();
                r.Position = startPos;

                r.Position += 44;
                var unk16 = r.ReadUInt32();
                r.Position = startPos;

                result += 12 * (unk12 + unk14) + 8 * unk16;
            }

            {
                r.Position += 60;
                var iterations = r.ReadUInt32();
                r.Position = startPos;

                if (iterations != 0)
                {
                    r.Position += 56;
                    var offsetPtr = r.Position + r.ReadUInt32();
                    r.Position = startPos;

                    for (int i = 0; i < iterations; i++)
                    {
                        result += 8;
                        offsetPtr += 8;

                        uint shouldContinue;

                        // FIXME: those loops can be simplified
                        do
                        {
                            while (true)
                            {
                                r.Position = offsetPtr;
                                shouldContinue = r.ReadUInt32();
                                r.Position = offsetPtr;

                                if (shouldContinue != 5)
                                    break;

                                r.Position = offsetPtr + 4;
                                var unkCount = r.ReadUInt32();
                                r.Position = offsetPtr;

                                long targetOffset = (offsetPtr + 4 + unkCount);

                                r.Position = targetOffset + 8;
                                var length1 = r.ReadUInt32();
                                r.Position = targetOffset + 12;
                                var length2 = r.ReadInt32();
                                r.Position = targetOffset;

                                result += 16 * length1 + 24;
                                offsetPtr = (targetOffset + 16 * length1 + 12 + length2);
                            }

                            result += 8;
                            offsetPtr += 8;
                        }
                        while (shouldContinue != 0);
                    }
                }
            }

            r.Position = startPos;

            return result;
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.State;
        }
    }

    // rage__mvNodeInvalid (28)
    [TC(typeof(EXP))]
    public class MrfNodeInvalidInfo : MrfNodeEightBytesBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Invalid;
    }

    // rage__mvNode* (29)
    [TC(typeof(EXP))]
    public class MrfNodeUnk29Info : MrfNodeFilterUnkBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.Unk29;
    }

    // rage__mvNodeSubNetworkClass (30)
    [TC(typeof(EXP))]
    public class MrfNodeSubNetworkClassInfo : MrfNodeHeaderOnlyBase
    {
        public override MrfNodeInfoType GetInfoType() => MrfNodeInfoType.SubNetworkClass;
    }

    // rage__mvNode* (31)
    [TC(typeof(EXP))]
    public class MrfNodeUnk31Info : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2_Count { get; set; }
        public uint[][] Unk2_Items { get; set; }
        public uint Unk3_Count { get; set; }
        public uint[][] Unk3_Items { get; set; }
        public uint Unk4_Count { get; set; }
        public uint[][] Unk4_Items { get; set; }
        public uint Unk5_Count { get; set; }
        public uint[][] Unk5_Items { get; set; }

        public override void Parse(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
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

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 16;
            var count1 = r.ReadUInt32();
            var count2 = r.ReadUInt32();
            var count3 = r.ReadUInt32();
            var count4 = r.ReadUInt32();
            r.Position = startPos;

            return 12 * count1 + 8 * (count2 + count3 + count4 + 4);
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.Unk31;
        }
    }
    #endregion

    [TC(typeof(EXP))]
    public class MrfFile : GameFile, PackedFile
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

        public MrfNodeInfoBase[] Nodes { get; set; }

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
            // TODO
            var buf = new byte[4];
            return buf;
        }

        private void Write(DataWriter w)
        {
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

            var nodeInfos = new List<MrfNodeInfoBase>();

            while (true)
            {
                // TODO: probably not safe enough as there might be a "lucky" case of bytes that will fit node type index.
                var nodeType = GetNextMrfNodeTypeIndex(r);

                if (nodeType == MrfNodeInfoType.None)
                    break;

                var nodeHandler = GetNextMrfNodeHandler(nodeType);
                var expectedSize = nodeHandler.CalculateSize(r);
                var expectedPos = r.Position + expectedSize;

                if (expectedPos > r.Length)
                    throw new Exception($"Calculated mrf node ({nodeType}) expected size is invalid ({expectedSize})");

                if (nodeType != nodeHandler.GetInfoType())
                    throw new Exception($"Parsed mrf node type ({nodeHandler.GetInfoType()}) is not equals expected type ({nodeType})");

                // Finally we can parse it.
                nodeHandler.Parse(r);

                if (expectedPos != r.Position)
                    throw new Exception($"Position of reader ({r.Position}) is not equals expected position ({expectedPos})");

                nodeInfos.Add(nodeHandler);
            }

            Nodes = new MrfNodeInfoBase[nodeInfos.Count];

            // Hacky remapping...
            for (int i = 0; i < nodeInfos.Count; i++)
                Nodes[i] = nodeInfos[i];

            if (FlagsCount != 0)
            {
                FlagsItems = new byte[FlagsCount];

                for (int i = 0; i < FlagsCount; i++)
                    FlagsItems[i] = r.ReadByte();
            }

            if (r.Position != r.Length)
                throw new Exception($"Failed to read MRF ({r.Position} / {r.Length})");
        }

        private MrfNodeInfoType GetNextMrfNodeTypeIndex(DataReader r)
        {
            var startPos = r.Position;
            var nodeInfoType = (MrfNodeInfoType)r.ReadUInt16();
            r.Position = startPos;

            if (nodeInfoType <= MrfNodeInfoType.None || nodeInfoType >= MrfNodeInfoType.Max)
                return MrfNodeInfoType.None;

            return nodeInfoType;
        }

        private MrfNodeInfoBase GetNextMrfNodeHandler(MrfNodeInfoType infoType)
        {
            if (infoType == MrfNodeInfoType.None)
                throw new Exception($"Attempt to get a handler of none mrf node");

            switch (infoType)
            {
                case MrfNodeInfoType.StateMachineClass:
                    return new MrfNodeStateMachineClassInfo();
                case MrfNodeInfoType.Tail:
                    return new MrfNodeTailInfo();
                case MrfNodeInfoType.InlinedStateMachine:
                    return new MrfNodeInlinedStateMachineInfo();
                case MrfNodeInfoType.Unk4:
                    return new MrfNodeUnk4Info();
                case MrfNodeInfoType.Blend:
                    return new MrfNodeBlendInfo();
                case MrfNodeInfoType.AddSubtract:
                    return new MrfNodeAddSubstractInfo();
                case MrfNodeInfoType.Filter:
                    return new MrfNodeFilterInfo();
                case MrfNodeInfoType.Unk8:
                    return new MrfNodeUnk8Info();
                case MrfNodeInfoType.Frame:
                    return new MrfNodeFrameInfo();
                case MrfNodeInfoType.Unk10:
                    return new MrfNodeUnk10Info();
                case MrfNodeInfoType.BlendN:
                    return new MrfNodeBlendNInfo();
                case MrfNodeInfoType.Clip:
                    return new MrfNodeClipInfo();
                case MrfNodeInfoType.Unk17:
                    return new MrfNodeUnk17Info();
                case MrfNodeInfoType.Unk18:
                    return new MrfNodeUnk18Info();
                case MrfNodeInfoType.Expression:
                    return new MrfNodeExpressionInfo();
                case MrfNodeInfoType.Unk20:
                    return new MrfNodeUnk20Info();
                case MrfNodeInfoType.Proxy:
                    return new MrfNodeProxyInfo();
                case MrfNodeInfoType.AddN:
                    return new MrfNodeAddNInfo();
                case MrfNodeInfoType.Identity:
                    return new MrfNodeIdentityInfo();
                case MrfNodeInfoType.Unk24:
                    return new MrfNodeUnk24Info();
                case MrfNodeInfoType.Unk25:
                    return new MrfNodeUnk25Info();
                case MrfNodeInfoType.MergeN:
                    return new MrfNodeMergeNInfo();
                case MrfNodeInfoType.State:
                    return new MrfNodeStateInfo();
                case MrfNodeInfoType.Invalid:
                    return new MrfNodeInvalidInfo();
                case MrfNodeInfoType.Unk29:
                    return new MrfNodeUnk29Info();
                case MrfNodeInfoType.SubNetworkClass:
                    return new MrfNodeSubNetworkClassInfo();
                case MrfNodeInfoType.Unk31:
                    return new MrfNodeUnk31Info();
            }

            throw new Exception($"A handler for ({infoType}) mrf node type is not implemented");
        }
    }
}
