using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.World
{
    public class Space
    {

        public LinkedList<Entity> TemporaryEntities = new LinkedList<Entity>();
        public LinkedList<Entity> PersistentEntities = new LinkedList<Entity>();
        public List<Entity> EnabledEntities = new List<Entity>(); //built each frame

        private GameFileCache GameFileCache = null;


        public SpaceGrid Grid;
        private Dictionary<SpaceBoundsKey, BoundsStoreItem> visibleBoundsDict = new Dictionary<SpaceBoundsKey, BoundsStoreItem>();

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


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            GameFileCache = gameFileCache;


            updateStatus("Scanning manifests...");

            InitManifestData();


            updateStatus("Scanning caches...");

            InitCacheData();


            updateStatus("Building world grid...");

            InitMapGrid();


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
            //    if (dg.YBNHashes_3298223272 != null)
            //    {
            //        foreach (var ybnh in dg.YBNHashes_3298223272)
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

        private void InitMapGrid()
        {

            Grid = new SpaceGrid();

            //List<MapDataStoreNode> containers = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> critnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> hdnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> lodnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> strmnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> intnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> occlnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> grassnodes = new List<MapDataStoreNode>();
            //List<MapDataStoreNode> lodlightsnodes = new List<MapDataStoreNode>();

            foreach (var node in nodedict.Values)
            {
                bool addtogrid = false;
                byte t = (byte)(node.ContentFlags & 0xFF);
                switch (node.ContentFlags)// t)
                {
                    case 0: //for mods/unused stuff? could be interesting.
                        addtogrid = true;
                        break;
                    case 16://"container" node?
                        //containers.Add(node);
                        break;
                    case 18:
                    case 82: //HD nodes
                        //hdnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 1:
                    case 65: //Stream nodes
                        //strmnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 513:
                    case 577: //critical nodes
                        //critnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 9:
                    case 73: //interior nodes
                        //intnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 2:
                    case 4:
                    case 20:
                    case 66:
                    case 514: //LOD nodes
                        //lodnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 128:
                    case 256: //LOD lights nodes
                        //lodlightsnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 32: //occlusion nodes
                        //occlnodes.Add(node);
                        addtogrid = true;
                        break;
                    case 1088: //grass nodes
                        //grassnodes.Add(node);
                        addtogrid = true;
                        break;
                    default:
                        addtogrid = true;
                        break;
                }


                if (addtogrid)
                {
                    Grid.AddNode(node);
                }

            }


            foreach (var item in boundsdict.Values)
            {
                Grid.AddBounds(item);
            }

            foreach (var intprx in interiorProxies.Values)
            {
                Grid.AddInterior(intprx);
            }

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
            foreach (var dlcrpf in GameFileCache.DlcActiveRpfs) //load nodes from current dlc rpfs
            {
                foreach (var rpffile in dlcrpf.Children)
                {
                    AddRpfYnds(rpffile, yndentries);
                }
            }
            var updrpf = rpfman.FindRpfFile("update\\update.rpf"); //load nodes from patch area...
            if (updrpf != null)
            {
                foreach (var rpffile in updrpf.Children)
                {
                    AddRpfYnds(rpffile, yndentries);
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
        public void BuildYndVerts(YndFile ynd, List<EditorVertex> tverts = null)
        {
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
                    var tnode = yl.Node2;
                    if (tnode == null) continue; //invalid links could hit here
                    var tvert = new EditorVertex();
                    tvert.Position = tnode.Position;
                    tvert.Colour = (uint)tnode.Colour.ToRgba();

                    tverts.Add(nvert);
                    tverts.Add(tvert);
                }
            }
            ynd.LinkedVerts = tverts.ToArray();


            ynd.UpdateTriangleVertices();
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

            BuildYndVerts(ynd, tverts);

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
            foreach (var dlcrpf in GameFileCache.DlcActiveRpfs) //load navmeshes from current dlc rpfs
            {
                foreach (var rpffile in dlcrpf.Children)
                {
                    AddRpfYnvs(rpffile, ynventries);
                }
            }
            var updrpf = rpfman.FindRpfFile("update\\update.rpf"); //load navmeshes from patch area...
            if (updrpf != null)
            {
                foreach (var rpffile in updrpf.Children)
                {
                    AddRpfYnvs(rpffile, ynventries);
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
            if (Grid == null) return;

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

            r.SphereHit = SphereIntersect(sph);

            if (!r.SphereHit.Hit)
            {
                if (absdisp > (e.Radius * 2.0f)) //fast-moving... do a ray test to make sure it's not tunnelling
                {
                    Ray rayt = new Ray(sphpos, r.HitVelDir);
                    float rayl = absdisp + e.Radius * 4.0f; //include some extra incase of glancing hit
                    var rayhit = RayIntersect(rayt, rayl);
                    if (rayhit.Hit) //looks like it is tunnelling... need to find the sphere hit point
                    {
                        sph.Center = rayhit.Position;
                        float hitd = rayhit.HitDist;
                        r.HitT = hitd / absdisp;
                        if (r.HitT > 1.0f)
                        {
                            r.HitT = 1.0f;
                            sph.Center = r.HitPos + e.Center; //this really shouldn't happen... but just in case of glancing hit..
                        }

                        r.SphereHit = SphereIntersect(sph); //this really should be a hit!
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
                    var tcollres = SphereIntersect(sph);
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
            if (Grid == null) return;


            ymaps.Clear();

            //var pos = Grid.GetCellPos(cam.Position);
            var cell = Grid.GetCell(cam.Position);
            if (cell.NodesList != null)
            {
                for (int n = 0; n < cell.NodesList.Count; n++)
                {
                    var node = cell.NodesList[n];
                    var hash = node.Name;
                    if (!ymaps.ContainsKey(hash))
                    {
                        var ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
                        while ((ymap != null) && (ymap.Loaded))
                        {
                            if (!IsYmapAvailable(hash, hour, weather)) break;
                            ymaps[hash] = ymap;
                            hash = ymap.CMapData.parent;
                            if (ymaps.ContainsKey(hash)) break;
                            ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
                        }
                    }
                }
            }



            //int gridrange = 0;

            //var pos = Grid.GetCellPos(cam.Position);
            //int minx = Math.Min(Math.Max(pos.X - gridrange, 0), SpaceGrid.LastCell);
            //int maxx = Math.Min(Math.Max(pos.X + gridrange, 0), SpaceGrid.LastCell);
            //int miny = Math.Min(Math.Max(pos.Y - gridrange, 0), SpaceGrid.LastCell);
            //int maxy = Math.Min(Math.Max(pos.Y + gridrange, 0), SpaceGrid.LastCell);

            //for (int x = minx; x <= maxx; x++)
            //{
            //    for (int y = miny; y <= maxy; y++)
            //    {
            //        var cell = Grid.GetCell(new Vector2I(x, y));
            //        if (cell.NodesList != null)
            //        {
            //            for (int n = 0; n < cell.NodesList.Count; n++)
            //            {
            //                var node = cell.NodesList[n];
            //                var hash = node.Name;
            //                if (!ymaps.ContainsKey(hash))
            //                {
            //                    var ymap = (hash > 0) ? GameFileCache.GetYmap(hash) : null;
            //                    if ((ymap != null) && (ymap.Loaded))
            //                    {
            //                        ymaps[hash] = ymap;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

        }


        public void GetVisibleBounds(Camera cam, int gridrange, bool[] layers, List<BoundsStoreItem> boundslist)
        {
            if (!Inited) return;
            if (Grid == null) return;

            visibleBoundsDict.Clear();

            var pos = Grid.GetCellPos(cam.Position);
            int minx = Math.Min(Math.Max(pos.X - gridrange, 0), SpaceGrid.LastCell);
            int maxx = Math.Min(Math.Max(pos.X + gridrange, 0), SpaceGrid.LastCell);
            int miny = Math.Min(Math.Max(pos.Y - gridrange, 0), SpaceGrid.LastCell);
            int maxy = Math.Min(Math.Max(pos.Y + gridrange, 0), SpaceGrid.LastCell);

            for (int x = minx; x <= maxx; x++)
            {
                for (int y = miny; y <= maxy; y++)
                {
                    var cell = Grid.GetCell(new Vector2I(x, y));
                    if (cell.BoundsList != null)
                    {
                        foreach (var item in cell.BoundsList)
                        {
                            uint l = item.Layer;
                            if (l < 3)
                            {
                                if (!layers[l]) continue;
                            }
                            else
                            { }

                            visibleBoundsDict[new SpaceBoundsKey(item.Name, item.Min)] = item;
                        }
                    }
                }
            }

            //var cell = grid.GetCell(cam.Position);
            //if (cell.BoundsList != null)
            //{
            //    boundslist.AddRange(cell.BoundsList);
            //}

            boundslist.AddRange(visibleBoundsDict.Values);

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


        public void GetVisibleYnvs(Camera cam, List<YnvFile> ynvs)
        {
            if (!Inited) return;
            if (Grid == null) return;


            ynvs.Clear();


            int gridrange = 30;
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


        public SpaceRayIntersectResult RayIntersect(Ray ray, float maxdist = float.MaxValue)
        {
            var res = new SpaceRayIntersectResult();
            if (GameFileCache == null) return res;
            int polytestcount = 0;
            int nodetestcount = 0;
            bool testcomplete = true;
            var cellpos = Grid.GetCellPos(ray.Position);
            var cell = Grid.GetCell(cellpos);
            var startz = ray.Position.Z;
            var maxcells = 5;
            var cellcount = 0;
            var box = new BoundingBox();
            var tsph = new BoundingSphere();
            var rayt = new Ray();
            var rp = ray.Position;
            var rd = ray.Direction;
            float dirx = (rd.X < 0) ? -1 : (rd.X > 0) ? 1 : 0;
            float diry = (rd.Y < 0) ? -1 : (rd.Y > 0) ? 1 : 0;
            var boxhitdist = float.MaxValue;
            var itemhitdist = float.MaxValue;
            Vector3 p1, p2, p3, p4, a1, a2, a3;
            Vector3 n1 = Vector3.Zero;
            float polyhittestdist = 0;
            bool hit = false;
            BoundPolygon hitpoly = null;
            Vector3 hitnorm = Vector3.Zero;
            Vector3 hitpos = Vector3.Zero;
            while (cell != null)
            {
                if (cell.BoundsList != null)
                {
                    foreach (var bound in cell.BoundsList)
                    {
                        box.Minimum = bound.Min;
                        box.Maximum = bound.Max;
                        float boxhitdisttest;
                        if (ray.Intersects(ref box, out boxhitdisttest))
                        {
                            YbnFile ybn = GameFileCache.GetYbn(bound.Name);
                            if (ybn == null)
                            { continue; } //ybn not found?
                            if (!ybn.Loaded)
                            { testcomplete = false; continue; } //ybn not loaded yet...

                            bool ybnhit = false;
                            var b = ybn.Bounds;
                            box.Minimum = b.BoundingBoxMin;
                            box.Maximum = b.BoundingBoxMax;
                            float itemboxhitdisttest;
                            if (!ray.Intersects(ref box, out itemboxhitdisttest))
                            { continue; } //ray doesn't hit this ybn
                            if (itemboxhitdisttest > itemhitdist)
                            { continue; } //already a closer hit.
                            if (itemboxhitdisttest > maxdist)
                            { continue; }

                            switch (b.Type)
                            {
                                case 10: //BoundComposite
                                    BoundComposite boundcomp = b as BoundComposite;
                                    if (boundcomp == null)
                                    { continue; }
                                    var compchilds = boundcomp.Children?.data_items;
                                    if (compchilds == null)
                                    { continue; }
                                    for (int i = 0; i < compchilds.Length; i++)
                                    {
                                        BoundBVH bgeom = compchilds[i] as BoundBVH;
                                        if (bgeom == null)
                                        { continue; }
                                        if (bgeom.Polygons == null)
                                        { continue; }
                                        if ((bgeom.BVH == null) || (bgeom.BVH.Trees == null))
                                        { continue; }

                                        box.Minimum = bgeom.BoundingBoxMin;
                                        box.Maximum = bgeom.BoundingBoxMax;
                                        float bvhboxhittest;
                                        if (!ray.Intersects(ref box, out bvhboxhittest))
                                        { continue; }
                                        if (bvhboxhittest > itemhitdist)
                                        { continue; } //already a closer hit.

                                        var q = bgeom.BVH.Quantum.XYZ();
                                        var c = bgeom.BVH.BoundingBoxCenter.XYZ();
                                        var cg = bgeom.CenterGeom;
                                        for (int t = 0; t < bgeom.BVH.Trees.Length; t++)
                                        {
                                            var tree = bgeom.BVH.Trees[t];
                                            box.Minimum = new Vector3(tree.MinX, tree.MinY, tree.MinZ) * q + c;
                                            box.Maximum = new Vector3(tree.MaxX, tree.MaxY, tree.MaxZ) * q + c;
                                            if (!ray.Intersects(ref box, out bvhboxhittest))
                                            { continue; }
                                            if (bvhboxhittest > itemhitdist)
                                            { continue; } //already a closer hit.
                                            if (bvhboxhittest > maxdist)
                                            { continue; }

                                            int nodeind = tree.NodeIndex1;
                                            int lastind = tree.NodeIndex2;
                                            while (nodeind < lastind)
                                            {
                                                var node = bgeom.BVH.Nodes[nodeind];
                                                box.Minimum = new Vector3(node.MinX, node.MinY, node.MinZ) * q + c;
                                                box.Maximum = new Vector3(node.MaxX, node.MaxY, node.MaxZ) * q + c;
                                                bool nodehit = ray.Intersects(ref box, out bvhboxhittest);
                                                bool nodeskip = !nodehit || (bvhboxhittest > itemhitdist);
                                                if (node.PolyCount <= 0) //intermediate node with child nodes
                                                {
                                                    if (nodeskip)
                                                    {
                                                        nodeind += node.PolyId; //(child node count)
                                                    }
                                                    else
                                                    {
                                                        nodeind++;
                                                    }
                                                }
                                                else //leaf node, with polygons
                                                {
                                                    if (!nodeskip)
                                                    {
                                                        var lastp = node.PolyId + node.PolyCount;
                                                        lastp = Math.Min(lastp, (int)bgeom.PolygonsCount);
                                                        for (int p = node.PolyId; p < lastp; p++)
                                                        {
                                                            var polygon = bgeom.Polygons[p];
                                                            bool polyhit = false;
                                                            switch (polygon.Type)
                                                            {
                                                                case BoundPolygonType.Triangle:
                                                                    var ptri = polygon as BoundPolygonTriangle;
                                                                    p1 = bgeom.GetVertex(ptri.vertIndex1) + cg;
                                                                    p2 = bgeom.GetVertex(ptri.vertIndex2) + cg;
                                                                    p3 = bgeom.GetVertex(ptri.vertIndex3) + cg;
                                                                    polyhit = ray.Intersects(ref p1, ref p2, ref p3, out polyhittestdist);
                                                                    if (polyhit) n1 = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
                                                                    break;
                                                                case BoundPolygonType.Sphere:
                                                                    var psph = polygon as BoundPolygonSphere;
                                                                    tsph.Center = bgeom.GetVertex(psph.sphereIndex) + cg;
                                                                    tsph.Radius = psph.sphereRadius;
                                                                    polyhit = ray.Intersects(ref tsph, out polyhittestdist);
                                                                    if (polyhit) n1 = Vector3.Normalize((ray.Position + ray.Direction * polyhittestdist) - tsph.Center);
                                                                    break;
                                                                case BoundPolygonType.Capsule:
                                                                    //TODO
                                                                    break;
                                                                case BoundPolygonType.Box:
                                                                    var pbox = polygon as BoundPolygonBox;
                                                                    p1 = bgeom.GetVertex(pbox.boxIndex1);// + cg; //corner
                                                                    p2 = bgeom.GetVertex(pbox.boxIndex2);// + cg;
                                                                    p3 = bgeom.GetVertex(pbox.boxIndex3);// + cg;
                                                                    p4 = bgeom.GetVertex(pbox.boxIndex4);// + cg;
                                                                    a1 = ((p3 + p4) - (p1 + p2)) * 0.5f;
                                                                    a2 = p3 - (p1 + a1);
                                                                    a3 = p4 - (p1 + a1);
                                                                    Vector3 bs = new Vector3(a1.Length(), a2.Length(), a3.Length());
                                                                    Vector3 m1 = a1 / bs.X;
                                                                    Vector3 m2 = a2 / bs.Y;
                                                                    Vector3 m3 = a3 / bs.Z;
                                                                    if ((bs.X < bs.Y) && (bs.X < bs.Z)) m1 = Vector3.Cross(m2, m3);
                                                                    else if (bs.Y < bs.Z) m2 = Vector3.Cross(m1, m3);
                                                                    else m3 = Vector3.Cross(m1, m2);
                                                                    Vector3 tp = rp - (p1 + cg);
                                                                    rayt.Position = new Vector3(Vector3.Dot(tp, m1), Vector3.Dot(tp, m2), Vector3.Dot(tp, m3));
                                                                    rayt.Direction = new Vector3(Vector3.Dot(rd, m1), Vector3.Dot(rd, m2), Vector3.Dot(rd, m3));
                                                                    box.Minimum = Vector3.Zero;
                                                                    box.Maximum = bs;
                                                                    polyhit = rayt.Intersects(ref box, out polyhittestdist);
                                                                    if (polyhit)
                                                                    {
                                                                        Vector3 hpt = rayt.Position + rayt.Direction * polyhittestdist;
                                                                        const float eps = 0.002f;
                                                                        if (Math.Abs(hpt.X) < eps) n1 = -m1;
                                                                        else if (Math.Abs(hpt.X - bs.X) < eps) n1 = m1;
                                                                        else if (Math.Abs(hpt.Y) < eps) n1 = -m2;
                                                                        else if (Math.Abs(hpt.Y - bs.Y) < eps) n1 = m2;
                                                                        else if (Math.Abs(hpt.Z) < eps) n1 = -m3;
                                                                        else if (Math.Abs(hpt.Z - bs.Z) < eps) n1 = m3;
                                                                        else
                                                                        { n1 = Vector3.UnitZ; } //ray starts inside the box...
                                                                    }
                                                                    break;
                                                                case BoundPolygonType.Cylinder:
                                                                    //TODO
                                                                    break;
                                                            }
                                                            if (polyhit && (polyhittestdist < itemhitdist) && (polyhittestdist < maxdist))
                                                            {
                                                                itemhitdist = polyhittestdist;
                                                                ybnhit = true;
                                                                hit = true;
                                                                hitnorm = n1;
                                                                hitpoly = polygon;
                                                            }
                                                            polytestcount++;
                                                        }
                                                    }
                                                    nodeind++;
                                                }
                                                nodetestcount++;
                                            }
                                        }
                                    }
                                    break;
                                case 3: //BoundBox - found in drawables - TODO
                                    BoundBox boundbox = b as BoundBox;
                                    if (boundbox == null)
                                    { continue; }
                                    break;
                                case 0: //BoundSphere - found in drawables - TODO
                                    BoundSphere boundsphere = b as BoundSphere;
                                    if (boundsphere == null)
                                    { continue; }
                                    break;
                                default:
                                    break;
                            }


                            if (ybnhit)
                            {
                                boxhitdist = boxhitdisttest;
                                //hit = true;
                            }

                        }
                    }
                }


                //walk the line to the next cell...

                cellcount++;
                if (cellcount >= maxcells) break;
                if ((dirx == 0) && (diry == 0)) break; //vertical ray

                Vector3 cellwp = Grid.GetWorldPos(cellpos);
                float compx = (dirx < 0) ? cellwp.X : cellwp.X + SpaceGrid.CellSize;
                float compy = (diry < 0) ? cellwp.Y : cellwp.Y + SpaceGrid.CellSize;
                float deltx = Math.Abs(compx - rp.X);
                float delty = Math.Abs(compy - rp.Y);
                float nextd = 0;
                if (deltx < delty)
                {
                    cellpos.X += (int)dirx;
                    nextd = (rd.X != 0) ? (deltx / rd.X) : 0;
                }
                else
                {
                    cellpos.Y += (int)diry;
                    nextd = (rd.Y != 0) ? (delty / rd.Y) : 0;
                }

                cell = Grid.GetCell(cellpos);

                if (nextd > itemhitdist)
                { break; } //next cell is further away than current hit.. no need to continue 
                if (nextd > maxdist)
                { break; } //next cell is out of the testing range.. stop now
            }

            if (hit)
            {
                hitpos = ray.Position + ray.Direction * itemhitdist;
            }

            res.TestedNodeCount = nodetestcount;
            res.TestedPolyCount = polytestcount;
            res.TestComplete = testcomplete;
            res.Hit = hit;
            res.HitDist = itemhitdist;
            res.HitPolygon = hitpoly;
            res.Position = hitpos;
            res.Normal = hitnorm;

            return res;
        }

        public SpaceSphereIntersectResult SphereIntersect(BoundingSphere sph)
        {
            var res = new SpaceSphereIntersectResult();
            if (GameFileCache == null) return res;
            int polytestcount = 0;
            int nodetestcount = 0;
            bool testcomplete = true;
            Vector3 sphmin = sph.Center - sph.Radius;
            Vector3 sphmax = sph.Center + sph.Radius;
            var cellmin = Grid.GetCellPos(sphmin);
            var cellmax = Grid.GetCellPos(sphmax);
            var cellcount = 0;
            var box = new BoundingBox();
            var tsph = new BoundingSphere();
            var spht = new BoundingSphere();
            var sp = sph.Center;
            var sr = sph.Radius;
            //var boxhitdist = float.MaxValue;
            //var itemhitdist = float.MaxValue;
            Vector3 p1, p2, p3, p4, a1, a2, a3;
            Vector3 n1 = Vector3.Zero;
            //float polyhittestdist = 0;
            bool hit = false;
            BoundPolygon hitpoly = null;
            Vector3 hitnorm = Vector3.Zero;
            Vector3 hitpos = Vector3.Zero;
            for (int x = cellmin.X; x <= cellmax.X; x++)
            {
                for (int y = cellmin.Y; y <= cellmax.Y; y++)
                {
                    var cell = Grid.GetCell(new Vector2I(x, y));
                    if (cell == null) continue;
                    if (cell.BoundsList == null) continue;

                    foreach (var bound in cell.BoundsList)
                    {
                        box.Minimum = bound.Min;
                        box.Maximum = bound.Max;
                        if (sph.Intersects(ref box))
                        {
                            YbnFile ybn = GameFileCache.GetYbn(bound.Name);
                            if (ybn == null)
                            { continue; } //ybn not found?
                            if (!ybn.Loaded)
                            { testcomplete = false; continue; } //ybn not loaded yet...

                            //bool ybnhit = false;
                            var b = ybn.Bounds;
                            box.Minimum = b.BoundingBoxMin;
                            box.Maximum = b.BoundingBoxMax;
                            if (!sph.Intersects(ref box))
                            { continue; } //ray doesn't hit this ybn

                            switch (b.Type)
                            {
                                case 10: //BoundComposite
                                    BoundComposite boundcomp = b as BoundComposite;
                                    if (boundcomp == null)
                                    { continue; }
                                    var compchilds = boundcomp.Children?.data_items;
                                    if (compchilds == null)
                                    { continue; }
                                    for (int i = 0; i < compchilds.Length; i++)
                                    {
                                        BoundBVH bgeom = compchilds[i] as BoundBVH;
                                        if (bgeom == null)
                                        { continue; }
                                        if (bgeom.Polygons == null)
                                        { continue; }
                                        if ((bgeom.BVH == null) || (bgeom.BVH.Trees == null))
                                        { continue; }

                                        box.Minimum = bgeom.BoundingBoxMin;
                                        box.Maximum = bgeom.BoundingBoxMax;
                                        if (!sph.Intersects(ref box))
                                        { continue; }

                                        var q = bgeom.BVH.Quantum.XYZ();
                                        var c = bgeom.BVH.BoundingBoxCenter.XYZ();
                                        var cg = bgeom.CenterGeom;
                                        for (int t = 0; t < bgeom.BVH.Trees.Length; t++)
                                        {
                                            var tree = bgeom.BVH.Trees[t];
                                            box.Minimum = new Vector3(tree.MinX, tree.MinY, tree.MinZ) * q + c;
                                            box.Maximum = new Vector3(tree.MaxX, tree.MaxY, tree.MaxZ) * q + c;
                                            if (!sph.Intersects(ref box))
                                            { continue; }

                                            int nodeind = tree.NodeIndex1;
                                            int lastind = tree.NodeIndex2;
                                            while (nodeind < lastind)
                                            {
                                                var node = bgeom.BVH.Nodes[nodeind];
                                                box.Minimum = new Vector3(node.MinX, node.MinY, node.MinZ) * q + c;
                                                box.Maximum = new Vector3(node.MaxX, node.MaxY, node.MaxZ) * q + c;
                                                bool nodehit = sph.Intersects(ref box);
                                                bool nodeskip = !nodehit;
                                                if (node.PolyCount <= 0) //intermediate node with child nodes
                                                {
                                                    if (nodeskip)
                                                    {
                                                        nodeind += node.PolyId; //(child node count)
                                                    }
                                                    else
                                                    {
                                                        nodeind++;
                                                    }
                                                }
                                                else //leaf node, with polygons
                                                {
                                                    if (!nodeskip)
                                                    {
                                                        var lastp = node.PolyId + node.PolyCount;
                                                        lastp = Math.Min(lastp, (int)bgeom.PolygonsCount);
                                                        for (int p = node.PolyId; p < lastp; p++)
                                                        {
                                                            var polygon = bgeom.Polygons[p];
                                                            bool polyhit = false;
                                                            switch (polygon.Type)
                                                            {
                                                                case BoundPolygonType.Triangle:
                                                                    var ptri = polygon as BoundPolygonTriangle;
                                                                    p1 = bgeom.GetVertex(ptri.vertIndex1) + cg;
                                                                    p2 = bgeom.GetVertex(ptri.vertIndex2) + cg;
                                                                    p3 = bgeom.GetVertex(ptri.vertIndex3) + cg;
                                                                    polyhit = sph.Intersects(ref p1, ref p2, ref p3);
                                                                    if (polyhit) n1 = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
                                                                    break;
                                                                case BoundPolygonType.Sphere:
                                                                    var psph = polygon as BoundPolygonSphere;
                                                                    tsph.Center = bgeom.GetVertex(psph.sphereIndex) + cg;
                                                                    tsph.Radius = psph.sphereRadius;
                                                                    polyhit = sph.Intersects(ref tsph);
                                                                    if (polyhit) n1 = Vector3.Normalize(sph.Center - tsph.Center);
                                                                    break;
                                                                case BoundPolygonType.Capsule:
                                                                    //TODO
                                                                    break;
                                                                case BoundPolygonType.Box:
                                                                    var pbox = polygon as BoundPolygonBox;
                                                                    p1 = bgeom.GetVertex(pbox.boxIndex1);// + cg; //corner
                                                                    p2 = bgeom.GetVertex(pbox.boxIndex2);// + cg;
                                                                    p3 = bgeom.GetVertex(pbox.boxIndex3);// + cg;
                                                                    p4 = bgeom.GetVertex(pbox.boxIndex4);// + cg;
                                                                    a1 = ((p3 + p4) - (p1 + p2)) * 0.5f;
                                                                    a2 = p3 - (p1 + a1);
                                                                    a3 = p4 - (p1 + a1);
                                                                    Vector3 bs = new Vector3(a1.Length(), a2.Length(), a3.Length());
                                                                    Vector3 m1 = a1 / bs.X;
                                                                    Vector3 m2 = a2 / bs.Y;
                                                                    Vector3 m3 = a3 / bs.Z;
                                                                    if ((bs.X < bs.Y) && (bs.X < bs.Z)) m1 = Vector3.Cross(m2, m3);
                                                                    else if (bs.Y < bs.Z) m2 = Vector3.Cross(m1, m3);
                                                                    else m3 = Vector3.Cross(m1, m2);
                                                                    Vector3 tp = sp - (p1 + cg);
                                                                    spht.Center = new Vector3(Vector3.Dot(tp, m1), Vector3.Dot(tp, m2), Vector3.Dot(tp, m3));
                                                                    spht.Radius = sph.Radius;
                                                                    box.Minimum = Vector3.Zero;
                                                                    box.Maximum = bs;
                                                                    polyhit = spht.Intersects(ref box);
                                                                    if (polyhit)
                                                                    {
                                                                        Vector3 smin = spht.Center - spht.Radius;
                                                                        Vector3 smax = spht.Center + spht.Radius;
                                                                        float eps = spht.Radius * 0.8f;
                                                                        n1 = Vector3.Zero;
                                                                        if (Math.Abs(smax.X) < eps) n1 -= m1;
                                                                        else if (Math.Abs(smin.X - bs.X) < eps) n1 += m1;
                                                                        if (Math.Abs(smax.Y) < eps) n1 -= m2;
                                                                        else if (Math.Abs(smin.Y - bs.Y) < eps) n1 += m2;
                                                                        if (Math.Abs(smax.Z) < eps) n1 -= m3;
                                                                        else if (Math.Abs(smin.Z - bs.Z) < eps) n1 += m3;
                                                                        float n1l = n1.Length();
                                                                        if (n1l > 0.0f) n1 = n1 / n1l;
                                                                        else n1 = Vector3.UnitZ;
                                                                    }
                                                                    break;
                                                                case BoundPolygonType.Cylinder:
                                                                    //TODO
                                                                    break;
                                                            }
                                                            if (polyhit) // && (polyhittestdist < itemhitdist) && (polyhittestdist < maxdist))
                                                            {
                                                                hitpoly = polygon;
                                                                //itemhitdist = polyhittestdist;
                                                                //ybnhit = true;
                                                                hit = true;
                                                                hitnorm = n1;
                                                            }
                                                            polytestcount++;
                                                        }
                                                    }
                                                    nodeind++;
                                                }
                                                nodetestcount++;
                                            }
                                        }
                                    }
                                    break;
                                case 3: //BoundBox - found in drawables - TODO
                                    BoundBox boundbox = b as BoundBox;
                                    if (boundbox == null)
                                    { continue; }
                                    break;
                                case 0: //BoundSphere - found in drawables - TODO
                                    BoundSphere boundsphere = b as BoundSphere;
                                    if (boundsphere == null)
                                    { continue; }
                                    break;
                                default:
                                    break;
                            }


                            //if (ybnhit)
                            //{
                            //    //boxhitdist = boxhitdisttest;
                            //    //hit = true;
                            //}

                        }
                    }

                    cellcount++;
                }
            }

            //if (hit)
            //{
            //    hitpos = ray.Position + ray.Direction * itemhitdist;
            //}

            res.TestedNodeCount = nodetestcount;
            res.TestedPolyCount = polytestcount;
            res.TestComplete = testcomplete;
            res.Hit = hit;
            res.HitDist = 0;// itemhitdist;
            res.HitPolygon = hitpoly;
            res.Position = hitpos;
            res.Normal = hitnorm;

            return res;
        }

    }

    public class SpaceGrid
    {
        public const int CellCount = 500; //cells along a side, total cell count is this squared
        public const int LastCell = CellCount - 1; //the last cell index in the array
        public const float WorldSize = 10000.0f; //max world grid size +/- 10000 units
        public const float CellSize = 2.0f * WorldSize / (float)CellCount;//20.0f; //size of a cell
        public const float CellSizeInv = 1.0f / CellSize; //inverse of the cell size.
        public const float CellSizeHalf = CellSize * 0.5f; //half the cell size

        public int TotalBoundsCount = 0; //total number of bounds in this grid
        public int TotalBoundsRefCount = 0; //total number of bounds placements in cells
        private int MaxBoundsInCell = 0; //biggest number of bounds added to a single cell
        private SpaceGridCell DensestBoundsCell = null;

        public int TotalNodeCount = 0; //total map nodes in grid
        public int TotalNodeRefCount = 0; //total number of map node placements in cells
        public int MaxNodesInCell = 0; //biggest number of nodes added to a single cell
        private SpaceGridCell DensestNodeCell = null;

        public int TotalInteriorCount = 0;
        public int TotalInteriorRefCount = 0;
        public int MaxInteriorsInCell = 0;
        private SpaceGridCell DensestInteriorCell = null;

        public SpaceGridCell[,] Cells { get; set; } = new SpaceGridCell[CellCount, CellCount];


        public Vector3 GetWorldPos(Vector2I p)
        {
            Vector3 ind = new Vector3(p.X, p.Y, 0.0f);
            return (ind * CellSize) - new Vector3(WorldSize, WorldSize, 0);
        }
        public Vector2I GetCellPos(Vector3 p)
        {
            Vector3 ind = (p + WorldSize) * CellSizeInv;
            int x = (int)ind.X;
            int y = (int)ind.Y;
            x = (x < 0) ? 0 : (x > LastCell) ? LastCell : x;
            y = (y < 0) ? 0 : (y > LastCell) ? LastCell : y;
            return new Vector2I(x, y);
        }
        public SpaceGridCell GetCell(Vector2I g)
        {
            if ((g.X < 0) || (g.Y < 0) || (g.X >= CellCount) || (g.Y >= CellCount))
            {
                return null;
            }
            var cell = Cells[g.X, g.Y];
            if (cell == null)
            {
                cell = new SpaceGridCell();
                Cells[g.X, g.Y] = cell;
            }
            return cell;
        }
        public SpaceGridCell GetCell(Vector3 p)
        {
            return GetCell(GetCellPos(p));
        }

        public void AddBounds(BoundsStoreItem item)
        {
            Vector2I min = GetCellPos(item.Min);
            Vector2I max = GetCellPos(item.Max);

            int cellcount = 0;
            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    var cell = GetCell(new Vector2I(x, y));
                    cell.AddBounds(item);
                    TotalBoundsRefCount++;
                    if (cell.BoundsList.Count > MaxBoundsInCell)
                    {
                        MaxBoundsInCell = cell.BoundsList.Count;
                        DensestBoundsCell = cell;
                    }
                    cellcount++;
                }
            }
            if (cellcount == 0)
            { }

            TotalBoundsCount++;
        }

        public void AddNode(MapDataStoreNode node)
        {
            bool useouter = true;// false;
            //switch (node.Unk01)
            //{
            //    case 2:
            //    case 4:
            //    case 20:
            //    case 66:
            //    case 514://lods
            //        useouter = true;
            //        break;
            //    case 128:
            //    case 256://lodlights
            //        useouter = true;
            //        break;
            //    case 18:
            //    case 82://HD nodes
            //        useouter = true;
            //        break;
            //}

            //Vector2I min = GetCellPos(node.OuterBBMin);
            //Vector2I max = GetCellPos(node.OuterBBMax);
            Vector2I min = GetCellPos(useouter ? node.streamingExtentsMin : node.entitiesExtentsMin);
            Vector2I max = GetCellPos(useouter ? node.streamingExtentsMax : node.entitiesExtentsMax);

            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    var cell = GetCell(new Vector2I(x, y));
                    cell.AddNode(node);
                    TotalNodeRefCount++;
                    if (cell.NodesList.Count > MaxNodesInCell)
                    {
                        MaxNodesInCell = cell.NodesList.Count;
                        DensestNodeCell = cell;
                    }
                }
            }

            TotalNodeCount++;
        }

        public void AddInterior(CInteriorProxy intprx)
        {
            Vector2I min = GetCellPos(intprx.BBMin);
            Vector2I max = GetCellPos(intprx.BBMax);

            int cellcount = 0;
            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    var cell = GetCell(new Vector2I(x, y));
                    cell.AddInterior(intprx);
                    TotalInteriorRefCount++;
                    if (cell.InteriorList.Count > MaxInteriorsInCell)
                    {
                        MaxInteriorsInCell = cell.InteriorList.Count;
                        DensestInteriorCell = cell;
                    }
                    cellcount++;
                }
            }
            if (cellcount == 0)
            { }

            TotalInteriorCount++;
        }
    }
    public class SpaceGridCell
    {
        public List<MapDataStoreNode> NodesList;
        public List<BoundsStoreItem> BoundsList;
        public List<CInteriorProxy> InteriorList;

        public void AddNode(MapDataStoreNode node)
        {
            if (NodesList == null)
            {
                NodesList = new List<MapDataStoreNode>();
            }
            NodesList.Add(node);
        }

        public void AddBounds(BoundsStoreItem item)
        {
            if (BoundsList == null)
            {
                BoundsList = new List<BoundsStoreItem>(5);
            }
            BoundsList.Add(item);
        }
        public void RemoveBounds(BoundsStoreItem item)
        {
            if (BoundsList != null)
            {
                BoundsList.Remove(item);
            }
        }

        public void AddInterior(CInteriorProxy intprx)
        {
            if (InteriorList == null)
            {
                InteriorList = new List<CInteriorProxy>(5);
            }
            InteriorList.Add(intprx);
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


        public YndNode GetYndNode(ushort areaid, ushort nodeid)
        {
            var cell = GetCell(areaid);
            if ((cell == null) || (cell.Ynd == null) || (cell.Ynd.Nodes == null))
            { return null; }
            if (nodeid >= cell.Ynd.Nodes.Length)
            { return null; }
            return cell.Ynd.Nodes[nodeid];
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
        public float CornerX = -6000.0f;
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
        public BoundPolygon HitPolygon;
        public Vector3 Position;
        public Vector3 Normal;
        public int TestedNodeCount;
        public int TestedPolyCount;
        public bool TestComplete;
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
