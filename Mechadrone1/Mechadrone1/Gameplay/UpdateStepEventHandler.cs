using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Gameplay
{

    delegate void UpdateStepEventHandler(object sender, UpdateStepEventArgs e);

    class UpdateStepEventArgs : EventArgs
    {
        public GameTime GameTime { get; set; }

        public UpdateStepEventArgs(GameTime gameTime) : base()
        {
            this.GameTime = gameTime;
        }
    }

}
