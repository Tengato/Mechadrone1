using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    // The GeometryNode renders itself when processed during traversal.
    class ModelGeometryNode : GeometryNode
    {
        public ModelMeshPart Geometry { get; private set; }

        public override VertexBuffer VertexBuffer { get { return Geometry.VertexBuffer; } }
        public override IndexBuffer IndexBuffer { get { return Geometry.IndexBuffer; } }
        protected override int mVertexOffset { get { return Geometry.VertexOffset; } }
        protected override int mNumVertices { get { return Geometry.NumVertices; } }
        protected override int mStartIndex { get { return Geometry.StartIndex; } }
        protected override int mPrimitiveCount { get { return Geometry.PrimitiveCount; } }

        public ModelGeometryNode(ModelMeshPart geometry, EffectApplication defaultMaterial)
            : base(defaultMaterial)
        {
            Geometry = geometry;
        }
    }
}
