using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    abstract class BVCullableRenderComponent : RenderComponent
    {
        private bool mIsStatic;

        public override SceneGraphRoot.PartitionCategories SceneGraphCategory
        {
            get { return mIsStatic ? SceneGraphRoot.PartitionCategories.Static : SceneGraphRoot.PartitionCategories.Dynamic; }
        }

        public BVCullableRenderComponent(Actor owner)
            : base(owner)
        {
            mIsStatic = false;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.IS_STATIC))
                mIsStatic = (bool)(manifest.Properties[ManifestKeys.IS_STATIC]);
            base.Initialize(contentLoader, manifest);
        }

        protected override void CreateSceneGraph(ComponentManifest manifest)
        {
            SceneGraph = new ActorTransformNode(Owner);

            SceneNode sphereParent = SceneGraph;

            Matrix modelAdjustment = Matrix.Identity;
            if (manifest.Properties.ContainsKey(ManifestKeys.MODEL_ADJUSTMENT))
                modelAdjustment = (Matrix)(manifest.Properties[ManifestKeys.MODEL_ADJUSTMENT]);

            if (modelAdjustment != Matrix.Identity)
            {
                sphereParent = new StaticTransformNode(modelAdjustment);
                SceneGraph.AddChild(sphereParent);
            }

            BuildSphereAndGeometryNodes(manifest, sphereParent);
        }

        protected abstract void BuildSphereAndGeometryNodes(ComponentManifest manifest, SceneNode parent);
    }
}
