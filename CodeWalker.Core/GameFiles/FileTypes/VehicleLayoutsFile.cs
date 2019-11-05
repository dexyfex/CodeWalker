using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class VehicleLayoutsFile : GameFile, PackedFile
    {
        public string Xml { get; set; }

        public VehicleLayoutsFile() : base(null, GameFileType.VehicleLayouts)
        { }
        public VehicleLayoutsFile(RpfFileEntry entry) : base(entry, GameFileType.VehicleLayouts)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            //always XML .meta
            Xml = TextUtil.GetUTF8Text(data);

            //TODO: parse CVehicleMetadataMgr XML


            Loaded = true;
        }
    }
}
