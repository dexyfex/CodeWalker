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
    [TypeConverter(typeof(ExpandableObjectConverter))] public class YddFile : GameFile, PackedFile
    {
        public DrawableDictionary DrawableDict { get; set; }

        public Dictionary<uint, Drawable> Dict { get; set; }
        public Drawable[] Drawables { get; set; }

        public YddFile() : base(null, GameFileType.Ydd)
        {
        }
        public YddFile(RpfFileEntry entry) : base(entry, GameFileType.Ydd)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ydd file

            RpfFile.LoadResourceFile(this, data, 165);

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

            using var rd = new ResourceDataReader(resentry, data);

            DrawableDict = rd.ReadBlock<DrawableDictionary>();

            //MemoryUsage = 0; //uses decompressed filesize now...
            //if (DrawableDict != null)
            //{
            //    MemoryUsage += DrawableDict.MemoryUsage;
            //}

            if ((DrawableDict != null) && 
                (DrawableDict.Drawables != null) && 
                (DrawableDict.Drawables.data_items != null) && 
                (DrawableDict.Hashes != null))
            {
                Dict = new Dictionary<uint, Drawable>();
                var drawables = DrawableDict.Drawables.data_items;
                var hashes = DrawableDict.Hashes;
                for (int i = 0; (i < drawables.Length) && (i < hashes.Length); i++)
                {
                    var drawable = drawables[i];
                    var hash = hashes[i];
                    Dict[hash] = drawable;
                    drawable.Owner = this;
                }


                for (int i = 0; (i < drawables.Length) && (i < hashes.Length); i++)
                {
                    var drawable = drawables[i];
                    var hash = hashes[i];
                    drawable.Hash = hash;
                    if ((drawable.Name == null) || (drawable.Name.EndsWith("#dd")))
                    {
                        string hstr = JenkIndex.TryGetString(hash);
                        if (!string.IsNullOrEmpty(hstr))
                        {
                            drawable.Name = hstr;
                        }
                        else
                        { }
                    }
                }

                Drawables = Dict.Values.ToArray();

            }

            Loaded = true;

        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(DrawableDict, 165); //ydd is type/version 165...

            return data;
        }

        new public long MemoryUsage {
            get {
                return DrawableDict.MemoryUsage;
            }
        }

    }




    public class YddXml : MetaXmlBase
    {

        public static string GetXml(YddFile ydd, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (ydd?.DrawableDict != null)
            {
                DrawableDictionary.WriteXmlNode(ydd.DrawableDict, sb, 0, outputFolder);
            }

            return sb.ToString();
        }

    }

    public class XmlYdd
    {

        public static YddFile GetYdd(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYdd(doc, inputFolder);
        }

        public static YddFile GetYdd(XmlDocument doc, string inputFolder = "")
        {
            YddFile r = new YddFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.DrawableDict = DrawableDictionary.ReadXmlNode(node, ddsfolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }



}
