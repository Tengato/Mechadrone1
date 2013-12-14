using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Manifracture
{
    public class GameObjectLoadInfo
    {
        public string TypeFullName { get; set; }
        public bool Spawn { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public GameObjectLoadInfo()
        {
            Properties = new Dictionary<string, object>();
        }
    }

}
