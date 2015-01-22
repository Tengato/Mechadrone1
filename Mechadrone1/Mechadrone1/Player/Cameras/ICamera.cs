using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    interface ICamera
    {
        Matrix View { get; }
        Matrix Transform { get; }
        BoundingFrustum Frustum { get; }
        float AspectRatio { get; set; }
    }
}
