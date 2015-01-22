using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    class RawGeometryNode : GeometryNode
    {
        public MeshPart Geometry { get; private set; }

        public override VertexBuffer VertexBuffer { get { return Geometry.VertexBuffer; } }
        public override IndexBuffer IndexBuffer { get { return Geometry.IndexBuffer; } }
        protected override int mVertexOffset { get { return Geometry.VertexOffset; } }
        protected override int mNumVertices { get { return Geometry.NumVertices; } }
        protected override int mStartIndex { get { return Geometry.StartIndex; } }
        protected override int mPrimitiveCount { get { return Geometry.PrimitiveCount; } }

        public RawGeometryNode(MeshPart geometry, EffectApplication defaultMaterial)
            : base(defaultMaterial)
        {
            Geometry = geometry;
        }
    }
}
