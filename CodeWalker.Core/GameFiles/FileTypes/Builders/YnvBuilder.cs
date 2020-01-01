using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.GameFiles.FileTypes.Builders
{
    public class YnvBuilder
    {
        /*
         * 
         * YnvBuilder by dexyfex
         * 
         * This class allows for conversion of navmesh data in a generic format into .ynv files.
         * The usage is to call AddPoly() with an array of vertex positions for each polygon.
         * Polygons should be wound in an anticlockwise direction.
         * The returned YnvPoly object needs to have its Edges array set by the importer.
         * YnvPoly.Edges is an array of YnvEdge, with one edge for each vertex in the poly. 
         * The first edge should join the first and second vertices, and the last edge should
         * join the last and first vertices.
         * The YnvEdge Poly1 and Poly2 both need to be set to the same value, which is the 
         * corresponding YnvPoly object that was returned by AddPoly.
         * Flags values on the polygons and edges also need to be set by the importer.
         * 
         * Once the polygons and edges have all been added, the Build() method should be called,
         * which will return a list of YnvFile objects. Call the Save() method on each of those
         * to get the byte array for the .ynv file. The correct filename is given by the
         * YnvFile.Name property.
         * Note that the .ynv building process will split polygons that cross .ynv area borders,
         * and assign all the new polygons into the correct .ynv's.
         * 
         */



        public List<YnvPoly> PolyList = new List<YnvPoly>();
        public string VehicleName = string.Empty;
        private SpaceNavGrid NavGrid = null;
        private List<YnvFile> YnvFiles = null;

        public YnvPoly AddPoly(Vector3[] verts)
        {
            if ((verts == null) || (verts.Length < 3))
            { return null; }

            YnvPoly poly = new YnvPoly();
            poly.AreaID = 0x3FFF;
            poly.Index = PolyList.Count;
            poly.Vertices = verts;

            PolyList.Add(poly);

            return poly;
        }






        public List<YnvFile> Build(bool forVehicle)
        {
            NavGrid = new SpaceNavGrid();
            YnvFiles = new List<YnvFile>();

            if (forVehicle) //for vehicle YNV, only need a single ynv, no splitting
            {
                AddVehiclePolys(PolyList);

                FinalizeYnvs(YnvFiles, true);
            }
            else //for static world ynv, need to split polys and generate a set of ynv's.
            {
                //1: split polys going over nav grid borders, first by X then by Y
                var splitpolysX = SplitPolys(PolyList, true);
                var splitpolysY = SplitPolys(splitpolysX, false);

                //2: assign polys into their new ynv's
                AddPolysIntoGrid(splitpolysY);


                //3: fix up generated ynv's
                FinalizeYnvs(YnvFiles, false);

            }

            return YnvFiles;
        }





        private List<YnvPoly> SplitPolys(List<YnvPoly> polys, bool xaxis)
        {
            var newpolys = new List<YnvPoly>();

            var verts1 = new List<Vector3>();
            var verts2 = new List<Vector3>();
            var edges1 = new List<YnvEdge>();
            var edges2 = new List<YnvEdge>();

            var polysplits = new Dictionary<YnvPoly, YnvPolySplit>();

            foreach (var poly in polys)  //split along borders
            {
                var verts = poly.Vertices;
                if (verts == null)
                { continue; }//ignore empty polys..
                if (verts.Length < 3)
                { continue; }//not enough verts for a triangle!

                Vector2I gprev = NavGrid.GetCellPos(verts[0]);
                int split1 = 0;
                int split2 = 0;
                for (int i = 1; i < verts.Length; i++)
                {
                    Vector2I g = NavGrid.GetCellPos(verts[i]);
                    int g1 = xaxis ? g.X : g.Y;
                    int g2 = xaxis ? gprev.X : gprev.Y;
                    if (g1 != g2) //this poly is crossing a border
                    {
                        if (split1 == 0) { split1 = i; }
                        else { split2 = i; break; }
                    }
                    gprev = g;
                }
                if (split1 > 0)
                {
                    var split2beg = (split2 > 0) ? split2 - 1 : verts.Length - 1;
                    var split2end = split2beg + 1;
                    var sv11 = verts[split1 - 1];
                    var sv12 = verts[split1];
                    var sv21 = verts[split2beg];
                    var sv22 = verts[split2];
                    var sp1 = GetSplitPos(sv11, sv12, xaxis);
                    var sp2 = GetSplitPos(sv21, sv22, xaxis);

                    //if ((sp1 == sp2) || (sp1 == sv11) || (sp1 == sv12) || (sp2 == sv21) || (sp2 == sv22))
                    if (!IsValidSplit(sp1, sp2, sv11, sv12, sv21, sv22))
                    {
                        //split did nothing, just leave this poly alone
                        newpolys.Add(poly);
                    }
                    else
                    {
                        //split it!
                        var poly1 = new YnvPoly();
                        var poly2 = new YnvPoly();
                        poly1.RawData = poly.RawData;
                        poly2.RawData = poly.RawData;
                        verts1.Clear();
                        verts2.Clear();

                        for (int i = 0; i < split1; i++) verts1.Add(verts[i]);
                        verts1.Add(sp1);
                        verts1.Add(sp2);
                        for (int i = split2end; i < verts.Length; i++) verts1.Add(verts[i]);

                        verts2.Add(sp1);
                        for (int i = split1; i < split2end; i++) verts2.Add(verts[i]);
                        verts2.Add(sp2);

                        poly1.Vertices = verts1.ToArray();
                        poly2.Vertices = verts2.ToArray();


                        //save this information for the edge splitting pass
                        var polysplit = new YnvPolySplit();
                        polysplit.Orig = poly;
                        polysplit.New1 = poly1;
                        polysplit.New2 = poly2;
                        polysplit.Split1 = split1;
                        polysplit.Split2 = split2end;
                        polysplits[poly] = polysplit;


                        newpolys.Add(poly1);
                        newpolys.Add(poly2);
                    }
                }
                else
                {
                    //no need to split
                    newpolys.Add(poly);
                }
            }


            foreach (var polysplit in polysplits.Values) //build new edges for split polys
            {
                //the two edges that were split each need to be turned into two new edges (1 for each poly).
                //also, the split itself needs to be added as a new edge to the original poly.

                var poly = polysplit.Orig;
                var poly1 = polysplit.New1;
                var poly2 = polysplit.New2;
                var edges = poly.Edges;
                var verts = poly.Vertices;
                var ec = edges?.Length ?? 0;
                if (ec <= 0)
                { continue; }//shouldn't happen - no edges?
                if (ec != poly.Vertices?.Length)
                { continue; }//shouldn't happen

                var split1beg = polysplit.Split1 - 1;
                var split1end = polysplit.Split1;
                var split2beg = polysplit.Split2 - 1;
                var split2end = polysplit.Split2;

                edges1.Clear();
                edges2.Clear();

                var se1 = edges[split1beg]; //the two original edges that got split 
                var se2 = edges[split2beg];
                var sp1 = TryGetSplit(polysplits, se1.Poly1);//could use Poly2, but they should be the same..
                var sp2 = TryGetSplit(polysplits, se2.Poly1);
                var sv1a = verts[split1beg];
                var sv2a = verts[split2beg];
                var sp1a = sp1?.GetNearest(sv1a);
                var sp1b = sp1?.GetOther(sp1a);
                var sp2b = sp2?.GetNearest(sv2a);
                var sp2a = sp2?.GetOther(sp2b);
                var edge1a = new YnvEdge(se1, sp1a);
                var edge1b = new YnvEdge(se1, sp1b);
                var edge2a = new YnvEdge(se2, sp2a);
                var edge2b = new YnvEdge(se2, sp2b);
                var splita = new YnvEdge(se1, poly2);
                var splitb = new YnvEdge(se1, poly1);

                for (int i = 0; i < split1beg; i++) edges1.Add(edges[i]);//untouched edges
                edges1.Add(edge1a);
                edges1.Add(splita);
                edges1.Add(edge2a);
                for (int i = split2end; i < ec; i++) edges1.Add(edges[i]);//untouched edges

                edges2.Add(edge1b);
                for (int i = split1end; i < split2beg; i++) edges2.Add(edges[i]);//untouched edges
                edges2.Add(edge2b);
                edges2.Add(splitb);


                poly1.Edges = edges1.ToArray();
                poly2.Edges = edges2.ToArray();

                if (poly1.Edges.Length != poly1.Vertices.Length)
                { }//debug
                if (poly2.Edges.Length != poly2.Vertices.Length)
                { }//debug

            }

            foreach (var poly in newpolys) //fix any untouched edges that joined to split polys
            {
                if (poly.Edges?.Length != poly.Vertices?.Length)
                { continue; }//shouldn't happen (no edges?)
                for (int i = 0; i < poly.Edges.Length; i++)
                {
                    var edge = poly.Edges[i];
                    var vert = poly.Vertices[i];
                    if (edge == null)
                    { continue; }//shouldn't happen
                    if (edge.Poly1 != edge.Poly2)
                    { continue; }//shouldn't happen?
                    if (edge.Poly1 == null)
                    { continue; }//probably this edge joins to nothing


                    YnvPolySplit polysplit;
                    if (polysplits.TryGetValue(edge.Poly1, out polysplit))
                    {
                        var newpoly = polysplit.GetNearest(vert);
                        if (newpoly == null)
                        { }//debug
                        edge.Poly1 = newpoly;
                        edge.Poly2 = newpoly;
                    }

                }
            }


            return newpolys;
        }

        private Vector3 GetSplitPos(Vector3 a, Vector3 b, bool xaxis)
        {
            Vector3 ca = NavGrid.GetCellRel(a);
            Vector3 cb = NavGrid.GetCellRel(b);
            float fa = xaxis ? ca.X : ca.Y;
            float fb = xaxis ? cb.X : cb.Y;
            float f = 0;
            if (fb > fa)
            {
                float ib = (float)Math.Floor(fb);
                f = (ib - fa) / (fb - fa);
            }
            else
            {
                float ia = (float)Math.Floor(fa);
                f = (fa - ia) / (fa - fb);
            }
            if (f < 0.0f)
            { }//debug
            if (f > 1.0f)
            { }//debug
            return a + (b - a) * Math.Min(Math.Max(f, 0.0f), 1.0f);
        }

        private bool IsValidSplit(Vector3 s1, Vector3 s2, Vector3 v1a, Vector3 v1b, Vector3 v2a, Vector3 v2b)
        {
            if (XYEqual(s1, s2)) return false;
            if (XYEqual(s1, v1a)) return false;
            if (XYEqual(s1, v1b)) return false;
            if (XYEqual(s2, v2a)) return false;
            if (XYEqual(s2, v2b)) return false;
            return true;
        }

        private bool XYEqual(Vector3 v1, Vector3 v2)
        {
            return ((v1.X == v2.X) && (v1.Y == v2.Y));
        }

        private class YnvPolySplit
        {
            public YnvPoly Orig;
            public YnvPoly New1;
            public YnvPoly New2;
            public int Split1;
            public int Split2;
            public YnvPoly GetNearest(Vector3 v)
            {
                if (New1?.Vertices == null) return New2;
                if (New2?.Vertices == null) return New1;
                float len1 = float.MaxValue;
                float len2 = float.MaxValue;
                for (int i = 0; i < New1.Vertices.Length; i++)
                {
                    len1 = Math.Min(len1, (v - New1.Vertices[i]).LengthSquared());
                }
                if (len1 == 0.0f) return New1;
                for (int i = 0; i < New2.Vertices.Length; i++)
                {
                    len2 = Math.Min(len2, (v - New2.Vertices[i]).LengthSquared());
                }
                if (len2 == 0.0f) return New2;
                return (len1 <= len2) ? New1 : New2;
            }
            public YnvPoly GetOther(YnvPoly p)
            {
                if (p == New1) return New2;
                return New1;
            }
        }
        private YnvPolySplit TryGetSplit(Dictionary<YnvPoly, YnvPolySplit> polysplits, YnvPoly poly)
        {
            if (poly == null) return null;
            YnvPolySplit r = null;
            polysplits.TryGetValue(poly, out r);
            return r;
        }



        private void AddPolysIntoGrid(List<YnvPoly> polys)
        {
            foreach (var poly in polys)
            {
                poly.CalculatePosition();
                var pos = poly.Position;
                var cell = NavGrid.GetCell(pos);

                var ynv = cell.Ynv;
                if (ynv == null)
                {
                    ynv = new YnvFile();
                    ynv.Name = "navmesh[" + cell.FileX.ToString() + "][" + cell.FileY.ToString() + "]";
                    ynv.Nav = new NavMesh();
                    ynv.Nav.SetDefaults(false);
                    ynv.Nav.AABBSize = new Vector3(NavGrid.CellSize, NavGrid.CellSize, 0.0f);
                    ynv.Nav.SectorTree = new NavMeshSector();
                    ynv.Nav.SectorTree.AABBMin = new Vector4(NavGrid.GetCellMin(cell), 0.0f);
                    ynv.Nav.SectorTree.AABBMax = new Vector4(NavGrid.GetCellMax(cell), 0.0f);
                    ynv.AreaID = cell.X + cell.Y * 100;
                    ynv.Polys = new List<YnvPoly>();
                    ynv.HasChanged = true;//mark it for the project window
                    ynv.RpfFileEntry = new RpfResourceFileEntry();
                    ynv.RpfFileEntry.Name = ynv.Name + ".ynv";
                    ynv.RpfFileEntry.Path = string.Empty;
                    cell.Ynv = ynv;
                    YnvFiles.Add(ynv);
                }

                poly.AreaID = (ushort)ynv.AreaID;
                poly.Index = ynv.Polys.Count;
                poly.Ynv = ynv;
                ynv.Polys.Add(poly);

            }
        }

        private void AddVehiclePolys(List<YnvPoly> polys)
        {
            var bbmin = new Vector3(float.MaxValue);
            var bbmax = new Vector3(float.MinValue);
            foreach (var poly in polys)
            {
                poly.CalculatePosition();
                var pos = poly.Position;
                var verts = poly.Vertices;
                if (verts != null)
                {
                    foreach (var vert in verts)
                    {
                        bbmin = Vector3.Min(bbmin, vert);
                        bbmax = Vector3.Max(bbmax, vert);
                    }
                }
            }
            var bbsize = bbmax - bbmin;

            var ynv = new YnvFile();
            ynv.Name = VehicleName;
            ynv.Nav = new NavMesh();
            ynv.Nav.SetDefaults(true);
            ynv.Nav.AABBSize = new Vector3(bbsize.X, bbsize.Y, 0.0f);
            ynv.Nav.SectorTree = new NavMeshSector();
            ynv.Nav.SectorTree.AABBMin = new Vector4(bbmin, 0.0f);
            ynv.Nav.SectorTree.AABBMax = new Vector4(bbmax, 0.0f);
            ynv.AreaID = 10000;
            ynv.Polys = new List<YnvPoly>();
            ynv.HasChanged = true;//mark it for the project window
            ynv.RpfFileEntry = new RpfResourceFileEntry();
            ynv.RpfFileEntry.Name = ynv.Name + ".ynv";
            ynv.RpfFileEntry.Path = string.Empty;
            YnvFiles.Add(ynv);

            foreach (var poly in polys)
            {
                poly.AreaID = (ushort)ynv.AreaID;
                poly.Index = ynv.Polys.Count;
                poly.Ynv = ynv;
                ynv.Polys.Add(poly);
            }
        }


        private void FinalizeYnvs(List<YnvFile> ynvs, bool vehicle)
        {

            foreach (var ynv in ynvs)
            {
                //find zmin and zmax and update AABBSize and SectorTree root
                float zmin = float.MaxValue;
                float zmax = float.MinValue;
                foreach (var poly in ynv.Polys)
                {
                    foreach (var vert in poly.Vertices)
                    {
                        zmin = Math.Min(zmin, vert.Z);
                        zmax = Math.Max(zmax, vert.Z);
                    }
                }
                var yn = ynv.Nav;
                var ys = yn.SectorTree;
                yn.AABBSize = new Vector3(yn.AABBSize.X, yn.AABBSize.Y, zmax - zmin);
                ys.AABBMin = new Vector4(ys.AABBMin.X, ys.AABBMin.Y, zmin, 0.0f);
                ys.AABBMax = new Vector4(ys.AABBMax.X, ys.AABBMax.Y, zmax, 0.0f);


                ynv.UpdateContentFlags(vehicle);



                //fix up flags on edges that cross ynv borders
                foreach (var poly in ynv.Polys)
                {
                    bool border = false;
                    if (poly.Edges == null)
                    { continue; }
                    foreach (var edge in poly.Edges)
                    {
                        if (edge.Poly1 != null)
                        {
                            if (edge.Poly1.AreaID != poly.AreaID)
                            {
                                edge._RawData._Poly1.Unk2 = 0;//crash without this
                                edge._RawData._Poly2.Unk2 = 0;//crash without this
                                edge._RawData._Poly2.Unk3 = 4;////// edge._RawData._Poly2.Unk3 | 4;
                                border = true;

                                ////DEBUG don't join edges
                                //edge.Poly1 = null;
                                //edge.Poly2 = null;
                                //edge.AreaID1 = 0x3FFF;
                                //edge.AreaID2 = 0x3FFF;
                                //edge._RawData._Poly1.PolyID = 0x3FFF;
                                //edge._RawData._Poly2.PolyID = 0x3FFF;
                                //edge._RawData._Poly1.Unk2 = 1;
                                //edge._RawData._Poly2.Unk2 = 1;
                                //edge._RawData._Poly1.Unk3 = 0;
                                //edge._RawData._Poly2.Unk3 = 0;

                            }
                        }
                    }
                    poly.B19_IsCellEdge = border;
                }


            }

        }

    }
}
