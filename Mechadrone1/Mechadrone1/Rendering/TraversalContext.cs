using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    // Propagates information across a scene graph traversal.
    class TraversalContext
    {
        // Specifying one of these values allows a GeometryNode to choose the most appropriate material from those available.
        [Flags]
        public enum MaterialFlags
        {
            // Lower numbers have higher precedence (excluding None, which has least precedence).
            // See GeometryNode Process method.
            None = 0x00,
            ShadowMap = 0x01,
            // Examples of other possible flags:
            Translucent = 0x02,
            Infrared = 0x04,
            Highlighted = 0x08,
            Simplified = 0x16, // Used for overhead map.
        }

        public MatrixStack Transform { get; private set; }
        public MaterialFlags ExternalMaterialFlags { get; set; }
        public BoundingFrustum VisibilityFrustum { get; set; }
        public SceneNode AcceptAllGrantor { get; set; }
        public TranslucencyNode TranslucencyGrantor { get; set; }
        private List<RenderEntry> mRenderEntries;
        private RenderEntryComparer mEntryComparer;

        public TraversalContext()
        {
            Transform = new MatrixStack();
            ExternalMaterialFlags = MaterialFlags.None;
            VisibilityFrustum = null;
            AcceptAllGrantor = null;
            TranslucencyGrantor = null;
            mRenderEntries = new List<RenderEntry>();
            mEntryComparer = new RenderEntryComparer();
            SharedResources.Game.GraphicsDevice.DeviceResetting += GraphicsDeviceResettingHandler;
            SharedResources.Game.GraphicsDevice.DeviceReset += GraphicsDeviceResetHandler;
        }

        public void Reset()
        {
            Transform.Clear();
            ExternalMaterialFlags = MaterialFlags.None;
            VisibilityFrustum = null;
            AcceptAllGrantor = null;
            TranslucencyGrantor = null;
            mRenderEntries.Clear();
        }

        public void AcceptForRender(GeometryNode drawNode, EffectApplication material)
        {
            RenderEntry newEntry = new RenderEntry(drawNode, material);
            if (material.UseAlphaPass)
            {
                Vector3 homogeneousClipSpacePos = Vector3.Transform(Transform.Top.Translation, VisibilityFrustum.Matrix);
                newEntry.SceneDepth = -homogeneousClipSpacePos.Z; // Negate because we want to sort far-to-near.
            }
            newEntry.Transform = Transform.Top;

            mRenderEntries.Add(newEntry);
        }

        public void ExecuteRenderQueue(RenderContext renderContext)
        {
            if (mRenderEntries.Count == 0)
                return;

            mRenderEntries.Sort(mEntryComparer);

            // Prime the previousEntry variable:
            RenderEntry previousEntry = mRenderEntries[0];
            GraphicsDevice gd = SharedResources.Game.GraphicsDevice;

            gd.BlendState = previousEntry.Material.BlendState;
            gd.DepthStencilState = previousEntry.Material.DepthStencilState;
            gd.RasterizerState = previousEntry.Material.RasterizerState;
            gd.SetVertexBuffer(previousEntry.DrawNode.VertexBuffer);
            gd.Indices = previousEntry.DrawNode.IndexBuffer;

            for (int i = 0; i < mRenderEntries.Count; ++i)
            {
                if (previousEntry.Material.BlendState != mRenderEntries[i].Material.BlendState)
                    gd.BlendState = mRenderEntries[i].Material.BlendState;

                if (previousEntry.Material.DepthStencilState != mRenderEntries[i].Material.DepthStencilState)
                    gd.DepthStencilState = mRenderEntries[i].Material.DepthStencilState;

                if (previousEntry.Material.RasterizerState != mRenderEntries[i].Material.RasterizerState)
                    gd.RasterizerState = mRenderEntries[i].Material.RasterizerState;

                if (previousEntry.DrawNode.VertexBuffer != mRenderEntries[i].DrawNode.VertexBuffer)
                    gd.SetVertexBuffer(mRenderEntries[i].DrawNode.VertexBuffer);

                if (previousEntry.DrawNode.IndexBuffer != mRenderEntries[i].DrawNode.IndexBuffer)
                    gd.Indices = mRenderEntries[i].DrawNode.IndexBuffer;

                mRenderEntries[i].DrawNode.Draw(renderContext, mRenderEntries[i].Material, mRenderEntries[i].Transform);
                previousEntry = mRenderEntries[i];
            }

            mRenderEntries.Clear();
            SharedResources.GraphicsDeviceReady = !SharedResources.GraphicsDeviceResetting;
        }

        private void GraphicsDeviceResettingHandler(object sender, EventArgs e)
        {
            SharedResources.GraphicsDeviceReady = false;
            SharedResources.GraphicsDeviceResetting = true;
        }

        private void GraphicsDeviceResetHandler(object sender, EventArgs e)
        {
            SharedResources.GraphicsDeviceResetting = false;
        }
    }
}
