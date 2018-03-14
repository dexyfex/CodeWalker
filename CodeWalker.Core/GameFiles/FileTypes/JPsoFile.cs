using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class JPsoFile : PackedFile
    {
        public RpfFileEntry FileEntry { get; set; }
        public PsoFile Pso { get; set; }


        public void Load(byte[] data, RpfFileEntry entry)
        {
            //MemoryStream ms = new MemoryStream(data);

            FileEntry = entry;

            MemoryStream ms = new MemoryStream(data);

            if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(ms);

                //PsoTypes.EnsurePsoTypes(Pso);

                var root = PsoTypes.GetRootEntry(Pso);
                if (root != null)
                {
                }
                return;
            }
            else
            {

            }




        }

    }
}
