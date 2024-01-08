using Collections.Pooled;
using SharpDX;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Archetype
    {
        public virtual MetaName Type => MetaName.CBaseArchetypeDef;

        public CBaseArchetypeDef _BaseArchetypeDef;
        public ref CBaseArchetypeDef BaseArchetypeDef => ref _BaseArchetypeDef; // for browsing.

        public MetaHash Hash { get; set; }
        public YtypFile Ytyp { get; set; }
        public MetaHash DrawableDict { get => _BaseArchetypeDef.drawableDictionary; set => _BaseArchetypeDef.drawableDictionary = value; }
        public MetaHash TextureDict { get => _BaseArchetypeDef.textureDictionary; set => _BaseArchetypeDef.textureDictionary = value; }
        public MetaHash ClipDict { get => _BaseArchetypeDef.clipDictionary; set => _BaseArchetypeDef.clipDictionary = value; }
        public ref Vector3 BBMin { get => ref _BaseArchetypeDef.bbMin; }
        public ref Vector3 BBMax { get => ref _BaseArchetypeDef.bbMax; }
        public ref Vector3 BSCenter { get => ref _BaseArchetypeDef.bsCentre; }
        public float BSRadius { get => _BaseArchetypeDef.bsRadius; set => _BaseArchetypeDef.bsRadius = value; }
        public float LodDist { get => _BaseArchetypeDef.lodDist; set => _BaseArchetypeDef.lodDist = value; }
        public MetaWrapper[] Extensions { get; set; }




        public string Name => _BaseArchetypeDef.name.ToString();
        public string AssetName => _BaseArchetypeDef.assetName.ToString();

        protected void InitVars()
        {
            Hash = _BaseArchetypeDef.assetName;
            if (Hash.Hash == 0)
                Hash = _BaseArchetypeDef.name;
            //DrawableDict = _BaseArchetypeDef.drawableDictionary;
            //TextureDict = _BaseArchetypeDef.textureDictionary;
            //ClipDict = _BaseArchetypeDef.clipDictionary;
            //BBMin = _BaseArchetypeDef.bbMin;
            //BBMax = _BaseArchetypeDef.bbMax;
            //BSCenter = _BaseArchetypeDef.bsCentre;
            //BSRadius = _BaseArchetypeDef.bsRadius;
            //LodDist = _BaseArchetypeDef.lodDist;
        }

        protected void InitVars(in CBaseArchetypeDef arch)
        {
            _BaseArchetypeDef = arch;
            InitVars();
        }

        public void Init(YtypFile ytyp, in CBaseArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(in arch);
        }

        public void Init(YtypFile ytyp, Span<byte> data)
        {
            Ytyp = ytyp;

            PsoTypes.TryConvertDataRaw(data, out _BaseArchetypeDef);
            InitVars();
        }

        public virtual bool IsActive(float hour)
        {
            return true;
        }

        public override string ToString()
        {
            return _BaseArchetypeDef.ToString();
        }
    }

    public readonly struct ActiveHours(uint flags)
    {
        public readonly uint TimeFlags = flags;

        public readonly bool this[int index] => ((TimeFlags >> index) & 1) == 1;

        public static implicit operator ActiveHours(uint d) => new (d);
        public static implicit operator uint(ActiveHours d) => d.TimeFlags;
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [SkipLocalsInit]
    public class TimeArchetype : Archetype
    {
        public override MetaName Type => MetaName.CTimeArchetypeDef;
        public CTimeArchetypeDef _TimeArchetypeDef;
        public CTimeArchetypeDef TimeArchetypeDef => _TimeArchetypeDef; // for browsing.
        public ActiveHours ActiveHours;
        public bool ExtraFlag => ActiveHours[24];

        public TimeArchetype()
        { }

        public void Init(YtypFile ytyp, in CTimeArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(in arch._BaseArchetypeDef);
            _TimeArchetypeDef = arch;
            ActiveHours = arch._TimeArchetypeDef.timeFlags;
        }

        new public void Init(YtypFile ytyp, Span<byte> data)
        {
            Ytyp = ytyp;
            PsoTypes.TryConvertDataRaw(data, out _TimeArchetypeDef);
            InitVars(in _TimeArchetypeDef._BaseArchetypeDef);
        }

        public override bool IsActive(float hour)
        {
            int h = ((int)hour) % 24;
            if ((h < 0) || (h > 23))
                return true;
            return ActiveHours[h];
        }

        public void SetTimeFlags(uint flags)
        {
            ActiveHours = flags;
            _TimeArchetypeDef._TimeArchetypeDef.timeFlags = flags;
        }
    }

    public class MloArchetype : Archetype
    {
        public override MetaName Type => MetaName.CMloArchetypeDef;


        public CMloArchetypeDef MloArchetypeDef => _MloArchetypeDef; // for browsing.
        public CMloArchetypeDef _MloArchetypeDef;
        public ref CMloArchetypeDefData _MloArchetypeDefData => ref _MloArchetypeDef._MloArchetypeDef;

        public MCEntityDef[] entities { get; set; }
        public MCMloRoomDef[] rooms { get; set; }
        public MCMloPortalDef[] portals { get; set; }
        public MCMloEntitySet[] entitySets { get; set; }
        public CMloTimeCycleModifier[] timeCycleModifiers { get; set; }


        public void Init(YtypFile ytyp, in CMloArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(in arch._BaseArchetypeDef);
            _MloArchetypeDef = arch;
            //_MloArchetypeDefData = ref arch._MloArchetypeDef;
        }

        public void Init(YtypFile ytyp, Span<byte> data)
        {
            Ytyp = ytyp;
            PsoTypes.TryConvertDataRaw(data, out _MloArchetypeDef);
            InitVars(in _MloArchetypeDef._BaseArchetypeDef);

            //_MloArchetypeDefData = ref _MloArchetypeDef._MloArchetypeDef;
        }

        public bool AddEntity(YmapEntityDef ent, int roomIndex, int portalIndex = -1, int entsetIndex = -1)
        {
            if (ent is null)
                return false;

            if (roomIndex >= (rooms?.Length ?? 0))
            {
                throw new ArgumentOutOfRangeException($"Room index {roomIndex} exceeds the amount of rooms in {Name}.");
            }
            if (portalIndex >= (portals?.Length ?? 0))
            {
                throw new ArgumentOutOfRangeException($"Portal index {portalIndex} exceeds the amount of portals in {Name}.");
            }
            if (entsetIndex >= (entitySets?.Length ?? 0))
            {
                throw new ArgumentOutOfRangeException($"EntitySet index {entsetIndex} exceeds the amount of entitySets in {Name}.");
            }

            var mcEntityDef = new MCEntityDef(in ent._CEntityDef, this);


            if ((roomIndex >= 0) || (portalIndex >= 0))
            {
                var entList = entities?.ToList() ?? new List<MCEntityDef>();
                ent.Index = entList.Count;
                entList.Add(mcEntityDef);
                entities = entList.ToArray();

                if (roomIndex >= 0)
                {
                    var attachedObjs = rooms[roomIndex].AttachedObjects?.ToList() ?? new List<uint>();
                    attachedObjs.Add((uint)ent.Index);
                    rooms[roomIndex].AttachedObjects = attachedObjs.ToArray();
                }
                if (portalIndex >= 0)
                {
                    var attachedObjs = portals[portalIndex].AttachedObjects?.ToList() ?? new List<uint>();
                    attachedObjs.Add((uint)ent.Index);
                    portals[portalIndex].AttachedObjects = attachedObjs.ToArray();
                }
            }
            else if (entsetIndex >= 0)
            {
                var entset = entitySets[entsetIndex];

                var entList = entset.Entities?.ToList() ?? new List<MCEntityDef>();
                entList.Add(mcEntityDef);
                entset.Entities = entList.ToArray();

                var locs = entset.Locations?.ToList() ?? new List<uint>();
                locs.Add(0);//choose a better default location?
                entset.Locations = locs.ToArray();


                var mloInstance = ent.MloParent?.MloInstance;
                if ((mloInstance?.EntitySets != null) && (entsetIndex < mloInstance.EntitySets.Length))
                {
                    ent.MloEntitySet = mloInstance.EntitySets[entsetIndex];
                }
            }
            else return false;

            UpdateAllEntityIndexes();

            return true;
        }
        public bool RemoveEntity(YmapEntityDef ent)
        {
            if (ent is null)
                return false;

            if ((ent.MloEntitySet?.Entities != null) && (ent.MloEntitySet?.EntitySet != null))
            {
                var instents = ent.MloEntitySet.Entities;
                var set = ent.MloEntitySet.EntitySet;
                var idx = instents.IndexOf(ent);
                if (idx >= 0)
                {
                    var ents = set.Entities.ToList();
                    ents.RemoveAt(idx);
                    set.Entities = ents.ToArray();

                    var locs = set.Locations.ToList();
                    locs.RemoveAt(idx);
                    set.Locations = locs.ToArray();

                    UpdateAllEntityIndexes();
                    return true;
                }
                return false;
            }

            if (ent.Index >= entities.Length) return false;

            var delent = entities[ent.Index];
            if (delent != null)
            {
                var newentities = new MCEntityDef[entities.Length - 1];
                var didDel = false;
                int index = 0;
                int delIndex = 0;
                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i] == delent)
                    {
                        delIndex = i;
                        didDel = true;
                        continue;
                    }

                    var newent = entities[i];
                    newentities[index] = newent;
                    index++;
                }

                entities = newentities;

                if (didDel)
                {
                    FixRoomIndexes(delIndex);
                    FixPortalIndexes(delIndex);
                    UpdateAllEntityIndexes();
                }
                return didDel;
            }

            return false;
        }

        public void AddRoom(MCMloRoomDef room)
        {
            if (room is null)
                return;

            room.OwnerMlo = this;
            room.Index = rooms?.Length ?? 0;

            var newrooms = rooms?.ToList() ?? new List<MCMloRoomDef>();
            newrooms.Add(room);
            rooms = newrooms.ToArray();
        }
        public void RemoveRoom(MCMloRoomDef room)
        {
            if (room is null)
                return;

            var newrooms = rooms.ToList();
            newrooms.Remove(room);
            rooms = newrooms.ToArray();

            for (int i = 0; i < rooms.Length; i++)
            {
                rooms[i].Index = i;
            }

            UpdatePortalCounts();//portal room indices probably would need to be updated anyway
        }

        public void AddPortal(MCMloPortalDef portal)
        {
            if (portal is null)
                return;

            portal.OwnerMlo = this;
            portal.Index = portals?.Length ?? 0;

            using var newportals = portals?.ToPooledList() ?? new PooledList<MCMloPortalDef>();
            newportals.Add(portal);
            portals = newportals.ToArray();

            UpdatePortalCounts();
        }
        public void RemovePortal(MCMloPortalDef portal)
        {
            if (portal is null)
                return;

            using var newportals = portals.ToPooledList();
            newportals.Remove(portal);
            portals = newportals.ToArray();

            for (int i = 0; i < portals.Length; i++)
            {
                portals[i].Index = i;
            }

            UpdatePortalCounts();
        }

        public void AddEntitySet(MCMloEntitySet set)
        {
            if (set is null)
                return;

            set.OwnerMlo = this;
            set.Index = entitySets?.Length ?? 0;

            var newsets = entitySets?.ToList() ?? new List<MCMloEntitySet>();
            newsets.Add(set);
            entitySets = newsets.ToArray();
        }
        public void RemoveEntitySet(MCMloEntitySet set)
        {
            if (set is null)
                return;

            var newsets = entitySets.ToList();
            newsets.Remove(set);
            entitySets = newsets.ToArray();

            for (int i = 0; i < entitySets.Length; i++)
            {
                entitySets[i].Index = i;
            }

            UpdateAllEntityIndexes();
        }




        private void FixPortalIndexes(int deletedIndex)
        {
            foreach (var portal in portals)
            {
                if (portal.AttachedObjects is null || portal.AttachedObjects.Length == 0)
                    continue;

                using PooledList<uint> newAttachedObjects = new PooledList<uint>(portal.AttachedObjects.Length);
                foreach (var objIndex in portal.AttachedObjects)
                {
                    if (objIndex == deletedIndex)
                        continue;
                    if (objIndex > deletedIndex)
                        newAttachedObjects.Add(objIndex - 1); // move the index back so it matches the index in the entitiy array.
                    else newAttachedObjects.Add(objIndex); // else just add the index to the attached objects.
                }

                portal.AttachedObjects = newAttachedObjects.ToArray();
            }
        }
        private void FixRoomIndexes(int deletedIndex)
        {
            foreach (var room in rooms)
            {
                if (room.AttachedObjects is null || room.AttachedObjects.Length == 0)
                    continue;

                using PooledList<uint> newAttachedObjects = new PooledList<uint>(room.AttachedObjects.Length);
                foreach (var objIndex in room.AttachedObjects)
                {
                    if (objIndex == deletedIndex)
                        continue;
                    if (objIndex > deletedIndex)
                        newAttachedObjects.Add(objIndex - 1); // move the index back so it matches the index in the entitiy array.
                    else newAttachedObjects.Add(objIndex); // else just add the index to the attached objects.
                }
                room.AttachedObjects = newAttachedObjects.ToArray();
            }
        }

        private void UpdateAllEntityIndexes()
        {
            var index = 0;
            if (entities != null)
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    entities[i].Index = index++;
                }
            }
            if (entitySets != null)
            {
                for (int e = 0; e < entitySets.Length; e++)
                {
                    var set = entitySets[e];
                    if (set?.Entities == null) continue;
                    for (int i = 0; i < set.Entities.Length; i++)
                    {
                        set.Entities[i].Index = index++;
                    }
                }
            }
        }

        public void UpdatePortalCounts()
        {
            if (rooms is null || portals is null)
                return;

            foreach (var room in rooms)
            {
                uint count = 0;
                foreach (var portal in portals)
                {
                    if (portal._Data.roomFrom == room.Index || portal._Data.roomTo == room.Index)
                    {
                        count++;
                    }
                }
                room._Data.portalCount = count;
            }

        }


        public void LoadChildren(Meta meta)
        {
            if (_MloArchetypeDefData.entities.Count1 > 0)
            {
                var centitiesBuffer = ArrayPool<CEntityDef>.Shared.Rent(_MloArchetypeDefData.entities.Count1);
                MetaTypes.ConvertDataArray(meta, MetaName.CEntityDef, in _MloArchetypeDefData.entities, centitiesBuffer);
                var centities = centitiesBuffer.AsSpan(0, _MloArchetypeDefData.entities.Count1);
                entities = new MCEntityDef[centities.Length];
                for (int i = 0; i < centities.Length; i++)
                {
                    entities[i] = new MCEntityDef(meta, centities[i]) { OwnerMlo = this, Index = i };
                }
                ArrayPool<CEntityDef>.Shared.Return(centitiesBuffer);
            }

            if (_MloArchetypeDefData.rooms.Count1 > 0)
            {
                var croomsBuffer = ArrayPool<CMloRoomDef>.Shared.Rent(_MloArchetypeDefData.rooms.Count1);
                MetaTypes.ConvertDataArray(meta, MetaName.CMloRoomDef, in _MloArchetypeDefData.rooms, croomsBuffer);
                var crooms = croomsBuffer.AsSpan(0, _MloArchetypeDefData.rooms.Count1);
                rooms = new MCMloRoomDef[crooms.Length];
                for (int i = 0; i < crooms.Length; i++)
                {
                    rooms[i] = new MCMloRoomDef(meta, crooms[i]) { OwnerMlo = this, Index = i };
                }
                ArrayPool<CMloRoomDef>.Shared.Return(croomsBuffer);
            }

            if (_MloArchetypeDefData.portals.Count1 > 0)
            {
                var cportalsBuffer = ArrayPool<CMloPortalDef>.Shared.Rent(_MloArchetypeDefData.portals.Count1);
                MetaTypes.ConvertDataArray(meta, MetaName.CMloPortalDef, in _MloArchetypeDefData.portals, cportalsBuffer);
                var cportals = cportalsBuffer.AsSpan(0, _MloArchetypeDefData.portals.Count1);
                portals = new MCMloPortalDef[cportals.Length];
                for (int i = 0; i < cportals.Length; i++)
                {
                    portals[i] = new MCMloPortalDef(meta, cportals[i]) { OwnerMlo = this, Index = i };
                }
                ArrayPool<CMloPortalDef>.Shared.Return(cportalsBuffer);
            }

            if (_MloArchetypeDefData.entitySets.Count1 > 0)
            {
                var centitySetsBuffer = ArrayPool<CMloEntitySet>.Shared.Rent(_MloArchetypeDefData.entitySets.Count1);
                MetaTypes.ConvertDataArray(meta, MetaName.CMloEntitySet, in _MloArchetypeDefData.entitySets, centitySetsBuffer);
                var centitySets = centitySetsBuffer.AsSpan(0, _MloArchetypeDefData.entitySets.Count1);

                entitySets = new MCMloEntitySet[centitySets.Length];
                for (int i = 0; i < centitySets.Length; i++)
                {
                    entitySets[i] = new MCMloEntitySet(meta, centitySets[i], this) { Index = i };
                }
                ArrayPool<CMloEntitySet>.Shared.Return(centitySetsBuffer);
                UpdateAllEntityIndexes();

            }

            if (_MloArchetypeDefData.timeCycleModifiers.Count1 > 0)
            {
                timeCycleModifiers = MetaTypes.ConvertDataArray<CMloTimeCycleModifier>(meta, MetaName.CMloTimeCycleModifier, in _MloArchetypeDefData.timeCycleModifiers);
            }
        }

        public int GetEntityObjectIndex(MCEntityDef ent)
        {
            if (entities == null) return -1;
            for (int i = 0; i < entities.Length; i++)
            {
                var e = entities[i];
                if (e == ent)
                {
                    return i;
                }
            }
            return -1;
        }
        public MCMloRoomDef? GetEntityRoom(MCEntityDef ent)
        {
            if (rooms == null)
                return null;

            int objectIndex = GetEntityObjectIndex(ent);
            if (objectIndex < 0)
                return null;

            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (r.AttachedObjects != null)
                {
                    for (int j = 0; j < r.AttachedObjects.Length; j++)
                    {
                        var ind = r.AttachedObjects[j];
                        if (ind == objectIndex)
                        {
                            return r;
                        }
                    }
                }
            }

            return null;
        }
        public MCMloPortalDef? GetEntityPortal(MCEntityDef ent)
        {
            if (portals is null)
                return null;

            int objectIndex = GetEntityObjectIndex(ent);
            if (objectIndex < 0)
                return null;

            for (int i = 0; i < portals.Length; i++)
            {
                var p = portals[i];
                if (p.AttachedObjects is not null)
                {
                    for (int j = 0; j < p.AttachedObjects.Length; j++)
                    {
                        var ind = p.AttachedObjects[j];
                        if (ind == objectIndex)
                        {
                            return p;
                        }
                    }
                }
            }

            return null;
        }
        public MCMloEntitySet? GetEntitySet(MCEntityDef ent)
        {
            if (entitySets == null) return null;

            for (int i = 0; i < entitySets.Length; i++)
            {
                var set = entitySets[i];
                if (set.Entities != null)
                {
                    for (int j = 0; j < set.Entities.Length; j++)
                    {
                        if (set.Entities[j] == ent)
                        {
                            return set;
                        }
                    }
                }
            }

            return null;
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MloInstanceData
    {
        public YmapEntityDef Owner { get; set; }
        public MloArchetype MloArch { get; set; }
        public CMloInstanceDef _Instance;
        public ref CMloInstanceDef Instance => ref _Instance;
        public uint[] defaultEntitySets { get; set; }

        public YmapEntityDef[] Entities { get; set; }
        public MloInstanceEntitySet[] EntitySets { get; set; }

        public MloInstanceData(YmapEntityDef owner, MloArchetype mloa)
        {
            Owner = owner;
            MloArch = mloa;
        }

        public void CreateYmapEntities()
        {
            if (Owner is null)
                return;
            if (MloArch?.entities is null)
                return;
            var ec = MloArch.entities.Length;

            var entlist = new List<YmapEntityDef>(ec);
            for (int i = 0; i < ec; i++)
            {
                var e = CreateYmapEntity(Owner, MloArch.entities[i], i);
                entlist.Add(e);
            }

            int lasti = ec;

            var entitySets = MloArch.entitySets;
            if (entitySets != null)
            {
                EntitySets = new MloInstanceEntitySet[entitySets.Length];
                for (int i = 0; i < entitySets.Length; i++)
                {
                    var entitySet = entitySets[i];
                    var instset = new MloInstanceEntitySet(entitySet, this);
                    if (entitySet.Entities != null)
                    {
                        for (int j = 0; j < entitySet.Entities.Length; j++)
                        {
                            var e = CreateYmapEntity(Owner, entitySet.Entities[j], lasti);
                            instset.Entities.Add(e);
                            e.MloEntitySet = instset;
                            lasti++;
                        }
                    }
                    EntitySets[i] = instset;
                }
            }

            if (defaultEntitySets != null && EntitySets != null)
            {
                for (var i = 0; i < defaultEntitySets.Length; i++)
                {
                    uint index = defaultEntitySets[i];
                    if (index >= EntitySets.Length) continue;
                    var instset = EntitySets[index];
                    instset.Visible = true;
                }
            }

            Entities = entlist.ToArray();
        }

        public void InitYmapEntityArchetypes(GameFileCache gfc)
        {
            if (Owner is null)
                return;
            var arch = Owner.Archetype;

            if (Entities is not null)
            {
                foreach(var ient in Entities)
                {
                    var iarch = gfc.GetArchetype(ient._CEntityDef.archetypeName);

                    if (iarch is null)
                    {
                        Console.WriteLine($"Can't find archetype for {ient._CEntityDef.archetypeName} for ymap {Owner.Name}!");
                    }
                    else
                    {
                        ient.SetArchetype(iarch);
                    }
                }

                UpdateBBs(arch);
            }

            if (EntitySets is not null)
            {
                foreach(var entitySet in EntitySets)
                {
                    var entities = entitySet.Entities;
                    if (entities is null)
                        continue;

                    for (int i = 0; i < entities.Count; i++)
                    {
                        var ient = entities[i];
                        var iarch = gfc.GetArchetype(ient._CEntityDef.archetypeName);

                        if (iarch is null)
                        {
                            Console.WriteLine($"Couldn't find archetype {ient._CEntityDef.archetypeName} ({ient._CEntityDef.archetypeName.Hash}) for ymap {Owner.Name}");
                        }
                        else
                        {
                            ient.SetArchetype(iarch);
                        }
                    }
                }
            }
        }

        public void UpdateBBs(Archetype arch)
        {
            //update archetype room AABB's.. bad to have this here? where else to put it?
            if (arch is MloArchetype mloa)
            {
                Vector3 mlobbmin = Vector3.Zero;
                Vector3 mlobbmax = Vector3.Zero;
                Vector3[] c = new Vector3[8];
                var rooms = mloa.rooms;
                if (rooms is not null)
                {
                    foreach(var room in rooms)
                    {
                        if (room.AttachedObjects == null || room.AttachedObjects.Length == 0)
                            continue;
                        Vector3 min = new Vector3(float.MaxValue);
                        Vector3 max = new Vector3(float.MinValue);
                        for (int k = 0; k < room.AttachedObjects.Length; k++)
                        {
                            var objid = room.AttachedObjects[k];
                            if (objid < Entities.Length)
                            {
                                var rooment = Entities[objid];
                                if ((rooment != null) && (rooment.Archetype != null))
                                {
                                    var earch = rooment.Archetype;
                                    var pos = rooment._CEntityDef.position;
                                    var ori = rooment.Orientation;
                                    Vector3 abmin = earch.BBMin * rooment.Scale; //entity box
                                    Vector3 abmax = earch.BBMax * rooment.Scale;
                                    c[0] = abmin;
                                    c[1] = new Vector3(abmin.X, abmin.Y, abmax.Z);
                                    c[2] = new Vector3(abmin.X, abmax.Y, abmin.Z);
                                    c[3] = new Vector3(abmin.X, abmax.Y, abmax.Z);
                                    c[4] = new Vector3(abmax.X, abmin.Y, abmin.Z);
                                    c[5] = new Vector3(abmax.X, abmin.Y, abmax.Z);
                                    c[6] = new Vector3(abmax.X, abmax.Y, abmin.Z);
                                    c[7] = abmax;
                                    for (int n = 0; n < 8; n++)
                                    {
                                        Vector3 corn = ori.Multiply(in c[n]) + pos;
                                        Vectors.Min(in min, in corn, out min);
                                        Vectors.Max(in max, in corn, out max);
                                    }
                                }
                            }
                        }
                        room.BBMin_CW = min;
                        room.BBMax_CW = max;
                        mlobbmin = Vector3.Min(mlobbmin, min);
                        mlobbmax = Vector3.Max(mlobbmax, max);
                    }
                }
                mloa.BBMin = mlobbmin;
                mloa.BBMax = mlobbmax;
            }
        }

        public YmapEntityDef CreateYmapEntity(YmapEntityDef owner, MCEntityDef ment, int index)
        {
            YmapEntityDef e = new YmapEntityDef(null, index, ment._Data);
            e.Extensions = ment.Extensions;
            e.MloRefPosition = e.Position;
            e.MloRefOrientation = e.Orientation;
            e.MloParent = owner;
            e.Position = owner.Position + owner.Orientation.Multiply(e.MloRefPosition);
            e.Orientation = Quaternion.Multiply(owner.Orientation, e.MloRefOrientation);
            e.UpdateWidgetPosition();
            e.UpdateWidgetOrientation();
            e.UpdateEntityHash();
            return e;
        }

        public MCEntityDef? TryGetArchetypeEntity(YmapEntityDef ymapEntity)
        {
            if (ymapEntity is null)
                return null;
            if (Owner?.Archetype is null)
                return null;
            if (Owner.Archetype is not MloArchetype mloa)
                return null;

            var index = Array.FindIndex(Entities, x => x == ymapEntity);
            if ((index >= 0) && (index < mloa.entities.Length))
            {
                return mloa.entities[index];
            }

            if (EntitySets != null)
            {
                foreach(var entset in EntitySets)
                {
                    var ents = entset.Entities;
                    var set = entset.EntitySet;
                    var setents = set?.Entities;
                    if (ents is null || setents is null)
                        continue;
                    var idx = ents.IndexOf(ymapEntity);
                    if ((idx >= 0) && (idx < setents.Length))
                    {
                        return setents[idx];
                    }
                }
            }

            return null;
        }

        public YmapEntityDef? TryGetYmapEntity(MCEntityDef mcEntity)
        {
            if (mcEntity is null)
                return null;
            if (Owner?.Archetype is null)
                return null;
            if (Owner.Archetype is not MloArchetype mloa)
                return null;

            var index = Array.FindIndex(mloa.entities, x => x == mcEntity);
            if ((index >= 0) && (index < Entities.Length))
            {
                return Entities[index];
            }

            if (EntitySets is not null)
            {
                foreach(var entset in EntitySets)
                {
                    var ents = entset.Entities;
                    var set = entset.EntitySet;
                    var setents = set?.Entities;
                    if (ents is null || setents is null)
                        continue;
                    var idx = Array.FindIndex(setents, x => x == mcEntity);
                    if ((idx >= 0) && (idx < ents.Count))
                    {
                        return ents[idx];
                    }
                }
            }

            return null;
        }

        public void SetPosition(Vector3 pos)
        {
            var cent = _Instance.CEntityDef;
            cent.position = pos;
            _Instance.CEntityDef = cent; //TODO: maybe find a better way of doing this...
        }

        public void SetOrientation(Quaternion ori)
        {
            var cent = _Instance.CEntityDef;
            cent.rotation = new Vector4(ori.X, ori.Y, ori.Z, ori.W); //mlo instances have oppposite orientations to normal entities...
            _Instance.CEntityDef = cent; //TODO: maybe find a better way of doing this...
        }

        public void UpdateEntities()
        {
            if (Entities is null)
                return;
            if (Owner is null)
                return;

            foreach(var ent in Entities)
            {
                UpdateEntity(ent);
            }

            if (EntitySets != null && EntitySets.Length > 0)
            {
                foreach(var entset in EntitySets)
                {
                    if (entset?.Entities is not null)
                    {
                        for (int i = 0; i < entset.Entities.Count; i++)
                        {
                            var ent = entset.Entities[i];
                            UpdateEntity(ent);
                        }
                    }
                }
            }

        }

        public void UpdateEntity(YmapEntityDef e)
        {
            e.Position = Owner.Position + Owner.Orientation.Multiply(e.MloRefPosition);
            e.Orientation = Quaternion.Multiply(Owner.Orientation, e.MloRefOrientation);
            e.UpdateWidgetPosition();
            e.UpdateWidgetOrientation();
            e.UpdateEntityHash();
        }

        public void AddEntity(YmapEntityDef e)
        {
            if (e is null)
                return;

            if (e.MloEntitySet is not null)
            {
                e.MloEntitySet.AddEntity(e);
            }
            else
            {
                Entities ??= Array.Empty<YmapEntityDef>();
                var entities = Entities.ToList();
                entities.Add(e);
                Entities = entities.ToArray();
            }
            UpdateAllEntityIndexes();
        }

        public bool DeleteEntity(YmapEntityDef entityToRemove)
        {
            var del = false;
            if (entityToRemove.MloEntitySet is not null)
            {
                del = entityToRemove.MloEntitySet.DeleteEntity(entityToRemove);
                UpdateAllEntityIndexes();
                return del;
            }

            if (Entities is null)
            {
                throw new NullReferenceException("The Entities list returned null in our MloInstanceData. This could be an issue with initialization. The MloInstance probably doesn't exist.");
            }
            if (entityToRemove.Index >= Entities.Length)
            {
                throw new ArgumentOutOfRangeException("The index of the entity was greater than the amount of entities that exist in this MloInstance. Likely an issue with initializion.");
            }

            int index = 0;
            var newentities = new YmapEntityDef[Entities.Length - 1];

            foreach(var thisEnt in Entities)
            { 
                if (thisEnt.Index == entityToRemove.Index)
                {
                    del = true;
                    continue;
                }
                newentities[index] = thisEnt;
                newentities[index].Index = index;
                index++;
            }
            if (del)
            {
                Entities = newentities;
                UpdateAllEntityIndexes();
                return true;
            }
            else throw new ArgumentException("The entity specified was not found in this MloInstance. It cannot be deleted.");
        }

        public void AddEntitySet(MCMloEntitySet set)
        {
            var instset = new MloInstanceEntitySet(set, this);
            using var esets = EntitySets?.ToPooledList() ?? new PooledList<MloInstanceEntitySet>();
            esets.Add(instset);
            EntitySets = esets.ToArray();
        }

        public bool DeleteEntitySet(MCMloEntitySet set)
        {
            if (EntitySets is null)
                return false;
            using var esets = EntitySets.ToPooledList();
            var rem = esets.RemoveAll(s => s.EntitySet == set);
            EntitySets = esets.ToArray();
            UpdateAllEntityIndexes();
            return rem == 1;
        }

        public void UpdateDefaultEntitySets()
        {
            var list = new List<uint>();

            if (EntitySets != null)
            {
                for (uint i = 0; i < EntitySets.Length; i++)
                {
                    var entset = EntitySets[i];
                    if (entset != null)
                    {
                        if (entset.Visible)
                        {
                            list.Add(i);
                        }
                    }
                }
            }

            defaultEntitySets = list.ToArray();
        }

        private void UpdateAllEntityIndexes()
        {
            var index = 0;
            if (Entities is not null)
            {
                foreach(var ent in Entities)
                {
                    ent.Index = index++;
                }
            }
            if (EntitySets is not null)
            {
                foreach(var set in EntitySets)
                {
                    if (set?.Entities is null)
                        continue;

                    foreach(var ent in set.Entities)
                    {
                        ent.Index = index++;
                    }
                }
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MloInstanceEntitySet
    {
        public MloInstanceEntitySet(MCMloEntitySet entSet, MloInstanceData instance)
        {
            EntitySet = entSet;
            Entities = new List<YmapEntityDef>();
            Instance = instance;
        }

        public MCMloEntitySet EntitySet { get; set; }
        public List<YmapEntityDef> Entities { get; set; }
        public MloInstanceData Instance { get; set; }

        public uint[] Locations
        {
            get {
                return EntitySet?.Locations;
            }
            set {
                if (EntitySet != null)
                    EntitySet.Locations = value;
            }
        }

        public bool Visible { get; set; }
        public bool VisibleOrForced
        {
            get
            {
                if (Visible)
                    return true;
                if (EntitySet == null)
                    return false;
                return EntitySet.ForceVisible;
            }
        }

        public void AddEntity(YmapEntityDef ent)
        {
            if (Entities == null) Entities = new List<YmapEntityDef>();
            Entities.Add(ent);
        }
        public bool DeleteEntity(YmapEntityDef ent)
        {
            if (Entities == null) return false;
            return Entities.Remove(ent);
        }
    }


}
