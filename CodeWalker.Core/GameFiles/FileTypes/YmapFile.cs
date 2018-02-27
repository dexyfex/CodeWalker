using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapFile : GameFile, PackedFile
    {

        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }

        public CMapData _CMapData;

        public CMapData CMapData { get { return _CMapData; } set { _CMapData = value; } }
        public CEntityDef[] CEntityDefs { get; set; }
        public CMloInstanceDef[] CMloInstanceDefs { get; set; }
        public CCarGen[] CCarGens { get; set; }
        public CTimeCycleModifier[] CTimeCycleModifiers { get; set; }
        public MetaHash[] physicsDictionaries { get; set; }

        public Unk_975711773[] CBoxOccluders { get; set; }
        public Unk_2741784237[] COccludeModels { get; set; }


        public string[] Strings { get; set; }
        public YmapEntityDef[] AllEntities;
        public YmapEntityDef[] RootEntities;
        public YmapEntityDef[] MloEntities;

        public YmapFile[] ChildYmaps = null;
        public bool MergedWithParent = false;

        public YmapGrassInstanceBatch[] GrassInstanceBatches { get; set; }
        public YmapPropInstanceBatch[] PropInstanceBatches { get; set; }

        public YmapDistantLODLights DistantLODLights { get; set; }

        public YmapTimeCycleModifier[] TimeCycleModifiers { get; set; }

        public YmapCarGen[] CarGenerators { get; set; }

        public YmapBoxOccluder[] BoxOccluders { get; set; }
        public YmapOccludeModel[] OccludeModels { get; set; }


        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;




        public YmapFile() : base(null, GameFileType.Ymap)
        {
        }
        public YmapFile(RpfFileEntry entry) : base(entry, GameFileType.Ymap)
        {
            RpfFileEntry = entry;
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ymap file (openIV-compatible format)

            RpfResourceFileEntry resentry = new RpfResourceFileEntry();

            //hopefully this format has an RSC7 header...
            uint rsc7 = BitConverter.ToUInt32(data, 0);
            if (rsc7 == 0x37435352) //RSC7 header present!
            {
                int version = BitConverter.ToInt32(data, 4);
                resentry.SystemFlags = BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);
                if (data.Length > 16)
                {
                    int newlen = data.Length - 16; //trim the header from the data passed to the next step.
                    byte[] newdata = new byte[newlen];
                    Buffer.BlockCopy(data, 16, newdata, 0, newlen);
                    data = newdata;
                }
                else
                {
                    data = null; //shouldn't happen... empty..
                }
            }
            else
            {
                //direct load from file without the rpf header..
                //assume it's in resource meta format
                resentry.SystemFlags = RpfResourceFileEntry.GetFlagsFromSize(data.Length, 0);
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, 2); //graphics type 2 for ymap
            }

            var oldresentry = RpfFileEntry as RpfResourceFileEntry;
            if (oldresentry != null) //update the existing entry with the new one
            {
                oldresentry.SystemFlags = resentry.SystemFlags;
                oldresentry.GraphicsFlags = resentry.GraphicsFlags;
                resentry.Name = oldresentry.Name;
                resentry.NameHash = oldresentry.NameHash;
                resentry.NameLower = oldresentry.NameLower;
                resentry.ShortNameHash = oldresentry.ShortNameHash;
            }
            else
            {
                RpfFileEntry = resentry; //just stick it in there for later...
            }

            data = ResourceBuilder.Decompress(data);


            Load(data, resentry);

            Loaded = true;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;

            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                NonMetaLoad(data);
                return;
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();



            CMapData = MetaTypes.GetTypedData<CMapData>(Meta, MetaName.CMapData);



            Strings = MetaTypes.GetStrings(Meta);
            if (Strings != null)
            {
                foreach (string str in Strings)
                {
                    JenkIndex.Ensure(str); //just shove them in there
                }
            }

            physicsDictionaries = MetaTypes.GetHashArray(Meta, CMapData.physicsDictionaries);


            EnsureEntities(Meta); //load all the entity data and create the YmapEntityDefs

            EnsureInstances(Meta);

            EnsureLodLights(Meta);

            EnsureDistantLODLights(Meta);

            EnsureTimeCycleModifiers(Meta);

            EnsureCarGens(Meta);

            EnsureBoxOccluders(Meta);

            EnsureOccludeModels(Meta);

            EnsureContainerLods(Meta);


            #region data block test and old code

            //foreach (var block in Meta.DataBlocks)
            //{
            //    switch (block.StructureNameHash)
            //    {
            //        case MetaName.STRING:
            //        case MetaName.POINTER:
            //        case MetaName.HASH:
            //        case MetaName.UINT:
            //        case MetaName.VECTOR3: //distant lod lights uses this
            //        case MetaName.CMapData:
            //        case MetaName.CEntityDef:
            //        case MetaName.CTimeCycleModifier: //these sections are handled already
            //        case MetaName.CCarGen:
            //        case MetaName.CLightAttrDef:
            //        case MetaName.CMloInstanceDef:
            //        case MetaName.CExtensionDefDoor:
            //        case MetaName.CExtensionDefLightEffect:
            //        case MetaName.CExtensionDefSpawnPointOverride:
            //        case MetaName.rage__fwGrassInstanceListDef: //grass instance buffer
            //        case MetaName.rage__fwGrassInstanceListDef__InstanceData: //grass instance buffer data
            //            break;
            //        case MetaName.PhVerletClothCustomBounds: //these sections still todo..
            //        case MetaName.SectionUNKNOWN1:
            //        case MetaName.SectionUNKNOWN5://occlusion vertex data container
            //        case MetaName.SectionUNKNOWN7://occlusion related?
            //            break;
            //        case (MetaName)17: //vertex data - occlusion related - SectionUNKNOWN5
            //            break;
            //        case (MetaName)33: //what is this? maybe lodlights related
            //            break;
            //        default:
            //            break;
            //    }
            //}



            //MetaTypes.ParseMetaData(Meta);

            //string shortname = resentry.Name.Substring(0, resentry.Name.LastIndexOf('.'));
            //uint namehash = JenkHash.GenHash(shortname);





            //CLightAttrDefs = MetaTypes.GetTypedDataArray<CLightAttrDef>(Meta, MetaName.CLightAttrDef);
            //if (CLightAttrDefs != null)
            //{ }


            //var unk5s = MetaTypes.GetTypedDataArray<SectionUNKNOWN5>(Meta, MetaName.SectionUNKNOWN5);
            //if (unk5s != null) //used in occlusion ymaps
            //{
            //    foreach (var unk5 in unk5s)
            //    {
            //        if ((unk5.verts.Ptr > 0) && (unk5.verts.Ptr <= (ulong)Meta.DataBlocks.Length))
            //        {
            //            var indicesoffset = unk5.Unk_853977995;
            //            var datablock = Meta.DataBlocks[((int)unk5.verts.Ptr) - 1];
            //            if (datablock != null)
            //            { }//vertex data... occlusion mesh?
            //        }
            //    }
            //}

            //var unk7s = MetaTypes.GetTypedDataArray<SectionUNKNOWN7>(Meta, MetaName.SectionUNKNOWN7);
            //if (unk7s != null)
            //{ } //used in occlusion ymaps

            //var unk10s = MetaTypes.GetTypedDataArray<SectionUNKNOWN10>(Meta, MetaName.SectionUNKNOWN10);
            //if (unk10s != null)
            //{ } //entity pointer array.. 

            //CDoors = MetaTypes.GetTypedDataArray<CExtensionDefDoor>(Meta, MetaName.CExtensionDefDoor);
            //if (CDoors != null)
            //{ } //needs work - doors can be different types? not enough bytes for one

            //CExtLightEffects = MetaTypes.GetTypedDataArray<CExtensionDefLightEffect>(Meta, MetaName.CExtensionDefLightEffect);
            //if (CExtLightEffects != null)
            //{ }

            //CSpawnOverrides = MetaTypes.GetTypedDataArray<CExtensionDefSpawnPointOverride>(Meta, MetaName.CExtensionDefSpawnPointOverride);
            //if (CSpawnOverrides != null)
            //{ }

            #endregion
        }



        private void NonMetaLoad(byte[] data)
        {
            //non meta not supported yet! but see what's in there...
            MemoryStream ms = new MemoryStream(data);
            if (RbfFile.IsRBF(ms))
            {
                var Rbf = new RbfFile();
                Rbf.Load(ms);
            }
            else if (PsoFile.IsPSO(ms))
            {
                var Pso = new PsoFile();
                Pso.Load(ms);
                //PsoTypes.EnsurePsoTypes(Pso);
            }
            else
            {
            }

        }



        private void EnsureEntities(Meta Meta)
        {
            //CMloInstanceDefs = MetaTypes.ConvertDataArray<CMloInstanceDef>(Meta, MetaName.CMloInstanceDef, CMapData.entities);
            CMloInstanceDefs = MetaTypes.GetTypedDataArray<CMloInstanceDef>(Meta, MetaName.CMloInstanceDef);
            if (CMloInstanceDefs != null)
            { }

            var eptrs = MetaTypes.GetPointerArray(Meta, CMapData.entities);
            //CEntityDefs = MetaTypes.ConvertDataArray<CEntityDef>(Meta, MetaName.CEntityDef, CMapData.entities);
            CEntityDefs = MetaTypes.GetTypedDataArray<CEntityDef>(Meta, MetaName.CEntityDef);
            if (CEntityDefs != null)
            { }




            int instcount = 0;
            if (CEntityDefs != null) instcount += CEntityDefs.Length;
            if (CMloInstanceDefs != null) instcount += CMloInstanceDefs.Length;

            if (instcount > 0)
            {

                //build the entity hierarchy.
                List<YmapEntityDef> roots = new List<YmapEntityDef>(instcount);
                List<YmapEntityDef> alldefs = new List<YmapEntityDef>(instcount);
                List<YmapEntityDef> mlodefs = null;

                if (CEntityDefs != null)
                {
                    for (int i = 0; i < CEntityDefs.Length; i++)
                    {
                        YmapEntityDef d = new YmapEntityDef(this, i, ref CEntityDefs[i]);
                        alldefs.Add(d);
                    }
                }
                if (CMloInstanceDefs != null)
                {
                    mlodefs = new List<YmapEntityDef>();
                    for (int i = 0; i < CMloInstanceDefs.Length; i++)
                    {
                        YmapEntityDef d = new YmapEntityDef(this, i, ref CMloInstanceDefs[i]);
                        uint[] defentsets = MetaTypes.GetUintArray(Meta, CMloInstanceDefs[i].defaultEntitySets);
                        if (d.MloInstance != null)
                        {
                            d.MloInstance.defaultEntitySets = defentsets;
                        }
                        alldefs.Add(d);
                        mlodefs.Add(d);
                    }
                }


                for (int i = 0; i < alldefs.Count; i++)
                {
                    YmapEntityDef d = alldefs[i];
                    int pind = d.CEntityDef.parentIndex;
                    bool isroot = false;
                    if ((pind < 0) || (pind >= alldefs.Count) || (pind >= i)) //index check? might be a problem
                    {
                        isroot = true;
                    }
                    else
                    {
                        YmapEntityDef p = alldefs[pind];
                        if ((p.CEntityDef.lodLevel <= d.CEntityDef.lodLevel) ||
                            ((p.CEntityDef.lodLevel == Unk_1264241711.LODTYPES_DEPTH_ORPHANHD) &&
                             (d.CEntityDef.lodLevel != Unk_1264241711.LODTYPES_DEPTH_ORPHANHD)))
                        {
                            isroot = true;
                            p = null;
                        }
                    }

                    if (isroot)
                    {
                        roots.Add(d);
                    }
                    else
                    {
                        YmapEntityDef p = alldefs[pind];
                        p.AddChild(d);
                    }
                }
                for (int i = 0; i < alldefs.Count; i++)
                {
                    alldefs[i].ChildListToArray();
                }

                AllEntities = alldefs.ToArray();
                RootEntities = roots.ToArray();
                if (mlodefs != null)
                {
                    MloEntities = mlodefs.ToArray();
                }


                foreach (var ent in AllEntities)
                {
                    ent.Extensions = MetaTypes.GetExtensions(Meta, ent.CEntityDef.extensions);
                }
            }

        }

        private void EnsureInstances(Meta Meta)
        {
            if (CMapData.instancedData.GrassInstanceList.Count1 != 0)
            {
                rage__fwGrassInstanceListDef[] batches = MetaTypes.ConvertDataArray<rage__fwGrassInstanceListDef>(Meta, MetaName.rage__fwGrassInstanceListDef, CMapData.instancedData.GrassInstanceList);
                YmapGrassInstanceBatch[] gbatches = new YmapGrassInstanceBatch[batches.Length];
                for (int i = 0; i < batches.Length; i++)
                {
                    var batch = batches[i];
                    rage__fwGrassInstanceListDef__InstanceData[] instdatas = MetaTypes.ConvertDataArray<rage__fwGrassInstanceListDef__InstanceData>(Meta, MetaName.rage__fwGrassInstanceListDef__InstanceData, batch.InstanceList);
                    YmapGrassInstanceBatch gbatch = new YmapGrassInstanceBatch();
                    gbatch.Ymap = this;
                    gbatch.Batch = batch;
                    gbatch.Instances = instdatas;
                    gbatch.Position = (batch.BatchAABB.min.XYZ() + batch.BatchAABB.max.XYZ()) * 0.5f;
                    gbatch.Radius = (batch.BatchAABB.max.XYZ() - gbatch.Position).Length();
                    gbatch.AABBMin = (batch.BatchAABB.min.XYZ());
                    gbatch.AABBMax = (batch.BatchAABB.max.XYZ());
                    gbatches[i] = gbatch;
                }
                GrassInstanceBatches = gbatches;
            }
            if (CMapData.instancedData.PropInstanceList.Count1 != 0)
            {
            }
        }

        private void EnsureLodLights(Meta Meta)
        {
            //TODO!
            if (CMapData.LODLightsSOA.direction.Count1 != 0)
            {
            }
        }

        private void EnsureDistantLODLights(Meta Meta)
        {
            if (CMapData.DistantLODLightsSOA.position.Count1 != 0)
            {
                DistantLODLights = new YmapDistantLODLights();
                DistantLODLights.Ymap = this;
                DistantLODLights.CDistantLODLight = CMapData.DistantLODLightsSOA;
                DistantLODLights.colours = MetaTypes.GetUintArray(Meta, CMapData.DistantLODLightsSOA.RGBI);
                DistantLODLights.positions = MetaTypes.ConvertDataArray<MetaVECTOR3>(Meta, MetaName.VECTOR3, CMapData.DistantLODLightsSOA.position);
                DistantLODLights.CalcBB();
            }
        }

        private void EnsureTimeCycleModifiers(Meta Meta)
        {
            CTimeCycleModifiers = MetaTypes.ConvertDataArray<CTimeCycleModifier>(Meta, MetaName.CTimeCycleModifier, CMapData.timeCycleModifiers);
            if (CTimeCycleModifiers != null)
            {
                TimeCycleModifiers = new YmapTimeCycleModifier[CTimeCycleModifiers.Length];
                for (int i = 0; i < CTimeCycleModifiers.Length; i++)
                {
                    YmapTimeCycleModifier tcm = new YmapTimeCycleModifier();
                    tcm.Ymap = this;
                    tcm.CTimeCycleModifier = CTimeCycleModifiers[i];
                    tcm.BBMin = tcm.CTimeCycleModifier.minExtents;
                    tcm.BBMax = tcm.CTimeCycleModifier.maxExtents;
                    TimeCycleModifiers[i] = tcm;
                }
            }
        }

        private void EnsureCarGens(Meta Meta)
        {

            CCarGens = MetaTypes.ConvertDataArray<CCarGen>(Meta, MetaName.CCarGen, CMapData.carGenerators);
            if (CCarGens != null)
            {
                //string str = MetaTypes.GetTypesInitString(resentry, Meta); //to generate structinfos and enuminfos
                CarGenerators = new YmapCarGen[CCarGens.Length];
                for (int i = 0; i < CCarGens.Length; i++)
                {
                    CarGenerators[i] = new YmapCarGen(this, CCarGens[i]);
                }
            }
        }

        private void EnsureBoxOccluders(Meta meta)
        {
            CBoxOccluders = MetaTypes.ConvertDataArray<Unk_975711773>(Meta, (MetaName)975711773, CMapData.boxOccluders);
            if (CBoxOccluders != null)
            {
                BoxOccluders = new YmapBoxOccluder[CBoxOccluders.Length];
                for (int i = 0; i < CBoxOccluders.Length; i++)
                {
                    BoxOccluders[i] = new YmapBoxOccluder(this, CBoxOccluders[i]);
                }
            }
        }

        private void EnsureOccludeModels(Meta meta)
        {
            COccludeModels = MetaTypes.ConvertDataArray<Unk_2741784237>(Meta, (MetaName)2741784237, CMapData.occludeModels);
            if (COccludeModels != null)
            {
                OccludeModels = new YmapOccludeModel[COccludeModels.Length];
                for (int i = 0; i < COccludeModels.Length; i++)
                {
                    OccludeModels[i] = new YmapOccludeModel(this, COccludeModels[i]);
                }
            }
        }

        private void EnsureContainerLods(Meta meta)
        {

            //TODO: containerLods
            if (CMapData.containerLods.Count1 > 0)
            {
                //string str = MetaTypes.GetTypesInitString(Meta); //to generate structinfos and enuminfos


            }

        }


        public void BuildCEntityDefs()
        {
            //recreates the CEntityDefs and CMloInstanceDefs arrays from AllEntities.
            CEntityDefs = null;
            CMloInstanceDefs = null;
            if (AllEntities == null)
            {
                return;
            }


            List<CEntityDef> centdefs = new List<CEntityDef>();
            List<CMloInstanceDef> cmlodefs = new List<CMloInstanceDef>();

            for (int i = 0; i < AllEntities.Length; i++)
            {
                var ent = AllEntities[i];
                if (ent.MloInstance != null)
                {
                    cmlodefs.Add(ent.MloInstance.Instance);
                }
                else
                {
                    centdefs.Add(ent.CEntityDef);
                }
            }

            if (centdefs.Count > 0)
            {
                CEntityDefs = centdefs.ToArray();
            }
            if (cmlodefs.Count > 0)
            {
                CMloInstanceDefs = cmlodefs.ToArray();
            }
        }
        public void BuildCCarGens()
        {
            //recreates the CCarGens array from CarGenerators.
            if (CarGenerators == null)
            {
                CCarGens = null;
                return;
            }

            int count = CarGenerators.Length;
            CCarGens = new CCarGen[count];
            for (int i = 0; i < count; i++)
            {
                CCarGens[i] = CarGenerators[i].CCarGen;
            }
        }

        public void BuildInstances()
        {
            if (GrassInstanceBatches == null)
            {
                return;
            }

            if (PropInstanceBatches == null)
            { }

            int count = GrassInstanceBatches.Length;
            for (int i = 0; i < count; i++)
            {
                var g = GrassInstanceBatches[i];
                var b = g.Batch;

                var aabb = new rage__spdAABB();
                aabb.min = new Vector4(g.AABBMin, 0);
                aabb.max = new Vector4(g.AABBMax, 0);

                b.BatchAABB = aabb;

                GrassInstanceBatches[i].Batch = b;
            }
        }

        public byte[] Save()
        {
            //direct save to a raw, compressed ymap file (openIV-compatible format)


            //since Ymap object contents have been modified, need to recreate the arrays which are what is saved.
            BuildCEntityDefs(); //technically this isn't required anymore since the CEntityDefs is no longer used for saving.
            BuildCCarGens();
            BuildInstances();

            //TODO:
            //BuildLodLights();
            //BuildDistantLodLights();
            //BuildTimecycleModifiers(); //already being saved - update them..
            //BuildBoxOccluders();
            //BuildOccludeModels();
            //BuildContainerLods();



            MetaBuilder mb = new MetaBuilder();


            var mdb = mb.EnsureBlock(MetaName.CMapData);

            CMapData mapdata = CMapData;



            if ((AllEntities != null) && (AllEntities.Length > 0))
            {
                for (int i = 0; i < AllEntities.Length; i++)
                {
                    var ent = AllEntities[i]; //save the extensions first..
                    ent._CEntityDef.extensions = mb.AddWrapperArrayPtr(ent.Extensions);
                }

                MetaPOINTER[] ptrs = new MetaPOINTER[AllEntities.Length];
                for (int i = 0; i < AllEntities.Length; i++)
                {
                    var ent = AllEntities[i];
                    if (ent.MloInstance != null)
                    {
                        ptrs[i] = mb.AddItemPtr(MetaName.CMloInstanceDef, ent.MloInstance.Instance);
                    }
                    else
                    {
                        ptrs[i] = mb.AddItemPtr(MetaName.CEntityDef, ent.CEntityDef);
                    }
                }
                mapdata.entities = mb.AddPointerArray(ptrs);
            }
            else
            {
                mapdata.entities = new Array_StructurePointer();
            }

            mapdata.timeCycleModifiers = mb.AddItemArrayPtr(MetaName.CTimeCycleModifier, CTimeCycleModifiers);

            mapdata.physicsDictionaries = mb.AddHashArrayPtr(physicsDictionaries);

            mapdata.carGenerators = mb.AddItemArrayPtr(MetaName.CCarGen, CCarGens);

            //clear everything out for now - TODO: fix
            if (mapdata.containerLods.Count1 != 0) LogSaveWarning("containerLods were not saved. (TODO!)");
            if (mapdata.occludeModels.Count1 != 0) LogSaveWarning("occludeModels were not saved. (TODO!)");
            if (mapdata.boxOccluders.Count1 != 0) LogSaveWarning("boxOccluders were not saved. (TODO!)");
            if (mapdata.instancedData.PropInstanceList.Count1 != 0) LogSaveWarning("instancedData.PropInstanceList was not saved. (TODO!)");
            if (mapdata.LODLightsSOA.direction.Count1 != 0) LogSaveWarning("LODLightsSOA was not saved. (TODO!)");
            if (mapdata.DistantLODLightsSOA.position.Count1 != 0) LogSaveWarning("DistantLODLightsSOA was not saved. (TODO!)");
            mapdata.containerLods = new Array_Structure();
            mapdata.occludeModels = new Array_Structure();
            mapdata.boxOccluders = new Array_Structure();

            if ((GrassInstanceBatches != null) && (GrassInstanceBatches.Length > 0))
            {
                var instancedData = new rage__fwInstancedMapData();
                rage__fwGrassInstanceListDef[] batches = new rage__fwGrassInstanceListDef[GrassInstanceBatches.Length];
                for (int i = 0; i < GrassInstanceBatches.Length; i++)
                {
                    var batch = GrassInstanceBatches[i];

                    if (batch != null)
                    {
                        var b = batch.Batch;
                        b.InstanceList = mb.AddItemArrayPtr(MetaName.rage__fwGrassInstanceListDef__InstanceData, batch.Instances);
                        batches[i] = b;
                    }
                }

                instancedData.GrassInstanceList = mb.AddItemArrayPtr(MetaName.rage__fwGrassInstanceListDef, batches);
                mapdata.instancedData = instancedData;
            }
            else
            {
                mapdata.instancedData = new rage__fwInstancedMapData();
            }

            mapdata.LODLightsSOA = new CLODLight();
            mapdata.DistantLODLightsSOA = new CDistantLODLight();



            var block = new CBlockDesc();
            block.name = mb.AddStringPtr(Path.GetFileNameWithoutExtension(Name));
            block.exportedBy = mb.AddStringPtr("CodeWalker");
            block.time = mb.AddStringPtr(DateTime.UtcNow.ToString("dd MMMM yyyy HH:mm"));

            mapdata.block = block;


            string name = Path.GetFileNameWithoutExtension(Name);
            uint nameHash = JenkHash.GenHash(name);
            mapdata.name = new MetaHash(nameHash);//make sure name is upto date...


            mb.AddItem(MetaName.CMapData, mapdata);



            //make sure all the relevant structure and enum infos are present.
            if ((GrassInstanceBatches != null) && (GrassInstanceBatches.Length > 0))
            {
                mb.AddStructureInfo(MetaName.rage__spdAABB);
                mb.AddStructureInfo(MetaName.rage__fwGrassInstanceListDef__InstanceData);
                mb.AddStructureInfo(MetaName.rage__fwGrassInstanceListDef);
            }
            mb.AddStructureInfo(MetaName.rage__fwInstancedMapData);
            mb.AddStructureInfo(MetaName.CLODLight);
            mb.AddStructureInfo(MetaName.CDistantLODLight);
            mb.AddStructureInfo(MetaName.CBlockDesc);
            mb.AddStructureInfo(MetaName.CMapData);
            mb.AddStructureInfo(MetaName.CEntityDef);
            mb.AddStructureInfo(MetaName.CMloInstanceDef);
            mb.AddStructureInfo(MetaName.CTimeCycleModifier);
            if ((CCarGens != null) && (CCarGens.Length > 0))
            {
                mb.AddStructureInfo(MetaName.CCarGen);
            }

            mb.AddEnumInfo((MetaName)1264241711); //LODTYPES_
            mb.AddEnumInfo((MetaName)648413703);  //PRI_


            Meta meta = mb.GetMeta();

            byte[] data = ResourceBuilder.Build(meta, 2); //ymap is version 2...


            return data;
        }

        private void LogSaveWarning(string w)
        {
            if (SaveWarnings == null) SaveWarnings = new List<string>();
            SaveWarnings.Add(w);
        }




        public void EnsureChildYmaps(GameFileCache gfc)
        {
            if (ChildYmaps == null)
            {
                //no children here... look for child ymap....
                var node = gfc.GetMapNode(RpfFileEntry.ShortNameHash);
                if ((node != null) && (node.Children != null) && (node.Children.Length > 0))
                {
                    ChildYmaps = new YmapFile[node.Children.Length];
                    for (int i = 0; i < ChildYmaps.Length; i++)
                    {
                        var chash = node.Children[i].Name;
                        ChildYmaps[i] = gfc.GetYmap(chash);
                        if (ChildYmaps[i] == null)
                        {
                            //couldn't find child ymap..
                        }
                    }
                }
            }


            bool needupd = false;
            if (ChildYmaps != null)
            {
                for (int i = 0; i < ChildYmaps.Length; i++)
                {
                    var cmap = ChildYmaps[i];
                    if (cmap == null) continue; //nothing here..
                    if (!cmap.Loaded)
                    {
                        //ChildYmaps[i] = gfc.GetYmap(cmap.Hash); //incase no load was requested.
                        cmap = gfc.GetYmap(cmap.Key.Hash);
                        ChildYmaps[i] = cmap;
                    }
                    if ((cmap.Loaded) && (!cmap.MergedWithParent))
                    {
                        needupd = true;
                    }
                }
            }

            if ((ChildYmaps != null) && needupd)
            {
                List<YmapEntityDef> newroots = new List<YmapEntityDef>(RootEntities);
                for (int i = 0; i < ChildYmaps.Length; i++)
                {
                    var cmap = ChildYmaps[i];
                    if (cmap == null) continue; //nothing here..
                    //cmap.EnsureChildYmaps();
                    if ((cmap.Loaded) && (!cmap.MergedWithParent))
                    {
                        cmap.MergedWithParent = true;
                        if (cmap.RootEntities != null)
                        {
                            foreach (var rcent in cmap.RootEntities)
                            {
                                int pind = rcent.CEntityDef.parentIndex;
                                if (pind < 0)
                                {
                                    if (rcent.CEntityDef.lodLevel != Unk_1264241711.LODTYPES_DEPTH_ORPHANHD)
                                    {
                                    }
                                    //pind = 0;
                                }
                                if ((pind >= 0) && (pind < AllEntities.Length))
                                {
                                    var pentity = AllEntities[pind];
                                    pentity.AddChild(rcent);
                                }
                                else
                                {
                                    //TODO: fix this!!
                                    //newroots.Add(rcent); //not sure this is the right approach.
                                    //////rcent.Owner = this;
                                }
                            }
                        }
                    }
                }
                if (AllEntities != null)
                {
                    for (int i = 0; i < AllEntities.Length; i++)
                    {
                        AllEntities[i].ChildListToMergedArray();
                    }
                }

                RootEntities = newroots.ToArray();
            }


        }




        public void AddEntity(YmapEntityDef ent)
        {
            //used by the editor to add to the ymap.

            List<YmapEntityDef> allents = new List<YmapEntityDef>();
            if (AllEntities != null) allents.AddRange(AllEntities);
            ent.Index = allents.Count;
            ent.Ymap = this;
            allents.Add(ent);
            AllEntities = allents.ToArray();


            if ((ent.Parent == null) || (ent.Parent.Ymap != this))
            {
                //root entity, add to roots.

                List<YmapEntityDef> rootents = new List<YmapEntityDef>();
                if (RootEntities != null) rootents.AddRange(RootEntities);
                rootents.Add(ent);
                RootEntities = rootents.ToArray();
            }

            HasChanged = true;
        }

        public bool RemoveEntity(YmapEntityDef ent)
        {
            //used by the editor to remove from the ymap.
            if (ent == null) return false;

            var res = true;

            int idx = ent.Index;
            List<YmapEntityDef> newAllEntities = new List<YmapEntityDef>();
            List<YmapEntityDef> newRootEntities = new List<YmapEntityDef>();

            for (int i = 0; i < AllEntities.Length; i++)
            {
                var oent = AllEntities[i];
                oent.Index = newAllEntities.Count;
                if (oent != ent) newAllEntities.Add(oent);
                else if (i != idx)
                {
                    res = false; //indexes didn't match.. this shouldn't happen!
                }
            }
            for (int i = 0; i < RootEntities.Length; i++)
            {
                var oent = RootEntities[i];
                if (oent != ent) newRootEntities.Add(oent);
            }

            if ((AllEntities.Length == newAllEntities.Count) || (RootEntities.Length == newRootEntities.Count))
            {
                res = false;
            }

            AllEntities = newAllEntities.ToArray();
            RootEntities = newRootEntities.ToArray();

            HasChanged = true;

            return res;
        }


        public void AddCarGen(YmapCarGen cargen)
        {
            List<YmapCarGen> cargens = new List<YmapCarGen>();
            if (CarGenerators != null) cargens.AddRange(CarGenerators);
            cargen.Ymap = this;
            cargens.Add(cargen);
            CarGenerators = cargens.ToArray();

            HasChanged = true;
        }

        public bool RemoveCarGen(YmapCarGen cargen)
        {
            if (cargen == null) return false;

            List<YmapCarGen> newcargens = new List<YmapCarGen>();

            if (CarGenerators != null)
            {
                for (int i = 0; i < CarGenerators.Length; i++)
                {
                    var cg = CarGenerators[i];
                    if (cg != cargen)
                    {
                        newcargens.Add(cg);
                    }
                }
                if (newcargens.Count == CarGenerators.Length)
                {
                    return false; //nothing removed... wasn't present?
                }
            }


            CarGenerators = newcargens.ToArray();

            HasChanged = true;

            return true;
        }


        public void AddGrassBatch(YmapGrassInstanceBatch newbatch)
        {
            List<YmapGrassInstanceBatch> batches = new List<YmapGrassInstanceBatch>();
            if (GrassInstanceBatches != null) batches.AddRange(GrassInstanceBatches);
            newbatch.Ymap = this;
            batches.Add(newbatch);
            GrassInstanceBatches = batches.ToArray();

            HasChanged = true;
        }

        public bool RemoveGrassBatch(YmapGrassInstanceBatch batch)
        {
            if (batch == null) return false;

            List<YmapGrassInstanceBatch> batches = new List<YmapGrassInstanceBatch>();

            if (GrassInstanceBatches != null)
            {
                for (int i = 0; i < GrassInstanceBatches.Length; i++)
                {
                    var gb = GrassInstanceBatches[i];
                    if (gb != batch)
                    {
                        batches.Add(gb);
                    }
                }
                if (batches.Count == GrassInstanceBatches.Length)
                {
                    return false; //nothing removed... wasn't present?
                }
            }


            GrassInstanceBatches = batches.ToArray();

            HasChanged = true;

            return true;
        }




        public bool CalcFlags()
        {
            uint flags = 0;
            uint contentFlags = 0;

            if (AllEntities != null)
            {
                foreach (var yent in AllEntities)
                {
                    var cent = yent.CEntityDef;
                    switch (cent.lodLevel)
                    {
                        case Unk_1264241711.LODTYPES_DEPTH_ORPHANHD:
                        case Unk_1264241711.LODTYPES_DEPTH_HD:
                            contentFlags = SetBit(contentFlags, 0); //1
                            break;
                        case Unk_1264241711.LODTYPES_DEPTH_LOD:
                            contentFlags = SetBit(contentFlags, 1); //2
                            flags = SetBit(flags, 1); //2
                            break;
                        case Unk_1264241711.LODTYPES_DEPTH_SLOD1:
                            contentFlags = SetBit(contentFlags, 4); //16
                            flags = SetBit(flags, 1); //2
                            break;
                        case Unk_1264241711.LODTYPES_DEPTH_SLOD2:
                        case Unk_1264241711.LODTYPES_DEPTH_SLOD3:
                        case Unk_1264241711.LODTYPES_DEPTH_SLOD4:
                            contentFlags = SetBit(contentFlags, 2); //4
                            contentFlags = SetBit(contentFlags, 4); //16
                            flags = SetBit(flags, 1); //2
                            break;
                    }
                }
            }

            if ((CMloInstanceDefs != null) && (CMloInstanceDefs.Length > 0))
            {
                contentFlags = SetBit(contentFlags, 3); //8
            }
            if ((physicsDictionaries != null) && (physicsDictionaries.Length > 0))
            {
                contentFlags = SetBit(contentFlags, 6); //64
            }
            if ((GrassInstanceBatches != null) && (GrassInstanceBatches.Length > 0))
            {
                contentFlags = SetBit(contentFlags, 10); //64
            }


            bool change = false;
            if (_CMapData.flags != flags)
            {
                _CMapData.flags = flags;
                change = true;
            }
            if (_CMapData.contentFlags != contentFlags)
            {
                _CMapData.contentFlags = contentFlags;
                change = true;
            }
            return change;
        }

        public bool CalcExtents()
        {
            Vector3 emin = new Vector3(float.MaxValue);
            Vector3 emax = new Vector3(float.MinValue);
            Vector3 smin = new Vector3(float.MaxValue);
            Vector3 smax = new Vector3(float.MinValue);
            Vector3[] c = new Vector3[8];
            Vector3[] s = new Vector3[8];

            if (AllEntities != null)
            {
                for (int i = 0; i < AllEntities.Length; i++)
                {
                    var ent = AllEntities[i];
                    var arch = ent.Archetype;
                    var ori = ent.Orientation;
                    float loddist = ent._CEntityDef.lodDist;

                    Vector3 bbmin = ent.Position - ent.BSRadius; //sphere
                    Vector3 bbmax = ent.Position + ent.BSRadius;
                    Vector3 sbmin = bbmin - loddist;
                    Vector3 sbmax = bbmax + loddist;
                    if (arch != null)
                    {
                        if (loddist <= 0.0f)
                        {
                            loddist = arch.LodDist;
                        }

                        Vector3 abmin = arch.BBMin * ent.Scale; //entity box
                        Vector3 abmax = arch.BBMax * ent.Scale;
                        c[0] = abmin;
                        c[1] = new Vector3(abmin.X, abmin.Y, abmax.Z);
                        c[2] = new Vector3(abmin.X, abmax.Y, abmin.Z);
                        c[3] = new Vector3(abmin.X, abmax.Y, abmax.Z);
                        c[4] = new Vector3(abmax.X, abmin.Y, abmin.Z);
                        c[5] = new Vector3(abmax.X, abmin.Y, abmax.Z);
                        c[6] = new Vector3(abmax.X, abmax.Y, abmin.Z);
                        c[7] = abmax;

                        abmin = arch.BBMin * ent.Scale - loddist; //streaming box
                        abmax = arch.BBMax * ent.Scale + loddist;
                        s[0] = abmin;
                        s[1] = new Vector3(abmin.X, abmin.Y, abmax.Z);
                        s[2] = new Vector3(abmin.X, abmax.Y, abmin.Z);
                        s[3] = new Vector3(abmin.X, abmax.Y, abmax.Z);
                        s[4] = new Vector3(abmax.X, abmin.Y, abmin.Z);
                        s[5] = new Vector3(abmax.X, abmin.Y, abmax.Z);
                        s[6] = new Vector3(abmax.X, abmax.Y, abmin.Z);
                        s[7] = abmax;

                        bbmin = new Vector3(float.MaxValue);
                        bbmax = new Vector3(float.MinValue);
                        sbmin = new Vector3(float.MaxValue);
                        sbmax = new Vector3(float.MinValue);
                        for (int j = 0; j < 8; j++)
                        {
                            Vector3 corn = ori.Multiply(c[j]) + ent.Position;
                            bbmin = Vector3.Min(bbmin, corn);
                            bbmax = Vector3.Max(bbmax, corn);

                            corn = ori.Multiply(s[j]) + ent.Position;
                            sbmin = Vector3.Min(sbmin, corn);
                            sbmax = Vector3.Max(sbmax, corn);
                        }
                    }

                    emin = Vector3.Min(emin, bbmin);
                    emax = Vector3.Max(emax, bbmax);

                    smin = Vector3.Min(smin, sbmin);
                    smax = Vector3.Max(smax, sbmax);
                }
            }

            if (GrassInstanceBatches != null)
            {
                //var lodoffset = Vector3.Zero;// new Vector3(0, 0, 100);  //IDK WHY -neos7  //dexy: i guess it's not completely necessary... //blame neos
                foreach (var batch in GrassInstanceBatches) //thanks to Neos7
                {
                    emin = Vector3.Min(emin, batch.AABBMin);
                    emax = Vector3.Max(emax, batch.AABBMax);

                    smin = Vector3.Min(smin, (batch.AABBMin - batch.Batch.lodDist)); // + lodoffset
                    smax = Vector3.Max(smax, (batch.AABBMax + batch.Batch.lodDist)); // - lodoffset
                }
            }


            bool change = false;
            if (_CMapData.entitiesExtentsMin != emin)
            {
                _CMapData.entitiesExtentsMin = emin;
                change = true;
            }
            if (_CMapData.entitiesExtentsMax != emax)
            {
                _CMapData.entitiesExtentsMax = emax;
                change = true;
            }
            if (_CMapData.streamingExtentsMin != smin)
            {
                _CMapData.streamingExtentsMin = smin;
                change = true;
            }
            if (_CMapData.streamingExtentsMax != smax)
            {
                _CMapData.streamingExtentsMax = smax;
                change = true;
            }
            return change;
        }



        private static uint SetBit(uint value, int bit)
        {
            return (value | (1u << bit));
        }

    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapEntityDef
    {
        public Archetype Archetype { get; set; } //cached by GameFileCache on loading...
        public Vector3 BBMin;//oriented archetype AABBmin
        public Vector3 BBMax;//oriented archetype AABBmax
        public Vector3 BSCenter; //oriented archetype BS center
        public float BSRadius;//cached from archetype

        public CEntityDef _CEntityDef;
        public CEntityDef CEntityDef { get { return _CEntityDef; } set { _CEntityDef = value; } }
        private List<YmapEntityDef> ChildList { get; set; }
        public YmapEntityDef[] Children { get; set; }
        public YmapEntityDef[] ChildrenMerged;// { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 Scale { get; set; }
        public bool IsMlo { get; set; }
        public MloInstanceData MloInstance { get; set; }
        public YmapEntityDef MloParent { get; set; }
        public MCMloEntitySet MloEntitySet { get; set; }
        public Vector3 MloRefPosition { get; set; }
        public Quaternion MloRefOrientation { get; set; }
        public MetaWrapper[] Extensions { get; set; }

        public int Index { get; set; }
        public bool IsVisible; //used for rendering
        public bool ChildrenVisible; //used for rendering
        public bool ChildrenRendered; //used when rendering ymap mode to reduce LOD flashing...
        public YmapEntityDef Parent { get; set; } //for browsing convenience, also used/updated for rendering
        public MetaHash ParentName { get; set; } //just for browsing convenience

        public YmapFile Ymap { get; set; }

        public Vector3 PivotPosition = Vector3.Zero;
        public Quaternion PivotOrientation = Quaternion.Identity;
        public Vector3 WidgetPosition = Vector3.Zero;
        public Quaternion WidgetOrientation = Quaternion.Identity;


        public string Name
        {
            get
            {
                return _CEntityDef.archetypeName.ToString();
            }
        }


        public YmapEntityDef()
        {
            Scale = Vector3.One;
            Position = Vector3.One;
            Orientation = Quaternion.Identity;
        }
        public YmapEntityDef(YmapFile ymap, int index, ref CEntityDef def)
        {
            Ymap = ymap;
            Index = index;
            CEntityDef = def;
            Scale = new Vector3(new Vector2(CEntityDef.scaleXY), CEntityDef.scaleZ);
            Position = CEntityDef.position;
            Orientation = new Quaternion(CEntityDef.rotation);
            if (Orientation != Quaternion.Identity)
            {
                Orientation = Quaternion.Invert(Orientation);
            }
            IsMlo = false;

            UpdateWidgetPosition();
            UpdateWidgetOrientation();
        }

        public YmapEntityDef(YmapFile ymap, int index, ref CMloInstanceDef mlo)
        {
            Ymap = ymap;
            Index = index;
            CEntityDef = mlo.CEntityDef;
            Scale = new Vector3(new Vector2(CEntityDef.scaleXY), CEntityDef.scaleZ);
            Position = CEntityDef.position;
            Orientation = new Quaternion(CEntityDef.rotation);
            //if (Orientation != Quaternion.Identity)
            //{
            //    Orientation = Quaternion.Invert(Orientation);
            //}
            IsMlo = true;

            MloInstance = new MloInstanceData();
            MloInstance.Instance = mlo;

            UpdateWidgetPosition();
            UpdateWidgetOrientation();
        }


        public void SetArchetype(Archetype arch)
        {
            Archetype = arch;
            if (Archetype != null)
            {
                float smax = Math.Max(Scale.X, Scale.Z);
                BSRadius = Archetype.BSRadius * smax;
                BSCenter = Orientation.Multiply(Archetype.BSCenter) * Scale;
                if (Orientation == Quaternion.Identity)
                {
                    BBMin = (Archetype.BBMin * Scale) + Position;
                    BBMax = (Archetype.BBMax * Scale) + Position;
                }
                else
                {
                    BBMin = Position - BSRadius;
                    BBMax = Position + BSRadius;
                    ////not ideal: should transform all 8 corners!
                }

                if (Archetype.Type == MetaName.CMloArchetypeDef)
                {
                    //transform interior entities into world space...
                    var mloa = Archetype as MloArchetype;
                    if (MloInstance == null)
                    {
                        MloInstance = new MloInstanceData();
                    }
                    MloInstance.CreateYmapEntities(this, mloa);

                    if (BSRadius == 0.0f)
                    {
                        BSRadius = CEntityDef.lodDist;//need something so it doesn't get culled...
                    }
                }

            }

        }

        public void SetPosition(Vector3 pos)
        {
            if (MloParent != null)
            {
                //TODO: SetPosition for interior entities!
                Position = pos;
                var inst = MloParent.MloInstance;
                if (inst != null)
                {
                    //transform world position into mlo space
                    //MloRefPosition = ...
                    //MloRefOrientation = ...
                }
            }
            else
            {
                Position = pos;
                _CEntityDef.position = pos;

                if (Archetype != null)
                {
                    BSCenter = Orientation.Multiply(Archetype.BSCenter) * Scale;
                }
                if ((Archetype != null) && (Orientation == Quaternion.Identity))
                {
                    BBMin = (Archetype.BBMin * Scale) + Position;
                    BBMax = (Archetype.BBMax * Scale) + Position;
                }
                else
                {
                    BBMin = Position - (BSRadius);
                    BBMax = Position + (BSRadius);
                    ////not ideal: should transform all 8 corners!
                }



                UpdateWidgetPosition();
            }


            if (MloInstance != null)
            {
                MloInstance.SetPosition(Position);
                MloInstance.UpdateEntities();
            }

        }

        public void SetOrientation(Quaternion ori)
        {
            Quaternion inv = Quaternion.Normalize(Quaternion.Invert(ori));
            Orientation = ori;
            _CEntityDef.rotation = new Vector4(inv.X, inv.Y, inv.Z, inv.W);

            if (MloInstance != null)
            {
                MloInstance.SetOrientation(ori);
            }


            if (Archetype != null)
            {
                BSCenter = Orientation.Multiply(Archetype.BSCenter) * Scale;
            }

            UpdateWidgetPosition();
            UpdateWidgetOrientation();
        }
        public void SetOrientationInv(Quaternion inv)
        {
            Quaternion ori = Quaternion.Normalize(Quaternion.Invert(inv));
            Orientation = ori;
            _CEntityDef.rotation = new Vector4(inv.X, inv.Y, inv.Z, inv.W);

            if (MloInstance != null)
            {
                MloInstance.SetOrientation(ori);
            }


            if (Archetype != null)
            {
                BSCenter = Orientation.Multiply(Archetype.BSCenter) * Scale;
            }

            UpdateWidgetPosition();
            UpdateWidgetOrientation();
        }

        public void SetScale(Vector3 s)
        {
            Scale = new Vector3(s.X, s.X, s.Z);
            _CEntityDef.scaleXY = s.X;
            _CEntityDef.scaleZ = s.Z;
            if (Archetype != null)
            {
                float smax = Math.Max(Scale.X, Scale.Z);
                BSRadius = Archetype.BSRadius * smax;
            }
            SetPosition(Position);//update the BB
        }



        public void SetPivotPosition(Vector3 pos)
        {
            PivotPosition = pos;

            UpdateWidgetPosition();
        }

        public void SetPivotOrientation(Quaternion ori)
        {
            PivotOrientation = ori;

            UpdateWidgetOrientation();
        }


        public void SetPositionFromWidget(Vector3 pos)
        {
            SetPosition(pos - Orientation.Multiply(PivotPosition));
        }
        public void SetOrientationFromWidget(Quaternion ori)
        {
            var newori = Quaternion.Normalize(Quaternion.Multiply(ori, Quaternion.Invert(PivotOrientation)));
            var newpos = WidgetPosition - newori.Multiply(PivotPosition);
            SetOrientation(newori);
            SetPosition(newpos);
        }
        public void SetPivotPositionFromWidget(Vector3 pos)
        {
            var orinv = Quaternion.Invert(Orientation);
            SetPivotPosition(orinv.Multiply(pos - Position));
        }
        public void SetPivotOrientationFromWidget(Quaternion ori)
        {
            var orinv = Quaternion.Invert(Orientation);
            SetPivotOrientation(Quaternion.Multiply(orinv, ori));
        }


        public void UpdateWidgetPosition()
        {
            WidgetPosition = Position + Orientation.Multiply(PivotPosition);
        }
        public void UpdateWidgetOrientation()
        {
            WidgetOrientation = Quaternion.Multiply(Orientation, PivotOrientation);
        }


        public void AddChild(YmapEntityDef c)
        {
            if (ChildList == null)
            {
                ChildList = new List<YmapEntityDef>();
            }
            c.Parent = this;
            c.ParentName = CEntityDef.archetypeName;

            ChildList.Add(c);
        }

        public void ChildListToArray()
        {
            if (ChildList == null) return;
            //if (Children == null)
            //{
            Children = ChildList.ToArray();
            ChildrenMerged = Children;//include these by default in merged array
            //}
            //else
            //{
            //    List<YmapEntityDef> newc = new List<YmapEntityDef>(Children.Length + ChildList.Count);
            //    newc.AddRange(Children);
            //    newc.AddRange(ChildList);
            //    Children = newc.ToArray();
            //}
            ChildList.Clear();
            ChildList = null;
        }
        public void ChildListToMergedArray()
        {
            if (ChildList == null) return;
            if (ChildrenMerged == null)
            {
                ChildrenMerged = ChildList.ToArray();
            }
            else
            {
                List<YmapEntityDef> newc = new List<YmapEntityDef>(ChildrenMerged.Length + ChildList.Count);
                newc.AddRange(ChildrenMerged);
                newc.AddRange(ChildList);
                ChildrenMerged = newc.ToArray();
            }
            ChildList.Clear();
            ChildList = null;
        }

        public override string ToString()
        {
            return CEntityDef.ToString() + ((ChildList != null) ? (" (" + ChildList.Count.ToString() + " children) ") : " ") + CEntityDef.lodLevel.ToString();
        }

    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapGrassInstanceBatch
    {
        public Archetype Archetype { get; set; } //cached by GameFileCache on loading...
        public rage__fwGrassInstanceListDef Batch { get; set; }
        public rage__fwGrassInstanceListDef__InstanceData[] Instances { get; set; }
        public Vector3 Position { get; set; } //calculated from AABB
        public float Radius { get; set; } //calculated from AABB
        public Vector3 AABBMin { get; set; }
        public Vector3 AABBMax { get; set; }
        public Vector3 CamRel; //used for rendering...
        public float Distance; //used for rendering
        public YmapFile Ymap { get; set; }

        public override string ToString()
        {
            return Batch.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapPropInstanceBatch
    {
        public YmapFile Ymap { get; set; }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapDistantLODLights
    {
        public CDistantLODLight CDistantLODLight { get; set; }
        public uint[] colours { get; set; }
        public MetaVECTOR3[] positions { get; set; }

        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public YmapFile Ymap { get; set; }

        public void CalcBB()
        {
            if (positions != null)
            {
                Vector3 min = new Vector3(float.MaxValue);
                Vector3 max = new Vector3(float.MinValue);
                for (int i = 0; i < positions.Length; i++)
                {
                    var p = positions[i];
                    Vector3 pv = p.ToVector3();
                    min = Vector3.Min(min, pv);
                    max = Vector3.Max(max, pv);
                }
                BBMin = min;
                BBMax = max;
            }

        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapTimeCycleModifier
    {
        public CTimeCycleModifier CTimeCycleModifier { get; set; }
        public World.TimecycleMod TimeCycleModData { get; set; }

        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }

        public YmapFile Ymap { get; set; }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapCarGen
    {
        public CCarGen _CCarGen;
        public CCarGen CCarGen { get { return _CCarGen; } set { _CCarGen = value; } }

        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }

        public YmapFile Ymap { get; set; }


        public YmapCarGen(YmapFile ymap, CCarGen cargen)
        {
            float hlen = cargen.perpendicularLength * 0.5f;
            Ymap = ymap;
            CCarGen = cargen;
            Position = cargen.position;
            CalcOrientation();
            BBMin = new Vector3(-hlen);
            BBMax = new Vector3(hlen);
        }


        public void CalcOrientation()
        {
            float angl = (float)Math.Atan2(_CCarGen.orientY, _CCarGen.orientX);
            Orientation = Quaternion.RotationYawPitchRoll(0.0f, 0.0f, angl);
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
            _CCarGen.position = pos;
        }
        public void SetOrientation(Quaternion ori)
        {
            Orientation = ori;

            float len = Math.Max(_CCarGen.perpendicularLength * 1.5f, 5.0f);
            Vector3 v = new Vector3(len, 0, 0);
            Vector3 t = ori.Multiply(v);

            _CCarGen.orientX = t.X;
            _CCarGen.orientY = t.Y;
        }
        public void SetScale(Vector3 scale)
        {
            float s = scale.X;
            float hlen = s * 0.5f;
            _CCarGen.perpendicularLength = s;

            BBMin = new Vector3(-hlen);
            BBMax = new Vector3(hlen);
        }

        public void SetLength(float length)
        {
            _CCarGen.perpendicularLength = length;
            float hlen = length * 0.5f;

            BBMin = new Vector3(-hlen);
            BBMax = new Vector3(hlen);
        }

        public string NameString()
        {
            MetaHash mh = _CCarGen.carModel;
            if ((mh == 0) && (_CCarGen.popGroup != 0))
            {
                mh = _CCarGen.popGroup;
            }
            return mh.ToString();
        }

        public override string ToString()
        {
            return _CCarGen.carModel.ToString() + ", " + Position.ToString() + ", " + _CCarGen.popGroup.ToString() + ", " + _CCarGen.livery.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapOccludeModel
    {
        public Unk_2741784237 _OccludeModel;
        public Unk_2741784237 OccludeModel { get { return _OccludeModel; } set { _OccludeModel = value; } }

        public YmapFile Ymap { get; set; }


        public YmapOccludeModel(YmapFile ymap, Unk_2741784237 model)
        {
            Ymap = ymap;
            _OccludeModel = model;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YmapBoxOccluder
    {
        public Unk_975711773 _Box;
        public Unk_975711773 Box { get { return _Box; } set { _Box = value; } }

        public YmapFile Ymap { get; set; }

        public YmapBoxOccluder(YmapFile ymap, Unk_975711773 box)
        {
            Ymap = ymap;
            _Box = box;
        }

    }

}
