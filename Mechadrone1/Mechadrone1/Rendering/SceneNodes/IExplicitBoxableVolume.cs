using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    interface IExplicitBoxableVolume
    {
        BoundingBox BoxTransformedVolume(Matrix transform);
    }
}
