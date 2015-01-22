using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class StaticCamera : ICamera
    {
        public Matrix Transform { get; set; }

        /// <summary>
        /// Angle swept out by frustum in the xz plane of camera space, in radians.
        /// </summary>
        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }

        public Matrix View
        {
            get { return Matrix.Invert(Transform); }
            set { Transform = Matrix.Invert(value); }
        }

        public Matrix Projection
        {
            get { return Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, 0.1f, 3500.0f); }
        }

        public BoundingFrustum Frustum
        {
            get { return new BoundingFrustum(View * Projection); }
        }

        public StaticCamera()
        {
            Transform = Matrix.Identity;
            FieldOfView = MathHelper.PiOver4;
            AspectRatio = 1.0f;
        }
    }
}
