using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CarModColsFile : GameFile, PackedFile
    {
        public CarModColsFile() : base(null, GameFileType.CarModCols)
        { }
        public CarModColsFile(RpfFileEntry entry) : base(entry, GameFileType.CarModCols)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;

            //TODO

            //always PSO .ymt


            Loaded = true;
        }
    }
}
