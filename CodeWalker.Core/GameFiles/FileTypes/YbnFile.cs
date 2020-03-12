using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YbnFile : GameFile, PackedFile
    {
        public Bounds Bounds { get; set; }


        //used by the editor:
        public bool HasChanged { get; set; } = false;

#if DEBUG
        public ResourceAnalyzer Analyzer { get; set; }
#endif


        public YbnFile() : base(null, GameFileType.Ybn)
        {
        }
        public YbnFile(RpfFileEntry entry) : base(entry, GameFileType.Ybn)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ybn file

            RpfFile.LoadResourceFile(this, data, 43);

            Loaded = true;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);


            Bounds = rd.ReadBlock<Bounds>();

            Bounds.OwnerYbn = this;
            Bounds.OwnerName = entry.Name;

#if DEBUG
            Analyzer = new ResourceAnalyzer(rd);
#endif

            Loaded = true;
        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Bounds, 43); //ybn is type/version 43...

            return data;
        }




        public bool RemoveBounds(Bounds b)
        {
            if (Bounds == b)
            {
                Bounds = null;
                return true;
            }
            return false;
        }
        public bool AddBounds(Bounds b)
        {
            if (b == null) return false;
            if (Bounds != null) return false;
            Bounds = b;
            Bounds.OwnerYbn = this;
            Bounds.OwnerName = Name ?? RpfFileEntry?.Name;
            return true;
        }

    }




    public class YbnXml : MetaXmlBase
    {

        public static string GetXml(YbnFile ybn)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            var name = "BoundsFile";
            OpenTag(sb, 0, name);

            if (ybn?.Bounds != null)
            {
                Bounds.WriteXmlNode(ybn.Bounds, sb, 1);
            }

            CloseTag(sb, 0, name);

            return sb.ToString();
        }


        public static string FormatBoundMaterialColour(BoundMaterialColour c) //for use with WriteItemArray
        {
            return c.R.ToString() + ", " + c.G.ToString() + ", " + c.B.ToString() + ", " + c.A.ToString();
        }

    }

    public class XmlYbn
    {

        public static YbnFile GetYbn(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYbn(doc);
        }

        public static YbnFile GetYbn(XmlDocument doc)
        {
            YbnFile r = new YbnFile();

            var node = doc.DocumentElement;
            var bnode = node?.SelectSingleNode("Bounds");
            if (bnode != null)
            {
                r.Bounds = Bounds.ReadXmlNode(bnode, r);
            }

            return r;
        }



        public static BoundMaterialColour[] GetRawBoundMaterialColourArray(XmlNode node)
        {
            if (node == null) return null;
            byte r, g, b, a;
            var items = new List<BoundMaterialColour>();
            var split = node.InnerText.Split('\n');// Regex.Split(node.InnerText, @"[\s\r\n\t]");
            for (int i = 0; i < split.Length; i++)
            {
                var s = split[i]?.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                var split2 = s.Split(',');// Regex.Split(s, @"[\s\t]");
                int c = 0;
                r = 0; g = 0; b = 0; a = 0;
                for (int n = 0; n < split2.Length; n++)
                {
                    var ts = split2[n]?.Trim();
                    if (string.IsNullOrEmpty(ts)) continue;
                    byte v = 0;
                    byte.TryParse(ts, out v);
                    switch (c)
                    {
                        case 0: r = v; break;
                        case 1: g = v; break;
                        case 2: b = v; break;
                        case 3: a = v; break;
                    }
                    c++;
                }
                if (c >= 2)
                {
                    var val = new BoundMaterialColour();
                    val.R = r;
                    val.G = g;
                    val.B = b;
                    val.A = a;
                    items.Add(val);
                }
            }

            return (items.Count > 0) ? items.ToArray() : null;
        }
        public static BoundMaterialColour[] GetChildRawBoundMaterialColourArray(XmlNode node, string name)
        {
            var cnode = node.SelectSingleNode(name);
            return GetRawBoundMaterialColourArray(cnode);
        }


    }



}
