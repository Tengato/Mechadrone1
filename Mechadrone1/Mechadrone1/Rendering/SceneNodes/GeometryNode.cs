using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1
{
    abstract class GeometryNode : SceneNode
    {
        protected Dictionary<TraversalContext.MaterialFlags, EffectApplication> mMaterials { get; private set; }
        public TraversalContext.MaterialFlags LocalMaterialFlags { get; set; } // This value can be used to toggle per-object effects.
        protected TraversalContext.MaterialFlags mSupportedMaterialFlags;

        public abstract VertexBuffer VertexBuffer { get; }
        public abstract IndexBuffer IndexBuffer { get; }
        protected abstract int mVertexOffset { get; }
        protected abstract int mNumVertices { get; }
        protected abstract int mStartIndex { get; }
        protected abstract int mPrimitiveCount { get; }

        public GeometryNode(EffectApplication defaultMaterial)
        {
            mMaterials = new Dictionary<TraversalContext.MaterialFlags, EffectApplication>();
            mMaterials.Add(TraversalContext.MaterialFlags.None, defaultMaterial);
        }

        // Provide a material to use for special rendering modes.  You can supply a null material if the node should not draw anything
        // for a certain mode (usually to not participate in the shadow pass.)
        public void AddMaterial(TraversalContext.MaterialFlags materialFlag, EffectApplication material) // For the key, please only set one flag at a time.
        {
            mMaterials.Add(materialFlag, material);
            mSupportedMaterialFlags |= materialFlag;
        }

        public override void Process(TraversalContext context)
        {
            // Choose the appropriate material
            TraversalContext.MaterialFlags materialFlags = (LocalMaterialFlags | context.ExternalMaterialFlags) & mSupportedMaterialFlags;
            EffectApplication currentMaterial = mMaterials[materialFlags & ~(materialFlags - 1)]; // Clear all but the lowest bit (highest priority flag)

            if (currentMaterial != null)
                context.AcceptForRender(this, currentMaterial);
        }

        public virtual void Draw(RenderContext context, EffectApplication material, Matrix transform)
        {
            material.SetEffectParams(context, transform);
            for (int p = 0; p < material.Effect.CurrentTechnique.Passes.Count; ++p)
            {
                material.Effect.CurrentTechnique.Passes[p].Apply();
                if (SharedResources.GraphicsDeviceReady)
                    SharedResources.Game.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        mVertexOffset,
                        0,
                        mNumVertices,
                        mStartIndex,
                        mPrimitiveCount);
            }
        }

        public override void ConnectToAnimationComponent(AnimationComponent animationComponent)
        {
            foreach (KeyValuePair<TraversalContext.MaterialFlags, EffectApplication> kvp in mMaterials)
            {
                if (kvp.Value.ParamSetters.ContainsKey(ParamSetter.Category.Skin))
                    ((SkinParamSetter)(kvp.Value.ParamSetters[ParamSetter.Category.Skin])).AnimationComponent = animationComponent;
            }
        }
    }
}
