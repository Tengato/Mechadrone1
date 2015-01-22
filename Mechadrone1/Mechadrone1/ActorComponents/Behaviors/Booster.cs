using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class Booster : Behavior
    {
        private BipedControllerComponent mBipedControl;

        // Times are in seconds.
        public float MinDuration { get; set; }
        public float MaxDuration { get; set; }
        public float BoostTimeRemaining { get; set; }
        public float RechargeRate { get; set; }
        public float Speed { get; set; }
        public float TurnLimit { get; set; }
        private float mBoostTime { get; set; }
        private float mRegularSpeed;
        private float mRegularTurnLimit;

        public bool BoostReady { get { return BoostTimeRemaining > 1.0f; } }

        public Booster(Actor owner)
            : base(owner)
        {
            MinDuration = 0.33333333f;
            MaxDuration = 4.0f;
            BoostTimeRemaining = MaxDuration;
            RechargeRate = 1.0f;
            Speed = 128.0f;
            TurnLimit = MathHelper.Pi / 80.0f;
            mBoostTime = 0.0f;
            mRegularSpeed = 1.0f;
            mRegularTurnLimit = 1.0f;
            Owner.ComponentsCreated += ComponentsCreatedHandler;
            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
            GameResources.ActorManager.PostPhysicsUpdateStep += PostPhysicsUpdateHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            mBipedControl = Owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            if (mBipedControl == null)
                throw new LevelManifestException("Expected ActorComponent missing.");

            mBipedControl.StateChanged += ControllerStateChangedHandler;
        }

        private void ControllerStateChangedHandler(object sender, ControllerStateChangedEventArgs e)
        {
            if (e.NewState == BipedControllerComponent.ControllerState.Boosting)
            {
                mBoostTime = 0.0f;
                mRegularSpeed = mBipedControl.RunSpeed;
                mBipedControl.RunSpeed = Speed;
                mRegularTurnLimit = mBipedControl.MaxTurnAnglePerTick;
                mBipedControl.MaxTurnAnglePerTick = TurnLimit;
            }
            else if (e.OldState == BipedControllerComponent.ControllerState.Boosting)
            {
                mBipedControl.RunSpeed = mRegularSpeed;
                mBipedControl.MaxTurnAnglePerTick = mRegularTurnLimit;
            }
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            if (mBipedControl.State ==  BipedControllerComponent.ControllerState.Boosting && mBoostTime < MinDuration)
            {
                // Keep boost on.
                mBipedControl.DesiredMovementActions |= BipedControllerComponent.MovementActions.Boosting;
            }
            else if (BoostTimeRemaining <= 0.0f)
            {
                // Force boost off.
                mBipedControl.DesiredMovementActions &= ~BipedControllerComponent.MovementActions.Boosting;
            }
        }

        private void PostPhysicsUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            float elapsedTime = (float)(e.GameTime.ElapsedGameTime.TotalSeconds);
            if (mBipedControl.State == BipedControllerComponent.ControllerState.Boosting)
            {
                mBoostTime += elapsedTime;
                BoostTimeRemaining -= elapsedTime;
            }
            else if (BoostTimeRemaining < MaxDuration)
            {
                BoostTimeRemaining += elapsedTime;
            }
        }

        public override void Release()
        {
            Owner.ComponentsCreated -= ComponentsCreatedHandler;
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
            GameResources.ActorManager.PostPhysicsUpdateStep -= PostPhysicsUpdateHandler;
            mBipedControl.StateChanged -= ControllerStateChangedHandler;
        }
    }
}
