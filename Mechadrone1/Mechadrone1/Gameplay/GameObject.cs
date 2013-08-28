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


namespace Mechadrone1.Gameplay
{
    class GameObject : IAudible
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

        private QuadTree quadTree { get; set; }


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
                        worldSpaceBoundingBox = CombineBBoxes((BoundingBox)worldSpaceBoundingBox,
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
                    quadTreeBoundingBox = QuadTreeRect.CreateFromBoundingBox(WorldSpaceBoundingBox, quadTree.WorldToQuadTreeTransform);
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
                    quadTree.AddOrUpdateSceneObject(this);

                hasMovedSinceLastUpdate = false;
            }
        }


        private BoundingBox CombineBBoxes(BoundingBox a, BoundingBox b)
        {
            BoundingBox result;
            result.Min.X = Math.Min(a.Min.X, b.Min.X);
            result.Min.Y = Math.Min(a.Min.Y, b.Min.Y);
            result.Min.Z = Math.Min(a.Min.Z, b.Min.Z);
            result.Max.X = Math.Max(a.Max.X, b.Max.X);
            result.Max.Y = Math.Max(a.Max.Y, b.Max.Y);
            result.Max.Z = Math.Max(a.Max.Z, b.Max.Z);

            return result;
        }


        public void JoinQuadTree(QuadTree quadTree)
        {
            this.quadTree = quadTree;

            if (Visible)
                this.quadTree.AddOrUpdateSceneObject(this);
        }
    }

}
