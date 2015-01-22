using System;
using BEPUphysicsDemos.AlternateMovement.Character;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SlagformCommon;
using Skelemator;
using BepuVec3 = BEPUutilities.Vector3;
using System.Diagnostics;

namespace Mechadrone1
{

    delegate void ControllerStateChangedEventHandler(object sender, ControllerStateChangedEventArgs e);

    class ControllerStateChangedEventArgs : EventArgs
    {
        public BipedControllerComponent.ControllerState NewState { get; set; }
        public BipedControllerComponent.ControllerState OldState { get; set; }

        public ControllerStateChangedEventArgs(BipedControllerComponent.ControllerState newState,
                                              BipedControllerComponent.ControllerState oldState)
            : base()
        {
            NewState = newState;
            OldState = oldState;
        }
    }

    // The term controller here is used in the abstract sense, it's not referring to a 'gamepad'
    class BipedControllerComponent : ActorComponent
    {
        protected const string JUMPING_SN = "Jumping";
        protected const string BOOSTING_SN = "Boosting";
        protected const string CROUCHING_SN = "Crouching";
        protected const string CROUCHMOVING_SN = "CrouchMoving";
        protected const string MOVING_SN = "Moving";
        protected const string STANDING_SN = "Standing";
        protected const string FALLING_SN = "Falling";
        protected const string KNOCKED_DOWN_SN = "KnockedDown";

        [Flags]
        public enum MovementActions
        {
            Neutral = 0x00,
            Crouching = 0x01,
            Jumping = 0x02,
            Boosting = 0x04,
        }

        [Flags]
        public enum OtherActions
        {
            Neutral = 0x00,
            PointingWeapon = 0x01,
            Throwing = 0x02,
        }

        public enum ControllerState
        {
            Neutral,
            InputDisabled,
            Boosting,
        }

        public MovementActions DesiredMovementActions { get; set; }
        public Quaternion OrientationChange { get; set; }
        /// <summary>
        /// Normalized xz movement from the Controller view direction perspective. +Y is forward, +X is right.
        /// </summary>
        public Vector2 HorizontalMovement { get; set; }
        // Ownership of the Controller is shared with the CollisionComponent - the only other object that should modify this member.
        public CharacterController Controller { get; private set; }
        public float MaxTurnAnglePerTick { get; set; }
        public event ControllerStateChangedEventHandler StateChanged;
        private AnimationStateMachine mAnimationStateMachine;
        private ControllerState mState;
        private Booster mBooster;
        public BoolMethodDelegate AimCheck;
        private Object mAttentionLock;
        public bool IsAttentionAvailable { get; private set; }
        public Vector3? WorldAim { get; set; }

        public override ActorComponent.ComponentType Category { get { return ComponentType.Control; } }

        public float RunSpeed
        {
            get { return Controller.HorizontalMotionConstraint.Speed; }

            set
            {
                Controller.HorizontalMotionConstraint.Speed = value;
                Controller.HorizontalMotionConstraint.CrouchingSpeed = value * 0.5f;
            }
        }

        public float JumpSpeed
        {
            get { return Controller.JumpSpeed; }
            set { Controller.JumpSpeed = value; }
        }

        public ControllerState State { get { return mState; } }

        public BipedControllerComponent(Actor owner)
            : base(owner)
        {
            DesiredMovementActions = MovementActions.Neutral;
            OrientationChange = Quaternion.Identity;
            HorizontalMovement = Vector2.Zero;
            Controller = null;
            MaxTurnAnglePerTick = MathHelper.Pi / 8.0f;
            mAnimationStateMachine = null;
            mState = ControllerState.Neutral;
            mBooster = null;
            AimCheck = delegate() { return false; };
            mAttentionLock = new Object();
            IsAttentionAvailable = true;
            Owner.ActorInitialized += ActorInitializedHandler;
            WorldAim = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            float jumpSpeed = 35.0f;
            if (manifest.Properties.ContainsKey(ManifestKeys.JUMP_SPEED))
                jumpSpeed = (float)(manifest.Properties[ManifestKeys.JUMP_SPEED]);

            float runSpeed = 32.0f;
            if (manifest.Properties.ContainsKey(ManifestKeys.RUN_SPEED))
                runSpeed = (float)(manifest.Properties[ManifestKeys.RUN_SPEED]);

            Controller = new CharacterController();

            JumpSpeed = jumpSpeed;
            RunSpeed = runSpeed;
            Controller.HorizontalMotionConstraint.SpeedScale = 1.0f;
            Controller.Body.CollisionInformation.CollisionRules.Group = GameResources.ActorManager.CharactersCollisionGroup;

            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
            GameResources.ActorManager.PostAnimationUpdateStep += PostAnimationUpdateHandler;
        }

        private void ActorInitializedHandler(object sender, EventArgs e)
        {
            StatefulAnimationComponent animationComponent = Owner.GetComponent<StatefulAnimationComponent>(ComponentType.Animation);
            if (animationComponent != null)
                mAnimationStateMachine = animationComponent.AnimationStateMachine;

            mBooster = Owner.GetBehavior<Booster>();
        }

        // This event was created so that other components, particularly the Booster, could have a way to manipulate the controller.
        // See the way that class uses this event.
        private void OnStateChanged(ControllerState newState)
        {
            ControllerStateChangedEventArgs e = new ControllerStateChangedEventArgs(newState, mState);
            mState = newState;

            ControllerStateChangedEventHandler handler = StateChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void CommitOrientationAndMovement()
        {
            if (mState == ControllerState.InputDisabled)
            {
                Controller.HorizontalMotionConstraint.SpeedScale = 0.0f;
                Controller.HorizontalMotionConstraint.MovementDirection = BEPUutilities.Vector2.UnitY;
                return;
            }

            float diffAngle = (float)(SpaceUtils.GetQuaternionAngle(OrientationChange));

            if (diffAngle > MaxTurnAnglePerTick)
            {
                OrientationChange = Quaternion.Slerp(Quaternion.Identity, OrientationChange, MaxTurnAnglePerTick / diffAngle);
                HorizontalMovement = Vector2.Zero; // We spent our movement turning (but this won't affect boosting.)
            }

            Controller.ViewDirection = BEPUutilities.Quaternion.Transform(Controller.ViewDirection, BepuConverter.Convert(OrientationChange));
            OrientationChange = Quaternion.Identity;

            if (mState == ControllerState.Boosting)
            {
                Controller.HorizontalMotionConstraint.SpeedScale = 1.0f;
                Controller.HorizontalMotionConstraint.MovementDirection = BEPUutilities.Vector2.UnitY;
            }
            else
            {
                if (AimCheck())
                {
                    Controller.HorizontalMotionConstraint.SpeedScale = 0.375f * Math.Min(1.0f, HorizontalMovement.Length());
                    Controller.HorizontalMotionConstraint.MovementDirection = BepuConverter.Convert(HorizontalMovement);
                }
                else
                {
                    SetHorizontalMovementAloof();
                }
            }
        }

        private void SetHorizontalMovementAloof()
        {
            // Map raw movement vector into our 2D movement space:
            if (HorizontalMovement.Y > 0.0f) // Positive Y is special because we can move faster in that direction.
            {
                double rawMovementTheta = Math.Atan2(HorizontalMovement.Y, HorizontalMovement.X);
                float maxRadius = (float)(GetMaxFwdMoveLength(rawMovementTheta));
                float totalMovementLength = HorizontalMovement.Length() * maxRadius;
                // Clamp the movement:
                Controller.HorizontalMotionConstraint.SpeedScale = 0.5f * Math.Min(maxRadius, totalMovementLength);
            }
            else
            {
                // Clamp the movement:
                Controller.HorizontalMotionConstraint.SpeedScale = 0.5f * Math.Min(1.0f, HorizontalMovement.Length());
            }

            Controller.HorizontalMotionConstraint.MovementDirection = BepuConverter.Convert(HorizontalMovement);
        }

        // Other actors or the owner can call this method when they've determined that the biped has been knocked over.
        public void KnockDown()
        {
            OnStateChanged(ControllerState.InputDisabled);
            if (mAnimationStateMachine != null)
                mAnimationStateMachine.DesiredStateName = KNOCKED_DOWN_SN;
        }

        // A possible way to recover from a knockdown.
        public void GetUp()
        {
            throw new NotImplementedException();
        }

        private double GetMaxFwdMoveLength(double theta)
        {
            return 2.0d / Math.Sqrt(4.0d * Math.Cos(theta) * Math.Cos(theta) + Math.Sin(theta) * Math.Sin(theta));
        }

        // Looks at DesiredMovementState and movement values and conveys the appropiate state to the animation controller.
        // Also commits the Controller movement and orientation input values.
        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            CommitOrientationAndMovement();

            if (mState != ControllerState.InputDisabled)
            {
                if (mAnimationStateMachine != null)
                {
                    if (Controller.SupportFinder.HasSupport)
                    {
                        Vector2 horizontalMovement = BepuConverter.Convert(Controller.HorizontalMotionConstraint.MovementDirection)
                            * Controller.HorizontalMotionConstraint.SpeedScale;

                        if ((DesiredMovementActions & MovementActions.Jumping) > 0)
                        {
                            mAnimationStateMachine.DesiredStateName = JUMPING_SN;
                        }
                        else if (DesiredMovementActions.HasFlag(MovementActions.Boosting) &&
                            mState == ControllerState.Neutral &&
                            mBooster != null &&
                            mBooster.BoostReady)
                        {
                            mAnimationStateMachine.DesiredStateName = BOOSTING_SN;
                        }
                        else if ((DesiredMovementActions & MovementActions.Crouching) > 0)
                        {
                            if (horizontalMovement.LengthSquared() > 0.0f)
                            {
                                mAnimationStateMachine.HorizontalMovement = horizontalMovement;
                                mAnimationStateMachine.DesiredStateName = CROUCHMOVING_SN;
                            }
                            else
                            {
                                mAnimationStateMachine.DesiredStateName = CROUCHING_SN;
                            }
                        }
                        else if (horizontalMovement.LengthSquared() > 0.0f)
                        {
                            mAnimationStateMachine.HorizontalMovement = horizontalMovement;
                            mAnimationStateMachine.DesiredStateName = MOVING_SN;
                        }
                        else
                        {
                            mAnimationStateMachine.DesiredStateName = STANDING_SN;
                        }
                    }
                    else
                    {
                        mAnimationStateMachine.DesiredStateName = FALLING_SN;
                    }
                }
            }
        }

        // Checks the animation state for control events and passes them on to the Controller.
        private void PostAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            if (mAnimationStateMachine != null)
            {
                if (mAnimationStateMachine.CurrentState.Name == CROUCHING_SN ||
                    mAnimationStateMachine.CurrentState.Name == CROUCHMOVING_SN)
                {
                    Controller.StanceManager.DesiredStance = Stance.Crouching;
                }
                else
                {
                    Controller.StanceManager.DesiredStance = Stance.Standing;
                }

                if (mAnimationStateMachine.ActiveControlEvents.HasFlag(AnimationControlEvents.Jump))
                    Controller.Jump();

                if (mAnimationStateMachine.ActiveControlEvents.HasFlag(AnimationControlEvents.Boost))
                    OnStateChanged(ControllerState.Boosting);
            }
            else // Just let the events happen immediately:
            {
                if (Controller.SupportFinder.HasSupport)
                {
                    if ((DesiredMovementActions & MovementActions.Jumping) > 0)
                        Controller.Jump();

                    if (DesiredMovementActions.HasFlag(MovementActions.Boosting) &&
                        mState == ControllerState.Neutral &&
                        mBooster != null &&
                        mBooster.BoostReady)
                    {
                        OnStateChanged(ControllerState.Boosting);
                    }
                }
            }

            if (mState == ControllerState.Boosting &&
                !(DesiredMovementActions.HasFlag(MovementActions.Boosting)))
            {
                OnStateChanged(ControllerState.Neutral);
            }
        }

        public override void Release()
        {
            mAnimationStateMachine = null;
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
            GameResources.ActorManager.PostAnimationUpdateStep -= PostAnimationUpdateHandler;
        }

        public OtherActions DesiredArmState { get; set; }

        public EventHandler TryGetAttention()
        {
            EventHandler result = null;

            lock (mAttentionLock)
            {
                if (IsAttentionAvailable)
                {
                    IsAttentionAvailable = false;
                    result = ReturnAttention;
                }
            }

            return result;
        }

        private void ReturnAttention(object sender, EventArgs e)
        {
            lock (mAttentionLock)
            {
                IsAttentionAvailable = true;
            }
        }
    }
}
