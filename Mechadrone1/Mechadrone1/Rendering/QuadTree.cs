using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Gameplay;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Rendering
{
    class QuadTree
    {

        const int NUM_TREE_LEVELS = 8;

        QuadTreeNode[][] levelNodes;
        public Matrix WorldToQuadTreeTransform;


        public QuadTree(BoundingBox worldExtents)
        {
            WorldToQuadTreeTransform = Matrix.CreateTranslation(-worldExtents.Min) *
                Matrix.CreateScale(
                256.0f / (worldExtents.Max.X - worldExtents.Min.X),
                32.0f / (worldExtents.Max.Y - worldExtents.Min.Y),
                256.0f / (worldExtents.Max.Z - worldExtents.Min.Z));

            levelNodes = new QuadTreeNode[NUM_TREE_LEVELS][];

            for (int i = 0; i < NUM_TREE_LEVELS; i++)
            {
                int levelDimension = (1 << i);
                int nodeCount = levelDimension * levelDimension;

                levelNodes[i] = new QuadTreeNode[nodeCount];

                int levelIndex = 0;
                for (int z = 0; z < levelDimension; z++)
                {
                    for (int x = 0; x < levelDimension; x++)
                    {
                        levelNodes[i][levelIndex] = new QuadTreeNode();

                        levelIndex++;
                    }
                }
            }

            // Set parent/child relationships:
            for (int i = 0; i < NUM_TREE_LEVELS; i++)
            {
                int levelDimension = (1 << i);

                int levelIndex = 0;
                for (int z = 0; z < levelDimension; z++)
                {
                    for (int x = 0; x < levelDimension; x++)
                    {
                        levelNodes[i][levelIndex].Initialize(
                            GetNodeFromLevelXZ(i - 1, (x >> 1), (z >> 1)),
                            GetNodeFromLevelXZ(i + 1, (x << 1), (z << 1)),
                            GetNodeFromLevelXZ(i + 1, (x << 1) + 1, (z << 1)),
                            GetNodeFromLevelXZ(i + 1, (x << 1), (z << 1) + 1),
                            GetNodeFromLevelXZ(i + 1, (x << 1) + 1, (z << 1) + 1));

                        levelIndex++;
                    }
                }
            }
        }


        public void AddOrUpdateSceneObject(ISceneObject obj)
        {
            if (obj.QuadTree != this)
                obj.QuadTree = this;

            QuadTreeNode node = FindTreeNode(obj.QuadTreeBoundingBox);

            node.AddOrUpdateMember(obj);
        }


        public List<ISceneObject> Search(BoundingBox worldRect, BoundingFrustum worldFrustum)
        {
            List<ISceneObject> results = new List<ISceneObject>();

            QuadTreeRect byteRect = QuadTreeRect.CreateFromBoundingBox(worldRect, WorldToQuadTreeTransform);

            uint searchYMask = byteRect.YMask;

            bool continueSearch = true;
            int level = 0;

            while (level < NUM_TREE_LEVELS && continueSearch)
            {
                int shiftCount = NUM_TREE_LEVELS - level;
                QuadTreeRect localRect = new QuadTreeRect();
                localRect.X0 = byteRect.X0 >> shiftCount;
                localRect.X1 = byteRect.X1 >> shiftCount;
                localRect.Z0 = byteRect.Z0 >> shiftCount;
                localRect.Z1 = byteRect.Z1 >> shiftCount;
                localRect.Y0 = 0;
                localRect.Y1 = 0;

                // do not continue unless a populated node is found
                continueSearch = false;

                for (int z = localRect.Z0; z <= localRect.Z1; z++)
                {
                    for (int x = localRect.X0; x <= localRect.X1; x++)
                    {
                        QuadTreeNode node = GetNodeFromLevelXZ(level, x, z);

                        if ((node.YMask & searchYMask) > 0)
                        {
                            // a populated node has been found
                            continueSearch = true;

                            // search all the edge cells with the full world rectangle,
                            // because objects in these cells may lie outside of the search
                            // area.
                            if (z == localRect.Z0
                                || z == localRect.Z1
                                || x == localRect.X0
                                || x == localRect.X1)
                            {
                                // test all members of this node against the world rect
                                results.AddRange(node.SearchLocalMembers(
                                    searchYMask,
                                    worldRect,
                                    worldFrustum));
                            }
                            else
                            {
                                // test all members of this node against 
                                // the world Y extents only
                                results.AddRange(node.SearchLocalMembers(
                                    searchYMask,
                                    worldFrustum));
                            }
                        }
                    }
                }

                // step up to the next level of the tree
                level++;
            }

            return results;
        }


        private QuadTreeNode FindTreeNode(QuadTreeRect worldByteRect)
        {
            int level;
            int levelX;
            int levelZ;

            FindTreeNodeInfo(worldByteRect, out level, out levelX, out levelZ);

            return GetNodeFromLevelXZ(level, levelX, levelZ);
        }


        private void FindTreeNodeInfo(QuadTreeRect worldByteRect, out int level, out int levelX, out int levelZ)
        {
            int xPattern = worldByteRect.X0 ^ worldByteRect.X1;
            int zPattern = worldByteRect.Z0 ^ worldByteRect.Z1;

            int bitPattern = Math.Max(xPattern, zPattern);
            int highBit = bitPattern > 0 ? (int)(Math.Log((double)bitPattern, 2.0d)) : 0;

            level = NUM_TREE_LEVELS - 1 - highBit;

            levelX = worldByteRect.X1 >> (highBit + 1);
            levelZ = worldByteRect.Z1 >> (highBit + 1);
        }


        private QuadTreeNode GetNodeFromLevelXZ(int level, int x, int z)
        {
            if (level >= 0 && level < NUM_TREE_LEVELS)
                return levelNodes[level][(z << level) + x];

            return null;
        }

    }
}
