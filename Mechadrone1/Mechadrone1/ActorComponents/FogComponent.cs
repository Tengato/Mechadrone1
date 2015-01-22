using System;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1
{
    class FogComponent : ActorComponent
    {
        public Color Color;
        public float Start;
        public float End;
        public static event EventHandler FogCreated;

        public override ComponentType Category
        {
            get { return ComponentType.Fog; }
        }

        public FogComponent(Actor owner)
            : base(owner)
        {
            Color = Color.Black;
            Start = 0.0f;
            End = 0.0f;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Color = (Color)(manifest.Properties[ManifestKeys.COLOR]);
            Start = (float)(manifest.Properties[ManifestKeys.START]);
            End = (float)(manifest.Properties[ManifestKeys.END]);

            Owner.ActorInitialized += ActorInitializedHandler;
        }

        protected virtual void ActorInitializedHandler(object sender, EventArgs e)
        {
            OnFogCreated(EventArgs.Empty);
        }

        private void OnFogCreated(EventArgs e)
        {
            EventHandler handler = FogCreated;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override void Release() { }
    }
}
