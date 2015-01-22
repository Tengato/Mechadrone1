using System;
using BEPUphysics;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using BEPUphysics.BroadPhaseEntries;

namespace Mechadrone1
{
    abstract class CollisionComponent : ActorComponent
    {
        public static event EventHandler CollisionComponentInitialized;
        public abstract ISpaceObject SimObject { get; }
        public abstract bool IsDynamic { get; }
        private bool mClipsCamera;

        protected TransformComponent mTransformComponent;

        public override ComponentType Category { get { return ComponentType.Physics; } }

        // TODO: P3: This is only used to flag camera clipping objects. Solve the problem with collision groups instead.
        protected abstract BroadPhaseEntry BroadPhaseEntry { get; }

        public CollisionComponent(Actor owner)
            : base(owner)
        {
            mTransformComponent = null;
            Owner.ComponentsCreated += ComponentsCreatedHandler;
            Owner.ActorInitialized += ActorInitializedHandler;
            mClipsCamera = false;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.CLIPS_CAMERA))
                mClipsCamera = (bool)(manifest.Properties[ManifestKeys.CLIPS_CAMERA]);
        }

        protected virtual void OnCollisionComponentInitialized(EventArgs e)
        {
            EventHandler handler = CollisionComponentInitialized;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            mTransformComponent = Owner.GetComponent<TransformComponent>(ComponentType.Transform);
            if (mTransformComponent == null)
                throw new LevelManifestException("CollisionComponents expect to be accompanied by TransformComponents.");
        }

        // If you override this, make sure this one gets called _after_ the derived method does its stuff.
        protected virtual void ActorInitializedHandler(object sender, EventArgs e)
        {
            if (mClipsCamera)
                GameResources.ActorManager.CameraClippingSimObjects.Add(BroadPhaseEntry.GetHashCode());
            OnCollisionComponentInitialized(EventArgs.Empty);
        }

        public override void Release()
        {
            mTransformComponent = null;
        }
    }
}
