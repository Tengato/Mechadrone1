using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;

namespace Mechadrone1
{
    class WeaponResource : Behavior
    {
        public float Value { get; set; }
        public float RegenerationRate { get; set; }

        public WeaponResource(Actor owner)
            : base(owner)
        {
            Value = 1.0f;
            GameResources.ActorManager.PostPhysicsUpdateStep += PostPhysicsUpdateHandler;
            RegenerationRate = 0.2f;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.REGENERATION_RATE))
                RegenerationRate = (float)(manifest.Properties[ManifestKeys.REGENERATION_RATE]);
        }

        private void PostPhysicsUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            Value += (float)(e.GameTime.ElapsedGameTime.TotalSeconds) * RegenerationRate;
            if (Value > 1.0f)
                Value = 1.0f;
        }

        public override void Release()
        {
            GameResources.ActorManager.PostPhysicsUpdateStep -= PostPhysicsUpdateHandler;
        }
    }
}
