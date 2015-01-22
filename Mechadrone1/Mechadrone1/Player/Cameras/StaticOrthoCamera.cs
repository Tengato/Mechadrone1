using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class StaticOrthoCamera : ICamera
    {
        public Matrix Transform { get; set; }
        public float Width { get; set; }
        public float AspectRatio { get; set; }

        public Matrix View
        {
            get { return Matrix.Invert(Transform); }
            set { Transform = Matrix.Invert(value); }
        }

        public Matrix Projection
        {
            get { return Matrix.CreateOrthographic(Width, Width / AspectRatio, 0.1f, 1000.0f); }
        }

        public BoundingFrustum Frustum
        {
            get { return new BoundingFrustum(View * Projection); }
        }

        public StaticOrthoCamera()
        {
            Transform = Matrix.Identity;
            Width = 25.0f;
            AspectRatio = 1.0f;
        }
    }
}
