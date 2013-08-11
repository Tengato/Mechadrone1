using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Manifracture
{
    public class Game1Manifest
    {
        public string TerrainAssetName { get; set; }
        public List<GameObjectLoadInfo> GameObjects { get; set; }
        public FogDesc Fog { get; set; }
        public List<Matrix> PlayerStartTransforms { get; set; }
        public DirectLight KeyLight { get; set; }
    }
}
