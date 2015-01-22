using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    /// <summary>
    /// BoundingSphereNodes are used to represent bounding volumes of model meshes in model space. So they should
    /// not have transform or other bounding volume nodes below them. The BV is specified explicitly, once, via the constructor.
    /// </summary>
    class ExplicitBoundingSphereNode : BoundingVolumeNode, IExplicitBoxableVolume
    {
        public BoundingSphere BoundingSphere { get; private set; }

        public ExplicitBoundingSphereNode(BoundingSphere sphere)
        {
            BoundingSphere = sphere;
        }

        protected override ContainmentType TestFrustum(BoundingFrustum frustum, Matrix transform)
        {
            return frustum.Contains(BoundingSphere.Transform(transform));
        }

        BoundingBox IExplicitBoxableVolume.BoxTransformedVolume(Matrix transform)
        {
            return BoundingBox.CreateFromSphere(BoundingSphere.Transform(transform));
        }
    }
}
