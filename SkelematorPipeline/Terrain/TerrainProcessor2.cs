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
    [ContentProcessor(DisplayName = "Skelemator Terrain Processor2")]
    public class TerrainProcessor2 : ContentProcessor<byte[], TerrainContent>
    {
        const int SECTOR_SIZE = 64;

        public int NumSubdivisions { get; set; }
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

        private int mCroppedXValues
        {
            get
            {
                return NumXValues - CropXRight - CropXLeft;
            }
        }

        private int mCroppedZValues
        {
            get
            {
                return NumZValues - CropZTop - CropZBottom;
            }
        }


        // Location of an XML file that describes which materials to use.
        public virtual string MaterialDataFilePath { get; set; }

        private List<MaterialData> mTerrainMaterial;

        private Vector3[,] mPosition;
        private Vector3[,] mNormal;

        private float mMapXRadius;
        private float mMapZRadius;

        private TerrainContent mOutputTC;
        private ContentProcessorContext mContext;

        public override TerrainContent Process(byte[] input, ContentProcessorContext context)
        {
            this.mContext = context;

            if (mCroppedXValues < 2 || mCroppedXValues % SECTOR_SIZE != 1)
                throw new InvalidContentException(String.Format("NumXValues property value after cropping must be an integer w = n * {0} + 1 where n: {1, 2, 3...}", SECTOR_SIZE));
            if (mCroppedZValues < 2 || mCroppedZValues % SECTOR_SIZE != 1)
                throw new InvalidContentException(String.Format("NumZValues property value after cropping must be an integer h = n * {0} + 1 where n: {1, 2, 3...}", SECTOR_SIZE));
            if (input.Length != NumXValues * NumZValues)
                throw new InvalidContentException("The number of bytes in the heightmap is not equal to the product of the Height and Width properties.");
            if (XZScale <= 0.0f)
                throw new InvalidContentException("XZScale property must be greater than 0.");
            if (YScale <= 0.0f)
                throw new InvalidContentException("YScale property must be greater than 0.");


            VertexElement vePosition0 = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
            VertexElement veNormal0 = new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
            int vertexStride = 24; // This is set based on the above VertexElement composition.

            mOutputTC = new TerrainContent();
            mOutputTC.VertexCountAlongXAxis = (mCroppedXValues - 1) * (1 << NumSubdivisions) + 1;
            mOutputTC.VertexCountAlongZAxis = (mCroppedZValues - 1) * (1 << NumSubdivisions) + 1;
            mOutputTC.SectorSize = SECTOR_SIZE;
            mOutputTC.XZScale = XZScale / (float)(1 << NumSubdivisions);
            mOutputTC.VertexCount = mOutputTC.VertexCountAlongXAxis * mOutputTC.VertexCountAlongZAxis;
            mOutputTC.VertexBufferContent = new VertexBufferContent(mOutputTC.VertexCount * vertexStride);
            mOutputTC.VertexBufferContent.VertexDeclaration.VertexElements.Add(vePosition0);
            mOutputTC.VertexBufferContent.VertexDeclaration.VertexElements.Add(veNormal0);
            mOutputTC.VertexBufferContent.VertexDeclaration.VertexStride = vertexStride;
            mOutputTC.TriangleCount = SECTOR_SIZE * SECTOR_SIZE * 2;

            mMapXRadius = mOutputTC.XZScale * (float)(mOutputTC.VertexCountAlongXAxis - 1) / 2.0f;
            mMapZRadius = mOutputTC.XZScale * (float)(mOutputTC.VertexCountAlongZAxis - 1) / 2.0f;

            GeneratePositions(input);
            GenerateNormals();
            InitializeIndices();

            context.AddDependency(Path.Combine(Environment.CurrentDirectory, MaterialDataFilePath));
            CreateMaterial();

            return mOutputTC;
        }


        private void InitializeIndices()
        {
            int[] indices = new int[SECTOR_SIZE * SECTOR_SIZE * 6];

            int i = 0;
            for (int row = 0; row < SECTOR_SIZE; row++)
            {
                for (int col = 0; col < SECTOR_SIZE; col++)
                {
                    indices[i++] = row * mOutputTC.VertexCountAlongXAxis + mOutputTC.VertexCountAlongXAxis + col;
                    indices[i++] = row * mOutputTC.VertexCountAlongXAxis + col;
                    indices[i++] = row * mOutputTC.VertexCountAlongXAxis + col + 1;

                    indices[i++] = row * mOutputTC.VertexCountAlongXAxis + mOutputTC.VertexCountAlongXAxis + col;
                    indices[i++] = row * mOutputTC.VertexCountAlongXAxis + col + 1;
                    indices[i++] = row * mOutputTC.VertexCountAlongXAxis + mOutputTC.VertexCountAlongXAxis + col + 1;
                }
            }

            mOutputTC.IndexCollection = new IndexCollection();
            mOutputTC.IndexCollection.AddRange(indices);
        }


        private void GeneratePositions(byte[] heightData)
        {
            float[,] prevHeights = new float[mCroppedXValues, mCroppedZValues];
            float[,] currHeights = null;

            for (int row = 0; row < mCroppedZValues; ++row)
            {
                for (int col = 0; col < mCroppedXValues; ++col)
                {
                    prevHeights[col, row] = YOffset + (float)(heightData[col + CropXLeft + (row + CropZTop) * NumXValues]) * YScale;
                }
            }

            int xAxisVertices = mCroppedXValues;
            int zAxisVertices = mCroppedZValues;

            for (int s = 1; s <= NumSubdivisions; ++s)
            {
                xAxisVertices = (xAxisVertices - 1) * 2 + 1;
                zAxisVertices = (zAxisVertices - 1) * 2 + 1;

                currHeights = new float[xAxisVertices, zAxisVertices];

                // New face points
                for (int row = 1; row < zAxisVertices; row += 2)
                {
                    for (int col = 1; col < xAxisVertices; col += 2)
                    {
                        currHeights[col, row] = (prevHeights[(col - 1) / 2, (row - 1) / 2] +
                                                 prevHeights[(col + 1) / 2, (row - 1) / 2] +
                                                 prevHeights[(col - 1) / 2, (row + 1) / 2] +
                                                 prevHeights[(col + 1) / 2, (row + 1) / 2]) / 4.0f;
                    }
                }

                // New boundary row edge points
                for (int col = 1; col < xAxisVertices; col += 2)
                {
                    currHeights[col, 0] = (prevHeights[(col - 1) / 2, 0] +
                                           prevHeights[(col + 1) / 2, 0] +
                                           currHeights[col, 1]) / 3.0f;

                    currHeights[col, zAxisVertices - 1] = (prevHeights[(col - 1) / 2, (zAxisVertices - 1) / 2] +
                                                           prevHeights[(col + 1) / 2, (zAxisVertices - 1) / 2] +
                                                           currHeights[col, zAxisVertices - 2]) / 3.0f;
                }

                // New internal even row edge points
                for (int row = 2; row < zAxisVertices - 1; row += 2)
                {
                    for (int col = 1; col < xAxisVertices; col += 2)
                    {
                        currHeights[col, row] = (prevHeights[(col - 1) / 2, row / 2] +
                                                 prevHeights[(col + 1) / 2, row / 2] +
                                                 currHeights[col, row - 1] +
                                                 currHeights[col, row + 1]) / 4.0f;
                    }
                }

                // New boundary column edge points
                for (int row = 1; row < zAxisVertices; row += 2)
                {
                    currHeights[0, row] = (prevHeights[0, (row - 1) / 2] +
                                           prevHeights[0, (row + 1) / 2] +
                                           currHeights[1, row]) / 3.0f;

                    currHeights[zAxisVertices - 1, row] = (prevHeights[(xAxisVertices - 1) / 2, (row - 1) / 2] +
                                                           prevHeights[(xAxisVertices - 1) / 2, (row + 1) / 2] +
                                                           currHeights[xAxisVertices - 2, row]) / 3.0f;
                }

                // New internal even column edge points
                for (int row = 1; row < zAxisVertices; row += 2)
                {
                    for (int col = 2; col < xAxisVertices - 1; col += 2)
                    {
                        currHeights[col, row] = (prevHeights[col / 2, (row - 1) / 2] +
                                                 prevHeights[col / 2, (row + 1) / 2] +
                                                 currHeights[col - 1, row] +
                                                 currHeights[col + 1, row]) / 4.0f;
                    }
                }

                // Reposition corner points:
                currHeights[0, 0] = CatmullAverage(currHeights[1, 1], (currHeights[0, 1] + currHeights[1, 0]) / 2.0f, prevHeights[0, 0]);
                currHeights[0, zAxisVertices - 1] = CatmullAverage(currHeights[1, zAxisVertices - 2], (currHeights[0, zAxisVertices - 2] + currHeights[1, zAxisVertices - 2]) / 2.0f, prevHeights[0, (zAxisVertices - 1) / 2]);
                currHeights[xAxisVertices - 1, 0] = CatmullAverage(currHeights[xAxisVertices - 2, 1], (currHeights[xAxisVertices - 1, 1] + currHeights[xAxisVertices - 2, 0]) / 2.0f, prevHeights[(xAxisVertices - 1) / 2, 0]);
                currHeights[xAxisVertices - 1, zAxisVertices - 1] = CatmullAverage(currHeights[xAxisVertices - 2, zAxisVertices - 2], (currHeights[xAxisVertices - 2, zAxisVertices - 1] + currHeights[xAxisVertices - 1, zAxisVertices - 2]) / 2.0f, prevHeights[(xAxisVertices - 1) / 2, (zAxisVertices - 1) / 2]);

                // Reposition original edge row points:
                for (int col = 2; col < xAxisVertices - 1; col += 2)
                {
                    currHeights[col, 0] = CatmullAverage((currHeights[col - 1, 1] + currHeights[col + 1, 1]) / 2.0f,
                                                         (currHeights[col - 1, 0] + currHeights[col + 1, 0] + currHeights[col, 1]) / 3.0f,
                                                         prevHeights[col / 2, 0]);

                    currHeights[col, zAxisVertices - 1] = CatmullAverage((currHeights[col - 1, zAxisVertices - 2] + currHeights[col + 1, zAxisVertices - 2]) / 2.0f,
                                                         (currHeights[col - 1, zAxisVertices - 1] + currHeights[col + 1, zAxisVertices - 1] + currHeights[col, zAxisVertices - 2]) / 3.0f,
                                                         prevHeights[col / 2, (zAxisVertices - 1) / 2]);
                }

                // Reposition original edge column points:
                for (int row = 2; row < zAxisVertices - 1; row += 2)
                {
                    currHeights[0, row] = CatmullAverage((currHeights[1, row - 1] + currHeights[1, row + 1]) / 2.0f,
                                                         (currHeights[0, row - 1] + currHeights[0, row + 1] + currHeights[1, row]) / 3.0f,
                                                         prevHeights[0, row / 2]);

                    currHeights[xAxisVertices - 1, row] = CatmullAverage((currHeights[xAxisVertices - 2, row - 1] + currHeights[xAxisVertices - 2, row + 1]) / 2.0f,
                                                         (currHeights[xAxisVertices - 1, row - 1] + currHeights[xAxisVertices - 1, row + 1] + currHeights[xAxisVertices - 2, row]) / 3.0f,
                                                         prevHeights[(xAxisVertices - 1) / 2, row / 2]);
                }

                // Reposition original internal points
                for (int row = 2; row < zAxisVertices - 1; row += 2)
                {
                    for (int col = 2; col < xAxisVertices - 1; col += 2)
                    {
                        currHeights[col, row] = CatmullAverage((currHeights[col - 1, row - 1] +
                                                                currHeights[col - 1, row + 1] +
                                                                currHeights[col + 1, row - 1] +
                                                                currHeights[col + 1, row + 1]) / 4.0f,
                                                               (currHeights[col - 1, row] +
                                                                currHeights[col + 1, row] +
                                                                currHeights[col, row - 1] +
                                                                currHeights[col, row + 1]) / 4.0f,
                                                                prevHeights[col / 2, row / 2]);
                    }
                }

                prevHeights = currHeights;
            }

            mPosition = new Vector3[xAxisVertices, zAxisVertices];
            int vertStride = (int)(mOutputTC.VertexBufferContent.VertexDeclaration.VertexStride);
            
            for (int row = 0; row < zAxisVertices; ++row)
            {
                for (int col = 0; col < xAxisVertices; ++col)
                {
                    float x = -mMapXRadius + (float)col * mOutputTC.XZScale;
                    float z = -mMapZRadius + (float)row * mOutputTC.XZScale;

                    mPosition[col, row] = new Vector3(x, currHeights[col, row], z);

                    byte[] xBytes = System.BitConverter.GetBytes(x);
                    byte[] yBytes = System.BitConverter.GetBytes(currHeights[col, row]);
                    byte[] zBytes = System.BitConverter.GetBytes(z);

                    for (int i = 0; i < 4; i++)
                    {
                        mOutputTC.VertexBufferContent.VertexData[(col + row * xAxisVertices) * vertStride + i] = xBytes[i];
                        mOutputTC.VertexBufferContent.VertexData[(col + row * xAxisVertices) * vertStride + 4 + i] = yBytes[i];
                        mOutputTC.VertexBufferContent.VertexData[(col + row * xAxisVertices) * vertStride + 8 + i] = zBytes[i];
                    }
                }
            }
        }

        private float CatmullAverage(float faceAvg, float edgeAvg, float originalVal)
        {
            return (faceAvg + 2.0f * edgeAvg + originalVal) / 4.0f;
        }

        private void GenerateNormals()
        {
            mNormal = new Vector3[mOutputTC.VertexCountAlongXAxis, mOutputTC.VertexCountAlongZAxis];

            for (int row = 0; row < mOutputTC.VertexCountAlongZAxis; row++)
            {
                for (int col = 0; col < mOutputTC.VertexCountAlongXAxis; col++)
                {
                    int numTris = 0;
                    Vector3 normalSum = Vector3.Zero;

                    if (col > 0 && row > 0)
                    {
                        Vector3 s1 = mPosition[col, row - 1] - mPosition[col, row];
                        Vector3 s2 = mPosition[col - 1, row] - mPosition[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        numTris++;
                    }

                    if (col > 0 && row < mOutputTC.VertexCountAlongZAxis - 1)
                    {
                        Vector3 s1 = mPosition[col - 1, row] - mPosition[col, row];
                        Vector3 s2 = mPosition[col - 1, row + 1] - mPosition[col, row];
                        Vector3 s3 = mPosition[col, row + 1] - mPosition[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        normalSum += Vector3.Normalize(Vector3.Cross(s2, s3));
                        numTris += 2;
                    }

                    if (col < mOutputTC.VertexCountAlongXAxis - 1 && row < mOutputTC.VertexCountAlongZAxis - 1)
                    {
                        Vector3 s1 = mPosition[col, row + 1] - mPosition[col, row];
                        Vector3 s2 = mPosition[col + 1, row] - mPosition[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        numTris++;
                    }

                    if (col < mOutputTC.VertexCountAlongXAxis - 1 && row > 0)
                    {
                        Vector3 s1 = mPosition[col + 1, row] - mPosition[col, row];
                        Vector3 s2 = mPosition[col + 1, row - 1] - mPosition[col, row];
                        Vector3 s3 = mPosition[col, row - 1] - mPosition[col, row];
                        normalSum += Vector3.Normalize(Vector3.Cross(s1, s2));
                        normalSum += Vector3.Normalize(Vector3.Cross(s2, s3));
                        numTris += 2;
                    }

                    mNormal[col, row] = normalSum / (float)numTris;

                    byte[] xBytes = System.BitConverter.GetBytes(mNormal[col, row].X);
                    byte[] yBytes = System.BitConverter.GetBytes(mNormal[col, row].Y);
                    byte[] zBytes = System.BitConverter.GetBytes(mNormal[col, row].Z);

                    int vertStride = (int)(mOutputTC.VertexBufferContent.VertexDeclaration.VertexStride);

                    for (int i = 0; i < 4; i++)
                    {
                        mOutputTC.VertexBufferContent.VertexData[(col + row * mOutputTC.VertexCountAlongXAxis) * vertStride + 12 + i] = xBytes[i];
                        mOutputTC.VertexBufferContent.VertexData[(col + row * mOutputTC.VertexCountAlongXAxis) * vertStride + 16 + i] = yBytes[i];
                        mOutputTC.VertexBufferContent.VertexData[(col + row * mOutputTC.VertexCountAlongXAxis) * vertStride + 20 + i] = zBytes[i];
                    }
                }
            }
        }

        private void CreateMaterial()
        {
            using (XmlReader reader = XmlReader.Create(MaterialDataFilePath))
            {
                mTerrainMaterial = IntermediateSerializer.Deserialize<List<MaterialData>>(reader, null);
            }

            MaterialData tmSingle = mTerrainMaterial.Single();

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

            mOutputTC.Tag = tmSingle.HandlingFlags;

#if XBOX
            outputTC.MaterialContent = context.Convert<MaterialContent, MaterialContent>(emc, "MaterialProcessor");
#else
            mOutputTC.MaterialContent = mContext.Convert<MaterialContent, MaterialContent>(emc, "FxcMaterialProcessor");
#endif

        }

    }
}