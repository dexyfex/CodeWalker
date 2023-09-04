using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeWalker.World
{
    public class Space
    {

        public LinkedList<Entity> TemporaryEntities = new LinkedList<Entity>();
        public LinkedList<Entity> PersistentEntities = new LinkedList<Entity>();
        public List<Entity> EnabledEntities = new List<Entity>(); //built each frame

        private GameFileCache GameFileCache = null;




        public SpaceMapDataStore MapDataStore;
        public SpaceBoundsStore BoundsStore;



        private Dictionary<MetaHash, MetaHash> interiorLookup = new Dictionary<MetaHash, MetaHash>();
        private Dictionary<MetaHash, YmfInterior> interiorManifest = new Dictionary<MetaHash, YmfInterior>();
        private Dictionary<SpaceBoundsKey, CInteriorProxy> interiorProxies = new Dictionary<SpaceBoundsKey, CInteriorProxy>();
        private Dictionary<MetaHash, YmfMapDataGroup> dataGroupDict = new Dictionary<MetaHash, YmfMapDataGroup>();
        private Dictionary<MetaHash, MapDataStoreNode> nodedict = new Dictionary<MetaHash, MapDataStoreNode>();
        private Dictionary<SpaceBoundsKey, BoundsStoreItem> boundsdict = new Dictionary<SpaceBoundsKey, BoundsStoreItem>();
        private Dictionary<MetaHash, BoundsStoreItem> usedboundsdict = new Dictionary<MetaHash, BoundsStoreItem>();

        private Dictionary<MetaHash, uint> ymaptimes = new Dictionary<MetaHash, uint>();
        private Dictionary<MetaHash, MetaHash[]> ymapweathertypes = new Dictionary<MetaHash, MetaHash[]>();

        public bool Inited = false;


        public SpaceNodeGrid NodeGrid;
        private Dictionary<uint, YndFile> AllYnds = new Dictionary<uint, YndFile>();

        public SpaceNavGrid NavGrid;

        public List<SpaceEntityCollision> Collisions = new List<SpaceEntityCollision>();
        private bool[] CollisionLayers = new[] { true, false, false };

        private int CurrentHour;
        private MetaHash CurrentWeather;


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            GameFileCache = gameFileCache;


            updateStatus("Scanning manifests...");

            InitManifestData();


            updateStatus("Scanning caches...");

            InitCacheData();


            updateStatus("Building map data store...");

            InitMapDataStore();


            updateStatus("Building bounds store...");

            InitBoundsStore();


            updateStatus("Loading paths...");

            InitNodeGrid();


            updateStatus("Loading nav meshes...");

            InitNavGrid();


            Inited = true;
            updateStatus("World initialised.");
        }


        private void InitManifestData()
        {
            interiorLookup.Clear();
            interiorManifest.Clear();
            ymaptimes.Clear();
            ymapweathertypes.Clear();
            dataGroupDict.Clear();

            var manifests = GameFileCache.AllManifests;
            foreach (var manifest in manifests)
            {
                //build interior lookup - maps child->parent interior bounds
                if (manifest.Interiors != null)
                {
                    foreach (var interior in manifest.Interiors)
                    {
                        var intname = interior.Interior.Name;
                        if (interiorManifest.ContainsKey(intname))
                        { }
                        interiorManifest[intname] = interior;

                        if (interior.Bounds != null)
                        {
                            foreach (var intbound in interior.Bounds)
                            {
                                if (interiorLookup.ContainsKey(intbound))
                                { }//updates can hit here
                                interiorLookup[intbound] = intname;
                            }
                        }
                        else
                        { }
                    }
                }

                //these appear to be all the dynamic "togglable" ymaps....
                if (manifest.MapDataGroups != null)
                {
                    foreach (var mapgroup in manifest.MapDataGroups)
                    {
                        if (mapgroup.HoursOnOff != 0)
                        {
                            ymaptimes[mapgroup.Name] = mapgroup.HoursOnOff;
                        }
                        if (mapgroup.WeatherTypes != null)
                        {
                            ymapweathertypes[mapgroup.Name] = mapgroup.WeatherTypes;
                        }

                        if (dataGroupDict.ContainsKey(mapgroup.DataGroup.Name))
                        {
                            if (mapgroup.Bounds != null)
                            {
                                var ex = dataGroupDict[mapgroup.DataGroup.Name];
                                if (ex.Bounds != null)
                                { } //only 1 hit here - redcarpet
                            }
                            else
                            {
                                continue;//nothing to replace with
                            }
                        }
                        dataGroupDict[mapgroup.DataGroup.Name] = mapgroup;
                    }
                }
            }

            //YmfMapDataGroups string
            //StringBuilder sb = new StringBuilder();
            //foreach (var dg in dataGroupDict.Values)
            //{
            //    sb.AppendLine(dg.ToString());
            //    if (dg.Bounds != null)
            //    {
            //        foreach (var ybnh in dg.Bounds)
            //        {
            //            sb.AppendLine("   " + ybnh.ToString());
            //        }
            //    }
            //}
            //string str = sb.ToString();

        }

        private void InitCacheData()
        {
            //build the grid from the cached data
            var caches = GameFileCache.AllCacheFiles;
            nodedict = new Dictionary<MetaHash, MapDataStoreNode>();
            MetaHash inthash;
            List<BoundsStoreItem> intlist = new List<BoundsStoreItem>();
            boundsdict = new Dictionary<SpaceBoundsKey, BoundsStoreItem>();
            usedboundsdict = new Dictionary<MetaHash, BoundsStoreItem>();
            interiorProxies = new Dictionary<SpaceBoundsKey, CInteriorProxy>();

            Dictionary<MetaHash, CacheFileDate> filedates = new Dictionary<MetaHash, CacheFileDate>();
            Dictionary<uint, CacheFileDate> filedates2 = new Dictionary<uint, CacheFileDate>();

            foreach (var cache in caches)
            {
                foreach (var filedate in cache.FileDates)
                {
                    CacheFileDate exdate;
                    if (filedates.TryGetValue(filedate.FileName, out exdate))
                    {
                        if (filedate.TimeStamp >= exdate.TimeStamp)
                        {
                            filedates[filedate.FileName] = filedate;
                        }
                        else //if (filedate.TimeStamp < exdate.TimeStamp)
                        { }
                    }
                    else
                    {
                        filedates[filedate.FileName] = filedate;
                    }

                    if (filedates2.TryGetValue(filedate.FileID, out exdate))
                    {
                        if (filedate.FileName != exdate.FileName)
                        { }
                        if (filedate.TimeStamp >= exdate.TimeStamp)
                        {
                            filedates2[filedate.FileID] = filedate;
                        }
                        else
                        { }
                    }
                    else
                    {
                        filedates2[filedate.FileID] = filedate;
                    }

                }



                foreach (var node in cache.AllMapNodes)
                {
                    if (!GameFileCache.YmapDict.ContainsKey(node.Name))
                    { continue; }
                    nodedict[node.Name] = node;
                }

                foreach (var intprx in cache.AllCInteriorProxies)
                {
                    //these might need to go into the grid. which grid..?
                    //but might need to map back to the bounds store... this has more info though!
                    SpaceBoundsKey key = new SpaceBoundsKey(intprx.Name, intprx.Position);
                    if (interiorProxies.ContainsKey(key))
                    { }//updates/dlc hit here
                    interiorProxies[key] = intprx;
                }

                foreach (var item in cache.AllBoundsStoreItems)
                {
                    if (!GameFileCache.YbnDict.ContainsKey(item.Name))
                    { continue; }

                    if ((item.Layer < 0) || (item.Layer > 3))
                    { } //won't hit here..
                    if (interiorLookup.TryGetValue(item.Name, out inthash))
                    {
                        //it's an interior... the vectors are in local space...
                        intlist.Add(item);//handle it later? use the parent for a dict?
                    }
                    else //interiors filtered out
                    {
                        SpaceBoundsKey key = new SpaceBoundsKey(item.Name, item.Min);
                        if (boundsdict.ContainsKey(key))
                        { }//updates/dlc hit here
                        boundsdict[key] = item;

                    }
                    usedboundsdict[item.Name] = item;
                }
            }




            //try and generate the cache data for uncached ymaps... mainly for mod maps!
            var maprpfs = GameFileCache.ActiveMapRpfFiles;
            foreach (var maprpf in maprpfs.Values)
            {
                foreach (var entry in maprpf.AllEntries)
                {
                    if (entry.NameLower.EndsWith(".ymap"))
                    {
                        if (!nodedict.ContainsKey(new MetaHash(entry.ShortNameHash)))
                        {
                            //non-cached ymap. mostly only mods... but some interesting test things also
                            var ymap = GameFileCache.RpfMan.GetFile<YmapFile>(entry);
                            if (ymap != null)
                            {
                                MapDataStoreNode dsn = new MapDataStoreNode(ymap);
                                if (dsn.Name != 0)
                                {
                                    nodedict[dsn.Name] = dsn;//perhaps should add as entry.ShortNameHash?
                                }
                                else
                                { }
                            }
                            else
                            { }
                        }
                    }
                    if (entry.NameLower.EndsWith(".ybn"))
                    {
                        MetaHash ehash = new MetaHash(entry.ShortNameHash);
                        if (!usedboundsdict.ContainsKey(ehash))
                        {
                            if (interiorLookup.ContainsKey(ehash))
                            {
                            }
                            else
                            {
                                //exterior ybn's that aren't already cached... only noncached modded bounds hit here...
                                //load the ybn and cache its extents.
                                var ybn = GameFileCache.RpfMan.GetFile<YbnFile>(entry);
                                BoundsStoreItem item = new BoundsStoreItem(ybn.Bounds);
                                item.Name = ehash;
                                SpaceBoundsKey key = new SpaceBoundsKey(ehash, item.Min);
                                if (boundsdict.ContainsKey(key))
                                { }
                                boundsdict[key] = item;
                            }
                        }
                    }
                }
            }


        }

        private void InitMapDataStore()
        {

            MapDataStore = new SpaceMapDataStore();

            MapDataStore.Init(nodedict.Values.ToList());

        }

        private void InitBoundsStore()
        {

            BoundsStore = new SpaceBoundsStore();

            BoundsStore.Init(boundsdict.Values.ToList());

        }

        private void InitNodeGrid()
        {

            NodeGrid = new SpaceNodeGrid();
            AllYnds.Clear();

            var rpfman = GameFileCache.RpfMan;
            Dictionary<uint, RpfFileEntry> yndentries = new Dictionary<uint, RpfFileEntry>();
            foreach (var rpffile in GameFileCache.BaseRpfs) //load nodes from base rpfs
            {
                AddRpfYnds(rpffile, yndentries);
            }
            if (GameFileCache.EnableDlc)
            {
                var updrpf = rpfman.FindRpfFile("update\\update.rpf"); //load nodes from patch area...
                if (updrpf != null)
                {
                    foreach (var rpffile in updrpf.Children)
                    {
                        AddRpfYnds(rpffile, yndentries);
                    }
                }
                foreach (var dlcrpf in GameFileCache.DlcActiveRpfs) //load nodes from current dlc rpfs
                {
                    if (dlcrpf.Path.StartsWith("x64")) continue; //don't override update.rpf YNDs with x64 ones! *hack
                    foreach (var rpffile in dlcrpf.Children)
                    {
                        AddRpfYnds(rpffile, yndentries);
                    }
                }
            }


            Vector3 corner = new Vector3(-8192, -8192, -2048);
            Vector3 cellsize = new Vector3(512, 512, 4096);

            for (int x = 0; x < NodeGrid.CellCountX; x++)
            {
                for (int y = 0; y < NodeGrid.CellCountY; y++)
                {
                    var cell = NodeGrid.Cells[x, y];
                    string fname = "nodes" + cell.ID + ".ynd";
                    uint fnhash = JenkHash.GenHash(fname);
                    RpfFileEntry fentry = null;
                    if (yndentries.TryGetValue(fnhash, out fentry))
                    {
                        cell.Ynd = rpfman.GetFile<YndFile>(fentry);
                        cell.Ynd.BBMin = corner + (cellsize * new Vector3(x, y, 0));
                        cell.Ynd.BBMax = cell.Ynd.BBMin + cellsize;
                        cell.Ynd.CellX = x;
                        cell.Ynd.CellY = y;
                        cell.Ynd.Loaded = true;

                        AllYnds[fnhash] = cell.Ynd;


                        #region node flags test

                        //if (cell.Ynd == null) continue;
                        //if (cell.Ynd.NodeDictionary == null) continue;
                        //if (cell.Ynd.NodeDictionary.Nodes == null) continue;
                        //var na = cell.Ynd.NodeDictionary.Nodes;

                        //for (int i = 0; i < na.Length; i++)
                        //{
                        //    var node = na[i];

                        //    int nodetype = node.Unk25Type & 7;
                        //    int linkcount = node.Unk25Type >> 3;
                        //    int nxtlink = node.LinkID + linkcount;
                        //    if (i < na.Length - 1)
                        //    {
                        //        var nxtnode = na[i + 1];
                        //        if (nxtnode.LinkID != nxtlink)
                        //        { }
                        //    }
                        //    else
                        //    {
                        //        if (nxtlink != cell.Ynd.NodeDictionary.LinksCount)
                        //        { }
                        //    }

                        //    switch (node.Flags0)
                        //    {
                        //        case 0:
                        //        case 1:
                        //        case 2:
                        //        case 8:
                        //        case 10:
                        //        case 32:
                        //        case 34:
                        //        case 35:
                        //        case 40:
                        //        case 42:
                        //        case 66:
                        //        case 98:
                        //        case 129:
                        //        case 130:
                        //        case 162:
                        //        case 194:
                        //        case 226:
                        //            break;
                        //        default:
                        //            break;
                        //    }
                        //    switch (node.Flags1)
                        //    {
                        //        case 0:
                        //        case 1:
                        //        case 2:
                        //        case 3:
                        //        case 4:
                        //        case 16:
                        //        case 80:
                        //        case 112:
                        //        case 120:
                        //        case 121:
                        //        case 122:
                        //        case 128:
                        //        case 129:
                        //        case 136:
                        //        case 144:
                        //        case 152:
                        //        case 160:
                        //            break;
                        //        default:
                        //            break;
                        //    }

                        //}
                        #endregion

                    }
                }
            }

            //join the dots....
            //StringBuilder sb = new StringBuilder();
            List<EditorVertex> tverts = new List<EditorVertex>();
            List<YndLink> tlinks = new List<YndLink>();
            List<YndLink> nlinks = new List<YndLink>();
            foreach (var ynd in AllYnds.Values)
            {
                BuildYndData(ynd, tverts, tlinks, nlinks);

                //sb.Append(ynd.nodestr);
            }

            //string str = sb.ToString();
        }

        public void PatchYndFile(YndFile ynd)
        {
            //ideally we should be able to revert to the vanilla ynd's after closing the project window,
            //but codewalker can always just be restarted, so who cares really
            NodeGrid.UpdateYnd(ynd);
        }

        private void AddRpfYnds(RpfFile rpffile, Dictionary<uint, RpfFileEntry> yndentries)
        {
            if (rpffile.AllEntries == null) return;
            foreach (var entry in rpffile.AllEntries)
            {
                if (entry is RpfFileEntry)
                {
                    RpfFileEntry fentry = entry as RpfFileEntry;
                    if (entry.NameLower.EndsWith(".ynd"))
                    {
                        if (yndentries.ContainsKey(entry.NameHash))
                        { }
                        yndentries[entry.NameHash] = fentry;
                    }
                }
            }
        }

        public void BuildYndLinks(YndFile ynd, List<YndLink> tlinks = null, List<YndLink> nlinks = null)
        {
            var ynodes = ynd.Nodes;
            var nodes = ynd.NodeDictionary?.Nodes;
            var links = ynd.NodeDictionary?.Links;
            if ((ynodes == null) || (nodes == null) || (links == null)) return;

            int nodecount = ynodes.Length;


            //build the links arrays.
            if(tlinks==null) tlinks = new List<YndLink>();
            if(nlinks==null) nlinks = new List<YndLink>();
            tlinks.Clear();
            for (int i = 0; i < nodecount; i++)
            {
                nlinks.Clear();
                var node = ynodes[i];

                var linkid = node.LinkID;
                for (int l = 0; l < node.LinkCount; l++)
                {
                    var llid = linkid + l;
                    if (llid >= links.Length) continue;
                    var link = links[llid];
                    YndNode tnode;
                    if (link.AreaID == node.AreaID)
                    {
                        if (link.NodeID >= ynodes.Length)
                        { continue; }
                        tnode = ynodes[link.NodeID];
                    }
                    else
                    {
                        tnode = NodeGrid.GetYndNode(link.AreaID, link.NodeID);
                        if (tnode == null)
                        { continue; }
                        if ((Math.Abs(tnode.Ynd.CellX - ynd.CellX) > 1) || (Math.Abs(tnode.Ynd.CellY - ynd.CellY) > 1))
                        { /*continue;*/ } //non-adjacent cell? seems to be the carrier problem...
                    }

                    YndLink yl = new YndLink();
                    yl.Init(ynd, node, tnode, link);
                    tlinks.Add(yl);
                    nlinks.Add(yl);
                }
                node.Links = nlinks.ToArray();
            }
            ynd.Links = tlinks.ToArray();

        }
        public void BuildYndVerts(YndFile ynd, YndNode[] selectedNodes, List<EditorVertex> tverts = null)
        {
            var laneColour = (uint) new Color4(0f, 0f, 1f, 1f).ToRgba();
            var ynodes = ynd.Nodes;
            if (ynodes == null) return;

            int nodecount = ynodes.Length;

            //build the main linked vertex array (used by the renderable to draw the lines).
            if(tverts==null) tverts = new List<EditorVertex>();
            tverts.Clear();
            for (int i = 0; i < nodecount; i++)
            {
                var node = ynodes[i];
                if (node.Links == null) continue;


                var nvert = new EditorVertex();
                nvert.Position = node.Position;
                nvert.Colour = (uint)node.Colour.ToRgba();


                for (int l = 0; l < node.Links.Length; l++)
                {
                    YndLink yl = node.Links[l];
                    var laneDir = yl.GetDirection();
                    var laneDirCross = Vector3.Cross(laneDir, Vector3.UnitZ);
                    var laneWidth = yl.GetLaneWidth();
                    var laneHalfWidth = laneWidth / 2;
                    var offset = yl.IsTwoWay()
                        ? yl.LaneOffset * laneWidth - laneHalfWidth
                        : yl.LaneOffset - yl.LaneCountForward * laneWidth / 2f + laneHalfWidth;

                    var iOffset = yl.IsTwoWay() ? 1 : 0;

                    var tnode = yl.Node2;

                    if (tnode == null) continue; //invalid links could hit here
                    var tvert = new EditorVertex();
                    tvert.Position = tnode.Position;
                    tvert.Colour = (uint)tnode.Colour.ToRgba();

                    tverts.Add(nvert);
                    tverts.Add(tvert);

                    // Add lane display
                    for (int j = iOffset; j < yl.LaneCountForward + iOffset; j++)
                    {
                        var vertOffset = laneDirCross * (offset + laneWidth * j);
                        
                        vertOffset.Z = 0.1f;
                        var lvert1 = new EditorVertex
                        {
                            Position = nvert.Position + vertOffset,
                            Colour = laneColour
                        };

                        var lvert2 = new EditorVertex
                        {
                            Position = tvert.Position + vertOffset,
                            Colour = laneColour
                        };

                        tverts.Add(lvert1);
                        tverts.Add(lvert2);

                        // Arrow
                        var apos = lvert1.Position + laneDir * yl.LinkLength / 2;
                        const float asize = 0.5f;
                        const float negasize = asize * -1f;
                        tverts.Add(new EditorVertex(){ Position = apos, Colour = laneColour});
                        tverts.Add(new EditorVertex() { Position = apos + laneDir * negasize + laneDirCross * asize, Colour = laneColour });
                        tverts.Add(new EditorVertex() { Position = apos, Colour = laneColour });
                        tverts.Add(new EditorVertex() { Position = apos + laneDir * negasize + laneDirCross * negasize, Colour = laneColour });
                    }
                }
            }
            ynd.LinkedVerts = tverts.ToArray();

            ynd.UpdateTriangleVertices(selectedNodes);
        }
        public void BuildYndJuncs(YndFile ynd)
        {
            //attach the junctions to the nodes.
            var yjuncs = ynd.Junctions;
            if (yjuncs != null)
            {
                var junccount = yjuncs.Length;
                for (int i = 0; i < junccount; i++)
                {
                    var junc = yjuncs[i];
                    var cell = NodeGrid.GetCell(junc.RefData.AreaID);
                    if ((cell == null) || (cell.Ynd == null) || (cell.Ynd.Nodes == null))
                    { continue; }

                    var jynd = cell.Ynd;
                    if (cell.Ynd != ynd) //junc in different ynd..? no hits here, except ynds in project..
                    {
                        if (cell.Ynd.AreaID == ynd.AreaID)
                        {
                            jynd = ynd;
                        }
                        else
                        { }
                    }

                    if (junc.RefData.NodeID >= jynd.Nodes.Length)
                    { continue; }

                    var jnode = jynd.Nodes[junc.RefData.NodeID];
                    jnode.Junction = junc;
                    jnode.HasJunction = true;
                }
            }

        }
        public void BuildYndData(YndFile ynd, List<EditorVertex> tverts = null, List<YndLink> tlinks = null, List<YndLink> nlinks = null)
        {

            BuildYndLinks(ynd, tlinks, nlinks);

            BuildYndJuncs(ynd);

            BuildYndVerts(ynd, null, tverts);

        }

        public YndFile[] GetYndFilesThatDependOnYndFile(YndFile file)
        {
            return AllYnds.Values.Where(y => y.Links.Any(l => l.Node2.AreaID == file.AreaID)).ToArray();
        }

        public void MoveYndArea(YndFile ynd, int desiredX, int desiredY)
        {
            var xDir = Math.Min(1, Math.Max(-1, desiredX - ynd.CellX));
            var yDir = Math.Min(1, Math.Max(-1, desiredY - ynd.CellY));

            var x = desiredX;
            var y = desiredY;

            if (xDir != 0)
            {
                while (x >= 0 && x <= 31)
                {
                    if (NodeGrid.Cells[x, y].Ynd == null)
                    {
                        break;
                    }

                    x += xDir;
                }
            }

            if (yDir != 0)
            {
                while (y >= 0 && y <= 31)
                {
                    if (NodeGrid.Cells[x, y].Ynd == null)
                    {
                        break;
                    }

                    y += yDir;
                }
            }

            ynd.CellX = x;
            ynd.CellY = y;
            var areaId = y * 32 + x;
            ynd.AreaID = areaId;
            ynd.Name = $"nodes{areaId}";
            NodeGrid.UpdateYnd(ynd);
        }

        public void RecalculateAllYndIndices()
        {
            foreach (var yndFile in AllYnds.Values)
            {
                yndFile.RecalculateNodeIndices();
            }
        }


        private void InitNavGrid()
        {
            NavGrid = new SpaceNavGrid();

            var rpfman = GameFileCache.RpfMan;
            Dictionary<uint, RpfFileEntry> ynventries = new Dictionary<uint, RpfFileEntry>();
            foreach (var rpffile in GameFileCache.BaseRpfs) //load navmeshes from base rpfs
            {
                AddRpfYnvs(rpffile, ynventries);
            }
            if (GameFileCache.EnableDlc)
            {
                var updrpf = rpfman.FindRpfFile("update\\update.rpf"); //load navmeshes from patch area...
                if (updrpf != null)
                {
                    foreach (var rpffile in updrpf.Children)
                    {
                        AddRpfYnvs(rpffile, ynventries);
                    }
                }
                foreach (var dlcrpf in GameFileCache.DlcActiveRpfs) //load navmeshes from current dlc rpfs
                {
                    foreach (var rpffile in dlcrpf.Children)
                    {
                        AddRpfYnvs(rpffile, ynventries);
                    }
                }
            }


            for (int x = 0; x < NavGrid.CellCountX; x++)
            {
                for (int y = 0; y < NavGrid.CellCountY; y++)
                {
                    var cell = NavGrid.Cells[x, y];
                    string fname = "navmesh[" + cell.FileX.ToString() + "][" + cell.FileY.ToString() + "].ynv";
                    uint fnhash = JenkHash.GenHash(fname);
                    RpfFileEntry fentry = null;
                    if (ynventries.TryGetValue(fnhash, out fentry))
                    {
                        cell.YnvEntry = fentry as RpfResourceFileEntry;
                        //cell.Ynv = rpfman.GetFile<YnvFile>(fentry);
                    }
                }
            }

        }

        private void AddRpfYnvs(RpfFile rpffile, Dictionary<uint, RpfFileEntry> ynventries)
        {
            if (rpffile.AllEntries == null) return;
            foreach (var entry in rpffile.AllEntries)
            {
                if (entry is RpfFileEntry)
                {
                    RpfFileEntry fentry = entry as RpfFileEntry;
                    if (entry.NameLower.EndsWith(".ynv"))
                    {
                        if (ynventries.ContainsKey(entry.NameHash))
                        { }
                        ynventries[entry.NameHash] = fentry;
                    }
                }
            }
        }



        public void Update(float elapsed)
        {
            if (!Inited) return;
            if (BoundsStore == null) return;

            if (elapsed > 0.1f) elapsed = 0.1f;


            Collisions.Clear();


            EnabledEntities.Clear();
            foreach (var e in PersistentEntities)
            {
                if (e.Enabled) EnabledEntities.Add(e);
            }
            foreach (var e in TemporaryEntities)
            {
                if (e.Enabled) EnabledEntities.Add(e);
            }



            float gravamt = -9.8f;
            Vector3 dvgrav = new Vector3(0, 0, gravamt * elapsed); //gravity acceleration vector
            dvgrav += (0.5f * dvgrav * elapsed); //v = ut+0.5at^2 !
            float minvel = 0.5f; // stop bouncing when slow...

            foreach (var e in EnabledEntities)
            {
                if (!e.Enabled) continue;

                e.Velocity += dvgrav; //apply gravity
                e.Momentum = e.Velocity * e.Mass;
                e.Age += elapsed;

                e.PreUpdate(elapsed);

                if (e.EnableCollisions)
                {
                    var coll = FindFirstCollision(e, elapsed);

                    if (coll.Hit)
                    {
                        Collisions.Add(coll);

                        float argvel = Math.Abs((e.Velocity - dvgrav).Length());

                        if (e.WasColliding && (argvel < minvel))
                        {
                            e.Velocity = Vector3.Zero;
                            e.Momentum = Vector3.Zero;
                        }
                        else
                        {
                            e.Position = coll.PrePos; //move to the last known position before collision

                            //bounce...
                            int maxbounce = 5;
                            int curbounce = 0;
                            float trem = 1.0f - coll.PreT;
                            while (trem > 0)
                            {
                                float vl = e.Velocity.Length();
                                float erem = elapsed * trem;
                                float drem = vl * erem;
                                Vector3 hitn = coll.SphereHit.Normal;
                                Vector3 bdir = Vector3.Reflect(coll.HitVelDir, hitn);
                                Vector3 newvel = bdir * (vl * 0.5f); //restitution/bouncyness
                                e.Velocity = newvel;

                                coll = FindFirstCollision(e, erem);

                                if (!coll.Hit)
                                {
                                    e.Position = coll.HitPos;//no hit, all done
                                    break;
                                }

                                Collisions.Add(coll);

                                e.Position = coll.PrePos;

                                trem = Math.Max(trem * (1.0f - coll.PreT), 0);

                                curbounce++;
                                if (curbounce >= maxbounce)
                                {
                                    e.Position = coll.HitPos;
                                    break;
                                }


                                //if ((coll.PreT <= 0))// || (coll.SphereHit.Normal == hitn))
                                //{
                                //    //ae.Velocity = Vector3.Zero; //same collision twice? abort?
                                //    break;
                                //}
                            }

                            e.Momentum = e.Velocity * e.Mass;
                        }
                        e.WasColliding = true;
                    }
                    else
                    {
                        e.Position = coll.HitPos; //hit pos is the end pos if no hit
                        e.WasColliding = false;
                    }
                }

                if (e.EntityDef != null)
                {
                    e.EntityDef.Position = e.Position;
                }


                if ((e.Lifetime > 0.0f) && (e.Age > e.Lifetime))
                {
                    TemporaryEntities.Remove(e);
                }

            }


        }


        public SpaceEntityCollision FindFirstCollision(Entity e, float elapsed)
        {
            SpaceEntityCollision r = new SpaceEntityCollision();
            r.Entity = e;

            Vector3 pos = e.Position;
            Vector3 sphpos = pos + e.Center;
            Vector3 disp = e.Velocity * elapsed;
            float absdisp = disp.Length();

            r.HitVelDir = Vector3.Normalize(disp);
            r.HitPos = pos + disp;
            r.HitVel = e.Velocity;
            r.HitT = 1.0f;
            r.PreT = 0.0f;
            r.PrePos = pos;

            BoundingSphere sph = new BoundingSphere(r.HitPos + e.Center, e.Radius);

            r.SphereHit = SphereIntersect(sph, CollisionLayers);

            if (!r.SphereHit.Hit)
            {
                if (absdisp > e.Radius) //fast-moving... do a ray test to make sure it's not tunnelling
                {
                    Ray rayt = new Ray(sphpos, r.HitVelDir);
                    float rayl = absdisp + e.Radius * 4.0f; //include some extra incase of glancing hit
                    var rayhit = RayIntersect(rayt, rayl);
                    if (rayhit.Hit) //looks like it is tunnelling... need to find the sphere hit point
                    {
                        sph.Center = rayhit.Position - (r.HitVelDir*Math.Min(e.Radius*0.5f, rayhit.HitDist));
                        float hitd = rayhit.HitDist;
                        r.HitT = hitd / absdisp;
                        if (r.HitT > 1.0f)
                        {
                            r.HitT = 1.0f;
                            sph.Center = r.HitPos + e.Center; //this really shouldn't happen... but just in case of glancing hit..
                        }

                        r.SphereHit = SphereIntersect(sph, CollisionLayers); //this really should be a hit!
                    }
                }
            }
            
            if (r.SphereHit.Hit)
            {
                int maxiter = 6;//(would be better to iterate until error within tolerance..)
                int curiter = 0;
                float curt = r.HitT * 0.5f;
                float step = curt * 0.5f;
                float minstep = 0.05f;
                while (curiter < maxiter) //iterate to find a closer hit time... improve this!
                {
                    sph.Center = sphpos + disp * curt;
                    var tcollres = SphereIntersect(sph, CollisionLayers);
                    if (tcollres.Hit)
                    {
                        r.HitT = curt;
                        r.HitPos = sph.Center - e.Center;
                        r.SphereHit = tcollres; //only use the best hit (ignore misses)
                        r.HitNumber = curiter;
                    }
                    else
                    {
                        r.PreT = curt;
                        r.PrePos = sph.Center - e.Center;
                    }
                    curiter++;
                    if (curiter < maxiter)
                    {
                        curt += step * (tcollres.Hit ? -1.0f : 1.0f);
                        step *= 0.5f;
                    }
                    if (absdisp * step < minstep)
                    {
                        break;
                    }
                }
            }

            r.Hit = r.SphereHit.Hit;

            return r;
        }


        public void AddTemporaryEntity(Entity e)
        {
            e.Space = this;
            while (TemporaryEntities.Count > 100)
            {
                TemporaryEntities.RemoveFirst();//don't be too laggy
            }
            TemporaryEntities.AddLast(e);
        }

        public void AddPersistentEntity(Entity e)
        {
            e.Space = this;
            PersistentEntities.AddLast(e);
        }

        public void RemovePersistentEntity(Entity e)
        {
            PersistentEntities.Remove(e);
        }



        private bool IsYmapAvailable(uint ymaphash, int hour, MetaHash weather)
        {
            MetaHash ymapname = new MetaHash(ymaphash);
            uint ymaptime;
            MetaHash[] weathers;
            if ((hour >= 0) && (hour <= 23))
            {
                if (ymaptimes.TryGetValue(ymapname, out ymaptime))
                {
                    uint mask = 1u << hour;
                    if ((ymaptime & mask) == 0) return false;
                }
            }
            if (weather.Hash != 0)
            {
                if (ymapweathertypes.TryGetValue(ymapname, out weathers))
                {
                    for (int i = 0; i < weathers.Length; i++)
                    {
                        if (weathers[i] == weather) return true;
                    }
                    return false;
                }
            }
            return true;
        }

        public void GetVisibleYmaps(Camera cam, int hour, MetaHash weather, Dictionary<MetaHash, YmapFile> ymaps)
        {
            if (!Inited) return;
            if (MapDataStore == null) return;
            CurrentHour = hour;
            CurrentWeather = weather;
            var items = MapDataStore.GetItems(ref cam.Position);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                var hash = item.Name;
                if (!ymaps.ContainsKey(hash))
                {
                    var ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
                    while ((ymap != null) && (ymap.Loaded))
                    {
                        if (!IsYmapAvailable(hash, hour, weather)) break;
                        ymaps[hash] = ymap;
                        hash = ymap._CMapData.parent;
                        if (ymaps.ContainsKey(hash)) break;
                        ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
                    }
                }
            }
        }


        public void GetVisibleBounds(Camera cam, int gridrange, bool[] layers, List<BoundsStoreItem> boundslist)
        {
            if (!Inited) return;

            if (BoundsStore == null) return;
            float dist = 50.0f * gridrange;
            var pos = cam.Position;
            var min = pos - dist;
            var max = pos + dist;
            var items = BoundsStore.GetItems(ref min, ref max, layers);
            boundslist.AddRange(items);
        }


        public void GetVisibleYnds(Camera cam, List<YndFile> ynds)
        {
            if (!Inited) return;
            if (NodeGrid == null) return;

            //int x = 9;
            //int y = 15; //== nodes489.ynd

            //ynds.Add(NodeGrid.Cells[x, y].Ynd);

            ynds.AddRange(AllYnds.Values);

        }


        public void GetVisibleYnvs(Camera cam, int gridrange, List<YnvFile> ynvs)
        {
            if (!Inited) return;
            if (NavGrid == null) return;


            ynvs.Clear();


            var pos = NavGrid.GetCellPos(cam.Position);
            int minx = Math.Min(Math.Max(pos.X - gridrange, 0), NavGrid.CellCountX-1);
            int maxx = Math.Min(Math.Max(pos.X + gridrange, 0), NavGrid.CellCountX-1);
            int miny = Math.Min(Math.Max(pos.Y - gridrange, 0), NavGrid.CellCountY-1);
            int maxy = Math.Min(Math.Max(pos.Y + gridrange, 0), NavGrid.CellCountY-1);
            for (int x = minx; x <= maxx; x++)
            {
                for (int y = miny; y <= maxy; y++)
                {
                    var cell = NavGrid.GetCell(new Vector2I(x, y));
                    if ((cell != null) && (cell.YnvEntry != null))
                    {
                        var hash = cell.YnvEntry.ShortNameHash;
                        var ynv = (hash > 0) ? GameFileCache.GetYnv(hash) : null;
                        if ((ynv != null) && (ynv.Loaded))
                        {
                            ynvs.Add(ynv);
                        }
                    }
                }
            }


        }


        public SpaceRayIntersectResult RayIntersect(Ray ray, float maxdist = float.MaxValue, bool[] layers = null)
        {
            var res = new SpaceRayIntersectResult();
            if (GameFileCache == null) return res;
            bool testcomplete = true;
            res.HitDist = maxdist;
            var box = new BoundingBox();
            float boxhitdisttest;

            if ((BoundsStore == null) || (MapDataStore == null)) return res;

            var boundslist = BoundsStore.GetItems(ref ray, layers);
            var mapdatalist = MapDataStore.GetItems(ref ray);

            for (int i = 0; i < boundslist.Count; i++)
            {
                var bound = boundslist[i];
                box.Minimum = bound.Min;
                box.Maximum = bound.Max;
                if (ray.Intersects(ref box, out boxhitdisttest))
                {
                    if (boxhitdisttest > res.HitDist)
                    { continue; } //already a closer hit

                    YbnFile ybn = GameFileCache.GetYbn(bound.Name);
                    if (ybn == null)
                    { continue; } //ybn not found?
                    if (!ybn.Loaded)
                    { testcomplete = false; continue; } //ybn not loaded yet...

                    var b = ybn.Bounds;
                    if (b == null)
                    { continue; }

                    var bhit = b.RayIntersect(ref ray, res.HitDist);
                    if (bhit.Hit)
                    {
                        bhit.HitYbn = ybn;
                    }
                    res.TryUpdate(ref bhit);
                }
            }

            for (int i = 0; i < mapdatalist.Count; i++)
            {
                var mapdata = mapdatalist[i];
                if (mapdata == null)
                {
                    continue;
                }
                if ((mapdata.ContentFlags & 1) == 0)
                { continue; } //only test HD ymaps

                box.Minimum = mapdata.entitiesExtentsMin;
                box.Maximum = mapdata.entitiesExtentsMax;
                if (ray.Intersects(ref box, out boxhitdisttest))
                {
                    if (boxhitdisttest > res.HitDist)
                    { continue; } //already a closer hit

                    var hash = mapdata.Name;
                    var ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
                    if ((ymap != null) && (ymap.Loaded) && (ymap.AllEntities != null))
                    {
                        if (!IsYmapAvailable(hash, CurrentHour, CurrentWeather))
                        { continue; }

                        for (int e = 0; e < ymap.AllEntities.Length; e++)
                        {
                            var ent = ymap.AllEntities[e];

                            if (!EntityCollisionsEnabled(ent))
                            { continue; }

                            box.Minimum = ent.BBMin;
                            box.Maximum = ent.BBMax;
                            if (ray.Intersects(ref box, out boxhitdisttest))
                            {
                                if (boxhitdisttest > res.HitDist)
                                { continue; } //already a closer hit

                                if (ent.IsMlo)
                                {
                                    var ihit = RayIntersectInterior(ref ray, ent, res.HitDist);
                                    res.TryUpdate(ref ihit);
                                }
                                else
                                {
                                    var ehit = RayIntersectEntity(ref ray, ent, res.HitDist);
                                    res.TryUpdate(ref ehit);
                                }
                            }
                        }
                    }
                    else if ((ymap != null) && (!ymap.Loaded))
                    {
                        testcomplete = false;
                    }
                }
            }




            if (res.Hit)
            {
                res.Position = ray.Position + ray.Direction * res.HitDist;
            }

            res.TestComplete = testcomplete;

            return res;
        }
        public SpaceRayIntersectResult RayIntersectEntity(ref Ray ray, YmapEntityDef ent, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            res.HitDist = maxdist;

            var drawable = GameFileCache.TryGetDrawable(ent.Archetype);
            if (drawable != null)
            {
                var eori = ent.Orientation;
                var eorinv = Quaternion.Invert(ent.Orientation);
                var eray = new Ray();
                eray.Position = eorinv.Multiply(ray.Position - ent.Position);
                eray.Direction = eorinv.Multiply(ray.Direction);

                if ((drawable is Drawable sdrawable) && (sdrawable.Bound != null))
                {
                    var dhit = sdrawable.Bound.RayIntersect(ref eray, res.HitDist);
                    if (dhit.Hit)
                    {
                        dhit.Position = eori.Multiply(dhit.Position) + ent.Position;
                        dhit.Normal = eori.Multiply(dhit.Normal);
                    }
                    res.TryUpdate(ref dhit);
                }
                else if (drawable is FragDrawable fdrawable)
                {
                    if (fdrawable.Bound != null)
                    {
                        var fhit = fdrawable.Bound.RayIntersect(ref eray, res.HitDist);
                        if (fhit.Hit)
                        {
                            fhit.Position = eori.Multiply(fhit.Position) + ent.Position;
                            fhit.Normal = eori.Multiply(fhit.Normal);
                        }
                        res.TryUpdate(ref fhit);
                    }
                    var fbound = fdrawable.OwnerFragment?.PhysicsLODGroup?.PhysicsLOD1?.Bound;
                    if (fbound != null)
                    {
                        var fhit = fbound.RayIntersect(ref eray, res.HitDist);//TODO: these probably have extra transforms..!
                        if (fhit.Hit)
                        {
                            fhit.Position = eori.Multiply(fhit.Position) + ent.Position;
                            fhit.Normal = eori.Multiply(fhit.Normal);
                        }
                        res.TryUpdate(ref fhit);
                    }
                }
            }
            if (res.Hit)
            {
                res.HitEntity = ent;
            }

            return res;
        }
        public SpaceRayIntersectResult RayIntersectInterior(ref Ray ray, YmapEntityDef mlo, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            res.HitDist = maxdist;

            if (mlo.Archetype == null)
            { return res; }

            var iori = mlo.Orientation;
            var iorinv = Quaternion.Invert(mlo.Orientation);
            var iray = new Ray();
            iray.Position = iorinv.Multiply(ray.Position - mlo.Position);
            iray.Direction = iorinv.Multiply(ray.Direction);

            var hash = mlo.Archetype.Hash;
            var ybn = GameFileCache.GetYbn(hash);
            if ((ybn != null) && (ybn.Loaded))
            {
                var ihit = ybn.Bounds.RayIntersect(ref iray, res.HitDist);
                if (ihit.Hit)
                {
                    ihit.HitYbn = ybn;
                    ihit.HitEntity = mlo;
                    ihit.Position = iori.Multiply(ihit.Position) + mlo.Position;
                    ihit.Normal = iori.Multiply(ihit.Normal);
                }
                res.TryUpdate(ref ihit);
            }

            var mlodat = mlo.MloInstance;
            if (mlodat == null)
            { return res; }

            var box = new BoundingBox();
            float boxhitdisttest;

            if (mlodat.Entities != null)
            {
                for (int j = 0; j < mlodat.Entities.Length; j++) //should really improve this by using rooms!
                {
                    var intent = mlodat.Entities[j];
                    if (intent.Archetype == null) continue; //missing archetype...

                    if (!EntityCollisionsEnabled(intent))
                    { continue; }

                    box.Minimum = intent.BBMin;
                    box.Maximum = intent.BBMax;
                    if (ray.Intersects(ref box, out boxhitdisttest))
                    {
                        if (boxhitdisttest > res.HitDist)
                        { continue; } //already a closer hit

                        var ehit = RayIntersectEntity(ref ray, intent, res.HitDist);
                        res.TryUpdate(ref ehit);
                    }
                }
            }
            if (mlodat.EntitySets != null)
            {
                for (int e = 0; e < mlodat.EntitySets.Length; e++)
                {
                    var entityset = mlodat.EntitySets[e];
                    if (!entityset.Visible) continue;
                    var entities = entityset.Entities;
                    if (entities == null) continue;
                    for (int i = 0; i < entities.Count; i++) //should really improve this by using rooms!
                    {
                        var intent = entities[i];
                        if (intent.Archetype == null) continue; //missing archetype...

                        if (!EntityCollisionsEnabled(intent))
                        { continue; }

                        box.Minimum = intent.BBMin;
                        box.Maximum = intent.BBMax;
                        if (ray.Intersects(ref box, out boxhitdisttest))
                        {
                            if (boxhitdisttest > res.HitDist)
                            { continue; } //already a closer hit

                            var ehit = RayIntersectEntity(ref ray, intent, res.HitDist);
                            res.TryUpdate(ref ehit);
                        }
                    }
                }
            }

            return res;
        }

        public SpaceSphereIntersectResult SphereIntersect(BoundingSphere sph, bool[] layers = null)
        {
            var res = new SpaceSphereIntersectResult();
            if (GameFileCache == null) return res;
            bool testcomplete = true;
            Vector3 sphmin = sph.Center - sph.Radius;
            Vector3 sphmax = sph.Center + sph.Radius;
            var box = new BoundingBox();

            if ((BoundsStore == null) || (MapDataStore == null)) return res;

            var boundslist = BoundsStore.GetItems(ref sphmin, ref sphmax, layers);
            var mapdatalist = MapDataStore.GetItems(ref sphmin, ref sphmax);

            for (int i = 0; i < boundslist.Count; i++)
            {
                var bound = boundslist[i];
                box.Minimum = bound.Min;
                box.Maximum = bound.Max;
                if (sph.Intersects(ref box))
                {
                    YbnFile ybn = GameFileCache.GetYbn(bound.Name);
                    if (ybn == null)
                    { continue; } //ybn not found?
                    if (!ybn.Loaded)
                    { testcomplete = false; continue; } //ybn not loaded yet...

                    var b = ybn.Bounds;
                    if (b == null)
                    { continue; }

                    var bhit = b.SphereIntersect(ref sph);
                    res.TryUpdate(ref bhit);
                }
            }

            for (int i = 0; i < mapdatalist.Count; i++)
            {
                var mapdata = mapdatalist[i];
                if ((mapdata.ContentFlags & 1) == 0)
                { continue; } //only test HD ymaps

                box.Minimum = mapdata.entitiesExtentsMin;
                box.Maximum = mapdata.entitiesExtentsMax;
                if (sph.Intersects(ref box))
                {
                    var hash = mapdata.Name;
                    var ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
                    if ((ymap != null) && (ymap.Loaded) && (ymap.AllEntities != null))
                    {
                        if (!IsYmapAvailable(hash, CurrentHour, CurrentWeather))
                        { continue; }

                        for (int e = 0; e < ymap.AllEntities.Length; e++)
                        {
                            var ent = ymap.AllEntities[e];

                            if (!EntityCollisionsEnabled(ent))
                            { continue; }

                            box.Minimum = ent.BBMin;
                            box.Maximum = ent.BBMax;
                            if (sph.Intersects(ref box))
                            {
                                if (ent.IsMlo)
                                {
                                    var ihit = SphereIntersectInterior(ref sph, ent);
                                    res.TryUpdate(ref ihit);
                                }
                                else
                                {
                                    var ehit = SphereIntersectEntity(ref sph, ent);
                                    res.TryUpdate(ref ehit);
                                }
                            }
                        }
                    }
                    else if ((ymap != null) && (!ymap.Loaded))
                    {
                        testcomplete = false;
                    }
                }
            }


            //if (hit)
            //{
            //    hitpos = ray.Position + ray.Direction * itemhitdist;
            //}

            res.TestComplete = testcomplete;

            return res;
        }
        public SpaceSphereIntersectResult SphereIntersectEntity(ref BoundingSphere sph, YmapEntityDef ent)
        {
            var res = new SpaceSphereIntersectResult();

            var drawable = GameFileCache.TryGetDrawable(ent.Archetype);
            if (drawable != null)
            {
                var eori = ent.Orientation;
                var eorinv = Quaternion.Invert(ent.Orientation);
                var esph = sph;
                esph.Center = eorinv.Multiply(sph.Center - ent.Position);

                if ((drawable is Drawable sdrawable) && (sdrawable.Bound != null))
                {
                    var dhit = sdrawable.Bound.SphereIntersect(ref esph);
                    if (dhit.Hit)
                    {
                        dhit.Position = eori.Multiply(dhit.Position) + ent.Position;
                        dhit.Normal = eori.Multiply(dhit.Normal);
                    }
                    res.TryUpdate(ref dhit);
                }
                else if (drawable is FragDrawable fdrawable)
                {
                    if (fdrawable.Bound != null)
                    {
                        var fhit = fdrawable.Bound.SphereIntersect(ref esph);
                        if (fhit.Hit)
                        {
                            fhit.Position = eori.Multiply(fhit.Position) + ent.Position;
                            fhit.Normal = eori.Multiply(fhit.Normal);
                        }
                        res.TryUpdate(ref fhit);
                    }
                    var fbound = fdrawable.OwnerFragment?.PhysicsLODGroup?.PhysicsLOD1?.Bound;
                    if (fbound != null)
                    {
                        var fhit = fbound.SphereIntersect(ref esph);//TODO: these probably have extra transforms..!
                        if (fhit.Hit)
                        {
                            fhit.Position = eori.Multiply(fhit.Position) + ent.Position;
                            fhit.Normal = eori.Multiply(fhit.Normal);
                        }
                        res.TryUpdate(ref fhit);
                    }
                }
            }

            return res;
        }
        public SpaceSphereIntersectResult SphereIntersectInterior(ref BoundingSphere sph, YmapEntityDef mlo)
        {
            var res = new SpaceSphereIntersectResult();

            if (mlo.Archetype == null)
            { return res; }

            var iori = mlo.Orientation;
            var iorinv = Quaternion.Invert(mlo.Orientation);
            var isph = sph;
            isph.Center = iorinv.Multiply(sph.Center - mlo.Position);

            var hash = mlo.Archetype.Hash;
            var ybn = GameFileCache.GetYbn(hash);
            if ((ybn != null) && (ybn.Loaded))
            {
                var ihit = ybn.Bounds.SphereIntersect(ref isph);
                if (ihit.Hit)
                {
                    ihit.Position = iori.Multiply(ihit.Position) + mlo.Position;
                    ihit.Normal = iori.Multiply(ihit.Normal);
                }
                res.TryUpdate(ref ihit);
            }

            var mlodat = mlo.MloInstance;
            if (mlodat == null)
            { return res; }

            var box = new BoundingBox();

            if (mlodat.Entities != null)
            {
                for (int j = 0; j < mlodat.Entities.Length; j++) //should really improve this by using rooms!
                {
                    var intent = mlodat.Entities[j];
                    if (intent.Archetype == null) continue; //missing archetype...

                    if (!EntityCollisionsEnabled(intent))
                    { continue; }

                    box.Minimum = intent.BBMin;
                    box.Maximum = intent.BBMax;
                    if (sph.Intersects(ref box))
                    {
                        var ehit = SphereIntersectEntity(ref sph, intent);
                        res.TryUpdate(ref ehit);
                    }
                }
            }
            if (mlodat.EntitySets != null)
            {
                for (int e = 0; e < mlodat.EntitySets.Length; e++)
                {
                    var entityset = mlodat.EntitySets[e];
                    if (!entityset.Visible) continue;
                    var entities = entityset.Entities;
                    if (entities == null) continue;
                    for (int i = 0; i < entities.Count; i++) //should really improve this by using rooms!
                    {
                        var intent = entities[i];
                        if (intent.Archetype == null) continue; //missing archetype...

                        if (!EntityCollisionsEnabled(intent))
                        { continue; }

                        box.Minimum = intent.BBMin;
                        box.Maximum = intent.BBMax;
                        if (sph.Intersects(ref box))
                        {
                            var ehit = SphereIntersectEntity(ref sph, intent);
                            res.TryUpdate(ref ehit);
                        }
                    }
                }
            }

            return res;
        }

        private bool EntityCollisionsEnabled(YmapEntityDef ent)
        {
            if ((ent._CEntityDef.lodLevel != rage__eLodType.LODTYPES_DEPTH_ORPHANHD) && (ent._CEntityDef.lodLevel != rage__eLodType.LODTYPES_DEPTH_HD))
            { return false; } //only test HD entities

            if ((ent._CEntityDef.flags & 4) > 0)
            { return false; } //embedded collisions disabled

            return true;
        }

    }



    public struct SpaceBoundsKey
    {
        public MetaHash Name { get; set; }
        public Vector3 Position { get; set; }
        public SpaceBoundsKey(MetaHash name, Vector3 position)
        {
            Name = name;
            Position = position;
        }
    }


    public class SpaceMapDataStore
    {
        public SpaceMapDataStoreNode RootNode;
        public int SplitThreshold = 10;

        public List<MapDataStoreNode> VisibleItems = new List<MapDataStoreNode>();

        public void Init(List<MapDataStoreNode> rootnodes)
        {
            RootNode = new SpaceMapDataStoreNode();
            RootNode.Owner = this;
            foreach (var item in rootnodes)
            {
                RootNode.Add(item);
            }
            RootNode.TrySplit(SplitThreshold);
        }

        public List<MapDataStoreNode> GetItems(ref Vector3 p) //get items at a point, using the streaming extents
        {
            VisibleItems.Clear();

            if (RootNode != null)
            {
                RootNode.GetItems(ref p, VisibleItems);
            }

            return VisibleItems;
        }
        public List<MapDataStoreNode> GetItems(ref Vector3 min, ref Vector3 max) //get items intersecting a box, using the entities extents
        {
            VisibleItems.Clear();

            if (RootNode != null)
            {
                RootNode.GetItems(ref min, ref max, VisibleItems);
            }

            return VisibleItems;
        }
        public List<MapDataStoreNode> GetItems(ref Ray ray) //get items intersecting a ray, using the entities extents
        {
            VisibleItems.Clear();

            if (RootNode != null)
            {
                RootNode.GetItems(ref ray, VisibleItems);
            }

            return VisibleItems;
        }
    }
    public class SpaceMapDataStoreNode
    {
        public SpaceMapDataStore Owner = null;
        public SpaceMapDataStoreNode[] Children = null;
        public List<MapDataStoreNode> Items = null;
        public Vector3 BBMin = new Vector3(float.MaxValue);
        public Vector3 BBMax = new Vector3(float.MinValue);
        public int Depth = 0;

        public void Add(MapDataStoreNode item)
        {
            if (Items == null)
            {
                Items = new List<MapDataStoreNode>();
            }
            BBMin = Vector3.Min(BBMin, item.streamingExtentsMin);
            BBMax = Vector3.Max(BBMax, item.streamingExtentsMax);
            Items.Add(item);
        }

        public void TrySplit(int threshold)
        {
            if ((Items == null) || (Items.Count <= threshold))
            { return; }

            Children = new SpaceMapDataStoreNode[4];

            var newItems = new List<MapDataStoreNode>();

            var ncen = (BBMax + BBMin) * 0.5f;
            var next = (BBMax - BBMin) * 0.5f;
            var nsiz = Math.Max(next.X, next.Y);
            var nsizh = nsiz * 0.5f;

            foreach (var item in Items)
            {
                var imin = item.streamingExtentsMin;
                var imax = item.streamingExtentsMax;
                var icen = (imax + imin) * 0.5f;
                var iext = (imax - imin) * 0.5f;
                var isiz = Math.Max(iext.X, iext.Y);

                if (isiz >= nsizh)
                {
                    newItems.Add(item);
                }
                else
                {
                    var cind = ((icen.X > ncen.X) ? 1 : 0) + ((icen.Y > ncen.Y) ? 2 : 0);
                    var c = Children[cind];
                    if (c == null)
                    {
                        c = new SpaceMapDataStoreNode();
                        c.Owner = Owner;
                        c.Depth = Depth + 1;
                        Children[cind] = c;
                    }
                    c.Add(item);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                var c = Children[i];
                if (c != null)
                {
                    c.TrySplit(threshold);
                }
            }

            Items = newItems;
        }

        public void GetItems(ref Vector3 p, List<MapDataStoreNode> items) //get items at a point, using the streaming extents
        {
            if ((p.X >= BBMin.X) && (p.X <= BBMax.X) && (p.Y >= BBMin.Y) && (p.Y <= BBMax.Y))
            {
                if (Items != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i];
                        var imin = item.streamingExtentsMin;
                        var imax = item.streamingExtentsMax;
                        if ((p.X >= imin.X) && (p.X <= imax.X) && (p.Y >= imin.Y) && (p.Y <= imax.Y))
                        {
                            items.Add(item);
                        }
                    }
                }
                if (Children != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var c = Children[i];
                        if (c != null)
                        {
                            c.GetItems(ref p, items);
                        }
                    }
                }
            }
        }
        public void GetItems(ref Vector3 min, ref Vector3 max, List<MapDataStoreNode> items) //get items intersecting a box, using the entities extents
        {
            if ((max.X >= BBMin.X) && (min.X <= BBMax.X) && (max.Y >= BBMin.Y) && (min.Y <= BBMax.Y))
            {
                if (Items != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i];
                        var imin = item.entitiesExtentsMin;
                        var imax = item.entitiesExtentsMax;
                        if ((max.X >= imin.X) && (min.X <= imax.X) && (max.Y >= imin.Y) && (min.Y <= imax.Y))
                        {
                            items.Add(item);
                        }
                    }
                }
                if (Children != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var c = Children[i];
                        if (c != null)
                        {
                            c.GetItems(ref min, ref max, items);
                        }
                    }
                }
            }
        }
        public void GetItems(ref Ray ray, List<MapDataStoreNode> items) //get items intersecting a ray, using the entities extents
        {
            var bb = new BoundingBox(BBMin, BBMax);
            if (ray.Intersects(ref bb))
            {
                if (Items != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i];
                        bb.Minimum = item.entitiesExtentsMin;
                        bb.Maximum = item.entitiesExtentsMax;
                        if (ray.Intersects(ref bb))
                        {
                            items.Add(item);
                        }
                    }
                }
                if (Children != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var c = Children[i];
                        if (c != null)
                        {
                            c.GetItems(ref ray, items);
                        }
                    }
                }
            }
        }
    }


    public class SpaceBoundsStore
    {
        public SpaceBoundsStoreNode RootNode;
        public int SplitThreshold = 10;

        public List<BoundsStoreItem> VisibleItems = new List<BoundsStoreItem>();

        public void Init(List<BoundsStoreItem> items)
        {
            RootNode = new SpaceBoundsStoreNode();
            RootNode.Owner = this;
            foreach (var item in items)
            {
                RootNode.Add(item);
            }
            RootNode.TrySplit(SplitThreshold);
        }

        public List<BoundsStoreItem> GetItems(ref Vector3 min, ref Vector3 max, bool[] layers = null)
        {
            VisibleItems.Clear();

            if (RootNode != null)
            {
                RootNode.GetItems(ref min, ref max, VisibleItems, layers);
            }

            return VisibleItems;
        }
        public List<BoundsStoreItem> GetItems(ref Ray ray, bool[] layers = null)
        {
            VisibleItems.Clear();

            if (RootNode != null)
            {
                RootNode.GetItems(ref ray, VisibleItems, layers);
            }

            return VisibleItems;
        }
    }
    public class SpaceBoundsStoreNode
    {
        public SpaceBoundsStore Owner = null;
        public SpaceBoundsStoreNode[] Children = null;
        public List<BoundsStoreItem> Items = null;
        public Vector3 BBMin = new Vector3(float.MaxValue);
        public Vector3 BBMax = new Vector3(float.MinValue);
        public int Depth = 0;

        public void Add(BoundsStoreItem item)
        {
            if (Items == null)
            {
                Items = new List<BoundsStoreItem>();
            }
            BBMin = Vector3.Min(BBMin, item.Min);
            BBMax = Vector3.Max(BBMax, item.Max);
            Items.Add(item);
        }

        public void TrySplit(int threshold)
        {
            if ((Items == null) || (Items.Count <= threshold))
            { return; }

            Children = new SpaceBoundsStoreNode[4];

            var newItems = new List<BoundsStoreItem>();

            var ncen = (BBMax + BBMin) * 0.5f;
            var next = (BBMax - BBMin) * 0.5f;
            var nsiz = Math.Max(next.X, next.Y);
            var nsizh = nsiz * 0.5f;

            foreach (var item in Items)
            {
                var imin = item.Min;
                var imax = item.Max;
                var icen = (imax + imin) * 0.5f;
                var iext = (imax - imin) * 0.5f;
                var isiz = Math.Max(iext.X, iext.Y);

                if (isiz >= nsizh)
                {
                    newItems.Add(item);
                }
                else
                {
                    var cind = ((icen.X > ncen.X) ? 1 : 0) + ((icen.Y > ncen.Y) ? 2 : 0);
                    var c = Children[cind];
                    if (c == null)
                    {
                        c = new SpaceBoundsStoreNode();
                        c.Owner = Owner;
                        c.Depth = Depth + 1;
                        Children[cind] = c;
                    }
                    c.Add(item);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                var c = Children[i];
                if (c != null)
                {
                    c.TrySplit(threshold);
                }
            }

            Items = newItems;
        }

        public void GetItems(ref Vector3 min, ref Vector3 max, List<BoundsStoreItem> items, bool[] layers = null)
        {
            if ((max.X >= BBMin.X) && (min.X <= BBMax.X) && (max.Y >= BBMin.Y) && (min.Y <= BBMax.Y))
            {
                if (Items != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i];

                        if ((layers != null) && (item.Layer < 3) && (!layers[item.Layer]))
                        { continue; }

                        if ((max.X >= item.Min.X) && (min.X <= item.Max.X) && (max.Y >= item.Min.Y) && (min.Y <= item.Max.Y))
                        {
                            items.Add(item);
                        }
                    }
                }
                if (Children != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var c = Children[i];
                        if (c != null)
                        {
                            c.GetItems(ref min, ref max, items, layers);
                        }
                    }
                }
            }
        }
        public void GetItems(ref Ray ray, List<BoundsStoreItem> items, bool[] layers = null)
        {
            var box = new BoundingBox(BBMin, BBMax);
            if (ray.Intersects(ref box))
            {
                if (Items != null)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var item = Items[i];

                        if ((layers != null) && (item.Layer < 3) && (!layers[item.Layer]))
                        { continue; }

                        box = new BoundingBox(item.Min, item.Max);
                        if (ray.Intersects(box))
                        {
                            items.Add(item);
                        }
                    }
                }
                if (Children != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var c = Children[i];
                        if (c != null)
                        {
                            c.GetItems(ref ray, items, layers);
                        }
                    }
                }
            }
        }
    }


    public class SpaceNodeGrid
    {
        //node grid for V paths
        public SpaceNodeGridCell[,] Cells { get; set; }
        public float CellSize = 512.0f;
        public float CellSizeInv; //inverse of the cell size.
        public int CellCountX = 32;
        public int CellCountY = 32;
        public float CornerX = -8192.0f;
        public float CornerY = -8192.0f;

        public SpaceNodeGrid()
        {
            CellSizeInv = 1.0f / CellSize;

            Cells = new SpaceNodeGridCell[CellCountX, CellCountY];

            for (int x = 0; x < CellCountX; x++)
            {
                for (int y = 0; y < CellCountY; y++)
                {
                    Cells[x, y] = new SpaceNodeGridCell(x, y);
                }
            }
        }

        public SpaceNodeGridCell GetCell(int id)
        {
            int x = id % CellCountX;
            int y = id / CellCountX;
            if ((x >= 0) && (x < CellCountX) && (y >= 0) && (y < CellCountY))
            {
                return Cells[x, y];
            }
            return null;
        }

        public SpaceNodeGridCell GetCellForPosition(Vector3 position)
        {
            var x = (int)((position.X - CornerX) / CellSize);
            var y = (int)((position.Y - CornerY) / CellSize);

            if ((x >= 0) && (x < CellCountX) && (y >= 0) && (y < CellCountY))
            {
                return Cells[x, y];
            }

            return null;
        }


        public YndNode GetYndNode(ushort areaid, ushort nodeid)
        {
            var cell = GetCell(areaid);
            if ((cell == null) || (cell.Ynd == null) || (cell.Ynd.Nodes == null))
            { return null; }
            if (nodeid >= cell.Ynd.Nodes.Length)
            { return null; }
            return cell.Ynd.Nodes[nodeid];
        }

        public void UpdateYnd(YndFile ynd)
        {
            for (int xx = 0; xx < Cells.GetLength(0); xx++)
            {
                for (int yy = 0; yy < Cells.GetLength(1); yy++)
                {
                    if (Cells[xx, yy].Ynd == ynd)
                    {
                        Cells[xx, yy].Ynd = null;
                    }
                }
            }

            var x = ynd.CellX;
            var y = ynd.CellY;
            Cells[x, y].Ynd = ynd;
        }
    }
    public class SpaceNodeGridCell
    {
        public int X;
        public int Y;
        public int ID;

        public YndFile Ynd;

        public SpaceNodeGridCell(int x, int y)
        {
            X = x;
            Y = y;
            ID = y * 32 + x;
        }

    }


    public class SpaceNavGrid
    {
        //grid for V navmeshes
        public SpaceNavGridCell[,] Cells { get; set; }
        public float CellSize = 150.0f;
        public float CellSizeInv; //inverse of the cell size.
        public int CellCountX = 100;
        public int CellCountY = 100;
        public float CornerX = -6000.0f;//max = -6000+(100*150) = 9000
        public float CornerY = -6000.0f;

        public SpaceNavGrid()
        {
            CellSizeInv = 1.0f / CellSize;

            Cells = new SpaceNavGridCell[CellCountX, CellCountY];

            for (int x = 0; x < CellCountX; x++)
            {
                for (int y = 0; y < CellCountY; y++)
                {
                    Cells[x, y] = new SpaceNavGridCell(x, y);
                }
            }
        }

        public SpaceNavGridCell GetCell(int id)
        {
            int x = id % CellCountX;
            int y = id / CellCountX;
            if ((x >= 0) && (x < CellCountX) && (y >= 0) && (y < CellCountY))
            {
                return Cells[x, y];
            }
            return null;
        }


        public Vector3 GetCellRel(Vector3 p)//float value in cell coords
        {
            return (p - new Vector3(CornerX, CornerY, 0)) * CellSizeInv;
        }
        public Vector2I GetCellPos(Vector3 p)
        {
            Vector3 ind = (p - new Vector3(CornerX, CornerY, 0)) * CellSizeInv;
            int x = (int)ind.X;
            int y = (int)ind.Y;
            x = (x < 0) ? 0 : (x >= CellCountX) ? CellCountX-1 : x;
            y = (y < 0) ? 0 : (y >= CellCountY) ? CellCountY-1 : y;
            return new Vector2I(x, y);
        }
        public SpaceNavGridCell GetCell(Vector2I g)
        {
            var cell = Cells[g.X, g.Y];
            if (cell == null)
            {
                //cell = new SpaceNavGridCell(g.X, g.Y);
                //Cells[g.X, g.Y] = cell;
            }
            return cell;
        }
        public SpaceNavGridCell GetCell(Vector3 p)
        {
            return GetCell(GetCellPos(p));
        }


        public Vector3 GetCellMin(SpaceNavGridCell cell)
        {
            Vector3 c = new Vector3(cell.X, cell.Y, 0);
            return new Vector3(CornerX, CornerY, 0) + (c * CellSize);
        }
        public Vector3 GetCellMax(SpaceNavGridCell cell)
        {
            return GetCellMin(cell) + new Vector3(CellSize, CellSize, 0.0f);
        }

    }
    public class SpaceNavGridCell
    {
        public int X;
        public int Y;
        public int ID;
        public int FileX;
        public int FileY;

        public RpfResourceFileEntry YnvEntry;
        public YnvFile Ynv;

        public SpaceNavGridCell(int x, int y)
        {
            X = x;
            Y = y;
            ID = y * 100 + x;
            FileX = x * 3;
            FileY = y * 3;
        }

    }



    public struct SpaceRayIntersectResult
    {
        public bool Hit;
        public float HitDist;
        public BoundVertexRef HitVertex;
        public BoundPolygon HitPolygon;
        public Bounds HitBounds;
        public YbnFile HitYbn;
        public YmapEntityDef HitEntity;
        public Vector3 Position;
        public Vector3 Normal;
        public int TestedNodeCount;
        public int TestedPolyCount;
        public bool TestComplete;
        public BoundMaterial_s Material;

        public void TryUpdate(ref SpaceRayIntersectResult r)
        {
            if (r.Hit)
            {
                Hit = true;
                HitDist = r.HitDist;
                HitVertex = r.HitVertex;
                HitPolygon = r.HitPolygon;
                HitBounds = r.HitBounds;
                HitYbn = r.HitYbn;
                HitEntity = r.HitEntity;
                Material = r.Material;
                Position = r.Position;
                Normal = r.Normal;
            }
            TestedNodeCount += r.TestedNodeCount;
            TestedPolyCount += r.TestedPolyCount;
        }
    }
    public struct SpaceSphereIntersectResult
    {
        public bool Hit;
        public float HitDist;
        public BoundPolygon HitPolygon;
        public Vector3 Position;
        public Vector3 Normal;
        public int TestedNodeCount;
        public int TestedPolyCount;
        public bool TestComplete;

        public void TryUpdate(ref SpaceSphereIntersectResult r)
        {
            if (r.Hit)
            {
                Hit = true;
                HitPolygon = r.HitPolygon;
                Normal = r.Normal;
            }
            TestedPolyCount += r.TestedPolyCount;
            TestedNodeCount += r.TestedNodeCount;
        }
    }

    public struct SpaceEntityCollision
    {
        public Entity Entity; //the entity owning this collision
        public Entity Entity2; //second entity, if this is a collision between two entities
        public SpaceSphereIntersectResult SphereHit; //details of the sphere intersection point
        public Vector3 PrePos; //last known position before hit
        public float PreT; //last known T before hit
        public float HitT; //fraction of the frame (0-1)
        public Vector3 HitPos; //position of the sphere center at hit point
        public Quaternion HitRot; //rotation of the entity at hit point
        public Vector3 HitVel; //velocity of the entity for this hit
        public Vector3 HitAngVel; //angular velocity of the entity for this hit
        public int HitNumber; //count of previous iterations
        public bool Hit;
        public Vector3 HitVelDir;
    }

}
