using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    /// <summary>
    /// Warning: BoundingBoxNodes cannot exist below any rotational transform nodes in a scene graph because we don't
    /// want to get involved in the messy business of rotating AABBs.
    /// Note: This class is abstract because child classes will implement the way to set Bound.
    /// </summary>
    abstract class BoundingBoxNode : BoundingVolumeNode
    {
        public BoundingBox Bound { get; protected set; }

        public BoundingBoxNode()
            : base() { }

        public BoundingBoxNode(BoundingBoxNode orig)
            : base(orig) { }

        protected override ContainmentType TestFrustum(BoundingFrustum frustum, Matrix transform)
        {
            return frustum.Contains(TransformBox(Bound, transform));
        }

        public static BoundingBox TransformBox(BoundingBox aabb, Matrix transform)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;

            transform.Decompose(out scale, out rotation, out translation);

            if (rotation != Quaternion.Identity)
                throw new ArgumentException("Rotation of AABBs is not supported.");

            Vector3 center = 0.5f * (aabb.Max + aabb.Min) + translation;
            Vector3 extent = 0.5f * (aabb.Max - aabb.Min) * scale;

            //return new BoundingBox(center - extent, center + extent);
            return BoundingBox.CreateFromPoints(new Vector3[] { center - extent, center + extent });
        }
    }
}
