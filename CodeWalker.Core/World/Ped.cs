using CodeWalker.Core.GameFiles.Resources;
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
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Ped
    {
        public string Name { get; set; } = string.Empty;
        public MetaHash NameHash { get; set; } = 0;//ped name hash
        public CPedModelInfo__InitData? InitData { get; set; } = null; //ped init data
        public YddFile? Ydd { get; set; } = null; //ped drawables
        public YtdFile? Ytd { get; set; } = null; //ped textures
        public YldFile? Yld { get; set; } = null; //ped clothes
        public YcdFile? Ycd { get; set; } = null; //ped animations
        public YedFile? Yed { get; set; } = null; //ped expressions
        public YftFile? Yft { get; set; } = null; //ped skeleton YFT
        public PedFile? Ymt { get; set; } = null; //ped variation info
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public PedsFiles PedsFiles { get; set; }
        public ICollection<PedFile> Ymts { get; set; } = new List<PedFile>();
        public ICollection<MetaHash> Dlcs { get; set; } = new List<MetaHash>();
        public PedsDlcFiles? PedsDlcFiles { get; set; } = null;
        public IDictionary<MetaHash, RpfFileEntry>? DrawableFilesDict { get; set; } = null;
        public IDictionary<MetaHash, RpfFileEntry>? TextureFilesDict { get; set; } = null;
        public IDictionary<MetaHash, RpfFileEntry>? ClothFilesDict { get; set; } = null;
        public RpfFileEntry[]? DrawableFiles { get; set; } = null;
        public RpfFileEntry[]? TextureFiles { get; set; } = null;
        public RpfFileEntry[]? ClothFiles { get; set; } = null;
        public ClipMapEntry? AnimClip { get; set; } = null;
        public Expression? Expression { get; set; } = null;
        public string[] DrawableNames { get; set; } = new string[12];
        public Drawable[] Drawables { get; set; } = new Drawable[12];
        public Texture[] Textures { get; set; } = new Texture[12];
        public Expression[] Expressions { get; set; } = new Expression[12];
        public ClothInstance[] Clothes { get; set; } = new ClothInstance[12];
        public bool EnableRootMotion { get; set; } = false; //used to toggle whether or not to include root motion when playing animations
        public Skeleton? Skeleton { get; set; } = null;

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        public YmapEntityDef RenderEntity = new YmapEntityDef(); //placeholder entity object for rendering


        public async ValueTask InitAsync(string name, GameFileCache gfc, MetaHash? selectedDlc = null)
        {
            var hash = JenkHash.GenHashLower(name);
            await InitAsync(hash, gfc, selectedDlc);
            Name = name;
        }
        public async ValueTask InitAsync(MetaHash pedhash, GameFileCache gfc, MetaHash? selectedDlc = null)
        {

            Name = string.Empty;
            NameHash = 0;
            InitData = null;
            Ydd = null;
            Ytd = null;
            Yld = null;
            Ycd = null;
            Yed = null;
            Yft = null;
            Ymt = null;
            AnimClip = null;
            for (int i = 0; i < 12; i++)
            {
                Drawables[i] = null;
                Textures[i] = null;
                Expressions[i] = null;
            }

            Console.WriteLine($"{selectedDlc} selected");

            if (!gfc.PedsFiles.TryGetValue(pedhash, out var pedsFiles))
            {
                Console.WriteLine("PedsFile not found");
            } else
            {
                PedsFiles = pedsFiles;
            }
            if (!gfc.PedsInitDict.TryGetValue(pedhash, out CPedModelInfo__InitData initdata))
            {
                return;
            }

            var ycdhash = JenkHash.GenHashLower(initdata.ClipDictionaryName);
            var yedhash = JenkHash.GenHashLower(initdata.ExpressionDictionaryName);

            //bool pedchange = NameHash != pedhash;
            //Name = pedname;
            NameHash = pedhash;
            InitData = initdata;
            Ydd = gfc.GetYdd(pedhash);
            Ytd = gfc.GetYtd(pedhash);
            Ycd = gfc.GetYcd(ycdhash);
            Yed = gfc.GetYed(yedhash);
            Yft = gfc.GetYft(pedhash);

            if (selectedDlc is not null)
            {
                Ymt = PedsFiles.Dlcs[selectedDlc.Value].PedFile;
            } else
            {
                Ymt = PedsFiles.Dlcs.Values.First().PedFile;
            }


            var dlcsOrdered = pedsFiles.Dlcs.Where(p =>
            {
                if (GameFileCache.Instance.DlcNameLookup.TryGetValue(p.Key, out var dlcName))
                {
                    if (dlcName.Contains("g9", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }).OrderBy(p => p.Value.Index).ToList();
            Ymts = dlcsOrdered.Select(p => p.Value.PedFile).ToList();
            Dlcs = dlcsOrdered.Select(p => p.Key).ToList();

            //if (gfc.PedVariationsDict?.TryGetValue(pedhash, out var pedFiles) ?? false)
            //{
            //    Ymt = pedFiles.First();
            //    Ymts = pedsFiles.Dlcs.Values.Select(p => p.PedFile).ToList();
            //}

            //Ymt = selectedFile ?? Ymt;

            Dictionary<MetaHash, RpfFileEntry> peddict = null;
            if (PedsFiles.TryGetPedsDlcFiles(Ymt, out var pedsDlcFiles))
            {
                Console.WriteLine($"Found {Ymt.RpfFileEntry.Path} in pedsDlclist");
                PedsDlcFiles = pedsDlcFiles;
                DrawableFilesDict = PedsDlcFiles.Drawables;
                TextureFilesDict = PedsDlcFiles.TextureDicts;
                ClothFilesDict = PedsDlcFiles.ClothDicts;
            }
            else
            {
                Console.WriteLine($"{Ymt.RpfFileEntry.Path} not found in pedsDlclist");
                gfc.PedDrawableDicts.TryGetValue(NameHash, out peddict);
                DrawableFilesDict = peddict;
                gfc.PedTextureDicts.TryGetValue(NameHash, out peddict);
                TextureFilesDict = peddict;
                gfc.PedClothDicts.TryGetValue(NameHash, out peddict);
                ClothFilesDict = peddict;
            }

            DrawableFiles = DrawableFilesDict.Values.ToArray();
            TextureFiles = TextureFilesDict.Values.ToArray();
            ClothFiles = ClothFilesDict?.Values?.ToArray() ?? Array.Empty<RpfFileEntry>();

            RpfFileEntry clothFile = null;
            if (ClothFilesDict?.TryGetValue(pedhash, out clothFile) ?? false)
            {
                Yld = await gfc.GetFileUncachedAsync<YldFile>(clothFile);
                while ((Yld != null) && (!Yld.Loaded))
                {
                    await Task.Delay(1);//kinda hacky
                    await gfc.TryLoadEnqueue(Yld);
                }
            }


            while ((Ydd != null) && (!Ydd.Loaded))
            {
                await Task.Delay(1);//kinda hacky
                await gfc.TryLoadEnqueue(Ydd);
            }
            while ((Ytd != null) && (!Ytd.Loaded))
            {
                await Task.Delay(1);//kinda hacky
                await gfc.TryLoadEnqueue(Ytd);
            }
            while ((Ycd != null) && (!Ycd.Loaded))
            {
                await Task.Delay(1);//kinda hacky
                await gfc.TryLoadEnqueue(Ycd);
            }
            while ((Yed != null) && (!Yed.Loaded))
            {
                await Task.Delay(1);//kinda hacky
                await gfc.TryLoadEnqueue(Yed);
            }
            while ((Yft != null) && (!Yft.Loaded))
            {
                await Task.Delay(1);//kinda hacky
                await gfc.TryLoadEnqueue(Yft);
            }


            Skeleton = Yft?.Fragment?.Drawable?.Skeleton?.Clone();

            MetaHash cliphash = JenkHash.GenHash("idle");
            ClipMapEntry cme = null;
            Ycd?.ClipMap?.TryGetValue(cliphash, out cme);
            AnimClip = cme;

            var exprhash = JenkHash.GenHashLower(initdata.ExpressionName);
            Expression expr = null;
            Yed?.ExprMap?.TryGetValue(exprhash, out expr);
            Expression = expr;


            UpdateEntity();
        }





        public async ValueTask SetComponentDrawableAsync(int index, string name, string tex, MetaHash dlc, GameFileCache gfc)
        {
            if (string.IsNullOrEmpty(name))
            {
                DrawableNames[index] = null;
                Drawables[index] = null;
                Textures[index] = null;
                Expressions[index] = null;
                return;
            }

            Console.WriteLine($"{index}: {name} - {tex} - {dlc}");
            MetaHash namehash = JenkHash.GenHashLower(name);
            PedsDlcFiles pedsDlcFiles;
            Drawable d = null;
            if (Ydd?.Dict != null)
            {
                Ydd.Dict.TryGetValue(namehash, out d);
            }
            if (PedsFiles.TryGetPedsDlcFiles(dlc, out pedsDlcFiles))
            {
                if (pedsDlcFiles.Drawables.TryGetValue(namehash, out var file))
                {
                    var ydd = await gfc.GetFileUncachedAsync<YddFile>(file);
                    while ((ydd != null) && (!ydd.Loaded))
                    {
                        await Task.Delay(1);//kinda hacky
                        await gfc.TryLoadEnqueue(ydd);
                    }
                    if (ydd?.Drawables?.Length > 0)
                    {
                        d = ydd.Drawables[0];//should only be one in this dict
                    }
                }
            }
            if ((d == null) && (DrawableFilesDict != null))
            {
                if (DrawableFilesDict.TryGetValue(namehash, out var file))
                {
                    var ydd = await gfc.GetFileUncachedAsync<YddFile>(file);
                    while ((ydd != null) && (!ydd.Loaded))
                    {
                        await Task.Delay(1);//kinda hacky
                        await gfc.TryLoadEnqueue(ydd);
                    }
                    if (ydd?.Drawables?.Length > 0)
                    {
                        d = ydd.Drawables[0];//should only be one in this dict
                    }
                }
            }

            MetaHash texhash = JenkHash.GenHashLower(tex);
            Texture? t = null;

            if (t is null && PedsFiles.TryGetPedsDlcFiles(dlc, out pedsDlcFiles))
            {
                if (pedsDlcFiles.TextureDicts.TryGetValue(texhash, out var file))
                {
                    var ytd = await gfc.GetFileUncachedAsync<YtdFile>(file);
                    while (ytd != null && !ytd.Loaded)
                    {
                        await Task.Delay(1);//kinda hacky
                        await gfc.TryLoadEnqueue(ytd);
                    }
                    if (ytd?.TextureDict?.Textures?.data_items.Length > 0)
                    {
                        t = ytd.TextureDict.Textures.data_items[0];//should only be one in this dict
                    }
                }
                else
                {
                    Console.WriteLine($"Couldn't find texture file in PedsFiles");
                }
            }

            if (t is null && Ytd?.TextureDict?.Dict != null)
            {
                Ytd.TextureDict.Dict.TryGetValue(texhash, out t);
            }

            if ((t == null) && (TextureFilesDict != null))
            {
                RpfFileEntry file = null;
                if (TextureFilesDict.TryGetValue(texhash, out file))
                {
                    var ytd = await gfc.GetFileUncachedAsync<YtdFile>(file);
                    while ((ytd != null) && (!ytd.Loaded))
                    {
                        await Task.Delay(1);//kinda hacky
                        await gfc.TryLoadEnqueue(ytd);
                    }
                    if (ytd?.TextureDict?.Textures?.data_items.Length > 0)
                    {
                        t = ytd.TextureDict.Textures.data_items[0];//should only be one in this dict
                    }
                }
            }

            CharacterCloth cc = null;
            if (Yld?.Dict != null)
            {
                Yld.Dict.TryGetValue(namehash, out cc);
            }
            if (cc == null && PedsFiles.TryGetPedsDlcFiles(dlc, out pedsDlcFiles))
            {
                if (pedsDlcFiles.TextureDicts.TryGetValue(namehash, out var file))
                {
                    var yld = await gfc.GetFileUncachedAsync<YldFile>(file);
                    while ((yld != null) && (!yld.Loaded))
                    {
                        await Task.Delay(1);//kinda hacky
                        await gfc.TryLoadEnqueue(yld);
                    }
                    if (yld?.ClothDictionary?.Clothes?.data_items?.Length > 0)
                    {
                        cc = yld.ClothDictionary.Clothes.data_items[0];//should only be one in this dict
                    }
                }
            }
            if ((cc == null) && (ClothFilesDict != null))
            {
                if (ClothFilesDict.TryGetValue(namehash, out RpfFileEntry file))
                {
                    var yld = await gfc.GetFileUncachedAsync<YldFile>(file);
                    while ((yld != null) && (!yld.Loaded))
                    {
                        await Task.Delay(1);//kinda hacky
                        await gfc.TryLoadEnqueue(yld);
                    }
                    if (yld?.ClothDictionary?.Clothes?.data_items?.Length > 0)
                    {
                        cc = yld.ClothDictionary.Clothes.data_items[0];//should only be one in this dict
                    }
                }
            }
            ClothInstance c = null;
            if (cc != null)
            {
                c = new ClothInstance();
                c.Init(cc, Skeleton);
            }

            Expression e = null;
            if (Yed?.ExprMap != null)
            {
                Yed.ExprMap.TryGetValue(namehash, out e);
            }


            if (d != null) Drawables[index] = d.ShallowCopy() as Drawable;
            if (t != null) Textures[index] = t;
            if (c != null) Clothes[index] = c;
            if (e != null) Expressions[index] = e;

            DrawableNames[index] = name;
        }

        public async ValueTask SetComponentDrawableAsync(int index, int drawbl, int alt, int tex, GameFileCache gfc)
        {
            var vi = Ymt?.VariationInfo;
            if (vi != null)
            {
                var compData = vi.GetComponentData(index);
                if (compData?.DrawblData3 != null)
                {
                    var item = (drawbl < (compData.DrawblData3?.Length ?? 0)) ? compData.DrawblData3[drawbl] : null;
                    if (item != null)
                    {
                        var name = item?.GetDrawableName(alt);
                        var texn = item?.GetTextureName(tex);
                        await SetComponentDrawableAsync(index, name, texn, 0, gfc);
                    }
                }
            }
        }

        public async ValueTask LoadDefaultComponentsAsync(GameFileCache gfc)
        {
            for (int i = 0; i < 12; i++)
            {
                await SetComponentDrawableAsync(i, 0, 0, 0, gfc);
            }
        }




        public void UpdateEntity()
        {
            RenderEntity.SetPosition(Position);
            RenderEntity.SetOrientation(Rotation);
        }

    }
}
