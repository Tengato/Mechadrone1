using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace SlagformCommon
{
    public static class Space
    {
        public static BoundingBox CombineBBoxes(BoundingBox a, BoundingBox b)
        {
            BoundingBox result = new BoundingBox();
            result.Min.X = Math.Min(a.Min.X, b.Min.X);
            result.Min.Y = Math.Min(a.Min.Y, b.Min.Y);
            result.Min.Z = Math.Min(a.Min.Z, b.Min.Z);
            result.Max.X = Math.Max(a.Max.X, b.Max.X);
            result.Max.Y = Math.Max(a.Max.Y, b.Max.Y);
            result.Max.Z = Math.Max(a.Max.Z, b.Max.Z);

            return result;
        }

    }
}
