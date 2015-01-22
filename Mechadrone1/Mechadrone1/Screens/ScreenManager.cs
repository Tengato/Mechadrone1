using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1.Screens
{
    /// <summary>
    /// The screen manager manages one or more GameScreen instances. It maintains
    /// a stack of screens, calls their Update and Draw methods at the appropriate
    /// times, and routes input to the topmost active screen. It also makes common
    /// resources available to screens.
    /// </summary>
    class ScreenManager : DrawableGameComponent, IScreenManager
    {
        private List<Screen> mScreens;
        private List<Screen> mScreensToUpdate;
        private InputManager mInputMan;
        private Texture2D mBlankTexture;

        public bool TraceEnabled { get; set; }

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game) : base(game)
        {
            mScreens = new List<Screen>();
            mScreensToUpdate = new List<Screen>();
            mInputMan = new InputManager();
            mBlankTexture = null;

            // Wait until LoadContent() is called before activating.
            Enabled = false;
            Visible = false;
        }

        public void Cleanup()
        {
            while (mScreens.Count > 0)
            {
                RemoveScreen(mScreens[mScreens.Count - 1]);
            }
            SharedResources.Game = null;
            SharedResources.SpriteBatch = null;
            SharedResources.FontManager = null;
            SharedResources.AudioEngine = null;
            SharedResources.WaveBank = null;
            SharedResources.SoundBank = null;
            GameResources.PlaySession = null;
            GameResources.GameDossier = null;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            SharedResources.Game = Game;
            SharedResources.SpriteBatch = new SpriteBatch(GraphicsDevice);
            SharedResources.FontManager = new FontManager(new SpriteBatch(GraphicsDevice));
            SharedResources.FontManager.LoadContent(Game.Content);
            SharedResources.AudioEngine = new AudioEngine("content/sounds/Mechadrone1.xgs");
            SharedResources.WaveBank = new WaveBank(SharedResources.AudioEngine, "content/sounds/WaBaUno.xwb");
            SharedResources.SoundBank = new SoundBank(SharedResources.AudioEngine, "content/sounds/SoBaUno.xsb");

            mBlankTexture = Game.Content.Load<Texture2D>("textures\\blank");

            // Tell each of the screens to load their content.
            foreach (Screen screen in mScreens)
            {
                screen.LoadContent();
            }

            Enabled = true;
            Visible = true;
        }

        protected override void UnloadContent()
        {
            Enabled = false;
            Visible = false;

            base.UnloadContent();
        }


        public override void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            mScreensToUpdate.Clear();

            foreach (Screen screen in mScreens)
            {
                mScreensToUpdate.Add(screen);
            }

            // These flags are initialized for the topmost screen:
            bool screenHasFocus = Game.IsActive;
            bool isInputConsumed = false;
            bool coveredByOtherScreen = false;
            bool isCoveringUnderlyingScreens = false;

            if (screenHasFocus)
                mInputMan.ReadInput();

            // Loop as long as there are screens waiting to be updated.
            while (mScreensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                Screen screen = mScreensToUpdate[mScreensToUpdate.Count - 1];

                mScreensToUpdate.RemoveAt(mScreensToUpdate.Count - 1);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (screenHasFocus)
                    {
                        screen.HandleInput(gameTime, mInputMan);
                        isInputConsumed = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (screen.IsPopup)
                        isCoveringUnderlyingScreens = false;
                    else
                        isCoveringUnderlyingScreens = true;
                }

                // Update the screen.
                screen.Update(gameTime, !screenHasFocus, coveredByOtherScreen);

                // Update flags for next screen:
                screenHasFocus &= !isInputConsumed;
                coveredByOtherScreen |= isCoveringUnderlyingScreens;

            }

            // Print debug trace?
            if (TraceEnabled)
                TraceScreens();

            base.Update(gameTime);
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (Screen screen in mScreens)
                screenNames.Add(screen.GetType().Name);

            Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Note: this will loop in back-to-front order.
            foreach (Screen screen in mScreens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(Screen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (Enabled)
            {
                screen.LoadContent();
            }

            mScreens.Add(screen);
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(Screen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (Enabled)
            {
                screen.UnloadContent();
            }

            mScreens.Remove(screen);
            mScreensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public Screen[] GetScreens()
        {
            return mScreens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            SharedResources.SpriteBatch.Begin();

            SharedResources.SpriteBatch.Draw(mBlankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             Color.Black * alpha);

            SharedResources.SpriteBatch.End();
        }
    }
}
