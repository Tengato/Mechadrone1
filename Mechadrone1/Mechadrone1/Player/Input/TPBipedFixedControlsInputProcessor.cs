using Microsoft.Xna.Framework;
using System;
using Manifracture;
using SlagformCommon;

namespace Mechadrone1
{
    class TPBipedFixedControlsInputProcessor : InputProcessor
    {
        private BipedControllerComponent mBipedControl;
        private ICamera mReferenceCam;

        public TPBipedFixedControlsInputProcessor(PlayerIndex inputIndex, ICamera referenceCam) 
            : base(inputIndex)
        {
            mReferenceCam = referenceCam;
            mBipedControl = null;
        }

        public void SetControllerComponent(BipedControllerComponent bipedController)
        {
            mBipedControl = bipedController;
            mBipedControl.Owner.ActorDespawning += ActorDespawningHandler;
        }

        public override void HandleInput( GameTime gameTime, InputManager input)
        {
            float dTimeMs = (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (mBipedControl != null)
                ControlBiped(input, dTimeMs);
        }

        private void ControlBiped(InputManager input, float dTimeMs)
        {
            // If input is in line with avatar orientation, you get all movement, otherwise, some of the movement is dulled to get reorientation

            Vector3 moveInput = Vector3.Zero;

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.MoveLeftRightRate))
            {
                moveInput.X += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.MoveLeftRightRate],
                    InputIndex);
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.MoveDownUpRate))
            {
                moveInput.Z -= input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.MoveDownUpRate],
                    InputIndex);
            }

            float moveAmount = moveInput.Length();

            if (moveAmount > 0.0f)
            {
                // Transform (rotation only) the input from view space into world space
                Matrix cameraRotation = mReferenceCam.Transform;
                cameraRotation.Translation = Vector3.Zero;
                moveInput = Vector3.Transform(moveInput, cameraRotation);

                if (moveInput.X != 0.0f || moveInput.Z != 0.0f)
                {
                    moveInput.Y = 0.0f;
                    Quaternion directionDiff = SpaceUtils.GetSweptQuaternion(BepuConverter.Convert(mBipedControl.Controller.HorizontalViewDirection), moveInput);

                    mBipedControl.OrientationChange = directionDiff;
                }
            }

            mBipedControl.HorizontalMovement = Vector2.UnitY * moveAmount;

            // Crouching:
            if (input.CheckForBinaryInput(ActiveInputMap, BinaryControlActions.Crouch, InputIndex))
            {
                mBipedControl.DesiredMovementActions |= BipedControllerComponent.MovementActions.Crouching;
            }
            else
            {
                mBipedControl.DesiredMovementActions &= ~BipedControllerComponent.MovementActions.Crouching;
            }

            // Jumping:
            if (input.CheckForNewBinaryInput(ActiveInputMap, BinaryControlActions.Jump, InputIndex))
            {
                mBipedControl.DesiredMovementActions |= BipedControllerComponent.MovementActions.Jumping;
            }
            else
            {
                mBipedControl.DesiredMovementActions &= ~BipedControllerComponent.MovementActions.Jumping;
            }
        }

        private void ActorDespawningHandler(object sender, EventArgs e)
        {
            mBipedControl = null;
        }

    }
}
