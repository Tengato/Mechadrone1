namespace Mechadrone1
{
    class TranslucencyNode : SceneNode
    {
        private static bool sPreviousFlagValue;

        public float Alpha { get; set; }

        static TranslucencyNode()
        {
            sPreviousFlagValue = false;
        }

        public TranslucencyNode()
        {
            Alpha = 0.5f;
        }

        public override void Process(TraversalContext context)
        {
            if (context.TranslucencyGrantor == null)
            {
                sPreviousFlagValue = (context.ExternalMaterialFlags & TraversalContext.MaterialFlags.Translucent) > 0;
                context.TranslucencyGrantor = this;
                context.ExternalMaterialFlags |= TraversalContext.MaterialFlags.Translucent;
            }
        }

        public override void PostProcess(TraversalContext context)
        {
            if (context.TranslucencyGrantor == this && !sPreviousFlagValue)
            {
                context.TranslucencyGrantor = null;
                context.ExternalMaterialFlags &= ~TraversalContext.MaterialFlags.Translucent;
            }
        }
    }
}
