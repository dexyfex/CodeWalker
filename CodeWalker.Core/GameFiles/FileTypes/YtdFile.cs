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


        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;


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

        }


        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(TextureDict, 13); //ytd is type/version 13...

            return data;
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
