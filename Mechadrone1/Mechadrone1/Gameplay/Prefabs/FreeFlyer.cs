using Microsoft.Xna.Framework;
using Mechadrone1.Gameplay.Helpers;
using Mechadrone1.Rendering;

namespace Mechadrone1.Gameplay.Prefabs
{
    class FreeFlyer : GameObject
    {

        FloatMovement mover;

        public FreeFlyer()
        {
            FloatMovementHandlingDesc handlingDesc;
            handlingDesc.DampingForce = 65.0f;
            handlingDesc.DampingRotationForce = 4.5f;
            handlingDesc.InputForce = 90.0f;
            handlingDesc.InputRotationForce = 10.0f;
            handlingDesc.MaxRotationVelocity = 1.8f;
            handlingDesc.MaxVelocity = 90.0f;
            mover = new FloatMovement(handlingDesc);

            mover.Reset(Matrix.CreateTranslation(Position) * Matrix.CreateFromQuaternion(Orientation));
        }

        public override void HandleInput(Microsoft.Xna.Framework.GameTime gameTime, InputManager input, PlayerIndex player, ICamera camera)
        {
            mover.ProcessInput((float)(gameTime.ElapsedGameTime.TotalSeconds), input.CurrentState, (int)player);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            mover.Update((float)(gameTime.ElapsedGameTime.TotalSeconds));
            Position = mover.position;
            Orientation = Quaternion.CreateFromRotationMatrix(mover.rotation);
        }
    }
}
