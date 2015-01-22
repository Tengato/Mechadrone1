using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Skelemator;
using Manifracture;

namespace Mechadrone1
{
    class TerrainRenderComponent : RenderComponent
    {
        public Terrain TerrainAsset { get; private set; }
        private EffectApplication mDefaultMaterial;
        private EffectApplication mDepthOnlyMaterial;
        public float[,] Heights { get; private set; }

        public override SceneGraphRoot.PartitionCategories SceneGraphCategory
        {
            get { return SceneGraphRoot.PartitionCategories.Static; }
        }

        public TerrainRenderComponent(Actor owner)
            : base(owner)
        {
            TerrainAsset = null;
            mDefaultMaterial = null;
            mDepthOnlyMaterial = null;
            Heights = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            TerrainAsset = contentLoader.Load<Terrain>((string)(manifest.Properties[ManifestKeys.TERRAIN]));
            Heights = TerrainAsset.GetGeometry();

            EffectRegistry.Add(TerrainAsset.Effect, (RenderOptions)(TerrainAsset.Tag));

            mDefaultMaterial = new EffectApplication(TerrainAsset.Effect, RenderStatePresets.Default);
            mDefaultMaterial.AddParamSetter(new CommonParamSetter());
            mDefaultMaterial.AddParamSetter(new ShadowParamSetter());
            mDefaultMaterial.AddParamSetter(new HDRLightParamSetter());
            mDefaultMaterial.AddParamSetter(new FogParamSetter());
            mDepthOnlyMaterial = new EffectApplication(EffectRegistry.DepthOnlyFx, RenderStatePresets.Default);
            mDepthOnlyMaterial.AddParamSetter(new WorldViewProjParamSetter());

            base.Initialize(contentLoader, manifest);
        }

        protected override void CreateSceneGraph(ComponentManifest manifest)
        {
            int numXSectors = (TerrainAsset.VertexCountAlongXAxis - 1) / TerrainAsset.SectorSize;
            int numZSectors = (TerrainAsset.VertexCountAlongZAxis - 1) / TerrainAsset.SectorSize;

            // Caution: Being a large hierarchy of ImplicitBoundingBox nodes, the SceneGraph property's meaning becomes
            // less clear as the scene graph grows. Other objects will be placed within the tree, making it quite difficult
            // to define how to remove this component from the game. Fortunately, that should never actually be necessary.
            SceneGraph = PartitionSectorGrid(0, numXSectors, 0, numZSectors);
        }

        private SceneNode PartitionSectorGrid(int xMinIndex, int xCount, int zMinIndex, int zCount)
        {
            if (xCount == 1 && zCount == 1)
                return CreateSectorNode(xMinIndex, zMinIndex);

            SceneNode child1 = null;
            SceneNode child2 = null;

            if (xCount > zCount)
            {
                child1 = PartitionSectorGrid(
                    xMinIndex,
                    xCount / 2,
                    zMinIndex,
                    zCount);
                child2 = PartitionSectorGrid(
                    xMinIndex + xCount / 2,
                    xCount - xCount / 2,
                    zMinIndex,
                    zCount);
            }
            else
            {
                child1 = PartitionSectorGrid(
                    xMinIndex,
                    xCount,
                    zMinIndex,
                    zCount / 2);
                child2 = PartitionSectorGrid(
                    xMinIndex,
                    xCount,
                    zMinIndex + zCount / 2,
                    zCount - zCount / 2);
            }

            ImplicitBoundingBoxNode implicitChild1 = child1 as ImplicitBoundingBoxNode;
            ImplicitBoundingBoxNode implicitChild2 = child2 as ImplicitBoundingBoxNode;
            // If either child is not an implicit node, then this node must be marked as a 'leaf' node so the BVH insertion will work correctly.
            ImplicitBoundingBoxNode parentPartition = new ImplicitBoundingBoxNode(implicitChild1 == null || implicitChild2 == null);
            parentPartition.AddChild(child1);
            parentPartition.AddChild(child2);

            return parentPartition;
        }

        private SceneNode CreateSectorNode(int indexXAxis, int indexZAxis)
        {
            ActorTransformNode actorTransform = new ActorTransformNode(Owner);
            ExplicitBoundingBoxNode explicitBox = new ExplicitBoundingBoxNode(ComputeSectorBound(indexXAxis, indexZAxis));
            actorTransform.AddChild(explicitBox);
            int vertexOffset = indexXAxis * TerrainAsset.SectorSize +
                indexZAxis * TerrainAsset.SectorSize * TerrainAsset.VertexCountAlongXAxis;
            GeometryNode geometryNode = new TerrainGeometryNode(TerrainAsset, vertexOffset, mDefaultMaterial);
            explicitBox.AddChild(geometryNode);
            geometryNode.AddMaterial(TraversalContext.MaterialFlags.ShadowMap, null); //mDepthOnlyMaterial);

            return actorTransform;
        }

        private BoundingBox ComputeSectorBound(int indexXAxis, int indexZAxis)
        {
            BoundingBox result = new BoundingBox();
            int xVertIndexStart = indexXAxis * TerrainAsset.SectorSize;
            int zVertIndexStart = indexZAxis * TerrainAsset.SectorSize;
            int xVertIndexLast = xVertIndexStart + TerrainAsset.SectorSize;
            int zVertIndexLast = zVertIndexStart + TerrainAsset.SectorSize;
            float xRadius = ((float)(TerrainAsset.VertexCountAlongXAxis - 1)) / 2.0f * TerrainAsset.XZScale;
            float zRadius = ((float)(TerrainAsset.VertexCountAlongZAxis - 1)) / 2.0f * TerrainAsset.XZScale;
            result.Min.X = (float)xVertIndexStart * TerrainAsset.XZScale - xRadius;
            result.Max.X = (float)xVertIndexLast * TerrainAsset.XZScale - xRadius;
            result.Min.Z = (float)zVertIndexStart * TerrainAsset.XZScale - zRadius;
            result.Max.Z = (float)zVertIndexLast * TerrainAsset.XZScale - zRadius;
            result.Min.Y = Single.PositiveInfinity;
            result.Max.Y = Single.NegativeInfinity;

            for (int xVertIndex = xVertIndexStart; xVertIndex <= xVertIndexLast; ++xVertIndex)
            {
                for (int zVertIndex = zVertIndexStart; zVertIndex <= zVertIndexLast; ++zVertIndex)
                {
                    if (Heights[xVertIndex, zVertIndex] > result.Max.Y)
                        result.Max.Y = Heights[xVertIndex, zVertIndex];
                    if (Heights[xVertIndex, zVertIndex] < result.Min.Y)
                        result.Min.Y = Heights[xVertIndex, zVertIndex];
                }
            }

            return result;
        }

        protected override void ActorInitializedHandler(object sender, EventArgs e)
        {
            ImplicitBoundingBoxNode implicitBox = SceneGraph as ImplicitBoundingBoxNode;
            if (implicitBox != null)
                implicitBox.BottomUpRecalculateBound();

            // We shouldn't need this data anymore.
            Heights = null;

            base.ActorInitializedHandler(sender, e);
        }

        public override void Release()
        {
            throw new NotSupportedException("Cannot despawn actors that have TerrainRenderComponents.");
        }
    }
}
