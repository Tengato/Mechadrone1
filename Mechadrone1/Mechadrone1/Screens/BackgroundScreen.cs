#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Mechadrone1.Screens
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class BackgroundScreen : Screen
    {
        private ContentManager mContentLoader;
        private Texture2D mImage;

        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5d);
            TransitionOffTime = TimeSpan.FromSeconds(0.5d);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (mContentLoader == null)
                mContentLoader = new ContentManager(SharedResources.Game.Services, "Content");

            mImage = mContentLoader.Load<Texture2D>("textures\\sci fi thing by rich4rt");
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            mContentLoader.Unload();
            mContentLoader.Dispose();
        }

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = SharedResources.Game.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            SharedResources.SpriteBatch.Begin();
            SharedResources.FontManager.BeginText();

            SharedResources.SpriteBatch.Draw(mImage, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            const string text = "image by:\nhttp://rich4rt.deviantart.com/";
            Vector2 position = new Vector2(20.0f, 620.0f);
            SharedResources.FontManager.DrawText(FontType.ArialSmall, text, position, Color.Multiply(Color.Multiply(Color.BurlyWood, 0.5f), TransitionAlpha), true);

            SharedResources.SpriteBatch.End();
            SharedResources.FontManager.EndText();
        }


        #endregion
    }
}
