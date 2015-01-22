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
                    Matrix.CreateFromQuaternion(Quaternion.Slerp(rotA, rotB, blendFactor)) *
                    Matrix.CreateTranslation(Vector3.Lerp(posA, posB, blendFactor));
            }

            return blendedTransforms;
        }

        public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            return new Matrix(
                2.0f / (right - left), 0.0f, 0.0f, 0.0f,
                0.0f, 2.0f / (top - bottom), 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f / (zFarPlane - zNearPlane), 0.0f,
                -(right + left) / (right - left), -(top + bottom) / (top - bottom), -zNearPlane / (zFarPlane - zNearPlane), 1.0f
                );
        }

        public static Quaternion GetOrientation(Vector3 forwardDirection, Vector3 upDirection)
        {
            return Quaternion.CreateFromRotationMatrix(Matrix.Transpose(Matrix.CreateLookAt(Vector3.Zero, forwardDirection, upDirection)));
        }

        public static double GetQuaternionAngle(Quaternion unitQ)
        {
            //Vector3 axis = new Vector3(unitQ.X, unitQ.Y, unitQ.Z);
            //return Math.Abs(2.0d * Math.Atan2(axis.Length(), unitQ.W));
            return 2.0d * Math.Acos(MathHelper.Clamp(unitQ.W, -1.0f, 1.0f));
        }

        public static Vector4 ToVec4Pos(Vector3 v)
        {
            return new Vector4(v.X, v.Y, v.Z, 1.0f);
        }

        public static Quaternion GetSweptQuaternion(Vector3 start, Vector3 stop)
        {
            float lengthSquared = start.LengthSquared();
            if (lengthSquared == 0.0f)
                throw new ArgumentException("start");

            if (Math.Abs(lengthSquared - 1.0f) > 0.00005)
                start.Normalize();

            lengthSquared = stop.LengthSquared();
            if (lengthSquared == 0.0f)
                throw new ArgumentException("stop");

            if (Math.Abs(lengthSquared - 1.0f) > 0.00005)
                stop.Normalize();

            float dot = Vector3.Dot(start, stop);
            Vector3 axis = Vector3.Cross(start, stop);
            float theta;
            if (axis.LengthSquared() == 0.0f)
            {
                // Linearly dependent.
                axis = SpaceUtils.GetPerpendicular(start);

                if (dot < 0.0f)
                    theta = MathHelper.Pi;
                else
                    theta = 0.0f;
            }
            else
            {
                theta = (float)(Math.Acos(MathHelper.Clamp(dot, -1.0f, 1.0f)));
            }
            axis.Normalize();

            return Quaternion.CreateFromAxisAngle(axis, theta);
        }

        private static Vector3 GetPerpendicular(Vector3 v)
        {
            if (v.X == 0.0f && v.Y == 0.0f)
            {
                if (v.Z == 0.0f)
                    throw new ArgumentException("Zero vector.");

                return Vector3.Up;
            }
            return new Vector3(-v.Y, v.X, 0.0f);
        }

        public static float GetSweptAngle(Vector2 start, Vector2 stop)
        {
            float lengthSquared = start.LengthSquared();
            if (lengthSquared == 0.0f)
                throw new ArgumentException("start");

            if (Math.Abs(lengthSquared - 1.0f) > 0.00005)
                start.Normalize();

            lengthSquared = stop.LengthSquared();
            if (lengthSquared == 0.0f)
                throw new ArgumentException("stop");

            if (Math.Abs(lengthSquared - 1.0f) > 0.00005)
                stop.Normalize();

            float dot = Vector2.Dot(start, stop);
            Vector3 start3D = new Vector3(start, 0.0f);
            Vector3 stop3D = new Vector3(stop, 0.0f);
            Vector3 cross = Vector3.Cross(start3D, stop3D);
            float theta;
            if (cross.Z == 0.0f)
            {
                if (dot < 0.0f)
                    return MathHelper.Pi;
                else
                    return 0.0f;
            }
            theta = (float)(Math.Acos(MathHelper.Clamp(dot, -1.0f, 1.0f)));

            return Math.Sign(cross.Z) == 1 ? theta : -theta;
        }

        public static BEPUutilities.Matrix2x2 Create2x2RotationMatrix(float radians)
        {
            float cos = (float)(Math.Cos(radians));
            float sin = (float)(Math.Sin(radians));
            return new BEPUutilities.Matrix2x2(cos, -sin, sin, cos);
        }

        public static BEPUutilities.Vector2 TransformVec2(BEPUutilities.Vector2 vector, BEPUutilities.Matrix2x2 transform)
        {
            return new BEPUutilities.Vector2(vector.X * transform.M11 + vector.Y * transform.M21,
                                             vector.X * transform.M12 + vector.Y * transform.M22);
        }
    }
}
