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
        Terrain Substrate { get; }
        List<GameObject> GameObjects { get; }
        ICamera GetCamera(PlayerIndex player);

        /// <summary>
        /// Field of view in the y direction, in radians.
        /// </summary>
        float FieldOfView { get; }
        FogDesc Fog { get; }
        Space SimSpace { get; }

        List<DirectLight> GetObjectLights(ModelMesh mesh, Matrix worldTransform, Vector3 eyePosition);
        List<DirectLight> TerrainLights { get; }
        DirectLight ShadowCastingLight { get; }

        BoundingBox WorldBounds { get; }
    }
}
