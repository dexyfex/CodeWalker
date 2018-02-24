using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YftFile : GameFile, PackedFile
    {
        public FragType Fragment { get; set; }

        public YftFile() : base(null, GameFileType.Yft)
        {
        }
        public YftFile(RpfFileEntry entry) : base(entry, GameFileType.Yft)
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

            Fragment = rd.ReadBlock<FragType>();

            if (Fragment.Drawable != null)
            {
                Fragment.Drawable.Owner = this;
            }
            if (Fragment.Unknown_F8h_Data != null)
            {
                Fragment.Unknown_F8h_Data.Owner = this;
            }

            Loaded = true;
        }


    }
}
