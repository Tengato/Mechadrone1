#region File Description
//-----------------------------------------------------------------------------
// FontManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Mechadrone1.StateManagement
{
    // supported font types and sizes
    public enum FontType
    {
        ArialSmall = 0,
        ArialMedium,
        ArialLarge
    };

    class FontManager
    {
        GraphicsDevice graphics;
        SpriteBatch sprite;
        List<SpriteFont> fonts;
        bool textMode;


        public FontManager(GraphicsDevice gd, SpriteBatch sb)
        {
            graphics = gd;
            sprite = sb;
            fonts = new List<SpriteFont>();
            textMode = false;
        }


        public void LoadContent(ContentManager content)
        {
            fonts.Add(content.Load<SpriteFont>("fonts/ArialS"));
            fonts.Add(content.Load<SpriteFont>("fonts/ArialM"));
            fonts.Add(content.Load<SpriteFont>("fonts/ArialL"));
        }


        /// <summary>
        /// Get the current screen rectangle
        /// </summary>
        public Rectangle ScreenRectangle
        {
            get
            {
                // TODO: This may not be correct:
                return new Rectangle( graphics.Viewport.X, graphics.Viewport.Y,
                    graphics.Viewport.Width, graphics.Viewport.Height);
            }
        }


        /// <summary>
        /// Enter text mode
        /// </summary>
        public void BeginText()
        {
            sprite.Begin();
            textMode = true;
        }


        /// <summary>
        /// Drawn text using given font, position and color
        /// </summary>
        public void DrawText(FontType font, String text, Vector2 position, Color color)
        {
            if (textMode)
                sprite.DrawString(fonts[(int)font], text, position, color);
        }


        /// <summary>
        /// Drawn text using given font, position and color, rotation, etc.
        /// </summary>
        public void DrawText(FontType font, String text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (textMode)
                sprite.DrawString(fonts[(int)font], text, position, color, rotation, origin, scale, effects, layerDepth);
        }


        /// <summary>
        /// End text mode
        /// </summary>
        public void EndText()
        {
            sprite.End();
            textMode = false;
        }


        /// <summary>
        /// Draw a texture in screen
        /// </summary>
        public void DrawTexture(
            Texture2D texture, 
            Rectangle rect, 
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();

            sprite.Begin(SpriteSortMode.Immediate, blend);
            sprite.Draw(texture, rect, color);
            sprite.End();

            if (textMode)
                sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }


        /// <summary>
        /// Draw a texture with rotation
        /// </summary>
        public void DrawTexture(
            Texture2D texture, 
            Rectangle rect, 
            float rotation, 
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();

            rect.X += rect.Width / 2;
            rect.Y += rect.Height / 2;

            sprite.Begin(SpriteSortMode.Immediate, blend);
            sprite.Draw(texture, rect, null, color, rotation, 
                new Vector2(rect.Width/2, rect.Height/2), SpriteEffects.None, 0);
            sprite.End();

            if (textMode)
                sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }


        /// <summary>
        /// Draw a texture with source and destination rectangles
        /// </summary>
        public void DrawTexture(
            Texture2D texture, 
            Rectangle destinationRect, 
            Rectangle sourceRect, 
            Color color,
            BlendState blend)
        {
            if (textMode)
                sprite.End();

            sprite.Begin(SpriteSortMode.Immediate, blend);
            sprite.Draw(texture, destinationRect, sourceRect, color);
            sprite.End();

            if (textMode)
                sprite.Begin();
        }


        public Vector2 MeasureString(FontType fontType, string str)
        {
            return fonts[(int)fontType].MeasureString(str);
        }


        public int LineSpacing(FontType fontType)
        {
            return fonts[(int)fontType].LineSpacing;
        }
    }
}
