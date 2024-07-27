using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    public class AudioZones
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        public Dictionary<RelFile, AudioPlacement[]> PlacementsDict = new Dictionary<RelFile, AudioPlacement[]>();


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            Inited = false;

            GameFileCache = gameFileCache;


            List<AudioPlacement> placements = new List<AudioPlacement>();

            foreach (var relfile in GameFileCache.AudioDatRelFiles)
            {
                if (relfile == null) continue;

                placements.Clear();

                CreatePlacements(relfile, placements, true);

                PlacementsDict[relfile] = placements.ToArray();
            }

            Inited = true;
        }


        private void CreatePlacements(RelFile relfile, List<AudioPlacement> placements, bool addtoLists = false)
        {
            foreach (var reldata in relfile.RelDatas)
            {
                AudioPlacement placement = null;
                if (reldata is Dat151AmbientZone)
                {
                    placement = new AudioPlacement(relfile, reldata as Dat151AmbientZone);
                }
                else if (reldata is Dat151AmbientRule)
                {
                    placement = new AudioPlacement(relfile, reldata as Dat151AmbientRule);
                }
                else if (reldata is Dat151StaticEmitter)
                {
                    placement = new AudioPlacement(relfile, reldata as Dat151StaticEmitter);
                }
                if (placement != null)
                {
                    placements.Add(placement);
                }
            }
        }


        public void GetPlacements(List<RelFile> relfiles, List<AudioPlacement> placements)
        {

            foreach (var relfile in relfiles)
            {
                AudioPlacement[] fileplacements = null;
                if (!PlacementsDict.TryGetValue(relfile, out fileplacements))
                {
                    List<AudioPlacement> newplacements = new List<AudioPlacement>();
                    CreatePlacements(relfile, newplacements);
                    fileplacements = newplacements.ToArray();
                    PlacementsDict[relfile] = fileplacements;
                }
                if (fileplacements != null)
                {
                    placements.AddRange(fileplacements);
                }
            }

        }

        public AudioPlacement FindPlacement(RelFile relfile, Dat151RelData reldata)
        {
            if (relfile == null) return null;
            if (reldata == null) return null;
            if (PlacementsDict.TryGetValue(relfile, out var placements))
            {
                foreach (var placement in placements)
                {
                    if (placement.AmbientZone == reldata) return placement;
                    if (placement.AmbientRule == reldata) return placement;
                    if (placement.StaticEmitter == reldata) return placement;
                }
            }
            return null;
        }


    }



    public class AudioPlacement
    {
        public string Name { get; set; }
        public MetaHash NameHash { get; set; }
        public RelFile RelFile { get; set; }
        public Dat151AmbientZone AmbientZone { get; set; }
        public Dat151AmbientRule AmbientRule { get; set; }
        public Dat151StaticEmitter StaticEmitter { get; set; }
        public Dat151ZoneShape Shape { get; set; }
        public string ShortTypeName { get; set; }
        public string FullTypeName { get; set; }
        public Vector3 InnerPos { get; set; }
        public Vector3 InnerMin { get; set; }
        public Vector3 InnerMax { get; set; }
        public float InnerRadius { get; set; }
        public Quaternion InnerOri { get; set; }
        public Vector3 OuterPos { get; set; }
        public Vector3 OuterMin { get; set; }
        public Vector3 OuterMax { get; set; }
        public float OuterRadius { get; set; }
        public Quaternion OuterOri { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 HitboxMin { get; set; }
        public Vector3 HitboxMax { get; set; }
        public Quaternion Orientation { get; set; }
        public Quaternion OrientationInv { get; set; }
        public float HitSphereRad { get; set; }


        public AudioPlacement(RelFile rel, Dat151AmbientZone zone)
        {
            RelFile = rel;
            AmbientZone = zone;
            ShortTypeName = "AmbientZone";
            FullTypeName = "Ambient Zone";

            UpdateFromAmbientZone();
        }
        public AudioPlacement(RelFile rel, Dat151AmbientRule rule)
        {
            RelFile = rel;
            AmbientRule = rule;
            ShortTypeName = "AmbientRule";
            FullTypeName = "Ambient Rule";

            UpdateFromAmbientRule();
        }
        public AudioPlacement(RelFile rel, Dat151StaticEmitter emitter)
        {
            RelFile = rel;
            StaticEmitter = emitter;
            ShortTypeName = "StaticEmitter";
            FullTypeName = "Static Emitter";

            UpdateFromStaticEmitter();
        }



        public void UpdateFromAmbientZone()
        {
            if (AmbientZone == null) return;
            var zone = AmbientZone;

            Name = zone.Name;
            NameHash = zone.NameHash;
            Shape = zone.Shape;

            float deg2rad = (float)(Math.PI / 180.0);

            switch (zone.Shape)
            {
                case Dat151ZoneShape.Box:
                    InnerPos = zone.PositioningZoneCentre;
                    InnerMax = zone.PositioningZoneSize * 0.5f;
                    InnerMin = -InnerMax;
                    InnerOri = Quaternion.RotationAxis(Vector3.UnitZ, zone.PositioningZoneRotationAngle * deg2rad);
                    break;
                case Dat151ZoneShape.Sphere:
                    InnerPos = zone.PositioningZoneCentre;
                    InnerOri = Quaternion.Identity;
                    InnerRadius = zone.PositioningZoneSize.X;
                    OuterRadius = zone.ActivationZoneSize.X;
                    break;
                case Dat151ZoneShape.Line:
                    InnerPos = zone.PositioningZoneCentre;
                    InnerMin = new Vector3(-1.0f, -1.0f, 0.0f);
                    InnerMax = new Vector3(1.0f, 1.0f, (zone.PositioningZoneSize - zone.PositioningZoneCentre).Length());
                    InnerOri = Quaternion.Invert(Quaternion.LookAtLH(zone.PositioningZoneCentre, zone.PositioningZoneSize, Vector3.UnitZ));
                    break;
            }

            OuterPos = zone.ActivationZoneCentre;
            OuterMax = zone.ActivationZoneSize * 0.5f;
            OuterMin = -OuterMax;
            OuterOri = Quaternion.RotationAxis(Vector3.UnitZ, zone.ActivationZoneRotationAngle * deg2rad);

            bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
            if (useouter && (zone.Shape != Dat151ZoneShape.Sphere))
            { } //not sure what these are yet!
            Position = useouter ? OuterPos : InnerPos;
            HitboxMax = useouter ? OuterMax : InnerMax;
            HitboxMin = useouter ? OuterMin : InnerMin;
            Orientation = useouter ? OuterOri : InnerOri;
            OrientationInv = Quaternion.Invert(Orientation);
            HitSphereRad = InnerRadius;
            if (zone.Shape == Dat151ZoneShape.Sphere)
            {
                Position = InnerPos;
            }

        }

        public void UpdateFromAmbientRule()
        {
            if (AmbientRule == null) return;
            var rule = AmbientRule;

            Name = rule.Name;
            NameHash = rule.NameHash;
            Shape = Dat151ZoneShape.Sphere;

            Orientation = Quaternion.Identity;
            OrientationInv = Quaternion.Identity;
            InnerPos = rule.Position;
            OuterPos = InnerPos;
            InnerRadius = rule.MinDist;
            OuterRadius = rule.MaxDist;

            bool useouter = (InnerRadius == 0);
            if (useouter)
            {
                InnerRadius = 1;
            }
            Position = InnerPos;
            HitSphereRad = InnerRadius;// useouter ? OuterRadius : InnerRadius;

        }

        public void UpdateFromStaticEmitter()
        {
            if (StaticEmitter == null) return;
            var emitter = StaticEmitter;

            Name = emitter.Name;
            NameHash = emitter.NameHash;
            Shape = Dat151ZoneShape.Sphere;

            Orientation = Quaternion.Identity;
            OrientationInv = Quaternion.Identity;
            InnerPos = emitter.Position;
            OuterPos = InnerPos;
            InnerRadius = emitter.MinDistance;
            OuterRadius = emitter.MaxDistance;

            bool useouter = (InnerRadius == 0);
            if (useouter)
            {
                InnerRadius = 1;
            }
            Position = InnerPos;
            HitSphereRad = InnerRadius;// useouter ? OuterRadius : InnerRadius;

        }


        public void SetPosition(Vector3 pos)
        {
            bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
            Vector3 delta = pos - InnerPos;
            InnerPos = pos;
            OuterPos += delta;
            Position = useouter ? OuterPos : InnerPos;

            if (AmbientZone != null)
            {
                AmbientZone.PositioningZoneCentre = InnerPos;
                AmbientZone.ActivationZoneCentre = OuterPos;
            }
            if (AmbientRule != null)
            {
                AmbientRule.Position = InnerPos;
            }
            if (StaticEmitter != null)
            {
                StaticEmitter.Position = InnerPos;
            }
        }
        public void SetOrientation(Quaternion ori)
        {
            Orientation = ori;
            OrientationInv = Quaternion.Invert(ori);

            Vector3 t = ori.Multiply(Vector3.UnitX);
            float angl = (float)Math.Atan2(t.Y, t.X);
            while (angl < 0) angl += ((float)Math.PI * 2.0f);
            float rad2deg = (float)(180.0 / Math.PI);
            float dangl = angl * rad2deg;
            uint uangl = (uint)dangl;


            if (InnerOri == OuterOri)
            {
                InnerOri = Orientation;
                OuterOri = Orientation;
                if (AmbientZone != null)
                {
                    AmbientZone.PositioningZoneRotationAngle = (ushort)uangl;
                    AmbientZone.ActivationZoneRotationAngle = (ushort)uangl;
                }
            }
            else
            {
                //not sure yet how to allow independent rotation of inner & outer boxes...
                //maybe only in project window?
                bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
                if (useouter)
                {
                    OuterOri = Orientation;
                    if (AmbientZone != null)
                    {
                        AmbientZone.ActivationZoneRotationAngle = (ushort)uangl;
                    }
                }
                else
                {
                    InnerOri = Orientation;
                    if (AmbientZone != null)
                    {
                        AmbientZone.PositioningZoneRotationAngle = (ushort)uangl;
                    }
                }
            }
        }


        public string GetNameString()
        {
            if (!string.IsNullOrEmpty(Name)) return Name;
            return NameHash.ToString();
        }
    }


}
