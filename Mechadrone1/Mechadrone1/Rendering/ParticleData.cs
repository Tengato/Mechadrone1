using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    class ParticleData
    {
        public DynamicVertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int FirstActiveParticleIndex { get; set; }
        public int FirstFreeParticleIndex { get; set; }
        public int MaxParticles { get; set; }

        public ParticleData()
        {
            VertexBuffer = null;
            IndexBuffer = null;
            FirstActiveParticleIndex = 0;
            FirstFreeParticleIndex = 0;
            MaxParticles = 0;
        }
    }
}
