using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Skelemator;

namespace Mechadrone1
{
    class ShapeRenderComponent : BVCullableRenderComponent
    {
        public Effect Effect { get; private set; }
        private Texture mTexture;

        public ShapeRenderComponent(Actor owner)
            : base(owner)
        {
            Effect = null;
            mTexture = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Effect genericEffect = contentLoader.Load<Effect>("shaders\\PhongShadow");

            // If we have several of these objects, the content manager will return
            // a single shared effect instance to them all. But we want to preconfigure
            // the effect with parameters that are specific to this particular
            // object. By cloning the effect, we prevent one
            // from stomping over the parameter settings of another.

            Effect = genericEffect.Clone();
            mTexture = contentLoader.Load<Texture>((string)(manifest.Properties[ManifestKeys.TEXTURE]));

            base.Initialize(contentLoader, manifest);
        }

        protected override void BuildSphereAndGeometryNodes(ComponentManifest manifest, SceneNode parent)
        {
            Vector4 specColor = (Vector4)(manifest.Properties[ManifestKeys.MAT_SPEC_COLOR]);
            Effect.Parameters[EffectRegistry.MATERIAL_SPECULAR_COLOR_PARAM_NAME].SetValue(specColor);
            Effect.Parameters[EffectRegistry.TEXTURE_PARAM_NAME].SetValue(mTexture);

            float brightness = 1.0f;
            if (manifest.Properties.ContainsKey(ManifestKeys.BRIGHTNESS))
                brightness = (float)(manifest.Properties[ManifestKeys.BRIGHTNESS]);
            Effect.Parameters[EffectRegistry.BRIGHTNESS_PARAM_NAME].SetValue(brightness);

            float contrast = 1.0f;
            if (manifest.Properties.ContainsKey(ManifestKeys.CONTRAST))
                contrast = (float)(manifest.Properties[ManifestKeys.CONTRAST]);
            Effect.Parameters[EffectRegistry.CONTRAST_PARAM_NAME].SetValue(contrast);

            // First, add the effect's params to the EffectRegistry so the ParamSetter can retrieve them.
            EffectRegistry.Add(Effect, RenderOptions.RequiresHDRLighting | RenderOptions.RequiresShadowMap);

            // Create the default material
            EffectApplication defaultMaterial = new EffectApplication(Effect, RenderStatePresets.Default);
            defaultMaterial.AddParamSetter(new CommonParamSetter());
            defaultMaterial.AddParamSetter(new HDRLightParamSetter());
            defaultMaterial.AddParamSetter(new FogParamSetter());
            defaultMaterial.AddParamSetter(new ShadowParamSetter());

            RawGeometryNode geometry = null;
            ExplicitBoundingSphereNode meshBound = null;
            switch ((Shape)(manifest.Properties[ManifestKeys.SHAPE]))
            {
                case Shape.Box:
                    geometry = new RawGeometryNode(GeneratedGeometry.Box, defaultMaterial);
                    meshBound = new ExplicitBoundingSphereNode(GeneratedGeometry.BoxBound);
                    break;
                case Shape.Sphere:
                    geometry = new RawGeometryNode(GeneratedGeometry.Sphere, defaultMaterial);
                    meshBound = new ExplicitBoundingSphereNode(GeneratedGeometry.SphereBound);
                    break;
            }

            parent.AddChild(meshBound);
            meshBound.AddChild(geometry);

            // Create the ShadowMap material
            // Default to CastsShadow = true
            if (!manifest.Properties.ContainsKey(ManifestKeys.CASTS_SHADOW) || (bool)(manifest.Properties[ManifestKeys.CASTS_SHADOW]))
            {
                EffectApplication depthOnlyMaterial;
                depthOnlyMaterial = new EffectApplication(EffectRegistry.DepthOnlyFx, RenderStatePresets.Default);
                depthOnlyMaterial.AddParamSetter(new WorldViewProjParamSetter());
                geometry.AddMaterial(TraversalContext.MaterialFlags.ShadowMap, depthOnlyMaterial);
            }
            else
            {
                geometry.AddMaterial(TraversalContext.MaterialFlags.ShadowMap, null);
            }

            // If the GeometryNode requires additional materials, they would need to be added here.
        }
    }
}
