using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skelemator;
using Microsoft.Xna.Framework;
using Mechadrone1.Rendering;
using Microsoft.Xna.Framework.Graphics;
using Manifracture;

namespace Mechadrone1.Gameplay
{
    class TerrainChunk
    {
        public Terrain BaseTerrain { get; private set; }

        public int XAxisSectorCount { get; private set; }
        public int ZAxisSectorCount { get; private set; }

        public Vector3 Position { get; set; }

        public Matrix SectorCoordToChunkLocalSpace { get; private set; }

        public TerrainSector[,] Sectors { get; private set; }

        private int readyBatchId;
        private Matrix world;
        private Matrix wvp;
        private Matrix wit;


        public TerrainChunk(Terrain baseTerrain)
        {
            BaseTerrain = baseTerrain;
            XAxisSectorCount = baseTerrain.VertexCountAlongXAxis / baseTerrain.SectorSize;
            ZAxisSectorCount = baseTerrain.VertexCountAlongZAxis / baseTerrain.SectorSize;

            SectorCoordToChunkLocalSpace  = Matrix.CreateScale((float)(BaseTerrain.SectorSize) / BaseTerrain.XZScale) *
                Matrix.CreateTranslation((-(float)(BaseTerrain.VertexCountAlongXAxis - 1) / 2.0f +
                    (float)(BaseTerrain.SectorSize) / 2.0f) * BaseTerrain.XZScale,
                    0.0f,
                    (-(float)(BaseTerrain.VertexCountAlongZAxis - 1) / 2.0f +
                    (float)(BaseTerrain.SectorSize) / 2.0f) * BaseTerrain.XZScale);

            Sectors = new TerrainSector[XAxisSectorCount, ZAxisSectorCount];

            for (int i = 0; i < XAxisSectorCount; i++)
            {
                for (int j = 0; j < ZAxisSectorCount; j++)
                {
                    Sectors[i, j] = new TerrainSector(this, i, j);
                }
            }

            readyBatchId = -1;
        }


        public void ReadyEffectParams(
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
            if (batchId == readyBatchId)
                return;

            world = Matrix.CreateTranslation(Position);

            wvp = step == RenderStep.Shadows ?
                world * shadowCastingLightView * shadowCastingLightProjection :
                world * view * projection;
            wit = Matrix.Transpose(Matrix.Invert(world));

            readyBatchId = batchId;
        }


        public virtual void DrawShadow(RenderEntry re)
        {
            EffectRegistry.DOWorldViewProj.SetValue(wvp);

            re.Pass.Apply();
            re.Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                re.VertexOffset, 0, re.NumVertices, 0, re.PrimitiveCount);
        }


        public virtual void Draw(RenderEntry re)
        {
            EffectRegistry.Params[BaseTerrain.Effect][EffectRegistry.SHADOWTRANSFORM_PARAM_NAME].SetValue(
                world * re.ShadowCastingLightView * re.ShadowCastingLightProjection * SceneManager.NDC_TO_TEXCOORDS);
            EffectRegistry.Params[BaseTerrain.Effect][EffectRegistry.SHADOWMAP_PARAM_NAME].SetValue(re.ShadowMap);
            EffectRegistry.Params[BaseTerrain.Effect][EffectRegistry.WORLD_PARAM_NAME].SetValue(world);
            EffectRegistry.Params[BaseTerrain.Effect][EffectRegistry.WORLDVIEWPROJ_PARAM_NAME].SetValue(wvp);
            EffectRegistry.Params[BaseTerrain.Effect][EffectRegistry.WORLDINVTRANSPOSE_PARAM_NAME].SetValue(wit);
            EffectRegistry.Params[BaseTerrain.Effect][EffectRegistry.EYEPOSITION_PARAM_NAME].SetValue(re.CameraTransform.Translation);
            EffectRegistry.SetLighting(re.Lights, re.Effect);

            re.Pass.Apply();
            re.Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                re.VertexOffset, 0, re.NumVertices, 0, re.PrimitiveCount);
        }


        public void Initialize()
        {
            if (!EffectRegistry.Params.Keys.Contains(BaseTerrain.Effect))
                EffectRegistry.Add(BaseTerrain.Effect, (RenderOptions)(BaseTerrain.Tag));
        }
    }
}
