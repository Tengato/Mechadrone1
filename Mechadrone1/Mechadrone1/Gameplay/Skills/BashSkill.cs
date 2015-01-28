using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using SlagformCommon;
using BepuRay = BEPUutilities.Ray;
using RigidTransform = BEPUutilities.RigidTransform;
using BepuVec3 = BEPUutilities.Vector3;
using BepuQuaternion = BEPUutilities.Quaternion;
using BEPUphysics;
using BEPUphysics.CollisionShapes.ConvexShapes;
using System.Collections.Generic;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

namespace Mechadrone1
{
    class BashSkill : BipedWeapon
    {
        static public WeaponState ReadyState { get; private set; }
        static public WeaponState SwingingState { get; private set; }

        public int Damage { get; set; }

        static BashSkill()
        {
            ReadyState = new BashReadyState();
            SwingingState = new BashSwingingState();
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);
            Damage = (int)(manifest.Properties[ManifestKeys.DAMAGE]);
        }

        public BashSkill(int ownerActorId)
            : base(ownerActorId)
        {
            // TODO: P2: Activation and weapon switching is not quite clear...
            CurrentState = ReadyState;
            // TODO: P2: This matrix should come from a component that knows about the biped's model?
            MuzzleOffset = new Vector3(1.2f, 1.3f, -3.0f);
            EffectiveRangeMax = 9.0f;
            EffectiveRangeMin = 0.0f;
        }

        public override BipedControllerComponent.OtherActions CurrentArmState
        {
            get
            {
                // TODO: P2: Decide what behavior is needed here:
                return BipedControllerComponent.OtherActions.Neutral;
            }
        }

        public override void Fire()
        {
            const float ATTACK_RADIUS = 3.0f;
            const float ATTACK_LENGTH = 4.0f;
            // Play 'thwack' sound
            Actor owner = GameResources.ActorManager.GetActorById(OwnerActorId);
            BipedControllerComponent bipedControl = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            RigidTransform alignCapsule = new RigidTransform(BepuVec3.Forward * ATTACK_LENGTH * 0.5f + BepuConverter.Convert(MuzzleOffset),
                BepuQuaternion.CreateFromAxisAngle(BepuVec3.Right, MathHelper.PiOver2));

            Vector3 aim = (bipedControl.WorldAim.HasValue ? bipedControl.WorldAim.Value :
                BepuConverter.Convert(bipedControl.Controller.ViewDirection));
            RigidTransform positionAndAim = new RigidTransform(bipedControl.Controller.Body.Position, BepuConverter.Convert(
                SpaceUtils.GetOrientation(aim, Vector3.Up)));

            RigidTransform attackTransform;
            RigidTransform.Transform(ref alignCapsule, ref positionAndAim, out attackTransform);

            ConvexShape bashShape = new CapsuleShape(ATTACK_LENGTH, ATTACK_RADIUS);
            BepuVec3 noSweep = BepuVec3.Zero;
            List<RayCastResult> dudesBashed = new List<RayCastResult>();
            AttackFilter filter = new AttackFilter(GameResources.ActorManager.IsMob(OwnerActorId));

            GameResources.ActorManager.SimSpace.ConvexCast(bashShape, ref attackTransform, ref noSweep, filter.Test, dudesBashed);

            foreach (RayCastResult dude in dudesBashed)
            {
                EntityCollidable otherEntityCollidable = dude.HitObject as EntityCollidable;
                Terrain otherTerrain = dude.HitObject as Terrain;
                if (otherEntityCollidable != null &&
                    otherEntityCollidable.Entity != null &&
                    otherEntityCollidable.Entity.Tag != null)
                {
                    Actor actorHit = GameResources.ActorManager.GetActorById((int)(otherEntityCollidable.Entity.Tag));
                    IDamagable damage = actorHit.GetBehaviorThatImplementsType<IDamagable>();
                    if (damage != null)
                    {
                        damage.TakeDamage(Damage);
                        // TODO: P2: Query hit actor for appropiate damage effect e.g. blood and create it;
                    }
                    BashDust(dude.HitData.Location);
                }
                else if (otherTerrain != null)
                {
                    BashDust(dude.HitData.Location);
                }
            }
        }

        private void BashDust(BEPUutilities.Vector3 position)
        {
            Actor dust = GameResources.ActorManager.SpawnTemplate("Dust");
            TransformComponent dustXForm = dust.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            dustXForm.Translation = BepuConverter.Convert(position);

            Sparks sparksBehavior = dust.GetBehavior<Sparks>();
            sparksBehavior.Emit();
        }
    }

    class AttackFilter
    {
        private bool mLookForPlayers;

        public AttackFilter(bool lookForPlayers)
        {
            mLookForPlayers = lookForPlayers;
        }

        public bool Test(BroadPhaseEntry test)
        {
            // TODO: P2: We want to be able to bash objects and terrain to create dust, decals, sounds, smash crates, etc.
            bool isFoe = false;

            EntityCollidable ec = test as EntityCollidable;
            if (ec != null &&
                ec.Entity != null &&
                ec.Entity.Tag != null)
            {
                int viewedActorId = (int)(ec.Entity.Tag);
                isFoe = mLookForPlayers ? GameResources.ActorManager.IsPlayer(viewedActorId) :
                    GameResources.ActorManager.IsMob(viewedActorId);
            }

            return isFoe;
        }
    }

    class BashReadyState : WeaponState
    {
        public override bool RequiresAttention { get { return false; } }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (input == WeaponFunctions.TriggerPulled)
            {
                weapon.Fire();
                weapon.CurrentState = BashSkill.SwingingState;
            }
        }
    }


    class BashSwingingState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public BashSwingingState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 1.0f);
        }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = BashSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }
}
