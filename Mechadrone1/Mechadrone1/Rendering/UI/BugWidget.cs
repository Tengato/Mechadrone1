using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class BugWidget : GameUIElement
    {
        public override void Draw(UIElementsWindow drawSegment, GameTime gameTime)
        {
            Vector2 textLocation;
            textLocation.X = 1000.0f;
            textLocation.Y = (float)(SharedResources.Game.GraphicsDevice.Viewport.Bounds.Bottom) - 30.0f;
            SharedResources.FontManager.BeginText();
            SharedResources.FontManager.DrawText(FontType.ArialLarge, "*", textLocation, Color.Crimson, false);
            SharedResources.FontManager.EndText();
        }
    }
}
