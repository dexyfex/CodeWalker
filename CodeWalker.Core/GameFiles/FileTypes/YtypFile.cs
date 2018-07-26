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
    public class YtypFile : GameFile, PackedFile
    {


        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }


        public uint NameHash { get; set; }
        public string[] Strings { get; set; }


        public CMapTypes CMapTypes { get; set; }

        public Archetype[] AllArchetypes { get; set; }

        public MetaWrapper[] Extensions { get; set; }

        public CCompositeEntityType[] CompositeEntityTypes { get; set; }


        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;



        public YtypFile() : base(null, GameFileType.Ytyp)
        {
        }
        public YtypFile(RpfFileEntry entry) : base(entry, GameFileType.Ytyp)
        {
        }


        public override string ToString()
        {
            return (RpfFileEntry != null) ? RpfFileEntry.Name : string.Empty;
        }


        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ytyp file (openIV-compatible format)

            RpfFile.LoadResourceFile(this, data, 2);

            Loaded = true;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
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
                    var offset = ptr.Offset;
                    var block = Meta.GetBlock(ptr.BlockID);
                    if (block == null)
                    { continue; }
                    if ((offset < 0) || (block.Data == null) || (offset >= block.Data.Length))
                    { continue; }

                    Archetype a = null;
                    switch (block.StructureNameHash)
                    {
                        case MetaName.CBaseArchetypeDef:
                            var basearch = PsoTypes.ConvertDataRaw<CBaseArchetypeDef>(block.Data, offset);
                            a = new Archetype();
                            a.Init(this, ref basearch);
                            a.Extensions = MetaTypes.GetExtensions(Meta, basearch.extensions);
                            break;
                        case MetaName.CTimeArchetypeDef:
                            var timearch = PsoTypes.ConvertDataRaw<CTimeArchetypeDef>(block.Data, offset);
                            var ta = new TimeArchetype();
                            ta.Init(this, ref timearch);
                            ta.Extensions = MetaTypes.GetExtensions(Meta, timearch._BaseArchetypeDef.extensions);
                            a = ta;
                            break;
                        case MetaName.CMloArchetypeDef:
                            var mloarch = PsoTypes.ConvertDataRaw<CMloArchetypeDef>(block.Data, offset);
                            var ma = new MloArchetype();
                            ma.Init(this, ref mloarch);
                            ma.Extensions = MetaTypes.GetExtensions(Meta, mloarch._BaseArchetypeDef.extensions);

                            ma.LoadChildren(Meta);

                            a = ma;
                            break;
                        default:
                            continue;
                    }

                    if (a != null)
                    {
                        allarchs.Add(a);
                    }
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

        public void AddArchetype(Archetype arch)
        {
            List<Archetype> allArchs = new List<Archetype>();

            if (AllArchetypes != null)
                allArchs.AddRange(AllArchetypes);

            allArchs.Add(arch);

            AllArchetypes = allArchs.ToArray();
        }

        public void RemoveArchetype(Archetype arch)
        {
            List<Archetype> allArchs = new List<Archetype>();

            if (AllArchetypes != null)
                allArchs.AddRange(AllArchetypes);

            if (allArchs.Contains(arch))
                allArchs.Remove(arch);

            AllArchetypes = allArchs.ToArray();
        }

        public byte[] Save()
        {
            MetaBuilder mb = new MetaBuilder();

            var mdb = mb.EnsureBlock(MetaName.CMapTypes);

            CMapTypes mapTypes = CMapTypes;

            if((AllArchetypes != null) && (AllArchetypes.Length > 0))
            {
                MetaPOINTER[] archPtrs = new MetaPOINTER[AllArchetypes.Length];

                for(int i=0; i<AllArchetypes.Length; i++)
                {
                    var arch = AllArchetypes[i];
                    arch._BaseArchetypeDef.extensions = mb.AddWrapperArrayPtr(arch.Extensions);
                }

                for (int i = 0; i < archPtrs.Length; i++)
                {
                    var arch = AllArchetypes[i];

                    if (arch is MloArchetype)
                    {
                        var mloArch = arch as MloArchetype;

                        mloArch._BaseMloArchetypeDef._MloArchetypeDef.entities           = mb.AddWrapperArrayPtr(mloArch.entities);
                        mloArch._BaseMloArchetypeDef._MloArchetypeDef.rooms              = mb.AddWrapperArray(mloArch.rooms);
                        mloArch._BaseMloArchetypeDef._MloArchetypeDef.portals            = mb.AddWrapperArray(mloArch.portals);
                        mloArch._BaseMloArchetypeDef._MloArchetypeDef.entitySets         = mb.AddWrapperArray(mloArch.entitySets);
                        mloArch._BaseMloArchetypeDef._MloArchetypeDef.timeCycleModifiers = mb.AddItemArrayPtr(MetaName.CTimeCycleModifier, mloArch.timeCycleModifiers);

                        archPtrs[i] = mb.AddItemPtr(MetaName.CMloArchetypeDef, mloArch.BaseMloArchetypeDef);

                    }
                    else if (arch is TimeArchetype)
                    {
                        var timeArch = arch as TimeArchetype;
                        archPtrs[i] = mb.AddItemPtr(MetaName.CTimeArchetypeDef, timeArch.TimeArchetypeDef);

                    }
                    else
                    {
                        archPtrs[i] = mb.AddItemPtr(MetaName.CBaseArchetypeDef, arch.BaseArchetypeDef);
                    }
                }

                mapTypes.archetypes = mb.AddPointerArray(archPtrs);
            }

            if((CompositeEntityTypes != null) && (CompositeEntityTypes.Length > 0))
            {
                MetaPOINTER[] cetPtrs = new MetaPOINTER[CompositeEntityTypes.Length] ;

                for (int i = 0; i < cetPtrs.Length; i++)
                {
                    var cet    = CompositeEntityTypes[i];
                    cetPtrs[i] = mb.AddItemPtr(MetaName.CCompositeEntityType, cet);
                }

                mapTypes.compositeEntityTypes = mb.AddItemArrayPtr(MetaName.CCompositeEntityType, cetPtrs);
            }

            mb.AddItem(MetaName.CMapTypes, mapTypes);

            mb.AddStructureInfo(MetaName.CMapTypes);
            mb.AddStructureInfo(MetaName.CBaseArchetypeDef);
            mb.AddStructureInfo(MetaName.CMloArchetypeDef);
            mb.AddStructureInfo(MetaName.CTimeArchetypeDef);
            mb.AddStructureInfo(MetaName.CMloRoomDef);
            mb.AddStructureInfo(MetaName.CMloPortalDef);
            mb.AddStructureInfo(MetaName.CMloEntitySet);
            mb.AddStructureInfo(MetaName.CCompositeEntityType);

            mb.AddEnumInfo((MetaName)1991964615);
            mb.AddEnumInfo((MetaName)1294270217);
            mb.AddEnumInfo((MetaName)1264241711);
            mb.AddEnumInfo((MetaName)648413703);
            mb.AddEnumInfo((MetaName)3573596290);
            mb.AddEnumInfo((MetaName)700327466);
            mb.AddEnumInfo((MetaName)193194928);
            mb.AddEnumInfo((MetaName)2266515059);

            Meta = mb.GetMeta();

            byte[] data = ResourceBuilder.Build(Meta, 2); //ymap is version 2...

            return data;

        }

    }






}
