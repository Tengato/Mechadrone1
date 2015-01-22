using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Manifracture;

namespace Mechadrone1
{
    class HDRLightComponent :ActorComponent
    {
        public Texture2D IrradianceMap { get; set; }
        public Texture2D SpecPrefilter { get; set; }
        public int NumSpecLevels { get; set; }
        public float SpecExponentFactor { get; set; }
        public Vector3 AmbientLight { get; set; }
        public static event EventHandler HDRLightCreated;

        public override ComponentType Category
        {
            get { return ComponentType.Light; }
        }

        public HDRLightComponent(Actor owner)
            : base(owner)
        {
            IrradianceMap = null;
            SpecPrefilter = null;
            NumSpecLevels = 0;
            SpecExponentFactor = 0.0f;
            AmbientLight = Vector3.Zero;
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager contentLoader, Manifracture.ComponentManifest manifest)
        {
            IrradianceMap = contentLoader.Load<Texture2D>((string)(manifest.Properties[ManifestKeys.IRRADIANCEMAP]));
            SpecPrefilter = contentLoader.Load<Texture2D>((string)(manifest.Properties[ManifestKeys.SPECPREFILTER]));
            NumSpecLevels = (int)(manifest.Properties[ManifestKeys.NUMSPECLEVELS]);
            SpecExponentFactor = (float)(manifest.Properties[ManifestKeys.SPECEXPONENTFACTOR]);
            AmbientLight = (Vector3)(manifest.Properties[ManifestKeys.AMBIENTLIGHT]);

            Owner.ActorInitialized += ActorInitializedHandler;
        }

        protected virtual void ActorInitializedHandler(object sender, EventArgs e)
        {
            OnHDRLightCreated(EventArgs.Empty);
        }

        private void OnHDRLightCreated(EventArgs e)
        {
            EventHandler handler = HDRLightCreated;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override void Release() { }

    }
}
