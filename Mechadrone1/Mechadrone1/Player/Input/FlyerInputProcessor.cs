using Microsoft.Xna.Framework;
using System;
using Manifracture;

namespace Mechadrone1
{
    class FlyerInputProcessor : InputProcessor
    {
        private FlyerControllerComponent mFlyerControl;

        public FlyerInputProcessor(PlayerIndex inputIndex)
            : base(inputIndex)
        {
            mFlyerControl = null;
        }

        public void SetControllerComponent(FlyerControllerComponent flyerController)
        {
            mFlyerControl = flyerController;
            mFlyerControl.Owner.ActorDespawning += ActorDespawningHandler;
        }

        public override void HandleInput(
            GameTime gameTime,
            InputManager input)
        {
            float dTimeMs = (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (mFlyerControl != null)
                ControlFlyer(input, dTimeMs);

        }

        private void ControlFlyer(InputManager input, float dTimeMs)
        {
            // Rotation force
            Vector3 rotationForce = Vector3.Zero;

            // X (pitch)
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.VehiclePitchUp))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.VehiclePitchUp])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rotationForce.X += 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.VehiclePitchDown))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.VehiclePitchDown])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rotationForce.X -= 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.VehiclePitchRate))
            {
                rotationForce.X += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.VehiclePitchRate], InputIndex);
            }

            // Y (yaw)
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.TurnLeft))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.TurnLeft])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rotationForce.Y += 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.TurnRight))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.TurnRight])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rotationForce.Y += -1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.TurnLeftRightRate))
            {
                rotationForce.Y += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.TurnLeftRightRate], InputIndex);
            }

            // Z (roll)
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.VehicleRollLeft))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.VehicleRollLeft])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rotationForce.Z += 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.VehicleRollRight))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.VehicleRollRight])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        rotationForce.Z += -1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.VehicleRollRate))
            {
                rotationForce.Z += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.VehicleRollRate], InputIndex);
            }

            mFlyerControl.SetRotationForce(rotationForce);

            // Direct rotation
            Quaternion rotation = Quaternion.Identity;

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.VehiclePitchDelta))
            {
                float inputAmount = input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.VehiclePitchDelta], InputIndex);
                rotation *= Quaternion.CreateFromAxisAngle(Vector3.Right, inputAmount * INPUT_MOUSE_LOOK_FACTOR);
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.TurnLeftRightDelta))
            {
                float inputAmount = input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.TurnLeftRightDelta], InputIndex);
                rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -inputAmount * INPUT_MOUSE_LOOK_FACTOR);     // Negate because we want positive axis mouse movement (usually to the right) to rotate a negative amount of radians around the up direction (turn right).
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.VehicleRollDelta))
            {
                float inputAmount = input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.VehicleRollDelta], InputIndex);
                rotation *= Quaternion.CreateFromAxisAngle(Vector3.Forward, inputAmount * INPUT_MOUSE_LOOK_FACTOR);
            }

            mFlyerControl.SetDirectRotation(rotation);


            // Movement force
            Vector3 force = Vector3.Zero;

            // Z (forward/backward)
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.MoveForward))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.MoveForward])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        force.Z -= 1.0f;    // Forward is -Z
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
                        force.Z += 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.MoveForwardBackwardRate))
            {
                force.Z -= input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.MoveForwardBackwardRate], InputIndex);   // Forward is -Z
            }

            // X (left/right)
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.StrafeLeft))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.StrafeLeft])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        force.X -= 1.0f;
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
                        force.X += 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.StrafeLeftRightRate))
            {
                force.X += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.StrafeLeftRightRate], InputIndex);
            }

            // Y (up/down)
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.LevitateUp))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.LevitateUp])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        force.Y += 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.LevitateDown))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.LevitateDown])
                {
                    if (input.IsBinaryControlDown(control, InputIndex))
                    {
                        force.Y -= 1.0f;
                        break;
                    }
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.LevitateUpDownRate))
            {
                force.Y += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.LevitateUpDownRate], InputIndex);
            }

            mFlyerControl.SetForce(force);
        }

        private void ActorDespawningHandler(object sender, EventArgs e)
        {
            mFlyerControl = null;
        }

    }
}
