#region File Description
//-----------------------------------------------------------------------------
// PlayerMovement.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion



namespace Mechadrone1.Gameplay.Helpers
{
    class FloatMovement
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

        FloatMovementHandlingDesc handling;

        /// <summary>
        /// Create a new player movement object for handling player motion
        /// </summary>
        public FloatMovement(FloatMovementHandlingDesc handlingDesc)
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

        /// <summary>
        /// Process movement input
        /// </summary>
        public void ProcessInput(float elapsedTime, InputState current, int player)
        {
            // camera rotation
            RotationForce.X =
                handling.InputRotationForce * current.PadState[player].ThumbSticks.Right.Y;
            RotationForce.Y =
                -handling.InputRotationForce * current.PadState[player].ThumbSticks.Right.X;
            RotationForce.Z = 0.0f;

            // camera bank
            if (current.PadState[player].Buttons.RightShoulder == ButtonState.Pressed)
                RotationForce.Z += handling.InputRotationForce;
            if (current.PadState[player].Buttons.LeftShoulder == ButtonState.Pressed)
                RotationForce.Z -= handling.InputRotationForce;

            // move forward/backward
            Force.X = handling.InputForce * current.PadState[player].ThumbSticks.Left.X;

            if (current.PadState[player].Buttons.RightStick == ButtonState.Pressed)
            {
                // slide up/down
                Force.Y = handling.InputForce * current.PadState[player].ThumbSticks.Left.Y;
                Force.Z = 0.0f;
            }
            else
            {
                // slide left/right
                Force.Y = 0.0f;
                Force.Z = handling.InputForce * current.PadState[player].ThumbSticks.Left.Y;
            }

            // keyboard camera rotation
            if (current.KeyState[player].IsKeyDown(Keys.Down))
                RotationForce.X = handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.Up))
                RotationForce.X = -handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.Left))
                RotationForce.Y = handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.Right))
                RotationForce.Y = -handling.InputRotationForce;
            // keyboard camera bank
            if (current.KeyState[player].IsKeyDown(Keys.Q))
                RotationForce.Z = -handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.E))
                RotationForce.Z = handling.InputRotationForce;
            // move forward/backward
            if (current.KeyState[player].IsKeyDown(Keys.W))
                Force.Z = handling.InputForce;
            if (current.KeyState[player].IsKeyDown(Keys.S))
                Force.Z = -handling.InputForce;
            // slide left/right
            if (current.KeyState[player].IsKeyDown(Keys.A))
                Force.X = -handling.InputForce / 2.0f;
            if (current.KeyState[player].IsKeyDown(Keys.D))
                Force.X = handling.InputForce / 2.0f;
            // slide up/down
            if (current.KeyState[player].IsKeyDown(Keys.X))
                Force.Y = -handling.InputForce / 2.0f;
            if (current.KeyState[player].IsKeyDown(Keys.D2))
                Force.Y = handling.InputForce / 2.0f;
        }

        public void Update(float elapsedTime)
        {
            // apply force
            Velocity += Force * elapsedTime;

            // apply damping
            if (Force.X > -0.001f && Force.X < 0.001f)
                if (Velocity.X > 0)
                    Velocity.X = Math.Max(0.0f, Velocity.X - handling.DampingForce * elapsedTime);
                else
                    Velocity.X = Math.Min(0.0f, Velocity.X + handling.DampingForce * elapsedTime);
            if (Force.Y > -0.001f && Force.Y < 0.001f)
                if (Velocity.Y > 0)
                    Velocity.Y = Math.Max(0.0f, Velocity.Y - handling.DampingForce * elapsedTime);
                else
                    Velocity.Y = Math.Min(0.0f, Velocity.Y + handling.DampingForce * elapsedTime);
            if (Force.Z > -0.001f && Force.Z < 0.001f)
                if (Velocity.Z > 0)
                    Velocity.Z = Math.Max(0.0f, Velocity.Z - handling.DampingForce * elapsedTime);
                else
                    Velocity.Z = Math.Min(0.0f, Velocity.Z + handling.DampingForce * elapsedTime);

            // crop with maximum velocity
            float velocityLength = Velocity.Length();
            if (velocityLength > handling.MaxVelocity)
                Velocity = Vector3.Normalize(Velocity) * handling.MaxVelocity;

            // apply velocity
            Position += Rotation.Right * Velocity.X * elapsedTime;
            Position += Rotation.Up * Velocity.Y * elapsedTime;
            Position += Rotation.Forward * Velocity.Z * elapsedTime;
            
            // apply rot force
            RotationVelocityAxis += RotationForce * elapsedTime;

            // apply rot damping
            if (RotationForce.X > -0.001f && RotationForce.X < 0.001f)
                if (RotationVelocityAxis.X > 0)
                    RotationVelocityAxis.X = Math.Max(0.0f, 
                                    RotationVelocityAxis.X -
                                    handling.DampingRotationForce * elapsedTime);
                else
                    RotationVelocityAxis.X = Math.Min(0.0f, 
                                    RotationVelocityAxis.X +
                                    handling.DampingRotationForce * elapsedTime);
            
            if (RotationForce.Y > -0.001f && RotationForce.Y < 0.001f)
                if (RotationVelocityAxis.Y > 0)
                    RotationVelocityAxis.Y = Math.Max(0.0f, 
                                    RotationVelocityAxis.Y -
                                    handling.DampingRotationForce * elapsedTime);
                else
                    RotationVelocityAxis.Y = Math.Min(0.0f, 
                                    RotationVelocityAxis.Y +
                                    handling.DampingRotationForce * elapsedTime);
            
            if (RotationForce.Z > -0.001f && RotationForce.Z < 0.001f)
                if (RotationVelocityAxis.Z > 0)
                    RotationVelocityAxis.Z = Math.Max(0.0f, 
                                    RotationVelocityAxis.Z -
                                    handling.DampingRotationForce * elapsedTime);
                else
                    RotationVelocityAxis.Z = Math.Min(0.0f, 
                                    RotationVelocityAxis.Z +
                                    handling.DampingRotationForce * elapsedTime);

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
                    RotationVelocityAxis.X * elapsedTime);

            if (RotationVelocityAxis.Y < -0.001f || RotationVelocityAxis.Y > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(Rotation.Up, 
                    RotationVelocityAxis.Y * elapsedTime);

            if (RotationVelocityAxis.Z < -0.001f || RotationVelocityAxis.Z > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(Rotation.Forward, 
                    RotationVelocityAxis.Z * elapsedTime);

            Rotation = Rotation * rotationVelocity;
        }
    }

    public struct FloatMovementHandlingDesc
    {
        public float MaxVelocity;           // maximum player velocity
        public float MaxRotationVelocity;   // maximum player rotation velocity
        public float DampingForce;          // damping force
        public float DampingRotationForce;  // damping rotation force
        public float InputForce;            // maximum force created by input stick
        public float InputRotationForce;    // maximum rotation force created by input stick
    }
}
