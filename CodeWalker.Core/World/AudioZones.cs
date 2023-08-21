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

        public List<AudioPlacement> Zones = new List<AudioPlacement>();
        public List<AudioPlacement> Emitters = new List<AudioPlacement>();
        public List<AudioPlacement> AllItems = new List<AudioPlacement>();

        public Dictionary<RelFile, AudioPlacement[]> PlacementsDict = new Dictionary<RelFile, AudioPlacement[]>();


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            Inited = false;

            GameFileCache = gameFileCache;

            Zones.Clear();
            Emitters.Clear();
            AllItems.Clear();


            List<AudioPlacement> placements = new List<AudioPlacement>();

            foreach (var relfile in GameFileCache.AudioDatRelFiles)
            {
                if (relfile == null) continue;

                placements.Clear();

                CreatePlacements(relfile, placements, true);

                PlacementsDict[relfile] = placements.ToArray();
            }

            AllItems.AddRange(Zones);
            AllItems.AddRange(Emitters);

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
                    if (addtoLists) Zones.Add(placement);
                }
                else if (reldata is Dat151AmbientRule)
                {
                    placement = new AudioPlacement(relfile, reldata as Dat151AmbientRule);
                    if (addtoLists) Emitters.Add(placement);
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


    }



    public class AudioPlacement
    {
        public string Name { get; set; }
        public MetaHash NameHash { get; set; }
        public RelFile RelFile { get; set; }
        public Dat151AmbientZone AudioZone { get; set; }
        public Dat151AmbientRule AudioEmitter { get; set; }
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
            AudioZone = zone;
            ShortTypeName = "AudioZone";
            FullTypeName = "Audio Zone";

            UpdateFromZone();
        }
        public AudioPlacement(RelFile rel, Dat151AmbientRule emitter)
        {
            RelFile = rel;
            AudioEmitter = emitter;
            ShortTypeName = "AudioEmitter";
            FullTypeName = "Audio Emitter";

            UpdateFromEmitter();
        }


        public void UpdateFromZone()
        {
            if (AudioZone == null) return;
            var zone = AudioZone;

            Name = zone.Name;
            NameHash = zone.NameHash;
            Shape = zone.Shape;

            float deg2rad = (float)(Math.PI / 180.0);

            switch (zone.Shape)
            {
                case Dat151ZoneShape.Box:
                    InnerPos = zone.PlaybackZonePosition;
                    InnerMax = zone.PlaybackZoneSize * 0.5f;
                    InnerMin = -InnerMax;
                    InnerOri = Quaternion.RotationAxis(Vector3.UnitZ, zone.PlaybackZoneAngle * deg2rad);
                    break;
                case Dat151ZoneShape.Sphere:
                    InnerPos = zone.PlaybackZonePosition;
                    InnerOri = Quaternion.Identity;
                    InnerRadius = zone.PlaybackZoneSize.X;
                    OuterRadius = zone.ActivationZoneSize.X;
                    break;
                case Dat151ZoneShape.Line:
                    InnerPos = zone.PlaybackZonePosition;
                    InnerMin = new Vector3(-1.0f, -1.0f, 0.0f);
                    InnerMax = new Vector3(1.0f, 1.0f, (zone.PlaybackZoneSize - zone.PlaybackZonePosition).Length());
                    InnerOri = Quaternion.Invert(Quaternion.LookAtLH(zone.PlaybackZonePosition, zone.PlaybackZoneSize, Vector3.UnitZ));
                    break;
            }

            OuterPos = zone.ActivationZonePosition;
            OuterMax = zone.ActivationZoneSize * 0.5f;
            OuterMin = -OuterMax;
            OuterOri = Quaternion.RotationAxis(Vector3.UnitZ, zone.ActivationZoneAngle * deg2rad);

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

        public void UpdateFromEmitter()
        {
            if (AudioEmitter == null) return;
            var emitter = AudioEmitter;

            Name = emitter.Name;
            NameHash = emitter.NameHash;
            Shape = Dat151ZoneShape.Sphere;

            Orientation = Quaternion.Identity;
            OrientationInv = Quaternion.Identity;
            InnerPos = emitter.Position;
            OuterPos = InnerPos;
            InnerRadius = emitter.InnerRadius;
            OuterRadius = emitter.OuterRadius;

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

            if (AudioZone != null)
            {
                AudioZone.PlaybackZonePosition = InnerPos;
                AudioZone.ActivationZonePosition = OuterPos;
            }
            if (AudioEmitter != null)
            {
                AudioEmitter.Position = InnerPos;
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
                if (AudioZone != null)
                {
                    AudioZone.PlaybackZoneAngle = uangl;
                    AudioZone.ActivationZoneAngle = uangl;
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
                    if (AudioZone != null)
                    {
                        AudioZone.ActivationZoneAngle = uangl;
                    }
                }
                else
                {
                    InnerOri = Orientation;
                    if (AudioZone != null)
                    {
                        AudioZone.PlaybackZoneAngle = uangl;
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
