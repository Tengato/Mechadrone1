using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SlagformCommon;

namespace Mechadrone1
{
    class BoxCollisionComponent : DynamicCollisionComponent
    {
        private Box mSimBox { get; set; }

        public override ISpaceObject SimObject { get { return mSimBox; } }
        public override Entity Entity { get { return mSimBox; } }
        protected override BEPUphysics.BroadPhaseEntries.BroadPhaseEntry BroadPhaseEntry { get { return mSimBox.CollisionInformation; } }
        public override bool IsDynamic { get { return mSimBox.IsDynamic; } }

        // The position of origin in the box's 'model' space.
        protected override Vector3 mPosition
        {
            get
            {
                return BepuConverter.Convert(mSimBox.Position) - Vector3.Transform(mTransformOffset, BepuConverter.Convert(mSimBox.Orientation));
            }

            //set
            //{
            //    mSimBox.Position = BepuConverter.Convert(value + Vector3.Transform(Vector3.Up, mTransformComponent.Orientation) * mSimBox.HalfHeight);
            //}
        }

        protected override Quaternion mOrientation
        {
            get { return BepuConverter.Convert(mSimBox.Orientation); }
        }

        public BoxCollisionComponent(Actor owner)
            : base(owner)
        {
            mSimBox = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            float width = (float)(manifest.Properties[ManifestKeys.WIDTH]);
            float height = (float)(manifest.Properties[ManifestKeys.HEIGHT]);
            float length = (float)(manifest.Properties[ManifestKeys.LENGTH]);

            if (manifest.Properties.ContainsKey(ManifestKeys.MASS))
            {
                float mass = (float)(manifest.Properties[ManifestKeys.MASS]);
                mSimBox = new Box(BEPUutilities.Vector3.Zero, width, height, length, mass);
            }
            else
            {
                mSimBox = new Box(BEPUutilities.Vector3.Zero, width, height, length);
            }

            base.Initialize(contentLoader, manifest);
        }
    }
}
