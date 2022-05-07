using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker
{



    public static class QuaternionExtension
    {
        public static Vector3 Multiply(this Quaternion a, Vector3 b)
        {
            float axx = a.X * 2.0f;
            float ayy = a.Y * 2.0f;
            float azz = a.Z * 2.0f;
            float awxx = a.W * axx;
            float awyy = a.W * ayy;
            float awzz = a.W * azz;
            float axxx = a.X * axx;
            float axyy = a.X * ayy;
            float axzz = a.X * azz;
            float ayyy = a.Y * ayy;
            float ayzz = a.Y * azz;
            float azzz = a.Z * azz;
            return new Vector3(((b.X * ((1.0f - ayyy) - azzz)) + (b.Y * (axyy - awzz))) + (b.Z * (axzz + awyy)),
                        ((b.X * (axyy + awzz)) + (b.Y * ((1.0f - axxx) - azzz))) + (b.Z * (ayzz - awxx)),
                        ((b.X * (axzz - awyy)) + (b.Y * (ayzz + awxx))) + (b.Z * ((1.0f - axxx) - ayyy)));
        }

        public static Matrix ToMatrix(this Quaternion q)
        {
            float xx = q.X * q.X;
            float yy = q.Y * q.Y;
            float zz = q.Z * q.Z;
            float xy = q.X * q.Y;
            float zw = q.Z * q.W;
            float zx = q.Z * q.X;
            float yw = q.Y * q.W;
            float yz = q.Y * q.Z;
            float xw = q.X * q.W;
            Matrix result = new Matrix();
            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M14 = 0.0f;
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M24 = 0.0f;
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        public static Vector4 ToVector4(this Quaternion q)
        {
            return new Vector4(q.X, q.Y, q.Z, q.W);
        }

        public static Quaternion FastLerp(Quaternion a, Quaternion b, float v)
        {
            var r = new Quaternion();
            var vi = 1.0f - v;
            r.X = vi * a.X + v * b.X;
            r.Y = vi * a.Y + v * b.Y;
            r.Z = vi * a.Z + v * b.Z;
            r.W = vi * a.W + v * b.W;
            r.Normalize();
            return r;
        }
    }



}
