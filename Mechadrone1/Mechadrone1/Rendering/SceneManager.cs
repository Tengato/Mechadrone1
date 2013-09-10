using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Skelemator;
using Mechadrone1.Gameplay;
using Manifracture;
using System;
using System.Linq;
using BEPUphysics;


namespace Mechadrone1.Rendering
{

    /// <summary>
    /// Organizes all objects and processes that are necessary for rendering a 3D scene.
    /// It shouldn't know details about the game, it just wants to know what to draw.
    /// </summary>
    class SceneManager
    {
        GraphicsDevice gd;
        IRenderableScene sceneModel;
        RenderQueue renderQueue;

        Matrix projection;
        BoundingFrustum cameraFrustum;

        // Shadow map stuff
        public const int SMAP_SIZE = 2048;
        public static readonly Matrix NDC_TO_TEXCOORDS = new Matrix(0.5f, 0.0f, 0.0f, 0.0f,
                                                                    0.0f, -0.5f, 0.0f, 0.0f,
                                                                    0.0f, 0.0f, 1.0f, 0.0f,
                                                                    0.5f, 0.5f, 0.0f, 1.0f);

        BoundingSphere shadowedSurface;
        Matrix lightView;
        Matrix lightProjection;
        RenderTarget2D smapRenderTarget;

        public bool GridEnabled { get; set; }


        public SceneManager(GraphicsDevice graphicsDevice)
        {
            gd = graphicsDevice;
        }


        public void LoadContent(IRenderableScene scene, ContentManager content)
        {
            sceneModel = scene;
            renderQueue = new RenderQueue();

            cameraFrustum = new BoundingFrustum(Matrix.Identity);

            EffectRegistry.DepthOnlySkinFx = content.Load<Effect>("shaders\\DepthOnlySkin");
            EffectRegistry.DepthOnlyFx = content.Load<Effect>("shaders\\DepthOnly");

            smapRenderTarget = new RenderTarget2D(gd,
                                                  SMAP_SIZE,
                                                  SMAP_SIZE,
                                                  false,
                                                  SurfaceFormat.Single,
                                                  DepthFormat.Depth24);

            EffectRegistry.SetFog(sceneModel.Fog);
        }


        /// <summary>
        /// Draw the 3D game scene
        /// </summary>
        public void DrawScene(PlayerIndex player)
        {
            ICamera camera = sceneModel.GetCamera(player);
            projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView, gd.Viewport.AspectRatio, 0.1f, 1000.0f);

            cameraFrustum.Matrix = camera.View * projection;
            BoundingBox cameraRect = BoundingBox.CreateFromPoints(cameraFrustum.GetCorners());

            // TODO: Put stuff into separate threads
            List<ISceneObject> visibleObjectsCamera = sceneModel.QuadTree.Search(cameraRect, cameraFrustum);

            // Build the shadow map. The main light will cast the shadow.
            // First, find the space to put into the shadow map:
            RayCastResult rcr;
            Ray cameraRay;

            cameraRay.Direction = Vector3.Down;
            cameraRay.Position = camera.Transform.Translation;

            if (sceneModel.SimSpace.RayCast(cameraRay, 90.0f, out rcr))
                shadowedSurface.Center = rcr.HitData.Location;
            else
                shadowedSurface.Center = cameraRay.Position + Vector3.Normalize(cameraRay.Direction) * 90.0f;

            shadowedSurface.Center += new Vector3(camera.Transform.Forward.X, 0.0f, camera.Transform.Forward.Z) * 50.0f;
            shadowedSurface.Radius = 50.0f;

            float texelsPerWorldUnit = (float)SMAP_SIZE / (shadowedSurface.Radius * 2.0f);

            Matrix lightViewAtOrigin = Matrix.CreateScale(texelsPerWorldUnit) *
                Matrix.CreateLookAt(Vector3.Zero, -sceneModel.ShadowCastingLight.Direction, Vector3.Up);
            Matrix lightViewAtOriginInv = Matrix.Invert(lightViewAtOrigin);

            shadowedSurface.Center = Vector3.Transform(shadowedSurface.Center, lightViewAtOrigin);

            shadowedSurface.Center.X = (float)(Math.Floor(shadowedSurface.Center.X));
            shadowedSurface.Center.Y = (float)(Math.Floor(shadowedSurface.Center.Y));

            shadowedSurface.Center = Vector3.Transform(shadowedSurface.Center, lightViewAtOriginInv);

            lightView = Matrix.CreateLookAt(shadowedSurface.Center - sceneModel.ShadowCastingLight.Direction * shadowedSurface.Radius * 2.0f, shadowedSurface.Center, Vector3.Up);
            lightProjection = Matrix.CreateOrthographic(shadowedSurface.Radius * 2.0f,
                                                     shadowedSurface.Radius * 2.0f,
                                                     0.0f,
                                                     shadowedSurface.Radius * 6.0f);

            BoundingFrustum lightFrustum = new BoundingFrustum(lightView * lightProjection);
            BoundingBox lightRect = BoundingBox.CreateFromPoints(lightFrustum.GetCorners());

            List<ISceneObject> visibleObjectsLight = sceneModel.QuadTree.Search(lightRect, lightFrustum);

            gd.SetRenderTarget(smapRenderTarget);
            gd.Clear(Color.White);

            // Draw objects that cast shadows:
            foreach (ISceneObject sObj in visibleObjectsLight.FindAll(obj => obj.CastsShadow == true))
            {
                renderQueue.AddSceneObject(sObj,
                    RenderStep.Shadows,
                    camera.View,
                    projection,
                    camera.Transform,
                    lightView,
                    lightProjection,
                    null,
                    null);
            }

            renderQueue.Execute();

            // TODO: Add optimizations like sort by material, z-prepass etc
            // Change state to draw camera view and draw objects:
            gd.SetRenderTarget(null);
            gd.Clear(Color.CornflowerBlue);

            // Draw objects:
            foreach (ISceneObject sObj in visibleObjectsCamera)
            {
                renderQueue.AddSceneObject(
                    sObj,
                    RenderStep.Default,
                    camera.View,
                    projection,
                    camera.Transform,
                    lightView,
                    lightProjection,
                    smapRenderTarget,
                    sceneModel.GetObjectLights(sObj.Position, camera.Transform.Translation));
            }

            renderQueue.Execute();
        }
    }


    public enum DrawMode
    {
        Alpha = 0,
        Additive = 1,
        AlphaAndGlow = 2,
        AdditiveAndGlow = 3,
    }

}
