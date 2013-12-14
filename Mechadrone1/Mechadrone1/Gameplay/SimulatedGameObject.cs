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
    class SimulatedGameObject : GameObject, ISimulationParticipant
    {
        // TODO: The CollisionModel property (not currently supported) could be pushed down to a subclass that wants a special collidable shape.
        [LoadedAsset]
        public Model CollisionModel { get; set; }
        public string SimulationObjectTypeFullName { get; set; }
        private Type simulationObjectType { get; set; }
        public object[] SimulationObjectCtorParams { get; set; }
        // SimulationObject is an abstract type to allow instances to choose from a few primitive shape types.
        public virtual ISpaceObject SimulationObject { get; protected set; }

        [NotInitializable]
        public virtual Vector3 SimulationPosition
        {
            get
            {
                Box soBox = SimulationObject as Box;
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
                Box soBox = SimulationObject as Box;
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


        public SimulatedGameObject(SimulatedGameObject a) : base(a)
        {
            CollisionModel = a.CollisionModel;
            SimulationObjectTypeFullName = a.SimulationObjectTypeFullName;
            SimulationObjectCtorParams = a.SimulationObjectCtorParams;
        }


        public override void Initialize()
        {
            base.Initialize();

            simulationObjectType = Type.GetType(SimulationObjectTypeFullName);
            SimulationObject = Activator.CreateInstance(simulationObjectType, SimulationObjectCtorParams) as ISpaceObject;

            Entity soEnt = SimulationObject as Entity;

            if (soEnt != null)
            {
                soEnt.Position = BepuConverter.Convert(SimulationPosition);
                soEnt.Orientation = BepuConverter.Convert(Orientation);
                soEnt.Material.Bounciness = 0.68f;
                soEnt.Material.StaticFriction = 0.6f;
                soEnt.Material.KineticFriction = 0.3f;
            }

        }


        public override void RegisterUpdateHandlers()
        {
            game.PostPhysicsUpdateStep += PostPhysicsUpdate;
        }


        public virtual void PostPhysicsUpdate(object sender, UpdateStepEventArgs e)
        {
            Entity soEnt = SimulationObject as Entity;
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
