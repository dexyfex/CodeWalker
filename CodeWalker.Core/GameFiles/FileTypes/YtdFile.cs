using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YtdFile : GameFile, PackedFile
    {
        public TextureDictionary TextureDict { get; set; }


        public YtdFile() : base(null, GameFileType.Ytd)
        {
        }
        public YtdFile(RpfFileEntry entry) : base(entry, GameFileType.Ytd)
        {
        }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);


            TextureDict = rd.ReadBlock<TextureDictionary>();

            //MemoryUsage = 0; //uses decompressed file size now..
            //if (TextureDict != null)
            //{
            //    MemoryUsage += TextureDict.MemoryUsage;
            //}

        }


    }
}
