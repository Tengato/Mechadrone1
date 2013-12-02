using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Gameplay;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Manifracture;

namespace Mechadrone1.Rendering
{
    class RenderQueue
    {
        private List<RenderEntry> entries;
        private int batchId;
        private RenderEntryComparer reComparer;

        public RenderQueue()
        {
            entries = new List<RenderEntry>();
            batchId = 0;
            reComparer = new RenderEntryComparer();
        }

        public void AddSceneObject(
            ISceneObject sceneObj,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightProjection,
            RenderTarget2D shadowMap,
            List<DirectLight> lights)
        {
            // prep the sceneObj's parameters.
            entries.AddRange(sceneObj.GetRenderEntries(
                batchId,
                step,
                view,
                projection,
                cameraTransform,
                shadowCastingLightView,
                shadowCastingLightProjection,
                shadowMap,
                lights));
        }

        public void Execute()
        {
            if (entries.Count == 0)
                return;

            entries.Sort(reComparer);

            RenderEntry previousEntry = entries[0];

            GraphicsDevice gd = previousEntry.Effect.GraphicsDevice;

            gd.BlendState = previousEntry.BlendState;
            gd.DepthStencilState = previousEntry.DepthStencilState;
            gd.RasterizerState = previousEntry.RasterizerState;
            gd.SetVertexBuffer(previousEntry.VertexBuffer);
            gd.Indices = previousEntry.IndexBuffer;

            for (int i = 0; i < entries.Count; i++)
            {
                if (previousEntry.BlendState != entries[i].BlendState)
                    gd.BlendState = entries[i].BlendState;

                if (previousEntry.DepthStencilState != entries[i].DepthStencilState)
                    gd.DepthStencilState = entries[i].DepthStencilState;

                if (previousEntry.RasterizerState != entries[i].RasterizerState)
                    gd.RasterizerState = entries[i].RasterizerState;

                if (previousEntry.VertexBuffer != entries[i].VertexBuffer)
                    gd.SetVertexBuffer(entries[i].VertexBuffer);

                if (previousEntry.IndexBuffer != entries[i].IndexBuffer)
                    gd.Indices = entries[i].IndexBuffer;

                entries[i].Draw();
                previousEntry = entries[i];
            }

            entries.Clear();
            batchId++;
        }
    }


    public enum RenderStep
    {
        Default,
        Shadows,
    }
}
