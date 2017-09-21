using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    }





}
