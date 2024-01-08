using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Buffers;
using CodeWalker.Core.Utils;
using System.Runtime.InteropServices;
using System.Reflection;
using Collections.Pooled;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CodeWalker.GameFiles
{



    [TC(typeof(EXP))]
    public class Meta : ResourceFileBase, IResourceBlockSpan
    {
        public override long BlockLength => 112;

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
        public ResourceSimpleArray<MetaStructureInfo>? StructureInfos { get; set; }
        public ResourceSimpleArray<MetaEnumInfo>? EnumInfos { get; set; }
        public ResourceSimpleArray<MetaDataBlock>? DataBlocks { get; set; }
        public string? Name { get; set; }
        //public string[] Strings { get; set; }

        private string_r? NameBlock = null;


#if DEBUG
        public MetaEncryptedStringsBlock EncryptedStrings { get; set; }
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
            using var list = new PooledList<IResourceBlock>(base.GetReferences());
            if (StructureInfos is not null && StructureInfos.Count > 0)
                list.Add(StructureInfos);
            if (EnumInfos is not null && EnumInfos.Count > 0)
                list.Add(EnumInfos);
            if (DataBlocks is not null && DataBlocks.Count > 0)
                list.Add(DataBlocks);

            if (!string.IsNullOrEmpty(Name))
            {
                NameBlock = (string_r)Name;
                list.Add(NameBlock);
            }
            return list.ToArray();
        }




        public MetaDataBlock? FindBlock(MetaName name)
        {
            if (DataBlocks is null)
                return null;

            foreach (var block in DataBlocks.Span)
            {
                if (block.StructureNameHash == name)
                {
                    return block;
                }
            }
            return null;
        }


        public MetaDataBlock? GetRootBlock()
        {
            if (DataBlocks?.Data is null)
                return null;

            MetaDataBlock? block = null;
            var rootind = RootBlockIndex - 1;
            if (rootind >= 0 && rootind < DataBlocks.Count)
            {
                block = DataBlocks[rootind];
            }
            return block;
        }
        public MetaDataBlock? GetBlock(int id)
        {
            if (DataBlocks?.Data is null)
                return null;

            var ind = id - 1;
            if (ind >= 0 && ind < DataBlocks.Count)
            {
                return DataBlocks[ind];
            }
            return null;
        }
    }

    [TC(typeof(EXP))]
    public class MetaStructureInfo : ResourceSystemBlock, IResourceBlockSpan
    {
        public override long BlockLength => 32;

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

        public void Read(ref SequenceReader<byte> reader, params object[] parameters)
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
            this.Entries = reader.ReadStructsAt<MetaStructureEntryInfo_s>((ulong)this.EntriesPointer, (int)this.EntriesCount).ToArray();
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
            EntriesBlock ??= new ResourceSystemStructBlock<MetaStructureEntryInfo_s>(Entries);

            //if (Entries != null) list.Add(Entries); //TODO: fix
            return [EntriesBlock];
        }

        public override string ToString() => StructureNameHash.ToString();
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
            return t switch
            {
                MetaStructureEntryDataType.Boolean => "bool",
                MetaStructureEntryDataType.SignedByte => "sbyte",
                MetaStructureEntryDataType.UnsignedByte => "byte",
                MetaStructureEntryDataType.SignedShort => "short",
                MetaStructureEntryDataType.UnsignedShort => "ushort",
                MetaStructureEntryDataType.SignedInt => "int",
                MetaStructureEntryDataType.UnsignedInt => "uint",
                MetaStructureEntryDataType.Float => "float",
                MetaStructureEntryDataType.Float_XYZ => "Vector3",
                MetaStructureEntryDataType.Float_XYZW => "Vector4",
                MetaStructureEntryDataType.Hash => "uint",//uint hashes...
                MetaStructureEntryDataType.ByteEnum => "byte",//convert to enum later..
                MetaStructureEntryDataType.IntEnum => "int",
                MetaStructureEntryDataType.ShortFlags => "short",
                MetaStructureEntryDataType.IntFlags1 => "int",
                MetaStructureEntryDataType.IntFlags2 => "int",
                _ => t.ToString(),
            };
        }
    }

    [TC(typeof(EXP))]
    public readonly struct MetaStructureEntryInfo_s(MetaName nameHash, int dataOffset, MetaStructureEntryDataType dataType, byte unk9h, short referenceTypeIndex, MetaName referenceKey)
    {
        // structure data
        public MetaName EntryNameHash { get; init; } = nameHash;
        public int DataOffset { get; init; } = dataOffset;
        public MetaStructureEntryDataType DataType { get; init; } = dataType;
        public byte Unknown_9h { get; init; } = unk9h;
        public short ReferenceTypeIndex { get; init; } = referenceTypeIndex;
        public MetaName ReferenceKey { get; init; } = referenceKey;

        public override string ToString() => $"{DataOffset}: {DataType}: {ReferenceKey}: {EntryNameHash}";
    }

    [TC(typeof(EXP))]
    public class MetaEnumInfo : ResourceSystemBlock, IResourceBlockSpan
    {
        public override long BlockLength => 24;

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

        public void Read(ref SequenceReader<byte> reader, params object[] parameters)
        {
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
            this.Entries = reader.ReadStructsAt<MetaEnumEntryInfo_s>((ulong)this.EntriesPointer, (uint)this.EntriesCount).ToArray();
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
            //if (Entries != null) list.Add(Entries);
            EntriesBlock ??= new ResourceSystemStructBlock<MetaEnumEntryInfo_s>(Entries);

            return [EntriesBlock];
        }

        public override string ToString() => EnumNameHash.ToString();
    }

    [TC(typeof(EXP))]
    public readonly record struct MetaEnumEntryInfo_s(MetaName EntryNameHash, int EntryValue)
    {
        public override string ToString() => $"{EntryNameHash}: {EntryValue}";
    }

    [TC(typeof(EXP))]
    public class MetaDataBlock : ResourceSystemBlock, IResourceBlockSpan
    {
        public override long BlockLength => 16;

        // structure data
        public MetaName StructureNameHash { get; set; }
        public int DataLength { get; set; }
        public long DataPointer { get; private set; }

        // reference data
        public byte[] Data { get; set; }
        private ResourceSystemDataBlock? DataBlock = null;

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

        public void Read(ref SequenceReader<byte> reader, params object[] parameters)
        {
            // read structure data
            this.StructureNameHash = (MetaName)reader.ReadInt32();
            this.DataLength = reader.ReadInt32();
            this.DataPointer = reader.ReadInt64();

            this.Data = reader.ReadBytesAt((ulong)this.DataPointer, (uint)DataLength).ToArray();
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
            DataBlock ??= new ResourceSystemDataBlock(Data);
            //if (Data != null) list.Add(Data);
            return [DataBlock];
        }

        public override string ToString() => $"{StructureNameHash}: {DataPointer} ({DataLength})";
    }


    [TC(typeof(EXP))]
    public class MetaEncryptedStringsBlock : ResourceSystemBlock
    {
        public override long BlockLength => 4 + Count; // + PadCount;

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

    [TC(typeof(EXP))]
    public readonly record struct Array_StructurePointer //16 bytes - pointer for a structure pointer array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public readonly uint Unk0 => (uint)(Pointer >> 32);
        public readonly uint PointerDataId => (uint)(Pointer & 0xFFF);
        public readonly uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public readonly uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);

        public readonly Array_StructurePointer SwapEnd()
        {
            return new Array_StructurePointer
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1),
            };
        }
        public override readonly string ToString() => $"Array_StructurePointer: {PointerDataIndex} ({Count1}/{Count2})";
    }

    [TC(typeof(EXP))]
    public readonly record struct Array_Structure //16 bytes - pointer for a structure array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public readonly uint Unk0 => (uint)(Pointer >> 32);
        public uint PointerDataId { readonly get { return (uint)(Pointer & 0xFFF); } init { Pointer = (Pointer & 0xFFFFFFFFFFFFF000) + (value & 0xFFF); } }
        public uint PointerDataIndex { readonly get { return (uint)(Pointer & 0xFFF) - 1; } init { PointerDataId = value + 1; } }
        public uint PointerDataOffset { readonly get { return (uint)((Pointer >> 12) & 0xFFFFF); } init { Pointer = (Pointer & 0xFFFFFFFF00000FFF) + ((value << 12) & 0xFFFFF000); } }

        public Array_Structure(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_Structure(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public readonly Array_Structure SwapEnd()
        {
            return new Array_Structure
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1),
            };
        }
        public override readonly string ToString() => $"Array_Structure: {PointerDataIndex} ({Count1}/{Count2})";
    }
    [TC(typeof(EXP))]
    public readonly record struct Array_uint //16 bytes - pointer for a uint array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public uint Unk0 => (uint)(Pointer >> 32);
        public uint PointerDataId => (uint)(Pointer & 0xFFF);
        public uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);


        public Array_uint(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_uint(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public Array_uint SwapEnd()
        {
            return new Array_uint
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1),
            };
        }
        public override string ToString() => $"Array_uint {{ PointerDataIndex = {PointerDataIndex}, Count1 = {Count1}, Count2 = {Count2} }}";
    }

    [TC(typeof(EXP))]
    public readonly struct Array_ushort //16 bytes - pointer for a ushort array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public readonly uint Unk0 => (uint)(Pointer >> 32);
        public readonly uint PointerDataId => (uint)(Pointer & 0xFFF);
        public readonly uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public readonly uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);


        public Array_ushort(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_ushort(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public readonly Array_ushort SwapEnd()
        {
            return new Array_ushort
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1),
            };
        }
        public override readonly string ToString() => $"Array_ushort {{ PointerDataIndex = {PointerDataIndex}, Count1 = {Count1}, Count2 = {Count2} }}";
    }

    [TC(typeof(EXP))]
    public readonly record struct Array_byte //16 bytes - pointer for a byte array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public readonly uint Unk0 => (uint)(Pointer >> 32);
        public readonly uint PointerDataId => (uint)(Pointer & 0xFFF);
        public readonly uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public readonly uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);

        public Array_byte(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }

        public Array_byte(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public readonly Array_byte SwapEnd()
        {
            return new Array_byte
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1),
            };
        }

        public override readonly string ToString() => $"Array_byte: {PointerDataIndex} ({Count1}/{Count2})";
    }

    [TC(typeof(EXP))]
    public readonly struct Array_float //16 bytes - pointer for a float array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public uint Unk0 =>  (uint)(Pointer >> 32);
        public uint PointerDataId =>  (uint)(Pointer & 0xFFF);
        public uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);

        public Array_float(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_float(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public Array_float SwapEnd()
        {
            return new Array_float
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1)
            };
        }
        public override string ToString() => $"Array_float: {PointerDataIndex} ({Count1}/{Count2})";
    }

    [TC(typeof(EXP))]
    public readonly struct Array_Vector3 //16 bytes - pointer for a Vector3 array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public uint Unk0 => (uint)(Pointer >> 32);
        public uint PointerDataId => (uint)(Pointer & 0xFFF);
        public uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);

        public Array_Vector3(ulong ptr, int cnt)
        {
            Pointer = ptr;
            Count1 = (ushort)cnt;
            Count2 = Count1;
            Unk1 = 0;
        }
        public Array_Vector3(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public Array_Vector3 SwapEnd()
        {
            return new Array_Vector3
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1)
            };
        }
        public override string ToString() => $"Array_Vector3: {PointerDataIndex} ({Count1}/{Count2})";
    }

    [TC(typeof(EXP))]
    public readonly struct CharPointer //16 bytes - pointer for a char array
    {
        public ulong Pointer { get; init; }
        public ushort Count1 { get; init; }
        public ushort Count2 { get; init; }
        public uint Unk1 { get; init; }

        public uint Unk0 => (uint)(Pointer >> 32);
        public uint PointerDataId => (uint)(Pointer & 0xFFF);
        public uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);

        public CharPointer(ulong ptr, int len)
        {
            Pointer = ptr;
            Count1 = (ushort)len;
            Count2 = Count1;
            Unk1 = 0;
        }
        public CharPointer(in MetaBuilderPointer ptr)
        {
            Pointer = ptr.Pointer;
            Count1 = (ushort)ptr.Length;
            Count2 = Count1;
            Unk1 = 0;
        }

        public CharPointer SwapEnd()
        {
            return new CharPointer
            {
                Pointer = MetaTypes.SwapBytes(Pointer),
                Count1 = MetaTypes.SwapBytes(Count1),
                Count2 = MetaTypes.SwapBytes(Count2),
                Unk1 = MetaTypes.SwapBytes(Unk1)
            };
        }
        public override string ToString() => $"CharPointer: {Pointer} ({Count1}/{Count2})";
    }

    [TC(typeof(EXP))]
    public readonly struct DataBlockPointer //8 bytes - pointer to data block
    {
        public ulong Pointer { get; init; }

        public uint Unk0 => (uint)(Pointer >> 32);
        public uint PointerDataId => (uint)(Pointer & 0xFFF);
        public uint PointerDataIndex => (uint)(Pointer & 0xFFF) - 1;
        public uint PointerDataOffset => (uint)((Pointer >> 12) & 0xFFFFF);


        public DataBlockPointer(int blockId, int offset)
        {
            Pointer = ((uint)blockId & 0xFFF) | (((uint)offset & 0xFFFFF) << 12);
        }

        public override string ToString() => $"DataBlockPointer: {Pointer}";

        public DataBlockPointer SwapEnd()
        {
            return new DataBlockPointer()
            {
                Pointer = MetaTypes.SwapBytes(Pointer)
            };
        }
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfUshorts3 //array of 3 ushorts
    {
        public readonly ushort u0, u1, u2;

        public ArrayOfUshorts3(ushort u0, ushort u1, ushort u2)
        {
            this.u0 = u0;
            this.u1 = u1;
            this.u2 = u2;
        }

        public Vector3 XYZ()
        {
            return new Vector3(u0, u1, u2);
        }
        public override string ToString() => $"{u0}, {u1}, {u2}";
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfBytes3 //array of 3 bytes
    {
        public readonly byte b0, b1, b2;

        public ArrayOfBytes3(byte b0, byte b1, byte b2)
        {
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
        }

        public byte[] GetArray()
        {
            return [b0, b1, b2];
        }
        public override string ToString() => $"{b0}, {b1}, {b2}";
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfBytes4 //array of 4 bytes
    {
        public readonly byte b0, b1, b2, b3;
        public byte[] GetArray()
        {
            return [b0, b1, b2, b3];
        }
        public override string ToString() => $"{b0}, {b1}, {b2}, {b3}";
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfBytes5 //array of 5 bytes
    {
        public readonly byte b0, b1, b2, b3, b4;
        public byte[] GetArray()
        {
            return [b0, b1, b2, b3, b4];
        }
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfBytes6 //array of 6 bytes
    {
        public readonly byte b0, b1, b2, b3, b4, b5;
        public byte[] GetArray()
        {
            return [b0, b1, b2, b3, b4, b5];
        }
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfBytes12 //array of 12 bytes
    {
        public readonly byte b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11;
        public byte[] GetArray()
        {
            return [b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11];
        }
    }

    [TC(typeof(EXP))]
    public readonly struct ArrayOfChars64 //array of 64 chars (bytes)
    {
        public readonly byte
            b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
            b20, b21, b22, b23, b24, b25, b26, b27, b28, b29, b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
            b40, b41, b42, b43, b44, b45, b46, b47, b48, b49, b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
            b60, b61, b62, b63;
        //public override readonly string ToString()
        //{
        //    byte[] bytes =
        //    {
        //        b00, b01, b02, b03, b04, b05, b06, b07, b08, b09, b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
        //        b20, b21, b22, b23, b24, b25, b26, b27, b28, b29, b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
        //        b40, b41, b42, b43, b44, b45, b46, b47, b48, b49, b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
        //        b60, b61, b62, b63
        //    };
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < bytes.Length; i++)
        //    {
        //        if (bytes[i] == 0)
        //            break;
        //        sb.Append((char)bytes[i]);
        //    }
        //    return sb.ToString();
        //}

        public ReadOnlySpan<byte> GetBytes()
        {
            return MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in this, 1));
        }

        public override string ToString()
        {
            var byteArr = GetBytes();

            var index = byteArr.IndexOf((byte)0);

            if (index == -1)
            {
                return Encoding.ASCII.GetString(byteArr);
            }

            return Encoding.ASCII.GetString(byteArr.Slice(0, index));
        }
    }

    [TC(typeof(EXP))]
    public readonly struct MetaVECTOR3(in Vector3 v) //12 bytes, Key:2751397072
    {
        public float X { get; init; } = v.X; //0   0: Float: 0: x
        public float Y { get; init; } = v.Y; //4   4: Float: 0: y
        public float Z { get; init; } = v.Z; //8   8: Float: 0: z

        public override string ToString() => $"{X}, {Y}, {Z}";
        public Vector3 ToVector3() => new Vector3(X, Y, Z);
    }

    [TC(typeof(EXP))]
    public readonly record struct MetaPOINTER //8 bytes - pointer to data item //was: SectionUNKNOWN10
    {
        public readonly ulong Pointer;

        public int BlockIndex => BlockID - 1;
        public int BlockID => (int)(Pointer & 0xFFF);
        public int Offset => (int)((Pointer >> 12) & 0xFFFFF);

        public MetaPOINTER(int blockID, int itemOffset)
        {
            Pointer = (((uint)itemOffset << 12) & 0xFFFFF000) + ((uint)blockID & 0xFFF);
        }

        public override string ToString() => $"{BlockID}, {Offset}";
    }

    [TC(typeof(EXP))]
    public readonly record struct MetaHash(uint Hash) : IEquatable<MetaHash>
    {
        public string Hex => Hash.ToString("X8");
        public float Float => MetaTypes.ConvertData<float>(MetaTypes.ConvertToBytes(Hash));
        public short Short1 => (short)(Hash & 0xFFFF);
        public short Short2 => (short)((Hash >> 16) & 0xFFFF);

        public override string ToString()
        {
            if (JenkIndex.TryGetString(Hash, out var str))
            {
                return str;
            }
            if (MetaNames.TryGetString(Hash, out str))
            {
                return str;
            }
            return GlobalText.GetString(Hash);
        }

        public string ToCleanString()
        {
            if (Hash == 0)
                return string.Empty;
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

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
    }

    
    [TC(typeof(EXP))]
    public readonly record struct TextHash(uint Hash)
    {
        public string Hex => Hash.ToString("X");

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