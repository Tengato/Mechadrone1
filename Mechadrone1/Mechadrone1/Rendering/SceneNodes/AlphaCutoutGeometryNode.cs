using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Mechadrone1
{
    class AlphaCutoutGeometryNode : RawGeometryNode
    {
        protected Dictionary<TraversalContext.MaterialFlags, EffectApplication> mFringeMaterials { get; private set; }

        public AlphaCutoutGeometryNode(MeshPart geometry, EffectApplication defaultOpaqueMaterial, EffectApplication defaultFringeMaterial)
            : base(geometry, defaultOpaqueMaterial)
        {
            mFringeMaterials = new Dictionary<TraversalContext.MaterialFlags, EffectApplication>();
            mFringeMaterials.Add(TraversalContext.MaterialFlags.None, defaultFringeMaterial);
        }

        public override void Process(TraversalContext context)
        {
            base.Process(context);

            // Choose the appropriate material
            TraversalContext.MaterialFlags materialFlags = (LocalMaterialFlags | context.ExternalMaterialFlags) & mSupportedMaterialFlags;
            EffectApplication currentFringeMaterial = mFringeMaterials[materialFlags & ~(materialFlags - 1)]; // Clear all but the lowest bit (highest priority flag)

            if (currentFringeMaterial != null)
                context.AcceptForRender(this, currentFringeMaterial);
        }

        public void AddFringeMaterial(TraversalContext.MaterialFlags materialFlag, EffectApplication fringeMaterial) // For the key, please only set one flag at a time.
        {
            mFringeMaterials.Add(materialFlag, fringeMaterial);
        }
    }
}
