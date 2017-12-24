using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    public class AudioZones // : BasePathData
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        //public Vector4[] GetNodePositions()
        //{
        //    return null;
        //}
        //public EditorVertex[] GetPathVertices()
        //{
        //    return null;
        //}
        //public EditorVertex[] GetTriangleVertices()
        //{
        //    return TriangleVerts;
        //}
        //public EditorVertex[] TriangleVerts;

        public List<AudioPlacement> Zones = new List<AudioPlacement>();
        public List<AudioPlacement> Emitters = new List<AudioPlacement>();
        public List<AudioPlacement> AllItems = new List<AudioPlacement>();



        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            Inited = false;

            GameFileCache = gameFileCache;

            var rpfman = gameFileCache.RpfMan;


            Zones.Clear();
            Emitters.Clear();
            AllItems.Clear();


            Dictionary<uint, RpfFileEntry> datrelentries = new Dictionary<uint, RpfFileEntry>();
            var audrpf = rpfman.FindRpfFile("x64\\audio\\audio_rel.rpf");
            if (audrpf != null)
            {
                AddRpfDatRels(audrpf, datrelentries);
            }

            if (gameFileCache.EnableDlc)
            {
                var updrpf = rpfman.FindRpfFile("update\\update.rpf");
                if (updrpf != null)
                {
                    AddRpfDatRels(updrpf, datrelentries);
                }
                foreach (var dlcrpf in GameFileCache.DlcActiveRpfs) //load from current dlc rpfs
                {
                    AddRpfDatRels(dlcrpf, datrelentries);
                }
            }


            foreach (var dat151entry in datrelentries.Values)
            {
                var relfile = rpfman.GetFile<RelFile>(dat151entry);
                if (relfile != null)
                {
                    foreach (var reldata in relfile.RelDatas)
                    {
                        if (reldata is Dat151AmbientZone)
                        {
                            Zones.Add(new AudioPlacement(relfile, reldata as Dat151AmbientZone));
                        }
                        else if (reldata is Dat151AmbientEmitter)
                        {
                            Emitters.Add(new AudioPlacement(relfile, reldata as Dat151AmbientEmitter));
                        }
                    }
                }
            }

            AllItems.AddRange(Zones);
            AllItems.AddRange(Emitters);

            Inited = true;
        }



        private void AddRpfDatRels(RpfFile rpffile, Dictionary<uint, RpfFileEntry> datrelentries)
        {
            if (rpffile.AllEntries == null) return;
            foreach (var entry in rpffile.AllEntries)
            {
                if (entry is RpfFileEntry)
                {
                    RpfFileEntry fentry = entry as RpfFileEntry;
                    //if (entry.NameLower.EndsWith(".rel"))
                    //{
                    //    datrelentries[entry.NameHash] = fentry;
                    //}
                    if (entry.NameLower.EndsWith(".dat54.rel"))
                    {
                        datrelentries[entry.NameHash] = fentry;
                    }
                    if (entry.NameLower.EndsWith(".dat151.rel"))
                    {
                        datrelentries[entry.NameHash] = fentry;
                    }
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
        public Dat151AmbientEmitter AudioEmitter { get; set; }
        public Dat151ZoneShape Shape { get; set; }
        public string ShortTypeName { get; set; }
        public string FullTypeName { get; set; }
        public Vector3 InnerPos { get; set; }
        public Vector3 InnerMin { get; set; }
        public Vector3 InnerMax { get; set; }
        public float InnerRad { get; set; }
        public Quaternion InnerOri { get; set; }
        public Vector3 OuterPos { get; set; }
        public Vector3 OuterMin { get; set; }
        public Vector3 OuterMax { get; set; }
        public float OuterRad { get; set; }
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
            Shape = zone.Shape;
            ShortTypeName = "AudioZone";
            FullTypeName = "Audio Zone";
            Name = zone.Name;
            NameHash = zone.NameHash;

            float deg2rad = (float)(Math.PI / 180.0);

            switch (zone.Shape)
            {
                case Dat151ZoneShape.Box:
                    InnerPos = zone.InnerPos;
                    InnerMax = zone.InnerSize * 0.5f;
                    InnerMin = -InnerMax;
                    InnerOri = Quaternion.RotationAxis(Vector3.UnitZ, zone.InnerAngle * deg2rad);
                    break;
                case Dat151ZoneShape.Sphere:
                    InnerPos = zone.InnerPos;
                    InnerOri = Quaternion.Identity;
                    InnerRad = zone.InnerSize.X;
                    OuterRad = zone.OuterSize.X;
                    break;
                case Dat151ZoneShape.Line:
                    InnerPos = zone.InnerPos;
                    InnerMin = new Vector3(-1.0f, -1.0f, 0.0f);
                    InnerMax = new Vector3(1.0f, 1.0f, (zone.InnerSize - zone.InnerPos).Length());
                    InnerOri = Quaternion.Invert(Quaternion.LookAtLH(zone.InnerPos, zone.InnerSize, Vector3.UnitZ));
                    break;
            }

            OuterPos = zone.OuterPos;
            OuterMax = zone.OuterSize * 0.5f;
            OuterMin = -OuterMax;
            OuterOri = Quaternion.RotationAxis(Vector3.UnitZ, zone.OuterAngle * deg2rad);

            bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
            if (useouter && (zone.Shape != Dat151ZoneShape.Sphere))
            { } //not sure what these are yet!
            Position = useouter ? OuterPos : InnerPos;
            HitboxMax = useouter ? OuterMax : InnerMax;
            HitboxMin = useouter ? OuterMin : InnerMin;
            Orientation = useouter ? OuterOri : InnerOri;
            OrientationInv = Quaternion.Invert(Orientation);
            HitSphereRad = InnerRad;
            if (zone.Shape == Dat151ZoneShape.Sphere)
            {
                Position = InnerPos;
            }
        }
        public AudioPlacement(RelFile rel, Dat151AmbientEmitter emitter)
        {
            RelFile = rel;
            AudioEmitter = emitter;
            Shape = Dat151ZoneShape.Sphere;
            ShortTypeName = "AudioEmitter";
            FullTypeName = "Audio Emitter";
            Name = emitter.Name;
            NameHash = emitter.NameHash;

            Orientation = Quaternion.Identity;
            OrientationInv = Quaternion.Identity;
            InnerPos = emitter.Position;
            OuterPos = InnerPos;
            InnerRad = emitter.InnerRad;
            OuterRad = emitter.OuterRad;

            bool useouter = (InnerRad == 0);
            if (useouter)
            {
                InnerRad = 1;
            }
            Position = InnerPos;
            HitSphereRad = InnerRad;// useouter ? OuterRad : InnerRad;
        }


        public void SetPosition(Vector3 pos)
        {
            bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
            Vector3 delta = pos - InnerPos;
            InnerPos = pos;
            OuterPos += delta;
            Position = useouter ? OuterPos : InnerPos;
        }
        public void SetOrientation(Quaternion ori)
        {
            Orientation = ori;
            OrientationInv = Quaternion.Invert(ori);

            if (InnerOri == OuterOri)
            {
                InnerOri = Orientation;
                OuterOri = Orientation;
            }
            else
            {
                //not sure yet how to allow independent rotation of inner & outer boxes...
                //maybe only in project window?
                bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
                if (useouter)
                {
                    OuterOri = Orientation;
                }
                else
                {
                    InnerOri = Orientation;
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
