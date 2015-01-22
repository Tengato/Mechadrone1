using System;
using BEPUphysics;
using BEPUutilities;
using Manifracture;
using Microsoft.Xna.Framework.Content;
using SlagformCommon;
using BEPUphysics.Entities;
using BepuTerrain = BEPUphysics.BroadPhaseEntries.Terrain;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;
using BEPUphysics.BroadPhaseEntries;

namespace Mechadrone1
{
    class TerrainCollisionComponent : CollisionComponent
    {
        private BepuTerrain mSimTerrain;

        public override ISpaceObject SimObject { get { return mSimTerrain; } }
        protected override BroadPhaseEntry BroadPhaseEntry { get { return mSimTerrain; } }
        public override bool IsDynamic { get { return false; } }

        public TerrainCollisionComponent(Actor owner)
            : base(owner)
        {
            mSimTerrain = null;
        }

        protected override void ComponentsCreatedHandler(object sender, EventArgs e)
        {
            base.ComponentsCreatedHandler(sender, e);

            TerrainRenderComponent terrainRenderComponent = Owner.GetComponent<TerrainRenderComponent>(ComponentType.Render);
            if (terrainRenderComponent == null)
                throw new LevelManifestException("TerrainCollisionComponent expect to be accompanied by a TerrainRenderComponent.");

            float[,] heightVals = terrainRenderComponent.Heights;

            XnaVector3 originShift = new XnaVector3(terrainRenderComponent.TerrainAsset.XZScale * (terrainRenderComponent.TerrainAsset.VertexCountAlongXAxis - 1) * 0.5f,
                0.0f,
                terrainRenderComponent.TerrainAsset.XZScale * (terrainRenderComponent.TerrainAsset.VertexCountAlongZAxis - 1) * 0.5f);

            AffineTransform terrainTransform = new BEPUutilities.AffineTransform(
                new BEPUutilities.Vector3(terrainRenderComponent.TerrainAsset.XZScale, 1.0f, terrainRenderComponent.TerrainAsset.XZScale),
                BepuConverter.Convert(mTransformComponent.Orientation),
                BepuConverter.Convert(mTransformComponent.Translation - originShift));

            mSimTerrain = new BepuTerrain(heightVals, terrainTransform);
            mSimTerrain.Material.Bounciness = 0.60f;
            mSimTerrain.Material.StaticFriction = 1.0f;
            mSimTerrain.Material.KineticFriction = 1.0f;
            mSimTerrain.Tag = Owner.Id;
        }
    }
}
