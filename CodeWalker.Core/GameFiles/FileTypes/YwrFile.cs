using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class YwrFile : GameFile, PackedFile
    {
        public WaypointRecordList Waypoints { get; set; }

        public YwrFile() : base(null, GameFileType.Ywr)
        {
        }
        public YwrFile(RpfFileEntry entry) : base(entry, GameFileType.Ywr)
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
                Waypoints = rd.ReadBlock<WaypointRecordList>();

            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }



            Loaded = true;

        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Waypoints, 1); //ywr is type/version 1...

            return data;
        }


    }


    public class YwrXml : MetaXmlBase
    {

        public static string GetXml(YwrFile ywr)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (ywr?.Waypoints != null)
            {
                WaypointRecordList.WriteXmlNode(ywr.Waypoints, sb, 0);
            }

            return sb.ToString();
        }

    }

    public class XmlYwr
    {

        public static YwrFile GetYwr(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYwr(doc, inputFolder);
        }

        public static YwrFile GetYwr(XmlDocument doc, string inputFolder = "")
        {
            YwrFile r = new YwrFile();

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.Waypoints = WaypointRecordList.ReadXmlNode(node);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }


}
