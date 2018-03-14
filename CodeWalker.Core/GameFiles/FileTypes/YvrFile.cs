using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YvrFile : GameFile, PackedFile
    {
        public VehicleRecordList Records { get; set; }

        public YvrFile() : base(null, GameFileType.Yvr)
        {
        }
        public YvrFile(RpfFileEntry entry) : base(entry, GameFileType.Yvr)
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
                Records = rd.ReadBlock<VehicleRecordList>();

            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }



            Loaded = true;

        }


    }
}
