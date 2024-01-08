using CodeWalker.GameFiles;
using Collections.Pooled;
using CommunityToolkit.HighPerformance.Buffers;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeWalker
{
    public static class Xml
    {
        public static void ValidateReaderState(XmlReader reader, string element)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            if (!reader.IsStartElement())
            {
                throw new InvalidOperationException($"Expected reader to be at a start element but was at \"{reader.NodeType}\" with name \"{reader.Name}\"");
            }
            if (reader.Name != element)
            {
                throw new InvalidOperationException($"Expected reader to be at start element of \"{element}\" but was at \"{reader.NodeType}\" with name \"{reader.Name}\"");
            }
        }

        public static bool TryGetAttribute(this XElement element, string attribute, [NotNullWhen(true)] out XAttribute? attr)
        {
            attr = element.Attribute(attribute);
            if (attr is null)
                return false;

            return true;
        }

        public static string? GetStringAttribute(this XmlNode? node, string attribute)
        {
            if (node is null || node.Attributes is null)
                return null;
            return node.Attributes[attribute]?.InnerText;
        }
        public static bool GetBoolAttribute(this XElement element, string attribute = "value")
        {
            ArgumentNullException.ThrowIfNull(element, nameof(element));

            var val = element.Attribute(attribute);
            if (val is null) return false;

            bool.TryParse(val.Value, out var b);
            return b;
        }

        public static bool GetBoolAttribute(this XmlNode? node, string attribute)
        {
            if (node is null)
                return false;
            string? val = node.Attributes?[attribute]?.InnerText;
            _ = bool.TryParse(val, out var b);
            return b;
        }
        public static int GetIntAttribute(this XmlNode? node, string attribute)
        {
            if (node is null)
                return 0;
            string? val = node.Attributes?[attribute]?.InnerText;
            _ = int.TryParse(val, out var i);
            return i;
        }
        public static uint GetUIntAttribute(this XmlNode? node, string attribute)
        {
            if (node is null)
                return 0;
            string? val = node.Attributes?[attribute]?.InnerText;
            _ = uint.TryParse(val, out var i);
            return i;
        }
        public static ulong GetULongAttribute(this XmlNode? node, string attribute)
        {
            if (node is null)
                return 0;
            string? val = node.Attributes?[attribute]?.InnerText;
            _ = ulong.TryParse(val, out var i);
            return i;
        }
        public static float GetFloatAttribute(this XmlNode? node, string attribute)
        {
            if (node is null)
                return 0;
            string? val = node.Attributes?[attribute]?.InnerText;
            _ = FloatUtil.TryParse(val, out var f);
            return f;
        }

        public static string? GetChildInnerText(this XmlNode? node, string name)
        {
            if (node is null)
                return null;
            return node.SelectSingleNode(name)?.InnerText;
        }

        public static string? GetChildInnerText(this XElement? node, string name) {
            if (node is null)
                return null;
            return node.Element(name)?.Value;
        }

        public static string GetChildInnerText(XmlReader reader, string name)
        {
            ValidateReaderState(reader, name);
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    return string.Empty;
                }
                return reader.ReadElementContentAsString();
            }
            else
            {
                return reader.ReadContentAsString();
            }
        }

        public static bool GetChildBoolInnerText(this XElement? node, string name)
        {
            if (node is null)
                return false;
            string? val = node.Element(name)?.Value;
            if (string.IsNullOrEmpty(val))
                return false;

            _ = bool.TryParse(val, out var b);

            return b;
        }

        public static bool GetChildBoolInnerText(XmlReader reader, string name)
        {
            ValidateReaderState(reader, name);
            if (reader.NodeType == XmlNodeType.Element)
            {
                return reader.ReadElementContentAsBoolean();
            }
            else
            {
                return reader.ReadContentAsBoolean();
            }

        }

        public static bool GetChildBoolInnerText(this XmlNode? node, string name)
        {
            if (node is null)
                return false;
            string? val = node.SelectSingleNode(name)?.InnerText;
            _ = bool.TryParse(val, out var b);
            return b;
        }
        public static int GetChildIntInnerText(this XmlNode? node, string name)
        {
            if (node is null)
                return 0;
            string? val = node.SelectSingleNode(name)?.InnerText;
            _ = int.TryParse(val, out var i);
            return i;
        }
        public static float GetChildFloatInnerText(this XmlNode? node, string name)
        {
            if (node is null)
                return 0;
            string? val = node.SelectSingleNode(name)?.InnerText;
            _ = FloatUtil.TryParse(val, out var f);
            return f;
        }

        public static float GetChildFloatInnerText(XmlReader reader, string name)
        {
            ValidateReaderState(reader, name);

            return reader.ReadElementContentAsFloat();
        }

        public static T GetChildEnumInnerText<T>(XmlNode node, string name) where T : struct
        {
            if (node is null)
                return new T();

            string? val = node.SelectSingleNode(name)?.InnerText;
            return GetEnumValue<T>(val);
        }
        public static T GetEnumValue<T>(string? val) where T : struct
        {
            if (string.IsNullOrEmpty(val))
            {
                return default;
            }
            if (val.StartsWith("hash_", StringComparison.OrdinalIgnoreCase))
            {
                //convert hash_12ABC to Unk_12345
                //var substr = val.Substring(5);
                //var uval = Convert.ToUInt32(substr, 16);
                _ = int.TryParse(val.AsSpan(5), System.Globalization.NumberStyles.HexNumber, null, out var result);
                val = $"Unk_{result}";
            }
            T enumval;
            Enum.TryParse(val, out enumval);
            return enumval;
        }

        public static bool GetChildBoolAttribute(XmlReader reader, string name, string attribute = "value")
        {
            ValidateReaderState(reader, name);

            string val = reader.GetAttribute(attribute);

            bool.TryParse(val, out bool boolval);

            reader.ReadStartElement();

            return boolval;
        }
        public static bool GetChildBoolAttribute(XmlNode node, string name, string attribute = "value")
        {
            if (node == null) return false;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            bool b;
            bool.TryParse(val, out b);
            return b;
        }

        public static int GetChildIntAttribute(XmlReader reader, string name, string attribute = "value")
        {
            ValidateReaderState(reader, name);

            string val = reader.GetAttribute(attribute);

            int.TryParse(val, out var i);

            reader.ReadStartElement();

            return i;
        }

        public static int GetChildIntAttribute(XmlNode node, string name, string attribute = "value")
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            int i;
            int.TryParse(val, out i);
            return i;
        }

        public static uint GetChildUIntAttribute(XmlReader reader, string name, string attribute = "value")
        {
            if (reader == null) return 0;

            uint i;
            string val = reader.GetAttribute(attribute);
            if (val?.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                //var subs = val.Substring(2);
                //i = Convert.ToUInt32(subs, 16);
                _ = uint.TryParse(val.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out i);
            }
            else
            {
                _ = uint.TryParse(val, out i);
            }
            return i;
        }

        public static uint GetChildUIntAttribute(XmlNode node, string name, string attribute = "value")
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            uint i;
            if (val?.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                //var subs = val.Substring(2);
                //i = Convert.ToUInt32(subs, 16);
                _ = uint.TryParse(val.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out i);
            }
            else
            {
                _ = uint.TryParse(val, out i);
            }
            return i;
        }
        public static ulong GetChildULongAttribute(XmlNode node, string name, string attribute = "value")
        {
            if (node == null) return 0;
            string val = node.SelectSingleNode(name)?.Attributes[attribute]?.InnerText;
            ulong i;
            if (val?.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                //var subs = val.Substring(2);
                //i = Convert.ToUInt64(subs, 16);
                _ = ulong.TryParse(val.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out i);
            }
            else
            {
                _ = ulong.TryParse(val, out i);
            }
            return i;
        }

        public static float GetChildFloatAttribute(XmlReader reader, string name, string attribute = "value")
        {
            if (!string.IsNullOrEmpty(name))
            {
                ValidateReaderState(reader, name);
            }

            string val = reader.GetAttribute(attribute);

            FloatUtil.TryParse(val, out float f);

            if (!string.IsNullOrEmpty(name))
            {
                reader.ReadStartElement();
            }

            return f;
        }

        public static float GetChildFloatAttribute(XmlNode? node, string name, string attribute = "value")
        {
            if (node is null)
                return 0;
            string? val = node.SelectSingleNode(name)?.Attributes?[attribute]?.InnerText;
            float f;
            FloatUtil.TryParse(val, out f);
            return f;
        }
        public static string? GetChildStringAttribute(XmlNode node, string name, string attribute = "value")
        {
            if (node is null) return string.Empty;
            string? val = node.SelectSingleNode(name)?.Attributes?[attribute]?.InnerText;
            return val;
        }

        public static string? GetChildStringAttribute(XmlReader reader, string name, string attribute = "value")
        {
            ValidateReaderState(reader, name);

            var val = reader.GetAttribute(attribute);

            reader.ReadStartElement();

            return val;
        }

        public static Vector2 GetChildVector2Attributes(XmlNode node, string name, string x = "x", string y = "y")
        {
            float fx = GetChildFloatAttribute(node, name, x);
            float fy = GetChildFloatAttribute(node, name, y);
            return new Vector2(fx, fy);
        }

        public static Vector3 GetChildVector3Attributes(XmlReader reader, string name, string x = "x", string y = "y", string z = "z")
        {
            ValidateReaderState(reader, name);
            float fx = GetChildFloatAttribute(reader, null, x);
            float fy = GetChildFloatAttribute(reader, null, y);
            float fz = GetChildFloatAttribute(reader, null, z);

            reader.ReadStartElement();

            return new Vector3(fx, fy, fz);
        }

        public static Vector3 GetChildVector3Attributes(XmlNode node, string name, string x = "x", string y = "y", string z = "z")
        {
            float fx = GetChildFloatAttribute(node, name, x);
            float fy = GetChildFloatAttribute(node, name, y);
            float fz = GetChildFloatAttribute(node, name, z);
            return new Vector3(fx, fy, fz);
        }
        public static Vector4 GetChildVector4Attributes(XmlNode node, string name, string x = "x", string y = "y", string z = "z", string w = "w")
        {
            float fx = GetChildFloatAttribute(node, name, x);
            float fy = GetChildFloatAttribute(node, name, y);
            float fz = GetChildFloatAttribute(node, name, z);
            float fw = GetChildFloatAttribute(node, name, w);
            return new Vector4(fx, fy, fz, fw);
        }

        public static XmlElement GetChild(XmlElement element, string name)
        {
            return element.SelectSingleNode(name) as XmlElement;
        }

        public static XmlElement AddChild(XmlDocument doc, XmlNode node, string name)
        {
            XmlElement child = doc.CreateElement(name);
            node.AppendChild(child);
            return child;
        }

        public static bool IsItemElement(this XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element && reader.Name == "Item")
            {
                return true;
            }

            return false;
        }

        public static void MoveToStartElement(this XmlReader reader, string name)
        {
            var startDepth = reader.Depth;
            while (reader.Read() && startDepth <= reader.Depth)
            {
                if (reader.IsStartElement() && reader.Name == name)
                {
                    break;
                }
            }
        }

        public static IEnumerable<XElement> IterateItems(this XmlReader reader, string parentElementName)
        {
            ValidateReaderState(reader, parentElementName);
            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                // Move past empty element
                reader.ReadStartElement(parentElementName);
                yield break;
            }
            var startDepth = reader.Depth;
            reader.ReadStartElement(parentElementName);
            while(reader.IsItemElement() && startDepth < reader.Depth)
            {
                if (XNode.ReadFrom(reader) is XElement el)
                {
                    yield return el;
                }
            }
            reader.ReadEndElement();
        }
        public static XmlElement AddChildWithInnerText(XmlDocument doc, XmlNode node, string name, string innerText)
        {
            XmlElement child = AddChild(doc, node, name);
            child.InnerText = innerText;
            return child;
        }
        public static XmlElement AddChildWithAttribute(XmlDocument doc, XmlNode node, string name, string attributeName, string attributeValue)
        {
            XmlElement child = AddChild(doc, node, name);
            child.SetAttribute(attributeName, attributeValue);
            return child;
        }




        public static byte[] GetRawByteArray(XmlNode? node, int fromBase = 16)
        {
            if (node == null)
                return [];
            using var data = new PooledList<byte>();
            var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                if (!string.IsNullOrEmpty(split[i]))
                {
                    var str = split[i];
                    if (string.IsNullOrEmpty(str)) continue;
                    var val = Convert.ToByte(str, fromBase);
                    data.Add(val);
                }
            }
            return data.ToArray();
        }
        public static byte[] GetChildRawByteArray(XmlNode node, string name, int fromBase = 16)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawByteArray(cnode, fromBase);
        }
        public static byte[]? GetChildRawByteArrayNullable(XmlNode node, string name, int fromBase = 16)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawByteArray(cnode, fromBase);
            return arr.Length > 0 ? arr : null;
        }

        public static ushort[] GetRawUshortArray(XmlNode? node)
        {
            if (node is null)
                return [];
            using var data = new PooledList<ushort>();
            var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                if (!string.IsNullOrEmpty(split[i]))
                {
                    var str = split[i];
                    if (string.IsNullOrEmpty(str)) continue;
                    var val = (ushort)0;
                    ushort.TryParse(str, out val);
                    data.Add(val);
                }
            }
            return data.ToArray();
        }
        public static ushort[] GetChildRawUshortArray(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawUshortArray(cnode);
        }
        public static ushort[]? GetChildRawUshortArrayNullable(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawUshortArray(cnode);
            return arr.Length > 0 ? arr : null;
        }

        public static uint[] GetRawUintArray(XmlNode? node)
        {
            if (node == null)
                return [];
            using var data = new PooledList<uint>();
            var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                if (!string.IsNullOrEmpty(split[i]))
                {
                    var str = split[i];
                    if (string.IsNullOrEmpty(str)) continue;
                    var val = 0u;
                    uint.TryParse(str, out val);
                    data.Add(val);
                }
            }
            return data.ToArray();
        }
        public static uint[] GetChildRawUintArray(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawUintArray(cnode);
        }
        public static uint[]? GetChildRawUintArrayNullable(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawUintArray(cnode);
            return arr.Length > 0 ? arr : null;
        }

        public static int[] GetRawIntArray(XmlNode? node)
        {
            if (node is null)
                return [];
            using var data = new PooledList<int>();
            var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                if (!string.IsNullOrEmpty(split[i]))
                {
                    var str = split[i];
                    if (string.IsNullOrEmpty(str)) continue;
                    var val = 0;
                    int.TryParse(str, out val);
                    data.Add(val);
                }
            }
            return data.ToArray();
        }
        public static int[] GetChildRawIntArray(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawIntArray(cnode);
        }
        public static int[]? GetChildRawIntArrayNullable(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawIntArray(cnode);
            return ((arr != null) && (arr.Length > 0)) ? arr : null;
        }

        public static float[] GetRawFloatArray(XmlNode? node)
        {
            if (node is null)
                return [];
            using var items = new PooledList<float>();
            var split = Regex.Split(node.InnerText, @"[\s\r\n\t]");//node.InnerText.Split('\n');// 
            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var f = FloatUtil.Parse(s);
                items.Add(f);
            }
            return items.ToArray();
        }
        public static float[] GetChildRawFloatArray(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawFloatArray(cnode);
        }
        public static float[]? GetChildRawFloatArrayNullable(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawFloatArray(cnode);
            return arr.Length > 0 ? arr : null;
        }

        public static Vector2[] GetRawVector2Array(XmlNode? node)
        {
            if (node is null)
                return [];
            float x = 0f;
            float y = 0f;
            using var items = new PooledList<Vector2>();
            var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var split2 = s.Split(',');// Regex.Split(s, @"[\s\t]");
                int c = 0;
                x = 0f; y = 0f;
                for (int n = 0; n < split2.Length; n++)
                {
                    var ts = split2[n]?.Trim();
                    if (string.IsNullOrEmpty(ts)) continue;
                    var f = FloatUtil.Parse(ts);
                    switch (c)
                    {
                        case 0: x = f; break;
                        case 1: y = f; break;
                        //case 2: z = f; break;
                    }
                    c++;
                }
                if (c >= 2)
                {
                    var val = new Vector2(x, y);
                    items.Add(val);
                }
            }

            return items.ToArray();
        }
        public static Vector2[] GetChildRawVector2Array(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawVector2Array(cnode);
        }

        public static Vector3[] GetRawVector3Array(XmlNode? node)
        {
            if (node is null)
                return [];
            float x = 0f;
            float y = 0f;
            float z = 0f;
            using var items = new PooledList<Vector3>();
            var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var split2 = s.Split(',');// Regex.Split(s, @"[\s\t]");
                int c = 0;
                x = 0f; y = 0f;
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
                    var val = new Vector3(x, y, z);
                    items.Add(val);
                }
            }

            return items.ToArray();
        }
        public static Vector3[] GetChildRawVector3Array(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawVector3Array(cnode);
        }
        public static Vector3[]? GetChildRawVector3ArrayNullable(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawVector3Array(cnode);
            return arr.Length > 0 ? arr : null;
        }

        public static Vector4[] GetRawVector4Array(XmlNode? node)
        {
            if (node is null)
                return [];
            float z = 0f;
            float w = 0f;
            using var items = new PooledList<Vector4>();
            var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var split2 = s.Split(',');// Regex.Split(s, @"[\s\t]");
                int c = 0;
                float x = 0f;
                float y = 0f;
                for (int n = 0; n < split2.Length; n++)
                {
                    var ts = split2[n]?.Trim();
                    if (string.IsNullOrEmpty(ts))
                        continue;
                    var f = FloatUtil.Parse(ts);
                    switch (c)
                    {
                        case 0: x = f; break;
                        case 1: y = f; break;
                        case 2: z = f; break;
                        case 3: w = f; break;
                    }
                    c++;
                }
                if (c >= 4)
                {
                    var val = new Vector4(x, y, z, w);
                    items.Add(val);
                }
            }

            return items.ToArray();
        }
        public static Vector4[] GetChildRawVector4Array(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawVector4Array(cnode);
        }
        public static Vector4[]? GetChildRawVector4ArrayNullable(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            var arr = GetRawVector4Array(cnode);
            return arr.Length > 0 ? arr : null;
        }

        public static Matrix GetMatrix(XmlNode? node)
        {
            if (node is null)
                return Matrix.Identity;
            var arr = GetRawFloatArray(node);
            if (arr.Length != 16)
                return Matrix.Identity;

            return new Matrix(arr);
        }
        public static Matrix GetChildMatrix(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetMatrix(cnode);
        }

    }

    public class XmlNameTableThreadSafe : NameTable
    {
        //private object _locker = new object();

        public StringPool StringPool;
        public XmlNameTableThreadSafe()
            : this(256)
        { }

        public XmlNameTableThreadSafe(int stringPoolSize)
            : base()
        {
            StringPool = new StringPool(stringPoolSize);
        }

        

        public override string Add(string key)
        {
            return StringPool.GetOrAdd(key);
        }

        public override string Add(char[] key, int start, int len)
        {
            return StringPool.GetOrAdd(key.AsSpan(start, len));
        }

        public override string? Get(char[] key, int start, int len)
        {
            StringPool.TryGet(key.AsSpan(start, len), out var value);
            return value;
            //return stringPool.Get(key.AsSpan(start, len));
        }

        public override string? Get(string value)
        {
            StringPool.TryGet(value, out var result);
            return result;
        }
    }
}
