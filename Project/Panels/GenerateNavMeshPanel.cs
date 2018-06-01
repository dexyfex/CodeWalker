using CodeWalker.Core.GameFiles.FileTypes.Builders;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project.Panels
{
    public partial class GenerateNavMeshPanel : ProjectPanel
    {
        public ProjectForm ProjectForm { get; set; }
        public ProjectFile CurrentProjectFile { get; set; }

        public GenerateNavMeshPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
            Tag = "GenerateNavMeshPanel";

            if (ProjectForm?.WorldForm == null)
            {
                //could happen in some other startup mode - world form is required for this..
                GenerateButton.Enabled = false;
                UpdateStatus("Unable to generate - World View not available!");
            }
        }

        public void SetProject(ProjectFile project)
        {
            CurrentProjectFile = project;



        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var space = ProjectForm?.WorldForm?.Space;
            if (space == null) return;
            var gameFileCache = ProjectForm?.WorldForm?.GameFileCache;
            if (gameFileCache == null) return;

            Vector2 min = FloatUtil.ParseVector2String(MinTextBox.Text);
            Vector2 max = FloatUtil.ParseVector2String(MaxTextBox.Text);

            if (min == max)
            {
                MessageBox.Show("Unable to generate - No valid area was specified!\nMake sure Min and Max form a box around the area you want to generate the nav meshes for.");
                return;
            }

            if ((min.X < -6000) || (min.Y < -6000) || (max.X > 9000) || (max.Y > 9000))//it's over 9000
            {
                if (MessageBox.Show("Warning: min/max goes outside the possible navmesh area - valid range is from -6000 to 9000 (X and Y).\nDo you want to continue anyway?", "Warning - specified area out of range", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            //if (string.IsNullOrEmpty(ProjectForm?.CurrentProjectFile?.Filepath))
            //{
            //    MessageBox.Show("Please save the current project first. Generated navmeshes will be placed in the project folder.");
            //    return;
            //}

            var path = ProjectForm.CurrentProjectFile.GetFullFilePath("navmeshes") + "\\";

            GenerateButton.Enabled = false;



            float density = 0.5f; //distance between vertices for the initial grid
            //float clipdz = 0.5f; //any polygons with greater steepness should be removed

            Vector2I imin = space.Grid.GetCellPos(new Vector3(min, 0));
            Vector2I imax = space.Grid.GetCellPos(new Vector3(max, 0));

            //Vector2 vertexCounts = (max - min) / density;
            //int vertexCountX = (int)vertexCounts.X;
            //int vertexCountY = (int)vertexCounts.Y;
            //int vertexCountTot = vertexCountX * vertexCountY;

            var layers = new[] { true, false, false }; //collision layers to use

            int hitTestCount = 0; //statistic for number of hit tests done
            int hitCount = 0;//statistic for total ray hits
            int newCount = 0;//statistic for total new control vertices

            Task.Run(() =>
            {


                //find vertices in one world cell at a time, by raycasting in a grid pattern. 
                //then filter those based on polygon slope deltas (and materials) to reduce the detail.
                //then add the generated verts for the cell into a master quadtree/bvh/grid
                //after all verts are generated, do voronoi tessellation with those to generate the nav polys.
                //finally, remove any polys that are steeper than the threshold.


                var vgrid = new VertexGrid();
                var vert = new GenVertex();
                var builder = new YnvBuilder();

                for (int x = imin.X; x <= imax.X; x++) //generate verts for each world cell
                {
                    for (int y = imin.Y; y <= imax.Y; y++)
                    {
                        Vector2I gi = new Vector2I(x, y);
                        var cell = space.Grid.GetCell(gi);
                        var cellstr = gi.ToString();
                        var cellmin = space.Grid.GetWorldPos(gi);
                        var cellmax = cellmin + SpaceGrid.CellSize;
                        var vertexCountXY = (cellmax - cellmin) / density;
                        int vertexCountX = (int)vertexCountXY.X;
                        int vertexCountY = (int)vertexCountXY.Y;
                        //int vertexCountTot = vertexCountX * vertexCountY;
                        vgrid.BeginGrid(vertexCountX, vertexCountY);
                        cellmin.Z = 0.0f;//min probably not needed here
                        cellmax.Z = 0.0f;
                        Ray ray = new Ray(Vector3.Zero, new Vector3(0, 0, -1));//for casting with

                        UpdateStatus("Loading cell " + cellstr + " ...");

                        //pre-warm the bounds cache for this cell, and find the min/max Z
                        if (cell.BoundsList != null)
                        {
                            foreach (var boundsitem in cell.BoundsList)
                            {
                                YbnFile ybn = gameFileCache.GetYbn(boundsitem.Name);
                                if (ybn == null)
                                { continue; } //ybn not found?
                                if (!ybn.Loaded) //ybn not loaded yet...
                                {
                                    UpdateStatus("Loading ybn: " + boundsitem.Name.ToString() + " ...");
                                    int waitCount = 0;
                                    while (!ybn.Loaded)
                                    {
                                        waitCount++;
                                        if (waitCount > 10000)
                                        {
                                            UpdateStatus("Timeout waiting for ybn " + boundsitem.Name.ToString() + " to load!");
                                            Thread.Sleep(1000); //just to let the message display for a second...
                                            break;
                                        }
                                        Thread.Sleep(20);//~50fps should be fine
                                    }
                                }
                                if (ybn.Loaded && (ybn.Bounds != null))
                                {
                                    cellmin.Z = Math.Min(cellmin.Z, ybn.Bounds.BoundingBoxMin.Z);
                                    cellmax.Z = Math.Max(cellmax.Z, ybn.Bounds.BoundingBoxMax.Z);
                                }
                            }
                        }



                        //ray-cast each XY vertex position, and find the height and surface from ybn's
                        //continue casting down to find more surfaces...

                        UpdateStatus("Processing cell " + cellstr + " ...");

                        for (int vx = 0; vx < vertexCountX; vx++)
                        {
                            for (int vy = 0; vy < vertexCountY; vy++)
                            {
                                vgrid.BeginCell(vx, vy);
                                var vcoffset = new Vector3(vx, vy, 0) * density;
                                ray.Position = cellmin + vcoffset;
                                ray.Position.Z = cellmax.Z + 1.0f;//start the ray at the top of the cell
                                var intres = space.RayIntersect(ray, float.MaxValue, layers);
                                hitTestCount++;
                                while (intres.Hit && (intres.HitDist > 0))
                                {
                                    hitCount++;
                                    vert.Position = intres.Position;
                                    vert.Normal = intres.Normal;
                                    vert.Material = intres.Material.Type;
                                    vert.PolyFlags = intres.Material.PolyFlags;
                                    vert.PrevIDX = -1;
                                    vert.PrevIDY = -1;
                                    vert.NextIDX = -1;
                                    vert.NextIDY = -1;
                                    vert.CompPrevX = false;
                                    vert.CompPrevY = false;
                                    vert.CompNextX = false;
                                    vert.CompNextY = false;
                                    vert.PolyID = -1;
                                    vgrid.AddVertex(ref vert);

                                    //continue down until no more hits..... step by 3m
                                    if (vgrid.CurVertexCount > 15) //too many hits?
                                    { break; }
                                    ray.Position.Z = intres.Position.Z - 3.0f;
                                    intres = space.RayIntersect(ray, float.MaxValue, layers);
                                }
                                vgrid.EndCell(vx, vy);
                            }
                        }

                        vgrid.EndGrid(); //build vertex array





                        vgrid.ConnectVertices();


                        var polys = vgrid.GenPolys();
                        newCount += polys.Count;

                        foreach (var poly in polys)
                        {
                            if (poly.Vertices == null) continue;

                            var ypoly = builder.AddPoly(poly.Vertices);


                            ypoly.B02_IsFootpath = (poly.Material.Index == 1);

                            ypoly.B18_IsRoad = (poly.Material.Index == 4);//4,5,6


                        }

                    }
                }




                var ynvs = builder.Build(false);//todo:vehicles!


                foreach (var ynv in ynvs)
                {
                    var bytes = ynv.Save();
                    var fpath = path + ynv.Name + ".ynv";
                    //File.WriteAllBytes(fpath, bytes);

                    YnvFile nynv = new YnvFile();
                    nynv.RpfFileEntry = new RpfResourceFileEntry();
                    nynv.RpfFileEntry.Name = ynv.Name + ".ynv";
                    nynv.FilePath = fpath;
                    nynv.Name = ynv.RpfFileEntry.Name;
                    nynv.Load(bytes);


                    ProjectForm.Invoke((MethodInvoker) delegate 
                    {
                        ProjectForm.AddYnvToProject(nynv);
                    });
                }



                var statf = "{0} hit tests, {1} hits, {2} new polys";
                var stats = string.Format(statf, hitTestCount, hitCount, newCount);
                UpdateStatus("Process complete. " + stats);
                GenerateComplete();
            });
        }




        private struct GenVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public BoundsMaterialType Material;
            public ushort PolyFlags;

            public int PrevIDX;
            public int PrevIDY;
            public int NextIDX;
            public int NextIDY;
            public bool CompPrevX;
            public bool CompPrevY;
            public bool CompNextX;
            public bool CompNextY;
            public int PolyID;

        }

        private class GenEdge
        {
            //public GenPoly From;
            //public GenPoly To;
        }

        private class GenPoly
        {
            public int Index;
            public Vector3 Normal;
            public BoundsMaterialType Material;
            public ushort PolyFlags;

            public int[] CornerIndices;
            public Vector3[] Vertices;
            //public GenEdge[] Edges;
        }

        private class VertexGrid
        {
            public List<GenVertex> VertexList = new List<GenVertex>();
            public GenVertex[] Vertices;
            public int[,] VertexOffsets;
            public int[,] VertexCounts;
            public int VertexCountX;
            public int VertexCountY;
            public int CurVertexCount;

            private List<int> CornersB = new List<int>();
            private List<int> CornersT = new List<int>();


            public void BeginGrid(int vertexCountX, int vertexCountY)
            {
                VertexList.Clear();
                Vertices = null;
                VertexOffsets = new int[vertexCountX, vertexCountY];
                VertexCounts = new int[vertexCountX, vertexCountY];
                VertexCountX = vertexCountX;
                VertexCountY = vertexCountY;
            }
            public void EndGrid()
            {
                Vertices = VertexList.ToArray();
            }

            public void BeginCell(int x, int y)
            {
                VertexOffsets[x, y] = VertexList.Count;
                CurVertexCount = 0;
            }
            public void EndCell(int x, int y)
            {
                VertexCounts[x, y] = CurVertexCount;
            }

            public void AddVertex(ref GenVertex v)
            {
                VertexList.Add(v);
                CurVertexCount++;
            }

            public int FindVertex(int x, int y, float z, float thresh)
            {
                //return the index of the closest vertex in the x,y cell that is within the Z threshold

                int offset = VertexOffsets[x, y];
                int count = VertexCounts[x, y];
                int lasti = offset + count;

                float minz = float.MaxValue;
                int mini = -1;
                for (int i = offset; i < lasti; i++)
                {
                    float vz = Vertices[i].Position.Z;
                    float dz = Math.Abs(vz - z);
                    if ((dz < thresh) && (dz < minz))
                    {
                        minz = dz;
                        mini = i;
                    }
                }

                return mini;
            }



            public bool CompareVertexTypes(int i1, int i2)
            {
                if (Vertices[i1].Material.Index != Vertices[i2].Material.Index) return false;
                if (Vertices[i1].PolyFlags != Vertices[i2].PolyFlags) return false;
                return true;
            }





            public void ConnectVertices()
            {
                var connectThresh = 0.2f;
                var density = 0.5f;//to match vertex density (x/y distance)
                for (int vx = 1; vx < VertexCountX; vx++)
                {
                    int px = vx - 1;
                    for (int vy = 1; vy < VertexCountY; vy++)
                    {
                        int py = vy - 1;
                        int imin = VertexOffsets[vx, vy];
                        int imax = VertexCounts[vx, vy] + imin;
                        for (int i = imin; i < imax; i++)
                        {
                            var vz = Vertices[i].Position.Z;
                            var vn = Vertices[i].Normal;
                            var vxz = vz + (vn.X / Math.Max(vn.Z, 1e-5f)) * density;
                            var vyz = vz + (vn.Y / Math.Max(vn.Z, 1e-5f)) * density;
                            var prevIDX = FindVertex(px, vy, vxz, connectThresh);
                            var prevIDY = FindVertex(vx, py, vyz, connectThresh);
                            var compPrevX = (prevIDX < 0) ? false : CompareVertexTypes(i, prevIDX);
                            var compPrevY = (prevIDY < 0) ? false : CompareVertexTypes(i, prevIDY);
                            Vertices[i].PrevIDX = prevIDX;
                            Vertices[i].PrevIDY = prevIDY;
                            Vertices[i].CompPrevX = compPrevX;
                            Vertices[i].CompPrevY = compPrevY;
                            if (prevIDX >= 0)
                            {
                                Vertices[prevIDX].NextIDX = i;
                                Vertices[prevIDX].CompNextX = compPrevX;
                            }
                            if (prevIDY >= 0)
                            {
                                Vertices[prevIDY].NextIDY = i;
                                Vertices[prevIDY].CompNextY = compPrevY;
                            }
                        }
                    }
                }

            }




            public List<GenPoly> GenPolys()
            {
                List<GenPoly> polys = new List<GenPoly>();

                //find new polygon edges and assign grid vertices
                for (int vx = 0; vx < VertexCountX; vx++)
                {
                    for (int vy = 0; vy < VertexCountY; vy++)
                    {
                        int imin = VertexOffsets[vx, vy];
                        int imax = VertexCounts[vx, vy] + imin;
                        for (int i = imin; i < imax; i++)
                        {
                            if (Vertices[i].PolyID >= 0) continue; //already assigned

                            if ((Vertices[i].PrevIDX < 0) && (Vertices[i].PrevIDY < 0) && (Vertices[i].NextIDX < 0) && (Vertices[i].NextIDY < 0)) continue; //(not connected to anything)

                            //if (!(Vertices[i].CompPrevX || Vertices[i].CompPrevY || Vertices[i].CompNextX || Vertices[i].CompNextY)) //continue; //all joins are different - discard this vertex
                            




                            GenPoly poly = new GenPoly(); //start a new poly
                            poly.Index = polys.Count;
                            poly.Normal = Vertices[i].Normal;
                            poly.Material = Vertices[i].Material;
                            poly.PolyFlags = Vertices[i].PolyFlags;
                            //polys.Add(poly);
                            //poly.AddGenVert(i);
                            //Vertices[i].PolyID = poly.Index;
                            Plane vplane = new Plane(Vertices[i].Position, Vertices[i].Normal);
                            float plthresh = 0.25f; //threshold for plane dist test

                            int dpx = FindPolyEdgeDist(ref vplane, plthresh, i, 0);
                            int dpy = FindPolyEdgeDist(ref vplane, plthresh, i, 1);
                            int dnx = FindPolyEdgeDist(ref vplane, plthresh, i, 2);
                            int dny = FindPolyEdgeDist(ref vplane, plthresh, i, 3);

                            bool addpoly = true;

                            int qnx = 0, qny = 0, qpy = 0, qdir = 0;
                            if ((dpx == 0) && (dpy == 0) && (dnx == 0) && (dny == 0))
                            {
                                //single vertex poly... connect to something else? currently remove
                                addpoly = false;
                            }
                            else if ((dpx >= dnx) && (dpx >= dpy) && (dpx >= dny))
                            {
                                //dpx is largest, move along -X  (dpx, dpy, dny, 0)
                                qnx = dpx;
                                qny = dpy;
                                qpy = dny;
                                qdir = 0;
                            }
                            else if ((dpy >= dnx) && (dpy >= dny))
                            {
                                //dpy is largest, move along -Y  (dpy, dnx, dpx, 1)
                                qnx = dpy;
                                qny = dnx;
                                qpy = dpx;
                                qdir = 1;
                            }
                            else if ((dnx >= dny))
                            {
                                //dnx is largest, move along +X  (dnx, dny, dpy, 2)
                                qnx = dnx;
                                qny = dny;
                                qpy = dpy;
                                qdir = 2;
                            }
                            else
                            {
                                //dny is largest, move along +Y (dny, dpx, dnx, 3)
                                qnx = dny;
                                qny = dpx;
                                qpy = dnx;
                                qdir = 3;
                            }
                            if (addpoly)
                            {
                                AssignVertices2(ref vplane, plthresh, i, qnx, qny, qpy, qdir, poly);
                                if (poly.CornerIndices?.Length > 2)
                                {
                                    polys.Add(poly);
                                }
                            }



                            //if (dnx > 0) //can move along +X
                            //{
                            //    AssignVertices(ref vplane, plthresh, i, dnx, dny, dpy, 2, poly);
                            //}
                            //else if (dny > 0) //can move along +Y
                            //{
                            //    AssignVertices(ref vplane, plthresh, i, dny, dpx, dnx, 3, poly);
                            //}
                            //else if (dpx > 0) //can move along -X
                            //{
                            //    AssignVertices(ref vplane, plthresh, i, dpx, dpy, dny, 0, poly);
                            //}
                            //else if (dpy > 0) //can move along -Y
                            //{
                            //    AssignVertices(ref vplane, plthresh, i, dpy, dnx, dpx, 1, poly);
                            //}
                            //else //single vertex poly... connected to something else
                            //{
                            //    addpolys = false;
                            //}
                            //if (addpolys)
                            //{
                            //    polys.Add(poly);
                            //}




                        }
                    }

                }


                //create corner vertex vectors and edges for the new polys
                foreach (var poly in polys)
                {
                    if (poly.CornerIndices == null) continue;
                    if (poly.CornerIndices.Length < 3) continue;

                    var verts = new Vector3[poly.CornerIndices.Length];

                    for (int i = 0; i < poly.CornerIndices.Length; i++)
                    {
                        int id = poly.CornerIndices[i];

                        verts[i] = Vertices[id].Position;//TODO: find actual corners
                    }

                    poly.Vertices = verts;

                }


                return polys;
            }


            private void AssignVertices(ref Plane vpl, float plt, int i, int dnx, int dny, int dpy, int dir, GenPoly poly)
            {
                int pid = poly.Index;
                int qi = i;
                int maxdnx = Math.Min(dnx, 40);
                int maxdpy = 50;// dpy;//
                int maxdny = 50;// dny;//
                int cdpy = dpy;
                int cdny = dny;
                int vertexCountP = 0;
                int vertexCountN = 0;
                int lastqx = 0;
                int lastqi = i;
                CornersB.Clear();
                CornersT.Clear();


                int dirpy, dirny;
                switch (dir) //lookup perpendicular directions
                {
                    default:
                    case 0: dirpy = 3; dirny = 1; break;
                    case 1: dirpy = 0; dirny = 2; break;
                    case 2: dirpy = 1; dirny = 3; break;
                    case 3: dirpy = 2; dirny = 0; break;
                }


                for (int qx = 0; qx <= maxdnx; qx++)//go along the row until the next poly is hit...
                {
                    lastqi = qi;
                    int qipy = qi;//bottom vertex id for this column
                    int qiny = qi;//top vertex id for this column
                    for (int qy = 0; qy <= cdpy; qy++)//assign this row of verts to the poly
                    {
                        Vertices[qipy].PolyID = pid;
                        vertexCountP++;

                        if (qy < cdpy) qipy = GetNextID(qipy, dirpy);
                    }
                    for (int qy = 0; qy <= cdny; qy++)
                    {
                        Vertices[qiny].PolyID = pid;
                        vertexCountN++;

                        if (qy < cdny) qiny = GetNextID(qiny, dirny);
                    }

                    qi = GetNextID(qi, dir);  //move on to the next column...

                    if (qx == dnx)//last column
                    {
                        if (qipy != lastqi) CornersB.Add(qipy);//lastqi will be added anyway, don't duplicate it
                        if (qiny != lastqi) CornersT.Add(qiny);
                        break;
                    }
                    if (qi < 0)//can't go any further.. most likely hit the end
                    { break; }//(shouldn't hit here because of above break)

                    if (Vertices[qi].PolyID >= 0) //already assigned to a poly.. stop!
                    { break; }//(shouldn't hit here because maxdnx shouldn't go that far!)

                    int ndpy = FindPolyEdgeDist(ref vpl, plt, qi, dirpy);//height for the next col..
                    int ndny = FindPolyEdgeDist(ref vpl, plt, qi, dirny);
                    int ddpy = ndpy - cdpy;
                    int ddny = ndny - cdny;

                    //TODO: step further along to find slope fraction if eg ddpy==0
                    if (ddpy > maxdpy/*+1*/) ddpy = maxdpy/*+1*/;//########### BAD
                    else if (ddpy < maxdpy) //bottom corner vertex
                    {
                        maxdpy = ddpy;
                        CornersB.Add(qipy);
                    }
                    if (ddny > maxdny/*+1*/) ddny = maxdny/*+1*/;//########### BAD
                    else if (ddny < maxdny) //top corner vertex..
                    {
                        maxdny = ddny;
                        CornersT.Add(qiny);
                    }
                    cdpy = cdpy + ddpy; //update comparison distances with limits, for next loop
                    cdny = cdny + ddny;
                    if ((cdpy < 0) || (cdny < 0))//can't go any further.. limit slope hit the axis
                    {
                        if (qipy != lastqi) CornersB.Add(qipy);//lastqi will be added anyway, don't duplicate it
                        if (qiny != lastqi) CornersT.Add(qiny);
                        break;
                    }


                    lastqx = qx;
                }

                var totverts = vertexCountN + vertexCountP;
                var fracused = (float)(lastqx+1) / dnx;

                CornersB.Add(lastqi);
                int cc = CornersB.Count + CornersT.Count - 1;
                int[] corners = new int[cc];
                int ci = 0;
                for (int c = 0; c < CornersB.Count; c++)
                {
                    corners[ci] = CornersB[c]; ci++;
                }
                for (int c = CornersT.Count - 1; c > 0; c--)
                {
                    corners[ci] = CornersT[c]; ci++;
                }
                poly.CornerIndices = corners;
                if (corners.Length < 3)
                { }//debug
            }


            private void AssignVertices2(ref Plane vpl, float plt, int i, int dnx, int dny, int dpy, int dir, GenPoly poly)
            {
                int pid = poly.Index;
                int qi = i;
                //int maxdnx = Math.Min(dnx, 40);
                //int maxdpy = 50;// dpy;//
                //int maxdny = 50;// dny;//
                //int cdpy = dpy;
                //int cdny = dny;
                //int vertexCountP = 0;
                //int vertexCountN = 0;
                //int lastqx = 0;
                //int lastqi = i;
                CornersB.Clear();
                CornersT.Clear();


                int dirpy, dirny, dirpx;
                switch (dir) //lookup perpendicular directions
                {
                    default:
                    case 0: dirpy = 3; dirny = 1; dirpx = 2; break;
                    case 1: dirpy = 0; dirny = 2; dirpx = 3; break;
                    case 2: dirpy = 1; dirny = 3; dirpx = 0; break;
                    case 3: dirpy = 2; dirny = 0; dirpx = 1; break;
                }

                int ti = i;
                while (CanPolyIncludeNext(ref vpl, plt, ti, dirpx, out ti))
                {
                    qi = ti; //make sure to start at the leftmost point...
                }



                //loop until top and bottom lines intersect, or moved more than max dist

                float slopeb = FindSlope(ref vpl, plt, qi, dir, dirpy, dirny, 100);
                float slopet = FindSlope(ref vpl, plt, qi, dir, dirny, dirpy, 100);
                int syb = MaxOffsetFromSlope(slopeb);
                int syt = MaxOffsetFromSlope(slopet);
                int ony = 0;
                int ldyb = 0;
                int ldyt = 0;
                int corndxb = 0;
                int corndxt = 0;

                for (int x = 0; x < 50; x++)
                {


                    //fill the column (assign the verts to this poly)
                    int qib = qi;
                    int qit = qi;
                    int dyb = 0;
                    int dyt = 0;
                    int nyb = ldyb + syb;
                    int nyt = ldyt + syt;
                    for (int yb = 0; yb <= nyb; yb++)
                    {
                        if (!CanPolyIncludeNext(ref vpl, plt, qib, dirpy, out ti)) break;
                        Vertices[ti].PolyID = pid;
                        qib = ti;
                        dyb++;
                    }
                    for (int yt = 0; yt <= nyt; yt++)
                    {
                        if (!CanPolyIncludeNext(ref vpl, plt, qit, dirny, out ti)) break;
                        Vertices[ti].PolyID = pid;
                        qit = ti;
                        dyt++;
                    }


                    //move on to the next column
                    //find the start point (and y offset) for the next column
                    //if none found, can't go further

                    int nxi = qi;
                    bool cgx = CanPolyIncludeNext(ref vpl, plt, qi, dir, out nxi);
                    if (!cgx)
                    {
                        int ybi = qi;
                        for (int yb = 0; yb <= dyb; yb++)
                        {
                            ybi = GetNextID(ybi, dirpy);
                            ony--;
                            if (CanPolyIncludeNext(ref vpl, plt, ybi, dir, out nxi))
                            {
                                cgx = true;
                                break;
                            }
                        }
                    }
                    if (!cgx)
                    {
                        int yti = qi;
                        for (int yt = 0; yt <= dyt; yt++)
                        {
                            yti = GetNextID(yti, dirny);
                            ony++;
                            if (CanPolyIncludeNext(ref vpl, plt, yti, dir, out nxi))
                            {
                                cgx = true;
                                break;
                            }
                        }
                    }
                    if (!cgx)
                    {
                        //can't go further... end of the poly
                        break;
                    }
                    if (nxi < 0)
                    { break; }//shouldn't happen?


                    int nextyb;
                    int nextyt;
                    int nextib = FindPolyEdgeID(ref vpl, plt, nxi, dirpy, out nextyb);
                    int nextit = FindPolyEdgeID(ref vpl, plt, nxi, dirny, out nextyt);

                    //int remyb = nyb - dyb;
                    //int remyt = nyt - dyt;
                    //int compyb = nextyb - ony;
                    //int compyt = nextyt + ony;
                    //int predyb = dyb + syb + ony;
                    //int predyt = dyt + syt - ony;

                    int nextsyb = nextyb - ony - dyb;
                    int nextsyt = nextyt + ony - dyt;

                    corndxb++;
                    corndxt++;

                    bool iscornerb = false;

                    if (slopeb > 1)
                    {
                        if (nextsyb < syb) iscornerb = true;
                        if (nextsyb > syb) nextsyb = syb;
                    }
                    else if (slopeb == 1)
                    {
                        if (nextsyb < syb) iscornerb = true;
                        if (nextsyb > 1) nextsyb = 1;
                    }
                    else if (slopeb > 0)
                    {

                    }
                    else if (slopeb == 0)
                    {
                        if (nextsyb < 0) iscornerb = true;
                        if (nextsyb > 0) nextsyb = 0;
                    }
                    else if (slopeb > -1)
                    {
                    }
                    else if (slopeb == -1)
                    {
                    }
                    else // (slopeb < -1)
                    {
                        if (nextsyb < syb) iscornerb = true;
                        if (nextsyb > syb) nextsyb = syb;
                    }





                    qi = nxi;
                    syb = nextsyb;// nextyb - dyb;
                    syt = nextsyt;// nextyt - dyt;
                    ldyb = dyb;
                    ldyt = dyt;

                    //find top/bottom max dists and limit them according to slope
                    //check if slopes intersect at this column, stop if they do


                }




            }



            private int MaxOffsetFromSlope(float s)
            {
                if (s >= 1) return (int)s;
                if (s > 0) return 1;
                if (s > -1) return 0;
                return -1;

                //return ((s>=1)||(s<=-1))?(int)s : (s>0)?1 : (s<0)?-1 : 0;
            }


            private int GetNextID(int i, int dir)
            {
                switch (dir)
                {
                    default:
                    case 0: return Vertices[i].PrevIDX;
                    case 1: return Vertices[i].PrevIDY;
                    case 2: return Vertices[i].NextIDX;
                    case 3: return Vertices[i].NextIDY;
                }
            }
            private bool CanPolyIncludeNext(ref Plane vplane, float plthresh, int i, int dir, out int ni)
            {
                bool ct;
                switch (dir)
                {
                    default:
                    case 0: ni = Vertices[i].PrevIDX; ct = Vertices[i].CompPrevX; break;
                    case 1: ni = Vertices[i].PrevIDY; ct = Vertices[i].CompPrevY; break;
                    case 2: ni = Vertices[i].NextIDX; ct = Vertices[i].CompNextX; break;
                    case 3: ni = Vertices[i].NextIDY; ct = Vertices[i].CompNextY; break;
                }
                if (ni < 0) return false; //not connected
                if (!ct) return false; //next one is a different type

                if (Vertices[ni].PolyID >= 0)
                { return false; } //already assigned a poly..

                var npdist = Math.Abs(Plane.DotCoordinate(vplane, Vertices[ni].Position));
                return (npdist <= plthresh);
            }

            private int FindPolyEdgeDist(ref Plane vplane, float plthresh, int i, int dir)
            {
                //d: 0=prevX, 1=prevY, 2=nextX, 3=nextY

                //find how many cells are between given vertex(id) and the edge of a poly,
                //in the specified direction

                int dist = 0;
                int ci = i;

                while (dist < 100)
                {
                    int ni;
                    if (!CanPolyIncludeNext(ref vplane, plthresh, ci, dir, out ni)) break;
                    ci = ni;
                    dist++;
                }

                return dist;
            }
            private int FindPolyEdgeID(ref Plane vplane, float plthresh, int i, int dir)
            {
                //d: 0=prevX, 1=prevY, 2=nextX, 3=nextY

                //find the last id of a vertex contained in this poly, starting from i,
                //in the specified direction

                int dist = 0;
                int ci = i;

                while (dist < 100)
                {
                    int ni;
                    if (!CanPolyIncludeNext(ref vplane, plthresh, ci, dir, out ni)) break;
                    ci = ni;
                    dist++;
                }

                return ci;
            }
            private int FindPolyEdgeID(ref Plane vplane, float plthresh, int i, int dir, out int dist)
            {
                //d: 0=prevX, 1=prevY, 2=nextX, 3=nextY

                //find how many cells are between given vertex(id) and the edge of a poly,
                //in the specified direction

                dist = 0;
                int ci = i;

                while (dist < 100)
                {
                    int ni;
                    if (!CanPolyIncludeNext(ref vplane, plthresh, ci, dir, out ni)) break;
                    ci = ni;
                    dist++;
                }

                return ci;
            }



            private float FindSlope(ref Plane vpl, float plt, int i, int dirnx, int dirny, int dirpy, float maxslope)
            {
                //find a slope from the given corner/start point that's less than the max slope

                int ti = i;
                int qi = i;
                float slope = maxslope;
                //int diry = (maxslope > 0) ? dirny : dirpy;
                //int incy = (maxslope > 0) ? 1 : -1;
                int sy = (int)Math.Abs(slope);


                bool cgx = CanPolyIncludeNext(ref vpl, plt, i, dirnx, out ti);


                if (cgx && (slope >= 0)) //new slope should be >=0
                {
                    int dy0 = FindPolyEdgeDist(ref vpl, plt, qi, dirny);
                    int dy1 = FindPolyEdgeDist(ref vpl, plt, ti, dirny);
                    int dy = dy1 - dy0;

                    if (dy1 > 1)
                    {
                        if (dy < 0) return Math.Min(slope, dy1); //can move up to next max
                        if (dy0 > dy) return Math.Min(slope, dy0);//first step was steepest
                        if (dy >= 1) return Math.Min(slope, dy);//second step steeper
                        //only (dy==0)&&(dy0==0) case remaining, shouldn't be possible here
                    }
                    if (dy1 == 1) return Math.Min(slope, 1);//can only go +1Y or slope limit
                    if (dy1 == 0)
                    {
                        //step +X until can't go further, or can step +Y
                        int dx = 1;
                        int xi = ti;//starting from y1
                        while (CanPolyIncludeNext(ref vpl, plt, xi, dirnx, out ti))
                        {
                            xi = ti;
                            dx++;
                            if (CanPolyIncludeNext(ref vpl, plt, xi, dirny, out ti))
                            {
                                //can move +Y now, calc new slope which is >0, <1
                                return Math.Min(slope, 1.0f / dx);
                            }
                        }

                        //couldn't go further +X or +Y...
                        //needs a corner at this next point at slope=0
                        //or could be "trapped" in a corner
                        return Math.Min(slope, 0);//should always return 0..
                    }
                }
                else //new slope must be <0
                {

                    if (!CanPolyIncludeNext(ref vpl, plt, i, dirpy, out ti))
                    {
                        return Math.Min(slope, 0); //can't move -Y.. could only happen at the end
                    }

                    int dx0 = FindPolyEdgeDist(ref vpl, plt, qi, dirnx);
                    int dx1 = FindPolyEdgeDist(ref vpl, plt, ti, dirnx);
                    int dx = dx1 - dx0;

                    if (dx1 > 1)
                    {
                        if (dx < 0) return Math.Min(slope, 0); //end corner, next slope is going backwards
                        if (dx0 > dx) return Math.Min(slope, -1.0f / dx0);//first step went furthest
                        if (dx >= 1) return Math.Min(slope, -1.0f / dx);//second step furthest
                        //only (dx==0)&&(dy0==0) case remaining, shouldn't be possible here
                    }
                    if (dx1 == 1) return Math.Min(slope, -1);
                    if (dx1 == 0)
                    {
                        //step -Y until can't go further, or can step +X
                        int dy = 1;
                        int yi = ti;
                        while(CanPolyIncludeNext(ref vpl, plt, yi, dirpy, out ti))
                        {
                            yi = ti;
                            dy++;
                            if (CanPolyIncludeNext(ref vpl, plt, yi, dirnx, out ti))
                            {
                                //can move +X now, calc new slope for <=-1
                                return Math.Min(slope, -dy);
                            }
                        }

                        //couldn't go further +Y or +X
                        //slope should be negative vertical
                        return Math.Min(slope, -100);

                    }

                }


                return slope;
            }


            private int FindNextCornerID(ref Plane vpl, float plt, int i, int dirnx, int dirny, int dirpy, float slope, out int dx, out int dy)
            {
                dx = 0;
                dy = 0;

                //try to step along the slope until can't go further
                int ti = i;
                int qi = i;
                int mx = 0;
                int my = 0;
                int diry = (slope > 0) ? dirny : dirpy;
                int incy = (slope > 0) ? 1 : -1;
                if ((slope >= 1) || (slope <= -1))
                {
                    int sy = (int)Math.Abs(slope);
                    while (my < sy)
                    {
                        if (CanPolyIncludeNext(ref vpl, plt, qi, diry, out ti))
                        {
                            qi = ti;
                            my++;
                            dy += incy;

                            if (my == sy)
                            {
                                if (CanPolyIncludeNext(ref vpl, plt, qi, dirnx, out ti))
                                {
                                    qi = ti;
                                    my = 0;
                                    mx++;
                                    dx++;
                                }
                                else//can't go further!
                                {
                                    return qi;
                                }
                            }
                        }
                        else if ((mx == 0) && (CanPolyIncludeNext(ref vpl, plt, qi, dirnx, out ti)))
                        {
                            //second chance to make beginning of the line
                            qi = ti;
                            my = 0;
                            mx++;
                            dx++;
                        }
                        else//can't go further!
                        {
                            return qi;
                        }
                    }
                    return qi;//shouldn't get here?
                }
                else if (slope != 0)
                {
                    int sx = (int)Math.Abs(1.0f / slope);
                    while (mx < sx)
                    {
                        if (CanPolyIncludeNext(ref vpl, plt, qi, dirnx, out ti))
                        {
                            qi = ti;
                            mx++;
                            dx++;

                            if (mx == sx)
                            {
                                if (CanPolyIncludeNext(ref vpl, plt, qi, diry, out ti))
                                {
                                    qi = ti;
                                    mx = 0;
                                    my++;
                                    dy += incy;
                                }
                                else//can't go further!
                                {
                                    return qi;
                                }
                            }
                        }
                        else if ((my == 0) && CanPolyIncludeNext(ref vpl, plt, qi, diry, out ti))
                        {
                            //second chance to make beginning of the line
                            qi = ti;
                            mx = 0;
                            my++;
                            dy += incy;
                        }
                        else//can't go further!
                        {
                            return qi;
                        }
                    }
                    return qi;//shouldn't get here?
                }
                else //slope==0
                {
                    for (int x = 0; x < 50; x++) //just try go +X until there's a hit.
                    {
                        if (CanPolyIncludeNext(ref vpl, plt, qi, dirnx, out ti))
                        {
                            qi = ti;
                            dx++;
                        }
                        else
                        {
                            return qi;
                        }
                    }
                    return qi;//could go further, but don't..
                }
            }
        }





        private void GenerateComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { GenerateComplete(); }));
                }
                else
                {
                    GenerateButton.Enabled = true;
                }
            }
            catch { }
        }


        private void UpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    StatusLabel.Text = text;
                }
            }
            catch { }
        }


    }
}
