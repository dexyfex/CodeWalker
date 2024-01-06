using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.Core.Utils;
using CodeWalker.World;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance;
using Collections.Pooled;
using System.Threading;

namespace CodeWalker.GameFiles;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapFile : GameFile, PackedFile
{
    public Meta Meta { get; set; }
    public PsoFile Pso { get; set; }
    public RbfFile Rbf { get; set; }

    public CMapData _CMapData;
    public CMapData CMapData { get => _CMapData; set => _CMapData = value; }

    public CEntityDef[]? CEntityDefs { get; set; }
    public CMloInstanceDef[]? CMloInstanceDefs { get; set; }
    public CCarGen[]? CCarGens { get; set; }
    public CTimeCycleModifier[]? CTimeCycleModifiers { get; set; }
    public MetaHash[] physicsDictionaries { get; set; }

    public BoxOccluder[] CBoxOccluders { get; set; }
    public OccludeModel[] COccludeModels { get; set; }


    public string[] Strings { get; set; }
    public YmapEntityDef[] AllEntities { get; set; } = [];
    public YmapEntityDef[] RootEntities { get; set; } = [];
    public YmapEntityDef[] MloEntities { get; set; } = [];

    private WeakReference<YmapFile> parent = new WeakReference<YmapFile>(null);

    public YmapFile? Parent
    {
        get
        {
            if (!parent.TryGetTarget(out var target))
            {
                return null;
            }

            return target;
        }
        set
        {
            parent.SetTarget(value);
        }
    }
    public YmapFile[]? ChildYmaps { get; set; }
    public bool MergedWithParent = false;

    public bool IsScripted => (_CMapData.flags & 1) > 0;

    public YmapGrassInstanceBatch[] GrassInstanceBatches { get; set; }
    public YmapPropInstanceBatch[] PropInstanceBatches { get; set; }

    public YmapDistantLODLights DistantLODLights { get; set; }

    public YmapLODLights LODLights { get; set; }

    public YmapTimeCycleModifier[] TimeCycleModifiers { get; set; }

    public YmapCarGen[] CarGenerators { get; set; }

    public YmapBoxOccluder[] BoxOccluders { get; set; }
    public YmapOccludeModel[] OccludeModels { get; set; }


    //fields used by the editor:
    public bool HasChanged { get; set; } = false;
    public List<string>? SaveWarnings = null;
    public bool LodManagerUpdate = false; //forces the LOD manager to refresh this ymap when rendering
    public YmapEntityDef[]? LodManagerOldEntities = null; //when entities are removed, need the old ones to remove from lod manager


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

        RpfFile.LoadResourceFile(this, data, 2);

        Loaded = true;
    }

    public async ValueTask LoadAsync(byte[] data)
    {
        await RpfFile.LoadResourceFileAsync(this, data, 2);

        Loaded = true;
    }

    public void Load(byte[] data, RpfFileEntry entry)
    {
        Name = entry.Name;
        RpfFileEntry = entry;

        if (entry is not RpfResourceFileEntry resentry)
        {
            NonMetaLoad(data);
            Loaded = true;
            return;
        }

        using var rd = new ResourceDataReader(resentry, data);

        Meta = rd.ReadBlock<Meta>();//maybe null this after load to reduce memory consumption?



        CMapData = MetaTypes.GetTypedData<CMapData>(Meta, MetaName.CMapData);



        Strings = MetaTypes.GetStrings(Meta) ?? Array.Empty<string>();

        foreach(var str in Strings)
        {
            JenkIndex.Ensure(str); //just shove them in there
        }

        physicsDictionaries = MetaTypes.GetHashArray(Meta, in _CMapData.physicsDictionaries) ?? Array.Empty<MetaHash>();


        EnsureEntities(); //load all the entity data and create the YmapEntityDefs

        EnsureInstances();

        EnsureLODLights();

        EnsureDistantLODLights();

        EnsureTimeCycleModifiers();

        EnsureCarGens();

        EnsureBoxOccluders();

        EnsureOccludeModels();

        EnsureContainerLods();


        #region data block test and old code

        //foreach (var block in Meta.DataBlocks)
        //{
        //    switch (block.StructureNameHash)
        //    {
        //        case (MetaName)MetaTypeName.STRING:
        //        case (MetaName)MetaTypeName.POINTER:
        //        case (MetaName)MetaTypeName.HASH:
        //        case (MetaName)MetaTypeName.UINT:
        //        case (MetaName)MetaTypeName.VECTOR3: //distant lod lights uses this
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
        //            var indicesoffset = unk5.numVertsInBytes;
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

//#if !DEBUG
//            Meta = null; //this object is required for XML conversion! can't just let go of it here
//#endif
    }



    private void NonMetaLoad(byte[] data)
    {
        Console.WriteLine($"Loading NonMeta ymap {RpfFileEntry.Path}");
        //non meta not supported yet! but see what's in there...
        if (RbfFile.IsRBF(data.AsSpan(0, 4)))
        {
            Rbf = new RbfFile();
            Rbf.Load(data);
        }
        else if (PsoFile.IsPSO(data.AsSpan(0, 4)))
        {
            Pso = new PsoFile();
            Pso.Load(data);
            //PsoTypes.EnsurePsoTypes(Pso);
        }
        else
        {
        }

    }



    private void EnsureEntities()
    {
        //CMloInstanceDefs = MetaTypes.ConvertDataArray<CMloInstanceDef>(Meta, MetaName.CMloInstanceDef, CMapData.entities);
        CMloInstanceDefs = MetaTypes.GetTypedDataArray<CMloInstanceDef>(Meta, MetaName.CMloInstanceDef) ?? [];

        //var eptrs = MetaTypes.GetPointerArray(Meta, _CMapData.entities);
        //CEntityDefs = MetaTypes.ConvertDataArray<CEntityDef>(Meta, MetaName.CEntityDef, CMapData.entities);
        CEntityDefs = MetaTypes.GetTypedDataArray<CEntityDef>(Meta, MetaName.CEntityDef) ?? [];

        int instcount = CEntityDefs.Length + CMloInstanceDefs.Length;

        if (instcount > 0)
        {

            //build the entity hierarchy.
            List<YmapEntityDef> roots = new List<YmapEntityDef>(instcount);
            List<YmapEntityDef> alldefs = new List<YmapEntityDef>(instcount);
            List<YmapEntityDef> mlodefs = null;

            if (CEntityDefs.Length > 0)
            {
                for (int i = 0; i < CEntityDefs.Length; i++)
                {
                    YmapEntityDef d = new YmapEntityDef(this, i, CEntityDefs[i]);
                    alldefs.Add(d);
                }
            }
            if (CMloInstanceDefs.Length > 0)
            {
                mlodefs = new List<YmapEntityDef>(CMloInstanceDefs.Length);
                for (int i = 0; i < CMloInstanceDefs.Length; i++)
                {
                    YmapEntityDef d = new YmapEntityDef(this, i, CMloInstanceDefs[i]);
                    uint[] defentsets = MetaTypes.GetUintArray(Meta, in CMloInstanceDefs[i].defaultEntitySets);
                    if (d.MloInstance is not null)
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
                int pind = d._CEntityDef.parentIndex;
                bool isroot = false;
                if ((pind < 0) || (pind >= alldefs.Count) || d.LodInParentYmap)
                {
                    isroot = true;
                }
                else
                {
                    YmapEntityDef p = alldefs[pind];
                    if ((p._CEntityDef.lodLevel <= d._CEntityDef.lodLevel) ||
                        ((p._CEntityDef.lodLevel == rage__eLodType.LODTYPES_DEPTH_ORPHANHD) &&
                         (d._CEntityDef.lodLevel != rage__eLodType.LODTYPES_DEPTH_ORPHANHD)))
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
            MloEntities = mlodefs?.ToArray() ?? Array.Empty<YmapEntityDef>();

            foreach(var ent in AllEntities)
            {
                ent.Extensions = MetaTypes.GetExtensions(Meta, in ent._CEntityDef.extensions) ?? Array.Empty<MetaWrapper>();
            }
        }

    }

    private void EnsureInstances()
    {
        if (_CMapData.instancedData.GrassInstanceList.Count1 != 0)
        {
            rage__fwGrassInstanceListDef[] batches = MetaTypes.ConvertDataArray<rage__fwGrassInstanceListDef>(Meta, MetaName.rage__fwGrassInstanceListDef, in _CMapData.instancedData.GrassInstanceList) ?? Array.Empty<rage__fwGrassInstanceListDef>();


            if (batches.Length == 0)
                return;

            YmapGrassInstanceBatch[] gbatches = new YmapGrassInstanceBatch[batches.Length];
            for (int i = 0; i < batches.Length; i++)
            {
                var batch = batches[i];
                rage__fwGrassInstanceListDef__InstanceData[] instdatas = MetaTypes.ConvertDataArray<rage__fwGrassInstanceListDef__InstanceData>(Meta, MetaName.rage__fwGrassInstanceListDef__InstanceData, in batch.InstanceList);
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
        if (_CMapData.instancedData.PropInstanceList.Count1 != 0)
        {
        }
    }

    private void EnsureLODLights()
    {
        if (_CMapData.LODLightsSOA.direction.Count1 != 0)
        {
            var soa = _CMapData.LODLightsSOA;
            LODLights = new YmapLODLights();
            LODLights.Ymap = this;
            LODLights.CLODLight = soa;
            LODLights.direction = MetaTypes.ConvertDataArray<MetaVECTOR3>(Meta, MetaName.FloatXYZ, in soa.direction);
            LODLights.falloff = MetaTypes.GetFloatArray(Meta, in soa.falloff);
            LODLights.falloffExponent = MetaTypes.GetFloatArray(Meta, in soa.falloffExponent);
            LODLights.timeAndStateFlags = MetaTypes.GetUintArray(Meta, in soa.timeAndStateFlags);
            LODLights.hash = MetaTypes.GetUintArray(Meta, in soa.hash);
            LODLights.coneInnerAngle = MetaTypes.GetByteArray(Meta, in soa.coneInnerAngle);
            LODLights.coneOuterAngleOrCapExt = MetaTypes.GetByteArray(Meta, in soa.coneOuterAngleOrCapExt);
            LODLights.coronaIntensity = MetaTypes.GetByteArray(Meta, in soa.coronaIntensity);
            LODLights.CalcBB();
        }
    }

    private void EnsureDistantLODLights()
    {
        if (_CMapData.DistantLODLightsSOA.position.Count1 != 0)
        {
            var soa = _CMapData.DistantLODLightsSOA;
            DistantLODLights = new YmapDistantLODLights();
            DistantLODLights.Ymap = this;
            DistantLODLights.CDistantLODLight = soa;
            DistantLODLights.colours = MetaTypes.GetUintArray(Meta, in soa.RGBI);
            DistantLODLights.positions = MetaTypes.ConvertDataArray<MetaVECTOR3>(Meta, MetaName.FloatXYZ, in soa.position);
            DistantLODLights.CalcBB();
        }
    }

    private void EnsureTimeCycleModifiers()
    {
        CTimeCycleModifiers = MetaTypes.ConvertDataArray<CTimeCycleModifier>(Meta, MetaName.CTimeCycleModifier, in _CMapData.timeCycleModifiers);
        if (CTimeCycleModifiers is null || CTimeCycleModifiers.Length == 0)
            return;

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

    private void EnsureCarGens()
    {
        CCarGens = MetaTypes.ConvertDataArray<CCarGen>(Meta, MetaName.CCarGen, in _CMapData.carGenerators);
        if (CCarGens is null || CCarGens.Length == 0)
            return;

        //string str = MetaTypes.GetTypesInitString(resentry, Meta); //to generate structinfos and enuminfos
        CarGenerators = new YmapCarGen[CCarGens.Length];
        for (int i = 0; i < CCarGens.Length; i++)
        {
            CarGenerators[i] = new YmapCarGen(this, CCarGens[i]);
        }
    }

    private void EnsureBoxOccluders()
    {
        CBoxOccluders = MetaTypes.ConvertDataArray<BoxOccluder>(Meta, MetaName.BoxOccluder, in _CMapData.boxOccluders);
        if (CBoxOccluders is null || CBoxOccluders.Length == 0)
            return;

        BoxOccluders = new YmapBoxOccluder[CBoxOccluders.Length];
        for (int i = 0; i < CBoxOccluders.Length; i++)
        {
            BoxOccluders[i] = new YmapBoxOccluder(this, CBoxOccluders[i]);
            BoxOccluders[i].Index = i;
        }
    }

    private void EnsureOccludeModels()
    {
        COccludeModels = MetaTypes.ConvertDataArray<OccludeModel>(Meta, MetaName.OccludeModel, in _CMapData.occludeModels);

        if (COccludeModels is null || COccludeModels.Length == 0)
            return;

        OccludeModels = new YmapOccludeModel[COccludeModels.Length];
        for (int i = 0; i < COccludeModels.Length; i++)
        {
            OccludeModels[i] = new YmapOccludeModel(this, COccludeModels[i]);
            OccludeModels[i].Index = i;
            OccludeModels[i].Load(Meta);

        }
    }

    private void EnsureContainerLods()
    {

        //TODO: containerLods
        if (_CMapData.containerLods.Count1 > 0)
        {
            //string str = MetaTypes.GetTypesInitString(Meta); //to generate structinfos and enuminfos


        }

    }


    public void BuildCEntityDefs()
    {
        //recreates the CEntityDefs and CMloInstanceDefs arrays from AllEntities.
        //TODO: save entity extensions!!?

        CEntityDefs = Array.Empty<CEntityDef>();
        CMloInstanceDefs = Array.Empty<CMloInstanceDef>();
        if (AllEntities.Length == 0)
        {
            return;
        }


        List<CEntityDef> centdefs = new List<CEntityDef>();
        List<CMloInstanceDef> cmlodefs = new List<CMloInstanceDef>();

        foreach(var ent in AllEntities)
        {
            if (ent.MloInstance != null)
            {
                cmlodefs.Add(ent.MloInstance._Instance);
            }
            else
            {
                centdefs.Add(ent._CEntityDef);
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
        if (GrassInstanceBatches == null || GrassInstanceBatches.Length == 0)
        {
            return;
        }

        //int count = GrassInstanceBatches.Length;

        foreach(var g in GrassInstanceBatches)
        {
            ref var b = ref g.Batch;

            b.BatchAABB = new rage__spdAABB(in g.AABBMin, in g.AABBMax);
        }
        //for (int i = 0; i < count; i++)
        //{

        //}
    }
    public void BuildLodLights()
    {
        if (LODLights == null)
            return;
        LODLights.RebuildFromLodLights();
    }
    public void BuildDistantLodLights()
    {
        //how to rebuild these here? the LODlights array is on the child ymap...
        //for now, they are being updated as they are edited in project window
    }
    public void BuildBoxOccluders()
    {
        if (BoxOccluders is null || BoxOccluders.Length == 0)
            return;

        var boxes = new BoxOccluder[BoxOccluders.Length];
        for (int i = 0; i < BoxOccluders.Length; i++)
        {
            var box = BoxOccluders[i];
            box.UpdateBoxStruct();
            boxes[i] = box._Box;
        }

        CBoxOccluders = boxes;

    }
    public void BuildOccludeModels()
    {
        if (OccludeModels is null || OccludeModels.Length == 0)
            return;
        //nothing to do here, has to be done later due to embedded data
    }

    public byte[] Save()
    {
        //direct save to a raw, compressed ymap file (openIV-compatible format)


        //since Ymap object contents have been modified, need to recreate the arrays which are what is saved.
        BuildCEntityDefs(); //technically this isn't required anymore since the CEntityDefs is no longer used for saving.
        BuildCCarGens();
        BuildInstances();
        BuildLodLights();
        BuildDistantLodLights();
        BuildBoxOccluders();
        BuildOccludeModels();

        //TODO:
        //BuildTimecycleModifiers(); //already being saved - update them..
        //BuildContainerLods();



        MetaBuilder mb = new MetaBuilder();


        var mdb = mb.EnsureBlock(MetaName.CMapData);

        CMapData mapdata = CMapData;



        if (AllEntities.Length > 0)
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
                    ent.MloInstance.UpdateDefaultEntitySets();

                    ent.MloInstance._Instance.CEntityDef = ent.CEntityDef; //overwrite with all the updated values..
                    ent.MloInstance._Instance.defaultEntitySets = mb.AddUintArrayPtr(ent.MloInstance.defaultEntitySets);

                    ptrs[i] = mb.AddItemPtr(MetaName.CMloInstanceDef, in ent.MloInstance._Instance);
                }
                else
                {
                    ptrs[i] = mb.AddItemPtr(MetaName.CEntityDef, in ent._CEntityDef);
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



        //clear everything else out for now - TODO: fix
        if (mapdata.containerLods.Count1 != 0) LogSaveWarning("containerLods were not saved. (TODO!)");
        if (mapdata.instancedData.PropInstanceList.Count1 != 0) LogSaveWarning("instancedData.PropInstanceList was not saved. (TODO!)");
        mapdata.containerLods = new Array_Structure();

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

        if ((LODLights != null) && (LODLights.direction != null))
        {
            var soa = new CLODLight();
            soa.direction = mb.AddItemArrayPtr(MetaName.FloatXYZ, LODLights.direction);
            soa.falloff = mb.AddFloatArrayPtr(LODLights.falloff);
            soa.falloffExponent = mb.AddFloatArrayPtr(LODLights.falloffExponent);
            soa.timeAndStateFlags = mb.AddUintArrayPtr(LODLights.timeAndStateFlags);
            soa.hash = mb.AddUintArrayPtr(LODLights.hash);
            soa.coneInnerAngle = mb.AddByteArrayPtr(LODLights.coneInnerAngle);
            soa.coneOuterAngleOrCapExt = mb.AddByteArrayPtr(LODLights.coneOuterAngleOrCapExt);
            soa.coronaIntensity = mb.AddByteArrayPtr(LODLights.coronaIntensity);
            mapdata.LODLightsSOA = soa;
        }
        else
        {
            mapdata.LODLightsSOA = new CLODLight();
        }
        if ((DistantLODLights != null) && (DistantLODLights.positions != null))
        {
            var soa = DistantLODLights.CDistantLODLight;//to copy base vars
            soa.position = mb.AddItemArrayPtr(MetaName.FloatXYZ, DistantLODLights.positions);
            soa.RGBI = mb.AddUintArrayPtr(DistantLODLights.colours);
            mapdata.DistantLODLightsSOA = soa;
        }
        else
        {
            mapdata.DistantLODLightsSOA = new CDistantLODLight();
        }

        if ((CBoxOccluders != null) && (CBoxOccluders.Length > 0))
        {
            mapdata.boxOccluders = mb.AddItemArrayPtr(MetaName.BoxOccluder, CBoxOccluders);
        }
        else
        {
            mapdata.boxOccluders = new Array_Structure();
        }
        if ((OccludeModels != null) && (OccludeModels.Length > 0))
        {
            COccludeModels = new OccludeModel[OccludeModels.Length];
            for (int i = 0; i < OccludeModels.Length; i++)
            {
                var model = OccludeModels[i];
                model.BuildVertices();
                model.BuildData();
                var cocc = model._OccludeModel;
                cocc.verts = mb.AddDataBlockPtr(model.Data, (MetaName)MetaStructureEntryDataType.UnsignedByte);//17
                COccludeModels[i] = cocc;
            }
            mapdata.occludeModels = mb.AddItemArrayPtr(MetaName.OccludeModel, COccludeModels);
        }
        else
        {
            mapdata.occludeModels = new Array_Structure();
        }


        var block = new CBlockDesc();
        block.name = mb.AddStringPtr(Path.GetFileNameWithoutExtension(Name));
        block.exportedBy = mb.AddStringPtr("CodeWalker");
        block.time = mb.AddStringPtr(DateTime.UtcNow.ToString("dd MMMM yyyy HH:mm"));

        mapdata.block = block;


        string name = Path.GetFileNameWithoutExtension(Name);
        uint nameHash = JenkHash.GenHash(name);
        mapdata.name = new MetaHash(nameHash);//make sure name is upto date...


        mb.AddItem(MetaName.CMapData, in mapdata);



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
        if (AllEntities.Length > 0)
        {
            mb.AddStructureInfo(MetaName.CEntityDef);
            mb.AddStructureInfo(MetaName.CMloInstanceDef);
            mb.AddEnumInfo(MetaName.rage__eLodType); //LODTYPES_
            mb.AddEnumInfo(MetaName.rage__ePriorityLevel);  //PRI_
        }
        if ((CTimeCycleModifiers != null) && (CTimeCycleModifiers.Length > 0))
        {
            mb.AddStructureInfo(MetaName.CTimeCycleModifier);
        }
        if ((CCarGens != null) && (CCarGens.Length > 0))
        {
            mb.AddStructureInfo(MetaName.CCarGen);
        }
        if ((LODLights != null) && (LODLights.direction != null))
        {
            mb.AddStructureInfo(MetaName.FloatXYZ);
        }
        if (DistantLODLights != null && DistantLODLights.positions.Length > 0)
        {
            mb.AddStructureInfo(MetaName.FloatXYZ);
        }
        if ((CBoxOccluders != null) && (CBoxOccluders.Length > 0))
        {
            mb.AddStructureInfo(MetaName.BoxOccluder);
        }
        if ((COccludeModels != null) && (COccludeModels.Length > 0))
        {
            mb.AddStructureInfo(MetaName.OccludeModel);
        }


        Meta meta = mb.GetMeta();

        byte[] data = ResourceBuilder.Build(meta, 2); //ymap is version 2...


        return data;
    }

    private void LogSaveWarning(string w)
    {
        SaveWarnings ??= new List<string>();
        SaveWarnings.Add(w);
    }




    public void EnsureChildYmaps(GameFileCache gfc)
    {
        if (ChildYmaps is null)
        {
            //no children here... look for child ymap....
            var node = gfc.GetMapNode(RpfFileEntry.ShortNameHash);
            if (node?.Children is not null && node.Children.Length > 0)
            {
                ChildYmaps = new YmapFile[node.Children.Length];
                for (int i = 0; i < ChildYmaps.Length; i++)
                {
                    var chash = node.Children[i].Name;
                    ChildYmaps[i] = gfc.GetYmap(chash);
                    if (ChildYmaps[i] is null)
                    {
                        Console.WriteLine($"Couldn't find child ymap! {chash} for {Name}");
                        //couldn't find child ymap..
                    }
                }
            }
        }


        bool needupd = false;
        if (ChildYmaps is not null)
        {
            for (int i = 0; i < ChildYmaps.Length; i++)
            {
                var cmap = ChildYmaps[i];
                if (cmap is null)
                    continue; //nothing here..
                if (!cmap.Loaded)
                {
                    //ChildYmaps[i] = gfc.GetYmap(cmap.Hash); //incase no load was requested.
                    cmap = gfc.GetYmap(cmap.Key.Hash);
                    if (cmap is null)
                        continue;
                    ChildYmaps[i] = cmap;
                }
                if (cmap.Loaded && !cmap.MergedWithParent)
                {
                    needupd = true;
                }
            }
        }

        if (ChildYmaps is not null && needupd)
        {
            using PooledList<YmapEntityDef> newroots = new PooledList<YmapEntityDef>(RootEntities);
            for (int i = 0; i < ChildYmaps.Length; i++)
            {
                var cmap = ChildYmaps[i];
                if (cmap is null)
                {
                    continue; //nothing here..
                }
                //cmap.EnsureChildYmaps();
                if (cmap.Loaded && !cmap.MergedWithParent)
                {
                    cmap.MergedWithParent = true;
                    foreach (var rcent in cmap.RootEntities)
                    {
                        int pind = rcent._CEntityDef.parentIndex;
                        //if (pind < 0)
                        //{
                        //    if (rcent._CEntityDef.lodLevel != rage__eLodType.LODTYPES_DEPTH_ORPHANHD)
                        //    {
                        //    }
                        //    //pind = 0;
                        //}
                        if (pind >= 0 && pind < AllEntities.Length && !rcent.LodInParentYmap)
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
            foreach(var ent in AllEntities)
            {
                ent.ChildListToMergedArray();
            }

            RootEntities = newroots.ToArray();
        }


    }


    public void ConnectToParent(YmapFile pymap)
    {
        Parent = pymap;
        foreach(var ent in RootEntities)
        {
            int pind = ent._CEntityDef.parentIndex;
            if (pind >= 0) //connect root entities to parents if they have them..
            {
                    
                if (pymap.AllEntities != null && pymap.AllEntities.Length > 0)
                {
                    if (pind < pymap.AllEntities.Length)
                    {
                        var p = pymap.AllEntities[pind];
                        ent.Parent = p;
                        ent.ParentName = p._CEntityDef.archetypeName;
                    }
                }
                else
                {
                    Console.WriteLine($"Parent not loaded yet for {pymap.Name}");
                }
            }
        }
        if (LODLights is not null)
        {
            var parent = Parent;
            if (parent?.DistantLODLights is not null)
            {
                LODLights.Init(parent.DistantLODLights);
            }
        }
    }





    public void AddEntity(YmapEntityDef ent)
    {
        //used by the editor to add to the ymap.

        var allents = new List<YmapEntityDef>(AllEntities);

        ent.Index = allents.Count;
        ent.Ymap = this;
        allents.Add(ent);
        AllEntities = allents.ToArray();


        if (ent.Parent is null || ent.Parent.Ymap != this)
        {
            //root entity, add to roots.

            List<YmapEntityDef> rootents = new List<YmapEntityDef>(RootEntities);
            rootents.Add(ent);
            RootEntities = rootents.ToArray();
        }

        HasChanged = true;
        LodManagerUpdate = true;
    }

    public bool RemoveEntity(YmapEntityDef ent)
    {
        //used by the editor to remove from the ymap.
        if (ent is null)
            return false;

        var res = true;

        int idx = ent.Index;
        List<YmapEntityDef> newAllEntities = new List<YmapEntityDef>();
        List<YmapEntityDef> newRootEntities = new List<YmapEntityDef>();

        if (AllEntities.Length > 0)
        {
            for (int i = 0; i < AllEntities.Length; i++)
            {
                var oent = AllEntities[i];
                oent.Index = newAllEntities.Count;
                if (oent != ent)
                {
                    newAllEntities.Add(oent);
                }
                else if (i != idx)
                {
                    res = false; //indexes didn't match.. this shouldn't happen!
                }
            }
        }
        foreach(var oent in RootEntities)
        {
            if (oent != ent)
                newRootEntities.Add(oent);
        }

        if (AllEntities.Length == newAllEntities.Count || RootEntities.Length == newRootEntities.Count)
        {
            res = false;
        }

        LodManagerOldEntities = AllEntities;
        AllEntities = newAllEntities.ToArray();
        RootEntities = newRootEntities.ToArray();

        HasChanged = true;
        LodManagerUpdate = true;

        return res;
    }


    public void AddCarGen(YmapCarGen cargen)
    {
        List<YmapCarGen> cargens = new List<YmapCarGen>();
        if (CarGenerators != null)
            cargens.AddRange(CarGenerators);
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
            foreach(var cg in CarGenerators)
            {
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


    public void AddLodLight(YmapLODLight lodlight)
    {
        if (LODLights == null)
        {
            LODLights = new YmapLODLights();
            LODLights.Ymap = this;
        }
        List<YmapLODLight> lodlights = new List<YmapLODLight>();
        if (LODLights?.LodLights != null)
            lodlights.AddRange(LODLights.LodLights);
        lodlight.LodLights = this.LODLights;
        lodlight.Index = lodlights.Count;
        lodlights.Add(lodlight);
        LODLights.LodLights = lodlights.ToArray();

        HasChanged = true;

        var parent = Parent;
        if (parent?.DistantLODLights is not null)
        {
            parent.DistantLODLights.RebuildFromLodLights(LODLights.LodLights);
            parent.HasChanged = true;
        }
    }

    public bool RemoveLodLight(YmapLODLight lodlight)
    {
        if (lodlight == null)
            return false;

        List<YmapLODLight> newlodlights = new List<YmapLODLight>();

        var lodlights = LODLights?.LodLights;
        if (lodlights != null)
        {
            foreach(var ll in lodlights)
            {
                if (ll != lodlight)
                {
                    newlodlights.Add(ll);
                }
            }
            if (newlodlights.Count == lodlights.Length)
            {
                return false; //nothing removed... wasn't present?
            }
        }

        for (int i = 0; i < newlodlights.Count; i++)
        {
            newlodlights[i].Index = i;
        }

        LODLights.LodLights = newlodlights.ToArray();

        HasChanged = true;

        var parent = Parent;
        if (parent?.DistantLODLights != null)
        {
            parent.DistantLODLights.RebuildFromLodLights(LODLights.LodLights);
            parent.HasChanged = true;
        }

        return true;
    }


    public void AddBoxOccluder(YmapBoxOccluder box)
    {
        if (box is null)
            return;
        var boxes = new List<YmapBoxOccluder>();
        if (BoxOccluders is not null)
            boxes.AddRange(BoxOccluders);
        box.Ymap = this;
        box.Index = boxes.Count;
        boxes.Add(box);
        BoxOccluders = boxes.ToArray();

        HasChanged = true;
    }

    public bool RemoveBoxOccluder(YmapBoxOccluder box)
    {
        if (box == null) return false;
        var newboxes = new List<YmapBoxOccluder>();
        if (BoxOccluders != null)
        {
            foreach (var oldbox in BoxOccluders)
            {
                if (oldbox != box)
                {
                    oldbox.Index = newboxes.Count;
                    newboxes.Add(oldbox);
                }
            }
            if (newboxes.Count == BoxOccluders.Length)
            {
                return false;//nothing removed... wasn't present?
            }
        }

        BoxOccluders = newboxes.ToArray();

        HasChanged = true;

        return true;
    }


    public void AddOccludeModel(YmapOccludeModel model)
    {
        if (model is null)
            return;
        var models = new List<YmapOccludeModel>();
        if (OccludeModels != null)
            models.AddRange(OccludeModels);
        model.Ymap = this;
        models.Add(model);
        OccludeModels = models.ToArray();

        HasChanged = true;
    }

    public bool RemoveOccludeModel(YmapOccludeModel model)
    {
        if (model == null) return false;
        var newmodels = new List<YmapOccludeModel>();
        if (OccludeModels != null)
        {
            foreach (var oldmodel in OccludeModels)
            {
                if (oldmodel != model)
                {
                    oldmodel.Index = newmodels.Count;
                    newmodels.Add(oldmodel);
                }
            }
            if (newmodels.Count == OccludeModels.Length)
            {
                return false;//nothing removed... wasn't present?
            }
        }

        OccludeModels = newmodels.ToArray();

        HasChanged = true;

        return true;
    }


    public void AddOccludeModelTriangle(YmapOccludeModelTriangle tri)
    {
        if (tri?.Model is null)
            return;

        var tris = tri.Model.Triangles.ToList();
        tri.Index = tris.Count;
        tris.Add(tri);
        tri.Model.Triangles = tris.ToArray();

        //tri.Model.BuildBVH();
        //...

        HasChanged = true;
    }

    public bool RemoveOccludeModelTriangle(YmapOccludeModelTriangle tri)
    {
        if (tri?.Model is null)
            return false;

        var newtris = new List<YmapOccludeModelTriangle>();
        if (tri.Model.Triangles is not null)
        {
            newtris.EnsureCapacity(tri.Model.Triangles.Length);
            foreach (var oldtri in tri.Model.Triangles)
            {
                if (oldtri != tri)
                {
                    oldtri.Index = newtris.Count;
                    newtris.Add(oldtri);
                }
            }
        }
        tri.Model.Triangles = newtris.ToArray();
        //tri.Model.BuildBVH();
        //...

        HasChanged = true;

        return true;
    }


    public void AddGrassBatch(YmapGrassInstanceBatch newbatch)
    {
        List<YmapGrassInstanceBatch> batches = new List<YmapGrassInstanceBatch>();
        if (GrassInstanceBatches is not null && GrassInstanceBatches.Length > 0)
            batches.AddRange(GrassInstanceBatches);
        newbatch.Ymap = this;
        batches.Add(newbatch);
        GrassInstanceBatches = batches.ToArray();

        HasChanged = true;
        UpdateGrassPhysDict(true);
    }

    public bool RemoveGrassBatch(YmapGrassInstanceBatch batch)
    {
        if (batch is null)
            return false;

        List<YmapGrassInstanceBatch> batches = new List<YmapGrassInstanceBatch>(GrassInstanceBatches.Length);

        if (GrassInstanceBatches is not null)
        {
            foreach(var gb in GrassInstanceBatches)
            {
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

        if (batches.Count <= 0)
        {
            UpdateGrassPhysDict(false);
        }

        GrassInstanceBatches = batches.ToArray();

        HasChanged = true;

        return true;
    }




    public void SetName(string newname)
    {
        var newnamex = newname + ".ymap";
        var newhash = JenkHash.GenHashLower(newname);
        JenkIndex.EnsureLower(newname);
        if (RpfFileEntry is not null)
        {
            RpfFileEntry.Name = newnamex;
        }
        Name = newnamex;
        _CMapData.name = newhash;
    }
    public void SetFilePath(string filepath)
    {
        FilePath = filepath.ToLowerInvariant();
        var newname = Path.GetFileNameWithoutExtension(filepath);
        SetName(newname);
    }


    public bool CalcFlags()
    {
        uint flags = 0;
        uint contentFlags = 0;

        foreach (var yent in AllEntities)
        {
            switch (yent._CEntityDef.lodLevel)
            {
                case rage__eLodType.LODTYPES_DEPTH_ORPHANHD:
                case rage__eLodType.LODTYPES_DEPTH_HD:
                    contentFlags = SetBit(contentFlags, 0); //1
                    break;
                case rage__eLodType.LODTYPES_DEPTH_LOD:
                    contentFlags = SetBit(contentFlags, 1); //2
                    flags = SetBit(flags, 1); //2
                    break;
                case rage__eLodType.LODTYPES_DEPTH_SLOD1:
                    contentFlags = SetBit(contentFlags, 4); //16
                    flags = SetBit(flags, 1); //2
                    break;
                case rage__eLodType.LODTYPES_DEPTH_SLOD2:
                case rage__eLodType.LODTYPES_DEPTH_SLOD3:
                case rage__eLodType.LODTYPES_DEPTH_SLOD4:
                    contentFlags = SetBit(contentFlags, 2); //4
                    contentFlags = SetBit(contentFlags, 4); //16
                    flags = SetBit(flags, 1); //2
                    break;
            }
            if (yent.MloInstance != null)
            {
                contentFlags = SetBit(contentFlags, 3); //8  //(interior instance)
            }
        }

        if (CMloInstanceDefs.Length > 0)
        {
            contentFlags = SetBit(contentFlags, 3); //8  //(interior instance) //is this still necessary?
        }
        if (physicsDictionaries.Length > 0)
        {
            contentFlags = SetBit(contentFlags, 6); //64
        }
        if ((GrassInstanceBatches != null) && (GrassInstanceBatches.Length > 0))
        {
            contentFlags = SetBit(contentFlags, 10); //64
        }
        if ((LODLights != null) && ((LODLights.direction?.Length ?? 0) > 0))
        {
            contentFlags = SetBit(contentFlags, 7); //128
        }
        if ((DistantLODLights != null) && ((DistantLODLights.positions?.Length ?? 0) > 0))
        {
            flags = SetBit(flags, 1); //2
            contentFlags = SetBit(contentFlags, 8); //256
        }
        if ((BoxOccluders != null) || (OccludeModels != null))
        {
            contentFlags = SetBit(contentFlags, 5); //32
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

        foreach(var ent in AllEntities)
        {
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
                    Vector3 corn = ori.Multiply(in c[j]) + ent.Position;
                    Vectors.Min(in bbmin, in corn, out bbmin);
                    Vectors.Max(in bbmax, in corn, out bbmax);

                    corn = ori.Multiply(in s[j]) + ent.Position;
                    Vectors.Min(in sbmin, in corn, out sbmin);
                    Vectors.Max(in sbmax, in corn, out sbmax);
                }
            }

            Vectors.Min(in emin, in bbmin, out emin);
            Vectors.Max(in emax, in bbmax, out emax);
            Vectors.Min(in smin, in sbmin, out smin);
            Vectors.Max(in smax, in sbmax, out smax);
        }

        if (GrassInstanceBatches != null)
        {
            //var lodoffset = Vector3.Zero;// new Vector3(0, 0, 100);  //IDK WHY -neos7  //dexy: i guess it's not completely necessary... //blame neos
            foreach (var batch in GrassInstanceBatches) //thanks to Neos7
            {
                Vectors.Min(in emin, in batch.AABBMin, out emin);
                Vectors.Max(in emax, in batch.AABBMax, out emax);
                Vectors.Min(in smin, (batch.AABBMin - batch.Batch.lodDist), out smin); // + lodoffset
                Vectors.Max(in smax, (batch.AABBMax + batch.Batch.lodDist), out smax); // - lodoffset
            }
        }

        if (CarGenerators != null)
        {
            foreach (var cargen in CarGenerators)
            {
                var len = cargen._CCarGen.perpendicularLength;
                Vectors.Min(in emin, cargen.Position - len, out emin);
                Vectors.Max(in emax, cargen.Position + len, out emax);
                Vectors.Min(in smin, cargen.Position - len*2.0f, out smin); //just a random guess, maybe should be more?
                Vectors.Max(in smax, cargen.Position + len*2.0f, out smax);
            }
        }

        if (LODLights != null)
        {
            LODLights.CalcBB();
            Vectors.Min(in emin, LODLights.BBMin - 20.0f, out emin); //about right
            Vectors.Max(in emax, LODLights.BBMax + 20.0f, out emax);
            Vectors.Min(in smin, LODLights.BBMin - 950.0f, out smin); //seems correct
            Vectors.Max(in smax, LODLights.BBMax + 950.0f, out smax);
        }

        if (DistantLODLights != null)
        {
            DistantLODLights.CalcBB();
            Vectors.Min(in emin, DistantLODLights.BBMin - 20.0f, out emin); //not exact, but probably close enough
            Vectors.Max(in emax, DistantLODLights.BBMax + 20.0f, out emax);
            Vectors.Min(in smin, (DistantLODLights.BBMin - 3000.0f), out smin); //seems correct
            Vectors.Max(in smax, (DistantLODLights.BBMax + 3000.0f), out smax);
        }

        if (BoxOccluders != null)
        {
            foreach (var box in BoxOccluders)
            {
                var siz = box.Size.Length() * 0.5f;//should really use box rotation instead....
                Vectors.Min(in emin, box.Position - siz, out emin);
                Vectors.Max(in emax, box.Position + siz, out emax);
                Vectors.Min(in smin, box.Position - siz, out smin);//check this! for some vanilla ymaps it seems right, others not
                Vectors.Max(in smax, box.Position + siz, out smax);//occluders don't seem to have a loddist
            }
        }

        if (OccludeModels != null)
        {
            foreach (var model in OccludeModels)
            {
                var boundingBox = model.BVH?._Box ?? model._OccludeModel.BoundingBox;
                Vectors.Min(in emin, in boundingBox.Minimum, out emin);//this needs to be updated!
                Vectors.Max(in emax, in boundingBox.Maximum, out emax);
                Vectors.Min(in smin, in boundingBox.Minimum, out smin);//check this! for some vanilla ymaps it seems right, others not
                Vectors.Max(in smax, in boundingBox.Maximum, out smax);//occluders don't seem to have a loddist
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


    private void UpdateGrassPhysDict(bool add)
    {
        var physDict = physicsDictionaries?.ToList() ?? new List<MetaHash>();
        var vproc1 = JenkHash.GenHash("v_proc1");
        var vproc2 = JenkHash.GenHash("v_proc2"); // I think you need vproc2 as well.
        var change = false;
        if (!physDict.Contains(vproc1))
        {
            change = true;
            if (add)
                physDict.Add(vproc1);
            else
                physDict.Remove(vproc1);
        }
        if (!physDict.Contains(vproc2))
        {
            change = true;
            if (add)
                physDict.Add(vproc2);
            else
                physDict.Remove(vproc2);
        }
        if (change)
            physicsDictionaries = physDict.ToArray();
    }

    public void InitYmapEntityArchetypes(GameFileCache gfc)
    {
        if (AllEntities.Length > 0)
        {
            foreach(var ent in AllEntities)
            {
                var arch = gfc.GetArchetype(ent._CEntityDef.archetypeName);
                ent.SetArchetype(arch);
                if (ent.IsMlo)
                    ent.MloInstance.InitYmapEntityArchetypes(gfc);
            }
        }
        if (GrassInstanceBatches != null && GrassInstanceBatches.Length > 0)
        {
            foreach(var batch in GrassInstanceBatches)
            {
                batch.Archetype = gfc.GetArchetype(batch.Batch.archetypeName);
            }
        }

        if (TimeCycleModifiers != null && TimeCycleModifiers.Length > 0)
        {
            foreach(var tcm in TimeCycleModifiers)
            {
                World.TimecycleMod wtcm;
                if (gfc.TimeCycleModsDict.TryGetValue(tcm.CTimeCycleModifier.name.Hash, out wtcm))
                {
                    tcm.TimeCycleModData = wtcm;
                }
            }
        }

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
    public Vector3 BBCenter; //oriented archetype AABB center
    public Vector3 BBExtent; //oriented archetype AABB extent
    public Vector3 BSCenter; //oriented archetype BS center
    public float BSRadius;//cached from archetype
    public float LodDist;
    public float ChildLodDist;

    public CEntityDef _CEntityDef;
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public ref CEntityDef CEntityDef => ref _CEntityDef;

    [TypeConverter(typeof(CollectionConverter))]
    private List<YmapEntityDef>? ChildList { get; set; }
    public YmapEntityDef[] Children { get; set; }
    public YmapEntityDef[] ChildrenMerged { get; set; }// { get; set; }

    public Vector3 _Position;
    public Vector3 Position { get => _Position; set => _Position = value; }
    public Quaternion _Orientation;
    public Quaternion Orientation { get => _Orientation; set => _Orientation = value; }
    public Vector3 _Scale;
    public Vector3 Scale { get => _Scale; set => _Scale = value; }
    public bool IsMlo { get; set; }
    public MloInstanceData MloInstance { get; set; }
    public YmapEntityDef MloParent { get; set; }
    public MloInstanceEntitySet MloEntitySet { get; set; }
    public Vector3 MloRefPosition { get; set; }
    public Quaternion MloRefOrientation { get; set; }
    public MetaWrapper[] Extensions { get; set; }

    public int Index { get; set; }

    [NotifyParentProperty(true)]
    public float Distance { get; set; } //used for rendering
    public bool IsWithinLodDist { get; set; } //used for rendering

    [NotifyParentProperty(true)]
    public bool ChildrenVisible { get; set; } //used for rendering

    [NotifyParentProperty(true)]
    public bool ChildrenRendered { get; set; } //used when rendering ymap mode to reduce LOD flashing...
    public YmapEntityDef Parent { get; set; } //for browsing convenience, also used/updated for rendering
    public MetaHash ParentName { get; set; } //just for browsing convenience

    public YmapFile Ymap { get; set; }

    public Vector3 PivotPosition = Vector3.Zero;
    public Quaternion PivotOrientation = Quaternion.Identity;
    public Vector3 WidgetPosition = Vector3.Zero;
    public Quaternion WidgetOrientation = Quaternion.Identity;

    public uint EntityHash { get; set; } = 0; //used by CW as a unique position+name identifier

    public LinkedList<YmapEntityDef> LodManagerChildren { get; set; } = null;
    public object LodManagerRenderable = null;


    public LightInstance[] Lights { get; set; }
    //public uint[] LightHashTest { get; set; }

    public bool LodInParentYmap => ((_CEntityDef.flags >> 3) & 1) > 0;


    public string Name => _CEntityDef.archetypeName.ToString();


    public YmapEntityDef()
    {
        Scale = Vector3.One;
        Position = Vector3.One;
        Orientation = Quaternion.Identity;
    }
    public YmapEntityDef(YmapFile ymap, int index, CEntityDef def)
    {
        Ymap = ymap;
        Index = index;
        _CEntityDef = def;
        Scale = new Vector3(new Vector2(_CEntityDef.scaleXY), _CEntityDef.scaleZ);
        Position = _CEntityDef.position;
        Orientation = new Quaternion(_CEntityDef.rotation);
        if (Orientation != Quaternion.Identity)
        {
            Orientation = Quaternion.Invert(Orientation);
        }
        IsMlo = false;

        UpdateWidgetPosition();
        UpdateWidgetOrientation();
        UpdateEntityHash();
    }

    public YmapEntityDef(YmapFile ymap, int index, CMloInstanceDef mlo)
    {
        Ymap = ymap;
        Index = index;
        _CEntityDef = mlo.CEntityDef;
        Scale = new Vector3(new Vector2(_CEntityDef.scaleXY), _CEntityDef.scaleZ);
        Position = _CEntityDef.position;
        Orientation = new Quaternion(_CEntityDef.rotation);
        //if (Orientation != Quaternion.Identity)
        //{
        //    Orientation = Quaternion.Invert(Orientation);
        //}
        IsMlo = true;

        MloInstance = new MloInstanceData(this, null);//is this necessary..? will get created in SetArchetype..
        MloInstance._Instance = mlo;

        UpdateWidgetPosition();
        UpdateWidgetOrientation();
        UpdateEntityHash();
    }


    public void SetArchetype(Archetype arch)
    {
        Archetype = arch;
        if (Archetype is null)
            return;


        UpdateBB();

        if (Archetype.Type == MetaName.CMloArchetypeDef)
        {
            //transform interior entities into world space...
            var mloa = Archetype as MloArchetype;
            MloInstance = new MloInstanceData(this, mloa);
            MloInstance._Instance = new CMloInstanceDef { CEntityDef = _CEntityDef };
            if (mloa != null)
            {
                if (!IsMlo)
                {
                    IsMlo = true;
                    List<YmapEntityDef> mloEntities = Ymap.MloEntities.ToList();
                    mloEntities.Add(this);
                    Ymap.MloEntities = mloEntities.ToArray();
                }

                MloInstance.CreateYmapEntities();
            }

            if (BSRadius == 0.0f)
            {
                BSRadius = LodDist;//need something so it doesn't get culled...
            }
            if (BBMin == BBMax)
            {
                BBMin = Position - BSRadius;
                BBMax = Position + BSRadius;//it's not ideal
                BBCenter = (BBMax + BBMin) * 0.5f;
                BBExtent = (BBMax - BBMin) * 0.5f;
            }
        }
        else if (IsMlo) // archetype is no longer an mlo
        {
            IsMlo = false;
            MloInstance = null;

            if (Ymap.MloEntities.Length > 0)
            {
                List<YmapEntityDef> mloEntities = Ymap.MloEntities.ToList();
                if (mloEntities.Remove(this))
                {
                    Ymap.MloEntities = mloEntities.ToArray();
                }
            }
        }
    }

    public void SetPosition(Vector3 pos)
    {
        Position = pos;
        if (MloParent != null)
        {
            _CEntityDef.position = Quaternion.Normalize(Quaternion.Invert(MloParent.Orientation)).Multiply(pos - MloParent.Position);
            MloRefPosition = _CEntityDef.position;
            UpdateBB();
            UpdateMloArchetype();
        }
        else
        {
            _CEntityDef.position = pos;
            UpdateBB();
        }


        if (MloInstance != null)
        {
            MloInstance.SetPosition(Position);
            MloInstance.UpdateEntities();
        }

        UpdateEntityHash();
        UpdateWidgetPosition();
    }

    private void UpdateBB()
    {
        if (Archetype != null)
        {
            BSCenter = _Orientation.Multiply(Archetype.BSCenter) * Scale;
            BSRadius = Archetype.BSRadius * Math.Max(Scale.X, Scale.Z);
            if (_Orientation == Quaternion.Identity)
            {
                BBMin = (Vector3.Min(Archetype.BBMin, Archetype.BBMax) * _Scale) + _Position;
                BBMax = (Vector3.Max(Archetype.BBMin, Archetype.BBMax) * _Scale) + _Position;
                BBCenter = (BBMax + BBMin) * 0.5f;
                BBExtent = (BBMax - BBMin) * 0.5f;
            }
            else
            {
                var mat = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, _Scale, Vector3.Zero, _Orientation, _Position);
                var matabs = mat;
                matabs.Column1 = mat.Column1.Abs();
                matabs.Column2 = mat.Column2.Abs();
                matabs.Column3 = mat.Column3.Abs();
                matabs.Column4 = mat.Column4.Abs();
                var bbcenter = (Archetype.BBMax + Archetype.BBMin) * 0.5f;
                var bbextent = (Archetype.BBMax - Archetype.BBMin) * 0.5f;
                Vector3.TransformCoordinate(ref bbcenter, ref mat, out var ncenter);
                var nextent = Vector3.TransformNormal(bbextent, matabs).Abs();
                BBCenter = ncenter;
                BBExtent = nextent;
                BBMin = ncenter - nextent;
                BBMax = ncenter + nextent;
            }
            LodDist = _CEntityDef.lodDist;
            if (LodDist <= 0)
            {
                LodDist = Archetype.LodDist;
            }
            ChildLodDist = _CEntityDef.childLodDist;
            if (ChildLodDist < 0)
            {
                ChildLodDist = LodDist * 0.5f;
            }
        }
    }

    public void UpdateEntityHash()
    {
        uint xhash = (uint)(Position.X * 100);
        uint yhash = (uint)(Position.Y * 100);
        uint zhash = (uint)(Position.Z * 100);
        EntityHash = _CEntityDef.archetypeName.Hash ^ xhash ^ yhash ^ zhash & 0xffffffff;
    }

    public void SetOrientation(Quaternion ori, bool inverse = false)
    {
        if (MloParent != null)
        {
            var mloInv = Quaternion.Normalize(Quaternion.Invert(MloParent.Orientation));
            Quaternion rel = Quaternion.Normalize(Quaternion.Multiply(mloInv, ori));
            Quaternion inv = Quaternion.Normalize(Quaternion.Invert(rel));
            Orientation = ori;
            _CEntityDef.rotation = inv.ToVector4();
        }
        else
        {
            Orientation = inverse ? Quaternion.Normalize(Quaternion.Invert(ori)) : ori;
            if (MloInstance != null)
            {
                _CEntityDef.rotation = Orientation.ToVector4();
            }
            else
            {
                Quaternion inv = inverse ? ori : Quaternion.Normalize(Quaternion.Invert(ori));
                _CEntityDef.rotation = inv.ToVector4();
            }
        }

        if (MloInstance != null)
        {
            MloInstance.SetOrientation(ori);
            MloInstance.UpdateEntities();
        }

        UpdateBB();
        UpdateWidgetPosition();
        UpdateWidgetOrientation();
    }

    public void SetScale(Vector3 s)
    {
        Scale = new Vector3(s.X, s.X, s.Z);
        _CEntityDef.scaleXY = s.X;
        _CEntityDef.scaleZ = s.Z;

        MloInstanceData mloInstance = MloParent?.MloInstance;
        if (mloInstance != null)
        {
            var mcEntity = mloInstance.TryGetArchetypeEntity(this);
            if (mcEntity != null)
            {
                mcEntity._Data.scaleXY = s.X;
                mcEntity._Data.scaleZ = s.Z;
            }
        }
        if (Archetype != null)
        {
            float smax = Math.Max(Scale.X, Scale.Z);
            BSRadius = Archetype.BSRadius * smax;
        }

        SetPosition(Position);//update the BB
    }

    private void UpdateMloArchetype()
    {
        if (!(MloParent.Archetype is MloArchetype mloArchetype))
            return;

        MCEntityDef entity;
        if ((MloEntitySet?.Entities != null) && (MloEntitySet?.EntitySet?.Entities != null))
        {
            var idx = MloEntitySet.Entities.IndexOf(this);
            if ((idx < 0) || (idx >= MloEntitySet.EntitySet.Entities.Length))
                return;
            entity = MloEntitySet.EntitySet.Entities[idx];
        }
        else
        {
            if (Index >= mloArchetype.entities.Length)
                return;
            entity = mloArchetype.entities[Index];
        }

        entity._Data.position = _CEntityDef.position;
        entity._Data.rotation = _CEntityDef.rotation;
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
        SetPosition(pos - Orientation.Multiply(in PivotPosition));
    }
    public void SetOrientationFromWidget(Quaternion ori)
    {
        var newori = Quaternion.Normalize(Quaternion.Multiply(ori, Quaternion.Invert(PivotOrientation)));
        var newpos = WidgetPosition - newori.Multiply(in PivotPosition);
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
        WidgetPosition = Position + Orientation.Multiply(in PivotPosition);
    }
    public void UpdateWidgetOrientation()
    {
        WidgetOrientation = Quaternion.Multiply(Orientation, PivotOrientation);
    }


    public void AddChild(YmapEntityDef c)
    {
        ChildList ??= new List<YmapEntityDef>();
        c.Parent = this;
        c.ParentName = _CEntityDef.archetypeName;

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
        if (ChildList is null)
            return;
        
        if (ChildList.Count == 0)
        {
            ChildList = null;
            return;
        }
        
        if (ChildrenMerged is null)
        {
            ChildrenMerged = ChildList.ToArray();
        }
        else
        {
            var newcArr = new YmapEntityDef[ChildrenMerged.Length + ChildList.Count];
            ChildrenMerged.CopyTo(newcArr, 0);
            ChildList.CopyTo(newcArr, ChildrenMerged.Length);
            ChildrenMerged = newcArr;
        }

        ChildList.Clear();
        ChildList = null;
    }


    public void LodManagerAddChild(YmapEntityDef child)
    {
        LodManagerChildren ??= new LinkedList<YmapEntityDef>();
        LodManagerChildren.AddLast(child);
    }
    public void LodManagerRemoveChild(YmapEntityDef child)
    {
        LodManagerChildren?.Remove(child);//could improve this by caching the list node....
    }


    public override string ToString()
    {
        return $"{_CEntityDef}{(ChildList is not null ? $" ({ChildList.Count} children)" : "")} {_CEntityDef.lodLevel}";
    }



    public void EnsureLights(DrawableBase db)
    {
        if (Lights is not null)
            return;
        if (Archetype is null)
            return;
        if (db == null)
            return;
        var skel = db.Skeleton;
        LightAttributes[] lightAttrs = null;
        Bounds b = null;
        if (db is Drawable dd)
        {
            lightAttrs = dd.LightAttributes?.data_items;
            b = dd.Bound;
        }
        else if (db is FragDrawable fd)
        {
            var frag = fd?.OwnerFragment;
            skel = skel ?? frag?.Drawable?.Skeleton;
            lightAttrs = frag?.LightAttributes?.data_items;
            b = frag?.PhysicsLODGroup?.PhysicsLOD1?.Bound;
        }
        if (lightAttrs == null) return;

        Vector3.Min(ref Archetype.BBMin, ref db.BoundingBoxMin, out var abmin);
        Vector3.Max(ref Archetype.BBMax, ref db.BoundingBoxMax, out var abmax);
        if (b != null)
        {
            Vector3.Min(ref abmin, ref b.BoxMin, out abmin);
            Vector3.Max(ref abmax, ref b.BoxMax, out abmax);
        }
        var bb = new BoundingBox(abmin, abmax).Transform(in _Position, in _Orientation, in _Scale);
        var ints = new uint[7];
        ints[0] = (uint)(bb.Minimum.X * 10.0f);
        ints[1] = (uint)(bb.Minimum.Y * 10.0f);
        ints[2] = (uint)(bb.Minimum.Z * 10.0f);
        ints[3] = (uint)(bb.Maximum.X * 10.0f);
        ints[4] = (uint)(bb.Maximum.Y * 10.0f);
        ints[5] = (uint)(bb.Maximum.Z * 10.0f);

        var bones = skel?.BonesMap;
        var exts = Archetype.Extensions.Length;// + (Extensions?.Length ?? 0);//seems entity extensions aren't included in this
        //todo: create extension light instances

        var lightInsts = new LightInstance[lightAttrs.Length];
        for (int i = 0; i < lightAttrs.Length; i++)
        {
            ints[6] = (uint)(exts + i);
            var la = lightAttrs[i];

            var xform = Matrix.Identity;
            if ((bones != null) && (bones.TryGetValue(la.BoneId, out Bone bone)))
            {
                xform = bone.AbsTransform;
            }

            var li = new LightInstance();
            li.Attributes = la;
            li.Hash = ComputeLightHash(ints);
            li.Position = Orientation.Multiply(xform.Multiply(la.Position)) + Position;
            li.Direction = Orientation.Multiply(xform.MultiplyRot(la.Direction));
            lightInsts[i] = li;
        }
        Lights = lightInsts;

        //LightHashTest = new uint[25];
        //for (int i = 0; i < 25; i++)
        //{
        //    ints[6] = (uint)(i);
        //    LightHashTest[i] = ComputeLightHash(ints);
        //}

    }


    public static uint ComputeLightHash(uint[] ints, uint seed = 0)
    {
        var a2 = ints.Length;
        var v3 = a2;
        var v5 = (uint)(seed + 0xDEADBEEF + 4 * ints.Length);
        var v6 = v5;
        var v7 = v5;

        var c = 0;
        for (var i = 0; i < (ints.Length - 4) / 3 + 1; i++, v3 -= 3, c += 3)
        {
            var v9 = ints[c + 2] + v5;
            var v10 = ints[c + 1] + v6;
            var v11 = ints[c] - v9;
            var v13 = v10 + v9;
            var v14 = (v7 + v11) ^ BitUtil.RotateLeft(v9, 4);
            var v15 = v10 - v14;
            var v17 = v13 + v14;
            var v18 = v15 ^ BitUtil.RotateLeft(v14, 6);
            var v19 = v13 - v18;
            var v21 = v17 + v18;
            var v22 = v19 ^ BitUtil.RotateLeft(v18, 8);
            var v23 = v17 - v22;
            var v25 = v21 + v22;
            var v26 = v23 ^ BitUtil.RotateLeft(v22, 16);
            var v27 = v21 - v26;
            var v29 = v27 ^ BitUtil.RotateRight(v26, 13);
            var v30 = v25 - v29;
            v7 = v25 + v26;
            v6 = v7 + v29;
            v5 = v30 ^ BitUtil.RotateLeft(v29, 4);
        }

        if (v3 == 3)
        {
            v5 += ints[c + 2];
        }

        if (v3 >= 2)
        {
            v6 += ints[c + 1];
        }

        if (v3 >= 1)
        {
            var v34 = (v6 ^ v5) - BitUtil.RotateLeft(v6, 14);
            var v35 = (v34 ^ (v7 + ints[c])) - BitUtil.RotateLeft(v34, 11);
            var v36 = (v35 ^ v6) - BitUtil.RotateRight(v35, 7);
            var v37 = (v36 ^ v34) - BitUtil.RotateLeft(v36, 16);
            var v38 = BitUtil.RotateLeft(v37, 4);
            var v39 = (((v35 ^ v37) - v38) ^ v36) - BitUtil.RotateLeft((v35 ^ v37) - v38, 14);
            return (v39 ^ v37) - BitUtil.RotateRight(v39, 8);
        }

        return v5;
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class LightInstance
    {
        public LightAttributes Attributes { get; set; } //just for display purposes!
        public uint Hash { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }

        public override string ToString()
        {
            return $"{Hash}: {Attributes.Type}";
        }
    }
}


[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapGrassInstanceBatch
{
    private const float BatchVertMultiplier = 0.00001525878f;

    public Archetype Archetype { get; set; } //cached by GameFileCache on loading...
    public rage__fwGrassInstanceListDef Batch;
    public rage__fwGrassInstanceListDef__InstanceData[] Instances { get; set; }
    public Vector3 Position { get; set; } //calculated from AABB
    public float Radius { get; set; } //calculated from AABB
    public Vector3 AABBMin;
    public Vector3 AABBMax;
    public Vector3 CamRel; //used for rendering...
    public float Distance; //used for rendering
    public YmapFile Ymap { get; set; }

    private BoundingBox[] grassBounds; // for brush
    public bool BrushEnabled; // for brush
    public float BrushRadius = 5f; // for brush
    public bool HasChanged; // for brush and renderer

    // TODO: Make configurable.
    const float BoundingSize = 0.3F;
    static readonly Vector3 GrassMinMax = Vector3.One * BoundingSize;

    public override string ToString()
    {
        return Batch.ToString();
    }

    public void UpdateInstanceCount()
    {
        var b = Batch;
        b.InstanceList = b.InstanceList with { Count1 = (ushort)Instances.Length };
        Batch = b;
    }

    public bool IsPointBlockedByInstance(Vector3 point)
    {
        for (int i = 0; i < grassBounds.Length; i++)
        {
            if (grassBounds[i].Contains(ref point) == ContainmentType.Contains)
            {
                return true;
            }
        }

        return false;
    }

    private void ReInitializeBoundingCache()
    {
        // cache is already initialized correctly.
        if (grassBounds is not null && grassBounds.Length == Instances.Length)
            return;

        // Clear the current bounding cache.
        grassBounds = new BoundingBox[Instances.Length];

        for (int i = 0; i < Instances.Length; i++)
        {
            var inst = Instances[i];
            // create bounding box for this instance.
            var worldPos = GetGrassWorldPos(in inst.Position, new BoundingBox(AABBMin, AABBMax));
            var bb = new BoundingBox(worldPos - GrassMinMax, worldPos + GrassMinMax);
            grassBounds[i] = bb;
        }
    }

    public bool EraseInstancesAtMouse(
        YmapGrassInstanceBatch batch,
        in SpaceRayIntersectResult mouseRay,
        float radius)
    {
        rage__spdAABB batchAABB = batch.Batch.BatchAABB;
        var oldInstanceBounds = new BoundingBox
        (
            batchAABB.min.XYZ(),
            batchAABB.max.XYZ()
        );
        var deleteSphere = new BoundingSphere(mouseRay.Position, radius);

        // check each instance to see if it's in the delete sphere
        // thankfully we've just avoided an O(n^2) op using this bounds stuff (doesn't mean it's super fast though,
        // but it's not super slow either, even at like 50,000 instances)
        var insList = new List<rage__fwGrassInstanceListDef__InstanceData>();
        foreach (var instance in batch.Instances)
        {
            // get the world pos
            var worldPos = GetGrassWorldPos(in instance.Position, in oldInstanceBounds);

            // create a boundary around the instance.
            var instanceBounds = new BoundingBox(worldPos - GrassMinMax, worldPos + GrassMinMax);

            // check if the sphere contains the boundary.
            var bb = new BoundingBox(instanceBounds.Minimum, instanceBounds.Maximum);
            var ct = deleteSphere.Contains(ref bb);
            if (ct == ContainmentType.Contains || ct == ContainmentType.Intersects)
            {
                //delInstances.Add(instance); // Add a copy of this instance
                continue;
            }
            insList.Add(instance);
        }
        if (insList.Count == Instances.Length)
            return false;

        var newBounds = GetNewGrassBounds(insList, oldInstanceBounds);
        // recalc instances
        RecalcBatch(newBounds, batch);
        insList = RecalculateInstances(insList, oldInstanceBounds, newBounds);
        batch.Instances = insList.ToArray();
        return true;
    }

    public void CreateInstancesAtMouse(
        YmapGrassInstanceBatch batch,
        SpaceRayIntersectResult mouseRay,
        float radius,
        int amount,
        Func<Vector3, SpaceRayIntersectResult> spawnRayFunc,
        Color color,
        int ao,
        int scale,
        Vector3 pad,
        bool randomScale)
    {

        ReInitializeBoundingCache();
        var spawnPosition = mouseRay.Position;
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();

        // Get rand positions.
        GetSpawns(spawnPosition, spawnRayFunc, positions, normals, radius, amount);
        if (positions.Count <= 0)
            return;

        // get the instance list
        var instances =
            batch.Instances?.ToList() ?? new List<rage__fwGrassInstanceListDef__InstanceData>();
        var batchAABB = batch.Batch.BatchAABB;

        // make sure to store the old instance bounds for the original
        // grass instances
        var oldInstanceBounds = new BoundingBox(batchAABB.min.XYZ(), batchAABB.max.XYZ());

        if (positions.Count <= 0)
            return;

        // Begin the spawn bounds.
        var grassBound = new BoundingBox(positions[0] - GrassMinMax, positions[0] + GrassMinMax);
        grassBound = EncapsulatePositions(positions, grassBound);

        // Calculate the new spawn bounds.
        var newInstanceBounds = new BoundingBox(oldInstanceBounds.Minimum, oldInstanceBounds.Maximum);
        newInstanceBounds = instances.Count > 0
            ? newInstanceBounds.Encapsulate(ref grassBound)
            : new BoundingBox(grassBound.Minimum, grassBound.Maximum);

        // now we need to recalculate the position of each instance
        instances = RecalculateInstances(instances, oldInstanceBounds, newInstanceBounds);

        // Add new instances at each spawn position with
        // the parameters in the brush.
        SpawnInstances(positions, normals, instances, newInstanceBounds, color, ao, scale, pad, randomScale);

        // then recalc the bounds of the grass batch
        RecalcBatch(newInstanceBounds, batch);

        // Give back the new intsances
        batch.Instances = instances.ToArray();
        grassBounds = Array.Empty<BoundingBox>();
    }

    // bhv approach recommended by dexy.
    public YmapGrassInstanceBatch[] OptimizeInstances(YmapGrassInstanceBatch batch, float minRadius)
    {
        // this function will return an array of grass instance batches
        // that are split up into sectors (groups) with a specific size.
        // say for instance we have 30,000 instances spread across a large
        // distance. We will split those instances into a grid-like group
        // and return the groups as an array of batches.
        var oldInstanceBounds = new BoundingBox(batch.Batch.BatchAABB.min.XYZ(), batch.Batch.BatchAABB.max.XYZ());

        if (oldInstanceBounds.Radius() < minRadius)
        {
            return new [] { batch };
        }

        // Get our optimized grassInstances
        var split = SplitGrassRecursive(batch.Instances.ToList(), oldInstanceBounds, minRadius);

        // Initiate a new batch list.
        var newBatches = new List<YmapGrassInstanceBatch>();

        foreach (var grassList in split)
        {
            // Create a new batch
            var newBatch = new YmapGrassInstanceBatch
            {
                Archetype = batch.Archetype,
                Ymap = batch.Ymap
            };

            // Get the boundary of the grassInstances
            var newInstanceBounds = GetNewGrassBounds(grassList, oldInstanceBounds);

            // Recalculate the batch boundaries.
            RecalcBatch(newInstanceBounds, newBatch);

            var ins = RecalculateInstances(grassList, oldInstanceBounds, newInstanceBounds);
            newBatch.Instances = ins.ToArray();
            newBatches.Add(newBatch);
        }

        return newBatches.ToArray();
    }

    private List<List<rage__fwGrassInstanceListDef__InstanceData>> SplitGrassRecursive(
        IReadOnlyList<rage__fwGrassInstanceListDef__InstanceData> grassInstances,
        BoundingBox batchAABB,
        float minRadius = 15F
    )
    {
        var ret = new List<List<rage__fwGrassInstanceListDef__InstanceData>>();
        var oldPoints = SplitGrass(grassInstances, batchAABB);
        while (true)
        {
            var stop = true;
            var newPoints = new List<List<rage__fwGrassInstanceListDef__InstanceData>>();
            foreach (var mb in oldPoints)
            {
                // for some reason we got a null group?
                if (mb == null)
                    continue;

                // Get the bounds of the grassInstances list
                var radius = GetNewGrassBounds(mb, batchAABB).Radius();

                // check if the radius of the grassInstances
                if (radius <= minRadius)
                {
                    // this point list is within the minimum 
                    // radius.
                    ret.Add(mb);
                    continue; // we don't need to continue.
                }

                // since we're here let's keep going
                stop = false;

                // split the grassInstances again
                var s = SplitGrass(mb, batchAABB);

                // add it into the new grassInstances list.
                newPoints.AddRange(s);
            }

            // set the old grassInstances to the new grassInstances.
            oldPoints = newPoints.ToArray();

            // if we're done, and all grassInstances are within the desired size
            // then end the loop.
            if (stop) break;
        }
        return ret;
    }

    private List<rage__fwGrassInstanceListDef__InstanceData>[] SplitGrass(
        IReadOnlyList<rage__fwGrassInstanceListDef__InstanceData> points,
        BoundingBox batchAABB)
    {
        var pointGroup = new List<rage__fwGrassInstanceListDef__InstanceData>[2];

        // Calculate the bounds of these grassInstances.
        var m = GetNewGrassBounds(points, batchAABB);

        // Get the center and size
        var mm = new Vector3
        {
            X = Math.Abs(m.Minimum.X - m.Maximum.X),
            Y = Math.Abs(m.Minimum.Y - m.Maximum.Y),
            Z = Math.Abs(m.Minimum.Z - m.Maximum.Z)
        };

        // x is the greatest axis...
        if (mm.X > mm.Y && mm.X > mm.Z)
        {
            // Calculate both boundaries.
            var lhs = new BoundingBox(m.Minimum, m.Maximum - new Vector3(mm.X * 0.5F, 0, 0));
            var rhs = new BoundingBox(m.Minimum + new Vector3(mm.X * 0.5F, 0, 0), m.Maximum);

            // Set the grassInstances accordingly.
            pointGroup[0] = points
                .Where(p => lhs.Contains(GetGrassWorldPos(in p.Position, in batchAABB)) == ContainmentType.Contains).ToList();
            pointGroup[1] = points
                .Where(p => rhs.Contains(GetGrassWorldPos(in p.Position, in batchAABB)) == ContainmentType.Contains).ToList();
        }
        // y is the greatest axis...
        else if (mm.Y > mm.X && mm.Y > mm.Z)
        {
            // Calculate both boundaries.
            var lhs = new BoundingBox(m.Minimum, m.Maximum - new Vector3(0, mm.Y * 0.5F, 0));
            var rhs = new BoundingBox(m.Minimum + new Vector3(0, mm.Y * 0.5F, 0), m.Maximum);

            // Set the grassInstances accordingly.
            pointGroup[0] = points
                .Where(p => lhs.Contains(GetGrassWorldPos(in p.Position, in batchAABB)) == ContainmentType.Contains).ToList();
            pointGroup[1] = points
                .Where(p => rhs.Contains(GetGrassWorldPos(in p.Position, in batchAABB)) == ContainmentType.Contains).ToList();
        }
        // z is the greatest axis...
        else if (mm.Z > mm.X && mm.Z > mm.Y)
        {
            // Calculate both boundaries.
            var lhs = new BoundingBox(m.Minimum, m.Maximum - new Vector3(0, 0, mm.Z * 0.5F));
            var rhs = new BoundingBox(m.Minimum + new Vector3(0, 0, mm.Z * 0.5F), m.Maximum);

            // Set the grassInstances accordingly.
            pointGroup[0] = points
                .Where(p => lhs.Contains(GetGrassWorldPos(in p.Position, in batchAABB)) == ContainmentType.Contains).ToList();
            pointGroup[1] = points
                .Where(p => rhs.Contains(GetGrassWorldPos(in p.Position, in batchAABB)) == ContainmentType.Contains).ToList();
        }
        return pointGroup;
    }

    private static BoundingBox GetNewGrassBounds(IReadOnlyList<rage__fwGrassInstanceListDef__InstanceData> newGrass, BoundingBox oldAABB)
    {
        var grassPositions = newGrass.Select(x => GetGrassWorldPos(in x.Position, in oldAABB)).ToArray();
        var bounds = BoundingBox.FromPoints(grassPositions);
        return bounds.Expand(1f);
    }

    private void SpawnInstances(
        IReadOnlyList<Vector3> positions,
        IReadOnlyList<Vector3> normals,
        ICollection<rage__fwGrassInstanceListDef__InstanceData> instanceList,
        BoundingBox instanceBounds,
        Color color,
        int ao,
        int scale,
        Vector3 pad,
        bool randomScale)
    {
        for (var i = 0; i < positions.Count; i++)
        {
            var pos = positions[i];
            // create the new instance.
            var newInstance = CreateNewInstance(normals[i], color, ao, scale, pad, randomScale);

            // get the grass position of the new instance and add it to the 
            // instance list
            var grassPosition = GetGrassPos(in pos, in instanceBounds);
            newInstance.Position = grassPosition;
            instanceList.Add(newInstance);
        }
    }

    private rage__fwGrassInstanceListDef__InstanceData CreateNewInstance(in Vector3 normal, in Color color, int ao, int scale, in Vector3 pad,
        bool randomScale = false)
    {
        //Vector3 pad = FloatUtil.ParseVector3String(PadTextBox.Text);
        //int scale = (int)ScaleNumericUpDown.Value;
        var rand = new Random();
        if (randomScale)
            scale = rand.Next(scale / 2, scale);
        var newInstance = new rage__fwGrassInstanceListDef__InstanceData
        {
            Ao = (byte)ao,
            Scale = (byte)scale,
            Color = new ArrayOfBytes3(color.R, color.G, color.B),
            Pad = new ArrayOfBytes3((byte)pad.X,(byte)pad.Y, (byte)pad.Z),
            NormalX = (byte)((normal.X + 1) * 0.5F * 255F),
            NormalY = (byte)((normal.Y + 1) * 0.5F * 255F)
        };
        return newInstance;
    }

    private void RecalcBatch(in BoundingBox newInstanceBounds, YmapGrassInstanceBatch batch)
    {
        batch.AABBMax = newInstanceBounds.Maximum;
        batch.AABBMin = newInstanceBounds.Minimum;
        batch.Position = newInstanceBounds.Center();
        batch.Radius = newInstanceBounds.Radius();
        batch.Batch.BatchAABB = new rage__spdAABB(in newInstanceBounds.Minimum, in newInstanceBounds.Maximum);
    }

    private void GetSpawns(
        Vector3 origin,
        Func<Vector3, SpaceRayIntersectResult> spawnRayFunc,
        List<Vector3> positions,
        List<Vector3> normals,
        float radius,
        int resolution = 28)
    {
        var rand = new Random();
        for (var i = 0; i < resolution; i++)
        {
            var randX = (float)rand.NextDouble(-radius, radius);
            var randY = (float)rand.NextDouble(-radius, radius);
            if (Math.Abs(randX) > 0 && Math.Abs(randY) > 0)
            {
                randX *= .7071f;
                randY *= .7071f;
            }
            var posOffset = origin + new Vector3(randX, randY, 2f);
            var spaceRay = spawnRayFunc.Invoke(posOffset);
            if (!spaceRay.Hit)
                continue;
            // not truly O(n^2) but may be slow...
            // actually just did some testing, not slow at all.
            if (IsPointBlockedByInstance(spaceRay.Position))
                continue;
            normals.Add(spaceRay.Normal);
            positions.Add(spaceRay.Position);
        }
    }

    private static List<rage__fwGrassInstanceListDef__InstanceData> RecalculateInstances(
        List<rage__fwGrassInstanceListDef__InstanceData> instances,
        BoundingBox oldInstanceBounds,
        BoundingBox newInstanceBounds)
    {
        var refreshList = new List<rage__fwGrassInstanceListDef__InstanceData>();
        foreach (var inst in instances)
        {
            // Copy instance
            var copy =
                new rage__fwGrassInstanceListDef__InstanceData
                {
                    Position = inst.Position,
                    Ao = inst.Ao,
                    Color = inst.Color,
                    NormalX = inst.NormalX,
                    NormalY = inst.NormalY,
                    Pad = inst.Pad,
                    Scale = inst.Scale
                };
            // get the position from where we would be in the old bounds, and move it to
            // the position it needs to be in the new bounds.
            var oldPos = GetGrassWorldPos(in copy.Position, in oldInstanceBounds);
            //var oldPos = oldInstanceBounds.min + oldInstanceBounds.Size * (grassPos * BatchVertMultiplier);
            copy.Position = GetGrassPos(in oldPos, in newInstanceBounds);
            refreshList.Add(copy);
        }
        instances = refreshList.ToList();
        return instances;
    }

    private static BoundingBox EncapsulatePositions(IEnumerable<Vector3> positions, BoundingBox bounds)
    {
        foreach (var pos in positions)
        {
            var posBounds = new BoundingBox(pos - (GrassMinMax + 0.1f), pos + (GrassMinMax + 0.1f));
            bounds = bounds.Encapsulate(ref posBounds);
        }
        return bounds;
    }

    private static ArrayOfUshorts3 GetGrassPos(in Vector3 worldPos, in BoundingBox batchAABB)
    {
        var offset = worldPos - batchAABB.Minimum;
        var size = batchAABB.Size();
        var percentage =
            new Vector3(
                offset.X / size.X,
                offset.Y / size.Y,
                offset.Z / size.Z
            );
        var instancePos = percentage / BatchVertMultiplier;
        return new ArrayOfUshorts3((ushort)instancePos.X, (ushort)instancePos.Y, (ushort)instancePos.Z);
    }

    private static Vector3 GetGrassWorldPos(in ArrayOfUshorts3 grassPos, in BoundingBox batchAABB)
    {
        return batchAABB.Minimum + batchAABB.Size() * (grassPos.XYZ() * BatchVertMultiplier);
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
    public CDistantLODLight CDistantLODLight;
    public uint[] colours { get; set; } = Array.Empty<uint>();
    public MetaVECTOR3[] positions { get; set; } = Array.Empty<MetaVECTOR3>();

    public Vector3 BBMin;
    public Vector3 BBMax;
    public YmapFile Ymap { get; set; }

    public void CalcBB()
    {
        if (positions.Length > 0)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach(ref var position in positions.AsSpan())
            {
                Vectors.Min(in min, in position, out min);
                Vectors.Max(in max, in position, out max);
            }
            BBMin = min;
            BBMax = max;
        }

    }


    public void RebuildFromLodLights(YmapLODLight[] lodlights)
    {
        var n = lodlights.Length;
        if (n == 0)
            return;

        colours = new uint[n];
        positions = new MetaVECTOR3[n];
        var nstreetlights = 0;
        for (int i = 0; i < n; i++)
        {
            var ll = lodlights[i];
            colours[i] = (uint)(ll.Colour.ToBgra());
            positions[i] = new MetaVECTOR3(ll.Position);
            if ((ll.StateFlags1 & 1) > 0)
            {
                nstreetlights++;
            }
        }

        var cdll = CDistantLODLight;
        cdll.numStreetLights = (ushort)nstreetlights;

        CalcBB();
    }


    public override string ToString()
    {
        if (Ymap != null)
        {
            return Ymap.ToString();
        }
        return base.ToString();
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapLODLights
{
    public CLODLight CLODLight { get; set; }
    public MetaVECTOR3[] direction { get; set; }
    public float[] falloff { get; set; }
    public float[] falloffExponent { get; set; }
    public uint[] timeAndStateFlags { get; set; }
    public uint[] hash { get; set; }
    public byte[] coneInnerAngle { get; set; }
    public byte[] coneOuterAngleOrCapExt { get; set; }
    public byte[] coronaIntensity { get; set; }

    public Vector3 BBMin { get; set; }
    public Vector3 BBMax { get; set; }
    public YmapFile Ymap { get; set; }

    public YmapLODLight[] LodLights { get; set; }

    public PathBVH BVH { get; set; }

    public void Init(YmapDistantLODLights parent)
    {
        if (parent == null)
            return;

        BuildLodLights(parent);
        CalcBB();
        BuildBVH();
    }

    public void BuildLodLights(YmapDistantLODLights parent)
    {
        var n = direction?.Length ?? 0;
        n = Math.Min(n, parent.positions.Length);
        n = Math.Min(n, parent.colours.Length);
        n = Math.Min(n, falloff?.Length ?? 0);
        n = Math.Min(n, falloffExponent?.Length ?? 0);
        n = Math.Min(n, timeAndStateFlags?.Length ?? 0);
        n = Math.Min(n, hash?.Length ?? 0);
        n = Math.Min(n, coneInnerAngle?.Length ?? 0);
        n = Math.Min(n, coneOuterAngleOrCapExt?.Length ?? 0);
        n = Math.Min(n, coronaIntensity?.Length ?? 0);
        if (n == 0)
            return;

        LodLights = new YmapLODLight[n];
        for (int i = 0; i < n; i++)
        {
            var l = new YmapLODLight();
            l.Init(this, parent, i);
            LodLights[i] = l;
        }
    }

    public void BuildBVH()
    {
        BVH = new PathBVH(LodLights, 10, 10);
    }


    public void CalcBB()
    {
        var positions = Ymap?.Parent?.DistantLODLights?.positions;
        if (positions != null)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach(ref var position in positions.AsSpan())
            {
                Vectors.Min(in min, in position, out min);
                Vectors.Max(in min, in position, out max);
            }
            BBMin = min;
            BBMax = max;
        }
        else if (Ymap != null)
        {
            BBMin = Ymap._CMapData.entitiesExtentsMin;
            BBMax = Ymap._CMapData.entitiesExtentsMax;
        }
    }

    public void RebuildFromLodLights()
    {
        var n = LodLights.Length;
        if (n <= 0)
        {
            direction = null;
            falloff = null;
            falloffExponent = null;
            timeAndStateFlags = null;
            hash = null;
            coneInnerAngle = null;
            coneOuterAngleOrCapExt = null;
            coronaIntensity = null;
        }
        else
        {
            direction = new MetaVECTOR3[n];
            falloff = new float[n];
            falloffExponent = new float[n];
            timeAndStateFlags = new uint[n];
            hash = new uint[n];
            coneInnerAngle = new byte[n];
            coneOuterAngleOrCapExt = new byte[n];
            coronaIntensity = new byte[n];

            for (int i = 0; i < n; i++)
            {
                var ll = LodLights[i];
                direction[i] = new MetaVECTOR3(ll.Direction);
                falloff[i] = ll.Falloff;
                falloffExponent[i] = ll.FalloffExponent;
                timeAndStateFlags[i] = ll.TimeAndStateFlags;
                hash[i] = ll.Hash;
                coneInnerAngle[i] = ll.ConeInnerAngle;
                coneOuterAngleOrCapExt[i] = ll.ConeOuterAngleOrCapExt;
                coronaIntensity[i] = ll.CoronaIntensity;
            }
        }

    }


    public override string ToString()
    {
        if (Ymap != null)
        {
            return Ymap.ToString();
        }
        return base.ToString();
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapLODLight : BasePathNode
{
    public YmapFile? Ymap => LodLights?.Ymap ?? DistLodLights?.Ymap;
    public YmapLODLights LodLights { get; set; }
    public YmapDistantLODLights DistLodLights { get; set; }
    public int Index { get; set; }
    public Color Colour;
    public Vector3 _Position;
    public ref Vector3 Position => ref _Position;
    public Vector3 Direction;
    public float Falloff { get; set; }
    public float FalloffExponent { get; set; }
    public uint TimeAndStateFlags { get; set; }
    public uint Hash { get; set; }
    public byte ConeInnerAngle { get; set; }
    public byte ConeOuterAngleOrCapExt { get; set; }
    public byte CoronaIntensity { get; set; }

    public Quaternion Orientation;
    public Vector3 Scale;

    public Vector3 TangentX;
    public Vector3 TangentY;

    public LightType Type
    {
        get => (LightType)((TimeAndStateFlags >> 26) & 7);
        set => TimeAndStateFlags = (TimeAndStateFlags & 0xE3FFFFFF) + (((uint)value & 7) << 26);
    }
    public FlagsUint TimeFlags
    {
        get => TimeAndStateFlags & 0xFFFFFF;
        set => TimeAndStateFlags = (TimeAndStateFlags & 0xFF000000) + (value & 0xFFFFFF);
    }
    public uint StateFlags1
    {
        get => (TimeAndStateFlags >> 24) & 3;
        set => TimeAndStateFlags = (TimeAndStateFlags & 0xFCFFFFFF) + ((value & 3) << 24);
    }
    public uint StateFlags2
    {
        get => (TimeAndStateFlags >> 29) & 7;
        set => TimeAndStateFlags = (TimeAndStateFlags & 0x1FFFFFFF) + ((value & 7) << 29);
    }

    public bool Enabled { get; set; } = true;

    public bool Visible { get; set; } = true;

    public void Init(YmapLODLights l, YmapDistantLODLights p, int i)
    {
        LodLights = l;
        DistLodLights = p;
        Index = i;

        if (p.colours == null) return;
        if ((i < 0) || (i >= p.colours.Length)) return;

        Colour = Color.FromBgra(p.colours[i]);
        Position = p.positions[i].ToVector3();
        Direction = l.direction[i].ToVector3();
        Falloff = l.falloff[i];
        FalloffExponent = l.falloffExponent[i];
        TimeAndStateFlags = l.timeAndStateFlags[i];
        Hash = l.hash[i];
        ConeInnerAngle = l.coneInnerAngle[i];
        ConeOuterAngleOrCapExt = l.coneOuterAngleOrCapExt[i];
        CoronaIntensity = l.coronaIntensity[i];

        UpdateTangentsAndOrientation();

        Scale = new Vector3(Falloff);
    }

    public void UpdateTangentsAndOrientation()
    {
        switch (Type)
        {
            default:
            case LightType.Point:
                TangentX = Vector3.UnitX;
                TangentY = Vector3.UnitY;
                Orientation = Quaternion.Identity;
                break;
            case LightType.Spot:
                TangentX = Vector3.Normalize(Direction.GetPerpVec());
                TangentY = Vector3.Normalize(Vector3.Cross(Direction, TangentX));
                break;
            case LightType.Capsule:
                TangentX = -Vector3.Normalize(Direction.GetPerpVec());
                TangentY = Vector3.Normalize(Vector3.Cross(Direction, TangentX));
                break;
        }

        if (Type == LightType.Point)
        {
            Orientation = Quaternion.Identity;
        }
        else
        {
            var m = new Matrix();
            m.Row1 = new Vector4(TangentX, 0);
            m.Row2 = new Vector4(TangentY, 0);
            m.Row3 = new Vector4(Direction, 0);
            Orientation = Quaternion.RotationMatrix(m);
        }
    }

    public void SetColour(in Color c)
    {
        Colour = c;

        if (DistLodLights is not null && DistLodLights.colours.Length >= Index)
        {
            DistLodLights.colours[Index] = (uint)(c.ToBgra());
        }

    }
    public void SetPosition(in Vector3 pos)
    {
        Position = pos;

        if (DistLodLights is not null && DistLodLights.positions.Length >= Index)
        {
            DistLodLights.positions[Index] = new MetaVECTOR3(pos);
        }

    }
    public void SetOrientation(in Quaternion ori)
    {
        Quaternion.Invert(ref Orientation, out var inv);
        var delta = ori * inv;
        TangentX = Vector3.Normalize(delta.Multiply(in TangentX));
        TangentY = Vector3.Normalize(delta.Multiply(in TangentY));
        Direction = Vector3.Normalize(delta.Multiply(in Direction));
        Orientation = ori;
    }
    public void SetScale(in Vector3 scale)
    {
        switch (Type)
        {
            case LightType.Point:
            case LightType.Spot:
                Falloff = scale.Z;
                break;
            case LightType.Capsule:
                Falloff = scale.X;
                ConeOuterAngleOrCapExt = (byte)Math.Min(255, Math.Max(0, Math.Round(Math.Max((uint)ConeOuterAngleOrCapExt, 2) * Math.Abs(scale.Z / Math.Max(Scale.Z, 0.01f)))));//lols
                break;
        }
        Scale = scale;
    }



    public void CopyFrom(YmapLODLight l)
    {
        Colour = l.Colour;
        Position = l.Position;
        Direction = l.Direction;
        Falloff = l.Falloff;
        FalloffExponent = l.FalloffExponent;
        TimeAndStateFlags = l.TimeAndStateFlags;
        Hash = l.Hash;
        ConeInnerAngle = l.ConeInnerAngle;
        ConeOuterAngleOrCapExt = l.ConeOuterAngleOrCapExt;
        CoronaIntensity = l.CoronaIntensity;

        Orientation = l.Orientation;
        Scale = l.Scale;
        TangentX = l.TangentX;
        TangentY = l.TangentY;
    }


    public override string ToString()
    {
        return $"{Index}: {Position}";
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapTimeCycleModifier
{
    public CTimeCycleModifier CTimeCycleModifier;
    public World.TimecycleMod TimeCycleModData { get; set; }

    public Vector3 BBMin;
    public Vector3 BBMax;

    public YmapFile Ymap { get; set; }
}


[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapCarGen
{
    public CCarGen _CCarGen;
    public CCarGen CCarGen { get => _CCarGen; set => _CCarGen = value; }

    public Vector3 Position;
    public Quaternion Orientation;
    public Vector3 BBMin;
    public Vector3 BBMax;

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

    public void SetPosition(in Vector3 pos)
    {
        Position = pos;
        _CCarGen.position = pos;
    }
    public void SetOrientation(in Quaternion ori)
    {
        Orientation = ori;

        float len = Math.Max(_CCarGen.perpendicularLength * 1.5f, 5.0f);
        Vector3 v = new Vector3(len, 0, 0);
        Vector3 t = ori.Multiply(in v);

        _CCarGen.orientX = t.X;
        _CCarGen.orientY = t.Y;
    }
    public void SetScale(in Vector3 scale)
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
        return $"{_CCarGen.carModel}, {Position}, {_CCarGen.popGroup}, {_CCarGen.livery}";
    }
}


[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapOccludeModel : BasePathData
{
    public OccludeModel _OccludeModel;

    public OccludeModel OccludeModel { get => _OccludeModel; set => _OccludeModel = value; }

    public YmapFile Ymap { get; set; }

    public byte[]? Data { get; set; }
    private int vertexCount;
    private int indicesOffset => vertexCount * 12;
    [NotNullIfNotNull(nameof(Data))]
    public Span<Vector3> Vertices => Data is not null ? MetaTypes.ConvertDataArray<Vector3>(Data, 0, vertexCount) : Span<Vector3>.Empty;
    [NotNullIfNotNull(nameof(Data))]
    public Span<byte> Indices => Data is not null ? Data.AsSpan(indicesOffset) : Span<byte>.Empty;
    public int Index { get; set; }

    public YmapOccludeModelTriangle[]? Triangles { get; set; }
    public TriangleBVH? BVH { get; set; }

    public FlagsUint Flags
    {
        get => _OccludeModel.flags;
        set => _OccludeModel.flags = value;
    }

    public YmapOccludeModel(YmapFile ymap, OccludeModel model)
    {
        Ymap = ymap;
        _OccludeModel = model;
    }


    public void Load(Meta meta)
    {
        var vptr = _OccludeModel.verts;
        var dataSize = _OccludeModel.dataSize;
        var indicesOffset = _OccludeModel.numVertsInBytes;
        vertexCount = indicesOffset / 12;
        Data = MetaTypes.GetByteArray(meta, in vptr, dataSize);
        BuildTriangles();
    }


    public void BuildTriangles()
    {
        if (Data is null)
        {
            Triangles = null;
            return;
        }
        using var tris = new PooledList<YmapOccludeModelTriangle>();
        for (int i = 0; i < Indices.Length; i += 3)
        {
            var tri = new YmapOccludeModelTriangle(this, Vertices[Indices[i]], Vertices[Indices[i+1]], Vertices[Indices[i+2]], tris.Count);
            tris.Add(tri);
        }
        Triangles = tris.ToArray();
    }
    public void BuildBVH()
    {
        if (Triangles is null)
        {
            BVH = null;
            return;
        }

        BVH = new TriangleBVH(Triangles);

    }
    public void BuildVertices()
    {
        //create vertices and indices arrays from Triangles
        if (Triangles is null)
        {
            return;
        }

        

        var vdict = new Dictionary<Vector3, byte>();
        var verts = new List<Vector3>();
        var inds = new List<byte>();
        byte ensureVert(in Vector3 v)
        {
            ref var b = ref CollectionsMarshal.GetValueRefOrAddDefault(vdict, v, out var exists);
            if (exists)
            {
                return b;
            }
            if (verts.Count > 255)
            {
                return 0;
            }
            var i = (byte)verts.Count;
            b = i;
            verts.Add(v);
            return i;
        }
        foreach(var tri in Triangles.AsSpan())
        {
            inds.Add(ensureVert(tri.Corner1));
            inds.Add(ensureVert(tri.Corner2));
            inds.Add(ensureVert(tri.Corner3));
        }

        var newVertsSize = Vector3.SizeInBytes * verts.Count;
        var newIndicesSize = Marshal.SizeOf<byte>() * inds.Count;

        if (newVertsSize + newIndicesSize != Data.Length)
        {
            Data = new byte[newVertsSize + newIndicesSize];
        }
        vertexCount = verts.Count;
        var _verts = verts.AsSpan();
        for (int i = 0; i < _verts.Length; i++)
        {
            Vertices[i] = _verts[i];
        }
        var _inds = inds.AsSpan();
        for (int i = 0; i < _inds.Length; i++)
        {
            Indices[i] = _inds[i];
        }
    }
    public void BuildData()
    {
        //create Data from vertices and indices arrays
        if (Data is null)
            return;
        var dlen = (Vertices.Length * Vector3.SizeInBytes) + (Indices.Length * 1);
        if (dlen != Data.Length)
        {
            throw new InvalidOperationException("Size mismatch in YmapOccludeModel BuildData");
        }
        //var d = new byte[dlen];
        //var vbytes = MetaTypes.ConvertArrayToBytes(Vertices);
        //var ibytes = Indices;
        //Buffer.BlockCopy(vbytes, 0, d, 0, vbytes.Length);
        //Buffer.BlockCopy(ibytes, 0, d, vbytes.Length, ibytes.Length);
        //Data = d;
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vector3.Min(ref min, ref Vertices[i], out min);
            Vector3.Max(ref max, ref Vertices[i], out max);
        }
        _OccludeModel.bmin = min;
        _OccludeModel.bmax = max;
        _OccludeModel.Unused0 = min.X;
        _OccludeModel.Unused1 = max.X;
        _OccludeModel.dataSize = (uint)dlen;
        _OccludeModel.numVertsInBytes = (ushort)(vertexCount * Vector3.SizeInBytes);
        _OccludeModel.numTris = (ushort)((Indices.Length / 3) + 32768);//is this actually a flag lurking..?
        //_OccludeModel.flags = ...
    }

    public YmapOccludeModelTriangle? RayIntersect(ref Ray ray, ref float hitdist)
    {
        if (Triangles is null)
        {
            BuildTriangles();
            BuildBVH();
        }
        else if (BVH is null)
        {
            BuildBVH();
        }

        if (BVH is null)
            return null;
        return BVH.RayIntersect(ref ray, ref hitdist) as YmapOccludeModelTriangle;
    }

    public EditorVertex[] GetTriangleVertices()
    {
        if (Data is null)
            return Array.Empty<EditorVertex>();
        EditorVertex[] res = new EditorVertex[Indices.Length];//changing from indexed to nonindexed triangle list
        var colour = new Color4(1.0f, 0.0f, 0.0f, 0.8f); //todo: colours for occluders?
        var colourval = (uint)colour.ToRgba();
        for (int i = 0; i < Indices.Length; i++)
        {
            res[i].Position = Vertices[Indices[i]];
            res[i].Colour = colourval;
        }
        return res;
    }

    public override string ToString()
    {
        return $"{Index}: {Vertices.Length} vertices, {Triangles?.Length ?? 0} triangles";
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapOccludeModelTriangle : TriangleBVHItem
{
    public YmapOccludeModel Model { get; set; }
    public YmapFile? Ymap => Model?.Ymap;
    public int Index { get; set; }

    public YmapOccludeModelTriangle(YmapOccludeModel model, Vector3 v1, Vector3 v2, Vector3 v3, int i)
    {
        Model = model;
        Corner1 = v1;
        Corner2 = v2;
        Corner3 = v3;
        Index = i;
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class YmapBoxOccluder : BasePathData
{
    public BoxOccluder _Box;
    public ref BoxOccluder Box => ref _Box;

    public YmapFile Ymap { get; set; }


    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }
    public Vector3 BBMin { get; set; }
    public Vector3 BBMax { get; set; }
    public Quaternion Orientation { get; set; }
    public int Index { get; set; }



    public YmapBoxOccluder(YmapFile ymap, BoxOccluder box)
    {
        Ymap = ymap;
        _Box = box;

        Position = new Vector3(box.iCenterX, box.iCenterY, box.iCenterZ) / 4.0f;
        Size = new Vector3(box.iLength, box.iWidth, box.iHeight) / 4.0f;
        BBMin = Size * -0.5f;
        BBMax = Size * 0.5f;

        float cosz = box.iCosZ / 32767.0f;// ((float)short.MaxValue)
        float sinz = box.iSinZ / 32767.0f;

        float angl = (float)Math.Atan2(cosz, sinz);
        Orientation = Quaternion.RotationYawPitchRoll(0.0f, 0.0f, angl);

    }

    public void UpdateBoxStruct()
    {
        _Box.iCenterX = (short)Math.Round(Position.X * 4.0f);
        _Box.iCenterY = (short)Math.Round(Position.Y * 4.0f);
        _Box.iCenterZ = (short)Math.Round(Position.Z * 4.0f);
        _Box.iLength = (short)Math.Round(Size.X * 4.0f);
        _Box.iWidth = (short)Math.Round(Size.Y * 4.0f);
        _Box.iHeight = (short)Math.Round(Size.Z * 4.0f);

        var dir = Orientation.Multiply(in Vector3.UnitX) * 0.5f;
        _Box.iSinZ = (short)Math.Round(dir.X * 32767.0f);
        _Box.iCosZ = (short)Math.Round(dir.Y * 32767.0f);
    }


    public void SetSize(Vector3 s)
    {
        Size = s;
        BBMin = Size * -0.5f;
        BBMax = Size * 0.5f;
    }


    public EditorVertex[] GetTriangleVertices()
    {
        Vector3 xform(float x, float y, float z)
        {
            return Orientation.Multiply(new Vector3(x, y, z)) + Position;
        }
        EditorVertex[] res = new EditorVertex[36];
        var colour = new Color4(0.0f, 0.0f, 1.0f, 0.8f); //todo: colours for occluders?
        var c = (uint)colour.ToRgba();
        var s = Size * 0.5f;
        var v0 = new EditorVertex(xform(-s.X, -s.Y, -s.Z), c);
        var v1 = new EditorVertex(xform(-s.X, -s.Y, +s.Z), c);
        var v2 = new EditorVertex(xform(-s.X, +s.Y, -s.Z), c);
        var v3 = new EditorVertex(xform(-s.X, +s.Y, +s.Z), c);
        var v4 = new EditorVertex(xform(+s.X, -s.Y, -s.Z), c);
        var v5 = new EditorVertex(xform(+s.X, -s.Y, +s.Z), c);
        var v6 = new EditorVertex(xform(+s.X, +s.Y, -s.Z), c);
        var v7 = new EditorVertex(xform(+s.X, +s.Y, +s.Z), c);
        res[00] = v0; res[01] = v1; res[02] = v2; res[03] = v2; res[04] = v1; res[05] = v3;
        res[06] = v2; res[07] = v3; res[08] = v6; res[09] = v6; res[10] = v3; res[11] = v7;
        res[12] = v1; res[13] = v5; res[14] = v3; res[15] = v3; res[16] = v5; res[17] = v7;
        res[18] = v6; res[19] = v7; res[20] = v4; res[21] = v4; res[22] = v7; res[23] = v5;
        res[24] = v4; res[25] = v5; res[26] = v0; res[27] = v0; res[28] = v5; res[29] = v1;
        res[30] = v2; res[31] = v6; res[32] = v0; res[33] = v0; res[34] = v6; res[35] = v4;
        return res;
    }

    public override string ToString()
    {
        return $"{Index}: {FloatUtil.GetVector3String(Position)}";
    }
}
