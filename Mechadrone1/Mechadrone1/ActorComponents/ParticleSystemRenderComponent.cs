using Manifracture;
using Microsoft.Xna.Framework;
using Skelemator;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;

namespace Mechadrone1
{
    class ParticleSystemRenderComponent : BVCullableRenderComponent
    {
        private Effect mEffect { get; set; }
        private ParticleSystemSettings mSettings;
        // An array of particles, treated as a circular queue.
        private ParticleVertex[] mParticles;
        private ParticleData mParticleData;
        private float mCurrentTime;
        private int mFirstNewParticleIndex;
        private int mFirstRetiredParticleIndex;
        private int mDrawCounter;

        public bool HasActiveParticles { get { return mParticleData.FirstFreeParticleIndex != mParticleData.FirstActiveParticleIndex; } }

        public ParticleSystemRenderComponent(Actor owner)
            : base(owner)
        {
            mEffect = null;
            mSettings = null;
            mParticles = null;
            mParticleData = null;
            mCurrentTime = 0.0f;
            mFirstNewParticleIndex = 0;
            mFirstRetiredParticleIndex = 0;
            mDrawCounter = 0;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            mSettings = contentLoader.Load<ParticleSystemSettings>((string)(manifest.Properties[ManifestKeys.PARTICLE_SYSTEM_SETTINGS]));

            string techniqueName = "StandardParticles";
            if (manifest.Properties.ContainsKey(ManifestKeys.TECHNIQUE_NAME))
                techniqueName = (string)(manifest.Properties[ManifestKeys.TECHNIQUE_NAME]);

            LoadParticleEffect(contentLoader, techniqueName);

            // Allocate the particle array, and fill in the corner fields (which never change).
            mParticles = new ParticleVertex[mSettings.MaxParticles * 4];

            for (int i = 0; i < mSettings.MaxParticles; ++i)
            {
                mParticles[i * 4 + 0].Corner = new Short2(-1.0f, -1.0f);
                mParticles[i * 4 + 1].Corner = new Short2(+1.0f, -1.0f);
                mParticles[i * 4 + 2].Corner = new Short2(+1.0f, +1.0f);
                mParticles[i * 4 + 3].Corner = new Short2(-1.0f, +1.0f);
            }

            mParticleData = new ParticleData();
            // Create a dynamic vertex buffer.
            mParticleData.VertexBuffer = new DynamicVertexBuffer(
                SharedResources.Game.GraphicsDevice,
                ParticleVertex.VertexDeclaration,
                mSettings.MaxParticles * 4,
                BufferUsage.WriteOnly);

            // Create and populate the index buffer.
            ushort[] indices = new ushort[mSettings.MaxParticles * 6];

            for (int i = 0; i < mSettings.MaxParticles; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            mParticleData.IndexBuffer = new IndexBuffer(
                SharedResources.Game.GraphicsDevice,
                typeof(ushort),
                indices.Length,
                BufferUsage.None);

            mParticleData.IndexBuffer.SetData(indices);
            mParticleData.MaxParticles = mSettings.MaxParticles;

            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
            GameResources.ActorManager.UpdateComplete += UpdateCompleteHandler;

            base.Initialize(contentLoader, manifest);
        }

        /// <summary>
        /// Helper for loading and initializing the particle effect.
        /// </summary>
        private void LoadParticleEffect(ContentManager contentLoader, string techniqueName)
        {
            Effect genericEffect = contentLoader.Load<Effect>("shaders\\Particles");

            // If we have several particle systems, the content manager will return
            // a single shared effect instance to them all. But we want to preconfigure
            // the effect with parameters that are specific to this particular
            // particle system. By cloning the effect, we prevent one particle system
            // from stomping over the parameter settings of another.

            mEffect = genericEffect.Clone();
            mEffect.CurrentTechnique = mEffect.Techniques[techniqueName];
            EffectRegistry.Add(mEffect, RenderOptions.NoStandardParams | RenderOptions.ParticleParams);

            EffectParameterCollection parameters = mEffect.Parameters;

            // Set the values of parameters that do not change.
            parameters["Duration"].SetValue((float)mSettings.Duration.TotalSeconds);
            parameters["DurationRandomness"].SetValue(mSettings.DurationRandomness);
            parameters["Gravity"].SetValue(mSettings.Gravity);
            parameters["EndVelocity"].SetValue(mSettings.EndVelocity);
            parameters["MinColor"].SetValue(mSettings.MinColor.ToVector4());
            parameters["MaxColor"].SetValue(mSettings.MaxColor.ToVector4());
            parameters["RotateSpeed"].SetValue(new Vector2(mSettings.MinRotateSpeed, mSettings.MaxRotateSpeed));
            parameters["StartSize"].SetValue(new Vector2(mSettings.MinStartSize, mSettings.MaxStartSize));
            parameters["EndSize"].SetValue(new Vector2(mSettings.MinEndSize, mSettings.MaxEndSize));
            Texture2D texture = contentLoader.Load<Texture2D>(mSettings.TextureName);
            parameters["Texture"].SetValue(texture);
        }

        protected override void BuildSphereAndGeometryNodes(ComponentManifest manifest, SceneNode parent)
        {
            BoundingSphere bound = new BoundingSphere();
            bound.Center = Vector3.Zero;
            // TODO: P2: Magic number?
            bound.Radius = 120.0f;
            ExplicitBoundingSphereNode meshBound = new ExplicitBoundingSphereNode(bound);
            parent.AddChild(meshBound);

            // Create the default material
            EffectApplication defaultMaterial = new EffectApplication(mEffect, mSettings.RenderState);
            defaultMaterial.AddParamSetter(new ParticlesParamSetter(delegate() { return mCurrentTime; }));

            ParticleSystemGeometryNode geometry = new ParticleSystemGeometryNode(mParticleData, delegate() { mDrawCounter++; }, defaultMaterial);
            meshBound.AddChild(geometry);

            // It won't cast a shadow.
            geometry.AddMaterial(TraversalContext.MaterialFlags.ShadowMap, null);
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            mCurrentTime += (float)(e.GameTime.ElapsedGameTime.TotalSeconds);

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.
            if (mParticleData.FirstActiveParticleIndex == mParticleData.FirstFreeParticleIndex)
                mCurrentTime = 0;

            if (mFirstRetiredParticleIndex == mParticleData.FirstActiveParticleIndex)
                mDrawCounter = 0;
        }

        private void UpdateCompleteHandler(object sender, UpdateStepEventArgs e)
        {
            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (mFirstNewParticleIndex != mParticleData.FirstFreeParticleIndex)
            {
                AddNewParticlesToVertexBuffer();
            }
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        private void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;

            if (mFirstNewParticleIndex < mParticleData.FirstFreeParticleIndex)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                mParticleData.VertexBuffer.SetData(
                    mFirstNewParticleIndex * stride * 4,
                    mParticles,
                    mFirstNewParticleIndex * 4,
                    (mParticleData.FirstFreeParticleIndex - mFirstNewParticleIndex) * 4,
                    stride,
                    SetDataOptions.NoOverwrite);
            }
            else // Note, we know newIndex != freeIndex a priori
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                mParticleData.VertexBuffer.SetData(
                    mFirstNewParticleIndex * stride * 4,
                    mParticles,
                    mFirstNewParticleIndex * 4,
                    (mSettings.MaxParticles - mFirstNewParticleIndex) * 4,
                    stride,
                    SetDataOptions.NoOverwrite);

                if (mParticleData.FirstFreeParticleIndex > 0)
                {
                    mParticleData.VertexBuffer.SetData(
                        0,
                        mParticles,
                        0,
                        mParticleData.FirstFreeParticleIndex * 4,
                        stride,
                        SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            mFirstNewParticleIndex = mParticleData.FirstFreeParticleIndex;
        }

        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        private void RetireActiveParticles()
        {
            float particleDuration = (float)mSettings.Duration.TotalSeconds;

            while (mParticleData.FirstActiveParticleIndex != mFirstNewParticleIndex)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = mCurrentTime - mParticles[mParticleData.FirstActiveParticleIndex * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                mParticles[mParticleData.FirstActiveParticleIndex * 4].Time = mDrawCounter;

                // Move the particle from the active to the retired queue.
                mParticleData.FirstActiveParticleIndex++;

                if (mParticleData.FirstActiveParticleIndex >= mSettings.MaxParticles)
                    mParticleData.FirstActiveParticleIndex = 0;
            }
        }

        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        private void FreeRetiredParticles()
        {
            while (mFirstRetiredParticleIndex != mParticleData.FirstActiveParticleIndex)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                int age = mDrawCounter - (int)mParticles[mFirstRetiredParticleIndex * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                mFirstRetiredParticleIndex++;

                if (mFirstRetiredParticleIndex >= mSettings.MaxParticles)
                    mFirstRetiredParticleIndex = 0;
            }
        }

        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = mParticleData.FirstFreeParticleIndex + 1;

            if (nextFreeParticle >= mSettings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == mFirstRetiredParticleIndex)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= mSettings.EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            Random random = GameResources.ActorManager.Random;
            float horizontalVelocity = MathHelper.Lerp(mSettings.MinHorizontalVelocity,
                                                       mSettings.MaxHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(mSettings.MinVerticalVelocity,
                                          mSettings.MaxVerticalVelocity,
                                          (float)random.NextDouble());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                mParticles[mParticleData.FirstFreeParticleIndex * 4 + i].Position = position;
                mParticles[mParticleData.FirstFreeParticleIndex * 4 + i].Velocity = velocity;
                mParticles[mParticleData.FirstFreeParticleIndex * 4 + i].Random = randomValues;
                mParticles[mParticleData.FirstFreeParticleIndex * 4 + i].Time = mCurrentTime;
            }

            mParticleData.FirstFreeParticleIndex = nextFreeParticle;
        }

        public override void Release()
        {
            GameResources.ActorManager.PreAnimationUpdateStep -= PreAnimationUpdateHandler;
            GameResources.ActorManager.UpdateComplete -= UpdateCompleteHandler;

            base.Release();
        }
    }
}
