using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1.Screens
{
    // A single entry in a MenuScreen.
    class MenuEntry
    {
        private float mSelectionFade;
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public event EventHandler<PlayerIndexEventArgs> Selected;

        public MenuEntry(string text)
        {
            Text = text;
            Position = Vector2.Zero;
            mSelectionFade = 0.0f;
        }

        public virtual void HandleInput(GameTime gameTime, InputManager input, PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            if (input.IsMenuSelect(controllingPlayer, out playerIndex))
                OnSelected(new PlayerIndexEventArgs(playerIndex));
        }

        protected void OnSelected(PlayerIndexEventArgs e)
        {
            EventHandler<PlayerIndexEventArgs> handler = Selected;
            if (handler != null)
                handler(this, e);
        }

        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                mSelectionFade = Math.Min(mSelectionFade + fadeSpeed, 1);
            else
                mSelectionFade = Math.Max(mSelectionFade - fadeSpeed, 0);
        }

        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(float transitionValue, bool isSelected, GameTime gameTime)
        {
            // Draw the selected entry in yellow, otherwise white.
            Color color = isSelected ? Color.Cyan : Color.DarkSlateGray;

            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;
            
            float pulsate = (float)Math.Sin(time * 6) + 1;

            float scale = 1 + pulsate * 0.05f * mSelectionFade;

            // Modify the alpha to fade text out during transitions.
            color *= transitionValue;

            Vector2 origin = new Vector2(0, SharedResources.FontManager.LineSpacing(FontType.ArialMedium) / 2);

            SharedResources.FontManager.DrawText(FontType.ArialMedium, Text, Position, color, true, 0,
                                   origin, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public virtual int GetHeight()
        {
            return SharedResources.FontManager.LineSpacing(FontType.ArialMedium);
        }

        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public virtual int GetWidth()
        {
            return (int)(SharedResources.FontManager.MeasureString(FontType.ArialMedium, Text).X);
        }
    }
}
