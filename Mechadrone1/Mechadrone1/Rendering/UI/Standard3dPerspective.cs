using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class Standard3dPerspective : GameUIElement
    {
        public override void Draw(UIElementsWindow drawSegment, GameTime gameTime)
        {
            drawSegment.Scene.Draw(drawSegment.Camera);
        }
    }
}
