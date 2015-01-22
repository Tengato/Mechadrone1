using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    abstract class TransformNode : SceneNode
    {
        public abstract Matrix Transform { get; }

        public override void PreProcess(TraversalContext context)
        {
            context.Transform.Push();
            context.Transform.MultMatrixLocal(Transform);
        }

        public override void PostProcess(TraversalContext context)
        {
            context.Transform.Pop();
        }

    }
}
