using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using SharpDX;

namespace CodeWalker.GameFiles
{
    public class XmlMeta
    {
        public static Meta GetMeta(XmlDocument doc)
        {
            MetaBuilder mb = new MetaBuilder();

            Traverse(doc.DocumentElement, mb, 0, true);

            var meta = mb.GetMeta();

            return meta;
        }

        private static byte[] Traverse(XmlNode node, MetaBuilder mb, MetaName type = 0, bool isRoot = false)
        {
            if(type == 0)
            {
                type = (MetaName)(uint)GetHash(node.Name);
            }

            var infos = MetaTypes.GetStructureInfo(type);

            if (infos != null)
            {
                byte[] data = new byte[infos.StructureSize];
                var arrayResults = new ArrayResults();

                arrayResults.Structures = new Dictionary<int, Array_Structure>();
                arrayResults.StructurePointers = new Dictionary<int, Array_StructurePointer>();
                arrayResults.UInts = new Dictionary<int, Array_uint>();
                arrayResults.UShorts = new Dictionary<int, Array_ushort>();
                arrayResults.UBytes = new Dictionary<int, Array_byte>();
                arrayResults.Floats = new Dictionary<int, Array_float>();
                arrayResults.Float_XYZs = new Dictionary<int, Array_Vector3>();
                arrayResults.Hashes = new Dictionary<int, Array_uint>();

                Array.Clear(data, 0, infos.StructureSize);

                MetaStructureEntryInfo_s arrEntry = new MetaStructureEntryInfo_s();

                if (isRoot)
                {
                    mb.EnsureBlock(type);
                }

                for (int i = 0; i < infos.Entries.Length; i++)
                {
                    var entry = infos.Entries[i];

                    var cnode = GetEntryNode(node.ChildNodes, entry);

                    if (entry.EntryNameHash == MetaName.ARRAYINFO)
                    {
                        arrEntry = entry;
                        continue;
                    }

                    if (cnode == null)
                    {
                        continue;
                    }

                    switch (entry.DataType)
                    {
                        case MetaStructureEntryDataType.Array:
                            {
                                TraverseArray(cnode, mb, arrEntry, entry.DataOffset, arrayResults);
                                break;
                            }

                        case MetaStructureEntryDataType.ArrayOfBytes:
                            {
                                GetParsedArrayOfBytes(cnode, data, entry, arrEntry);
                                break;
                            }
                            
                        case MetaStructureEntryDataType.ArrayOfChars:
                            {
                                int offset = entry.DataOffset;
                                var split = cnode.InnerText;// Split(cnode.InnerText, 1);

                                for (int j = 0; j < split.Length; j++)
                                {
                                    byte val = (byte)split[j];// Convert.ToByte(split[j], 16);
                                    data[offset] = val;
                                    offset += sizeof(byte);
                                }

                                break;
                            }

                        case MetaStructureEntryDataType.Boolean:
                            {
                                byte val = (cnode.Attributes["value"].Value == "false") ? (byte)0 : (byte)1;
                                data[entry.DataOffset] = val;
                                break;
                            }

                        case MetaStructureEntryDataType.ByteEnum:
                            {
                                byte val = Convert.ToByte(cnode.Attributes["value"].Value);
                                data[entry.DataOffset] = val;
                                break;
                            }


                        case MetaStructureEntryDataType.CharPointer:
                            {
                                if (!string.IsNullOrEmpty(cnode.InnerText))
                                {
                                    var ptr = mb.AddStringPtr(cnode.InnerText);
                                    var val = MetaTypes.ConvertToBytes(ptr);

                                    Buffer.BlockCopy(val, 0, data, entry.DataOffset, val.Length);
                                }

                                break;
                            }

                        case MetaStructureEntryDataType.DataBlockPointer:
                            {
                                var ns = NumberStyles.HexNumber;
                                var ic = CultureInfo.InvariantCulture;
                                var sa = new[] { ' ', '\n' };
                                var so = StringSplitOptions.RemoveEmptyEntries;
                                var split = cnode.InnerText.Trim().Split(sa, so); //split = Split(node.InnerText, 2); to read as unsplitted HEX
                                var bytes = new List<byte>();
                                for (int j = 0; j < split.Length; j++)
                                {
                                    byte val;// = Convert.ToByte(split[j], 10);
                                    if (byte.TryParse(split[j].Trim(), ns, ic, out val))
                                    {
                                        bytes.Add(val);
                                    }
                                }
                                var ptr = mb.AddDataBlockPtr(bytes.ToArray(), MetaName.BYTE);
                                var byt = MetaTypes.ConvertToBytes(ptr);
                                Buffer.BlockCopy(byt, 0, data, entry.DataOffset, byt.Length);
                                break;
                            }

                        case MetaStructureEntryDataType.Float:
                            {
                                float val = FloatUtil.Parse(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }

                        case MetaStructureEntryDataType.Float_XYZ:
                            {
                                float x = FloatUtil.Parse(cnode.Attributes["x"].Value);
                                float y = FloatUtil.Parse(cnode.Attributes["y"].Value);
                                float z = FloatUtil.Parse(cnode.Attributes["z"].Value);

                                Write(x, data, entry.DataOffset);
                                Write(y, data, entry.DataOffset + sizeof(float));
                                Write(z, data, entry.DataOffset + sizeof(float) * 2);

                                break;
                            }


                        case MetaStructureEntryDataType.Float_XYZW:
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

                        case MetaStructureEntryDataType.Hash:
                            {
                                var hash = GetHash(cnode.InnerText);
                                Write(hash, data, entry.DataOffset);
                                break;
                            }

                        case MetaStructureEntryDataType.IntEnum:
                        case MetaStructureEntryDataType.IntFlags1:
                        case MetaStructureEntryDataType.IntFlags2:
                            {
                                if (entry.ReferenceKey != 0)
                                {
                                    var _infos = MetaTypes.GetEnumInfo(entry.ReferenceKey);
                                    mb.AddEnumInfo(_infos.EnumNameHash);
                                }

                                int val = GetEnumInt(entry.ReferenceKey, cnode.InnerText, entry.DataType);
                                Write(val, data, entry.DataOffset);
                                break;
                            }

                        case MetaStructureEntryDataType.ShortFlags:
                            {
                                if (entry.ReferenceKey != 0)
                                {
                                    var _infos = MetaTypes.GetEnumInfo(entry.ReferenceKey);
                                    mb.AddEnumInfo(_infos.EnumNameHash);
                                }

                                int val = GetEnumInt(entry.ReferenceKey, cnode.InnerText, entry.DataType);
                                Write((short)val, data, entry.DataOffset);
                                break;
                            }

                        case MetaStructureEntryDataType.SignedByte:
                            {
                                var val = Convert.ToSByte(cnode.Attributes["value"].Value);
                                data[entry.DataOffset] = (byte)val;
                                break;
                            }

                        case MetaStructureEntryDataType.SignedInt:
                            {
                                var val = Convert.ToInt32(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }

                        case MetaStructureEntryDataType.SignedShort:
                            {
                                var val = Convert.ToInt16(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }

                        case MetaStructureEntryDataType.Structure:
                            {
                                var struc = Traverse(cnode, mb, entry.ReferenceKey);

                                if(struc != null)
                                {
                                    Buffer.BlockCopy(struc, 0, data, entry.DataOffset, struc.Length);
                                }

                                break;
                            }

                        case MetaStructureEntryDataType.StructurePointer:
                            {
                                // TODO
                                break;
                            }

                        case MetaStructureEntryDataType.UnsignedByte:
                            {
                                var val = Convert.ToByte(cnode.Attributes["value"].Value);
                                data[entry.DataOffset] = val;
                                break;
                            }

                        case MetaStructureEntryDataType.UnsignedInt:
                            {
                                switch (entry.EntryNameHash)
                                {
                                    case MetaName.color:
                                        {
                                            var val = Convert.ToUInt32(cnode.Attributes["value"].Value, 16);
                                            Write(val, data, entry.DataOffset);
                                            break;
                                        }

                                    default:
                                        {
                                            var val = Convert.ToUInt32(cnode.Attributes["value"].Value);
                                            Write(val, data, entry.DataOffset);
                                            break;
                                        }
                                }

                                break;
                            }

                        case MetaStructureEntryDataType.UnsignedShort:
                            {
                                var val = Convert.ToUInt16(cnode.Attributes["value"].Value);
                                Write(val, data, entry.DataOffset);
                                break;
                            }

                        default: break;

                    }
                }

                arrayResults.WriteArrays(data);

                mb.AddStructureInfo(infos.StructureNameHash);

                if (isRoot)
                {
                    mb.AddItem(type, data);
                }

                return data;
            }

            return null;
        }

        private static void GetParsedArrayOfBytes(XmlNode node, byte[] data, MetaStructureEntryInfo_s entry, MetaStructureEntryInfo_s arrEntry)
        {
            int offset = entry.DataOffset;

            var ns = NumberStyles.Any;
            var ic = CultureInfo.InvariantCulture;
            var sa = new[] { ' ' };
            var so = StringSplitOptions.RemoveEmptyEntries;
            var split = node.InnerText.Trim().Split(sa, so); //split = Split(node.InnerText, 2); to read as unsplitted HEX

            switch (arrEntry.DataType)
            {
                default: //expecting hex string.
                    split = Split(node.InnerText, 2);
                    for (int j = 0; j < split.Length; j++)
                    {
                        byte val = Convert.ToByte(split[j], 16);
                        data[offset] = val;
                        offset += sizeof(byte);
                    }
                    break;
                case MetaStructureEntryDataType.SignedByte: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        sbyte val;// = Convert.ToSByte(split[j], 10);
                        if (sbyte.TryParse(split[j].Trim(), ns, ic, out val))
                        {
                            data[offset] = (byte)val;
                            offset += sizeof(sbyte);
                        }
                    }
                    break;
                case MetaStructureEntryDataType.UnsignedByte: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        byte val;// = Convert.ToByte(split[j], 10);
                        if (byte.TryParse(split[j].Trim(), ns, ic, out val))
                        {
                            data[offset] = val;
                            offset += sizeof(byte);
                        }
                    }
                    break;
                case MetaStructureEntryDataType.SignedShort: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        short val;// = Convert.ToInt16(split[j], 10);
                        if (short.TryParse(split[j].Trim(), ns, ic, out val))
                        {
                            Write(val, data, offset);
                            offset += sizeof(short);
                        }
                    }
                    break;
                case MetaStructureEntryDataType.UnsignedShort: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        ushort val;// = Convert.ToUInt16(split[j], 10);
                        if (ushort.TryParse(split[j].Trim(), ns, ic, out val))
                        {
                            Write(val, data, offset);
                            offset += sizeof(ushort);
                        }
                    }
                    break;
                case MetaStructureEntryDataType.SignedInt: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        int val;// = Convert.ToInt32(split[j], 10);
                        if (int.TryParse(split[j].Trim(), ns, ic, out val))
                        {
                            Write(val, data, offset);
                            offset += sizeof(int);
                        }
                    }
                    break;
                case MetaStructureEntryDataType.UnsignedInt: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        uint val;// = Convert.ToUInt32(split[j], 10);
                        if (uint.TryParse(split[j].Trim(), ns, ic, out val))
                        {
                            Write(val, data, offset);
                            offset += sizeof(uint);
                        }
                    }
                    break;
                case MetaStructureEntryDataType.Float: //expecting space-separated array.
                    for (int j = 0; j < split.Length; j++)
                    {
                        float val;// = FloatUtil.Parse(split[j]);
                        if (FloatUtil.TryParse(split[j].Trim(), out val))
                        {
                            Write(val, data, offset);
                            offset += sizeof(float);
                        }
                    }
                    break;
            }
        }

        private static void TraverseArray(XmlNode node, MetaBuilder mb, MetaStructureEntryInfo_s arrEntry, int offset, ArrayResults results)
        {
            switch (arrEntry.DataType)
            {
                case MetaStructureEntryDataType.Structure:
                    {
                        results.Structures[offset] = TraverseArrayStructure(node, mb, arrEntry.ReferenceKey);
                        break;
                    }

                case MetaStructureEntryDataType.StructurePointer:
                    {
                        results.StructurePointers[offset] = TraverseArrayStructurePointer(node, mb);
                        break;
                    }

                case MetaStructureEntryDataType.UnsignedInt:
                    {
                        results.UInts[offset] = TraverseRawUIntArray(node, mb);
                        break;
                    }
                case MetaStructureEntryDataType.UnsignedShort:
                    {
                        results.UShorts[offset] = TraverseRawUShortArray(node, mb);
                        break;
                    }
                case MetaStructureEntryDataType.UnsignedByte:
                    {
                        results.UBytes[offset] = TraverseRawUByteArray(node, mb);
                        break;
                    }
                case MetaStructureEntryDataType.Float:
                    {
                        results.Floats[offset] = TraverseRawFloatArray(node, mb);
                        break;
                    }
                case MetaStructureEntryDataType.Float_XYZ:
                    {
                        results.Float_XYZs[offset] = TraverseRawVector3Array(node, mb);
                        break;
                    }
                case MetaStructureEntryDataType.Hash:
                    {
                        results.Hashes[offset] = TraverseHashArray(node, mb);
                        break;
                    }
                case MetaStructureEntryDataType.CharPointer:
                    {
                        // TODO
                        break;
                    }
                case MetaStructureEntryDataType.DataBlockPointer:
                    {
                        // TODO
                        break;
                    }

                default: break;
            }

        }

        private static Array_Structure TraverseArrayStructure(XmlNode node, MetaBuilder mb, MetaName type)
        {
            var strucs = new List<byte[]>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var struc = Traverse(cnode, mb, type);

                if (struc != null)
                {
                    strucs.Add(struc);
                }
            }

            return mb.AddItemArrayPtr(type, strucs.ToArray());
        }

        private static Array_StructurePointer TraverseArrayStructurePointer(XmlNode node, MetaBuilder mb)
        {
            var ptrs = new List<MetaPOINTER>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var type = (MetaName)(uint)GetHash(cnode.Attributes["type"].Value);
                var struc = Traverse(cnode, mb, type);

                if(struc != null)
                {
                    var ptr = mb.AddItemPtr(type, struc);
                    ptrs.Add(ptr);
                }

            }

            return mb.AddPointerArray(ptrs.ToArray());

        }

        private static Array_uint TraverseRawUIntArray(XmlNode node, MetaBuilder mb)
        {
            var data = new List<uint>();

            if (node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    if(!string.IsNullOrEmpty(split[i]))
                    {
                        var val = Convert.ToUInt32(split[i]);
                        data.Add(val);
                    }

                }
            }

            return mb.AddUintArrayPtr(data.ToArray());
        }

        private static Array_ushort TraverseRawUShortArray(XmlNode node, MetaBuilder mb)
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
                        data.Add(val);
                    }
                }
            }

            return mb.AddUshortArrayPtr(data.ToArray());
        }

        private static Array_byte TraverseRawUByteArray(XmlNode node, MetaBuilder mb)
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

            return mb.AddByteArrayPtr(data.ToArray());
        }

        private static Array_float TraverseRawFloatArray(XmlNode node, MetaBuilder mb)
        {
            var data = new List<float>();

            if(node.InnerText != "")
            {
                var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");

                for (int i = 0; i < split.Length; i++)
                {
                    var ts = split[i]?.Trim();
                    if (!string.IsNullOrEmpty(ts))
                    {
                        var val = FloatUtil.Parse(ts);// Convert.ToSingle(split[i]);
                        data.Add(val);
                    }
                }
            }

            return mb.AddFloatArrayPtr(data.ToArray());
        }

        private static Array_Vector3 TraverseRawVector3Array(XmlNode node, MetaBuilder mb)
        {
            var items = new List<Vector4>();

            float x = 0f;
            float y = 0f;
            float z = 0f;
            float w = 0f;

            var cnodes = node.SelectNodes("Item");
            if (cnodes.Count > 0)
            {
                foreach (XmlNode cnode in cnodes)
                {
                    var str = cnode.InnerText;
                    var strs = str.Split(',');
                    if (strs.Length >= 3)
                    {
                        x = FloatUtil.Parse(strs[0].Trim());
                        y = FloatUtil.Parse(strs[1].Trim());
                        z = FloatUtil.Parse(strs[2].Trim());
                        if (strs.Length >= 4)
                        {
                            w = FloatUtil.Parse(strs[3].Trim());
                        }
                        var val = new Vector4(x, y, z, w);
                        items.Add(val);
                    }
                }
            }
            else
            {
                var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");

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
                        items.Add(val);
                    }
                }
            }


            return mb.AddPaddedVector3ArrayPtr(items.ToArray());
        }

        private static Array_uint TraverseHashArray(XmlNode node, MetaBuilder mb)
        {
            var items = new List<MetaHash>();

            foreach (XmlNode cnode in node.ChildNodes)
            {
                var val = GetHash(cnode.InnerText);
                items.Add(val);
            }

            return mb.AddHashArrayPtr(items.ToArray());
        }

        private static void Write(int val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(int));
        }

        private static void Write(uint val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(uint));
        }

        private static void Write(short val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(short));
        }

        private static void Write(ushort val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(ushort));
        }

        private static void Write(float val, byte[] data, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Buffer.BlockCopy(bytes, 0, data, offset, sizeof(float));
        }

        public static MetaHash GetHash(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            if (str.StartsWith("hash_"))
            {
                return (MetaHash) Convert.ToUInt32(str.Substring(5), 16);
            }
            else
            {
                JenkIndex.Ensure(str);
                return JenkHash.GenHash(str);
            }
        }

        private static XmlNode GetEntryNode(XmlNodeList nodes, MetaStructureEntryInfo_s entry)
        {
            foreach (XmlNode node in nodes)
            {
                if (GetHash(node.Name) == (uint)entry.EntryNameHash)
                {
                    return node;
                }
            }

            return null;
        }

        private static string[] Split(string str, int maxChunkSize)
        {
            var chunks = new List<String>();

            for (int i = 0; i < str.Length; i += maxChunkSize)
            {
                chunks.Add(str.Substring(i, Math.Min(maxChunkSize, str.Length - i)));
            }

            return chunks.ToArray();
        }

        private static int GetEnumInt(MetaName type, string enumString, MetaStructureEntryDataType dataType)
        {
            var intval = 0;
            if (int.TryParse(enumString, out intval))
            {
                return intval; //it's already an int.... maybe enum not found or has no entries... or original value didn't match anything
            }

            var infos = MetaTypes.GetEnumInfo(type);
            if (infos == null)
            {
                return 0;
            }


            bool isFlags = (dataType == MetaStructureEntryDataType.IntFlags1) ||
                           (dataType == MetaStructureEntryDataType.IntFlags2);// ||
                           //(dataType == MetaStructureEntryDataType.ShortFlags);

            if (isFlags)
            {
                //flags enum. (multiple values, comma-separated)
                var split = enumString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int enumVal = 0;

                for (int i = 0; i < split.Length; i++)
                {
                    var enumName = (MetaName)(uint)GetHash(split[i].Trim());

                    for (int j = 0; j < infos.Entries.Length; j++)
                    {
                        var entry = infos.Entries[j];
                        if (entry.EntryNameHash == enumName)
                        {
                            enumVal += (1 << entry.EntryValue);
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
                        return entry.EntryValue;
                    }
                }
            }

            return 0;
        }
    }

    struct ArrayResults
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
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_StructurePointer> ptr in StructurePointers)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_uint> ptr in UInts)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_ushort> ptr in UShorts)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_byte> ptr in UBytes)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_float> ptr in Floats)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_Vector3> ptr in Float_XYZs)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }

            foreach (KeyValuePair<int, Array_uint> ptr in Hashes)
            {
                var _data = MetaTypes.ConvertToBytes(ptr.Value);
                Buffer.BlockCopy(_data, 0, data, ptr.Key, _data.Length);
            }
        }
    }
}
