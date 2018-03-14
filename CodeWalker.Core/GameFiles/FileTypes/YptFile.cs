using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YptFile : GameFile, PackedFile
    {
        public ParticleEffectsList PtfxList { get; set; }

        public Dictionary<uint, Drawable> DrawableDict { get; set; }

        public string ErrorMessage { get; set; }


        public YptFile() : base(null, GameFileType.Ypt)
        {
        }
        public YptFile(RpfFileEntry entry) : base(entry, GameFileType.Ypt)
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
                PtfxList = rd.ReadBlock<ParticleEffectsList>();
                //Drawable.Owner = this;
                //MemoryUsage += Drawable.MemoryUsage; //uses decompressed filesize now...
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
            }


            var dDict = PtfxList?.DrawableDictionary;

            if ((dDict != null) &&
                (dDict.Drawables != null) &&
                (dDict.Drawables.data_items != null) &&
                (dDict.Hashes != null))
            {
                DrawableDict = new Dictionary<uint, Drawable>();
                var drawables = dDict.Drawables.data_items;
                var hashes = dDict.Hashes;
                for (int i = 0; (i < drawables.Length) && (i < hashes.Length); i++)
                {
                    var drawable = drawables[i];
                    var hash = hashes[i];
                    DrawableDict[hash] = drawable;
                    drawable.Owner = this;
                }

                for (int i = 0; (i < drawables.Length) && (i < hashes.Length); i++)
                {
                    var drawable = drawables[i];
                    var hash = hashes[i];
                    if ((drawable.Name == null) || (drawable.Name.EndsWith("#dd")))
                    {
                        string hstr = JenkIndex.TryGetString(hash);
                        if (!string.IsNullOrEmpty(hstr))
                        {
                            drawable.Name = hstr;
                        }
                        else
                        {
                            drawable.Name = "0x" + hash.ToString("X").PadLeft(8, '0');
                        }
                    }
                }

            }





            Loaded = true;

        }


    }





}
