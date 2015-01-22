using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System;

namespace Mechadrone1
{
    /// <summary>
    /// ImplicitBoundingBoxNode are used as the branching node in the scene graph's BVH. Their BV is implied by the tree
    /// content, so it needs to be recomputed often.
    /// </summary>
    class ImplicitBoundingBoxNode : BoundingBoxNode
    {
        // ImplicitBoundingBoxNodes must have children. The children must either all be ImplicitBoundingBoxNodes, or none
        // may be ImplicitBoundingBoxNodes. An ImplicitBoundingBoxNode is a leaf when the children are not ImplicitBoundingBoxNodes.
        // If the node is not a leaf, it should have at least two children (otherwise it's providing no value.)
        public bool IsLeaf { get; private set; }

        public ImplicitBoundingBoxNode(bool isLeaf)
        {
            Bound = new BoundingBox();
            IsLeaf = isLeaf;
        }

        public ImplicitBoundingBoxNode(ImplicitBoundingBoxNode orig)
            : base(orig)
        {
            Bound = orig.Bound;
            IsLeaf = orig.IsLeaf;
        }

        protected override void RemoveChild(SceneNode child)
        {
            base.RemoveChild(child);

            if (mChildren.Count == 0)
            {
                if (IsLeaf)
                {
                    Release();
                }
                else
                {
                    throw new InvalidOperationException("An ImplicitBoundingBoxNode that is not a leaf should never have fewer than two children.");
                }
            }
            else if (mChildren.Count == 1 && !IsLeaf)
            {
                mParent.AddChild(mChildren.Single());
                Release();
            }
            else
            {
                RecalculateBound();
            }
        }

        // Precondition: The children's bounds are accurate. It will cascade upwards to parent nodes.
        public void RecalculateBound()
        {
            BoundingBox newBound;
            if (IsLeaf)
            {
                List<BoundingBox> explicitBVs = FindExplicitBoundingVolumes();

                newBound = explicitBVs[0];

                for (int b = 1; b < explicitBVs.Count; ++b)
                {
                    newBound = BoundingBox.CreateMerged(newBound, explicitBVs[b]);
                }
            }
            else
            {
                IEnumerable<ImplicitBoundingBoxNode> childBoxes = mChildren.OfType<ImplicitBoundingBoxNode>();
                newBound = childBoxes.First().Bound;

                foreach (ImplicitBoundingBoxNode childBox in childBoxes.Skip(1))
                {
                    newBound = BoundingBox.CreateMerged(newBound, childBox.Bound);
                }
            }

            SetBoundCascading(newBound);
        }

        // This method will recompute the Bound without precondition. It will not cascade upwards to parent nodes, because
        // it is intended to be called recursively.
        public void BottomUpRecalculateBound()
        {
            List<BoundingBox> childBounds = new List<BoundingBox>();
            foreach (SceneNode child in mChildren)
            {
                ImplicitBoundingBoxNode implicitBVChild = child as ImplicitBoundingBoxNode;
                if (implicitBVChild != null)
                {
                    implicitBVChild.BottomUpRecalculateBound();
                    childBounds.Add(implicitBVChild.Bound);
                }
                else
                {
                    childBounds.AddRange(child.FindExplicitBoundingVolumes());
                }
            }

            BoundingBox combinedBound = childBounds.First();

            foreach (BoundingBox childBox in childBounds.Skip(1))
            {
                combinedBound = BoundingBox.CreateMerged(combinedBound, childBox);
            }

            Bound = combinedBound;
        }

        public void SetBoundCascading(BoundingBox newBound)
        {
            if (newBound != Bound)
            {
                Bound = newBound;
                ImplicitBoundingBoxNode parentBox = mParent as ImplicitBoundingBoxNode;

                if (parentBox != null)
                {
                    parentBox.RecalculateBound();
                }
            }
        }


        public void InsertNodeIntoBVH(ImplicitBoundingBoxNode aabb)
        {
            // See http://tog.acm.org/resources/RTNews/html/rtnews3a.html#art6 for algorithm details.
            BVHInsertion bestInsertion = FindBestLocalInsertion(aabb, 0.0f);
            BVHInsertion bestDeepInsertion = FindBestDeepInsertion(aabb, bestInsertion.LocationInheritance, bestInsertion.Cost);

            if (bestDeepInsertion.Cost < bestInsertion.Cost)
                bestInsertion = bestDeepInsertion;

            // Perform insertion.
            switch (bestInsertion.Method)
            {
                case BVHInsertion.InsertionMethod.AsSibling:
                    ImplicitBoundingBoxNode copyOfLocation = new ImplicitBoundingBoxNode(bestInsertion.Location);
                    bestInsertion.Location.IsLeaf = false;
                    bestInsertion.Location.mChildren = new List<SceneNode>();
                    bestInsertion.Location.AddChild(copyOfLocation);
                    bestInsertion.Location.AddChild(aabb);
                    break;
                case BVHInsertion.InsertionMethod.AsChild:
                    bestInsertion.Location.AddChild(aabb);
                    break;
            }

            // The Bound must be recomputed after this operation.
            bestInsertion.Location.RecalculateBound();
        }

        private BVHInsertion FindBestLocalInsertion(ImplicitBoundingBoxNode aabb, float inheritedCost)
        {
            float newArea = GetAABBArea(BoundingBox.CreateMerged(Bound, aabb.Bound));
            float asSiblingCost = 2.0f * newArea;
            float areaDiff = newArea - GetAABBArea(Bound);

            if (IsLeaf)
            {
                BVHInsertion leafInsertion = new BVHInsertion(this, BVHInsertion.InsertionMethod.AsSibling);
                leafInsertion.Cost = asSiblingCost + inheritedCost;
                leafInsertion.LocationInheritance = inheritedCost;
                return leafInsertion;
            }

            float asChildCost = mChildren.Count * areaDiff + newArea;

            BVHInsertion bestInsertion;
            if (asSiblingCost < asChildCost)
            {
                bestInsertion = new BVHInsertion(this, BVHInsertion.InsertionMethod.AsSibling);
                bestInsertion.Cost = asSiblingCost + inheritedCost;
            }
            else
            {
                bestInsertion = new BVHInsertion(this, BVHInsertion.InsertionMethod.AsChild);
                bestInsertion.Cost = asChildCost + inheritedCost;
            }
            bestInsertion.LocationInheritance = mChildren.Count * areaDiff + inheritedCost;

            return bestInsertion;
        }

        private BVHInsertion FindBestDeepInsertion(ImplicitBoundingBoxNode aabb, float inheritance, float costToBeat)
        {
            if (IsLeaf)
                return BVHInsertion.WorstInsertion;

            // Find best local insertion cost for each child:
            BVHInsertion bestChildInsertion = BVHInsertion.WorstInsertion;

            foreach (ImplicitBoundingBoxNode child in mChildren.OfType<ImplicitBoundingBoxNode>())
            {
                BVHInsertion altChildInsertion = child.FindBestLocalInsertion(aabb, inheritance);

                if (altChildInsertion.Cost < bestChildInsertion.Cost)
                    bestChildInsertion = altChildInsertion;
            }

            // Which child had the best local cost? Was it better than our local cost? If so, update the costToBeat.
            if (bestChildInsertion.Cost < costToBeat)
                costToBeat = bestChildInsertion.Cost;

            // Finally, explore the deeper insertion costs of that child...
            // Make sure the child's inheritance permits improvement before recursing.
            if (bestChildInsertion.LocationInheritance < costToBeat)
            {
                BVHInsertion bestChildDeepInsertion = bestChildInsertion.Location.FindBestDeepInsertion(aabb, bestChildInsertion.LocationInheritance, costToBeat);

                // Compare the deep insertion cost with our best so far...
                if (bestChildDeepInsertion.Cost < bestChildInsertion.Cost)
                    return bestChildDeepInsertion;
            }

            return bestChildInsertion;
        }

        private float GetAABBArea(BoundingBox box)
        {
            Vector3 diff = box.Max - box.Min;
            return diff.X * (diff.Y + diff.Z) + diff.Y * diff.Z;
        }

        /*
        [Obsolete]
        private BVHInsertion FindBestInsertion(BoundingBoxNode aabb, float inheritedCost)
        {
            BVHInsertion bestInsertion = FindBestLocalInsertion(aabb, inheritedCost);
            float inheritance = bestInsertion.LocationInheritance;

            // Find best local insertion cost for each child:
            BVHInsertion bestChildInsertion = new BVHInsertion(null, BVHInsertion.InsertionMethod.AsChild);
            bestChildInsertion.Cost = Single.MaxValue;
            bestChildInsertion.LocationInheritance = Single.MaxValue;

            foreach (BoundingBoxNode child in mChildren.OfType<BoundingBoxNode>())
            {
                BVHInsertion altChildInsertion = child.FindBestLocalInsertion(aabb, inheritance);

                if (altChildInsertion.Cost < bestChildInsertion.Cost)
                    bestChildInsertion = altChildInsertion;
            }

            // Which child had the best local cost? Was it better than our local cost? If so, explore the deeper insertion costs of that child...
            if (bestChildInsertion.Cost < bestInsertion.Cost)
                bestInsertion = bestChildInsertion;

            if (bestChildInsertion.LocationInheritance < bestInsertion.Cost)
            {
                bestChildInsertion = bestChildInsertion.Location.FindBestInsertion(aabb, inheritance);

                // Compare the deep insertion cost with our best so far...
                if (bestChildInsertion.Cost < bestInsertion.Cost)
                    bestInsertion = bestChildInsertion;
            }

            return bestInsertion;
        }
        */
    }
}
