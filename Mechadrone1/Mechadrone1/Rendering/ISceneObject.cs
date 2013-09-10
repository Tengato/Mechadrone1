using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Manifracture;

namespace Mechadrone1.Rendering
{
    interface ISceneObject
    {
        Vector3 Position { get; }
        bool CastsShadow { get; }
        QuadTree QuadTree { get; set; }
        QuadTreeRect QuadTreeBoundingBox { get; }
        QuadTreeNode QuadTreeNode { get; set; }
        List<RenderEntry> GetRenderEntries(
            int batchId,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightProjection,
            RenderTarget2D shadowMap,
            List<DirectLight> lights);
        BoundingBox WorldSpaceBoundingBox { get; }
    }
}
