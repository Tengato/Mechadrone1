using Microsoft.Xna.Framework;
using System;
using BEPUphysicsDemos.AlternateMovement.Character;
using SlagformCommon;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class FlyerControllerComponent : ActorComponent
    {
        private LevitationMovementController mController;
        private TransformComponent mTransformComponent;

        public override ActorComponent.ComponentType Category { get { return ComponentType.Control; } }

        public FlyerControllerComponent(Actor owner)
            : base(owner)
        {
            Owner.ComponentsCreated += ComponentsCreatedHandler;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);

            LevitationHandlingDesc levitationDesc = new LevitationHandlingDesc();
            levitationDesc.DampingForce = GameOptions.MovementForceDamping;
            levitationDesc.DampingRotationForce = GameOptions.MovementRotationForceDamping;
            levitationDesc.MaxRotationVelocity = GameOptions.MovementRotationVelocity;
            levitationDesc.MaxVelocity = GameOptions.MovementVelocity;

            mController = new LevitationMovementController(levitationDesc);
            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
        }

        // Vector3 parameter rotationForce's components represent the normalized (between -1 and 1) magnitude of the torque applied on each axis.
        public void SetRotationForce(Vector3 rotationForce)
        {
            mController.RotationForce = GameOptions.MovementRotationForce * rotationForce;
        }

        public void SetForce(Vector3 force)
        {
            if (force.LengthSquared() > 1.0f)
                force.Normalize();

            mController.Force = GameOptions.MovementForce * force;
        }

        public void SetDirectRotation(Quaternion rotation)
        {
            mController.Rotation = Matrix.CreateFromQuaternion(rotation) * mController.Rotation; 
        }

        private void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            mTransformComponent = Owner.GetComponent<TransformComponent>(ComponentType.Transform);
            if (mTransformComponent == null)
                throw new LevelManifestException("ControllerComponents expect to be accompanied by TransformComponents.");

            mController.Reset(mTransformComponent.Transform);
        }

        public void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            mController.Update((float)(e.GameTime.ElapsedGameTime.TotalSeconds));
            mTransformComponent.Transform = mController.Transform;
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }
}
