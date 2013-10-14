using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1
{

    class InputState
    {
        public GamePadState[] PadState;
        public KeyboardState[] KeyState;
        public MouseState MouseState;

        public static int MouseUser = 0;

        public const int MAX_PLAYERS = 4;

        public InputState()
        {
            PadState = new GamePadState[MAX_PLAYERS];
            KeyState = new KeyboardState[MAX_PLAYERS];
            ReadInput();
        }

        public void ReadInput()
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PadState[i] = GamePad.GetState((PlayerIndex)i);
                KeyState[i] = Keyboard.GetState((PlayerIndex)i);
            }

            MouseState = Mouse.GetState();
        }

        public void CopyInput(InputState state)
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PadState[i] = state.PadState[i];
                KeyState[i] = state.KeyState[i];
            }

            MouseState = state.MouseState;
        }

        public MouseState GetMouseState(int playerIndex)
        {
            if (playerIndex == MouseUser)
                return MouseState;
            else
                return new MouseState();
        }
    }

}
