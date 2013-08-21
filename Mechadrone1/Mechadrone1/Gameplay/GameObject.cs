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

        protected Vector3 position;
        virtual public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        protected Quaternion orientation;
        virtual public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        protected float scale;
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
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
        }

    }

}
