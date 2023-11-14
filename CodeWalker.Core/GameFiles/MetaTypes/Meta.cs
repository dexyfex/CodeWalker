using System;using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{



    [TC(typeof(EXP))] public class Meta : ResourceFileBase
    {
        public override long BlockLength
        {
            get { return 112; }
        }

        // structure data
        public int Unknown_10h { get; set; } = 0x50524430;
        public short Unknown_14h { get; set; } = 0x0079;
        public byte HasEncryptedStrings { get; set; }
        public byte Unknown_17h { get; set; } = 0x00;
        public int Unknown_18h { get; set; } = 0x00000000;
        public int RootBlockIndex { get; set; }
        public long StructureInfosPointer { get; set; }
        public long EnumInfosPointer { get; set; }
        public long DataBlocksPointer { get; set; }
        public long NamePointer { get; set; }
        public long EncryptedStringsPointer { get; set; }
        public short StructureInfosCount { get; set; }
        public short EnumInfosCount { get; set; }
        public short DataBlocksCount { get; set; }
        public short Unknown_4Eh { get; set; } = 0x0000;
        public uint Unknown_50h { get; set; } = 0x00000000;
        public uint Unknown_54h { get; set; } = 0x00000000;
        public uint Unknown_58h { get; set; } = 0x00000000;
        public uint Unknown_5Ch { get; set; } = 0x00000000;
        public uint Unknown_60h { get; set; } = 0x00000000;
        public uint Unknown_64h { get; set; } = 0x00000000;
        public uint Unknown_68h { get; set; } = 0x00000000;
        public uint Unknown_6Ch { get; set; } = 0x00000000;

        // reference data
        public ResourceSimpleArray<MetaStructureInfo> StructureInfos { get; set; }
        public ResourceSimpleArray<MetaEnumInfo> EnumInfos { get; set; }
        public ResourceSimpleArray<MetaDataBlock> DataBlocks { get; set; }
        public string Name { get; set; }
        //public string[] Strings { get; set; }
        public MetaEncryptedStringsBlock EncryptedStrings { get; set; }

        private string_r NameBlock = null;


#if DEBUG
        public ResourceAnalyzer Analyzer { get; set; }
#endif


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            base.Read(reader, parameters);

            // read structure data
            this.Unknown_10h = reader.ReadInt32();
            this.Unknown_14h = reader.ReadInt16();
            this.HasEncryptedStrings = reader.ReadByte();
            this.Unknown_17h = reader.ReadByte();
            this.Unknown_18h = reader.ReadInt32();
            this.RootBlockIndex = reader.ReadInt32();
            this.StructureInfosPointer = reader.ReadInt64();
            this.EnumInfosPointer = reader.ReadInt64();
            this.DataBlocksPointer = reader.ReadInt64();
            this.NamePointer = reader.ReadInt64();
            this.EncryptedStringsPointer = reader.ReadInt64();
            this.StructureInfosCount = reader.ReadInt16();
            this.EnumInfosCount = reader.ReadInt16();
            this.DataBlocksCount = reader.ReadInt16();
            this.Unknown_4Eh = reader.ReadInt16();
            this.Unknown_50h = reader.ReadUInt32();
            this.Unknown_54h = reader.ReadUInt32();
            this.Unknown_58h = reader.ReadUInt32();
            this.Unknown_5Ch = reader.ReadUInt32();
            this.Unknown_60h = reader.ReadUInt32();
            this.Unknown_64h = reader.ReadUInt32();
            this.Unknown_68h = reader.ReadUInt32();
            this.Unknown_6Ch = reader.ReadUInt32();

            // read reference data
            this.StructureInfos = reader.ReadBlockAt<ResourceSimpleArray<MetaStructureInfo>>(
                (ulong)this.StructureInfosPointer, // offset
                this.StructureInfosCount
            );

            this.EnumInfos = reader.ReadBlockAt<ResourceSimpleArray<MetaEnumInfo>>(
                (ulong)this.EnumInfosPointer, // offset
                this.EnumInfosCount
            );

            this.DataBlocks = reader.ReadBlockAt<ResourceSimpleArray<MetaDataBlock>>(
                (ulong)this.DataBlocksPointer, // offset
                this.DataBlocksCount
            );

            this.Name = reader.ReadStringAt(//BlockAt<string_r>(
                (ulong)this.NamePointer // offset
            );

            //Strings = MetaTypes.GetStrings(this);

#if DEBUG
            EncryptedStrings = reader.ReadBlockAt<MetaEncryptedStringsBlock>((ulong)EncryptedStringsPointer);

            Analyzer = new ResourceAnalyzer(reader);
#endif

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            base.Write(writer, parameters);

            // update structure data
            this.StructureInfosPointer = this.StructureInfos?.FilePosition ?? 0;
            this.EnumInfosPointer = this.EnumInfos?.FilePosition ?? 0;
            this.DataBlocksPointer = this.DataBlocks?.FilePosition ?? 0;
            this.NamePointer = this.NameBlock?.FilePosition ?? 0;
            this.EncryptedStringsPointer = 0;
            this.StructureInfosCount = (short)(this.StructureInfos?.Count ?? 0);
            this.EnumInfosCount = (short)(this.EnumInfos?.Count ?? 0);
            this.DataBlocksCount = (short)(this.DataBlocks?.Count ?? 0);

            // write structure data
            writer.Write(this.Unknown_10h);
            writer.Write(this.Unknown_14h);
            writer.Write(this.HasEncryptedStrings);
            writer.Write(this.Unknown_17h);
            writer.Write(this.Unknown_18h);
            writer.Write(this.RootBlockIndex);
            writer.Write(this.StructureInfosPointer);
            writer.Write(this.EnumInfosPointer);
            writer.Write(this.DataBlocksPointer);
            writer.Write(this.NamePointer);
            writer.Write(this.EncryptedStringsPointer);
            writer.Write(this.StructureInfosCount);
            writer.Write(this.EnumInfosCount);
            writer.Write(this.DataBlocksCount);
            writer.Write(this.Unknown_4Eh);
            writer.Write(this.Unknown_50h);
            writer.Write(this.Unknown_54h);
            writer.Write(this.Unknown_58h);
            writer.Write(this.Unknown_5Ch);
            writer.Write(this.Unknown_60h);
            writer.Write(this.Unknown_64h);
            writer.Write(this.Unknown_68h);
            writer.Write(this.Unknown_6Ch);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>(base.GetReferences());
            if ((StructureInfos != null) && (StructureInfos.Count > 0)) list.Add(StructureInfos);
            if ((EnumInfos != null) && (EnumInfos.Count > 0)) list.Add(EnumInfos);
            if ((DataBlocks != null) && (DataBlocks.Count > 0)) list.Add(DataBlocks);
            if (!string.IsNullOrEmpty(Name))
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
            return list.ToArray();
        }




        public MetaDataBlock FindBlock(MetaName name)
        {
            if (DataBlocks == null) return null;
            foreach (var block in DataBlocks)
            {
                if (block.StructureNameHash == name)
                {
                    return block;
                }
            }
            return null;
        }


        public MetaDataBlock GetRootBlock()
        {
            MetaDataBlock block = null;
            var rootind = RootBlockIndex - 1;
            if ((rootind >= 0) && (rootind < DataBlocks.Count) && (DataBlocks.Data != null))
            {
                block = DataBlocks[rootind];
            }
            return block;
        }
        public MetaDataBlock GetBlock(int id)
        {
            MetaDataBlock block = null;
            var ind = id - 1;
            if ((ind >= 0) && (ind < DataBlocks.Count) && (DataBlocks.Data != null))
            {
                block = DataBlocks[ind];
            }
            return block;
        }

    }

    [TC(typeof(EXP))] public class MetaStructureInfo : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 32; }
        }

        // structure data
        public MetaName StructureNameHash { get; set; }
        public uint StructureKey { get; set; }
        public uint Unknown_8h { get; set; }
        public uint Unknown_Ch { get; set; } = 0x00000000;
        public long EntriesPointer { get; private set; }
        public int StructureSize { get; set; }
        public short Unknown_1Ch { get; set; } = 0x0000;
        public short EntriesCount { get; private set; }

        // reference data
        public MetaStructureEntryInfo_s[] Entries { get; private set; }

        private ResourceSystemStructBlock<MetaStructureEntryInfo_s> EntriesBlock = null;


        public MetaStructureInfo()
        { }
        public MetaStructureInfo(MetaName nameHash, uint key, uint unknown, int length, params MetaStructureEntryInfo_s[] entries)
        {
            StructureNameHash = nameHash;
            StructureKey = key;
            Unknown_8h = unknown;
            StructureSize = length;
            Entries = entries;
        }



        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.StructureNameHash = (MetaName)reader.ReadInt32();
            this.StructureKey = reader.ReadUInt32();
            this.Unknown_8h = reader.ReadUInt32();
            this.Unknown_Ch = reader.ReadUInt32();
            this.EntriesPointer = reader.ReadInt64();
            this.StructureSize = reader.ReadInt32();
            this.Unknown_1Ch = reader.ReadInt16();
            this.EntriesCount = reader.ReadInt16();

            // read reference data
            this.Entries = reader.ReadStructsAt<MetaStructureEntryInfo_s>((ulong)this.EntriesPointer, (uint)this.EntriesCount);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.EntriesPointer = this.Entries?.Position ?? 0; //TODO: fix
            //this.EntriesCount = (short)(this.Entries?.Count ?? 0);
            this.EntriesPointer = this.EntriesBlock?.FilePosition ?? 0; //TODO: fix
            this.EntriesCount = (short)(this.EntriesBlock?.ItemCount ?? 0);

            // write structure data
            writer.Write((int)this.StructureNameHash);
            writer.Write(this.StructureKey);
            writer.Write(this.Unknown_8h);
            writer.Write(this.Unknown_Ch);
            writer.Write(this.EntriesPointer);
            writer.Write(this.StructureSize);
            writer.Write(this.Unknown_1Ch);
            writer.Write(this.EntriesCount);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (EntriesBlock == null)
            {
                EntriesBlock = new ResourceSystemStructBlock<MetaStructureEntryInfo_s>(Entries);
            }
            if (EntriesBlock != null) list.Add(EntriesBlock);

            //if (Entries != null) list.Add(Entries); //TODO: fix
            return list.ToArray();
        }

        public override string ToString()
        {
            return StructureNameHash.ToString();
        }
    }

    public enum MetaStructureEntryDataType : byte
    {
        Boolean = 0x01,
        SignedByte = 0x10,
        UnsignedByte = 0x11, // OCCURS IN ARRAY
        SignedShort = 0x12,
        UnsignedShort = 0x13, // OCCURS IN ARRAY
        SignedInt = 0x14,
        UnsignedInt = 0x15, // OCCURS IN ARRAY
        Float = 0x21, // OCCURS IN ARRAY
        Float_XYZ = 0x33, // OCCURS IN ARRAY
        Float_XYZW = 0x34,
        ByteEnum = 0x60, // has enum name hash in info
        IntEnum = 0x62, // has enum name hash in info
        ShortFlags = 0x64, // has enum name hash in info     
        IntFlags1 = 0x63, // has enum name hash in info
        IntFlags2 = 0x65, // has enum name hash in info (optional?)
        Hash = 0x4A, // OCCURS IN ARRAY
        Array = 0x52,
        ArrayOfChars = 0x40, // has length in info
        ArrayOfBytes = 0x50, // has length in info
        DataBlockPointer = 0x59,
        CharPointer = 0x44,
        StructurePointer = 0x07, // OCCURS IN ARRAY
        Structure = 0x05 // has structure name hash in info, OCCURS IN ARRAY
    }
    public static class MetaStructureEntryDataTypes
    {
        public static string GetCSharpTypeName(MetaStructureEntryDataType t)
        {
            switch (t)
            {
                case MetaStructureEntryDataType.Boolean: return "bool";
                case MetaStructureEntryDataType.SignedByte: return "sbyte";
                case MetaStructureEntryDataType.UnsignedByte: return "byte";
                case MetaStructureEntryDataType.SignedShort: return "short";
                case MetaStructureEntryDataType.UnsignedShort: return "ushort";
                case MetaStructureEntryDataType.SignedInt: return "int";
                case MetaStructureEntryDataType.UnsignedInt: return "uint";
                case MetaStructureEntryDataType.Float: return "float";
                case MetaStructureEntryDataType.Float_XYZ: return "Vector3";
                case MetaStructureEntryDataType.Float_XYZW: return "Vector4";

                case MetaStructureEntryDataType.Hash: return "uint"; //uint hashes...
                case MetaStructureEntryDataType.ByteEnum: return "byte"; //convert to enum later..
                case MetaStructureEntryDataType.IntEnum: return "int";
                case MetaStructureEntryDataType.ShortFlags: return "short";
                case MetaStructureEntryDataType.IntFlags1: return "int";
                case MetaStructureEntryDataType.IntFlags2: return "int";

                case MetaStructureEntryDataType.Array:
                case MetaStructureEntryDataType.ArrayOfChars:
                case MetaStructureEntryDataType.ArrayOfBytes:
                case MetaStructureEntryDataType.DataBlockPointer:
                case MetaStructureEntryDataType.CharPointer:
                case MetaStructureEntryDataType.StructurePointer:
                case MetaStructureEntryDataType.Structure:
                default:
                    return t.ToString();
            }
        }
    }

    [TC(typeof(EXP))] public struct MetaStructureEntryInfo_s
    {
        // structure data
        public MetaName EntryNameHash { get; set; }
        public int DataOffset { get; set; }
        public MetaStructureEntryDataType DataType { get; set; }
        public byte Unknown_9h { get; set; }
        public short ReferenceTypeIndex { get; set; }
        public MetaName ReferenceKey { get; set; }

        public MetaStructureEntryInfo_s(MetaName nameHash, int dataOffset, MetaStructureEntryDataType dataType, byte unk9h, short referenceTypeIndex, MetaName referenceKey)
        {
            EntryNameHash = nameHash;
            DataOffset = dataOffset;
            DataType = dataType;
            Unknown_9h = unk9h;
            ReferenceTypeIndex = referenceTypeIndex;
            ReferenceKey = referenceKey;
        }

        public override string ToString()
        {
            return DataOffset.ToString() + ": " + DataType.ToString() + ": " + ReferenceKey.ToString() + ": " + EntryNameHash.ToString();
        }
    }

    [TC(typeof(EXP))] public class MetaEnumInfo : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 24; }
        }

        // structure data
        public MetaName EnumNameHash { get; set; }
        public uint EnumKey { get; set; }
        public long EntriesPointer { get; private set; }
        public int EntriesCount { get; private set; }
        public int Unknown_14h { get; set; } = 0x00000000;

        // reference data
        //public ResourceSimpleArray<MetaEnumEntryInfo> Entries;
        public MetaEnumEntryInfo_s[] Entries { get; private set; }

        private ResourceSystemStructBlock<MetaEnumEntryInfo_s> EntriesBlock = null;


        public MetaEnumInfo()
        { }
        public MetaEnumInfo(MetaName nameHash, uint key, params MetaEnumEntryInfo_s[] entries)
        {
            EnumNameHash = nameHash;
            EnumKey = key;
            Entries = entries;
        }


        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
            this.EnumNameHash = (MetaName)reader.ReadInt32();
            this.EnumKey = reader.ReadUInt32();
            this.EntriesPointer = reader.ReadInt64();
            this.EntriesCount = reader.ReadInt32();
            this.Unknown_14h = reader.ReadInt32();

            // read reference data
            //this.Entries = reader.ReadBlockAt<ResourceSimpleArray<MetaEnumEntryInfo>>(
            //    (ulong)this.EntriesPointer, // offset
            //    this.EntriesCount
            //);
            this.Entries = reader.ReadStructsAt<MetaEnumEntryInfo_s>((ulong)this.EntriesPointer, (uint)this.EntriesCount);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            //this.EntriesPointer = this.Entries?.Position ?? 0; //TODO: fix
            //this.EntriesCount = this.Entries?.Count ?? 0;
            this.EntriesPointer = this.EntriesBlock?.FilePosition ?? 0; //TODO: fix
            this.EntriesCount = (short)(this.EntriesBlock?.ItemCount ?? 0);

            // write structure data
            writer.Write((int)this.EnumNameHash);
            writer.Write(this.EnumKey);
            writer.Write(this.EntriesPointer);
            writer.Write(this.EntriesCount);
            writer.Write(this.Unknown_14h);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            //if (Entries != null) list.Add(Entries);
            if (EntriesBlock == null)
            {
                EntriesBlock = new ResourceSystemStructBlock<MetaEnumEntryInfo_s>(Entries);
            }
            if (EntriesBlock != null) list.Add(EntriesBlock);

            return list.ToArray();
        }

        public override string ToString()
        {
            return EnumNameHash.ToString();
        }
    }

    [TC(typeof(EXP))] public struct MetaEnumEntryInfo_s
    {
        // structure data
        public MetaName EntryNameHash { get; set; }
        public int EntryValue { get; set; }

        public MetaEnumEntryInfo_s(MetaName nameHash, int value)
        {
            EntryNameHash = nameHash;
            EntryValue = value;
        }

        public override string ToString()
        {
            return EntryNameHash.ToString() + ": " + EntryValue.ToString();
        }
    }

    [TC(typeof(EXP))] public class MetaDataBlock : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get { return 16; }
        }

        // structure data
        public MetaName StructureNameHash { get; set; }
        public int DataLength { get; set; }
        public long DataPointer { get; private set; }

        // reference data
        public byte[] Data { get; set; }
        private ResourceSystemDataBlock DataBlock = null;

        /// <summary>
        /// Reads the data-block from a stream.
        /// </summary>
        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            // read structure data
          this.StructureNameHash = (MetaName)reader.ReadInt32();
            this.DataLength = reader.ReadInt32();
            this.DataPointer = reader.ReadInt64();


            this.Data = reader.ReadBytesAt((ulong)this.DataPointer, (uint)DataLength);

        }

        /// <summary>
        /// Writes the data-block to a stream.
        /// </summary>
        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            // update structure data
            this.DataLength = this.Data?.Length ?? 0;
            //this.DataPointer = (this.Data!=null)? DataPos : 0; //TODO:fix...
            this.DataPointer = this.DataBlock?.FilePosition ?? 0;

            // write structure data
            writer.Write((int)this.StructureNameHash);
            writer.Write(this.DataLength);
            writer.Write(this.DataPointer);
        }

        /// <summary>
        /// Returns a list of data blocks which are referenced by this block.
        /// </summary>
        public override IResourceBlock[] GetReferences()
        {
            var list = new List<IResourceBlock>();
            if (DataBlock == null)
            {
                DataBlock = new ResourceSystemDataBlock(Data);
            }
            if (DataBlock != null) list.Add(DataBlock);
            //if (Data != null) list.Add(Data);
            return list.ToArray();
        }

        public override string ToString()
        {
            return StructureNameHash.ToString() + ": " + DataPointer.ToString() + " (" + DataLength.ToString() + ")";
        }
    }


    [TC(typeof(EXP))] public class MetaEncryptedStringsBlock : ResourceSystemBlock
    {
        public override long BlockLength
        {
            get
            {
                return 4 + Count;// + PadCount;
            }
        }

        public uint Count { get; set; }
        public byte[] EncryptedData { get; set; }
        //public uint PadCount { get; set; }
        //public byte[] PadData { get; set; }
        //public string[] TestStrings { get; set; }


        public override void Read(ResourceDataReader reader, params object[] parameters)
        {
            Count = MetaTypes.SwapBytes(reader.ReadUInt32()); //okay. so this is big endian
            EncryptedData = reader.ReadBytes((int)Count);
            //PadCount = (uint)((8 - (reader.Position % 8)) % 8);//maybe next block just needs to be aligned instead?
            //PadData = reader.ReadBytes((int)PadCount);


            ////none of these work :(
            //var strs = new List<string>();
            //foreach (var key in GTA5Keys.PC_NG_KEYS)
            //{
            //    var decr = GTACrypto.DecryptNG(EncryptedData, key);
            //    strs.Add(Encoding.ASCII.GetString(decr));
            //}
            //TestStrings = strs.ToArray();


        }

        public override void Write(ResourceDataWriter writer, params object[] parameters)
        {
            throw new NotImplementedException();
        }
    }











    //derived types - manually created (array & pointer structs)

    [TC(typeof(EXP))] public struct Array_StructurePointer //16 bytes - pointer for a structure pointer array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "Array_StructurePointer: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }

        public override bool Equals(object obj)
        {
            return obj is Array_StructurePointer pointer &&
                   Pointer == pointer.Pointer &&
                   Count1 == pointer.Count1 &&
                   Count2 == pointer.Count2 &&
                   Unk1 == pointer.Unk1;
        }

        public override int GetHashCode()
        {
            int hashCode = -1900453823;
            hashCode = hashCode * -1521134295 + Pointer.GetHashCode();
            hashCode = hashCode * -1521134295 + Count1.GetHashCode();
            hashCode = hashCode * -1521134295 + Count2.GetHashCode();
            hashCode = hashCode * -1521134295 + Unk1.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Array_StructurePointer left, Array_StructurePointer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Array_StructurePointer left, Array_StructurePointer right)
        {
            return !(left == right);
        }
    }
    [TC(typeof(EXP))] public struct Array_Structure //16 bytes - pointer for a structure array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } set { Pointer = (Pointer & 0xFFFFFFFFFFFFF000) + (value & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } set { PointerDataId = value + 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } set { Pointer = (Pointer & 0xFFFFFFFF00000FFF) + ((value << 12) & 0xFFFFF000); } }



        public Array_Structure(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_Structure(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public Array_Structure SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
            return this;
        }
        public override string ToString()
        {
            return "Array_Structure: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }

        public static bool operator ==(Array_Structure x, Array_Structure y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Array_Structure x, Array_Structure y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not Array_Structure arrObj)
                return false;

            return arrObj.Pointer == this.Pointer
                && arrObj.Count1 == this.Count1
                && arrObj.Count2 == this.Count2
                && arrObj.Unk1 == this.Unk1;
        }

        public override int GetHashCode()
        {
            int hashCode = -1900453823;
            hashCode = hashCode * -1521134295 + Pointer.GetHashCode();
            hashCode = hashCode * -1521134295 + Count1.GetHashCode();
            hashCode = hashCode * -1521134295 + Count2.GetHashCode();
            hashCode = hashCode * -1521134295 + Unk1.GetHashCode();
            return hashCode;
        }
    }
    [TC(typeof(EXP))] public struct Array_uint //16 bytes - pointer for a uint array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }


        public Array_uint(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_uint(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "Array_uint: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }

        public static bool operator ==(Array_uint x, Array_uint y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Array_uint x, Array_uint y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not Array_uint arrObj)
                return false;

            return arrObj.Pointer == this.Pointer
                && arrObj.Count1 == this.Count1
                && arrObj.Count2 == this.Count2
                && arrObj.Unk1 == this.Unk1;
        }

        public override int GetHashCode()
        {
            int hashCode = -1900453823;
            hashCode = hashCode * -1521134295 + Pointer.GetHashCode();
            hashCode = hashCode * -1521134295 + Count1.GetHashCode();
            hashCode = hashCode * -1521134295 + Count2.GetHashCode();
            hashCode = hashCode * -1521134295 + Unk1.GetHashCode();
            return hashCode;
        }
    }
    [TC(typeof(EXP))] public struct Array_ushort //16 bytes - pointer for a ushort array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }


        public Array_ushort(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_ushort(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "Array_ushort: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }

        public static bool operator ==(Array_ushort x, Array_ushort y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Array_ushort x, Array_ushort y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not Array_ushort arrObj)
                return false;

            return arrObj.Pointer == this.Pointer
                && arrObj.Count1 == this.Count1
                && arrObj.Count2 == this.Count2
                && arrObj.Unk1 == this.Unk1;
        }

        public override int GetHashCode()
        {
            int hashCode = -1900453823;
            hashCode = hashCode * -1521134295 + Pointer.GetHashCode();
            hashCode = hashCode * -1521134295 + Count1.GetHashCode();
            hashCode = hashCode * -1521134295 + Count2.GetHashCode();
            hashCode = hashCode * -1521134295 + Unk1.GetHashCode();
            return hashCode;
        }
    }
    [TC(typeof(EXP))] public struct Array_byte //16 bytes - pointer for a byte array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }

        public Array_byte(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_byte(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "Array_byte: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }

        public override bool Equals(object obj)
        {
            return obj is Array_byte @byte &&
                   Pointer == @byte.Pointer &&
                   Count1 == @byte.Count1 &&
                   Count2 == @byte.Count2 &&
                   Unk1 == @byte.Unk1;
        }

        public override int GetHashCode()
        {
            int hashCode = -1900453823;
            hashCode = hashCode * -1521134295 + Pointer.GetHashCode();
            hashCode = hashCode * -1521134295 + Count1.GetHashCode();
            hashCode = hashCode * -1521134295 + Count2.GetHashCode();
            hashCode = hashCode * -1521134295 + Unk1.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Array_byte left, Array_byte right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Array_byte left, Array_byte right)
        {
            return !(left == right);
        }
    }
    [TC(typeof(EXP))] public struct Array_float //16 bytes - pointer for a float array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }

        public Array_float(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_float(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "Array_float: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }
    }
    [TC(typeof(EXP))] public struct Array_Vector3 //16 bytes - pointer for a Vector3 array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }

        public Array_Vector3(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_Vector3(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "Array_Vector3: " + PointerDataIndex.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }
    }
    [TC(typeof(EXP))] public struct CharPointer //16 bytes - pointer for a char array
    {
        public ulong Pointer { get; set; }
        public ushort Count1 { get; set; }
        public ushort Count2 { get; set; }
        public uint Unk1 { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }

        public CharPointer(ulong ptr, int len)
        {
            Pointer = ptr;
            Count1 = (ushort)len;
            Count2 = Count1;
            Unk1 = 0;
        }
        public CharPointer(MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Count1 = MetaTypes.SwapBytes(Count1);
            Count2 = MetaTypes.SwapBytes(Count2);
            Unk1 = MetaTypes.SwapBytes(Unk1);
        }
        public override string ToString()
        {
            return "CharPointer: " + Pointer.ToString() + " (" + Count1.ToString() + "/" + Count2.ToString() + ")";
        }
    }
    [TC(typeof(EXP))] public struct DataBlockPointer //8 bytes - pointer to data block
    {
        public ulong Pointer { get; set; }

        public uint Unk0 { get { return (uint)(Pointer >> 32); } }
        public uint PointerDataId { get { return (uint)(Pointer & 0xFFF); } }
        public uint PointerDataIndex { get { return (uint)(Pointer & 0xFFF) - 1; } }
        public uint PointerDataOffset { get { return (uint)((Pointer >> 12) & 0xFFFFF); } }


        public DataBlockPointer(int blockId, int offset)
        {
            Pointer = ((uint)blockId & 0xFFF) | (((uint)offset & 0xFFFFF) << 12);
        }

        public override string ToString()
        {
            return "DataBlockPointer: " + Pointer.ToString();
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
        }
    }

    [TC(typeof(EXP))] public struct ArrayOfUshorts3 //array of 3 ushorts
    {
        public ushort u0, u1, u2;
        public Vector3 XYZ()
        {
            return new Vector3(u0, u1, u2);
        }
        public override string ToString()
        {
            return u0.ToString() + ", " + u1.ToString() + ", " + u2.ToString();
        }
    }
    [TC(typeof(EXP))] public struct ArrayOfBytes3 //array of 3 bytes
    {
        public byte b0, b1, b2;
        public byte[] GetArray()
        {
            return new[] { b0, b1, b2 };
        }
        public override string ToString()
        {
            return b0.ToString() + ", " + b1.ToString() + ", " + b2.ToString();
        }
    }
    [TC(typeof(EXP))] public struct ArrayOfBytes4 //array of 4 bytes
    {
        public byte b0, b1, b2, b3;
        public byte[] GetArray()
        {
            return new[] { b0, b1, b2, b3 };
        }
        public override string ToString()
        {
            return b0.ToString() + ", " + b1.ToString() + ", " + b2.ToString() + ", " + b3.ToString();
        }
    }
    [TC(typeof(EXP))] public struct ArrayOfBytes5 //array of 5 bytes
    {
        public byte b0, b1, b2, b3, b4;
        public byte[] GetArray()
        {
            return new[] { b0, b1, b2, b3, b4 };
        }
    }
    [TC(typeof(EXP))] public struct ArrayOfBytes6 //array of 6 bytes
    {
        public byte b0, b1, b2, b3, b4, b5;
        public byte[] GetArray()
        {
            return new[] { b0, b1, b2, b3, b4, b5 };
        }
    }
    [TC(typeof(EXP))] public struct ArrayOfBytes12 //array of 12 bytes
    {
        public byte b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11;
        public byte[] GetArray()
        {
            return new[] { b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11 };
        }
    }
    [TC(typeof(EXP))] public struct ArrayOfChars64 //array of 64 chars (bytes)
    {
        public byte
            b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
            b20, b21, b22, b23, b24, b25, b26, b27, b28, b29, b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
            b40, b41, b42, b43, b44, b45, b46, b47, b48, b49, b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
            b60, b61, b62, b63;
        public override string ToString()
        {
            byte[] bytes =
            {
                b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
                b20, b21, b22, b23, b24, b25, b26, b27, b28, b29, b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
                b40, b41, b42, b43, b44, b45, b46, b47, b48, b49, b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
                b60, b61, b62, b63
            };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0) break;
                sb.Append((char)bytes[i]);
            }
            return sb.ToString();
        }
    }

    [TC(typeof(EXP))] public struct MetaVECTOR3 //12 bytes, Key:2751397072
    {
        public float x { get; set; } //0   0: Float: 0: x
        public float y { get; set; } //4   4: Float: 0: y
        public float z { get; set; } //8   8: Float: 0: z

        public override string ToString()
        {
            return x.ToString() + ", " + y.ToString() + ", " + z.ToString();
        }

        public MetaVECTOR3(Vector3 v)
        {
            x = v.X;
            y = v.Y;
            z = v.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [TC(typeof(EXP))] public struct MetaPOINTER //8 bytes - pointer to data item //was: SectionUNKNOWN10
    {
        public ulong Pointer { get; set; }

        public int BlockIndex { get { return BlockID - 1; } }
        public int BlockID { get { return (int)(Pointer & 0xFFF); } set { Pointer = (Pointer & 0xFFFFF000) + ((uint)value & 0xFFF); } }
        public int Offset { get { return (int)((Pointer >> 12) & 0xFFFFF); } set { Pointer = (Pointer & 0xFFF) + (((uint)value << 12) & 0xFFFFF000); } }

        public MetaPOINTER(int blockID, int itemOffset)
        {
            Pointer = (((uint)itemOffset << 12) & 0xFFFFF000) + ((uint)blockID & 0xFFF);
        }

        public override string ToString()
        {
            return BlockID.ToString() + ", " + Offset.ToString();
        }
    }










    [TC(typeof(EXP))] public struct MetaHash : IEquatable<MetaHash>
    {
        public uint Hash { get; set; }

        public string Hex
        {
            get
            {
                return Hash.ToString("X").PadLeft(8, '0');
            }
        }

        public float Float
        {
            get
            {
                return MetaTypes.ConvertData<float>(MetaTypes.ConvertToBytes(Hash));
            }
        }

        public short Short1
        {
            get
            {
                return (short)(Hash & 0xFFFF);
            }
        }
        public short Short2
        {
            get
            {
                return (short)((Hash >> 16) & 0xFFFF);
            }
        }


        public MetaHash(uint h) { Hash = h; }

        public override string ToString()
        {
            var str = JenkIndex.TryGetString(Hash);
            if (!string.IsNullOrEmpty(str)) return str;
            if (MetaNames.TryGetString(Hash, out str)) return str;
            return GlobalText.GetString(Hash);
        }

        public string ToCleanString()
        {
            if (Hash == 0) return string.Empty;
            return ToString();
        }

        public static implicit operator uint(MetaHash h)
        {
            return h.Hash;  //implicit conversion
        }

        public static implicit operator MetaHash(uint v)
        {
            return new MetaHash(v);
        }

        public override readonly bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is not MetaHash metaHash) return false;

            return metaHash.Hash == Hash;
        }

        public readonly bool Equals(MetaHash obj)
        {
            if (obj == null) return false;

            return obj.Hash == Hash;
        }

        public override readonly int GetHashCode()
        {
            return (int)Hash;
        }

        public static bool operator ==(MetaHash a, MetaHash b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MetaHash a, MetaHash b)
        {
            return !a.Equals(b);
        }
    }

    
    [TC(typeof(EXP))] public struct TextHash
    {
        public uint Hash { get; set; }

        public string Hex
        {
            get
            {
                return Hash.ToString("X");
            }
        }

        public TextHash(uint h) { Hash = h; }

        public override string ToString()
        {
            return GlobalText.GetString(Hash);
        }


        public static implicit operator uint(TextHash h)
        {
            return h.Hash;  //implicit conversion
        }

        public static implicit operator TextHash(uint v)
        {
            return new TextHash(v);
        }
    }








}