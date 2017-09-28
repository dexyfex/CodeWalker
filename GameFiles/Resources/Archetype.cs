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

        public CEntityDef[] entities { get; set; }
        public CMloRoomDef[] rooms { get; set; }
        public CMloPortalDef[] portals { get; set; }
        public CMloEntitySet[] entitySets { get; set; }
        public CMloTimeCycleModifier[] timeCycleModifiers { get; set; }

        public void Init(YtypFile ytyp, ref CMloArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            MloArchetypeDef = arch.MloArchetypeDef;
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MloInstanceData
    {
        public YmapEntityDef Owner { get; set; }
        public CMloInstanceDef _Instance;
        public CMloInstanceDef Instance { get { return _Instance; } set { _Instance = value; } }
        public uint[] Unk_1407157833 { get; set; }

        public YmapEntityDef[] Entities { get; set; }


        public void CreateYmapEntities(YmapEntityDef owner, MloArchetype mloa)
        {
            Owner = owner;
            if (owner == null) return;
            if (mloa.entities == null) return;
            var ec = mloa.entities.Length;
            Entities = new YmapEntityDef[ec];
            for (int i = 0; i < ec; i++)
            {
                YmapEntityDef e = new YmapEntityDef(null, i, ref mloa.entities[i]);
                e.MloRefPosition = e.Position;
                e.MloRefOrientation = e.Orientation;
                e.MloParent = owner;
                e.Position = owner.Position + owner.Orientation.Multiply(e.MloRefPosition);
                e.Orientation = Quaternion.Multiply(owner.Orientation, e.MloRefOrientation);
                e.UpdateWidgetPosition();
                e.UpdateWidgetOrientation();
                Entities[i] = e;
            }
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
