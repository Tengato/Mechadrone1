using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace SkelematorPipeline
{
    [ContentProcessor]
    public class FxcMaterialProcessor : MaterialProcessor
    {
        protected override ExternalReference<CompiledEffectContent> BuildEffect(
            ExternalReference<EffectContent> effect, ContentProcessorContext context)
        {
            return context.BuildAsset<EffectContent, CompiledEffectContent>(effect, "FXProcessor");
        }
    }
}
