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
    public class YldFile : GameFile, PackedFile
    {
        public ClothDictionary ClothDictionary { get; set; }

        public Dictionary<uint, CharacterCloth> Dict { get; set; }


        public string LoadException { get; set; }


        public YldFile() : base(null, GameFileType.Yld)
        {
        }
        public YldFile(RpfFileEntry entry) : base(entry, GameFileType.Yld)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
            //Hash = entry.ShortNameHash;


            if (entry is not RpfResourceFileEntry resentry)
            {
                ThrowFileIsNotAResourceException();
                return;
            }


            try
            {
                using var rd = new ResourceDataReader(resentry, data);

                ClothDictionary = rd?.ReadBlock<ClothDictionary>();

                if (ClothDictionary != null)
                {
                    Dict = new Dictionary<uint, CharacterCloth>();
                    int n = ClothDictionary.ClothNameHashes?.data_items?.Length ?? 0;
                    for (int i = 0; i < n; i++)
                    {
                        if (i >= (ClothDictionary.Clothes?.data_items?.Length ?? 0)) break;

                        var hash = ClothDictionary.ClothNameHashes.data_items[i];
                        var cloth = ClothDictionary.Clothes.data_items[i];

                        Dict[hash] = cloth;
                    }
                }

                Loaded = true;
            }
            catch (Exception ex)
            {
                //data = entry.File.DecompressBytes(data); //??
                LoadException = ex.ToString();
            }
        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(ClothDictionary, 8); //yld is type/version 8...

            return data;
        }

    }



    public class YldXml : MetaXmlBase
    {

        public static string GetXml(YldFile yld, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (yld?.ClothDictionary != null)
            {
                ClothDictionary.WriteXmlNode(yld.ClothDictionary, sb, 0);
            }

            return sb.ToString();
        }

    }

    public class XmlYld
    {

        public static YldFile GetYld(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYld(doc, inputFolder);
        }

        public static YldFile GetYld(XmlDocument doc, string inputFolder = "")
        {
            YldFile r = new YldFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.ClothDictionary = ClothDictionary.ReadXmlNode(node);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }

}
