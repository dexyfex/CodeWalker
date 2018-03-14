using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
