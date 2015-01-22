using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class ActorTransformNode : TransformNode
    {
        public Actor Subject { get; private set; }
        private TransformComponent mTransform;

        public ActorTransformNode(Actor subject)
            : base()
        {
            Subject = subject;
            Subject.ComponentsCreated += ComponentsCreatedHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            // Holding on to this reference should be okay since the scene graph nodes which own this object should
            // be released when the Actor is despawned.
            mTransform = Subject.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
        }

        public override Matrix Transform
        {
            get { return mTransform.Transform; }
        }
    }
}
