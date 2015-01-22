using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    class MeshPart
    {
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int VertexOffset { get; set; }
        public int NumVertices { get; set; }
        public int StartIndex { get; set; }
        public int PrimitiveCount { get; set; }
    }
}
