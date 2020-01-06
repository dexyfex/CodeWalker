using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YbnFile : GameFile, PackedFile
    {
        public Bounds Bounds { get; set; }


        //used by the editor:
        public bool HasChanged { get; set; } = false;


        public YbnFile() : base(null, GameFileType.Ybn)
        {
        }
        public YbnFile(RpfFileEntry entry) : base(entry, GameFileType.Ybn)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ybn file

            RpfFile.LoadResourceFile(this, data, 43);

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


            Bounds = rd.ReadBlock<Bounds>();

            Bounds.OwnerYbn = this;
            Bounds.OwnerName = entry.Name;

            Loaded = true;
        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Bounds, 43); //ybn is type/version 43...

            return data;
        }




        public bool RemoveBounds(Bounds b)
        {
            return false;
        }
        public bool RemovePoly(BoundPolygon p)
        {
            return false;
        }

    }
}
