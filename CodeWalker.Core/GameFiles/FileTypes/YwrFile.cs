using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YwrFile : GameFile, PackedFile
    {
        public WaypointRecordList Waypoints { get; set; }

        public YwrFile() : base(null, GameFileType.Ywr)
        {
        }
        public YwrFile(RpfFileEntry entry) : base(entry, GameFileType.Ywr)
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
                Waypoints = rd.ReadBlock<WaypointRecordList>();

            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }



            Loaded = true;

        }


    }
}
