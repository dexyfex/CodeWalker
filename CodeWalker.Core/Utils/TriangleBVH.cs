using Collections.Pooled;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeWalker
{
    public class TriangleBVH : TriangleBVHNode
    {

        public TriangleBVH(TriangleBVHItem[] tris, int depth = 8)
        {
            if (tris is null || tris.Length == 0)
                return;
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            for (int i = 0; i < tris.Length; i++)
            {
                var tri = tris[i];
                tri.UpdateBox();
                Vector3.Min(ref min, ref tri.Box.Minimum, out min);
                Vector3.Max(ref max, ref tri.Box.Maximum, out max);
            }
            _Box = new BoundingBox(min, max);

            Build(tris, depth);

        }
    }

    public class TriangleBVHNode
    {
        public TriangleBVHItem[] Triangles { get; set; }
        public TriangleBVHNode? Node1 { get; set; }
        public TriangleBVHNode? Node2 { get; set; }

        public BoundingBox _Box;
        public BoundingBox Box => _Box;

        public void Build(TriangleBVHItem[] tris, int depth)
        {
            if (tris.Length <= 10) depth = 0;
            if (depth <= 0)
            {
                Triangles = tris;
                Node1 = null;
                Node2 = null;
            }
            else
            {
                var min = _Box.Minimum;
                var max = _Box.Maximum;
                var cen = _Box.Center;
                var siz = _Box.Size;
                BoundingBox b1, b2;
                if ((siz.X >= siz.Y) && (siz.X >= siz.Z))
                {
                    b1 = new BoundingBox(min, new Vector3(cen.X, max.Y, max.Z));
                    b2 = new BoundingBox(new Vector3(cen.X, min.Y, min.Z), max);
                }
                else if (siz.Y >= siz.Z)
                {
                    b1 = new BoundingBox(min, new Vector3(max.X, cen.Y, max.Z));
                    b2 = new BoundingBox(new Vector3(min.X, cen.Y, min.Z), max);
                }
                else
                {
                    b1 = new BoundingBox(min, new Vector3(max.X, max.Y, cen.Z));
                    b2 = new BoundingBox(new Vector3(min.X, min.Y, cen.Z), max);
                }
                using var l1 = new PooledList<TriangleBVHItem>();
                using var l2 = new PooledList<TriangleBVHItem>();
                for (int i = 0; i < tris.Length; i++)
                {
                    var tri = tris[i];
                    if (tri.Box.Contains(ref b1) != ContainmentType.Disjoint)// (tri.Box.Intersects(b1))
                    {
                        l1.Add(tri);
                    }
                    if (tri.Box.Contains(ref b2) != ContainmentType.Disjoint)// (tri.Box.Intersects(b2))
                    {
                        l2.Add(tri);
                    }
                }
                if (l1.Count > 0)
                {
                    Node1 = new TriangleBVHNode();
                    Node1._Box = b1;
                    Node1.Build(l1.ToArray(), depth - 1);
                }
                if (l2.Count > 0)
                {
                    Node2 = new TriangleBVHNode();
                    Node2._Box = b2;
                    Node2.Build(l2.ToArray(), depth - 1);
                }
            }
        }


        public TriangleBVHItem? RayIntersect(ref Ray ray, ref float hitdist)
        {
            if (!ray.Intersects(ref _Box))
                return null;

            TriangleBVHItem? hit = null;
            if (Triangles is not null)
            {
                foreach(var tri in Triangles)
                {
                    if (ray.Intersects(ref tri.Corner1, ref tri.Corner2, ref tri.Corner3, out float d) && d < hitdist && d > 0)
                    {
                        hitdist = d;
                        hit = tri;
                    }
                }
            }
            if (Node1 is not null)
            {
                var hd = hitdist;
                var h = Node1.RayIntersect(ref ray, ref hd);
                if (h != null && hd < hitdist)
                {
                    hitdist = hd;
                    hit = h;
                }
            }
            if (Node2 is not null)
            {
                var hd = hitdist;
                var h = Node2.RayIntersect(ref ray, ref hd);
                if (h != null && hd < hitdist)
                {
                    hitdist = hd;
                    hit = h;
                }
            }

            return hit;
        }

    }

    public abstract class TriangleBVHItem
    {
        public Vector3 Corner1;
        public Vector3 Corner2;
        public Vector3 Corner3;
        public BoundingBox Box;

        public Vector3 Center
        {
            get => (Corner1 + Corner2 + Corner3) * 0.3333333f;
            set
            {
                var delta = value - Center;
                Corner1 += delta;
                Corner2 += delta;
                Corner3 += delta;
            }
        }
        public Quaternion Orientation 
        {
            get => _Orientation;
            set
            {
                Quaternion.Invert(ref _Orientation, out var inv);
                var delta = value * inv;
                var cen = Center;
                Corner1 = cen + delta.Multiply(Corner1 - cen);
                Corner2 = cen + delta.Multiply(Corner2 - cen);
                Corner3 = cen + delta.Multiply(Corner3 - cen);
                _Orientation = value;
            }
        }
        private Quaternion _Orientation = Quaternion.Identity;
        public Vector3 Scale
        {
            get => _Scale;
            set
            {
                Quaternion.Invert(ref _Orientation, out var inv);
                var delta = value / _Scale;
                var cen = Center;
                Corner1 = cen + _Orientation.Multiply(inv.Multiply(Corner1 - cen) * delta);
                Corner2 = cen + _Orientation.Multiply(inv.Multiply(Corner2 - cen) * delta);
                Corner3 = cen + _Orientation.Multiply(inv.Multiply(Corner3 - cen) * delta);
                _Scale = value;
            }
        }
        private Vector3 _Scale = Vector3.One;

        public void UpdateBox()
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            Vector3.Min(ref min, ref Corner1, out min);
            Vector3.Min(ref min, ref Corner2, out min);
            Vector3.Min(ref min, ref Corner3, out min);
            Vector3.Max(ref max, ref Corner1, out max);
            Vector3.Max(ref max, ref Corner2, out max);
            Vector3.Max(ref max, ref Corner3, out max);
            Box = new BoundingBox(min, max);
        }


    }

}
