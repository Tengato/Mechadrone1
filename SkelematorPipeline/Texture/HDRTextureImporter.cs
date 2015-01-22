using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace SkelematorPipeline
{
    [ContentImporter(".xml", DefaultProcessor = "HDRTextureProcessor", DisplayName = "Skelemator HDR Texture Importer")]
    class HDRTextureImporter : ContentImporter<List<byte[]>>
    {
        public override List<byte[]> Import(string filename, ContentImporterContext context)
        {
            List<byte[]> imageData = new List<byte[]>();
            List<string> imageFilenames;
            context.AddDependency(Path.Combine(Environment.CurrentDirectory, filename));
            using (XmlReader reader = XmlReader.Create(filename))
            {
                imageFilenames = IntermediateSerializer.Deserialize<List<string>>(reader, null);
            }

            foreach (string imageFilename in imageFilenames)
            {
                byte[] imageBytes = File.ReadAllBytes(imageFilename);
                imageData.Add(imageBytes);
                context.AddDependency(Path.Combine(Environment.CurrentDirectory, imageFilename));
            }

            return imageData;
        }

    }
}
