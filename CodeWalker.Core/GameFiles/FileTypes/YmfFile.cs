using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public class YmfFile : PackedFile
    {
        public RpfFileEntry? FileEntry { get; set; }

        public Meta? Meta { get; set; }
        public PsoFile? Pso { get; set; }
        public RbfFile? Rbf { get; set; }

        public YmfMapDataGroup[] MapDataGroups { get; set; } = Array.Empty<YmfMapDataGroup>();
        public CImapDependency[] imapDependencies { get; set; } = Array.Empty<CImapDependency>();
        public YmfImapDependency2[] imapDependencies2 { get; set; } = Array.Empty<YmfImapDependency2>();
        public YmfItypDependency2[] itypDependencies2 { get; set; } = Array.Empty<YmfItypDependency2>();
        public CHDTxdAssetBinding[] HDTxdAssetBindings { get; set; } = Array.Empty<CHDTxdAssetBinding>();
        public YmfInterior[] Interiors { get; set; } = Array.Empty<YmfInterior>();

        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;

            if (entry is not RpfResourceFileEntry resentry)
            {
                if (RbfFile.IsRBF(data.AsSpan(0, 4)))
                {
                    Rbf = new RbfFile();
                    Rbf.Load(data);

                    //x64j.rpf\\levels\\gta5\\_citye\\indust_01\\id1_props.rpf\\_manifest.ymf
                    //x64j.rpf\\levels\\gta5\\_citye\\indust_02\\id2_props.rpf\\_manifest.ymf
                    //x64q.rpf\\levels\\gta5\\_hills\\country_01\\cs1_railwyc.rpf\\_manifest.ymf
                    //all just HDTxd bindings

                    return;
                }
                if (PsoFile.IsPSO(data.AsSpan(0, 4)))
                {
                    Pso = new PsoFile();
                    Pso.Load(data);

                    //PsoTypes.EnsurePsoTypes(Pso);

                    ProcessPSO();

                    return;
                }
                else
                {
                    Console.WriteLine("Neither");
                }
                return;
            }

            using var rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();
        }


        private void ProcessPSO()
        {

            //See x64m.rpf\levels\gta5\_cityw\venice_01\venice_metadata.rpf\_manifest.ymf
            //for TIMED YMAP stuff!!!!
            //check CMapDataGroup.HoursOnOff

            ArgumentNullException.ThrowIfNull(Pso, nameof(Pso));


            var d = PsoTypes.GetRootItem<CPackFileMetaData>(Pso);

            MapDataGroups = PsoTypes.GetObjectArray<YmfMapDataGroup, CMapDataGroup>(Pso, in d._MapDataGroups) ?? Array.Empty<YmfMapDataGroup>();

            imapDependencies = PsoTypes.GetItemArray<CImapDependency>(Pso, in d._imapDependencies)?.ToArray() ?? Array.Empty<CImapDependency>();

            imapDependencies2 = PsoTypes.GetObjectArray<YmfImapDependency2, CImapDependencies>(Pso, in d._imapDependencies_2) ?? Array.Empty<YmfImapDependency2>();

            itypDependencies2 = PsoTypes.GetObjectArray<YmfItypDependency2, CItypDependencies>(Pso, in d._itypDependencies_2) ?? Array.Empty<YmfItypDependency2>();

            HDTxdAssetBindings = PsoTypes.GetItemArray<CHDTxdAssetBinding>(Pso, in d._HDTxdBindingArray)?.ToArray() ?? Array.Empty<CHDTxdAssetBinding>();

            Interiors = PsoTypes.GetObjectArray<YmfInterior, CInteriorBoundsFiles>(Pso, in d._Interiors) ?? Array.Empty<YmfInterior>();
        }


        public override string ToString()
        {
            return (FileEntry != null) ? FileEntry.Path : string.Empty;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmfMapDataGroup : PsoClass<CMapDataGroup>
    {
        public CMapDataGroup DataGroup { get; set; } //ymap name
        public MetaHash[] Bounds { get; set; } = Array.Empty<MetaHash>();
        public MetaHash[] WeatherTypes { get; set; } = Array.Empty<MetaHash>();
        public MetaHash Name { get; set; }
        public ushort Flags { get; set; }
        public uint HoursOnOff { get; set; }

        public override string ToString()
        {
            return DataGroup.ToString();
        }

        public override void Init(PsoFile pso, in CMapDataGroup v)
        {
            DataGroup = v;
            Bounds = PsoTypes.GetHashArray(pso, v.Bounds) ?? Array.Empty<MetaHash>();
            WeatherTypes = PsoTypes.GetHashArray(pso, v.WeatherTypes) ?? Array.Empty<MetaHash>();
            Name = v.Name;
            Flags = v.Flags;
            HoursOnOff = v.HoursOnOff;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmfImapDependency2 : PsoClass<CImapDependencies>
    {
        public CImapDependencies Dep { get; set; }
        public MetaHash[] itypDepArray { get; set; }//ybn hashes?

        public override void Init(PsoFile pso, in CImapDependencies v)
        {
            Dep = v;
            itypDepArray = PsoTypes.GetHashArray(pso, v.itypDepArray);
        }

        public override string ToString()
        {
            return Dep.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmfItypDependency2 : PsoClass<CItypDependencies>
    {
        public CItypDependencies Dep { get; set; }
        public MetaHash[] itypDepArray { get; set; }//ytyp hashes?

        public override void Init(PsoFile pso, in CItypDependencies v)
        {
            Dep = v;
            itypDepArray = PsoTypes.GetHashArray(pso, v.itypDepArray);
        }

        public override string ToString()
        {
            return Dep.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmfInterior : PsoClass<CInteriorBoundsFiles>
    {
        public CInteriorBoundsFiles Interior { get; set; }
        public MetaHash[] Bounds { get; set; }//ybn hashes?

        public override string ToString() => Interior.ToString();

        public override void Init(PsoFile pso, in CInteriorBoundsFiles v)
        {
            Interior = v;
            Bounds = PsoTypes.GetHashArray(pso, in v._Bounds);
        }
    }

}
