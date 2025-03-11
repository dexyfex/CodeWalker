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
    public class YtdFile : GameFile, PackedFile
    {
        public TextureDictionary TextureDict { get; set; }


        public YtdFile() : base(null, GameFileType.Ytd)
        {
        }
        public YtdFile(RpfFileEntry entry) : base(entry, GameFileType.Ytd)
        {
        }


        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ytd file

            RpfFile.LoadResourceFile(this, data, (uint)GetVersion(RpfManager.IsGen9));

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


            TextureDict = rd.ReadBlock<TextureDictionary>();

            //MemoryUsage = 0; //uses decompressed file size now..
            //if (TextureDict != null)
            //{
            //    MemoryUsage += TextureDict.MemoryUsage;
            //}

            //var analyzer = new ResourceAnalyzer(rd);

        }


        public byte[] Save()
        {
            var gen9 = RpfManager.IsGen9;
            if (gen9)
            {
                TextureDict?.EnsureGen9();
            }

            byte[] data = ResourceBuilder.Build(TextureDict, GetVersion(gen9), true, gen9);

            return data;
        }

        public int GetVersion(bool gen9)
        {
            return gen9 ? 5 : 13;
        }

    }




    public class YtdXml : MetaXmlBase
    {

        public static string GetXml(YtdFile ytd, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (ytd?.TextureDict != null)
            {
                TextureDictionary.WriteXmlNode(ytd.TextureDict, sb, 0, outputFolder);
            }

            return sb.ToString();
        }

    }

    public class XmlYtd
    {

        public static YtdFile GetYtd(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYtd(doc, inputFolder);
        }

        public static YtdFile GetYtd(XmlDocument doc, string inputFolder = "")
        {
            YtdFile r = new YtdFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.TextureDict = TextureDictionary.ReadXmlNode(node, ddsfolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }



}
