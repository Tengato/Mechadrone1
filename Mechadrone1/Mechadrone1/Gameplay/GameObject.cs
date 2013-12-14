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
using System.Text.RegularExpressions;


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
                    animations = ap.SkinningData;
                    if (ap.SkinningData != null)
                    {
                        animationPlayer = new ClipPlayer(animations);
                    }
                }
            }
        }

        protected IGameManager game;

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

        protected SkinningData animations { get; set; }
        protected ISkinnedSkeletonPoser animationPlayer { get; set; }

        public bool CastsShadow { get; set; }

        protected bool visible;
        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    hasMovedSinceLastUpdate = true;
                    visible = value;
                    UpdateQuadTree();
                }
            }
        }

        public event MakeSoundEventHandler MakeSound;
        public string DebugMessage { get; set; }


        static GameObject()
        {
            const int MAX_BONES = 72;
            bindPose = new Matrix[MAX_BONES];

            for (int i = 0; i < MAX_BONES; i++)
            {
                bindPose[i] = Matrix.Identity;
            }
        }


        public GameObject(IGameManager owner)
        {
            this.game = owner;

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
        /// Copy constructor. The name property of the copy should be updated as soon as possible,
        /// along with the other properties that need to be unique. Then the copy will have to be
        /// initialized and spawned to enter the game.
        /// </summary>
        /// <param name="a"></param>
        public GameObject(GameObject a)
        {
            game = a.game;
            ModelAdjustment = a.ModelAdjustment;
            Position = a.Position;
            Orientation = a.Orientation;
            Scale = a.Scale;
            CastsShadow = a.CastsShadow;
            Visible = a.Visible;
            CameraOffset = a.CameraOffset;
            CameraTargetOffset = a.CameraTargetOffset;
            Name = a.Name;
            VisualModel = a.VisualModel;
            DebugMessage = String.Empty;
        }


        /// <summary>
        /// Performs required additional processing of the object once all properties have been set.
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


        /// <summary>
        /// Updates the objects's goal states based on control input
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <param name="input">The control information</param>
        /// <param name="player">PlayerIndex of player sending the input.</param>
        public virtual void HandleInput(GameTime gameTime, InputManager input, PlayerIndex player) { }


        protected void UpdateQuadTree()
        {
            if (QuadTreeNode != null && hasMovedSinceLastUpdate)
            {
                if (Visible)
                {
                    QuadTree.AddOrUpdateSceneObject(this);
                }
                else
                {
                    // remove the member from it's previous quad tree node (if any)
                    if (QuadTreeNode != null)
                    {
                        QuadTreeNode.RemoveMember(this);
                    }
                }

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

            if (animations != null && animationPlayer != null && animationPlayer.IsActive == true)
            {
                bones = animationPlayer.GetSkinTransforms();
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
                            if ((re.RenderOptions & RenderOptions.RequiresSkeletalPose) > 0 && animations != null)
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

            if (animations != null)
            {
                EffectRegistry.DOSWeightsPerVert.SetValue(animations.WeightsPerVert);
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
                if (animations != null)
                {
                    EffectRegistry.Params[re.Effect][EffectRegistry.WEIGHTS_PER_VERT_PARAM_NAME].SetValue(animations.WeightsPerVert);
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
