using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mechadrone1
{
    class LevitationMovementController
    {
        public Vector3 Position;       // player position
        public Vector3 Velocity;       // velocity in local player space
        public Vector3 Force;          // forces in local player space

        // player rotation
        public Matrix Rotation;
        // rotation velocities around each local player axis
        public Vector3 RotationVelocityAxis;
        // rotation forces around each local player axis
        public Vector3 RotationForce;

        LevitationHandlingDesc handling;

        /// <summary>
        /// Create a new player movement object for handling player motion
        /// </summary>
        public LevitationMovementController(LevitationHandlingDesc handlingDesc)
        {
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            Force = Vector3.Zero;

            Rotation = Matrix.Identity;
            RotationVelocityAxis = Vector3.Zero;
            RotationForce = Vector3.Zero;

            handling = handlingDesc;
        }

        /// <summary>
        /// Resets the position and rotation of the player and zero forces
        /// </summary>
        public void Reset(Matrix transfrom)
        {
            Rotation = transfrom;
            Rotation.Translation = Vector3.Zero;
            Position = transfrom.Translation;

            Velocity = Vector3.Zero;
            Force = Vector3.Zero;
            
            RotationVelocityAxis = Vector3.Zero;
            RotationForce = Vector3.Zero;
        }

        /// <summary>
        /// Get the current postion and rotation as a matrix
        /// </summary>
        public Matrix Transform
        {
            get
            {
                Matrix transform;

                // set rotation
                transform = Rotation;

                // set translation
                transform.Translation = Position;

                return transform;
            }
        }

        /// <summary>
        /// Get the normalized velocity
        /// </summary>
        public float VelocityFactor
        {
            get { return Velocity.Length() / handling.MaxVelocity; }
        }

        /// <summary>
        /// Get/Set the velocity vector transformed to world space
        /// </summary>
        public Vector3 WorldVelocity
        {
            // transform local velocity to world space
            get
            {
                return Velocity.X * Rotation.Right +
                Velocity.Y * Rotation.Up + Velocity.Z * Rotation.Forward;
            }
            set
            {
                // transform world velocity into local space
                Velocity.X = Vector3.Dot(Rotation.Right, value);
                Velocity.Y = Vector3.Dot(Rotation.Up, value);
                Velocity.Z = Vector3.Dot(Rotation.Forward, value);
            }
        }

        public void Update(float dTimeSeconds)
        {
            // apply force
            Velocity += Force * dTimeSeconds;

            // apply damping
            if (Force.X > -0.001f && Force.X < 0.001f)
                if (Velocity.X > 0)
                    Velocity.X = Math.Max(0.0f, Velocity.X - handling.DampingForce * dTimeSeconds);
                else
                    Velocity.X = Math.Min(0.0f, Velocity.X + handling.DampingForce * dTimeSeconds);
            if (Force.Y > -0.001f && Force.Y < 0.001f)
                if (Velocity.Y > 0)
                    Velocity.Y = Math.Max(0.0f, Velocity.Y - handling.DampingForce * dTimeSeconds);
                else
                    Velocity.Y = Math.Min(0.0f, Velocity.Y + handling.DampingForce * dTimeSeconds);
            if (Force.Z > -0.001f && Force.Z < 0.001f)
                if (Velocity.Z > 0)
                    Velocity.Z = Math.Max(0.0f, Velocity.Z - handling.DampingForce * dTimeSeconds);
                else
                    Velocity.Z = Math.Min(0.0f, Velocity.Z + handling.DampingForce * dTimeSeconds);

            // crop with maximum velocity
            float velocityLength = Velocity.Length();
            if (velocityLength > handling.MaxVelocity)
                Velocity = Vector3.Normalize(Velocity) * handling.MaxVelocity;

            // apply velocity
            if (Velocity.LengthSquared() > 0.0f)
                Position += dTimeSeconds * Vector3.Transform(Velocity, Rotation);

            // apply rot force
            RotationVelocityAxis += RotationForce * dTimeSeconds;

            // apply rot damping
            if (RotationForce.X > -0.001f && RotationForce.X < 0.001f)
                if (RotationVelocityAxis.X > 0)
                    RotationVelocityAxis.X = Math.Max(0.0f, 
                                    RotationVelocityAxis.X -
                                    handling.DampingRotationForce * dTimeSeconds);
                else
                    RotationVelocityAxis.X = Math.Min(0.0f, 
                                    RotationVelocityAxis.X +
                                    handling.DampingRotationForce * dTimeSeconds);
            
            if (RotationForce.Y > -0.001f && RotationForce.Y < 0.001f)
                if (RotationVelocityAxis.Y > 0)
                    RotationVelocityAxis.Y = Math.Max(0.0f, 
                                    RotationVelocityAxis.Y -
                                    handling.DampingRotationForce * dTimeSeconds);
                else
                    RotationVelocityAxis.Y = Math.Min(0.0f, 
                                    RotationVelocityAxis.Y +
                                    handling.DampingRotationForce * dTimeSeconds);
            
            if (RotationForce.Z > -0.001f && RotationForce.Z < 0.001f)
                if (RotationVelocityAxis.Z > 0)
                    RotationVelocityAxis.Z = Math.Max(0.0f, 
                                    RotationVelocityAxis.Z -
                                    handling.DampingRotationForce * dTimeSeconds);
                else
                    RotationVelocityAxis.Z = Math.Min(0.0f, 
                                    RotationVelocityAxis.Z +
                                    handling.DampingRotationForce * dTimeSeconds);

            // crop with maximum rot velocity
            float rotationVelocityLength = RotationVelocityAxis.Length();
            if (rotationVelocityLength > handling.MaxRotationVelocity)
                RotationVelocityAxis = Vector3.Normalize(RotationVelocityAxis) *
                    handling.MaxRotationVelocity;

            // apply rot vel
            Matrix rotationVelocity = Matrix.Identity;

            if (RotationVelocityAxis.X < -0.001f || RotationVelocityAxis.X > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(Rotation.Right, 
                    RotationVelocityAxis.X * dTimeSeconds);

            if (RotationVelocityAxis.Y < -0.001f || RotationVelocityAxis.Y > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(Rotation.Up, 
                    RotationVelocityAxis.Y * dTimeSeconds);

            if (RotationVelocityAxis.Z < -0.001f || RotationVelocityAxis.Z > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(Rotation.Backward, 
                    RotationVelocityAxis.Z * dTimeSeconds);

            Rotation = Rotation * rotationVelocity;
        }
    }

    public struct LevitationHandlingDesc
    {
        public float MaxVelocity;           // maximum player velocity
        public float MaxRotationVelocity;   // maximum player rotation velocity
        public float DampingForce;          // damping force
        public float DampingRotationForce;  // damping rotation force
    }
}
