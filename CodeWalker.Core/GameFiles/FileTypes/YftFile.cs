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
    public class YftFile : GameFile, PackedFile
    {
        public FragType Fragment { get; set; }

        public YftFile() : base(null, GameFileType.Yft)
        {
        }
        public YftFile(RpfFileEntry entry) : base(entry, GameFileType.Yft)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed yft file

            RpfFile.LoadResourceFile(this, data, 162);

            Loaded = true;
        }
        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;

            if (entry is not RpfResourceFileEntry resentry)
            {
                ThrowFileIsNotAResourceException();
                return;
            }

            using var rd = new ResourceDataReader(resentry, data);

            Fragment = rd.ReadBlock<FragType>();

            if (Fragment != null)
            {
                Fragment.Yft = this;

                if (Fragment.Drawable != null)
                {
                    Fragment.Drawable.Owner = this;
                }
                if (Fragment.DrawableCloth != null)
                {
                    Fragment.DrawableCloth.Owner = this;
                }
            }

            Loaded = true;
        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Fragment, 162); //yft is type/version 162...

            return data;
        }

        public long PhysicalMemoryUsage => Fragment.Drawable.PhysicalMemoryUsage;
        public long VirtualMemoryUsage => Fragment.Drawable.VirtualMemoryUsage;
    }





    public class YftXml : MetaXmlBase
    {

        public static string GetXml(YftFile yft, string outputFolder = "")
        {
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.AppendLine(XmlHeader);

                if (yft?.Fragment != null)
                {
                    FragType.WriteXmlNode(yft.Fragment, sb, 0, outputFolder);
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }
    }

    public class XmlYft
    {

        public static YftFile GetYft(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYft(doc, inputFolder);
        }

        public static YftFile GetYft(XmlDocument doc, string inputFolder = "")
        {
            YftFile r = new YftFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.Fragment = FragType.ReadXmlNode(node, ddsfolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }




}
