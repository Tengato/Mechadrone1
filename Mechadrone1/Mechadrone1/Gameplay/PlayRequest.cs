using System.Collections.Generic;

namespace Mechadrone1
{
    // Information required to instantiate a GameplayScreen object.
    class PlayRequest
    {
        public Dictionary<int, CharacterInfo> CharacterSelections { get; set; } // The int key is a player id
        public string LevelName { get; set; }

        public PlayRequest()
        {
            CharacterSelections = new Dictionary<int, CharacterInfo>();
        }
    }
}
