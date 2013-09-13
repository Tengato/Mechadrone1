using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Mechadrone1.Gameplay;
using Manifracture;
using Skelemator;
using BEPUphysics;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1.Rendering
{
    interface IRenderableScene
    {
        ICamera GetCamera(PlayerIndex player);

        FogDesc Fog { get; }
        Space SimSpace { get; }

        List<DirectLight> GetObjectLights(ISceneObject sceneObject, Vector3 eyePosition);
        DirectLight ShadowCastingLight { get; }

        BoundingBox WorldBounds { get; }
        QuadTree QuadTree { get; }
    }
}
