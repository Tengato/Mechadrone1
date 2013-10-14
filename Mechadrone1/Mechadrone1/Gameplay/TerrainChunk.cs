using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skelemator;
using Microsoft.Xna.Framework;
using Mechadrone1.Rendering;
using Microsoft.Xna.Framework.Graphics;
using Manifracture;
using BepuTerrain = BEPUphysics.BroadPhaseEntries.Terrain;
using SlagformCommon;

namespace Mechadrone1.Gameplay
{
    class TerrainChunk
    {
        public Terrain BaseTerrain { get; private set; }
        public bool CastsShadow { get; private set; }

        public int XAxisSectorCount { get; private set; }
        public int ZAxisSectorCount { get; private set; }

        public Vector3 Position { get; private set; }

        public Matrix SectorCoordToChunkLocalSpace { get; private set; }

        public TerrainSector[,] Sectors { get; private set; }

        public BepuTerrain SimulationObject { get; private set; }

        private int readyBatchId;
        private Matrix world;
        private Matrix wvp;
        private Matrix wit;


        public TerrainChunk(Terrain baseTerrain, Vector3 position, bool castsShadow)
        {
            BaseTerrain = baseTerrain;
            Position = position;
            CastsShadow = castsShadow;

            XAxisSectorCount = baseTerrain.VertexCountAlongXAxis / baseTerrain.SectorSize;
            ZAxisSectorCount = baseTerrain.VertexCountAlongZAxis / baseTerrain.SectorSize;

            SectorCoordToChunkLocalSpace = Matrix.CreateScale((float)(BaseTerrain.SectorSize) * BaseTerrain.XZScale) *
                Matrix.CreateTranslation((-(float)(BaseTerrain.VertexCountAlongXAxis - 1) * BaseTerrain.XZScale / 2.0f +
                    (float)(BaseTerrain.SectorSize) * BaseTerrain.XZScale / 2.0f),
                    0.0f,
                    (-(float)(BaseTerrain.VertexCountAlongZAxis - 1) * BaseTerrain.XZScale / 2.0f +
                    (float)(BaseTerrain.SectorSize) * BaseTerrain.XZScale / 2.0f));

            float[,] heightVals = BaseTerrain.GetGeometry();

            Sectors = new TerrainSector[XAxisSectorCount, ZAxisSectorCount];

            for (int i = 0; i < XAxisSectorCount; i++)
            {
                for (int j = 0; j < ZAxisSectorCount; j++)
                {
                    float minHeight = 1.0e10f;
                    float maxHeight = -1.0e10f;

                    for (int m = i * BaseTerrain.SectorSize; m <= (i + 1) * BaseTerrain.SectorSize; m++)
                    {
                        for (int n = j * BaseTerrain.SectorSize; n <= (j + 1) * BaseTerrain.SectorSize; n++)
                        {
                            if (heightVals[m, n] < minHeight)
                                minHeight = heightVals[m, n];
                            if (heightVals[m, n] > maxHeight)
                                maxHeight = heightVals[m, n];
                        }
                    }

                    minHeight += Position.Y;
                    maxHeight += Position.Y;

                    Sectors[i, j] = new TerrainSector(this, i, j, minHeight, maxHeight);
                    Sectors[i, j].CastsShadow = CastsShadow;
                }
            }

            if (!EffectRegistry.Params.Keys.Contains(BaseTerrain.Effect))
                EffectRegistry.Add(BaseTerrain.Effect, (RenderOptions)(BaseTerrain.Tag));

            readyBatchId = -1;

            SimulationObject = new BepuTerrain(heightVals,
                new BEPUutilities.AffineTransform(new BEPUutilities.Vector3(BaseTerrain.XZScale, 1.0f, BaseTerrain.XZScale),
                BEPUutilities.Quaternion.Identity,
                BepuConverter.Convert(Position + BaseTerrain.TransformForGeometry)));
            SimulationObject.Material.Bounciness = 0.60f;
            SimulationObject.Material.StaticFriction = 1.0f;
            SimulationObject.Material.KineticFriction = 1.0f;
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
        }
    }
}
