using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;

namespace Mechadrone1.Gameplay
{
    interface IGameManager
    {
        GameObject GetGameObject(string name);
        Dictionary<PlayerIndex, GameObject> Avatars { get; }
        event PreAnimationUpdateEventHandler PreAnimationUpdateStep;
        event PostPhysicsUpdateEventHandler PostPhysicsUpdateStep;
        Space SimSpace { get; }
    }
}
