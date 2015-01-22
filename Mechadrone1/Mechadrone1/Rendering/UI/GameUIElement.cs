using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    abstract class GameUIElement
    {
        // Usually a subclass will need to use the Scene object to draw itself and will make a call like this:
        // drawSegment.Scene.Draw(drawSegment.Camera);
        public abstract void Draw(UIElementsWindow drawSegment, GameTime gameTime);
    }
}
