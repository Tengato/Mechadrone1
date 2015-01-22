using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class OverheadView : GameUIElement
    {
        private StaticOrthoCamera mMapCamera;
        public float CameraHeight;
        private int mActorId;

        public OverheadView(int actorId)
        {
            mMapCamera = new StaticOrthoCamera();
            mMapCamera.Transform = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward);
            CameraHeight = 100.0f;
            mActorId = actorId;
        }

        public override void Draw(UIElementsWindow drawSegment, GameTime gameTime)
        {
            Matrix cameraTransform = mMapCamera.Transform;
            Actor actor = GameResources.ActorManager.GetActorById(mActorId);
            cameraTransform.Translation = actor.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform).Translation + Vector3.Up * CameraHeight;
            mMapCamera.Transform = cameraTransform;
            drawSegment.Scene.SceneGraph.ResetTraversal();
            drawSegment.Scene.SceneGraph.ExternalMaterialFlags = TraversalContext.MaterialFlags.Simplified;
            drawSegment.Scene.SceneGraph.VisibilityFrustum = mMapCamera.Frustum;
            drawSegment.Scene.SceneGraph.EyePosition = mMapCamera.Transform.Translation;
            drawSegment.Scene.SceneGraph.Draw();
        }
    }
}
