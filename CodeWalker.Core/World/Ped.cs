using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class Ped
    {
        public string Name { get; set; } = string.Empty;
        public MetaHash NameHash { get; set; } = 0;//ped name hash
        public CPedModelInfo__InitData InitData { get; set; } = null; //ped init data
        public YddFile Ydd { get; set; } = null; //ped drawables
        public YtdFile Ytd { get; set; } = null; //ped textures
        public YcdFile Ycd { get; set; } = null; //ped animations
        public YftFile Yft { get; set; } = null; //ped skeleton YFT
        public PedFile Ymt { get; set; } = null; //ped variation info
        public Dictionary<MetaHash, RpfFileEntry> DrawableFilesDict { get; set; } = null;
        public Dictionary<MetaHash, RpfFileEntry> TextureFilesDict { get; set; } = null;
        public RpfFileEntry[] DrawableFiles { get; set; } = null;
        public RpfFileEntry[] TextureFiles { get; set; } = null;
        public ClipMapEntry AnimClip { get; set; } = null;
        public string[] DrawableNames { get; set; } = new string[12];
        public Drawable[] Drawables { get; set; } = new Drawable[12];
        public Texture[] Textures { get; set; } = new Texture[12];
        public bool EnableRootMotion { get; set; } = false; //used to toggle whether or not to include root motion when playing animations

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;


        public void Init(string name, GameFileCache gfc)
        {
            var hash = JenkHash.GenHash(name.ToLowerInvariant());
            Init(hash, gfc);
            Name = name;
        }
        public void Init(MetaHash pedhash, GameFileCache gfc)
        {

            Name = string.Empty;
            NameHash = 0;
            InitData = null;
            Ydd = null;
            Ytd = null;
            Ycd = null;
            Yft = null;
            Ymt = null;
            AnimClip = null;
            for (int i = 0; i < 12; i++)
            {
                Drawables[i] = null;
                Textures[i] = null;
            }


            CPedModelInfo__InitData initdata = null;
            if (!gfc.PedsInitDict.TryGetValue(pedhash, out initdata)) return;

            var ycdhash = JenkHash.GenHash(initdata.ClipDictionaryName.ToLowerInvariant());

            //bool pedchange = NameHash != pedhash;
            //Name = pedname;
            NameHash = pedhash;
            InitData = initdata;
            Ydd = gfc.GetYdd(pedhash);
            Ytd = gfc.GetYtd(pedhash);
            Ycd = gfc.GetYcd(ycdhash);
            Yft = gfc.GetYft(pedhash);

            PedFile pedFile = null;
            gfc.PedVariationsDict?.TryGetValue(pedhash, out pedFile);
            Ymt = pedFile;

            Dictionary<MetaHash, RpfFileEntry> peddict = null;
            gfc.PedDrawableDicts.TryGetValue(NameHash, out peddict);
            DrawableFilesDict = peddict;
            DrawableFiles = DrawableFilesDict?.Values.ToArray();
            gfc.PedTextureDicts.TryGetValue(NameHash, out peddict);
            TextureFilesDict = peddict;
            TextureFiles = TextureFilesDict?.Values.ToArray();


            while ((Ydd != null) && (!Ydd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                Ydd = gfc.GetYdd(pedhash);
            }
            while ((Ytd != null) && (!Ytd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                Ytd = gfc.GetYtd(pedhash);
            }
            while ((Ycd != null) && (!Ycd.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                Ycd = gfc.GetYcd(ycdhash);
            }
            while ((Yft != null) && (!Yft.Loaded))
            {
                Thread.Sleep(20);//kinda hacky
                Yft = gfc.GetYft(pedhash);
            }


            MetaHash cliphash = JenkHash.GenHash("idle");
            ClipMapEntry cme = null;
            Ycd?.ClipMap?.TryGetValue(cliphash, out cme);
            AnimClip = cme;

        }



    }
}
