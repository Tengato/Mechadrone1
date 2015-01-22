using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    delegate void AvatarSpawnedEventHandler(object sender, AvatarSpawnedEventArgs e);

    class AvatarSpawnedEventArgs : EventArgs
    {
        public int ActorId { get; set; }
        public int SpawnRandom { get; set; }

        public AvatarSpawnedEventArgs(int actorId, int spawnRandom)
            : base()
        {
            ActorId = actorId;
            SpawnRandom = spawnRandom;
        }
    }

}
