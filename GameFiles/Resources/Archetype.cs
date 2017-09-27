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
        public MetaHash Hash { get; set; }
        public YtypFile Ytyp { get; set; }
        public CBaseArchetypeDef BaseArchetype { get; set; }
        public CTimeArchetypeDef TimeArchetype { get; set; }
        public CMloArchetypeDef MloArchetype { get; set; }
        public MetaHash DrawableDict { get; set; }
        public MetaHash TextureDict { get; set; }
        public MetaHash ClipDict { get; set; }
        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public Vector3 BSCenter { get; set; }
        public float BSRadius { get; set; }
        public bool IsTimeArchetype { get; set; }
        public bool IsMloArchetype { get; set; }
        public float LodDist { get; set; }
        public MloArchetypeData MloData { get; set; }
        public MetaWrapper[] Extensions { get; set; }
        public TimedArchetypeTimes Times { get; set; }


        public string Name
        {
            get
            {
                if (IsTimeArchetype) return TimeArchetype.BaseArchetypeDef.name.ToString();
                if (IsMloArchetype) return MloArchetype.BaseArchetypeDef.name.ToString();
                return BaseArchetype.name.ToString();
            }
        }
        public string AssetName
        {
            get
            {
                if (IsTimeArchetype) return TimeArchetype.BaseArchetypeDef.assetName.ToString();
                if (IsMloArchetype) return MloArchetype.BaseArchetypeDef.assetName.ToString();
                return BaseArchetype.assetName.ToString();
            }
        }

        private void InitVars(ref CBaseArchetypeDef arch)
        {
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
            BaseArchetype = arch;
            IsTimeArchetype = false;
            IsMloArchetype = false;
        }
        public void Init(YtypFile ytyp, ref CTimeArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            TimeArchetype = arch;
            IsTimeArchetype = true;
            IsMloArchetype = false;
            Times = new TimedArchetypeTimes(arch.TimeArchetypeDef.timeFlags);
        }
        public void Init(YtypFile ytyp, ref CMloArchetypeDef arch)
        {
            Ytyp = ytyp;
            InitVars(ref arch._BaseArchetypeDef);
            MloArchetype = arch;
            IsTimeArchetype = false;
            IsMloArchetype = true;
        }

        public bool IsActive(float hour)
        {
            if (Times == null) return true;
            //if (Times.ExtraFlag) hour -= 0.5f;
            //if (hour < 0.0f) hour += 24.0f;
            int h = ((int)hour) % 24;
            if ((h < 0) || (h > 23)) return true;
            return Times.ActiveHours[h];
        }

        public override string ToString()
        {
            if (IsTimeArchetype) return TimeArchetype.ToString();
            if (IsMloArchetype) return MloArchetype.ToString();
            return BaseArchetype.ToString();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MloArchetypeData
    {
        public CEntityDef[] entities { get; set; }
        public CMloRoomDef[] rooms { get; set; }
        public CMloPortalDef[] portals { get; set; }
        public CMloEntitySet[] entitySets { get; set; }
        public CMloTimeCycleModifier[] timeCycleModifiers { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MloInstanceData
    {
        public YmapEntityDef Owner { get; set; }
        public CMloInstanceDef _Instance;
        public CMloInstanceDef Instance { get { return _Instance; } set { _Instance = value; } }
        public uint[] Unk_1407157833 { get; set; }

        public YmapEntityDef[] Entities { get; set; }


        public void CreateYmapEntities(YmapEntityDef owner, MloArchetypeData mlod)
        {
            Owner = owner;
            if (owner == null) return;
            if (mlod.entities == null) return;
            var ec = mlod.entities.Length;
            Entities = new YmapEntityDef[ec];
            for (int i = 0; i < ec; i++)
            {
                YmapEntityDef e = new YmapEntityDef(null, i, ref mlod.entities[i]);
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


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TimedArchetypeTimes
    {
        public uint TimeFlags { get; set; }
        public bool[] ActiveHours { get; set; }
        public string[] ActiveHoursText { get; set; }
        public bool ExtraFlag { get; set; }

        public TimedArchetypeTimes(uint timeFlags)
        {
            TimeFlags = timeFlags;
            ActiveHours = new bool[24];
            ActiveHoursText = new string[24];
            for (int i = 0; i < 24; i++)
            {
                bool v = ((timeFlags >> i) & 1) == 1;
                ActiveHours[i] = v;

                int nxth = (i < 23) ? (i + 1) : 0;
                string hrs = string.Format("{0:00}:00 - {1:00}:00", i, nxth);
                ActiveHoursText[i] = (hrs + (v ? " - On" : " - Off"));
            }
            ExtraFlag = ((timeFlags >> 24) & 1) == 1;
        }

    }

}
