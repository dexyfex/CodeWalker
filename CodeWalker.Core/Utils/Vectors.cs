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

        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }


        public static Vector4 Floor(this Vector4 v)
        {
            return new Vector4((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z), (float)Math.Floor(v.W));
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


    }



}