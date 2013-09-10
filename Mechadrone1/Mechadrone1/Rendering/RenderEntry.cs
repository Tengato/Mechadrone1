using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Skelemator;

namespace Mechadrone1.Rendering
{
    /// <summary>
    /// The RenderEntry class abstracts a single DrawPrimitives call.
    /// </summary>
    class RenderEntry
    {
        public delegate void DrawMethod(RenderEntry renderEntry);

        public DrawMethod DrawCallback { private get; set; }

        public ISceneObject SceneObject;
        public EffectPass Pass { get; set; }

        public BlendState BlendState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;

        public Effect Effect { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public int NumVertices { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int VertexOffset { get; set; }
        public int StartIndex { get; set; }
        public RenderOptions RenderOptions { get; set; }
        public int PrimitiveCount { get; set; }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Matrix CameraTransform { get; set; }
        public Matrix ShadowCastingLightView { get; set; }
        public Matrix ShadowCastingLightProjection { get; set; }
        public RenderTarget2D ShadowMap { get; set; }
        public List<Manifracture.DirectLight> Lights { get; set; }

        public RenderEntry()
        {
            // Set some default values:
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;
            RenderOptions = RenderOptions.None;
        }


        public RenderEntry(RenderEntry reOrig)
        {
            this.DrawCallback = reOrig.DrawCallback;
            this.SceneObject = reOrig.SceneObject;
            this.Pass = reOrig.Pass;
            this.BlendState = reOrig.BlendState;
            this.DepthStencilState = reOrig.DepthStencilState;
            this.Effect = reOrig.Effect;
            this.VertexBuffer = reOrig.VertexBuffer;
            this.NumVertices = reOrig.NumVertices;
            this.IndexBuffer = reOrig.IndexBuffer;
            this.VertexOffset = reOrig.VertexOffset;
            this.StartIndex = reOrig.StartIndex;
            this.RenderOptions = reOrig.RenderOptions;
            this.PrimitiveCount = reOrig.PrimitiveCount;
            this.View = reOrig.View;
            this.Projection = reOrig.Projection;
            this.CameraTransform = reOrig.CameraTransform;
            this.ShadowCastingLightView = reOrig.ShadowCastingLightView;
            this.ShadowCastingLightProjection = reOrig.ShadowCastingLightProjection;
            this.ShadowMap = reOrig.ShadowMap;
        }


        public void Draw()
        {
            DrawCallback(this);
        }


    }

    class RenderEntryComparer : Comparer<RenderEntry>
    {
        public override int Compare(RenderEntry x, RenderEntry y)
        {
            if (x.BlendState != y.BlendState)
                return x.BlendState.GetHashCode() - y.BlendState.GetHashCode();
            if (x.DepthStencilState != y.DepthStencilState)
                return x.DepthStencilState.GetHashCode() - y.DepthStencilState.GetHashCode();
            if (x.RasterizerState != y.RasterizerState)
                return x.RasterizerState.GetHashCode() - y.RasterizerState.GetHashCode();
            if (x.Effect != y.Effect)
                return x.Effect.GetHashCode() - y.Effect.GetHashCode();
            if (x.Pass != y.Pass)
                return x.Pass.GetHashCode() - y.Pass.GetHashCode();
            if (x.VertexBuffer != y.VertexBuffer)
                return x.VertexBuffer.GetHashCode() - y.VertexBuffer.GetHashCode();
            if (x.IndexBuffer != y.IndexBuffer)
                return x.IndexBuffer.GetHashCode() - y.IndexBuffer.GetHashCode();

            return 0;
        }
    }
}
