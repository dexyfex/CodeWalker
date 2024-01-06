using System;
using SharpDX;

namespace CodeWalker.Core.Utils
{
    public static class BoundingBoxExtensions
    {
        public static Vector3 Size(in this BoundingBox bounds)
        {
            return new Vector3(
                Math.Abs(bounds.Maximum.X - bounds.Minimum.X), 
                Math.Abs(bounds.Maximum.Y - bounds.Minimum.Y),
                Math.Abs(bounds.Maximum.Z - bounds.Minimum.Z));
        }

        public static Vector3 Center(in this BoundingBox bounds)
        {
            return (bounds.Minimum + bounds.Maximum) * 0.5F;
        }

        public static BoundingBox Encapsulate(ref this BoundingBox box, ref BoundingBox bounds)
        {
            Vector3.Min(ref box.Minimum, ref bounds.Minimum, out box.Minimum);
            Vector3.Max(ref box.Maximum, ref bounds.Maximum, out box.Maximum);
            return box;
        }

        public static float Radius(in this BoundingBox box)
        {
            var extents = (box.Maximum - box.Minimum) * 0.5F;
            return extents.Length();
        }

        public static BoundingBox Expand(in this BoundingBox b, float amount)
        {
            return new BoundingBox(b.Minimum - Vector3.One * amount, b.Maximum + Vector3.One * amount);
        }
    }
}
