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
        // TODO: The CollisionModel property (not currently supported) could belong to a subclass that needs a special collidable shape.
        [LoadedAsset]
        public Model CollisionModel { get; set; }
        public string SimulationObjectTypeFullName { get; set; }
        [NotInitializable]
        public Type SimulationObjectType { get; set; }
        public object[] SimulationObjectCtorParams { get; set; }
        [NotInitializable]
        public ISpaceObject SimulationObject { get; set; }

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
                // TODO: Somewhat hacky...
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
                // TODO: Somewhat hacky...

                Position = value;
            }
        }


        public SimulatedGameObject(IGameManager owner)
            : base(owner)
        {
        }


        public virtual void Initialize()
        {
            base.Initialize();

            SimulationObjectType = Type.GetType(SimulationObjectTypeFullName);
            SimulationObject = Activator.CreateInstance(SimulationObjectType, SimulationObjectCtorParams) as ISpaceObject;

            Entity soEnt = SimulationObject as Entity;

            if (soEnt != null)
            {
                soEnt.Position = BepuConverter.Convert(SimulationPosition);
                soEnt.Orientation = BepuConverter.Convert(Orientation);
                soEnt.Material.Bounciness = 0.68f;
                soEnt.Material.StaticFriction = 0.6f;
                soEnt.Material.KineticFriction = 0.3f;
            }

            owner.SimSpace.Add(SimulationObject);
        }


        public override void RegisterUpdateHandlers()
        {
            owner.PostPhysicsUpdateStep += PostPhysicsUpdate;
        }


        public virtual void PostPhysicsUpdate(object sender, UpdateEventArgs e)
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
