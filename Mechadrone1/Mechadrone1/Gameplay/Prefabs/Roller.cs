using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Rendering;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1.Gameplay.Prefabs
{
    class Roller : GameObject
    {
        private const float INPUT_FORCE = 1.0f;
        private const float INPUT_ROTATION_FORCE = 0.05f;

        private float cameraYaw;
        private float cameraPitch;

        public override Matrix CameraAnchor
        {
            get
            {
                return Matrix.CreateFromYawPitchRoll(cameraYaw, cameraPitch, 0.0f) * Matrix.CreateTranslation(position);
            }
        }

        public Roller()
        {
            cameraYaw = 0.0f;
            cameraPitch = -MathHelper.Pi / 8.0f;
        }

        public override void HandleInput(Microsoft.Xna.Framework.GameTime gameTime, InputManager input, PlayerIndex player, ICamera camera)
        {
            base.HandleInput(gameTime, input, player, camera);

            // camera rotation
            cameraYaw -= input.CurrentState.PadState[(int)player].ThumbSticks.Right.X *
                INPUT_ROTATION_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;

            cameraPitch -= input.CurrentState.PadState[(int)player].ThumbSticks.Right.Y *
                INPUT_ROTATION_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;

            cameraPitch = MathHelper.Clamp(cameraPitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);

            Entity soEnt = SimulationObject as Entity;

            Vector3 controlForceDir = new Vector3();
            controlForceDir.X = input.CurrentState.PadState[(int)player].ThumbSticks.Left.X;
            controlForceDir.Y = 0.0f;
            controlForceDir.Z = -input.CurrentState.PadState[(int)player].ThumbSticks.Left.Y;
            if (controlForceDir.Length() > 1.0f)
                controlForceDir = Vector3.Normalize(controlForceDir);

            Matrix invView = Matrix.Invert(camera.View);

            controlForceDir = Vector3.TransformNormal(controlForceDir, invView);

            if (soEnt != null)
            {
                soEnt.LinearMomentum += controlForceDir * INPUT_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;

                if (input.CurrentState.PadState[(int)player].IsButtonDown(Buttons.B) && soEnt.LinearMomentum.Length() > 0.0f)
                {
                    Vector3 moveDir = Vector3.Normalize(soEnt.LinearMomentum);
                    float brakeForce = INPUT_FORCE * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;

                    soEnt.LinearMomentum -= Math.Min(soEnt.LinearMomentum.Length(), brakeForce) * moveDir;
                }

            }

        }

    }
}
