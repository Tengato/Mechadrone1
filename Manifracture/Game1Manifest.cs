using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Manifracture
{
    public class Game1Manifest
    {
        public string SkydomeTextureName { get; set; }
        public List<TerrainChunkLoadInfo> TerrainChunks { get; set; }
        public List<GameObjectLoadInfo> GameObjects { get; set; }
        public FogDesc Fog { get; set; }
        public List<Matrix> PlayerStartTransforms { get; set; }
        public DirectLight KeyLight { get; set; }
    }
}
