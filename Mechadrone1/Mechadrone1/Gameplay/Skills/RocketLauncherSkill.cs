using System;
using Microsoft.Xna.Framework;
using SlagformCommon;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class RocketLauncherSkill : BipedWeapon
    {
        static public WeaponState ReadyState { get; private set; }
        static public WeaponState FiringState { get; private set; }
        static public WeaponState DryFireState { get; private set; }

        static RocketLauncherSkill()
        {
            ReadyState = new RocketLauncherReadyState();
            FiringState = new RocketLauncherFiringState();
            DryFireState = new RocketLauncherDryFiringState();
        }

        public string ProjectileTemplateName { get; set; }

        public RocketLauncherSkill(int ownerActorId)
            : base(ownerActorId)
        {
            // TODO: P2: Activation and weapon switching is not quite clear...
            CurrentState = ReadyState;
            ProjectileTemplateName = String.Empty;
            // TODO: P2: This matrix should come from a component that knows about the biped's model?
            FirePoint = Matrix.CreateTranslation(5.0f, 3.0f, -3.0f);
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
            // Play 'shwoomph' sound
            Actor rocket = GameResources.ActorManager.SpawnTemplate(ProjectileTemplateName);
            Actor owner = GameResources.ActorManager.GetActorById(OwnerActorId);
            BipedControllerComponent bipedControl = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            TransformComponent rocketXform = rocket.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            rocketXform.Transform = FirePoint * Matrix.CreateWorld(BepuConverter.Convert(bipedControl.Controller.Body.Position),
                BepuConverter.Convert(bipedControl.Controller.ViewDirection), Vector3.Up);
            Rocket boltProj = rocket.GetBehavior<Rocket>();
            boltProj.Propel();
        }

        public override void DryFire()
        {
            // Play 'click' sound
        }
    }


    class RocketLauncherReadyState : WeaponState
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
                    weapon.CurrentState = RocketLauncherSkill.FiringState;
                }
                else
                {
                    weapon.DryFire();
                    weapon.CurrentState = RocketLauncherSkill.DryFireState;
                }
            }
        }
    }


    class RocketLauncherFiringState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public RocketLauncherFiringState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 2.0f);
        }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = RocketLauncherSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }


    class RocketLauncherDryFiringState : WeaponState
    {
        public override bool RequiresAttention { get { return true; } }

        public RocketLauncherDryFiringState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 0.05f);
        }

        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= mDurationTicks)
            {
                weapon.CurrentState = RocketLauncherSkill.ReadyState;
                ResetStateTime(gameTime, weapon, WeaponFunctions.Neutral);
            }
        }
    }
}
