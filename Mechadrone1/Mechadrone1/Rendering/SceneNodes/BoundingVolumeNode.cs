using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    abstract class BoundingVolumeNode : SceneNode
    {
        public BoundingVolumeNode()
            : base() { }

        public BoundingVolumeNode(BoundingVolumeNode orig)
            : base(orig) { }

        public override void ProcessChildren(TraversalContext context)
        {
            if (context.AcceptAllGrantor == null)
            {
                ContainmentType result = TestFrustum(context.VisibilityFrustum, context.Transform.Top);
                if (result == ContainmentType.Contains)
                {
                    context.AcceptAllGrantor = this;
                    base.ProcessChildren(context);
                }
                else if (result == ContainmentType.Intersects)
                {
                    base.ProcessChildren(context);
                }
            }
            else
            {
                base.ProcessChildren(context);
            }
        }

        public override void PostProcess(TraversalContext context)
        {
            if (context.AcceptAllGrantor == this)
                context.AcceptAllGrantor = null;
        }

        protected abstract ContainmentType TestFrustum(BoundingFrustum frustum, Matrix transform);
    }
}
