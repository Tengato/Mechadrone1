using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Skelemator;
using Manifracture;

namespace Mechadrone1.Gameplay
{
    class TerrainSector : ISceneObject
    {
        private TerrainChunk parent;
        private int xIndex;
        private int zIndex;

        public Vector3 Position
        {
            get
            {
                return Vector3.Transform(new Vector3((float)xIndex, 0.0f, (float)zIndex),
                    parent.SectorCoordToChunkLocalSpace) +
                    parent.Position;
            }
        }

        public bool CastsShadow { get; set; }

        public BoundingBox WorldSpaceBoundingBox { get; set; }

        private QuadTree quadTree;
        public QuadTree QuadTree 
        {
            get
            {
                return quadTree;
            }
            set
            {
                quadTree = value;
                QuadTreeBoundingBox = QuadTreeRect.CreateFromBoundingBox(WorldSpaceBoundingBox, quadTree.WorldToQuadTreeTransform);
            }
        }

        public QuadTreeRect QuadTreeBoundingBox { get; private set; }

        public QuadTreeNode QuadTreeNode { get; set; }


        public TerrainSector(TerrainChunk parent, int XIndex, int ZIndex, float minHeight, float maxHeight)
        {
            this.parent = parent;
            xIndex = XIndex;
            zIndex = ZIndex;

            Vector3 XZRadius = new Vector3(
                (float)(parent.BaseTerrain.SectorSize) * parent.BaseTerrain.XZScale / 2.0f,
                0.0f,
                (float)(parent.BaseTerrain.SectorSize) * parent.BaseTerrain.XZScale / 2.0f);

            WorldSpaceBoundingBox = new BoundingBox(Position - XZRadius + Vector3.Up * minHeight, Position + XZRadius + Vector3.Up * maxHeight);
        }


        public List<RenderEntry> GetRenderEntries(
            int batchId,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightProjection,
            RenderTarget2D shadowMap,
            List<DirectLight> lights)
        {
            parent.ReadyEffectParams(
                batchId,
                step,
                view,
                projection,
                cameraTransform,
                shadowCastingLightView,
                shadowCastingLightProjection,
                shadowMap,
                lights);

            List<RenderEntry> results = new List<RenderEntry>();

            RenderEntry re = new RenderEntry();

            re.VertexBuffer = parent.BaseTerrain.Vertices;
            re.NumVertices = parent.BaseTerrain.SectorSize * parent.BaseTerrain.VertexCountAlongXAxis + parent.BaseTerrain.SectorSize + 1;
            re.IndexBuffer = parent.BaseTerrain.Indices;
            re.VertexOffset = xIndex * parent.BaseTerrain.SectorSize +
                            zIndex * parent.BaseTerrain.SectorSize * parent.BaseTerrain.VertexCountAlongXAxis;
            re.StartIndex = 0;
            re.RenderOptions = RenderOptions.None;
            re.PrimitiveCount = 2 * parent.BaseTerrain.SectorSize * parent.BaseTerrain.SectorSize;

            re.View = view;
            re.Projection = projection;
            re.ShadowCastingLightView = shadowCastingLightView;
            re.ShadowCastingLightProjection = shadowCastingLightProjection;
            re.ShadowMap = shadowMap;
            re.CameraTransform = cameraTransform;
            re.SceneObject = this;
            re.Lights = lights;

            switch (step)
            {
                case RenderStep.Default:
                    re.Effect = parent.BaseTerrain.Effect;
                    re.DrawCallback = parent.Draw;
                    break;
                case RenderStep.Shadows:
                    re.Effect = EffectRegistry.DepthOnlyFx;
                    re.DrawCallback = parent.DrawShadow;
                    break;
                default:
                    throw new NotImplementedException();
            }

            re.Pass = re.Effect.CurrentTechnique.Passes[0];
            results.Add(re);

            // Make additional copies of the render entry for multi-pass techniques:
            for (int i = 1; i < re.Effect.CurrentTechnique.Passes.Count; i++)
            {
                RenderEntry reCopy = new RenderEntry(re);
                reCopy.Pass = re.Effect.CurrentTechnique.Passes[i];
                results.Add(reCopy);
            }

            return results;
        }

    }
}
