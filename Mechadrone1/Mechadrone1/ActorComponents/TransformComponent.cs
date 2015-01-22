using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class TransformComponent : ActorComponent
    {
        public event EventHandler TransformChanged;
        protected float mScale;
        protected Quaternion mOrientation;
        protected Vector3 mTranslation;

        public float Scale
        {
            get { return mScale; }
            set
            {
                mScale = value;
                OnTransformChanged(EventArgs.Empty);
            }
        }

        public Quaternion Orientation
        {
            get { return mOrientation; }
            set
            {
                mOrientation = value;
                OnTransformChanged(EventArgs.Empty);
            }
        }

        public Vector3 Translation
        {
            get { return mTranslation; }
            set
            {
                mTranslation = value;
                OnTransformChanged(EventArgs.Empty);
            }
        }

        public override ComponentType Category { get { return ComponentType.Transform; } }

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Translation);
            }

            set
            {
                Vector3 scale;
                value.Decompose(out scale, out mOrientation, out mTranslation);
                mScale = scale.X;
                OnTransformChanged(EventArgs.Empty);
            }
        }

        public TransformComponent(Actor owner)
            : base(owner)
        {
            mScale = 1.0f;
            mOrientation = Quaternion.Identity;
            mTranslation = Vector3.Zero;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.SCALE))
                mScale = (float)(manifest.Properties[ManifestKeys.SCALE]);

            if (manifest.Properties.ContainsKey(ManifestKeys.ORIENTATION))
                mOrientation = (Quaternion)(manifest.Properties[ManifestKeys.ORIENTATION]);

            if (manifest.Properties.ContainsKey(ManifestKeys.POSITION))
                mTranslation = (Vector3)(manifest.Properties[ManifestKeys.POSITION]);

            if (manifest.Properties.ContainsKey(ManifestKeys.LOOKAT))
                mOrientation = Quaternion.CreateFromRotationMatrix(
                    Matrix.Transpose(Matrix.CreateLookAt(Vector3.Zero, (Vector3)(manifest.Properties[ManifestKeys.LOOKAT]) - mTranslation, Vector3.Up)));
        }

        protected virtual void OnTransformChanged(EventArgs e)
        {
            EventHandler handler = TransformChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        // The physics simulation will need to update these values without triggering a TransformChanged event, which should be used by the physics system only
        public void PhysicsUpdate(Vector3 position, Quaternion orientation)
        {
            mTranslation = position;
            mOrientation = orientation;
        }

        public override void Release()
        {
        }

    }
}
