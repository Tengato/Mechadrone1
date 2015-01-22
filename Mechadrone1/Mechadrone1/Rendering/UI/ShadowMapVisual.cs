using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    class ShadowMapVisual : GameUIElement
    {
        public override void Draw(UIElementsWindow drawSegment, GameTime gameTime)
        {
            SharedResources.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            SharedResources.SpriteBatch.Draw((Texture2D)(drawSegment.Scene.Resources.ShadowMap), SharedResources.Game.GraphicsDevice.Viewport.Bounds, Color.White);
            SharedResources.SpriteBatch.End();
        }
    }
}
