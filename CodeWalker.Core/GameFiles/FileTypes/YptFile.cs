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
    public class YptFile : GameFile, PackedFile
    {
        public ParticleEffectsList PtfxList { get; set; }

        public Dictionary<uint, DrawableBase> DrawableDict { get; set; }

        public Dictionary<MetaHash, ParticleEffectRule> EffectDict { get; set; }
        public ParticleEffectRule[] AllEffects { get; set; }

        public string ErrorMessage { get; set; }

#if DEBUG
        public ResourceAnalyzer Analyzer { get; set; }
#endif

        public YptFile() : base(null, GameFileType.Ypt)
        {
        }
        public YptFile(RpfFileEntry entry) : base(entry, GameFileType.Ypt)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ypt file

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

            if (rd.IsGen9)
            {
                switch (resentry.Version)
                {
                    case 71:
                        break;
                    case 68:
                        rd.IsGen9 = false;
                        break;
                    default:
                        break;
                }
            }

            //MemoryUsage = 0;

            try
            {
                PtfxList = rd.ReadBlock<ParticleEffectsList>();
                //Drawable.Owner = this;
                //MemoryUsage += Drawable.MemoryUsage; //uses decompressed filesize now...
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
            }


            BuildDrawableDict();
            BuildParticleDict();

#if DEBUG
            Analyzer = new ResourceAnalyzer(rd);
#endif

            Loaded = true;

        }


        public byte[] Save()
        {
            var drawables = PtfxList?.DrawableDictionary?.Drawables?.data_items;
            var gen9 = RpfManager.IsGen9;
            if (gen9)
            {
                PtfxList?.TextureDictionary?.EnsureGen9();
                if (drawables != null)
                {
                    foreach (var drawable in drawables)
                    {
                        drawable?.EnsureGen9();
                    }
                }
            }

            byte[] data = ResourceBuilder.Build(PtfxList, GetVersion(gen9), true, gen9);

            return data;
        }

        public int GetVersion(bool gen9)
        {
            return gen9 ? 71 : 68;
        }




        private void BuildDrawableDict()
        {
            var dDict = PtfxList?.DrawableDictionary;

            if ((dDict?.Drawables?.data_items != null) && (dDict?.Hashes != null))
            {
                DrawableDict = new Dictionary<uint, DrawableBase>();
                var drawables = dDict.Drawables.data_items;
                var hashes = dDict.Hashes;
                for (int i = 0; (i < drawables.Length) && (i < hashes.Length); i++)
                {
                    var drawable = drawables[i];
                    var hash = hashes[i];
                    DrawableDict[hash] = drawable;
                    drawable.Owner = this;
                }

                //for (int i = 0; (i < drawables.Length) && (i < hashes.Length); i++)
                //{
                //    var drawable = drawables[i];
                //    var hash = hashes[i];
                //    if ((drawable.Name == null) || (drawable.Name.EndsWith("#dd")))
                //    {
                //        string hstr = JenkIndex.TryGetString(hash);
                //        if (!string.IsNullOrEmpty(hstr))
                //        {
                //            drawable.Name = hstr;
                //        }
                //        else
                //        {
                //            drawable.Name = "0x" + hash.ToString("X").PadLeft(8, '0');
                //        }
                //    }
                //}
            }

        }

        private void BuildParticleDict()
        {
            var pdict = PtfxList?.EffectRuleDictionary;

            if (pdict?.EffectRules?.data_items != null)
            {

                EffectDict = new Dictionary<MetaHash, ParticleEffectRule>();
                var elist = new List<ParticleEffectRule>();

                foreach (var e in pdict.EffectRules.data_items)
                {
                    EffectDict[e.NameHash] = e;
                    elist.Add(e);
                }

                elist.Sort((a, b) => { return (a.Name?.Value ?? "").CompareTo(b.Name?.Value ?? ""); });
                AllEffects = elist.ToArray();

            }

        }

    }





    public class YptXml : MetaXmlBase
    {

        public static string GetXml(YptFile ypt, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if (ypt?.PtfxList != null)
            {
                ParticleEffectsList.WriteXmlNode(ypt.PtfxList, sb, 0, outputFolder);
            }

            return sb.ToString();
        }

    }

    public class XmlYpt
    {

        public static YptFile GetYpt(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYpt(doc, inputFolder);
        }

        public static YptFile GetYpt(XmlDocument doc, string inputFolder = "")
        {
            YptFile r = new YptFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.PtfxList = ParticleEffectsList.ReadXmlNode(node, ddsfolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }




}
