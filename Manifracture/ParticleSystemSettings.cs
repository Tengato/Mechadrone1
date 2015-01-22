using System;
using Microsoft.Xna.Framework;
using Skelemator;

namespace Manifracture
{
    public class ParticleSystemSettings
    {
        // Name of the texture used by this particle system.
        public string TextureName { get; set; }

        // Maximum number of particles that can be displayed at one time.
        public int MaxParticles { get; set; }

        // How long these particles will last.
        public TimeSpan Duration { get; set; }

        // If greater than zero, some particles will last a shorter time than others.
        public float DurationRandomness { get; set; }

        // Controls how much particles are influenced by the velocity of the object
        // which created them. You can see this in action with the explosion effect,
        // where the flames continue to move in the same direction as the source
        // projectile. The projectile trail particles, on the other hand, set this
        // value very low so they are less affected by the velocity of the projectile.
        public float EmitterVelocitySensitivity { get; set; }

        // Range of values controlling how much X and Z axis velocity to give each
        // particle. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        public float MinHorizontalVelocity { get; set; }
        public float MaxHorizontalVelocity { get; set; }

        // Range of values controlling how much Y axis velocity to give each particle.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        public float MinVerticalVelocity { get; set; }
        public float MaxVerticalVelocity { get; set; }

        // Direction and strength of the gravity effect. Note that this can point in any
        // direction, not just down! The fire effect points it upward to make the flames
        // rise, and the smoke plume points it sideways to simulate wind.
        public Vector3 Gravity { get; set; }

        // Controls how the particle velocity will change over their lifetime. If set
        // to 1, particles will keep going at the same speed as when they were created.
        // If set to 0, particles will come to a complete stop right before they die.
        // Values greater than 1 make the particles speed up over time.
        public float EndVelocity { get; set; }

        // Range of values controlling the particle color and alpha. Values for
        // individual particles are randomly chosen from somewhere between these limits.
        public Color MinColor { get; set; }
        public Color MaxColor { get; set; }

        // Range of values controlling how fast the particles rotate. Values for
        // individual particles are randomly chosen from somewhere between these
        // limits. If both these values are set to 0, the particle system will
        // automatically switch to an alternative shader technique that does not
        // support rotation, and thus requires significantly less GPU power. This
        // means if you don't need the rotation effect, you may get a performance
        // boost from leaving these values at 0.
        public float MinRotateSpeed { get; set; }
        public float MaxRotateSpeed { get; set; }

        // Range of values controlling how big the particles are when first created.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        public float MinStartSize { get; set; }
        public float MaxStartSize { get; set; }

        // Range of values controlling how big particles become at the end of their
        // life. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        public float MinEndSize { get; set; }
        public float MaxEndSize { get; set; }

        // Alpha blending settings.
        public RenderStatePresets RenderState { get; set; }

        public ParticleSystemSettings()
        {
            TextureName = null;
            MaxParticles = 100;
            Duration = TimeSpan.FromSeconds(1);
            DurationRandomness = 0;
            EmitterVelocitySensitivity = 1;
            MinHorizontalVelocity = 0;
            MaxHorizontalVelocity = 0;
            MinVerticalVelocity = 0;
            MaxVerticalVelocity = 0;
            Gravity = Vector3.Zero;
            EndVelocity = 1;
            MinColor = Color.White;
            MaxColor = Color.White;
            MinRotateSpeed = 0;
            MaxRotateSpeed = 0;
            MinStartSize = 100;
            MaxStartSize = 100;
            MinEndSize = 100;
            MaxEndSize = 100;
            RenderState = RenderStatePresets.AlphaBlend;
        }
    }
}
