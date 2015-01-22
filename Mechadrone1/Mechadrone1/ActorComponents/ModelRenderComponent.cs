using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Manifracture;
using Skelemator;

namespace Mechadrone1
{
    class ModelRenderComponent : BVCullableRenderComponent
    {
        public Model VisualModel { get; private set; }

        public ModelRenderComponent(Actor owner)
            : base(owner)
        {
            VisualModel = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            VisualModel = contentLoader.Load<Model>((string)(manifest.Properties[ManifestKeys.VISUAL_MODEL]));
            base.Initialize(contentLoader, manifest);
        }

        protected override void BuildSphereAndGeometryNodes(ComponentManifest manifest, SceneNode parent)
        {
            foreach (ModelMesh mm in VisualModel.Meshes)
            {
                ExplicitBoundingSphereNode meshBound = new ExplicitBoundingSphereNode(mm.BoundingSphere);
                parent.AddChild(meshBound);

                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    MaterialInfo mi = mmp.Tag as MaterialInfo;
                    if (mi == null)
                        throw new AssetFormatException("The VisualModel's ModelMeshParts do not contain the MaterialInfo in the Tag property.");

                    // First, add the effect's params to the EffectRegistry so the ParamSetter can retrieve them.
                    EffectRegistry.Add(mmp.Effect, mi.HandlingFlags);

                    // Create the default material
                    EffectApplication defaultMaterial = new EffectApplication(mmp.Effect, mi.RenderState);
                    defaultMaterial.AddParamSetter(new CommonParamSetter());
                    defaultMaterial.AddParamSetter(new FogParamSetter());

                    if ((mi.HandlingFlags & RenderOptions.RequiresHDRLighting) > 0)
                        defaultMaterial.AddParamSetter(new HDRLightParamSetter());

                    if ((mi.HandlingFlags & RenderOptions.RequiresSkeletalPose) > 0)
                        defaultMaterial.AddParamSetter(new SkinParamSetter());

                    if ((mi.HandlingFlags & RenderOptions.RequiresShadowMap) > 0)
                        defaultMaterial.AddParamSetter(new ShadowParamSetter());

                    if ((mi.HandlingFlags & RenderOptions.RequiresFringeMap) > 0)
                        defaultMaterial.AddParamSetter(new FringeMapParamSetter());

                    ModelGeometryNode geometry = new ModelGeometryNode(mmp, defaultMaterial);
                    meshBound.AddChild(geometry);

                    // Create the ShadowMap material
                    // Default to CastsShadow = true
                    if (!manifest.Properties.ContainsKey(ManifestKeys.CASTS_SHADOW) || (bool)(manifest.Properties[ManifestKeys.CASTS_SHADOW]))
                    {
                        EffectApplication depthOnlyMaterial;
                        if ((mi.HandlingFlags & RenderOptions.RequiresSkeletalPose) > 0)
                        {
                            depthOnlyMaterial = new EffectApplication(EffectRegistry.DepthOnlySkinFx, RenderStatePresets.Default);
                            depthOnlyMaterial.AddParamSetter(new SkinParamSetter());
                        }
                        else
                        {
                            depthOnlyMaterial = new EffectApplication(EffectRegistry.DepthOnlyFx, RenderStatePresets.Default);
                        }

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
    }
}
