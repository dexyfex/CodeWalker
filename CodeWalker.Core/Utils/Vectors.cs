using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.GameFiles;

namespace CodeWalker
{
    public static class Vectors
    {
        public static Vector3 XYZ(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 Round(this Vector3 v)
        {
            return new Vector3((float)Math.Round(v.X), (float)Math.Round(v.Y), (float)Math.Round(v.Z));
        }

        public static Vector3 GetPerpVec(this Vector3 n)
        {
            //make a vector perpendicular to the given one
            float nx = Math.Abs(n.X);
            float ny = Math.Abs(n.Y);
            float nz = Math.Abs(n.Z);
            if ((nx < ny) && (nx < nz))
            {
                return Vector3.Cross(n, Vector3.Right);
            }
            else if (ny < nz)
            {
                return Vector3.Cross(n, Vector3.Up);
            }
            else
            {
                return Vector3.Cross(n, Vector3.ForwardLH);
            }
        }

        public static Vector3 Floor(this Vector3 v)
        {
            return new Vector3((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z));
        }
        public static Vector3 Ceiling(this Vector3 v)
        {
            return new Vector3((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y), (float)Math.Ceiling(v.Z));
        }

        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

        public static int CompareTo(this Vector3 a, Vector3 b)
        {
            int c;
            c = a.X.CompareTo(b.X); if (c != 0) return c;
            c = a.Y.CompareTo(b.Y); if (c != 0) return c;
            c = a.Z.CompareTo(b.Z); if (c != 0) return c;
            return 0;
        }


        public static Vector4 Floor(this Vector4 v)
        {
            return new Vector4((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z), (float)Math.Floor(v.W));
        }
        public static Vector4 Ceiling(this Vector4 v)
        {
            return new Vector4((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y), (float)Math.Ceiling(v.Z), (float)Math.Ceiling(v.W));
        }

        public static Vector4 Abs(this Vector4 v)
        {
            return new Vector4(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z), Math.Abs(v.W));
        }

        public static Quaternion ToQuaternion(this Vector4 v)
        {
            return new Quaternion(v);
        }
    }


    public struct Vector2I
    {
        public int X;
        public int Y;

        public Vector2I(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Vector2I(Vector2 v)
        {
            X = (int)Math.Floor(v.X);
            Y = (int)Math.Floor(v.Y);
        }

        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString();
        }


        public static Vector2I operator +(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2I operator -(Vector2I a, Vector2I b)
        {
            return new Vector2I(a.X - b.X, a.Y - b.Y);
        }

    }



    public static class BoundingBoxMath
    {

        public static BoundingBox Transform(this BoundingBox b, Vector3 position, Quaternion orientation, Vector3 scale)
        {
            var mat = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, scale, Vector3.Zero, orientation, position);
            return b.Transform(mat);
        }

        public static BoundingBox Transform(this BoundingBox b, Matrix mat)
        {
            var matabs = mat;
            matabs.Column1 = mat.Column1.Abs();
            matabs.Column2 = mat.Column2.Abs();
            matabs.Column3 = mat.Column3.Abs();
            matabs.Column4 = mat.Column4.Abs();
            var bbcenter = (b.Maximum + b.Minimum) * 0.5f;
            var bbextent = (b.Maximum - b.Minimum) * 0.5f;
            var ncenter = Vector3.TransformCoordinate(bbcenter, mat);
            var nextent = Vector3.TransformNormal(bbextent, matabs).Abs();
            return new BoundingBox(ncenter - nextent, ncenter + nextent);
        }

    }



    public struct BoundingCapsule
    {
        public Vector3 PointA;
        public Vector3 PointB;
        public float Radius;
    }

    public static class BoundingCapsuleMath
    {

        public static bool Intersects(this Ray r, ref BoundingCapsule capsule, out float dist)
        {
            // intersect capsule : http://www.iquilezles.org/www/articles/intersectors/intersectors.htm
            Vector3  ba = capsule.PointB - capsule.PointA;
            Vector3  oa = r.Position - capsule.PointA;
            float baba = Vector3.Dot(ba,ba);
            float bard = Vector3.Dot(ba,r.Direction);
            float baoa = Vector3.Dot(ba,oa);
            float rdoa = Vector3.Dot(r.Direction,oa);
            float oaoa = Vector3.Dot(oa,oa);

            float r2 = capsule.Radius * capsule.Radius;
            float a = baba      - bard*bard;
            float b = baba*rdoa - baoa*bard;
            float c = baba*oaoa - baoa*baoa - r2*baba;
            float h = b*b - a*c;

            if( h>=0.0f )
            {
                float t = (-b-(float)Math.Sqrt(h))/a;

                float y = baoa + t*bard;

                // body
                if (y > 0.0f && y < baba)
                {
                    dist = t;
                    return true;
                }

                // caps
                Vector3 oc = (y<=0.0f) ? oa : r.Position - capsule.PointB;
                b = Vector3.Dot(r.Direction,oc);
                c = Vector3.Dot(oc,oc) - r2;
                h = b*b - c;
                if( h>0.0f )
                {
                    dist = -b - (float)Math.Sqrt(h);
                    return true;
                }
            }
            dist = -1.0f;
            return false;
        }
        public static Vector3 Normal(this BoundingCapsule c, ref Vector3 position)
        {
            Vector3 ba = c.PointB - c.PointA;
            Vector3 pa = position - c.PointA;
            float h = Math.Min(Math.Max(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0f), 1.0f);
            return Vector3.Normalize((pa - h * ba) / c.Radius);
        }


        public static bool Intersects(this BoundingSphere sph, ref BoundingCapsule capsule, out Vector3 norm)
        {
            var dist = LineMath.PointSegmentDistance(ref sph.Center, ref capsule.PointA, ref capsule.PointB);
            var rads = sph.Radius + capsule.Radius;
            if (dist <= rads)
            {
                norm = LineMath.PointSegmentNormal(ref sph.Center, ref capsule.PointA, ref capsule.PointB);
                return true;
            }
            else
            {
                norm = Vector3.Up;
                return false;
            }
        }

    }



    public struct BoundingCylinder
    {
        public Vector3 PointA;
        public Vector3 PointB;
        public float Radius;
    }
    public static class BoundingCylinderMath
    {

        public static bool Intersects(this Ray r, ref BoundingCylinder cylinder, out float dist, out Vector3 norm)
        {
            // intersect cylinder : https://www.shadertoy.com/view/4lcSRn
            Vector3 ba = cylinder.PointB - cylinder.PointA;
            Vector3 oc = r.Position - cylinder.PointA;
            float baba = Vector3.Dot(ba, ba);
            float bard = Vector3.Dot(ba, r.Direction);
            float baoc = Vector3.Dot(ba, oc);

            float r2 = cylinder.Radius * cylinder.Radius;
            float k2 = baba - bard * bard;
            float k1 = baba * Vector3.Dot(oc, r.Direction) - baoc * bard;
            float k0 = baba * Vector3.Dot(oc, oc) - baoc * baoc - r2 * baba;

            float h = k1 * k1 - k2 * k0;
            if (h < 0.0f)
            {
                dist = -1.0f;
                norm = Vector3.Up;
                return false;
            }
            h = (float)Math.Sqrt(h);
            float t = (-k1 - h) / k2;

            // body
            float y = baoc + t * bard;
            if (y > 0.0f && y < baba)
            {
                dist = t;
                norm = Vector3.Normalize((oc + t * r.Direction - ba * y / baba) / cylinder.Radius);
                return true;
            }

            // caps
            t = (((y < 0.0f) ? 0.0f : baba) - baoc) / bard;
            if (Math.Abs(k1 + k2 * t) < h)
            {
                dist = t;
                norm = Vector3.Normalize(ba * Math.Sign(y) / baba);
                return true;
            }

            dist = -1.0f;
            norm = Vector3.Up;
            return false;
        }

    }



    public static class LineMath
    {


        public static float PointSegmentDistance(ref Vector3 v, ref Vector3 a, ref Vector3 b)
        {
            //https://stackoverflow.com/questions/4858264/find-the-distance-from-a-3d-point-to-a-line-segment
            Vector3 ab = b - a;
            Vector3 av = v - a;

            if (Vector3.Dot(av, ab) <= 0.0f)// Point is lagging behind start of the segment, so perpendicular distance is not viable.
            {
                return av.Length();         // Use distance to start of segment instead.
            }

            Vector3 bv = v - b;
            if (Vector3.Dot(bv, ab) >= 0.0f)// Point is advanced past the end of the segment, so perpendicular distance is not viable.
            {
                return bv.Length();         // Use distance to end of the segment instead.
            }

            return Vector3.Cross(ab, av).Length() / ab.Length();// Perpendicular distance of point to segment.
        }

        public static Vector3 PointSegmentNormal(ref Vector3 v, ref Vector3 a, ref Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 av = v - a;

            if (Vector3.Dot(av, ab) <= 0.0f)
            {
                return Vector3.Normalize(av);
            }

            Vector3 bv = v - b;
            if (Vector3.Dot(bv, ab) >= 0.0f)
            {
                return Vector3.Normalize(bv);
            }

            return Vector3.Normalize(Vector3.Cross(Vector3.Cross(ab, av), ab));
        }

        public static float PointRayDist(ref Vector3 p, ref Vector3 ro, ref Vector3 rd)
        {
            return Vector3.Cross(rd, p - ro).Length();
        }

    }


    public static class TriangleMath
    {

        public static float AreaPart(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, out float angle)
        {
            var va = v2 - v1;
            var vb = v3 - v1;
            var na = Vector3.Normalize(va);
            var nb = Vector3.Normalize(vb);
            var a = va.Length();
            var b = vb.Length();
            var c = Math.Acos(Vector3.Dot(na, nb));
            var area = (float)(0.5 * a * b * Math.Sin(c));
            angle = (float)Math.Abs(c);
            return area;
        }

        public static float Area(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3)
        {
            var a1 = AreaPart(ref v1, ref v2, ref v3, out float t1);
            var a2 = AreaPart(ref v2, ref v3, ref v1, out float t2);
            var a3 = AreaPart(ref v3, ref v1, ref v2, out float t3);
            var fp = (float)Math.PI;
            var d1 = Math.Min(t1, Math.Abs(t1 - fp));
            var d2 = Math.Min(t2, Math.Abs(t2 - fp));
            var d3 = Math.Min(t3, Math.Abs(t3 - fp));
            if ((d1 >= d2) && (a1 != 0))
            {
                if ((d1 >= d3) || (a3 == 0))
                {
                    return a1;
                }
                else
                {
                    return a3;
                }
            }
            else
            {
                if ((d2 >= d3) || (a3 == 0))
                {
                    return a2;
                }
                else
                {
                    return a3;
                }
            }
        }

    }



}