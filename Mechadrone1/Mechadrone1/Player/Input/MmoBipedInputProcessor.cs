using Microsoft.Xna.Framework;
using System;
using Manifracture;

namespace Mechadrone1
{
    class MmoBipedInputProcessor : InputProcessor
    {
        private BipedControllerComponent mBipedControl;
        private MmoCameraDesc mCameraDesc;

        public MmoBipedInputProcessor(PlayerIndex inputIndex, MmoCameraDesc cameraDesc)
            : base(inputIndex)
        {
            mBipedControl = null;
            mCameraDesc = cameraDesc;
        }

        public void SetControllerComponent(BipedControllerComponent bipedController)
        {
            mBipedControl = bipedController;
            mBipedControl.Owner.ActorDespawning += ActorDespawningHandler;
        }

        public override void HandleInput( GameTime gameTime, InputManager input)
        {
            float dTimeMs = (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            // Camera yaw
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraYawDecrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraYawDecrease])
                {
                    mCameraDesc.Yaw += (input.IsBinaryControlDown(control, InputIndex) ? -INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraYawIncrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraYawIncrease])
                {
                    mCameraDesc.Yaw += (input.IsBinaryControlDown(control, InputIndex) ? INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.CameraYawRate))
            {
                mCameraDesc.Yaw += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.CameraYawRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs;
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.CameraYawDelta))
            {
                mCameraDesc.Yaw += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.CameraYawDelta],
                    InputIndex) * INPUT_MOUSE_LOOK_FACTOR;
            }

            mCameraDesc.Yaw = mCameraDesc.Yaw % MathHelper.TwoPi;

            // Camera pitch
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraPitchDecrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraPitchDecrease])
                {
                    mCameraDesc.Pitch += (input.IsBinaryControlDown(control, InputIndex) ? -INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraPitchIncrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraPitchIncrease])
                {
                    mCameraDesc.Pitch += (input.IsBinaryControlDown(control, InputIndex) ? INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.CameraPitchRate))
            {
                mCameraDesc.Pitch += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.CameraPitchRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs;
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.CameraPitchDelta))
            {
                mCameraDesc.Pitch += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.CameraPitchDelta],
                    InputIndex) * INPUT_MOUSE_LOOK_FACTOR;
            }

            mCameraDesc.Pitch = MathHelper.Clamp(mCameraDesc.Pitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);

            if (mBipedControl != null)
                ControlBiped(input, dTimeMs);

        }

        private void ControlBiped(InputManager input, float dTimeMs)
        {
            // Player orientation
            Quaternion orientationChange = Quaternion.Identity;

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.TurnLeft))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.TurnLeft])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                        orientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, INPUT_PAD_LOOK_FACTOR * dTimeMs);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.TurnRight))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.TurnRight])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                        orientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, -INPUT_PAD_LOOK_FACTOR * dTimeMs);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.TurnLeftRightRate))
            {
                orientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up,
                    -input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.TurnLeftRightRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs);
            }

            float fullAxisTurnAmount = 0.0f;
            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.TurnLeftRightDelta))
            {
                fullAxisTurnAmount += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.TurnLeftRightDelta],
                    InputIndex);
            }

            orientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, -fullAxisTurnAmount * INPUT_MOUSE_LOOK_FACTOR);

            // We are collecting the pad movement before we're totally done with orientation to check for special case...
            Vector2 padMovement = Vector2.Zero;

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.StrafeLeftRightRate))
            {
                padMovement.X += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.StrafeLeftRightRate], InputIndex);
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.MoveForwardBackwardRate))
            {
                padMovement.Y += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.MoveForwardBackwardRate], InputIndex);
            }

            // Special case for reorientation upon full axis (mouse) turn or pad movement:
            if (fullAxisTurnAmount != 0.0f || padMovement.LengthSquared() > 0.0f)
            {
                // Bake the camera yaw into the orientation:
                orientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, mCameraDesc.Yaw);
                mCameraDesc.Yaw = 0.0f;
            }

            // It's good practice to make sure floating point errors don't accumulate and change the length of our unit quaternion:
            orientationChange = Quaternion.Normalize(orientationChange);
            mBipedControl.OrientationChange = orientationChange;

            // Character movement:
            Vector2 rawMovement = Vector2.Zero;

            // Character movement (binary):
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.MoveForward))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.MoveForward])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rawMovement += new Vector2(0.0f, 1.0f);
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.MoveBackward))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.MoveBackward])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rawMovement += new Vector2(0.0f, -1.0f);
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.StrafeLeft))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.StrafeLeft])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rawMovement += new Vector2(-1.0f, 0.0f);
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.StrafeRight))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.StrafeRight])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rawMovement += new Vector2(1.0f, 0.0f);
                        break;
                    }
                }
            }

            // Character movement (interval i.e. pad):
            rawMovement += padMovement;

            // Character movement (full axis i.e. mouse):
            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.StrafeLeftRightDelta))
            {
                rawMovement.X += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.StrafeLeftRightDelta],
                InputIndex);
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.MoveForwardBackwardDelta))
            {
                rawMovement.Y += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.MoveForwardBackwardDelta],
                InputIndex) * INPUT_MOUSE_MOVE_FACTOR;
            }

            mBipedControl.HorizontalMovement = rawMovement;

            bool pressConfirmed = false;

            // Crouching:
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.Crouch))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.Crouch])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        pressConfirmed = true;
                        break;
                    }
                }
            }

            if (pressConfirmed)
            {
                mBipedControl.DesiredMovementActions |= BipedControllerComponent.MovementActions.Crouching;
            }
            else
            {
                mBipedControl.DesiredMovementActions &= ~BipedControllerComponent.MovementActions.Crouching;
            }

            // Jumping:
            pressConfirmed = false;
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.Jump))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.Jump])
                {
                    if (input.IsNewBinaryControlPress(control, InputIndex))
                    {
                        pressConfirmed = true;
                        break;
                    }
                }
            }

            if (pressConfirmed)
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
