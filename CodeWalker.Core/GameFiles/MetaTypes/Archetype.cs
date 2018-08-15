using SharpDX;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Archetype
    {
        public virtual MetaName Type => MetaName.CBaseArchetypeDef;

        public CBaseArchetypeDef _BaseArchetypeDef;
        public CBaseArchetypeDef BaseArchetypeDef { get { return _BaseArchetypeDef; } set { _BaseArchetypeDef = value; } }

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
            BaseArchetypeDef = arch;
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
        public CTimeArchetypeDef TimeArchetypeDef { get { return _TimeArchetypeDef; } set { _TimeArchetypeDef = value; } }

        public CTimeArchetypeDefData _TimeArchetypeDefData;
        public CTimeArchetypeDefData TimeArchetypeDefData { get { return _TimeArchetypeDefData; } set { _TimeArchetypeDefData = value; } }


        public uint TimeFlags { get; set; }
        public bool[] ActiveHours { get; set; }
        public string[] ActiveHoursText { get; set; }
        public bool ExtraFlag { get; set; }


        public void Init(YtypFile ytyp, ref CTimeArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);

            TimeArchetypeDef = arch;
            TimeArchetypeDefData = arch.TimeArchetypeDef;

            TimeFlags = _TimeArchetypeDefData.timeFlags;
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

        public CMloArchetypeDefData _MloArchetypeDefData;
        public CMloArchetypeDefData MloArchetypeDefData { get { return _MloArchetypeDefData; } set { _MloArchetypeDefData = value; } }

        public CMloArchetypeDef _MloArchetypeDef;
        public CMloArchetypeDef MloArchetypeDef { get { return _MloArchetypeDef; } set { _MloArchetypeDef = value; } }

        public MCEntityDef[] entities { get; set; }
        public MCMloRoomDef[] rooms { get; set; }
        public MCMloPortalDef[] portals { get; set; }
        public MCMloEntitySet[] entitySets { get; set; }
        public CMloTimeCycleModifier[] timeCycleModifiers { get; set; }

        public YmapEntityDef EntityDef { get; set; }
        public List<YmapEntityDef> InstancedEntities { get; set; }

        public void Init(YtypFile ytyp, ref CMloArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            MloArchetypeDef = arch;
            MloArchetypeDefData = arch.MloArchetypeDef;
        }

        public void AddEntity(YmapEntityDef ent, int roomIndex)
        {
            if (InstancedEntities == null)
                InstancedEntities = new List<YmapEntityDef>();

            if (InstancedEntities.Contains(ent))
                return;

            InstancedEntities.Add(ent);

            MCEntityDef entDef = entities.FirstOrDefault(x => x.EntityInstance == ent);
            if (entDef != null) return;
            if (roomIndex > rooms.Length)
            {
                MessageBox.Show($@"Room index {roomIndex} does not exist in {Name}.");
                return;
            }

            MCEntityDef mcEntityDef;
            if (ent.MloEntityDef != null)
            {
                mcEntityDef = ent.MloEntityDef;
            }
            else
            {
                mcEntityDef = new MCEntityDef(ref ent._CEntityDef, this, ent);
                ent.MloEntityDef = mcEntityDef;
            }

            List<MCEntityDef> entList = entities.ToList();
            entList.Add(mcEntityDef);
            ent.Index = entList.IndexOf(mcEntityDef);
            entities = entList.ToArray();

            var attachedObjs = rooms[roomIndex].AttachedObjects?.ToList() ?? new List<uint>();
            attachedObjs.Add((uint)ent.Index);
            rooms[roomIndex].AttachedObjects = attachedObjs.ToArray();
        }

        public bool RemoveEntity(YmapEntityDef ent)
        {
            if (InstancedEntities == null)
                return false;

            if (!InstancedEntities.Contains(ent))
                return false;

            InstancedEntities.Remove(ent);

            if (ent.Index >= entities.Length) return false;

            MCEntityDef delent = entities[ent.Index];
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
                    newentities[index].EntityInstance.Index = index;
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
                entlist.Add(CreateEntity(owner, mloa, mloa.entities[i], i, 0));
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

        public bool DeleteEntity(YmapEntityDef ent)
        {
            if (Entities == null)
                return false;

            if (ent.Index >= Entities.Length)
                return false;

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
            if (del)
            {
                if (Owner.Archetype is MloArchetype arch)
                {
                    if (arch.RemoveEntity(ent))
                    {
                        // Delete was successful...
                        Entities = newentities;
                        return true;
                    }
                }
            }
            return false;
        }

        public YmapEntityDef CreateEntity(YmapEntityDef mlo, MloArchetype mloa, MCEntityDef ment, int i, int roomIndex)
        {
            YmapEntityDef e = CreateYmapEntity(mlo, ment, i);
            mloa.AddEntity(e, roomIndex);
            return e;
        }

        private YmapEntityDef CreateYmapEntity(YmapEntityDef owner, MCEntityDef ment, int i)
        {
            YmapEntityDef e = new YmapEntityDef(null, i, ref ment._Data);
            e.MloEntityDef = ment;
            e.Extensions = ment.Extensions;
            e.MloRefPosition = e.Position;
            e.MloRefOrientation = e.Orientation;
            e.MloParent = owner;
            e.Position = owner.Position + owner.Orientation.Multiply(e.MloRefPosition);
            e.Orientation = Quaternion.Multiply(owner.Orientation, e.MloRefOrientation);
            e.UpdateWidgetPosition();
            e.UpdateWidgetOrientation();

            ment.EntityInstance = e; // set the entity instance
            return e;
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
