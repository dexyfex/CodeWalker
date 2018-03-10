using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvFile : GameFile, PackedFile, BasePathData
    {
        public NavMesh Nav { get; set; }

        public List<Vector3> Vertices { get; set; }
        public List<ushort> Indices { get; set; }
        public List<NavMeshAdjPoly> AdjPolys { get; set; }
        public List<YnvPoly> Polys { get; set; }


        public EditorVertex[] PathVertices { get; set; }
        public EditorVertex[] TriangleVerts { get; set; }
        public Vector4[] NodePositions { get; set; }


        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;



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


            if ((Nav != null) && (Nav.SectorTree != null))
            {
                if (Nav.Vertices != null)
                {
                    Vector3 posoffset = Nav.SectorTree.AABBMin.XYZ();
                    Vector3 aabbsize = Nav.AABBSize;

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
                if (Nav.AdjPolys != null)
                {
                    AdjPolys = Nav.AdjPolys.GetFullList();
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


                        //calc poly center.
                        if ((Indices == null) || (Vertices == null))
                        { continue; }
                        var vc = Vertices.Count;
                        var ic = poly._RawData.IndexCount;
                        var startid = poly._RawData.IndexID;
                        var endid = startid + ic;
                        if (startid >= Indices.Count)
                        { continue; }
                        if (endid > Indices.Count)
                        { continue; }
                        Vector3 pcenter = Vector3.Zero;
                        float pcount = 0.0f;
                        for (int id = startid; id < endid; id++)
                        {
                            var ind = Indices[id];
                            if(ind>=vc)
                            { continue; }

                            pcenter += Vertices[ind];
                            pcount += 1.0f;
                        }
                        poly.Position = pcenter * (1.0f / pcount);


                    }
                }

            }



            UpdateAllNodePositions();

            UpdateTriangleVertices();


            Loaded = true;
            LoadQueued = true;
        }


        public byte[] Save()
        {
            BuildStructs();

            byte[] data = ResourceBuilder.Build(Nav, 2); //ynv is version 2...

            return data;
        }

        public void BuildStructs()
        {
            Vector3 posoffset = Nav.SectorTree?.AABBMin.XYZ() ?? Vector3.Zero;
            Vector3 aabbsize = Nav.AABBSize;
            Vector3 aabbsizeinv = 1.0f / aabbsize;

            var vertlist = new List<NavMeshVertex>();
            if (Vertices != null)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    vertlist.Add(NavMeshVertex.Create((Vertices[i] - posoffset) * aabbsizeinv));
                }
            }
            var polylist = new List<NavMeshPoly>();
            if (Polys != null)
            {
                for (int i = 0; i < Polys.Count; i++)
                {
                    polylist.Add(Polys[i].RawData);
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
            if (Nav.AdjPolys == null)
            {
                Nav.AdjPolys = new NavMeshList<NavMeshAdjPoly>();
                Nav.AdjPolys.VFT = 1080158440;
            }
            if (Nav.Polys == null)
            {
                Nav.Polys = new NavMeshList<NavMeshPoly>();
                Nav.Polys.VFT = 1080158408;
            }


            Nav.Vertices.RebuildList(vertlist);

            Nav.Indices.RebuildList(Indices);

            Nav.AdjPolys.RebuildList(AdjPolys);

            Nav.Polys.RebuildList(polylist);



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

        }





        public bool RemovePoly(YnvPoly poly)
        {
            return false;
        }





        public void UpdateAllNodePositions()
        {
            if (Nav == null) return;
            if (Nav.Portals == null) return;

            int cnt = Nav.Portals?.Length ?? 0;
            if (cnt <= 0)
            {
                NodePositions = null;
                return;
            }

            Vector3 posoffset = Nav.SectorTree.AABBMin.XYZ();
            Vector3 aabbsize = Nav.AABBSize;

            var np = new Vector4[cnt];
            for (int i = 0; i < cnt; i++)
            {
                var portal = Nav.Portals[i];
                var pv = portal.Position1.ToVector3();
                //var pv = portal.Position2.ToVector3();
                np[i] = new Vector4(posoffset + pv * aabbsize, 1.0f);
            }
            NodePositions = np;



            int lcnt = Nav.PortalLinks?.Length ?? 0;
            if (lcnt <= 0)
            {
                PathVertices = null;
                return;
            }

            //var lv = new EditorVertex[lcnt];
            //for (int i = 0; i < lcnt; i++)
            //{
            //    var pl = Nav.PortalLinks[i];
            //    if (pl >= np.Length) lv[i] = new EditorVertex();
            //    else
            //    {
            //        lv[i].Position = np[pl].XYZ();
            //        lv[i].Colour = 0xFF0000FF;
            //    }
            //}
            //PathVertices = lv;

            EditorVertex v = new EditorVertex();
            v.Colour = 0xFF0000FF;
            var lv = new List<EditorVertex>();
            //int cind = 0;
            var plinks = Nav.PortalLinks;
            for (int i = 0; i < cnt; i++)
            {
                var portal = Nav.Portals[i];

                //var plcnt = 2;

                v.Position = posoffset + portal.Position1.ToVector3() * aabbsize;
                lv.Add(v);
                v.Position = posoffset + portal.Position2.ToVector3() * aabbsize;
                lv.Add(v);


                //var plcnt = portal.LinkCount;
                //if (plcnt < 2) continue;
                //var plink = (cind < plinks.Length) ? plinks[cind] : 0xFFFF;
                //var ppos = (plink < np.Length) ? np[plink].XYZ() : Vector3.Zero;
                //for (int pl = 1; pl < plcnt; pl++)
                //{
                //    var ind2 = cind + pl;
                //    var plink2 = (ind2 < plinks.Length) ? plinks[ind2] : 0xFFFF;
                //    var ppos2 = (plink2 < np.Length) ? np[plink2].XYZ() : Vector3.Zero;
                //    v.Position = ppos; lv.Add(v);
                //    v.Position = ppos2; lv.Add(v);
                //}
                //cind += plcnt;
            }
            PathVertices = lv.ToArray();

        }

        public void UpdateTriangleVertices()
        {
            if (Nav == null) return;
            if (Nav.Polys == null) return;
            if (Nav.Vertices == null) return;

            //need position and colour for each vertex.
            //render as a triangle list... (no indices needed)

            //go through the nav mesh polys and generate verts to render...

            if ((Vertices == null) || (Vertices.Count == 0)) return;
            if ((Indices == null) || (Indices.Count == 0)) return;
            if ((Polys == null) || (Polys.Count == 0)) return;


            int vc = Vertices.Count;

            List<EditorVertex> rverts = new List<EditorVertex>();
            foreach (var ypoly in Polys)
            {
                var poly = ypoly.RawData;
                var colour = ypoly.GetColour();
                var colourval = (uint)colour.ToRgba();

                var ic = poly.IndexCount;
                var startid = poly.IndexID;
                var endid = startid + ic;
                if (startid >= Indices.Count)
                { continue; }
                if (endid > Indices.Count)
                { continue; }


                if(ic<3)
                { continue; }//not enough verts to make a triangle...

                if (ic > 15)
                { }


                EditorVertex p0 = new EditorVertex();
                EditorVertex p1 = new EditorVertex();
                EditorVertex p2 = new EditorVertex();
                p0.Colour = colourval;
                p1.Colour = colourval;
                p2.Colour = colourval;

                var startind = Indices[startid];
                if (startind >= vc)
                { continue; }

                p0.Position = Vertices[startind];

                //build triangles for the poly.
                int tricount = ic - 2;
                for (int t = 0; t < tricount; t++)
                {
                    int tid = startid + t;
                    int ind1 = Indices[tid + 1];
                    int ind2 = Indices[tid + 2];
                    if ((ind1 >= vc) || (ind2 >= vc))
                    { continue; }

                    p1.Position = Vertices[ind1];
                    p2.Position = Vertices[ind2];

                    rverts.Add(p0);
                    rverts.Add(p1);
                    rverts.Add(p2);
                }

            }

            TriangleVerts = rverts.ToArray();

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



    [TypeConverter(typeof(ExpandableObjectConverter))] public class YnvPoly
    {
        public NavMeshPoly _RawData;

        public YnvFile Ynv { get; set; }
        public NavMeshPoly RawData { get { return _RawData; } set { _RawData = value; } }

        public ushort AreaID { get { return _RawData.AreaID; } set { _RawData.AreaID = value; } }
        public ushort PartID { get { return _RawData.PartID; } set { _RawData.PartID = value; } }
        public ushort PortalID { get { return _RawData.PortalID; } set { _RawData.PortalID = value; } }
        public byte PortalUnk { get { return _RawData.PortalUnk; } set { _RawData.PortalUnk = value; } }
        public byte Flags1 { get { return (byte)(_RawData.Unknown_00h & 0xFF); } set { _RawData.Unknown_00h = (ushort)((_RawData.Unknown_00h & 0xFF00) | (value & 0xFF)); } }
        public byte Flags2 { get { return (byte)((_RawData.Unknown_24h.Value >> 0) & 0xFF); } set { _RawData.Unknown_24h = ((_RawData.Unknown_24h.Value & 0xFFFFFF00u) | ((value & 0xFFu) << 0)); } }
        public byte Flags3 { get { return (byte)((_RawData.Unknown_24h.Value >> 9) & 0xFF); } set { _RawData.Unknown_24h = ((_RawData.Unknown_24h.Value & 0xFFFE01FFu) | ((value & 0xFFu) << 9)); } }
        public byte Flags4 { get { return (byte)((_RawData.Unknown_28h.Value >> 16) & 0xFF); } set { _RawData.Unknown_28h = ((_RawData.Unknown_28h.Value & 0x0000FFFFu) | ((value & 0xFFu) << 16)); } }
        public bool B00_AvoidUnk        { get { return (_RawData.Unknown_00h & 1) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 0, value); } }
        public bool B01_AvoidUnk        { get { return (_RawData.Unknown_00h & 2) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 1, value); } }
        public bool B02_IsFootpath      { get { return (_RawData.Unknown_00h & 4) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 2, value); } }
        public bool B03_IsUnderground   { get { return (_RawData.Unknown_00h & 8) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 3, value); } }
        //public bool B04_Unused          { get { return (_RawData.Unknown_00h & 16) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 4, value); } }
        //public bool B05_Unused          { get { return (_RawData.Unknown_00h & 32) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 5, value); } }
        public bool B06_SteepSlope      { get { return (_RawData.Unknown_00h & 64) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 6, value); } }
        public bool B07_IsWater         { get { return (_RawData.Unknown_00h & 128) > 0; } set { _RawData.Unknown_00h = (ushort)BitUtil.UpdateBit(_RawData.Unknown_00h, 7, value); } }
        public bool B08_UndergroundUnk0 { get { return (_RawData.Unknown_24h.Value & 1) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 0, value); } }
        public bool B09_UndergroundUnk1 { get { return (_RawData.Unknown_24h.Value & 2) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 1, value); } }
        public bool B10_UndergroundUnk2 { get { return (_RawData.Unknown_24h.Value & 4) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 2, value); } }
        public bool B11_UndergroundUnk3 { get { return (_RawData.Unknown_24h.Value & 8) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 3, value); } }
        //public bool B12_Unused          { get { return (_RawData.Unknown_24h.Value & 16) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 4, value); } }
        public bool B13_HasPathNode     { get { return (_RawData.Unknown_24h.Value & 32) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 5, value); } }
        public bool B14_IsInterior      { get { return (_RawData.Unknown_24h.Value & 64) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 6, value); } }
        public bool B15_InteractionUnk  { get { return (_RawData.Unknown_24h.Value & 128) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 7, value); } }
        //public bool B16_Unused          { get { return (_RawData.Unknown_24h.Value & 256) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 8, value); } }
        public bool B17_IsFlatGround    { get { return (_RawData.Unknown_24h.Value & 512) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 9, value); } }
        public bool B18_IsRoad          { get { return (_RawData.Unknown_24h.Value & 1024) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 10, value); } }
        public bool B19_IsCellEdge      { get { return (_RawData.Unknown_24h.Value & 2048) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 11, value); } }
        public bool B20_IsTrainTrack    { get { return (_RawData.Unknown_24h.Value & 4096) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 12, value); } }
        public bool B21_IsShallowWater  { get { return (_RawData.Unknown_24h.Value & 8192) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 13, value); } }
        public bool B22_FootpathUnk1    { get { return (_RawData.Unknown_24h.Value & 16384) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 14, value); } }
        public bool B23_FootpathUnk2    { get { return (_RawData.Unknown_24h.Value & 32768) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 15, value); } }
        public bool B24_FootpathMall    { get { return (_RawData.Unknown_24h.Value & 65536) > 0; } set { _RawData.Unknown_24h = BitUtil.UpdateBit(_RawData.Unknown_24h.Value, 16, value); } }
        public bool B25_SlopeSouth      { get { return (_RawData.Unknown_28h.Value & 65536) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 16, value); } }
        public bool B26_SlopeSouthEast  { get { return (_RawData.Unknown_28h.Value & 131072) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 17, value); } }
        public bool B27_SlopeEast       { get { return (_RawData.Unknown_28h.Value & 262144) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 18, value); } }
        public bool B28_SlopeNorthEast  { get { return (_RawData.Unknown_28h.Value & 524288) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 19, value); } }
        public bool B29_SlopeNorth      { get { return (_RawData.Unknown_28h.Value & 1048576) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 20, value); } }
        public bool B30_SlopeNorthWest  { get { return (_RawData.Unknown_28h.Value & 2097152) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 21, value); } }
        public bool B31_SlopeWest       { get { return (_RawData.Unknown_28h.Value & 4194304) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 22, value); } }
        public bool B32_SlopeSouthWest  { get { return (_RawData.Unknown_28h.Value & 8388608) > 0; } set { _RawData.Unknown_28h = BitUtil.UpdateBit(_RawData.Unknown_28h.Value, 23, value); } }
        //public bool B33_PortalUnk1      { get { return (_RawData.PortalUnk & 1) > 0; } }
        //public bool B34_PortalUnk2      { get { return (_RawData.PortalUnk & 2) > 0; } }
        //public bool B35_PortalUnk3      { get { return (_RawData.PortalUnk & 4) > 0; } }
        //public bool B36_PortalUnk4      { get { return (_RawData.PortalUnk & 8) > 0; } }
        public byte UnkX { get { return _RawData.Unknown_28h_8a; } set { _RawData.Unknown_28h_8a = value; } }
        public byte UnkY { get { return _RawData.Unknown_28h_8b; } set { _RawData.Unknown_28h_8b = value; } }


        public Vector3 Position { get; set; }
        public int Index { get; set; }


        public void Init(YnvFile ynv, NavMeshPoly poly)
        {
            Ynv = ynv;
            RawData = poly;

        }


        public Color4 GetColour()
        {
            var colour = new Color4();
            var u0 = _RawData.Unknown_00h;
            if ((u0 & 1) > 0) colour.Red += 0.01f;//avoid? loiter?
            if ((u0 & 2) > 0) colour.Red += 0.01f; //avoid?
            if ((u0 & 4) > 0) colour.Green += 0.25f; //ped/footpath
            if ((u0 & 8) > 0) colour.Green += 0.02f; //underground?
            ////if ((u0 & 16) > 0) colour.Red += 1.0f; //not used?
            ////if ((u0 & 32) > 0) colour.Green += 1.0f;//not used?
            if ((u0 & 64) > 0) colour.Red += 0.25f; //steep slope
            if ((u0 & 128) > 0) colour.Blue += 0.25f; //water
            //if (u0 >= 256) colour.Green += 1.0f;//other bits unused...

            var u2 = _RawData.Unknown_24h.Value;
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

            var u5 = _RawData.Unknown_28h.Value; //32 bits
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

            var u1 = _RawData.PortalUnk;
            //if ((u1 & 1) > 0) colour.Red += 1.0f; //portal - don't interact?
            //if ((u1 & 2) > 0) colour.Green += 1.0f; //portal - ladder/fence interaction?
            //if ((u1 & 4) > 0) colour.Blue += 1.0f; //portal - fence interaction / go away from?
            //if ((u1 & 8) > 0) colour.Red += 1.0f;//something file-specific? portal index related?



            colour.Alpha = 0.75f;

            return colour;
        }


        public override string ToString()
        {
            return AreaID.ToString() + ", " + Index.ToString();
        }
    }


}
