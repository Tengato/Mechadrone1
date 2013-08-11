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
        public Vector3 position;       // player position
        public Vector3 velocity;       // velocity in local player space
        public Vector3 force;          // forces in local player space

        // player rotation
        public Matrix rotation;                 
        // rotation velocities around each local player axis
        public Vector3 rotationVelocityAxis;
        // rotation forces around each local player axis
        public Vector3 rotationForce;

        FloatMovementHandlingDesc handling;

        /// <summary>
        /// Create a new player movement object for handling player motion
        /// </summary>
        public FloatMovement(FloatMovementHandlingDesc handlingDesc)
        {
            position = Vector3.Zero;
            velocity = Vector3.Zero;
            force = Vector3.Zero;

            rotation = Matrix.Identity;
            rotationVelocityAxis = Vector3.Zero;
            rotationForce = Vector3.Zero;

            handling = handlingDesc;
        }

        /// <summary>
        /// Resets the position and rotation of the player and zero forces
        /// </summary>
        public void Reset(Matrix transfrom)
        {
            rotation = transfrom;
            position = transfrom.Translation;

            velocity = Vector3.Zero;
            force = Vector3.Zero;
            
            rotationVelocityAxis = Vector3.Zero;
            rotationForce = Vector3.Zero;
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
                transform = rotation;

                // set translation
                transform.Translation = position;

                return transform;
            }
        }

        /// <summary>
        /// Get the normalized velocity
        /// </summary>
        public float VelocityFactor
        {
            get { return velocity.Length() / handling.MaxVelocity; }
        }

        /// <summary>
        /// Get/Set the velocity vector transformed to world space
        /// </summary>
        public Vector3 WorldVelocity
        {
            // transform local velocity to world space
            get
            {
                return velocity.X * rotation.Right +
                velocity.Y * rotation.Up + velocity.Z * rotation.Forward;
            }
            set
            {
                // transform world velocity into local space
                velocity.X = Vector3.Dot(rotation.Right, value);
                velocity.Y = Vector3.Dot(rotation.Up, value);
                velocity.Z = Vector3.Dot(rotation.Forward, value);
            }
        }

        /// <summary>
        /// Process movement input
        /// </summary>
        public void ProcessInput(float elapsedTime, InputState current, int player)
        {
            // camera rotation
            rotationForce.X =
                handling.InputRotationForce * current.PadState[player].ThumbSticks.Right.Y;
            rotationForce.Y =
                -handling.InputRotationForce * current.PadState[player].ThumbSticks.Right.X;
            rotationForce.Z = 0.0f;

            // camera bank
            if (current.PadState[player].Buttons.RightShoulder == ButtonState.Pressed)
                rotationForce.Z += handling.InputRotationForce;
            if (current.PadState[player].Buttons.LeftShoulder == ButtonState.Pressed)
                rotationForce.Z -= handling.InputRotationForce;

            // move forward/backward
            force.X = handling.InputForce * current.PadState[player].ThumbSticks.Left.X;

            if (current.PadState[player].Buttons.RightStick == ButtonState.Pressed)
            {
                // slide up/down
                force.Y = handling.InputForce * current.PadState[player].ThumbSticks.Left.Y;
                force.Z = 0.0f;
            }
            else
            {
                // slide left/right
                force.Y = 0.0f;
                force.Z = handling.InputForce * current.PadState[player].ThumbSticks.Left.Y;
            }

            // keyboard camera rotation
            if (current.KeyState[player].IsKeyDown(Keys.Down))
                rotationForce.X = handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.Up))
                rotationForce.X = -handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.Left))
                rotationForce.Y = handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.Right))
                rotationForce.Y = -handling.InputRotationForce;
            // keyboard camera bank
            if (current.KeyState[player].IsKeyDown(Keys.Q))
                rotationForce.Z = -handling.InputRotationForce;
            if (current.KeyState[player].IsKeyDown(Keys.E))
                rotationForce.Z = handling.InputRotationForce;
            // move forward/backward
            if (current.KeyState[player].IsKeyDown(Keys.W))
                force.Z = handling.InputForce;
            if (current.KeyState[player].IsKeyDown(Keys.S))
                force.Z = -handling.InputForce;
            // slide left/right
            if (current.KeyState[player].IsKeyDown(Keys.A))
                force.X = -handling.InputForce / 2.0f;
            if (current.KeyState[player].IsKeyDown(Keys.D))
                force.X = handling.InputForce / 2.0f;
            // slide up/down
            if (current.KeyState[player].IsKeyDown(Keys.X))
                force.Y = -handling.InputForce / 2.0f;
            if (current.KeyState[player].IsKeyDown(Keys.D2))
                force.Y = handling.InputForce / 2.0f;
        }

        public void Update(float elapsedTime)
        {
            // apply force
            velocity += force * elapsedTime;

            // apply damping
            if (force.X > -0.001f && force.X < 0.001f)
                if (velocity.X > 0)
                    velocity.X = Math.Max(0.0f, velocity.X - handling.DampingForce * elapsedTime);
                else
                    velocity.X = Math.Min(0.0f, velocity.X + handling.DampingForce * elapsedTime);
            if (force.Y > -0.001f && force.Y < 0.001f)
                if (velocity.Y > 0)
                    velocity.Y = Math.Max(0.0f, velocity.Y - handling.DampingForce * elapsedTime);
                else
                    velocity.Y = Math.Min(0.0f, velocity.Y + handling.DampingForce * elapsedTime);
            if (force.Z > -0.001f && force.Z < 0.001f)
                if (velocity.Z > 0)
                    velocity.Z = Math.Max(0.0f, velocity.Z - handling.DampingForce * elapsedTime);
                else
                    velocity.Z = Math.Min(0.0f, velocity.Z + handling.DampingForce * elapsedTime);

            // crop with maximum velocity
            float velocityLength = velocity.Length();
            if (velocityLength > handling.MaxVelocity)
                velocity = Vector3.Normalize(velocity) * handling.MaxVelocity;

            // apply velocity
            position += rotation.Right * velocity.X * elapsedTime;
            position += rotation.Up * velocity.Y * elapsedTime;
            position += rotation.Forward * velocity.Z * elapsedTime;
            
            // apply rot force
            rotationVelocityAxis += rotationForce * elapsedTime;

            // apply rot damping
            if (rotationForce.X > -0.001f && rotationForce.X < 0.001f)
                if (rotationVelocityAxis.X > 0)
                    rotationVelocityAxis.X = Math.Max(0.0f, 
                                    rotationVelocityAxis.X -
                                    handling.DampingRotationForce * elapsedTime);
                else
                    rotationVelocityAxis.X = Math.Min(0.0f, 
                                    rotationVelocityAxis.X +
                                    handling.DampingRotationForce * elapsedTime);
            
            if (rotationForce.Y > -0.001f && rotationForce.Y < 0.001f)
                if (rotationVelocityAxis.Y > 0)
                    rotationVelocityAxis.Y = Math.Max(0.0f, 
                                    rotationVelocityAxis.Y -
                                    handling.DampingRotationForce * elapsedTime);
                else
                    rotationVelocityAxis.Y = Math.Min(0.0f, 
                                    rotationVelocityAxis.Y +
                                    handling.DampingRotationForce * elapsedTime);
            
            if (rotationForce.Z > -0.001f && rotationForce.Z < 0.001f)
                if (rotationVelocityAxis.Z > 0)
                    rotationVelocityAxis.Z = Math.Max(0.0f, 
                                    rotationVelocityAxis.Z -
                                    handling.DampingRotationForce * elapsedTime);
                else
                    rotationVelocityAxis.Z = Math.Min(0.0f, 
                                    rotationVelocityAxis.Z +
                                    handling.DampingRotationForce * elapsedTime);

            // crop with maximum rot velocity
            float rotationVelocityLength = rotationVelocityAxis.Length();
            if (rotationVelocityLength > handling.MaxRotationVelocity)
                rotationVelocityAxis = Vector3.Normalize(rotationVelocityAxis) *
                    handling.MaxRotationVelocity;

            // apply rot vel
            Matrix rotationVelocity = Matrix.Identity;

            if (rotationVelocityAxis.X < -0.001f || rotationVelocityAxis.X > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(rotation.Right, 
                    rotationVelocityAxis.X * elapsedTime);

            if (rotationVelocityAxis.Y < -0.001f || rotationVelocityAxis.Y > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(rotation.Up, 
                    rotationVelocityAxis.Y * elapsedTime);

            if (rotationVelocityAxis.Z < -0.001f || rotationVelocityAxis.Z > 0.001f)
                rotationVelocity = rotationVelocity * 
                    Matrix.CreateFromAxisAngle(rotation.Forward, 
                    rotationVelocityAxis.Z * elapsedTime);

            rotation = rotation * rotationVelocity;
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
