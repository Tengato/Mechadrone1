using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;

namespace Mechadrone1
{
    class EnergyProjectile : Behavior
    {
        private float mSpeed;
        private int mDamage;
        private float mTimeAlive;
        private float mOrigScale;
        private int mOwnerActorId;

        public EnergyProjectile(Actor owner)
            : base(owner)
        {
            mSpeed = 145.0f;
            mTimeAlive = 0.0f;
            mOrigScale = 1.0f;
            mDamage = 10;
            mOwnerActorId = Actor.INVALID_ACTOR_ID;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.SPEED))
                mSpeed = (float)(manifest.Properties[ManifestKeys.SPEED]);

            if (manifest.Properties.ContainsKey(ManifestKeys.DAMAGE))
                mDamage = (int)(manifest.Properties[ManifestKeys.DAMAGE]);

            Owner.ComponentsCreated += ComponentsCreatedHandler;
            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            DynamicCollisionComponent cc = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            if (cc == null)
                throw new LevelManifestException("Expected ActorComponent missing.");

            TransformComponent tc = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            if (tc == null)
                throw new LevelManifestException("Expected ActorComponent missing.");

            cc.Entity.IsAffectedByGravity = false;
            cc.Entity.CollisionInformation.Events.InitialCollisionDetected += CollisionDetectedHandler;

            mOrigScale = tc.Scale;
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            mTimeAlive += (float)(e.GameTime.ElapsedGameTime.TotalSeconds);

            float lifeMeasure = mTimeAlive * mSpeed;

            if (lifeMeasure > 335.0f)
            {
                Owner.Despawn();
            }
            else if (lifeMeasure > 285.0f)
            {
                TransformComponent tc = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
                tc.Scale = MathHelper.Lerp(mOrigScale, 0.01f, (lifeMeasure - 285.0f) / 50.0f);
            }
        }

        private void CollisionDetectedHandler(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            EntityCollidable otherEntityCollidable = other as EntityCollidable;
            Terrain otherTerrain = other as Terrain;
            if (otherEntityCollidable != null &&
                otherEntityCollidable.Entity != null &&
                otherEntityCollidable.Entity.Tag != null)
            {
                int actorId = (int)(otherEntityCollidable.Entity.Tag);
                if (actorId == mOwnerActorId)
                    return;
                Actor actorHit = GameResources.ActorManager.GetActorById(actorId);
                IDamagable damage = actorHit.GetBehaviorThatImplementsType<IDamagable>();
                if (damage != null)
                {
                    damage.TakeDamage(mDamage);
                }

                Impact();
            }
            else if (otherTerrain != null)
            {
                Impact();
            }
        }

        public void Propel(int ownerActorId)
        {
            mOwnerActorId = ownerActorId;
            DynamicCollisionComponent collisionComponent = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            BEPUutilities.Vector3 forward = collisionComponent.Entity.OrientationMatrix.Forward * mSpeed;
            collisionComponent.Entity.ApplyLinearImpulse(ref forward);
        }

        public void Impact()
        {
            // We hijack this member to make sure we only run the rest of this method once.
            if (mTimeAlive < 0)
                return;

            mTimeAlive = -1.0f;

            // TODO: P2: Some whizzzamp effects...
            Owner.Despawn();
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }
}
