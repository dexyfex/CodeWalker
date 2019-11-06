using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class PedFile : GameFile, PackedFile
    {
        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }
        public string Xml { get; set; }

        public MCPedVariationInfo VariationInfo { get; set; }



        public string[] Strings { get; set; }




        public PedFile() : base(null, GameFileType.Ped)
        { }
        public PedFile(RpfFileEntry entry) : base(entry, GameFileType.Ped)
        { }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                NonMetaLoad(data);
                Loaded = true;
                return;
            }


            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();


            LoadMeta();



            Loaded = true;
        }



        private void LoadMeta()
        {
            var vVariationInfo = MetaTypes.GetTypedData<CPedVariationInfo>(Meta, MetaName.CPedVariationInfo);
            VariationInfo = new MCPedVariationInfo();
            VariationInfo.Load(Meta, vVariationInfo);

            Strings = MetaTypes.GetStrings(Meta);
            if (Strings != null)
            {
                foreach (string str in Strings)
                {
                    JenkIndex.Ensure(str); //just shove them in there
                }
            }
        }
        private void LoadPso()
        {

            var vVariationInfo = PsoTypes.GetRootItem<CPedVariationInfo>(Pso);
            VariationInfo = new MCPedVariationInfo();
            VariationInfo.Load(Pso, vVariationInfo);

        }



        private void NonMetaLoad(byte[] data)
        {
            //non meta not supported yet! but see what's in there...
            MemoryStream ms = new MemoryStream(data);
            if (RbfFile.IsRBF(ms))
            {
                Rbf = new RbfFile();
                Rbf.Load(ms);
            }
            else if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(ms);
                LoadPso();
            }
            else
            {
            }

        }












    }
}
