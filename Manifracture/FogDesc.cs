using Microsoft.Xna.Framework;

namespace Manifracture
{
    public struct FogDesc
    {
        public float StartDistance;
        public float EndDistance;
        public Color Color;

        public FogDesc(float start, float end, Color color)
        {
            StartDistance = start;
            EndDistance = end;
            Color = color;
        }
    }
}
