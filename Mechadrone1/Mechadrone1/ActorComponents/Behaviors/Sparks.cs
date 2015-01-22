using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class Sparks : Behavior
    {
        private ParticleSystemRenderComponent mParticleSystem;

        public Sparks(Actor owner)
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

        public void Emit()
        {
            TransformComponent myXForm = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);

            for (int i = 0; i < 20; ++i)
            {
                mParticleSystem.AddParticle(myXForm.Translation, Vector3.Zero);
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
