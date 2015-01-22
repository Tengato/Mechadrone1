using System;
using Microsoft.Xna.Framework;
using SlagformCommon;
using BepuRay = BEPUutilities.Ray;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class MachineGunSkill : BipedWeapon
    {
        static public WeaponState ReadyState { get; private set; }
        static public WeaponState FiringState { get; private set; }
        static public WeaponState DryFireState { get; private set; }

        public int Damage { get; set; }

        static MachineGunSkill()
        {
            ReadyState = new MachineGunReadyState();
            FiringState = new MachineGunFiringState();
            DryFireState = new MachineGunDryFiringState();
        }

        public MachineGunSkill(int ownerActorId)
            : base(ownerActorId)
        {
            // TODO: P2: Activation and weapon switching is not quite clear...
            CurrentState = ReadyState;
            // TODO: P2: This matrix should come from a component that knows about the biped's model?
            FirePoint = Matrix.CreateTranslation(5.0f, 3.0f, -3.0f);
            Damage = 1;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);
            Damage = (int)(manifest.Properties[ManifestKeys.DAMAGE]);
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
            // Play 'pop' sound
            Actor owner = GameResources.ActorManager.GetActorById(OwnerActorId);
            WeaponResource wr = owner.GetBehavior<WeaponResource>();
            wr.Value -= ResourceCostToUse;
            BipedControllerComponent bipedControl = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            Matrix muzzleTransform = FirePoint * Matrix.CreateWorld(BepuConverter.Convert(bipedControl.Controller.Body.Position),
                BepuConverter.Convert(bipedControl.Controller.ViewDirection), Vector3.Up);
            BepuRay shootRay = new BepuRay(BepuConverter.Convert(muzzleTransform.Translation), BepuConverter.Convert(muzzleTransform.Forward));
            RayCastResult result;

            GameResources.ActorManager.SimSpace.RayCast(shootRay, 500.0f, out result);

            EntityCollidable otherEntityCollidable = result.HitObject as EntityCollidable;
            Terrain otherTerrain = result.HitObject as Terrain;
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
                HitSparks(result.HitData.Location);
            }
            else if (otherTerrain != null)
            {
                HitSparks(result.HitData.Location);
            }
        }

        private void HitSparks(BEPUutilities.Vector3 position)
        {
            Actor sparks = GameResources.ActorManager.SpawnTemplate("Sparks");
            TransformComponent sparksXForm = sparks.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            sparksXForm.Translation = BepuConverter.Convert(position);
            Sparks sparksBehavior = sparks.GetBehavior<Sparks>();
            sparksBehavior.Emit();
        }


        public override void DryFire()
        {
            // Play 'click' sound
        }
    }

    class MachineGunReadyState : WeaponState
    {
        public override bool RequiresAttention { get { return false; } }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (input == WeaponFunctions.TriggerPulled)
            {
                Actor owner = GameResources.ActorManager.GetActorById(weapon.OwnerActorId);
                WeaponResource wr = owner.GetBehavior<WeaponResource>();
                if (wr.Value >= weapon.ResourceCostToUse)
                {
                    weapon.Fire();
                    weapon.CurrentState = MachineGunSkill.FiringState;
                }
                else
                {
                    weapon.DryFire();
                    weapon.CurrentState = MachineGunSkill.DryFireState;
                }
            }
        }
    }


    class MachineGunFiringState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public MachineGunFiringState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 0.04f);
        }


        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = MachineGunSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }


    class MachineGunDryFiringState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public MachineGunDryFiringState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 0.05f);
        }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = MachineGunSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }
}
