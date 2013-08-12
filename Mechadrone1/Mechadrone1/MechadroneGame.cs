#define PROFILE

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.StateManagement;
using Mechadrone1.Screens;

namespace Mechadrone1
{
    class MechadroneGame : Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenMan;

        public MechadroneGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = GameOptions.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameOptions.ScreenHeight;

            screenMan = new ScreenManager(this);
            Components.Add(screenMan);

            // Prime the ScreenManager with the first set of screens:
            screenMan.AddScreen(new BackgroundScreen(), null);
            screenMan.AddScreen(new MainMenuScreen(), null);

            #if PROFILE
                this.IsFixedTimeStep=false;
                graphics.SynchronizeWithVerticalRetrace = false;
            #endif
        }

        /// <summary>
        /// Toggles between full screen and windowed mode.
        /// </summary>
        public void ToggleFullScreen()
        {
            graphics.ToggleFullScreen();
        }

    }
}
