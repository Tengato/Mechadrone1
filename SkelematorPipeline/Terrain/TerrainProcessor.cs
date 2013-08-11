using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace SkelematorPipeline
{
    [ContentProcessor(DisplayName = "Skelemator Terrain Processor")]
    public class TerrainProcessor : ContentProcessor<byte[], TerrainContent>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float XZScale { get; set; }
        public float YScale { get; set; }

        // Location of an XML file that describes which materials to use.
        public virtual string MaterialDataFilePath { get; set; }

        private List<MaterialData> terrainMaterial;

        private Vector3[,] position;
        private Vector3[,] normal;

        private float mapXRadius;
        private float mapZRadius;

        private TerrainContent outputTC;
        ContentProcessorContext context;

        public override TerrainContent Process(byte[] input, ContentProcessorContext context)
        {
            this.context = context;

            if (Width < 1)
                throw new InvalidContentException("Width property must be greater than 0.");
            if (Height < 1)
                throw new InvalidContentException("Height property must be greater than 0.");
            if (input.Length != Width * Height)
                throw new InvalidContentException("Height and/or Width properties are incorrect.");
            if (XZScale <= 0.0f)
                throw new InvalidContentException("XZScale property must be greater than 0.");
            if (YScale <= 0.0f)
                throw new InvalidContentException("YScale property must be greater than 0.");

            mapXRadius = XZScale * (float)(Width - 1) / 2.0f;
            mapZRadius = XZScale * (float)(Height - 1) / 2.0f;

            VertexElement vePosition0 = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
            VertexElement veNormal0 = new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
            int vertexStride = 24; // This is set based on the above VertexElement composition.

            outputTC = new TerrainContent();
            outputTC.VertexCountAlongXAxis = Width;
            outputTC.VertexCountAlongZAxis = Height;
            outputTC.VertexBufferContent = new VertexBufferContent(Width * Height * vertexStride);
            outputTC.VertexBufferContent.VertexDeclaration.VertexElements.Add(vePosition0);
            outputTC.VertexBufferContent.VertexDeclaration.VertexElements.Add(veNormal0);
            outputTC.VertexBufferContent.VertexDeclaration.VertexStride = vertexStride;
            outputTC.TriangleCount = (Width - 1) * (Height - 1) * 2;
            outputTC.VertexCount = Width * Height;
            outputTC.Position = Vector3.Zero;

            GeneratePositions(input);
            GenerateNormals();
            InitializeIndicies();
            CreateMaterial();

            return outputTC;
        }


        private void InitializeIndicies()
        {
            int[] indices = new int[outputTC.TriangleCount * 3];

            int i = 0;
            for (int row = 0; row < Height - 1; row++)
            {
                for (int col = 0; col < Width - 1; col++)
                {
                    indices[i++] = row * Width + Width + col;
                    indices[i++] = row * Width + col;
                    indices[i++] = row * Width + col + 1;

                    indices[i++] = row * Width + Width + col;
                    indices[i++] = row * Width + col + 1;
                    indices[i++] = row * Width + Width + col + 1;
                }
            }

            outputTC.IndexCollection = new IndexCollection();
            outputTC.IndexCollection.AddRange(indices);
        }


        private void GeneratePositions(byte[] heightData)
        {
            const int PIXEL_RADIUS = 5;

            position = new Vector3[Width, Height];

            float x;
            float y;
            float z;
            byte[] xBytes;
            byte[] yBytes;
            byte[] zBytes;

            int sampleColumn;
            int sampleRow;
            float pixelDistSq;

            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    x = -mapXRadius + col * XZScale;
                    z = -mapZRadius + row * XZScale;
                    y = 0.0f;

                    // Apply smoothing:
                    for (int i = -PIXEL_RADIUS; i <= PIXEL_RADIUS; i++)
                    {
                        for (int j = -PIXEL_RADIUS; j <= PIXEL_RADIUS; j++)
                        {
                            sampleRow = Math.Min(Math.Max(row + i, 0), Height - 1);
                            sampleColumn = Math.Min(Math.Max(col + j, 0), Width - 1);

                            // Assuming std deviation is 1 pixel.
                            pixelDistSq = (float)(i * i + j * j);
                            y += (float)(Math.Exp(-pixelDistSq / 2.0f)) / MathHelper.TwoPi * (float)(heightData[sampleColumn + sampleRow * Width]) * YScale;
                        }
                    }

                    position[col, row] = new Vector3(x, y, z);

                    xBytes = System.BitConverter.GetBytes(x);
                    yBytes = System.BitConverter.GetBytes(y);
                    zBytes = System.BitConverter.GetBytes(z);

                    int vertStride = (int)(outputTC.VertexBufferContent.VertexDeclaration.VertexStride);

                    for (int i = 0; i < 4; i++)
                    {
                        outputTC.VertexBufferContent.VertexData[(col + row * Width) * vertStride + i] = xBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * Width) * vertStride + 4 + i] = yBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * Width) * vertStride + 8 + i] = zBytes[i];
                    }
                }
            }
        }


        private void GenerateNormals()
        {
            Vector3 right;
            Vector3 down;
            Vector3 cross;

            int rightStartCol;
            int rightEndCol;
            int downStartRow;
            int downEndRow;

            normal = new Vector3[Width, Height];

            byte[] xBytes;
            byte[] yBytes;
            byte[] zBytes;

            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (col == 0)
                    {
                        rightStartCol = 0;    // *
                        rightEndCol = 1;
                    }
                    else if (col == Width - 1)
                    {
                        rightStartCol = Width - 2;
                        rightEndCol = Width - 1;    // *
                    }
                    else
                    {
                        rightStartCol = col - 1;
                        rightEndCol = col + 1;
                    }

                    if (row == 0)
                    {
                        downStartRow = 0;   // *
                        downEndRow = 1;
                    }
                    else if (row == Height - 1)
                    {
                        downStartRow = Height - 2;
                        downEndRow = Height - 1;  // *
                    }
                    else
                    {
                        downStartRow = row - 1;
                        downEndRow = row + 1;
                    }

                    right = position[rightEndCol, row] - position[rightStartCol, row];
                    down = position[col, downEndRow] - position[col, downStartRow];
                    cross = Vector3.Cross(down, right);
                    cross.Normalize();
                    normal[col, row] = cross;

                    xBytes = System.BitConverter.GetBytes(cross.X);
                    yBytes = System.BitConverter.GetBytes(cross.Y);
                    zBytes = System.BitConverter.GetBytes(cross.Z);

                    int vertStride = (int)(outputTC.VertexBufferContent.VertexDeclaration.VertexStride);

                    for (int i = 0; i < 4; i++)
                    {
                        outputTC.VertexBufferContent.VertexData[(col + row * Width) * vertStride + 12 + i] = xBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * Width) * vertStride + 16 + i] = yBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * Width) * vertStride + 20 + i] = zBytes[i];
                    }
                }
            }
        }

        private void CreateMaterial()
        {
            using (XmlReader reader = XmlReader.Create(MaterialDataFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<MaterialData>));
                terrainMaterial = (List<MaterialData>)serializer.Deserialize(reader);
            }

            MaterialData tmSingle = terrainMaterial.Single();

            EffectMaterialContent emc = new EffectMaterialContent();

            emc.Effect = new ExternalReference<EffectContent>(Path.Combine(Environment.CurrentDirectory,
                tmSingle.CustomEffect));
            emc.Name = tmSingle.Name;

            foreach (EffectParam ep in tmSingle.EffectParams)
            {
                if (ep.Category == EffectParamCategory.OpaqueData)
                {
                    emc.OpaqueData.Add(ep.Name, ep.Value);
                }
                else if (ep.Category == EffectParamCategory.Texture)
                {
                    emc.Textures.Add(ep.Name, new ExternalReference<TextureContent>((string)(ep.Value)));
                }
            }

            outputTC.Tag = tmSingle.HandlingFlags;

            outputTC.MaterialContent = context.Convert<MaterialContent, MaterialContent>(emc, "MaterialProcessor");
        }

    }
}