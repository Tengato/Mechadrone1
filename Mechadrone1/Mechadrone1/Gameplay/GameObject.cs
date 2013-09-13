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


namespace Mechadrone1.Gameplay
{
    class GameObject : IAudible, ISceneObject
    {
        public string Name { get; set; }

        [LoadedAsset]
        public Model VisualModel { get; set; }

        public bool IsSimulationParticipant { get; set; }

        // TODO: Maybe the simulation can be added using a decorator?
        [LoadedAsset]
        public Model CollisionModel { get; set; }
        public string SimulationObjectTypeFullName { get; set; }
        [NotInitializable]
        public Type SimulationObjectType { get; set; }
        public object[] SimulationObjectCtorParams { get; set; }
        [NotInitializable]
        public ISpaceObject SimulationObject { get; set; }

        [NotInitializable]
        public QuadTreeNode QuadTreeNode { get; set; }

        [NotInitializable]
        public QuadTree QuadTree { get; set; }

        protected Matrix world;
        protected Matrix wvp;
        protected Matrix wit;

        private Matrix[] bones;

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
                        worldSpaceBoundingBox = SlagformCommon.Space.CombineBBoxes((BoundingBox)worldSpaceBoundingBox,
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

        private bool hasMovedSinceLastUpdate;

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

        // Default pose
        static private Matrix[] bindPose;

        public SkinningData Animations { get; set; }
        public AnimationPlayer AnimationPlayer { get; set; }
        public bool CastsShadow { get; set; }
        public bool IsAlive { get; set; }
        public bool Visible { get; set; }
        public bool Dynamic { get; set; }
        public event MakeSoundEventHandler MakeSound;


        [NotInitializable]
        public Vector3 SimulationPosition
        {
            get
            {
                Box soBox = SimulationObject as Box;
                if (soBox != null)
                {
                    return Position + Matrix.CreateFromQuaternion(Orientation).Up * soBox.HalfHeight;
                }
                // TODO: Somewhat hacky...
                else
                    return Position;
            }

            set
            {
                Box soBox = SimulationObject as Box;
                if (soBox != null)
                {
                    Position =  value - Matrix.CreateFromQuaternion(Orientation).Up * soBox.HalfHeight;
                }
                // TODO: Somewhat hacky...
                else
                    Position = value;
            }
        }


        public GameObject()
        {
            if (bindPose == null)
            {
                bindPose = new Matrix[72];

                for (int i = 0; i < bindPose.Length; i++)
                {
                    bindPose[i] = Matrix.Identity;
                }
            }

            // Set defaults:
            IsSimulationParticipant = false;
            Position = Vector3.Zero;
            Orientation = Quaternion.Identity;
            Scale = 1.0f;
            CastsShadow = true;
            IsAlive = false;
            Visible = true;
            Dynamic = true;
        }


        public virtual void Initialize()
        {
            if (IsSimulationParticipant)
            {
                SimulationObjectType = Type.GetType(SimulationObjectTypeFullName);
                SimulationObject = Activator.CreateInstance(SimulationObjectType, SimulationObjectCtorParams) as ISpaceObject;

                Entity soEnt = SimulationObject as Entity;

                if (soEnt != null)
                {
                    soEnt.Position = SimulationPosition;
                    soEnt.Orientation = Orientation;
                    soEnt.Material.Bounciness = 0.68f;
                    soEnt.Material.StaticFriction = 0.6f;
                    soEnt.Material.KineticFriction = 0.3f;
                }
            }

            if (VisualModel != null)
            {
                if (EffectRegistry.RegisteredModels.Add(VisualModel))
                {
                    foreach (ModelMesh mesh in VisualModel.Meshes)
                    {
                        foreach (ModelMeshPart mmp in mesh.MeshParts)
                        {
                            EffectRegistry.Add(mmp.Effect, (RenderOptions)(mmp.Tag));
                        }
                    }
                }
            }
            else
            {
                Visible = false;
            }
        }


        /// <summary>
        /// Get actor's up vector in world space
        /// </summary>
        public Vector3 ViewUp
        {
            get { return (Matrix.CreateFromQuaternion(Orientation)).Up; }
        }


        protected void OnMakeSound(string soundName)
        {
            MakeSound(this, new MakeSoundEventArgs(soundName, 0.1f));
        }


        public virtual void HandleInput(GameTime gameTime, InputManager input, PlayerIndex player, ICamera camera) { }


        public virtual void Update(GameTime gameTime)
        {
            if (IsSimulationParticipant)
            {
                Entity soEnt = SimulationObject as Entity;
                if (soEnt != null)
                {
                    if (soEnt.IsDynamic)
                    {
                        SimulationPosition = soEnt.Position;
                        Orientation = soEnt.Orientation;
                    }
                }
            }

            if (hasMovedSinceLastUpdate)
            {
                if (Visible)
                    QuadTree.AddOrUpdateSceneObject(this);

                hasMovedSinceLastUpdate = false;
            }
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
            world = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(Position);

            wvp = step == RenderStep.Shadows ?
                world * shadowCastingLightView * shadowCastingLightProjection :
                world * view * projection;
            wit = Matrix.Transpose(Matrix.Invert(world));

            bones = bindPose;

            if (Animations != null && AnimationPlayer != null && AnimationPlayer.CurrentClip != null)
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
                            if (re.RenderOptions.HasFlag(RenderOptions.RequiresSkeletalPose) && Animations != null)
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
            if (re.RenderOptions.HasFlag(RenderOptions.RequiresSkeletalPose))
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

            if (re.RenderOptions.HasFlag(RenderOptions.RequiresShadowMap))
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
