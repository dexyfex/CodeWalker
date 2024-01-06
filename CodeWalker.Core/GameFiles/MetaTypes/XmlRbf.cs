using Collections.Pooled;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeWalker.GameFiles
{
    public class XmlRbf
    {

        public static RbfFile GetRbf(XmlDocument doc)
        {
            var rbf = new RbfFile();

            using (var reader = new XmlNodeReader(doc))
            {
                reader.MoveToContent();
                rbf.current = (RbfStructure) Traverse(XDocument.Load(reader).Root);
            }

            return rbf;
        }

        private static IRbfType? Traverse(XNode node)
        {
            if (node is XElement element)
            {
                if (element.TryGetAttribute("value", out var value))
                {
                    var val = value.Value;
                    if (!string.IsNullOrEmpty(val))
                    {
                        var rval = CreateValueNode(element.Name.LocalName, val);
                        if (rval != null)
                        {
                            return rval;
                        }
                    }
                }
                else if (element.Attributes().Count() == 3 && element.TryGetAttribute("x", out var xAttr) && element.TryGetAttribute("y", out var yAttr) && element.TryGetAttribute("z", out var zAttr))
                {
                    FloatUtil.TryParse(xAttr.Value, out float x);
                    FloatUtil.TryParse(yAttr.Value, out float y);
                    FloatUtil.TryParse(zAttr.Value, out float z);
                    return new RbfFloat3()
                    {
                        Name = element.Name.LocalName,
                        X = x,
                        Y = y,
                        Z = z
                    };
                }
                else if (!element.HasElements && !element.HasAttributes && !element.IsEmpty) //else if (element.Name == "type" || element.Name == "key" || element.Name == "platform")
                {
                    var bytearr = Encoding.ASCII.GetBytes(element.Value);
                    var bytearrnt = new byte[bytearr.Length + 1];
                    Buffer.BlockCopy(bytearr, 0, bytearrnt, 0, bytearr.Length);
                    var bytes = new RbfBytes() { Value = bytearrnt };
                    var struc = new RbfStructure() { Name = element.Name.LocalName };
                    struc.Children.Add(bytes);
                    return struc;
                }

                var n = new RbfStructure();
                n.Name = element.Name.LocalName;
                n.Children = element.Nodes().Select(Traverse).ToPooledList();

                foreach (var attr in element.Attributes())
                {
                    var val = attr.Value;
                    var aval = CreateValueNode(attr.Name.LocalName, val);
                    if (aval != null)
                    {
                        n.Attributes.Add(aval);
                    }
                }

                return n;
            }
            else if (node is XText text)
            {
                byte[]? bytes = null;
                if (node.Parent?.TryGetAttribute("content", out var contentAttr) ?? false)
                {
                    if (contentAttr.Value == "char_array")
                    {
                        bytes = GetByteArray(text.Value);
                    }
                    else if (contentAttr.Value == "short_array")
                    {
                        bytes = GetUshortArray(text.Value);
                    }
                }
                else
                {
                    bytes = Encoding.ASCII.GetBytes(text.Value).Concat(new byte[] { 0x00 }).ToArray();
                }
                if (bytes is not null)
                {
                    return new RbfBytes()
                    {
                        Name = "",
                        Value = bytes
                    };
                }
            }

            return null;
        }


        private static IRbfType CreateValueNode(string name, string val)
        {
            if (val == "True")
            {
                return new RbfBoolean()
                {
                    Name = name,
                    Value = true
                };
            }
            else if (val == "False")
            {
                return new RbfBoolean()
                {
                    Name = name,
                    Value = false
                };
            }
            else if (val.StartsWith("0x"))
            {
                uint.TryParse(val.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint u);
                return new RbfUint32()
                {
                    Name = name,
                    Value = u
                };
            }
            else if (FloatUtil.TryParse(val, out float f))
            {
                return new RbfFloat()
                {
                    Name = name,
                    Value = f
                };
            }
            else
            {
                return new RbfString()
                {
                    Name = name,
                    Value = val
                };
            }
        }





        private static byte[]? GetByteArray(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            using var data = new PooledList<byte>();
            var split = Regex.Split(text, @"[\s\r\n\t]");
            foreach(var str in split)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                var val = Convert.ToByte(str);
                data.Add(val);
            }
            return data.ToArray();
        }
        private static byte[] GetUshortArray(string text)
        {
            using var data = new PooledList<byte>();
            var split = Regex.Split(text, @"[\s\r\n\t]");
            foreach(var str in split)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                var val = Convert.ToUInt16(str);
                data.Add((byte)((val >> 0) & 0xFF));
                data.Add((byte)((val >> 8) & 0xFF));
            }
            return data.ToArray();
        }

    }
}
