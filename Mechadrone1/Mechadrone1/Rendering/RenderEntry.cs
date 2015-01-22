using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Mechadrone1
{
    // Saves information about a geometry object that needs to be rendered.
    class RenderEntry
    {
        public GeometryNode DrawNode { get; set; }
        public float SceneDepth { get; set; }   // Only used to sort translucent objects.
        public EffectApplication Material { get; set; }
        public Matrix Transform { get; set; }

        public RenderEntry(GeometryNode drawNode, EffectApplication material)
        {
            DrawNode = drawNode;
            SceneDepth = 0.0f;
            Material = material;
            Transform = Matrix.Identity;
        }
    }

    class RenderEntryComparer : Comparer<RenderEntry>
    {
        public override int Compare(RenderEntry x, RenderEntry y)
        {
            // If x does not use alpha, but y does, return a negative value (x is sorted to precede y)
            int alphaCompare = (x.Material.UseAlphaPass ? 1 : 0) - (y.Material.UseAlphaPass ? 1 : 0);
            if (alphaCompare != 0)
                return alphaCompare;
            if (x.Material.UseAlphaPass && y.Material.UseAlphaPass)
                return Math.Sign(x.SceneDepth - y.SceneDepth);
            if (x.Material.RenderState != y.Material.RenderState)
                return x.Material.RenderState - y.Material.RenderState;
            if (x.Material.Effect != y.Material.Effect)
                return x.Material.Effect.GetHashCode() - y.Material.Effect.GetHashCode();
            if (x.DrawNode.VertexBuffer != y.DrawNode.VertexBuffer)
                return x.DrawNode.VertexBuffer.GetHashCode() - y.DrawNode.VertexBuffer.GetHashCode();
            if (x.DrawNode.IndexBuffer != y.DrawNode.IndexBuffer)
                return x.DrawNode.IndexBuffer.GetHashCode() - y.DrawNode.IndexBuffer.GetHashCode();

            return 0;
        }
    }
}
