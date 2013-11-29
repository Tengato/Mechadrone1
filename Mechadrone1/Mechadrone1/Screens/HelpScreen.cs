#region File Description
//-----------------------------------------------------------------------------
// HelpScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Mechadrone1.StateManagement;
#endregion

namespace Mechadrone1.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class HelpScreen : MenuScreen
    {

        /// <summary>
        /// Constructor.
        /// </summary>
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
            ScreenManager.FontManager.BeginText();
            ScreenManager.FontManager.DrawText(FontType.ArialSmall, textCol1, position1, Color.Multiply(Color.PaleGreen, TransitionAlpha), true);
            ScreenManager.FontManager.DrawText(FontType.ArialSmall, textCol2, position2, Color.Multiply(Color.PaleGreen, TransitionAlpha), true);
            ScreenManager.FontManager.EndText();
        }

    }
}
