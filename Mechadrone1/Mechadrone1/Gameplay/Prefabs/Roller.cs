using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Rendering;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SlagformCommon;

namespace Mechadrone1.Gameplay.Prefabs
{
    class Roller : SimulatedGameObject
    {
        private const float INPUT_FORCE = 0.06f;
        private const float INPUT_ROTATION_FORCE = 0.05f;

        // Override camera anchor to not be dependent on the object's orientation because this
        // object will be rolling around.
        private float cameraYaw;
        private float cameraPitch;


        public override Matrix CameraAnchor
        {
            get
            {
                return Matrix.CreateFromYawPitchRoll(cameraYaw, cameraPitch, 0.0f) * Matrix.CreateTranslation(position);
            }
        }

        public Roller(IGameManager owner) : base(owner)
        {
            cameraYaw = 0.0f;
            cameraPitch = -MathHelper.Pi / 8.0f;
        }

        public override void HandleInput(Microsoft.Xna.Framework.GameTime gameTime, InputManager input, PlayerIndex player)
        {
            base.HandleInput(gameTime, input, player);

            // camera rotation
            cameraYaw -= input.CurrentState.PadState[(int)player].ThumbSticks.Right.X *
                INPUT_ROTATION_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;

            cameraPitch -= input.CurrentState.PadState[(int)player].ThumbSticks.Right.Y *
                INPUT_ROTATION_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;

            cameraPitch = MathHelper.Clamp(cameraPitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);

            Entity soEnt = simulationObject as Entity;

            Vector3 controlForceDir = new Vector3();
            controlForceDir.X = input.CurrentState.PadState[(int)player].ThumbSticks.Left.X;
            controlForceDir.Y = 0.0f;
            controlForceDir.Z = -input.CurrentState.PadState[(int)player].ThumbSticks.Left.Y;
            if (controlForceDir.Length() > 1.0f)
                controlForceDir = Vector3.Normalize(controlForceDir);

            // Camera will be null if this object is not an avatar, but it should also not be receiving
            // input if that is the case.
            controlForceDir = Vector3.TransformNormal(controlForceDir, Camera.Transform);

            if (soEnt != null)
            {
                soEnt.LinearMomentum += BepuConverter.Convert(controlForceDir * INPUT_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds));

                if (input.CurrentState.PadState[(int)player].IsButtonDown(Buttons.B) && soEnt.LinearMomentum.Length() > 0.0f)
                {
                    BEPUutilities.Vector3 moveDir = BEPUutilities.Vector3.Normalize(soEnt.LinearMomentum);
                    float brakeForce = INPUT_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

                    soEnt.LinearMomentum -= Math.Min(soEnt.LinearMomentum.Length(), brakeForce) * moveDir;
                }

            }

        }

    }
}
