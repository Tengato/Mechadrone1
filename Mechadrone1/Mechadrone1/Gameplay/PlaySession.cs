using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    // Information about players participating in a game.
    class PlaySession
    {
        public List<PlayerInfo> Players { get; private set; }
        public Dictionary<int, PlayerIndex> LocalPlayers { get; set; }   // Key is player id.

        public PlaySession()
        {
            Players = new List<PlayerInfo>();
            LocalPlayers = new Dictionary<int, PlayerIndex>();
        }
    }
}
