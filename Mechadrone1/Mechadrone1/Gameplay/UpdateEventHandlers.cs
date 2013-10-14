using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Gameplay
{

    delegate void PreAnimationUpdateEventHandler(object sender, UpdateEventArgs e);
    delegate void PostPhysicsUpdateEventHandler(object sender, UpdateEventArgs e);

    class UpdateEventArgs : EventArgs
    {
        public GameTime GameTime { get; set; }

        public UpdateEventArgs(GameTime gameTime) : base()
        {
            this.GameTime = gameTime;
        }
    }

}
