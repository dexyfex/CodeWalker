using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Buffers;

namespace CodeWalker.GameFiles;

public static class PsoTypes
{
    //for parsing schema info in PSO files to generate structs for PSO parsing.
    //equivalent of MetaTypes but for PSO.

    public static Dictionary<MetaName, PsoEnumInfo> EnumDict = new Dictionary<MetaName, PsoEnumInfo>();
    public static Dictionary<MetaName, PsoStructureInfo> StructDict = new Dictionary<MetaName, PsoStructureInfo>();



    public static void Clear()
    {
        EnumDict.Clear();
        StructDict.Clear();
    }

    public static void EnsurePsoTypes(PsoFile pso)
    {

        if ((pso.SchemaSection == null) || (pso.SchemaSection.Entries == null) || (pso.SchemaSection.EntriesIdx == null))
        {
            return;
        }


        for (int i = 0; i < pso.SchemaSection.Entries.Length; i++)
        {
            var entry = pso.SchemaSection.Entries[i];

            if (entry is PsoEnumInfo enuminfo)
            {
                if (!EnumDict.ContainsKey(enuminfo.IndexInfo.NameHash))
                {
                    EnumDict.Add(enuminfo.IndexInfo.NameHash, enuminfo);
                }
                else
                {
                    PsoEnumInfo oldei = EnumDict[enuminfo.IndexInfo.NameHash];
                    if (!ComparePsoEnumInfos(oldei, enuminfo))
                    {
                        if (enuminfo.EntriesCount > oldei.EntriesCount)//assume this is newer...
                        {
                            EnumDict[enuminfo.IndexInfo.NameHash] = enuminfo;
                        }
                    }
                }
            }
            else if (entry is PsoStructureInfo structinfo)
            {
                if (!StructDict.ContainsKey(structinfo.IndexInfo.NameHash))
                {
                    StructDict.Add(structinfo.IndexInfo.NameHash, structinfo);
                }
                else
                {
                    PsoStructureInfo oldsi = StructDict[structinfo.IndexInfo.NameHash];
                    if (!ComparePsoStructureInfos(oldsi, structinfo))
                    {
                        if (structinfo.EntriesCount > oldsi.EntriesCount) //assume more entries is newer.. maybe not correct
                        {
                            StructDict[structinfo.IndexInfo.NameHash] = structinfo;
                        }
                    }
                }
            }

        }

    }

    public static bool ComparePsoEnumInfos(PsoEnumInfo a, PsoEnumInfo b)
    {
        //returns true if they are the same.

        if (a.Entries.Length != b.Entries.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Entries.Length; i++)
        {
            if ((a.Entries[i].EntryNameHash != b.Entries[i].EntryNameHash) ||
                (a.Entries[i].EntryKey != b.Entries[i].EntryKey))
            {
                return false;
            }
        }

        return true;
    }
    public static bool ComparePsoStructureInfos(PsoStructureInfo a, PsoStructureInfo b)
    {
        //returns true if they are the same.

        if (a.Entries.Length != b.Entries.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Entries.Length; i++)
        {
            if ((a.Entries[i].EntryNameHash != b.Entries[i].EntryNameHash) ||
                (a.Entries[i].DataOffset != b.Entries[i].DataOffset) ||
                (a.Entries[i].Type != b.Entries[i].Type))
            {
                return false;
            }
        }

        return true;
    }


    public static string GetTypesString()
    {
        StringBuilder sbe = new StringBuilder();
        StringBuilder sbs = new StringBuilder();

        sbe.AppendLine("//Enum infos");
        sbs.AppendLine("//Struct infos");


        foreach (var kvp in EnumDict)
        {
            var ei = kvp.Value;
            string name = GetSafeName(ei.IndexInfo.NameHash, ei.Type);
            sbe.AppendLine("public enum " + name + " //Type:" + ei.Type.ToString());
            sbe.AppendLine("{");
            foreach (var entry in ei.Entries)
            {
                string eename = GetSafeName(entry.EntryNameHash, (uint)entry.EntryKey);
                sbe.AppendFormat("   {0} = {1},", eename, entry.EntryKey);
                sbe.AppendLine();
            }
            sbe.AppendLine("}");
            sbe.AppendLine();
        }

        foreach (var kvp in StructDict)
        {
            var si = kvp.Value;
            string name = GetSafeName(si.IndexInfo.NameHash, si.Type);
            sbs.AppendLine("public struct " + name + " //" + si.StructureLength.ToString() + " bytes, Type:" + si.Type.ToString());
            sbs.AppendLine("{");
            for (int i = 0; i < si.Entries.Length; i++)
            {
                var entry = si.Entries[i];

                if ((entry.DataOffset == 0) && (entry.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO)) //referred to by array
                {
                }
                else
                {
                    string sename = GetSafeName(entry.EntryNameHash, entry.ReferenceKey);
                    string fmt = "   public {0} {1}; //{2}   {3}";

                    if (entry.Type == PsoDataType.Array)
                    {
                        if (entry.ReferenceKey >= si.Entries.Length)
                        {
                            sbs.AppendFormat(fmt, entry.Type.ToString(), sename, entry.DataOffset, entry.ToString() + "  { unexpected key! " + entry.ReferenceKey.ToString() + "}");
                            sbs.AppendLine();
                        }
                        else
                        {
                            var structentry = si.Entries[(int)entry.ReferenceKey];
                            var typename = "Array_" + PsoDataTypes.GetCSharpTypeName(structentry.Type);
                            sbs.AppendFormat(fmt, typename, sename, entry.DataOffset, entry.ToString() + "  {" + structentry.ToString() + "}");
                            sbs.AppendLine();
                        }
                    }
                    else if (entry.Type == PsoDataType.Structure)
                    {
                        var typename = GetSafeName((MetaName)entry.ReferenceKey, entry.ReferenceKey);
                        sbs.AppendFormat(fmt, typename, sename, entry.DataOffset, entry.ToString());
                        sbs.AppendLine();
                    }
                    else
                    {
                        var typename = PsoDataTypes.GetCSharpTypeName(entry.Type);
                        sbs.AppendFormat(fmt, typename, sename, entry.DataOffset, entry);
                        sbs.AppendLine();
                    }
                }
            }
            sbs.AppendLine("}");
            sbs.AppendLine();
        }


        sbe.AppendLine();
        sbe.AppendLine();
        sbe.AppendLine();
        sbe.AppendLine();
        sbe.AppendLine();
        sbe.Append(sbs.ToString());

        string result = sbe.ToString();

        return result;
    }


    public static string GetTypesInitString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var si in StructDict.Values)
        {
            AddStructureInfoString(si, sb);
        }

        sb.AppendLine();

        foreach (var ei in EnumDict.Values)
        {
            AddEnumInfoString(ei, sb);
        }

        string str = sb.ToString();
        return str;
    }
    private static void AddStructureInfoString(PsoStructureInfo si, StringBuilder sb)
    {
        var ns = GetMetaNameString(si.IndexInfo.NameHash);
        sb.AppendFormat("case " + ns + ":");
        sb.AppendLine();
        sb.AppendFormat("return new PsoStructureInfo({0}, {1}, {2}, {3},", ns, si.Type, si.Unk, si.StructureLength);
        sb.AppendLine();
        for (int i = 0; i < si.Entries.Length; i++)
        {
            var e = si.Entries[i];
            string refkey = "0";
            if (e.ReferenceKey != 0)
            {
                refkey = GetMetaNameString((MetaName)e.ReferenceKey);
            }
            sb.AppendFormat(" new PsoStructureEntryInfo({0}, PsoDataType.{1}, {2}, {3}, {4})", GetMetaNameString(e.EntryNameHash), e.Type, e.DataOffset, e.Unk_5h, refkey);
            if (i < si.Entries.Length - 1) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendFormat(");");
        sb.AppendLine();
    }
    private static void AddEnumInfoString(PsoEnumInfo ei, StringBuilder sb)
    {
        var ns = GetMetaNameString(ei.IndexInfo.NameHash);
        sb.AppendFormat("case " + ns + ":");
        sb.AppendLine();
        sb.AppendFormat("return new PsoEnumInfo({0}, {1},", ns, ei.Type);
        sb.AppendLine();
        for (int i = 0; i < ei.Entries.Length; i++)
        {
            var e = ei.Entries[i];
            sb.AppendFormat(" new PsoEnumEntryInfo({0}, {1})", GetMetaNameString(e.EntryNameHash), e.EntryKey);
            if (i < ei.Entries.Length - 1) sb.Append(',');
            sb.AppendLine();
        }
        sb.AppendFormat(");");
        sb.AppendLine();
    }
    private static string GetMetaNameString(MetaName n)
    {
        if (Enum.IsDefined(typeof(MetaName), n))
        {
            return "MetaName." + n.ToString();
        }
        else
        {
            return "(MetaName)" + n.ToString();
        }
    }




    private static string GetSafeName(MetaName namehash, uint key)
    {
        string? name = Enum.GetName(namehash);
        if (string.IsNullOrEmpty(name))
        {
            name = $"Unk_{key}";
        }
        if (!char.IsLetter(name[0]))
        {
            name = $"Unk_{name}";
        }
        return name;
    }



    public static T ConvertDataRaw<T>(byte[] data) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var h = handle.AddrOfPinnedObject();
        var r = Marshal.PtrToStructure<T>(h);
        handle.Free();
        return r;
    }
    public static T ConvertDataRawOld<T>(byte[] data, int offset) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var h = handle.AddrOfPinnedObject();
        var r = Marshal.PtrToStructure<T>(h + offset);
        handle.Free();
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertDataRaw<T>(byte[] data, int offset, out T value) where T : struct
    {
        return MemoryMarshal.TryRead<T>(data.AsSpan(offset), out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertDataRaw<T>(Span<byte> data, out T value) where T : struct
    {
        return MemoryMarshal.TryRead<T>(data, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertDataRaw<T>(byte[] data, int offset) where T : struct
    {
        MemoryMarshal.TryRead<T>(data.AsSpan(offset), out var value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertData<T>(byte[] data, int offset) where T : struct, IPsoSwapEnd<T>
    {
        TryConvertData<T>(data.AsSpan(offset), out var value);

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertData<T>(byte[] data, int offset, out T value) where T : struct, IPsoSwapEnd<T>
    {
        return TryConvertData(data.AsSpan(offset), out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertData<T>(Span<byte> data, out T value) where T : struct, IPsoSwapEnd<T>
    {
        MemoryMarshal.TryRead<T>(data, out value);
        value = value.SwapEnd();
        return true;
    }

    public static T[] ConvertDataArrayRaw<T>(byte[] data, int offset, int count, T[]? buffer = null) where T : struct
    {
        if (count == 0)
        {
            return [];
        }
        int itemsize = Marshal.SizeOf(typeof(T));
        var result = MemoryMarshal.Cast<byte, T>(data.AsSpan(offset, count * itemsize));

        var resultArr = buffer ?? new T[result.Length];

        result.CopyTo(resultArr.AsSpan());

        return resultArr;
    }


    public static T GetItem<T>(PsoFile pso, int offset) where T : struct, IPsoSwapEnd<T>
    {
        return ConvertData<T>(pso.DataSection.Data, offset);
    }
    public static T GetRootItem<T>(PsoFile pso) where T : struct, IPsoSwapEnd<T>
    {
        var i = pso.DataMapSection.RootId - 1;
        var e = pso.DataMapSection.Entries[i];
        return GetItem<T>(pso, e.Offset);
    }
    public static PsoDataMappingEntry GetRootEntry(PsoFile pso)
    {
        var i = pso.DataMapSection.RootId - 1;
        var e = pso.DataMapSection.Entries[i];
        return e;
    }

    public static Span<T> GetItemArrayRaw<T>(PsoFile pso, Array_Structure arr) where T : struct
    {
        if ((arr.Count1 > 0) && (arr.Pointer > 0))
        {
            var entry = pso.DataMapSection.Entries[arr.PointerDataIndex];
            return ConvertDataArrayRaw<T>(pso.DataSection.Data, entry.Offset, arr.Count1);
        }
        return null;
    }
    public static T[] GetItemArray<T>(PsoFile pso, in Array_Structure arr) where T : struct, IPsoSwapEnd<T>
    {
        if ((arr.Count1 > 0) && (arr.Pointer > 0))
        {
            var entry = pso.DataMapSection.Entries[arr.PointerDataIndex];
            var res = ConvertDataArrayRaw<T>(pso.DataSection.Data, entry.Offset, arr.Count1);
            if (res != null)
            {
                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = res[i].SwapEnd();
                }
            }
            return res;
        }
        return null;
    }


    public static uint[]? GetUintArrayRaw(PsoFile pso, in Array_uint arr, uint[]? buffer = null)
    {
        byte[] data = pso.DataSection.Data;
        var entryid = arr.PointerDataId;
        if ((entryid == 0) || (entryid > pso.DataMapSection.EntriesCount))
        {
            return null;
        }
        var entryoffset = arr.PointerDataOffset;
        var arrentry = pso.DataMapSection.Entries[(int)entryid - 1];
        int totoffset = arrentry.Offset + (int)entryoffset;
        var readdata = ConvertDataArrayRaw<uint>(data, totoffset, arr.Count1, buffer);
        return readdata;
    }

    public static uint[]? GetUintArray(PsoFile pso, in Array_uint arr)
    {
        var uints = GetUintArrayRaw(pso, in arr);
        if (uints == null)
            return null;

        BinaryPrimitives.ReverseEndianness(uints, uints);

        return uints;
    }

    public static MetaHash[]? GetHashArray(PsoFile pso, in Array_uint arr)
    {
        var uints = ArrayPool<uint>.Shared.Rent((int)arr.Count1);
        try
        {
            GetUintArrayRaw(pso, in arr, uints);
            if (uints is null)
                return null;

            BinaryPrimitives.ReverseEndianness(uints, uints);

            return MemoryMarshal.Cast<uint, MetaHash>(uints).ToArray();
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(uints);
        }
    }




    public static float[] GetFloatArrayRaw(PsoFile pso, in Array_float arr)
    {
        byte[] data = pso.DataSection.Data;
        var entryid = arr.PointerDataId;
        if ((entryid == 0) || (entryid > pso.DataMapSection.EntriesCount))
        {
            return null;
        }
        var entryoffset = arr.PointerDataOffset;
        var arrentry = pso.DataMapSection.Entries[(int)entryid - 1];
        int totoffset = arrentry.Offset + (int)entryoffset;
        var readdata = ConvertDataArrayRaw<float>(data, totoffset, arr.Count1);
        return readdata;
    }
    public static float[] GetFloatArray(PsoFile pso, in Array_float arr)
    {
        var floats = GetFloatArrayRaw(pso, in arr);
        if (floats == null)
            return null;

        var uints = MemoryMarshal.Cast<float, uint>(floats);
        BinaryPrimitives.ReverseEndianness(uints, uints);
        return floats;
    }





    public static ushort[] GetUShortArrayRaw(PsoFile pso, in Array_Structure arr)
    {
        byte[] data = pso.DataSection.Data;
        var entryid = arr.PointerDataId;
        if (entryid == 0 || entryid > pso.DataMapSection.EntriesCount)
        {
            return null;
        }
        var entryoffset = arr.PointerDataOffset;
        var arrentry = pso.DataMapSection.Entries[(int)entryid - 1];
        int totoffset = arrentry.Offset + (int)entryoffset;
        ushort[] readdata = ConvertDataArrayRaw<ushort>(data, totoffset, arr.Count1);
        return readdata;
    }

    public static ushort[] GetUShortArray(PsoFile pso, in Array_Structure arr)
    {
        var ushorts = GetUShortArrayRaw(pso, in arr);
        if (ushorts == null)
            return null;

        BinaryPrimitives.ReverseEndianness(ushorts, ushorts);
        return ushorts;
    }






    public static T[]? GetObjectArray<T, U>(PsoFile pso, in Array_Structure arr) where U : struct, IPsoSwapEnd<U> where T : PsoClass<U>, new()
    {
        U[] items = GetItemArray<U>(pso, in arr);
        if (items == null)
            return null;

        if (items.Length == 0)
            return null;

        T[] result = new T[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            T newitem = new T();
            newitem.Init(pso, in items[i]);
            result[i] = newitem;
        }
        return result;
    }


    public static byte[]? GetByteArray(PsoFile pso, PsoStructureEntryInfo entry, int offset)
    {
        var aCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
        var aBlockId = (int)entry.ReferenceKey & 0x0000FFFF;
        var block = pso.GetBlock(aBlockId);
        if (block == null)
            return null;

        //block.Offset

        return null;
    }






    public static PsoPOINTER[]? GetPointerArray(PsoFile pso, in Array_StructurePointer array, PsoPOINTER[]? buffer = null)
    {
        uint count = array.Count1;
        if (count == 0)
            return null;

        uint ptrindex = array.PointerDataIndex;
        uint ptroffset = array.PointerDataOffset;
            
        if (ptrindex >= pso.DataMapSection.EntriesCount)
        {
            return null;
        }

        var ptrblock = pso.DataMapSection.Entries[ptrindex];
        if (ptrblock.NameHash != (MetaName)MetaTypeName.PsoPOINTER)
        {
            return null;
        }

        var offset = ptrblock.Offset;
        int boffset = (int)(offset + ptroffset);

        var ptrs = ConvertDataArrayRaw<PsoPOINTER>(pso.DataSection.Data, boffset, (int)count);
        if (ptrs != null)
        {
            for (int i = 0; i < ptrs.Length; i++)
            {
                ptrs[i] = ptrs[i].SwapEnd();
            }
        }

        return ptrs;
    }


    public static T[]? ConvertDataArray<T>(PsoFile pso, in Array_StructurePointer array) where T : struct, IPsoSwapEnd<T>
    {
        uint count = array.Count1;
        if (count == 0)
        {
            Console.WriteLine($"count is 0");
            return null;
        }
            
        var ptrs = GetPointerArray(pso, in array);
        if (ptrs == null)
        {
            Console.WriteLine($"ptrs is null");
            return null;
        }
        if (ptrs.Length < count || ptrs.Length == 0)
        {
            Console.WriteLine($"ptrs.Length smaller than count or ptrs.Length == 0; ptrs.Length = {ptrs.Length}; count = {count}");
            return null;
        }

        T[] items = new T[count];

        for (int i = 0; i < count; i++)
        {
            var sptr = ptrs[i];
            int blocki = sptr.BlockID - 1;
            int offset = (int)sptr.ItemOffset;// * 16;//block data size...
            if (blocki >= pso.DataMapSection.EntriesCount)
            {
                continue;
            }
            var block = pso.DataMapSection.Entries[blocki];

            if (offset < 0 || offset >= block.Length)
            {
                continue;
            }

            int boffset = block.Offset + offset;

            TryConvertData<T>(pso.DataSection.Data, boffset, out items[i]);
        }

        return items;
    }



    public static string GetString(PsoFile pso, in CharPointer ptr)
    {
        if (ptr.Count1 == 0)
            return string.Empty;

        var blocki = (int)ptr.PointerDataId;// (ptr.Pointer & 0xFFF) - 1;
        var offset = (int)ptr.PointerDataOffset;// (ptr.Pointer >> 12) & 0xFFFFF;

        if (!pso.TryGetBlock(blocki, out var block))
        {
            return string.Empty;
        }

        //if (block.NameHash != (MetaName)1)
        //{ }

        var length = ptr.Count1;
        var lastbyte = offset + length;
        if (lastbyte >= block.Length)
        {
            return string.Empty;
        }

        var data = pso.DataSection?.Data;
        if (data == null)
        {
            return string.Empty;
        }

        var doffset = block.Offset + offset;

        string s = Encoding.ASCII.GetString(data, doffset, length);

        return s;
    }
    public static string? GetString(PsoFile pso, in DataBlockPointer ptr)
    {
        var blocki = (int)ptr.PointerDataId;// (ptr.Pointer & 0xFFF) - 1;
        var offset = (int)ptr.PointerDataOffset;// (ptr.Pointer >> 12) & 0xFFFFF;

        if (!pso.TryGetBlock(blocki, out var block))
        {
            return null;
        }

        //if (block.NameHash != (MetaName)1)
        //{ }

        //var length = ptr.Count1;
        //var lastbyte = offset + length;
        //if (lastbyte >= block.Length)
        //{ return null; }

        var data = pso.DataSection?.Data;
        if (data == null)
        {
            return null;
        }

        //var doffset = block.Offset + offset;

        //string s = Encoding.ASCII.GetString(data, doffset, length);

        var o = block.Offset + offset;

        char c = (char)data[o];

        var strData = data.AsSpan(o);
        var index = strData.IndexOf((byte)'\0');

        if (index == -1)
        {
            return Encoding.ASCII.GetString(strData);
        }

        return Encoding.ASCII.GetString(strData.Slice(0, index));
    }
}


public interface IPsoSwapEnd<T>
{
    T SwapEnd();
}

public abstract class PsoClass<T> where T : struct, IPsoSwapEnd<T>
{
    public abstract void Init(PsoFile pso, in T v);
}

public readonly struct PsoChar64
{
    public readonly byte b00, b01, b02, b03, b04, b05, b06, b07, b08, b09,
                b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
                b20, b21, b22, b23, b24, b25, b26, b27, b28, b29,
                b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
                b40, b41, b42, b43, b44, b45, b46, b47, b48, b49,
                b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
                b60, b61, b62, b63;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> GetBytes()
    {
        return MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in this, 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> GetBytesNullTerminated()
    {
        var bytes = GetBytes();

        var index = bytes.IndexOf((byte)0);

        if (index == -1)
        {
            return bytes;
        }

        return bytes.Slice(0, index);
    }

    public override string ToString()
    {
        var byteArr = GetBytesNullTerminated();

        return Encoding.ASCII.GetString(byteArr);
    }
}
public readonly struct PsoChar32
{
    public readonly byte b00, b01, b02, b03, b04, b05, b06, b07, b08, b09,
                b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
                b20, b21, b22, b23, b24, b25, b26, b27, b28, b29,
                b30, b31;

    public PsoChar32(string s)
    {
        s = s.PadRight(32, '\0');
        b00 = (byte)s[0];
        b01 = (byte)s[1];
        b02 = (byte)s[2];
        b03 = (byte)s[3];
        b04 = (byte)s[4];
        b05 = (byte)s[5];
        b06 = (byte)s[6];
        b07 = (byte)s[7];
        b08 = (byte)s[8];
        b09 = (byte)s[9];
        b10 = (byte)s[10];
        b11 = (byte)s[11];
        b12 = (byte)s[12];
        b13 = (byte)s[13];
        b14 = (byte)s[14];
        b15 = (byte)s[15];
        b16 = (byte)s[16];
        b17 = (byte)s[17];
        b18 = (byte)s[18];
        b19 = (byte)s[19];
        b20 = (byte)s[20];
        b21 = (byte)s[21];
        b22 = (byte)s[22];
        b23 = (byte)s[23];
        b24 = (byte)s[24];
        b25 = (byte)s[25];
        b26 = (byte)s[26];
        b27 = (byte)s[27];
        b28 = (byte)s[28];
        b29 = (byte)s[29];
        b30 = (byte)s[30];
        b31 = (byte)s[31];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> GetBytes()
    {
        return MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in this, 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> GetBytesNullTerminated()
    {
        var bytes = GetBytes();

        var index = bytes.IndexOf((byte)0);

        if (index == -1)
        {
            return bytes;
        }

        return bytes.Slice(0, index);
    }

    public override string ToString()
    {
        var byteArr = GetBytesNullTerminated();

        return Encoding.ASCII.GetString(byteArr);
    }
}


[TC(typeof(EXP))]
public readonly struct PsoPOINTER : IPsoSwapEnd<PsoPOINTER> //8 bytes - pointer to data item
{
    public ulong Pointer { get; init; }

    public ushort BlockID => (ushort)(Pointer & 0xFFF); //1-based ID
    public uint ItemOffset => (uint)((Pointer>>12) & 0xFFFFF); //byte offset
    public uint Unk0 => (uint)((Pointer>>32) & 0xFFFFFFFF);


    public PsoPOINTER(int blockID, int itemOffset)
    {
        Pointer = (((uint)itemOffset << 12) & 0xFFFFF000) + ((uint)blockID & 0xFFF);
    }


    public override string ToString() => $"{BlockID}, {ItemOffset}";

    public PsoPOINTER SwapEnd()
    {
        return this with
        {
            Pointer = MetaTypes.SwapBytes(Pointer),
        };
    }
}

//Struct infos
[TC(typeof(EXP))]
public readonly struct CPackFileMetaData : IPsoSwapEnd<CPackFileMetaData> //96 bytes, Type:0
{
    public readonly Array_Structure _MapDataGroups; //0   MapDataGroups: Array: 0  {256: Structure: 0: 3260758307}
    public Array_Structure MapDataGroups { get => _MapDataGroups; init => _MapDataGroups = value; }

    public readonly Array_Structure _HDTxdBindingArray; //16   HDTxdBindingArray: Array: 16: 2  {256: Structure: 0: CHDTxdAssetBinding}
    public Array_Structure HDTxdBindingArray { get => _HDTxdBindingArray; init => _HDTxdBindingArray = value; }

    public readonly Array_Structure _imapDependencies; //32   imapDependencies: Array: 32: 4  {256: Structure: 0: 3501026914}
    public Array_Structure imapDependencies { get => _imapDependencies; init => _imapDependencies = value; }

    public readonly Array_Structure _imapDependencies_2; //48   imapDependencies_2: Array: 48: 6  {256: Structure: 0: 3240050401}
    public Array_Structure imapDependencies_2 { get => _imapDependencies_2; init => _imapDependencies_2 = value; }

    public readonly Array_Structure _itypDependencies_2; //64   itypDependencies_2: Array: 64: 8  {256: Structure: 0: 1515605584}
    public Array_Structure itypDependencies_2 { get => _itypDependencies_2; init => _itypDependencies_2 = value; }

    public readonly Array_Structure _Interiors; //80   Interiors: Array: 80: 10  {256: Structure: 0: 741495440}
    public Array_Structure Interiors { get => _Interiors; init => _Interiors = value; }

    public CPackFileMetaData SwapEnd()
    {
        return this with
        {
            MapDataGroups = _MapDataGroups.SwapEnd(),
            HDTxdBindingArray = _HDTxdBindingArray.SwapEnd(),
            imapDependencies = _imapDependencies.SwapEnd(),
            imapDependencies_2 = _imapDependencies_2.SwapEnd(),
            itypDependencies_2 = _itypDependencies_2.SwapEnd(),
            Interiors = _Interiors.SwapEnd(),
        };
    }
}

[TC(typeof(EXP))] public struct CMapDataGroup : IPsoSwapEnd<CMapDataGroup> //56 bytes, Type:0
{
    public MetaHash Name { get; set; } //0   Name: INT_0Bh: 0
    public uint Unused0 { get; set; } //4
    public Array_uint Bounds { get; set; } //8   Bounds: Array: 8: 1  {256: INT_0Bh: 0}
    public ushort Flags { get; set; } //24   Flags: SHORT_0Fh: 24: 2097155
    public ushort Unused1 { get; set; }//26
    public uint Unused2 { get; set; }//28
    public Array_uint WeatherTypes { get; set; } //32   WeatherTypes: Array: 32: 5  {256: INT_0Bh: 0}
    public uint HoursOnOff { get; set; } //48   HoursOnOff//4190815249: INT_06h: 48
    public uint Unused3 { get; set; }//52

    public override readonly string ToString()
    {
        return $"{Name}: ybn:{Bounds.Count1}, wt:{WeatherTypes.Count1}, flags:{Flags}, hours:{HoursOnOff}";
    }

    public CMapDataGroup SwapEnd()
    {
        return this with
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash)),
            Bounds = Bounds.SwapEnd(),
            WeatherTypes = WeatherTypes.SwapEnd(),
            HoursOnOff = MetaTypes.SwapBytes(HoursOnOff),
            Flags = MetaTypes.SwapBytes(Flags),
        };
    }
}

[TC(typeof(EXP))]
public readonly struct CHDTxdAssetBinding : IPsoSwapEnd<CHDTxdAssetBinding> //132 bytes, Type:0
{
    public byte assetType { get; init; } //0   assetType: BYTE_ENUM_VALUE: 0: 3387532954
    public byte Unused01 { get; init; }//1
    public ushort Unused02 { get; init; }//2

    public readonly PsoChar64 _targetAsset;
    public PsoChar64 targetAsset { get => _targetAsset; init => _targetAsset = value; } //4   targetAsset: INT_0Bh: 4: 4194304

    public readonly PsoChar64 _HDTxd;
    public PsoChar64 HDTxd { get => _HDTxd; init => _HDTxd = value; } //68   HDTxd: INT_0Bh: 68: 4194304

    public override string ToString()
    {
        return $"{assetType}: {_targetAsset}: {_HDTxd}";
    }
    public readonly CHDTxdAssetBinding SwapEnd()
    {
        return this;
        //targetAsset.Hash = MetaTypes.SwapBytes(targetAsset.Hash);
        //HDTxd.Hash = MetaTypes.SwapBytes(HDTxd.Hash);
    }
}

[TC(typeof(EXP))] public struct CImapDependency : IPsoSwapEnd<CImapDependency> //12 bytes, Type:0  //  CImapDependency//3501026914
{
    public MetaHash imapName { get; set; } //0   imapName: INT_0Bh: 0
    public MetaHash itypName { get; set; } //4   itypName//2890158180: INT_0Bh: 4
    public MetaHash packFileName { get; set; } //8   packFileName//4216494073: INT_0Bh: 8

    public override string ToString()
    {
        return $"{imapName}, {itypName}, {packFileName}";
    }
    public CImapDependency SwapEnd()
    {
        return this with
        {
            imapName = new MetaHash(MetaTypes.SwapBytes(imapName.Hash)),
            itypName = new MetaHash(MetaTypes.SwapBytes(itypName.Hash)),
            packFileName = new MetaHash(MetaTypes.SwapBytes(packFileName.Hash)),
        };
    }
}

[TC(typeof(EXP))]
public struct CImapDependencies : IPsoSwapEnd<CImapDependencies> //24 bytes, Type:0  // CImapDependencies//3240050401 imapDependencies_2
{
    public MetaHash imapName { get; set; } //0   imapName: INT_0Bh: 0  //name hash
    public ushort manifestFlags { get; set; } //4   manifestFlags//1683136603: SHORT_0Fh: 4: 2097153
    public ushort Unused0 { get; set; } //6
    public Array_uint itypDepArray { get; set; } //8   itypDepArray//2410949350: Array: 8: 3  {256: INT_0Bh: 0} //children...


    public override string ToString()
    {
        return $"{imapName}: {manifestFlags}: {itypDepArray}";
    }
    public CImapDependencies SwapEnd()
    {
        return this with
        {
            imapName = new MetaHash(MetaTypes.SwapBytes(imapName.Hash)),
            manifestFlags = MetaTypes.SwapBytes(manifestFlags),
            itypDepArray = itypDepArray.SwapEnd(),
        };
    }
}

[TC(typeof(EXP))]
public struct CItypDependencies : IPsoSwapEnd<CItypDependencies> //24 bytes, Type:0 // CItypDependencies//1515605584  itypDependencies_2
{
    public MetaHash itypName { get; set; } //0   itypName//2890158180: INT_0Bh: 0
    public ushort manifestFlags { get; set; } //4   manifestFlags//1683136603: SHORT_0Fh: 4: 2097153
    public ushort Unused0 { get; set; } //6
    public Array_uint itypDepArray { get; set; } //8   itypDepArray//2410949350: Array: 8: 3  {256: INT_0Bh: 0}

    public override string ToString()
    {
        return $"{itypName}: {manifestFlags}: {itypDepArray}";
    }
    public CItypDependencies SwapEnd()
    {
        return this with
        {
            itypName = new MetaHash(MetaTypes.SwapBytes(itypName.Hash)),
            manifestFlags = MetaTypes.SwapBytes(manifestFlags),
            itypDepArray = itypDepArray.SwapEnd(),
        };
    }
}

[TC(typeof(EXP))]
public struct CInteriorBoundsFiles : IPsoSwapEnd<CInteriorBoundsFiles> //24 bytes, Type:0   // Interiors
{
    public MetaHash Name { get; init; } //0   Name: INT_0Bh: 0
    public uint Unused0 { get; init; } //4
    public readonly Array_uint _Bounds; //8   Bounds: Array: 8: 1  {256: INT_0Bh: 0}
    public Array_uint Bounds { get => _Bounds; init => _Bounds = value; }

    public override string ToString() => JenkIndex.GetString(Name);
    public CInteriorBoundsFiles SwapEnd()
    {
        return this with
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash)),
            Bounds = Bounds.SwapEnd(),
        };
    }
}






[TC(typeof(EXP))]
public readonly struct CScenarioPointManifest : IPsoSwapEnd<CScenarioPointManifest> //56 bytes, Type:0
{
    public readonly int _VersionNumber;
    public int VersionNumber { get => _VersionNumber; init => _VersionNumber = value; } //0   VersionNumber: INT_05h: 0

    public readonly uint _Unused0;
    public uint Unused0 { get => _Unused0; init => _Unused0 = value; } //4

    public readonly Array_StructurePointer _RegionDefs;
    public Array_StructurePointer RegionDefs { get => _RegionDefs; init => _RegionDefs = value; } //8   RegionDefs: Array: 8: 1  {ARRAYINFO: Structure: 0}

    public readonly Array_StructurePointer _Groups;
    public Array_StructurePointer Groups { get => _Groups; init => _Groups = value; } //24   Groups: Array: 24: 3  {ARRAYINFO: Structure: 0}

    public readonly Array_uint _InteriorNames;
    public Array_uint InteriorNames { get => _InteriorNames; init => _InteriorNames = value; } //40   InteriorNames: Array: 40: 5  {ARRAYINFO: INT_0Bh: 0}

    public override readonly string ToString() => VersionNumber.ToString();
    public CScenarioPointManifest SwapEnd()
    {
        return this with
        {
            VersionNumber = MetaTypes.SwapBytes(VersionNumber),
            RegionDefs = RegionDefs.SwapEnd(),
            Groups = Groups.SwapEnd(),
            InteriorNames = InteriorNames.SwapEnd(),
        };
    }
}

[TC(typeof(EXP))]
public readonly struct CScenarioPointRegionDef : IPsoSwapEnd<CScenarioPointRegionDef> //64 bytes, Type:0
{
    public MetaHash Name { get; init; } //0   Name: INT_0Bh: 0
    public uint Unused0 { get; init; } //4
    public uint Unused1 { get; init; } //8
    public uint Unused2 { get; init; } //12
    public rage__spdAABB AABB { get; init; } //16   AABB: Structure: 16: rage__spdAABB
    public uint Unused3 { get; init; } //48
    public uint Unused4 { get; init; } //52
    public uint Unused5 { get; init; } //56
    public uint Unused6 { get; init; } //60

    public override readonly string ToString() => $"{Name}, {AABB}";
    public CScenarioPointRegionDef SwapEnd()
    {
        return this with
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash)),
            AABB = AABB.SwapEnd(),
        };
    }
}

[TC(typeof(EXP))] public struct CScenarioPointGroup : IPsoSwapEnd<CScenarioPointGroup> //8 bytes, Type:0
{
    public MetaHash Name { get; set; } //0   Name: INT_0Bh: 0
    public byte EnabledByDefault { get; set; } //4   EnabledByDefault: BYTE_00h: 4
    public byte Unused0 { get; set; } //5
    public ushort Unused1 { get; set; } //6

    public override string ToString() => Name.ToString();
    public CScenarioPointGroup SwapEnd()
    {
        return this with
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash)),
        };
    }
}
