﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Skelemator
{
    public class Terrain
    {
        public int VertexCountAlongXAxis;
        public int VertexCountAlongZAxis;
        public int SectorSize;
        public float XZScale;
        public VertexBuffer Vertices;
        public IndexBuffer Indices;
        public int TriangleCount;
        public int VertexCount;
        public Effect Effect;
        public object Tag;

        public int SectorVertexSpan
        {
            get
            {
                return SectorSize * (VertexCountAlongXAxis + 1) + 1;
            }
        }   // We assume the heightmap vertices are sliced parallel to the x-axis.


        public Vector3 TransformForGeometry
        {
            get
            {
                return new Vector3(
                -(float)(VertexCountAlongXAxis - 1) * XZScale / 2.0f,
                0.0f,
                -(float)(VertexCountAlongZAxis - 1) * XZScale / 2.0f);
            }
        }


        public float[,] GetGeometry()
        {
            float[,] heights = new float[VertexCountAlongXAxis, VertexCountAlongZAxis];

            VertexPositionNormal[] cpuVertices = new VertexPositionNormal[VertexCount];
            Vertices.GetData(cpuVertices);

            for (int x = 0; x < VertexCountAlongXAxis; x++)
            {
                for (int z = 0; z < VertexCountAlongZAxis; z++)
                {
                    heights[x, z] = cpuVertices[x + VertexCountAlongXAxis * z].Position.Y;
                }
            }

            return heights;
        }

    }


    struct VertexPositionNormal : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;


        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexPositionNormal.VertexDeclaration; }
        }

    }

}
