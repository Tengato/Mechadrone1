using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Skelemator;

namespace Mechadrone1
{
    // The GeometryNode renders itself when processed during traversal.
    class TerrainGeometryNode : GeometryNode
    {
        public Terrain Geometry { get; private set; }
        public int VertexOffset { get; private set; }

        public override VertexBuffer VertexBuffer { get { return Geometry.Vertices; } }
        public override IndexBuffer IndexBuffer { get { return Geometry.Indices; } }
        protected override int mVertexOffset { get { return VertexOffset; } }
        protected override int mNumVertices { get { return Geometry.SectorVertexSpan; } }
        protected override int mStartIndex { get { return 0; } }
        protected override int mPrimitiveCount { get { return Geometry.TriangleCount; } }

        public TerrainGeometryNode(Terrain geometry, int vertexOffset, EffectApplication defaultMaterial)
            : base(defaultMaterial)
        {
            Geometry = geometry;
            VertexOffset = vertexOffset;
        }
    }
}
