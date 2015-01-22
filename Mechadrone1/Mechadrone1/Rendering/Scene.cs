using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using SlagformCommon;

namespace Mechadrone1
{
    // Manages the scene graph by keeping track of RenderComponents that get created by the ActorManager.
    // It also contains the high-level logic for rendering a standard 3D perspective scene.
    class Scene
    {
        private static readonly Matrix sShadowTextureShift;

        public SceneResources Resources { get; private set; }
        private ContentManager mContentLoader;
        public SceneGraphRoot SceneGraph { get; private set; }

        static Scene()
        {
            sShadowTextureShift = new Matrix(
                0.5f, 0.0f, 0.0f, 0.0f,
                0.0f, -0.5f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.5f, 0.5f, 0.0f, 1.0f);
        }

        public Scene()
        {
            Resources = new SceneResources();
            mContentLoader = new ContentManager(SharedResources.Game.Services, "Content");
            SceneGraph = new SceneGraphRoot(Resources);
            RenderComponent.RenderComponentInitialized += RenderComponentInitializedHandler;
            SkyboxRenderComponent.EnvironmentMapAdded += EnvironmentMapAddedHandler;
            ShadowCasterComponent.ShadowCasterCreated += ShadowCasterCreatedHandler;
            FogComponent.FogCreated += FogCreatedHandler;
            HDRLightComponent.HDRLightCreated += HDRLightCreatedHandler;
        }

        public void Release()
        {
            mContentLoader.Unload();
            mContentLoader.Dispose();
            RenderComponent.RenderComponentInitialized -= RenderComponentInitializedHandler;
            SkyboxRenderComponent.EnvironmentMapAdded -= EnvironmentMapAddedHandler;
            ShadowCasterComponent.ShadowCasterCreated -= ShadowCasterCreatedHandler;
            FogComponent.FogCreated -= FogCreatedHandler;
            HDRLightComponent.HDRLightCreated -= HDRLightCreatedHandler;
        }

        public void Load()
        {
            EffectRegistry.DepthOnlySkinFx = mContentLoader.Load<Effect>("shaders\\DepthOnlySkin");
            EffectRegistry.DepthOnlyFx = mContentLoader.Load<Effect>("shaders\\DepthOnly");
            EffectRegistry.SkyboxFx = mContentLoader.Load<Effect>("shaders\\Skymap");

            Resources.FringeMap = FringeMap.Initialize();
            Resources.ShadowMap = new RenderTarget2D(SharedResources.Game.GraphicsDevice,
                                                  SceneResources.SMAP_SIZE,
                                                  SceneResources.SMAP_SIZE,
                                                  false,
                                                  SurfaceFormat.Single,
                                                  DepthFormat.Depth24);

            GeneratedGeometry.Initialize();
        }

        private void RenderComponentInitializedHandler(object sender, EventArgs e)
        {
            RenderComponent rc = sender as RenderComponent;
            SceneGraph.InsertNode(rc.SceneGraph, rc.SceneGraphCategory);
        }

        private void EnvironmentMapAddedHandler(object sender, EventArgs e)
        {
            SkyboxRenderComponent skyRC = sender as SkyboxRenderComponent;
            Resources.EnvironmentMap = skyRC.SkyTexture;
        }

        private void ShadowCasterCreatedHandler(object sender, EventArgs e)
        {
            ShadowCasterComponent caster = sender as ShadowCasterComponent;
            Resources.ShadowCasterActorId = caster.Owner.Id;
        }

        private void FogCreatedHandler(object sender, EventArgs e)
        {
            FogComponent fog = sender as FogComponent;
            Resources.FogActorId = fog.Owner.Id;
        }

        private void HDRLightCreatedHandler(object sender, EventArgs e)
        {
            HDRLightComponent hdr = sender as HDRLightComponent;
            Resources.HDRLightActorId = hdr.Owner.Id;
        }

        public void Update(float elapsedTimeSeconds)
        {
            // Allow scene nodes to update themselves.
        }

        // Some clients of this class may want to customize the render pass.  This is just a basic one...
        public void Draw(ICamera camera)
        {
            Actor castingActor = GameResources.ActorManager.GetActorById(Resources.ShadowCasterActorId);
            if (castingActor != null)
                DrawShadowMap(camera, castingActor);

            SharedResources.Game.GraphicsDevice.SetRenderTarget(null);
            SharedResources.Game.GraphicsDevice.Clear(Color.CornflowerBlue);

            SceneGraph.ResetTraversal();
            SceneGraph.VisibilityFrustum = camera.Frustum;
            SceneGraph.EyePosition = camera.Transform.Translation;

            SceneGraph.Draw();
        }

        private void DrawShadowMap(ICamera camera, Actor castingActor)
        {
            TransformComponent shadowCasterTransform = castingActor.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            Matrix casterView = Matrix.Invert(shadowCasterTransform.Transform);

            // Find the front half of the frustum corners in world space.
            Matrix invCamFrustum = Matrix.Invert(camera.Frustum.Matrix);
            Vector3[] halfCamCorners = new Vector3[] {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, 1.0f, 0.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(-1.0f, 1.0f, 1.0f),
            };

            for (int c = 0; c < 8; ++c)
            {
                Vector4 transformedCorner = Vector4.Transform(halfCamCorners[c], invCamFrustum);
                transformedCorner /= transformedCorner.W;
                halfCamCorners[c].X = transformedCorner.X;
                halfCamCorners[c].Y = transformedCorner.Y;
                halfCamCorners[c].Z = transformedCorner.Z;
            }

            for (int c = 0; c < 4; ++c)
            {
                halfCamCorners[c + 4] = halfCamCorners[c] + 0.05f * (halfCamCorners[c + 4] - halfCamCorners[c]);
            }

            Vector3[] camCorners = camera.Frustum.GetCorners();

            // Transform those corners into to caster space, and form a bounding box around them, which will become our caster projection.
            Vector3 shadowMapFrustumMinHalfCorner = Vector3.Transform(halfCamCorners[0], casterView);
            Vector3 shadowMapFrustumMaxHalfCorner = shadowMapFrustumMinHalfCorner;
            Vector3 shadowMapFrustumMinCorner = Vector3.Transform(camCorners[0], casterView);
            Vector3 shadowMapFrustumMaxCorner = shadowMapFrustumMinHalfCorner;
            for (int fc = 1; fc < 8; ++fc)
            {
                Vector3 currTranslatedCorner = Vector3.Transform(halfCamCorners[fc], casterView);
                shadowMapFrustumMinHalfCorner = Vector3.Min(currTranslatedCorner, shadowMapFrustumMinHalfCorner);
                shadowMapFrustumMaxHalfCorner = Vector3.Max(currTranslatedCorner, shadowMapFrustumMaxHalfCorner);
                currTranslatedCorner = Vector3.Transform(camCorners[fc], casterView);
                shadowMapFrustumMinCorner = Vector3.Min(currTranslatedCorner, shadowMapFrustumMinCorner);
                shadowMapFrustumMaxCorner = Vector3.Max(currTranslatedCorner, shadowMapFrustumMaxCorner);
            }

            Matrix shadowMapProj = SpaceUtils.CreateOrthographicOffCenter(
                shadowMapFrustumMinHalfCorner.X,
                shadowMapFrustumMaxHalfCorner.X,
                shadowMapFrustumMinHalfCorner.Y,
                shadowMapFrustumMaxHalfCorner.Y,
                shadowMapFrustumMaxHalfCorner.Z + 1000.0f,  // Extra room to include shadow casting objects.
                shadowMapFrustumMinHalfCorner.Z);

            SceneGraph.ResetTraversal();
            SceneGraph.VisibilityFrustum = new BoundingFrustum(casterView * shadowMapProj);

            Resources.ShadowTransform = SceneGraph.VisibilityFrustum.Matrix * sShadowTextureShift;
            SceneGraph.ExternalMaterialFlags = TraversalContext.MaterialFlags.ShadowMap;

            SharedResources.Game.GraphicsDevice.SetRenderTarget(Resources.ShadowMap);
            SharedResources.Game.GraphicsDevice.Clear(Color.White);

            SceneGraph.Draw();
        }
    }
}
