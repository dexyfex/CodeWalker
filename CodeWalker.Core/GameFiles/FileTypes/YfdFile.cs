using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YfdFile : GameFile, PackedFile
    {
        public FrameFilterDictionary FrameFilterDictionary { get; set; }

        public string LoadException { get; set; }


        public YfdFile() : base(null, GameFileType.Yfd)
        {
        }
        public YfdFile(RpfFileEntry entry) : base(entry, GameFileType.Yfd)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
            //Hash = entry.ShortNameHash;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = null;
            try
            {
                rd = new ResourceDataReader(resentry, data);
            }
            catch (Exception ex)
            {
                //data = entry.File.DecompressBytes(data); //??
                LoadException = ex.ToString();
            }

            FrameFilterDictionary = rd?.ReadBlock<FrameFilterDictionary>();

        }
    }
}
