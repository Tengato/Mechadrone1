using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Mechadrone1
{
    /// <summary>
    /// Custom vertex structure for drawing billboards.
    /// </summary>
    struct BillboardVertex
    {
        // Stores the starting position of the billboard. Will be the same for all four corners.
        public Vector3 Position;

        // The up direction of the billboard model.
        public Vector3 Normal;

        // The uv coordinate of the vertex.
        public Vector2 TexCoord;

        // A random value between -1 to 1, used to make each billboard look slightly different. Usually the value should be the same across all four corners.
        public float Random;

        // Describe the layout of this vertex structure.
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3,
                                 VertexElementUsage.Position, 0),

            new VertexElement(12, VertexElementFormat.Vector3,
                                 VertexElementUsage.Normal, 0),

            new VertexElement(24, VertexElementFormat.Vector2,
                                  VertexElementUsage.TextureCoordinate, 0),

            new VertexElement(32, VertexElementFormat.Single,
                                  VertexElementUsage.TextureCoordinate, 1)
        );

        // Describe the size of this vertex structure.
        public const int SizeInBytes = 36;
    }
}
