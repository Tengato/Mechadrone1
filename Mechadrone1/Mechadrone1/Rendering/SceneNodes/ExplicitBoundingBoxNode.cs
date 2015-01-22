using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class ExplicitBoundingBoxNode : BoundingBoxNode, IExplicitBoxableVolume
    {
        public ExplicitBoundingBoxNode(BoundingBox aabb)
        {
            Bound = aabb;
        }

        BoundingBox IExplicitBoxableVolume.BoxTransformedVolume(Matrix transform)
        {
            return TransformBox(Bound, transform);
        }
    }
}
