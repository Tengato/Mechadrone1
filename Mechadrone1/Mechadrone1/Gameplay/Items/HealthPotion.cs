using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class HealthPotion : Item
    {
        public int Power { get; set; }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);

            Power = (int)(manifest.Properties[ManifestKeys.POWER]);
        }

        public void Use()
        {

        }
    }
}
