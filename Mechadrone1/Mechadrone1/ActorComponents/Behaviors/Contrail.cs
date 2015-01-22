using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class Contrail : Behavior
    {
        private ParticleSystemRenderComponent mParticleSystem;
        public Vector3 OffsetFromTarget { get; set; }
        private Vector3 mPreviousPosition;
        private float mTimeBetweenParticles;
        private float mTimeLeftOver;
        private int mTargetActorId;

        public float ParticlesPerSecond
        {
            get { return 1.0f / mTimeBetweenParticles; }
            set { mTimeBetweenParticles = 1.0f / value; }
        }

        public Contrail(Actor owner)
            : base(owner)
        {
            mParticleSystem = null;
            mTargetActorId = Actor.INVALID_ACTOR_ID;
            OffsetFromTarget = Vector3.Zero;
            mPreviousPosition = Vector3.One * Single.MaxValue;
            ParticlesPerSecond = 60.0f;
            mTimeLeftOver = 0.0f;
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
        }

        public void SetTarget(int targetActorId)
        {
            mTargetActorId = targetActorId;
            mPreviousPosition = GetPosition();
            Actor target = GameResources.ActorManager.GetActorById(mTargetActorId);
            target.ActorDespawning += TargetDespawningHandler;
            // TODO: P2: Figure out how to maintain a correct bound for the visual.
            TransformComponent xForm = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            xForm.Translation = mPreviousPosition;
        }

        private Vector3 GetPosition()
        {
            Actor target = GameResources.ActorManager.GetActorById(mTargetActorId);
            if (target != null)
            {
                TransformComponent targetXForm = target.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
                return targetXForm.Translation + Vector3.Transform(OffsetFromTarget, targetXForm.Orientation);
            }
            else
            {
                return mPreviousPosition;
            }
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            if (mTargetActorId == Actor.INVALID_ACTOR_ID)
            {
                if (!mParticleSystem.HasActiveParticles)
                    Owner.Despawn();
                return;
            }

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)(e.GameTime.ElapsedGameTime.TotalSeconds);

            Vector3 newPosition = GetPosition();

            if (elapsedTime > 0)
            {
                // Work out how fast we are moving.
                Vector3 velocity = (newPosition - mPreviousPosition) / elapsedTime;

                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = mTimeLeftOver + elapsedTime;

                // Counter for looping over the time interval.
                float currentTime = -mTimeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > mTimeBetweenParticles)
                {
                    currentTime += mTimeBetweenParticles;
                    timeToSpend -= mTimeBetweenParticles;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedTime;

                    Vector3 position = Vector3.Lerp(mPreviousPosition, newPosition, mu);

                    // Create the particle.
                    mParticleSystem.AddParticle(position, velocity);
                }

                // Store any time we didn't use, so it can be part of the next update.
                mTimeLeftOver = timeToSpend;
            }

            mPreviousPosition = newPosition;
        }

        private void TargetDespawningHandler(object sender, EventArgs e)
        {
            mTargetActorId = Actor.INVALID_ACTOR_ID;
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }
}
