using Microsoft.Xna.Framework;

namespace Mechadrone1.Screens
{
    class HelpScreen : MenuScreen
    {
        public HelpScreen()
            : base("Help")
        {
            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(back);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            const string textCol1 = "Move\n" +
                                    "Look\n" +
                                    "Jump\n" +
                                    "Crouch\n" +
                                    "Zoom";

            const string textCol2 = "WASD, Left Stick\n" +
                                    "Drag Mouse, Right Stick\n" +
                                    "Space, A Button\n" +
                                    "LShift, Left Stick Button\n" +
                                    "Mouse Wheel, Left/Right Triggers\n";

            Vector2 position1 = new Vector2(400.0f, 320.0f);
            Vector2 position2 = new Vector2(670.0f, 320.0f);
            SharedResources.FontManager.BeginText();
            SharedResources.FontManager.DrawText(FontType.ArialSmall, textCol1, position1, Color.Multiply(Color.PaleGreen, TransitionAlpha), true);
            SharedResources.FontManager.DrawText(FontType.ArialSmall, textCol2, position2, Color.Multiply(Color.PaleGreen, TransitionAlpha), true);
            SharedResources.FontManager.EndText();
        }
    }
}
