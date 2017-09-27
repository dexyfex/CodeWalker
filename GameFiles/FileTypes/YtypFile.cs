using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YtypFile : PackedFile
    {

        public RpfFileEntry FileEntry { get; set; }

        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }


        public uint NameHash { get; set; }
        public string[] Strings { get; set; }


        public CMapTypes CMapTypes { get; set; }

        public Archetype[] AllArchetypes { get; set; }

        public MetaWrapper[] Extensions { get; set; }

        public CCompositeEntityType[] CompositeEntityTypes { get; set; }




        public override string ToString()
        {
            return (FileEntry != null) ? FileEntry.Name : string.Empty;
        }

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
                }
                else if (PsoFile.IsPSO(ms))
                {
                    Pso = new PsoFile();
                    Pso.Load(ms);
                    //PsoTypes.EnsurePsoTypes(Pso);
                }
                else
                {
                }
                return;
            }





            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();


            CMapTypes = MetaTypes.GetTypedData<CMapTypes>(Meta, MetaName.CMapTypes);


            List<Archetype> allarchs = new List<Archetype>();

            var ptrs = MetaTypes.GetPointerArray(Meta, CMapTypes.archetypes);
            if (ptrs != null)
            {
                for (int i = 0; i < ptrs.Length; i++)
                {
                    var ptr = ptrs[i];
                    int blocki = ptr.BlockID - 1;
                    int offset = ptr.ItemOffset * 16;//block data size...
                    if (blocki >= Meta.DataBlocks.Count)
                    { continue; }
                    var block = Meta.DataBlocks[blocki];
                    if ((offset < 0) || (block.Data == null) || (offset >= block.Data.Length))
                    { continue; }

                    var ba = new Archetype();
                    switch (block.StructureNameHash)
                    {
                        case MetaName.CBaseArchetypeDef:
                            var basearch = PsoTypes.ConvertDataRaw<CBaseArchetypeDef>(block.Data, offset);
                            ba.Init(this, ref basearch);
                            ba.Extensions = MetaTypes.GetExtensions(Meta, basearch.extensions);
                            break;
                        case MetaName.CTimeArchetypeDef:
                            var timearch = PsoTypes.ConvertDataRaw<CTimeArchetypeDef>(block.Data, offset);
                            ba.Init(this, ref timearch);
                            ba.Extensions = MetaTypes.GetExtensions(Meta, timearch.BaseArchetypeDef.extensions);
                            break;
                        case MetaName.CMloArchetypeDef:
                            var mloarch = PsoTypes.ConvertDataRaw<CMloArchetypeDef>(block.Data, offset);
                            ba.Init(this, ref mloarch);
                            ba.Extensions = MetaTypes.GetExtensions(Meta, mloarch.BaseArchetypeDef.extensions);

                            MloArchetypeData mlod = new MloArchetypeData();
                            var mlodef = mloarch.MloArchetypeDef;
                            mlod.entities = MetaTypes.ConvertDataArray<CEntityDef>(Meta, MetaName.CEntityDef, mlodef.entities);
                            mlod.rooms = MetaTypes.ConvertDataArray<CMloRoomDef>(Meta, MetaName.CMloRoomDef, mlodef.rooms);
                            mlod.portals = MetaTypes.ConvertDataArray<CMloPortalDef>(Meta, MetaName.CMloPortalDef, mlodef.portals);
                            mlod.entitySets = MetaTypes.ConvertDataArray<CMloEntitySet>(Meta, MetaName.CMloEntitySet, mlodef.entitySets);
                            mlod.timeCycleModifiers = MetaTypes.ConvertDataArray<CMloTimeCycleModifier>(Meta, MetaName.CMloTimeCycleModifier, mlodef.timeCycleModifiers);
                            ba.MloData = mlod;

                            if (mlod.entities != null)
                            {
                                //for (int e = 0; e < mlod.entities.Length; e++)
                                //{
                                //    if (mlod.entities[e].extensions.Count1 > 0)
                                //    {
                                //        var exts = MetaTypes.GetExtensions(Meta, mlod.entities[e].extensions);
                                //        if (exts != null)
                                //        { }
                                //    }
                                //}
                            }

                            break;
                        default:
                            continue;
                    }
                    

                    allarchs.Add(ba);
                }
            }
            AllArchetypes = allarchs.ToArray();


            Extensions = MetaTypes.GetExtensions(Meta, CMapTypes.extensions);
            if (Extensions != null)
            { }


            //AudioEmitters = MetaTypes.GetTypedDataArray<CExtensionDefAudioEmitter>(Meta, MetaName.CExtensionDefAudioEmitter);
            //if (AudioEmitters != null)
            //{ }

            //CEntityDefs = MetaTypes.GetTypedDataArray<CEntityDef>(Meta, MetaName.CEntityDef);


            CompositeEntityTypes = MetaTypes.ConvertDataArray<CCompositeEntityType>(Meta, MetaName.CCompositeEntityType, CMapTypes.compositeEntityTypes);
            if (CompositeEntityTypes != null)
            { }

            NameHash = CMapTypes.name;
            if (NameHash == 0)
            {
                int ind = entry.NameLower.LastIndexOf('.');
                if (ind > 0)
                {
                    NameHash = JenkHash.GenHash(entry.NameLower.Substring(0, ind));
                }
                else
                {
                    NameHash = JenkHash.GenHash(entry.NameLower);
                }
            }

            Strings = MetaTypes.GetStrings(Meta);
            if (Strings != null)
            {
                foreach (string str in Strings)
                {
                    JenkIndex.Ensure(str); //just shove them in there
                }
            }


            //foreach (var block in Meta.DataBlocks)
            //{
            //    switch(block.StructureNameHash)
            //    {
            //        case MetaName.CMapTypes:
            //        case MetaName.CTimeArchetypeDef:
            //        case MetaName.CBaseArchetypeDef:
            //        case MetaName.CMloArchetypeDef:
            //        case MetaName.CMloTimeCycleModifier:
            //        case MetaName.CMloRoomDef:
            //        case MetaName.CMloPortalDef:
            //        case MetaName.CMloEntitySet:
            //        case MetaName.CEntityDef:
            //        case MetaName.CExtensionDefParticleEffect:
            //        case MetaName.CExtensionDefAudioCollisionSettings:
            //        case MetaName.CExtensionDefSpawnPoint:
            //        case MetaName.CExtensionDefSpawnPointOverride:
            //        case MetaName.CExtensionDefExplosionEffect:
            //        case MetaName.CExtensionDefAudioEmitter:
            //        case MetaName.CExtensionDefLadder:
            //        case MetaName.CExtensionDefBuoyancy:
            //        case MetaName.CExtensionDefExpression:
            //        case MetaName.CExtensionDefLightShaft:
            //        case MetaName.CExtensionDefLightEffect:
            //        case MetaName.CExtensionDefDoor:
            //        case MetaName.CExtensionDefWindDisturbance:
            //        case MetaName.CExtensionDefProcObject:
            //        case MetaName.CLightAttrDef:
            //        case MetaName.STRING:
            //        case MetaName.POINTER:
            //        case MetaName.UINT:
            //        case MetaName.VECTOR4:
            //            break;
            //        default:
            //            break;
            //    }
            //}




            //MetaTypes.ParseMetaData(Meta);






        }


    }






}
