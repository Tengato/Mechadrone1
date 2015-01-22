using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    abstract class ActorComponent
    {
        public Actor Owner { get; private set; }

        // Don't change the signature of this constructor in derived classes.
        public ActorComponent(Actor owner)
        {
            Owner = owner;
        }

        public enum ComponentType
        {
            Transform,
            Render,
            Behavior,
            Physics,
            Audio,
            Animation,
            Control,
            Template,
            Light,
            ShadowCaster,
            Fog,
            Camera,
        }

        public abstract ComponentType Category { get; }

        // Called after construction and having been added to the actor's component list. Override this to initialize the component's
        // properties and load assets using the ComponentManifest.
        public virtual void Initialize(ContentManager contentLoader, ComponentManifest manifest) { }

        // If you need to process dependencies on other components, handle the owner's ComponentsCreated event,
        // which is fired once all components have been initialized.

        // Use this method to unsubscribe from events, so we can be garbage collected.
        public abstract void Release();
    }
}
