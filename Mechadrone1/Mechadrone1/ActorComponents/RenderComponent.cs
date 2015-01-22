using System;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    // Responsible for providing a scene graph visualization of the actor.
    abstract class RenderComponent : ActorComponent
    {
        public static event EventHandler RenderComponentInitialized;

        public SceneNode SceneGraph { get; protected set; }

        public override ComponentType Category { get { return ComponentType.Render; } }

        public abstract SceneGraphRoot.PartitionCategories SceneGraphCategory { get; }

        public RenderComponent(Actor owner)
            : base(owner)
        {
            SceneGraph = null;
        }

        // If you override this, this one probably should get called _after_ the derived method does its stuff.
        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            CreateSceneGraph(manifest);
            Owner.ComponentsCreated += ComponentsCreatedHandler;
            Owner.ActorInitialized += ActorInitializedHandler;
        }

        protected abstract void CreateSceneGraph(ComponentManifest manifest);

        protected virtual void OnRenderComponentInitialized(EventArgs e)
        {
            EventHandler handler = RenderComponentInitialized;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            AnimationComponent animationComponent = Owner.GetComponent<AnimationComponent>(ComponentType.Animation);
            if (animationComponent != null)
                SceneGraph.ConnectToAnimationComponent(animationComponent);
        }

        // If you override this, make sure this one gets called _after_ the derived method does its stuff.
        protected virtual void ActorInitializedHandler(object sender, EventArgs e)
        {
            OnRenderComponentInitialized(EventArgs.Empty);
        }

        public override void Release()
        {
            SceneGraph.Release();
        }

    }
}
