using Mechadrone1.Gameplay.Decorators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1.Gameplay.Prefabs
{
    class OrbitingLightbulb : GameObject
    {
        public Color Color { get; set; }
        public Vector3 Center { get; set; }

        public override void Update(GameTime gameTime)
        {
            Position = Center + Vector3.Transform(Position - Center, Matrix.CreateFromAxisAngle(Vector3.Up, (float)(gameTime.ElapsedGameTime.TotalSeconds)));
        }
    }
}
