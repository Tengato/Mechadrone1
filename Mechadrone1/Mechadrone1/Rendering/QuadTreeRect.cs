using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Rendering
{
    struct QuadTreeRect
    {
        public int X0 { get; set; }
        public int Z0 { get; set; }

        private int y0;
        public int Y0
        {
            get
            {
                return y0;
            }
            set
            {
                y0 = value;
                yMask = null;
            }
        }

        public int X1 { get; set; }
        public int Z1 { get; set; }

        private int y1;
        public int Y1
        {
            get
            {
                return y1;
            }
            set
            {
                y1 = value;
                yMask = null;
            }
        }

        private int? yMask;

        public int YMask
        {
            get
            {
                if (yMask == null)
                {
                    int high = (1 << Y1);
                    int low = (1 << Y0);
                    int setMask = high - 1;
                    int clearMask = low - 1;

                    yMask = setMask;
                    if (Y0 > 0)
                    {
                        yMask &= ~clearMask;
                    }
                    yMask |= high;
                    yMask |= low;
                }

                return (int)yMask;
            }
        }


        public static QuadTreeRect CreateFromBoundingBox(BoundingBox worldRect, Matrix worldToQuadTreeTransform)
        {
            QuadTreeRect result = new QuadTreeRect();

            // reposition and scale world coordinates to quad tree coordinates
            worldRect.Min = Vector3.Transform(worldRect.Min, worldToQuadTreeTransform);
            worldRect.Max = Vector3.Transform(worldRect.Max, worldToQuadTreeTransform);

            // reduce by a tiny amount to handle tiled data
            worldRect.Max -= (worldRect.Max - worldRect.Min) * 0.0001f;

            // convert to integer values, taking the floor of each real
            result.X0 = (int)(Math.Floor((double)(worldRect.Min.X)));
            result.X1 = (int)(Math.Floor((double)(worldRect.Max.X)));
            result.Z0 = (int)(Math.Floor((double)(worldRect.Min.Z)));
            result.Z1 = (int)(Math.Floor((double)(worldRect.Max.Z)));
            result.Y0 = (int)(Math.Floor((double)(worldRect.Min.Y)));
            result.Y1 = (int)(Math.Floor((double)(worldRect.Max.Y)));

            // we must be positive
            result.X0 = Clamp(result.X0, 0, 254);
            result.Z0 = Clamp(result.Z0, 0, 254);
            result.Y0 = Clamp(result.Y0, 0, 30);

            // we must be at least one unit large
            result.X1 = Clamp(result.X1, result.X0 + 1, 255);
            result.Z1 = Clamp(result.Z1, result.Z0 + 1, 255);
            result.Y1 = Clamp(result.Y1, result.Y0 + 1, 31);

            return result;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }


    }
}
