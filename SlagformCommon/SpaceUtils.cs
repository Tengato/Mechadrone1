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
    public static class SpaceUtils
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

        public static Vector3 GetBarycentricCoords(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 b)
        {
            // Get the barycentric coords of the b position.
            float detT = (p2.Y - p3.Y) * (p1.X - p3.X) +
                (p3.X - p2.X) * (p1.Y - p3.Y);
            float b1 = ((p2.Y - p3.Y) * (b.X - p3.X) +
                (p3.X - p2.X) * (b.Y - p3.Y)) / detT;
            float b2 = ((p3.Y - p1.Y) * (b.X - p3.X) +
                (p1.X - p3.X) * (b.Y - p3.Y)) / detT;
            float b3 = 1.0f - b1 - b2;

            return new Vector3(b1, b2, b3);
        }


        public static Matrix[] LerpSkeletalPose(Matrix[] poseA, Matrix[] poseB, float blendFactor)
        {
            Matrix[] blendedTransforms = new Matrix[poseA.Length];

            Vector3 scaleA;
            Quaternion rotA;
            Vector3 posA;
            Vector3 scaleB;
            Quaternion rotB;
            Vector3 posB;

            for (int p = 0; p < poseA.Length; p++)
            {
                poseA[p].Decompose(out scaleA, out rotA, out posA);
                poseB[p].Decompose(out scaleB, out rotB, out posB);
                blendedTransforms[p] = Matrix.CreateScale(Vector3.Lerp(scaleA, scaleB, blendFactor)) *
                    Matrix.CreateFromQuaternion(Quaternion.Lerp(rotA, rotB, blendFactor)) *
                    Matrix.CreateTranslation(Vector3.Lerp(posA, posB, blendFactor));
            }

            return blendedTransforms;
        }

    }
}
