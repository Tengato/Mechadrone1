using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1
{

    class InputState
    {
        public static PlayerIndex[] PlayerIndices = (PlayerIndex[])(Enum.GetValues(typeof(PlayerIndex)));
        public GamePadState[] PadState;
        public KeyboardState[] KeyState;

        public InputState()
        {
            PadState = new GamePadState[PlayerIndices.Length];
            KeyState = new KeyboardState[PlayerIndices.Length];
            ReadInput();
        }

        public void ReadInput()
        {
            for (int i = 0; i < PlayerIndices.Length; i++)
            {
                PadState[i] = GamePad.GetState(PlayerIndices[i]);
                KeyState[i] = Keyboard.GetState(PlayerIndices[i]);
            }
        }

        public void CopyInput(InputState state)
        {
            for (int i = 0; i < PlayerIndices.Length; i++)
            {
                PadState[i] = state.PadState[i];
                KeyState[i] = state.KeyState[i];
            }
        }
    }

}
