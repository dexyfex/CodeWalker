using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeWalker.World
{
    public class Heightmaps : BasePathData
    {
        public volatile bool Inited = false;
        public GameFileCache GameFileCache;

        public List<HeightmapFile> HeightmapFiles = new List<HeightmapFile>();


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


            HeightmapFiles.Clear();


            if (gameFileCache.EnableDlc)
            {
                LoadHeightmap("update\\update.rpf\\common\\data\\levels\\gta5\\heightmap.dat");
                LoadHeightmap("update\\update.rpf\\common\\data\\levels\\gta5\\heightmapheistisland.dat");
            }
            else
            {
                LoadHeightmap("common.rpf\\data\\levels\\gta5\\heightmap.dat");
            }


            BuildVertices();

            Inited = true;
        }

        private void LoadHeightmap(string filename)
        {
            var hmf = GameFileCache.RpfMan.GetFile<HeightmapFile>(filename);
            HeightmapFiles.Add(hmf);
        }



        public void BuildVertices()
        {

            var vlist = new List<EditorVertex>();
            var nlist = new List<Vector4>();

            foreach (var hmf in HeightmapFiles)
            {
                BuildHeightmapVertices(hmf, vlist, nlist);
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
        private void BuildHeightmapVertices(HeightmapFile hmf, List<EditorVertex> vl, List<Vector4> nl)
        {
            var v1 = new EditorVertex();
            var v2 = new EditorVertex();
            var v3 = new EditorVertex();
            var v4 = new EditorVertex();

            uint cgrn = (uint)new Color(0, 128, 0, 60).ToRgba();
            uint cyel = (uint)new Color(128, 128, 0, 200).ToRgba();

            var w = hmf.Width;
            var h = hmf.Height;
            var hmin = hmf.MinHeights;
            var hmax = hmf.MaxHeights;
            var min = hmf.BBMin;
            var max = hmf.BBMax;
            var siz = max - min;
            var step = siz / new Vector3(w - 1, h - 1, 255);

            v1.Colour = v2.Colour = v3.Colour = v4.Colour = cyel;
            for (int yi = 1; yi < h; yi++)
            {
                var yo = yi - 1;
                for (int xi = 1; xi < w; xi++)
                {
                    var xo = xi - 1;
                    var o1 = yo * w + xo;
                    var o2 = yo * w + xi;
                    var o3 = yi * w + xo;
                    var o4 = yi * w + xi;
                    v1.Position = min + step * new Vector3(xo, yo, hmin[o1]);
                    v2.Position = min + step * new Vector3(xi, yo, hmin[o2]);
                    v3.Position = min + step * new Vector3(xo, yi, hmin[o3]);
                    v4.Position = min + step * new Vector3(xi, yi, hmin[o4]);
                    vl.Add(v1); vl.Add(v2); vl.Add(v3);
                    vl.Add(v3); vl.Add(v2); vl.Add(v4);
                }
            }
            v1.Colour = v2.Colour = v3.Colour = v4.Colour = cgrn;
            for (int yi = 1; yi < h; yi++)
            {
                var yo = yi - 1;
                for (int xi = 1; xi < w; xi++)
                {
                    var xo = xi - 1;
                    var o1 = yo * w + xo;
                    var o2 = yo * w + xi;
                    var o3 = yi * w + xo;
                    var o4 = yi * w + xi;
                    v1.Position = min + step * new Vector3(xo, yo, hmax[o1]);
                    v2.Position = min + step * new Vector3(xi, yo, hmax[o2]);
                    v3.Position = min + step * new Vector3(xo, yi, hmax[o3]);
                    v4.Position = min + step * new Vector3(xi, yi, hmax[o4]);
                    vl.Add(v1); vl.Add(v2); vl.Add(v3);
                    vl.Add(v3); vl.Add(v2); vl.Add(v4);
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var o = y * w + x;
                    nl.Add(new Vector4(min + step * new Vector3(x, y, hmin[o]), 10));
                    nl.Add(new Vector4(min + step * new Vector3(x, y, hmax[o]), 10));
                }
            }


        }


    }
}
