using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SlagformCommon;

namespace Mechadrone1
{
    class SphereCollisionComponent : DynamicCollisionComponent
    {
        private Sphere mSimSphere { get; set; }

        public override ISpaceObject SimObject { get { return mSimSphere; } }
        public override Entity Entity { get { return mSimSphere; } }
        protected override BEPUphysics.BroadPhaseEntries.BroadPhaseEntry BroadPhaseEntry { get { return mSimSphere.CollisionInformation; } }
        public override bool IsDynamic { get { return mSimSphere.IsDynamic; } }

        protected override Vector3 mPosition
        {
            get
            {
                return BepuConverter.Convert(mSimSphere.Position) - Vector3.Transform(mTransformOffset, BepuConverter.Convert(mSimSphere.Orientation));
            }

            //set
            //{
            //    mSimBox.Position = BepuConverter.Convert(value + Vector3.Transform(Vector3.Up, mTransformComponent.Orientation) * mSimBox.HalfHeight);
            //}
        }

        protected override Quaternion mOrientation
        {
            get { return BepuConverter.Convert(mSimSphere.Orientation); }
        }

        public SphereCollisionComponent(Actor owner)
            : base(owner)
        {
            mSimSphere = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            float radius = (float)(manifest.Properties[ManifestKeys.RADIUS]);

            if (manifest.Properties.ContainsKey(ManifestKeys.MASS))
            {
                float mass = (float)(manifest.Properties[ManifestKeys.MASS]);
                mSimSphere = new Sphere(BEPUutilities.Vector3.Zero, radius, mass);
            }
            else
            {
                mSimSphere = new Sphere(BEPUutilities.Vector3.Zero, radius);
            }

            base.Initialize(contentLoader, manifest);
        }
    }
}
