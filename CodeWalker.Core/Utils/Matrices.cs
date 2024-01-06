using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MatrixSystem = System.Numerics.Matrix4x4;

namespace CodeWalker
{
    public static class MatrixExtensions
    {

        public static Vector3 MultiplyW(in this Matrix m, Vector3 v)
        {
            float x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            float y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            float z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            float w = (((m.M14 * v.X) + (m.M24 * v.Y)) + (m.M34 * v.Z)) + m.M44;
            float iw = 1.0f / Math.Abs(w);
            return new Vector3(x * iw, y * iw, z * iw);
        }

        public static Vector3 MultiplyW(in this Matrix m, in Vector3 v)
        {
            float x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            float y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            float z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            float w = (((m.M14 * v.X) + (m.M24 * v.Y)) + (m.M34 * v.Z)) + m.M44;
            float iw = 1.0f / Math.Abs(w);
            return new Vector3(x * iw, y * iw, z * iw);
        }

        public static Vector3 Multiply(in this Matrix m, Vector3 v)
        {
            float x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            float y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            float z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            return new Vector3(x, y, z);
            //this quick mul ignores W...
        }
        public static Vector3 MultiplyRot(in this Matrix m, Vector3 v)
        {
            float x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z));// + m.M41;
            float y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z));// + m.M42;
            float z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z));// + m.M43;
            return new Vector3(x, y, z);
            //this quick mul ignores W and translation...
        }

        public static Vector4 Multiply(in this Matrix m, Vector4 v)
        {
            float x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            float y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            float z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            float w = (((m.M14 * v.X) + (m.M24 * v.Y)) + (m.M34 * v.Z)) + m.M44;
            return new Vector4(x, y, z, w);
        }

        public static Quaternion ToQuaternion(in this Matrix m)
        {
            var rmat = m;
            rmat.TranslationVector = Vector3.Zero;
            Quaternion.RotationMatrix(ref rmat, out var result);
            return result;
        }

    }
}
