using System;
using Manifracture;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1
{
    class ShadowCasterComponent : ActorComponent
    {
        public static event EventHandler ShadowCasterCreated;

        public override ComponentType Category
        {
            get { return ComponentType.ShadowCaster; }
        }

        public ShadowCasterComponent(Actor owner)
            : base(owner)
        {
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Owner.ActorInitialized += ActorInitializedHandler;
        }

        // If you override this, make sure this one gets called _after_ the derived method does its stuff.
        protected virtual void ActorInitializedHandler(object sender, EventArgs e)
        {
            OnShadowCasterCreated(EventArgs.Empty);
        }

        private void OnShadowCasterCreated(EventArgs e)
        {
            EventHandler handler = ShadowCasterCreated;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override void Release() { }
    }
}
