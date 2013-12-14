using Microsoft.Xna.Framework;
using Mechadrone1.Gameplay;
using Mechadrone1.Gameplay.Helpers;
using Mechadrone1.Rendering;

namespace Mechadrone1.Gameplay.Prefabs
{
    class FreeFlyer : GameObject
    {
        FloatMovement mover;


        public FreeFlyer(IGameManager owner) : base(owner)
        {
            FloatMovementHandlingDesc handlingDesc;
            handlingDesc.DampingForce = 65.0f;
            handlingDesc.DampingRotationForce = 4.5f;
            handlingDesc.InputForce = 90.0f;
            handlingDesc.InputRotationForce = 10.0f;
            handlingDesc.MaxRotationVelocity = 1.8f;
            handlingDesc.MaxVelocity = 90.0f;
            mover = new FloatMovement(handlingDesc);

            mover.Reset(Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position));
        }


        public override void Initialize()
        {
            base.Initialize();

            mover.Reset(Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position));
        }


        public override void RegisterUpdateHandlers()
        {
            game.PreAnimationUpdateStep += PreAnimationUpdate;
        }


        public override void HandleInput(Microsoft.Xna.Framework.GameTime gameTime, InputManager input, PlayerIndex player)
        {
            mover.ProcessInput((float)(gameTime.ElapsedGameTime.TotalSeconds), input.CurrentState, (int)player);
        }


        public void PreAnimationUpdate(object sender, UpdateStepEventArgs e)
        {
            mover.Update((float)(e.GameTime.ElapsedGameTime.TotalSeconds));
            Position = mover.Position;
            Orientation = Quaternion.CreateFromRotationMatrix(mover.Rotation);

            UpdateQuadTree();
        }
    }
}
