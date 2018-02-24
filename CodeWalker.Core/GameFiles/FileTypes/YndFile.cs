using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class YndFile : GameFile, PackedFile, BasePathData
    {

        public NodeDictionary NodeDictionary { get; set; }

        public YndNode[] Nodes { get; set; }
        public YndLink[] Links { get; set; }
        public YndJunction[] Junctions { get; set; }

        public EditorVertex[] LinkedVerts { get; set; }//populated by the space (needs to use grid of all ynd's!)
        public EditorVertex[] TriangleVerts { get; set; } //used for junctions display
        public Vector4[] NodePositions { get; set; }

        public Vector3 BBMin { get; set; }
        public Vector3 BBMax { get; set; }
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int AreaID
        {
            get
            {
                return CellY * 32 + CellX;
            }
            set
            {
                CellX = value % 32;
                CellY = value / 32;
                UpdateBoundingBox();
            }
        }

        public PathBVH BVH { get; set; }



        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;



        public YndFile() : base(null, GameFileType.Ynd)
        {
        }
        public YndFile(RpfFileEntry entry) : base(entry, GameFileType.Ynd)
        {
        }



        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ynd file (openIV-compatible format)

            RpfResourceFileEntry resentry = new RpfResourceFileEntry();

            //hopefully this format has an RSC7 header...
            uint rsc7 = BitConverter.ToUInt32(data, 0);
            if (rsc7 == 0x37435352) //RSC7 header present!
            {
                int version = BitConverter.ToInt32(data, 4);
                resentry.SystemFlags = BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);
                if (data.Length > 16)
                {
                    int newlen = data.Length - 16; //trim the header from the data passed to the next step.
                    byte[] newdata = new byte[newlen];
                    Buffer.BlockCopy(data, 16, newdata, 0, newlen);
                    data = newdata;
                }
                else
                {
                    data = null; //shouldn't happen... empty..
                }
            }
            else
            {
                //direct load from file without the rpf header..
                //assume it's in resource meta format
                resentry.SystemFlags = RpfResourceFileEntry.GetFlagsFromSize(data.Length, 0);
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, 2); //graphics type 2 for ymap
            }

            var oldresentry = RpfFileEntry as RpfResourceFileEntry;
            if (oldresentry != null) //update the existing entry with the new one
            {
                oldresentry.SystemFlags = resentry.SystemFlags;
                oldresentry.GraphicsFlags = resentry.GraphicsFlags;
                resentry.Name = oldresentry.Name;
                resentry.NameHash = oldresentry.NameHash;
                resentry.NameLower = oldresentry.NameLower;
                resentry.ShortNameHash = oldresentry.ShortNameHash;
            }
            else
            {
                RpfFileEntry = resentry; //just stick it in there for later...
            }

            data = ResourceBuilder.Decompress(data);


            Load(data, resentry);

            Loaded = true;
            LoadQueued = true;
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


            NodeDictionary = rd.ReadBlock<NodeDictionary>();

            if (NodeDictionary != null)
            {
                if (NodeDictionary.Nodes != null)
                {
                    var nodes = NodeDictionary.Nodes;
                    Nodes = new YndNode[nodes.Length];
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var n = new YndNode();
                        n.Init(this, nodes[i]);
                        Nodes[i] = n;
                        if (n.NodeID != i)
                        { } //never hit here - nodeid's have to match the index!
                    }
                }
                if ((NodeDictionary.JunctionRefs != null) && (NodeDictionary.Junctions != null))
                {
                    var juncrefs = NodeDictionary.JunctionRefs;
                    var juncs = NodeDictionary.Junctions;
                    Junctions = new YndJunction[juncrefs.Length];
                    for (int i = 0; i < juncrefs.Length; i++)
                    {
                        var juncref = NodeDictionary.JunctionRefs[i];
                        if (juncref.JunctionID >= juncs.Length)
                        { continue; }

                        var j = new YndJunction();
                        j.Init(this, juncs[juncref.JunctionID], juncref);
                        j.Heightmap = new YndJunctionHeightmap(NodeDictionary.JunctionHeightmapBytes, j);
                        Junctions[i] = j;
                    }
                }
            }

            UpdateAllNodePositions();

            //links will be populated by the space... maybe move that code here?



            string areaidstr = Name.ToLowerInvariant().Replace("nodes", "").Replace(".ynd", "");
            int areaid = 0;
            int.TryParse(areaidstr, out areaid);
            AreaID = areaid;

            UpdateBoundingBox();


            BuildBVH();


            Loaded = true;
            LoadQueued = true;
        }


        public byte[] Save()
        {

            BuildStructs();

            byte[] data = ResourceBuilder.Build(NodeDictionary, 1); //ynd is version 1...


            return data;

        }

        public void BuildStructs()
        {

            List<NodeLink> newlinks = new List<NodeLink>();
            List<NodeJunction> newjuncs = new List<NodeJunction>();
            List<NodeJunctionRef> newjuncrefs = new List<NodeJunctionRef>();
            List<byte> newjuncheightmaps = new List<byte>();

            if (Nodes != null)
            {
                int count = Nodes.Length;
                var nodes = new Node[count];
                for (int i = 0; i < count; i++)
                {
                    var node = Nodes[i];
                    if (node.Links != null)
                    {
                        node.LinkCount = node.Links.Length;
                        node.LinkID = (ushort)newlinks.Count;
                        for (int l = 0; l < node.Links.Length; l++)
                        {
                            var nlink = node.Links[l];
                            var rlink = nlink.RawData;
                            if (nlink.Node2 != null)
                            {
                                rlink.AreaID = nlink.Node2.AreaID;
                                rlink.NodeID = nlink.Node2.NodeID;
                            }
                            newlinks.Add(rlink);
                        }
                    }
                    else
                    {
                        node.LinkCount = 0;
                    }

                    //LinkCount = node.LinkCountFlags.Value >> 3;
                    //LinkCountUnk = node.LinkCountFlags.Value & 7;
                    byte lcflags = (byte)((node.LinkCount << 3) + (node.LinkCountUnk & 7));
                    node._RawData.LinkCountFlags = lcflags;

                    nodes[i] = node.RawData;

                    if ((node.HasJunction) && (node.Junction != null))
                    {
                        var nj = node.Junction;
                        var heightmapoff = newjuncheightmaps.Count;
                        var heightmap = nj.Heightmap.GetBytes();
                        nj._RawData.HeightmapPtr = (ushort)heightmapoff;
                        var jref = nj.RefData;
                        jref.AreaID = node.AreaID;
                        jref.NodeID = node.NodeID;
                        jref.JunctionID = (ushort)newjuncs.Count;
                        //jref.Unk0 = 0;//always 0?
                        nj.RefData = jref;//move this somewhere else..??
                        newjuncs.Add(nj.RawData);
                        newjuncrefs.Add(jref);
                        newjuncheightmaps.AddRange(heightmap);
                    }
                }
                NodeDictionary.Nodes = nodes;
                NodeDictionary.NodesCount = (uint)count;
                NodeDictionary.Links = newlinks.ToArray();
                NodeDictionary.LinksCount = (uint)newlinks.Count;
                NodeDictionary.Junctions = newjuncs.ToArray();
                NodeDictionary.JunctionsCount = (uint)newjuncs.Count;
                NodeDictionary.JunctionRefs = newjuncrefs.ToArray();
                NodeDictionary.JunctionRefsCount0 = (ushort)newjuncrefs.Count;
                NodeDictionary.JunctionRefsCount1 = (ushort)newjuncrefs.Count;
                NodeDictionary.JunctionHeightmapBytes = newjuncheightmaps.ToArray();
                NodeDictionary.JunctionHeightmapBytesCount = (uint)newjuncheightmaps.Count;
            }
            else
            {
                NodeDictionary.Nodes = null;
                NodeDictionary.NodesCount = 0;
                NodeDictionary.Links = null;
                NodeDictionary.LinksCount = 0;
                NodeDictionary.Junctions = null;
                NodeDictionary.JunctionsCount = 0;
                NodeDictionary.JunctionRefs = null;
                NodeDictionary.JunctionRefsCount0 = 0;
                NodeDictionary.JunctionRefsCount1 = 0;
                NodeDictionary.JunctionHeightmapBytes = null;
                NodeDictionary.JunctionHeightmapBytesCount = 0;
            }
        }



        public YndNode AddNode()
        {
            int cnt = Nodes?.Length ?? 0;
            YndNode yn = new YndNode();
            Node n = new Node();
            n.AreaID = (ushort)AreaID;
            n.NodeID = (ushort)cnt;
            yn.Init(this, n);

            int ncnt = cnt + 1;
            YndNode[] nnodes = new YndNode[ncnt];
            for (int i = 0; i < cnt; i++)
            {
                nnodes[i] = Nodes[i];
            }
            nnodes[cnt] = yn;
            Nodes = nnodes;
            NodeDictionary.NodesCount = (uint)ncnt;

            return yn;
        }

        public bool RemoveNode(YndNode node)
        {
            List<YndNode> newnodes = new List<YndNode>();
            int cnt = Nodes?.Length ?? 0;
            bool r = false;
            int ri = -1;
            for (int i = 0; i < cnt; i++)
            {
                var tn = Nodes[i];
                if (tn != node)
                {
                    newnodes.Add(tn);
                }
                else
                {
                    r = true;
                    ri = i;
                }
            }
            Nodes = newnodes.ToArray();
            NodeDictionary.NodesCount = (uint)newnodes.Count;
            NodeDictionary.NodesCountVehicle = Math.Min(NodeDictionary.NodesCountVehicle, NodeDictionary.NodesCount);
            NodeDictionary.NodesCountPed = Math.Min(NodeDictionary.NodesCountPed, NodeDictionary.NodesCount);

            //remap node ID's...
            List<YndLink> remlinks = new List<YndLink>();
            if (ri >= 0)
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    var n = Nodes[i];
                    if (n.NodeID != i)
                    {
                        n.NodeID = (ushort)i;


                        //update nodeid's in links...
                        for (int j = 0; j < Nodes.Length; j++)
                        {
                            var tn = Nodes[j];
                            if ((tn != null) && (tn.Links != null))
                            {
                                for (int bl = 0; bl < tn.Links.Length; bl++)
                                {
                                    var backlink = tn.Links[bl];
                                    if (backlink.Node2 == n)
                                    {
                                        backlink._RawData.NodeID = (ushort)i;
                                    }
                                }
                            }
                        }
                    }

                    //remove any links referencing the node.
                    remlinks.Clear();
                    if (n.Links != null)
                    {
                        for (int l = 0; l < n.Links.Length; l++)
                        {
                            var nlink = n.Links[l];
                            if (nlink.Node2 == node)
                            {
                                remlinks.Add(nlink);
                            }
                        }
                        for (int l = 0; l < remlinks.Count; l++)
                        {
                            n.RemoveLink(remlinks[l]);
                        }
                    }

                }

            }

            UpdateAllNodePositions();

            return r;
        }





        public void UpdateBoundingBox()
        {
            Vector3 corner = new Vector3(-8192, -8192, -2048);
            Vector3 cellsize = new Vector3(512, 512, 4096);

            BBMin = corner + (cellsize * new Vector3(CellX, CellY, 0));
            BBMax = BBMin + cellsize;
        }

        public void UpdateAllNodePositions()
        {
            int cnt = Nodes?.Length ?? 0;
            if (cnt <= 0)
            {
                NodePositions = null;
                return;
            }
            var np = new Vector4[cnt];
            for (int i = 0; i < cnt; i++)
            {
                np[i] = new Vector4(Nodes[i].Position, 1.0f);
            }
            NodePositions = np;
        }

        public void UpdateTriangleVertices()
        {
            //note: called from space.BuildYndVerts()

            UpdateLinkTriangleVertices();

            //UpdateJunctionTriangleVertices();
        }

        private void UpdateLinkTriangleVertices()
        {
            //build triangles for the path links display


            int vc = 0;
            if (Links != null)
            {
                vc = Links.Length * 6;
            }

            List<EditorVertex> verts = new List<EditorVertex>(vc);
            EditorVertex v0 = new EditorVertex();
            EditorVertex v1 = new EditorVertex();
            EditorVertex v2 = new EditorVertex();
            EditorVertex v3 = new EditorVertex();
            if ((Links != null) && (Nodes != null))
            {
                foreach (var node in Nodes)
                {
                    foreach (var link in node.Links)
                    {
                        var p0 = link.Node1?.Position ?? Vector3.Zero;
                        var p1 = link.Node2?.Position ?? Vector3.Zero;
                        var diff = p1 - p0;
                        var dir = Vector3.Normalize(diff);
                        var ax = Vector3.Cross(dir, Vector3.UnitZ);


                        float lanestot = link.LaneCountForward + link.LaneCountBackward;
                        //float backfrac = Math.Min(Math.Max(link.LaneCountBackward / lanestot, 0.1f), 0.9f);
                        //float lanewidth = 7.0f;
                        //float inner = totwidth*(backfrac-0.5f);
                        //float outer = totwidth*0.5f;

                        float lanewidth = node.IsPedNode ? 0.5f : 5.5f;
                        float inner = link.LaneOffset * lanewidth;// 0.0f;
                        float outer = inner + Math.Max(lanewidth * link.LaneCountForward, 0.5f);

                        float totwidth = lanestot * lanewidth;
                        float halfwidth = totwidth * 0.5f;
                        if (link.LaneCountBackward == 0)
                        {
                            inner -= halfwidth;
                            outer -= halfwidth;
                        }
                        if (link.LaneCountForward == 0)
                        {
                            inner += halfwidth;
                            outer += halfwidth;
                        }


                        v0.Position = p1 + ax * inner;
                        v1.Position = p0 + ax * inner;
                        v2.Position = p1 + ax * outer;
                        v3.Position = p0 + ax * outer;
                        var c = (uint)link.GetColour().ToRgba();
                        v0.Colour = c;
                        v1.Colour = c;
                        v2.Colour = c;
                        v3.Colour = c;
                        verts.Add(v0);
                        verts.Add(v1);
                        verts.Add(v2);
                        verts.Add(v2);
                        verts.Add(v1);
                        verts.Add(v3);
                    }
                }
            }


            if (verts.Count > 0)
            {
                TriangleVerts = verts.ToArray();
            }
            else
            {
                TriangleVerts = null;
            }
        }

        private void UpdateJunctionTriangleVertices()
        {
            //build triangles for the junctions bytes display....

            int vc = 0;
            if (Junctions != null)
            {
                foreach (var j in Junctions)
                {
                    var d = j.Heightmap;
                    if (d == null) continue;
                    vc += d.CountX * d.CountY * 6;
                }
            }

            List<EditorVertex> verts = new List<EditorVertex>(vc);
            EditorVertex v0 = new EditorVertex();
            EditorVertex v1 = new EditorVertex();
            EditorVertex v2 = new EditorVertex();
            EditorVertex v3 = new EditorVertex();
            if (Nodes != null)
            {
                foreach (var node in Nodes)
                {
                    if (node.Junction == null) continue;
                    var j = node.Junction;
                    var d = j.Heightmap;
                    if (d == null) continue;

                    float maxz = j.MaxZ / 32.0f;
                    float minz = j.MinZ / 32.0f;
                    float rngz = maxz - minz;
                    float posx = j.PositionX / 4.0f;
                    float posy = j.PositionY / 4.0f;

                    Vector3 pos = new Vector3(posx, posy, 0.0f);
                    Vector3 siz = new Vector3(d.CountX, d.CountY, 0.0f) * 2.0f;
                    //Vector3 siz = new Vector3(jx, jy, 0.0f);
                    Vector3 cnr = pos;// - siz * 0.5f;
                    //Vector3 inc = new Vector3(1.0f/jx)

                    cnr.Z = minz;// + 2.0f;

                    for (int y = 1; y < d.CountY; y++) //rows progress up the Y axis.
                    {
                        var row0 = d.Rows[y - 1];
                        var row1 = d.Rows[y];
                        float offy = y * 2.0f;

                        for (int x = 1; x < d.CountX; x++) //values progress along the X axis.
                        {
                            var val0 = row0.Values[x - 1] / 255.0f;
                            var val1 = row0.Values[x] / 255.0f;
                            var val2 = row1.Values[x - 1] / 255.0f;
                            var val3 = row1.Values[x] / 255.0f;
                            float offx = x * 2.0f;
                            v0.Position = cnr + new Vector3(offx - 2.0f, offy - 2.0f, val0 * rngz);
                            v1.Position = cnr + new Vector3(offx + 0.0f, offy - 2.0f, val1 * rngz);
                            v2.Position = cnr + new Vector3(offx - 2.0f, offy + 0.0f, val2 * rngz);
                            v3.Position = cnr + new Vector3(offx + 0.0f, offy + 0.0f, val3 * rngz);
                            v0.Colour = (uint)new Color4(val0, 1.0f - val0, 0.0f, 1.0f).ToRgba();
                            v1.Colour = (uint)new Color4(val1, 1.0f - val1, 0.0f, 1.0f).ToRgba();
                            v2.Colour = (uint)new Color4(val2, 1.0f - val2, 0.0f, 1.0f).ToRgba();
                            v3.Colour = (uint)new Color4(val3, 1.0f - val3, 0.0f, 1.0f).ToRgba();
                            verts.Add(v0);
                            verts.Add(v1);
                            verts.Add(v2);
                            verts.Add(v2);
                            verts.Add(v1);
                            verts.Add(v3);
                        }
                    }


                }
            }

            if (verts.Count > 0)
            {
                TriangleVerts = verts.ToArray();
            }
            else
            {
                TriangleVerts = null;
            }


        }


        public void UpdateBvhForNode(YndNode node)
        {
            //this needs to be called when a node's position changes...
            //if it changes a lot, need to recalc the BVH for mouse intersection optimisation purposes.

            //if (BVH == null) return;
            //BVH.UpdateForNode(node);

            BuildBVH();

            //also updates the NodePositions for the visible vertex
            if (Nodes != null)
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    if (Nodes[i] == node)
                    {
                        NodePositions[i] = new Vector4(node.Position, 1.0f);
                        break;
                    }
                }
            }

        }

        public void BuildBVH()
        {
            BVH = new PathBVH(Nodes, 10, 10);
        }




        public EditorVertex[] GetPathVertices()
        {
            return LinkedVerts;
        }
        public EditorVertex[] GetTriangleVertices()
        {
            return TriangleVerts;
        }
        public Vector4[] GetNodePositions()
        {
            return NodePositions;
        }



        public override string ToString()
        {
            return RpfFileEntry?.ToString() ?? string.Empty;
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))] public class YndNode : BasePathNode
    {
        public Node _RawData;

        public YndFile Ynd { get; set; }
        public Node RawData { get { return _RawData; } set { _RawData = value; } }
        public Vector3 Position { get; set; }
        public int LinkCount { get; set; }
        public int LinkCountUnk { get; set; }
        public YndLink[] Links { get; set; }

        public ushort AreaID { get { return _RawData.AreaID; } set { _RawData.AreaID = value; } }
        public ushort NodeID { get { return _RawData.NodeID; } set { _RawData.NodeID = value; } }
        public ushort LinkID { get { return _RawData.LinkID; } set { _RawData.LinkID = value; } }
        public FlagsByte Flags0 { get { return _RawData.Flags0; } set { _RawData.Flags0 = value; } }
        public FlagsByte Flags1 { get { return _RawData.Flags1; } set { _RawData.Flags1 = value; } }
        public FlagsByte Flags2 { get { return _RawData.Flags2; } set { _RawData.Flags2 = value; } }
        public FlagsByte Flags3 { get { return _RawData.Flags3; } set { _RawData.Flags3 = value; } }
        public FlagsByte Flags4 { get { return _RawData.Flags4; } set { _RawData.Flags4 = value; } }
        public TextHash StreetName { get { return _RawData.StreetName; } set { _RawData.StreetName = value; } }

        public Color4 Colour { get; set; }

        public YndJunction Junction { get; set; }
        public bool HasJunction;


        public bool IsPedNode
        {
            get
            {
                return false;// ((Flags4.Value >> 4) & 7) == 7;
            }
        }




        public void Init(YndFile ynd, Node node)
        {
            Ynd = ynd;
            RawData = node;
            Vector3 p = new Vector3();
            p.X = node.PositionX / 4.0f;
            p.Y = node.PositionY / 4.0f;
            p.Z = node.PositionZ / 32.0f;
            Position = p;

            LinkCount = node.LinkCountFlags.Value >> 3;
            LinkCountUnk = node.LinkCountFlags.Value & 7;

            Colour = GetColour();

        }

        public Color4 GetColour()
        {
            Color4 c = new Color4(LinkCountUnk / 7.0f, Flags0.Value / 255.0f, Flags1.Value / 255.0f, 1.0f);
            //Color4 c = new Color4(0.0f, 0.0f, 0.0f, 1.0f);

            //c.Red = (LinkCountUnk >> 1) / 3.0f;
            //c.Red = (Flags3.Value >> 1) / 127.0f;
            //c.Red = ((Flags4.Value >> 1) & 7) / 7.0f; //density value?
            //c.Green = 1.0f - c.Red;


            //if ((Flags0.Value & 1) > 0) c.Red += 1.0f; //script activated? N Yankton only + small piece in self storage
            //if ((Flags0.Value & 2) > 0) c.Red += 1.0f; //car can use / gps?
            //if ((Flags0.Value & 4) > 0) c.Red += 1.0f; //***not used
            //if ((Flags0.Value & 8) > 0) c.Red += 1.0f; //gravel surface? country roads mostly
            //if ((Flags0.Value & 16) > 0) c.Red += 1.0f; //***not used
            //if ((Flags0.Value & 32) > 0) c.Red += 1.0f; //slow speed? hills roads, prison boundary, carparks, airport roads etc
            //if ((Flags0.Value & 64) > 0) c.Red += 1.0f; //intersection entry 1 (has priority)?
            //if ((Flags0.Value & 128) > 0) c.Green += 1.0f; //intersection entry 2 unk?

            //if ((Flags1.Value & 1) > 0) c.Red += 1.0f; //left turn lane?
            //if ((Flags1.Value & 2) > 0) c.Red += 1.0f; //left turn node of no return
            //if ((Flags1.Value & 4) > 0) c.Red += 1.0f; //right turn node of no return
            //if ((Flags1.Value & 8) > 0) c.Red += 1.0f; //entry for traffic lights / boom gates etc
            //if ((Flags1.Value & 16) > 0) c.Red += 1.0f; //entry for traffic lights / boom gates etc + peds crossing
            //if ((Flags1.Value & 32) > 0) c.Red += 1.0f; //intersection entry 3 unk?
            //if ((Flags1.Value & 64) > 0) c.Red += 1.0f; //entry for traffic lights / boom gates etc + peds crossing
            //if ((Flags1.Value & 128) > 0) c.Red += 1.0f; //intersection minor/stop, T?

            ////[16 bits pos Z here]

            //if ((Flags2.Value & 1) > 0) c.Red += 1.0f; //slow traffic? peds? carparks? military? GPS disable routing??
            //if ((Flags2.Value & 2) > 0) c.Red += 1.0f; //***not used
            //if ((Flags2.Value & 4) > 0) c.Red += 1.0f; //intersection decision?
            //if ((Flags2.Value & 8) > 0) c.Red += 1.0f; //***not used
            //if ((Flags2.Value & 16) > 0) c.Red += 1.0f; //slower traffic?
            //if ((Flags2.Value & 32) > 0) c.Red += 1.0f; //water/boat
            //if ((Flags2.Value & 64) > 0) c.Red += 1.0f; //freeways  /peds?
            //if ((Flags2.Value & 128) > 0) c.Red += 1.0f; //not a main road...?

            //if ((LinkCountUnk & 1) > 0) c.Red += 1.0f; //has junction heightmap
            //if ((LinkCountUnk & 2) > 0) c.Red += 1.0f; //speed/density/type related? not runways, not freeways
            //if ((LinkCountUnk & 4) > 0) c.Red += 1.0f; //higher speed? eg freeway
            ////[5 bits LinkCount here]

            //if ((Flags3.Value & 1) > 0) c.Red += 1.0f; //is in an interior
            //if ((Flags3.Value & 2) > 0) c.Red += 1.0f; //heuristic val?
            //if ((Flags3.Value & 4) > 0) c.Red += 1.0f; //heuristic val?
            //if ((Flags3.Value & 8) > 0) c.Red += 1.0f; //heuristic val?
            //if ((Flags3.Value & 16) > 0) c.Red += 1.0f; //heuristic val?
            //if ((Flags3.Value & 32) > 0) c.Red += 1.0f; //heuristic val?
            //if ((Flags3.Value & 64) > 0) c.Red += 1.0f; //heuristic val?
            //if ((Flags3.Value & 128) > 0) c.Red += 1.0f; //heuristic val?

            //if ((Flags4.Value & 1) > 0) c.Red += 1.0f; //slow traffic?
            //if ((Flags4.Value & 2) > 0) c.Red += 1.0f; //density/popgroup value..?
            //if ((Flags4.Value & 4) > 0) c.Green += 1.0f; //density/popgroup value..?
            //if ((Flags4.Value & 8) > 0) c.Blue += 1.0f; //density/popgroup value..?
            //if ((Flags4.Value & 16) > 0) c.Red += 1.0f; //special/peds path?
            //if ((Flags4.Value & 32) > 0) c.Green += 1.0f; //special/peds path?
            //if ((Flags4.Value & 64) > 0) c.Blue += 1.0f; //special/peds path?
            //if ((Flags4.Value & 128) > 0) c.Blue += 1.0f; //intersection entry left turn?






            ////regarding paths.xml:
            ////rubidium - Today at 8:37 AM
            //also, quick glimpse over the xml for attributes:
            ////> grep - i "attribute name" paths.xml | awk - F'^"' ' { print $2 }' | sort - u
            //Block If No Lanes
            //Cannot Go Left
            //Cannot Go Right
            //Density
            //Disabled
            //Dont Use For Navigation
            //GpsBothWays
            //Highway
            //Indicate Keep Left
            //Indicate Keep Right
            //Lanes In
            //Lanes Out
            //Left Turns Only
            //Narrowroad
            //No Big Vehicles
            //NoGps
            //Off Road
            //Shortcut
            //Slip Lane
            //Special
            //Speed
            //Streetname
            //Tunnel
            //Water
            //Width




            return c;
        }


        public void SetPosition(Vector3 pos)
        {
            _RawData.PositionX = (short)(pos.X * 4.0f);
            _RawData.PositionY = (short)(pos.Y * 4.0f);
            _RawData.PositionZ = (short)(pos.Z * 32.0f);

            Vector3 newpos = pos;
            //newpos.X = _RawData.PositionX / 4.0f;
            //newpos.Y = _RawData.PositionY / 4.0f;
            //newpos.Z = _RawData.PositionZ / 32.0f;
            Position = newpos;

            UpdateLinkLengths();
        }


        public void UpdateLinkLengths()
        {
            if (Links == null) return;
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                link.UpdateLength(); //update this node's links

                var n2 = link.Node2;
                if ((n2 == null) || (n2.Links == null)) continue;

                for (int j = 0; j < n2.Links.Length; j++)
                {
                    var n2l = n2.Links[j];
                    if (n2l.Node2 == this)
                    {
                        n2l.UpdateLength(); //update back links
                    }
                }
            }
        }


        public YndLink AddLink(YndNode tonode = null)
        {
            YndLink l = new YndLink();
            l._RawData.AreaID = AreaID;
            l.Node1 = this;
            if (tonode != null)
            {
                l.Node2 = tonode;
                l._RawData.AreaID = tonode.AreaID;
                l._RawData.NodeID = tonode.NodeID;
            }
            else if ((Ynd.Nodes != null) && (Ynd.Nodes.Length > 0))
            {
                l.Node2 = Ynd.Nodes[0];
            }
            else
            {
                l.Node2 = this;
                l._RawData.NodeID = NodeID;
            }
            l.UpdateLength();

            int cnt = Links?.Length ?? 0;
            int ncnt = cnt + 1;
            YndLink[] nlinks = new YndLink[ncnt];
            for (int i = 0; i < cnt; i++)
            {
                nlinks[i] = Links[i];
            }
            nlinks[cnt] = l;
            Links = nlinks;
            LinkCount = ncnt;

            return l;
        }

        public bool RemoveLink(YndLink l)
        {
            List<YndLink> newlinks = new List<YndLink>();
            int cnt = Links?.Length ?? 0;
            bool r = false;
            for (int i = 0; i < cnt; i++)
            {
                var tl = Links[i];
                if (tl != l)
                {
                    newlinks.Add(tl);
                }
                else
                {
                    r = true;
                }
            }
            Links = newlinks.ToArray();
            LinkCount = newlinks.Count;
            return r;
        }


        public override string ToString()
        {
            //return AreaID.ToString() + "." + NodeID.ToString();
            return StreetName.ToString() + ", " + Position.X.ToString() + ", " + Position.Y.ToString() + ", " + Position.Z.ToString() + ", " + NodeID.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YndLink
    {
        public YndFile Ynd { get; set; }
        public YndNode Node1 { get; set; }
        public YndNode Node2 { get; set; }
        public NodeLink _RawData;
        public NodeLink RawData { get { return _RawData; } set { _RawData = value; } }
        public FlagsByte Flags0 { get { return _RawData.Flags0; } set { _RawData.Flags0 = value; } }
        public FlagsByte Flags1 { get { return _RawData.Flags1; } set { _RawData.Flags1 = value; } }
        public FlagsByte Flags2 { get { return _RawData.Flags2; } set { _RawData.Flags2 = value; } }
        public FlagsByte LinkLength { get { return _RawData.LinkLength; } set { _RawData.LinkLength = value; } }

        public int LaneCountForward { get { return (Flags2.Value >> 5) & 7; } }
        public int LaneCountBackward { get { return (Flags2.Value >> 2) & 7; } }

        public int OffsetValue { get { return (Flags1.Value >> 4) & 7; } }
        public bool NegativeOffset { get { return (Flags1.Value >> 7) > 0; } }
        public float LaneOffset { get { return (OffsetValue / 7.0f) * (NegativeOffset ? -0.5f : 0.5f); } }


        public void Init(YndFile ynd, YndNode node1, YndNode node2, NodeLink link)
        {
            Ynd = ynd;
            Node1 = node1;
            Node2 = node2;
            RawData = link;
        }


        public void UpdateLength()
        {
            if (Node1 == null) return;
            if (Node2 == null) return;

            LinkLength = (byte)Math.Min(255, (Node2.Position - Node1.Position).Length());
        }


        public void CopyFlags(YndLink link)
        {
            if (link == null) return;
            Flags0 = link.Flags0;
            Flags1 = link.Flags1;
            Flags2 = link.Flags2;
        }



        public Color4 GetColour()
        {


            //float f0 = Flags0.Value / 255.0f;
            //float f1 = Flags1.Value / 255.0f;
            //float f2 = Flags2.Value / 255.0f;
            //var c = new Color4(f0, f1, f2, 1.0f);

            var c = new Color4(0.0f, 0.0f, 0.0f, 0.5f);
            c.Green = LaneCountForward / 7.0f;
            c.Red = LaneCountBackward / 7.0f;


            //if ((Flags0.Value & 1) > 0) c.Red = 1.0f; //? some small pieces in city, roads at docks, and mall at beach
            //if ((Flags0.Value & 2) > 0) c.Red = 1.0f; //3x segments joining east canal paths to roads, also josh's driveway - scripted?
            //if ((Flags0.Value & 4) > 0) c.Red = 1.0f; //? looks fairly random, 0 for water, alternating - slope related?
            //if ((Flags0.Value & 8) > 0) c.Red = 1.0f; //? like above
            //if ((Flags0.Value & 16) > 0) c.Red = 1.0f; //? similar to above, but less
            //if ((Flags0.Value & 32) > 0) c.Red = 1.0f; //? like above
            //if ((Flags0.Value & 64) > 0) c.Red = 1.0f; //? slightly less random
            //if ((Flags0.Value & 128) > 0) c.Red = 1.0f; //? still looks random...

            //if ((Flags1.Value & 1) > 0) c.Red = 1.0f; //***not used?
            //if ((Flags1.Value & 2) > 0) c.Red = 1.0f; //?possibly width/type bit
            //if ((Flags1.Value & 4) > 0) c.Red = 1.0f; //avoid routing? no through road / no other exit?
            //if ((Flags1.Value & 8) > 0) c.Red = 1.0f; //prefer routing? exit from dead end?
            //if ((Flags1.Value & 16) > 0) c.Red = 1.0f; //offset value.   mostly single lane, carpark access, golf course, alleyways, driveways, beach area etc
            //if ((Flags1.Value & 32) > 0) c.Green = 1.0f; //offset value.  similar to above
            //if ((Flags1.Value & 64) > 0) c.Green = 1.0f; //offset value.  similar to above
            //if ((Flags1.Value & 128) > 0) c.Red = 1.0f; //offset value. (sign)  similar to above (all paired with their back links!)

            //if ((Flags2.Value & 1) > 0) c.Red = 1.0f; //angled link - merge? enter/exit divided road section, most big junctions, always paired
            //if ((Flags2.Value & 2) > 0) c.Red = 1.0f; //lane change/u-turn link?  always paired
            //if ((Flags2.Value & 4) > 0) c.Red = 1.0f; //lane count back dir
            //if ((Flags2.Value & 8) > 0) c.Red = 1.0f; //lane count back dir
            //if ((Flags2.Value & 16) > 0) c.Red = 1.0f; //lane count back dir
            //if ((Flags2.Value & 32) > 0) c.Green = 1.0f; //lane count forward dir
            //if ((Flags2.Value & 64) > 0) c.Green = 1.0f; //lane count forward dir
            //if ((Flags2.Value & 128) > 0) c.Green = 1.0f; //lane count forward dir

            ////var lanesfwd = (Flags2.Value >> 5) & 7;
            ////var lanesbck = (Flags2.Value >> 2) & 7;
            //////if ((lanesfwd > 0) && (lanesbck > 0) && (lanesfwd != lanesbck))
            //////{ }



            //var t = (Flags1.Value >> 4)&1;
            //c.Red = t / 1.0f;
            //c.Green = 1.0f - c.Red;
            ////if (((Flags1.Value & 128) > 0))// && ((Flags1.Value & 64) == 0))
            ////{ c.Red += 1.0f; }



            return c;
        }



        public override string ToString()
        {
            return Node2._RawData.ToString();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YndJunction
    {
        public YndFile Ynd { get; set; }
        public NodeJunction _RawData;
        public NodeJunction RawData { get { return _RawData; } set { _RawData = value; } }
        public NodeJunctionRef RefData { get; set; }
        public YndJunctionHeightmap Heightmap { get; set; }
        public short MaxZ { get; set; }
        public short MinZ { get; set; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }

        public void Init(YndFile ynd, NodeJunction junc, NodeJunctionRef reff)
        {
            Ynd = ynd;
            RawData = junc;
            RefData = reff;
            MaxZ = junc.MaxZ;
            MinZ = junc.MinZ;
            PositionX = junc.PositionX;
            PositionY = junc.PositionY;
        }


        public void ResizeHeightmap()
        {
            Heightmap.Resize(_RawData.HeightmapDimX, _RawData.HeightmapDimY);
        }

        public void SetHeightmap(string text)
        {
            Heightmap.SetData(text);
        }


        public override string ToString()
        {
            return RefData.ToString();
        }


    }

    [TypeConverter(typeof(ExpandableObjectConverter))] public class YndJunctionHeightmap
    {
        public YndJunctionHeightmapRow[] Rows { get; set; }
        public int CountX { get; set; }
        public int CountY { get; set; }

        public YndJunctionHeightmap(byte[] data, YndJunction junc)
        {
            if (data == null)
            { return; }

            var d = junc.RawData;
            int s = d.HeightmapPtr;
            CountX = d.HeightmapDimX;
            CountY = d.HeightmapDimY;

            if ((s + CountX * CountY) > data.Length)
            { return; }

            Rows = new YndJunctionHeightmapRow[CountY];


            for (int y = 0; y < CountY; y++)
            {
                int o = s + y * CountX;
                byte[] vals = new byte[CountX];
                Buffer.BlockCopy(data, o, vals, 0, CountX);
                Rows[y] = new YndJunctionHeightmapRow(vals);
            }
        }


        public byte[] GetBytes()
        {
            int cnt = CountX * CountY;
            var bytes = new byte[cnt];
            for (int y = 0; y < CountY; y++)
            {
                int o = y * CountX;
                var rowvals = Rows[y].Values;
                Buffer.BlockCopy(rowvals, 0, bytes, o, CountX);
            }
            return bytes;
        }


        public void Resize(int cx, int cy)
        {
            var nrows = new YndJunctionHeightmapRow[cy];
            for (int y = 0; y < cy; y++)
            {
                byte[] nvals = new byte[cx];
                int minx = Math.Min(cx, CountX);
                if ((Rows != null) && (y < Rows.Length))
                {
                    Buffer.BlockCopy(Rows[y].Values, 0, nvals, 0, minx);
                }
                nrows[y] = new YndJunctionHeightmapRow(nvals);
            }
            Rows = nrows;

            CountX = cx;
            CountY = cy;
        }

        public void SetData(string text)
        {
            var rsplit = new[] { '\n' };
            var csplit = new[] { ' ' };
            string[] rowstrs = text.Split(rsplit, StringSplitOptions.RemoveEmptyEntries);
            for (int y = 0; y < rowstrs.Length; y++)
            {
                string[] colstrs = rowstrs[y].Trim().Split(csplit, StringSplitOptions.RemoveEmptyEntries);
                int cx = colstrs.Length;
                byte[] vals = new byte[cx];
                for (int x = 0; x < cx; x++)
                {
                    byte.TryParse(colstrs[x], out vals[x]);
                }
                int minx = Math.Min(cx, CountX);
                if ((Rows != null) && (y < Rows.Length))
                {
                    var nrow = new byte[CountX];
                    Buffer.BlockCopy(vals, 0, nrow, 0, minx);
                    Rows[y].Values = nrow;
                }
            }
        }

        public string GetDataString()
        {
            StringBuilder sb = new StringBuilder();
            if (Rows != null)
            {
                foreach (var row in Rows)
                {
                    sb.AppendLine(row.ToString());
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return CountX.ToString() + " x " + CountY.ToString();
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))] public class YndJunctionHeightmapRow
    {
        public byte[] Values { get; set; }

        public YndJunctionHeightmapRow(byte[] vals)
        {
            Values = vals;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Values.Length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(Values[i].ToString().PadLeft(3, '0'));
                //sb.Append(Convert.ToString(Values[i], 16).ToUpper().PadLeft(2, '0'));
            }
            return sb.ToString();
        }
    }




    public class PathBVHNode
    {
        public int Depth;
        public int MaxDepth;
        public int Threshold;
        public List<BasePathNode> Nodes;
        public BoundingBox Box;
        public BoundingSphere Sphere;
        public PathBVHNode Node1;
        public PathBVHNode Node2;


        public void CalcBounds()
        {
            if ((Nodes == null) || (Nodes.Count <= 0)) return;

            Box.Minimum = new Vector3(float.MaxValue);
            Box.Maximum = new Vector3(float.MinValue);
            foreach (var node in Nodes)
            {
                Box.Minimum = Vector3.Min(Box.Minimum, node.Position);
                Box.Maximum = Vector3.Max(Box.Maximum, node.Position);
            }
            Sphere.Center = (Box.Minimum + Box.Maximum) * 0.5f;
            Sphere.Radius = (Box.Maximum - Box.Minimum).Length() * 0.5f;
        }


        public void Build()
        {
            if ((Nodes == null) || (Nodes.Count <= Threshold) || (Depth >= MaxDepth)) return;

            Vector3 avgsum = Vector3.Zero;
            foreach (var node in Nodes)
            {
                avgsum += node.Position;
            }
            Vector3 avg = avgsum * (1.0f / Nodes.Count);

            int countx = 0, county = 0, countz = 0;
            foreach (var node in Nodes)
            {
                if (node.Position.X < avg.X) countx++;
                if (node.Position.Y < avg.Y) county++;
                if (node.Position.Z < avg.Z) countz++;
            }

            int target = Nodes.Count / 2;
            int dx = Math.Abs(target - countx);
            int dy = Math.Abs(target - county);
            int dz = Math.Abs(target - countz);

            int axis = -1;
            if ((dx <= dy) && (dx <= dz)) axis = 0; //x seems best
            else if (dy <= dz) axis = 1; //y seems best
            else axis = 2; //z seems best


            List<BasePathNode> l1 = new List<BasePathNode>();
            List<BasePathNode> l2 = new List<BasePathNode>();
            foreach (var node in Nodes)
            {
                bool s = false;
                switch (axis)
                {
                    default:
                    case 0: s = (node.Position.X > avg.X); break;
                    case 1: s = (node.Position.Y > avg.Y); break;
                    case 2: s = (node.Position.Z > avg.Z); break;
                }
                if (s) l1.Add(node);
                else l2.Add(node);
            }

            var cdepth = Depth + 1;

            Node1 = new PathBVHNode();
            Node1.Depth = cdepth;
            Node1.MaxDepth = MaxDepth;
            Node1.Threshold = Threshold;
            Node1.Nodes = new List<BasePathNode>(l1);
            Node1.CalcBounds();
            Node1.Build();

            Node2 = new PathBVHNode();
            Node2.Depth = cdepth;
            Node2.MaxDepth = MaxDepth;
            Node2.Threshold = Threshold;
            Node2.Nodes = new List<BasePathNode>(l2);
            Node2.CalcBounds();
            Node2.Build();
        }


        public void UpdateForNode(BasePathNode node)
        {
            if (!Nodes.Contains(node)) return;
            Box.Minimum = Vector3.Min(Box.Minimum, node.Position);
            Box.Maximum = Vector3.Max(Box.Maximum, node.Position);

            if (Node1 != null) Node1.UpdateForNode(node);
            if (Node2 != null) Node2.UpdateForNode(node);
        }

    }

    public class PathBVH : PathBVHNode
    {

        public PathBVH(IEnumerable<BasePathNode> nodes, int threshold, int maxdepth)
        {
            Threshold = threshold;
            MaxDepth = maxdepth;
            Nodes = (nodes != null) ? new List<BasePathNode>(nodes) : new List<BasePathNode>();
            CalcBounds();
            Build();
        }

    }




    public interface BasePathNode
    {
        Vector3 Position { get; set; }
    }

    public interface BasePathData
    {
        //reuse this interface for file types that need to get paths rendered...

        EditorVertex[] GetPathVertices();
        EditorVertex[] GetTriangleVertices();
        Vector4[] GetNodePositions();
    }

    

}
