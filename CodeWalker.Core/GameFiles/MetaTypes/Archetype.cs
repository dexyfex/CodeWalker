using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Archetype
    {
        public virtual MetaName Type => MetaName.CBaseArchetypeDef;

        public CBaseArchetypeDef _BaseArchetypeDef;
        public CBaseArchetypeDef BaseArchetypeDef => _BaseArchetypeDef; // for browsing.

        public MetaHash Hash { get; set; }
        public YtypFile Ytyp { get; set; }
        public MetaHash DrawableDict { get; set; }
        public MetaHash TextureDict { get; set; }
        public MetaHash ClipDict { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public Vector3 BSCenter { get; set; }
        public float BSRadius { get; set; }
        public float LodDist { get; set; }
        public MetaWrapper[] Extensions { get; set; }




        public string Name
        {
            get
            {
                return _BaseArchetypeDef.name.ToString();
            }
        }
        public string AssetName
        {
            get
            {
                return _BaseArchetypeDef.assetName.ToString();
            }
        }


        protected void InitVars(ref CBaseArchetypeDef arch)
        {
            _BaseArchetypeDef = arch;
            Hash = arch.assetName;
            if (Hash.Hash == 0) Hash = arch.name;
            DrawableDict = arch.drawableDictionary;
            TextureDict = arch.textureDictionary;
            ClipDict = arch.clipDictionary;
            BBMin = arch.bbMin;
            BBMax = arch.bbMax;
            BSCenter = arch.bsCentre;
            BSRadius = arch.bsRadius;
            LodDist = arch.lodDist;
        }

        public void Init(YtypFile ytyp, ref CBaseArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch);
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

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TimeArchetype : Archetype
    {
        public override MetaName Type => MetaName.CTimeArchetypeDef;
        public CTimeArchetypeDef _TimeArchetypeDef;
        public CTimeArchetypeDef TimeArchetypeDef => _TimeArchetypeDef; // for browsing.

        public uint TimeFlags { get; set; }
        public bool[] ActiveHours { get; set; }
        public string[] ActiveHoursText { get; set; }
        public bool ExtraFlag { get; set; }


        public void Init(YtypFile ytyp, ref CTimeArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            _TimeArchetypeDef = arch;

            TimeFlags = arch.TimeArchetypeDef.timeFlags;
            ActiveHours = new bool[24];
            ActiveHoursText = new string[24];
            for (int i = 0; i < 24; i++)
            {
                bool v = ((TimeFlags >> i) & 1) == 1;
                ActiveHours[i] = v;

                int nxth = (i < 23) ? (i + 1) : 0;
                string hrs = string.Format("{0:00}:00 - {1:00}:00", i, nxth);
                ActiveHoursText[i] = (hrs + (v ? " - On" : " - Off"));
            }
            ExtraFlag = ((TimeFlags >> 24) & 1) == 1;
        }

        public override bool IsActive(float hour)
        {
            if (ActiveHours == null) return true;
            int h = ((int)hour) % 24;
            if ((h < 0) || (h > 23)) return true;
            return ActiveHours[h];
        }
    }

    public class MloArchetype : Archetype
    {
        public override MetaName Type => MetaName.CMloArchetypeDef;


        public CMloArchetypeDef MloArchetypeDef => _MloArchetypeDef; // for browsing.
        public CMloArchetypeDef _MloArchetypeDef;
        public CMloArchetypeDefData _MloArchetypeDefData;

        public MCEntityDef[] entities { get; set; }
        public MCMloRoomDef[] rooms { get; set; }
        public MCMloPortalDef[] portals { get; set; }
        public MCMloEntitySet[] entitySets { get; set; }
        public CMloTimeCycleModifier[] timeCycleModifiers { get; set; }


        public void Init(YtypFile ytyp, ref CMloArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            _MloArchetypeDef = arch;
            _MloArchetypeDefData = arch.MloArchetypeDef;
        }

        public bool AddEntity(YmapEntityDef ent, int roomIndex)
        {
            

            // entity already exists in our array. so we'll just add
            // it to the instanced entities list and continue.
            MloInstanceData mloInstance = ent.MloParent?.MloInstance;
            MCEntityDef ymcent = mloInstance?.TryGetArchetypeEntity(ent);
            if (ymcent != null)
            {
                return true;
            }

            if (roomIndex > rooms.Length)
            {
                throw new ArgumentOutOfRangeException($"Room index {roomIndex} exceeds the amount of rooms in {Name}.");
            }

            MCEntityDef mcEntityDef;
            if (ymcent != null)
            {
                mcEntityDef = ymcent;
            }
            else
            {
                mcEntityDef = new MCEntityDef(ref ent._CEntityDef, this/*, ent*/);
            }

            // Add the new entity def to the entities list.
            AddEntity(ent, mcEntityDef);

            // Update the attached objects in the room index specified.
            AttachEntityToRoom(ent, roomIndex);
            return true;
        }

        // attaches the specified ymap entity index to the room at roomIndex. 
        private void AttachEntityToRoom(YmapEntityDef ent, int roomIndex)
        {
            if (roomIndex > rooms.Length)
            {
                return; // the room index is bigger than the rooms we have...
            }
            var attachedObjs = rooms[roomIndex].AttachedObjects?.ToList() ?? new List<uint>();
            attachedObjs.Add((uint)ent.Index);
            rooms[roomIndex].AttachedObjects = attachedObjs.ToArray();
        }

        // Adds an entity to the entities array and then set's the index of the
        // ymap entity to the index of the new MCEntityDef.
        private void AddEntity(YmapEntityDef ent, MCEntityDef mcEntityDef)
        {
            if (ent == null || mcEntityDef == null) return; // no entity?...
            // initialize the array.
            if (entities == null) entities = new MCEntityDef[0];

            List<MCEntityDef> entList = entities.ToList();
            entList.Add(mcEntityDef);
            ent.Index = entList.IndexOf(mcEntityDef);
            entities = entList.ToArray();
        }

        public bool RemoveEntity(YmapEntityDef ent)
        {
            if (ent.Index >= entities.Length) return false;

            MCEntityDef delent = entities[ent.Index];
            MloInstanceData inst = ent.MloParent?.MloInstance;
            if (inst == null) return false;

            if (delent != null)
            {
                MCEntityDef[] newentities = new MCEntityDef[entities.Length - 1];
                bool didDel = false;
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

                    newentities[index] = entities[i];
                    YmapEntityDef ymapEntityDef = inst.TryGetYmapEntity(newentities[index]);
                    if (ymapEntityDef != null) ymapEntityDef.Index = index;
                    index++;
                }
                entities = newentities;

                if (didDel) FixRoomIndexes(delIndex);
                return didDel;
            }

            return false;
        }

        private void FixRoomIndexes(int deletedIndex)
        {
            foreach (var room in rooms)
            {
                List<uint> newAttachedObjects = new List<uint>();
                if (room.AttachedObjects == null)
                    continue;
                foreach (var objIndex in room.AttachedObjects)
                {
                    if (objIndex == deletedIndex) continue;
                    if (objIndex > deletedIndex)
                        newAttachedObjects.Add(objIndex - 1); // move the index back so it matches the index in the entitiy array.
                    else newAttachedObjects.Add(objIndex); // else just add the index to the attached objects.
                }
                room.AttachedObjects = newAttachedObjects.ToArray();
            }
        }

        public void LoadChildren(Meta meta)
        {
            var centities = MetaTypes.ConvertDataArray<CEntityDef>(meta, MetaName.CEntityDef, _MloArchetypeDefData.entities);
            if (centities != null)
            {
                entities = new MCEntityDef[centities.Length];
                for (int i = 0; i < centities.Length; i++)
                {
                    entities[i] = new MCEntityDef(meta, ref centities[i]) { MloArchetype = this };
                }
            }

            var crooms = MetaTypes.ConvertDataArray<CMloRoomDef>(meta, MetaName.CMloRoomDef, _MloArchetypeDefData.rooms);
            if (crooms != null)
            {
                rooms = new MCMloRoomDef[crooms.Length];
                for (int i = 0; i < crooms.Length; i++)
                {
                    rooms[i] = new MCMloRoomDef(meta, crooms[i]);
                }
            }

            var cportals = MetaTypes.ConvertDataArray<CMloPortalDef>(meta, MetaName.CMloPortalDef, _MloArchetypeDefData.portals);
            if (cportals != null)
            {
                portals = new MCMloPortalDef[cportals.Length];
                for (int i = 0; i < cportals.Length; i++)
                {
                    portals[i] = new MCMloPortalDef(meta, cportals[i]);
                }
            }

            var centitySets = MetaTypes.ConvertDataArray<CMloEntitySet>(meta, MetaName.CMloEntitySet, _MloArchetypeDefData.entitySets);
            if (centitySets != null)
            {
                entitySets = new MCMloEntitySet[centitySets.Length];
                for (int i = 0; i < centitySets.Length; i++)
                {
                    entitySets[i] = new MCMloEntitySet(meta, centitySets[i]);
                }
            }


            timeCycleModifiers = MetaTypes.ConvertDataArray<CMloTimeCycleModifier>(meta, MetaName.CMloTimeCycleModifier, _MloArchetypeDefData.timeCycleModifiers);

        }

    }



    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MloInstanceData
    {
        public YmapEntityDef Owner { get; set; }
        public CMloInstanceDef _Instance;
        public CMloInstanceDef Instance { get { return _Instance; } set { _Instance = value; } }
        public uint[] defaultEntitySets { get; set; }

        public YmapEntityDef[] Entities { get; set; }


        public void CreateYmapEntities(YmapEntityDef owner, MloArchetype mloa)
        {
            Owner = owner;
            if (owner == null) return;
            if (mloa.entities == null) return;
            var ec = mloa.entities.Length;

            var entlist = new List<YmapEntityDef>();
            for (int i = 0; i < ec; i++)
            {
                // Create the entity in the mlo, if it already exists it will not
                // be added to the mlo's entity list but it will be added to InstancedEntities.
                YmapEntityDef e = CreateEntity(owner, mloa, mloa.entities[i], i, 0);
                entlist.Add(e);
            }

            int lasti = ec;

            var entitySets = mloa.entitySets;
            if (entitySets != null)
            {
                for (int i = 0; i < entitySets.Length; i++)
                {
                    var entitySet = entitySets[i];
                    if (entitySet.Entities != null)
                    {
                        for (int j = 0; j < entitySet.Entities.Length; j++)
                        {
                            YmapEntityDef e = CreateYmapEntity(owner, entitySet.Entities[j], lasti);
                            e.MloEntitySet = entitySet;
                            entlist.Add(e);
                            lasti++;
                        }
                    }
                }
            }

            if (defaultEntitySets != null)
            {
                // todo: need to add support for this!!
            }

            Entities = entlist.ToArray();
        }

        public void InitYmapEntityArchetypes(GameFileCache gfc)
        {
            var arch = Owner.Archetype;

            if (Entities != null)
            {
                for (int j = 0; j < Entities.Length; j++)
                {
                    var ient = Entities[j];
                    var iarch = gfc.GetArchetype(ient.CEntityDef.archetypeName);
                    ient.SetArchetype(iarch);

                    if (iarch == null)
                    { } //can't find archetype - des stuff eg {des_prologue_door}
                }


                //update archetype room AABB's.. bad to have this here? where else to put it?
                var mloa = arch as MloArchetype;
                if (mloa != null)
                {
                    Vector3 mlobbmin = Vector3.Zero;
                    Vector3 mlobbmax = Vector3.Zero;
                    Vector3[] c = new Vector3[8];
                    var rooms = mloa.rooms;
                    if (rooms != null)
                    {
                        for (int j = 0; j < rooms.Length; j++)
                        {
                            var room = rooms[j];
                            if ((room.AttachedObjects == null) || (room.AttachedObjects.Length == 0)) continue;
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
                                            Vector3 corn = ori.Multiply(c[n]) + pos;
                                            min = Vector3.Min(min, corn);
                                            max = Vector3.Max(max, corn);
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
        }

        public bool DeleteEntity(YmapEntityDef ent)
        {
            if (Entities == null)
            {
                throw new NullReferenceException("The Entities list returned null in our MloInstanceData. This could be an issue with initialization. The MloInstance probably doesn't exist.");
            }

            if (ent.Index >= Entities.Length)
            {
                throw new ArgumentOutOfRangeException("The index of the entity was greater than the amount of entities that exist in this MloInstance. Likely an issue with initializion.");
            }

            int index = 0;
            YmapEntityDef[] newentities = new YmapEntityDef[Entities.Length - 1];
            YmapEntityDef delentity = Entities[ent.Index];
            bool del = false;

            for (int i = 0; i < Entities.Length; i++)
            {
                if (Entities[i] == delentity)
                {
                    del = true;
                    continue;
                }
                newentities[index] = Entities[i];
                newentities[index].Index = index;
                index++;
            }
            if (!del)
                throw new ArgumentException("The entity specified was not found in this MloInstance. It cannot be deleted.");

            if (Owner.Archetype is MloArchetype arch)
            {
                if (arch.RemoveEntity(ent))
                {
                    // Delete was successful...
                    Entities = newentities;
                    return true;
                }
            }
            throw new InvalidCastException("The owner of this archetype's archetype definition is not an MloArchetype.");
        }

        public YmapEntityDef CreateEntity(YmapEntityDef mlo, MloArchetype mloa, MCEntityDef ment, int index, int roomIndex)
        {
            YmapEntityDef e = CreateYmapEntity(mlo, ment, index);
            mloa.AddEntity(e, roomIndex);
            return e;
        }

        private YmapEntityDef CreateYmapEntity(YmapEntityDef owner, MCEntityDef ment, int i)
        {
            YmapEntityDef e = new YmapEntityDef(null, i, ref ment._Data);
            e.Extensions = ment.Extensions;
            e.MloRefPosition = e.Position;
            e.MloRefOrientation = e.Orientation;
            e.MloParent = owner;
            e.Position = owner.Position + owner.Orientation.Multiply(e.MloRefPosition);
            e.Orientation = Quaternion.Multiply(owner.Orientation, e.MloRefOrientation);
            e.UpdateWidgetPosition();
            e.UpdateWidgetOrientation();

            return e;
        }

        public MCEntityDef TryGetArchetypeEntity(YmapEntityDef ymapEntity)
        {
            if (ymapEntity == null) return null;
            if (Owner?.Archetype == null) return null;
            if (!(Owner.Archetype is MloArchetype mloa)) return null;
            if (ymapEntity.Index >= mloa.entities.Length) return null;

            var entity = mloa.entities[ymapEntity.Index];
            return entity;
        }

        public YmapEntityDef TryGetYmapEntity(MCEntityDef mcEntity)
        {
            if (mcEntity == null) return null;
            if (Owner?.Archetype == null) return null;
            if (!(Owner.Archetype is MloArchetype mloa)) return null;

            var index = Array.FindIndex(mloa.entities, x => x == mcEntity);
            if (index == -1 || index >= Entities.Length) return null;
            return Entities[index];
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
            if (Entities == null) return;
            if (Owner == null) return;

            for (int i = 0; i < Entities.Length; i++)
            {
                YmapEntityDef e = Entities[i];
                UpdateEntity(e);
            }

        }

        public void UpdateEntity(YmapEntityDef e)
        {
            e.Position = Owner.Position + Owner.Orientation.Multiply(e.MloRefPosition);
            e.Orientation = Quaternion.Multiply(Owner.Orientation, e.MloRefOrientation);
            e.UpdateWidgetPosition();
            e.UpdateWidgetOrientation();
        }

        public void AddEntity(YmapEntityDef e)
        {
            if (e == null) return;
            if (Entities == null) Entities = new YmapEntityDef[0];
            var entities = Entities.ToList();
            entities.Add(e);
            Entities = entities.ToArray();
        }



    }


}
