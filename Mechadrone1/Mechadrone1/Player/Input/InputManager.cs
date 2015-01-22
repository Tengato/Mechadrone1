//-----------------------------------------------------------------------------
// InputManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using Manifracture;
using System.Collections.Generic;

namespace Mechadrone1
{

    class InputManager
    {
        public static InputManager NeutralInput { get; private set; }

        static InputManager()
        {
            NeutralInput = new InputManager();
        }

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
                return mouseDragButton == null ? false : ((button & mouseDragButton) > 0);
            }

            return false;
        }


        public int ScrollWheelDiff()
        {
            return currentState.MouseState.ScrollWheelValue - lastState.MouseState.ScrollWheelValue;
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

        public bool IsMenuDelete(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Delete, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.X, controllingPlayer, out playerIndex);
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

        public bool CheckForNewBinaryInput(InputMap inputMap, BinaryControlActions action, PlayerIndex inputIndex)
        {
            if (inputMap.BinaryMap.ContainsKey(action))
            {
                foreach (BinaryControls control in inputMap.BinaryMap[action])
                {
                    if (IsNewBinaryControlPress(control, inputIndex))
                        return true;
                }
            }
            return false;
        }

        public bool CheckForBinaryInput(InputMap inputMap, BinaryControlActions action, PlayerIndex inputIndex)
        {
            if (inputMap.BinaryMap.ContainsKey(action))
            {
                foreach (BinaryControls control in inputMap.BinaryMap[action])
                {
                    if (IsBinaryControlDown(control, inputIndex))
                        return true;
                }
            }
            return false;
        }

        public bool IsNewMouseButtonPress(MouseButtons buttons, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                if (controllingPlayer != (PlayerIndex)(InputState.MouseUser))
                    return false;

                MouseButtons newPresses = 0;

                if ((CurrentState.MouseState.LeftButton == ButtonState.Pressed &&
                LastState.MouseState.LeftButton == ButtonState.Released))
                    newPresses |= MouseButtons.Left;

                if ((CurrentState.MouseState.MiddleButton == ButtonState.Pressed &&
                LastState.MouseState.MiddleButton == ButtonState.Released))
                    newPresses |= MouseButtons.Middle;

                if ((CurrentState.MouseState.RightButton == ButtonState.Pressed &&
                LastState.MouseState.RightButton == ButtonState.Released))
                    newPresses |= MouseButtons.Right;

                if ((CurrentState.MouseState.XButton1 == ButtonState.Pressed &&
                LastState.MouseState.XButton1 == ButtonState.Released))
                    newPresses |= MouseButtons.XButton1;

                if ((CurrentState.MouseState.XButton2 == ButtonState.Pressed &&
                LastState.MouseState.XButton2 == ButtonState.Released))
                    newPresses |= MouseButtons.XButton2;

                return newPresses == buttons;
            }
            else
            {
                // Accept input from any player.
                return (IsNewMouseButtonPress(buttons, PlayerIndex.One, out playerIndex) ||
                        IsNewMouseButtonPress(buttons, PlayerIndex.Two, out playerIndex) ||
                        IsNewMouseButtonPress(buttons, PlayerIndex.Three, out playerIndex) ||
                        IsNewMouseButtonPress(buttons, PlayerIndex.Four, out playerIndex));
            }
        }

        public bool IsAnyBinaryControlPressed(List<BinaryControls> controls, PlayerIndex player)
        {
            foreach (BinaryControls control in controls)
            {
                if (IsBinaryControlDown(control, player))
                    return true;
            }

            return false;
        }

        public bool IsNewBinaryControlPress(BinaryControls control, PlayerIndex player)
        {
            int controlInt = (int)control;
            if (controlInt <= 290)
            {
                return IsBinaryControlDown(control, player, CurrentState) &&
                    !IsBinaryControlDown(control, player, LastState);
            }
            else
            {
                if ((int)player == InputState.MouseUser)
                {
                    switch (control)
                    {
                        case BinaryControls.ScrollWheelDown:
                            return (ScrollWheelDiff() < 0);
                        case BinaryControls.ScrollWheelUp:
                            return (ScrollWheelDiff() > 0);
                    }
                }
                return false;
            }
        }

        public bool IsBinaryControlDown(BinaryControls control, PlayerIndex player)
        {
            return IsBinaryControlDown(control, player, CurrentState);
        }

        static public bool IsBinaryControlDown(BinaryControls control, PlayerIndex player, InputState state)
        {
            int controlInt = (int)control;
            if (controlInt <= 254)
            {
                return (state.KeyState[(int)player].IsKeyDown((Keys)controlInt));
            }
            else if (controlInt <= 279)
            {
                return (state.PadState[(int)player].IsButtonDown((Buttons)(1 << (controlInt - 255))));
            }
            else if (controlInt <= 290)
            {
                MouseState ms = state.GetMouseState((int)player);
                switch ((MouseButtons)(1 << (controlInt - 286)))
                {
                    case MouseButtons.Left:
                        return (ms.LeftButton == ButtonState.Pressed);
                    case MouseButtons.Middle:
                        return (ms.MiddleButton == ButtonState.Pressed);
                    case MouseButtons.Right:
                        return (ms.RightButton == ButtonState.Pressed);
                    case MouseButtons.XButton1:
                        return (ms.XButton1 == ButtonState.Pressed);
                    case MouseButtons.XButton2:
                        return (ms.XButton2 == ButtonState.Pressed);
                }
            }
            return false;
        }

        public float GetFullIntervalControlValue(FullIntervalControlSpecification control, PlayerIndex player)
        {
            float result = 0.0f;

            if (control.EnablerControl == BinaryControls.None ||
                IsBinaryControlDown(control.EnablerControl, player) ^ control.IsEnablerInverted)
            {
                switch (control.ValueControl)
                {
                    case FullIntervalControls.LeftAndRightTriggersGamepad:
                        result = CurrentState.PadState[(int)player].Triggers.Right - CurrentState.PadState[(int)player].Triggers.Left;
                        break;
                    case FullIntervalControls.LeftThumbstickXGamePad:
                        result = CurrentState.PadState[(int)player].ThumbSticks.Left.X;
                        break;
                    case FullIntervalControls.LeftThumbstickYGamePad:
                        result = CurrentState.PadState[(int)player].ThumbSticks.Left.Y;
                        break;
                    case FullIntervalControls.RightThumbstickXGamePad:
                        result = CurrentState.PadState[(int)player].ThumbSticks.Right.X;
                        break;
                    case FullIntervalControls.RightThumbstickYGamePad:
                        result = CurrentState.PadState[(int)player].ThumbSticks.Right.Y;
                        break;
                }
            }

            return control.IsValueInverted ? -result : result;
        }

        public float GetHalfIntervalControlValue(HalfIntervalControlSpecification control, PlayerIndex player)
        {
            float result = 0.0f;


            if (control.EnablerControl == BinaryControls.None ||
                IsBinaryControlDown(control.EnablerControl, player) ^ control.IsEnablerInverted)
            {
                switch (control.ValueControl)
                {
                    case HalfIntervalControls.LeftTriggerGamePad:
                        result = CurrentState.PadState[(int)player].Triggers.Left;
                        break;
                    case HalfIntervalControls.RightTriggerGamePad:
                        result = CurrentState.PadState[(int)player].Triggers.Right;
                        break;
                    case HalfIntervalControls.LeftThumbStickUpGamePad:
                        result = Math.Max(CurrentState.PadState[(int)player].ThumbSticks.Left.Y, 0.0f);
                        break;
                    case HalfIntervalControls.LeftThumbStickDownGamePad:
                        result = -Math.Min(CurrentState.PadState[(int)player].ThumbSticks.Left.Y, 0.0f);
                        break;
                    case HalfIntervalControls.LeftThumbStickLeftGamePad:
                        result = -Math.Min(CurrentState.PadState[(int)player].ThumbSticks.Left.X, 0.0f);
                        break;
                    case HalfIntervalControls.LeftThumbStickRightGamePad:
                        result = Math.Max(CurrentState.PadState[(int)player].ThumbSticks.Left.X, 0.0f);
                        break;
                    case HalfIntervalControls.RightThumbStickUpGamePad:
                        result = Math.Max(CurrentState.PadState[(int)player].ThumbSticks.Right.Y, 0.0f);
                        break;
                    case HalfIntervalControls.RightThumbStickDownGamePad:
                        result = -Math.Min(CurrentState.PadState[(int)player].ThumbSticks.Right.Y, 0.0f);
                        break;
                    case HalfIntervalControls.RightThumbStickLeftGamePad:
                        result = -Math.Min(CurrentState.PadState[(int)player].ThumbSticks.Right.X, 0.0f);
                        break;
                    case HalfIntervalControls.RightThumbStickRightGamePad:
                        result = Math.Max(CurrentState.PadState[(int)player].ThumbSticks.Right.X, 0.0f);
                        break;
                }
            }

            return control.IsValueInverted ? 1.0f - result : result;
        }

        public float GetFullAxisControlValue(FullAxisControlSpecification control, PlayerIndex player)
        {
            float result = 0.0f;

            PlayerIndex dummyPlayerIndex;
            Vector2 displacement = Vector2.Zero;

            if (player == (PlayerIndex)(InputState.MouseUser))
            {
                switch (control.ValueControl)
                {
                    case FullAxisControls.NoDragMoveMouseX:
                    case FullAxisControls.NoDragMoveMouseY:
                        if (IsMouseDragging((MouseButtons)31, player, out dummyPlayerIndex, out displacement))
                            displacement = Vector2.Zero;
                        break;
                    case FullAxisControls.LeftOrRightDragMouseX:
                    case FullAxisControls.LeftOrRightDragMouseY:
                        if (!IsMouseDragging(MouseButtons.Left | MouseButtons.Right, player, out dummyPlayerIndex, out displacement))
                            displacement = Vector2.Zero;
                        break;
                    case FullAxisControls.AnyMoveMouseX:
                    case FullAxisControls.AnyMoveMouseY:
                        displacement = new Vector2(CurrentState.MouseState.X, CurrentState.MouseState.Y) -
                                new Vector2(LastState.MouseState.X, LastState.MouseState.Y);
                        break;
                    case FullAxisControls.LeftDragMouseX:
                    case FullAxisControls.LeftDragMouseY:
                        if (!IsMouseDragging(MouseButtons.Left, player, out dummyPlayerIndex, out displacement))
                            displacement = Vector2.Zero;
                        break;
                    case FullAxisControls.MiddleDragMouseX:
                    case FullAxisControls.MiddleDragMouseY:
                        if (!IsMouseDragging(MouseButtons.Middle, player, out dummyPlayerIndex, out displacement))
                            displacement = Vector2.Zero;
                        break;
                    case FullAxisControls.RightDragMouseX:
                    case FullAxisControls.RightDragMouseY:
                        if (!IsMouseDragging(MouseButtons.Right, player, out dummyPlayerIndex, out displacement))
                            displacement = Vector2.Zero;
                        break;
                    default:
                        break;
                }
            }

            switch (control.ValueControl)
            {
                case FullAxisControls.NoDragMoveMouseX:
                case FullAxisControls.AnyMoveMouseX:
                case FullAxisControls.LeftDragMouseX:
                case FullAxisControls.MiddleDragMouseX:
                case FullAxisControls.RightDragMouseX:
                case FullAxisControls.LeftOrRightDragMouseX:
                    result = displacement.X;
                    break;
                case FullAxisControls.NoDragMoveMouseY:
                case FullAxisControls.AnyMoveMouseY:
                case FullAxisControls.LeftDragMouseY:
                case FullAxisControls.MiddleDragMouseY:
                case FullAxisControls.RightDragMouseY:
                case FullAxisControls.LeftOrRightDragMouseY:
                    result = -displacement.Y;   // On a mouse pad, the y-axis is pointing toward the user (typically down on the screen)
                    break;
                case FullAxisControls.ScrollWheelMouse:
                    result = ScrollWheelDiff();
                    break;
            }

            return control.IsValueInverted ? -result : result;
        }
    }

    [Flags]
    public enum MouseButtons
    {
        Left = 1,
        Middle = 2,
        Right = 4,
        XButton1 = 8,
        XButton2 = 16,
    }
}
