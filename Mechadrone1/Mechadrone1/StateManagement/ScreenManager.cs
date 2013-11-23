#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1;
using Microsoft.Xna.Framework.Audio;
using System.IO;
#endregion

namespace Mechadrone1.StateManagement
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen. It also makes common resources available to
    /// screens.
    /// </summary>
    class ScreenManager : DrawableGameComponent
    {

        List<Screen> screens;
        List<Screen> screensToUpdate;
        InputManager inputMan;
        FontManager fontMan;
        SpriteBatch spriteBatch;
        Texture2D blankTexture;
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        public FontManager FontManager
        {
            get { return fontMan; }
        }

        public SoundBank SoundBank
        {
            get { return soundBank; }
        }

        public bool TraceEnabled { get; set; }

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game) : base(game)
        {
            screens = new List<Screen>();
            screensToUpdate = new List<Screen>();
            inputMan = new InputManager();

            audioEngine = new AudioEngine("content/sounds/Mechadrone1.xgs");
            waveBank = new WaveBank(audioEngine, "content/sounds/WaBaUno.xwb");
            soundBank = new SoundBank(audioEngine, "content/sounds/SoBaUno.xsb");

            // Wait until LoadContent() is called before activating.
            Enabled = false;
            Visible = false;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontMan = new FontManager(GraphicsDevice, spriteBatch);
            fontMan.LoadContent(Game.Content);
            blankTexture = Game.Content.Load<Texture2D>("textures\\blank");

            // Tell each of the screens to load their content.
            foreach (Screen screen in screens)
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

            foreach (Screen screen in screens)
            {
                screen.UnloadContent();
            }
        }


        public override void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            foreach (Screen screen in screens)
            {
                screensToUpdate.Add(screen);
            }

            // These flags are initialized for the topmost screen:
            bool screenHasFocus = Game.IsActive;
            bool isInputConsumed = false;
            bool coveredByOtherScreen = false;
            bool isCoveringUnderlyingScreens = false;

            if (screenHasFocus)
                inputMan.ReadInput();

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                Screen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (screenHasFocus)
                    {
                        screen.HandleInput(gameTime, inputMan);
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
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (Screen screen in screens)
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
            foreach (Screen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }


        #region Public Methods


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

            screens.Add(screen);
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

            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public Screen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             Color.Black * alpha);

            spriteBatch.End();
        }


        #endregion
    }
}
