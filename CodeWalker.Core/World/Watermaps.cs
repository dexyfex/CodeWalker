using CodeWalker.Core.Utils;
using CodeWalker.GameFiles;
using Collections.Pooled;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeWalker.World
{
    public class Watermaps : BasePathData
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        public List<WatermapFile> WatermapFiles = new List<WatermapFile>();


        public Vector4[] GetNodePositions()
        {
            return NodePositions;
        }
        public EditorVertex[] GetTriangleVertices()
        {
            return TriangleVerts;
        }

        public Vector4[] NodePositions = [];
        public EditorVertex[] TriangleVerts = [];


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
            using var _ = new DisposableTimer("Watermaps Init");
            Inited = false;

            GameFileCache = gameFileCache;


            WatermapFiles.Clear();


            LoadWatermap("common.rpf\\data\\levels\\gta5\\waterheight.dat");



            BuildVertices();

            Inited = true;
        }

        private void LoadWatermap(string filename)
        {
            var wmf = GameFileCache.RpfMan.GetFile<WatermapFile>(filename);
            WatermapFiles.Add(wmf);
        }



        public void BuildVertices()
        {

            using var vlist = new PooledList<EditorVertex>();
            using var nlist = new PooledList<Vector4>();

            foreach (var wmf in WatermapFiles)
            {
                BuildWatermapVertices(wmf, vlist, nlist);
            }

            if (vlist.Count > 0)
            {
                TriangleVerts = vlist.ToArray();
            }
            if (nlist.Count > 0)
            {
                NodePositions = nlist.ToArray();
            }

        }
        private void BuildWatermapVertices(WatermapFile wmf, IList<EditorVertex> vl, IList<Vector4> nl)
        {
            EditorVertex v1;
            EditorVertex v2;
            EditorVertex v3;
            EditorVertex v4;

            uint cblu = (uint)new Color(0, 0, 128, 60).ToRgba();


            float getHeight(int o)
            {
                var harr = wmf.GridWatermapRefs[o];
                if (harr == null) return 0;
                if (harr.Length == 0) return 0;
                var h0 = harr[0];
                var i0 = h0.Item;
                if (h0.Type == WatermapFile.WaterItemType.River)
                {
                    return h0.Vector.Z;
                }
                if (h0.Type == WatermapFile.WaterItemType.Lake)
                {
                    if (i0 != null) return i0.Position.Z;
                }
                if (h0.Type == WatermapFile.WaterItemType.Pool)
                {
                    if (i0 != null) return i0.Position.Z;
                }
                return h0.Vector.Z;
            }
            uint getColour(int o)
            {
                var harr = wmf.GridWatermapRefs[o];
                if (harr == null)
                    return cblu;
                if (harr.Length == 0)
                    return cblu;
                var i0 = harr[0].Item;
                if (i0 == null)
                    return cblu;
                var c = i0.Colour;
                c.A = 128;
                return (uint)c.ToRgba();
            }
            //var w = wmf.Width;
            //var h = wmf.Height;
            //var min = new Vector3(wmf.CornerX, wmf.CornerY, 0.0f);
            //var step = new Vector3(wmf.TileX, -wmf.TileY, 1.0f);
            //var siz = new Vector3(w, h, 1) * step;
            //for (int yi = 1; yi < h; yi++)
            //{
            //    var yo = yi - 1;
            //    for (int xi = 1; xi < w; xi++)
            //    {
            //        var xo = xi - 1;
            //        var o1 = yi * w + xo;
            //        var o2 = yi * w + xi;
            //        var o3 = yo * w + xo;
            //        var o4 = yo * w + xi;
            //        v1.Position = min + step * new Vector3(xo, yi, getHeight(o1));
            //        v2.Position = min + step * new Vector3(xi, yi, getHeight(o2));
            //        v3.Position = min + step * new Vector3(xo, yo, getHeight(o3));
            //        v4.Position = min + step * new Vector3(xi, yo, getHeight(o4));
            //        v1.Colour = getColour(o1);
            //        v2.Colour = getColour(o2);
            //        v3.Colour = getColour(o3);
            //        v4.Colour = getColour(o4);
            //        //vl.Add(v1); vl.Add(v2); vl.Add(v3);
            //        //vl.Add(v3); vl.Add(v2); vl.Add(v4);
            //    }
            //}
            //for (int y = 0; y < h; y++)
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        var o = y * w + x;
            //        nl.Add(new Vector4(min + step * new Vector3(x, y, getHeight(o)), 10));
            //    }
            //}


            void addQuad(in Quad q, uint color)
            {
                v1 = new EditorVertex(q.P1, color);
                v2 = new EditorVertex(q.P2, color);
                v3 = new EditorVertex(q.P3, color);
                v4 = new EditorVertex(q.P4, color);
                vl.Add(new EditorVertex()); vl.Add(v2); vl.Add(v3);
                vl.Add(v3); vl.Add(v2); vl.Add(v4);
            }
            void addRivEnd(in Vector3 p, in Vector3 s, in Vector3 d, float r, uint color)
            {
                v1 = new EditorVertex(p, color);
                v2 = new EditorVertex(p + s * r, color);
                v3 = new EditorVertex(p + d * r, color);
                v4 = new EditorVertex(p - s * r, color);
                vl.Add(new EditorVertex()); vl.Add(v2); vl.Add(v3);
                vl.Add(new EditorVertex()); vl.Add(v3); vl.Add(v4);
            }
            var rivers = wmf.Rivers;
            if (rivers != null)
            {
                foreach (var river in rivers)
                {
                    if ((river.Vectors == null) || (river.VectorCount <= 1))
                    { continue; }

                    var rwid = 20.0f;
                    var rc = river.Colour;
                    rc.A = 128;
                    var riverColor = (uint)rc.ToRgba();
                    var quads = new Quad[river.Vectors.Length - 1];
                    var li = river.Vectors.Length - 1;
                    for (int i = 1; i < river.Vectors.Length; i++)
                    {
                        var o = i - 1;
                        var vo = river.Vectors[o];
                        var vi = river.Vectors[i];
                        var dif = vi.XYZ() - vo.XYZ();
                        var dir = Vector3.Normalize(dif);
                        var sid = Vector3.Normalize(Vector3.Cross(dir, Vector3.UnitZ));
                        if (Math.Abs(dir.Z) > 0.95f)
                        {
                            dir = Vector3.UnitY;
                            sid = Vector3.UnitX;
                        }
                        quads[o].P1 = vo.XYZ() - sid*rwid;
                        quads[o].P2 = vo.XYZ() + sid*rwid;
                        quads[o].P3 = vi.XYZ() - sid*rwid;
                        quads[o].P4 = vi.XYZ() + sid*rwid;
                        if (i == 1) addRivEnd(vo.XYZ(), -sid, -dir, rwid, riverColor);
                        if (i == li) addRivEnd(vi.XYZ(), in sid, in dir, rwid, riverColor);
                    }
                    for (int i = 1; i < quads.Length; i++)
                    {
                        var o = i - 1;
                        quads[o].P3 = quads[i].P1 = (quads[o].P3 + quads[i].P1) * 0.5f;
                        quads[o].P4 = quads[i].P2 = (quads[o].P4 + quads[i].P2) * 0.5f;
                    }
                    for (int i = 0; i < quads.Length; i++)
                    {
                        addQuad(in quads[i], riverColor);
                    }
                }
            }
            var lakes = wmf.Lakes;
            if (lakes != null)
            {
                foreach (var lake in lakes)
                {
                    if ((lake.Vectors == null) || (lake.VectorCount == 0))
                    { continue; }

                    var lp = lake.Position;
                    var lc = lake.Colour;
                    lc.A = 128;
                    var lakeColor = (uint)lc.ToRgba();
                    for (int i = 0; i < lake.Vectors.Length; i++)
                    {
                        var vi = lake.Vectors[i];
                        var vp = new Vector3(vi.X, vi.Y, lp.Z);
                        var q = new Quad
                        {
                            P1 = vp + new Vector3(vi.Z, -vi.W, 0),
                            P2 = vp + new Vector3(vi.Z, vi.W, 0),
                            P3 = vp + new Vector3(-vi.Z, -vi.W, 0),
                            P4 = vp + new Vector3(-vi.Z, vi.W, 0),
                        };
                        addQuad(in q, lakeColor);
                    }
                }
            }
            var pools = wmf.Pools;
            if (pools != null)
            {
                foreach (var pool in pools)
                {
                    var pp = pool.Position;
                    var ps = pool.Size;
                    var pc = pool.Colour;
                    pc.A = 128;
                    var q = new Quad
                    {
                        P1 = pp + new Vector3(ps.X, -ps.Y, 0),
                        P2 = pp + new Vector3(ps.X, ps.Y, 0),
                        P3 = pp + new Vector3(-ps.X, -ps.Y, 0),
                        P4 = pp + new Vector3(-ps.X, ps.Y, 0)
                    };
                    addQuad(in q, (uint)pc.ToRgba());
                }
            }


        }



        struct Quad
        {
            public Vector3 P1;
            public Vector3 P2;
            public Vector3 P3;
            public Vector3 P4;
        }
    }
}
