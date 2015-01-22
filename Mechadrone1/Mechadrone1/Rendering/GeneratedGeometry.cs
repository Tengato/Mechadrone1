using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using SlagformCommon;

namespace Mechadrone1
{
    static class GeneratedGeometry
    {
        public static MeshPart Sphere { get; private set; }
        public static MeshPart Box { get; private set; }

        public static BoundingSphere SphereBound { get; private set; }
        public static BoundingSphere BoxBound { get; private set; }

        static GeneratedGeometry()
        {
            Sphere = null;
            Box = null;
            SphereBound = new BoundingSphere(Vector3.Zero, 1.0f);
            BoxBound = new BoundingSphere(Vector3.Zero, SlagMath.SQRT_3);
        }

        public static void Initialize()
        {
            List<VertexPositionNormalTexture> shapesVertices = new List<VertexPositionNormalTexture>();
            List<short> shapesIndices = new List<short>();

            // Sphere.
            Sphere = new MeshPart();
            Sphere.VertexOffset = shapesVertices.Count;
            Sphere.StartIndex = shapesIndices.Count;

            const int SPHERE_NUM_SEGMENTS = 32;
            const int SPHERE_NUM_RINGS = 18;

            // Poles: note that there will be texture coordinate distortion as there is
            // not a unique point on the texture map to assign to the pole when mapping
            // a rectangular texture onto a sphere.
            VertexPositionNormalTexture topVertex = new VertexPositionNormalTexture(Vector3.Up, Vector3.Up, Vector2.Zero);
            VertexPositionNormalTexture bottomVertex = new VertexPositionNormalTexture(Vector3.Down, Vector3.Down, Vector2.UnitY);

            shapesVertices.Add(topVertex);

            float phiStep = MathHelper.Pi / SPHERE_NUM_RINGS;
            float thetaStep = MathHelper.TwoPi / SPHERE_NUM_SEGMENTS;

            // Compute vertices for each ring (do not count the poles as rings).
            for (int i = 1; i < SPHERE_NUM_RINGS; ++i)
            {
                float phi = i * phiStep;

                // Vertices of ring. Note that we're adding an extra vertex on each ring so the uv coords will wrap correctly.
                for (int j = 0; j <= SPHERE_NUM_SEGMENTS; ++j)
                {
                    float theta = j * thetaStep;

                    VertexPositionNormalTexture v;

                    // spherical to cartesian
                    v.Position.X = (float)(Math.Sin(phi) * Math.Cos(theta));
                    v.Position.Y = (float)(Math.Cos(phi));
                    v.Position.Z = (float)(Math.Sin(phi) * Math.Sin(theta));

                    v.Normal = v.Position;

                    v.TextureCoordinate.X = theta / MathHelper.TwoPi;
                    v.TextureCoordinate.Y = phi / MathHelper.Pi;

                    shapesVertices.Add(v);
                }
            }

            shapesVertices.Add(bottomVertex);

            // Compute indices for top ring.  The top ring was written first to the vertex buffer
            // and connects the top pole to the first ring.

            for(short j = 1; j <= SPHERE_NUM_SEGMENTS; ++j)
            {
                shapesIndices.Add(0);
                shapesIndices.Add((short)(j + 1));
                shapesIndices.Add(j);
            }

            // Compute indices for inner stacks (not connected to poles).

            // Offset the indices to the index of the first vertex in the first ring.
            // This is just skipping the top pole vertex.
            int baseIndex = 1;
            int verticesPerRing = SPHERE_NUM_SEGMENTS + 1;

            for (int i = 0; i < SPHERE_NUM_RINGS - 2; ++i)
            {
                for (int j = 0; j < SPHERE_NUM_SEGMENTS; ++j)
                {
                    shapesIndices.Add((short)(baseIndex + i * verticesPerRing + j));
                    shapesIndices.Add((short)(baseIndex + i * verticesPerRing + j + 1));
                    shapesIndices.Add((short)(baseIndex + (i + 1) * verticesPerRing + j));

                    shapesIndices.Add((short)(baseIndex + i * verticesPerRing + j + 1));
                    shapesIndices.Add((short)(baseIndex + (i + 1) * verticesPerRing + j + 1));
                    shapesIndices.Add((short)(baseIndex + (i + 1) * verticesPerRing + j));
                }
            }

            // Compute indices for bottom stack.  The bottom stack was written last to the vertex buffer
            // and connects the bottom pole to the bottom ring.

            short southPoleIndex = (short)(1 + verticesPerRing * (SPHERE_NUM_RINGS - 1));

            // Offset the indices to the index of the first vertex in the last ring.
            baseIndex = southPoleIndex - verticesPerRing;

            for (short j = 0; j < SPHERE_NUM_SEGMENTS; ++j)
            {
                shapesIndices.Add(southPoleIndex);
                shapesIndices.Add((short)(baseIndex + j));
                shapesIndices.Add((short)(baseIndex + j + 1));
            }

            Sphere.NumVertices = shapesVertices.Count - Sphere.VertexOffset;
            Sphere.PrimitiveCount = (shapesIndices.Count - Sphere.StartIndex) / 3;

            // Box.
            Box = new MeshPart();
            Box.VertexOffset = shapesVertices.Count;
            Box.StartIndex = shapesIndices.Count;

            VertexPositionNormalTexture[] box = new VertexPositionNormalTexture[24];
            Vector3[] orderOfFaces = { Vector3.Backward, Vector3.Forward, Vector3.Up, Vector3.Down, Vector3.Left, Vector3.Right };
            Vector2[] orderOfTexCoords = { Vector2.UnitY, Vector2.One, Vector2.UnitX, Vector2.Zero };

            for (int b = 0; b < 24; ++b)
            {
                box[b].Normal = orderOfFaces[b / 4];
                box[b].TextureCoordinate = orderOfTexCoords[b % 4];
            }

            // Back face (normal is backward vector (0, 0, -1,))
            box[0].Position = new Vector3(-1.0f, -1.0f, 1.0f);
            box[1].Position = new Vector3(+1.0f, -1.0f, 1.0f);
            box[2].Position = new Vector3(+1.0f, +1.0f, 1.0f);
            box[3].Position = new Vector3(-1.0f, +1.0f, 1.0f);

            // Front face
            box[4].Position = new Vector3(+1.0f, -1.0f, -1.0f);
            box[5].Position = new Vector3(-1.0f, -1.0f, -1.0f);
            box[6].Position = new Vector3(-1.0f, +1.0f, -1.0f);
            box[7].Position = new Vector3(+1.0f, +1.0f, -1.0f);

            // Top face
            box[8].Position =  new Vector3(-1.0f, 1.0f, +1.0f);
            box[9].Position =  new Vector3(+1.0f, 1.0f, +1.0f);
            box[10].Position = new Vector3(+1.0f, 1.0f, -1.0f);
            box[11].Position = new Vector3(-1.0f, 1.0f, -1.0f);

            // Bottom face
            box[12].Position = new Vector3(-1.0f, -1.0f, -1.0f);
            box[13].Position = new Vector3(+1.0f, -1.0f, -1.0f);
            box[14].Position = new Vector3(+1.0f, -1.0f, +1.0f);
            box[15].Position = new Vector3(-1.0f, -1.0f, +1.0f);

            // Left face
            box[16].Position = new Vector3(-1.0f, -1.0f, -1.0f);
            box[17].Position = new Vector3(-1.0f, -1.0f, +1.0f);
            box[18].Position = new Vector3(-1.0f, +1.0f, +1.0f);
            box[19].Position = new Vector3(-1.0f, +1.0f, -1.0f);

            // Right face
            box[20].Position = new Vector3(1.0f, -1.0f, +1.0f);
            box[21].Position = new Vector3(1.0f, -1.0f, -1.0f);
            box[22].Position = new Vector3(1.0f, +1.0f, -1.0f);
            box[23].Position = new Vector3(1.0f, +1.0f, +1.0f);

            shapesVertices.AddRange(box);

            for (int b = 0; b < 21; b += 4)
            {
                shapesIndices.Add((short)b);
                shapesIndices.Add((short)(b + 2));
                shapesIndices.Add((short)(b + 1));

                shapesIndices.Add((short)(b));
                shapesIndices.Add((short)(b + 3));
                shapesIndices.Add((short)(b + 2));
            }

            Box.NumVertices = shapesVertices.Count - Box.VertexOffset;
            Box.PrimitiveCount = (shapesIndices.Count - Box.StartIndex) / 3;

            VertexBuffer shapesVB = new VertexBuffer(SharedResources.Game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, shapesVertices.Count, BufferUsage.None);
            shapesVB.SetData(shapesVertices.ToArray());
            IndexBuffer shapesIB = new IndexBuffer(SharedResources.Game.GraphicsDevice, IndexElementSize.SixteenBits, shapesIndices.Count, BufferUsage.None);
            shapesIB.SetData(shapesIndices.ToArray());

            Sphere.IndexBuffer = shapesIB;
            Sphere.VertexBuffer = shapesVB;
            Box.IndexBuffer = shapesIB;
            Box.VertexBuffer = shapesVB;
        }
    }
}
