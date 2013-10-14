using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Mechadrone1.Rendering;
using SlagformCommon;

namespace Mechadrone1.Gameplay.Prefabs
{
    class Bouncer : SimulatedGameObject
    {

        public Bouncer(IGameManager owner)
            : base(owner)
        {
        }


        public override void HandleInput(Microsoft.Xna.Framework.GameTime gameTime, InputManager input, PlayerIndex player)
        {
            base.HandleInput(gameTime, input, player);

            Entity soEnt = SimulationObject as Entity;

            if (soEnt != null)
            {
                if (input.CurrentState.KeyState[(int)player].IsKeyDown(Keys.Space))
                {
                    soEnt.LinearMomentum += BepuConverter.Convert((Vector3.Up + owner.Avatars[player].Camera.Transform.Forward) * (float)(gameTime.ElapsedGameTime.TotalMilliseconds) * 0.42f);
                }
            }

        }
    }
}
