using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class ParticleSystemGeometryNode : GeometryNode
    {
        public delegate void IncrementDrawCounterDelegate();

        private ParticleData mParticleData;
        private IncrementDrawCounterDelegate mIncrementDrawCounter;

        public override VertexBuffer VertexBuffer { get { return mParticleData.VertexBuffer; } }
        public override IndexBuffer IndexBuffer { get { return mParticleData.IndexBuffer; } }

        // HACK: We hide the Draw method that uses these values, so it's sort of pointless to implement them...
        protected override int mVertexOffset { get { return 0; } }
        protected override int mNumVertices { get { return 0; } }
        protected override int mStartIndex { get { return 0; } }
        protected override int mPrimitiveCount { get { return 0; } }

        public ParticleSystemGeometryNode(ParticleData particleData, IncrementDrawCounterDelegate incrementDrawCounter, EffectApplication defaultMaterial)
            : base(defaultMaterial)
        {
            mParticleData = particleData;
            mIncrementDrawCounter = incrementDrawCounter;
        }

        public override void Process(TraversalContext context)
        {
            // Choose the appropriate material
            TraversalContext.MaterialFlags materialFlags = (LocalMaterialFlags | context.ExternalMaterialFlags) & mSupportedMaterialFlags;
            EffectApplication currentMaterial = mMaterials[materialFlags & ~(materialFlags - 1)]; // Clear all but the lowest bit (highest priority flag)

            if (currentMaterial != null &&
                mParticleData.FirstActiveParticleIndex != mParticleData.FirstFreeParticleIndex)
            {
                context.AcceptForRender(this, currentMaterial);
            }
        }

        public override void Draw(RenderContext context, EffectApplication material, Matrix transform)
        {
            material.SetEffectParams(context, transform);
            for (int p = 0; p < material.Effect.CurrentTechnique.Passes.Count; ++p)
            {
                material.Effect.CurrentTechnique.Passes[p].Apply();
                if (mParticleData.FirstActiveParticleIndex < mParticleData.FirstFreeParticleIndex)
                {
                    // If the active particles are all in one consecutive range, we can draw them all in a single call.
                    if (SharedResources.GraphicsDeviceReady)
                    {
                        SharedResources.Game.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            mParticleData.FirstActiveParticleIndex * 4,
                            (mParticleData.FirstFreeParticleIndex - mParticleData.FirstActiveParticleIndex) * 4,
                            mParticleData.FirstActiveParticleIndex * 6,
                            (mParticleData.FirstFreeParticleIndex - mParticleData.FirstActiveParticleIndex) * 2);
                    }
                }
                else
                {
                    // If the active particle range wraps past the end of the queue back to the start, we must split them over two draw calls.
                    if (SharedResources.GraphicsDeviceReady)
                    {
                        SharedResources.Game.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            mParticleData.FirstActiveParticleIndex * 4,
                            (mParticleData.MaxParticles - mParticleData.FirstActiveParticleIndex) * 4,
                            mParticleData.FirstActiveParticleIndex * 6,
                            (mParticleData.MaxParticles - mParticleData.FirstActiveParticleIndex) * 2);
                    }

                    if (mParticleData.FirstFreeParticleIndex > 0 && SharedResources.GraphicsDeviceReady)
                    {
                        SharedResources.Game.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            0,
                            mParticleData.FirstFreeParticleIndex * 4,
                            0,
                            mParticleData.FirstFreeParticleIndex * 2);
                    }
                }
            }

            mIncrementDrawCounter();
        }
    }
}
