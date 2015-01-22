using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    // Gathers data required to make a Draw call to the GraphicsDevice.
    class RenderContext
    {
        public SceneResources SceneResources { get; private set; }
        public BoundingFrustum VisibilityFrustum { get; set; }
        public Vector3 EyePosition { get; set; }

        public RenderContext(SceneResources sceneResources)
        {
            SceneResources = sceneResources;
            VisibilityFrustum = null;
            EyePosition = Vector3.Zero;
        }
    }
}
