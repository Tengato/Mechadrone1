using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class Rotation : Behavior
    {
        private double mAngularVelocity;
        private TransformComponent mTransform;

        public Rotation(Actor owner)
            : base(owner)
        {
            mAngularVelocity = 1.0d;
            mTransform = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Owner.ComponentsCreated += ComponentsCreatedHandler;
            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            mTransform = Owner.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
            if (mTransform == null)
                throw new LevelManifestException("Expected ActorComponent missing.");
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (float)(mAngularVelocity * e.GameTime.ElapsedGameTime.TotalSeconds));
            mTransform.Orientation = rotation * mTransform.Orientation;
            mTransform.Orientation.Normalize();
        }

        public override void Release()
        {
        }
    }
}
