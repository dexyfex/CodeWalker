using CodeWalker.GameFiles;
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
        public EditorVertex[] GetPathVertices()
        {
            return null;
        }
        public EditorVertex[] GetTriangleVertices()
        {
            return TriangleVerts;
        }

        public Vector4[] NodePositions;
        public EditorVertex[] TriangleVerts;


        public void Init(GameFileCache gameFileCache, Action<string> updateStatus)
        {
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

            var vlist = new List<EditorVertex>();
            var nlist = new List<Vector4>();

            foreach (var wmf in WatermapFiles)
            {
                BuildWatermapVertices(wmf, vlist, nlist);
            }

            if (vlist.Count > 0)
            {
                TriangleVerts = vlist.ToArray();
            }
            else
            {
                TriangleVerts = null;
            }
            if (nlist.Count > 0)
            {
                NodePositions = nlist.ToArray();
            }
            else
            {
                NodePositions = null;
            }

        }
        private void BuildWatermapVertices(WatermapFile wmf, List<EditorVertex> vl, List<Vector4> nl)
        {
            var v1 = new EditorVertex();
            var v2 = new EditorVertex();
            var v3 = new EditorVertex();
            var v4 = new EditorVertex();

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
                if (harr == null) return cblu;
                if (harr.Length == 0) return cblu;
                var i0 = harr[0].Item;
                if (i0 == null) return cblu;
                var c = i0.Colour;
                c.A = 128;
                return (uint)c.ToRgba();
            }
            var w = wmf.Width;
            var h = wmf.Height;
            var min = new Vector3(wmf.CornerX, wmf.CornerY, 0.0f);
            var step = new Vector3(wmf.TileX, -wmf.TileY, 1.0f);
            //var siz = new Vector3(w, h, 1) * step;
            for (int yi = 1; yi < h; yi++)
            {
                var yo = yi - 1;
                for (int xi = 1; xi < w; xi++)
                {
                    var xo = xi - 1;
                    var o1 = yi * w + xo;
                    var o2 = yi * w + xi;
                    var o3 = yo * w + xo;
                    var o4 = yo * w + xi;
                    v1.Position = min + step * new Vector3(xo, yi, getHeight(o1));
                    v2.Position = min + step * new Vector3(xi, yi, getHeight(o2));
                    v3.Position = min + step * new Vector3(xo, yo, getHeight(o3));
                    v4.Position = min + step * new Vector3(xi, yo, getHeight(o4));
                    v1.Colour = getColour(o1);
                    v2.Colour = getColour(o2);
                    v3.Colour = getColour(o3);
                    v4.Colour = getColour(o4);
                    //vl.Add(v1); vl.Add(v2); vl.Add(v3);
                    //vl.Add(v3); vl.Add(v2); vl.Add(v4);
                }
            }
            //for (int y = 0; y < h; y++)
            //{
            //    for (int x = 0; x < w; x++)
            //    {
            //        var o = y * w + x;
            //        nl.Add(new Vector4(min + step * new Vector3(x, y, getHeight(o)), 10));
            //    }
            //}


            void addQuad(Quad q)
            {
                v1.Position = q.P1;
                v2.Position = q.P2;
                v3.Position = q.P3;
                v4.Position = q.P4;
                vl.Add(v1); vl.Add(v2); vl.Add(v3);
                vl.Add(v3); vl.Add(v2); vl.Add(v4);
            }
            void addRivEnd(Vector3 p, Vector3 s, Vector3 d, float r)
            {
                v1.Position = p;
                v2.Position = p + s * r;
                v3.Position = p + d * r;
                v4.Position = p - s * r;
                vl.Add(v1); vl.Add(v2); vl.Add(v3);
                vl.Add(v1); vl.Add(v3); vl.Add(v4);
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
                    v1.Colour = v2.Colour = v3.Colour = v4.Colour = (uint)rc.ToRgba();
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
                        if (i == 1) addRivEnd(vo.XYZ(), -sid, -dir, rwid);
                        if (i == li) addRivEnd(vi.XYZ(), sid, dir, rwid);
                    }
                    for (int i = 1; i < quads.Length; i++)
                    {
                        var o = i - 1;
                        quads[o].P3 = quads[i].P1 = (quads[o].P3 + quads[i].P1) * 0.5f;
                        quads[o].P4 = quads[i].P2 = (quads[o].P4 + quads[i].P2) * 0.5f;
                    }
                    for (int i = 0; i < quads.Length; i++)
                    {
                        addQuad(quads[i]);
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
                    v1.Colour = v2.Colour = v3.Colour = v4.Colour = (uint)lc.ToRgba();
                    for (int i = 0; i < lake.Vectors.Length; i++)
                    {
                        var vi = lake.Vectors[i];
                        var vp = new Vector3(vi.X, vi.Y, lp.Z);
                        var q = new Quad();
                        q.P1 = vp + new Vector3(vi.Z, -vi.W, 0);
                        q.P2 = vp + new Vector3(vi.Z, vi.W, 0);
                        q.P3 = vp + new Vector3(-vi.Z, -vi.W, 0);
                        q.P4 = vp + new Vector3(-vi.Z, vi.W, 0);
                        addQuad(q);
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
                    v1.Colour = v2.Colour = v3.Colour = v4.Colour = (uint)pc.ToRgba();
                    var q = new Quad();
                    q.P1 = pp + new Vector3(ps.X, -ps.Y, 0);
                    q.P2 = pp + new Vector3(ps.X, ps.Y, 0);
                    q.P3 = pp + new Vector3(-ps.X, -ps.Y, 0);
                    q.P4 = pp + new Vector3(-ps.X, ps.Y, 0);
                    addQuad(q);
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
