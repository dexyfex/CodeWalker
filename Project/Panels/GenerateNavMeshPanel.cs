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


            GenerateButton.Enabled = false;



            float density = 0.5f; //distance between vertices for the initial grid
            //float clipdz = 0.5f; //any polygons with greater steepness should be removed


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

                var polys = new List<GenPoly>();

                var vertexCountXY = (max - min) / density;
                int vertexCountX = (int)vertexCountXY.X+1;
                int vertexCountY = (int)vertexCountXY.Y+1;
                //int vertexCountTot = vertexCountX * vertexCountY;
                vgrid.BeginGrid(vertexCountX, vertexCountY);

                Ray ray = new Ray(Vector3.Zero, new Vector3(0, 0, -1));//for casting with

                UpdateStatus("Loading YBNs...");

                var bmin = new Vector3(min, 0);
                var bmax = new Vector3(max, 0);
                var boundslist = space.BoundsStore.GetItems(ref bmin, ref bmax);

                //pre-warm the bounds cache for this area, and find the min/max Z
                foreach (var boundsitem in boundslist)
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
                            ybn = gameFileCache.GetYbn(boundsitem.Name); //try queue it again..
                        }
                    }
                    if (ybn.Loaded && (ybn.Bounds != null))
                    {
                        bmin.Z = Math.Min(bmin.Z, ybn.Bounds.BoundingBoxMin.Z);
                        bmax.Z = Math.Max(bmax.Z, ybn.Bounds.BoundingBoxMax.Z);
                    }
                }



                //ray-cast each XY vertex position, and find the height and surface from ybn's
                //continue casting down to find more surfaces...

                UpdateStatus("Processing...");

                for (int vx = 0; vx < vertexCountX; vx++)
                {
                    for (int vy = 0; vy < vertexCountY; vy++)
                    {
                        vgrid.BeginCell(vx, vy);
                        var vcoffset = new Vector3(vx, vy, 0) * density;
                        ray.Position = bmin + vcoffset;
                        ray.Position.Z = bmax.Z + 1.0f;//start the ray at the top of the cell
                        var intres = space.RayIntersect(ray, float.MaxValue, layers);
                        hitTestCount++;
                        while (intres.Hit)// && (intres.HitDist > 0))
                        {
                            if (intres.HitDist > 0)
                            {
                                hitCount++;
                                vert.Position = intres.Position;
                                vert.Normal = intres.Normal;
                                vert.Material = intres.Material.Type;
                                vert.PolyFlags = (ushort)intres.Material.Flags;
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

                                if (vgrid.CurVertexCount > 15) //too many hits?
                                { break; }
                            }
                            //continue down until no more hits..... step by 3m
                            ray.Position.Z = intres.Position.Z - 3.0f;
                            intres = space.RayIntersect(ray, float.MaxValue, layers);
                        }
                        vgrid.EndCell(vx, vy);
                    }
                }

                vgrid.EndGrid(); //build vertex array





                vgrid.ConnectVertices();


                var genPolys = vgrid.GenPolys2();
                polys.AddRange(genPolys);

                newCount += genPolys.Count;


















                //try merge generated polys into bigger ones, while keeping convex!
                UpdateStatus("Building edge dictionary...");
                var edgeDict = new Dictionary<GenEdgeKey, GenEdge>();
                var tryGetEdge = new Func<Vector3, Vector3, GenEdge>((v1, v2) => 
                {
                    var key1 = new GenEdgeKey(v1, v2);
                    var key2 = new GenEdgeKey(v2, v1);
                    GenEdge edge = null;
                    if (edgeDict.TryGetValue(key1, out edge) || edgeDict.TryGetValue(key2, out edge))
                    {
                        return edge;
                    }
                    return null;
                });
                var tryRemoveEdge = new Action<Vector3, Vector3>((v1, v2) =>
                {
                    var key1 = new GenEdgeKey(v1, v2);
                    var key2 = new GenEdgeKey(v2, v1);
                    edgeDict.Remove(key1);
                    edgeDict.Remove(key2);
                });
                var buildEdgeDict = new Action(() =>
                {
                    for (int p = 0; p < polys.Count; p++) //build edge dict
                    {
                        var poly = polys[p];
                        poly.Index = p;
                        for (int i = 0; i < poly.Vertices.Length; i++)
                        {
                            var ip = (i + 1) % poly.Vertices.Length;
                            var edge = tryGetEdge(poly.Vertices[i], poly.Vertices[ip]);
                            if (edge != null)
                            {
                                if (edge.Poly2 != null)
                                { } //edge already assigned a second poly! shouldn't happen...
                                edge.Poly2 = poly;
                                edge.EdgeIndex2 = i;
                            }
                            else
                            {
                                var key = new GenEdgeKey(poly.Vertices[i], poly.Vertices[ip]);
                                edge = new GenEdge(poly, i);
                                edgeDict[key] = edge;
                            }
                        }
                    }
                });
                buildEdgeDict();

                UpdateStatus("Merging polygons...");
                float plthresh = 0.3f;//threshold for plane dist test
                float dpthresh = 0.75f;//threshold for plane normals test
                float dthresh = 6.0f;//absolute distance thresh
                for (int p = 0; p < polys.Count; p++)
                {
                    //UpdateStatus("Merging polygons... (" + p.ToString() + "/" + polys.Count.ToString() + ")");
                    var poly = polys[p];
                    if (poly == null) continue;
                    if (poly.Merged) continue;
                    var p1cnt = poly.Vertices.Length;
                    if (p1cnt < 3) continue;
                    var vplane = new Plane(poly.Vertices[0], poly.Normal);

                    var polycenter = poly.GetCenter();

                    for (int i = 0; i < poly.Vertices.Length; i++)
                    {
                        var ip = (i + 1) % poly.Vertices.Length;
                        var eind1 = i;
                        var edge = tryGetEdge(poly.Vertices[i], poly.Vertices[ip]);
                        if (edge == null) continue;
                        var poly2 = edge.Poly1;
                        var eind2 = edge.EdgeIndex1;
                        if (poly2 == poly) { poly2 = edge.Poly2; eind2 = edge.EdgeIndex2; }
                        if (poly2 == poly) continue;//can't merge with itself! redundant edges/verts...
                        if (poly2 == null) continue;
                        if (poly2.Merged) continue;//new merged poly will get checked later..
                        if (poly.Material.Index != poly2.Material.Index) continue;
                        if (poly.PolyFlags != poly2.PolyFlags) continue;

                        var poly2center = poly2.GetCenter();

                        var npdist = Math.Abs(Plane.DotCoordinate(vplane, poly2center));
                        if (npdist > plthresh) continue;
                        var dpval = Vector3.Dot(poly.Normal, poly2.Normal);
                        if (dpval < dpthresh) continue;
                        var dist = (polycenter - poly2center).Length();
                        if (dist > dthresh) continue;


                        //if we got here, can merge these 2 polys....


                        var newverts = new List<Vector3>();
                        //add verts from poly1 from 0 to poly1 edge index (ip)
                        //add verts from poly2 from poly2 edge index+2 to poly2 edge index (wrap/mod!)
                        //add verts from poly1 from poly1 edge index+2 to last
                        var p2cnt = poly2.Vertices.Length;
                        var l2beg = (eind2 + 2) % p2cnt;
                        var l2end = eind2;
                        if (l2end < l2beg) l2end += p2cnt;
                        var l1beg = (eind1 + 2);
                        if (l1beg > p1cnt) l2end--;//don't add the first vertex again in this case!
                        for (int j = 0; j <= eind1; j++) newverts.Add(poly.Vertices[j]);
                        for (int j = l2beg; j <= l2end; j++) newverts.Add(poly2.Vertices[j % p2cnt]);
                        for (int j = l1beg; j < p1cnt; j++) newverts.Add(poly.Vertices[j]);




                        var varr = newverts.ToArray();
                        var remredun = true;
                        while (remredun)
                        {
                            remredun = false;
                            newverts.Clear(); // remove redundant edges!
                            for (int j = 0; j < varr.Length; j++)
                            {
                                var j0 = j - 1; if (j0 < 0) j0 += varr.Length;
                                var j2 = j + 1; j2 = j2 % varr.Length;
                                var v0 = varr[j0];
                                var v1 = varr[j];
                                var v2 = varr[j2];
                                if (v0 == v2)
                                {
                                    if (j2 > j)
                                    {
                                        j = j2;
                                    }
                                    else
                                    {
                                        if (j == varr.Length - 1)
                                        {
                                            newverts = newverts.GetRange(0, newverts.Count - 1);
                                        }
                                        else
                                        { }
                                        j = varr.Length;
                                    }
                                    remredun = true;
                                }
                                else
                                {
                                    newverts.Add(v1);
                                }
                            }
                            varr = newverts.ToArray();
                            if (remredun)
                            { }
                        }





                        var newpoly = new GenPoly(newverts.ToArray(), poly);
                        newpoly.Index = polys.Count;
                        polys.Add(newpoly);//try merge this poly again later...


                        //for all the edges in this new poly, need to update all the values!!! polys and indices!
                        for (int j = 0; j < newpoly.Vertices.Length; j++)
                        {
                            var jp = (j + 1) % newpoly.Vertices.Length;
                            var v = newpoly.Vertices[j];
                            var vp = newpoly.Vertices[jp];
                            var tedge = tryGetEdge(v, vp);
                            if (tedge == null)
                            { continue; }//shouldn't happen..
                            if (tedge.Poly1 == poly) { tedge.Poly1 = newpoly; tedge.EdgeIndex1 = j; }
                            if (tedge.Poly2 == poly) { tedge.Poly2 = newpoly; tedge.EdgeIndex2 = j; }
                            if (tedge.Poly1 == poly2) { tedge.Poly1 = newpoly; tedge.EdgeIndex1 = j; }
                            if (tedge.Poly2 == poly2) { tedge.Poly2 = newpoly; tedge.EdgeIndex2 = j; }
                            if (tedge.Poly1 == tedge.Poly2)
                            { } //why does this happen..? probably when an edge can't be removed due to an enclosed poly
                        }

                        //tryRemoveEdge(poly.Vertices[i], poly.Vertices[ip]);
                        polys[p] = null;//free up some memory..?
                        polys[poly2.Index] = null;
                        poly.Merged = true;
                        poly2.Merged = true;
                        break;//go to the next poly: don't do more than 1 merge at a time..
                    }

                }

                var mergedPolys = new List<GenPoly>();
                foreach (var poly in polys)
                {
                    if (poly == null) continue;
                    if (poly.Merged) continue;
                    mergedPolys.Add(poly);
                }
                polys = mergedPolys;

                
                UpdateStatus("Merging edges...");
                edgeDict = new Dictionary<GenEdgeKey, GenEdge>();
                buildEdgeDict();
                float dpthresh1 = 0.5f;
                float dpthresh2 = 0.7f; //try preserve shape more when not attached
                foreach (var poly in polys)
                {
                    if (poly?.Vertices == null) continue;
                    if (poly.Vertices.Length < 5) continue;
                    for (int i = 1; i < poly.Vertices.Length; i++)
                    {
                        var ni = i - 1;
                        var edge0 = tryGetEdge(poly.Vertices[ni], poly.Vertices[i]);
                        if (edge0 == null)
                        { continue; }//really shouldn't happen
                        var poly0 = (edge0.Poly1 != poly) ? edge0.Poly1 : edge0.Poly2;
                        var vert0 = poly.Vertices[ni];

                        var ip = (i + 1) % poly.Vertices.Length;
                        var ip2 = (i + 2) % poly.Vertices.Length;
                        var edge1 = tryGetEdge(poly.Vertices[i], poly.Vertices[ip]);
                        if (edge1 == null)
                        { continue; }//really shouldn't happen
                        var poly1 = (edge1.Poly1 != poly) ? edge1.Poly1 : edge1.Poly2;
                        var vert1 = poly.Vertices[ip];
                        var verti = poly.Vertices[i];
                        var vert2 = poly.Vertices[ip2];
                        var dp = Vector3.Dot(Vector3.Normalize(verti - vert0), Vector3.Normalize(vert2 - verti));
                        var dp2 = Vector3.Dot(Vector3.Normalize(verti - vert0), Vector3.Normalize(vert1 - verti));

                        var usedpthresh = ((poly0 == null) || (poly0 == poly)) ? dpthresh2 : dpthresh1;

                        if ((poly0 != poly1) || (dp < usedpthresh) || (dp2 < -0.05))//can't merge, move on to next edge
                        { continue; }


                        if ((poly0 != null) && (poly0.Vertices.Length < 5))
                        { continue; }

                        //remove the relevant vertex from both polys, and start again for this poly (reset i to 1)
                        poly.RemoveVertex(verti);
                        poly0?.RemoveVertex(verti);//if poly0==poly, remove same vertex twice?

                        
                        //remove merged edges from edge dict, and add new edge to it
                        tryRemoveEdge(vert0, verti);
                        tryRemoveEdge(verti, vert1);

                        var key = new GenEdgeKey(vert0, vert1);
                        var edge = new GenEdge(poly, i-1);
                        edge.Poly2 = poly0;
                        edge.EdgeIndex2 = poly0?.FindVertex(vert0) ?? -1; //(edge0.Poly2 != poly0) ? edge0.EdgeIndex1 : edge0.EdgeIndex2;
                        edgeDict[key] = edge;

                        i = 0;//will be incremented to 1 before next loop
                        if (poly.Vertices.Length < 5) break;//don't make polys disappear! shouldn't happen anyway
                    }
                }


                
                UpdateStatus("Convexifying polygons...");
                mergedPolys = new List<GenPoly>();
                var getAngle = new Func<GenPoly, int, int, float>((poly, i1, i2) => 
                {
                    var edge0 = poly.Vertices[i2] - poly.Vertices[i1];
                    return (float)Math.Atan2(edge0.Y, edge0.X);
                });
                var getAngleDiff = new Func<float, float, float>((a1, a2) => 
                {
                    var angldiff = a2 - a1;
                    if (angldiff > Math.PI) angldiff -= (float)(Math.PI * 2);
                    if (angldiff < -Math.PI) angldiff += (float)(Math.PI * 2);
                    return angldiff;
                });
                var findInflection = new Func<GenPoly, int, int>((poly, starti) => 
                {
                    var vcnt = poly.Vertices.Length;
                    var i0 = starti % vcnt;
                    var i1 = (i0 + 1) % vcnt;
                    var angl0 = getAngle(poly, i0, i1);
                    var curangl = angl0;
                    for (int i = starti+1; i <= vcnt; i++)
                    {
                        i0 = i % vcnt;
                        i1 = (i0 + 1) % vcnt;
                        angl0 = getAngle(poly, i0, i1);
                        var angldiff = getAngleDiff(curangl, angl0);
                        if (angldiff < 0)
                        {
                            return i0;
                        }
                        curangl = angl0;
                    }
                    return -1;
                });
                var findIntersection = new Func<GenPoly, int, int, int>((poly, i0, i1) => 
                {
                    var vcnt = poly.Vertices.Length;
                    var v0 = poly.Vertices[i0];
                    var v1 = poly.Vertices[i1];
                    var minx0 = Math.Min(v0.X, v1.X);
                    var miny0 = Math.Min(v0.Y, v1.Y);
                    var maxx0 = Math.Max(v0.X, v1.X);
                    var maxy0 = Math.Max(v0.Y, v1.Y);
                    for (int i = 1; i < vcnt; i++)
                    {
                        var i2 = (i + i0) % vcnt;
                        var i3 = (i2 + 1) % vcnt;
                        if (i3 == i1) break;
                        var v2 = poly.Vertices[i2];
                        var v3 = poly.Vertices[i3];

                        if ((v0 == v2) || (v0 == v3) || (v1 == v2) || (v1 == v3)) continue; //don't test if sharing a vertex.

                        //https://rosettacode.org/wiki/Find_the_intersection_of_two_lines
                        float a1 = v1.Y - v0.Y;
                        float b1 = v0.X - v1.X;
                        float c1 = a1 * v0.X + b1 * v0.Y;
                        float a2 = v3.Y - v2.Y;
                        float b2 = v2.X - v3.X;
                        float c2 = a2 * v2.X + b2 * v2.Y;
                        float delta = a1 * b2 - a2 * b1;
                        if (delta != 0)
                        {
                            var deltai = 1.0f / delta;
                            var vix = (b2 * c1 - b1 * c2) * deltai;
                            var viy = (a1 * c2 - a2 * c1) * deltai;

                            var minx1 = Math.Min(v2.X, v3.X);
                            var miny1 = Math.Min(v2.Y, v3.Y);
                            var maxx1 = Math.Max(v2.X, v3.X);
                            var maxy1 = Math.Max(v2.Y, v3.Y);

                            if ((vix >= minx0) && (vix >= minx1) && (vix <= maxx0) && (vix <= maxx1) &&
                                (viy >= miny0) && (viy >= miny1) && (viy <= maxy0) && (viy <= maxy1))
                            {
                                return i2;
                            }
                        }
                    }
                    return -1;
                });
                var findConvexSplit = new Func<GenPoly, int, int>((poly, starti) => 
                {
                    var vcnt = poly.Vertices.Length;


                    //step backwards to find a valid split
                    var i0 = starti - 1; if (i0 < 0) i0 += vcnt;
                    var curangl = getAngle(poly, i0, starti);
                    var prevangl = curangl;
                    var iok = starti - 2; if (iok < 0) iok += vcnt;
                    var anyok = false;
                    for (int i = -2; i >= -vcnt; i--)
                    {
                        var i1 = i + starti; if (i1 < 0) i1 += vcnt; //i1 = i1 % vcnt;
                        var angl0 = getAngle(poly, starti, i1);
                        var angldiff0 = getAngleDiff(curangl, angl0);
                        if (angldiff0 < 0)
                        {
                            break;//split line would not be convex at starti
                        }
                        var i2 = (i1 + 1) % vcnt;
                        var angl1 = getAngle(poly, i1, i2);
                        var angldiff1 = getAngleDiff(angl0, angl1);
                        if (angldiff1 < 0)
                        {
                            break;//split line would not be convex at i1
                        }
                        var angl2 = getAngle(poly, i1, i2);
                        var angldiff2 = getAngleDiff(angl2, prevangl);
                        if (angldiff2 < 0)
                        {
                            break;//this step back is not convex
                        }
                        var inti = findIntersection(poly, starti, i1);
                        if (inti >= 0)
                        {
                            break;//split line intersects a poly edge!
                        }
                        prevangl = angl2;
                        anyok = true;
                        iok = i1;
                    }
                    if (anyok)
                    {
                        return iok;
                    }



                    //couldn't split by stepping backwards... so try split by stepping forwards!
                    i0 = (starti + 1) % vcnt;
                    curangl = getAngle(poly, starti, i0);
                    prevangl = curangl;
                    iok = (starti + 2) % vcnt;
                    for (int i = 2; i <= vcnt; i++)
                    {
                        var i1 = (i + starti) % vcnt;
                        var angl0 = getAngle(poly, i1, starti);
                        var angldiff0 = getAngleDiff(angl0, curangl);
                        if (angldiff0 < 0)
                        {
                            break;//split line would not be convex at starti
                        }
                        var i2 = (i1 - 1); if (i2 < 0) i2 += vcnt;
                        var angl1 = getAngle(poly, i2, i1);
                        var angldiff1 = getAngleDiff(angl1, angl0);
                        if (angldiff1 < 0)
                        {
                            break;//split line would not be convex at i1
                        }
                        var angl2 = getAngle(poly, i2, i1);
                        var angldiff2 = getAngleDiff(prevangl, angl2);
                        if (angldiff2 < 0)
                        {
                            break;//this step forward is not convex..
                        }
                        var inti = findIntersection(poly, i1, starti);
                        if (inti >= 0)
                        {
                            break;//split line intersects poly edge!
                        }
                        prevangl = angl2;
                        anyok = true;
                        iok = i1;
                    }
                    if (anyok)
                    {
                        return iok | 0x40000000;//set this flag to indicate polys got switched
                    }



                    //can't go either way... what now?
                    { }




                    return -1;
                });
                foreach (var poly in polys)
                {
                    if (poly?.Vertices == null) continue;

                    var infi = findInflection(poly, 0);
                    var infi1 = infi;
                    //bool split = false;

                    while (infi >= 0)
                    {
                        //split = true;
                        var convi = findConvexSplit(poly, infi);
                        if (convi >= 0)
                        {
                            var flag = 0x40000000;
                            var reversed = (convi & flag) == flag;
                            convi = convi & 0x3FFFFFFF;//mask out that flag (don't care about sign bit)

                            //make a new poly, starting at convi and ending at spliti
                            var newverts = new List<Vector3>();
                            var vcnt = poly.Vertices.Length;
                            var endi = infi;
                            if (endi < convi) endi += vcnt;
                            for (int i = convi; i <= endi; i++)
                            {
                                var i0 = i % vcnt;
                                newverts.Add(poly.Vertices[i0]);
                            }
                            var varr1 = newverts.ToArray();

                            //remove the clipped vertices from the current poly
                            newverts.Clear();
                            if (convi < endi) convi += vcnt; 
                            for (int i = endi; i <= convi; i++)
                            {
                                var i0 = i % vcnt;
                                newverts.Add(poly.Vertices[i0]);
                            }
                            var varr2 = newverts.ToArray();


                            var newpoly = new GenPoly((reversed ? varr2 : varr1), poly);
                            newpoly.Index = mergedPolys.Count;
                            mergedPolys.Add(newpoly);

                            poly.Vertices = (reversed ? varr1 : varr2);

                            infi = findInflection(poly, 0);
                            infi1 = infi;
                        }
                        else
                        {
                            //couldn't split at this inflection point, move on to the next...
                            var infi2 = findInflection(poly, infi);
                            if (infi2 != infi1)
                            {
                                infi = infi2;
                            }
                            else
                            {
                                infi = -1;//don't get stuck in the loop!
                            }
                        }
                    }
                    //if (split) continue;
                    //else
                    //{ } //poly is already convex..






                    poly.Index = mergedPolys.Count;
                    mergedPolys.Add(poly);
                }

                polys = mergedPolys;
                edgeDict = new Dictionary<GenEdgeKey, GenEdge>();
                buildEdgeDict();

                newCount = polys.Count;





                UpdateStatus("Building YNVs...");
                foreach (var poly in polys)
                {
                    if (poly.Vertices == null) continue;

                    var ypoly = builder.AddPoly(poly.Vertices);
                    if (ypoly == null)
                    { continue; }


                    //TODO: add poly edges!

                    ypoly.B02_IsFootpath = (poly.Material.Index == 1);

                    ypoly.B18_IsRoad = (poly.Material.Index == 4);//4,5,6


                }




                var ynvs = builder.Build(false);//todo:vehicles!





                UpdateStatus("Creating YNV files...");

                var path = ProjectForm.CurrentProjectFile?.GetFullFilePath("navmeshes") + "\\";
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


                    ProjectForm.Invoke((MethodInvoker)delegate
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

        private struct GenEdgeKey
        {
            public Vector3 V1;
            public Vector3 V2;
            public GenEdgeKey(Vector3 v1, Vector3 v2)
            {
                V1 = v1;
                V2 = v2;
            }

            //public int V1X;
            //public int V1Y;
            //public int V1Z;
            //public int V2X;
            //public int V2Y;
            //public int V2Z;
            //public GenEdgeKey(Vector3 v1, Vector3 v2)
            //{
            //    V1X = (int)(v1.X * 100);
            //    V1Y = (int)(v1.Y * 100);
            //    V1Z = (int)(v1.Z * 100);
            //    V2X = (int)(v2.X * 100);
            //    V2Y = (int)(v2.Y * 100);
            //    V2Z = (int)(v2.Z * 100);
            //}

        }

        private class GenEdge
        {
            public GenPoly Poly1;
            public GenPoly Poly2;
            public int EdgeIndex1;
            public int EdgeIndex2;
            public GenEdge(GenPoly p1, int e1)
            {
                Poly1 = p1;
                EdgeIndex1 = e1;
            }
            public GenEdge(GenPoly p1, GenPoly p2, int e1, int e2)
            {
                Poly1 = p1;
                Poly2 = p2;
                EdgeIndex1 = e1;
                EdgeIndex2 = e2;
            }
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

            public bool Merged = false;


            public GenPoly() { }
            public GenPoly(Vector3[] verts, ref GenVertex vert)
            {
                Vertices = verts;
                Normal = vert.Normal;
                Material = vert.Material;
                PolyFlags = vert.PolyFlags;
            }
            public GenPoly(Vector3[] verts, GenPoly orig)
            {
                Vertices = verts;
                Normal = orig.Normal;
                Material = orig.Material;
                PolyFlags = orig.PolyFlags;
            }

            public Vector3 GetCenter()
            {
                var c = Vector3.Zero;
                if (Vertices?.Length > 0)
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        c += Vertices[i];
                    }
                    c /= Vertices.Length;
                }
                return c;
            }

            public int FindVertex(Vector3 v)
            {
                if (Vertices != null)
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        if (Vertices[i] == v) return i;
                    }
                }
                return -1;
            }
            public void RemoveVertex(Vector3 v)
            {
                var newverts = new List<Vector3>();
                bool removed = false;
                if (Vertices != null)
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        if (Vertices[i] == v)
                        {
                            removed = true;
                        }
                        else
                        {
                            newverts.Add(Vertices[i]);
                        }
                    }
                }
                if (removed)
                {
                    Vertices = newverts.ToArray();
                }
                else
                { }//probably shouldn't happen
            }
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
            private List<Vector3> VerticesB = new List<Vector3>();
            private List<Vector3> VerticesT = new List<Vector3>();

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
                var connectThresh = 0.4f;
                var density = 0.5f;//to match vertex density (x/y distance)
                for (int vx = 0; vx < VertexCountX; vx++)
                {
                    int px = vx - 1;
                    for (int vy = 0; vy < VertexCountY; vy++)
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
                            var prevIDX = (px < 0) ? -1 : FindVertex(px, vy, vxz, connectThresh);
                            var prevIDY = (py < 0) ? -1 : FindVertex(vx, py, vyz, connectThresh);
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



                            {
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
                //int corndxb = 0;
                //int corndxt = 0;

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

                    //corndxb++;
                    //corndxt++;

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

                    if (iscornerb) { }



                    qi = nxi;
                    syb = nextsyb;// nextyb - dyb;
                    syt = nextsyt;// nextyt - dyt;
                    ldyb = dyb;
                    ldyt = dyt;

                    //find top/bottom max dists and limit them according to slope
                    //check if slopes intersect at this column, stop if they do


                }




            }
            private void AssignVertices3(ref Plane vpl, float plt, int i, int dir, GenPoly poly)
            {
                int pid = poly.Index;
                int qi = i;
                CornersB.Clear();
                CornersT.Clear();
                VerticesB.Clear();
                VerticesT.Clear();


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


                //find the bottom and top leftmost points to start the first col, and fill the col
                int qib = qi;
                int qit = qi;
                int dyb = 0;
                int dyt = 0;
                while (CanPolyIncludeNext(ref vpl, plt, qib, dirpy, out ti))
                {
                    Vertices[ti].PolyID = pid;
                    qib = ti;
                    dyb++;
                }
                while (CanPolyIncludeNext(ref vpl, plt, qit, dirny, out ti))
                {
                    Vertices[ti].PolyID = pid;
                    qit = ti;
                    dyt++;
                }
                int dy = dyb + dyt; //total distance between bottom and top

                CornersB.Add(qib);
                CornersT.Add(qit);



                //find bottom and top slopes
                float slopeb = FindSlope(ref vpl, plt, qib, dir, dirpy, dirny, dyb > 0 ? dyb : 100);
                float slopet = FindSlope(ref vpl, plt, qit, dir, dirny, dirpy, dyt > 0 ? dyt : 100);
                int syob = MaxOffsetFromSlope(slopeb);
                int syot = MaxOffsetFromSlope(slopet);

                //find the next bottom and top indexes, step by the max offset
                int nqib = qib;
                int nqit = qit;
                //int ndyb = 0;
                //int ndyt = 0;




            }














            public List<GenPoly> GenPolys2()
            {
                List<GenPoly> polys = new List<GenPoly>();

                //do marching squares on the grid, assuming each vertex starts a cell

                for (int vx = 0; vx < VertexCountX; vx++)
                {
                    for (int vy = 0; vy < VertexCountY; vy++)
                    {
                        int imin = VertexOffsets[vx, vy];
                        int imax = VertexCounts[vx, vy] + imin;
                        for (int i = imin; i < imax; i++)
                        {
                            var nidx = Vertices[i].NextIDX;
                            var nidy = Vertices[i].NextIDY;
                            var nidxy = -1;
                            var nidyx = -1;


                            if ((nidx < 0) || (nidy < 0)) continue; //(can't form a square... try with less verts?)

                            //try to find the index of the opposite corner... 
                            //there's 2 possibilities, can only form the square if they are both the same...
                            //what to do if they're different..? just choose one?

                            nidxy = Vertices[nidx].NextIDY;
                            nidyx = Vertices[nidy].NextIDX;

                            if (nidxy != nidyx)
                            { }


                            if (nidxy == -1)
                            {
                                if (nidyx == -1)
                                { continue; } //can't form a square! could use the 3?
                                nidxy = nidyx;
                            }


                            bool f0 = CompareVertexTypes(i, nidx);
                            bool f1 = CompareVertexTypes(nidx, nidxy);
                            bool f2 = CompareVertexTypes(nidy, nidxy);
                            bool f3 = CompareVertexTypes(i, nidy);
                            //bool f4 = CompareVertexTypes(i, nidxy); //diagonal
                            //bool f5 = CompareVertexTypes(nidx, nidy); //diagonal


                            var v0 = Vertices[i];
                            var v1 = Vertices[nidx];
                            var v2 = Vertices[nidxy];
                            var v3 = Vertices[nidy];
                            var p0 = v0.Position;
                            var p1 = v1.Position;
                            var p2 = v2.Position;
                            var p3 = v3.Position;

                            var s0 = (p0 + p1) * 0.5f; //edge splits
                            var s1 = (p1 + p2) * 0.5f;
                            var s2 = (p2 + p3) * 0.5f;
                            var s3 = (p3 + p0) * 0.5f;
                            var sc = (s0 + s2) * 0.5f;//square center



                            var id = (f0 ? 8 : 0) + (f1 ? 4 : 0) + (f2 ? 2 : 0) + (f3 ? 1 : 0);
                            switch (id)
                            {
                                case 15: //all corners same
                                    polys.Add(new GenPoly(new[] { p0, p1, p2, p3 }, ref v0));
                                    break;
                                case 3://single split cases
                                    polys.Add(new GenPoly(new[] { s0, p1, s1 }, ref v1));
                                    polys.Add(new GenPoly(new[] { s1, p2, p3, p0, s0 }, ref v2));
                                    break;
                                case 5:
                                    polys.Add(new GenPoly(new[] { s0, p1, p2, s2 }, ref v1));
                                    polys.Add(new GenPoly(new[] { s2, p3, p0, s0 }, ref v3));
                                    break;
                                case 6:
                                    polys.Add(new GenPoly(new[] { s0, p1, p2, p3, s3 }, ref v1));
                                    polys.Add(new GenPoly(new[] { s3, p0, s0 }, ref v0));
                                    break;
                                case 9:
                                    polys.Add(new GenPoly(new[] { s1, p2, s2 }, ref v2));
                                    polys.Add(new GenPoly(new[] { s2, p3, p0, p1, s1 }, ref v3));
                                    break;
                                case 10:
                                    polys.Add(new GenPoly(new[] { s1, p2, p3, s3 }, ref v2));
                                    polys.Add(new GenPoly(new[] { s3, p0, p1, s1 }, ref v0));
                                    break;
                                case 12:
                                    polys.Add(new GenPoly(new[] { s2, p3, s3 }, ref v3));
                                    polys.Add(new GenPoly(new[] { s3, p0, p1, p2, s2 }, ref v0));
                                    break;
                                case 1://double split cases
                                    polys.Add(new GenPoly(new[] { p0, s0, sc, s2, p3 }, ref v0));
                                    polys.Add(new GenPoly(new[] { p1, s1, sc, s0 }, ref v1));
                                    polys.Add(new GenPoly(new[] { p2, s2, sc, s1 }, ref v2));
                                    break;
                                case 2:
                                    polys.Add(new GenPoly(new[] { p0, s0, sc, s3 }, ref v0));
                                    polys.Add(new GenPoly(new[] { p1, s1, sc, s0 }, ref v1));
                                    polys.Add(new GenPoly(new[] { p2, p3, s3, sc, s1 }, ref v2));
                                    break;
                                case 4:
                                    polys.Add(new GenPoly(new[] { p0, s0, sc, s3 }, ref v0));
                                    polys.Add(new GenPoly(new[] { p1, p2, s2, sc, s0 }, ref v1));
                                    polys.Add(new GenPoly(new[] { p3, s3, sc, s2 }, ref v3));
                                    break;
                                case 8:
                                    polys.Add(new GenPoly(new[] { p0, p1, s1, sc, s3 }, ref v0));
                                    polys.Add(new GenPoly(new[] { p2, s2, sc, s1 }, ref v2));
                                    polys.Add(new GenPoly(new[] { p3, s3, sc, s2 }, ref v3));
                                    break;
                                case 0: //all corners different? maybe check diagonals?
                                    polys.Add(new GenPoly(new[] { p0, s0, sc, s3 }, ref v0));
                                    polys.Add(new GenPoly(new[] { p1, s1, sc, s0 }, ref v1));
                                    polys.Add(new GenPoly(new[] { p2, s2, sc, s1 }, ref v2));
                                    polys.Add(new GenPoly(new[] { p3, s3, sc, s2 }, ref v3));
                                    break;
                                default://shouldn't happen?
                                    break;
                            }
                        


                        }
                    }
                }



                return polys;

            }












            private int FindNextID(ref Plane vpl, float plt, int i, int dirnx, int dirny, int dirpy, float slope, out int dx, out int dy)
            {
                //find the next vertex along the slope in the given direction

                int ti = i;
                int qi = i;

                bool cgx = CanPolyIncludeNext(ref vpl, plt, i, dirnx, out ti);




                dx = 0;
                dy = 0;
                return i;
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
                if ((i < 0) || (i >= Vertices.Length))
                {
                    ni = -1;
                    return false;
                }
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
                    BeginInvoke(new Action(() => { UpdateStatus(text); }));
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
