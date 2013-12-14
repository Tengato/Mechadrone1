using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Gameplay.Prefabs
{
    abstract class WeaponState
    {
        protected long durationTicks;

        public virtual void Update(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
        }

        protected void ResetStateTime(GameTime gameTime, BipedWeapon weapon, WeaponFunctions input)
        {
            TimeSpan extraTime = TimeSpan.FromTicks(weapon.TimeInState.Ticks - durationTicks);
            weapon.TimeInState = extraTime;
            weapon.CurrentState.Update(new GameTime(gameTime.TotalGameTime, extraTime, gameTime.IsRunningSlowly), weapon, input);
        }
    }
}
