using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Xml;
using System.ComponentModel;
using System.Xml.Linq;
using CodeWalker.Core.Utils;
using System.Runtime.InteropServices;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace CodeWalker.World
{
    [SkipLocalsInit]
    public class Scenarios
    {
        public volatile bool Inited = false;

        public Timecycle Timecycle { get; set; }
        public GameFileCache GameFileCache { get; set; }

        public static ScenarioTypes ScenarioTypes { get; set; }

        public List<YmtFile> ScenarioRegions { get; set; } = new List<YmtFile>();


        public async Task InitAsync(GameFileCache gameFileCache, Action<string> updateStatus, Timecycle timecycle)
        {
            using var _ = new DisposableTimer("Scenarios Init");
            Timecycle = timecycle;
            GameFileCache = gameFileCache;

            using (new DisposableTimer("EnsureScenarioTypes"))
            {
                await EnsureScenarioTypes(gameFileCache);
            }

            ScenarioRegions.Clear();


            //rubidium:
            //the non-replacement [XML] is hash 1074D56E
            //replacement XML is 203D234 I think and replacement PSO A6F20ADA
            //('replacement' implies 'when a DLC loads it it unloads the existing spmanifest first')
            //- content.xml fileType for sp_manifest



            //Vector2I maxgrid = new Vector2I(0, 0);
            //List<Vector2I> griddims = new List<Vector2I>();
            //int maxcells = 0;

            var rpfman = gameFileCache.RpfMan;
            string manifestfilename = "update\\update.rpf\\x64\\levels\\gta5\\sp_manifest.ymt";
            YmtFile? manifestymt = await rpfman.GetFileAsync<YmtFile>(manifestfilename);
            if (manifestymt is not null && manifestymt.CScenarioPointManifest is not null)
            {
                var regionDefs = manifestymt.CScenarioPointManifest.RegionDefs;

                ScenarioRegions.EnsureCapacity(regionDefs.Length);
                using var timerSummed = new DisposableTimerSummed("LoadYmtFile");

                await Parallel.ForAsync(0, regionDefs.Length, async (i, cancellationToken) =>
                {
                    var region = regionDefs[i];
                    string regionfilename = $"{region.Name}.ymt"; //JenkIndex lookup... ymt should have already loaded path strings into it! maybe change this...
                    string basefilename = regionfilename.Replace("platform:", "x64a.rpf");
                    string updfilename = regionfilename.Replace("platform:", "update\\update.rpf\\x64");
                    string usefilename = updfilename;

                    if (!gameFileCache.EnableDlc)
                    {
                        usefilename = basefilename;
                    }

                    YmtFile? regionymt = await rpfman.GetFileAsync<YmtFile>(usefilename) ?? await rpfman.GetFileAsync<YmtFile>(basefilename);
                    //YmtFile? regionymt = await rpfman.GetFileAsync<YmtFile>(usefilename) ?? await rpfman.GetFileAsync<YmtFile>(basefilename);

                    if (regionymt is not null)
                    {
                        var sregion = regionymt.ScenarioRegion;
                        if (sregion is not null)
                        {
                            lock(ScenarioRegions)
                            {
                                ScenarioRegions.Add(regionymt);
                            }



                            ////testing stuff...

                            //var gd = regionymt?.CScenarioPointRegion?.Data.AccelGrid.Dimensions ?? new Vector2I();
                            //griddims.Add(gd);
                            //maxgrid = new Vector2I(Math.Max(maxgrid.X, gd.X), Math.Max(maxgrid.Y, gd.Y));
                            //maxcells = Math.Max(maxcells, gd.X * gd.Y);

                            //byte[] data = regionymt.Save();
                            //System.IO.File.WriteAllBytes("C:\\CodeWalker.Projects\\YmtTest\\AllYmts\\" + regionymt.Name, data);
                        }
                    }
                });
            }

            Inited = true;
        }


        public static async Task EnsureScenarioTypes(GameFileCache gfc)
        {
            //if (ScenarioTypes == null)
            //{
                var stypes = new ScenarioTypes();
                await stypes.LoadAsync(gfc);
                ScenarioTypes = stypes;
            //}
        }


    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioRegion : BasePathData
    {
        public EditorVertex[] PathVerts { get; set; } = [];
        public Vector4[] NodePositions { get; set; } = [];

        public EditorVertex[] GetPathVertices()
        {
            return PathVerts;
        }
        public Vector4[] GetNodePositions()
        {
            return NodePositions;
        }

        public YmtFile Ymt { get; set; }
        public MCScenarioPointRegion Region { get; set; }

        private Dictionary<Vector3, ScenarioNode> NodeDict { get; set; }
        public List<ScenarioNode> Nodes { get; set; }

        public PathBVH BVH { get; set; }


        public bool Loaded { get; set; }




        public void Load(YmtFile ymt)
        {
            Ymt = ymt;
            Region = ymt?.CScenarioPointRegion;

            BuildNodes();

            BuildBVH();

            BuildVertices();

            LoadTypes();

            Loaded = true;
        }


        public void LoadTypes()
        {
            if (Ymt == null || Region == null)
            {
                return;
            }

            if (Region.LookUps == null)
            {
                return;
            }

            if (Scenarios.ScenarioTypes == null) //these are loaded by Scenarios.Init
            {
                return;
            }

            if (Nodes == null) //nodes not loaded yet - BuildVertices needs to be called first!
            {
                return;
            }

            foreach (var node in Nodes)
            {
                LoadTypes(Region, node.MyPoint);
                LoadTypes(Region, node.ClusterMyPoint);
                LoadTypes(Region, node.ChainingNode);
            }
        }

        private void LoadTypes(MCScenarioPointRegion r, MCScenarioPoint scp)
        {
            if (scp is null)
                return;

            var types = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            if (types is null)
                return;

            var typhashes = r.LookUps.TypeNames;
            var pedhashes = r.LookUps.PedModelSetNames;
            var vehhashes = r.LookUps.VehicleModelSetNames;
            var inthashes = r.LookUps.InteriorNames;
            var grphashes = r.LookUps.GroupNames;
            var maphashes = r.LookUps.RequiredIMapNames;

            bool isveh = false;
            var tpind = scp.TypeId;
            if (tpind < typhashes.Length)
            {
                var hash = typhashes[tpind];
                scp.Type = types.GetScenarioTypeRef(hash);
                isveh = scp.Type?.IsVehicle ?? false; //TODO: make a warning about this if scp.Type is null?
            }

            var msind = scp.ModelSetId;
            if (isveh)
            {
                if (msind < vehhashes.Length)
                {
                    var hash = vehhashes[msind];
                    scp.ModelSet = types.GetVehicleModelSet(hash);
                }
            }
            else
            {
                if (msind < pedhashes.Length)
                {
                    var hash = pedhashes[msind];
                    scp.ModelSet = types.GetPedModelSet(hash);
                }
            }

            var intind = scp.InteriorId;
            if (intind < inthashes.Length)
            {
                var hash = inthashes[intind];
                scp.InteriorName = hash;
            }

            var grpid = scp.GroupId;
            if (grpid < grphashes.Length)
            {
                var hash = grphashes[grpid];
                scp.GroupName = hash;
            }

            var mapid = scp.IMapId;
            if (mapid < maphashes.Length)
            {
                var hash = maphashes[mapid];
                scp.IMapName = hash;
            }

        }
        private void LoadTypes(MCScenarioPointRegion r, MCScenarioChainingNode spn)
        {
            if (spn == null)
                return;
            var types = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            if (types == null)
            {
                return;
            }

            uint hash = spn._Data.ScenarioType;
            if ((hash != 0) && (hash != 493038497))
            {
                bool isveh = false;
                spn.Type = types.GetScenarioTypeRef(hash);
                isveh = spn.Type?.IsVehicle ?? false;
            }
        }



        public void BuildNodes()
        {

            NodeDict = new Dictionary<Vector3, ScenarioNode>();
            Nodes = new List<ScenarioNode>();

            if (Ymt?.CScenarioPointRegion is not null)
            {
                var r = Ymt.CScenarioPointRegion;

                if (r.Paths?.Nodes is not null)
                {
                    foreach (var node in r.Paths.Nodes)
                    {
                        EnsureNode(node);
                    }

                    if (r.Paths.Chains is not null && r.Paths.Edges is not null)
                    {
                        List<MCScenarioChainingEdge> chainedges = new List<MCScenarioChainingEdge>();

                        var rp = r.Paths;
                        var rpc = rp.Chains;
                        var rpe = rp.Edges;
                        var rpeLength = rp.Edges.Length;
                        var rpn = rp.Nodes;
                        var rpnLength = rpn.Length;

                        foreach (var chain in rpc)
                        {
                            if (chain.EdgeIds is not null)
                            {
                                chainedges.Clear();

                                foreach (var edgeId in chain.EdgeIds)
                                {
                                    if (edgeId >= rpeLength)
                                        continue;
                                    var edge = rpe[edgeId];

                                    if (edge.NodeIndexFrom >= rpnLength)
                                        continue;
                                    if (edge.NodeIndexTo >= rpnLength)
                                        continue;

                                    edge.NodeFrom = rpn[edge.NodeIndexFrom];
                                    edge.NodeTo = rpn[edge.NodeIndexTo];

                                    if (edge.NodeFrom is not null)
                                        edge.NodeFrom.Chain = chain;
                                    if (edge.NodeTo is not null)
                                        edge.NodeTo.Chain = chain;

                                    chainedges.Add(edge);
                                }

                                chain.Edges = chainedges.ToArray();
                            }
                            else
                            {
                                chain.Edges = [];
                            }
                        }
                    }

                }

                if (r.Points is not null)
                {
                    if (r.Points.MyPoints is not null)
                    {
                        foreach (var point in r.Points.MyPoints)
                        {
                            EnsureNode(point);
                        }
                    }
                    if (r.Points.LoadSavePoints is not null)
                    {
                        foreach (var point in r.Points.LoadSavePoints)
                        {
                            EnsureNode(point); //no hits here - not used?
                        }
                    }
                }

                if (r.Clusters is not null) //spawn groups
                {
                    foreach (var cluster in r.Clusters)
                    {
                        EnsureClusterNode(cluster);

                        if (cluster.Points is not null)
                        {
                            if (cluster.Points.MyPoints is not null)
                            {
                                foreach (var point in cluster.Points.MyPoints)
                                {
                                    var node = EnsureClusterNode(point);
                                    node.Cluster = cluster;
                                }
                            }
                            if (cluster.Points.LoadSavePoints is not null)
                            {
                                foreach (var point in cluster.Points.LoadSavePoints)
                                {
                                    var node = EnsureClusterNode(point); //no hits here - not used?
                                    node.Cluster = cluster;
                                }
                            }
                        }
                    }
                }

                if (r.EntityOverrides is not null)
                {
                    foreach (var overr in r.EntityOverrides)
                    {
                        EnsureEntityNode(overr);

                        if (overr.ScenarioPoints is not null)
                        {
                            foreach (var point in overr.ScenarioPoints)
                            {
                                var node = EnsureEntityNode(point);
                                node.Entity = overr;
                            }
                        }
                    }
                }
            }

            //Nodes = NodeDict.Values.ToList();

        }

        public void BuildBVH()
        {
            BVH = new PathBVH(Nodes.ToArray(), 10, 10);
        }

        public void BuildVertices()
        {
            PathVerts = [];
            uint cred = (uint)Color.Red.ToRgba();
            uint cblu = (uint)Color.Blue.ToRgba();
            uint cgrn = (uint)Color.Green.ToBgra();
            uint cblk = (uint)Color.Black.ToBgra();

            var r = Ymt?.CScenarioPointRegion?.Paths;
            if (r?.Nodes is not null && r?.Chains is not null && r?.Edges is not null)
            {
                List<EditorVertex> pathverts = new List<EditorVertex>();

                
                var nodes = r.Nodes;
                var nodesLength = nodes.Length;
                foreach (var chain in r.Chains)
                {
                    if (chain.Edges is null)
                        continue;

                    foreach (var edge in chain.Edges)
                    {
                        var vid1 = edge._Data.NodeIndexFrom;
                        var vid2 = edge._Data.NodeIndexTo;
                        if ((vid1 >= nodesLength) || (vid2 >= nodesLength))
                            continue;
                        var v1 = nodes[vid1];
                        var v2 = nodes[vid2];
                        byte cr1 = (v1.HasIncomingEdges) ? (byte)255 : (byte)0;
                        byte cr2 = (v2.HasIncomingEdges) ? (byte)255 : (byte)0;
                        byte cg = 0;// (chain._Data.Unk_1156691834 > 1) ? (byte)255 : (byte)0;
                        //cg = ((v1.Unk1 != 0) || (v2.Unk1 != 0)) ? (byte)255 : (byte)0;
                        //cg = (edge.Action == CScenarioChainingEdge__eAction.Unk_7865678) ? (byte)255 : (byte)0;
                        //cg = ((v1.UnkValTest != 0) || (v2.UnkValTest != 0)) ? (byte)255 : (byte)0;

                        byte cb1 = (byte)(255 - cr1);
                        byte cb2 = (byte)(255 - cr2);
                        var colour1 = (uint)new Color(cr1, cg, cb1, (byte)255).ToRgba();
                        var colour2 = (uint)new Color(cr2, cg, cb2, (byte)255).ToRgba();
                        pathverts.Add(new EditorVertex(v1.Position, colour1));
                        pathverts.Add(new EditorVertex(v2.Position, colour2));
                    }
                }

                //if (r.Unk_3844724227 != null) //visualise AccelGrid...
                //{
                //    var grid = r._Data.AccelGrid;
                //    var minx = grid.MinCellX;
                //    var maxx = grid.MaxCellX;
                //    var miny = grid.MinCellY;
                //    var maxy = grid.MaxCellY;
                //    var cntx = (maxx - minx) + 1;
                //    var cnty = (maxy - miny) + 1;
                //    var calclen = cntx * cnty; //==r.Unk_3844724227.Length;
                //    var gscale = grid.Scale;
                //    var posz = BVH?.Box.Maximum.Z ?? 0;
                //    var minpos = new Vector3(grid.Min, posz);
                //    var sizx = cntx * gscale.X;
                //    var sizy = cnty * gscale.Y;
                //    for (var x = 0; x <= cntx; x++)
                //    {
                //        var fx = x * gscale.X;
                //        pv1.Position = minpos + new Vector3(fx, 0, 0);
                //        pv2.Position = pv1.Position + new Vector3(0, sizy, 0);
                //        pv1.Colour = cblk;
                //        pv2.Colour = cblk;
                //        pathverts.Add(pv1);
                //        pathverts.Add(pv2);
                //    }
                //    for (var y = 0; y <= cnty; y++)
                //    {
                //        var fy = y * gscale.Y;
                //        pv1.Position = minpos + new Vector3(0, fy, 0);
                //        pv2.Position = pv1.Position + new Vector3(sizx, 0, 0);
                //        pv1.Colour = cblk;
                //        pv2.Colour = cblk;
                //        pathverts.Add(pv1);
                //        pathverts.Add(pv2);
                //    }
                //}

                if (pathverts.Count > 0)
                {
                    PathVerts = pathverts.ToArray();
                }
            }

            var _nodes = Nodes;
            var count = _nodes.Count;
            Vector4[] nodePositions = new Vector4[count];
            for (int i = 0; i < count; i++)
            {
                nodePositions[i] = new Vector4(_nodes[i].Position, 1.0f);
            }

            NodePositions = nodePositions;
        }






        private ScenarioNode EnsureNode(MCScenarioChainingNode cnode)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, cnode.Position, out var exists);
            if (exists && (exnode?.ChainingNode is null))
            {
                exnode!.ChainingNode = cnode;
            }
            else
            {
                exnode = new ScenarioNode(cnode.Region?.Ymt)
                {
                    ChainingNode = cnode,
                    Position = cnode.Position,
                };
                Nodes.Add(exnode);
            }
            cnode.ScenarioNode = exnode;
            return exnode;
        }
        private ScenarioNode EnsureNode(MCScenarioPoint point)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, point.Position, out var exists);
            if (exists && (exnode!.MyPoint is null))
            {
                exnode.MyPoint = point;
                exnode.Orientation = point.Orientation;
            }
            else
            {
                exnode = new ScenarioNode(point.Region?.Ymt);
                exnode.MyPoint = point;
                exnode.Position = point.Position;
                exnode.Orientation = point.Orientation;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureNode(MCExtensionDefSpawnPoint point)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, point.Position, out var exists);
            if (exists && (exnode!.LoadSavePoint is null))
            {
                exnode.LoadSavePoint = point;
            }
            else
            {
                exnode = new ScenarioNode(point.ScenarioRegion?.Ymt);
                exnode.LoadSavePoint = point;
                exnode.Position = point.Position;
                exnode.Orientation = point.Orientation;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureClusterNode(MCScenarioPointCluster cluster)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, cluster.Position, out var exists);
            if (exists && (exnode!.Cluster is null))
            {
                exnode.Cluster = cluster;
            }
            else
            {
                exnode = new ScenarioNode(cluster.Region?.Ymt);
                exnode.Cluster = cluster;
                exnode.Position = cluster.Position;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureClusterNode(MCScenarioPoint point)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, point.Position, out var exists);
            if (exists && (exnode!.ClusterMyPoint is null))
            {
                exnode.ClusterMyPoint = point;
                exnode.Orientation = point.Orientation;
            }
            else
            {
                exnode = new ScenarioNode(point.Region?.Ymt);
                exnode.ClusterMyPoint = point;
                exnode.Position = point.Position;
                exnode.Orientation = point.Orientation;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureClusterNode(MCExtensionDefSpawnPoint point)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, point.Position, out var exists);
            if (exists && exnode is not null && exnode.ClusterLoadSavePoint is null)
            {
                exnode.ClusterLoadSavePoint = point;
            }
            else
            {
                exnode = new ScenarioNode(point.ScenarioRegion?.Ymt);
                exnode.ClusterLoadSavePoint = point;
                exnode.Position = point.Position;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureEntityNode(MCExtensionDefSpawnPoint point)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, point.Position, out var exists);
            if (exists && exnode is not null && exnode.EntityPoint is null)
            {
                exnode.EntityPoint = point;
            }
            else
            {
                exnode = new ScenarioNode(point.ScenarioRegion?.Ymt);
                exnode.EntityPoint = point;
                exnode.Position = point.Position;
                exnode.Orientation = point.Orientation;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureEntityNode(MCScenarioEntityOverride entity)
        {
            ref var exnode = ref CollectionsMarshal.GetValueRefOrAddDefault(NodeDict, entity.Position, out var exists);
            if (exists && exnode is not null && exnode.Entity is null)
            {
                exnode.Entity = entity;
            }
            else
            {
                exnode = new ScenarioNode(entity.Region?.Ymt);
                exnode.Entity = entity;
                exnode.Position = entity.Position;
                Nodes.Add(exnode);
            }
            return exnode;
        }






        public ScenarioNode AddNode(ScenarioNode? copy = null)
        {
            var n = new ScenarioNode(Ymt);

            var rgn = Ymt.CScenarioPointRegion;

            if (copy is not null)
            {
                if (copy.MyPoint is not null)
                    n.MyPoint = new MCScenarioPoint(rgn, copy.MyPoint);
                if (copy.LoadSavePoint is not null)
                    n.LoadSavePoint = new MCExtensionDefSpawnPoint(rgn, copy.LoadSavePoint);
                if (copy.ClusterMyPoint is not null)
                {
                    n.Cluster = copy.Cluster;
                    n.ClusterMyPoint = new MCScenarioPoint(rgn, copy.ClusterMyPoint);
                }
                else if (copy.ClusterLoadSavePoint is not null)
                {
                    n.Cluster = copy.Cluster;
                    n.ClusterLoadSavePoint = new MCExtensionDefSpawnPoint(rgn, copy.ClusterLoadSavePoint);
                }
                else if (copy.Cluster is not null)
                {
                    n.Cluster = new MCScenarioPointCluster(rgn, copy.Cluster);
                }
                if (copy.EntityPoint is not null)
                {
                    n.Entity = copy.Entity;
                    n.EntityPoint = new MCExtensionDefSpawnPoint(rgn, copy.EntityPoint);
                }
                else if (copy.Entity is not null)
                {
                    n.Entity = new MCScenarioEntityOverride(rgn, copy.Entity);
                }
                if (copy.ChainingNode is not null)
                {
                    n.ChainingNode = new MCScenarioChainingNode(rgn, copy.ChainingNode);
                    n.ChainingNode.ScenarioNode = n;
                }
            }
            else
            {
                n.MyPoint = new MCScenarioPoint(rgn);
                n.MyPoint.InteriorName = 493038497; //JenkHash.GenHash("none");
                n.MyPoint.GroupName = 493038497;
                n.MyPoint.IMapName = 493038497;
                n.MyPoint.TimeStart = 0;
                n.MyPoint.TimeEnd = 24;
            }


            if (Region is not null && Region.Points is not null)
            {
                if (n.MyPoint != null) Region.Points.AddMyPoint(n.MyPoint);
                if (n.LoadSavePoint != null) Region.Points.AddLoadSavePoint(n.LoadSavePoint);
                if (n.Cluster is not null && n.Cluster.Points is not null)
                {
                    if (n.ClusterMyPoint is not null)
                        n.Cluster.Points.AddMyPoint(n.ClusterMyPoint);
                    if (n.ClusterLoadSavePoint is not null)
                        n.Cluster.Points.AddLoadSavePoint(n.ClusterLoadSavePoint);
                }
                if ((n.Entity is not null) && (n.Entity.ScenarioPoints is not null))
                {
                    if (n.EntityPoint is not null)
                        n.Entity.AddScenarioPoint(n.EntityPoint);
                }
            }
            if (Region is not null && Region.Paths is not null)
            {
                if (n.ChainingNode != null)
                {
                    Region.Paths.AddNode(n.ChainingNode);

                    n.ChainingNode.Chain = copy?.ChainingNode?.Chain;

                    //create a new edge connecting from the existing node...
                    if ((copy?.ChainingNode != null) && (Region.Paths.Edges != null))
                    {
                        MCScenarioChainingEdge exEdge = null;
                        foreach (var edge in Region.Paths.Edges)
                        {
                            if (edge.NodeTo == copy.ChainingNode)
                            {
                                exEdge = edge;
                                break;
                            }
                        }
                        if (exEdge != null)
                        {
                            MCScenarioChainingEdge newEdge = new MCScenarioChainingEdge(rgn, exEdge);
                            newEdge.NodeFrom = copy.ChainingNode;
                            newEdge.NodeIndexFrom = (ushort)copy.ChainingNode.NodeIndex;
                            newEdge.NodeTo = n.ChainingNode;
                            newEdge.NodeIndexTo = (ushort)n.ChainingNode.NodeIndex;
                            Region.Paths.AddEdge(newEdge);

                            //chain start/end have these flags set... make sure they are updated!
                            copy.ChainingNode.HasOutgoingEdges = true;
                            n.ChainingNode.HasOutgoingEdges = false;

                            if (copy.Region == Region) //only add the new edge if we're in the same region...
                            {
                                n.ChainingNode.Chain.AddEdge(newEdge);
                            }
                            else
                            {
                                //create a new chain..?
                            }
                        }
                    }
                }
            }


            Nodes.Add(n);

            return n;
        }


        public bool RemoveNode(ScenarioNode node)
        {
            if (node == null) return false;

            var rgn = Region;
            if (rgn == null) return false;

            bool res = true;

            if (rgn.Points != null)
            {
                if (node.MyPoint != null)
                {
                    res = res && rgn.Points.RemoveMyPoint(node.MyPoint);
                }
                if (node.LoadSavePoint != null)
                {
                    res = res && rgn.Points.RemoveLoadSavePoint(node.LoadSavePoint);
                }
            }
            if ((node.Cluster != null) && (node.Cluster.Points != null))
            {
                if (node.ClusterMyPoint != null)
                {
                    res = res && node.Cluster.Points.RemoveMyPoint(node.ClusterMyPoint);
                }
                if (node.ClusterLoadSavePoint != null)
                {
                    res = res && node.Cluster.Points.RemoveLoadSavePoint(node.ClusterLoadSavePoint);
                }
            }
            if (node.Entity != null)
            {
                if (node.EntityPoint != null)
                {
                    res = res && node.Entity.RemoveScenarioPoint(node.EntityPoint);
                }
            }
            if ((node.ChainingNode != null) && (rgn.Paths != null))
            {
                res = res && rgn.Paths.RemoveNode(node.ChainingNode);
            }

            res = res && Nodes.Remove(node);

            return res;
        }

        public bool RemoveChain(MCScenarioChain chain, bool delpoints)
        {
            var paths = Region?.Paths;
            if (paths == null) return false;


            Dictionary<MCScenarioChainingNode, int> ndict = new Dictionary<MCScenarioChainingNode, int>();

            var edges = chain.Edges;
            if (edges != null)
            {
                foreach (var edge in edges)
                {
                    //paths.RemoveEdge(edge); //removing nodes also removes edges!
                    paths.RemoveNode(edge.NodeFrom);
                    paths.RemoveNode(edge.NodeTo);

                    ndict[edge.NodeFrom] = 1;
                    ndict[edge.NodeTo] = 1;
                }
            }

            paths.RemoveChain(chain);


            List<ScenarioNode> delnodes = new List<ScenarioNode>();
            foreach (var node in Nodes)
            {
                if ((node.ChainingNode != null) && (ndict.ContainsKey(node.ChainingNode)))
                {
                    delnodes.Add(node);
                }
            }
            foreach (var delnode in delnodes)
            {
                delnode.ChainingNode = null;//this chaining node has been removed from the region. remove this association.
                if (delpoints)
                {
                    RemoveNode(delnode);
                }
            }

            return true;
        }

        public bool RemoveCluster(MCScenarioPointCluster cluster, bool delpoints)
        {
            var crgn = Region;
            if (crgn == null) return false;


            crgn.RemoveCluster(cluster);





            Dictionary<MCScenarioPoint, int> ndict = new Dictionary<MCScenarioPoint, int>();
            if (cluster?.Points?.MyPoints != null)
            {
                foreach (var point in cluster.Points.MyPoints)
                {
                    ndict[point] = 1;
                }
            }
            List<ScenarioNode> delnodes = new List<ScenarioNode>();
            foreach (var node in Nodes)
            {
                if ((node.ClusterMyPoint != null) && (ndict.ContainsKey(node.ClusterMyPoint)))
                {
                    delnodes.Add(node);
                }
                else if (node.Cluster == cluster)
                {
                    delnodes.Add(node);
                }
            }
            foreach (var delnode in delnodes)
            {
                if (!delpoints && (crgn.Points != null) && (delnode.ClusterMyPoint != null))
                {
                    var copypt = new MCScenarioPoint(crgn, delnode.ClusterMyPoint);
                    crgn.Points.AddMyPoint(copypt);
                    delnode.MyPoint = copypt;
                }
                bool iscl = false;
                if ((delnode.Cluster != null) && (delnode.ClusterMyPoint == null) && (delnode.ClusterLoadSavePoint == null))
                {
                    iscl = true;
                }
                delnode.Cluster = null;
                delnode.ClusterMyPoint = null;//this cluster point has been removed from the region. remove this association.
                delnode.ClusterLoadSavePoint = null;
                if (delpoints)
                {
                    //if ((delnode.ChainingNode == null) && (delnode.EntityPoint == null))
                    {
                        RemoveNode(delnode);
                    }
                }
                else if (iscl)
                {
                    RemoveNode(delnode); //remove the cluster node itself.
                }
            }


            return true;
        }

        public bool RemoveEntity(MCScenarioEntityOverride entity)
        {
            var crgn = Region;
            if (crgn == null) return false;


            crgn.RemoveEntity(entity);





            Dictionary<MCExtensionDefSpawnPoint, int> ndict = new Dictionary<MCExtensionDefSpawnPoint, int>();
            if (entity.ScenarioPoints != null)
            {
                foreach (var point in entity.ScenarioPoints)
                {
                    ndict[point] = 1;
                }
            }
            List<ScenarioNode> delnodes = new List<ScenarioNode>();
            foreach (var node in Nodes)
            {
                if ((node.EntityPoint != null) && (ndict.ContainsKey(node.EntityPoint)))
                {
                    delnodes.Add(node);
                }
                else if (node.Entity == entity)
                {
                    delnodes.Add(node);
                }
            }
            foreach (var delnode in delnodes)
            {
                delnode.Entity = null;
                delnode.EntityPoint = null;//this entity point has been removed from the region. remove this association.
                RemoveNode(delnode);
            }

            return true;
        }



        public byte[] Save()
        {
            if (Region == null) return null;

            RebuildAccelGrid();
            RebuildLookUps();
            RebuildChains();

            MetaBuilder mb = new MetaBuilder();

            var ptr = Region.Save(mb);

            Meta meta = mb.GetMeta();

            byte[] data = ResourceBuilder.Build(meta, 2); //scenario ymt is version 2...

            return data;
        }




        public void RebuildAccelGrid()
        {
            if (Region == null)
                return;

            //find the grid extents, then sort points into the cell buckets.
            //output cell end point indexes to the accel grid data.

            Vector3 vmin = new Vector3(float.MaxValue);
            Vector3 vmax = new Vector3(float.MinValue);
            var points = Region.Points?.MyPoints;
            if ((points != null) && (points.Length > 0))
            {
                foreach (var point in points)
                {
                    var pos = point.Position;
                    vmin = Vector3.Min(vmin, pos);
                    vmax = Vector3.Max(vmax, pos);
                }
            }
            else
            {
                vmin = Vector3.Zero;
                vmax = Vector3.Zero;
            }

            //need to first find the correct cell size - aim for a maximum of 999 cells
            //start at 32x32 size, increment until cell count is within the limit.
            float cellsize = 32;
            Vector2 gmin = new Vector2(vmin.X / cellsize, vmin.Y / cellsize);
            Vector2 gmax = new Vector2(vmax.X / cellsize, vmax.Y / cellsize);
            Vector2I imin = new Vector2I(gmin);
            Vector2I imax = new Vector2I(gmax);
            Vector2I irng = new Vector2I(1, 1) + imax - imin;
            int cellcount = irng.X * irng.Y;
            while (cellcount > 999)
            {
                cellsize *= 2.0f;
                gmin *= 0.5f;
                gmax *= 0.5f;
                imin = new Vector2I(gmin);
                imax = new Vector2I(gmax);
                irng = new Vector2I(1, 1) + imax - imin;
                cellcount = irng.X * irng.Y;
            }



            List<MCScenarioPoint>[] cells = new List<MCScenarioPoint>[cellcount];
            if ((points != null) && (points.Length > 0))
            {
                foreach (var point in points)
                {
                    var pos = point.Position;
                    Vector2 gpos = new Vector2(pos.X / cellsize, pos.Y / cellsize);
                    Vector2I ipos = new Vector2I(gpos) - imin;
                    if (ipos.X < 0)
                    { ipos.X = 0; }
                    if (ipos.Y < 0)
                    { ipos.Y = 0; }
                    if (ipos.X >= irng.X)
                    { ipos.X = irng.X - 1; }
                    if (ipos.Y >= irng.Y)
                    { ipos.Y = irng.Y - 1; }

                    int idx = ipos.X + ipos.Y * irng.X;
                    if (idx < 0)
                    { idx = 0; }
                    if (idx >= cellcount)
                    { idx = cellcount - 1; }

                    var cell = cells[idx];
                    if (cell == null)
                    {
                        cell = new List<MCScenarioPoint>();
                        cells[idx] = cell;
                    }

                    cell.Add(point);
                }
            }

            List<MCScenarioPoint> newpoints = new List<MCScenarioPoint>();
            List<ushort> newids = new List<ushort>();
            foreach (var cell in cells)
            {
                bool flag = false;
                if (cell != null)
                {
                    newpoints.AddRange(cell);
                    foreach (var point in cell)
                    {
                        if ((point.Flags & CScenarioPointFlags__Flags.ExtendedRange) > 0)
                        {
                            flag = true;
                        }
                    }
                }

                ushort cid = (ushort)newpoints.Count;
                if (flag)
                {
                    cid += 32768; //any cells with extended range points have this bit set.
                }

                newids.Add(cid);
            }

            Region.Unk_3844724227 = newids.ToArray();

            


            rage__spdGrid2D grid = new rage__spdGrid2D();
            grid.CellDimX = cellsize;
            grid.CellDimY = cellsize;
            grid.MinCellX = imin.X;
            grid.MinCellY = imin.Y;
            grid.MaxCellX = imax.X;
            grid.MaxCellY = imax.Y;
            Region._Data.AccelGrid = grid;

            //store the reordered points.
            if (newpoints.Count > 0)
            {
                Region.Points.MyPoints = newpoints.ToArray();
            }
            else
            {
                Region.Points.MyPoints = null; //todo: error instead?
            }


        }
        public void RebuildLookUps()
        {
            if (Region is null)
                return;

            //find all unique hashes from the points, and assign new indices on points.

            //var d = Region.LookUps.Data;
            Dictionary<MetaHash, int> typeNames = new Dictionary<MetaHash, int>(); //scenario type hashes used by points
            Dictionary<MetaHash, int> pedModelSetNames = new Dictionary<MetaHash, int>(); //ped names
            Dictionary<MetaHash, int> vehicleModelSetNames = new Dictionary<MetaHash, int>(); //vehicle names
            Dictionary<MetaHash, int> interiorNames = new Dictionary<MetaHash, int>();
            Dictionary<MetaHash, int> groupNames = new Dictionary<MetaHash, int>();  //scenario group names?
            Dictionary<MetaHash, int> imapNames = new Dictionary<MetaHash, int>(); //ymap names
            var nonehash = JenkHash.GenHash("none");
            //typeNames[nonehash] = 0;
            pedModelSetNames[nonehash] = 0;
            vehicleModelSetNames[nonehash] = 0;
            interiorNames[nonehash] = 0;
            groupNames[nonehash] = 0;
            imapNames[nonehash] = 0;

            foreach (var node in Nodes)
            {
                if (node.MyPoint != null)
                {
                    var mp = node.MyPoint;
                    int typeid = 0;
                    int modelsetid = 0;
                    int interiorid = 0;
                    int groupid = 0;
                    int imapid = 0;
                    if (mp.Type != null && !typeNames.TryGetValue(mp.Type.NameHash, out typeid))
                    {
                        typeid = typeNames.Count;
                        typeNames[mp.Type.NameHash] = typeid;
                    }
                    if (mp.ModelSet != null)
                    {
                        bool isveh = mp.Type?.IsVehicle ?? false;
                        if (isveh)
                        {
                            if (!vehicleModelSetNames.TryGetValue(mp.ModelSet.NameHash, out modelsetid))
                            {
                                modelsetid = vehicleModelSetNames.Count;
                                vehicleModelSetNames[mp.ModelSet.NameHash] = modelsetid;
                            }
                        }
                        else
                        {
                            if (!pedModelSetNames.TryGetValue(mp.ModelSet.NameHash, out modelsetid))
                            {
                                modelsetid = pedModelSetNames.Count;
                                pedModelSetNames[mp.ModelSet.NameHash] = modelsetid;
                            }
                        }
                    }
                    if (mp.InteriorName != 0 && !interiorNames.TryGetValue(mp.InteriorName, out interiorid))
                    {
                        interiorid = interiorNames.Count;
                        interiorNames[mp.InteriorName] = interiorid;
                    }
                    if (mp.GroupName != 0 && !groupNames.TryGetValue(mp.GroupName, out groupid))
                    {
                        groupid = groupNames.Count;
                        groupNames[mp.GroupName] = groupid;
                    }
                    if (mp.IMapName != 0 && !imapNames.TryGetValue(mp.IMapName, out imapid))
                    {
                        imapid = imapNames.Count;
                        imapNames[mp.IMapName] = imapid;
                    }
                    mp.TypeId = (byte)typeid;
                    mp.ModelSetId = (byte)modelsetid;
                    mp.InteriorId = (byte)interiorid;
                    mp.GroupId = (ushort)groupid;
                    mp.IMapId = (byte)imapid;
                }
                if (node.LoadSavePoint is not null)
                {
                    var sp = node.LoadSavePoint;
                    if (sp.SpawnType != 0 && !typeNames.TryGetValue(sp.SpawnType, out var typeid))
                    {
                        typeid = typeNames.Count;
                        typeNames[sp.SpawnType] = typeid;
                    }
                    if (sp.PedType != 0 && !pedModelSetNames.TryGetValue(sp.PedType, out var modelsetid))
                    {
                        modelsetid = pedModelSetNames.Count;
                        pedModelSetNames[sp.PedType] = modelsetid;
                    }
                    if (sp.Group != 0 && !groupNames.TryGetValue(sp.Group, out var groupid))
                    {
                        groupid = groupNames.Count;
                        groupNames[sp.Group] = groupid;
                    }
                    if (sp.Interior != 0 && !interiorNames.TryGetValue(sp.Interior, out var interiorid))
                    {
                        interiorid = interiorNames.Count;
                        interiorNames[sp.Interior] = interiorid;
                    }
                    if (sp.RequiredImap != 0 && !imapNames.TryGetValue(sp.RequiredImap, out var imapid))
                    {
                        imapid = imapNames.Count;
                        imapNames[sp.RequiredImap] = imapid;
                    }
                }
                //if (node.Cluster is not null)
                //{
                //    var cl = node.Cluster;
                //}
                if (node.ClusterMyPoint != null)
                {
                    var mp = node.ClusterMyPoint;
                    int typeid = 0;
                    int modelsetid = 0;
                    int interiorid = 0;
                    int groupid = 0;
                    int imapid = 0;
                    if (mp.Type != null && !typeNames.TryGetValue(mp.Type.NameHash, out typeid))
                    {
                        typeid = typeNames.Count;
                        typeNames[mp.Type.NameHash] = typeid;
                    }
                    if (mp.ModelSet != null)
                    {
                        bool isveh = mp.Type?.IsVehicle ?? false;
                        if (isveh)
                        {
                            if (!vehicleModelSetNames.TryGetValue(mp.ModelSet.NameHash, out modelsetid))
                            {
                                modelsetid = vehicleModelSetNames.Count;
                                vehicleModelSetNames[mp.ModelSet.NameHash] = modelsetid;
                            }
                        }
                        else
                        {
                            if (!pedModelSetNames.TryGetValue(mp.ModelSet.NameHash, out modelsetid))
                            {
                                modelsetid = pedModelSetNames.Count;
                                pedModelSetNames[mp.ModelSet.NameHash] = modelsetid;
                            }
                        }
                    }
                    if ((mp.InteriorName != 0) && (!interiorNames.TryGetValue(mp.InteriorName, out interiorid)))
                    {
                        interiorid = interiorNames.Count;
                        interiorNames[mp.InteriorName] = interiorid;
                    }
                    if ((mp.GroupName != 0) && (!groupNames.TryGetValue(mp.GroupName, out groupid)))
                    {
                        groupid = groupNames.Count;
                        groupNames[mp.GroupName] = groupid;
                    }
                    if ((mp.IMapName != 0) && (!imapNames.TryGetValue(mp.IMapName, out imapid)))
                    {
                        imapid = imapNames.Count;
                        imapNames[mp.IMapName] = imapid;
                    }
                    mp.TypeId = (byte)typeid;
                    mp.ModelSetId = (byte)modelsetid;
                    mp.InteriorId = (byte)interiorid;
                    mp.GroupId = (ushort)groupid;
                    mp.IMapId = (byte)imapid;
                }
                //if (node.ClusterLoadSavePoint != null)
                //{
                //    var sp = node.ClusterLoadSavePoint;
                //    //int typeid = 0;
                //    //int modelsetid = 0;
                //    //int interiorid = 0;
                //    //int groupid = 0;
                //    //int imapid = 0;
                //    //if ((sp.SpawnType != 0) && (!typeNames.TryGetValue(sp.SpawnType, out typeid)))
                //    //{
                //    //    typeid = typeNames.Count;
                //    //    typeNames[sp.SpawnType] = typeid;
                //    //}
                //    //if ((sp.PedType != 0) && (!pedModelSetNames.TryGetValue(sp.PedType, out modelsetid)))
                //    //{
                //    //    modelsetid = pedModelSetNames.Count;
                //    //    pedModelSetNames[sp.PedType] = modelsetid;
                //    //}
                //    //if ((sp.Group != 0) && (!groupNames.TryGetValue(sp.Group, out groupid)))
                //    //{
                //    //    groupid = groupNames.Count;
                //    //    groupNames[sp.Group] = groupid;
                //    //}
                //    //if ((sp.Interior != 0) && (!interiorNames.TryGetValue(sp.Interior, out interiorid)))
                //    //{
                //    //    interiorid = interiorNames.Count;
                //    //    interiorNames[sp.Interior] = interiorid;
                //    //}
                //    //if ((sp.RequiredImap != 0) && (!imapNames.TryGetValue(sp.RequiredImap, out imapid)))
                //    //{
                //    //    imapid = imapNames.Count;
                //    //    imapNames[sp.RequiredImap] = imapid;
                //    //}
                //}
                //if (node.Entity != null)
                //{
                //    var en = node.Entity;
                //}
                //if (node.EntityPoint != null)
                //{
                //    var sp = node.EntityPoint;
                //    //int typeid = 0;
                //    //int modelsetid = 0;
                //    //int interiorid = 0;
                //    //int groupid = 0;
                //    //int imapid = 0;
                //    //if ((sp.SpawnType != 0) && (!typeNames.TryGetValue(sp.SpawnType, out typeid)))
                //    //{
                //    //    typeid = typeNames.Count;
                //    //    typeNames[sp.SpawnType] = typeid;
                //    //}
                //    //if ((sp.PedType != 0) && (!pedModelSetNames.TryGetValue(sp.PedType, out modelsetid)))
                //    //{
                //    //    modelsetid = pedModelSetNames.Count;
                //    //    pedModelSetNames[sp.PedType] = modelsetid;
                //    //}
                //    //if ((sp.Group != 0) && (!groupNames.TryGetValue(sp.Group, out groupid)))
                //    //{
                //    //    groupid = groupNames.Count;
                //    //    groupNames[sp.Group] = groupid;
                //    //}
                //    //if ((sp.Interior != 0) && (!interiorNames.TryGetValue(sp.Interior, out interiorid)))
                //    //{
                //    //    interiorid = interiorNames.Count;
                //    //    interiorNames[sp.Interior] = interiorid;
                //    //}
                //    //if ((sp.RequiredImap != 0) && (!imapNames.TryGetValue(sp.RequiredImap, out imapid)))
                //    //{
                //    //    imapid = imapNames.Count;
                //    //    imapNames[sp.RequiredImap] = imapid;
                //    //}
                //}
                //if (node.ChainingNode != null)
                //{
                //    var cn = node.ChainingNode;
                //    //int typeid = 0;
                //    //if ((cn.Type != null) && (!typeNames.TryGetValue(cn.Type.NameHash, out typeid)))
                //    //{
                //    //    typeid = typeNames.Count;
                //    //    typeNames[cn.Type.NameHash] = typeid;
                //    //}
                //    //cn.TypeHash = cn.Type?.NameHash ?? 0;
                //}
            }


            MetaHash[] htypeNames = new MetaHash[typeNames.Count];
            MetaHash[] hpedModelSetNames = new MetaHash[pedModelSetNames.Count];
            MetaHash[] hvehicleModelSetNames = new MetaHash[vehicleModelSetNames.Count];
            MetaHash[] hinteriorNames = new MetaHash[interiorNames.Count];
            MetaHash[] hgroupNames = new MetaHash[groupNames.Count];
            MetaHash[] himapNames = new MetaHash[imapNames.Count];
            foreach (var kvp in typeNames)
            {
                if (kvp.Value >= htypeNames.Length)
                    continue;
                htypeNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in pedModelSetNames)
            {
                if (kvp.Value >= hpedModelSetNames.Length)
                    continue;
                hpedModelSetNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in vehicleModelSetNames)
            {
                if (kvp.Value >= hvehicleModelSetNames.Length)
                    continue;
                hvehicleModelSetNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in interiorNames)
            {
                if (kvp.Value >= hinteriorNames.Length)
                    continue;
                hinteriorNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in groupNames)
            {
                if (kvp.Value >= hgroupNames.Length)
                    continue;
                hgroupNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in imapNames)
            {
                if (kvp.Value >= himapNames.Length)
                    continue;
                himapNames[kvp.Value] = kvp.Key;
            }


            if (Region.LookUps is null)
            {
                Region.LookUps = new MCScenarioPointLookUps();
                Region.LookUps.Region = Region;
            }

            var d = Region.LookUps;
            d.TypeNames = htypeNames;
            d.PedModelSetNames = hpedModelSetNames;
            d.VehicleModelSetNames = hvehicleModelSetNames;
            d.InteriorNames = hinteriorNames;
            d.GroupNames = hgroupNames;
            d.RequiredIMapNames = himapNames;


        }
        public void RebuildChains()
        {
            if (Region is null) return;

            //update chain nodes array, update from/to indexes
            //currently not necessary - editor updates indexes and arrays already.

        }


    }







    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ScenarioNode : BasePathNode
    {
        public YmtFile Ymt { get; set; }
        public MCScenarioPointRegion Region { get; set; }

        public MCScenarioPoint MyPoint { get; set; }
        public MCExtensionDefSpawnPoint LoadSavePoint { get; set; }
        public MCScenarioPointCluster Cluster { get; set; }
        public MCScenarioPoint ClusterMyPoint { get; set; }
        public MCExtensionDefSpawnPoint ClusterLoadSavePoint { get; set; }
        public MCScenarioEntityOverride Entity { get; set; }
        public MCExtensionDefSpawnPoint EntityPoint { get; set; }
        public MCScenarioChainingNode ChainingNode { get; set; }

        public Vector3 _Position;
        public ref Vector3 Position => ref _Position;

        public Quaternion _Orientation = Quaternion.Identity;
        public ref Quaternion Orientation => ref _Orientation;

        public string ShortTypeName
        {
            get
            {
                if (MyPoint is not null)
                    return "ScenarioPoint";

                if (LoadSavePoint is not null)
                    return "ScenarioPoint";

                if (ClusterMyPoint is not null)
                    return "ScenarioPoint";

                if (ClusterLoadSavePoint is not null)
                    return "ScenarioPoint";

                if (Cluster is not null)
                    return "ScenarioCluster";

                if (EntityPoint is not null)
                    return "ScenarioPoint";

                if (Entity is not null)
                    return "ScenarioPoint";

                if (ChainingNode is not null)
                    return "ScenarioPoint";

                return "ScenarioPoint";
            }
        }
        public string FullTypeName
        {
            get
            {
                if (MyPoint is not null)
                    return "Scenario MyPoint";

                if (LoadSavePoint is not null)
                    return "Scenario LoadSavePoint";

                if (ClusterMyPoint is not null)
                    return "Scenario Cluster MyPoint";

                if (ClusterLoadSavePoint is not null)
                    return "Scenario Cluster LoadSavePoint";

                if (Cluster is not null)
                    return "Scenario Cluster";

                if (EntityPoint is not null)
                    return "Scenario Entity Override Point";

                if (Entity is not null)
                    return "Scenario Entity Override";

                if (ChainingNode is not null)
                    return "Scenario Chaining Node";

                return "Scenario Point";
            }
        }
        public string MedTypeName
        {
            get
            {
                if (MyPoint is not null)
                    return "MyPoint";
                if (LoadSavePoint is not null)
                    return "LoadSavePoint";
                if (ClusterMyPoint is not null)
                    return "Cluster MyPoint";
                if (ClusterLoadSavePoint is not null)
                    return "Cluster LoadSavePoint";
                if (Cluster is not null)
                    return "Cluster";
                if (EntityPoint is not null)
                    return "Entity Override Point";
                if (Entity is not null)
                    return "Entity Override";
                if (ChainingNode is not null)
                    return "Chaining Node";
                return "Point";
            }
        }
        public string StringText
        {
            get
            {
                if (MyPoint is not null)
                    return MyPoint.ToString();

                if (LoadSavePoint is not null)
                    return LoadSavePoint.ToString();

                if (ClusterMyPoint is not null)
                    return ClusterMyPoint.ToString();

                if (ClusterLoadSavePoint is not null)
                    return ClusterLoadSavePoint.ToString();

                if (Cluster is not null)
                    return Cluster.ToString();

                if (EntityPoint is not null)
                    return EntityPoint.ToString();

                if (Entity is not null)
                    return Entity.ToString();

                if (ChainingNode is not null)
                    return ChainingNode.ToString();

                return FloatUtil.GetVector3String(Position);
            }
        }



        public ScenarioNode(YmtFile ymt)
        {
            Ymt = ymt;
            Region = ymt.ScenarioRegion?.Region;
        }


        public void SetPosition(Vector3 position)
        {
            Position = position;

            if (MyPoint is not null)
                MyPoint.Position = position;

            if (LoadSavePoint is not null)
                LoadSavePoint.Position = position;

            if (ClusterMyPoint is not null)
                ClusterMyPoint.Position = position;

            if (ClusterLoadSavePoint is not null)
                ClusterLoadSavePoint.Position = position;

            if (Cluster is not null && ClusterMyPoint is null && ClusterLoadSavePoint is null)
                Cluster.Position = position;

            if (EntityPoint is not null)
                EntityPoint.Position = position;

            if (Entity is not null && EntityPoint is null)
                Entity.Position = position;

            if (ChainingNode is not null)
                ChainingNode.Position = position;
        }
        public void SetOrientation(Quaternion orientation)
        {
            Orientation = orientation;

            if (MyPoint is not null)
                MyPoint.Orientation = orientation;

            if (LoadSavePoint is not null)
                LoadSavePoint.Orientation = orientation;

            if (ClusterMyPoint is not null)
                ClusterMyPoint.Orientation = orientation;

            if (ClusterLoadSavePoint is not null)
                ClusterLoadSavePoint.Orientation = orientation;

            //if (Cluster != null) Cluster.Orientation = orientation;
            if (EntityPoint is not null)
                EntityPoint.Orientation = orientation;

            //if (Entity != null) Entity.Orientation = orientation;
            //if (ChainingNode != null) ChainingNode.Orientation = orientation;
        }


        public override string ToString()
        {
            return $"{MedTypeName} {StringText}";
        }

    }






    public class ScenarioTypes
    {
        private Dictionary<uint, ScenarioTypeRef>? TypeRefs { get; set; }
        private Dictionary<uint, ScenarioType>? Types { get; set; }
        private Dictionary<uint, ScenarioTypeGroup>? TypeGroups { get; set; }
        private Dictionary<uint, AmbientModelSet>? PropSets { get; set; }
        private Dictionary<uint, AmbientModelSet>? PedModelSets { get; set; }
        private Dictionary<uint, AmbientModelSet>? VehicleModelSets { get; set; }
        private Dictionary<uint, ConditionalAnimsGroup>? AnimGroups { get; set; }



        public async Task LoadAsync(GameFileCache gfc)
        {
            await Task.WhenAll([
                Task.Run(() => Types = LoadTypes(gfc, "common:\\data\\ai\\scenarios.meta")),
                Task.Run(() => TypeGroups = LoadTypeGroups(gfc, "common:\\data\\ai\\scenarios.meta")),
                Task.Run(() => PropSets = LoadModelSets(gfc, "common:\\data\\ai\\propsets.meta")),
                Task.Run(() => PedModelSets = LoadModelSets(gfc, "common:\\data\\ai\\ambientpedmodelsets.meta")),
                Task.Run(() => VehicleModelSets = LoadModelSets(gfc, "common:\\data\\ai\\vehiclemodelsets.meta")),
                Task.Run(() => AnimGroups = LoadAnimGroups(gfc, "common:\\data\\ai\\conditionalanims.meta")),
            ]);

            TypeRefs = new Dictionary<uint, ScenarioTypeRef>(Types.Count + TypeGroups.Count);

            lock (TypeRefs)
            {
                if (Types is not null)
                {
                    lock (Types)
                    {
                        foreach (var (key, value) in Types)
                        {
                            TypeRefs[key] = new ScenarioTypeRef(value);
                        }
                    }
                }
                if (TypeGroups is not null)
                {
                    lock(TypeGroups)
                    {
                        foreach (var (key, value) in TypeGroups)
                        {
                            TypeRefs[key] = new ScenarioTypeRef(value);
                        }
                    }
                }
            }
        }


        private XmlDocument LoadXml(GameFileCache gfc, string filename)
        {
            string comstr = filename.Replace("common:", "common.rpf");
            string updstr = filename.Replace("common:", "update\\update.rpf\\common");
            string usestr = gfc.EnableDlc ? updstr : comstr;
            var xml = gfc.RpfMan.GetFileXml(usestr);
            if ((xml == null) || (xml.DocumentElement == null))
            {
                xml = gfc.RpfMan.GetFileXml(comstr);
            }
            return xml;
        }

        private Dictionary<uint, ScenarioType> LoadTypes(GameFileCache gfc, string filename)
        {
            Dictionary<uint, ScenarioType> types = new Dictionary<uint, ScenarioType>();

            var xml = LoadXml(gfc, filename);

            if (xml?.DocumentElement is null)
            {
                return types;
            }

            var typesxml = xml.DocumentElement;
            var items = typesxml.SelectNodes("Scenarios/Item");

            if (items is null)
                return types;

            foreach (XmlNode item in items)
            {
                var typestr = Xml.GetStringAttribute(item, "type");
                ScenarioType typeobj;
                switch (typestr)
                {
                    case "CScenarioPlayAnimsInfo":
                        typeobj = new ScenarioTypePlayAnims();
                        break;
                    case "CScenarioWanderingInfo":
                    case "CScenarioJoggingInfo":
                    case "CScenarioFleeInfo":
                    case "CScenarioLookAtInfo":
                        typeobj = new ScenarioType();
                        break;
                    case "CScenarioVehicleInfo":
                    case "CScenarioVehicleParkInfo":
                        typeobj = new ScenarioType();
                        typeobj.IsVehicle = true;
                        break;
                    default:
                        typeobj = new ScenarioType();
                        break;
                }

                if (typeobj is not null)
                {
                    typeobj.Load(item);
                    if (!string.IsNullOrEmpty(typeobj.Name))
                    {
                        JenkIndex.EnsureLower(typeobj.Name);
                        uint hash = JenkHash.GenHashLower(typeobj.Name);
                        types[hash] = typeobj;
                    }
                }

            }

            JenkIndex.Ensure("none");

            return types;
        }

        private Dictionary<uint, ScenarioTypeGroup> LoadTypeGroups(GameFileCache gfc, string filename)
        {
            Dictionary<uint, ScenarioTypeGroup> types = new Dictionary<uint, ScenarioTypeGroup>();

            var xml = LoadXml(gfc, filename);

            if (xml?.DocumentElement == null)
            {
                return types;
            }

            var typesxml = xml.DocumentElement;
            var items = typesxml.SelectNodes("ScenarioTypeGroups/Item");

            if (items is null)
                return types;

            foreach (XmlNode item in items)
            {
                ScenarioTypeGroup group = new ScenarioTypeGroup();

                group.Load(item);
                if (!string.IsNullOrEmpty(group.Name))
                {
                    JenkIndex.EnsureLower(group.Name);
                    uint hash = JenkHash.GenHashLower(group.Name);
                    types[hash] = group;
                }
            }

            JenkIndex.Ensure("none");

            return types;
        }

        private Dictionary<uint, AmbientModelSet> LoadModelSets(GameFileCache gfc, string filename)
        {
            Dictionary<uint, AmbientModelSet> sets = new Dictionary<uint, AmbientModelSet>();

            var xml = LoadXml(gfc, filename);

            if (xml?.DocumentElement == null)
            {
                return sets;
            }

            var setsxml = xml.DocumentElement;
            var items = setsxml.SelectNodes("ModelSets/Item");

            if (items is null)
                return sets;

            var noneset = new AmbientModelSet();
            noneset.Name = "NONE";
            noneset.NameHash = JenkHash.GenHash("none");
            sets[noneset.NameHash] = noneset;


            foreach (XmlNode item in items)
            {
                AmbientModelSet set = new AmbientModelSet();
                set.Load(item);
                if (!string.IsNullOrEmpty(set.Name))
                {
                    uint hash = JenkHash.GenHashLower(set.Name);
                    JenkIndex.Ensure(set.Name, hash);
                    sets[hash] = set;
                }
            }

            return sets;
        }

        private Dictionary<uint, ConditionalAnimsGroup> LoadAnimGroups(GameFileCache gfc, string filename)
        {
            Dictionary<uint, ConditionalAnimsGroup> groups = new Dictionary<uint, ConditionalAnimsGroup>();

            var xml = LoadXml(gfc, filename);

            if (xml?.DocumentElement is null)
            {
                return groups;
            }

            var setsxml = xml.DocumentElement;
            var items = setsxml.SelectNodes("ConditionalAnimsGroup/Item");

            if (items is null)
                return groups;

            foreach (XmlNode item in items)
            {
                ConditionalAnimsGroup group = new ConditionalAnimsGroup();
                group.Load(item);
                if (!string.IsNullOrEmpty(group.Name))
                {
                    uint hash = JenkHash.GenHashLower(group.Name);
                    JenkIndex.Ensure(group.Name);
                    JenkIndex.Ensure(group.Name, hash);
                    
                    groups[hash] = group;
                }
            }

            return groups;
        }




        public ScenarioTypeRef? GetScenarioTypeRef(uint hash)
        {
            if (TypeRefs is null)
                return null;

            lock (TypeRefs)
            {
                TypeRefs.TryGetValue(hash, out var st);
                return st;
            }
        }
        public ScenarioType? GetScenarioType(uint hash)
        {
            if (Types is null)
                return null;

            lock (Types)
            {
                Types.TryGetValue(hash, out var st);
                return st;
            }
        }
        public ScenarioTypeGroup? GetScenarioTypeGroup(uint hash)
        {
            if (TypeGroups is null)
                return null;

            lock (TypeGroups)
            {
                TypeGroups.TryGetValue(hash, out var tg);
                return tg;
            }
        }
        public AmbientModelSet? GetPropSet(uint hash)
        {
            if (PropSets is null)
                return null;

            lock (PropSets)
            {
                PropSets.TryGetValue(hash, out var ms);
                return ms;
            }
        }
        public AmbientModelSet? GetPedModelSet(uint hash)
        {
            if (PedModelSets is null)
                return null;

            lock(PedModelSets)
            {
                ref var ms = ref CollectionsMarshal.GetValueRefOrAddDefault(PedModelSets, hash, out var exists);
                if (!exists)
                {
                    string s_hash = hash.ToString("X");
                    ms = new AmbientModelSet();
                    ms.Name = $"UNKNOWN PED MODELSET ({s_hash})";
                    ms.NameHash = new MetaHash(hash);
                    ms.Models = [];
                }
                return ms;
            }
        }
        public AmbientModelSet? GetVehicleModelSet(uint hash)
        {

            if (VehicleModelSets is null)
                return null;
            lock(VehicleModelSets)
            {
                ref var ms = ref CollectionsMarshal.GetValueRefOrAddDefault(VehicleModelSets, hash, out var exists);
                if (!exists)
                {
                    string s_hash = hash.ToString("X");
                    ms = new AmbientModelSet();
                    ms.Name = $"UNKNOWN VEHICLE MODELSET ({s_hash})";
                    ms.NameHash = new MetaHash(hash);
                    ms.Models = [];
                }
                return ms;
            }
        }
        public ConditionalAnimsGroup? GetAnimGroup(uint hash)
        {
            if (AnimGroups == null)
                return null;

            lock(AnimGroups)
            {
                AnimGroups.TryGetValue(hash, out var ag);
                return ag;
            }
        }

        public ScenarioTypeRef[] GetScenarioTypeRefs()
        {
            if (TypeRefs is null)
                return [];

            lock(TypeRefs)
            {
                return TypeRefs.Values.ToArray();
            }
        }
        public ScenarioType[] GetScenarioTypes()
        {
            if (Types is null)
                return [];

            lock (Types)
            {
                return Types.Values.ToArray();
            }
        }
        public ScenarioTypeGroup[] GetScenarioTypeGroups()
        {
            if (TypeGroups is null)
                return [];
            lock (TypeGroups)
            {
                return TypeGroups.Values.ToArray();
            }
        }
        public AmbientModelSet[] GetPropSets()
        {
            if (PropSets is null)
                return [];
            lock (PropSets)
            {
                return PropSets.Values.ToArray();
            }
        }
        public AmbientModelSet[] GetPedModelSets()
        {
            if (PedModelSets is null)
                return [];
            lock (PedModelSets)
            {
                return PedModelSets.Values.ToArray();
            }
        }
        public AmbientModelSet[] GetVehicleModelSets()
        {
            if (VehicleModelSets is null)
                return [];
            lock (VehicleModelSets)
            {
                return VehicleModelSets.Values.ToArray();
            }
        }
        public ConditionalAnimsGroup[] GetAnimGroups()
        {
            if (AnimGroups is null)
                return [];

            lock (AnimGroups)
            {
                return AnimGroups.Values.ToArray();
            }
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ScenarioTypeRef
    {
        /// <summary>
        /// Represents a scenario type that may either be a <see cref="ScenarioType"/> or a <see cref="ScenarioTypeGroup"/>.
        /// Used with CScenarioChainingNode and CScenarioPoint.
        /// </summary>
        
        public string Name => IsGroup ? Group.Name : Type.Name;
        public MetaHash NameHash => IsGroup ? Group.NameHash : Type.NameHash;
        public bool IsVehicle => IsGroup ? false : Type.IsVehicle; // groups don't support vehicle infos, so always false
        public string VehicleModelSet => IsGroup ? null : Type.VehicleModelSet;
        public MetaHash VehicleModelSetHash => IsGroup ? 0 : Type.VehicleModelSetHash;

        public bool IsGroup { get; }
        public ScenarioType Type { get; }
        public ScenarioTypeGroup Group { get; }


        public ScenarioTypeRef(ScenarioType type)
        {
            IsGroup = false;
            Type = type;
            Group = null;
        }

        public ScenarioTypeRef(ScenarioTypeGroup group)
        {
            IsGroup = true;
            Type = null;
            Group = group;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ScenarioType
    {
        public string? Name { get; set; }
        public MetaHash NameHash { get; set; }
        public bool IsVehicle { get; set; }
        public string? VehicleModelSet { get; set; }
        public MetaHash VehicleModelSetHash { get; set; }


        public virtual void Load(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameHash = JenkHash.GenHashLower(Name);


            if (IsVehicle)
            {
                VehicleModelSet = Xml.GetChildStringAttribute(node, "VehicleModelSet", "ref");
                if (!string.IsNullOrEmpty(VehicleModelSet) && (VehicleModelSet != "NULL"))
                {
                    VehicleModelSetHash = JenkHash.GenHashLower(VehicleModelSet);
                }
            }
        }

        public override string? ToString()
        {
            return Name;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioTypePlayAnims : ScenarioType
    {

        public override void Load(XmlNode node)
        {
            base.Load(node);
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ScenarioTypeGroup
    {
        public string Name { get; set; }
        public MetaHash NameHash { get; set; }


        public void Load(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameHash = JenkHash.GenHashLower(Name);
        }

        public override string ToString() => Name;
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class AmbientModelSet
    {
        public string Name { get; set; }
        public MetaHash NameHash { get; set; }
        public AmbientModel[] Models { get; set; }


        public void Load(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameHash = JenkHash.GenHashLower(Name);

            var models = node.SelectNodes("Models/Item");
            var modellist = new List<AmbientModel>(models.Count);
            foreach (XmlNode item in models)
            {
                AmbientModel model = new AmbientModel();
                model.Load(item);
                modellist.Add(model);
            }
            Models = modellist.ToArray();
        }

        public override string ToString()
        {
            return Name;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AmbientModel
    {
        public string Name { get; set; }
        public string NameLower { get; set; }
        public float Probability { get; set; }
        public string VariationsType { get; set; }
        public AmbientModelVariation Variations { get; set; }

        public void Load(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameLower = Name.ToLowerInvariant();
            Probability = Xml.GetChildFloatAttribute(node, "Probability", "value");
            VariationsType = Xml.GetChildStringAttribute(node, "Variations", "type");
            var vars = node.SelectSingleNode("Variations");
            switch (VariationsType)
            {
                case "NULL":
                    break;
                case "CAmbientPedModelVariations":
                    break;
                case "CAmbientVehicleModelVariations":
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return Name + ", " + VariationsType;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class AmbientModelVariation
    {
        public string Type { get; set; }


        public void Load(XmlNode node)
        {
        }

        public override string ToString()
        {
            return Type;
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ConditionalAnimsGroup
    {
        public string Name { get; set; }


        public void Load(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
        }

        public override string ToString()
        {
            return Name;
        }
    }

}
