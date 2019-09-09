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
        public RpfFileEntry FileEntry { get; set; }

        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }

        public YmfMapDataGroup[] MapDataGroups { get; set; }
        public CImapDependency[] imapDependencies { get; set; }
        public YmfImapDependency2[] imapDependencies2 { get; set; }
        public YmfItypDependency2[] itypDependencies2 { get; set; }
        public CHDTxdAssetBinding[] HDTxdAssetBindings { get; set; }
        public YmfInterior[] Interiors { get; set; }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            FileEntry = entry;

            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                MemoryStream ms = new MemoryStream(data);
                if (RbfFile.IsRBF(ms))
                {
                    Rbf = new RbfFile();
                    Rbf.Load(ms);

                    //x64j.rpf\\levels\\gta5\\_citye\\indust_01\\id1_props.rpf\\_manifest.ymf
                    //x64j.rpf\\levels\\gta5\\_citye\\indust_02\\id2_props.rpf\\_manifest.ymf
                    //x64q.rpf\\levels\\gta5\\_hills\\country_01\\cs1_railwyc.rpf\\_manifest.ymf
                    //all just HDTxd bindings

                    return;
                }
                if (PsoFile.IsPSO(ms))
                {
                    Pso = new PsoFile();
                    Pso.Load(ms);

                    //PsoTypes.EnsurePsoTypes(Pso);

                    ProcessPSO();

                    return;
                }
                else
                {

                }
                return;
            }
            else
            { }//doesn't get here





            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();





        }


        private void ProcessPSO()
        {

            //See x64m.rpf\levels\gta5\_cityw\venice_01\venice_metadata.rpf\_manifest.ymf
            //for TIMED YMAP stuff!!!!
            //check CMapDataGroup.HoursOnOff


            var d = PsoTypes.GetRootItem<CPackFileMetaData>(Pso);

            MapDataGroups = PsoTypes.GetObjectArray<YmfMapDataGroup, CMapDataGroup>(Pso, d.MapDataGroups);

            imapDependencies = PsoTypes.GetItemArray<CImapDependency>(Pso, d.imapDependencies);

            imapDependencies2 = PsoTypes.GetObjectArray<YmfImapDependency2, CImapDependencies>(Pso, d.imapDependencies_2);

            itypDependencies2 = PsoTypes.GetObjectArray<YmfItypDependency2, CItypDependencies>(Pso, d.itypDependencies_2);

            HDTxdAssetBindings = PsoTypes.GetItemArray<CHDTxdAssetBinding>(Pso, d.HDTxdBindingArray);

            Interiors = PsoTypes.GetObjectArray<YmfInterior, CInteriorBoundsFiles>(Pso, d.Interiors);


        }


        public override string ToString()
        {
            return (FileEntry != null) ? FileEntry.Path : string.Empty;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmfMapDataGroup : PsoClass<CMapDataGroup>
    {
        public CMapDataGroup DataGroup { get; set; } //ymap name
        public MetaHash[] Bounds { get; set; }
        public MetaHash[] WeatherTypes { get; set; }
        public MetaHash Name { get; set; }
        public ushort Flags { get; set; }
        public uint HoursOnOff { get; set; }

        public override string ToString()
        {
            return DataGroup.ToString();
        }

        public override void Init(PsoFile pso, ref CMapDataGroup v)
        {
            DataGroup = v;
            Bounds = PsoTypes.GetHashArray(pso, v.Bounds);
            WeatherTypes = PsoTypes.GetHashArray(pso, v.WeatherTypes);
            Name = v.Name;
            Flags = v.Flags;
            HoursOnOff = v.HoursOnOff;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmfImapDependency2 : PsoClass<CImapDependencies>
    {
        public CImapDependencies Dep { get; set; }
        public MetaHash[] itypDepArray { get; set; }//ybn hashes?

        public override void Init(PsoFile pso, ref CImapDependencies v)
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

        public override void Init(PsoFile pso, ref CItypDependencies v)
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

        public override string ToString()
        {
            return Interior.ToString();
        }

        public override void Init(PsoFile pso, ref CInteriorBoundsFiles v)
        {
            Interior = v;
            Bounds = PsoTypes.GetHashArray(pso, v.Bounds);
        }
    }

}
