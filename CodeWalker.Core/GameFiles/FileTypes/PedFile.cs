using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Buffers.Binary;
using System.Buffers;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class PedFile : GameFile, PackedFile
    {
        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }
        public string Xml { get; set; }

        public MetaHash DlcName => VariationInfo.Data.dlcName;
        public string GameDlcName { get; set; }
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


            if (entry is not RpfResourceFileEntry resentry)
            {
                NonMetaLoad(data);
                Loaded = true;
                return;
            }


            using var rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();


            LoadMeta();



            Loaded = true;
        }



        private void LoadMeta()
        {
            var vVariationInfo = MetaTypes.GetTypedData<CPedVariationInfo>(Meta, MetaName.CPedVariationInfo);
            VariationInfo = new MCPedVariationInfo();
            VariationInfo.Load(Meta, in vVariationInfo);

            Strings = MetaTypes.GetStrings(Meta);
            if (Strings is not null)
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
            VariationInfo.Load(Pso, in vVariationInfo);

        }


        private void NonMetaLoad(byte[] data)
        {
            //non meta not supported yet! but see what's in there...
            if (RbfFile.IsRBF(data.AsSpan(0, 4)))
            {
                Rbf = new RbfFile();
                var sequence = new ReadOnlySequence<byte>(data);
                var reader = new SequenceReader<byte>(sequence);
                Rbf.Load(ref reader);
            }
            else if (PsoFile.IsPSO(data.AsSpan(0, 4)))
            {
                Pso = new PsoFile();
                Pso.Load(data);
                LoadPso();
            }
        }

        public override string ToString()
        {
            return Path.GetFileName(RpfFileEntry.Parent.Path);
        }
    }
}
