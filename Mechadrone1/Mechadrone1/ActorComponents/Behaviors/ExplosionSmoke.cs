using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class ExplosionSmoke : Behavior
    {
        private ParticleSystemRenderComponent mParticleSystem;

        public ExplosionSmoke(Actor owner)
            : base(owner)
        {
            mParticleSystem = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Owner.ComponentsCreated += ComponentsCreatedHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            mParticleSystem = Owner.GetComponent<ParticleSystemRenderComponent>(ActorComponent.ComponentType.Render);
            if (mParticleSystem  == null)
                throw new LevelManifestException("Expected ActorComponent missing.");
        }

        public void Emit(Vector3 velocity)
        {
            TransformComponent myXForm = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);

            for (int i = 0; i < 180; ++i)
            {
                mParticleSystem.AddParticle(myXForm.Translation, velocity);
            }

            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            if (!mParticleSystem.HasActiveParticles)
                Owner.Despawn();
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }
}
