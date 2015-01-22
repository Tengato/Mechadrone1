#region File Description
//-----------------------------------------------------------------------------
// GameOptions.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Mechadrone1
{
    class GameOptions
    {
        // game screen horizontal resolution
        public static int        ScreenWidth                   = 1200;
        // game screen vertical resolution
        public static int ScreenHeight = 675;

        // glow buffer resolution
        public static int        GlowResolution                = 512;

        // maximum number of supported players
        public static int        MaxPlayers                    = 2;
        // max points (kills) to end game
        public static int        MaxPoints                     = 10;

        // how many octree subdivisions in collision mesh
        public static uint       CollisionMeshSubdivisions     = 4;

        public static int NumberOfTerrainVertexCols = 513;
        public static float GroundRadius = 256.0f;
        
        // size of player collision box
        public static int        CollisionBoxRadius            = 60;

        // maximum bones per model
        public static int        MaxBonesPerModel              = 128;
        
        // inpulse force when two ships collide
        public static float      ShipCollidePush               = 500;

        // use game pade vibrate?
        public static bool       UseGamepadVibrate             = true;
        // gamepad vibration fadeout time
        public static float      VibrationFadeout              = 0.1f;
        // gamepad vibration intensity
        public static float      VibrationIntensity            = 0.5f;

        // max simultaneous particles per frame
        public static int        MaxParticles                  = 8192;
        // max simultaneous animated sprites per frame
        public static int        MaxSprites                    = 128;

        // color used for screen transitions
        public static Vector4    FadeColor                     = Vector4.Zero;
        // time for screen transition in seconds
        public static float      FadeTime                      = 1.0f;

        // time shield is active
        public static float      ShieldUse                     = 2.0f;
        // time for shield recharge
        public static float      ShieldRecharge                = 8.0f;

        // time boost is active
        public static float      BoostUse                      = 2.0f;
        // time for boost recgarge
        public static float      BoostRecharge                 = 8.0f;
        // how fast boost slows down after finished
        public static float      BoostSlowdown                 = 1000.0f;
        // force to apply forward when using boost
        public static float      BoostForce                    = 50.0f;

        // fadeout time for damage effect
        public static float      DamageFadeout                 = 0.5f;
        // timeout before you respawn after a kill
        public static float      DeathTimeout                  = 3.0f;

        // bobbing distance
        public static float      ShipBobbingRange              = 4.0f;
        // bobbing speed
        public static float      ShipBobbingSpeed              = 4.0f;

        // time between two blasters fire
        public static float      BlasterChargeTime             = 0.2f;
        // time between two missiles fire
        public static float      MissileChargeTime             = 0.5f;
        // blaster velocity
        public static float      BlasterVelocity               = 6000;
        // missile velocity
        public static float      MissileVelocity               = 4000;

        // stiffness for camera in 3rd person mode
        public static float      CameraStiffness               = 15000;
        // damping for camera in 3rd person mode
        public static float      CameraDamping                 = 1000;
        // mass for camera in 3rd person mode
        public static float      CameraMass                    = 25;
        // offset for missile trail
        public static Vector3    MissileTrailOffset            = new Vector3(0, 0, -10);

        // powerups rotation speed
        public static float      PowerupTurnSpeed              = 2.0f;
        // up/down powerup movement speed 
        public static float      PowerupMoveSpeed              = 4.0f;
        // up/down powerup movement distance
        public static float      PowerupMoveDistance           = 4.0f;
        // time for powerup respawn afetr picked up
        public static float      PowerupRespawnTime            = 5.0f;

        // max ship velocity
        public static float      MovementVelocity              = 300;
        // max ship velocity with boost activated
        public static float      MovementVelocityBoost         = 1200;
        // force applied by controls to move ship
        public static float      MovementForce                 = 300;
        // damping force used to stop movemnt 
        public static float      MovementForceDamping          = 750;
        // max rotation velocity
        public static float      MovementRotationVelocity      = 1.1f;
        // rotation force applied by controls to rotate ship
        public static float      MovementRotationForce         = 5.0f;
        // damping force used to stop rotation
        public static float      MovementRotationForceDamping  = 3.0f;

        // Screen split style preference when there are two local players.
        public static bool PreferHorizontalSplit { get; set; }
    }
}
