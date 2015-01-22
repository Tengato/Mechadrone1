using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    public delegate void LoadNewLevelDelegate(string levelName);

    static class GameResources
    {
        public static ActorManager ActorManager { get; set; }
        public static PlaySession PlaySession { get; set; }
        public static GameDossier GameDossier { get; set; }
        public static LoadNewLevelDelegate LoadNewLevelDelegate { get; set; }

        static GameResources()
        {
            ActorManager = null;
            PlaySession = null;
            GameDossier = null;
            LoadNewLevelDelegate = null;
        }
    }
}
