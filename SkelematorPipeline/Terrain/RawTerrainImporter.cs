using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace SkelematorPipeline
{
    [ContentImporter(".raw", DefaultProcessor = "TerrainProcessor", DisplayName = "Skelemator Raw Terrain Importer")]
    class RawTerrainImporter : ContentImporter<byte[]>
    {
        public override byte[] Import(string filename, ContentImporterContext context)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            return bytes;
        }

    }
}
