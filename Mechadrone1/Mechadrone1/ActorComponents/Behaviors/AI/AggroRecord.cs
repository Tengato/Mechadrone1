using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class AggroRecord
    {
        public const float NEW_FOE_ENMITY = 10.0f;
        public const float ENMITY_PER_DAMAGE = 0.1f;
        public const float ENMITY_FADE_PER_SEC = 0.9f;

        // This will be in a dictionary with the ActorId as the key.
        // TimeSpan values represent total game time elapsed since the start of the game to the event.
        public TimeSpan TimeLastSensed { get; set; } // This could be from touch, sound, damage, link, etc.
        public TimeSpan TimeBecameVisible { get; set; }
        public TimeSpan TimeLastVisible { get; set; }
        public Vector3 PositionLastSensed { get; set; }
        public float Enmity { get; set; }
    }
}
