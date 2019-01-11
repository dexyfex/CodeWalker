using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CarVariationsFile : GameFile, PackedFile
    {
        public CarVariationsFile() : base(null, GameFileType.CarVariations)
        { }
        public CarVariationsFile(RpfFileEntry entry) : base(entry, GameFileType.CarVariations)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;

            //TODO

            //can be PSO .ymt or XML .meta


            Loaded = true;
        }
    }
}
