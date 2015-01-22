using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    class SceneGraphRoot
    {
        public enum PartitionCategories
        {
            Static,
            Dynamic,
            Skybox,
        }

        private SceneNode mDynamicRoot;
        private ImplicitBoundingBoxNode mStaticTreeRoot;
        private SceneNode mSkyboxRoot;
        private TraversalContext mTraversalData;
        private RenderContext mRenderContext;

        public TraversalContext.MaterialFlags ExternalMaterialFlags
        {
            get { return mTraversalData.ExternalMaterialFlags; }
            set { mTraversalData.ExternalMaterialFlags = value; }
        }

        public BoundingFrustum VisibilityFrustum
        {
            get { return mTraversalData.VisibilityFrustum; }
            set
            {
                mTraversalData.VisibilityFrustum = value;
                mRenderContext.VisibilityFrustum = value;
            }
        }

        public Vector3 EyePosition
        {
            get { return mRenderContext.EyePosition; }
            set { mRenderContext.EyePosition = value; }
        }

        public SceneGraphRoot(SceneResources sceneResources)
        {
            mRenderContext = new RenderContext(sceneResources);
            mDynamicRoot = new SceneNode();
            mStaticTreeRoot = null;
            mSkyboxRoot = new SceneNode();
            mTraversalData = new TraversalContext();
        }

        public void ResetTraversal()
        {
            mTraversalData.Reset();
        }

        public void Draw()
        {
            mDynamicRoot.ProcessChildren(mTraversalData);

            if (mStaticTreeRoot != null)
            {
                mStaticTreeRoot.PreProcess(mTraversalData);
                mStaticTreeRoot.Process(mTraversalData);
                mStaticTreeRoot.ProcessChildren(mTraversalData);
                mStaticTreeRoot.PostProcess(mTraversalData);
            }

            mSkyboxRoot.ProcessChildren(mTraversalData);

            mTraversalData.ExecuteRenderQueue(mRenderContext);
        }

        public void InsertNode(SceneNode node, PartitionCategories category)
        {
            switch (category)
            {
                case PartitionCategories.Dynamic:
                    mDynamicRoot.AddChild(node);
                    break;
                case PartitionCategories.Static:
                    ImplicitBoundingBoxNode boxNode = node as ImplicitBoundingBoxNode;
                    if (boxNode == null)
                    {
                        boxNode = new ImplicitBoundingBoxNode(true);
                        boxNode.AddChild(node);
                        boxNode.RecalculateBound();
                    }

                    if (mStaticTreeRoot == null)
                    {
                        mStaticTreeRoot = boxNode;
                    }
                    else
                    {
                        mStaticTreeRoot.InsertNodeIntoBVH(boxNode);
                    }
                    break;
                case PartitionCategories.Skybox:
                    mSkyboxRoot.AddChild(node);
                    break;
            }
        }
    }
}
