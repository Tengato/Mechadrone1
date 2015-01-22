using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using SlagformCommon;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.CollisionShapes.ConvexShapes;
using RigidTransform = BEPUutilities.RigidTransform;
using BepuRay = BEPUutilities.Ray;
using BepuVec3 = BEPUutilities.Vector3;
using BEPUphysicsDemos.AlternateMovement;

namespace Mechadrone1
{
    class Rocket : Behavior
    {
        private float mSpeed;
        private int mDamage;
        private float mTimeAlive;
        private float mOrigScale;
        private float mBlastRadius;

        public Rocket(Actor owner)
            : base(owner)
        {
            mSpeed = 65.0f;
            mTimeAlive = 0.0f;
            mOrigScale = 1.0f;
            mDamage = 60;
            mBlastRadius = 20.0f;
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

            if (lifeMeasure > 435.0f)
            {
                Owner.Despawn();
            }
            else if (lifeMeasure > 285.0f)
            {
                DynamicCollisionComponent collisionComponent = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
                collisionComponent.Entity.IsAffectedByGravity = true;
            }
        }

        private void CollisionDetectedHandler(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            EntityCollidable otherEntityCollidable = other as EntityCollidable;
            Terrain otherTerrain = other as Terrain;
            if (otherTerrain != null ||
                (otherEntityCollidable != null &&
                otherEntityCollidable.Entity != null &&
                otherEntityCollidable.Entity.Tag != null))
            {
                GameResources.ActorManager.PostPhysicsUpdateStep += ImpactHandler;
            }
        }

        public void Propel()
        {
            DynamicCollisionComponent collisionComponent = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            BEPUutilities.Vector3 forward = collisionComponent.Entity.OrientationMatrix.Forward * mSpeed;
            collisionComponent.Entity.ApplyLinearImpulse(ref forward);

            Actor contrailActor = GameResources.ActorManager.SpawnTemplate("Contrail");
            Contrail contrailBehavior = contrailActor.GetBehavior<Contrail>();
            contrailBehavior.OffsetFromTarget = Vector3.Backward;
            contrailBehavior.SetTarget(Owner.Id);

            // TODO: P2: Add a launch cloud and a firey tail, and some acceleration
        }

        public void ImpactHandler(object sender, UpdateStepEventArgs e)
        {
            GameResources.ActorManager.PostPhysicsUpdateStep -= ImpactHandler;
            // TODO: P2: Some boooom sound effects...

            TransformComponent myXForm = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            // Do a sphere cast to get actors in the blast radius
            List<RayCastResult> inRangeActors = new List<RayCastResult>();
            SphereShape blastZone = new SphereShape(mBlastRadius);
            RigidTransform blastPosition = new RigidTransform(BepuConverter.Convert(myXForm.Translation));
            BEPUutilities.Vector3 bepuZero = BEPUutilities.Vector3.Zero;
            GameResources.ActorManager.SimSpace.ConvexCast(blastZone, ref blastPosition, ref bepuZero, inRangeActors);

            RayCastDistanceComparer rcdc = new RayCastDistanceComparer();

            for (int a = 0; a < inRangeActors.Count; ++a)
            {
                EntityCollidable inRangeEntityCollidable = inRangeActors[a].HitObject as EntityCollidable;
                if (inRangeEntityCollidable != null &&
                    inRangeEntityCollidable.Entity != null &&
                    inRangeEntityCollidable.Entity.Tag != null)
                {
                    Actor blastedActor = GameResources.ActorManager.GetActorById((int)(inRangeEntityCollidable.Entity.Tag));
                    IDamagable actorDamage = blastedActor.GetBehaviorThatImplementsType<IDamagable>();
                    DynamicCollisionComponent actorCollidable = blastedActor.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
                    BepuVec3 blastToActorCenter = actorCollidable.Entity.Position - blastPosition.Position;
                    BepuRay loeRay = new BepuRay(blastPosition.Position, blastToActorCenter);
                    bool hasCover = false;
                    float distance = mBlastRadius;
                    if (actorDamage != null || (actorCollidable != null && actorCollidable.IsDynamic && !(actorCollidable.Entity.CollisionInformation.CollisionRules.Personal.HasFlag(BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver))))
                    {
                        List<RayCastResult> loeResults = new List<RayCastResult>();
                        GameResources.ActorManager.SimSpace.RayCast(loeRay, mBlastRadius, loeResults);
                        loeResults.Sort(rcdc);

                        for (int c = 0; c < loeResults.Count; ++c)
                        {
                            EntityCollidable possibleCover = loeResults[c].HitObject as EntityCollidable;
                            if (possibleCover != null &&
                                possibleCover.Entity == inRangeEntityCollidable.Entity)
                            {
                                // Hit
                                distance = loeResults[c].HitData.T;
                                break;
                            }
                            Terrain possibleCoverTerrain = loeResults[c].HitObject as Terrain;
                            if (possibleCoverTerrain != null)
                            {
                                hasCover = true;
                                break;
                            }
                            if (possibleCover != null &&
                                possibleCover.Entity != null &&
                                !possibleCover.Entity.IsDynamic)
                            {
                                hasCover = true;
                                break;
                            }
                        }
                    }

                    if (!hasCover && actorDamage != null)
                    {

                        actorDamage.TakeDamage((int)(MathHelper.Lerp(1.0f, 0.25f, distance / mBlastRadius) * mDamage));
                    }

                    if (!hasCover && actorCollidable != null && actorCollidable.IsDynamic && !(actorCollidable.Entity.CollisionInformation.CollisionRules.Personal.HasFlag(BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver)))
                    {
                        blastToActorCenter.Normalize();
                        blastToActorCenter = blastToActorCenter * 1200; // Math.Min(5000.0f / (distance + 1.0f));
                        actorCollidable.Entity.ApplyLinearImpulse(ref blastToActorCenter);
                        if (!actorCollidable.Entity.ActivityInformation.IsActive)
                            actorCollidable.Entity.ActivityInformation.Activate();
                    }
                }
            }

            DynamicCollisionComponent dcc = Owner.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            Vector3 myVelocity = BepuConverter.Convert(dcc.Entity.LinearVelocity);
            Actor fireball = GameResources.ActorManager.SpawnTemplate("ExplosionFire");
            TransformComponent fireXForm = fireball.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            fireXForm.Translation = myXForm.Translation;
            ExplosionFire fireBehavior = fireball.GetBehavior<ExplosionFire>();
            fireBehavior.Emit(myVelocity);
            Actor smoke = GameResources.ActorManager.SpawnTemplate("ExplosionSmoke");
            TransformComponent smokeXForm = smoke.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            smokeXForm.Translation = myXForm.Translation;
            ExplosionSmoke smokeBehavior = smoke.GetBehavior<ExplosionSmoke>();
            smokeBehavior.Emit(myVelocity);

            Owner.Despawn();
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }

    public class RayCastDistanceComparer : IComparer<RayCastResult>
    {
        public int Compare(RayCastResult a, RayCastResult b)
        {
            return a.HitData.T.CompareTo(b.HitData.T);
        }
    }
}
