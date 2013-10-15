using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities;
using Manifracture;
using SlagformCommon;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysicsDemos.AlternateMovement.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mechadrone1.Gameplay
{
    class SimulatedGameObject : GameObject
    {
        // TODO: The CollisionModel property (not currently supported) could be pushed down to a subclass that wants a special collidable shape.
        [LoadedAsset]
        public Model CollisionModel { get; set; }
        public string SimulationObjectTypeFullName { get; set; }
        private Type simulationObjectType { get; set; }
        public object[] SimulationObjectCtorParams { get; set; }
        // simulationObject is an abstract type to allow instances to choose from a few primitive shape types.
        protected ISpaceObject simulationObject { get; set; }

        [NotInitializable]
        public virtual Vector3 SimulationPosition
        {
            get
            {
                Box soBox = simulationObject as Box;
                if (soBox != null)
                {
                    return Position + Matrix.CreateFromQuaternion(Orientation).Up * soBox.HalfHeight;
                }
                // TODO: Define calculations for all supported shape types. (Somewhat hacky...)
                else
                    return Position;

                //Capsule d = new Capsule(
            }

            set
            {
                Box soBox = simulationObject as Box;
                if (soBox != null)
                {
                    Position = value - Matrix.CreateFromQuaternion(Orientation).Up * soBox.HalfHeight;
                    return;
                }
                // TODO: Define calculations for all supported shape types. (Somewhat hacky...)

                Position = value;
            }
        }


        public SimulatedGameObject(IGameManager owner)
            : base(owner)
        {
        }


        public override void Initialize()
        {
            base.Initialize();

            simulationObjectType = Type.GetType(SimulationObjectTypeFullName);
            simulationObject = Activator.CreateInstance(simulationObjectType, SimulationObjectCtorParams) as ISpaceObject;

            Entity soEnt = simulationObject as Entity;

            if (soEnt != null)
            {
                soEnt.Position = BepuConverter.Convert(SimulationPosition);
                soEnt.Orientation = BepuConverter.Convert(Orientation);
                soEnt.Material.Bounciness = 0.68f;
                soEnt.Material.StaticFriction = 0.6f;
                soEnt.Material.KineticFriction = 0.3f;
            }

            owner.SimSpace.Add(simulationObject);
        }


        public override void RegisterUpdateHandlers()
        {
            owner.PostPhysicsUpdateStep += PostPhysicsUpdate;
        }


        public virtual void PostPhysicsUpdate(object sender, UpdateStepEventArgs e)
        {
            Entity soEnt = simulationObject as Entity;
            if (soEnt != null)
            {
                if (soEnt.IsDynamic)
                {
                    SimulationPosition = BepuConverter.Convert(soEnt.Position);
                    Orientation = BepuConverter.Convert(soEnt.Orientation);
                }
            }

            UpdateQuadTree();
        }

    }
}
