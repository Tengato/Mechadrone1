using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    delegate void UpdateStepEventHandler(object sender, UpdateStepEventArgs e);

    class UpdateStepEventArgs : EventArgs
    {
        public GameTime GameTime { get; set; }

        public UpdateStepEventArgs(GameTime gameTime)
            : base()
        {
            this.GameTime = gameTime;
        }
    }

}
