using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Mechadrone1.Gameplay.Prefabs;

namespace Mechadrone1.Gameplay
{
    abstract class BipedWeapon
    {
        abstract public TPPedestrian.BipedArmStates CurrentArmState { get; }
        public WeaponFunctions CurrentOperation { get; set; }

        protected WeaponState currentState;
        public WeaponState CurrentState {
            get
            {
                return currentState;
            }
            set
            {
                if (value != currentState)
                {
                    currentState = value;
                    TimeInState = TimeSpan.Zero;
                }
            }
        }

        public TimeSpan TimeInState { get; set; }

        public int LoadedAmmo { get; set; }

        public int ReserveAmmo { get; set; }

        public int ClipSize { get; set; }

        public Matrix FirePoint { get; set; }

        protected IGameManager gameManager;
        protected GameObject owner;


        public BipedWeapon(IGameManager game, GameObject owner)
        {
            gameManager = game;
            this.owner = owner;
        }


        public virtual void PreAnimationUpdate(object sender, UpdateStepEventArgs e)
        {
            TimeInState += e.GameTime.ElapsedGameTime;
            CurrentState.Update(e.GameTime, this, CurrentOperation);
        }


        public virtual void AnimationUpdate(object sender, UpdateStepEventArgs e)
        {
        }


        public virtual void Fire()
        {
        }


        public virtual void DryFire()
        {
        }


        public virtual void BeginReload()
        {
        }


        public virtual void CompleteReload()
        {
        }
    }


    enum WeaponFunctions
    {
        Neutral,
        TriggerPulled,
        Reloading,
        Activating,
    }
}
