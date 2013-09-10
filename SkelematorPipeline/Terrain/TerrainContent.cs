using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SkelematorPipeline
{
    [ContentSerializerRuntimeType("Skelemator.Terrain, Skelemator")]
    public class TerrainContent
    {
        public int VertexCountAlongXAxis;
        public int VertexCountAlongZAxis;
        public int SectorSize;
        public float XZScale;
        public VertexBufferContent VertexBufferContent;
        public IndexCollection IndexCollection;
        public int TriangleCount;
        public int VertexCount;
        public MaterialContent MaterialContent;
        public object Tag;
    }
}
