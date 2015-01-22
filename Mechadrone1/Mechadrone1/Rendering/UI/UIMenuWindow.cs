using System.Collections.Generic;
using Mechadrone1.Screens;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    abstract class UIMenuWindow : UIWindow
    {
        protected List<MenuEntry> mMenuEntries;
        protected int mSelectedEntryIndex;
        public string Title { get; set; }

        public override InputHandler InputHandler { get { return HandleInput; } }

        public UIMenuWindow()
        {
            mMenuEntries = new List<MenuEntry>();
            mSelectedEntryIndex = 0;
            Title = String.Empty;
        }

        public override void Draw(float aspectRatio, GameTime gameTime)
        {
            SharedResources.FontManager.BeginText();

            Vector2 position = new Vector2(0f, 175f);

            // Draw each menu entry in turn.
            for (int i = 0; i < mMenuEntries.Count; ++i)
            {
                MenuEntry menuEntry = mMenuEntries[i];
                position.X = SharedResources.Game.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth() / 2;
                menuEntry.Position = position;
                position.Y += menuEntry.GetHeight();

                bool isSelected = (i == mSelectedEntryIndex);

                menuEntry.Draw(1.0f, isSelected, gameTime);
            }

            // Draw the window title centered on the screen
            Vector2 titlePosition = new Vector2(SharedResources.Game.GraphicsDevice.Viewport.Width / 2, 80);
            Vector2 titleOrigin = SharedResources.FontManager.MeasureString(FontType.ArialMedium, Title) / 2;
            float titleScale = 1.25f;

            SharedResources.FontManager.DrawText(FontType.ArialMedium, Title, titlePosition, Color.OldLace, true, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            SharedResources.FontManager.EndText();
        }

        private void HandleInput(GameTime gameTime, InputManager input)
        {
            PlayerIndex dummyPlayerIndex;

            if (input.IsMenuCancel(ControllingPlayer, out dummyPlayerIndex))
            {
                ExitWindow();
                return;
            }

            if (mSelectedEntryIndex >= 0)
                mMenuEntries[mSelectedEntryIndex].HandleInput(gameTime, input, ControllingPlayer);

            if (input.IsMenuUp(ControllingPlayer))
            {
                mSelectedEntryIndex--;

                if (mSelectedEntryIndex < 0)
                    mSelectedEntryIndex = 0;
            }

            if (input.IsMenuDown(ControllingPlayer))
            {
                mSelectedEntryIndex++;

                if (mSelectedEntryIndex >= mMenuEntries.Count)
                    mSelectedEntryIndex = mMenuEntries.Count - 1;
            }
        }
    }
}
