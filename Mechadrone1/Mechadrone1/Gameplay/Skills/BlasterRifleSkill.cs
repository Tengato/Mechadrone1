using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using SlagformCommon;

namespace Mechadrone1
{
    class BlasterRifleSkill : BipedWeapon
    {
        static public WeaponState ReadyState { get; private set; }
        static public WeaponState FiringState { get; private set; }
        static public WeaponState DryFireState { get; private set; }

        static BlasterRifleSkill()
        {
            ReadyState = new BlasterRifleReadyState();
            FiringState = new BlasterRifleFiringState();
            DryFireState = new BlasterRifleDryFiringState();
        }

        public string ProjectileTemplateName { get; set; }

        public BlasterRifleSkill(int ownerActorId)
            : base(ownerActorId)
        {
            // TODO: P2: Activation and weapon switching is not quite clear...
            CurrentState = ReadyState;
            ProjectileTemplateName = String.Empty;
            // TODO: P2: This matrix should come from a component that knows about the biped's model?
            MuzzleOffset = new Vector3(1.2f, 1.3f, -3.0f);
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);
            ProjectileTemplateName = (string)(manifest.Properties[ManifestKeys.PROJECTILE_NAME]);
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
            // Play 'pew' sound
            Actor bolt = GameResources.ActorManager.SpawnTemplate(ProjectileTemplateName);
            Actor owner = GameResources.ActorManager.GetActorById(OwnerActorId);
            BipedControllerComponent bipedControl = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            TransformComponent boltXform = bolt.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            Vector3 aim = (bipedControl.WorldAim.HasValue ? bipedControl.WorldAim.Value :
                BepuConverter.Convert(bipedControl.Controller.ViewDirection));
            boltXform.Transform = Matrix.CreateTranslation(MuzzleOffset) * Matrix.CreateWorld(BepuConverter.Convert(
                bipedControl.Controller.Body.Position), aim, Vector3.Up);
            EnergyProjectile boltProj = bolt.GetBehavior<EnergyProjectile>();
            boltProj.Propel(OwnerActorId);
        }


        public override void DryFire()
        {
            // Play 'click' sound
        }
    }


    class BlasterRifleReadyState : WeaponState
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
                    weapon.CurrentState = BlasterRifleSkill.FiringState;
                }
                else
                {
                    weapon.DryFire();
                    weapon.CurrentState = BlasterRifleSkill.DryFireState;
                }
            }
        }
    }


    class BlasterRifleFiringState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public BlasterRifleFiringState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 0.08f);
        }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = BlasterRifleSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }


    class BlasterRifleDryFiringState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public BlasterRifleDryFiringState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 0.05f);
        }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = BlasterRifleSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }
}
