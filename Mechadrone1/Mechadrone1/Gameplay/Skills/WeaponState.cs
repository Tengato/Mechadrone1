using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    abstract class WeaponState
    {
        protected long mDurationTicks;

        public abstract bool RequiresAttention { get; }

        public virtual void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
        }

        protected void ResetStateTime(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            TimeSpan extraTime = TimeSpan.FromTicks(weapon.TimeInState.Ticks - mDurationTicks);
            weapon.TimeInState = extraTime;
            weapon.CurrentState.Update(new GameTime(gameTime.TotalGameTime, extraTime, gameTime.IsRunningSlowly), weapon, input);
        }
    }
}
