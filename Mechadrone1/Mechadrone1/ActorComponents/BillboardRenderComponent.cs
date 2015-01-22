using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using Microsoft.Xna.Framework;
using SlagformCommon;
using Skelemator;

namespace Mechadrone1
{
    class BillboardRenderComponent : BVCullableRenderComponent
    {
        public Effect Effect;
        private Effect mDepthOnlyEffect;
        private Texture mTexture;
        private MeshPart mGeometry;
        private float mCurrentTime;

        public BillboardRenderComponent(Actor owner)
            : base(owner)
        {
            Effect = null;
            mDepthOnlyEffect = null;
            mTexture = null;
            mGeometry = null;
            mCurrentTime = 0.0f;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Effect genericEffect = contentLoader.Load<Effect>("shaders\\Billboard");

            // If we have several of these objects, the content manager will return
            // a single shared effect instance to them all. But we want to preconfigure
            // the effect with parameters that are specific to this particular
            // object. By cloning the effect, we prevent one
            // from stomping over the parameter settings of another.

            Effect = genericEffect.Clone();
            Effect.CurrentTechnique = Effect.Techniques["Billboard"];
            EffectRegistry.Add(Effect, RenderOptions.BillboardParams);

            EffectParameterCollection parameters = Effect.Parameters;
            // Set the values of parameters that do not change.
            parameters["WindAmount"].SetValue(0.0f);
            parameters["BillboardWidth"].SetValue(2.0f);
            parameters["BillboardHeight"].SetValue(2.0f);
            mTexture = contentLoader.Load<Texture>((string)(manifest.Properties[ManifestKeys.TEXTURE]));
            parameters["Texture"].SetValue(mTexture);
            parameters["gBright"].SetValue(0.17f);
            parameters["gContrast"].SetValue(0.9f);

            mDepthOnlyEffect = Effect.Clone();
            mDepthOnlyEffect.CurrentTechnique = mDepthOnlyEffect.Techniques["DepthOnlyBillboard"];
            EffectRegistry.Add(mDepthOnlyEffect, RenderOptions.BillboardParams);

            mGeometry = new MeshPart();

            BillboardVertex[] vertices = new BillboardVertex[4];

            vertices[0].Position = Vector3.Zero;
            vertices[1].Position = Vector3.Zero;
            vertices[2].Position = Vector3.Zero;
            vertices[3].Position = Vector3.Zero;

            vertices[0].Normal = Vector3.Up;
            vertices[1].Normal = Vector3.Up;
            vertices[2].Normal = Vector3.Up;
            vertices[3].Normal = Vector3.Up;

            vertices[0].TexCoord = Vector2.Zero;
            vertices[1].TexCoord = Vector2.UnitX;
            vertices[2].TexCoord = Vector2.One;
            vertices[3].TexCoord = Vector2.UnitY;

            float randValue = 0.5f;
            if (manifest.Properties.ContainsKey(ManifestKeys.IS_RANDOMIZED) &&
                (bool)(manifest.Properties[ManifestKeys.IS_RANDOMIZED]))
                randValue = (float)(GameResources.ActorManager.Random.Next(-524288, 524288)) / (float)524288;

            vertices[0].Random = randValue;
            vertices[1].Random = randValue;
            vertices[2].Random = randValue;
            vertices[3].Random = randValue;

            mGeometry.VertexBuffer = new VertexBuffer(
                SharedResources.Game.GraphicsDevice,
                BillboardVertex.VertexDeclaration,
                4,
                BufferUsage.None);

            mGeometry.VertexBuffer.SetData(vertices);

            // Create and populate the index buffer.
            ushort[] indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            mGeometry.IndexBuffer = new IndexBuffer(
                SharedResources.Game.GraphicsDevice,
                typeof(ushort),
                indices.Length,
                BufferUsage.None);

            mGeometry.IndexBuffer.SetData(indices);

            mGeometry.NumVertices = 4;
            mGeometry.PrimitiveCount = 2;
            mGeometry.StartIndex = 0;
            mGeometry.VertexOffset = 0;

            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;

            base.Initialize(contentLoader, manifest);
        }

        protected override void BuildSphereAndGeometryNodes(ComponentManifest manifest, SceneNode parent)
        {
            BoundingSphere bound = new BoundingSphere();
            bound.Center = Vector3.Up;
            bound.Radius = SlagMath.SQRT_2 * 1.25f; // Billboards can be randomly enlarged and waving back and forth, so expand the bound by 25%.
            ExplicitBoundingSphereNode meshBound = new ExplicitBoundingSphereNode(bound);
            parent.AddChild(meshBound);

            // Create the default material
            EffectApplication defaultMaterial = new EffectApplication(Effect, Skelemator.RenderStatePresets.Default);
            defaultMaterial.AddParamSetter(new CommonParamSetter());
            defaultMaterial.AddParamSetter(new FogParamSetter());
            defaultMaterial.AddParamSetter(new BillboardParamSetter(true, delegate() { return mCurrentTime; }));
            EffectApplication defaultFringeMaterial = new EffectApplication(Effect, Skelemator.RenderStatePresets.AlphaBlendNPM);
            defaultFringeMaterial.AddParamSetter(new CommonParamSetter());
            defaultFringeMaterial.AddParamSetter(new FogParamSetter());
            defaultFringeMaterial.AddParamSetter(new BillboardParamSetter(false, delegate() { return mCurrentTime; }));

            AlphaCutoutGeometryNode geometry = new AlphaCutoutGeometryNode(mGeometry, defaultMaterial, defaultFringeMaterial);
            meshBound.AddChild(geometry);

            EffectApplication depthMaterial = new EffectApplication(mDepthOnlyEffect, Skelemator.RenderStatePresets.Default);
            depthMaterial.AddParamSetter(new CommonParamSetter());
            depthMaterial.AddParamSetter(new BillboardParamSetter(true, delegate() { return mCurrentTime; }));
            geometry.AddMaterial(TraversalContext.MaterialFlags.ShadowMap, null);
            geometry.AddFringeMaterial(TraversalContext.MaterialFlags.ShadowMap, null);
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            mCurrentTime += (float)(e.GameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Release()
        {
            base.Release();
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
        }
    }
}
