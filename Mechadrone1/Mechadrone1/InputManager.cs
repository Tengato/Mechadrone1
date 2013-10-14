//-----------------------------------------------------------------------------
// InputManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1
{

    class InputManager
    {
        InputState currentState;   // current frame input
        InputState lastState;      // last frame input
        Vector2 mouseDragStart;
        MouseButtons? mouseDragButton;


        /// <summary>
        /// Create a new input manager
        /// </summary>
        public InputManager()
        {
            currentState = new InputState();
            lastState = new InputState();
            mouseDragButton = null;
        }


        /// <summary>
        /// Acquire input from all devices.
        /// </summary>
        public void ReadInput()
        {
            lastState.CopyInput(currentState);
            currentState.ReadInput();
            if (mouseDragButton == null)
            {
                if (currentState.MouseState.LeftButton == ButtonState.Pressed &&
                    lastState.MouseState.LeftButton == ButtonState.Pressed)
                {
                    mouseDragButton = MouseButtons.Left;
                }
                else if (currentState.MouseState.RightButton == ButtonState.Pressed &&
                    lastState.MouseState.RightButton == ButtonState.Pressed)
                {
                    mouseDragButton = MouseButtons.Right;
                }
                else if (currentState.MouseState.MiddleButton == ButtonState.Pressed &&
                    lastState.MouseState.MiddleButton == ButtonState.Pressed)
                {
                    mouseDragButton = MouseButtons.Middle;
                }

                if (mouseDragButton != null)
                {
                    mouseDragStart.X = lastState.MouseState.X;
                    mouseDragStart.Y = lastState.MouseState.Y;
                    Mouse.SetPosition((int)(mouseDragStart.X), (int)(mouseDragStart.Y));
                }

            }
            else
            {

                if (mouseDragButton == MouseButtons.Left &&
                    currentState.MouseState.LeftButton != ButtonState.Pressed ||
                    mouseDragButton == MouseButtons.Middle &&
                    currentState.MouseState.MiddleButton != ButtonState.Pressed ||
                    mouseDragButton == MouseButtons.Right &&
                    currentState.MouseState.RightButton != ButtonState.Pressed)
                {
                    mouseDragButton = null;
                }
                else
                {
                    Mouse.SetPosition((int)(mouseDragStart.X), (int)(mouseDragStart.Y));
                }
            }
        }


        public InputState CurrentState
        {
            get { return currentState; }
        }


        public InputState LastState
        {
            get { return lastState; }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                            out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentState.KeyState[i].IsKeyDown(key) &&
                        LastState.KeyState[i].IsKeyUp(key));
            }
            else
            {
                // Accept input from any player.
                return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
            }
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentState.PadState[i].IsButtonDown(button) &&
                        LastState.PadState[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
            }
        }


        public bool IsMouseDragging(MouseButtons button, PlayerIndex? controllingPlayer,
            out PlayerIndex playerIndex, out Vector2 displacement)
        {
            playerIndex = (PlayerIndex)(InputState.MouseUser);
            displacement = new Vector2(currentState.MouseState.X, currentState.MouseState.Y) -
                mouseDragStart;

            if (controllingPlayer == (PlayerIndex)(InputState.MouseUser) || controllingPlayer == null)
            {
                return button == mouseDragButton;
            }

            return false;
        }


        /// <summary>
        /// Checks for a "menu select" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "pause the game" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }

    }


    public enum MouseButtons
    {
        Left,
        Middle,
        Right,
        XButton1,
        XButton2
    }
}
