using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    abstract class Behavior
    {
        public Actor Owner { get; private set; }
        
        // Don't change the signature of this constructor in derived classes.
        public Behavior(Actor owner)
        {
            Owner = owner;
        }

        public virtual void Initialize(ContentManager contentLoader, ComponentManifest manifest) { }

        public abstract void Release();
    }
}
