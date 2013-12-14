using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using Mechadrone1.Gameplay.Prefabs;

namespace Mechadrone1.Gameplay
{
    interface IGameManager
    {
        GameObject GetGameObject(string name);
        Dictionary<PlayerIndex, GameObject> Avatars { get; }
        event UpdateStepEventHandler PreAnimationUpdateStep;
        event UpdateStepEventHandler AnimationUpdateStep;
        event UpdateStepEventHandler PostPhysicsUpdateStep;
        event UpdateStepEventHandler BotControlUpdateStep;
        Space SimSpace { get; }
        List<Axes> DebugAxes { get; }
        GameObjectLoader Builder { get; }
        void SpawnInitializedObject(GameObject spawn);
        List<GameObject> DespawnList { get; }
    }
}
