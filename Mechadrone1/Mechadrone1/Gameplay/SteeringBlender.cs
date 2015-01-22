using Microsoft.Xna.Framework;
using SlagformCommon;
using System;
using BEPUphysics;
using System.Collections.Generic;
using BEPUphysics.CollisionShapes.ConvexShapes;
using RigidTransform = BEPUutilities.RigidTransform;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BepuVec3 = BEPUutilities.Vector3;

namespace Mechadrone1
{
    class SteeringBlender
    {
        public enum WeightType
        {
            Wait,
            Wander,
            Seek,
            Arrive,
            NUM_WEIGHT_TYPES,
        }

        // TODO: P3: This could be separate if the behavior doesn't need wander...
        private const float WANDER_VARIANCE = MathHelper.Pi * MathHelper.Pi / 1296.0f;
        private const float WANDER_RADIUS = 0.1f;
        private const float WANDER_DISTANCE = 10.0f;
        private const float ARRIVE_TOLERANCE = 2.0f;
        private float wanderTheta;
        private Vector3[] mDirHistory;
        private float[] mForceHistory;
        private const int HISTORY_SIZE = 17;
        private int mHistoryIndex;
        public Vector3 Target { get; set; }
        // Determines how aggressively acceleration is changed
        public float Urgency { get; set; }
        public float[] Weights { get; set; }
        public float ForceScale { get; set; }

        public float TotalWeight
        {
            get
            {
                float result = 0.0f;
                for (int w = 0; w < (int)WeightType.NUM_WEIGHT_TYPES; ++w)
                {
                    result += Weights[w];
                }
                return result;
            }
        }

        public SteeringBlender()
        {
            wanderTheta = 0.0f;
            mDirHistory = new Vector3[HISTORY_SIZE];
            mForceHistory = new float[HISTORY_SIZE];
            mHistoryIndex = 0;
            Target = Vector3.Zero;
            Urgency = 1.0f;
            Weights = new float[(int)WeightType.NUM_WEIGHT_TYPES];
            for (int w = 0; w < (int)WeightType.NUM_WEIGHT_TYPES; ++w)
            {
                Weights[w] = 0.0f;
            }
            Weights[(int)WeightType.Wander] = 1.0f;
            ForceScale = 1.0f;
        }

        public void NormalizeWeights()
        {
            if (TotalWeight > 0.0f)
            {
                float totalWeight = TotalWeight;
                for (int w = 0; w < (int)WeightType.NUM_WEIGHT_TYPES; ++w)
                {
                    Weights[w] = Weights[w] / totalWeight;
                }
            }
        }

        public Vector2 ComputeForce(Actor owner)
        {
            // Aggregate the behaviors to produce an initial 'immediate' result:
            Vector2 result = Vector2.Zero;
            if (Weights[(int)WeightType.Seek] > 0.0f)
                result += Weights[(int)WeightType.Seek] / TotalWeight * Seek(owner);

            if (Weights[(int)WeightType.Arrive] > 0.0f)
                result += Weights[(int)WeightType.Arrive] / TotalWeight * Arrive(owner);

            if (Weights[(int)WeightType.Wander] > 0.0f)
                result += Weights[(int)WeightType.Wander] / TotalWeight * Wander();

            if (Weights[(int)WeightType.Wait] > 0.0f)
                result += Weights[(int)WeightType.Wait] / TotalWeight * Wait();

            result = FitIntoSlack(result, AvoidObstacles(owner));

            // Create a 3D version of the result (so we can transform it using rotation about the y-axis)
            Vector3 res3D = new Vector3(result.X, 0.0f, -result.Y);

            // Keep a history of unit force directions, in world space.  And magnitudes.
            // Rotate into world space before storing so the history is all in the same space.
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            Matrix controllerRotation = Matrix.Transpose(Matrix.CreateLookAt(Vector3.Zero, BepuConverter.Convert(bcc.Controller.HorizontalViewDirection), Vector3.Up));
            res3D = Vector3.Transform(res3D, controllerRotation);

            // Store in the history arrays;
            mHistoryIndex = (mHistoryIndex + 1) % HISTORY_SIZE;
            mForceHistory[mHistoryIndex] = res3D.Length();
            if (mForceHistory[mHistoryIndex] != 0.0f)
            {
                mDirHistory[mHistoryIndex] = res3D / mForceHistory[mHistoryIndex];
            }
            else
            {
                mDirHistory[mHistoryIndex] = Vector3.Forward;
            }

            // Get the average direction:
            Vector3 dirAvg = Vector3.Zero;
            for (int v = 0; v < HISTORY_SIZE; ++v)
            {
                int vIndex = (mHistoryIndex - v + HISTORY_SIZE) % HISTORY_SIZE;
                dirAvg += mDirHistory[vIndex] * (HISTORY_SIZE - v);  // Weight to favor more recent entries.
            }

            if (dirAvg.LengthSquared() != 0.0f)
                dirAvg.Normalize();

            // avgTheta is a measure of how much each direction in the history varies with the average.
            float avgTheta = 0.0f;
            int numWeights = ((HISTORY_SIZE + 1) * HISTORY_SIZE) / 2;
            for (int v = 0; v < HISTORY_SIZE; ++v)
            {
                int vIndex = (mHistoryIndex - v + HISTORY_SIZE) % HISTORY_SIZE;
                avgTheta += mForceHistory[vIndex] == 0.0f ? 0.0f : (float)(Math.Acos(MathHelper.Clamp(Vector3.Dot(mDirHistory[vIndex], dirAvg), 0.0f, 1.0f))) * (float)(HISTORY_SIZE - v);
            }

            avgTheta /= (float)numWeights;

            // If avgTheta is large, the immediate forces are jittery, and we should use more history to smooth out the final result.
            // otherwise, the immediate force is more continuous and we can use fewer histories.

            int numHistoriesToUse = Math.Max((int)(Math.Min(avgTheta / MathHelper.Pi * 8.0f, 1.0f) * (float)HISTORY_SIZE), 1);

            // In addition, we give recent forces more weight using a simple arithmetic sequence.
            numWeights = ((numHistoriesToUse + 1) * numHistoriesToUse) / 2;

            res3D = Vector3.Zero;
            for (int v = 0; v < numHistoriesToUse; ++v)
            {
                int vIndex = (mHistoryIndex - v + HISTORY_SIZE) % HISTORY_SIZE;
                res3D += mDirHistory[vIndex] * (float)(numHistoriesToUse - v) * mForceHistory[vIndex];
            }

            res3D /= (float)(numWeights);

            // Back again into controller space:
            controllerRotation = Matrix.Transpose(controllerRotation);
            res3D = Vector3.Transform(res3D, controllerRotation);

            result.X = res3D.X;
            result.Y = -res3D.Z;
            return result * ForceScale;
        }

        private Vector2 FitIntoSlack(Vector2 newForce, Vector2 baseForce)
        {
            float baseLengthSq = baseForce.LengthSquared();

            if (baseLengthSq < 1.0f && baseLengthSq > 0.0f)
            {
                Vector2 unitBase = baseForce;
                unitBase.Normalize();

                float fwdCompNew = Vector2.Dot(newForce, unitBase);
                if (fwdCompNew < 0.0f)
                    newForce -= fwdCompNew * unitBase;

                Vector2 unitNew = newForce;
                unitNew.Normalize();

                // Use up the slack to satisfy newForce as much as possible. Calculating the amount to add
                // so that the resulting length is one is a bit tricky:
                float fwdCompBase = Vector2.Dot(unitNew, baseForce);
                float latCompBase = (baseForce - unitNew * fwdCompBase).Length();

                float slack = (float)(Math.Sqrt(1.0f - latCompBase * latCompBase)) - fwdCompBase;
                baseForce += (newForce.LengthSquared() > slack * slack) ? slack * unitNew : newForce;
            }
            else if (baseLengthSq > 1.0f)
            {
                // Clamp:
                baseForce.Normalize();
            }
            else
            {
                float newLengthSq = newForce.LengthSquared();
                if (newLengthSq > 1.0f)
                {
                    newForce.Normalize();
                    return newForce;
                }
                else if (newLengthSq > 0.0f)
                {
                    return newForce;
                }
            }

            return baseForce;
        }

        // The following methods decide how to spend a locomotion impulse, constrained to the horizontal plane,
        // to achieve a particular behavior. The impulse is given in the controller's local horizontal space.

        // Don't move.
        private Vector2 Wait()
        {
            return Vector2.Zero;
        }

        // Get to a target ASAP.
        private Vector2 Seek(Actor owner)
        {
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            Vector3 relativeTarget = Target - BepuConverter.Convert(bcc.Controller.Body.Position);

            if (relativeTarget.X == 0.0f && relativeTarget.Z == 0.0f)
                return Vector2.Zero;

            // Rotate into controller space.
            Matrix controllerRotation = Matrix.CreateLookAt(Vector3.Zero, BepuConverter.Convert(bcc.Controller.HorizontalViewDirection), Vector3.Up);
            relativeTarget = Vector3.Transform(relativeTarget, controllerRotation);

            // Project the target onto the horizontal plane and normalize (via simple trig since we know Y == 0.0f).
            // Take theta = 0.0f is facing forward (-Z)
            double theta = Math.Atan2(-relativeTarget.X, -relativeTarget.Z);

            return new Vector2(-(float)(Math.Sin(theta)), (float)(Math.Cos(theta)));
        }

        // Come to a smooth stop at a target.
        private Vector2 Arrive(Actor owner)
        {
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            Vector3 relativeTarget = Target - BepuConverter.Convert(bcc.Controller.Body.Position);

            relativeTarget.Y = 0.0f;

            if (relativeTarget.LengthSquared() < ARRIVE_TOLERANCE)
                return Vector2.Zero;

            float distToTarget = relativeTarget.Length();
            relativeTarget /= distToTarget;
            float normalizedSpeedToTarget = Vector3.Dot(BepuConverter.Convert(bcc.Controller.Body.LinearVelocity), relativeTarget) /
                bcc.RunSpeed;

            // Rotate into controller space.
            Matrix controllerRotation = Matrix.CreateLookAt(Vector3.Zero, BepuConverter.Convert(bcc.Controller.HorizontalViewDirection), Vector3.Up);
            relativeTarget = Vector3.Transform(relativeTarget, controllerRotation);

            Vector2 seekResult = new Vector2(relativeTarget.X, -relativeTarget.Z);

            const float DECELERATION_RADIUS = 24.0f; // Tweak this

            float closeness = Math.Max(0.0f, normalizedSpeedToTarget * normalizedSpeedToTarget - distToTarget / DECELERATION_RADIUS * Urgency);
            float brakes = closeness > 1.0f ? 1.0f : (float)(Math.Asin(closeness) / MathHelper.PiOver2);

            return (1.0f - brakes) * seekResult;
        }

        // Add a little variation into the steering.
        private Vector2 Wander()
        {
            // Update wanderTheta.
            wanderTheta += SlagMath.GenerateGaussianNoise(WANDER_VARIANCE, GameResources.ActorManager.Random);
            while (wanderTheta > MathHelper.Pi)
                wanderTheta -= MathHelper.TwoPi;

            while(wanderTheta < -MathHelper.Pi)
                wanderTheta += MathHelper.TwoPi;

            Vector2 newHeading = Vector2.UnitY * WANDER_DISTANCE +
                new Vector2((float)(-Math.Sin(wanderTheta)), (float)(Math.Cos(wanderTheta))) * WANDER_RADIUS;

            if (newHeading.X == 0.0f && newHeading.Y == 0.0f)
                return Vector2.Zero;

            newHeading.Normalize();
            return newHeading;
        }

        private static float GetAngleFromVertical(BEPUutilities.Vector3 vector)
        {
            return (float)(Math.Atan2(Math.Sqrt(vector.X * vector.X + vector.Z * vector.Z), Math.Abs(vector.Y)));
        }

        // Steer away from obstacles in the way. This method returns a zero vector if no correction is required.
        // It should be high priority and the steering from other behaviors should blend into the remaining space.
        // So if this returns a length 1.0f vector, avoiding the obstacle is most urgent and there is no room for other
        // steering.
        private Vector2 AvoidObstacles(Actor owner)
        {
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            // Conditions where we do not want to use this steering force.
            if (GetAngleFromVertical(bcc.Controller.Body.LinearVelocity) < MathHelper.PiOver4 ||    // We're probably falling...
                !bcc.Controller.SupportFinder.HasSupport ||
                !bcc.Controller.SupportFinder.HasTraction)
                return Vector2.Zero;

            // Sphere cast ahead along facing.
            List<RayCastResult> obstacles = new List<RayCastResult>();
            SphereShape probe = new SphereShape(bcc.Controller.BodyRadius * 1.1f);
            RigidTransform probeStartPosition = new RigidTransform(bcc.Controller.Body.Position);
            // Add a small constant to the probe length because we want a minimum amount of forward probing, even if we are not moving.
            float probeLength = Math.Max(BepuVec3.Dot(bcc.Controller.Body.LinearVelocity, bcc.Controller.ViewDirection), 0.0f) + 1.0f;
            BepuVec3 probeSweep = bcc.Controller.ViewDirection * probeLength;
            ObstacleFilter filter = new ObstacleFilter(bcc.Controller.Body.CollisionInformation);
            GameResources.ActorManager.SimSpace.ConvexCast(probe, ref probeStartPosition, ref probeSweep, filter.Test, obstacles);

            RayCastDistanceComparer rcdc = new RayCastDistanceComparer();

            obstacles.Sort(rcdc);

            BEPUutilities.Vector3 cross = BEPUutilities.Vector3.Zero;
            int obstacleIndex = 0;
            do
            {
                if (obstacles.Count == obstacleIndex)
                    return Vector2.Zero;

                cross = BEPUutilities.Vector3.Cross(bcc.Controller.ViewDirection, -obstacles[obstacleIndex++].HitData.Normal);
            }
            while (cross.X > 0.7f); // if cross.X > 0.7f, the obstacle is some kind of gentle ramp; ignore it.

            // dot will typically be negative and magnitude indicates how directly ahead the obstacle is.
            float dot = BEPUutilities.Vector3.Dot(bcc.Controller.ViewDirection, -obstacles[0].HitData.Normal);
            if (dot >= 0.0f) // The obstacle won't hinder us if we touch it.
                return Vector2.Zero;

            // When cross.Y is positive, the object is generally to the right, so veer left (and vice versa).
            float directionSign = cross.Y >= 0.0f ? -1.0f : 1.0f;
            BEPUutilities.Vector2 result = BEPUutilities.Vector2.UnitX * directionSign * -dot;

            // Also scale response by how close the obstacle is.
            float distance = (obstacles[0].HitData.Location - bcc.Controller.Body.Position).Length();

            result *= MathHelper.Clamp((1.0f - distance / probeLength), 0.0f, 1.0f); // / Math.Abs(dot);


            // So far the result is in terms of 'velocity space'. Rotate it to align with the controller facing.
            float velocityTheta = (float)(Math.Atan2(-probeSweep.X, -probeSweep.Z));
            BEPUutilities.Matrix2x2 velocityWorld = SpaceUtils.Create2x2RotationMatrix(velocityTheta);
            float facingTheta = (float)(Math.Atan2(-bcc.Controller.HorizontalViewDirection.X, -bcc.Controller.HorizontalViewDirection.Z));
            BEPUutilities.Matrix2x2 facingWorldInv = SpaceUtils.Create2x2RotationMatrix(facingTheta);
            facingWorldInv.Transpose(); // We want the transpose/inverse of the facing transform because we want to transform the movement into 'facing space'.

            return BepuConverter.Convert(SpaceUtils.TransformVec2(SpaceUtils.TransformVec2(result, velocityWorld), facingWorldInv));
        }

        private class ObstacleFilter
        {
            private BroadPhaseEntry mSelf;

            public ObstacleFilter(BroadPhaseEntry self)
            {
                mSelf = self;
            }

            public bool Test(BroadPhaseEntry test)
            {
                //EntityCollidable ec = test as EntityCollidable;
                //if (ec != null && ec.Tag != null && (int)(ec.Tag) == mSelfId)
                //    return false;

                return test != mSelf && test.CollisionRules.Group != GameResources.ActorManager.PickupsCollisionGroup;
            }
        }

        // This was my original attempt at seek, but it's not sure of what it needs to do. There are some interesting
        // computations in here that I might want to use later though.
        private Vector3 AbandonedSeek(Actor owner, Vector3 target, Plane surface)
        {
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            // The location of the feet is the most reliable way to get current position in a nav surface
            Vector3 bipedFeetPosition = BepuConverter.Convert(bcc.Controller.Body.Position) -
                BepuConverter.Convert(bcc.Controller.Down) * bcc.Controller.SupportFinder.RayLengthToBottom;

            Vector3 relativeTarget = target - bipedFeetPosition;
            // Project the target onto the surface plane.
            Vector3 surfaceProjectedRelativeTarget = relativeTarget - surface.Normal * Vector3.Dot(relativeTarget, surface.Normal);

            // Unlikely that we're spot on, but we must avoid divide by zero on Normalize.
            if (surfaceProjectedRelativeTarget.LengthSquared() == 0.0f)
                return Vector3.Zero;

            surfaceProjectedRelativeTarget.Normalize();

            // Difference between actual and ideal velocity. This will be the direction of our impulse with a few exceptions.
            Vector3 velocityDiff = surfaceProjectedRelativeTarget * bcc.RunSpeed - BepuConverter.Convert(bcc.Controller.Body.LinearVelocity);

            // Exception 1: We don't want to remove excess velocity that's in the target's direction.
            float deceleration = Vector3.Dot(velocityDiff, surfaceProjectedRelativeTarget);
            if (deceleration < 0)
                velocityDiff -= surfaceProjectedRelativeTarget * deceleration;

            // Normalize velocityDiff in terms of max speed.
            velocityDiff /= bcc.RunSpeed;

            float lengthSq = velocityDiff.LengthSquared();
            if (lengthSq < 1.0f)
            {
                // Exception 2 : Use up the extra impulse capacity to push against friction. Calculating the amount to add
                // so that the resulting length is one is a bit tricky:
                float fwdComp = Vector3.Dot(velocityDiff, surfaceProjectedRelativeTarget);
                float latComp = (velocityDiff - surfaceProjectedRelativeTarget * fwdComp).Length();

                float makeupLength = (float)(Math.Sqrt(1.0f - latComp * latComp)) - fwdComp;
                velocityDiff += makeupLength * surfaceProjectedRelativeTarget;
            }
            else if (lengthSq > 1.0f)
            {
                // Clamp
                velocityDiff.Normalize();
            }

            return velocityDiff;
        }
    }
}
