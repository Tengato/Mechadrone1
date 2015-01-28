using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    enum WeaponFunctions
    {
        Neutral,
        TriggerPulled,
        Reloading,
        Activating,
    }

    /// <summary>
    /// An attack skill whose usage can be modeled with a push button (such as a firearm with a trigger)
    /// </summary>
    abstract class BipedWeapon : ISkill
    {
        public WeaponFunctions CurrentOperation { get; set; }
        public TimeSpan TimeInState { get; set; }
        public Vector3 MuzzleOffset { get; set; }
        public int OwnerActorId { get; private set; }
        public float ResourceCostToUse { get; set; }
        private EventHandler mReturnAttention;

        protected WeaponState mCurrentState;
        public WeaponState CurrentState {
            get
            {
                return mCurrentState;
            }
            set
            {
                if (value != mCurrentState)
                {
                    mCurrentState = value;
                    TimeInState = TimeSpan.Zero;
                    if (!mCurrentState.RequiresAttention && mReturnAttention != null)
                        OnDoneWithAttention();
                }
            }
        }
        public float EffectiveRangeMin { get; set; }
        public float EffectiveRangeMax { get; set; }

        abstract public BipedControllerComponent.OtherActions CurrentArmState { get; }

        public BipedWeapon(int ownerActorId)
        {
            OwnerActorId = ownerActorId;
            CurrentOperation = WeaponFunctions.Neutral;
            TimeInState = TimeSpan.Zero;
            MuzzleOffset = Vector3.Zero;
            ResourceCostToUse = 0.01f;
            mReturnAttention = null;
            EffectiveRangeMax = 120.0f;
            EffectiveRangeMin = 6.0f;
        }

        public virtual void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            ResourceCostToUse = (float)(manifest.Properties[ManifestKeys.RESOURCE_COST_TO_USE]);
            // TODO: P2: Init range info.
            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdate;
        }

        public void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdate;
        }

        public virtual void PreAnimationUpdate(object sender, UpdateStepEventArgs e)
        {
            TimeInState += e.GameTime.ElapsedGameTime;
            CurrentState.Update(e.GameTime, this, CurrentOperation);
        }

        public virtual void Fire() { }

        public virtual void DryFire() { }

        public void UpdateInputState(bool inputState, BipedControllerComponent bipedController)
        {
            if (inputState)
            {
                if (mReturnAttention == null)
                {
                    mReturnAttention = bipedController.TryGetAttention();
                    if (mReturnAttention != null)
                    {
                        CurrentOperation = WeaponFunctions.TriggerPulled;
                    }
                    else
                    {
                        CurrentOperation = WeaponFunctions.Neutral;
                    }
                }
                else
                {
                    CurrentOperation = WeaponFunctions.TriggerPulled;
                }
            }
            else if (CurrentOperation == WeaponFunctions.TriggerPulled)
            {
                CurrentOperation = WeaponFunctions.Neutral;
            }
        }

        protected void OnDoneWithAttention()
        {
            mReturnAttention(this, EventArgs.Empty);
            mReturnAttention = null;
        }
    }
}
