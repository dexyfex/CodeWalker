using System;
using System.IO;
using System.Collections.Generic;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    // Unused node indexes by GTAV: 11, 12, 14, 16
    // Exist in GTAV but not used in MRFs: 4, 8, 10, 17, 21, 22, 28, 29, 31, 32
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
        public abstract void Read(DataReader r);

        public abstract long CalculateSize(DataReader r);

        public abstract void Write(DataWriter w);

        public abstract MrfNodeInfoType GetInfoType();
    }

    // Not real classes, just abstractions for sharing some parsers.

    [TC(typeof(EXP))]
    public abstract class MrfNodeEightBytesBase : MrfNodeInfoBase
    {
        public MrfHeaderNodeInfo Header { get; set; }
        public uint Value { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNodeInfo(r);
            Value = r.ReadUInt32();
        }

        public override long CalculateSize(DataReader r)
        {
            return 8;
        }

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Value);
        }
    }

    [TC(typeof(EXP))]
    public abstract class MrfNodeBlendAddSubtractBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public int Unk1 { get; set; }
        public int Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public MetaHash Unk4 { get; set; }
        public MetaHash Unk5 { get; set; }
        public MetaHash Unk6 { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadInt32();
            Unk2 = r.ReadInt32();

            if ((Header.Flags & 0x180000) == 0x80000)
                Unk3 = r.ReadUInt32();

            if ((Header.Flags & 3) != 0)
                Unk4 = new MetaHash(r.ReadUInt32());

            switch ((Header.Flags >> 2) & 3)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk1);
            w.Write(Unk2);

            if ((Header.Flags & 0x180000) == 0x80000)
                w.Write(Unk3);

            if ((Header.Flags & 3) != 0)
                w.Write(Unk4);

            switch ((Header.Flags >> 2) & 3)
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

    [TC(typeof(EXP))]
    public abstract class MrfNodeFilterUnkBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }
        public uint Unk1 { get; set; }
        public MetaHash Unk2 { get; set; }
        public MetaHash Unk3 { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();

            switch (Header.Flags & 3)
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

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 8;
            var headerFlag = r.ReadUInt32();
            r.Position = startPos;

            var unkTypeFlag = headerFlag & 3;

            if (unkTypeFlag == 2)
                return 20;

            if (unkTypeFlag == 1)
                return 24;

            return 16;
        }

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk1);

            switch (Header.Flags & 3)
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

    [TC(typeof(EXP))]
    public abstract class MrfNodeHeaderOnlyBase : MrfNodeInfoBase
    {
        public MrfHeaderNameFlag Header { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
        }

        public override long CalculateSize(DataReader r)
        {
            return 12;
        }

        public override void Write(DataWriter w)
        {
            Header.Write(w);
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
        public int[] Unk7 { get; set; }
        public MrfStructNegativeInfoDataUnk7[] Unk7_Items { get; set; }
        public uint[] Unk8 { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);

            var unkTypeFlag1 = Header.Flags & 3;
            var unkTypeFlag2 = (Header.Flags >> 2) & 3;
            var unk7Count = Header.Flags >> 26;

            if ((Header.Flags & 0x180000) == 0x80000)
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

            Unk7_Items = new MrfStructNegativeInfoDataUnk7[unk7Count];
            int iteration = 0;

            for (int i = 0; i < unk7Count; i++)
            {
                var unk8Value = Unk8[iteration >> 3];
                var unk7Flag = unk8Value >> (4 * (iteration & 7));
                var unkTypeFlag3 = (unk7Flag >> 4) & 3;

                var item = new MrfStructNegativeInfoDataUnk7();

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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            var unkTypeFlag1 = Header.Flags & 3;
            var unkTypeFlag2 = (Header.Flags >> 2) & 3;
            var unk7Count = Header.Flags >> 26;

            if ((Header.Flags & 0x180000) == 0x80000)
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
    public abstract class MrfStructBase
    {
        public abstract void Write(DataWriter w);
    }

    [TC(typeof(EXP))]
    public class MrfStructHeaderUnk1 : MrfStructBase
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
    }

    [TC(typeof(EXP))]
    public class MrfStructHeaderUnk2 : MrfStructBase
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
    }

    [TC(typeof(EXP))]
    public class MrfStructHeaderUnk3 : MrfStructBase
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
    }

    [TC(typeof(EXP))]
    public class MrfStructStateMainSection : MrfStructBase
    {
        public uint Unk1 { get; set; }
        public int Unk2 { get; set; }
        public float Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }
        public uint Unk8 { get; set; }
        public MrfStructStateLoopSection[] Items { get; set; }

        public MrfStructStateMainSection(DataReader r)
        {
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadInt32();
            Unk3 = r.ReadSingle();
            Unk4 = r.ReadUInt32();
            Unk5 = r.ReadUInt32();
            Unk6 = r.ReadUInt32();

            uint flags = Unk1 & 0xFFFBFFFF;
            var iterations = (flags >> 20) & 0xF;

            Items = new MrfStructStateLoopSection[iterations];

            // FIXME: for-loop?
            while (iterations != 0)
            {
                Items[iterations - 1] = new MrfStructStateLoopSection(r);

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

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(Unk6);

            // FIXME: might be broken if changed without flags, see "iterations"
            foreach (var item in Items)
                item.Write(w);

            // FIXME: might be broken if changed without flags
            uint flags = Unk1 & 0xFFFBFFFF;

            if ((flags & 0x40000000) != 0)
            {
                w.Write(Unk7);
                w.Write(Unk8);
            }
        }
    }

    [TC(typeof(EXP))]
    public class MrfStructStateLoopSection : MrfStructBase
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
    }

    [TC(typeof(EXP))]
    public class MrfStructStateSection : MrfStructBase
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

        public override void Write(DataWriter w)
        {
            foreach (var section in Sections)
                section.Write(w);
        }
    }

    [TC(typeof(EXP))]
    public class MrfStructStateInfoVariable : MrfStructBase
    {
        public MetaHash VariableName { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public uint Unk4 { get; }

        public MrfStructStateInfoVariable(DataReader r)
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
    }

    [TC(typeof(EXP))]
    public class MrfStructStateInfoEvent : MrfStructBase
    {
        public ushort Unk1 { get; }
        public ushort Unk2 { get; }
        public MetaHash NameHash { get; }

        public MrfStructStateInfoEvent(DataReader r)
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
    }

    [TC(typeof(EXP))]
    public class MrfStructStateInfoUnk6 : MrfStructBase
    {
        public MetaHash Unk1 { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public uint Unk4 { get; }

        public MrfStructStateInfoUnk6(DataReader r)
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
    }

    [TC(typeof(EXP))]
    public class MrfStructStateInfoSignalDataUnk3 : MrfStructBase
    {
        public uint UnkValue { get; }
        public uint UnkDefault { get; }
        public ulong UnkRange { get; }

        public MrfStructStateInfoSignalDataUnk3(DataReader r)
        {
            UnkValue = r.ReadUInt32();
            UnkDefault = r.ReadUInt32();
            UnkRange = r.ReadUInt64(); // 2 merged 32 bit values?
        }

        public override void Write(DataWriter w)
        {
            w.Write(UnkValue);
            w.Write(UnkDefault);
            w.Write(UnkRange);
        }
    }

    [TC(typeof(EXP))]
    public class MrfStructStateMachineState : MrfStructBase
    {
        public MetaHash StateName { get; }
        public uint UnkValue { get; }

        public MrfNodeStateInfo __StateInfoNode { get; set; }

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
    }

    // FIXME: most likely broken

    [TC(typeof(EXP))]
    public class MrfStructStateInfoSignalData : MrfStructBase
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
            UnkType = r.ReadUInt32();
            NameHash = r.ReadUInt32();

            if (UnkType != 5)
                return;

            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32(); // Default value too?
            Unk3_Count = r.ReadUInt32();
            Unk4 = r.ReadUInt32();

            Unk3_Items = new MrfStructStateInfoSignalDataUnk3[Unk3_Count];

            for (int i = 0; i < Unk3_Count; i++)
                Unk3_Items[i] = new MrfStructStateInfoSignalDataUnk3(r);
        }

        public override void Write(DataWriter w)
        {
            w.Write(UnkType);
            w.Write(NameHash);

            if (UnkType != 5)
                return;

            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3_Count);
            w.Write(Unk4);

            // FIXME: might be broken if changed outside
            foreach (var item in Unk3_Items)
                item.Write(w);
        }
    }

    [TC(typeof(EXP))]
    public class MrfStructStateInfoSignal : MrfStructBase
    {
        public ushort Unk1 { get; }
        public ushort Unk2 { get; }
        public ushort Unk3 { get; }
        public ushort Unk4 { get; }
        public MrfStructStateInfoSignalData[] Items { get; }

        public MrfStructStateInfoSignal(DataReader r)
        {
            Unk1 = r.ReadUInt16();
            Unk2 = r.ReadUInt16();
            Unk3 = r.ReadUInt16();
            Unk4 = r.ReadUInt16();

            uint shouldContinue;
            var itemsList = new List<MrfStructStateInfoSignalData>();

            // FIXME: those loops looks weird
            do
            {
                while (true)
                {
                    var data = new MrfStructStateInfoSignalData(r);
                    itemsList.Add(data);

                    shouldContinue = data.UnkType;

                    if (data.UnkType != 5)
                        break;
                }
            }
            while (shouldContinue != 0);

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
    }

    [TC(typeof(EXP))]
    public class MrfStructNegativeInfoDataUnk7 : MrfStructBase
    {
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }

        public MrfStructNegativeInfoDataUnk7()
        {
            Unk1 = 0;
            Unk2 = 0;
            Unk3 = 0;
            Unk4 = 0;
        }

        public override void Write(DataWriter w)
        {
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
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

        public virtual void Write(DataWriter w)
        {
            w.Write((ushort)NodeInfoType);
            w.Write(NodeInfoUnk);
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

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(NameHash);
            w.Write(Flags);
        }
    }

    [TC(typeof(EXP))]
    public class MrfHeaderStateBase : MrfHeaderNodeInfo
    {
        public MetaHash NameHash { get; set; } // Used as an identifier for transitions
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public byte StateCount { get; set; }
        public byte Unk7 { get; set; }
        public uint Unk8 { get; set; }
        public uint Unk9 { get; set; }
        public uint Unk10 { get; set; }

        public MrfHeaderStateBase(DataReader r) : base(r)
        {
            NameHash = new MetaHash(r.ReadUInt32());
            Unk2 = r.ReadUInt32();
            Unk3 = r.ReadUInt32();
            Unk4 = r.ReadByte();
            Unk5 = r.ReadByte();
            StateCount = r.ReadByte();
            Unk7 = r.ReadByte();
            Unk8 = r.ReadUInt32();
            Unk9 = r.ReadUInt32();
            Unk10 = r.ReadUInt32();
        }

        public override void Write(DataWriter w)
        {
            base.Write(w);
            w.Write(NameHash);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(StateCount);
            w.Write(Unk7);
            w.Write(Unk8);
            w.Write(Unk9);
            w.Write(Unk10);
        }
    }
    #endregion

    #region mrf node classes
    // rage__mvNodeStateMachineClass (1)
    [TC(typeof(EXP))]
    public class MrfNodeStateMachineClassInfo : MrfNodeInfoBase
    {
        public MrfHeaderStateBase Header { get; set; }
        public MrfStructStateMachineState[] States { get; set; }
        public MrfStructStateSection Header_Unk7_Data { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderStateBase(r);
            States = new MrfStructStateMachineState[Header.StateCount];

            for (int i = 0; i < Header.StateCount; i++)
                States[i] = new MrfStructStateMachineState(r);

            if (Header.Unk7 != 0)
                Header_Unk7_Data = new MrfStructStateSection(r, Header.Unk7);
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            foreach (var item in States)
                item.Write(w);

            if (Header.Unk7 != 0)
                Header_Unk7_Data.Write(w);
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
        public MrfHeaderStateBase Header { get; set; }
        public uint Unk { get; set; }
        public MrfStructStateMachineState[] Items { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderStateBase(r);
            Items = new MrfStructStateMachineState[Header.StateCount];

            Unk = r.ReadUInt32();

            for (int i = 0; i < Header.StateCount; i++)
                Items[i] = new MrfStructStateMachineState(r);
        }

        public override long CalculateSize(DataReader r)
        {
            var startPos = r.Position;

            r.Position += 18;
            var length = r.ReadByte();
            r.Position = startPos;

            return 8 * length + 36;
        }

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk);

            foreach (var item in Items)
                item.Write(w);
        }

        public override MrfNodeInfoType GetInfoType()
        {
            return MrfNodeInfoType.InlinedStateMachine;
        }
    }

    // rage__mvNode* (4) not used in final game
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            switch (Header.Flags & 3)
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

            if (((Header.Flags >> 2) & 3) != 0)
                w.Write(Unk3);

            if (((Header.Flags >> 4) & 3) != 0)
                w.Write(Unk4);

            if (((Header.Flags >> 6) & 3) != 0)
                w.Write(Unk5);

            if (((Header.Flags >> 8) & 3) != 0)
                w.Write(Unk6);

            if (((Header.Flags >> 10) & 3) != 0)
                w.Write(Unk7);
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            w.Write(Unk1);
            w.Write(Flags);

            if ((Flags & 3) != 0)
                w.Write(Unk2);

            if ((Flags & 0x30) != 0)
                w.Write(Unk3);
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
                    VariableName = new MetaHash(r.ReadUInt32());
                    break;
            }

            if (((Header.Flags >> 2) & 3) != 0)
                Unk5 = new MetaHash(r.ReadUInt32());

            if (((Header.Flags >> 4) & 3) != 0)
                Unk6 = new MetaHash(r.ReadUInt32());

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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            switch (Header.Flags & 3)
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

            if (((Header.Flags >> 2) & 3) != 0)
                w.Write(Unk5);

            if (((Header.Flags >> 4) & 3) != 0)
                w.Write(Unk6);

            if (((Header.Flags >> 6) & 3) != 0)
                w.Write(Unk7);

            if (((Header.Flags >> 6) & 3) != 0)
                w.Write(Unk8);
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            if ((Header.Flags & 3) == 1)
            {
                w.Write(Unk1_Count);
                w.Write(Unk1_Bytes);
            }
            else if ((Header.Flags & 3) == 2)
                w.Write(Unk2);

            if (((Header.Flags >> 2) & 3) != 0)
                w.Write(Unk3);

            if (((Header.Flags >> 4) & 3) != 0)
                w.Write(Unk4);

            if ((Header.Flags >> 6) != 0)
                w.Write(Unk5);

            var unk6Count = (Header.Flags >> 10) & 0xF;

            if (unk6Count > 0)
            {
                foreach (var value in Unk6)
                    w.Write(value);
            }
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk1);

            if ((Header.Flags & 3) != 0)
                w.Write(Unk2);
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
        public MetaHash ExpressionDict { get; set; }
        public MetaHash ExpressionName { get; set; }
        public MetaHash VariableName { get; set; }
        public uint Unk6 { get; set; }
        public uint[][] Unk7 { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();

            switch (Header.Flags & 3)
            {
                case 1:
                    ExpressionDict = new MetaHash(r.ReadUInt32());
                    ExpressionName = new MetaHash(r.ReadUInt32());
                    break;
                case 2:
                    VariableName = new MetaHash(r.ReadUInt32());
                    break;
            }

            if (((Header.Flags >> 2) & 3) != 0)
                Unk6 = r.ReadUInt32();

            var unk7Count = (Header.Flags >> 28);

            if (unk7Count == 0)
                return;

            var unkHeaderFlag = (Header.Flags >> 4) & 0xFFFFFF;
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
                    result = 20;
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk1);

            switch (Header.Flags & 3)
            {
                case 1:
                    w.Write(ExpressionDict);
                    w.Write(ExpressionName);
                    break;
                case 2:
                    w.Write(VariableName);
                    break;
            }

            if (((Header.Flags >> 2) & 3) != 0)
                w.Write(Unk6);

            var unk7Count = (Header.Flags >> 28);

            if (unk7Count == 0)
                return;

            var unkHeaderFlag = (Header.Flags >> 4) & 0xFFFFFF;
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk1);

            if ((Header.Flags & 3) != 0)
                w.Write(Unk2);

            if ((Header.Flags & 0x30) != 0)
                w.Write(Unk3);
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
        public MetaHash Unk5 { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderNameFlag(r);
            Unk1 = r.ReadUInt32();
            Unk2 = r.ReadUInt32();

            if ((Header.Flags & 0x180000) == 0x80000)
                Unk3 = r.ReadUInt32();

            switch (Header.Flags & 3)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);
            w.Write(Unk1);
            w.Write(Unk2);

            if ((Header.Flags & 0x180000) == 0x80000)
                w.Write(Unk3);

            switch (Header.Flags & 3)
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            if ((Header.Flags & 3) != 0)
                w.Write(Unk1);
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
        public MrfHeaderStateBase Header { get; set; }
        public uint Unk1 { get; set; }
        public uint VariablesCount { get; set; }
        public uint Unk3 { get; set; }
        public uint EventsCount { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6_Count { get; set; }
        public uint Unk7 { get; set; }
        public uint SignalsCount { get; set; }

        public MrfStructStateSection Header_Unk7_Data { get; set; }
        public MrfStructStateInfoVariable[] Variables { get; set; }
        public MrfStructStateInfoEvent[] Events { get; set; }
        public MrfStructStateInfoUnk6[] Unk6_Items { get; set; }
        public MrfStructStateInfoSignal[] Signals { get; set; }

        public override void Read(DataReader r)
        {
            Header = new MrfHeaderStateBase(r);

            Unk1 = r.ReadUInt32();
            VariablesCount = r.ReadUInt32();
            Unk3 = r.ReadUInt32();
            EventsCount = r.ReadUInt32();
            Unk5 = r.ReadUInt32();
            Unk6_Count = r.ReadUInt32();
            Unk7 = r.ReadUInt32();
            SignalsCount = r.ReadUInt32();

            Variables = new MrfStructStateInfoVariable[VariablesCount];
            Events = new MrfStructStateInfoEvent[EventsCount];
            Unk6_Items = new MrfStructStateInfoUnk6[Unk6_Count];
            Signals = new MrfStructStateInfoSignal[SignalsCount];

            if (Header.Unk7 != 0)
                Header_Unk7_Data = new MrfStructStateSection(r, Header.Unk7);

            for (int i = 0; i < VariablesCount; i++)
                Variables[i] = new MrfStructStateInfoVariable(r);

            for (int i = 0; i < EventsCount; i++)
                Events[i] = new MrfStructStateInfoEvent(r);

            for (int i = 0; i < Unk6_Count; i++)
                Unk6_Items[i] = new MrfStructStateInfoUnk6(r);

            for (int i = 0; i < SignalsCount; i++)
                Signals[i] = new MrfStructStateInfoSignal(r);
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);

            w.Write(Unk1);
            w.Write(VariablesCount);
            w.Write(Unk3);
            w.Write(EventsCount);
            w.Write(Unk5);
            w.Write(Unk6_Count);
            w.Write(Unk7);
            w.Write(SignalsCount);

            if (Header.Unk7 != 0)
                Header_Unk7_Data.Write(w);

            foreach (var item in Variables)
                item.Write(w);

            foreach (var item in Events)
                item.Write(w);

            foreach (var item in Unk6_Items)
                item.Write(w);

            foreach (var item in Signals)
                item.Write(w);
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

        public override void Read(DataReader r)
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

        public override void Write(DataWriter w)
        {
            Header.Write(w);
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

            if (Nodes != null)
            {
                foreach (var node in Nodes)
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
                nodeHandler.Read(r);

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

            throw new Exception($"A handler for ({infoType}) mrf node type is not valid");
        }
    }
}
