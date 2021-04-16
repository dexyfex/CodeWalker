using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SharpDX;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvFile : GameFile, PackedFile, BasePathData
    {
        public NavMesh Nav { get; set; }

        public List<Vector3> Vertices { get; set; }
        public List<ushort> Indices { get; set; }
        public List<YnvEdge> Edges { get; set; }
        public List<YnvPoly> Polys { get; set; }
        public List<YnvPortal> Portals { get; set; }
        public List<YnvPoint> Points { get; set; }


        public EditorVertex[] PathVertices { get; set; }
        public EditorVertex[] TriangleVerts { get; set; }
        public Vector4[] NodePositions { get; set; }


        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;

        public bool BuildStructsOnSave { get; set; } = true;


        public PathBVH BVH { get; set; }


        public int AreaID
        {
            get
            {
                return (int)(Nav?.AreaID ?? 0);
            }
            set
            {
                if (Nav != null) Nav.AreaID = (uint)value;
            }
        }
        public int CellX { get { return AreaID % 100; } set { AreaID = (CellY * 100) + value; } }
        public int CellY { get { return AreaID / 100; } set { AreaID = (value * 100) + CellX; } }



        //getters for property grids viewing of the lists
        public Vector3[] AllVertices { get { return Vertices?.ToArray(); } }
        public ushort[] AllIndices { get { return Indices?.ToArray(); } }
        public YnvEdge[] AllEdges { get { return Edges?.ToArray(); } }
        public YnvPoly[] AllPolys { get { return Polys?.ToArray(); } }
        public YnvPortal[] AllPortals { get { return Portals?.ToArray(); } }
        public YnvPoint[] AllPoints { get { return Points?.ToArray(); } }




        public YnvFile() : base(null, GameFileType.Ynv)
        {
        }
        public YnvFile(RpfFileEntry entry) : base(entry, GameFileType.Ynv)
        {
        }

        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ynv file (openIV-compatible format)

            RpfFile.LoadResourceFile(this, data, 2);

            Loaded = true;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;

            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);


            Nav = rd.ReadBlock<NavMesh>();


            InitFromNav();

            UpdateAllNodePositions();

            UpdateTriangleVertices();

            BuildBVH();


            Loaded = true;
            LoadQueued = true;
        }

        public void InitFromNav()
        {
            if (Nav == null) return;

            Vector3 posoffset = Nav.SectorTree?.AABBMin.XYZ() ?? Vector3.Zero;
            Vector3 aabbsize = Nav.AABBSize;

            if (Nav.Vertices != null)
            {
                var verts = Nav.Vertices.GetFullList();
                Vertices = new List<Vector3>(verts.Count);
                for (int i = 0; i < verts.Count; i++)
                {
                    var ov = verts[i].ToVector3();
                    Vertices.Add(posoffset + ov * aabbsize);
                }
            }
            if (Nav.Indices != null)
            {
                Indices = Nav.Indices.GetFullList();
            }
            if (Nav.Edges != null)
            {
                var edges = Nav.Edges.GetFullList();
                Edges = new List<YnvEdge>(edges.Count);
                for (int i = 0; i < edges.Count; i++)
                {
                    YnvEdge edge = new YnvEdge();
                    edge.Init(this, edges[i]);
                    Edges.Add(edge);
                }
            }
            if (Nav.Polys != null)
            {
                var polys = Nav.Polys.GetFullList();
                Polys = new List<YnvPoly>(polys.Count);
                for (int i = 0; i < polys.Count; i++)
                {
                    YnvPoly poly = new YnvPoly();
                    poly.Init(this, polys[i]);
                    poly.Index = i;
                    Polys.Add(poly);
                }
            }
            if (Nav.Portals != null)
            {
                var portals = Nav.Portals;
                Portals = new List<YnvPortal>(portals.Length);
                for (int i = 0; i < portals.Length; i++)
                {
                    YnvPortal portal = new YnvPortal();
                    portal.Init(this, portals[i]);
                    portal.Index = i;
                    portal.PositionFrom = posoffset + portal._RawData.PositionFrom.ToVector3() * aabbsize;
                    portal.PositionTo = posoffset + portal._RawData.PositionTo.ToVector3() * aabbsize;
                    Portals.Add(portal);
                }
            }


            ////### add points to the list and calculate positions...
            var treestack = new Stack<NavMeshSector>();
            var pointi = 0;
            if (Nav.SectorTree != null)
            {
                treestack.Push(Nav.SectorTree);
            }
            while (treestack.Count > 0)
            {
                var sector = treestack.Pop();
                if (sector.Data != null)
                {
                    var points = sector.Data.Points;
                    if (points != null)
                    {
                        if (Points == null)
                        {
                            Points = new List<YnvPoint>();
                        }
                        for (int i = 0; i < points.Length; i++)
                        {
                            YnvPoint point = new YnvPoint();
                            point.Init(this, points[i]);
                            point.Index = pointi; pointi++;
                            point.Position = posoffset + point._RawData.Position * aabbsize;
                            Points.Add(point);
                        }
                    }
                }
                if (sector.SubTree1 != null) treestack.Push(sector.SubTree1);
                if (sector.SubTree2 != null) treestack.Push(sector.SubTree2);
                if (sector.SubTree3 != null) treestack.Push(sector.SubTree3);
                if (sector.SubTree4 != null) treestack.Push(sector.SubTree4);
            }

        }




        public byte[] Save()
        {
            if (BuildStructsOnSave)
            {
                BuildStructs();
            }

            byte[] data = ResourceBuilder.Build(Nav, 2); //ynv is version 2...

            return data;
        }


        private void BuildStructs()
        {
            Vector3 posoffset = Nav.SectorTree?.AABBMin.XYZ() ?? Vector3.Zero;
            Vector3 aabbsize = Nav.AABBSize;
            Vector3 aabbsizeinv = 1.0f / aabbsize;

            var vertlist = new List<NavMeshVertex>();
            var indslist = new List<ushort>();
            var edgelist = new List<NavMeshEdge>();
            var polylist = new List<NavMeshPoly>();
            var portallist = new List<NavMeshPortal>();
            var portallinks = new List<ushort>();

            var vertdict = new Dictionary<Vector3, ushort>();
            var areadict = new Dictionary<uint, uint>();
            var arealist = new List<uint>();
            var areaid = Nav.AreaID;
            EnsureEdgeAreaID(areaid, areadict, arealist);
            EnsureEdgeAreaID(0x3FFF, areadict, arealist);
            EnsureEdgeAreaID(areaid - 100, areadict, arealist);
            EnsureEdgeAreaID(areaid - 1, areadict, arealist);
            EnsureEdgeAreaID(areaid + 1, areadict, arealist);
            EnsureEdgeAreaID(areaid + 100, areadict, arealist);



            if (Polys != null) //rebuild vertices, indices, edges and polys lists from poly data.
            {
                for (int i = 0; i < Polys.Count; i++)
                {
                    var poly = Polys[i];
                    var vc = poly.Vertices?.Length ?? 0;
                    //poly.AreaID = (ushort)Nav.AreaID;
                    poly._RawData.IndexID = (ushort)indslist.Count;
                    for (int n = 0; n < vc; n++)
                    {
                        Vector3 v = poly.Vertices[n];
                        YnvEdge e = ((poly.Edges != null) && (n < poly.Edges.Length)) ? poly.Edges[n] : null;
                        ushort ind;
                        if (!vertdict.TryGetValue(v, out ind))
                        {
                            ind = (ushort)vertlist.Count;
                            vertdict[v] = ind;
                            vertlist.Add(NavMeshVertex.Create(Vector3.Clamp((v - posoffset) * aabbsizeinv, Vector3.Zero, Vector3.One)));
                        }
                        if ((poly.Indices != null) && (n < poly.Indices.Length))
                        {
                            poly.Indices[n] = ind;
                        }
                        indslist.Add(ind);

                        NavMeshEdge edge;
                        if (e != null)
                        {
                            if (e.Poly1 != null)
                            {
                                e.PolyID1 = (uint)e.Poly1.Index;
                                e.AreaID1 = e.Poly1.AreaID;
                                if (e.AreaID1 == 0x3FFF)
                                { }//debug
                            }
                            if (e.Poly2 != null)
                            {
                                e.PolyID2 = (uint)e.Poly2.Index;
                                e.AreaID2 = e.Poly2.AreaID;
                                if (e.AreaID2 == 0x3FFF)
                                { }//debug
                            }
                            if ((e.AreaID1 == 0) || (e.AreaID2 == 0))
                            { }//debug
                            e._RawData._Poly1.AreaIDInd = EnsureEdgeAreaID(e.AreaID1, areadict, arealist);
                            e._RawData._Poly2.AreaIDInd = EnsureEdgeAreaID(e.AreaID2, areadict, arealist);
                            edge = e.RawData;
                        }
                        else
                        {
                            var areaind = EnsureEdgeAreaID(0x3FFF, areadict, arealist);
                            edge = new NavMeshEdge();//create an empty edge
                            edge._Poly1.PolyID = 0x3FFF;
                            edge._Poly2.PolyID = 0x3FFF;
                            edge._Poly1.AreaIDInd = areaind;
                            edge._Poly2.AreaIDInd = areaind;
                        }
                        edgelist.Add(edge);
                    }
                    poly._RawData.IndexCount = vc;
                    poly._RawData.PortalLinkID = (uint)portallinks.Count;//these shouldn't be directly editable!
                    poly._RawData.PortalLinkCount = (byte)(poly.PortalLinks?.Length ?? 0);
                    if (poly.PortalLinks != null)
                    {
                        portallinks.AddRange(poly.PortalLinks);
                    }
                    poly.Index = i;//this should be redundant...
                    poly.CalculateAABB();//make sure this is up to date!
                    polylist.Add(poly.RawData);
                }
            }

            if (Portals != null)
            {
                for (int i = 0; i < Portals.Count; i++)
                {
                    var portal = Portals[i];
                    var pdata = portal.RawData;
                    pdata.PositionFrom = NavMeshVertex.Create((portal.PositionFrom - posoffset) * aabbsizeinv);
                    pdata.PositionTo = NavMeshVertex.Create((portal.PositionTo - posoffset) * aabbsizeinv);
                    portallist.Add(pdata);
                }
            }

            if (Points != null) //points will be built into the sector tree
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    var point = Points[i];
                    var pdata = point.RawData;
                    pdata.Position = (point.Position - posoffset) * aabbsizeinv;
                    point.RawData = pdata;
                }
            }


            if (Nav.Vertices == null)
            {
                Nav.Vertices = new NavMeshList<NavMeshVertex>();
                Nav.Vertices.VFT = 1080158456;
            }
            if (Nav.Indices == null)
            {
                Nav.Indices = new NavMeshList<ushort>();
                Nav.Indices.VFT = 1080158424;
            }
            if (Nav.Edges == null)
            {
                Nav.Edges = new NavMeshList<NavMeshEdge>();
                Nav.Edges.VFT = 1080158440;
            }
            if (Nav.Polys == null)
            {
                Nav.Polys = new NavMeshList<NavMeshPoly>();
                Nav.Polys.VFT = 1080158408;
            }


            Nav.Vertices.RebuildList(vertlist);
            Nav.VerticesCount = Nav.Vertices.ItemCount;

            Nav.Indices.RebuildList(indslist);

            Nav.Edges.RebuildList(edgelist);
            Nav.EdgesIndicesCount = Nav.Indices.ItemCount;

            Nav.Polys.RebuildList(polylist);
            Nav.PolysCount = Nav.Polys.ItemCount;

            Nav.Portals = (portallist.Count > 0) ? portallist.ToArray() : null;
            Nav.PortalsCount = (uint)(Nav.Portals?.Length ?? 0);
            Nav.PortalLinks = (portallinks.Count > 0) ? portallinks.ToArray() : null;
            Nav.PortalLinksCount = (uint)(Nav.PortalLinks?.Length ?? 0);

            var adjAreaIds = new NavMeshUintArray();
            adjAreaIds.Set(arealist.ToArray());
            Nav.AdjAreaIDs = adjAreaIds;


            for (int i = 0; i < Nav.Polys.ListParts.Count; i++) //reassign part id's on all the polys...
            {
                var listpart = Nav.Polys.ListParts[i];
                var partitems = listpart?.Items;
                if (partitems == null) continue;
                ushort iu = (ushort)i;
                for (int j = 0; j < partitems.Length; j++)
                {
                    partitems[j].PartID = iu;
                }
            }



            //Build Sector Tree
            int depth = 0;
            if ((Nav.ContentFlags & NavMeshFlags.Vehicle) == 0) depth = 2;
            //vehicle navmesh has a single level, static has 3..

            NavMeshSector orig = Nav.SectorTree;
            NavMeshSector root = new NavMeshSector();
            root.SetAABBs(orig.AABBMin.XYZ(), orig.AABBMax.XYZ());

            uint pointindex = 0;
            var pointflags = new bool[Points?.Count ?? 0];

            BuildSectorTree(root, depth, ref pointindex, pointflags);

            Nav.SectorTree = root;

        }

        private uint EnsureEdgeAreaID(uint areaid, Dictionary<uint, uint> areadict, List<uint> arealist)
        {
            uint ind;
            if (!areadict.TryGetValue(areaid, out ind))
            {
                ind = (uint)arealist.Count;
                areadict[areaid] = ind;
                arealist.Add(areaid);
            }
            return ind;
        }


        private void BuildSectorTree(NavMeshSector node, int depth, ref uint pointindex, bool[] pointflags)
        {
            Vector3 min = node.AABBMin.XYZ();
            Vector3 max = node.AABBMax.XYZ();
            Vector3 cen = (min + max) * 0.5f;


            if (depth <= 0)
            {
                //go through polys and points and create new lists for this node
                NavMeshSectorData data = new NavMeshSectorData();
                node.Data = data;

                data.PointsStartID = pointindex;


                if (Polys != null)
                {
                    List<ushort> polyids = new List<ushort>();
                    for (int i = 0; i < Polys.Count; i++)
                    {
                        var poly = Polys[i];
                        var b = poly._RawData.CellAABB;
                        if (BoxOverlaps(b, node.CellAABB))
                        {
                            polyids.Add((ushort)poly.Index);
                        }
                    }
                    if (polyids.Count > 0)
                    {
                        data.PolyIDs = polyids.ToArray();
                    }
                }

                if (Points != null)
                {
                    List<NavMeshPoint> points = new List<NavMeshPoint>();
                    for (int i = 0; i < Points.Count; i++)
                    {
                        var point = Points[i];
                        if (IsInBox(point.Position, min, max, true) && (pointflags[i] == false))
                        {
                            points.Add(point.RawData);
                            pointflags[i] = true;
                        }
                    }
                    if (points.Count > 0)
                    {
                        data.Points = points.ToArray();
                        data.PointsCount = (ushort)points.Count;
                        pointindex += (uint)points.Count;
                    }
                }

            }
            else
            {
                //recurse quadtree... clockwise from +XY (top right)
                int cdepth = depth - 1;
                node.SubTree1 = new NavMeshSector();
                node.SubTree2 = new NavMeshSector();
                node.SubTree3 = new NavMeshSector();
                node.SubTree4 = new NavMeshSector();
                node.SubTree1.SetAABBs(new Vector3(cen.X, cen.Y, cen.Z), new Vector3(max.X, max.Y, max.Z)); //for some reason Z values seem to get arranged like this...
                node.SubTree2.SetAABBs(new Vector3(cen.X, min.Y, 0.0f), new Vector3(max.X, cen.Y, 0.0f));
                node.SubTree3.SetAABBs(new Vector3(min.X, min.Y, min.Z), new Vector3(cen.X, cen.Y, cen.Z));
                node.SubTree4.SetAABBs(new Vector3(min.X, cen.Y, 0.0f), new Vector3(cen.X, max.Y, 0.0f));
                BuildSectorTree(node.SubTree1, cdepth, ref pointindex, pointflags);
                BuildSectorTree(node.SubTree2, cdepth, ref pointindex, pointflags);
                BuildSectorTree(node.SubTree3, cdepth, ref pointindex, pointflags);
                BuildSectorTree(node.SubTree4, cdepth, ref pointindex, pointflags);
            }
        }






        public void WriteXml(StringBuilder sb, int indent)
        {
            YnvXml.StringTag(sb, indent, "ContentFlags", Nav.ContentFlags.ToString());
            YnvXml.ValueTag(sb, indent, "AreaID", AreaID.ToString());
            YnvXml.SelfClosingTag(sb, indent, "BBMin " + FloatUtil.GetVector3XmlString(Nav.AABBMin));
            YnvXml.SelfClosingTag(sb, indent, "BBMax " + FloatUtil.GetVector3XmlString(Nav.AABBMax));
            YnvXml.SelfClosingTag(sb, indent, "BBSize " + FloatUtil.GetVector3XmlString(Nav.AABBSize));
            YnvXml.WriteItemArray(sb, AllPolys, indent, "Polygons");
            YnvXml.WriteItemArray(sb, AllPortals, indent, "Portals");
            YnvXml.WriteItemArray(sb, AllPoints, indent, "Points");
        }
        public void ReadXml(XmlNode node)
        {
            Nav = new NavMesh();
            Nav.SectorTree = new NavMeshSector();
            Nav.ContentFlags = Xml.GetChildEnumInnerText<NavMeshFlags>(node, "ContentFlags");
            Nav.AreaID = Xml.GetChildUIntAttribute(node, "AreaID");
            Nav.AABBMin = Xml.GetChildVector3Attributes(node, "BBMin");
            Nav.AABBMax = Xml.GetChildVector3Attributes(node, "BBMax");
            Nav.AABBSize = Xml.GetChildVector3Attributes(node, "BBSize");
            Polys = XmlYnv.ReadItemList<YnvPoly>(node, "Polygons");
            Portals = XmlYnv.ReadItemList<YnvPortal>(node, "Portals");
            Points = XmlYnv.ReadItemList<YnvPoint>(node, "Points");

            if (Polys != null)
            {
                for (int i = 0; i < Polys.Count; i++)
                {
                    var poly = Polys[i];
                    poly.Ynv = this;
                    poly.Index = i;
                    poly.AreaID = (ushort)AreaID;
                }
            }
            if (Portals != null)
            {
                for (int i = 0; i < Portals.Count; i++)
                {
                    var portal = Portals[i];
                    portal.Ynv = this;
                    portal.Index = i;
                    portal.AreaIDFrom = (ushort)AreaID;
                    portal.AreaIDTo = (ushort)AreaID;
                }
            }
            if (Points != null)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    var point = Points[i];
                    point.Ynv = this;
                    point.Index = i;
                }
            }


            bool vehicle = ((Nav.ContentFlags & NavMeshFlags.Vehicle) != 0);
            Nav.VersionUnk1 = 0x00010011;
            Nav.VersionUnk2 = vehicle ? 0 : 0x85CB3561;
            Nav.Transform = Matrix.Identity;


        }






        private bool IsInBox(Vector3 p, Vector3 min, Vector3 max, bool outer)
        {
            if (outer)
                return (p.X >= min.X) && (p.X <= max.X) &&
                       (p.Y >= min.Y) && (p.Y <= max.Y);// && 
                        //(p.Z >= min.Z) && (p.Z < max.Z);
            else
                return (p.X >= min.X) && (p.X < max.X) &&
                       (p.Y >= min.Y) && (p.Y < max.Y);// && 
                        //(p.Z >= min.Z) && (p.Z < max.Z);
        }
        private bool BoxOverlaps(Vector3 bmin, Vector3 bmax, Vector3 min, Vector3 max)
        {
            return (bmax.X >= min.X) && (bmin.X <= max.X) &&
                   (bmax.Y >= min.Y) && (bmin.Y <= max.Y);// && 
                   //(bmax.Z >= min.Z) && (bmin.Z <= max.Z);
        }
        private bool BoxOverlaps(NavMeshAABB a, NavMeshAABB b)
        {
            return (a.MaxX >= b.MinX) && (a.MinX <= b.MaxX) &&
                   (a.MaxY >= b.MinY) && (a.MinY <= b.MaxY);
        }


        public bool RemovePoly(YnvPoly poly)
        {
            return false;
        }
        public bool RemovePoint(YnvPoint point)
        {
            return false;
        }
        public bool RemovePortal(YnvPortal portal)
        {
            return false;
        }




        public void UpdateAllNodePositions()
        {
            if (Nav == null) return;


            Vector3 posoffset = Nav.SectorTree.AABBMin.XYZ();
            Vector3 aabbsize = Nav.AABBSize;

            EditorVertex v = new EditorVertex();
            v.Colour = 0xFF0000FF;
            var lv = new List<EditorVertex>();
            var nv = new List<Vector4>();


            ////### add portal positions to the node list, also add links to the link vertex array
            int cnt = Portals?.Count ?? 0;
            if (cnt > 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    var portal = Portals[i];
                    nv.Add(new Vector4(portal.PositionFrom, 1.0f));
                    v.Position = portal.PositionFrom; lv.Add(v);
                    v.Position = portal.PositionTo; lv.Add(v);
                }
            }


            ////### add point positions to the node list
            cnt = Points?.Count ?? 0;
            if (cnt >= 0)
            {
                for (int i = 0; i < cnt; i++)
                {
                    var point = Points[i];
                    nv.Add(new Vector4(point.Position, 1.0f));
                }
            }


            NodePositions = (nv.Count > 0) ? nv.ToArray() : null;
            PathVertices = (lv.Count > 0) ? lv.ToArray() : null;


        }

        public void UpdateTriangleVertices()
        {
            //need position and colour for each vertex.
            //render as a triangle list... (no indices needed)

            //go through the nav mesh polys and generate verts to render...

            if ((Polys == null) || (Polys.Count == 0)) return;


            int vc = Vertices.Count;

            List<EditorVertex> rverts = new List<EditorVertex>();
            EditorVertex p0 = new EditorVertex();
            EditorVertex p1 = new EditorVertex();
            EditorVertex p2 = new EditorVertex();
            foreach (var ypoly in Polys)
            {
                if ((ypoly.Vertices == null) || (ypoly.Vertices.Length < 3))
                { continue; }

                var colour = ypoly.GetColour();
                var colourval = (uint)colour.ToRgba();

                p0.Colour = colourval;
                p1.Colour = colourval;
                p2.Colour = colourval;

                p0.Position = ypoly.Vertices[0];

                //build triangles for the poly.
                int tricount = ypoly.Vertices.Length - 2;
                for (int t = 0; t < tricount; t++)
                {
                    p1.Position = ypoly.Vertices[t + 1];
                    p2.Position = ypoly.Vertices[t + 2];
                    rverts.Add(p0);
                    rverts.Add(p1);
                    rverts.Add(p2);
                }
            }

            TriangleVerts = rverts.ToArray();

        }



        public void UpdateContentFlags(bool vehicle)
        {
            NavMeshFlags f = NavMeshFlags.None;
            if (Polys?.Count > 0) f = f | NavMeshFlags.Polygons;
            if (Portals?.Count > 0) f = f | NavMeshFlags.Portals;
            if (vehicle) f = f | NavMeshFlags.Vehicle;
            else f = f | NavMeshFlags.Unknown8; //what exactly is this?
            Nav.ContentFlags = f;
        }




        public void BuildBVH()
        {
            var nodes = new List<BasePathNode>();
            if (Portals != null) nodes.AddRange(Portals);
            if (Points != null) nodes.AddRange(Points);
            BVH = new PathBVH(nodes, 10, 10);
        }



        public EditorVertex[] GetPathVertices()
        {
            return PathVertices;
        }
        public EditorVertex[] GetTriangleVertices()
        {
            return TriangleVerts;
        }
        public Vector4[] GetNodePositions()
        {
            return NodePositions;
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvPoly : IMetaXmlItem
    {
        public NavMeshPoly _RawData;
        public NavMeshPoly RawData { get { return _RawData; } set { _RawData = value; } }

        public YnvFile Ynv { get; set; }

        public ushort AreaID { get { return _RawData.AreaID; } set { _RawData.AreaID = value; } }
        public ushort PartID { get { return _RawData.PartID; } set { _RawData.PartID = value; } }
        public uint PortalLinkID { get { return _RawData.PortalLinkID; } set { _RawData.PortalLinkID = value; } }
        public byte PortalLinkCount { get { return _RawData.PortalLinkCount; } set { _RawData.PortalLinkCount = value; } }
        public byte Flags1 { get { return _RawData.Flags1; } set { _RawData.Flags1 = value; } }
        public byte Flags2 { get { return _RawData.Flags2; } set { _RawData.Flags2 = value; } }
        public byte Flags3 { get { return _RawData.Flags3; } set { _RawData.Flags3 = value; } }
        public byte Flags4 { get { return _RawData.Flags4; } set { _RawData.Flags4 = value; } }
        public bool B00_AvoidUnk        { get { return (_RawData.PolyFlags0 & 1) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 0, value); } }
        public bool B01_AvoidUnk        { get { return (_RawData.PolyFlags0 & 2) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 1, value); } }
        public bool B02_IsFootpath      { get { return (_RawData.PolyFlags0 & 4) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 2, value); } }
        public bool B03_IsUnderground   { get { return (_RawData.PolyFlags0 & 8) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 3, value); } }
        public bool B04_Unused          { get { return (_RawData.PolyFlags0 & 16) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 4, value); } }
        public bool B05_Unused          { get { return (_RawData.PolyFlags0 & 32) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 5, value); } }
        public bool B06_SteepSlope      { get { return (_RawData.PolyFlags0 & 64) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 6, value); } }
        public bool B07_IsWater         { get { return (_RawData.PolyFlags0 & 128) > 0; } set { _RawData.PolyFlags0 = (ushort)BitUtil.UpdateBit(_RawData.PolyFlags0, 7, value); } }
        public bool B08_UndergroundUnk0 { get { return (_RawData.PolyFlags1 & 1) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 0, value); } }
        public bool B09_UndergroundUnk1 { get { return (_RawData.PolyFlags1 & 2) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 1, value); } }
        public bool B10_UndergroundUnk2 { get { return (_RawData.PolyFlags1 & 4) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 2, value); } }
        public bool B11_UndergroundUnk3 { get { return (_RawData.PolyFlags1 & 8) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 3, value); } }
        public bool B12_Unused          { get { return (_RawData.PolyFlags1 & 16) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 4, value); } }
        public bool B13_HasPathNode     { get { return (_RawData.PolyFlags1 & 32) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 5, value); } }
        public bool B14_IsInterior      { get { return (_RawData.PolyFlags1 & 64) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 6, value); } }
        public bool B15_InteractionUnk  { get { return (_RawData.PolyFlags1 & 128) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 7, value); } }
        public bool B16_Unused          { get { return (_RawData.PolyFlags1 & 256) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 8, value); } }
        public bool B17_IsFlatGround    { get { return (_RawData.PolyFlags1 & 512) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 9, value); } }
        public bool B18_IsRoad          { get { return (_RawData.PolyFlags1 & 1024) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 10, value); } }
        public bool B19_IsCellEdge      { get { return (_RawData.PolyFlags1 & 2048) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 11, value); } }
        public bool B20_IsTrainTrack    { get { return (_RawData.PolyFlags1 & 4096) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 12, value); } }
        public bool B21_IsShallowWater  { get { return (_RawData.PolyFlags1 & 8192) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 13, value); } }
        public bool B22_FootpathUnk1    { get { return (_RawData.PolyFlags1 & 16384) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 14, value); } }
        public bool B23_FootpathUnk2    { get { return (_RawData.PolyFlags1 & 32768) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 15, value); } }
        public bool B24_FootpathMall    { get { return (_RawData.PolyFlags1 & 65536) > 0; } set { _RawData.PolyFlags1 = BitUtil.UpdateBit(_RawData.PolyFlags1, 16, value); } }
        public bool B25_SlopeSouth      { get { return (_RawData.PolyFlags2 & 65536) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 16, value); } }
        public bool B26_SlopeSouthEast  { get { return (_RawData.PolyFlags2 & 131072) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 17, value); } }
        public bool B27_SlopeEast       { get { return (_RawData.PolyFlags2 & 262144) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 18, value); } }
        public bool B28_SlopeNorthEast  { get { return (_RawData.PolyFlags2 & 524288) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 19, value); } }
        public bool B29_SlopeNorth      { get { return (_RawData.PolyFlags2 & 1048576) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 20, value); } }
        public bool B30_SlopeNorthWest  { get { return (_RawData.PolyFlags2 & 2097152) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 21, value); } }
        public bool B31_SlopeWest       { get { return (_RawData.PolyFlags2 & 4194304) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 22, value); } }
        public bool B32_SlopeSouthWest  { get { return (_RawData.PolyFlags2 & 8388608) > 0; } set { _RawData.PolyFlags2 = BitUtil.UpdateBit(_RawData.PolyFlags2, 23, value); } }
        public byte UnkX { get { return _RawData.UnkX; } set { _RawData.UnkX = value; } }
        public byte UnkY { get { return _RawData.UnkY; } set { _RawData.UnkY = value; } }


        public Vector3 Position { get; set; }
        public int Index { get; set; }

        public ushort[] Indices { get; set; }
        public Vector3[] Vertices { get; set; }
        public YnvEdge[] Edges { get; set; }
        public ushort[] PortalLinks { get; set; }


        public void Init(YnvFile ynv, NavMeshPoly poly)
        {
            Ynv = ynv;
            RawData = poly;

            LoadIndices();
            LoadPortalLinks();
            CalculatePosition(); //calc poly center for display purposes..
        }


        public void LoadIndices()
        {
            //load indices, vertices and edges
            var indices = Ynv.Indices;
            var vertices = Ynv.Vertices;
            var edges = Ynv.Edges;
            if ((indices == null) || (vertices == null) || (edges == null))
            { return; }
            var vc = vertices.Count;
            var ic = _RawData.IndexCount;
            var startid = _RawData.IndexID;
            var endid = startid + ic;
            if (startid >= indices.Count)
            { return; }
            if (endid > indices.Count)
            { return; }
            if (endid > edges.Count)
            { return; }

            Indices = new ushort[ic];
            Vertices = new Vector3[ic];
            Edges = new YnvEdge[ic];

            int i = 0;
            for (int id = startid; id < endid; id++)
            {
                var ind = indices[id];

                Indices[i] = ind;
                Vertices[i] = (ind < vc) ? vertices[ind] : Vector3.Zero;
                Edges[i] = edges[id];

                i++;
            }
        }

        public void LoadPortalLinks()
        {
            if (PortalLinkCount == 0)
            { return; }
            var links = Ynv.Nav?.PortalLinks;
            if (links == null)
            { return; }

            var ll = links.Length;

            PortalLinks = new ushort[PortalLinkCount];

            int offset = (int)PortalLinkID;
            for (int i = 0; i < PortalLinkCount; i++)
            {
                int idx = offset + i;
                PortalLinks[i] = (idx < ll) ? links[idx] : (ushort)0;
            }

            if (PortalLinkCount != 2)
            { }//debug

        }


        public void SetPosition(Vector3 pos)
        {
            Vector3 delta = pos - Position;
            Position = pos;
            //TODO: update vertex positions!!!
        }

        public Color4 GetColour()
        {
            var colour = new Color4();
            var u0 = _RawData.PolyFlags0;
            if ((u0 & 1) > 0) colour.Red += 0.01f;//avoid? loiter?
            if ((u0 & 2) > 0) colour.Red += 0.01f; //avoid?
            if ((u0 & 4) > 0) colour.Green += 0.25f; //ped/footpath
            if ((u0 & 8) > 0) colour.Green += 0.02f; //underground?
            ////if ((u0 & 16) > 0) colour.Red += 1.0f; //not used?
            ////if ((u0 & 32) > 0) colour.Green += 1.0f;//not used?
            if ((u0 & 64) > 0) colour.Red += 0.25f; //steep slope
            if ((u0 & 128) > 0) colour.Blue += 0.25f; //water
            //if (u0 >= 256) colour.Green += 1.0f;//other bits unused...

            var u2 = _RawData.PolyFlags1;
            //colour.Green = (u2 & 15) / 15.0f; //maybe underground amount..?
            //if ((u2 & 1) > 0) colour.Blue += 1.0f; //peds interact with something? underground?
            //if ((u2 & 2) > 0) colour.Green += 1.0f;//underneath something?
            //if ((u2 & 4) > 0) colour.Red += 0.5f;//peds interact with something..? underground?
            //if ((u2 & 8) > 0) colour.Red += 0.5f; //underground?
            //if ((u2 & 16) > 0) colour.Red += 1.0f; //not used..
            //if ((u2 & 32) > 0) colour.Green += 1.0f;//use path node?
            if ((u2 & 64) > 0) colour.Blue += 0.1f; //is interior?
            //if ((u2 & 128) > 0) colour.Red += 1.0f; //interacting areas? veg branches, roofs, vents, worker areas?
            //if ((u2 & 256) > 0) colour.Green += 1.0f; //not used?
            if ((u2 & 512) > 0) colour.Green += 0.1f;//is flat ground? ped-navigable?
            if ((u2 & 1024) > 0) colour.Blue += 0.03f;//is a road
            //if ((u2 & 2048) > 0) colour.Green += 1.0f; //poly is on a cell edge
            if ((u2 & 4096) > 0) colour.Green += 0.75f; //is a train track
            if ((u2 & 8192) > 0) colour.Blue += 0.75f;//shallow water/moving water
            if ((u2 & 16384) > 0) colour.Red += 0.2f; //footpaths/beach - peds walking?
            if ((u2 & 32768) > 0) colour.Blue += 0.2f; //footpaths - special?
            if ((u2 & 65536) > 0) colour.Green = 0.2f;//footpaths - mall areas? eg mall, vinewood blvd
            //if (u2 >= 131072) { }//other bits unused

            var u5 = _RawData.PolyFlags2; //32 bits
            //colour.Red = poly.Unknown_28h_8a / 255.0f; //heuristic vals..?
            //colour.Green = poly.Unknown_28h_8b / 255.0f; //heuristic vals..?
            //if ((u5 & 65536) > 0) colour.Red += 1.0f; //slope facing -Y       (south)
            //if ((u5 & 131072) > 0) colour.Blue += 1.0f; //slope facing +X,-Y   (southeast)
            //if ((u5 & 262144) > 0) colour.Green += 1.0f; //slope facing +X    (east)
            //if ((u5 & 524288) > 0) colour.Red += 1.0f; //slope facing +X,+Y   (northeast)
            //if ((u5 & 1048576) > 0) colour.Green += 1.0f; //slope facing +Y   (north)
            //if ((u5 & 2097152) > 0) colour.Blue += 1.0f; //slope facing -X,+Y  (northwest)
            //if ((u5 & 4194304) > 0) colour.Green += 1.0f; //slope facing -X    (west)
            //if ((u5 & 8388608) > 0) colour.Red += 1.0f; //slope facing -X,-Y   (southwest)
            //if (u5 >= 16777216) { } //other bits unused

            var u1 = _RawData.PortalLinkCount;
            //if ((u1 & 1) > 0) colour.Red += 1.0f; //portal - don't interact?
            //if ((u1 & 2) > 0) colour.Green += 1.0f; //portal - ladder/fence interaction?
            //if ((u1 & 4) > 0) colour.Blue += 1.0f; //portal - fence interaction / go away from?
            //if ((u1 & 8) > 0) colour.Red += 1.0f;//something file-specific? portal index related?


            //colour.Red = (PortalID) / 65535.0f; //portal ID testing... portalID only valid when portalType > 0!
            //colour.Green = (PortalID%5)/4.0f;
            //colour.Blue = ((PortalID/5)%5)/4.0f;


            colour.Alpha = 0.75f;

            return colour;
        }



        public void CalculatePosition()
        {
            //calc poly center for display purposes.
            Vector3 pcenter = Vector3.Zero;
            if (Vertices != null)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    pcenter += Vertices[i];
                }
            }
            float c = ((float)Vertices?.Length);
            if (c == 0.0f) c = 1.0f;
            Position = pcenter * (1.0f / c);
        }

        public void CalculateAABB()
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            if ((Vertices != null) && (Vertices.Length > 0))
            {
                min = new Vector3(float.MaxValue);
                max = new Vector3(float.MinValue);
                for (int i = 0; i < Vertices.Length; i++)
                {
                    min = Vector3.Min(min, Vertices[i]);
                    max = Vector3.Max(max, Vertices[i]);
                }
            }

            _RawData.CellAABB = new NavMeshAABB() { Min = min, Max = max };
        }


        public void WriteXml(StringBuilder sb, int indent)
        {
            byte[] flags = { Flags1, Flags2, Flags3, Flags4, UnkX, UnkY };
            YnvXml.WriteRawArray(sb, flags, indent, "Flags", "");
            YnvXml.WriteRawArray(sb, Vertices, indent, "Vertices", "", YnvXml.FormatVector3, 1);
            var cind = indent + 1;
            YnvXml.OpenTag(sb, indent, "Edges");
            foreach (var e in Edges)
            {
                YnvXml.Indent(sb, cind);
                sb.AppendFormat("{0}:{1}, {2}:{3}", e.AreaID1, e.PolyID1, e.AreaID2, e.PolyID2);
                sb.AppendLine();
            }
            YnvXml.CloseTag(sb, indent, "Edges");
            if ((PortalLinks != null) && (PortalLinks.Length > 0))
            {
                YnvXml.WriteRawArray(sb, PortalLinks, indent, "Portals", "");
            }
        }
        public void ReadXml(XmlNode node)
        {
            var flags = Xml.GetChildRawByteArrayNullable(node, "Flags", 10);
            if (flags != null)
            {
                Flags1 = (flags.Length > 0) ? flags[0] : (byte)0;
                Flags2 = (flags.Length > 1) ? flags[1] : (byte)0;
                Flags3 = (flags.Length > 2) ? flags[2] : (byte)0;
                Flags4 = (flags.Length > 3) ? flags[3] : (byte)0;
                UnkX = (flags.Length > 4) ? flags[4] : (byte)0;
                UnkY = (flags.Length > 5) ? flags[5] : (byte)0;
            }
            Vertices = Xml.GetChildRawVector3Array(node, "Vertices");
            Indices = new ushort[Vertices?.Length ?? 0];//needs to be present for later
            var edgesstr = Xml.GetChildInnerText(node, "Edges");
            var edgesstrarr = edgesstr.Trim().Split('\n');
            var edges = new List<YnvEdge>();
            foreach (var edgestr in edgesstrarr)
            {
                var estrparts = edgestr.Trim().Split(',');
                if (estrparts.Length != 2)
                { continue; }
                var estrp0 = estrparts[0].Trim().Split(':');
                var estrp1 = estrparts[1].Trim().Split(':');
                if (estrp0.Length != 2)
                { continue; }
                if (estrp1.Length != 2)
                { continue; }

                uint aid1, aid2, pid1, pid2;
                uint.TryParse(estrp0[0].Trim(), out aid1);
                uint.TryParse(estrp0[1].Trim(), out pid1);
                uint.TryParse(estrp1[0].Trim(), out aid2);
                uint.TryParse(estrp1[1].Trim(), out pid2);

                var e = new YnvEdge();
                e.AreaID1 = aid1;
                e.AreaID2 = aid2;
                e.PolyID1 = pid1;
                e.PolyID2 = pid2;
                edges.Add(e);
            }
            if (edges.Count > 0)
            {
                Edges = edges.ToArray();
            }

            PortalLinks = Xml.GetChildRawUshortArrayNullable(node, "Portals");
        }


        public override string ToString()
        {
            return AreaID.ToString() + ", " + Index.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvPortal : BasePathNode, IMetaXmlItem
    {
        public NavMeshPortal _RawData;

        public YnvFile Ynv { get; set; }
        public NavMeshPortal RawData { get { return _RawData; } set { _RawData = value; } }

        public Vector3 Position { get { return PositionFrom; } set { PositionFrom = value; } }
        public Vector3 PositionFrom { get; set; }
        public Vector3 PositionTo { get; set; }

        public byte Angle { get { return _RawData.Angle; } set { _RawData.Angle = value; } }
        public float Direction
        {
            get
            {
                return (float)Math.PI * 2.0f * Angle / 255.0f;
            }
            set
            {
                Angle = (byte)Math.Round(value * 255.0f / ((float)Math.PI * 2.0f));
            }
        }
        public Quaternion Orientation
        {
            get { return Quaternion.RotationAxis(Vector3.UnitZ, Direction); }
            set
            {
                Vector3 dir = value.Multiply(Vector3.UnitX);
                float dira = (float)Math.Atan2(dir.Y, dir.X);
                Direction = dira;
            }
        }

        public int Index { get; set; }
        public byte Type { get { return _RawData.Type; } set { _RawData.Type = value; } }//1,2,3
        public ushort AreaIDFrom { get { return _RawData.AreaIDFrom; } set { _RawData.AreaIDFrom = value; } }//always Ynv.AreaID
        public ushort AreaIDTo { get { return _RawData.AreaIDTo; } set { _RawData.AreaIDTo = value; } }//always Ynv.AreaID
        public ushort PolyIDFrom1 { get { return _RawData.PolyIDFrom1; } set { _RawData.PolyIDFrom1 = value; } }
        public ushort PolyIDFrom2 { get { return _RawData.PolyIDFrom2; } set { _RawData.PolyIDFrom2 = value; } }
        public ushort PolyIDTo1 { get { return _RawData.PolyIDTo1; } set { _RawData.PolyIDTo1 = value; } }
        public ushort PolyIDTo2 { get { return _RawData.PolyIDTo2; } set { _RawData.PolyIDTo2 = value; } }
        public ushort Unk1 { get { return _RawData.FlagsUnk; } set { _RawData.FlagsUnk = value; } }//always 0
        public byte Unk2 { get { return _RawData.AreaUnk; } set { _RawData.AreaUnk = value; } }//always 0


        public void Init(YnvFile ynv, NavMeshPortal portal)
        {
            Ynv = ynv;
            RawData = portal;
        }

        public void SetPosition(Vector3 pos)
        {
            var delta = pos - PositionFrom;
            PositionFrom = pos;
            PositionTo += delta;
            //TODO: update _RawData positions!
        }
        public void SetOrientation(Quaternion orientation)
        {
            Orientation = orientation;
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YnvXml.ValueTag(sb, indent, "Type", Type.ToString());
            YnvXml.ValueTag(sb, indent, "Angle", FloatUtil.ToString(Direction));
            YnvXml.ValueTag(sb, indent, "PolyFrom", PolyIDFrom1.ToString());
            YnvXml.ValueTag(sb, indent, "PolyTo", PolyIDTo1.ToString());
            YnvXml.SelfClosingTag(sb, indent, "PositionFrom " + FloatUtil.GetVector3XmlString(PositionFrom));
            YnvXml.SelfClosingTag(sb, indent, "PositionTo " + FloatUtil.GetVector3XmlString(PositionTo));
        }
        public void ReadXml(XmlNode node)
        {
            Type = (byte)Xml.GetChildUIntAttribute(node, "Type");
            Direction = Xml.GetChildFloatAttribute(node, "Angle");
            PolyIDFrom1 = PolyIDFrom2 = (ushort)Xml.GetChildUIntAttribute(node, "PolyFrom");
            PolyIDTo1 = PolyIDTo2 = (ushort)Xml.GetChildUIntAttribute(node, "PolyTo");
            PositionFrom = Xml.GetChildVector3Attributes(node, "PositionFrom");
            PositionTo = Xml.GetChildVector3Attributes(node, "PositionTo");
        }

        public override string ToString()
        {
            return Index.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvPoint : BasePathNode, IMetaXmlItem
    {
        public NavMeshPoint _RawData;

        public YnvFile Ynv { get; set; }
        public NavMeshPoint RawData { get { return _RawData; } set { _RawData = value; } }

        public Vector3 Position { get; set; }
        public byte Angle { get { return _RawData.Angle; } set { _RawData.Angle = value; } }
        public float Direction
        {
            get
            {
                return (float)Math.PI * 2.0f * Angle / 255.0f;
            }
            set
            {
                Angle = (byte)Math.Round(value * 255.0f / ((float)Math.PI * 2.0f));
            }
        }
        public Quaternion Orientation
        {
            get { return Quaternion.RotationAxis(Vector3.UnitZ, Direction); }
            set
            {
                Vector3 dir = value.Multiply(Vector3.UnitX);
                float dira = (float)Math.Atan2(dir.Y, dir.X);
                Direction = dira;
            }
        }

        public int Index { get; set; }
        public byte Type { get { return _RawData.Type; } set { _RawData.Type = value; } }//0,1,2,3,4,5,128,171,254

        public void Init(YnvFile ynv, NavMeshPoint point)
        {
            Ynv = ynv;
            RawData = point;
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
            //TODO! update _RawData.Position!!!
        }
        public void SetOrientation(Quaternion orientation)
        {
            Orientation = orientation;
        }

        public void WriteXml(StringBuilder sb, int indent)
        {
            YnvXml.ValueTag(sb, indent, "Type", Type.ToString());
            YnvXml.ValueTag(sb, indent, "Angle", FloatUtil.ToString(Direction));
            YnvXml.SelfClosingTag(sb, indent, "Position " + FloatUtil.GetVector3XmlString(Position));
        }
        public void ReadXml(XmlNode node)
        {
            Type = (byte)Xml.GetChildUIntAttribute(node, "Type");
            Direction = Xml.GetChildFloatAttribute(node, "Angle");
            Position = Xml.GetChildVector3Attributes(node, "Position");
        }

        public override string ToString()
        {
            return Index.ToString() + ": " + Type.ToString();
        }
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvEdge
    {
        public NavMeshEdge _RawData;
        public NavMeshEdge RawData { get { return _RawData; } set { _RawData = value; } }
        public YnvFile Ynv { get; set; }


        public uint AreaID1 { get; set; }
        public uint AreaID2 { get; set; }
        public uint PolyID1 { get { return _RawData._Poly1.PolyID; } set { _RawData._Poly1.PolyID = value; } }
        public uint PolyID2 { get { return _RawData._Poly2.PolyID; } set { _RawData._Poly2.PolyID = value; } }
        public YnvPoly Poly1 { get; set; }
        public YnvPoly Poly2 { get; set; }


        public YnvEdge() { }
        public YnvEdge(YnvEdge copy, YnvPoly poly)
        {
            _RawData = copy._RawData;
            _RawData._Poly1.PolyID = 0x3FFF;
            _RawData._Poly2.PolyID = 0x3FFF;
            Poly1 = poly;
            Poly2 = poly;
            AreaID1 = 0x3FFF;
            AreaID2 = 0x3FFF;
        }

        public void Init(YnvFile ynv, NavMeshEdge edge)
        {
            Ynv = ynv;
            RawData = edge;

            if (ynv.Nav == null) return;
            var n = ynv.Nav;

            var ai1 = edge.Poly1.AreaIDInd;
            var ai2 = edge.Poly2.AreaIDInd;

            AreaID1 = (ai1 < n.AdjAreaIDs.Count) ? n.AdjAreaIDs.Get(ai1) : 16383;
            AreaID2 = (ai2 < n.AdjAreaIDs.Count) ? n.AdjAreaIDs.Get(ai2) : 16383;

        }

        public override string ToString()
        {
            return AreaID1.ToString() + ", " + AreaID2.ToString() + ", " + PolyID1.ToString() + ", " + PolyID2.ToString() + ", " +
                _RawData._Poly1.Unk2.ToString() + ", " + _RawData._Poly2.Unk2.ToString() + ", " +
                _RawData._Poly1.Unk3.ToString() + ", " + _RawData._Poly2.Unk3.ToString();
        }

    }









    public class YnvXml : MetaXmlBase
    {

        public static string GetXml(YnvFile ynv)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            if ((ynv != null) && (ynv.Nav != null))
            {
                var name = "NavMesh";

                OpenTag(sb, 0, name);

                ynv.WriteXml(sb, 1);

                CloseTag(sb, 0, name);
            }

            return sb.ToString();
        }


    }


    public class XmlYnv
    {

        public static YnvFile GetYnv(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYnv(doc);
        }

        public static YnvFile GetYnv(XmlDocument doc)
        {
            YnvFile ynv = new YnvFile();
            ynv.ReadXml(doc.DocumentElement);
            return ynv;
        }





        public static List<T> ReadItemList<T>(XmlNode node, string name) where T : IMetaXmlItem, new()
        {
            var vnode2 = node.SelectSingleNode(name);
            if (vnode2 != null)
            {
                var inodes = vnode2.SelectNodes("Item");
                if (inodes?.Count > 0)
                {
                    var vlist = new List<T>();
                    foreach (XmlNode inode in inodes)
                    {
                        var v = new T();
                        v.ReadXml(inode);
                        vlist.Add(v);
                    }
                    return vlist;
                }
            }
            return null;
        }

    }

}
