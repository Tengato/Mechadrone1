using System.Threading;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class AvatarSpawnComponent : ActorComponent
    {
        private static int sNextId;

        public int Id { get; private set; }

        static AvatarSpawnComponent()
        {
            sNextId = 0;
        }

        public static void Reset()
        {
            sNextId = 0;
        }

        public AvatarSpawnComponent(Actor owner)
            : base(owner)
        {
            Id = Interlocked.Increment(ref sNextId) - 1;
            GameResources.ActorManager.AvatarSpawned += AvatarSpawnedHandler;
        }

        public override ComponentType Category
        {
            get { return ComponentType.Behavior; }
        }

        protected void AvatarSpawnedHandler(object sender, AvatarSpawnedEventArgs e)
        {
            if ((e.SpawnRandom % sNextId) != Id)
                return;

            // Ok, we'll handle this one.
            Actor spawnedActor = GameResources.ActorManager.GetActorById(e.ActorId);

            TransformComponent tc = Owner.GetComponent<TransformComponent>(ComponentType.Transform);
            spawnedActor.GetComponent<TransformComponent>(ComponentType.Transform).Transform = tc.Transform;
        }

        public override void Release()
        {
            GameResources.ActorManager.AvatarSpawned -= AvatarSpawnedHandler;
        }
    }
}
