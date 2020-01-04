using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class XmlPso
    {

        public static PsoFile GetPso(XmlDocument doc)
        {
            PsoBuilder pb = new PsoBuilder();

            Traverse(doc.DocumentElement, pb, 0, true);

            var pso = pb.GetPso();

            return pso;
        }



        private static byte[] Traverse(XmlNode node, PsoBuilder pb, MetaName type = 0, bool isRoot = false)
        {
            if (type == 0)
            {
                type = (MetaName)(uint)GetHash(node.Name);
            }

            var infos = PsoTypes.GetStructureInfo(type);
            if (infos != null)
            {
                byte[] data = new byte[infos.StructureLength];
                var arrayResults = new PsoArrayResults();

                arrayResults.Structures = new Dictionary<int, Array_Structure>();
                arrayResults.StructurePointers = new Dictionary<int, Array_StructurePointer>();
                arrayResults.UInts = new Dictionary<int, Array_uint>();
                arrayResults.UShorts = new Dictionary<int, Array_ushort>();
                arrayResults.UBytes = new Dictionary<int, Array_byte>();
                arrayResults.Floats = new Dictionary<int, Array_float>();
                arrayResults.Float_XYZs = new Dictionary<int, Array_Vector3>();
                arrayResults.Hashes = new Dictionary<int, Array_uint>();

                Array.Clear(data, 0, infos.StructureLength); //shouldn't really be necessary...

                PsoStructureEntryInfo arrEntry = null;


                //if (isRoot)
                //{
                //    pb.EnsureBlock(type);
                //}

                for (int i = 0; i < infos.Entries.Length; i++)
                {
                    var entry = infos.Entries[i];
                    var cnode = GetEntryNode(node.ChildNodes, entry.EntryNameHash);

                    if (entry.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO)
                    {
                        arrEntry = entry;
                        continue;
                    }

                    if (cnode == null)
                    {
                        //warning: node not found in XML for this entry!
                        continue;
                    }

                    switch (entry.Type)
                    {
                        case PsoDataType.Array:
                            {
                                TraverseArray(cnode, pb, entry, arrEntry, arrayResults, data, infos);
                                break;
                            }
                        case PsoDataType.Structure:
                            {
                                var stype = (MetaName)entry.ReferenceKey;
                                if (stype == 0)
                                {
                                    var stypestr = Xml.GetStringAttribute(cnode, "type");
                                    if (!string.IsNullOrEmpty(stypestr))
                                    {
                                        stype = (MetaName)(uint)GetHash(stypestr);
                                    }
                                }
                                var struc = Traverse(cnode, pb, stype);
                                if (struc != null)
                                {
                                    switch (entry.Unk_5h)
                                    {
                                        default:
                                            //ErrorXml(sb, cind, ename + ": Unexpected Structure subtype: " + entry.Unk_5h.ToString());
                                            break;
                                        case 0: //default structure

                                            Buffer.BlockCopy(struc, 0, data, entry.DataOffset, struc.Length);

                                            break;
                                        case 3: //structure pointer...
                                        case 4: //also pointer? what's the difference?

                                            var bptr = pb.AddItem(stype, struc);
                                            var ptr = new PsoPOINTER(bptr.BlockID, bptr.Offset);
                                            ptr.SwapEnd();
                                            var ptrb = MetaTypes.ConvertToBytes(ptr);

                                            Buffer.BlockCopy(ptrb, 0, data, entry.DataOffset, ptrb.Length);

                                            break;
                                    }
                                }
                                else
                                { }
                                break;
                            }
                        case PsoDataType.Map:
                            {
                                TraverseMap(cnode, pb, entry, infos, data, arrayResults);

                                break;
                            }

                        case PsoDataType.Bool:
                            {
                                byte val = (cnode.Attributes["value"].Value == "false") ? (byte)0 : (byte)1;
                                data[entry.DataOffset] = val;
                                break;
                            }
                        case PsoDataType.SByte:
                            {
                                var val = Convert.ToSByte(cnode.Attributes["value"].Value);
                                data[entry.DataOffset] = (byte)val;
                                break;
                            }
                        case PsoDataType.UByte:
                            {
                                var val = Convert.ToByte(cnode.Attributes["value"].Value);
                                data[entry.DataOffset] = val;
                                break;
                            }
                        case PsoDataType.SShort:
                            {
                                var val = Convert.ToInt16(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }
                        case PsoDataType.UShort:
                            {
                                var val = Convert.ToUInt16(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }
                        case PsoDataType.SInt:
                            {
                                var val = Convert.ToInt32(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }
                        case PsoDataType.UInt:
                            {
                                switch (entry.Unk_5h)
                                {
                                    default:
                                        //ErrorXml(sb, cind, ename + ": Unexpected Integer subtype: " + entry.Unk_5h.ToString());
                                        break;
                                    case 0: //signed int (? flags?)
                                        var sval = Convert.ToInt32(cnode.Attributes["value"].Value);
                                        Write(sval, data, entry.DataOffset);
                                        break;
                                    case 1: //unsigned int
                                        var ustr = cnode.Attributes["value"].Value;
                                        uint uval = 0;
                                        if (ustr.StartsWith("0x"))
                                        {
                                            ustr = ustr.Substring(2);
                                            uval = Convert.ToUInt32(ustr, 16);
                                        }
                                        else
                                        {
                                            uval = Convert.ToUInt32(ustr);
                                        }
                                        Write(uval, data, entry.DataOffset);
                                        break;
                                }

                                break;
                            }
                        case PsoDataType.Float:
                            {
                                float val = FloatUtil.Parse(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }
                        case PsoDataType.Float2:
                            {
                                float x = FloatUtil.Parse(cnode.Attributes["x"].Value);
                                float y = FloatUtil.Parse(cnode.Attributes["y"].Value);
                                Write(x, data, entry.DataOffset);
                                Write(y, data, entry.DataOffset + sizeof(float));
                                break;
                            }
                        case PsoDataType.Float3:
                            {
                                float x = FloatUtil.Parse(cnode.Attributes["x"].Value);
                                float y = FloatUtil.Parse(cnode.Attributes["y"].Value);
                                float z = FloatUtil.Parse(cnode.Attributes["z"].Value);
                                Write(x, data, entry.DataOffset);
                                Write(y, data, entry.DataOffset + sizeof(float));
                                Write(z, data, entry.DataOffset + sizeof(float) * 2);
                                break;
                            }
                        case PsoDataType.Float4:
                            {
                                float x = FloatUtil.Parse(cnode.Attributes["x"].Value);
                                float y = FloatUtil.Parse(cnode.Attributes["y"].Value);
                                float z = FloatUtil.Parse(cnode.Attributes["z"].Value);
                                float w = FloatUtil.Parse(cnode.Attributes["w"].Value);
                                Write(x, data, entry.DataOffset);
                                Write(y, data, entry.DataOffset + sizeof(float));
                                Write(z, data, entry.DataOffset + sizeof(float) * 2);
                                Write(w, data, entry.DataOffset + sizeof(float) * 3);
                                break;
                            }
                        case PsoDataType.String:
                            {
                                TraverseString(cnode, pb, entry, data);
                                break;
                            }
                        case PsoDataType.Enum:
                            {
                                pb.AddEnumInfo((MetaName)entry.ReferenceKey);
                                switch (entry.Unk_5h)
                                {
                                    default:
                                        //ErrorXml(sb, cind, ename + ": Unexpected Enum subtype: " + entry.Unk_5h.ToString());
                                        break;
                                    case 0: //int enum
                                        int ival = GetEnumInt((MetaName)entry.ReferenceKey, cnode.InnerText, entry.Type);
                                        Write(ival, data, entry.DataOffset);
                                        break;
                                    case 1: //short enum?
                                        short sval = (short)GetEnumInt((MetaName)entry.ReferenceKey, cnode.InnerText, entry.Type);
                                        Write(sval, data, entry.DataOffset);
                                        break;
                                    case 2: //byte enum
                                        byte bval = (byte)GetEnumInt((MetaName)entry.ReferenceKey, cnode.InnerText, entry.Type);
                                        data[entry.DataOffset] = bval;
                                        break;
                                }
                                break;
                            }
                        case PsoDataType.Flags:
                            {
                                //uint fCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
                                uint fEntry = (entry.ReferenceKey & 0xFFF);
                                var fEnt = (fEntry != 0xFFF) ? infos.GetEntry((int)fEntry) : null;
                                PsoEnumInfo flagsInfo = null;
                                MetaName fEnum = (MetaName)(fEnt?.ReferenceKey ?? 0);
                                if ((fEnt != null) && (fEnt.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO))
                                {
                                    flagsInfo = PsoTypes.GetEnumInfo(fEnum);
                                }
                                if (flagsInfo == null)
                                {
                                    if (fEntry != 0xFFF)
                                    { }
                                    //flagsInfo = PsoTypes.GetEnumInfo(entry.EntryNameHash);
                                }
                                if (flagsInfo != null)
                                {
                                    pb.AddEnumInfo(flagsInfo.IndexInfo.NameHash);
                                }
                                else
                                { }//error?

                                switch (entry.Unk_5h)
                                {
                                    default:
                                        //ErrorXml(sb, cind, ename + ": Unexpected Flags subtype: " + entry.Unk_5h.ToString());
                                        break;
                                    case 0: //int flags
                                        int ival = GetEnumInt(fEnum, cnode.InnerText, entry.Type);
                                        Write(ival, data, entry.DataOffset);
                                        break;
                                    case 1: //short flags
                                        short sval = (short)GetEnumInt(fEnum, cnode.InnerText, entry.Type);
                                        Write(sval, data, entry.DataOffset);
                                        break;
                                    case 2: //byte flags
                                        byte bval = (byte)GetEnumInt(fEnum, cnode.InnerText, entry.Type);
                                        data[entry.DataOffset] = bval;
                                        break;
                                }
                                break;
                            }
                        case PsoDataType.Float3a:
                            {
                                float x = FloatUtil.Parse(cnode.Attributes["x"].Value);
                                float y = FloatUtil.Parse(cnode.Attributes["y"].Value);
                                float z = FloatUtil.Parse(cnode.Attributes["z"].Value);
                                Write(x, data, entry.DataOffset);
                                Write(y, data, entry.DataOffset + sizeof(float));
                                Write(z, data, entry.DataOffset + sizeof(float) * 2);
                                break;
                            }
                        case PsoDataType.Float4a:
                            {
                                float x = FloatUtil.Parse(cnode.Attributes["x"].Value);
                                float y = FloatUtil.Parse(cnode.Attributes["y"].Value);
                                float z = FloatUtil.Parse(cnode.Attributes["z"].Value);
                                //float w = FloatUtil.Parse(cnode.Attributes["w"].Value);
                                Write(x, data, entry.DataOffset);
                                Write(y, data, entry.DataOffset + sizeof(float));
                                Write(z, data, entry.DataOffset + sizeof(float) * 2);
                                //Write(w, data, entry.DataOffset + sizeof(float) * 3);
                                break;
                            }
                        case PsoDataType.HFloat:
                            {
                                var val = Convert.ToInt16(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }
                        case PsoDataType.Long:
                            {
                                var uval = Convert.ToUInt64(cnode.Attributes["value"].Value);
                                Write(uval, data, entry.DataOffset);
                                break;
                            }


                        default:
                            break;

                    }


                }


                arrayResults.WriteArrays(data);

                pb.AddStructureInfo(infos.IndexInfo.NameHash);

                if (isRoot)
                {
                    pb.RootPointer = pb.AddItem(type, data);
                }

                return data;

            }
            else
            { }//info not found

            return null;
        }

        private static void TraverseMap(XmlNode node, PsoBuilder pb, PsoStructureEntryInfo entry, PsoStructureInfo infos, byte[] data, PsoArrayResults arrayResults)
        {
            var mapidx1 = entry.ReferenceKey & 0x0000FFFF;
            var mapidx2 = (entry.ReferenceKey >> 16) & 0x0000FFFF;
            var mapreftype1 = infos.Entries[mapidx2];
            var mapreftype2 = infos.Entries[mapidx1];

            if (mapreftype2.ReferenceKey != 0)
            { }

            var xStruct = pb.AddMapNodeStructureInfo((MetaName)mapreftype2.ReferenceKey);
            var xName = xStruct.IndexInfo.NameHash;
            var kEntry = xStruct?.FindEntry(MetaName.Key);
            var iEntry = xStruct?.FindEntry(MetaName.Item);

            if (kEntry.Type != PsoDataType.String)
            { }



            List<byte[]> nodesData = new List<byte[]>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var kattr = cnode.Attributes["key"].Value;
                var tattr = cnode.Attributes["type"].Value;//CW invention for convenience..!
                var khash = (MetaName)(uint)GetHash(kattr);
                var thash = (MetaName)(uint)GetHash(tattr);

                byte[] strucBytes = Traverse(cnode, pb, thash);
                byte[] nodeBytes = new byte[xStruct.StructureLength];

                TraverseStringRaw(kattr, pb, kEntry, nodeBytes); //write the key

                if (xName != (MetaName)MetaTypeName.ARRAYINFO)// (mapreftype2.ReferenceKey != 0)
                {
                    //value struct embedded in ARRAYINFO node
                    Buffer.BlockCopy(strucBytes, 0, nodeBytes, iEntry.DataOffset, strucBytes.Length);
                }
                else
                {
                    //normal ARRAYINFO with pointer value
                    var itemptr = pb.AddItemPtr(thash, strucBytes);
                    itemptr.SwapEnd(); //big schmigg
                    var ptrbytes = MetaTypes.ConvertToBytes(itemptr);
                    Buffer.BlockCopy(ptrbytes, 0, nodeBytes, iEntry.DataOffset, ptrbytes.Length);
                }

                nodesData.Add(nodeBytes);

            }



            Write(0x1000000, data, entry.DataOffset);
            Write(0, data, entry.DataOffset + 4);

            arrayResults.Structures[entry.DataOffset + 8] = pb.AddItemArrayPtr(xName, nodesData.ToArray());  //pb.AddPointerArray(nodeptrs);
        }

        private static void TraverseArray(XmlNode node, PsoBuilder pb, PsoStructureEntryInfo entry, PsoStructureEntryInfo arrEntry, PsoArrayResults results, byte[] data, PsoStructureInfo structInfo)
        {
            int offset = entry.DataOffset;
            uint aCount = (entry.ReferenceKey >> 16) & 0x0000FFFF;
            uint aPtr = (entry.ReferenceKey) & 0x0000FFFF;
            byte[] adata = null;

            //how do we know when it's an "embedded" array?
            bool embedded = true;
            switch (entry.Unk_5h)
            {
                default:
                    //ErrorXml(sb, indent, ename + ": WIP! Unsupported Array subtype: " + entry.Unk_5h.ToString());
                    break;
                case 0: //Array_Structure
                    //var arrStruc = MetaTypes.ConvertData<Array_Structure>(data, eoffset);
                    embedded = false;
                    break;
                case 1: //Raw in-line array
                    break;
                case 2: //also raw in-line array, but how different from above?
                    break;
                case 4: //pointer array? default array?
                    if (arrEntry.Unk_5h == 3) //pointers...
                    {
                        //var arrStruc4 = MetaTypes.ConvertData<Array_Structure>(data, eoffset);
                        embedded = false;
                    }
                    else
                    {
                    }
                    break;
                case 129: //also raw inline array? in junctions.pso  (AutoJunctionAdjustments)
                    break;
            }




            switch (arrEntry.Type)
            {
                case PsoDataType.Structure:
                    {
                        if (embedded)
                        {
                            if (arrEntry.ReferenceKey != 0)
                            {
                                var datas = TraverseArrayStructureRaw(node, pb, (MetaName)arrEntry.ReferenceKey);
                                int aoffset = offset;
                                for (int i = 0; i < datas.Length; i++)
                                {
                                    Buffer.BlockCopy(datas[i], 0, data, aoffset, datas[i].Length);
                                    aoffset += datas[i].Length;
                                }
                            }
                            else
                            {
                                var ptrs = TraverseArrayStructurePointerRaw(node, pb);
                                adata = MetaTypes.ConvertArrayToBytes(ptrs);
                            }
                        }
                        else
                        {
                            if (arrEntry.ReferenceKey != 0)
                            {
                                results.Structures[offset] = TraverseArrayStructure(node, pb, (MetaName)arrEntry.ReferenceKey);
                            }
                            else
                            {
                                results.StructurePointers[offset] = TraverseArrayStructurePointer(node, pb);
                            }
                        }
                        break;
                    }

                case PsoDataType.Float2:
                    {
                        var arr = TraverseVector2ArrayRaw(node);
                        if (embedded)
                        {
                            adata = MetaTypes.ConvertArrayToBytes(arr);
                            aCount *= 8;
                        }
                        else
                        {
                            results.Float_XYZs[offset] = pb.AddVector2ArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.Float3:
                    {
                        var arr = TraverseVector3ArrayRaw(node);
                        if (embedded)
                        {
                            adata = MetaTypes.ConvertArrayToBytes(arr);
                            aCount *= 16;
                        }
                        else
                        {
                            results.Float_XYZs[offset] = pb.AddPaddedVector3ArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.UByte:
                    {
                        var arr = TraverseUByteArrayRaw(node);
                        if (embedded)
                        {
                            adata = arr;
                        }
                        else
                        {
                            results.UBytes[offset] = pb.AddByteArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.Bool:
                    {
                        var arr = TraverseUByteArrayRaw(node);
                        if (embedded)
                        {
                            adata = arr;
                        }
                        else
                        {
                            results.UBytes[offset] = pb.AddByteArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.UInt:
                    {
                        var arr = TraverseUIntArrayRaw(node);
                        if (embedded)
                        {
                            adata = MetaTypes.ConvertArrayToBytes(arr);
                            aCount *= 4;
                        }
                        else
                        {
                            results.UInts[offset] = pb.AddUIntArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.SInt:
                    {
                        var arr = TraverseSIntArrayRaw(node);
                        if (embedded)
                        {
                            adata = MetaTypes.ConvertArrayToBytes(arr);
                            aCount *= 4;
                        }
                        else
                        {
                            results.UInts[offset] = pb.AddSIntArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.Float:
                    {
                        var arr = TraverseFloatArrayRaw(node);
                        if (embedded)
                        {
                            adata = MetaTypes.ConvertArrayToBytes(arr);
                            aCount *= 4;
                        }
                        else
                        {
                            results.Floats[offset] = pb.AddFloatArrayPtr(arr);
                        }
                        break;
                    }
                case PsoDataType.UShort:
                    {
                        var arr = TraverseUShortArrayRaw(node);
                        if (embedded)
                        {
                            adata = MetaTypes.ConvertArrayToBytes(arr);
                            aCount *= 2;
                        }
                        else
                        {
                            results.UShorts[offset] = pb.AddUShortArrayPtr(arr);
                        }
                        break;
                    }

                case PsoDataType.String:
                    {
                        switch (entry.Unk_5h)
                        {
                            default:
                                //ErrorXml(sb, indent, ename + ": Unexpected String array subtype: " + entry.Unk_5h.ToString());
                                break;
                            case 0: //hash array...
                                var hashes = TraverseHashArrayRaw(node);
                                if (embedded)
                                {
                                    adata = MetaTypes.ConvertArrayToBytes(hashes);
                                    aCount *= 4;
                                }
                                else
                                {
                                    results.Hashes[offset] = pb.AddHashArrayPtr(hashes);
                                }
                                break;
                        }


                        break;
                    }


                case PsoDataType.Enum:
                    {
                        var hashes = TraverseHashArrayRaw(node);

                        if (arrEntry.ReferenceKey != 0)
                        {
                            var _infos = PsoTypes.GetEnumInfo((MetaName)arrEntry.ReferenceKey);
                            pb.AddEnumInfo(_infos.IndexInfo.NameHash);

                            var values = new uint[hashes.Length];
                            for (int i = 0; i < hashes.Length; i++)
                            {
                                var enumname = (MetaName)MetaTypes.SwapBytes(hashes[i]);//yeah swap it back to little endian..!
                                var enuminf = _infos.FindEntryByName(enumname);
                                if (enuminf != null)
                                {
                                    values[i] = MetaTypes.SwapBytes((uint)enuminf.EntryKey);
                                }
                                else
                                { } //error?
                            }

                            if (embedded)
                            { } //TODO?
                            else
                            {
                                results.UInts[offset] = pb.AddUIntArrayPtr(values);
                            }

                        }
                        else
                        { }


                        break;
                    }


                case PsoDataType.Array:
                    {
                        //array array...
                        var rk0 = (entry.ReferenceKey >> 16) & 0x0000FFFF;
                        var rk1 = arrEntry.ReferenceKey & 0x0000FFFF;
                        if (rk0 > 0) //should be count of items
                        {
                            var subarrEntry = structInfo.GetEntry((int)rk1);
                            var subarrType = (MetaName)subarrEntry.ReferenceKey;

                            var origOffset = arrEntry.DataOffset;
                            arrEntry.DataOffset = entry.DataOffset;//slight hack for traversing array array
                            foreach (XmlNode cnode in node.ChildNodes)
                            {
                                TraverseArray(cnode, pb, arrEntry, subarrEntry, results, data, structInfo);

                                arrEntry.DataOffset += 16;//ptr size... todo: what if not pointer array?
                            }
                            arrEntry.DataOffset = origOffset;


                        }


                        break;
                    }




                default:
                    break;
            }

            if (embedded)
            {
                if (adata?.Length > 0)
                {
                    if (adata.Length > aCount)
                    { }//bad! old data won't fit in new slot...

                    Buffer.BlockCopy(adata, 0, data, offset, adata.Length);
                }
                else
                { }
            }
        }

        private static void TraverseString(XmlNode node, PsoBuilder pb, PsoStructureEntryInfo entry, byte[] data)
        {
            TraverseStringRaw(node.InnerText, pb, entry, data);
        }
        private static void TraverseStringRaw(string str, PsoBuilder pb, PsoStructureEntryInfo entry, byte[] data)
        {
            switch (entry.Unk_5h)
            {
                default:
                    break;
                case 0:
                    var str0len = (int)((entry.ReferenceKey >> 16) & 0xFFFF);
                    if (!string.IsNullOrEmpty(str))
                    {
                        byte[] strdata = Encoding.ASCII.GetBytes(str);
                        Buffer.BlockCopy(strdata, 0, data, entry.DataOffset, strdata.Length);
                        if (strdata.Length > str0len)
                        { }
                    }
                    break;
                case 1:
                case 2:
                    if (!string.IsNullOrEmpty(str))
                    {
                        var bptr = pb.AddString(str);
                        var ptr = new PsoPOINTER(bptr.BlockID, bptr.Offset);
                        ptr.SwapEnd();
                        var val = MetaTypes.ConvertToBytes(ptr);
                        Buffer.BlockCopy(val, 0, data, entry.DataOffset, val.Length);
                    }
                    break;
                case 3:
                    if (!string.IsNullOrEmpty(str))
                    {
                        var bptr = pb.AddString(str);
                        var ptr = new CharPointer(bptr.Pointer, str.Length);
                        ptr.SwapEnd();
                        var val = MetaTypes.ConvertToBytes(ptr);
                        Buffer.BlockCopy(val, 0, data, entry.DataOffset, val.Length);
                    }
                    break;
                case 7://hash only?
                case 8://hash with STRF entry?

                    var hashVal = string.IsNullOrEmpty(str) ? 0 : GetHash(str);
                    Write(hashVal, data, entry.DataOffset);

                    if (entry.Unk_5h == 8)
                    {
                        pb.AddStringToSTRF(str);
                    }
                    break;
            }
        }


        private static byte[][] TraverseArrayStructureRaw(XmlNode node, PsoBuilder pb, MetaName type)
        {
            var strucs = new List<byte[]>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var struc = Traverse(cnode, pb, type);

                if (struc != null)
                {
                    strucs.Add(struc);
                }
            }

            return strucs.ToArray();
        }
        private static Array_Structure TraverseArrayStructure(XmlNode node, PsoBuilder pb, MetaName type)
        {
            var bytes = TraverseArrayStructureRaw(node, pb, type);

            return pb.AddItemArrayPtr(type, bytes);
        }

        private static PsoPOINTER[] TraverseArrayStructurePointerRaw(XmlNode node, PsoBuilder pb)
        {
            var ptrs = new List<PsoPOINTER>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var type = (MetaName)(uint)GetHash(cnode.Attributes["type"]?.Value ?? "");
                if (type != 0)
                {
                    var struc = Traverse(cnode, pb, type);

                    if (struc != null)
                    {
                        var ptr = pb.AddItemPtr(type, struc);
                        ptr.SwapEnd(); //big schmigg
                        ptrs.Add(ptr);
                    }
                    else
                    { } //error?
                }
                else
                {
                    ptrs.Add(new PsoPOINTER());//null value?
                }
            }

            return ptrs.ToArray();
        }
        private static Array_StructurePointer TraverseArrayStructurePointer(XmlNode node, PsoBuilder pb)
        {
            var ptrs = TraverseArrayStructurePointerRaw(node, pb);

            return pb.AddPointerArray(ptrs);

        }

        private static int[] TraverseSIntArrayRaw(XmlNode node)
        {
            var data = new List<int>();

            if (node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                    {
                        var val = Convert.ToInt32(split[i]);
                        data.Add(MetaTypes.SwapBytes(val));
                    }

                }
            }

            return data.ToArray();
        }
        private static uint[] TraverseUIntArrayRaw(XmlNode node)
        {
            var data = new List<uint>();

            if (node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                    {
                        var val = Convert.ToUInt32(split[i]);
                        data.Add(MetaTypes.SwapBytes(val));
                    }

                }
            }

            return data.ToArray();
        }
        private static byte[] TraverseUByteArrayRaw(XmlNode node)
        {
            var data = new List<byte>();

            if (node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                    {
                        var val = Convert.ToByte(split[i]);
                        data.Add(val);
                    }
                }
            }

            return data.ToArray();
        }
        private static ushort[] TraverseUShortArrayRaw(XmlNode node)
        {
            var data = new List<ushort>();

            if (node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                    {
                        var val = Convert.ToUInt16(split[i]);
                        data.Add(MetaTypes.SwapBytes(val));
                    }

                }
            }

            return data.ToArray();
        }
        private static float[] TraverseFloatArrayRaw(XmlNode node)
        {
            var data = new List<float>();

            if (node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                    {
                        var val = FloatUtil.Parse(split[i]);
                        data.Add(MetaTypes.SwapBytes(val));
                    }

                }
            }

            return data.ToArray();
        }
        private static Vector4[] TraverseVector3ArrayRaw(XmlNode node)
        {
            var items = new List<Vector4>();


            var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");


            float x = 0f;
            float y = 0f;
            float z = 0f;
            float w = 0f;

            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var split2 = Regex.Split(s, @"[\s\t]");
                int c = 0;
                x = 0f; y = 0f; z = 0f;
                for (int n = 0; n < split2.Length; n++)
                {
                    var ts = split2[n]?.Trim();
                    if (ts.EndsWith(",")) ts = ts.Substring(0, ts.Length - 1);
                    if (string.IsNullOrEmpty(ts)) continue;
                    var f = FloatUtil.Parse(ts);
                    switch (c)
                    {
                        case 0: x = f; break;
                        case 1: y = f; break;
                        case 2: z = f; break;
                    }
                    c++;
                }
                if (c >= 3)
                {
                    var val = new Vector4(x, y, z, w);
                    items.Add(MetaTypes.SwapBytes(val)); //big schmig
                }
            }


            return items.ToArray();
        }
        private static Vector2[] TraverseVector2ArrayRaw(XmlNode node)
        {
            var items = new List<Vector2>();


            var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");


            float x = 0f;
            float y = 0f;

            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var split2 = Regex.Split(s, @"[\s\t]");
                int c = 0;
                x = 0f; y = 0f;
                for (int n = 0; n < split2.Length; n++)
                {
                    var ts = split2[n]?.Trim();
                    if (ts.EndsWith(",")) ts = ts.Substring(0, ts.Length - 1);
                    if (string.IsNullOrEmpty(ts)) continue;
                    var f = FloatUtil.Parse(ts);
                    switch (c)
                    {
                        case 0: x = f; break;
                        case 1: y = f; break;
                    }
                    c++;
                }
                if (c >= 3)
                {
                    var val = new Vector2(x, y);
                    items.Add(MetaTypes.SwapBytes(val)); //big schmig
                }
            }


            return items.ToArray();
        }
        private static MetaHash[] TraverseHashArrayRaw(XmlNode node)
        {
            var items = new List<MetaHash>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var val = GetHash(cnode.InnerText);
                items.Add(MetaTypes.SwapBytes(val));
            }

            return items.ToArray();
        }




        private static void Write(int val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(MetaTypes.SwapBytes(val));
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(int));
        }

        private static void Write(uint val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(MetaTypes.SwapBytes(val));
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(uint));
        }

        private static void Write(short val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(MetaTypes.SwapBytes(val));
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(short));
        }

        private static void Write(ushort val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(MetaTypes.SwapBytes(val));
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(ushort));
        }

        private static void Write(float val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(MetaTypes.SwapBytes(val));//big fkn end
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(float));
        }

        private static void Write(ulong val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(MetaTypes.SwapBytes(val));
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(ulong));
        }

        private static MetaHash GetHash(string str)
        {
            if (str.StartsWith("hash_"))
            {
                return (MetaHash)Convert.ToUInt32(str.Substring(5), 16);
            }
            else
            {
                JenkIndex.Ensure(str);
                return JenkHash.GenHash(str);
            }
        }

        private static XmlNode GetEntryNode(XmlNodeList nodes, MetaName name)
        {
            foreach (XmlNode node in nodes)
            {
                if (GetHash(node.Name) == (uint)name)
                {
                    return node;
                }
            }
            return null;
        }


        private static int GetEnumInt(MetaName type, string enumString, PsoDataType dataType)
        {
            var infos = PsoTypes.GetEnumInfo(type);

            if (infos == null)
            {
                return 0;
            }


            bool isFlags = (dataType == PsoDataType.Flags);// ||
                           //(dataType == PsoDataType.IntFlags2);// ||
                            //(dataType == PsoDataType.ShortFlags);

            if (isFlags)
            {
                //flags enum. (multiple values, comma-separated)
                var split = enumString.Split(new[] { ',' ,' '}, StringSplitOptions.RemoveEmptyEntries);
                int enumVal = 0;

                for (int i = 0; i < split.Length; i++)
                {
                    var enumName = (MetaName)(uint)GetHash(split[i].Trim());

                    for (int j = 0; j < infos.Entries.Length; j++)
                    {
                        var entry = infos.Entries[j];
                        if (entry.EntryNameHash == enumName)
                        {
                            enumVal += (1 << entry.EntryKey);
                            break;
                        }
                    }
                }

                return enumVal;
            }
            else
            {
                //single value enum.

                var enumName = (MetaName)(uint)GetHash(enumString);

                for (int j = 0; j < infos.Entries.Length; j++)
                {
                    var entry = infos.Entries[j];

                    if (entry.EntryNameHash == enumName)
                    {
                        return entry.EntryKey;
                    }
                }
            }

            return -1;
        }

    }

    struct PsoArrayResults
    {
        public Dictionary<int, Array_Structure> Structures;
        public Dictionary<int, Array_StructurePointer> StructurePointers;
        public Dictionary<int, Array_uint> UInts;
        public Dictionary<int, Array_ushort> UShorts;
        public Dictionary<int, Array_byte> UBytes;
        public Dictionary<int, Array_float> Floats;
        public Dictionary<int, Array_Vector3> Float_XYZs;
        public Dictionary<int, Array_uint> Hashes;

        public void WriteArrays(byte[] data)
        {
            foreach (KeyValuePair<int, Array_Structure> ptr in Structures)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_StructurePointer> ptr in StructurePointers)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_uint> ptr in UInts)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_ushort> ptr in UShorts)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_byte> ptr in UBytes)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_float> ptr in Floats)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_Vector3> ptr in Float_XYZs)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_uint> ptr in Hashes)
            {
                var val = ptr.Value;
                val.SwapEnd();
                var _data = MetaTypes.ConvertToBytes(val);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }
        }
    }

}
