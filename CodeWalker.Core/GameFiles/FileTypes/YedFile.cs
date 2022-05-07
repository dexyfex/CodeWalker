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
    public class YedFile : GameFile, PackedFile
    {
        public ExpressionDictionary ExpressionDictionary { get; set; }

        public string LoadException { get; set; }


        public Dictionary<MetaHash, Expression> ExprMap { get; set; }



        public YedFile() : base(null, GameFileType.Yed)
        {
        }
        public YedFile(RpfFileEntry entry) : base(entry, GameFileType.Yed)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
            //Hash = entry.ShortNameHash;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = null;
            try
            {
                rd = new ResourceDataReader(resentry, data);
            }
            catch (Exception ex)
            {
                //data = entry.File.DecompressBytes(data); //??
                LoadException = ex.ToString();
            }

            ExpressionDictionary = rd?.ReadBlock<ExpressionDictionary>();


            InitDictionaries();
        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(ExpressionDictionary, 25); //yed is type/version 25...

            return data;
        }


        public void InitDictionaries()
        {
            ExprMap = ExpressionDictionary?.ExprMap ?? new Dictionary<MetaHash, Expression>();

        }
    }


    public class YedXml : MetaXmlBase
    {

        public static string GetXml(YedFile yed, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (yed?.ExpressionDictionary != null)
            {
                ExpressionDictionary.WriteXmlNode(yed.ExpressionDictionary, sb, 0);
            }

            return sb.ToString();
        }

    }

    public class XmlYed
    {

        public static YedFile GetYed(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYed(doc, inputFolder);
        }

        public static YedFile GetYed(XmlDocument doc, string inputFolder = "")
        {
            YedFile r = new YedFile();

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.ExpressionDictionary = ExpressionDictionary.ReadXmlNode(node);

                r.InitDictionaries();
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }


}
