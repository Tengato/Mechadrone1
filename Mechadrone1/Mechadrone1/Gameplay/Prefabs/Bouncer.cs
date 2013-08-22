using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Mechadrone1.Rendering;

namespace Mechadrone1.Gameplay.Prefabs
{
    class Bouncer : GameObject
    {

        public override void HandleInput(Microsoft.Xna.Framework.GameTime gameTime, InputManager input, PlayerIndex player, ICamera camera)
        {
            base.HandleInput(gameTime, input, player, camera);

            Entity soEnt = SimulationObject as Entity;


            if (soEnt != null)
            {
                if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.Space))
                {
                    soEnt.LinearMomentum += (Vector3.Up + camera.Transform.Forward) * 7.0f * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.06f;
                }
            }

        }
    }
}
