using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YedFile : GameFile, PackedFile
    {
        public ExpressionDictionary ExpressionDictionary { get; set; }

        public string LoadException { get; set; }


        public Dictionary<MetaHash, Expression> ExprMap { get; set; }



        public YedFile() : base(null, GameFileType.Yed)
        {
        }
        public YedFile(RpfFileEntry entry) : base(entry, GameFileType.Yed)
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

            ExpressionDictionary = rd?.ReadBlock<ExpressionDictionary>();


            InitDictionaries();
        }

        public void InitDictionaries()
        {
            ExprMap = ExpressionDictionary?.ExprMap ?? new Dictionary<MetaHash, Expression>();

        }
    }
}
