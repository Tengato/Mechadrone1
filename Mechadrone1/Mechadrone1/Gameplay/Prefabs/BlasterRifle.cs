using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1.Gameplay.Prefabs
{
    class BlasterRifle : BipedWeapon
    {
        static public WeaponState ActivatingState { get; private set; }
        static public WeaponState ReadyState { get; private set; }
        static public WeaponState FiringState { get; private set; }
        static public WeaponState ReloadingState { get; private set; }
        static public WeaponState DryFireState { get; private set; }

        protected BlasterBolt templateBolt;


        static BlasterRifle()
        {
            ActivatingState = new BlasterRifleActivatingState();
            ReadyState = new BlasterRifleReadyState();
            FiringState = new BlasterRifleFiringState();
            ReloadingState = new BlasterRifleReloadingState();
            DryFireState = new BlasterRifleDryFiringState();
        }


        public BlasterRifle(IGameManager game, GameObject owner) : base(game, owner)
        {
            // TODO: Activation and weapon switching is not quite clear...
            CurrentState = ActivatingState;
            FirePoint = Matrix.CreateTranslation(-1.2f, 7.3f, 4.0f);
            ClipSize = 48;

            templateBolt = game.GetGameObject("BlasterBoltTemplate") as BlasterBolt;
        }


        public override TPPedestrian.BipedArmStates CurrentArmState
        {
            get
            {
                // TODO: Decide what behavior is needed here:
                return TPPedestrian.BipedArmStates.Neutral;
            }
        }


        public override void Fire()
        {
            // Play 'pew' sound
            LoadedAmmo--;
            BlasterBolt bolt = new BlasterBolt(templateBolt);
            bolt.Name = DateTime.Now.Ticks.ToString();
            Matrix boltTransform = FirePoint * owner.WorldTransform;
            bolt.Position = boltTransform.Translation;
            bolt.Orientation = Quaternion.CreateFromRotationMatrix(boltTransform);
            bolt.Initialize();
            gameManager.SpawnInitializedObject(bolt);
        }


        public override void DryFire()
        {
            // Play 'click' sound
        }


        public override void BeginReload()
        {
            // Play 'kla-chuk' sound
        }


        public override void CompleteReload()
        {
            int ammoMoved = Math.Min(ClipSize - LoadedAmmo, ReserveAmmo);
            LoadedAmmo += ammoMoved;
            ReserveAmmo -= ammoMoved;
        }

    }


    class BlasterRifleActivatingState : WeaponState
    {
        public BlasterRifleActivatingState()
        {
            durationTicks = (long)(TimeSpan.TicksPerSecond * 1.0f);
        }


        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= durationTicks)
            {
                weapon.CurrentState = BlasterRifle.ReadyState;

                ResetStateTime(gameTime, weapon, input);
            }
        }
    }


    class BlasterRifleReadyState : WeaponState
    {
        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (input == WeaponFunctions.TriggerPulled)
            {
                if (weapon.LoadedAmmo > 0)
                {
                    weapon.Fire();
                    weapon.CurrentState = BlasterRifle.FiringState;
                }
                else
                {
                    weapon.DryFire();
                    weapon.CurrentState = BlasterRifle.DryFireState;
                }
            }
            else if (input == WeaponFunctions.Reloading && weapon.LoadedAmmo < weapon.ClipSize)
            {
                weapon.BeginReload();
                weapon.CurrentState = BlasterRifle.ReloadingState;
            }
        }
    }


    class BlasterRifleFiringState : WeaponState
    {
        public BlasterRifleFiringState()
        {
            durationTicks = (long)(TimeSpan.TicksPerSecond * 0.08f);
        }


        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= durationTicks)
            {
                if (weapon.LoadedAmmo <= 0)
                {
                    if (weapon.ReserveAmmo > 0)
                    {
                        weapon.CurrentState = BlasterRifle.ReloadingState;
                    }
                    else
                    {
                        weapon.CurrentState = BlasterRifle.ReadyState;
                    }
                }
                else
                {
                    weapon.CurrentState = BlasterRifle.ReadyState;
                }

                ResetStateTime(gameTime, weapon, input);
            }
        }
    }


    class BlasterRifleReloadingState : WeaponState
    {
        public BlasterRifleReloadingState()
        {
            durationTicks = (long)(TimeSpan.TicksPerSecond * 0.5f);
        }


        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= durationTicks)
            {
                weapon.CompleteReload();
                weapon.CurrentState = BlasterRifle.ReadyState;

                ResetStateTime(gameTime, weapon, input);
            }
        }
    }


    class BlasterRifleDryFiringState : WeaponState
    {
        public BlasterRifleDryFiringState()
        {
            durationTicks = (long)(TimeSpan.TicksPerSecond * 0.05f);
        }


        public override void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            if (weapon.TimeInState.Ticks >= durationTicks)
            {
                weapon.CurrentState = BlasterRifle.ReadyState;

                ResetStateTime(gameTime, weapon, input);
            }
        }
    }
}
