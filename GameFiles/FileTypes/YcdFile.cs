using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YcdFile : GameFile, PackedFile
    {
        public ClipDictionary ClipDictionary { get; set; }

        public Dictionary<MetaHash, ClipMapEntry> ClipMap { get; set; }

        public YcdFile() : base(null, GameFileType.Ycd)
        {
        }
        public YcdFile(RpfFileEntry entry) : base(entry, GameFileType.Ycd)
        {
        }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            //Name = entry.Name;
            //Hash = entry.ShortNameHash;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);


            ClipDictionary = rd.ReadBlock<ClipDictionary>();

            ClipMap = new Dictionary<MetaHash, ClipMapEntry>();
            if ((ClipDictionary != null) && (ClipDictionary.Clips != null) && (ClipDictionary.Clips.data_items != null))
            {
                foreach (var cme in ClipDictionary.Clips.data_items)
                {
                    if (cme != null)
                    {
                        ClipMap[cme.Hash] = cme;
                    }
                }
            }


        }
    }
}
