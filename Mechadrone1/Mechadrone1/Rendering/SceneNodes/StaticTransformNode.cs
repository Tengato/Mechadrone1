using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class StaticTransformNode : TransformNode
    {
        private Matrix mTransform;

        public StaticTransformNode(Matrix transform)
        {
            mTransform = transform;
        }

        public override Matrix Transform { get { return mTransform; } }
    }
}
