using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SlagformCommon
{
    public class SlagMath
    {
        public const float INV_SQRT_3 = 0.577350269f;
        public const float SQRT_3 = 1.73205081f;
        public const float SQRT_2 = 1.41421356f;

        private static Object sRandLock;
        private static bool sHaveSpare;
        private static float sRand1;
        private static float sRand2;

        static SlagMath()
        {
            sRandLock = new Object();
            sHaveSpare = false;
            sRand1 = -1.0f;
            sRand2 = -1.0f;
        }

        public static float GenerateGaussianNoise(float variance, Random rng)
        {
            float result;

            lock (sRandLock)
            {
                if (sHaveSpare)
                {
                    sHaveSpare = false;
                    return (float)(Math.Sqrt(variance * sRand1) * Math.Sin(sRand2));
                }

                sHaveSpare = true;

                sRand1 = Get0To1UpperFloat(rng);
                sRand1 = -2.0f * (float)(Math.Log(sRand1));
                sRand2 = Get0To1UpperFloat(rng) * MathHelper.TwoPi;

                result = (float)(Math.Sqrt(variance * sRand1) * Math.Cos(sRand2));
            }

            return result;
        }

        public static float Get0To1UpperFloat(Random rng)
        {
            return (rng.Next(524288) + 1) / (524288.0f);
        }
    }
}
