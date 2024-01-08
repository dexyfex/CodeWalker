

using CodeWalker.Core.Utils;
using CommunityToolkit.HighPerformance;
using Microsoft.Extensions.ObjectPool;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static CodeWalker.GameFiles.MetaXmlBase;

namespace CodeWalker.GameFiles
{
    public class MetaXml : MetaXmlBase
    {

        public static string GetXml(RpfFileEntry e, byte[] data, out string filename, string outputfolder = "")
        {
            var fn = e.Name;

            if (!string.IsNullOrEmpty(outputfolder))
            {
                outputfolder = Path.Join(outputfolder, e.ShortName);
            }

            if (fn.EndsWith(".ymt", StringComparison.OrdinalIgnoreCase))
            {
                YmtFile ymt = RpfFile.GetFile<YmtFile>(e, data);
                return GetXml(ymt, out filename);
            }
            else if (fn.EndsWith(".ymf", StringComparison.OrdinalIgnoreCase))
            {
                YmfFile ymf = RpfFile.GetFile<YmfFile>(e, data);
                return GetXml(ymf, out filename);
            }
            else if (fn.EndsWith(".ymap", StringComparison.OrdinalIgnoreCase))
            {
                YmapFile ymap = RpfFile.GetFile<YmapFile>(e, data);
                return GetXml(ymap, out filename);
            }
            else if (fn.EndsWith(".ytyp", StringComparison.OrdinalIgnoreCase))
            {
                YtypFile ytyp = RpfFile.GetFile<YtypFile>(e, data);
                return GetXml(ytyp, out filename);
            }
            else if (fn.EndsWith(".pso", StringComparison.OrdinalIgnoreCase))
            {
                JPsoFile pso = RpfFile.GetFile<JPsoFile>(e, data);
                return GetXml(pso, out filename);
            }
            else if (fn.EndsWith(".cut", StringComparison.OrdinalIgnoreCase))
            {
                CutFile cut = RpfFile.GetFile<CutFile>(e, data);
                return GetXml(cut, out filename);
            }
            else if (fn.EndsWith(".rel", StringComparison.OrdinalIgnoreCase))
            {
                RelFile rel = RpfFile.GetFile<RelFile>(e, data);
                return GetXml(rel, out filename);
            }
            else if (fn.EndsWith(".ynd", StringComparison.OrdinalIgnoreCase))
            {
                YndFile ynd = RpfFile.GetFile<YndFile>(e, data);
                return GetXml(ynd, out filename);
            }
            else if (fn.EndsWith(".ynv", StringComparison.OrdinalIgnoreCase))
            {
                YnvFile ynv = RpfFile.GetFile<YnvFile>(e, data);
                return GetXml(ynv, out filename);
            }
            else if (fn.EndsWith(".ycd", StringComparison.OrdinalIgnoreCase))
            {
                YcdFile ycd = RpfFile.GetFile<YcdFile>(e, data);
                return GetXml(ycd, out filename);
            }
            else if (fn.EndsWith(".ybn", StringComparison.OrdinalIgnoreCase))
            {
                YbnFile ybn = RpfFile.GetFile<YbnFile>(e, data);
                return GetXml(ybn, out filename);
            }
            else if (fn.EndsWith(".ytd", StringComparison.OrdinalIgnoreCase))
            {
                YtdFile ytd = RpfFile.GetFile<YtdFile>(e, data);
                return GetXml(ytd, out filename, outputfolder);
            }
            else if (fn.EndsWith(".ydr", StringComparison.OrdinalIgnoreCase))
            {
                YdrFile ydr = RpfFile.GetFile<YdrFile>(e, data);
                return GetXml(ydr, out filename, outputfolder);
            }
            else if (fn.EndsWith(".ydd", StringComparison.OrdinalIgnoreCase))
            {
                YddFile ydd = RpfFile.GetFile<YddFile>(e, data);
                return GetXml(ydd, out filename, outputfolder);
            }
            else if (fn.EndsWith(".yft", StringComparison.OrdinalIgnoreCase))
            {
                YftFile yft = RpfFile.GetFile<YftFile>(e, data);
                return GetXml(yft, out filename, outputfolder);
            }
            else if (fn.EndsWith(".ypt", StringComparison.OrdinalIgnoreCase))
            {
                YptFile ypt = RpfFile.GetFile<YptFile>(e, data);
                return GetXml(ypt, out filename, outputfolder);
            }
            else if (fn.EndsWith(".yld", StringComparison.OrdinalIgnoreCase))
            {
                YldFile yld = RpfFile.GetFile<YldFile>(e, data);
                return GetXml(yld, out filename);
            }
            else if (fn.EndsWith(".yed", StringComparison.OrdinalIgnoreCase))
            {
                YedFile yed = RpfFile.GetFile<YedFile>(e, data);
                return GetXml(yed, out filename);
            }
            else if (fn.EndsWith(".ywr", StringComparison.OrdinalIgnoreCase))
            {
                YwrFile ywr = RpfFile.GetFile<YwrFile>(e, data);
                return GetXml(ywr, out filename);
            }
            else if (fn.EndsWith(".yvr", StringComparison.OrdinalIgnoreCase))
            {
                YvrFile yvr = RpfFile.GetFile<YvrFile>(e, data);
                return GetXml(yvr, out filename);
            }
            else if (fn.EndsWith(".ypdb", StringComparison.OrdinalIgnoreCase))
            {
                YpdbFile ypdb = RpfFile.GetFile<YpdbFile>(e, data);
                return GetXml(ypdb, out filename);
            }
            else if (fn.EndsWith(".yfd", StringComparison.OrdinalIgnoreCase))
            {
                YfdFile yfd = RpfFile.GetFile<YfdFile>(e, data);
                return GetXml(yfd, out filename);
            }
            else if (fn.EndsWith(".awc", StringComparison.OrdinalIgnoreCase))
            {
                AwcFile awc = RpfFile.GetFile<AwcFile>(e, data);
                return GetXml(awc, out filename, outputfolder);
            }
            else if (fn.EndsWith(".fxc", StringComparison.OrdinalIgnoreCase))
            {
                FxcFile fxc = RpfFile.GetFile<FxcFile>(e, data);
                return GetXml(fxc, out filename, outputfolder);
            }
            else if (fn.EndsWith("cache_y.dat", StringComparison.OrdinalIgnoreCase))
            {
                CacheDatFile cdf = RpfFile.GetFile<CacheDatFile>(e, data);
                return GetXml(cdf, out filename, outputfolder);
            }
            else if (fn.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) && fn.StartsWith("heightmap", StringComparison.OrdinalIgnoreCase))
            {
                HeightmapFile hmf = RpfFile.GetFile<HeightmapFile>(e, data);
                return GetXml(hmf, out filename, outputfolder);
            }
            else if (fn.EndsWith(".mrf", StringComparison.OrdinalIgnoreCase))
            {
                MrfFile mrf = RpfFile.GetFile<MrfFile>(e, data);
                return GetXml(mrf, out filename, outputfolder);
            }
            filename = fn;
            return string.Empty;
        }
        public static string GetXml(YmtFile ymt, out string filename)
        {
            var fn = (ymt?.RpfFileEntry?.Name) ?? "";
            if (ymt.Meta != null) { filename = fn + ".xml"; return GetXml(ymt.Meta); }
            else if (ymt.Pso != null) { filename = fn + ".pso.xml"; return PsoXml.GetXml(ymt.Pso); }
            else if (ymt.Rbf != null) { filename = fn + ".rbf.xml"; return RbfXml.GetXml(ymt.Rbf); }
            filename = string.Empty;
            return string.Empty;
        }
        public static string GetXml(YmfFile ymf, out string filename)
        {
            var fn = (ymf?.FileEntry?.Name) ?? "";
            if (ymf.Meta != null) { filename = fn + ".xml"; return GetXml(ymf.Meta); }
            else if (ymf.Pso != null) { filename = fn + ".pso.xml"; return PsoXml.GetXml(ymf.Pso); }
            else if (ymf.Rbf != null) { filename = fn + ".rbf.xml"; return RbfXml.GetXml(ymf.Rbf); }
            filename = string.Empty;
            return string.Empty;
        }
        public static string GetXml(YmapFile? ymap, out string filename)
        {
            var fn = (ymap?.RpfFileEntry?.Name) ?? "";
            if (ymap.Meta is not null) {
                filename = fn + ".xml";
                return GetXml(ymap.Meta);
            }
            else if (ymap.Pso is not null) {
                filename = fn + ".pso.xml";
                return PsoXml.GetXml(ymap.Pso);
            }
            else if (ymap.Rbf is not null) {
                filename = fn + ".rbf.xml";
                return RbfXml.GetXml(ymap.Rbf);
            }
            filename = string.Empty;
            return string.Empty;
        }
        public static string GetXml(YtypFile ytyp, out string filename)
        {
            if (ytyp is null)
            {
                filename = string.Empty;
                return string.Empty;
            }

            var fn = (ytyp.RpfFileEntry?.Name) ?? "";
            if (ytyp.Meta is not null)
            {
                filename = fn + ".xml";
                return GetXml(ytyp.Meta);
            }
            else if (ytyp.Pso is not null)
            {
                filename = fn + ".pso.xml";
                return PsoXml.GetXml(ytyp.Pso);
            }
            else if (ytyp.Rbf is not null) {
                filename = fn + ".rbf.xml";
                return RbfXml.GetXml(ytyp.Rbf);
            }
            filename = string.Empty;
            return string.Empty;
        }
        public static string GetXml(JPsoFile pso, out string filename)
        {
            var fn = (pso?.FileEntry?.Name) ?? "";
            if (pso.Pso != null) { filename = fn + ".pso.xml"; return PsoXml.GetXml(pso.Pso); }
            filename = string.Empty;
            return string.Empty;
        }
        public static string GetXml(CutFile cut, out string filename)
        {
            var fn = (cut?.FileEntry?.Name) ?? "";
            if (cut.Pso != null) { filename = fn + ".pso.xml"; return PsoXml.GetXml(cut.Pso); }
            filename = string.Empty;
            return string.Empty;
        }
        public static string GetXml(RelFile rel, out string filename)
        {
            var fn = (rel?.RpfFileEntry?.Name) ?? "";
            filename = fn + ".xml";
            return RelXml.GetXml(rel);
        }
        public static string GetXml(YndFile ynd, out string filename)
        {
            var fn = (ynd?.RpfFileEntry?.Name) ?? "";
            filename = fn + ".xml";
            return YndXml.GetXml(ynd);
        }
        public static string GetXml(YnvFile ynv, out string filename)
        {
            var fn = (ynv?.RpfFileEntry?.Name) ?? "";
            filename = fn + ".xml";
            return YnvXml.GetXml(ynv);
        }
        public static string GetXml(YcdFile ycd, out string filename)
        {
            var fn = (ycd?.RpfFileEntry?.Name) ?? "";
            filename = fn + ".xml";
            return YcdXml.GetXml(ycd);
        }
        public static string GetXml(YbnFile ybn, out string filename)
        {
            var fn = (ybn?.RpfFileEntry?.Name) ?? "";
            filename = fn + ".xml";
            return YbnXml.GetXml(ybn);
        }
        public static string GetXml(YtdFile ytd, out string filename, string outputfolder)
        {
            var fn = (ytd?.Name) ?? "";
            filename = fn + ".xml";
            return YtdXml.GetXml(ytd, outputfolder);
        }
        public static string GetXml(YdrFile ydr, out string filename, string outputfolder)
        {
            var fn = (ydr?.Name) ?? "";
            filename = fn + ".xml";
            return YdrXml.GetXml(ydr, outputfolder);
        }
        public static string GetXml(YddFile ydd, out string filename, string outputfolder)
        {
            var fn = (ydd?.Name) ?? "";
            filename = fn + ".xml";
            return YddXml.GetXml(ydd, outputfolder);
        }
        public static string GetXml(YftFile yft, out string filename, string outputfolder)
        {
            var fn = (yft?.Name) ?? "";
            filename = fn + ".xml";
            return YftXml.GetXml(yft, outputfolder);
        }
        public static string GetXml(YptFile ypt, out string filename, string outputfolder)
        {
            var fn = (ypt?.Name) ?? "";
            filename = fn + ".xml";
            return YptXml.GetXml(ypt, outputfolder);
        }
        public static string GetXml(YldFile yld, out string filename)
        {
            var fn = (yld?.Name) ?? "";
            filename = fn + ".xml";
            return YldXml.GetXml(yld);
        }
        public static string GetXml(YedFile yed, out string filename)
        {
            var fn = (yed?.Name) ?? "";
            filename = fn + ".xml";
            return YedXml.GetXml(yed);
        }
        public static string GetXml(YwrFile ywr, out string filename)
        {
            var fn = (ywr?.Name) ?? "";
            filename = fn + ".xml";
            return YwrXml.GetXml(ywr);
        }
        public static string GetXml(YvrFile yvr, out string filename)
        {
            var fn = (yvr?.Name) ?? "";
            filename = fn + ".xml";
            return YvrXml.GetXml(yvr);
        }
        public static string GetXml(YpdbFile ypdb, out string filename)
        {
            var fn = (ypdb?.Name) ?? "";
            filename = fn + ".xml";
            return YpdbXml.GetXml(ypdb);
        }
        public static string GetXml(YfdFile yfd, out string filename)
        {
            var fn = (yfd?.Name) ?? "";
            filename = fn + ".xml";
            return YfdXml.GetXml(yfd);
        }
        public static string GetXml(AwcFile awc, out string filename, string outputfolder)
        {
            var fn = (awc?.Name) ?? "";
            filename = fn + ".xml";
            return AwcXml.GetXml(awc, outputfolder);
        }
        public static string GetXml(FxcFile fxc, out string filename, string outputfolder)
        {
            var fn = (fxc?.Name) ?? "";
            filename = fn + ".xml";
            return FxcXml.GetXml(fxc, outputfolder);
        }
        public static string GetXml(CacheDatFile cdf, out string filename, string outputfolder)
        {
            var fn = (cdf?.FileEntry?.Name) ?? "";
            filename = fn + ".xml";
            return CacheDatXml.GetXml(cdf);
        }
        public static string GetXml(HeightmapFile hmf, out string filename, string outputfolder)
        {
            var fn = (hmf?.Name) ?? "";
            filename = fn + ".xml";
            return HmapXml.GetXml(hmf);
        }
        public static string GetXml(MrfFile mrf, out string filename, string outputfolder)
        {
            var fn = (mrf?.Name) ?? "";
            filename = fn + ".xml";
            return MrfXml.GetXml(mrf);
        }









        public static string GetXml(Meta meta)
        {
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.AppendLine(XmlHeader);

                if (meta != null)
                {
                    var cont = new MetaCont(meta);

                    WriteNode(sb, 0, cont, meta.RootBlockIndex, 0, XmlTagMode.Structure, 0, (string)meta.Name);
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        private static void WriteNode(StringBuilder sb, int indent, MetaCont cont, int blockId, int offset, XmlTagMode tagMode = XmlTagMode.None, MetaName structName = 0, string metaName = "")
        {

            var block = cont.Meta.GetBlock(blockId);
            if (block == null)
            {
                ErrorXml(sb, indent, "Couldn't find block " + blockId + "!");
                return;
            }

            if (structName == 0)
            {
                structName = block.StructureNameHash;
            }

            var name = HashString(structName);
            var data = block.Data;

            var structInfo = cont.GetStructureInfo(structName);
            if (structInfo == null)
            {
                ErrorXml(sb, indent, "Couldn't find structure info " + name + "!");
                return;
            }
            if (structInfo.Entries == null)
            {
                ErrorXml(sb, indent, "Couldn't find structure info entries for " + name + "!");
                return;
            }


            switch (tagMode)
            {
                case XmlTagMode.Structure:
                    OpenTag(sb, indent, name, metaName, true);
                    break;
                case XmlTagMode.Item:
                    OpenTag(sb, indent, "Item", metaName, true);
                    break;
                case XmlTagMode.ItemAndType:
                    OpenTag(sb, indent, $"Item type=\"{name}\"", metaName, true);
                    break;
            }

            var cind = indent + 1;
            MetaStructureEntryInfo_s arrEntry = new MetaStructureEntryInfo_s();
            for (int i = 0; i < structInfo.Entries.Length; i++)
            {
                var entry = structInfo.Entries[i];
                if (entry.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO)
                {
                    arrEntry = entry;
                    continue;
                }
                var ename = HashString(entry.EntryNameHash);
                var eoffset = offset + entry.DataOffset;
                switch (entry.DataType)
                {
                    default:
                        ErrorXml(sb, cind, $"{ename}: Unexpected entry DataType: {entry.DataType}");
                        break;
                    case MetaStructureEntryDataType.Array:

                        WriteArrayNode(sb, cind, cont, data, arrEntry, ename, eoffset);

                        break;
                    case MetaStructureEntryDataType.ArrayOfBytes:

                        WriteParsedArrayOfBytesNode(sb, cind, data, ename, eoffset, entry, arrEntry);

                        break;
                    case MetaStructureEntryDataType.ArrayOfChars:
                        OpenTag(sb, cind, ename, false);
                        uint charArrLen = (uint)entry.ReferenceKey;
                        for (int n = 0; n < charArrLen; n++)
                        {
                            var bidx = eoffset + n;
                            if ((bidx >= 0) && (bidx < data.Length))
                            {
                                byte b = data[bidx];
                                if (b == 0) break;
                                sb.Append((char)b);
                            }
                        }
                        CloseTag(sb, 0, ename);
                        break;
                    case MetaStructureEntryDataType.Boolean:
                        var boolVal = BitConverter.ToBoolean(data, eoffset);
                        ValueTag(sb, cind, ename, boolVal?"true":"false");
                        break;
                    case MetaStructureEntryDataType.ByteEnum:
                        var byteEnumVal = data[eoffset];
                        ValueTag(sb, cind, ename, byteEnumVal.ToString());
                        break;
                    case MetaStructureEntryDataType.CharPointer:
                        MetaTypes.TryConvertData<CharPointer>(data.AsSpan(eoffset), out var charPtr);
                        string charStr = MetaTypes.GetString(cont.Meta, charPtr);
                        OneLineTag(sb, cind, ename, charStr);
                        break;
                    case MetaStructureEntryDataType.DataBlockPointer:
                        MetaTypes.TryConvertData<DataBlockPointer>(data.AsSpan(eoffset), out var dataPtr);
                        //need to just get all the data from that block, since this pointer is referring to the whole block! it should be of type BYTE!
                        var dblock = cont.Meta.GetBlock((int)dataPtr.PointerDataId);
                        WriteRawArray(sb, dblock.Data, cind, ename, "ByteArray", FormatHexByte, 32);
                        break;
                    case MetaStructureEntryDataType.Float:
                        var floatVal = BitConverter.ToSingle(data, eoffset);
                        ValueTag(sb, cind, ename, FloatUtil.ToString(floatVal));
                        break;
                    case MetaStructureEntryDataType.Float_XYZ:
                        MetaTypes.TryConvertData<Vector3>(data.AsSpan(eoffset), out var v3);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v3.X)}\" y=\"{FloatUtil.ToString(v3.Y)}\" z=\"{FloatUtil.ToString(v3.Z)}\"");
                        break;
                    case MetaStructureEntryDataType.Float_XYZW:
                        MetaTypes.TryConvertData<Vector4>(data.AsSpan(eoffset), out var v4);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v4.X)}\" y=\"{FloatUtil.ToString(v4.Y)}\" z=\"{FloatUtil.ToString(v4.Z)}\" w=\"{FloatUtil.ToString(v4.W)}\"");
                        break;
                    case MetaStructureEntryDataType.Hash:
                        MetaTypes.TryConvertData<MetaHash>(data.AsSpan(eoffset), out var hashVal);
                        var hashStr = HashString(hashVal);
                        StringTag(sb, cind, ename, hashStr);
                        break;
                    case MetaStructureEntryDataType.IntEnum:
                        var intEnumVal = BitConverter.ToInt32(data, eoffset);
                        var intEnumStr = GetEnumString(cont, entry, intEnumVal);
                        StringTag(sb, cind, ename, intEnumStr);
                        break;
                    case MetaStructureEntryDataType.IntFlags1:
                        var intFlags1Val = BitConverter.ToInt32(data, eoffset);
                        var intFlags1Str = GetEnumString(cont, entry, intFlags1Val);
                        StringTag(sb, cind, ename, intFlags1Str);
                        break;
                    case MetaStructureEntryDataType.IntFlags2:
                        var intFlags2Val = BitConverter.ToInt32(data, eoffset);
                        var intFlags2Str = GetEnumString(cont, entry, intFlags2Val);
                        StringTag(sb, cind, ename, intFlags2Str);
                        break;
                    case MetaStructureEntryDataType.ShortFlags:
                        var shortFlagsVal = BitConverter.ToInt16(data, eoffset);
                        var shortFlagsStr = GetEnumString(cont, entry, shortFlagsVal);
                        StringTag(sb, cind, ename, shortFlagsStr);
                        break;
                    case MetaStructureEntryDataType.SignedByte:
                        sbyte sbyteVal = (sbyte)data[eoffset];
                        ValueTag(sb, cind, ename, sbyteVal.ToString());
                        break;
                    case MetaStructureEntryDataType.SignedInt:
                        var intVal = BitConverter.ToInt32(data, eoffset);
                        ValueTag(sb, cind, ename, intVal.ToString());
                        break;
                    case MetaStructureEntryDataType.SignedShort:
                        var shortVal = BitConverter.ToInt16(data, eoffset);
                        ValueTag(sb, cind, ename, shortVal.ToString());
                        break;
                    case MetaStructureEntryDataType.Structure:
                        OpenTag(sb, cind, ename);
                        WriteNode(sb, cind, cont, blockId, eoffset, XmlTagMode.None, entry.ReferenceKey);
                        CloseTag(sb, cind, ename);
                        break;
                    case MetaStructureEntryDataType.StructurePointer:
                        OpenTag(sb, cind, ename);
                        ErrorXml(sb, cind + 1, "StructurePointer not supported here! Tell dexy!");
                        CloseTag(sb, cind, ename);
                        break;
                    case MetaStructureEntryDataType.UnsignedByte:
                        var byteVal = data[eoffset];
                        ValueTag(sb, cind, ename, byteVal.ToString());
                        //ValueTag(sb, cind, ename, "0x" + byteVal.ToString("X").PadLeft(2, '0'));
                        break;
                    case MetaStructureEntryDataType.UnsignedInt:
                        var uintVal = BitConverter.ToUInt32(data, eoffset);
                        switch (entry.EntryNameHash)
                        {
                            default:
                                ValueTag(sb, cind, ename, uintVal.ToString());
                                break;
                            case MetaName.color:
                                ValueTag(sb, cind, ename, $"0x{uintVal:X8}");
                                break;
                        }

                        break;
                    case MetaStructureEntryDataType.UnsignedShort:
                        var ushortVal = BitConverter.ToUInt16(data, eoffset);
                        ValueTag(sb, cind, ename, ushortVal.ToString());// "0x" + ushortVal.ToString("X").PadLeft(4, '0'));
                        break;
                }
            }

            switch (tagMode)
            {
                case XmlTagMode.Structure:
                    CloseTag(sb, indent, name);
                    break;
                case XmlTagMode.Item:
                case XmlTagMode.ItemAndType:
                    CloseTag(sb, indent, "Item");
                    break;
            }

        }

        private static void WriteArrayNode(StringBuilder sb, int indent, MetaCont cont, byte[] data, MetaStructureEntryInfo_s arrEntry, string ename, int eoffset)
        {
            int aCount = 0;
            var aind = indent + 1;
            string arrTag = ename;
            switch (arrEntry.DataType)
            {
                default:
                    ErrorXml(sb, indent, $"{ename}: Unexpected array entry DataType: {arrEntry.DataType}");
                    break;
                case MetaStructureEntryDataType.Structure:
                    MetaTypes.TryConvertData<Array_Structure>(data, eoffset, out var arrStruc);
                    var aBlockId = (int)arrStruc.PointerDataId;
                    var aOffset = (int)arrStruc.PointerDataOffset;
                    aCount = arrStruc.Count1;
                    arrTag += $" itemType=\"{HashString(arrEntry.ReferenceKey)}\"";
                    if (aCount > 0)
                    {
                        OpenTag(sb, indent, arrTag);
                        var atyp = cont.GetStructureInfo(arrEntry.ReferenceKey);
                        var aBlock = cont.Meta.GetBlock(aBlockId);
                        for (int n = 0; n < aCount; n++)
                        {
                            WriteNode(sb, aind, cont, aBlockId, aOffset, XmlTagMode.Item, arrEntry.ReferenceKey);
                            aOffset += atyp.StructureSize;

                            if ((n < (aCount - 1)) && (aBlock != null) && (aOffset >= aBlock.DataLength))
                            {
                                aOffset = 0;
                                aBlockId++;
                                aBlock = cont.Meta.GetBlock(aBlockId);
                            }
                        }
                        CloseTag(sb, indent, ename);
                    }
                    else
                    {
                        SelfClosingTag(sb, indent, arrTag);
                    }
                    break;
                case MetaStructureEntryDataType.StructurePointer:
                    MetaTypes.TryConvertData<Array_StructurePointer>(data, eoffset, out var arrStrucP);
                    var ptrArr = MetaTypes.GetPointerArray(cont.Meta, in arrStrucP);
                    aCount = ptrArr.Length;
                    if (aCount > 0)
                    {
                        OpenTag(sb, indent, arrTag);
                        for (int n = 0; n < aCount; n++)
                        {
                            ref var ptr = ref ptrArr[n];
                            var offset = ptr.Offset;
                            WriteNode(sb, aind, cont, ptr.BlockID, offset, XmlTagMode.ItemAndType);
                        }
                        CloseTag(sb, indent, ename);
                    }
                    else
                    {
                        SelfClosingTag(sb, indent, arrTag);
                    }
                    break;
                case MetaStructureEntryDataType.UnsignedInt:
                    MetaTypes.TryConvertData<Array_uint>(data, eoffset, out var arrUint);
                    var uintArr = MetaTypes.GetUintArray(cont.Meta, in arrUint);
                    WriteRawArray(sb, uintArr, indent, ename, "uint");
                    break;
                case MetaStructureEntryDataType.UnsignedShort:
                    MetaTypes.TryConvertData<Array_ushort>(data, eoffset, out var arrUshort);
                    var ushortArr = MetaTypes.GetUshortArray(cont.Meta, in arrUshort);
                    WriteRawArray(sb, ushortArr, indent, ename, "ushort");
                    break;
                case MetaStructureEntryDataType.UnsignedByte:
                    MetaTypes.TryConvertData<Array_byte>(data, eoffset, out var arrUbyte);
                    var byteArr = MetaTypes.GetByteArray(cont.Meta, in arrUbyte);
                    WriteRawArray(sb, byteArr, indent, ename, "byte");
                    break;
                case MetaStructureEntryDataType.Float:
                    MetaTypes.TryConvertData<Array_float>(data, eoffset, out var arrFloat);
                    var floatArr = MetaTypes.GetFloatArray(cont.Meta, in arrFloat);
                    WriteRawArray(sb, floatArr, indent, ename, "float");
                    break;
                case MetaStructureEntryDataType.Float_XYZ:
                    MetaTypes.TryConvertData<Array_Vector3>(data, eoffset, out var arrV3);
                    var v4Arr = MetaTypes.ConvertDataArray<Vector4>(cont.Meta, (MetaName)MetaTypeName.VECTOR4, arrV3.Pointer, arrV3.Count1);
                    WriteItemArray(sb, v4Arr, indent, ename, "Vector3/4", FormatVector4);
                    break;
                case MetaStructureEntryDataType.CharPointer:
                    ErrorXml(sb, indent, "CharPointer ARRAY not supported here! Tell dexy!");
                    break;
                case MetaStructureEntryDataType.DataBlockPointer:
                    ErrorXml(sb, indent, "DataBlockPointer ARRAY not supported here! Tell dexy!");
                    break;
                case MetaStructureEntryDataType.Hash:
                    MetaTypes.TryConvertData<Array_uint>(data, eoffset, out var arrHash);
                    var hashArr = MetaTypes.GetHashArray(cont.Meta, in arrHash);
                    WriteItemArray(sb, hashArr, indent, ename, "Hash", FormatHash);
                    break;
            }
        }

        private static void WriteParsedArrayOfBytesNode(StringBuilder sb, int indent, byte[] data, string ename, int eoffset, MetaStructureEntryInfo_s entry, MetaStructureEntryInfo_s arrEntry)
        {
            OpenTag(sb, indent, ename, false);

            var byteArrLen = ((int)entry.ReferenceKey);

            switch (arrEntry.DataType)
            {
                default:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n;
                        byte b = ((bidx >= 0) && (bidx < data.Length)) ? data[bidx] : (byte)0;
                        sb.Append(b.ToString("X2"));
                    }
                    break;
                case MetaStructureEntryDataType.SignedByte:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n;
                        sbyte b = ((bidx >= 0) && (bidx < data.Length)) ? (sbyte)data[bidx] : (sbyte)0;
                        sb.Append(b.ToString()); //sb.Append(b.ToString("X").PadLeft(2, '0')); to show HEX values
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;

                case MetaStructureEntryDataType.UnsignedByte:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n;
                        byte b = ((bidx >= 0) && (bidx < data.Length)) ? data[bidx] : (byte)0;
                        sb.Append(b.ToString());
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;
                case MetaStructureEntryDataType.SignedShort:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n * 2;
                        short b = ((bidx >= 0) && (bidx < data.Length)) ? BitConverter.ToInt16(data, bidx) : (short)0;
                        sb.Append(b.ToString());
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;
                case MetaStructureEntryDataType.UnsignedShort:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n * 2;
                        ushort b = ((bidx >= 0) && (bidx < data.Length)) ? BitConverter.ToUInt16(data, bidx) : (ushort)0;
                        sb.Append(b.ToString());
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;
                case MetaStructureEntryDataType.SignedInt:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n * 4;
                        int b = ((bidx >= 0) && (bidx < data.Length)) ? BitConverter.ToInt32(data, bidx) : (int)0;
                        sb.Append(b.ToString());
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;
                case MetaStructureEntryDataType.UnsignedInt:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n * 4;
                        uint b = ((bidx >= 0) && (bidx < data.Length)) ? BitConverter.ToUInt32(data, bidx) : (uint)0;
                        sb.Append(b.ToString());
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;
                case MetaStructureEntryDataType.Float:
                    for (int n = 0; n < byteArrLen; n++)
                    {
                        var bidx = eoffset + n * 4;
                        float b = ((bidx >= 0) && (bidx < data.Length)) ? BitConverter.ToSingle(data, bidx) : (float)0;
                        sb.Append(FloatUtil.ToString(b));
                        if (n < byteArrLen - 1) sb.Append(' ');
                    }
                    break;
            }

            CloseTag(sb, 0, ename);
        }



        private static string GetEnumString(MetaCont cont, MetaStructureEntryInfo_s entry, int value)
        {
            var eName = entry.ReferenceKey;
            var eInfo = cont.GetEnumInfo(eName);
            if ((eInfo == null) || (eInfo.Entries == null))
            {
                return value.ToString();
            }

            bool isFlags = (entry.DataType == MetaStructureEntryDataType.IntFlags1) ||
                           (entry.DataType == MetaStructureEntryDataType.IntFlags2);// ||
                           //(entry.DataType == MetaStructureEntryDataType.ShortFlags);

            if (isFlags)
            {
                StringBuilder sb = StringBuilderPool.Get();
                try
                {
                    foreach (var ev in eInfo.Entries)
                    {
                        var v = ev.EntryValue;
                        var m = 1 << v;
                        if ((value & m) > 0)
                        {
                            if (sb.Length > 0) sb.Append(", ");
                            sb.Append(HashString(ev.EntryNameHash));
                        }
                    }
                    return sb.ToString();
                }
                finally
                {
                    StringBuilderPool.Return(sb);
                }
            }
            else
            {
                foreach (var ev in eInfo.Entries)
                {
                    if (ev.EntryValue == value)
                    {
                        return HashString(ev.EntryNameHash);
                    }
                }
                return value.ToString(); //if we got here, there was no match...
            }
        }




        private class MetaCont
        {
            public Meta Meta { get; set; }

            Dictionary<MetaName, MetaStructureInfo> structInfos = new Dictionary<MetaName, MetaStructureInfo>();
            Dictionary<MetaName, MetaEnumInfo> enumInfos = new Dictionary<MetaName, MetaEnumInfo>();

            public MetaCont(Meta meta)
            {
                Meta = meta;

                if (meta.StructureInfos?.Data != null)
                {
                    foreach (var si in meta.StructureInfos)
                    {
                        structInfos[si.StructureNameHash] = si;
                    }
                }
                if (meta.EnumInfos?.Data != null)
                {
                    foreach (var ei in meta.EnumInfos)
                    {
                        enumInfos[ei.EnumNameHash] = ei;
                    }
                }
            }

            public MetaStructureInfo GetStructureInfo(MetaName name)
            {
                MetaStructureInfo i = null;
                structInfos.TryGetValue(name, out i);
                return i;
            }
            public MetaEnumInfo GetEnumInfo(MetaName name)
            {
                MetaEnumInfo i = null;
                enumInfos.TryGetValue(name, out i);
                return i;
            }

        }

    }

    public class PsoXml : MetaXmlBase
    {
        public static string GetXml(PsoFile pso)
        {
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.AppendLine(XmlHeader);

                if (pso?.DataSection is not null && pso.DataMapSection is not null)
                {
                    var cont = new PsoCont(pso);

                    WriteNode(sb, 0, cont, pso.DataMapSection.RootId, 0, XmlTagMode.Structure);
                }

                return sb.ToString();
            }
            finally
            {
                Console.WriteLine($"StringBuilder Capacity: {sb.Capacity}");
                StringBuilderPool.Return(sb);
            }
        }


        private static void WriteNode(StringBuilder sb, int indent, PsoCont cont, int blockId, int offset, XmlTagMode tagMode = XmlTagMode.None, MetaName structName = 0)
        {
            if (!cont.Pso.TryGetBlock(blockId, out var block))
            {
                ErrorXml(sb, indent, $"Couldn't find block {blockId}!");
                return;
            }

            if (offset >= block.Length)
            {
                offset >>= 8; //how to tell when to do this??
            }

            var boffset = offset + block.Offset;

            if (structName == 0)
            {
                structName = block.NameHash;
            }

            var name = HashString(structName);
            var data = cont.Pso.DataSection.Data;

            var structInfo = cont.GetStructureInfo(structName);
            if (structInfo == null)
            {
                structInfo = PsoTypesStructureInfo.GetStructureInfo(structName);//fallback to builtin...
            }
            if (structInfo == null)
            {
                ErrorXml(sb, indent, $"Couldn't find structure info {name}!");
                return;
            }
            if (structInfo.Entries == null)
            {
                ErrorXml(sb, indent, $"Couldn't find structure info entries for {name}!");
                return;
            }

            switch (tagMode)
            {
                case XmlTagMode.Structure:
                    OpenTag(sb, indent, name);
                    break;
                case XmlTagMode.Item:
                    OpenTag(sb, indent, "Item");
                    break;
                case XmlTagMode.ItemAndType:
                    OpenTag(sb, indent, $"Item type=\"{name}\"");
                    break;
            }


            var cind = indent + 1;
            foreach(ref var entry in structInfo.Entries.AsSpan())
            {
                if (entry.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO)
                {
                    continue;
                }
                var ename = HashString(entry.EntryNameHash);
                var eoffset = boffset + entry.DataOffset;
                switch (entry.Type)
                {
                    default:
                        ErrorXml(sb, cind, $"{ename}: Unexpected entry DataType: {entry.Type}");
                        break;
                    case PsoDataType.Array:

                        WriteArrayNode(sb, cind, cont, blockId, offset, entry, structInfo, ename);

                        break;
                    case PsoDataType.Bool:
                        var boolVal = BitConverter.ToBoolean(data, eoffset);
                        ValueTag(sb, cind, ename, boolVal?"true":"false");
                        break;
                    case PsoDataType.SByte: //was LONG_01h //signed byte?
                        //var long1Val = MetaTypes.SwapBytes(BitConverter.ToUInt64(data, eoffset));
                        //ValueTag(sb, cind, ename, long1Val.ToString());
                        var byte1Val = (sbyte)data[eoffset];
                        ValueTag(sb, cind, ename, byte1Val.ToString());
                        break;
                    case PsoDataType.UByte:
                        var byte2Val = data[eoffset];
                        ValueTag(sb, cind, ename, byte2Val.ToString());
                        break;
                    case PsoDataType.Enum:
                        var enumInfo = cont.GetEnumInfo((MetaName)entry.ReferenceKey);
                        switch (entry.Unk_5h)
                        {
                            default:
                                ErrorXml(sb, cind, $"{ename}: Unexpected Enum subtype: {entry.Unk_5h}");
                                break;
                            case 0: //int enum
                                var intEVal = MetaTypes.SwapBytes(BitConverter.ToInt32(data, eoffset));
                                var intH = string.Empty;
                                if (enumInfo.TryFindEntry(intEVal, out var intE))
                                {
                                    intH = HashString(intE.EntryNameHash);
                                }

                                StringTag(sb, cind, ename, intH);

                                break;
                            case 2: //byte enum
                                var byteEVal = data[eoffset];
                                var byteH = string.Empty;
                                if (enumInfo.TryFindEntry(byteEVal, out var byteE))
                                {
                                    byteH = HashString(byteE.EntryNameHash);
                                }
                                StringTag(sb, cind, ename, byteH);
                                break;
                        }
                        break;
                    case PsoDataType.Flags:
                        //uint fCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
                        uint fEntry = (entry.ReferenceKey & 0xFFF);
                        var fEnt = (fEntry != 0xFFF) ? structInfo.GetEntry((int)fEntry) : null;
                        PsoEnumInfo flagsInfo = null;
                        if ((fEnt != null) && (fEnt.Value.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO))
                        {
                            flagsInfo = cont.GetEnumInfo((MetaName)fEnt.Value.ReferenceKey);
                        }
                        if (flagsInfo == null)
                        {
                            if (fEntry != 0xFFF)
                            { }
                            //flagsInfo = cont.GetEnumInfo(entry.EntryNameHash);
                        }
                        uint? flagsVal = null;
                        switch (entry.Unk_5h)
                        {
                            default:
                                ErrorXml(sb, cind, $"{ename}: Unexpected Flags subtype: {entry.Unk_5h}");
                                break;
                            case 0: //int flags
                                flagsVal = MetaTypes.SwapBytes(BitConverter.ToUInt32(data, eoffset));
                                break;
                            case 1: //short flags
                                flagsVal = MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset));
                                break;
                            case 2: //byte flags
                                flagsVal = data[eoffset];
                                break;
                        }
                        if (flagsVal.HasValue)
                        {
                            uint fv = flagsVal.Value;
                            if (flagsInfo != null)
                            {
                                string fstr = "";
                                for (int n = 0; n < flagsInfo.EntriesCount; n++)
                                {
                                    var fentry = flagsInfo.Entries[n];
                                    var fmask = (1 << fentry.EntryKey);
                                    if ((fv & fmask) > 0)
                                    {
                                        if (fstr != "") fstr += " ";
                                        fstr += HashString(fentry.EntryNameHash);
                                    }
                                }
                                StringTag(sb, cind, ename, fstr);
                            }
                            else
                            {
                                if (fv != 0) ValueTag(sb, cind, ename, fv.ToString());
                                else SelfClosingTag(sb, cind, ename);
                            }
                        }
                        break;
                    case PsoDataType.Float:
                        var floatVal = MetaTypes.SwapBytes(BitConverter.ToSingle(data, eoffset));
                        ValueTag(sb, cind, ename, FloatUtil.ToString(floatVal));
                        break;
                    case PsoDataType.Float2:
                        MetaTypes.TryConvertData<Vector2>(data, eoffset, out var v2);
                        v2 = MetaTypes.SwapBytes(in v2);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v2.X)}\" y=\"{FloatUtil.ToString(v2.Y)}\"");
                        break;
                    case PsoDataType.Float3:
                        MetaTypes.TryConvertData<Vector3>(data, eoffset, out var v3);
                        v3 = MetaTypes.SwapBytes(in v3);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v3.X)}\" y=\"{FloatUtil.ToString(v3.Y)}\" z=\"{FloatUtil.ToString(v3.Z)}\"");
                        break;
                    case PsoDataType.Float3a: //TODO: check this!
                        MetaTypes.TryConvertData<Vector3>(data, eoffset, out var v3a);
                        v3a = MetaTypes.SwapBytes(in v3a);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v3a.X)}\" y=\"{FloatUtil.ToString(v3a.Y)}\" z=\"{FloatUtil.ToString(v3a.Z)}\"");
                        break;
                    case PsoDataType.Float4a: //TODO: check this! //...why are there 3 different types of float3?
                        MetaTypes.TryConvertData<Vector3>(data, eoffset, out var v3b);
                        v3b = MetaTypes.SwapBytes(in v3b);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v3b.X)}\" y=\"{FloatUtil.ToString(v3b.Y)}\" z=\"{FloatUtil.ToString(v3b.Z)}\"");
                        break;
                    case PsoDataType.Float4:
                        MetaTypes.TryConvertData<Vector4>(data, eoffset, out var v4);
                        v4 = MetaTypes.SwapBytes(in v4);
                        SelfClosingTag(sb, cind, $"{ename} x=\"{FloatUtil.ToString(v4.X)}\" y=\"{FloatUtil.ToString(v4.Y)}\" z=\"{FloatUtil.ToString(v4.Z)}\" w=\"{FloatUtil.ToString(v4.W)}\"");
                        break;
                    case PsoDataType.SInt: //TODO: convert hashes?
                        var int5Val = MetaTypes.SwapBytes(BitConverter.ToInt32(data, eoffset));
                        ValueTag(sb, cind, ename, int5Val.ToString());
                        break;
                    case PsoDataType.UInt:
                        switch (entry.Unk_5h)
                        {
                            default:
                                ErrorXml(sb, cind, $"{ename}: Unexpected Integer subtype: {entry.Unk_5h}");
                                break;
                            case 0: //signed int (? flags?)
                                var int6aVal = MetaTypes.SwapBytes(BitConverter.ToInt32(data, eoffset));
                                ValueTag(sb, cind, ename, int6aVal.ToString());
                                break;
                            case 1: //unsigned int
                                var int6bVal = MetaTypes.SwapBytes(BitConverter.ToUInt32(data, eoffset));
                                //ValueTag(sb, cind, ename, int6bVal.ToString());
                                ValueTag(sb, cind, ename, $"0x{int6bVal:X8}");
                                break;
                        }
                        break;
                    case PsoDataType.Long:
                        var long2Val = MetaTypes.SwapBytes(BitConverter.ToUInt64(data, eoffset));
                        ValueTag(sb, cind, ename, long2Val.ToString());
                        break;
                    case PsoDataType.Map:

                        WriteMapNode(sb, indent, cont, eoffset, entry, structInfo, ename);

                        break;
                    case PsoDataType.SShort:
                        var short3Val = (short)MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset));
                        ValueTag(sb, cind, ename, short3Val.ToString());
                        break;
                    case PsoDataType.UShort:
                        var short4Val = MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset));
                        ValueTag(sb, cind, ename, short4Val.ToString());
                        break;
                    case PsoDataType.HFloat://half float?
                        var short1EVal = MetaTypes.SwapBytes(BitConverter.ToInt16(data, eoffset));
                        ValueTag(sb, cind, ename, short1EVal.ToString());
                        break;
                    case PsoDataType.String:
                        var str0 = XmlEscape(GetStringValue(cont.Pso, entry, data, eoffset));
                        //if (str0 == null)
                        //{
                        //    ErrorXml(sb, cind, ename + ": Unexpected String subtype: " + entry.Unk_5h.ToString());
                        //}
                        //else
                        {
                            StringTag(sb, cind, ename, str0);
                        }
                        break;
                    case PsoDataType.Structure:
                        switch (entry.Unk_5h)
                        {
                            default:
                                ErrorXml(sb, cind, $"{ename}: Unexpected Structure subtype: {entry.Unk_5h}");
                                break;
                            case 0: //default structure
                                OpenTag(sb, cind, ename);
                                WriteNode(sb, cind, cont, blockId, offset + entry.DataOffset, XmlTagMode.None, (MetaName)entry.ReferenceKey);
                                CloseTag(sb, cind, ename);
                                break;
                            case 3: //structure pointer...
                            case 4: //also pointer? what's the difference?
                                MetaTypes.TryConvertData<PsoPOINTER>(data.AsSpan(eoffset), out var ptrVal);
                                ptrVal = ptrVal.SwapEnd();
                                var pbid = ptrVal.BlockID;
                                bool pbok = true;
                                if (pbid <= 0)
                                {
                                    pbok = false; //no block specified?
                                }
                                if (pbid > cont.Pso.DataMapSection.EntriesCount)
                                {
                                    pbok = false; //bad pointer? different type..? should output an error message here?
                                }
                                if (pbok)
                                {
                                    var typename = string.Empty;
                                    if (cont.Pso.TryGetBlock(pbid, out var _block))
                                    {
                                        typename = HashString(_block.NameHash);
                                    }
                                    OpenTag(sb, cind, $"{ename} type=\"{typename}\"");
                                    WriteNode(sb, cind, cont, ptrVal.BlockID, (int)ptrVal.ItemOffset, XmlTagMode.None, (MetaName)entry.ReferenceKey);
                                    CloseTag(sb, cind, ename);
                                }
                                else
                                {
                                    SelfClosingTag(sb, cind, ename);
                                }
                                break;
                        }
                        break;
                }
            }




            switch (tagMode)
            {
                case XmlTagMode.Structure:
                    CloseTag(sb, indent, name);
                    break;
                case XmlTagMode.Item:
                case XmlTagMode.ItemAndType:
                    CloseTag(sb, indent, "Item");
                    break;
            }
        }

        private static void WriteArrayNode(StringBuilder sb, int indent, PsoCont cont, int blockId, int offset, PsoStructureEntryInfo entry, PsoStructureInfo estruct, string ename)
        {
            if (!cont.Pso.TryGetBlock(blockId, out var block))
            {
                Console.WriteLine("Couldn't get blockId");
                return;
            }

            var boffset = offset + block.Offset;
            var eoffset = boffset + entry.DataOffset;
            var aOffset = offset + entry.DataOffset;
            var abOffset = aOffset + block.Offset;
            var aBlockId = blockId;
            uint aCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
            Array_Structure arrStruc = new Array_Structure
            {
                PointerDataId = (uint)aBlockId,
                PointerDataOffset = (uint)aOffset,
                Count1 = (ushort)aCount,
                Count2 = (ushort)aCount,
            };
            var aind = indent + 1;
            string arrTag = ename;
            var arrEntInd = (entry.ReferenceKey & 0xFFFF);
            if (arrEntInd >= estruct.EntriesCount)
            {
                arrEntInd = (entry.ReferenceKey & 0xFFF);
            }
            if (!estruct.TryGetEntry((int)arrEntInd, out var arrEntry))
            {
                ErrorXml(sb, indent, $"ARRAYINFO not found for {ename}!");
                return;
            }

            var data = cont.Pso.DataSection.Data;

            bool embedded = true;
            switch (entry.Unk_5h)
            {
                default:
                    ErrorXml(sb, indent, $"{ename}: WIP! Unsupported Array subtype: {entry.Unk_5h}");
                    break;
                case 0: //Array_Structure
                    MetaTypes.TryConvertData<Array_Structure>(data, eoffset, out arrStruc);
                    arrStruc = arrStruc.SwapEnd();
                    aBlockId = (int)arrStruc.PointerDataId;
                    aOffset = (int)arrStruc.PointerDataOffset;
                    aCount = arrStruc.Count1;
                    if (cont.Pso.TryGetBlock(aBlockId, out var aBlock))
                    {
                        abOffset = aOffset + aBlock.Offset;
                    }
                    embedded = false;
                    break;
                case 1: //Raw in-line array
                    break;
                case 2: //also raw in-line array, but how different from above?
                    break;
                case 4: //pointer array? default array?
                    if (arrEntry.Unk_5h == 3) //pointers...
                    {
                        MetaTypes.TryConvertData<Array_Structure>(data, eoffset, out arrStruc);
                        arrStruc = arrStruc.SwapEnd();
                        aBlockId = (int)arrStruc.PointerDataId;
                        aOffset = (int)arrStruc.PointerDataOffset;
                        aCount = arrStruc.Count1;
                        if (cont.Pso.TryGetBlock(aBlockId, out var aBlock2))
                        {
                            abOffset = aOffset + aBlock2.Offset;
                        }
                        embedded = false;
                    }
                    else
                    {
                    }
                    break;
                case 129: //also raw inline array? in junctions.pso
                    break;
            }

            switch (arrEntry.Type)
            {
                default:
                    ErrorXml(sb, indent, ename + ": WIP! Unsupported array entry DataType: " + arrEntry.Type.ToString());
                    break;
                case PsoDataType.Array:
                    var rk0 = (entry.ReferenceKey >> 16) & 0x0000FFFF;
                    //var rk1 = entry.ReferenceKey & 0x0000FFFF;
                    //var rk3 = (arrEntry.ReferenceKey >> 16) & 0x0000FFFF;
                    //var rk4 = arrEntry.ReferenceKey & 0x0000FFFF;
                    if (rk0 > 0)
                    {
                        aOffset = offset + entry.DataOffset;

                        OpenTag(sb, indent, arrTag);
                        for (int n = 0; n < rk0; n++) //ARRAY ARRAY!
                        {
                            WriteArrayNode(sb, aind, cont, blockId, aOffset, arrEntry, estruct, "Item");

                            aOffset += 16;//ptr size... todo: what if not pointer array?
                        }
                        CloseTag(sb, indent, ename);
                    }
                    else
                    {
                        SelfClosingTag(sb, indent, arrTag);
                    }
                    break;
                case PsoDataType.Structure:
                    switch (arrEntry.Unk_5h)
                    {
                        case 0:
                            break;
                        case 3://structure pointer array
                            MetaTypes.TryConvertData<Array_StructurePointer>(data, eoffset, out var arrStrucPtr);
                            arrStrucPtr = arrStrucPtr.SwapEnd();
                            aBlockId = (int)arrStrucPtr.PointerDataId;
                            aOffset = (int)arrStrucPtr.PointerDataOffset;
                            aCount = arrStrucPtr.Count1;
                            if (aCount > 0)
                            {
                                var ptrArr = PsoTypes.GetPointerArray(cont.Pso, in arrStrucPtr);
                                OpenTag(sb, indent, arrTag);
                                for (int n = 0; n < ptrArr.Length; n++)
                                {
                                    var ptrVal = ptrArr[n];
                                    if (ptrVal.Pointer == 0)
                                    {
                                        SelfClosingTag(sb, aind, "Item"); //"null" entry...
                                    }
                                    else
                                    {
                                        WriteNode(sb, aind, cont, ptrVal.BlockID, (int)ptrVal.ItemOffset, XmlTagMode.ItemAndType);
                                    }
                                }
                                CloseTag(sb, indent, ename);
                            }
                            break;
                        default:
                            break;
                    }
                    arrTag += " itemType=\"" + HashString((MetaName)arrEntry.ReferenceKey) + "\"";
                    if (aCount > 0)
                    {
                        var atyp = cont.GetStructureInfo((MetaName)arrEntry.ReferenceKey);
                        if (!cont.Pso.TryGetBlock(aBlockId, out var aBlock))
                        {
                            ErrorXml(sb, indent, ename + ": Array block not found: " + aBlockId.ToString());
                        }
                        else if (aBlock.NameHash != (MetaName)MetaTypeName.PsoPOINTER)
                        {
                            OpenTag(sb, indent, arrTag);
                            if (atyp == null)
                            {
                                ErrorXml(sb, indent, ename + ": Array type not found: " + HashString((MetaHash)arrEntry.ReferenceKey));
                            }
                            else
                            {
                                for (int n = 0; n < aCount; n++)
                                {
                                    WriteNode(sb, aind, cont, aBlockId, aOffset, XmlTagMode.Item, (MetaName)arrEntry.ReferenceKey);
                                    aOffset += atyp.StructureLength;
                                    if ((n < (aCount - 1)) && (aOffset >= aBlock.Length))
                                    {
                                        break;
                                    }
                                }
                            }
                            CloseTag(sb, indent, ename);
                        }
                        else
                        { } //pointer array should get here, but it's already handled above. should improve this.
                    }
                    else
                    {
                        SelfClosingTag(sb, indent, arrTag);
                    }
                    break;
                case PsoDataType.String:
                    switch (entry.Unk_5h)
                    {
                        default:
                            ErrorXml(sb, indent, $"{ename}: Unexpected String array subtype: {entry.Unk_5h}");
                            break;
                        case 0: //hash array...
                            if (embedded)
                            { }
                            MetaTypes.TryConvertData<Array_uint>(data, eoffset, out var arrHash);
                            arrHash = arrHash.SwapEnd();
                            var hashArr = PsoTypes.GetHashArray(cont.Pso, arrHash);
                            WriteItemArray(sb, hashArr, indent, ename, "Hash", HashString);
                            break;
                    }
                    break;
                case PsoDataType.Float2:
                    aCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
                    arrTag += " itemType=\"Vector2\"";
                    var v2Arr = MetaTypes.ConvertDataArray<Vector2>(data, eoffset, (int)aCount);
                    WriteRawArray(sb, v2Arr, indent, ename, "Vector2", FormatVector2Swap, 1);
                    break;
                case PsoDataType.Float3:
                    if (!embedded)
                    { }
                    arrTag += " itemType=\"Vector3\""; //this is actually aligned as vector4, the W values are crazy in places
                    var v4Arr = MetaTypes.ConvertDataArray<Vector4>(data, eoffset, (int)aCount);
                    WriteRawArray(sb, v4Arr, indent, ename, "Vector3", FormatVector4SwapXYZOnly, 1);
                    break;
                case PsoDataType.UByte:
                    if (embedded)
                    { }
                    else
                    { } //block type 2
                    var barr = new byte[aCount];
                    if (aCount > 0)
                    {
                        //var bblock = cont.Pso.GetBlock(aBlockId);
                        //var boffs = bblock.Offset + aOffset;
                        Buffer.BlockCopy(data, abOffset /*boffs*/, barr, 0, (int)aCount);
                    }
                    WriteRawArray(sb, barr, indent, ename, "byte");
                    break;
                case PsoDataType.Bool:
                    if (embedded)
                    { }
                    else
                    { }
                    var barr2 = new byte[aCount];
                    if (aCount > 0)
                    {
                        //var bblock = cont.Pso.GetBlock(aBlockId);
                        //var boffs = bblock.Offset + aOffset;
                        Buffer.BlockCopy(data, abOffset /*boffs*/, barr2, 0, (int)aCount);
                    }
                    WriteRawArray(sb, barr2, indent, ename, "boolean"); //todo: true/false output
                    break;
                case PsoDataType.Float:
                    if (embedded)
                    { }
                    var arrFloat = new Array_float(arrStruc.Pointer, arrStruc.Count1); //block type 7
                    var floatArr = PsoTypes.GetFloatArray(cont.Pso, in arrFloat);
                    WriteRawArray(sb, floatArr, indent, ename, "float", FloatUtil.ToString);
                    break;
                case PsoDataType.UShort:
                    if (embedded)
                    { }
                    var shortArr = PsoTypes.GetUShortArray(cont.Pso, in arrStruc); //block type 4
                    WriteRawArray(sb, shortArr, indent, ename, "ushort");
                    break;
                case PsoDataType.UInt:
                    if (embedded)
                    { }
                    var arrUint = new Array_uint(arrStruc.Pointer, arrStruc.Count1); //block type 6
                    var intArr = PsoTypes.GetUintArray(cont.Pso, arrUint);
                    WriteRawArray(sb, intArr, indent, ename, "int");
                    break;
                case PsoDataType.SInt:
                    if (embedded)
                    { }
                    var arrUint2 = new Array_uint(arrStruc.Pointer, arrStruc.Count1); //block type 5
                    var intArr2 = PsoTypes.GetUintArray(cont.Pso, arrUint2);
                    WriteRawArray(sb, intArr2, indent, ename, "int");
                    break;
                case PsoDataType.Enum:
                    MetaTypes.TryConvertData<Array_uint>(data, eoffset, out var arrEnum);
                    arrEnum = arrEnum.SwapEnd();
                    var enumArr = PsoTypes.GetUintArray(cont.Pso, arrEnum);
                    var enumDef = cont.GetEnumInfo((MetaName)arrEntry.ReferenceKey);
                    WriteItemArray<uint>(sb, enumArr, indent, ename, "enum", (ie)=> {
                        var eval = enumDef?.FindEntry((int)ie);
                        return HashString(eval?.EntryNameHash ?? 0);
                    });
                    break;
            }

        }

        private static void WriteMapNode(StringBuilder sb, int indent, PsoCont cont, int eoffset, PsoStructureEntryInfo entry, PsoStructureInfo structInfo, string ename)
        {
            var cind = indent + 1;
            var data = cont.Pso.DataSection.Data;
            switch (entry.Unk_5h)
            {
                default:
                    ErrorXml(sb, cind, ename + ": Unexpected Map subtype: " + entry.Unk_5h.ToString());
                    break;
                case 1:
                    var mapidx1 = entry.ReferenceKey & 0x0000FFFF;
                    var mapidx2 = (entry.ReferenceKey >> 16) & 0x0000FFFF;
                    var mapreftype1 = structInfo.Entries[mapidx2];
                    var mapreftype2 = structInfo.Entries[mapidx1];
                    var x1 = MetaTypes.SwapBytes(BitConverter.ToInt32(data, eoffset));
                    var x2 = MetaTypes.SwapBytes(BitConverter.ToInt32(data, eoffset + 4));
                    MetaTypes.TryConvertData<Array_Structure>(data, eoffset + 8, out var sptr);
                    sptr = sptr.SwapEnd();


                    if (x1 != 0x1000000)
                    {
                        var c1 = MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset));
                        var c2 = MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset + 2));
                        var u1 = MetaTypes.SwapBytes(BitConverter.ToUInt32(data, eoffset + 4));
                        var c3 = MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset + 8));
                        var c4 = MetaTypes.SwapBytes(BitConverter.ToUInt16(data, eoffset + 10));
                        var u3 = MetaTypes.SwapBytes(BitConverter.ToUInt32(data, eoffset + 12));
                        ulong ptr = MetaTypes.SwapBytes(BitConverter.ToUInt64(data, eoffset + 16));
                        sptr = new Array_Structure(ptr, c2);
                        if (c3 != 256)
                        { }
                        if (c1 != c2)
                        { }
                    }

                    var xBlockId = (int)sptr.PointerDataId;// x3 & 0xFFF;
                    var xOffset = sptr.PointerDataOffset;// (x3 >> 12) & 0xFFFFF;
                    var xCount1 = sptr.Count1;// x5 & 0xFFFF;

                    if (xCount1 == 0)
                    {
                        SelfClosingTag(sb, cind, ename);
                        break;
                    }

                    var xCount2 = sptr.Count2;// (x5 >> 16) & 0xFFFF;

                    if (!cont.Pso.TryGetBlock(xBlockId, out var xBlock))
                    {
                        ErrorXml(sb, cind, ename + ": Couldn't find Map xBlock: " + xBlockId.ToString());
                    }
                    else
                    {
                        var xStruct = cont.GetStructureInfo(xBlock.NameHash);
                        var xind = indent + 1;
                        var aind = indent + 2;

                        if (xOffset >= xBlock.Length)
                        {
                            xOffset = xOffset >> 8; //how to tell when to do this??
                        }

                        if (xStruct == null && xBlock.NameHash == 0)
                        {
                            SelfClosingTag(sb, cind, ename);
                        }
                        else if (xStruct == null)
                        {
                            ErrorXml(sb, aind, ename + ": Map struct type not found: " + HashString(xBlock.NameHash));
                        }
                        else if (xStruct.IndexInfo.NameHash == 0 && xStruct.IndexInfo.Offset == 0)// || (xStruct.IndexInfo.NameHash != (MetaName)MetaTypeName.ARRAYINFO))
                        {
                            ErrorXml(sb, aind, ename + ": Map struct was missing IndexInfo! " + (xStruct == null ? "" : xStruct.ToString()));
                        }
                        else if (!xStruct.TryFindEntry(MetaName.Key, out var kEntry) || !xStruct.TryFindEntry(MetaName.Item, out var iEntry))
                        {
                            ErrorXml(sb, aind, ename + ": Map Key/Item entries not found!");
                        }
                        else if (kEntry.Type != PsoDataType.String)
                        {
                            ErrorXml(sb, aind, ename + ": Map Key was not a string!");
                        }
                        else if ((iEntry.Type != PsoDataType.Structure) && (iEntry.Type != PsoDataType.String))
                        {
                            ErrorXml(sb, aind, ename + ": Map Item was not a structure or string!");
                        }
                        //else if (iEntry.Unk_5h != 3)
                        //{
                        //    ErrorXml(sb, aind, ename + ": Map Item was not a structure pointer - TODO!");
                        //}
                        else
                        {
                            OpenTag(sb, xind, ename);

                            for (int n = 0; n < xCount1; n++)
                            {
                                if (xOffset >= xBlock.Length)
                                {
                                    ErrorXml(sb, aind, "Offset out of range! Count is " + xCount1.ToString());
                                    break; //out of range...
                                }
                                //WriteNode(sb, aind, cont, xBlockId, xOffset, XmlTagMode.Item, xStruct.IndexInfo.NameHash);

                                int sOffset = (int)xOffset + xBlock.Offset;
                                var kOffset = sOffset + kEntry.DataOffset;
                                var iOffset = sOffset + iEntry.DataOffset;
                                var kStr = GetStringValue(cont.Pso, kEntry, data, kOffset);
                                if (iEntry.Type == PsoDataType.String)
                                {
                                    var iStr = GetStringValue(cont.Pso, iEntry, data, iOffset);
                                    OpenTag(sb, aind, $"Item type=\"String\" key=\"{kStr}\"", false);
                                    sb.Append(XmlEscape(iStr));
                                    CloseTag(sb, 0, "Item");
                                }
                                else if (iEntry.ReferenceKey != 0)//(xBlock.NameHash != (MetaName)MetaTypeName.ARRAYINFO)//257,258,259
                                {
                                    //embedded map values
                                    var vOffset = (int)xOffset + iEntry.DataOffset;
                                    OpenTag(sb, aind, $"Item type=\"{HashString((MetaName)iEntry.ReferenceKey)}\" key=\"{kStr}\"");
                                    WriteNode(sb, aind, cont, xBlockId, vOffset, XmlTagMode.None, (MetaName)iEntry.ReferenceKey);
                                    CloseTag(sb, aind, "Item");
                                }
                                else
                                {
                                    MetaTypes.TryConvertData<PsoPOINTER>(data, iOffset, out var iPtr);
                                    iPtr = iPtr.SwapEnd();
                                    if (!cont.Pso.TryGetBlock(iPtr.BlockID, out var iBlock))
                                    {
                                        ErrorXml(sb, aind, ename + ": Could not find iBlock for Map entry!");
                                    }
                                    else
                                    {
                                        var iStr = $"Item type=\"{HashString(iBlock.NameHash)}\" key=\"{kStr}\"";
                                        var iStruc = cont.GetStructureInfo(iBlock.NameHash);
                                        if (iStruc?.EntriesCount == 0)
                                        {
                                            //SelfClosingTag(sb, aind, iStr);
                                            OpenTag(sb, aind, iStr);
                                            CloseTag(sb, aind, "Item");
                                        }
                                        else
                                        {
                                            var iOff = (int)iPtr.ItemOffset;
                                            OpenTag(sb, aind, iStr);
                                            WriteNode(sb, aind, cont, iPtr.BlockID, iOff, XmlTagMode.None);//, (MetaName)entry.ReferenceKey);
                                            CloseTag(sb, aind, "Item");
                                        }
                                    }
                                }
                                xOffset += (uint)xStruct.StructureLength;
                            }
                            CloseTag(sb, xind, ename);
                        }
                    }
                    break;
            }
        }




        private static string? GetStringValue(PsoFile pso, PsoStructureEntryInfo entry, byte[] data, int eoffset)
        {
            switch (entry.Unk_5h)
            {
                default:
                    return null;
                case 0:
                    var str0len = (int)((entry.ReferenceKey >> 16) & 0xFFFF);
                    return Encoding.ASCII.GetString(data, eoffset, str0len).Replace("\0", "");
                case 1:
                case 2:
                    MetaTypes.TryConvertData<DataBlockPointer>(data, eoffset, out var dataPtr2);
                    dataPtr2 = dataPtr2.SwapEnd();
                    return PsoTypes.GetString(pso, in dataPtr2);
                case 3:
                    MetaTypes.TryConvertData<CharPointer>(data, eoffset, out var charPtr3);
                    charPtr3 = charPtr3.SwapEnd();
                    return PsoTypes.GetString(pso, in charPtr3);
                case 7:
                case 8:
                    MetaTypes.TryConvertData<MetaHash>(data, eoffset, out var rawVal);
                    var hashVal = (MetaHash)MetaTypes.SwapBytes(rawVal);
                    return HashString(hashVal);
            }

        }


        public class PsoCont
        {
            public PsoFile Pso { get; set; }

            public Dictionary<MetaName, PsoEnumInfo> EnumDict = new Dictionary<MetaName, PsoEnumInfo>();
            public Dictionary<MetaName, PsoStructureInfo> StructDict = new Dictionary<MetaName, PsoStructureInfo>();


            public PsoCont(PsoFile pso)
            {
                Pso = pso;

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
                            //PsoEnumInfo oldei = EnumDict[enuminfo.IndexInfo.NameHash];
                            //if (!ComparePsoEnumInfos(oldei, enuminfo))
                            //{
                            //}
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
                            //PsoStructureInfo oldsi = StructDict[structinfo.IndexInfo.NameHash];
                            //if (!ComparePsoStructureInfos(oldsi, structinfo))
                            //{
                            //}
                        }
                    }

                }
            }


            public PsoStructureInfo GetStructureInfo(MetaName name)
            {
                StructDict.TryGetValue(name, out PsoStructureInfo i);
                return i;
            }
            public PsoEnumInfo GetEnumInfo(MetaName name)
            {
                EnumDict.TryGetValue(name, out PsoEnumInfo i);
                return i;
            }

        }

    }

    public class RbfXml : MetaXmlBase
    {

        public static string GetXml(RbfFile rbf)
        {
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.AppendLine(XmlHeader);

                WriteNode(sb, 0, rbf.current);

                return sb.ToString();
            } finally
            {
                StringBuilderPool.Return(sb);
            }

        }

        private static void WriteNode(StringBuilder sb, int indent, RbfStructure rs)
        {
            var attStr = "";
            if (rs.Attributes is not null && rs.Attributes.Count > 0)
            {
                var asb = StringBuilderPool.Get();
                try
                {
                    foreach (var attr in rs.Attributes)
                    {
                        if (attr is RbfString str)
                        {
                            asb.Append($" {attr.Name}=\"{str.Value}\"");
                        }
                        else if (attr is RbfFloat flt)
                        {
                            asb.Append($" {attr.Name}=\"{FloatUtil.ToString(flt.Value)}\"");
                        }
                        else if (attr is RbfUint32 unt)
                        {
                            asb.Append($" {attr.Name}=\"{unt.Value}\"");
                        }
                        else if (attr is RbfBoolean bln)
                        {
                            asb.Append($" {attr.Name}=\"{bln.Value}\"");
                        }
                    }
                    attStr = $"{asb}";
                }
                finally
                {
                    StringBuilderPool.Return(asb);
                }
            }

            if (rs.Children is null || rs.Children.Count == 0)
            {
                SelfClosingTag(sb, indent, rs.Name + attStr);
                return;
            }

            int cind = indent + 1;

            bool oneline = ((rs.Children.Count == 1) && (rs.Children[0].Name == null) && (rs.Attributes.Count == 0));

            OpenTag(sb, indent, rs.Name + attStr, !oneline);


            foreach (var child in rs.Children)
            {
                if (child is RbfBytes)
                {
                    var bytesChild = (RbfBytes)child;
                    //TODO: fix this to output nicer XML!
                    if (rs.FindAttribute("content") is RbfString contentField)
                    {
                        if (contentField.Value == "char_array")
                        {
                            foreach (byte k in bytesChild.Value)
                            {
                                Indent(sb, cind);
                                sb.AppendLine(k.ToString());
                            }
                        }
                        else if (contentField.Value.Equals("short_array"))
                        {
                            var valueReader = new DataReader(new MemoryStream(bytesChild.Value));
                            while (valueReader.Position < valueReader.Length)
                            {
                                Indent(sb, cind);
                                var y = valueReader.ReadUInt16();
                                sb.AppendLine(y.ToString());
                            }
                        }
                        else
                        {
                            ErrorXml(sb, cind, $"Unexpected content type: {contentField.Value}");
                        }
                    }
                    else
                    {
                        string stringValue = Encoding.ASCII.GetString(bytesChild.Value);
                        string str = stringValue.Substring(0, stringValue.Length - 1); //removes null terminator
                        sb.Append(str);
                    }
                }
                if (child is RbfFloat)
                {
                    var floatChild = (RbfFloat)child;
                    ValueTag(sb, cind, child.Name, FloatUtil.ToString(floatChild.Value));
                }
                if (child is RbfString)
                {
                    ////// this doesn't seem to be used! it's always using RbfBytes child...

                    var stringChild = (RbfString)child;
                    StringTag(sb, cind, stringChild.Name, stringChild.Value);

                    //if (stringChild.Name.Equals("content"))
                    //else if (stringChild.Name.Equals("type"))
                    //else throw new Exception("Unexpected string content");
                }
                if (child is RbfStructure rbfStructure)
                {
                    WriteNode(sb, cind, rbfStructure);
                }
                if (child is RbfUint32 intChild)
                {
                    ValueTag(sb, cind, intChild.Name, UintString(intChild.Value));
                }
                if (child is RbfBoolean booleanChild)
                {
                    ValueTag(sb, cind, booleanChild.Name, booleanChild.Value.ToString());
                }
                if (child is RbfFloat3 v3)
                {
                    SelfClosingTag(sb, cind, $"{v3.Name} x=\"{FloatUtil.ToString(v3.X)}\" y=\"{FloatUtil.ToString(v3.Y)}\" z=\"{FloatUtil.ToString(v3.Z)}\"");
                }


            }

            CloseTag(sb, oneline ? 0 : indent, rs.Name);
        }



    }

    public class MetaXmlBase
    {
        public static ObjectPool<StringBuilder> StringBuilderPool = ObjectPool.Create<StringBuilder>(new StringBuilderPooledObjectPolicy { MaximumRetainedCapacity = 4 * 1024 * 1024, InitialCapacity = 1024 * 32 });
        public const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";



        public static void Indent(StringBuilder sb, int indent)
        {
            sb.Append(' ', indent);
        }

        public static void ErrorXml(StringBuilder sb, int indent, string msg)
        {
            Indent(sb, indent);
            sb.AppendLine($"<error>{msg}</error>");
        }

        public static void OpenTag(StringBuilder sb, int indent, string name, bool appendLine = true)
        {
            Indent(sb, indent);
            sb.Append('<');
            sb.Append(name);
            sb.Append('>');
            if (appendLine)
                sb.AppendLine();
        }

        public static void OpenTag(StringBuilder sb, int indent, string name, string metaName, bool appendLine = true)
        {
            Indent(sb, indent);
            sb.Append('<');
            sb.Append(name);
            if (string.IsNullOrWhiteSpace(metaName))
            {
                sb.Append('>');
            }
            else
            {
                sb.Append($" name=\"{metaName}\">");
            }
            if (appendLine) sb.AppendLine();
        }

        //public static void OpenTag(StringBuilder sb, int indent, string name, bool appendLine = true, string metaName = "")
        //{
        //    Indent(sb, indent);
        //    sb.Append('<');
        //    sb.Append(name);
        //    if (string.IsNullOrWhiteSpace(metaName))
        //    {
        //        sb.Append('>');
        //    }
        //    else
        //    {
        //        sb.Append($" name=\"{metaName}\">");
        //    }
        //    if (appendLine) sb.AppendLine();
        //}
        public static void CloseTag(StringBuilder sb, int indent, string name, bool appendLine = true)
        {
            Indent(sb, indent);
            sb.Append($"</{name}>");
            if (appendLine) sb.AppendLine();
        }
        public static void ValueTag(StringBuilder sb, int indent, string name, string val, string attr = "value")
        {
            Indent(sb, indent);
            sb.AppendLine($"<{name} {attr}=\"{val}\" />");
        }
        public static void OneLineTag(StringBuilder sb, int indent, string name, string text)
        {
            Indent(sb, indent);
            sb.AppendLine($"<{name}>{text}</{name}>");
        }
        public static void SelfClosingTag(StringBuilder sb, int indent, string val)
        {
            Indent(sb, indent);
            sb.AppendLine($"<{val} />");
        }
        public static void StringTag(StringBuilder sb, int indent, string name, string text)
        {
            if (!string.IsNullOrEmpty(text)) OneLineTag(sb, indent, name, text);
            else SelfClosingTag(sb, indent, name);
        }

        public static void WriteRawArrayContent<T>(StringBuilder sb, T[]? arr, int ind, Func<T, string> formatter = null, int arrRowSize = 10) where T : struct
        {
            if (arr is null || arr.Length == 0)
                return;

            var aCount = arr.Length;
            for (int n = 0; n < aCount; n++)
            {
                ref var value = ref arr[n];
                var col = n % arrRowSize;
                if (col == 0) Indent(sb, ind);
                if (col > 0) sb.Append(' ');
                string? str = formatter?.Invoke(value) ?? value.ToString();
                sb.Append(str);
                bool lastcol = (col == (arrRowSize - 1));
                bool lastn = (n == (aCount - 1));
                if (lastcol || lastn) sb.AppendLine();
            }
        }

        public static void WriteRawArray<T>(StringBuilder sb, Span<T> arr, int ind, string name, string typeName, FormatterRef<T>? formatter = null, int arrRowSize = 10) where T : struct
        {
            var aCount = arr.Length;
            //var arrRowSize = 10;
            var aind = ind + 1;
            var arrTag = name;// + " itemType=\"" + typeName + "\"";
            if (aCount > 0)
            {
                if (aCount <= arrRowSize)
                {
                    OpenTag(sb, ind, arrTag, false);
                    for (int n = 0; n < aCount; n++)
                    {
                        if (n > 0) sb.Append(' ');
                        string? str = formatter?.Invoke(in arr[n]) ?? arr[n].ToString();
                        sb.Append(str);
                    }
                    CloseTag(sb, 0, name);
                }
                else
                {
                    OpenTag(sb, ind, arrTag);
                    for (int n = 0; n < aCount; n++)
                    {
                        var col = n % arrRowSize;
                        if (col == 0) Indent(sb, aind);
                        if (col > 0) sb.Append(' ');
                        string? str = formatter?.Invoke(in arr[n]) ?? arr[n].ToString();
                        sb.Append(str);
                        bool lastcol = (col == (arrRowSize - 1));
                        bool lastn = (n == (aCount - 1));
                        if (lastcol || lastn) sb.AppendLine();
                    }
                    CloseTag(sb, ind, name);
                }
            }
            else
            {
                SelfClosingTag(sb, ind, arrTag);
            }
        }

        public delegate string FormatterRef<T>(in T y);

        public delegate string Formatter<T>(T y);

        public static void WriteRawArray<T>(StringBuilder sb, T[] arr, int ind, string name, string typeName, FormatterRef<T>? formatter = null, int arrRowSize = 10) where T : struct
        {
            WriteRawArray<T>(sb, arr.AsSpan(), ind, name, typeName, formatter, arrRowSize);
        }

        public static void WriteRawArray<T>(StringBuilder sb, T[] arr, int ind, string name, string typeName, Func<T, string> formatter, int arrRowSize = 10) where T : struct
        {
            WriteRawArray<T>(sb, arr.AsSpan(), ind, name, typeName, (in T value) => formatter(value), arrRowSize);
        }

        public static void WriteRawArray<T>(StringBuilder sb, Span<T> arr, int ind, string name, string typeName) where T : struct
        {
            WriteRawArray<T>(sb, arr, ind, name, typeName);
        }

        public static void WriteItemArray<T>(StringBuilder sb, T[]? arr, int ind, string name, string typeName, FormatterRef<T> formatter) where T : struct
        {
            WriteItemArray<T>(sb, arr.AsSpan(), ind, name, typeName, formatter);
        }

        public static void WriteItemArray<T>(StringBuilder sb, T[]? arr, int ind, string name, string typeName, Func<T, string> formatter) where T : struct
        {
            WriteItemArray<T>(sb, arr.AsSpan(), ind, name, typeName, (in T value) => formatter(value));
        }

        public static void WriteItemArray<T>(StringBuilder sb, Span<T> arr, int ind, string name, string typeName, FormatterRef<T> formatter) where T : struct
        {
            var itemCount = arr.Length;
            var arrTag = name;// + " itemType=\"Hash\"";
            var aind = ind + 1;
            if (itemCount > 0)
            {
                OpenTag(sb, ind, arrTag, typeName);
                for (int n = 0; n < itemCount; n++)
                {
                    Indent(sb, aind);
                    sb.AppendLine($"<Item>{formatter(arr[n])}</Item>");
                }
                CloseTag(sb, ind, name);
            }
            else
            {
                SelfClosingTag(sb, ind, arrTag);
            }
        }
        public static void WriteItemArray<T>(StringBuilder sb, T[]? arr, int ind, string name) where T : IMetaXmlItem
        {
            if (arr is null || arr.Length == 0)
            {
                SelfClosingTag(sb, ind, name);
                return;
            }

            OpenTag(sb, ind, name);
            var cind = ind + 1;
            var cind2 = ind + 2;
            foreach(var item in arr)
            {
                if (item is null)
                {
                    SelfClosingTag(sb, cind, "Item");
                    continue;
                }

                OpenTag(sb, cind, "Item");
                item.WriteXml(sb, cind2);
                CloseTag(sb, cind, "Item");
            }
            CloseTag(sb, ind, name);
        }
        public static void WriteCustomItemArray<T>(StringBuilder sb, T[]? arr, int ind, string name) where T : IMetaXmlItem
        {
            if (arr is null || arr.Length == 0)
            {
                SelfClosingTag(sb, ind, name);
                return;
            }

            OpenTag(sb, ind, name);
            var cind = ind + 1;
            foreach(var item in arr)
            {
                item?.WriteXml(sb, cind);
            }
            CloseTag(sb, ind, name);
        }
        public static void WriteHashItemArray(StringBuilder sb, MetaHash[]? arr, int ind, string name)
        {
            if (arr is null || arr.Length == 0)
            {
                SelfClosingTag(sb, ind, name);
                return;
            }

            OpenTag(sb, ind, name);
            var cind = ind + 1;
            foreach(var item in arr)
            {
                var iname = HashString(item);
                StringTag(sb, cind, "Item", iname);
            }
            CloseTag(sb, ind, name);
        }

        public static string FormatHash(MetaHash h) //for use with WriteItemArray
        {
            
            if (JenkIndex.TryGetString(h, out var str))
                return str;

            if (GlobalText.TryGetString(h, out str))
                return str;

            return HashString(h);// "hash_" + h.Hex;
            //return h.ToString();
        }
        public static string FormatVector2(in Vector2 v) //for use with WriteItemArray
        {
            return FloatUtil.GetVector2String(v);
        }
        public static string FormatVector3(in Vector3 v) //for use with WriteItemArray
        {
            return FloatUtil.GetVector3String(in v);
        }
        public static string FormatVector4(in Vector4 v) //for use with WriteItemArray
        {
            return FloatUtil.GetVector4String(in v);
        }
        public static string FormatHexByte(byte b)
        {
            return b.ToString("X2");
        }

        public static string FormatHashSwap(MetaHash h) //for use with WriteItemArray, swaps endianness
        {
            return MetaTypes.SwapBytes(h).ToString();
        }
        public static string FormatVector2Swap(in Vector2 v) //for use with WriteItemArray, swaps endianness
        {
            return FloatUtil.GetVector2String(MetaTypes.SwapBytes(in v));
        }
        public static string FormatVector3Swap(Vector3 v) //for use with WriteItemArray, swaps endianness
        {
            return FloatUtil.GetVector3String(MetaTypes.SwapBytes(in v));
        }
        public static string FormatVector4Swap(Vector4 v) //for use with WriteItemArray, swaps endianness
        {
            return FloatUtil.GetVector4String(MetaTypes.SwapBytes(in v));
        }
        public static string FormatVector4SwapXYZOnly(in Vector4 v) //for use with WriteItemArray, swaps endianness, and outputs only XYZ components
        {
            return FloatUtil.GetVector3String(MetaTypes.SwapBytes(v.XYZ()));
        }



        public static string HashString(MetaName h)
        {
            uint uh = (uint)h;
            if (uh == 0)
                return string.Empty;

            string? str;
            if (MetaNames.TryGetString(uh, out str))
                return str;

            str = JenkIndex.TryGetString(uh);
            if (!string.IsNullOrEmpty(str))
                return str;

            //TODO: do extra hash lookup here
            //if(Lookup.TryGetValue(uh, out str)) ...

            return $"hash_{uh:X8}";

        }
        public static string HashString(MetaHash h)
        {
            if (h == 0)
                return string.Empty;

            if (JenkIndex.TryGetString(h, out var str))
                return str;

            if (MetaNames.TryGetString(h, out str))
                return str;

            //todo: make sure JenkIndex is built!
            //todo: do extra hash lookup here


            if (!string.IsNullOrEmpty(str))
                return str;

            return $"hash_{h.Hex}";
        }
        public static string HashString(TextHash h)
        {
            uint uh = h.Hash;
            if (uh == 0)
                return string.Empty;

            if (GlobalText.TryGetString(uh, out var str))
                return str;

            //TODO: do extra hash lookup here
            //if(Lookup.TryGetValue(uh, out str)) ...

            return $"hash_{uh:X8}";
        }


        public static string UintString(uint h)
        {
            //string str;
            //if (MetaNames.TryGetString(h, out str)) return str;

            //str = JenkIndex.TryGetString(h);
            //if (!string.IsNullOrEmpty(str)) return str;

            ////TODO: do extra hash lookup here
            ////if(Lookup.TryGetValue(uh, out str)) ...


            //if (h == 0) return "";
            return $"0x{h:X}";

        }



        [return: NotNullIfNotNull(nameof(unescaped))]
        public static string? XmlEscape(string? unescaped)
        {
            if (unescaped is null)
                return null;
            var escaped = System.Web.HttpUtility.HtmlEncode(unescaped).Replace("\"", "&quot;");
            return escaped;
        }




        public enum XmlTagMode
        {
            None = 0,
            Structure = 1,
            Item = 2,
            ItemAndType = 3,
        }
    }

    public interface IMetaXmlItem
    {
        void WriteXml(StringBuilder sb, int indent);
        void ReadXml(XmlNode node);
    }



    public enum MetaFormat
    {
        XML = 0,
        RSC = 1,
        PSO = 2,
        RBF = 3,
        CacheFile = 4,
        AudioRel = 5,
        Ynd = 6,
        Ynv = 7,
        Ycd = 8,
        Ybn = 9,
        Ytd = 10,
        Ydr = 11,
        Ydd = 12,
        Yft = 13,
        Ypt = 14,
        Yld = 15,
        Yed = 16,
        Ywr = 17,
        Yvr = 18,
        Awc = 19,
        Fxc = 20,
        Heightmap = 21,
        Ypdb = 22,
        Mrf = 23,
        Yfd = 24,
    }

}
