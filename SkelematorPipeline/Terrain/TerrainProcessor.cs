using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;

namespace SkelematorPipeline
{
    [ContentProcessor(DisplayName = "Skelemator Terrain Processor")]
    public class TerrainProcessor : ContentProcessor<byte[], TerrainContent>
    {
        const int SECTOR_SIZE = 64;

        public int NumXValues { get; set; }
        public int NumZValues { get; set; }
        public float XZScale { get; set; }
        public float YScale { get; set; }
        public float YOffset { get; set; }

        // We allow the heightmaps to contain extra data that is cropped so that the edges (after cropping) will
        // be smoothed to match neighboring terrain chunks.
        public int CropXLeft { get; set; }
        public int CropXRight { get; set; }
        public int CropZTop { get; set; }
        public int CropZBottom { get; set; }

        private int CroppedXValues
        {
            get
            {
                return NumXValues - CropXRight - CropXLeft;
            }
        }

        private int CroppedZValues
        {
            get
            {
                return NumZValues - CropZTop - CropZBottom;
            }
        }


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

            if (CroppedXValues < 2 || CroppedXValues % SECTOR_SIZE != 1)
                throw new InvalidContentException(String.Format("NumXValues property value after cropping must be an integer w = n * {0} + 1 where n: {1, 2, 3...}", SECTOR_SIZE));
            if (CroppedZValues < 2 || CroppedZValues % SECTOR_SIZE != 1)
                throw new InvalidContentException(String.Format("NumZValues property value after cropping must be an integer h = n * {0} + 1 where n: {1, 2, 3...}", SECTOR_SIZE));
            if (input.Length != NumXValues * NumZValues)
                throw new InvalidContentException("The number of bytes in the heightmap is not equal to the product of the Height and Width properties.");
            if (XZScale <= 0.0f)
                throw new InvalidContentException("XZScale property must be greater than 0.");
            if (YScale <= 0.0f)
                throw new InvalidContentException("YScale property must be greater than 0.");

            mapXRadius = XZScale * (float)(CroppedXValues - 1) / 2.0f;
            mapZRadius = XZScale * (float)(CroppedZValues - 1) / 2.0f;

            VertexElement vePosition0 = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
            VertexElement veNormal0 = new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
            int vertexStride = 24; // This is set based on the above VertexElement composition.

            outputTC = new TerrainContent();
            outputTC.VertexCountAlongXAxis = CroppedXValues;
            outputTC.VertexCountAlongZAxis = CroppedZValues;
            outputTC.SectorSize = SECTOR_SIZE;
            outputTC.XZScale = XZScale;
            outputTC.VertexBufferContent = new VertexBufferContent(CroppedXValues * CroppedZValues * vertexStride);
            outputTC.VertexBufferContent.VertexDeclaration.VertexElements.Add(vePosition0);
            outputTC.VertexBufferContent.VertexDeclaration.VertexElements.Add(veNormal0);
            outputTC.VertexBufferContent.VertexDeclaration.VertexStride = vertexStride;
            outputTC.TriangleCount = SECTOR_SIZE * SECTOR_SIZE * 2;
            outputTC.VertexCount = CroppedXValues * CroppedZValues;

            GeneratePositions(input);
            GenerateNormals();
            InitializeIndices();
            CreateMaterial();

            return outputTC;
        }


        private void InitializeIndices()
        {
            int[] indices = new int[SECTOR_SIZE * SECTOR_SIZE * 6];

            int i = 0;
            for (int row = 0; row < SECTOR_SIZE; row++)
            {
                for (int col = 0; col < SECTOR_SIZE; col++)
                {
                    indices[i++] = row * CroppedXValues + CroppedXValues + col;
                    indices[i++] = row * CroppedXValues + col;
                    indices[i++] = row * CroppedXValues + col + 1;

                    indices[i++] = row * CroppedXValues + CroppedXValues + col;
                    indices[i++] = row * CroppedXValues + col + 1;
                    indices[i++] = row * CroppedXValues + CroppedXValues + col + 1;
                }
            }

            outputTC.IndexCollection = new IndexCollection();
            outputTC.IndexCollection.AddRange(indices);
        }


        private void GeneratePositions(byte[] heightData)
        {
            const float STD_DEV = 1.7f;
            const float STD_DEV_SQ = STD_DEV * STD_DEV;
            int pixelRadius = (int)(Math.Ceiling(3.0f * STD_DEV));

            position = new Vector3[CroppedXValues, CroppedZValues];

            float x;
            float y;
            float z;
            byte[] xBytes;
            byte[] yBytes;
            byte[] zBytes;

            int sampleColumn;
            int sampleRow;
            float pixelDistSq;

            for (int row = 0; row < CroppedZValues; row++)
            {
                for (int col = 0; col < CroppedXValues; col++)
                {
                    x = -mapXRadius + col * XZScale;
                    z = -mapZRadius + row * XZScale;
                    y = YOffset;

                    //// Apply smoothing:
                    //for (int i = -pixelRadius; i <= pixelRadius; i++)
                    //{
                    //    for (int j = -pixelRadius; j <= pixelRadius; j++)
                    //    {
                    //        sampleRow = Math.Min(Math.Max(row + i, -CropZTop), CroppedZValues + CropZBottom - 1);
                    //        sampleColumn = Math.Min(Math.Max(col + j, -CropXLeft), CroppedXValues + CropXRight - 1);

                    //        pixelDistSq = (float)(i * i + j * j);
                    //        y += (float)(Math.Exp(-pixelDistSq / (2.0f * STD_DEV_SQ))) / (MathHelper.TwoPi * STD_DEV_SQ) * (float)(heightData[sampleColumn + CropXLeft + (sampleRow + CropZTop) * NumXValues]) * YScale;
                    //    }
                    //}
                    y += (float)(heightData[col + CropXLeft + (row + CropZTop) * NumXValues]) * YScale;

                    position[col, row] = new Vector3(x, y, z);

                    xBytes = System.BitConverter.GetBytes(x);
                    yBytes = System.BitConverter.GetBytes(y);
                    zBytes = System.BitConverter.GetBytes(z);

                    int vertStride = (int)(outputTC.VertexBufferContent.VertexDeclaration.VertexStride);

                    for (int i = 0; i < 4; i++)
                    {
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + i] = xBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 4 + i] = yBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 8 + i] = zBytes[i];
                    }
                }
            }
        }

        private void GenerateNormals()
        {
            normal = new Vector3[CroppedXValues, CroppedZValues];

            for (int row = 0; row < CroppedZValues; row++)
            {
                for (int col = 0; col < CroppedXValues; col++)
                {
                    int numTris = 0;
                    Vector3 normalSum = Vector3.Zero;

                    if (col > 0 && row > 0)
                    {
                        Vector3 s1 = position[col, row - 1] - position[col, row];
                        Vector3 s2 = position[col - 1, row] - position[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        numTris++;
                    }

                    if (col > 0 && row < CroppedZValues - 1)
                    {
                        Vector3 s1 = position[col - 1, row] - position[col, row];
                        Vector3 s2 = position[col - 1, row + 1] - position[col, row];
                        Vector3 s3 = position[col, row + 1] - position[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        normalSum += Vector3.Normalize(Vector3.Cross(s2, s3));
                        numTris += 2;
                    }

                    if (col < CroppedXValues - 1 && row < CroppedZValues - 1)
                    {
                        Vector3 s1 = position[col, row + 1] - position[col, row];
                        Vector3 s2 = position[col + 1, row] - position[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        numTris++;
                    }

                    if (col < CroppedXValues - 1 && row > 0)
                    {
                        Vector3 s1 = position[col + 1, row] - position[col, row];
                        Vector3 s2 = position[col + 1, row - 1] - position[col, row];
                        Vector3 s3 = position[col, row - 1] - position[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        normalSum += Vector3.Normalize(Vector3.Cross(s2, s3));
                        numTris += 2;
                    }

                    normal[col, row] = normalSum / (float)numTris;

                    byte[] xBytes = System.BitConverter.GetBytes(normal[col, row].X);
                    byte[] yBytes = System.BitConverter.GetBytes(normal[col, row].Y);
                    byte[] zBytes = System.BitConverter.GetBytes(normal[col, row].Z);

                    int vertStride = (int)(outputTC.VertexBufferContent.VertexDeclaration.VertexStride);

                    for (int i = 0; i < 4; i++)
                    {
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 12 + i] = xBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 16 + i] = yBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 20 + i] = zBytes[i];
                    }
                }
            }
        }


        private void GenerateNormals2()
        {
            Vector3 right;
            Vector3 down;
            Vector3 cross;

            int rightStartCol;
            int rightEndCol;
            int downStartRow;
            int downEndRow;

            normal = new Vector3[CroppedXValues, CroppedZValues];

            byte[] xBytes;
            byte[] yBytes;
            byte[] zBytes;

            for (int row = 0; row < CroppedZValues; row++)
            {
                for (int col = 0; col < CroppedXValues; col++)
                {
                    if (col == 0)
                    {
                        rightStartCol = 0;    // *
                        rightEndCol = 1;
                    }
                    else if (col == CroppedXValues - 1)
                    {
                        rightStartCol = CroppedXValues - 2;
                        rightEndCol = CroppedXValues - 1;    // *
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
                    else if (row == CroppedZValues - 1)
                    {
                        downStartRow = CroppedZValues - 2;
                        downEndRow = CroppedZValues - 1;  // *
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
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 12 + i] = xBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 16 + i] = yBytes[i];
                        outputTC.VertexBufferContent.VertexData[(col + row * CroppedXValues) * vertStride + 20 + i] = zBytes[i];
                    }
                }
            }
        }

        private void CreateMaterial()
        {
            using (XmlReader reader = XmlReader.Create(MaterialDataFilePath))
            {
                terrainMaterial = IntermediateSerializer.Deserialize<List<MaterialData>>(reader, null);
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

#if XBOX
            outputTC.MaterialContent = context.Convert<MaterialContent, MaterialContent>(emc, "MaterialProcessor");
#else
            outputTC.MaterialContent = context.Convert<MaterialContent, MaterialContent>(emc, "FxcMaterialProcessor");
#endif

        }

    }
}