using Mechadrone1.Gameplay.Decorators;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Skelemator;
using BEPUphysics;
using System;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Entities;
using Mechadrone1.Rendering;
using System.Collections.Generic;
using SlagformCommon;
using BEPUphysicsDemos.AlternateMovement.Character;


namespace Mechadrone1.Gameplay
{
    class GameObject : IAudible, ISceneObject
    {
        public string Name { get; set; }

        protected Model visualModel;
        [LoadedAsset]
        public virtual Model VisualModel
        {
            get
            {
                return visualModel;
            }
            set
            {
                visualModel = value;
                AnimationPackage ap = visualModel.Tag as AnimationPackage;
                if (ap != null)
                {
                    Animations = ap.SkinningData;
                    if (ap.SkinningData != null)
                    {
                        AnimationPlayer = new ClipPlayer(Animations);
                    }
                }
            }
        }


        protected IGameManager owner;

        [NotInitializable]
        public QuadTreeNode QuadTreeNode { get; set; }

        [NotInitializable]
        public QuadTree QuadTree { get; set; }

        protected Matrix world;
        protected Matrix wvp;
        protected Matrix wit;

        [NotInitializable]
        public ICamera Camera { get; set; }

        protected Matrix[] bones;

        private BoundingBox? worldSpaceBoundingBox;
        [NotInitializable]
        public BoundingBox WorldSpaceBoundingBox
        {
            get
            {
                if (worldSpaceBoundingBox == null)
                {
                    worldSpaceBoundingBox = BoundingBox.CreateFromSphere(VisualModel.Meshes[0].BoundingSphere.Transform(WorldTransform));

                    for (int i = 1; i < VisualModel.Meshes.Count; i++)
                    {
                        worldSpaceBoundingBox = SlagformCommon.SpaceUtils.CombineBBoxes((BoundingBox)worldSpaceBoundingBox,
                            BoundingBox.CreateFromSphere(VisualModel.Meshes[i].BoundingSphere.Transform(WorldTransform)));
                    }
                }

                return (BoundingBox)worldSpaceBoundingBox;
            }
        }

        private QuadTreeRect? quadTreeBoundingBox;
        [NotInitializable]
        public QuadTreeRect QuadTreeBoundingBox
        {
            get
            {
                if (quadTreeBoundingBox == null)
                {
                    quadTreeBoundingBox = QuadTreeRect.CreateFromBoundingBox(WorldSpaceBoundingBox, QuadTree.WorldToQuadTreeTransform);
                }

                return (QuadTreeRect)quadTreeBoundingBox;
            }
        }

        protected bool hasMovedSinceLastUpdate;

        protected Vector3 position;
        virtual public Vector3 Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    worldSpaceBoundingBox = null;
                    quadTreeBoundingBox = null;
                    hasMovedSinceLastUpdate = true;
                }
                position = value;
            }
        }


        protected Quaternion orientation;
        virtual public Quaternion Orientation
        {
            get { return orientation; }
            set
            {
                if (orientation != value)
                {
                worldSpaceBoundingBox = null;
                quadTreeBoundingBox = null;
                hasMovedSinceLastUpdate = true;
                }
                orientation = value;
            }
        }

        public Quaternion ModelAdjustment { get; set; }

        protected float scale;
        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    worldSpaceBoundingBox = null;
                    quadTreeBoundingBox = null;
                    hasMovedSinceLastUpdate = true;
                }
                scale = value;
            }
        }


        [NotInitializable]
        public Matrix WorldTransform
        {
            get
            {
                return Matrix.CreateScale(Scale) *
                    Matrix.CreateFromQuaternion(Orientation) *
                    Matrix.CreateTranslation(Position);
            }
        }


        [NotInitializable]
        public virtual Matrix CameraAnchor
        {
            get
            {
                return Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position);
            }
        }

        public Vector3 CameraOffset { get; set; }
        public Vector3 CameraTargetOffset { get; set; }

        // Default pose
        static protected Matrix[] bindPose;

        public SkinningData Animations { get; set; }
        public ISkinnedSkeletonPoser AnimationPlayer { get; set; }
        public bool CastsShadow { get; set; }
        public bool Visible { get; set; }
        public event MakeSoundEventHandler MakeSound;
        public string DebugMessage { get; set; }


        public GameObject(IGameManager owner)
        {
            this.owner = owner;

            if (bindPose == null)
            {
                bindPose = new Matrix[72];

                for (int i = 0; i < bindPose.Length; i++)
                {
                    bindPose[i] = Matrix.Identity;
                }
            }

            // Set defaults:
            Position = Vector3.Zero;
            Orientation = Quaternion.Identity;
            ModelAdjustment = Quaternion.Identity;
            Scale = 1.0f;
            CastsShadow = true;
            Visible = true;
            CameraOffset = new Vector3(0, 5, -5);
            CameraTargetOffset = new Vector3(0, 3, 0);
            DebugMessage = String.Empty;
        }

        /// <summary>
        /// Performs additional processing of the object once all properties have been set from the level manifest.
        /// </summary>
        public virtual void Initialize()
        {
            RegisterUpdateHandlers();

            if (VisualModel != null)
            {
                if (!EffectRegistry.RegisteredModels.Contains(VisualModel))
                {
                    foreach (ModelMesh mesh in VisualModel.Meshes)
                    {
                        foreach (ModelMeshPart mmp in mesh.MeshParts)
                        {
                            if (mmp.Tag != null)
                            {
                                EffectRegistry.Add(mmp.Effect, (RenderOptions)(mmp.Tag));
                            }
                            else
                            {
                                throw new InvalidOperationException("The model must be conditioned using a Skelemator processor.");
                            }
                        }
                    }

                    EffectRegistry.RegisteredModels.Add(VisualModel);
                }
            }
            else
            {
                Visible = false;
            }
        }


        public virtual void RegisterUpdateHandlers() { }


        // TODO: Probably this should be purely virtual:
        public virtual void CreateCamera()
        {
            ChaseCamera newCam = new ChaseCamera();

            newCam.DesiredPositionOffset = CameraOffset;
            newCam.LookAtOffset = CameraTargetOffset;
            newCam.Stiffness = GameOptions.CameraStiffness;
            newCam.Damping = GameOptions.CameraDamping;
            newCam.Mass = GameOptions.CameraMass;
            newCam.FieldOfView = MathHelper.ToRadians(45.0f);

            newCam.ChasePosition = CameraAnchor.Translation;
            newCam.ChaseDirection = CameraAnchor.Forward;
            newCam.Up = CameraAnchor.Up;
            newCam.Reset();

            Camera = newCam;
        }


        /// <summary>
        /// Get objects's up vector in world space
        /// </summary>
        public Vector3 ViewUp
        {
            get { return (Matrix.CreateFromQuaternion(Orientation)).Up; }
        }


        protected void OnMakeSound(string soundName)
        {
            MakeSound(this, new MakeSoundEventArgs(soundName, 0.1f));
        }


        public virtual void HandleInput(GameTime gameTime, InputManager input, PlayerIndex player) { }


        protected void UpdateQuadTree()
        {
            if (hasMovedSinceLastUpdate)
            {
                if (Visible)
                    QuadTree.AddOrUpdateSceneObject(this);

                hasMovedSinceLastUpdate = false;
            }
        }


        public virtual void UpdateCamera(float elapsedTime)
        {
            ChaseCamera chaseCam = Camera as ChaseCamera;
            chaseCam.ChasePosition = CameraAnchor.Translation;
            chaseCam.ChaseDirection = CameraAnchor.Forward;
            chaseCam.Up = CameraAnchor.Up;
            chaseCam.Update(elapsedTime);
        }


        public virtual List<RenderEntry> GetRenderEntries(
            int batchId,
            RenderStep step,
            Matrix view,
            Matrix projection,
            Matrix cameraTransform,
            Matrix shadowCastingLightView,
            Matrix shadowCastingLightProjection,
            RenderTarget2D shadowMap,
            List<DirectLight> lights)
        {
            world = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation * ModelAdjustment) * Matrix.CreateTranslation(Position);

            wvp = step == RenderStep.Shadows ?
                world * shadowCastingLightView * shadowCastingLightProjection :
                world * view * projection;
            wit = Matrix.Transpose(Matrix.Invert(world));

            bones = bindPose;

            if (Animations != null && AnimationPlayer != null && AnimationPlayer.IsActive == true)
            {
                bones = AnimationPlayer.GetSkinTransforms();
            }

            List<RenderEntry> results = new List<RenderEntry>();

            foreach (ModelMesh mesh in VisualModel.Meshes)
            {
                foreach (ModelMeshPart mmp in mesh.MeshParts)
                {
                    RenderEntry re = new RenderEntry();

                    re.VertexBuffer = mmp.VertexBuffer;
                    re.NumVertices = mmp.NumVertices;
                    re.IndexBuffer = mmp.IndexBuffer;
                    re.VertexOffset = mmp.VertexOffset;
                    re.StartIndex = mmp.StartIndex;
                    re.RenderOptions = (RenderOptions)(mmp.Tag);
                    re.PrimitiveCount = mmp.PrimitiveCount;

                    re.View = view;
                    re.Projection = projection;
                    re.ShadowCastingLightView = shadowCastingLightView;
                    re.ShadowCastingLightProjection = shadowCastingLightProjection;
                    re.ShadowMap = shadowMap;
                    re.CameraTransform = cameraTransform;
                    re.SceneObject = this;
                    re.Lights = lights;

                    switch (step)
                    {
                        case RenderStep.Default:
                            re.DrawCallback = Draw;
                            re.Effect = mmp.Effect;
                            break;
                        case RenderStep.Shadows:
                            re.DrawCallback = DrawShadow;

                            // TODO: perhaps put these fx into separate techniques instead maybe?
                            if ((re.RenderOptions & RenderOptions.RequiresSkeletalPose) > 0 && Animations != null)
                            {
                                re.Effect = EffectRegistry.DepthOnlySkinFx;
                            }
                            else
                            {
                                re.Effect = EffectRegistry.DepthOnlyFx;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    re.Pass = re.Effect.CurrentTechnique.Passes[0];
                    results.Add(re);

                    // Make additional copies of the render entry for multi-pass techniques:
                    for (int i = 1; i < re.Effect.CurrentTechnique.Passes.Count; i++)
                    {
                        RenderEntry reCopy = new RenderEntry(re);
                        reCopy.Pass = re.Effect.CurrentTechnique.Passes[i];
                        results.Add(reCopy);
                    }
                }
            }

            return results;
        }


        public virtual void DrawShadow(RenderEntry re)
        {
            EffectRegistry.DOWorldViewProj.SetValue(wvp);
            EffectRegistry.DOSWorldViewProj.SetValue(wvp);

            if (Animations != null)
            {
                EffectRegistry.DOSWeightsPerVert.SetValue(Animations.WeightsPerVert);
                EffectRegistry.DOSPosedBones.SetValue(bones);
            }

            re.Pass.Apply();
            re.Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, re.VertexOffset, 0,
                re.NumVertices, re.StartIndex, re.PrimitiveCount);
        }


        public virtual void Draw(RenderEntry re)
        {
            if ((re.RenderOptions & RenderOptions.RequiresSkeletalPose) > 0)
            {
                EffectRegistry.Params[re.Effect][EffectRegistry.POSEDBONES_PARAM_NAME].SetValue(bones);
                if (Animations != null)
                {
                    EffectRegistry.Params[re.Effect][EffectRegistry.WEIGHTS_PER_VERT_PARAM_NAME].SetValue(Animations.WeightsPerVert);
                }
                else
                {
                    EffectRegistry.Params[re.Effect][EffectRegistry.WEIGHTS_PER_VERT_PARAM_NAME].SetValue(4);
                }

            }

            if ((re.RenderOptions & RenderOptions.RequiresShadowMap) > 0)
            {
                EffectRegistry.Params[re.Effect][EffectRegistry.SHADOWTRANSFORM_PARAM_NAME].SetValue(
                    world * re.ShadowCastingLightView * re.ShadowCastingLightProjection * SceneManager.NDC_TO_TEXCOORDS);
                EffectRegistry.Params[re.Effect][EffectRegistry.SHADOWMAP_PARAM_NAME].SetValue(re.ShadowMap);
            }

            EffectRegistry.Params[re.Effect][EffectRegistry.WORLD_PARAM_NAME].SetValue(world);
            EffectRegistry.Params[re.Effect][EffectRegistry.WORLDVIEWPROJ_PARAM_NAME].SetValue(wvp);
            EffectRegistry.Params[re.Effect][EffectRegistry.WORLDINVTRANSPOSE_PARAM_NAME].SetValue(wit);
            EffectRegistry.Params[re.Effect][EffectRegistry.EYEPOSITION_PARAM_NAME].SetValue(re.CameraTransform.Translation);
            EffectRegistry.SetLighting(re.Lights, re.Effect);

            re.Pass.Apply();
            re.Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                re.VertexOffset, 0, re.NumVertices, re.StartIndex, re.PrimitiveCount);
        }

    }

}
