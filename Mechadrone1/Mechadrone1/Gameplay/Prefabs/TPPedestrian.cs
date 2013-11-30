using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mechadrone1.Gameplay.Helpers;
using BEPUphysicsDemos.AlternateMovement.Character;
using SlagformCommon;
using Manifracture;
using Skelemator;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1.Gameplay.Prefabs
{
    class TPPedestrian : GameObject
    {
        public float Height { get; set; }
        public float Radius { get; set; }
        public float Mass { get; set; }
        public float JumpSpeed { get; set; }
        public float RunSpeed { get; set; }

        protected CharacterController character;
        protected const float INPUT_FORCE = 1.0f;
        protected const float INPUT_ROTATION_FORCE = 0.003f;
        protected const float INPUT_MOUSE_LOOK_RATE = 0.002f;
        protected const float INPUT_PAD_CAM_DIST_MOVE_RATE = 0.03f;
        protected const float INPUT_SCROLLWHEEL_CAM_DIST_MOVE_RATE = 0.008f;

        protected AnimationStateMachine animationStateMachine;

        // TODO: This should be part of an input customization system.
        public int LookFactor { get; set; }

        // Override CameraAnchor because we want to look around without moving the object's orientation.
        protected float cameraYaw;
        protected float cameraPitch;
        protected float cameraDist;


        [NotInitializable]
        public Vector3 SimulationPosition
        {
            get
            {
                return Position - BepuConverter.Convert(character.Down) / 2.0f *
                        (character.StanceManager.CurrentStance == Stance.Crouching ?
                        character.StanceManager.CrouchingHeight :
                        character.StanceManager.StandingHeight);
            }

            set
            {
                Position = value + BepuConverter.Convert(character.Down) / 2.0f *
                    (character.StanceManager.CurrentStance == Stance.Crouching ?
                    character.StanceManager.CrouchingHeight :
                    character.StanceManager.StandingHeight);
            }
        }


        public override Matrix CameraAnchor
        {
            get
            {
                Vector3 heading = Vector3.Transform(Vector3.Backward, Matrix.CreateFromQuaternion(orientation));
                float yaw = MathHelper.PiOver2 - (float)(Math.Atan2(heading.Z, heading.X));
                return Matrix.CreateFromYawPitchRoll(cameraYaw, cameraPitch, 0.0f) * Matrix.CreateFromYawPitchRoll(yaw, 0.0f, 0.0f) * Matrix.CreateTranslation(position);
            }
        }

        [LoadedAsset]
        public override Model VisualModel
        {
            get
            {
                return visualModel;
            }
            set
            {
                visualModel = value;
                AnimationPackage ap = visualModel.Tag as AnimationPackage;
                if (ap != null)
                {
                    Animations = ap.SkinningData;
                    if (ap.SkinningData != null)
                    {
                        animationStateMachine = new AnimationStateMachine(ap);
                        AnimationPlayer = animationStateMachine;
                    }
                }
            }
        }


        [Flags]
        protected enum BipedStates
        {
            Neutral = 0x00,
            Crouching = 0x01,
            Jumping = 0x04,
        }

        protected BipedStates desiredState;


        public TPPedestrian(IGameManager owner) : base(owner)
        {
            cameraYaw = 0.0f;
            cameraPitch = 0.0f;
            LookFactor = -1;
            desiredState = BipedStates.Neutral;

            // Default values for the CharacterController:
            Height = 9.3f;
            Radius = 1.0f;
            Mass = 9.0f;
            JumpSpeed = 35.0f;
            RunSpeed = 32.0f;
        }


        public override void Initialize()
        {
            base.Initialize();

            character = new CharacterController(BepuConverter.Convert(Position + Vector3.Up * Height / 2.0f), Height, Height / 2.0f, Radius, Mass);

            character.JumpSpeed = JumpSpeed;
            character.HorizontalMotionConstraint.Speed = RunSpeed;
            character.HorizontalMotionConstraint.CrouchingSpeed = RunSpeed * 0.5f;
            character.HorizontalMotionConstraint.SpeedScale = 1.0f;

            owner.PostPhysicsUpdateStep += PostPhysicsUpdate;

            owner.SimSpace.Add(character);
        }


        public override void RegisterUpdateHandlers()
        {
            owner.PreAnimationUpdateStep += PreAnimationUpdate;
            owner.PostPhysicsUpdateStep += PostPhysicsUpdate;
            owner.AnimationUpdateStep += AnimationUpdate;
        }


        public override void CreateCamera()
        {
            ArcBallCamera newCam = new ArcBallCamera(ArcBallCameraMode.RollConstrained);
            newCam.Distance = (CameraTargetOffset - CameraOffset).Length();
            newCam.SetCamera(Vector3.Transform(CameraOffset, CameraAnchor),
                Vector3.Transform(CameraTargetOffset, CameraAnchor),
                Vector3.Up);

            Camera = newCam;
            cameraDist = newCam.Distance;
        }


        public override void HandleInput(GameTime gameTime, InputManager input, PlayerIndex player)
        {
            base.HandleInput(gameTime, input, player);

            float dTimeMs = (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            // Mouse camera & object orientation:
            PlayerIndex dummyPlayerIndex;
            Vector2 mouseDragDisplacement;

            // Check for 'drag' condition:
            if (input.IsMouseDragging(MouseButtons.Left, player, out dummyPlayerIndex, out mouseDragDisplacement))
            {
                cameraYaw += -mouseDragDisplacement.X * INPUT_MOUSE_LOOK_RATE;
                cameraPitch += LookFactor * mouseDragDisplacement.Y * INPUT_MOUSE_LOOK_RATE;
            }
            else if (input.IsMouseDragging(MouseButtons.Right, player, out dummyPlayerIndex, out mouseDragDisplacement))
            {
                Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -mouseDragDisplacement.X * INPUT_MOUSE_LOOK_RATE);
                cameraPitch += LookFactor * mouseDragDisplacement.Y * INPUT_MOUSE_LOOK_RATE;
            }

            // Gamepad camera & object orientation:
            Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -input.CurrentState.PadState[(int)player].ThumbSticks.Right.X *
                INPUT_ROTATION_FORCE * dTimeMs);

            cameraPitch += LookFactor * -input.CurrentState.PadState[(int)player].ThumbSticks.Right.Y *
                INPUT_ROTATION_FORCE * dTimeMs;

            cameraPitch = MathHelper.Clamp(cameraPitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);

            cameraYaw = cameraYaw % MathHelper.TwoPi;

            // Collect gamepad movement a bit early, we need it for the following step:
            BEPUutilities.Vector2 padMovement = new BEPUutilities.Vector2(
                input.CurrentState.PadState[(int)player].ThumbSticks.Left.X,
                input.CurrentState.PadState[(int)player].ThumbSticks.Left.Y);

            // Special case for reorientation upon mouse right-click or pad movement:
            if (input.IsNewMouseButtonPress(MouseButtons.Right, player, out dummyPlayerIndex) ||
                padMovement.LengthSquared() > 0.0f)
            {
                // Bake the camera yaw into the orientation:
                Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, cameraYaw);
                cameraYaw = 0.0f;
            }

            // Update the body's orientation:
            character.ViewDirection = BepuConverter.Convert(Vector3.Transform(Vector3.Backward, Orientation));

            // Camera distance:
            cameraDist += input.CurrentState.PadState[(int)player].Triggers.Right * dTimeMs * INPUT_PAD_CAM_DIST_MOVE_RATE;
            cameraDist -= input.CurrentState.PadState[(int)player].Triggers.Left * dTimeMs * INPUT_PAD_CAM_DIST_MOVE_RATE;

            cameraDist -= (float)input.ScrollWheelDiff() * INPUT_SCROLLWHEEL_CAM_DIST_MOVE_RATE;

            if (cameraDist < .001f) cameraDist = .001f;

            // It's good practice to make sure floating point errors don't accumulate and change the length of our unit quaternion:
            Orientation = Quaternion.Normalize(Orientation);

            // Process movement:
            BEPUutilities.Vector2 rawMovement = BEPUutilities.Vector2.Zero;

            // Keyboard:
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.W))
            {
                rawMovement += new BEPUutilities.Vector2(0, 1);
            }
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.S))
            {
                rawMovement += new BEPUutilities.Vector2(0, -1);
            }
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.A))
            {
                rawMovement += new BEPUutilities.Vector2(-1, 0);
            }
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.D))
            {
                rawMovement += new BEPUutilities.Vector2(1, 0);
            }

            // Gamepad:
            rawMovement += padMovement;

            ConditionAndSetMovement(rawMovement);

            // Stance changes and other actions:
            // Crouching:
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.LeftShift) ||
                input.CurrentState.PadState[(int)player].IsButtonDown(Buttons.LeftStick))
            {
                desiredState |= BipedStates.Crouching;
            }
            else
            {
                desiredState &= ~BipedStates.Crouching;
            }

            // Jumping:
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.Space) ||
                input.IsNewButtonPress(Buttons.A, player, out dummyPlayerIndex))
            {
                desiredState |= BipedStates.Jumping;
            }
            else
            {
                desiredState &= ~BipedStates.Jumping;
            }
        }


        // Clamps and scales the movement into the desired shape (kind of an egg in this case), then
        // updates the physics body.
        protected void ConditionAndSetMovement(BEPUutilities.Vector2 rawMovement)
        {
            // Map raw movement vector into our 2D movement space:
            if (rawMovement.Y > 0.0f)
            {
                double rawMovementTheta = Math.Atan2(rawMovement.Y, rawMovement.X);
                float maxRadius = (float)GetMaxFwdMoveLength(rawMovementTheta);
                float totalMovementLength = rawMovement.Length() * maxRadius;
                // Clamp the movement:
                character.HorizontalMotionConstraint.SpeedScale = 0.5f * Math.Min(maxRadius, totalMovementLength);
            }
            else
            {
                // Clamp the movement:
                character.HorizontalMotionConstraint.SpeedScale = 0.5f * Math.Min(1.0f, rawMovement.Length());
            }

            character.HorizontalMotionConstraint.MovementDirection = rawMovement;
        }


        private double GetMaxFwdMoveLength(double theta)
        {
            return 2.0d / Math.Sqrt(4.0d * Math.Cos(theta) * Math.Cos(theta) + Math.Sin(theta) * Math.Sin(theta));
        }


        public void PreAnimationUpdate(object sender, UpdateStepEventArgs e)
        {
            // look at desiredState flags and convey the appropiate state to the animation controller.

            if (character.SupportFinder.HasSupport)
            {
                Vector2 horizontalMovement = BepuConverter.Convert(character.HorizontalMotionConstraint.MovementDirection)
                    * character.HorizontalMotionConstraint.SpeedScale;

                if ((desiredState & BipedStates.Jumping) > 0)
                {
                    animationStateMachine.DesiredStateName = "Jumping";
                }
                else if ((desiredState & BipedStates.Crouching) > 0)
                {
                    if (horizontalMovement.LengthSquared() > 0.0f)
                    {
                        animationStateMachine.HorizontalMovement = horizontalMovement;
                        animationStateMachine.DesiredStateName = "CrouchMoving";
                    }
                    else
                    {
                        animationStateMachine.DesiredStateName = "Crouching";
                    }
                }
                else if (horizontalMovement.LengthSquared() > 0.0f)
                {
                    animationStateMachine.HorizontalMovement = horizontalMovement;
                    animationStateMachine.DesiredStateName = "Moving";
                }
                else
                {
                    animationStateMachine.DesiredStateName = "Standing";
                }
            }
            else
            {
                animationStateMachine.DesiredStateName = "Falling";
            }

            DebugMessage = String.Format("{0}{1}  ->  {2}",
                animationStateMachine.CurrentState.Name,
                animationStateMachine.ActiveTransition != null ? "*" : "",
                animationStateMachine.DesiredStateName);
        }


        public void AnimationUpdate(object sender, UpdateStepEventArgs e)
        {
            List<AnimationControlEvents> animationControlEvents = animationStateMachine.Update(e.GameTime);

            if (animationStateMachine.CurrentState.Name == "Crouching" ||
                animationStateMachine.CurrentState.Name == "CrouchMoving")
            {
                character.StanceManager.DesiredStance = Stance.Crouching;
            }
            else
            {
                character.StanceManager.DesiredStance = Stance.Standing;
            }

            for (int v = 0; v < animationControlEvents.Count; v++)
            {
                switch (animationControlEvents[v])
                {
                    case AnimationControlEvents.Jump:
                        character.Jump();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }


        public void PostPhysicsUpdate(object sender, UpdateStepEventArgs e)
        {
            SimulationPosition = BepuConverter.Convert(character.Body.Position);
            float viewAngle = (float)(Math.Atan2(character.ViewDirection.X, character.ViewDirection.Z));
            Orientation = Quaternion.CreateFromAxisAngle(BepuConverter.Convert(-character.Down), viewAngle);

            UpdateQuadTree();
        }


        public override void UpdateCamera(float elapsedTime)
        {
            ArcBallCamera arcBallCam = Camera as ArcBallCamera;
            arcBallCam.SetCamera(Vector3.Transform(CameraOffset, CameraAnchor),
                Vector3.Transform(CameraTargetOffset, CameraAnchor),
                Vector3.Up);
            arcBallCam.Distance = cameraDist;
        }
    }
}
