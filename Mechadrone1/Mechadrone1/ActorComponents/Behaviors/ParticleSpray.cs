using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class ParticleSpray : Behavior
    {
        private ParticleSystemRenderComponent mParticleSystem;
        private TransformComponent mTransform;

        public ParticleSpray(Actor owner)
            : base(owner)
        {
            mParticleSystem = null;
            mTransform = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Owner.ComponentsCreated += ComponentsCreatedHandler;
            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            mParticleSystem = Owner.GetComponent<ParticleSystemRenderComponent>(ActorComponent.ComponentType.Render);
            if (mParticleSystem  == null)
                throw new LevelManifestException("Expected ActorComponent missing.");

            mTransform = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            if (mTransform == null)
                throw new LevelManifestException("Expected ActorComponent missing.");
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            mParticleSystem.AddParticle(mTransform.Translation, Vector3.Zero);
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }
}
