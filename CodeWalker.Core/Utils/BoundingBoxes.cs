using System;
using SharpDX;

namespace CodeWalker.Core.Utils
{
    public static class BoundingBoxExtensions
    {
        public static Vector3 Size(this BoundingBox bounds)
        {
            return new Vector3(
                Math.Abs(bounds.Maximum.X - bounds.Minimum.X), 
                Math.Abs(bounds.Maximum.Y - bounds.Minimum.Y),
                Math.Abs(bounds.Maximum.Z - bounds.Minimum.Z));
        }

        public static Vector3 Center(this BoundingBox bounds)
        {
            return (bounds.Minimum + bounds.Maximum) * 0.5F;
        }

        public static BoundingBox Encapsulate(this BoundingBox box, BoundingBox bounds)
        {
            box.Minimum = Vector3.Min(box.Minimum, bounds.Minimum);
            box.Maximum = Vector3.Max(box.Maximum, bounds.Maximum);
            return box;
        }

        public static float Radius(this BoundingBox box)
        {
            var extents = (box.Maximum - box.Minimum) * 0.5F;
            return extents.Length();
        }

        public static BoundingBox Expand(this BoundingBox b, float amount)
        {
            return new BoundingBox(b.Minimum - Vector3.One * amount, b.Maximum + Vector3.One * amount);
        }
    }
}
