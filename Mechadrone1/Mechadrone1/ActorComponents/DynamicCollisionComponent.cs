using System;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using SlagformCommon;

namespace Mechadrone1
{
    // A simulation object that has a position and orientation. Despite the name, it *may or may not* be dynamic, by the definition of the simulation.
    abstract class DynamicCollisionComponent : CollisionComponent
    {
        protected abstract Vector3 mPosition { get; }   // The position of the entity's model space origin, in world coords.
        protected abstract Quaternion mOrientation { get; } // The world space rotation of the entity about its model space origin.
        public abstract Entity Entity { get; }
        protected Vector3 mTransformOffset { get; set; }  // The vector from the model space origin to the center of the collision sphere.

        public DynamicCollisionComponent(Actor owner)
            : base(owner)
        {
            mTransformOffset = Vector3.Zero;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);

            if (manifest.Properties.ContainsKey(ManifestKeys.NO_SOLVER) && (bool)(manifest.Properties[ManifestKeys.NO_SOLVER]))   // Setting this to true will allow the object to pass through others.
                Entity.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

            if (manifest.Properties.ContainsKey(ManifestKeys.TRANSFORM_OFFSET))
                mTransformOffset = (Vector3)(manifest.Properties[ManifestKeys.TRANSFORM_OFFSET]);

            if (manifest.Properties.ContainsKey(ManifestKeys.BOUNCINESS))
                Entity.Material.Bounciness = (float)(manifest.Properties[ManifestKeys.BOUNCINESS]);

            if (manifest.Properties.ContainsKey(ManifestKeys.STATIC_FRICTION))
                Entity.Material.StaticFriction = (float)(manifest.Properties[ManifestKeys.STATIC_FRICTION]);

            if (manifest.Properties.ContainsKey(ManifestKeys.KINETIC_FRICTION))
                Entity.Material.KineticFriction = (float)(manifest.Properties[ManifestKeys.KINETIC_FRICTION]);

            if (manifest.Properties.ContainsKey(ManifestKeys.LOCK_ROTATION) && (bool)(manifest.Properties[ManifestKeys.LOCK_ROTATION]))   // Setting this to true will prevent the object from rotating
                Entity.LocalInertiaTensorInverse = new BEPUutilities.Matrix3x3(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
        }

        protected override void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            base.ComponentsCreatedHandler(sender, e);

            Entity.Position = BepuConverter.Convert(mTransformComponent.Translation
                + Vector3.Transform(mTransformOffset, mTransformComponent.Orientation));
            Entity.Orientation = BepuConverter.Convert(mTransformComponent.Orientation);

            mTransformComponent.TransformChanged += TransformChangedHandler;
        }

        protected override void ActorInitializedHandler(object sender, EventArgs e)
        {
            base.ActorInitializedHandler(sender, e);
            if (IsDynamic)
                GameResources.ActorManager.PostPhysicsUpdateStep += PostPhysicsUpdateHandler;

            Entity.Tag = Owner.Id;
        }

        protected virtual void PostPhysicsUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            mTransformComponent.PhysicsUpdate(mPosition, mOrientation);
        }

        public override void Release()
        {
            base.Release();
            GameResources.ActorManager.PostPhysicsUpdateStep -= PostPhysicsUpdateHandler;
        }

        private void TransformChangedHandler(object sender, EventArgs e)
        {
            Entity.Position = BepuConverter.Convert(mTransformComponent.Translation
                + Vector3.Transform(mTransformOffset, mTransformComponent.Orientation));
            Entity.Orientation = BepuConverter.Convert(mTransformComponent.Orientation);
        }
    }
}
