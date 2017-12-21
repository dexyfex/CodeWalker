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


            Dictionary<uint, RpfFileEntry> dat151entries = new Dictionary<uint, RpfFileEntry>();
            var audrpf = rpfman.FindRpfFile("x64\\audio\\audio_rel.rpf");
            if (audrpf != null)
            {
                AddRpfDat151s(audrpf, dat151entries);
            }

            if (gameFileCache.EnableDlc)
            {
                var updrpf = rpfman.FindRpfFile("update\\update.rpf");
                if (updrpf != null)
                {
                    AddRpfDat151s(updrpf, dat151entries);
                }
                foreach (var dlcrpf in GameFileCache.DlcActiveRpfs) //load from current dlc rpfs
                {
                    AddRpfDat151s(dlcrpf, dat151entries);
                }
            }


            foreach (var dat151entry in dat151entries.Values)
            {
                var relfile = rpfman.GetFile<RelFile>(dat151entry);
                if (relfile != null)
                {
                    foreach (var reldata in relfile.RelDatas)
                    {
                        if (reldata is Dat151Unk37)
                        {
                            Zones.Add(new AudioPlacement(relfile, reldata as Dat151Unk37));
                        }
                        else if (reldata is Dat151Unk38)
                        {
                            Emitters.Add(new AudioPlacement(relfile, reldata as Dat151Unk38));
                        }
                    }
                }
            }

            AllItems.AddRange(Zones);
            AllItems.AddRange(Emitters);

            Inited = true;
        }



        private void AddRpfDat151s(RpfFile rpffile, Dictionary<uint, RpfFileEntry> dat151entries)
        {
            if (rpffile.AllEntries == null) return;
            foreach (var entry in rpffile.AllEntries)
            {
                if (entry is RpfFileEntry)
                {
                    RpfFileEntry fentry = entry as RpfFileEntry;
                    if (entry.NameLower.EndsWith(".dat151.rel"))
                    {
                        if (dat151entries.ContainsKey(entry.NameHash))
                        { }
                        dat151entries[entry.NameHash] = fentry;
                    }
                }
            }
        }



    }



    public class AudioPlacement
    {
        public RelFile RelFile { get; set; }
        public Dat151Unk37 AudioZone { get; set; }
        public Dat151Unk38 AudioEmitter { get; set; }
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
        public Vector3 HitboxPos { get; set; }
        public Vector3 HitboxMin { get; set; }
        public Vector3 HitboxMax { get; set; }
        public Quaternion HitboxOri { get; set; }
        public Quaternion HitboxOriInv { get; set; }
        public float HitSphereRad { get; set; }



        public AudioPlacement(RelFile rel, Dat151Unk37 zone)
        {
            RelFile = rel;
            AudioZone = zone;
            Shape = zone.Shape;
            ShortTypeName = "AudioZone";
            FullTypeName = "Audio Zone";

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
            HitboxPos = useouter ? OuterPos : InnerPos;
            HitboxMax = useouter ? OuterMax : InnerMax;
            HitboxMin = useouter ? OuterMin : InnerMin;
            HitboxOri = useouter ? OuterOri : InnerOri;
            HitboxOriInv = Quaternion.Invert(HitboxOri);
        }
        public AudioPlacement(RelFile rel, Dat151Unk38 emitter)
        {
            RelFile = rel;
            AudioEmitter = emitter;
            Shape = Dat151ZoneShape.Sphere;
            ShortTypeName = "AudioEmitter";
            FullTypeName = "Audio Emitter";

            HitboxOri = Quaternion.Identity;
            HitboxOriInv = Quaternion.Identity;
            InnerPos = emitter.Position;
            InnerRad = emitter.InnerRad;
            OuterRad = emitter.OuterRad;

            bool useouter = (InnerRad == 0);
            HitboxPos = InnerPos;
            HitSphereRad = useouter ? OuterRad : InnerRad;
        }


        public void SetPosition(Vector3 pos)
        {
            bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
            Vector3 delta = pos - InnerPos;
            InnerPos = pos;
            OuterPos += delta;
            HitboxPos = useouter ? OuterPos : InnerPos;
        }
        public void SetOrientation(Quaternion ori)
        {
            HitboxOri = ori;
            HitboxOriInv = Quaternion.Invert(ori);

            if (InnerOri == OuterOri)
            {
                InnerOri = HitboxOri;
                OuterOri = HitboxOri;
            }
            else
            {
                //not sure yet how to allow independent rotation of inner & outer boxes...
                //maybe only in project window?
                bool useouter = ((InnerMax.X == 0) || (InnerMax.Y == 0) || (InnerMax.Z == 0));
                if (useouter)
                {
                    OuterOri = HitboxOri;
                }
                else
                {
                    InnerOri = HitboxOri;
                }
            }
        }

    }


}
