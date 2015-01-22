using Microsoft.Xna.Framework;
using Mechadrone1.Screens;
using Manifracture;

namespace Mechadrone1
{
    abstract class InputProcessor
    {
        protected const float INPUT_MOUSE_LOOK_FACTOR = 0.002f;
        protected const float INPUT_MOUSE_MOVE_FACTOR = 0.002f;
        protected const float INPUT_PAD_LOOK_FACTOR = 0.003f;
        protected const float INPUT_PAD_MOVE_FACTOR = 0.003f;

        public InputMap ActiveInputMap { get; set; }
        public PlayerIndex InputIndex { get; protected set; }

        public InputProcessor(PlayerIndex inputIndex)
        {
            InputIndex = inputIndex;
            ActiveInputMap = InputMap.Empty;
        }

        public abstract void HandleInput(GameTime gameTime, InputManager input);
    }
}
