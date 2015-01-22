using System;
using Microsoft.Xna.Framework.Graphics;
using Manifracture;
using Skelemator;

namespace Mechadrone1
{
    class SkyboxRenderComponent : RenderComponent
    {
        public static event EventHandler EnvironmentMapAdded;

        public TextureCube SkyTexture { get; private set; }

        public SkyboxRenderComponent(Actor owner)
            : base(owner)
        {
            SkyTexture = null;
        }

        public override SceneGraphRoot.PartitionCategories SceneGraphCategory
        {
            get { return SceneGraphRoot.PartitionCategories.Skybox; }
        }

        public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager contentLoader, ComponentManifest manifest)
        {
            SkyTexture = contentLoader.Load<TextureCube>((string)(manifest.Properties[ManifestKeys.TEXTURE]));
            OnEnvironmentMapAdded(EventArgs.Empty);

            base.Initialize(contentLoader, manifest);
        }

        protected override void CreateSceneGraph(ComponentManifest manifest)
        {
            EffectApplication defaultMaterial = new EffectApplication(EffectRegistry.SkyboxFx, RenderStatePresets.Skybox);
            defaultMaterial.AddParamSetter(new SkyboxWvpParamSetter());
            defaultMaterial.AddParamSetter(new EnvironmentMapParamSetter());
            SceneGraph = new RawGeometryNode(GeneratedGeometry.Sphere, defaultMaterial);
            ((GeometryNode)SceneGraph).AddMaterial(TraversalContext.MaterialFlags.ShadowMap, null);
        }

        protected void OnEnvironmentMapAdded(EventArgs e)
        {
            EventHandler handler = EnvironmentMapAdded;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
