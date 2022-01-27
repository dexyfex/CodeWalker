using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Xml;
using System.ComponentModel;

namespace CodeWalker.World
{
    public class Scenarios
    {
        public volatile bool Inited = false;

        public Timecycle Timecycle { get; set; }
        public GameFileCache GameFileCache { get; set; }

        public static ScenarioTypes ScenarioTypes { get; set; }

        public List<YmtFile> ScenarioRegions { get; set; }


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus, Timecycle timecycle)
        {
            Timecycle = timecycle;
            GameFileCache = gameFileCache;


            EnsureScenarioTypes(gameFileCache);


            ScenarioRegions = new List<YmtFile>();


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
            YmtFile manifestymt = rpfman.GetFile<YmtFile>(manifestfilename);
            if ((manifestymt != null) && (manifestymt.CScenarioPointManifest != null))
            {

                foreach (var region in manifestymt.CScenarioPointManifest.RegionDefs)
                {
                    string regionfilename = region.Name.ToString() + ".ymt"; //JenkIndex lookup... ymt should have already loaded path strings into it! maybe change this...
                    string basefilename = regionfilename.Replace("platform:", "x64a.rpf");
                    string updfilename = regionfilename.Replace("platform:", "update\\update.rpf\\x64");
                    string usefilename = updfilename;

                    if (!gameFileCache.EnableDlc)
                    {
                        usefilename = basefilename;
                    }
                    YmtFile regionymt = rpfman.GetFile<YmtFile>(usefilename);

                    if (regionymt == null)
                    {
                        regionymt = rpfman.GetFile<YmtFile>(basefilename);
                    }

                    if (regionymt != null)
                    {
                        var sregion = regionymt.ScenarioRegion;
                        if (sregion != null)
                        {
                            ScenarioRegions.Add(regionymt);



                            ////testing stuff...

                            //var gd = regionymt?.CScenarioPointRegion?.Data.AccelGrid.Dimensions ?? new Vector2I();
                            //griddims.Add(gd);
                            //maxgrid = new Vector2I(Math.Max(maxgrid.X, gd.X), Math.Max(maxgrid.Y, gd.Y));
                            //maxcells = Math.Max(maxcells, gd.X * gd.Y);

                            //byte[] data = regionymt.Save();
                            //System.IO.File.WriteAllBytes("C:\\CodeWalker.Projects\\YmtTest\\AllYmts\\" + regionymt.Name, data);
                        }
                    }


                }

            }


            Inited = true;
        }


        public static void EnsureScenarioTypes(GameFileCache gfc)
        {
            //if (ScenarioTypes == null)
            //{
                var stypes = new ScenarioTypes();
                stypes.Load(gfc);
                ScenarioTypes = stypes;
            //}
        }


    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioRegion : BasePathData
    {
        public EditorVertex[] PathVerts { get; set; }
        public EditorVertex[] TriangleVerts { get; set; }
        public Vector4[] NodePositions { get; set; }

        public EditorVertex[] GetPathVertices()
        {
            return PathVerts;
        }
        public EditorVertex[] GetTriangleVertices()
        {
            return TriangleVerts;
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
            if ((Ymt == null) || (Region == null))
            { return; }

            if (Region.LookUps == null)
            { return; }

            if (Scenarios.ScenarioTypes == null) //these are loaded by Scenarios.Init
            { return; }

            if (Nodes == null) //nodes not loaded yet - BuildVertices needs to be called first!
            { return; }

            foreach (var node in Nodes)
            {
                LoadTypes(Region, node.MyPoint);
                LoadTypes(Region, node.ClusterMyPoint);
                LoadTypes(Region, node.ChainingNode);
            }
        }

        private void LoadTypes(MCScenarioPointRegion r, MCScenarioPoint scp)
        {
            if (scp == null) return;

            var types = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            if (types == null)
            { return; }

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
            else
            { }

            var msind = scp.ModelSetId;
            if (isveh)
            {
                if (msind < vehhashes.Length)
                {
                    var hash = vehhashes[msind];
                    scp.ModelSet = types.GetVehicleModelSet(hash);
                    if (scp.ModelSet != null)
                    { }
                    else if (hash != 493038497)//"None"
                    { }
                }
                else
                { }
            }
            else
            {
                if (msind < pedhashes.Length)
                {
                    var hash = pedhashes[msind];
                    scp.ModelSet = types.GetPedModelSet(hash);
                    if (scp.ModelSet != null)
                    { }
                    else if (hash != 493038497)//"None"
                    { }
                }
                else
                { }
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
            if (spn == null) return;
            var types = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            if (types == null)
            { return; }

            uint hash = spn._Data.ScenarioType;
            if ((hash != 0) && (hash != 493038497))
            {
                bool isveh = false;
                spn.Type = types.GetScenarioTypeRef(hash);
                isveh = spn.Type?.IsVehicle ?? false;
                if (isveh)
                { }
                else
                { }
            }
        }



        public void BuildNodes()
        {

            NodeDict = new Dictionary<Vector3, ScenarioNode>();
            Nodes = new List<ScenarioNode>();

            if ((Ymt != null) && (Ymt.CScenarioPointRegion != null))
            {
                var r = Ymt.CScenarioPointRegion;

                if ((r.Paths != null) && (r.Paths.Nodes != null))
                {
                    foreach (var node in r.Paths.Nodes)
                    {
                        EnsureNode(node);
                    }


                    List<MCScenarioChainingEdge> chainedges = new List<MCScenarioChainingEdge>();

                    if ((r.Paths.Chains != null) && (r.Paths.Edges != null))
                    {
                        var rp = r.Paths;
                        var rpc = rp.Chains;
                        var rpe = rp.Edges;
                        var rpn = rp.Nodes;

                        foreach (var chain in rpc)
                        {
                            chainedges.Clear();

                            if (chain.EdgeIds != null)
                            {
                                foreach (var edgeId in chain.EdgeIds)
                                {
                                    if (edgeId >= rpe.Length)
                                    { continue; }
                                    var edge = rpe[edgeId];

                                    if (edge.NodeIndexFrom >= rpn.Length)
                                    { continue; }
                                    if (edge.NodeIndexTo >= rpn.Length)
                                    { continue; }

                                    edge.NodeFrom = rpn[edge.NodeIndexFrom];
                                    edge.NodeTo = rpn[edge.NodeIndexTo];

                                    var nfc = edge.NodeFrom?.Chain;
                                    var ntc = edge.NodeTo?.Chain;

                                    if ((nfc != null) && (nfc != chain))
                                    { }
                                    if ((ntc != null) && (ntc != chain))
                                    { }

                                    if (edge.NodeFrom != null) edge.NodeFrom.Chain = chain;
                                    if (edge.NodeTo != null) edge.NodeTo.Chain = chain;

                                    chainedges.Add(edge);
                                }
                            }

                            chain.Edges = chainedges.ToArray();
                        }
                    }

                }

                if (r.Points != null)
                {
                    if (r.Points.MyPoints != null)
                    {
                        foreach (var point in r.Points.MyPoints)
                        {
                            EnsureNode(point);
                        }
                    }
                    if (r.Points.LoadSavePoints != null)
                    {
                        foreach (var point in r.Points.LoadSavePoints)
                        {
                            EnsureNode(point); //no hits here - not used?
                        }
                    }
                }

                if (r.Clusters != null) //spawn groups
                {
                    foreach (var cluster in r.Clusters)
                    {
                        EnsureClusterNode(cluster);

                        if (cluster.Points != null)
                        {
                            if (cluster.Points.MyPoints != null)
                            {
                                foreach (var point in cluster.Points.MyPoints)
                                {
                                    var node = EnsureClusterNode(point);
                                    node.Cluster = cluster;
                                }
                            }
                            if (cluster.Points.LoadSavePoints != null)
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

                if (r.EntityOverrides != null)
                {
                    foreach (var overr in r.EntityOverrides)
                    {
                        EnsureEntityNode(overr);

                        if (overr.ScenarioPoints != null)
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
            BVH = new PathBVH(Nodes, 10, 10);
        }

        public void BuildVertices()
        {

            List<EditorVertex> pathverts = new List<EditorVertex>();

            uint cred = (uint)Color.Red.ToRgba();
            uint cblu = (uint)Color.Blue.ToRgba();
            uint cgrn = (uint)Color.Green.ToBgra();
            uint cblk = (uint)Color.Black.ToBgra();

            if ((Ymt != null) && (Ymt.CScenarioPointRegion != null))
            {
                var r = Ymt.CScenarioPointRegion;
                EditorVertex pv1 = new EditorVertex();
                EditorVertex pv2 = new EditorVertex();

                if ((r.Paths != null) && (r.Paths.Nodes != null))
                {
                    if ((r.Paths.Chains != null) && (r.Paths.Edges != null))
                    {
                        foreach (var chain in r.Paths.Chains)
                        {
                            if (chain.Edges == null) continue;
                            foreach (var edge in chain.Edges)
                            {
                                var vid1 = edge._Data.NodeIndexFrom;
                                var vid2 = edge._Data.NodeIndexTo;
                                if ((vid1 >= r.Paths.Nodes.Length) || (vid2 >= r.Paths.Nodes.Length)) continue;
                                var v1 = r.Paths.Nodes[vid1];
                                var v2 = r.Paths.Nodes[vid2];
                                byte cr1 = (v1.HasIncomingEdges) ? (byte)255 : (byte)0;
                                byte cr2 = (v2.HasIncomingEdges) ? (byte)255 : (byte)0;
                                byte cg = 0;// (chain._Data.Unk_1156691834 > 1) ? (byte)255 : (byte)0;
                                //cg = ((v1.Unk1 != 0) || (v2.Unk1 != 0)) ? (byte)255 : (byte)0;
                                //cg = (edge.Action == CScenarioChainingEdge__eAction.Unk_7865678) ? (byte)255 : (byte)0;
                                //cg = ((v1.UnkValTest != 0) || (v2.UnkValTest != 0)) ? (byte)255 : (byte)0;

                                byte cb1 = (byte)(255 - cr1);
                                byte cb2 = (byte)(255 - cr2);
                                pv1.Position = v1.Position;
                                pv2.Position = v2.Position;
                                pv1.Colour = (uint)new Color(cr1, cg, cb1, (byte)255).ToRgba();// (v1._Data.HasIncomingEdges == 1) ? cred : cblu;
                                pv2.Colour = (uint)new Color(cr2, cg, cb2, (byte)255).ToRgba();// (v2._Data.HasIncomingEdges == 1) ? cred : cblu;
                                pathverts.Add(pv1);
                                pathverts.Add(pv2);
                            }
                        }
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


            }


            if (pathverts.Count > 0)
            {
                PathVerts = pathverts.ToArray();
            }
            else
            {
                PathVerts = null;
            }



            List<Vector4> nodes = new List<Vector4>(Nodes.Count);
            foreach (var node in Nodes)
            {
                nodes.Add(new Vector4(node.Position, 1.0f));
            }
            if (nodes.Count > 0)
            {
                NodePositions = nodes.ToArray();
            }
            else
            {
                NodePositions = null;
            }

        }






        private ScenarioNode EnsureNode(MCScenarioChainingNode cnode)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(cnode.Position, out exnode) && (exnode.ChainingNode == null))
            {
                exnode.ChainingNode = cnode;
            }
            else
            {
                exnode = new ScenarioNode(cnode.Region?.Ymt);
                exnode.ChainingNode = cnode;
                exnode.Position = cnode.Position;
                NodeDict[cnode.Position] = exnode;
                Nodes.Add(exnode);
            }
            cnode.ScenarioNode = exnode;
            return exnode;
        }
        private ScenarioNode EnsureNode(MCScenarioPoint point)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(point.Position, out exnode) && (exnode.MyPoint == null))
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
                NodeDict[point.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureNode(MCExtensionDefSpawnPoint point)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(point.Position, out exnode) && (exnode.LoadSavePoint == null))
            {
                exnode.LoadSavePoint = point;
            }
            else
            {
                exnode = new ScenarioNode(point.ScenarioRegion?.Ymt);
                exnode.LoadSavePoint = point;
                exnode.Position = point.Position;
                exnode.Orientation = point.Orientation;
                NodeDict[point.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureClusterNode(MCScenarioPointCluster cluster)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(cluster.Position, out exnode) && (exnode.Cluster == null))
            {
                exnode.Cluster = cluster;
            }
            else
            {
                exnode = new ScenarioNode(cluster.Region?.Ymt);
                exnode.Cluster = cluster;
                exnode.Position = cluster.Position;
                NodeDict[cluster.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureClusterNode(MCScenarioPoint point)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(point.Position, out exnode) && (exnode.ClusterMyPoint == null))
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
                NodeDict[point.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureClusterNode(MCExtensionDefSpawnPoint point)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(point.Position, out exnode) && (exnode.ClusterLoadSavePoint == null))
            {
                exnode.ClusterLoadSavePoint = point;
            }
            else
            {
                exnode = new ScenarioNode(point.ScenarioRegion?.Ymt);
                exnode.ClusterLoadSavePoint = point;
                exnode.Position = point.Position;
                NodeDict[point.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureEntityNode(MCExtensionDefSpawnPoint point)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(point.Position, out exnode) && (exnode.EntityPoint == null))
            {
                exnode.EntityPoint = point;
            }
            else
            {
                exnode = new ScenarioNode(point.ScenarioRegion?.Ymt);
                exnode.EntityPoint = point;
                exnode.Position = point.Position;
                exnode.Orientation = point.Orientation;
                NodeDict[point.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }
        private ScenarioNode EnsureEntityNode(MCScenarioEntityOverride entity)
        {
            ScenarioNode exnode;
            if (NodeDict.TryGetValue(entity.Position, out exnode) && (exnode.Entity == null))
            {
                exnode.Entity = entity;
            }
            else
            {
                exnode = new ScenarioNode(entity.Region?.Ymt);
                exnode.Entity = entity;
                exnode.Position = entity.Position;
                NodeDict[entity.Position] = exnode;
                Nodes.Add(exnode);
            }
            return exnode;
        }






        public ScenarioNode AddNode(ScenarioNode copy = null)
        {
            var n = new ScenarioNode(Ymt);

            var rgn = Ymt.CScenarioPointRegion;

            if (copy != null)
            {
                if (copy.MyPoint != null) n.MyPoint = new MCScenarioPoint(rgn, copy.MyPoint);
                if (copy.LoadSavePoint != null) n.LoadSavePoint = new MCExtensionDefSpawnPoint(rgn, copy.LoadSavePoint);
                if (copy.ClusterMyPoint != null)
                {
                    n.Cluster = copy.Cluster;
                    n.ClusterMyPoint = new MCScenarioPoint(rgn, copy.ClusterMyPoint);
                }
                else if (copy.ClusterLoadSavePoint != null)
                {
                    n.Cluster = copy.Cluster;
                    n.ClusterLoadSavePoint = new MCExtensionDefSpawnPoint(rgn, copy.ClusterLoadSavePoint);
                }
                else if (copy.Cluster != null)
                {
                    n.Cluster = new MCScenarioPointCluster(rgn, copy.Cluster);
                }
                if (copy.EntityPoint != null)
                {
                    n.Entity = copy.Entity;
                    n.EntityPoint = new MCExtensionDefSpawnPoint(rgn, copy.EntityPoint);
                }
                else if (copy.Entity != null)
                {
                    n.Entity = new MCScenarioEntityOverride(rgn, copy.Entity);
                }
                if (copy.ChainingNode != null)
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


            if ((Region != null) && (Region.Points != null))
            {
                if (n.MyPoint != null) Region.Points.AddMyPoint(n.MyPoint);
                if (n.LoadSavePoint != null) Region.Points.AddLoadSavePoint(n.LoadSavePoint);
                if ((n.Cluster != null) && (n.Cluster.Points != null))
                {
                    if (n.ClusterMyPoint != null) n.Cluster.Points.AddMyPoint(n.ClusterMyPoint);
                    if (n.ClusterLoadSavePoint != null) n.Cluster.Points.AddLoadSavePoint(n.ClusterLoadSavePoint);
                }
                if ((n.Entity != null) && (n.Entity.ScenarioPoints != null))
                {
                    if (n.EntityPoint != null) n.Entity.AddScenarioPoint(n.EntityPoint);
                }
            }
            if ((Region != null) && (Region.Paths != null))
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
            if (Region == null) return;

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
            if (Region == null) return;

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
                    if ((mp.Type != null) && (!typeNames.TryGetValue(mp.Type.NameHash, out typeid)))
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
                if (node.LoadSavePoint != null)
                {
                    var sp = node.LoadSavePoint;
                    int typeid = 0;
                    int modelsetid = 0;
                    int interiorid = 0;
                    int groupid = 0;
                    int imapid = 0;
                    if ((sp.SpawnType != 0) && (!typeNames.TryGetValue(sp.SpawnType, out typeid)))
                    {
                        typeid = typeNames.Count;
                        typeNames[sp.SpawnType] = typeid;
                    }
                    if ((sp.PedType != 0) && (!pedModelSetNames.TryGetValue(sp.PedType, out modelsetid)))
                    {
                        modelsetid = pedModelSetNames.Count;
                        pedModelSetNames[sp.PedType] = modelsetid;
                    }
                    if ((sp.Group != 0) && (!groupNames.TryGetValue(sp.Group, out groupid)))
                    {
                        groupid = groupNames.Count;
                        groupNames[sp.Group] = groupid;
                    }
                    if ((sp.Interior != 0) && (!interiorNames.TryGetValue(sp.Interior, out interiorid)))
                    {
                        interiorid = interiorNames.Count;
                        interiorNames[sp.Interior] = interiorid;
                    }
                    if ((sp.RequiredImap != 0) && (!imapNames.TryGetValue(sp.RequiredImap, out imapid)))
                    {
                        imapid = imapNames.Count;
                        imapNames[sp.RequiredImap] = imapid;
                    }
                }
                if (node.Cluster != null)
                {
                    var cl = node.Cluster;
                }
                if (node.ClusterMyPoint != null)
                {
                    var mp = node.ClusterMyPoint;
                    int typeid = 0;
                    int modelsetid = 0;
                    int interiorid = 0;
                    int groupid = 0;
                    int imapid = 0;
                    if ((mp.Type != null) && (!typeNames.TryGetValue(mp.Type.NameHash, out typeid)))
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
                if (node.ClusterLoadSavePoint != null)
                {
                    var sp = node.ClusterLoadSavePoint;
                    //int typeid = 0;
                    //int modelsetid = 0;
                    //int interiorid = 0;
                    //int groupid = 0;
                    //int imapid = 0;
                    //if ((sp.SpawnType != 0) && (!typeNames.TryGetValue(sp.SpawnType, out typeid)))
                    //{
                    //    typeid = typeNames.Count;
                    //    typeNames[sp.SpawnType] = typeid;
                    //}
                    //if ((sp.PedType != 0) && (!pedModelSetNames.TryGetValue(sp.PedType, out modelsetid)))
                    //{
                    //    modelsetid = pedModelSetNames.Count;
                    //    pedModelSetNames[sp.PedType] = modelsetid;
                    //}
                    //if ((sp.Group != 0) && (!groupNames.TryGetValue(sp.Group, out groupid)))
                    //{
                    //    groupid = groupNames.Count;
                    //    groupNames[sp.Group] = groupid;
                    //}
                    //if ((sp.Interior != 0) && (!interiorNames.TryGetValue(sp.Interior, out interiorid)))
                    //{
                    //    interiorid = interiorNames.Count;
                    //    interiorNames[sp.Interior] = interiorid;
                    //}
                    //if ((sp.RequiredImap != 0) && (!imapNames.TryGetValue(sp.RequiredImap, out imapid)))
                    //{
                    //    imapid = imapNames.Count;
                    //    imapNames[sp.RequiredImap] = imapid;
                    //}
                }
                if (node.Entity != null)
                {
                    var en = node.Entity;
                }
                if (node.EntityPoint != null)
                {
                    var sp = node.EntityPoint;
                    //int typeid = 0;
                    //int modelsetid = 0;
                    //int interiorid = 0;
                    //int groupid = 0;
                    //int imapid = 0;
                    //if ((sp.SpawnType != 0) && (!typeNames.TryGetValue(sp.SpawnType, out typeid)))
                    //{
                    //    typeid = typeNames.Count;
                    //    typeNames[sp.SpawnType] = typeid;
                    //}
                    //if ((sp.PedType != 0) && (!pedModelSetNames.TryGetValue(sp.PedType, out modelsetid)))
                    //{
                    //    modelsetid = pedModelSetNames.Count;
                    //    pedModelSetNames[sp.PedType] = modelsetid;
                    //}
                    //if ((sp.Group != 0) && (!groupNames.TryGetValue(sp.Group, out groupid)))
                    //{
                    //    groupid = groupNames.Count;
                    //    groupNames[sp.Group] = groupid;
                    //}
                    //if ((sp.Interior != 0) && (!interiorNames.TryGetValue(sp.Interior, out interiorid)))
                    //{
                    //    interiorid = interiorNames.Count;
                    //    interiorNames[sp.Interior] = interiorid;
                    //}
                    //if ((sp.RequiredImap != 0) && (!imapNames.TryGetValue(sp.RequiredImap, out imapid)))
                    //{
                    //    imapid = imapNames.Count;
                    //    imapNames[sp.RequiredImap] = imapid;
                    //}
                }
                if (node.ChainingNode != null)
                {
                    var cn = node.ChainingNode;
                    //int typeid = 0;
                    //if ((cn.Type != null) && (!typeNames.TryGetValue(cn.Type.NameHash, out typeid)))
                    //{
                    //    typeid = typeNames.Count;
                    //    typeNames[cn.Type.NameHash] = typeid;
                    //}
                    //cn.TypeHash = cn.Type?.NameHash ?? 0;
                }
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
                { continue; }
                htypeNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in pedModelSetNames)
            {
                if (kvp.Value >= hpedModelSetNames.Length)
                { continue; }
                hpedModelSetNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in vehicleModelSetNames)
            {
                if (kvp.Value >= hvehicleModelSetNames.Length)
                { continue; }
                hvehicleModelSetNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in interiorNames)
            {
                if (kvp.Value >= hinteriorNames.Length)
                { continue; }
                hinteriorNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in groupNames)
            {
                if (kvp.Value >= hgroupNames.Length)
                { continue; }
                hgroupNames[kvp.Value] = kvp.Key;
            }
            foreach (var kvp in imapNames)
            {
                if (kvp.Value >= himapNames.Length)
                { continue; }
                himapNames[kvp.Value] = kvp.Key;
            }


            if (Region.LookUps == null)
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
            if (Region == null) return;

            //update chain nodes array, update from/to indexes
            //currently not necessary - editor updates indexes and arrays already.

        }


    }







    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioNode : BasePathNode
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

        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; } = Quaternion.Identity;

        public string ShortTypeName
        {
            get
            {
                if (MyPoint != null) return "ScenarioPoint";
                if (LoadSavePoint != null) return "ScenarioPoint";
                if (ClusterMyPoint != null) return "ScenarioPoint";
                if (ClusterLoadSavePoint != null) return "ScenarioPoint";
                if (Cluster != null) return "ScenarioCluster";
                if (EntityPoint != null) return "ScenarioPoint";
                if (Entity != null) return "ScenarioPoint";
                if (ChainingNode != null) return "ScenarioPoint";
                return "ScenarioPoint";
            }
        }
        public string FullTypeName
        {
            get
            {
                if (MyPoint != null) return "Scenario MyPoint";
                if (LoadSavePoint != null) return "Scenario LoadSavePoint";
                if (ClusterMyPoint != null) return "Scenario Cluster MyPoint";
                if (ClusterLoadSavePoint != null) return "Scenario Cluster LoadSavePoint";
                if (Cluster != null) return "Scenario Cluster";
                if (EntityPoint != null) return "Scenario Entity Override Point";
                if (Entity != null) return "Scenario Entity Override";
                if (ChainingNode != null) return "Scenario Chaining Node";
                return "Scenario Point";
            }
        }
        public string MedTypeName
        {
            get
            {
                if (MyPoint != null) return "MyPoint";
                if (LoadSavePoint != null) return "LoadSavePoint";
                if (ClusterMyPoint != null) return "Cluster MyPoint";
                if (ClusterLoadSavePoint != null) return "Cluster LoadSavePoint";
                if (Cluster != null) return "Cluster";
                if (EntityPoint != null) return "Entity Override Point";
                if (Entity != null) return "Entity Override";
                if (ChainingNode != null) return "Chaining Node";
                return "Point";
            }
        }
        public string StringText
        {
            get
            {
                if (MyPoint != null) return MyPoint.ToString();
                if (LoadSavePoint != null) return LoadSavePoint.ToString();
                if (ClusterMyPoint != null) return ClusterMyPoint.ToString();
                if (ClusterLoadSavePoint != null) return ClusterLoadSavePoint.ToString();
                if (Cluster != null) return Cluster.ToString();
                if (EntityPoint != null) return EntityPoint.ToString();
                if (Entity != null) return Entity.ToString();
                if (ChainingNode != null) return ChainingNode.ToString();
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

            if (MyPoint != null) MyPoint.Position = position;
            if (LoadSavePoint != null) LoadSavePoint.Position = position;
            if (ClusterMyPoint != null) ClusterMyPoint.Position = position;
            if (ClusterLoadSavePoint != null) ClusterLoadSavePoint.Position = position;
            if ((Cluster != null) && (ClusterMyPoint == null) && (ClusterLoadSavePoint == null)) Cluster.Position = position;
            if (EntityPoint != null) EntityPoint.Position = position;
            if ((Entity != null) && (EntityPoint == null)) Entity.Position = position;
            if (ChainingNode != null) ChainingNode.Position = position;
        }
        public void SetOrientation(Quaternion orientation)
        {
            Orientation = orientation;

            if (MyPoint != null) MyPoint.Orientation = orientation;
            if (LoadSavePoint != null) LoadSavePoint.Orientation = orientation;
            if (ClusterMyPoint != null) ClusterMyPoint.Orientation = orientation;
            if (ClusterLoadSavePoint != null) ClusterLoadSavePoint.Orientation = orientation;
            //if (Cluster != null) Cluster.Orientation = orientation;
            if (EntityPoint != null) EntityPoint.Orientation = orientation;
            //if (Entity != null) Entity.Orientation = orientation;
            //if (ChainingNode != null) ChainingNode.Orientation = orientation;
        }



        public override string ToString()
        {
            return MedTypeName + " " + StringText;
        }

    }






    public class ScenarioTypes
    {
        private object SyncRoot = new object(); //keep this thread-safe.. technically shouldn't be necessary, but best to be safe

        private Dictionary<uint, ScenarioTypeRef> TypeRefs { get; set; }
        private Dictionary<uint, ScenarioType> Types { get; set; }
        private Dictionary<uint, ScenarioTypeGroup> TypeGroups { get; set; }
        private Dictionary<uint, AmbientModelSet> PropSets { get; set; }
        private Dictionary<uint, AmbientModelSet> PedModelSets { get; set; }
        private Dictionary<uint, AmbientModelSet> VehicleModelSets { get; set; }
        private Dictionary<uint, ConditionalAnimsGroup> AnimGroups { get; set; }



        public void Load(GameFileCache gfc)
        {
            lock (SyncRoot)
            {
                Types = LoadTypes(gfc, "common:\\data\\ai\\scenarios.meta");
                TypeGroups = LoadTypeGroups(gfc, "common:\\data\\ai\\scenarios.meta");
                PropSets = LoadModelSets(gfc, "common:\\data\\ai\\propsets.meta");
                PedModelSets = LoadModelSets(gfc, "common:\\data\\ai\\ambientpedmodelsets.meta");
                VehicleModelSets = LoadModelSets(gfc, "common:\\data\\ai\\vehiclemodelsets.meta");
                AnimGroups = LoadAnimGroups(gfc, "common:\\data\\ai\\conditionalanims.meta");

                TypeRefs = new Dictionary<uint, ScenarioTypeRef>();
                foreach (var kvp in Types)
                {
                    TypeRefs[kvp.Key] = new ScenarioTypeRef(kvp.Value);
                }
                foreach (var kvp in TypeGroups)
                {
                    TypeRefs[kvp.Key] = new ScenarioTypeRef(kvp.Value);
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

            if ((xml == null) || (xml.DocumentElement == null))
            {
                return types;
            }

            var typesxml = xml.DocumentElement;
            var items = typesxml.SelectNodes("Scenarios/Item");

            foreach (XmlNode item in items)
            {
                var typestr = Xml.GetStringAttribute(item, "type");
                ScenarioType typeobj = null;
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

                if (typeobj != null)
                {
                    typeobj.Load(item);
                    if (!string.IsNullOrEmpty(typeobj.NameLower))
                    {
                        JenkIndex.Ensure(typeobj.NameLower);
                        uint hash = JenkHash.GenHash(typeobj.NameLower);
                        types[hash] = typeobj;
                    }
                    else
                    { }
                }

            }

            JenkIndex.Ensure("none");

            return types;
        }

        private Dictionary<uint, ScenarioTypeGroup> LoadTypeGroups(GameFileCache gfc, string filename)
        {
            Dictionary<uint, ScenarioTypeGroup> types = new Dictionary<uint, ScenarioTypeGroup>();

            var xml = LoadXml(gfc, filename);

            if ((xml == null) || (xml.DocumentElement == null))
            {
                return types;
            }

            var typesxml = xml.DocumentElement;
            var items = typesxml.SelectNodes("ScenarioTypeGroups/Item");

            foreach (XmlNode item in items)
            {
                ScenarioTypeGroup group = new ScenarioTypeGroup();

                group.Load(item);
                if (!string.IsNullOrEmpty(group.NameLower))
                {
                    JenkIndex.Ensure(group.NameLower);
                    uint hash = JenkHash.GenHash(group.NameLower);
                    types[hash] = group;
                }
                else
                { }
            }

            JenkIndex.Ensure("none");

            return types;
        }

        private Dictionary<uint, AmbientModelSet> LoadModelSets(GameFileCache gfc, string filename)
        {
            Dictionary<uint, AmbientModelSet> sets = new Dictionary<uint, AmbientModelSet>();

            var xml = LoadXml(gfc, filename);

            if ((xml == null) || (xml.DocumentElement == null))
            {
                return sets;
            }

            var setsxml = xml.DocumentElement;
            var items = setsxml.SelectNodes("ModelSets/Item");


            var noneset = new AmbientModelSet();
            noneset.Name = "NONE";
            noneset.NameLower = "none";
            noneset.NameHash = JenkHash.GenHash("none");
            sets[noneset.NameHash] = noneset;


            foreach (XmlNode item in items)
            {
                AmbientModelSet set = new AmbientModelSet();
                set.Load(item);
                if (!string.IsNullOrEmpty(set.NameLower))
                {
                    JenkIndex.Ensure(set.NameLower);
                    uint hash = JenkHash.GenHash(set.NameLower);
                    sets[hash] = set;
                }
            }

            return sets;
        }

        private Dictionary<uint, ConditionalAnimsGroup> LoadAnimGroups(GameFileCache gfc, string filename)
        {
            Dictionary<uint, ConditionalAnimsGroup> groups = new Dictionary<uint, ConditionalAnimsGroup>();

            var xml = LoadXml(gfc, filename);

            if ((xml == null) || (xml.DocumentElement == null))
            {
                return groups;
            }

            var setsxml = xml.DocumentElement;
            var items = setsxml.SelectNodes("ConditionalAnimsGroup/Item");

            foreach (XmlNode item in items)
            {
                ConditionalAnimsGroup group = new ConditionalAnimsGroup();
                group.Load(item);
                if (!string.IsNullOrEmpty(group.NameLower))
                {
                    JenkIndex.Ensure(group.Name);
                    JenkIndex.Ensure(group.NameLower);
                    uint hash = JenkHash.GenHash(group.NameLower);
                    groups[hash] = group;
                }
            }

            return groups;
        }




        public ScenarioTypeRef GetScenarioTypeRef(uint hash)
        {
            lock (SyncRoot)
            {
                if (TypeRefs == null) return null;
                ScenarioTypeRef st;
                TypeRefs.TryGetValue(hash, out st);
                return st;
            }
        }
        public ScenarioType GetScenarioType(uint hash)
        {
            lock (SyncRoot)
            {
                if (Types == null) return null;
                ScenarioType st;
                Types.TryGetValue(hash, out st);
                return st;
            }
        }
        public ScenarioTypeGroup GetScenarioTypeGroup(uint hash)
        {
            lock (SyncRoot)
            {
                if (TypeGroups == null) return null;
                ScenarioTypeGroup tg;
                TypeGroups.TryGetValue(hash, out tg);
                return tg;
            }
        }
        public AmbientModelSet GetPropSet(uint hash)
        {
            lock (SyncRoot)
            {
                if (PropSets == null) return null;
                AmbientModelSet ms;
                PropSets.TryGetValue(hash, out ms);
                return ms;
            }
        }
        public AmbientModelSet GetPedModelSet(uint hash)
        {
            lock (SyncRoot)
            {
                if (PedModelSets == null) return null;
                AmbientModelSet ms;
                if(!PedModelSets.TryGetValue(hash, out ms))
                {
                    string s_hash = hash.ToString("X");
                    ms = new AmbientModelSet();
                    ms.Name = $"UNKNOWN PED MODELSET ({s_hash})";
                    ms.NameLower = ms.Name.ToLowerInvariant();
                    ms.NameHash = new MetaHash(hash);
                    ms.Models = new AmbientModel[] { };
                    PedModelSets.Add(hash, ms);
                }
                return ms;
            }
        }
        public AmbientModelSet GetVehicleModelSet(uint hash)
        {
            lock (SyncRoot)
            {
                if (VehicleModelSets == null) return null;
                AmbientModelSet ms;
                if(!VehicleModelSets.TryGetValue(hash, out ms))
                {
                    string s_hash = hash.ToString("X");
                    ms = new AmbientModelSet();
                    ms.Name = $"UNKNOWN VEHICLE MODELSET ({s_hash})";
                    ms.NameLower = ms.Name.ToLowerInvariant();
                    ms.NameHash = new MetaHash(hash);
                    ms.Models = new AmbientModel[] {};
                    VehicleModelSets.Add(hash, ms);
                }
                return ms;
            }
        }
        public ConditionalAnimsGroup GetAnimGroup(uint hash)
        {
            lock (SyncRoot)
            {
                if (AnimGroups == null) return null;
                ConditionalAnimsGroup ag;
                AnimGroups.TryGetValue(hash, out ag);
                return ag;
            }
        }

        public ScenarioTypeRef[] GetScenarioTypeRefs()
        {
            lock (SyncRoot)
            {
                if (TypeRefs == null) return null;
                return TypeRefs.Values.ToArray();
            }
        }
        public ScenarioType[] GetScenarioTypes()
        {
            lock (SyncRoot)
            {
                if (Types == null) return null;
                return Types.Values.ToArray();
            }
        }
        public ScenarioTypeGroup[] GetScenarioTypeGroups()
        {
            lock (SyncRoot)
            {
                if (TypeGroups == null) return null;
                return TypeGroups.Values.ToArray();
            }
        }
        public AmbientModelSet[] GetPropSets()
        {
            lock (SyncRoot)
            {
                if (PropSets == null) return null;
                return PropSets.Values.ToArray();
            }
        }
        public AmbientModelSet[] GetPedModelSets()
        {
            lock (SyncRoot)
            {
                if (PedModelSets == null) return null;
                return PedModelSets.Values.ToArray();
            }
        }
        public AmbientModelSet[] GetVehicleModelSets()
        {
            lock (SyncRoot)
            {
                if (VehicleModelSets == null) return null;
                return VehicleModelSets.Values.ToArray();
            }
        }
        public ConditionalAnimsGroup[] GetAnimGroups()
        {
            lock (SyncRoot)
            {
                if (AnimGroups == null) return null;
                return AnimGroups.Values.ToArray();
            }
        }

    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioTypeRef
    {
        /// <summary>
        /// Represents a scenario type that may either be a <see cref="ScenarioType"/> or a <see cref="ScenarioTypeGroup"/>.
        /// Used with CScenarioChainingNode and CScenarioPoint.
        /// </summary>
        
        public string Name => IsGroup ? Group.Name : Type.Name;
        public string NameLower => IsGroup ? Group.NameLower : Type.NameLower;
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

    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioType
    {
        public string OuterXml { get; set; }
        public string Name { get; set; }
        public string NameLower { get; set; }
        public MetaHash NameHash { get; set; }
        public bool IsVehicle { get; set; }
        public string VehicleModelSet { get; set; }
        public MetaHash VehicleModelSetHash { get; set; }


        public virtual void Load(XmlNode node)
        {
            OuterXml = node.OuterXml;
            Name = Xml.GetChildInnerText(node, "Name");
            NameLower = Name.ToLowerInvariant();
            NameHash = JenkHash.GenHash(NameLower);


            if (IsVehicle)
            {
                VehicleModelSet = Xml.GetChildStringAttribute(node, "VehicleModelSet", "ref");
                if (!string.IsNullOrEmpty(VehicleModelSet) && (VehicleModelSet != "NULL"))
                {
                    VehicleModelSetHash = JenkHash.GenHash(VehicleModelSet.ToLowerInvariant());
                }
            }
        }

        public override string ToString()
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


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ScenarioTypeGroup
    {
        public string OuterXml { get; set; }
        public string Name { get; set; }
        public string NameLower { get; set; }
        public MetaHash NameHash { get; set; }


        public void Load(XmlNode node)
        {
            OuterXml = node.OuterXml;
            Name = Xml.GetChildInnerText(node, "Name");
            NameLower = Name.ToLowerInvariant();
            NameHash = JenkHash.GenHash(NameLower);
        }

        public override string ToString()
        {
            return Name;
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class AmbientModelSet
    {
        public string Name { get; set; }
        public string NameLower { get; set; }
        public MetaHash NameHash { get; set; }
        public AmbientModel[] Models { get; set; }


        public void Load(XmlNode node)
        {
            Name = Xml.GetChildInnerText(node, "Name");
            NameLower = Name.ToLowerInvariant();
            NameHash = JenkHash.GenHash(NameLower);

            var models = node.SelectNodes("Models/Item");
            var modellist = new List<AmbientModel>();
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


    [TypeConverter(typeof(ExpandableObjectConverter))] public class ConditionalAnimsGroup
    {
        public string OuterXml { get; set; }
        public string Name { get; set; }
        public string NameLower { get; set; }


        public void Load(XmlNode node)
        {
            OuterXml = node.OuterXml;
            Name = Xml.GetChildInnerText(node, "Name");
            NameLower = Name.ToLowerInvariant();
        }

        public override string ToString()
        {
            return Name;
        }
    }

}
