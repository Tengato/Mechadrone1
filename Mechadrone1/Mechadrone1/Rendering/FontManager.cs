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

namespace Mechadrone1
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
        private SpriteBatch mSpriteBatch;
        public List<SpriteFont> Fonts { get; private set; }
        private bool mIsTextModeActive;
        private Vector2 mDropShadowOffset;

        public FontManager(SpriteBatch sb)
        {
            mSpriteBatch = sb;
            Fonts = new List<SpriteFont>();
            mIsTextModeActive = false;
            mDropShadowOffset = new Vector2(1.0f, 1.0f);
        }

        public void LoadContent(ContentManager content)
        {
            Fonts.Add(content.Load<SpriteFont>("fonts/ArialS"));
            Fonts.Add(content.Load<SpriteFont>("fonts/ArialM"));
            Fonts.Add(content.Load<SpriteFont>("fonts/ArialL"));
        }

        public void BeginText()
        {
            mSpriteBatch.Begin();
            mIsTextModeActive = true;
        }


        /// <summary>
        /// Drawn text using given font, position and color
        /// </summary>
        public void DrawText(FontType font, String text, Vector2 position, Color color, bool dropShadow)
        {
            if (mIsTextModeActive)
            {
                if (dropShadow)
                    mSpriteBatch.DrawString(Fonts[(int)font], text, position + mDropShadowOffset, new Color(color.ToVector4() * new Vector4(0, 0, 0, 1)));

                mSpriteBatch.DrawString(Fonts[(int)font], text, position, color);
            }
        }


        /// <summary>
        /// Drawn text using given font, position and color, rotation, etc.
        /// </summary>
        public void DrawText(FontType font, String text, Vector2 position, Color color, bool dropShadow, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            if (mIsTextModeActive)
            {
                if (dropShadow)
                    mSpriteBatch.DrawString(Fonts[(int)font], text, position + mDropShadowOffset, new Color(color.ToVector4() * new Vector4(0, 0, 0, 1)), rotation, origin, scale, effects, layerDepth);

                mSpriteBatch.DrawString(Fonts[(int)font], text, position, color, rotation, origin, scale, effects, layerDepth);
            }
        }


        /// <summary>
        /// End text mode
        /// </summary>
        public void EndText()
        {
            mSpriteBatch.End();
            mIsTextModeActive = false;
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
            if (mIsTextModeActive)
                mSpriteBatch.End();

            mSpriteBatch.Begin(SpriteSortMode.Immediate, blend);
            mSpriteBatch.Draw(texture, rect, color);
            mSpriteBatch.End();

            if (mIsTextModeActive)
                mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
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
            if (mIsTextModeActive)
                mSpriteBatch.End();

            rect.X += rect.Width / 2;
            rect.Y += rect.Height / 2;

            mSpriteBatch.Begin(SpriteSortMode.Immediate, blend);
            mSpriteBatch.Draw(texture, rect, null, color, rotation, 
                new Vector2(rect.Width/2, rect.Height/2), SpriteEffects.None, 0);
            mSpriteBatch.End();

            if (mIsTextModeActive)
                mSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
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
            if (mIsTextModeActive)
                mSpriteBatch.End();

            mSpriteBatch.Begin(SpriteSortMode.Immediate, blend);
            mSpriteBatch.Draw(texture, destinationRect, sourceRect, color);
            mSpriteBatch.End();

            if (mIsTextModeActive)
                mSpriteBatch.Begin();
        }


        public Vector2 MeasureString(FontType fontType, string str)
        {
            return Fonts[(int)fontType].MeasureString(str);
        }


        public int LineSpacing(FontType fontType)
        {
            return Fonts[(int)fontType].LineSpacing;
        }
    }
}
