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
                if (IsTimeArchetype) return TimeArchetype.CBaseArchetypeDef.name.ToString();
                if (IsMloArchetype) return MloArchetype.CBaseArchetypeDef.name.ToString();
                return BaseArchetype.name.ToString();
            }
        }
        public string AssetName
        {
            get
            {
                if (IsTimeArchetype) return TimeArchetype.CBaseArchetypeDef.assetName.ToString();
                if (IsMloArchetype) return MloArchetype.CBaseArchetypeDef.assetName.ToString();
                return BaseArchetype.assetName.ToString();
            }
        }

        public void Init(YtypFile ytyp, CBaseArchetypeDef arch)
        {
            Hash = arch.assetName;
            if (Hash.Hash == 0) Hash = arch.name;
            Ytyp = ytyp;
            BaseArchetype = arch;
            DrawableDict = arch.drawableDictionary;
            TextureDict = arch.textureDictionary;
            ClipDict = arch.clipDictionary;
            BBMin = arch.bbMin;
            BBMax = arch.bbMax;
            BSCenter = arch.bsCentre;
            BSRadius = arch.bsRadius;
            IsTimeArchetype = false;
            IsMloArchetype = false;
            LodDist = arch.lodDist;
        }
        public void Init(YtypFile ytyp, CTimeArchetypeDef arch)
        {
            Hash = arch.CBaseArchetypeDef.assetName;
            if (Hash.Hash == 0) Hash = arch.CBaseArchetypeDef.name;
            Ytyp = ytyp;
            TimeArchetype = arch;
            DrawableDict = arch.CBaseArchetypeDef.drawableDictionary;
            TextureDict = arch.CBaseArchetypeDef.textureDictionary;
            ClipDict = arch.CBaseArchetypeDef.clipDictionary;
            BBMin = arch.CBaseArchetypeDef.bbMin;
            BBMax = arch.CBaseArchetypeDef.bbMax;
            BSCenter = arch.CBaseArchetypeDef.bsCentre;
            BSRadius = arch.CBaseArchetypeDef.bsRadius;
            IsTimeArchetype = true;
            IsMloArchetype = false;
            LodDist = arch.CBaseArchetypeDef.lodDist;
            Times = new TimedArchetypeTimes(arch.timeFlags);
        }
        public void Init(YtypFile ytyp, CMloArchetypeDef arch)
        {
            Hash = arch.CBaseArchetypeDef.assetName;
            if (Hash.Hash == 0) Hash = arch.CBaseArchetypeDef.name;
            Ytyp = ytyp;
            MloArchetype = arch;
            DrawableDict = arch.CBaseArchetypeDef.drawableDictionary;
            TextureDict = arch.CBaseArchetypeDef.textureDictionary;
            ClipDict = arch.CBaseArchetypeDef.clipDictionary;
            BBMin = arch.CBaseArchetypeDef.bbMin;
            BBMax = arch.CBaseArchetypeDef.bbMax;
            BSCenter = arch.CBaseArchetypeDef.bsCentre;
            BSRadius = arch.CBaseArchetypeDef.bsRadius;
            IsTimeArchetype = false;
            IsMloArchetype = true;
            LodDist = arch.CBaseArchetypeDef.lodDist;
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
    public class MloEntityData
    {
        public YmapEntityDef[] AllEntities { get; set; }

        public void CreateYmapEntities(YmapEntityDef owner, MloArchetypeData mlod)
        {
            if (owner == null) return;
            if (mlod.entities == null) return;
            AllEntities = new YmapEntityDef[mlod.entities.Length];
            for (int i = 0; i < mlod.entities.Length; i++)
            {
                YmapEntityDef e = new YmapEntityDef(null, i, ref mlod.entities[i]);

                e.MloParent = owner;
                e.Position = owner.Position + owner.Orientation.Multiply(e.Position);
                e.Orientation = Quaternion.Multiply(owner.Orientation, e.Orientation);

                e.UpdateWidgetPosition();
                e.UpdateWidgetOrientation();

                if ((owner.Orientation != Quaternion.Identity)&&(owner.Orientation.Z!=1.0f))
                { }

                AllEntities[i] = e;
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
