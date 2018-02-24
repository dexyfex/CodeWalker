using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;


namespace CodeWalker.GameFiles
{
    public static class PsoTypes
    {
        //for parsing schema info in PSO files to generate structs for PSO parsing.
        //equivalent of MetaTypes but for PSO.

        public static Dictionary<MetaName, PsoEnumInfo> EnumDict = new Dictionary<MetaName, PsoEnumInfo>();
        public static Dictionary<MetaName, PsoStructureInfo> StructDict = new Dictionary<MetaName, PsoStructureInfo>();



        public static void Clear()
        {
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
                var enuminfo = entry as PsoEnumInfo;
                var structinfo = entry as PsoStructureInfo;

                if (enuminfo != null)
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
                        }
                    }
                }
                else if (structinfo != null)
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

                    if ((entry.DataOffset == 0) && (entry.EntryNameHash == MetaName.ARRAYINFO)) //referred to by array
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


        private static string GetSafeName(MetaName namehash, uint key)
        {
            string name = namehash.ToString();
            if (string.IsNullOrEmpty(name))
            {
                name = "Unk_" + key;
            }
            if (!char.IsLetter(name[0]))
            {
                name = "Unk_" + name;
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
        public static T ConvertDataRaw<T>(byte[] data, int offset) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            var r = Marshal.PtrToStructure<T>(h + offset);
            handle.Free();
            return r;
        }
        public static T ConvertData<T>(byte[] data, int offset) where T : struct, IPsoSwapEnd
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            var r = Marshal.PtrToStructure<T>(h + offset);
            handle.Free();
            r.SwapEnd();
            return r;
        }
        public static T[] ConvertDataArrayRaw<T>(byte[] data, int offset, int count) where T : struct
        {
            T[] items = new T[count];
            int itemsize = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < count; i++)
            {
                int off = offset + i * itemsize;
                items[i] = ConvertDataRaw<T>(data, off);
            }
            return items;
        }


        public static T GetItem<T>(PsoFile pso, int offset) where T : struct, IPsoSwapEnd
        {
            return ConvertData<T>(pso.DataSection.Data, offset);
        }
        public static T GetRootItem<T>(PsoFile pso) where T : struct, IPsoSwapEnd
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

        public static T[] GetItemArrayRaw<T>(PsoFile pso, Array_Structure arr) where T : struct
        {
            if ((arr.Count1 > 0) && (arr.Pointer > 0))
            {
                var entry = pso.DataMapSection.Entries[(int)arr.PointerDataIndex];
                return ConvertDataArrayRaw<T>(pso.DataSection.Data, entry.Offset, arr.Count1);
            }
            return null;
        }
        public static T[] GetItemArray<T>(PsoFile pso, Array_Structure arr) where T : struct, IPsoSwapEnd
        {
            if ((arr.Count1 > 0) && (arr.Pointer > 0))
            {
                var entry = pso.DataMapSection.Entries[(int)arr.PointerDataIndex];
                var res = ConvertDataArrayRaw<T>(pso.DataSection.Data, entry.Offset, arr.Count1);
                if (res != null)
                {
                    for (int i = 0; i < res.Length; i++)
                    {
                        res[i].SwapEnd();
                    }
                }
                return res;
            }
            return null;
        }


        public static uint[] GetUintArrayRaw(PsoFile pso, Array_uint arr)
        {
            byte[] data = pso.DataSection.Data;
            var entryid = arr.Pointer & 0xFFF;
            if ((entryid == 0) || (entryid > pso.DataMapSection.EntriesCount))
            {
                return null;
            }
            var entryoffset = (arr.Pointer & 0xFFFFFF) >> 12;
            var arrentry = pso.DataMapSection.Entries[(int)entryid - 1];
            int totoffset = arrentry.Offset + (int)entryoffset;
            uint[] readdata = ConvertDataArrayRaw<uint>(data, totoffset, arr.Count1);
            return readdata;
        }
        public static uint[] GetUintArray(PsoFile pso, Array_uint arr)
        {
            uint[] uints = GetUintArrayRaw(pso, arr);
            if (uints == null) return null;
            for (int i = 0; i < uints.Length; i++)
            {
                uints[i] = MetaTypes.SwapBytes(uints[i]);
            }
            return uints;
        }

        public static MetaHash[] GetHashArray(PsoFile pso, Array_uint arr)
        {
            uint[] uints = GetUintArrayRaw(pso, arr);
            if (uints == null) return null;
            MetaHash[] hashes = new MetaHash[uints.Length];
            for (int n = 0; n < uints.Length; n++)
            {
                hashes[n].Hash = MetaTypes.SwapBytes(uints[n]);
            }
            return hashes;
        }




        public static float[] GetFloatArrayRaw(PsoFile pso, Array_float arr)
        {
            byte[] data = pso.DataSection.Data;
            var entryid = arr.Pointer & 0xFFF;
            if ((entryid == 0) || (entryid > pso.DataMapSection.EntriesCount))
            {
                return null;
            }
            var entryoffset = (arr.Pointer & 0xFFFFFF) >> 12;
            var arrentry = pso.DataMapSection.Entries[(int)entryid - 1];
            int totoffset = arrentry.Offset + (int)entryoffset;
            float[] readdata = ConvertDataArrayRaw<float>(data, totoffset, arr.Count1);
            return readdata;
        }
        public static float[] GetFloatArray(PsoFile pso, Array_float arr)
        {
            float[] floats = GetFloatArrayRaw(pso, arr);
            if (floats == null) return null;
            for (int i = 0; i < floats.Length; i++)
            {
                floats[i] = MetaTypes.SwapBytes(floats[i]);
            }
            return floats;
        }





        public static ushort[] GetUShortArrayRaw(PsoFile pso, Array_Structure arr)
        {
            byte[] data = pso.DataSection.Data;
            var entryid = arr.Pointer & 0xFFF;
            if ((entryid == 0) || (entryid > pso.DataMapSection.EntriesCount))
            {
                return null;
            }
            var entryoffset = (arr.Pointer & 0xFFFFFF) >> 12;
            var arrentry = pso.DataMapSection.Entries[(int)entryid - 1];
            int totoffset = arrentry.Offset + (int)entryoffset;
            ushort[] readdata = ConvertDataArrayRaw<ushort>(data, totoffset, arr.Count1);
            return readdata;
        }
        public static ushort[] GetUShortArray(PsoFile pso, Array_Structure arr)
        {
            ushort[] ushorts = GetUShortArrayRaw(pso, arr);
            if (ushorts == null) return null;
            for (int i = 0; i < ushorts.Length; i++)
            {
                ushorts[i] = MetaTypes.SwapBytes(ushorts[i]);
            }
            return ushorts;
        }






        public static T[] GetObjectArray<T, U>(PsoFile pso, Array_Structure arr) where U : struct, IPsoSwapEnd where T : PsoClass<U>, new()
        {
            U[] items = GetItemArray<U>(pso, arr);
            if (items == null) return null;
            if (items.Length == 0) return null;
            T[] result = new T[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                T newitem = new T();
                newitem.Init(pso, ref items[i]);
                result[i] = newitem;
            }
            return result;
        }


        public static byte[] GetByteArray(PsoFile pso, PsoStructureEntryInfo entry, int offset)
        {
            var aCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
            var aBlockId = (int)entry.ReferenceKey & 0x0000FFFF;
            var block = pso.GetBlock(aBlockId);
            if (block == null) return null;

            //block.Offset

            return null;
        }






        public static PsoPOINTER[] GetPointerArray(PsoFile pso, Array_StructurePointer array)
        {
            uint count = array.Count1;
            if (count == 0) return null;

            int ptrsize = Marshal.SizeOf(typeof(MetaPOINTER));
            int itemsleft = (int)count; //large arrays get split into chunks...
            uint ptr = array.Pointer;
            int ptrindex = (int)(ptr & 0xFFF) - 1;
            int ptroffset = (int)((ptr >> 12) & 0xFFFFF);
            var ptrblock = (ptrindex < pso.DataMapSection.EntriesCount) ? pso.DataMapSection.Entries[ptrindex] : null;
            if ((ptrblock == null) || (ptrblock.NameHash != MetaName.PsoPOINTER))
            { return null; }

            var offset = ptrblock.Offset;
            int boffset = offset + ptroffset;

            var ptrs = ConvertDataArrayRaw<PsoPOINTER>(pso.DataSection.Data, boffset, (int)count);
            if (ptrs != null)
            {
                for (int i = 0; i < ptrs.Length; i++)
                {
                    ptrs[i].SwapEnd();
                }
            }

            return ptrs;
        }


        public static T[] ConvertDataArray<T>(PsoFile pso, Array_StructurePointer array) where T : struct, IPsoSwapEnd
        {
            uint count = array.Count1;
            if (count == 0) return null;
            PsoPOINTER[] ptrs = GetPointerArray(pso, array);
            if (ptrs == null) return null;
            if (ptrs.Length < count)
            { return null; }

            T[] items = new T[count];
            int itemsize = Marshal.SizeOf(typeof(T));

            for (int i = 0; i < count; i++)
            {
                var sptr = ptrs[i];
                int blocki = sptr.BlockID - 1;
                int offset = (int)sptr.ItemOffset;// * 16;//block data size...
                if (blocki >= pso.DataMapSection.EntriesCount)
                { continue; }
                var block = pso.DataMapSection.Entries[blocki];

                if ((offset < 0) || (offset >= block.Length))
                { continue; }

                int boffset = block.Offset + offset;

                items[i] = ConvertData<T>(pso.DataSection.Data, boffset);
            }

            return items;
        }



        public static string GetString(PsoFile pso, CharPointer ptr)
        {
            if (ptr.Count1 == 0) return null;

            var blocki = (int)ptr.PointerDataId;// (ptr.Pointer & 0xFFF) - 1;
            var offset = (int)ptr.PointerDataOffset;// (ptr.Pointer >> 12) & 0xFFFFF;

            var block = pso.GetBlock(blocki);
            if (block == null)
            { return null; }

            var length = ptr.Count1;
            var lastbyte = offset + length;
            if (lastbyte >= block.Length)
            { return null; }

            var data = pso.DataSection?.Data;
            if (data == null)
            { return null; }

            var doffset = block.Offset + offset;

            string s = Encoding.ASCII.GetString(data, doffset, length);

            //if (meta.Strings == null) return null;
            //if (offset < 0) return null;
            //if (offset >= meta.Strings.Length) return null;
            //string s = meta.Strings[offset];

            return s;
        }
        public static string GetString(PsoFile pso, DataBlockPointer ptr)
        {
            var blocki = (int)ptr.PointerDataId;// (ptr.Pointer & 0xFFF) - 1;
            var offset = (int)ptr.PointerDataOffset;// (ptr.Pointer >> 12) & 0xFFFFF;

            var block = pso.GetBlock(blocki);
            if (block == null)
            { return null; }

            //var length = ptr.Count1;
            //var lastbyte = offset + length;
            //if (lastbyte >= block.Length)
            //{ return null; }

            var data = pso.DataSection?.Data;
            if (data == null)
            { return null; }

            //var doffset = block.Offset + offset;

            //string s = Encoding.ASCII.GetString(data, doffset, length);

            StringBuilder sb = new StringBuilder();
            var o = block.Offset + offset;
            char c = (char)data[o];
            while (c != 0)
            {
                sb.Append(c);
                o++;
                c = (char)data[o];
            }
            var s = sb.ToString();

            return s;
        }


    }


    public interface IPsoSwapEnd
    {
        void SwapEnd();
    }

    public abstract class PsoClass<T> where T : struct, IPsoSwapEnd
    {
        public abstract void Init(PsoFile pso, ref T v);
    }


    public struct PsoChar64
    {
        public byte b00, b01, b02, b03, b04, b05, b06, b07, b08, b09,
                    b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
                    b20, b21, b22, b23, b24, b25, b26, b27, b28, b29,
                    b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
                    b40, b41, b42, b43, b44, b45, b46, b47, b48, b49,
                    b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
                    b60, b61, b62, b63;

        public override string ToString()
        {
            byte[] bytes = new byte[]
            {
                b00, b01, b02, b03, b04, b05, b06, b07, b08, b09,
                b10, b11, b12, b13, b14, b15, b16, b17, b18, b19,
                b20, b21, b22, b23, b24, b25, b26, b27, b28, b29,
                b30, b31, b32, b33, b34, b35, b36, b37, b38, b39,
                b40, b41, b42, b43, b44, b45, b46, b47, b48, b49,
                b50, b51, b52, b53, b54, b55, b56, b57, b58, b59,
                b60, b61, b62, b63
            };
            return Encoding.ASCII.GetString(bytes).Replace("\0", string.Empty);
        }
    }




    [TC(typeof(EXP))] public struct PsoPOINTER : IPsoSwapEnd //8 bytes - pointer to data item
    {
        public uint Pointer { get; set; }
        public uint Unk2 { get; set; }

        public ushort BlockID { get { return (ushort)(Pointer & 0xFFF); } } //1-based ID
        public uint ItemOffset { get { return ((Pointer>>12) & 0xFFFFF); } } //byte offset


        public override string ToString()
        {
            return BlockID.ToString() + ", " + ItemOffset.ToString() + ", " + Unk2.ToString();
        }

        public void SwapEnd()
        {
            Pointer = MetaTypes.SwapBytes(Pointer);
            Unk2 = MetaTypes.SwapBytes(Unk2);
        }
    }









    //Struct infos
    [TC(typeof(EXP))] public struct CPackFileMetaData : IPsoSwapEnd //96 bytes, Type:0
    {
        public Array_Structure MapDataGroups; //0   MapDataGroups: Array: 0  {256: Structure: 0: 3260758307}
        public Array_Structure HDTxdBindingArray; //16   HDTxdBindingArray: Array: 16: 2  {256: Structure: 0: CHDTxdAssetBinding}
        public Array_Structure imapDependencies; //32   imapDependencies: Array: 32: 4  {256: Structure: 0: 3501026914}
        public Array_Structure imapDependencies_2; //48   imapDependencies_2: Array: 48: 6  {256: Structure: 0: 3240050401}
        public Array_Structure itypDependencies_2; //64   itypDependencies_2: Array: 64: 8  {256: Structure: 0: 1515605584}
        public Array_Structure Interiors; //80   Interiors: Array: 80: 10  {256: Structure: 0: 741495440}

        public void SwapEnd()
        {
            MapDataGroups.SwapEnd();
            HDTxdBindingArray.SwapEnd();
            imapDependencies.SwapEnd();
            imapDependencies_2.SwapEnd();
            itypDependencies_2.SwapEnd();
            Interiors.SwapEnd();
        }
    }

    [TC(typeof(EXP))] public struct CMapDataGroup : IPsoSwapEnd //56 bytes, Type:0
    {
        public MetaHash Name { get; set; } //0   Name: INT_0Bh: 0
        public uint Unused0 { get; set; } //4
        public Array_uint Bounds { get; set; } //8   Bounds//3298223272: Array: 8: 1  {256: INT_0Bh: 0}
        public ushort Flags { get; set; } //24   Flags: SHORT_0Fh: 24: 2097155
        public ushort Unused1 { get; set; }//26
        public uint Unused2 { get; set; }//28
        public Array_uint WeatherTypes { get; set; } //32   WeatherTypes: Array: 32: 5  {256: INT_0Bh: 0}
        public uint HoursOnOff { get; set; } //48   HoursOnOff//4190815249: INT_06h: 48
        public uint Unused3 { get; set; }//52

        public override string ToString()
        {
            return Name.ToString() + ": ybn:" + Bounds.Count1.ToString() + ", wt:" + WeatherTypes.Count1.ToString() + ", flags:" + Flags.ToString() + ", hours:" + HoursOnOff.ToString();
        }

        public void SwapEnd()
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash));
            var b = Bounds; b.SwapEnd(); Bounds = b;
            var w = WeatherTypes; w.SwapEnd(); WeatherTypes = w;
            HoursOnOff = MetaTypes.SwapBytes(HoursOnOff);
            Flags = MetaTypes.SwapBytes(Flags);
        }
    }

    [TC(typeof(EXP))] public struct CHDTxdAssetBinding : IPsoSwapEnd //132 bytes, Type:0
    {
        public byte assetType { get; set; } //0   assetType: BYTE_ENUM_VALUE: 0: 3387532954
        public byte Unused01 { get; set; }//1
        public ushort Unused02 { get; set; }//2
        public PsoChar64 targetAsset { get; set; } //4   targetAsset: INT_0Bh: 4: 4194304
        public PsoChar64 HDTxd { get; set; } //68   HDTxd: INT_0Bh: 68: 4194304

        public override string ToString()
        {
            return assetType.ToString() + ": " + targetAsset.ToString() + ": " + HDTxd.ToString();
        }
        public void SwapEnd()
        {
            //targetAsset.Hash = MetaTypes.SwapBytes(targetAsset.Hash);
            //HDTxd.Hash = MetaTypes.SwapBytes(HDTxd.Hash);
        }
    }

    [TC(typeof(EXP))] public struct CImapDependency : IPsoSwapEnd //12 bytes, Type:0  //  CImapDependency//3501026914
    {
        public MetaHash imapName { get; set; } //0   imapName: INT_0Bh: 0
        public MetaHash itypName { get; set; } //4   itypName//2890158180: INT_0Bh: 4
        public MetaHash packFileName { get; set; } //8   packFileName//4216494073: INT_0Bh: 8

        public override string ToString()
        {
            return imapName.ToString() + ", " + itypName.ToString() + ", " + packFileName.ToString();
        }
        public void SwapEnd()
        {
            imapName = new MetaHash(MetaTypes.SwapBytes(imapName.Hash));
            itypName = new MetaHash(MetaTypes.SwapBytes(itypName.Hash));
            packFileName = new MetaHash(MetaTypes.SwapBytes(packFileName.Hash));
        }
    }

    [TC(typeof(EXP))] public struct CImapDependencies : IPsoSwapEnd //24 bytes, Type:0  // CImapDependencies//3240050401 imapDependencies_2
    {
        public MetaHash imapName { get; set; } //0   imapName: INT_0Bh: 0  //name hash
        public ushort manifestFlags { get; set; } //4   manifestFlags//1683136603: SHORT_0Fh: 4: 2097153
        public ushort Unused0 { get; set; } //6
        public Array_uint itypDepArray { get; set; } //8   itypDepArray//2410949350: Array: 8: 3  {256: INT_0Bh: 0} //children...


        public override string ToString()
        {
            return imapName.ToString() + ": " + manifestFlags.ToString() + ": " + itypDepArray.ToString();
        }
        public void SwapEnd()
        {
            imapName = new MetaHash(MetaTypes.SwapBytes(imapName.Hash));
            manifestFlags = MetaTypes.SwapBytes(manifestFlags);
            var d = itypDepArray; d.SwapEnd(); itypDepArray = d;
        }
    }

    [TC(typeof(EXP))] public struct CItypDependencies : IPsoSwapEnd //24 bytes, Type:0 // CItypDependencies//1515605584  itypDependencies_2
    {
        public MetaHash itypName { get; set; } //0   itypName//2890158180: INT_0Bh: 0
        public ushort manifestFlags { get; set; } //4   manifestFlags//1683136603: SHORT_0Fh: 4: 2097153
        public ushort Unused0 { get; set; } //6
        public Array_uint itypDepArray { get; set; } //8   itypDepArray//2410949350: Array: 8: 3  {256: INT_0Bh: 0}

        public override string ToString()
        {
            return itypName.ToString() + ": " + manifestFlags.ToString() + ": " + itypDepArray.ToString();
        }
        public void SwapEnd()
        {
            itypName = new MetaHash(MetaTypes.SwapBytes(itypName.Hash));
            manifestFlags = MetaTypes.SwapBytes(manifestFlags);
            var d = itypDepArray; d.SwapEnd(); itypDepArray = d;
        }
    }

    [TC(typeof(EXP))] public struct Unk_741495440 : IPsoSwapEnd //24 bytes, Type:0   // Interiors
    {
        public MetaHash Name { get; set; } //0   Name: INT_0Bh: 0
        public uint Unused0 { get; set; } //4
        public Array_uint Bounds { get; set; } //8   Bounds//3298223272: Array: 8: 1  {256: INT_0Bh: 0}

        public override string ToString()
        {
            return JenkIndex.GetString(Name);
        }
        public void SwapEnd()
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash));
            var b = Bounds; b.SwapEnd(); Bounds = b;
        }
    }






    [TC(typeof(EXP))] public struct CScenarioPointManifest : IPsoSwapEnd //56 bytes, Type:0
    {
        public int VersionNumber { get; set; } //0   VersionNumber: INT_05h: 0
        public uint Unused0 { get; set; } //4
        public Array_StructurePointer RegionDefs { get; set; } //8   RegionDefs: Array: 8: 1  {ARRAYINFO: Structure: 0}
        public Array_StructurePointer Groups { get; set; } //24   Groups: Array: 24: 3  {ARRAYINFO: Structure: 0}
        public Array_uint InteriorNames { get; set; } //40   InteriorNames: Array: 40: 5  {ARRAYINFO: INT_0Bh: 0}

        public override string ToString()
        {
            return VersionNumber.ToString();
        }
        public void SwapEnd()
        {
            VersionNumber = MetaTypes.SwapBytes(VersionNumber);
            var r = RegionDefs; r.SwapEnd(); RegionDefs = r;
            var g = Groups; g.SwapEnd(); Groups = g;
            var i = InteriorNames; i.SwapEnd(); InteriorNames = i;
        }
    }

    [TC(typeof(EXP))] public struct CScenarioPointRegionDef : IPsoSwapEnd //64 bytes, Type:0
    {
        public MetaHash Name { get; set; } //0   Name: INT_0Bh: 0
        public uint Unused0 { get; set; } //4
        public uint Unused1 { get; set; } //8
        public uint Unused2 { get; set; } //12
        public rage__spdAABB AABB { get; set; } //16   AABB: Structure: 16: rage__spdAABB
        public uint Unused3 { get; set; } //48
        public uint Unused4 { get; set; } //52
        public uint Unused5 { get; set; } //56
        public uint Unused6 { get; set; } //60

        public override string ToString()
        {
            return Name.ToString() + ", " + AABB.ToString();
        }
        public void SwapEnd()
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash));
            var aabb = AABB; aabb.SwapEnd(); AABB = aabb;
        }
    }

    [TC(typeof(EXP))] public struct CScenarioPointGroup : IPsoSwapEnd //8 bytes, Type:0
    {
        public MetaHash Name { get; set; } //0   Name: INT_0Bh: 0
        public byte EnabledByDefault { get; set; } //4   EnabledByDefault: BYTE_00h: 4
        public byte Unused0 { get; set; } //5
        public ushort Unused1 { get; set; } //6

        public override string ToString()
        {
            return Name.ToString();
        }
        public void SwapEnd()
        {
            Name = new MetaHash(MetaTypes.SwapBytes(Name.Hash));
        }
    }




}
