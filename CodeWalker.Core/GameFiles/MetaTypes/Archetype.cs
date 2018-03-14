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

        public CTimeArchetypeDefData _TimeArchetypeDef;
        public CTimeArchetypeDefData TimeArchetypeDef { get { return _TimeArchetypeDef; } set { _TimeArchetypeDef = value; } }


        public uint TimeFlags { get; set; }
        public bool[] ActiveHours { get; set; }
        public string[] ActiveHoursText { get; set; }
        public bool ExtraFlag { get; set; }


        public void Init(YtypFile ytyp, ref CTimeArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            TimeArchetypeDef = arch.TimeArchetypeDef;

            TimeFlags = _TimeArchetypeDef.timeFlags;
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

        public CMloArchetypeDefData _MloArchetypeDef;
        public CMloArchetypeDefData MloArchetypeDef { get { return _MloArchetypeDef; } set { _MloArchetypeDef = value; } }

        public MCEntityDef[] entities { get; set; }
        public MCMloRoomDef[] rooms { get; set; }
        public MCMloPortalDef[] portals { get; set; }
        public MCMloEntitySet[] entitySets { get; set; }
        public CMloTimeCycleModifier[] timeCycleModifiers { get; set; }

        public void Init(YtypFile ytyp, ref CMloArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            MloArchetypeDef = arch.MloArchetypeDef;
        }

        public void LoadChildren(Meta meta)
        {
            var centities = MetaTypes.ConvertDataArray<CEntityDef>(meta, MetaName.CEntityDef, _MloArchetypeDef.entities);
            if (centities != null)
            {
                entities = new MCEntityDef[centities.Length];
                for (int i = 0; i < centities.Length; i++)
                {
                    entities[i] = new MCEntityDef(meta, centities[i]);
                }
            }

            var crooms = MetaTypes.ConvertDataArray<CMloRoomDef>(meta, MetaName.CMloRoomDef, _MloArchetypeDef.rooms);
            if (crooms != null)
            {
                rooms = new MCMloRoomDef[crooms.Length];
                for (int i = 0; i < crooms.Length; i++)
                {
                    rooms[i] = new MCMloRoomDef(meta, crooms[i]);
                }
            }

            var cportals = MetaTypes.ConvertDataArray<CMloPortalDef>(meta, MetaName.CMloPortalDef, _MloArchetypeDef.portals);
            if (cportals != null)
            {
                portals = new MCMloPortalDef[cportals.Length];
                for (int i = 0; i < cportals.Length; i++)
                {
                    portals[i] = new MCMloPortalDef(meta, cportals[i]);
                }
            }

            var centitySets = MetaTypes.ConvertDataArray<CMloEntitySet>(meta, MetaName.CMloEntitySet, _MloArchetypeDef.entitySets);
            if (centitySets != null)
            {
                entitySets = new MCMloEntitySet[centitySets.Length];
                for (int i = 0; i < centitySets.Length; i++)
                {
                    entitySets[i] = new MCMloEntitySet(meta, centitySets[i]);
                }
            }


            timeCycleModifiers = MetaTypes.ConvertDataArray<CMloTimeCycleModifier>(meta, MetaName.CMloTimeCycleModifier, _MloArchetypeDef.timeCycleModifiers);

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
                YmapEntityDef e = CreateYmapEntity(owner, mloa.entities[i], i);
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
            }

            Entities = entlist.ToArray();
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
                e.Position = Owner.Position + Owner.Orientation.Multiply(e.MloRefPosition);
                e.Orientation = Quaternion.Multiply(Owner.Orientation, e.MloRefOrientation);
                e.UpdateWidgetPosition();
                e.UpdateWidgetOrientation();
            }

        }



    }


}
