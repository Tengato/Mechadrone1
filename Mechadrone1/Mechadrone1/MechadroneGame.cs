using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.Screens;
using Microsoft.Xna.Framework.GamerServices;

namespace Mechadrone1
{
    class MechadroneGame : Game
    {
        GraphicsDeviceManager mGraphics;
        ScreenManager mScreenMan;

        public MechadroneGame()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            mGraphics.PreferredBackBufferWidth = GameOptions.ScreenWidth;
            mGraphics.PreferredBackBufferHeight = GameOptions.ScreenHeight;
            mGraphics.PreferMultiSampling = false;

            mScreenMan = new ScreenManager(this);
            Components.Add(mScreenMan);
            SharedResources.GamerServices = new GamerServicesComponent(this);
            Components.Add(SharedResources.GamerServices);

            // Prime the ScreenManager with the first set of screens:
            mScreenMan.AddScreen(new BackgroundScreen(), null);
            mScreenMan.AddScreen(new MainMenuScreen(), null);

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
            mGraphics.ToggleFullScreen();
        }

        public void Cleanup()
        {
            Components.Clear();
            SharedResources.GamerServices = null;
            mScreenMan.Cleanup();
        }
    }
}
