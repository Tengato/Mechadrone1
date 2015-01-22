using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1
{
    class CrosshairsWidget : GameUIElement
    {
        private Texture2D mCrosshairs;

        public CrosshairsWidget(ContentManager contentLoader)
        {
            mCrosshairs = contentLoader.Load<Texture2D>("textures\\crosshairs");
        }

        public override void Draw(UIElementsWindow drawSegment, GameTime gameTime)
        {
            Rectangle chRect = SharedResources.Game.GraphicsDevice.Viewport.Bounds;
            int size = (int)(MathHelper.Min(chRect.Width, chRect.Height) * 0.06f);

            chRect.Y = (chRect.Height - size) / 2;
            chRect.X = (chRect.Width - size) / 2;
            chRect.Width = size;
            chRect.Height = size;

            SharedResources.SpriteBatch.Begin();
            SharedResources.SpriteBatch.Draw(mCrosshairs, chRect,  Color.White);
            SharedResources.SpriteBatch.End();
        }
    }
}
