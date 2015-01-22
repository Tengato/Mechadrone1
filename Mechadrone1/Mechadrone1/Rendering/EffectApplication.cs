using System.Collections.Generic;
using Skelemator;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    // An object of this class, aka a material, specifies an effect to use with a compatible geometry parent, and
    // contains the knowledge to set the effect parameters.
    sealed class EffectApplication
    {
        // Tables of render state presets
        private static DepthStencilState sDSStateSky;
        private static Dictionary<RenderStatePresets, BlendState> sRenderStateBlendStateMap;
        private static Dictionary<RenderStatePresets, DepthStencilState> sRenderStateDepthStencilStateMap;
        private static Dictionary<RenderStatePresets, RasterizerState> sRenderStateRasterizerStateMap;
        private static Dictionary<RenderStatePresets, bool> sRenderStateAlphaPassMap;

        static EffectApplication()
        {
            sDSStateSky = new DepthStencilState();
            sDSStateSky.DepthBufferFunction = CompareFunction.LessEqual;

            sRenderStateBlendStateMap = new Dictionary<RenderStatePresets, BlendState>();
            sRenderStateBlendStateMap.Add(RenderStatePresets.Default, BlendState.Opaque);
            sRenderStateBlendStateMap.Add(RenderStatePresets.AlphaAdd, BlendState.Additive);
            sRenderStateBlendStateMap.Add(RenderStatePresets.AlphaBlend, BlendState.AlphaBlend);
            sRenderStateBlendStateMap.Add(RenderStatePresets.AlphaBlendNPM, BlendState.NonPremultiplied);
            sRenderStateBlendStateMap.Add(RenderStatePresets.Skybox, BlendState.Opaque);

            sRenderStateDepthStencilStateMap = new Dictionary<RenderStatePresets, DepthStencilState>();
            sRenderStateDepthStencilStateMap.Add(RenderStatePresets.Default, DepthStencilState.Default);
            sRenderStateDepthStencilStateMap.Add(RenderStatePresets.AlphaAdd, DepthStencilState.DepthRead);
            sRenderStateDepthStencilStateMap.Add(RenderStatePresets.AlphaBlend, DepthStencilState.DepthRead);
            sRenderStateDepthStencilStateMap.Add(RenderStatePresets.AlphaBlendNPM, DepthStencilState.DepthRead);
            sRenderStateDepthStencilStateMap.Add(RenderStatePresets.Skybox, sDSStateSky);

            sRenderStateRasterizerStateMap = new Dictionary<RenderStatePresets, RasterizerState>();
            sRenderStateRasterizerStateMap.Add(RenderStatePresets.Default, RasterizerState.CullCounterClockwise);
            sRenderStateRasterizerStateMap.Add(RenderStatePresets.AlphaAdd, RasterizerState.CullNone);
            sRenderStateRasterizerStateMap.Add(RenderStatePresets.AlphaBlend, RasterizerState.CullCounterClockwise);
            sRenderStateRasterizerStateMap.Add(RenderStatePresets.AlphaBlendNPM, RasterizerState.CullCounterClockwise);
            sRenderStateRasterizerStateMap.Add(RenderStatePresets.Skybox, RasterizerState.CullNone);

            sRenderStateAlphaPassMap = new Dictionary<RenderStatePresets, bool>();
            sRenderStateAlphaPassMap.Add(RenderStatePresets.Default, false);
            sRenderStateAlphaPassMap.Add(RenderStatePresets.AlphaAdd, true);
            sRenderStateAlphaPassMap.Add(RenderStatePresets.AlphaBlend, true);
            sRenderStateAlphaPassMap.Add(RenderStatePresets.AlphaBlendNPM, true);
            sRenderStateAlphaPassMap.Add(RenderStatePresets.Skybox, false);
        }

        public Effect Effect { get; private set; }
        public Dictionary<ParamSetter.Category, ParamSetter> ParamSetters { get; private set; }
        public RenderStatePresets RenderState { get; private set; }

        public BlendState BlendState { get { return sRenderStateBlendStateMap[RenderState]; } }
        public DepthStencilState DepthStencilState { get { return sRenderStateDepthStencilStateMap[RenderState]; } }
        public RasterizerState RasterizerState { get { return sRenderStateRasterizerStateMap[RenderState]; } }
        public bool UseAlphaPass { get { return sRenderStateAlphaPassMap[RenderState]; } }

        public EffectApplication(Effect effect, RenderStatePresets renderState)
        {
            Effect = effect;
            RenderState = renderState;
            ParamSetters = new Dictionary<ParamSetter.Category, ParamSetter>();
        }

        public void AddParamSetter(ParamSetter paramSetter)
        {
            ParamSetters.Add(paramSetter.Domain, paramSetter);
        }

        public void SetEffectParams(RenderContext context, Matrix transform)
        {
            foreach (KeyValuePair<ParamSetter.Category, ParamSetter> kvp in ParamSetters)
            {
                kvp.Value.Set(Effect, context, transform);
            }
        }
    }
}
