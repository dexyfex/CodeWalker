using SharpDX.Win32;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [SkipLocalsInit]
    public class YtypFile : GameFile, PackedFile
    {
        public Meta? Meta { get; set; }
        public PsoFile? Pso { get; set; }
        public RbfFile? Rbf { get; set; }


        public uint NameHash { get; set; }
        public string[] Strings => Meta?.GetStrings() ?? [];

        public CMapTypes _CMapTypes;
        public CMapTypes CMapTypes => _CMapTypes;

        public Archetype[] AllArchetypes { get => allArchetypes; set => allArchetypes = value; }
        public MetaWrapper[] Extensions { get; set; } = [];

        public MloArchetype[] MloArchetypes => allArchetypes.Where(p => p is MloArchetype).Select(p => (p as MloArchetype)!).ToArray();

        public CCompositeEntityType[] CompositeEntityTypes { get; set; } = [];


        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string>? SaveWarnings = null;
        private Archetype[] allArchetypes = [];

        public YtypFile() : base(null, GameFileType.Ytyp)
        {
        }

        public YtypFile(RpfFileEntry entry) : base(entry, GameFileType.Ytyp)
        {
        }


        public override string ToString() => RpfFileEntry?.Name ?? string.Empty;

        public byte[] Save()
        {
            MetaBuilder mb = new MetaBuilder();

            var mdb = mb.EnsureBlock(MetaName.CMapTypes);

            CMapTypes mapTypes = _CMapTypes;

            if (Extensions.Length == 0)
                mapTypes.extensions = new Array_StructurePointer();
            else
                mapTypes.extensions = mb.AddWrapperArrayPtr(Extensions);

            if (AllArchetypes.Length > 0)
            {
                MetaPOINTER[] ptrs = new MetaPOINTER[AllArchetypes.Length];
                var i = 0;
                foreach(var arch in AllArchetypes)
                {
                    if (arch._BaseArchetypeDef.extensions.Count1 > 0)
                    {
                        arch._BaseArchetypeDef.extensions = mb.AddWrapperArrayPtr(arch.Extensions);
                    }
                    switch (arch)
                    {
                        case TimeArchetype t:
                            ptrs[i] = mb.AddItemPtr(MetaName.CTimeArchetypeDef, in t._TimeArchetypeDef);
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
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                            ptrs[i] = mb.AddItemPtr(MetaName.CMloArchetypeDef, in m._MloArchetypeDef);
                            break;
                        case Archetype a:
                            ptrs[i] = mb.AddItemPtr(MetaName.CBaseArchetypeDef, in a._BaseArchetypeDef);
                            break;
                    }
                    i++;
                }
                mapTypes.archetypes = mb.AddPointerArray(ptrs);
            }
            else
            {
                mapTypes.archetypes = new Array_StructurePointer();
            }

            if (CompositeEntityTypes.Length > 0)
            {
                var cptrs = new MetaPOINTER[CompositeEntityTypes.Length];

                for (int i = 0; i < cptrs.Length; i++)
                {
                    var cet = CompositeEntityTypes[i];
                    cptrs[i] = mb.AddItemPtr(MetaName.CCompositeEntityType, in cet);
                }
                mapTypes.compositeEntityTypes = mb.AddItemArrayPtr(MetaName.CCompositeEntityType, cptrs);
            }

            mapTypes.name = NameHash;
            mapTypes.dependencies = new Array_uint(); // is this never used? possibly a todo?

            mb.AddStructureInfo(MetaName.CMapTypes);

            if (AllArchetypes is not null && AllArchetypes.Length > 0)
            {
                mb.AddStructureInfo(MetaName.CBaseArchetypeDef);
                mb.AddEnumInfo(MetaName.rage__fwArchetypeDef__eAssetType); // ASSET_TYPE_
            }

            if (AllArchetypes is not null && AllArchetypes.Any(x => x is MloArchetype))
            {
                mb.AddStructureInfo(MetaName.CMloArchetypeDef);
                mb.AddStructureInfo(MetaName.CMloRoomDef);
                mb.AddStructureInfo(MetaName.CMloPortalDef);
                mb.AddStructureInfo(MetaName.CMloEntitySet);
                mb.AddStructureInfo(MetaName.CMloTimeCycleModifier);
            }

            if (AllArchetypes is not null && AllArchetypes.Any(x => x is MloArchetype m && m.entities.Length > 0))
            {
                mb.AddStructureInfo(MetaName.CEntityDef);
                mb.AddEnumInfo(MetaName.rage__eLodType); //LODTYPES_
                mb.AddEnumInfo(MetaName.rage__ePriorityLevel);  //PRI_
            }

            if (AllArchetypes is not null && AllArchetypes.Any(x => x is TimeArchetype))
            {
                mb.AddStructureInfo(MetaName.CTimeArchetypeDef);
            }

            if (CompositeEntityTypes.Length > 0)
            {
                mb.AddStructureInfo(MetaName.CCompositeEntityType);
            }

            mb.AddItem(MetaName.CMapTypes, in mapTypes);

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

        public async ValueTask LoadAsync(byte[] data)
        {
            //direct load from a raw, compressed ytyp file (openIV-compatible format)

            await RpfFile.LoadResourceFileAsync(this, data, 2);

            Loaded = true;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
            if (entry is not RpfResourceFileEntry resentry)
            {
                if (RbfFile.IsRBF(data.AsSpan(0, 4)))
                {
                    Rbf = new RbfFile();
                    Rbf.Load(data);
                }
                else if (PsoFile.IsPSO(data.AsSpan(0, 4)))
                {
                    Pso = new PsoFile();
                    Pso.Load(data);
                }
                return;
            }

            using var rd = new ResourceDataReader(resentry, data);

            var meta = rd.ReadBlock<Meta>();
            Meta = meta;


            _CMapTypes = MetaTypes.GetTypedData<CMapTypes>(meta, MetaName.CMapTypes);

            var ptrs = MetaTypes.GetPointerArray(meta, in _CMapTypes.archetypes);
            //List<Archetype> allarchs = new List<Archetype>(ptrs.Length);

            if (ptrs.Length > 0)
            {
                Archetype[] allarchs = new Archetype[ptrs.Length];
                var count = 0;
                for (int i = 0; i < ptrs.Length; i++)
                {
                    ref var ptr = ref ptrs[i];
                    var offset = ptr.Offset;
                    var block = Meta.GetBlock(ptr.BlockID);
                    if (block == null)
                    {
                        continue;
                    }
                    if ((offset < 0) || (block.Data == null) || (offset >= block.Data.Length))
                    {
                        continue;
                    }

                    Archetype? a;
                    switch (block.StructureNameHash)
                    {
                        case MetaName.CBaseArchetypeDef:
                            //PsoTypes.TryConvertDataRaw<CBaseArchetypeDef>(block.Data, offset, out var basearch);
                            a = new Archetype();
                            a.Init(this, block.Data.AsSpan(offset));
                            a.Extensions = MetaTypes.GetExtensions(meta, in a._BaseArchetypeDef.extensions) ?? [];
                            break;
                        case MetaName.CTimeArchetypeDef:
                            //PsoTypes.TryConvertDataRaw<CTimeArchetypeDef>(block.Data, offset, out var timearch);
                            var ta = new TimeArchetype();
                            ta.Init(this, block.Data.AsSpan(offset));
                            ta.Extensions = MetaTypes.GetExtensions(meta, in ta._BaseArchetypeDef.extensions) ?? [];
                            a = ta;
                            break;
                        case MetaName.CMloArchetypeDef:
                            //PsoTypes.TryConvertDataRaw<CMloArchetypeDef>(block.Data, offset, out var mloarch);
                            var ma = new MloArchetype();
                            ma.Init(this, block.Data.AsSpan(offset));
                            ma.Extensions = MetaTypes.GetExtensions(meta, in ma._BaseArchetypeDef.extensions) ?? [];

                            ma.LoadChildren(Meta);

                            a = ma;
                            break;
                        default:
                            a = null;
                            continue;
                    }

                    if (a is not null)
                    {
                        allarchs[count] = a;
                        count++;
                    }
                }


                if (allarchs.Length != count)
                {
                    Console.WriteLine("Resizing array");
                    Array.Resize(ref allarchs, count);
                }

                allArchetypes = allarchs;
            }
            else
            {
                allArchetypes = [];
            }


            Extensions = MetaTypes.GetExtensions(Meta, in _CMapTypes.extensions) ?? Array.Empty<MetaWrapper>();


            //AudioEmitters = MetaTypes.GetTypedDataArray<CExtensionDefAudioEmitter>(Meta, MetaName.CExtensionDefAudioEmitter);
            //if (AudioEmitters != null)
            //{ }

            //CEntityDefs = MetaTypes.GetTypedDataArray<CEntityDef>(Meta, MetaName.CEntityDef);


            CompositeEntityTypes = MetaTypes.ConvertDataArray<CCompositeEntityType>(Meta, MetaName.CCompositeEntityType, in _CMapTypes.compositeEntityTypes) ?? Array.Empty<CCompositeEntityType>();

            NameHash = _CMapTypes.name;
            if (NameHash == 0 && entry.Name is not null)
            {
                int ind = entry.Name.LastIndexOf('.');
                if (ind > 0)
                {
                    NameHash = JenkHash.GenHashLower(entry.Name.AsSpan(0, ind));
                }
                else
                {
                    NameHash = JenkHash.GenHashLower(entry.Name);
                }
            }

            foreach(var str in Strings)
            {
                JenkIndex.Ensure(str); //just shove them in there
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
            allArchetypes ??= Array.Empty<Archetype>();

            Array.Resize(ref allArchetypes, allArchetypes.Length + 1);
            allArchetypes[allArchetypes.Length - 1] = archetype;
            archetype.Ytyp = this;
        }

        public Archetype AddArchetype()
        {
            var a = new Archetype
            {
                _BaseArchetypeDef = new CBaseArchetypeDef
                {
                    assetType = rage__fwArchetypeDef__eAssetType.ASSET_TYPE_DRAWABLE,
                    lodDist = 60,
                    hdTextureDist = 15,
                },
                Ytyp = this
            };

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
