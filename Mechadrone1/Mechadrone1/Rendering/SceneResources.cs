using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    class SceneResources
    {
        public const int SMAP_SIZE = 2048;
        public RenderTarget2D ShadowMap { get; set; }
        public Matrix ShadowTransform { get; set; }
        public TextureCube EnvironmentMap { get; set; }
        public Texture FringeMap { get; set; }
        public int ShadowCasterActorId { get; set; }
        public int FogActorId { get; set; }
        public int HDRLightActorId { get; set; }

        public SceneResources()
        {
            ShadowMap = null;
            ShadowTransform = Matrix.Identity;
            EnvironmentMap = null;
            FringeMap = null;
            ShadowCasterActorId = Actor.INVALID_ACTOR_ID;
            FogActorId = Actor.INVALID_ACTOR_ID;
            HDRLightActorId = Actor.INVALID_ACTOR_ID;
        }

    }
}
