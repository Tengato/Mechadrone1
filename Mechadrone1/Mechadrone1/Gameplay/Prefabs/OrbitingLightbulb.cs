using Mechadrone1.Gameplay.Decorators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1.Gameplay.Prefabs
{
    class OrbitingLightbulb : GameObject
    {
        public Color Color { get; set; }
        public Vector3 Center { get; set; }


        public OrbitingLightbulb(IGameManager owner)
            : base(owner)
        {
        }


        public override void RegisterUpdateHandlers()
        {
            owner.PreAnimationUpdateStep += PreAnimationUpdate;
        }


        public void PreAnimationUpdate(object sender, UpdateEventArgs e)
        {
            Position = Center + Vector3.Transform(Position - Center, Matrix.CreateFromAxisAngle(Vector3.Up, (float)(e.GameTime.ElapsedGameTime.TotalSeconds)));

            UpdateQuadTree();
        }
    }
}
