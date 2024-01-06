/*
    Copyright(c) 2015 Neodymium

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

//shamelessly stolen and mangled






/*

Regarding saving PSO files:

[for checksum - use whole file]
Brick - Today
uint32_t joaat_checksum(const void* memory, const size_t length)
{
    uint32_t hash = 0x3FAC7125;

    for (size_t i = 0; i < length; ++i)
    {
        hash += static_cast<const int8_t*>(memory)[i];
        hash += hash << 10;
        hash ^= hash >> 6;
    }

    hash += hash << 3;
    hash ^= hash >> 11;
    hash += hash << 15;

    return hash;
}


[before doing checksum for file:]
      v12->Checksum = 0;
      v12->FileSize = 0;
      v12->Magic = 'SKHC';
      v12->Size = 0x14000000;
      v22 = v12;
      LOBYTE(v12->Platform) = platformChar[0];







Brick - Today
This is a table i made a while ago for the pso types btw
| Index | Type   | Size | Align | Name       | Serialization
| 0     | Simple | 1    | 1     | bool       |
| 1     | Simple | 1    | 1     | s8         |
| 2     | Simple | 1    | 1     | u8         |
| 3     | Simple | 2    | 2     | s16        |
| 4     | Simple | 2    | 2     | u16        |
| 5     | Simple | 4    | 4     | s32        |
| 6     | Simple | 4    | 4     | u32        |
| 7     | Simple | 4    | 4     | f32        |
| 8     | Vector | 8    | 4     | vec2       |
| 9     | Vector | 16   | 16    | vec3       |
| 10    | Vector | 16   | 16    | vec4       |
| 11    | String | 0    | 0     | string     |
| 12    | Struct | 0    | 0     | struct     |
| 13    | Array  | 0    | 0     | array      |
| 14    | Enum   | 0    | 0     | enum       |
| 15    | Bitset | 0    | 0     | bitset     |
| 16    | Map    | 0    | 0     | map        |
| 17    | Matrix | 64   | 16    | matrix43   | shuffled
| 18    | Matrix | 64   | 16    | matrix44   | shuffled
| 19    | Vector | 16   | 16    | vec4       | x, y, x, x
| 20    | Vector | 16   | 16    | vec4       | x, y, z, x
| 21    | Vector | 16   | 16    | vec4       | x, y, z, w
| 22    | Matrix | 48   | 16    | matrix34   |
| 23    | Matrix | 64   | 16    | matrix43   |
| 24    | Matrix | 64   | 16    | matrix44   |
| 25    | Simple | 16   | 16    | f32_vec4   | fill all with f32
| 26    | Simple | 16   | 16    | bool_int4  | fill all with 0x00000000 or 0xFFFFFFFF depending on bool
| 27    | Vector | 16   | 16    | bool4_int4 | fill each with 0x00000000 or 0xFFFFFFFF depending on bools
| 28    | Simple | 8    | 8     | s32_i64    | sign extended s32
| 29    | Simple | 8    | 8     | s32_u64    | sign extended s32
| 30    | Simple | 2    | 2     | f16        | f64 converted to f16
| 31    | Simple | 8    | 8     | s64        |
| 32    | Simple | 8    | 8     | u64        |
| 33    | Simple | 8    | 8     | f64        |



 */





using CodeWalker.Core.Utils;
using CommunityToolkit.Diagnostics;
using SharpDX.Win32;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{


    public enum PsoSection : uint
    {
        PSIN = 0x5053494E,
        PMAP = 0x504D4150,
        PSCH = 0x50534348,
        PSIG = 0x50534947,
        STRF = 0x53545246,
        STRS = 0x53545253,
        STRE = 0x53545245,
        CHKS = 0x43484B53,
    }

    public enum PsoDataType : byte
    {
        Bool = 0x00,
        SByte = 0x01,
        UByte = 0x02,
        SShort = 0x03,
        UShort = 0x04,
        SInt = 0x05,
        UInt = 0x06,
        Float = 0x07,
        Float2 = 0x08,
        Float3 = 0x09,
        Float4 = 0x0a,
        String = 0x0b,
        Structure = 0x0c,
        Array = 0x0d,
        Enum = 0x0e,
        Flags = 0x0f,
        Map = 0x10,
        Float3a = 0x14,
        Float4a = 0x15,
        HFloat = 0x1e,
        Long = 0x20,
    }
    public static class PsoDataTypes
    {
        public static string GetCSharpTypeName(PsoDataType t)
        {
            switch (t)
            {
                case PsoDataType.Bool: return "bool";
                case PsoDataType.SByte: return "sbyte";
                case PsoDataType.UByte: return "byte";
                case PsoDataType.SShort: return "short";
                case PsoDataType.UShort: return "ushort";
                case PsoDataType.SInt: return "int";
                case PsoDataType.UInt: return "int";
                case PsoDataType.Float: return "float";
                case PsoDataType.Float2: return "long";
                case PsoDataType.String: return "uint"; //hash? NEEDS WORK?
                case PsoDataType.Enum: return "byte";
                case PsoDataType.Flags: return "short";
                case PsoDataType.HFloat: return "short";
                case PsoDataType.Long: return "long";
                case PsoDataType.Float3:
                case PsoDataType.Float4:
                case PsoDataType.Map:
                case PsoDataType.Float3a:
                case PsoDataType.Float4a:
                case PsoDataType.Structure:
                case PsoDataType.Array:
                default:
                    return t.ToString();
            }
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class PsoFile
    {
        public PsoDataSection DataSection { get; set; }
        public PsoDataMapSection DataMapSection { get; set; }
        public PsoSchemaSection SchemaSection { get; set; }
        public PsoSTRFSection STRFSection { get; set; }
        public PsoSTRSSection STRSSection { get; set; }
        public PsoPSIGSection PSIGSection { get; set; }
        public PsoSTRESection STRESection { get; set; }
        public PsoCHKSSection CHKSSection { get; set; }


        public void Load(byte[] data)
        {
            var sequence = new ReadOnlySequence<byte>(data);
            var reader = new SequenceReader<byte>(sequence);

            Load(ref reader);
        }

        public void Load(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
                Load(stream);
        }

        public virtual void Load(Stream stream)
        {
            if (stream.Position != 0)
            {
                stream.Position = 0;
            }

            var reader = new DataReader(stream, Endianess.BigEndian);
            while (reader.Position < reader.Length)
            {
                var identInt = reader.ReadUInt32();
                var ident = (PsoSection)identInt;
                var length = reader.ReadInt32();

                reader.Position -= 8;

                var sectionData = reader.ReadBytes(length);
                var sectionStream = new MemoryStream(sectionData);
                var sectionReader = new DataReader(sectionStream, Endianess.BigEndian);

                switch (ident)
                {
                    case PsoSection.PSIN:  //0x5053494E  "PSIN"  - ID / data section
                        DataSection = new PsoDataSection();
                        DataSection.Read(sectionReader);
                        break;
                    case PsoSection.PMAP:  //0x504D4150  "PMAP"  //data mapping
                        DataMapSection = new PsoDataMapSection();
                        DataMapSection.Read(sectionReader);
                        break;
                    case PsoSection.PSCH:  //0x50534348  "PSCH"  //schema
                        SchemaSection = new PsoSchemaSection();
                        SchemaSection.Read(sectionReader);
                        break;
                    case PsoSection.STRF:  //0x53545246  "STRF"  //paths/STRINGS  (folder strings?)
                        STRFSection = new PsoSTRFSection();
                        STRFSection.Read(sectionReader);
                        break;
                    case PsoSection.STRS:  //0x53545253  "STRS"  //names/strings  (DES_)
                        STRSSection = new PsoSTRSSection();
                        STRSSection.Read(sectionReader);
                        break;
                    case PsoSection.STRE:  //0x53545245  "STRE"  //probably encrypted strings.....
                        STRESection = new PsoSTRESection();
                        STRESection.Read(sectionReader);
                        break;
                    case PsoSection.PSIG:  //0x50534947  "PSIG"  //signature?
                        PSIGSection = new PsoPSIGSection();
                        PSIGSection.Read(sectionReader);
                        break;
                    case PsoSection.CHKS:  //0x43484B53  "CHKS"  //checksum?
                        CHKSSection = new PsoCHKSSection();
                        CHKSSection.Read(sectionReader);
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void Load(ref SequenceReader<byte> reader)
        {
            if (reader.Consumed != 0)
            {
                reader.SetPosition(0);
            }
            while (reader.Consumed < reader.Length)
            {
                var identInt = reader.ReadUInt32BigEndian();
                var ident = (PsoSection)identInt;
                var length = reader.ReadInt32BigEndian();

                reader.Rewind(8);

                var sectionData = reader.ReadSubSequence(length);
                var sectionReader = new SequenceReader<byte>(sectionData);

                switch (ident)
                {
                    case PsoSection.PSIN:  //0x5053494E  "PSIN"  - ID / data section
                        DataSection = new PsoDataSection();
                        DataSection.Read(ref sectionReader, identInt, length);
                        break;
                    case PsoSection.PMAP:  //0x504D4150  "PMAP"  //data mapping
                        DataMapSection = new PsoDataMapSection();
                        DataMapSection.Read(ref sectionReader);
                        break;
                    case PsoSection.PSCH:  //0x50534348  "PSCH"  //schema
                        SchemaSection = new PsoSchemaSection();
                        SchemaSection.Read(ref sectionReader);
                        break;
                    case PsoSection.STRF:  //0x53545246  "STRF"  //paths/STRINGS  (folder strings?)
                        STRFSection = new PsoSTRFSection();
                        STRFSection.Read(ref sectionReader);
                        break;
                    case PsoSection.STRS:  //0x53545253  "STRS"  //names/strings  (DES_)
                        STRSSection = new PsoSTRSSection();
                        STRSSection.Read(ref sectionReader);
                        break;
                    case PsoSection.STRE:  //0x53545245  "STRE"  //probably encrypted strings.....
                        STRESection = new PsoSTRESection();
                        STRESection.Read(ref sectionReader);
                        break;
                    case PsoSection.PSIG:  //0x50534947  "PSIG"  //signature?
                        PSIGSection = new PsoPSIGSection();
                        PSIGSection.Read(ref sectionReader);
                        break;
                    case PsoSection.CHKS:  //0x43484B53  "CHKS"  //checksum?
                        CHKSSection = new PsoCHKSSection();
                        CHKSSection.Read(ref sectionReader);
                        break;
                    default:
                        break;
                }
            }
        }


        public byte[] Save()
        {
            var ms = new MemoryStream();
            Save(ms);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            return buf;
        }

        public void Save(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
                Save(stream);
        }

        public virtual void Save(Stream stream)
        {
            var writer = new DataWriter(stream, Endianess.BigEndian);
            if (DataSection != null) DataSection.Write(writer);
            if (DataMapSection != null) DataMapSection.Write(writer);
            if (SchemaSection != null) SchemaSection.Write(writer);
            if (STRFSection != null) STRFSection.Write(writer);
            if (STRSSection != null) STRSSection.Write(writer);
            if (STRESection != null) STRESection.Write(writer);
            if (PSIGSection != null) PSIGSection.Write(writer);
            if (CHKSSection != null) CHKSSection.Write(writer);
        }





        public bool TryGetBlock(int id, out PsoDataMappingEntry block)
        {
            if (DataMapSection?.Entries is null)
            {
                block = default;
                return false;
            }
            var ind = id - 1;
            var blocks = DataMapSection.Entries;
            if ((ind >= 0) && (ind < blocks.Length))
            {
                block = blocks[ind];
                return true;
            }

            block = default;
            return false;
        }

        public PsoDataMappingEntry? GetBlock(int id)
        {
            if (DataMapSection?.Entries == null)
                return null;
            PsoDataMappingEntry? block = null;
            var ind = id - 1;
            var blocks = DataMapSection.Entries;
            if ((ind >= 0) && (ind < blocks.Length))
            {
                block = blocks[ind];
            }
            return block;
        }

        public const uint PSO_IDENT_LITTLE_ENDIAN = 0x4e495350;
        public static readonly uint PSO_IDENT_BIG_ENDIAN = BinaryPrimitives.ReverseEndianness(PSO_IDENT_LITTLE_ENDIAN);


        public static bool IsPSO(Stream stream)
        {
            //return !IsRBF(stream);

            //1347635534

            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            stream.Position = 0;
            return IsPSO(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPSO(Span<byte> data)
        {
            var identInt = BinaryPrimitives.ReadUInt32LittleEndian(data);

            return IsPsoLittleEndian(identInt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPSO(uint identInt)
        {
            return identInt == PSO_IDENT_BIG_ENDIAN;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPsoLittleEndian(uint identInt)
        {
            return identInt == PSO_IDENT_LITTLE_ENDIAN;
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class PsoDataSection
    {
        public uint Ident { get; set; } = 0x5053494E;
        public int Length { get; private set; }
        public byte[] Data { get; set; }

        public void Read(DataReader reader)
        {
            Ident = reader.ReadUInt32();
            Length = reader.ReadInt32();
            reader.Position -= 8;
            Data = reader.ReadBytes(Length);
        }

        public void Read(ref SequenceReader<byte> reader, uint ident, int length)
        {
            Ident = ident;
            Length = length;
            Data = reader.ReadBytes(Length).ToArray();
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadUInt32BigEndian();
            Length = reader.ReadInt32BigEndian();
            reader.Rewind(8);
            Data = reader.ReadBytes(Length).ToArray();
        }

        public void Write(DataWriter writer)
        {
            Length = Data.Length;

            writer.Write(Data);
            writer.Position -= Length;
            writer.Write(Ident);
            writer.Write((uint)(Length));
            writer.Position += Length - 8;
        }

        public override string ToString()
        {
            return $"{Ident}: {Length}";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoDataMapSection
    {
        public int Ident { get; set; } = 0x504D4150;
        public int Length { get; private set; }
        public int RootId { get; set; }
        public short EntriesCount { get; private set; }
        public short Unknown_Eh { get; set; } = 0x7070;
        public PsoDataMappingEntry[] Entries { get; set; }

        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();
            RootId = reader.ReadInt32();
            EntriesCount = reader.ReadInt16();
            Unknown_Eh = reader.ReadInt16();

            if (EntriesCount <= 0) //any other way to know which version?
            {
                EntriesCount = reader.ReadInt16();
                var unk1 = reader.ReadInt16();
                var unk2 = reader.ReadInt16();
                var unk3 = reader.ReadInt16();
            }

            Entries = new PsoDataMappingEntry[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                Entries[i] = new PsoDataMappingEntry(reader);
            }
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();
            RootId = reader.ReadInt32BigEndian();
            EntriesCount = reader.ReadInt16BigEndian();
            Unknown_Eh = reader.ReadInt16BigEndian();

            if (EntriesCount <= 0) //any other way to know which version?
            {
                EntriesCount = reader.ReadInt16BigEndian();
                reader.Advance(6);
                //var unk1 = reader.ReadInt16BigEndian();
                //var unk2 = reader.ReadInt16BigEndian();
                //var unk3 = reader.ReadInt16BigEndian();
            }

            Entries = new PsoDataMappingEntry[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                Entries[i] = new PsoDataMappingEntry(ref reader);
            }
        }

        public void Write(DataWriter writer)
        {
            // update...
            EntriesCount = (short)Entries.Length;
            Length = 16 + EntriesCount * 16;

            writer.Write(Ident);
            writer.Write(Length);
            writer.Write(RootId);
            writer.Write(EntriesCount);
            writer.Write(Unknown_Eh);
            foreach (var entry in Entries)
            {
                entry.Write(writer);
            }
        }

        public override string ToString()
        {
            return $"{Ident}: {EntriesCount}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public readonly struct PsoDataMappingEntry
    {
        public MetaName NameHash { get; init; }
        public int Offset { get; init; }
        public int Unknown_8h { get; init; }
        public int Length { get; init; }

        public PsoDataMappingEntry(DataReader reader)
        {
            NameHash = (MetaName)reader.ReadUInt32();
            Offset = reader.ReadInt32();
            Unknown_8h = reader.ReadInt32();
            Length = reader.ReadInt32();
        }

        public PsoDataMappingEntry(ref SequenceReader<byte> reader)
        {
            NameHash = (MetaName)reader.ReadUInt32BigEndian();
            Offset = reader.ReadInt32BigEndian();
            Unknown_8h = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();
        }

        public void Write(DataWriter writer)
        {
            writer.Write((uint)NameHash);
            writer.Write(Offset);
            writer.Write(Unknown_8h);
            writer.Write(Length);
        }

        public override string ToString()
        {
            return $"{NameHash}: {Offset}: {Length}";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoSchemaSection
    {
        public int Ident { get; private set; } = 0x50534348;
        public int Length { get; set; }
        public uint Count { get; set; }

        public PsoElementIndexInfo[] EntriesIdx { get; set; }
        public PsoElementInfo[] Entries { get; set; }

        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();
            Count = reader.ReadUInt32();

            EntriesIdx = new PsoElementIndexInfo[Count];
            for (int i = 0; i < Count; i++)
            {
                EntriesIdx[i] = new PsoElementIndexInfo(reader);
            }

            Entries = new PsoStructureInfo[Count];
            for (int i = 0; i < Count; i++)
            {
                reader.Position = EntriesIdx[i].Offset;
                var type = reader.ReadByte();

                reader.Position = EntriesIdx[i].Offset;
                if (type == 0)
                {
                    Entries[i] = new PsoStructureInfo(reader) { IndexInfo = EntriesIdx[i] };
                }
                else if (type == 1)
                {
                    Entries[i] = new PsoEnumInfo(reader) { IndexInfo = EntriesIdx[i] };
                }
                else
                    ThrowHelper.ThrowNotSupportedException("Unknown type");
            }
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();
            Count = reader.ReadUInt32BigEndian();

            EntriesIdx = new PsoElementIndexInfo[Count];
            for (int i = 0; i < Count; i++)
            {
                EntriesIdx[i] = new PsoElementIndexInfo(ref reader);
            }

            Entries = new PsoElementInfo[Count];
            for (int i = 0; i < Count; i++)
            {
                reader.SetPosition(EntriesIdx[i].Offset);
                var type = reader.ReadByte();

                reader.SetPosition(EntriesIdx[i].Offset);
                if (type == 0)
                {
                    Entries[i] = new PsoStructureInfo(ref reader) { IndexInfo = EntriesIdx[i] };
                }
                else if (type == 1)
                {
                    Entries[i] = new PsoEnumInfo(ref reader) { IndexInfo = EntriesIdx[i], };
                }
                else
                    ThrowHelper.ThrowNotSupportedException("Unknown type");
            }
        }

        public void Write(DataWriter writer)
        {

            var entriesStream = new MemoryStream();
            var entriesWriter = new DataWriter(entriesStream, Endianess.BigEndian);
            for (int i = 0; i < Entries.Length; i++)
            {
                EntriesIdx[i] = EntriesIdx[i] with { Offset = 12 + 8 * Entries.Length + (int)entriesWriter.Position, };
                Entries[i].Write(entriesWriter);
            }



            var indexStream = new MemoryStream();
            var indexWriter = new DataWriter(indexStream, Endianess.BigEndian);
            foreach (var entry in EntriesIdx)
                entry.Write(indexWriter);




            writer.Write(Ident);
            writer.Write((int)(12 + entriesStream.Length + indexStream.Length));
            writer.Write((uint)(Entries.Length));

            // write entries index data
            var buf1 = new byte[indexStream.Length];
            indexStream.Position = 0;
            indexStream.Read(buf1, 0, buf1.Length);
            writer.Write(buf1);

            // write entries data
            var buf2 = new byte[entriesStream.Length];
            entriesStream.Position = 0;
            entriesStream.Read(buf2, 0, buf2.Length);
            writer.Write(buf2);


        }

        public override string ToString()
        {
            return $"{Ident}: {Count}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct PsoElementIndexInfo(MetaName nameHash, int offset)
    {
        public MetaName NameHash { get; init; } = nameHash;
        public int Offset { get; init; } = offset;

        public PsoElementIndexInfo(DataReader reader)
            : this ((MetaName)reader.ReadUInt32(), reader.ReadInt32())
        { }

        public PsoElementIndexInfo(ref SequenceReader<byte> reader)
            : this((MetaName)reader.ReadUInt32BigEndian(), reader.ReadInt32BigEndian())
        { }

        public void Write(DataWriter writer)
        {
            writer.Write((uint)NameHash);
            writer.Write(Offset);
        }

        public override string ToString()
        {
            return $"{NameHash}: {Offset}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface PsoElementInfo
    {
        public PsoElementIndexInfo IndexInfo { get; set; }

        public abstract void Write(DataWriter writer);
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoStructureInfo : PsoElementInfo
    {
        public PsoElementIndexInfo IndexInfo { get; set; }
        public byte Type { get; set; } = 0;
        public short EntriesCount { get; private set; }
        public byte Unk { get; set; }
        public int StructureLength { get; set; }
        public uint Unk_Ch { get; set; } = 0x00000000;
        public PsoStructureEntryInfo[] Entries { get; set; }

        public PsoStructureInfo()
        { }

        public PsoStructureInfo(DataReader reader)
        {
            uint x = reader.ReadUInt32();
            Type = (byte)((x & 0xFF000000) >> 24);
            EntriesCount = (short)(x & 0xFFFF);
            Unk = (byte)((x & 0x00FF0000) >> 16);
            StructureLength = reader.ReadInt32();
            Unk_Ch = reader.ReadUInt32();

            Entries = new PsoStructureEntryInfo[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                Entries[i] = new PsoStructureEntryInfo(reader);
            }
        }

        public PsoStructureInfo(ref SequenceReader<byte> reader)
        {
            uint x = reader.ReadUInt32BigEndian();
            Type = (byte)((x & 0xFF000000) >> 24);
            EntriesCount = (short)(x & 0xFFFF);
            Unk = (byte)((x & 0x00FF0000) >> 16);
            StructureLength = reader.ReadInt32BigEndian();
            Unk_Ch = reader.ReadUInt32BigEndian();

            Entries = new PsoStructureEntryInfo[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                Entries[i] = new PsoStructureEntryInfo(ref reader);
            }
        }

        public PsoStructureInfo(MetaName nameHash, byte type, byte unk, int length, params PsoStructureEntryInfo[] entries)
        {
            IndexInfo = new PsoElementIndexInfo(nameHash, 0);

            Type = type;
            EntriesCount = (short)(entries?.Length ?? 0);
            Unk = unk;
            StructureLength = length;
            Unk_Ch = 0;
            Entries = entries;
        }

        public void Write(DataWriter writer)
        {
            Type = 0;
            EntriesCount = (short)Entries.Length;

            uint typeAndEntriesCount = (uint)(Type << 24) | (uint)(Unk << 16) | (ushort)EntriesCount;
            writer.Write(typeAndEntriesCount);
            writer.Write(StructureLength);
            writer.Write(Unk_Ch);

            foreach (var entry in Entries)
            {
                entry.Write(writer);
            }
        }

        public override string ToString()
        {
            return $"{IndexInfo} - {Type}: {EntriesCount}";
        }

        public bool TryFindEntry(MetaName name, [MaybeNullWhen(false)] out PsoStructureEntryInfo entryInfo)
        {
            if (Entries is null)
            {
                entryInfo = default;
                return false;
            }

            foreach (var entry in Entries.AsSpan())
            {
                if (entry.EntryNameHash == name)
                {
                    entryInfo = entry;
                    return true;
                }
            }

            entryInfo = default;
            return false;
        }

        public PsoStructureEntryInfo? FindEntry(MetaName name)
        {
            if (Entries != null)
            {
                foreach (var entry in Entries)
                {
                    if (entry.EntryNameHash == name)
                        return entry;
                }
            }
            return null;
        }

        public bool TryGetEntry(int id, [MaybeNullWhen(false)] out PsoStructureEntryInfo entry)
        {
            if ((Entries != null) && (id >= 0) && (id < Entries.Length))
            {
                entry = Entries[id];
                return true;
            }
            entry = default;
            return false;
        }

        public PsoStructureEntryInfo? GetEntry(int id)
        {
            if ((Entries != null) && (id >= 0) && (id < Entries.Length))
            {
                return Entries[id];
            }
            return null;
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public readonly struct PsoStructureEntryInfo
    {
        public MetaName EntryNameHash { get; init; }
        public PsoDataType Type { get; init; }
        public byte Unk_5h { get; init; } //0 = default, 3 = pointer array?
        public ushort DataOffset { get; init; }
        public uint ReferenceKey { get; init; } // when array -> entry index with type


        public PsoStructureEntryInfo()
        { }


        public PsoStructureEntryInfo(DataReader reader)
        {
            EntryNameHash = (MetaName)reader.ReadUInt32();
            Type = (PsoDataType)reader.ReadByte();
            Unk_5h = reader.ReadByte();
            DataOffset = reader.ReadUInt16();
            ReferenceKey = reader.ReadUInt32();
        }

        public PsoStructureEntryInfo(ref SequenceReader<byte> reader)
        {
            EntryNameHash = (MetaName)reader.ReadUInt32BigEndian();
            Type = (PsoDataType)reader.ReadByte();
            Unk_5h = reader.ReadByte();
            DataOffset = reader.ReadUInt16BigEndian();
            ReferenceKey = reader.ReadUInt32BigEndian();
        }

        public PsoStructureEntryInfo(MetaName nameHash, PsoDataType type, ushort offset, byte unk, MetaName refKey)
        {
            EntryNameHash = nameHash;
            Type = type;
            Unk_5h = unk;
            DataOffset = offset;
            ReferenceKey = (uint)refKey;
        }

        public void Write(DataWriter writer)
        {
            writer.Write((uint)EntryNameHash);
            writer.Write((byte)Type);
            writer.Write(Unk_5h);
            writer.Write(DataOffset);
            writer.Write(ReferenceKey);
        }

        public override string ToString()
        {
            if(ReferenceKey!=0)
            {
                return $"{EntryNameHash}: {Type}: {DataOffset}: {(MetaName)ReferenceKey}";
            }
            return $"{EntryNameHash}: {Type}: {DataOffset}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoEnumInfo : PsoElementInfo
    {
        public PsoElementIndexInfo IndexInfo { get; set; }
        public byte Type { get; private set; } = 1;
        public int EntriesCount { get; private set; }
        public PsoEnumEntryInfo[] Entries { get; set; }


        public PsoEnumInfo()
        { }
        public PsoEnumInfo(MetaName nameHash, byte type, params PsoEnumEntryInfo[] entries)
        {
            IndexInfo = new PsoElementIndexInfo(nameHash, 0);

            EntriesCount = entries?.Length ?? 0;
            Entries = entries;
        }

        public PsoEnumInfo(DataReader reader)
        {
            uint x = reader.ReadUInt32();
            Type = (byte)((x & 0xFF000000) >> 24);
            EntriesCount = (int)(x & 0x00FFFFFF);

            Entries = new PsoEnumEntryInfo[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                Entries[i] = new PsoEnumEntryInfo(reader);
            }
        }

        public PsoEnumInfo(ref SequenceReader<byte> reader)
        {
            uint x = reader.ReadUInt32BigEndian();
            Type = (byte)((x & 0xFF000000) >> 24);
            EntriesCount = (int)(x & 0x00FFFFFF);

            Entries = new PsoEnumEntryInfo[EntriesCount];
            for (int i = 0; i < EntriesCount; i++)
            {
                Entries[i] = new PsoEnumEntryInfo(ref reader);
            }
        }

        public void Write(DataWriter writer)
        {
            // update...
            Type = 1;
            EntriesCount = Entries.Length;

            uint typeAndEntriesCount = (uint)(Type << 24) | (uint)EntriesCount;
            writer.Write(typeAndEntriesCount);

            foreach (var entry in Entries)
            {
                entry.Write(writer);
            }
        }

        public bool TryFindEntry(int val, out PsoEnumEntryInfo entry)
        {
            if (Entries == null)
            {
                entry = default;
                return false;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                var _entry = Entries[i];
                if (_entry.EntryKey == val)
                {
                    entry = _entry;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public PsoEnumEntryInfo? FindEntry(int val)
        {
            if (TryFindEntry(val, out var entry))
            {
                return entry;
            }
            return null;
        }

        public bool TryFindEntryByName(MetaName name, out PsoEnumEntryInfo entry)
        {
            if (Entries == null)
            {
                entry = default;
                return false;
            }
            for (int i = 0; i < Entries.Length; i++)
            {
                var _entry = Entries[i];
                if (_entry.EntryNameHash == name)
                {
                    entry = _entry;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public PsoEnumEntryInfo? FindEntryByName(MetaName name)
        {
            if (!TryFindEntryByName(name, out var entry))
            {
                return entry;
            }

            return null;
        }



        public override string ToString()
        {
            return $"{IndexInfo} - {Type}: {EntriesCount}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public readonly struct PsoEnumEntryInfo
    {
        public readonly MetaName EntryNameHash;
        public readonly int EntryKey;


        public PsoEnumEntryInfo()
        { }
        public PsoEnumEntryInfo(MetaName nameHash, int key)
        {
            EntryNameHash = nameHash;
            EntryKey = key;
        }

        public PsoEnumEntryInfo(DataReader reader)
        {
            EntryNameHash = (MetaName)reader.ReadUInt32();
            EntryKey = reader.ReadInt32();
        }

        public PsoEnumEntryInfo(ref SequenceReader<byte> reader)
        {
            EntryNameHash = (MetaName)reader.ReadUInt32BigEndian();
            EntryKey = reader.ReadInt32BigEndian();
        }

        public void Write(DataWriter writer)
        {
            writer.Write((uint)EntryNameHash);
            writer.Write(EntryKey);
        }

        public override string ToString()
        {
            return $"{EntryNameHash}: {EntryKey}";
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoSTRFSection
    {
        public int Ident { get; private set; } = 0x53545246;
        public int Length { get; set; }
        public string[] Strings { get; set; }

        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();
            List<string> strs = new List<string>();
            while (reader.Position < reader.Length)
            {
                var str = reader.ReadString();
                strs.Add(str);
                JenkIndex.EnsureBoth(str);
            }
            Strings = strs.ToArray();
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();
            List<string> strs = new List<string>();
            while (reader.Consumed < reader.Length)
            {
                var str = reader.ReadString();
                strs.Add(str);
                JenkIndex.EnsureBoth(str);
            }
            Strings = strs.ToArray();
        }

        public void Write(DataWriter writer)
        {
            using var strStream = new MemoryStream();
            using var strWriter = new DataWriter(strStream, Endianess.BigEndian);
            foreach (var str in Strings)
            {
                strWriter.Write(str);
            }

            Length = (int)strStream.Length + 8;

            writer.Write(Ident);
            writer.Write(Length);

            if (strStream.Length > 0)
            {
                var buf1 = new byte[strStream.Length];
                strStream.Position = 0;
                strStream.Read(buf1, 0, buf1.Length);
                writer.Write(buf1);
            }

        }

        public override string ToString()
        {
            return $"{Ident}: {Length}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoSTRSSection
    {
        public int Ident { get; private set; } = 0x53545253;
        public int Length { get; set; }
        public string[] Strings { get; set; }


        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();

            List<string> strs = new List<string>();
            while (reader.Position < reader.Length)
            {
                var str = reader.ReadString();
                strs.Add(str);
                JenkIndex.EnsureBoth(str);
            }
            Strings = strs.ToArray();
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();

            List<string> strs = new List<string>();
            while (reader.Consumed < reader.Length)
            {
                var str = reader.ReadString();
                strs.Add(str);
                JenkIndex.EnsureBoth(str);
            }
            Strings = strs.ToArray();
        }

        public void Write(DataWriter writer)
        {
            var strStream = new MemoryStream();
            var strWriter = new DataWriter(strStream, Endianess.BigEndian);
            foreach (var str in Strings)
            {
                strWriter.Write(str);
            }

            Length = (int)strStream.Length + 8;

            writer.Write(Ident);
            writer.Write(Length);

            if (strStream.Length > 0)
            {
                var buf1 = new byte[strStream.Length];
                strStream.Position = 0;
                strStream.Read(buf1, 0, buf1.Length);
                writer.Write(buf1);
            }

        }

        public override string ToString()
        {
            return $"{Ident}: {Length}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoSTRESection
    {
        public int Ident { get; private set; } = 0x53545245;
        public int Length { get; set; }
        public byte[] Data { get; set; }

        //public byte[] Decr1 { get; set; }
        //public byte[] Decr2 { get; set; }

        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();

            if (Length > 8)
            {
                Data = reader.ReadBytes(Length - 8);

                //Decr1 = GTACrypto.DecryptAES(Data);
                //Decr2 = GTACrypto.DecryptNG(Data, )

                //TODO: someone plz figure out that encryption
            }
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();

            if (Length > 8)
            {
                Data = reader.ReadBytes(Length - 8).ToArray();

                //Decr1 = GTACrypto.DecryptAES(Data);
                //Decr2 = GTACrypto.DecryptNG(Data, )

                //TODO: someone plz figure out that encryption
            }
        }

        public void Write(DataWriter writer)
        {

            Length = (Data?.Length??0) + 8;

            writer.Write(Ident);
            writer.Write(Length);

            if (Length > 8)
            {
                writer.Write(Data);
            }

        }

        public override string ToString()
        {
            return $"{Ident}: {Length}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoPSIGSection
    {
        public int Ident { get; private set; } = 0x50534947;
        public int Length { get; set; }
        public byte[] Data { get; set; }

        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();

            if (Length > 8)
            {
                Data = reader.ReadBytes(Length - 8);
            }
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();

            if (Length > 8)
            {
                Data = reader.ReadBytes(Length - 8).ToArray();
            }
        }

        public void Write(DataWriter writer)
        {
            Length = (Data?.Length ?? 0) + 8;

            writer.Write(Ident);
            writer.Write(Length);

            if (Length > 8)
            {
                writer.Write(Data);
            }

        }

        public override string ToString()
        {
            return $"{Ident}: {Length}";
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PsoCHKSSection
    {
        public int Ident { get; private set; } = 0x43484B53;
        public int Length { get; set; }
        public uint FileSize { get; set; }
        public uint Checksum { get; set; }
        public uint Unk0 { get; set; } = 0x79707070;  // "yppp"

        public void Read(DataReader reader)
        {
            Ident = reader.ReadInt32();
            Length = reader.ReadInt32();

            if (Length != 20)
            { return; }

            FileSize = reader.ReadUInt32();
            Checksum = reader.ReadUInt32();
            Unk0 = reader.ReadUInt32();
        }

        public void Read(ref SequenceReader<byte> reader)
        {
            Ident = reader.ReadInt32BigEndian();
            Length = reader.ReadInt32BigEndian();

            if (Length != 20)
            { return; }

            FileSize = reader.ReadUInt32BigEndian();
            Checksum = reader.ReadUInt32BigEndian();
            Unk0 = reader.ReadUInt32BigEndian();
        }

        public void Write(DataWriter writer)
        {
            Length = 20;

            writer.Write(Ident);
            writer.Write(Length);
            writer.Write(FileSize);
            writer.Write(Checksum);
            writer.Write(Unk0);
        }

        public override string ToString()
        {
            return $"{Ident}: {Length}";
        }
    }



}
