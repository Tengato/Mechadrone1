using System;
using BEPUphysics;
using BEPUphysicsDemos.AlternateMovement.Character;
using Microsoft.Xna.Framework;
using SlagformCommon;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;

namespace Mechadrone1
{
    class BipedCollisionComponent : DynamicCollisionComponent
    {
        // We have to share this object with the controller component.
        private CharacterController mSimController;
        private float mHeight;
        private float mRadius;
        private float mMass;

        public override ISpaceObject SimObject { get { return mSimController; } }
        public override Entity Entity { get { return mSimController.Body; } }
        protected override BroadPhaseEntry BroadPhaseEntry { get { return mSimController.Body.CollisionInformation; } }
        public override bool IsDynamic { get { return true; } }

        protected override Vector3 mPosition
        {
            get
            {
                return BepuConverter.Convert(mSimController.Body.Position) + BepuConverter.Convert(mSimController.Down) * 0.5f *
                    (mSimController.StanceManager.CurrentStance == Stance.Crouching ?
                    mSimController.StanceManager.CrouchingHeight :
                    mSimController.StanceManager.StandingHeight);
            }
        }

        protected override Quaternion mOrientation
        {
            get
            {
                return SpaceUtils.GetOrientation(BepuConverter.Convert(mSimController.ViewDirection), -BepuConverter.Convert(mSimController.Down));
            }
        }

        public BipedCollisionComponent(Actor owner)
            : base(owner)
        {
            mSimController = null;
            mHeight = 9.3f;
            mRadius = 1.7f;
            mMass = 9.0f;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);

            if (manifest.Properties.ContainsKey(ManifestKeys.HEIGHT))
                mHeight = (float)(manifest.Properties[ManifestKeys.HEIGHT]);

            if (manifest.Properties.ContainsKey(ManifestKeys.RADIUS))
                mRadius = (float)(manifest.Properties[ManifestKeys.RADIUS]);

            if (manifest.Properties.ContainsKey(ManifestKeys.MASS))
                mMass = (float)(manifest.Properties[ManifestKeys.MASS]);
        }

        protected override void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            BipedControllerComponent bipedController = Owner.GetComponent<BipedControllerComponent>(ComponentType.Control);
            if (bipedController == null)
                throw new LevelManifestException("BipedCollisionComponents expect to be accompanied by BipedControllerComponents.");

            mSimController = bipedController.Controller;

            mSimController.StanceManager.StandingHeight = mHeight;
            mSimController.StanceManager.CrouchingHeight = mHeight * 0.5f;
            mSimController.BodyRadius = mRadius;
            mSimController.Body.Mass = mMass;

            base.ComponentsCreatedHandler(sender, e);

            mSimController.ViewDirection = BepuConverter.Convert(Vector3.Transform(Vector3.Forward, mTransformComponent.Orientation));
        }

        public override void Release()
        {
            mSimController = null;
            base.Release();
        }
    }
}
