//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Threading;
using Manifracture;
using Mechadrone1.Gameplay;
using Mechadrone1.Rendering;
using Mechadrone1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1.Screens
{
    /// <summary>
    /// This screen takes care of the gameplay presentation.
    /// </summary>
    class GameplayScreen : Screen
    {

        ContentManager content;
        SceneManager sceneMan;
        Game1Manager gameMan;

        string level;
        Game1Manifest manifest;

        float pauseAlpha;
        int frameCounter;
        int frameRate;
        TimeSpan fpsTimer;


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(string level)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            this.level = level;

            frameCounter = 0;
            frameRate = 0;
        }


        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            manifest = content.Load<Game1Manifest>(level);

            gameMan = new Game1Manager(ScreenManager.SoundBank);
            gameMan.LoadContent(ScreenManager.GraphicsDevice, content, manifest);
            gameMan.AddPlayer((PlayerIndex)ControllingPlayer, "CameraTarget");
            sceneMan = new SceneManager(ScreenManager.GraphicsDevice);
            sceneMan.LoadContent(gameMan, content);

        }


        public override void UnloadContent()
        {
            content.Unload();
        }


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            // TODO: I'm guessing the 2nd param is false here because this screen wants to do it's own covered effect?
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            fpsTimer += gameTime.ElapsedGameTime;

            if (fpsTimer > TimeSpan.FromSeconds(1))
            {
                fpsTimer -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            if (IsActive)
            {
                gameMan.Update(gameTime);
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputManager input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // TODO: You should let any active player participate in some of these checks
            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            // TODO: This won't work for more than one frame:
            bool gamePadDisconnected = !input.CurrentState.PadState[playerIndex].IsConnected &&
                                       input.LastState.PadState[playerIndex].IsConnected;

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                gameMan.HandleInput(gameTime, input);
            }

        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            sceneMan.DrawScene((PlayerIndex)ControllingPlayer);

            string fps = string.Format("fps: {0}", frameRate);

            ScreenManager.FontManager.BeginText();
            ScreenManager.FontManager.DrawText(FontType.ArialSmall, fps, new Vector2(33, 33), Color.Black);
            ScreenManager.FontManager.DrawText(FontType.ArialSmall, fps, new Vector2(32, 32), Color.White);
            ScreenManager.FontManager.EndText();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

        }

    }

}
