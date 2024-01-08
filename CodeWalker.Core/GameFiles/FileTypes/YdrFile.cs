using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class YdrFile : GameFile, PackedFile
    {
        public Drawable Drawable { get; set; }

        public YdrFile() : base(null, GameFileType.Ydr)
        {
        }
        public YdrFile(RpfFileEntry entry) : base(entry, GameFileType.Ydr)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ydr file

            RpfFile.LoadResourceFile(this, data, 165);

            Loaded = true;
        }

        public async Task LoadAsync(byte[] data)
        {
            //direct load from a raw, compressed ydr file

            await RpfFile.LoadResourceFileAsync(this, data, 165);

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

            //MemoryUsage = 0;

            try
            {
                Drawable = rd.ReadBlock<Drawable>();
                Drawable.Owner = this;
                //MemoryUsage += Drawable.MemoryUsage; //uses decompressed filesize now...
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }

            Loaded = true;

        }

        public void Unload()
        {

        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Drawable, 165); //ydr is type/version 165...

            return data;
        }

        public long PhysicalMemoryUsage => Drawable.PhysicalMemoryUsage;
        public long VirtualMemoryUsage => Drawable.VirtualMemoryUsage;
    }




    public class YdrXml : MetaXmlBase
    {
        public static string GetXml(YdrFile ydr, string outputFolder = "")
        {
            StringBuilder sb = StringBuilderPool.Get();
            try
            {
                sb.AppendLine(XmlHeader);

                if (ydr?.Drawable != null)
                {
                    Drawable.WriteXmlNode(ydr.Drawable, sb, 0, outputFolder);
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }
    }

    public class XmlYdr
    {

        public static YdrFile GetYdr(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYdr(doc, inputFolder);
        }

        public static YdrFile GetYdr(XmlDocument doc, string inputFolder = "")
        {
            YdrFile r = new YdrFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.Drawable = Drawable.ReadXmlNode(node, ddsfolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }



}
