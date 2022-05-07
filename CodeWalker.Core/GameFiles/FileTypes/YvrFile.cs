using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class YvrFile : GameFile, PackedFile
    {
        public VehicleRecordList Records { get; set; }

        public YvrFile() : base(null, GameFileType.Yvr)
        {
        }
        public YvrFile(RpfFileEntry entry) : base(entry, GameFileType.Yvr)
        {
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

            //MemoryUsage = 0;

            try
            {
                Records = rd.ReadBlock<VehicleRecordList>();

            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }



            Loaded = true;

        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Records, 1); //yvr is type/version 1...

            return data;
        }


    }


    public class YvrXml : MetaXmlBase
    {

        public static string GetXml(YvrFile yvr)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (yvr?.Records != null)
            {
                VehicleRecordList.WriteXmlNode(yvr.Records, sb, 0);
            }

            return sb.ToString();
        }

    }

    public class XmlYvr
    {

        public static YvrFile GetYvr(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYvr(doc, inputFolder);
        }

        public static YvrFile GetYvr(XmlDocument doc, string inputFolder = "")
        {
            YvrFile r = new YvrFile();

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.Records = VehicleRecordList.ReadXmlNode(node);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }

}
