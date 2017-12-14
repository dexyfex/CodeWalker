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
        public List<YnvPoly> Polys { get; set; }


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
        }




        public YnvFile() : base(null, GameFileType.Ynv)
        {
        }
        public YnvFile(RpfFileEntry entry) : base(entry, GameFileType.Ynv)
        {
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
            return null;
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

        public ushort AreaID { get { return _RawData.AreaID; } }
        public bool B00_AvoidUnk        { get { return (_RawData.Unknown_00h & 1) > 0; } }
        public bool B01_AvoidUnk        { get { return (_RawData.Unknown_00h & 2) > 0; } }
        public bool B02_IsFootpath      { get { return (_RawData.Unknown_00h & 4) > 0; } }
        public bool B03_IsUnderground   { get { return (_RawData.Unknown_00h & 8) > 0; } }
        //public bool B04_Unused          { get { return (_RawData.Unknown_00h & 16) > 0; } }
        //public bool B05_Unused          { get { return (_RawData.Unknown_00h & 32) > 0; } }
        public bool B06_SteepSlope      { get { return (_RawData.Unknown_00h & 64) > 0; } }
        public bool B07_IsWater         { get { return (_RawData.Unknown_00h & 128) > 0; } }
        public bool B08_UndergroundUnk1 { get { return (_RawData.Unknown_24h.Value & 1) > 0; } }
        public bool B09_UndergroundUnk2 { get { return (_RawData.Unknown_24h.Value & 2) > 0; } }
        public bool B10_UndergroundUnk3 { get { return (_RawData.Unknown_24h.Value & 4) > 0; } }
        public bool B11_UndergroundUnk4 { get { return (_RawData.Unknown_24h.Value & 8) > 0; } }
        //public bool B12_Unused          { get { return (_RawData.Unknown_24h.Value & 16) > 0; } }
        public bool B13_HasPathNode     { get { return (_RawData.Unknown_24h.Value & 32) > 0; } }
        public bool B14_IsInterior      { get { return (_RawData.Unknown_24h.Value & 64) > 0; } }
        public bool B15_InteractionUnk  { get { return (_RawData.Unknown_24h.Value & 128) > 0; } }
        //public bool B16_Unused          { get { return (_RawData.Unknown_24h.Value & 256) > 0; } }
        public bool B17_IsFlatGround    { get { return (_RawData.Unknown_24h.Value & 512) > 0; } }
        public bool B18_IsRoad          { get { return (_RawData.Unknown_24h.Value & 1024) > 0; } }
        public bool B19_IsCellEdge      { get { return (_RawData.Unknown_24h.Value & 2048) > 0; } }
        public bool B20_IsTrainTrack    { get { return (_RawData.Unknown_24h.Value & 4096) > 0; } }
        public bool B21_IsShallowWater  { get { return (_RawData.Unknown_24h.Value & 8192) > 0; } }
        public bool B22_FootpathUnk1    { get { return (_RawData.Unknown_24h.Value & 16384) > 0; } }
        public bool B23_FootpathUnk2    { get { return (_RawData.Unknown_24h.Value & 32768) > 0; } }
        public bool B24_FootpathMall    { get { return (_RawData.Unknown_24h.Value & 65536) > 0; } }
        public bool B25_SlopeSouth      { get { return (_RawData.Unknown_28h.Value & 65536) > 0; } }
        public bool B26_SlopeSouthEast  { get { return (_RawData.Unknown_28h.Value & 131072) > 0; } }
        public bool B27_SlopeEast       { get { return (_RawData.Unknown_28h.Value & 262144) > 0; } }
        public bool B28_SlopeNorthEast  { get { return (_RawData.Unknown_28h.Value & 524288) > 0; } }
        public bool B29_SlopeNorth      { get { return (_RawData.Unknown_28h.Value & 1048576) > 0; } }
        public bool B30_SlopeNorthWest  { get { return (_RawData.Unknown_28h.Value & 2097152) > 0; } }
        public bool B31_SlopeWest       { get { return (_RawData.Unknown_28h.Value & 4194304) > 0; } }
        public bool B32_SlopeSouthWest  { get { return (_RawData.Unknown_28h.Value & 8388608) > 0; } }
        public bool B33_PortalUnk1      { get { return (_RawData.PartUnk2 & 1) > 0; } }
        public bool B34_PortalUnk2      { get { return (_RawData.PartUnk2 & 2) > 0; } }
        public bool B35_PortalUnk3      { get { return (_RawData.PartUnk2 & 4) > 0; } }
        public bool B36_PortalUnk4      { get { return (_RawData.PartUnk2 & 8) > 0; } }
        public byte HeuristicXUnk { get { return (byte)_RawData.Unknown_28h_8a; } }
        public byte HeuristicYUnk { get { return (byte)_RawData.Unknown_28h_8b; } }


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

            var u1 = _RawData.PartUnk2;
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
