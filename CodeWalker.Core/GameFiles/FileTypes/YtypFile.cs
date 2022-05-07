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

        public CMapTypes _CMapTypes;
        public CMapTypes CMapTypes { get { return _CMapTypes; } set { _CMapTypes = value; } }

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

        public byte[] Save()
        {
            MetaBuilder mb = new MetaBuilder();

            var mdb = mb.EnsureBlock(MetaName.CMapTypes);

            CMapTypes mapTypes = _CMapTypes;

            if (Extensions == null || Extensions.Length <= 0)
                mapTypes.extensions = new Array_StructurePointer();
            else
                mapTypes.extensions = mb.AddWrapperArrayPtr(Extensions);

            if ((AllArchetypes != null) && (AllArchetypes.Length > 0))
            {
                for (int i = 0; i < AllArchetypes.Length; i++)
                {
                    var arch = AllArchetypes[i]; //save the extensions first..
                    if (arch._BaseArchetypeDef.extensions.Count1 > 0)
                    {
                        arch._BaseArchetypeDef.extensions = mb.AddWrapperArrayPtr(arch.Extensions);
                    }
                }

                MetaPOINTER[] ptrs = new MetaPOINTER[AllArchetypes.Length];
                for (int i = 0; i < AllArchetypes.Length; i++)
                {
                    var arch = AllArchetypes[i];
                    switch (arch)
                    {
                        case TimeArchetype t:
                            ptrs[i] = mb.AddItemPtr(MetaName.CTimeArchetypeDef, t._TimeArchetypeDef);
                            break;
                        case MloArchetype m:
                            try
                            {
                                m._MloArchetypeDef._MloArchetypeDef.entities = mb.AddWrapperArrayPtr(m.entities);
                                m._MloArchetypeDef._MloArchetypeDef.rooms = mb.AddWrapperArray(m.rooms);
                                m._MloArchetypeDef._MloArchetypeDef.portals = mb.AddWrapperArray(m.portals);
                                m._MloArchetypeDef._MloArchetypeDef.entitySets = mb.AddWrapperArray(m.entitySets);
                                m._MloArchetypeDef._MloArchetypeDef.timeCycleModifiers = mb.AddItemArrayPtr(MetaName.CMloTimeCycleModifier, m.timeCycleModifiers);
                            }
                            catch/* (Exception e)*/
                            {
                                //todo: log save error.
                            }
                            ptrs[i] = mb.AddItemPtr(MetaName.CMloArchetypeDef, m._MloArchetypeDef);
                            break;
                        case Archetype a:
                            ptrs[i] = mb.AddItemPtr(MetaName.CBaseArchetypeDef, a._BaseArchetypeDef);
                            break;
                    }
                }
                mapTypes.archetypes = mb.AddPointerArray(ptrs);
            }
            else
            {
                mapTypes.archetypes = new Array_StructurePointer();
            }

            if (CompositeEntityTypes != null && CompositeEntityTypes.Length > 0)
            {
                var cptrs = new MetaPOINTER[CompositeEntityTypes.Length];

                for (int i = 0; i < cptrs.Length; i++)
                {
                    var cet = CompositeEntityTypes[i];
                    cptrs[i] = mb.AddItemPtr(MetaName.CCompositeEntityType, cet);
                }
                mapTypes.compositeEntityTypes = mb.AddItemArrayPtr(MetaName.CCompositeEntityType, cptrs);
            }

            mapTypes.name = NameHash;
            mapTypes.dependencies = new Array_uint(); // is this never used? possibly a todo?

            mb.AddStructureInfo(MetaName.CMapTypes);

            if ((AllArchetypes != null && AllArchetypes.Length > 0))
            {
                mb.AddStructureInfo(MetaName.CBaseArchetypeDef);
                mb.AddEnumInfo(MetaName.rage__fwArchetypeDef__eAssetType); // ASSET_TYPE_
            }

            if ((AllArchetypes != null) && (AllArchetypes.Any(x => x is MloArchetype)))
            {
                mb.AddStructureInfo(MetaName.CMloArchetypeDef);
                mb.AddStructureInfo(MetaName.CMloRoomDef);
                mb.AddStructureInfo(MetaName.CMloPortalDef);
                mb.AddStructureInfo(MetaName.CMloEntitySet);
                mb.AddStructureInfo(MetaName.CMloTimeCycleModifier);
            }

            if ((AllArchetypes != null) && (AllArchetypes.Any(x => x is MloArchetype m && m.entities.Length > 0)))
            {
                mb.AddStructureInfo(MetaName.CEntityDef);
                mb.AddEnumInfo(MetaName.rage__eLodType); //LODTYPES_
                mb.AddEnumInfo(MetaName.rage__ePriorityLevel);  //PRI_
            }

            if ((AllArchetypes != null) && (AllArchetypes.Any(x => x is TimeArchetype)))
            {
                mb.AddStructureInfo(MetaName.CTimeArchetypeDef);
            }

            if (CompositeEntityTypes?.Length > 0)
            {
                mb.AddStructureInfo(MetaName.CCompositeEntityType);
            }


            mb.AddItem(MetaName.CMapTypes, mapTypes);

            Meta meta = mb.GetMeta();
            byte[] data = ResourceBuilder.Build(meta, 2);

            HasChanged = false;

            return data;
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


            _CMapTypes = MetaTypes.GetTypedData<CMapTypes>(Meta, MetaName.CMapTypes);


            List<Archetype> allarchs = new List<Archetype>();

            var ptrs = MetaTypes.GetPointerArray(Meta, _CMapTypes.archetypes);
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


            Extensions = MetaTypes.GetExtensions(Meta, _CMapTypes.extensions);

            if (Extensions != null)
            { }


            //AudioEmitters = MetaTypes.GetTypedDataArray<CExtensionDefAudioEmitter>(Meta, MetaName.CExtensionDefAudioEmitter);
            //if (AudioEmitters != null)
            //{ }

            //CEntityDefs = MetaTypes.GetTypedDataArray<CEntityDef>(Meta, MetaName.CEntityDef);


            CompositeEntityTypes = MetaTypes.ConvertDataArray<CCompositeEntityType>(Meta, MetaName.CCompositeEntityType, _CMapTypes.compositeEntityTypes);
            if (CompositeEntityTypes != null)
            { }

            NameHash = _CMapTypes.name;
            if ((NameHash == 0) && (entry.NameLower != null))
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
            //        case (MetaName)MetaTypeName.STRING:
            //        case (MetaName)MetaTypeName.POINTER:
            //        case (MetaName)MetaTypeName.UINT:
            //        case (MetaName)MetaTypeName.VECTOR4:
            //            break;
            //        default:
            //            break;
            //    }
            //}




            //MetaTypes.ParseMetaData(Meta);






        }


        public void AddArchetype(Archetype archetype)
        {
            if (AllArchetypes == null)
                AllArchetypes = new Archetype[0];

            List<Archetype> archetypes = AllArchetypes.ToList();
            archetype.Ytyp = this;
            archetypes.Add(archetype);
            AllArchetypes = archetypes.ToArray();
        }

        public Archetype AddArchetype()
        {
            var a = new Archetype();
            a._BaseArchetypeDef.assetType = rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE;
            a._BaseArchetypeDef.lodDist = 60;
            a._BaseArchetypeDef.hdTextureDist = 15;
            a.Ytyp = this;
            AddArchetype(a);
            return a;
        }

        public bool RemoveArchetype(Archetype archetype)
        {
            List<Archetype> archetypes = AllArchetypes.ToList();
            if (archetypes.Remove(archetype))
            {
                AllArchetypes = archetypes.ToArray();
                return true;
            }
            return false;
        }
    }






}
