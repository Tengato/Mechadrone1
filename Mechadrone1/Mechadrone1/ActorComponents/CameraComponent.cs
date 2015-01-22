using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class CameraComponent : ActorComponent
    {
        public static event EventHandler CameraUpdated;

        private StaticCamera mCamera;

        public ICamera Camera { get { return mCamera; } }
        public override ActorComponent.ComponentType Category { get { return ComponentType.Camera; } }

        public CameraComponent(Actor owner)
            : base(owner)
        {
            mCamera = new StaticCamera();
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Owner.ActorInitialized += ActorInitializedHandler;
        }

        public void SetActive()
        {
            OnCameraUpdated(EventArgs.Empty);
        }

        protected virtual void ActorInitializedHandler(object sender, EventArgs e)
        {
            TransformComponent tc = Owner.GetComponent<TransformComponent>(ComponentType.Transform);
            if (tc == null)
                throw new LevelManifestException("CameraComponents expect to be accompanied by TransformComponents.");

            mCamera.Transform = tc.Transform;
            tc.TransformChanged += TransformChangedHandler;
            SetActive();
        }

        private void TransformChangedHandler(object sender, EventArgs e)
        {
            mCamera.Transform = ((TransformComponent)sender).Transform;
        }

        public override void Release()
        {
        }

        private void OnCameraUpdated(EventArgs e)
        {
            EventHandler handler = CameraUpdated;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
