using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Vector4 Floor(this Vector4 v)
        {
            return new Vector4((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z), (float)Math.Floor(v.W));
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
}