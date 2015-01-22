using Microsoft.Xna.Framework;
using System;
using Manifracture;
using SlagformCommon;

namespace Mechadrone1
{
    class TPAimingBipedMovementInputProcessor : InputProcessor
    {
        private BipedControllerComponent mBipedControl;

        public TPAimingBipedMovementInputProcessor(PlayerIndex inputIndex)
            : base(inputIndex)
        {
            mBipedControl = null;
        }

        public void SetControllerComponent(BipedControllerComponent bipedController)
        {
            mBipedControl = bipedController;
            mBipedControl.Owner.ActorDespawning += ActorDespawningHandler;
        }

        public override void HandleInput(GameTime gameTime, InputManager input)
        {
            float dTimeMs = (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (mBipedControl != null)
                ControlBiped(input, dTimeMs);
        }

        private void ControlBiped(InputManager input, float dTimeMs)
        {
            // Aim pitch
            float aimPitch = (float)(Math.Asin(mBipedControl.Controller.ViewDirection.Y));
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraPitchDecrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraPitchDecrease])
                {
                    aimPitch += (input.IsBinaryControlDown(control, InputIndex) ? -INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraPitchIncrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraPitchIncrease])
                {
                    aimPitch += (input.IsBinaryControlDown(control, InputIndex) ? INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.CameraPitchRate))
            {
                aimPitch += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.CameraPitchRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs;
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.CameraPitchDelta))
            {
                aimPitch += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.CameraPitchDelta],
                    InputIndex) * INPUT_MOUSE_LOOK_FACTOR;
            }

            aimPitch = MathHelper.Clamp(aimPitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);

            // Aim yaw
            Quaternion yawOrientationChange = Quaternion.Identity;
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.TurnLeft))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.TurnLeft])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                        yawOrientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, INPUT_PAD_LOOK_FACTOR * dTimeMs);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.TurnRight))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.TurnRight])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                        yawOrientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, -INPUT_PAD_LOOK_FACTOR * dTimeMs);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.TurnLeftRightRate))
            {
                yawOrientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up,
                    -input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.TurnLeftRightRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs);
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.TurnLeftRightDelta))
            {
                float fullAxisYawAmount = input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.TurnLeftRightDelta],
                    InputIndex);
                yawOrientationChange *= Quaternion.CreateFromAxisAngle(Vector3.Up, -fullAxisYawAmount * INPUT_MOUSE_LOOK_FACTOR);
            }

            // It's good practice to make sure floating point errors don't accumulate and change the length of our unit quaternion:
            yawOrientationChange = Quaternion.Normalize(yawOrientationChange);
            mBipedControl.OrientationChange = yawOrientationChange;

            mBipedControl.Controller.ViewDirection = mBipedControl.Controller.HorizontalViewDirection + BEPUutilities.Vector3.Up * (float)(Math.Sin(aimPitch));

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
            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.StrafeLeftRightRate))
            {
                rawMovement.X += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.StrafeLeftRightRate],
                    InputIndex);
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.MoveForwardBackwardRate))
            {
                rawMovement.Y += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.MoveForwardBackwardRate], 
                    InputIndex);
            }

            // Character movement (full axis i.e. mouse):
            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.StrafeLeftRightDelta))
            {
                rawMovement.X += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.StrafeLeftRightDelta],
                    InputIndex) * INPUT_MOUSE_MOVE_FACTOR;
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.MoveForwardBackwardDelta))
            {
                rawMovement.Y += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.MoveForwardBackwardDelta],
                    InputIndex) * INPUT_MOUSE_MOVE_FACTOR;
            }

            mBipedControl.HorizontalMovement = rawMovement;

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
