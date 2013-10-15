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

        // TODO: This should be part of an input customization system.
        public int LookFactor { get; set; }

        // Override CameraAnchor because we want to look around without moving the object's orientation.
        protected float cameraYaw;
        protected float cameraPitch;


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


        public TPPedestrian(IGameManager owner) : base(owner)
        {
            cameraYaw = 0.0f;
            cameraPitch = 0.0f;
            LookFactor = -1;

            // Default values for the CharacterController:
            Height = 9.3f;
            Radius = 1.0f;
            Mass = 17.0f;
            JumpSpeed = 35.0f;
            RunSpeed = 32.0f;

            // TODO: Tweak friction and mass.
        }

        public override void Initialize()
        {
            base.Initialize();

            character = new CharacterController(BepuConverter.Convert(Position + Vector3.Up * Height / 2.0f), Height, Height / 2.0f, Radius, Mass);

            character.JumpSpeed = JumpSpeed;
            character.HorizontalMotionConstraint.Speed = RunSpeed;
            character.HorizontalMotionConstraint.SpeedScale = 1.0f;

            owner.PostPhysicsUpdateStep += PostPhysicsUpdate;

            owner.SimSpace.Add(character);
        }


        public override void CreateCamera()
        {
            ArcBallCamera newCam = new ArcBallCamera(ArcBallCameraMode.RollConstrained);
            newCam.Distance = (CameraTargetOffset - CameraOffset).Length();
            newCam.SetCamera(Vector3.Transform(CameraOffset, CameraAnchor),
                Vector3.Transform(CameraTargetOffset, CameraAnchor),
                Vector3.Up);

            Camera = newCam;
        }


        public override void HandleInput(GameTime gameTime, InputManager input, PlayerIndex player)
        {
            base.HandleInput(gameTime, input, player);

            // Mouse camera & object orientation:
            PlayerIndex dummyPlayerIndex;
            Vector2 mouseDragDisplacement;

            // Check for 'drag' condition:
            if (input.IsMouseDragging(MouseButtons.Left, player, out dummyPlayerIndex, out mouseDragDisplacement))
            {
                //Mouse.SetPosition(input.LastState.GetMouseState((int)player).X, input.LastState.GetMouseState((int)player).Y);
                cameraYaw += -mouseDragDisplacement.X * INPUT_MOUSE_LOOK_RATE;
                cameraPitch += LookFactor * mouseDragDisplacement.Y * INPUT_MOUSE_LOOK_RATE;
            }
            else if (input.IsMouseDragging(MouseButtons.Right, player, out dummyPlayerIndex, out mouseDragDisplacement))
            {
                //Mouse.SetPosition(input.LastState.GetMouseState((int)player).X, input.LastState.GetMouseState((int)player).Y);
                Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -mouseDragDisplacement.X * INPUT_MOUSE_LOOK_RATE);
                cameraPitch += LookFactor * mouseDragDisplacement.Y * INPUT_MOUSE_LOOK_RATE;
            }

            // Gamepad camera & object orientation:
            Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -input.CurrentState.PadState[(int)player].ThumbSticks.Right.X *
                INPUT_ROTATION_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds));

            cameraPitch += LookFactor * -input.CurrentState.PadState[(int)player].ThumbSticks.Right.Y *
                INPUT_ROTATION_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            cameraPitch = MathHelper.Clamp(cameraPitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);

            cameraYaw = cameraYaw % MathHelper.TwoPi;

            // Special mouse right-click reorientation:
            if (input.IsNewMouseButtonPress(MouseButtons.Right, player, out dummyPlayerIndex))
            {
                // Bake the camera yaw into the orientation:
                Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, cameraYaw);
                cameraYaw = 0.0f;
            }

            // It's good practice to make sure floating point errors don't accumulate on the unit quaternions:
            Orientation = Quaternion.Normalize(Orientation);

            // Movement:
            BEPUutilities.Vector2 totalMovement = BEPUutilities.Vector2.Zero;

            // Keyboard:
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.W))
            {
                totalMovement += new BEPUutilities.Vector2(0, 1);
            }
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.S))
            {
                totalMovement += new BEPUutilities.Vector2(0, -1);
            }
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.A))
            {
                totalMovement += new BEPUutilities.Vector2(-1, 0);
            }
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.D))
            {
                totalMovement += new BEPUutilities.Vector2(1, 0);
            }

            // Gamepad:
            totalMovement += new BEPUutilities.Vector2(input.CurrentState.PadState[(int)player].ThumbSticks.Left.X,
                    input.CurrentState.PadState[(int)player].ThumbSticks.Left.Y);

            // Clamp the movement:
            if (totalMovement.Length() > 1.0f)
            {
                character.HorizontalMotionConstraint.MovementDirection = BEPUutilities.Vector2.Normalize(totalMovement);
            }
            else
            {
                character.HorizontalMotionConstraint.MovementDirection = totalMovement;
            }

            // Crouching:
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.LeftShift) ||
                input.IsNewButtonPress(Buttons.LeftStick, player, out dummyPlayerIndex))
            {
                character.StanceManager.DesiredStance = Stance.Crouching;
            }
            else
            {
                character.StanceManager.DesiredStance = Stance.Standing;
            }

            // Jumping:
            if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.Space) ||
                input.IsNewButtonPress(Buttons.A, player, out dummyPlayerIndex))
            {
                character.Jump();
            }

            character.ViewDirection = BepuConverter.Convert(Vector3.Transform(Vector3.Backward, Orientation));

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
        }
    }
}
