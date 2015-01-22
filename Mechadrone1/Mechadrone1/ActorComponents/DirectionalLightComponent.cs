using System.Threading;
using System;
using Microsoft.Xna.Framework;
using Manifracture;

namespace Mechadrone1
{
    class DirectionalLightComponent : ActorComponent
    {
        public Vector3 Radiance { get; set; }

        public override ComponentType Category
        {
            get { return ComponentType.Light; }
        }

        public DirectionalLightComponent(Actor owner)
            : base(owner)
        {
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager contentLoader, Manifracture.ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.RADIANCE))
                Radiance = (Vector3)(manifest.Properties[ManifestKeys.RADIANCE]);
        }

        public override void Release() { }
    }
}
