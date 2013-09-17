using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mechadrone1.Gameplay;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Rendering
{
    class QuadTreeNode
    {
        QuadTreeNode[] children;
        QuadTreeNode parent;
        List<ISceneObject> sceneObjects;
        public uint YMask;
        uint yLocalMask;


        internal void Initialize(QuadTreeNode parent, QuadTreeNode child0, QuadTreeNode child1, QuadTreeNode child2, QuadTreeNode child3)
        {
            this.parent = parent;
            children = new QuadTreeNode[4];
            children[0] = child0;
            children[1] = child1;
            children[2] = child2;
            children[3] = child3;
            sceneObjects = new List<ISceneObject>();
            YMask = 0;
            yLocalMask = 0;
        }


        internal void AddOrUpdateMember(ISceneObject obj)
        {
            // is this object not already a member?
            if (obj.QuadTreeNode != this)
            {
                // remove the member from it's previous quad tree node (if any)
                if (obj.QuadTreeNode != null)
                {
                    obj.QuadTreeNode.RemoveMember(obj);
                }

                sceneObjects.Add(obj);

                // update our yMask
                yLocalMask |= obj.QuadTreeBoundingBox.YMask;
                YMask |= yLocalMask;

                // notify our parent of the change
                if (parent != null)
                {
                    parent.OnDescendantMemberAdded(YMask);
                }
            }
            else
            {
                // refresh our yMask for all members
                RebuildLocalYMask();
            }

            obj.QuadTreeNode = this;
        }


        private void OnDescendantMemberAdded(uint descendantsYMask)
        {
            // update our yMask
            YMask |= descendantsYMask;

            // notify our parent of the addition
            if (parent != null)
            {
                parent.OnDescendantMemberAdded(YMask);
            }
        }


        private void RebuildLocalYMask()
        {
            yLocalMask = 0;

            // add add any local members
            foreach (ISceneObject so in sceneObjects)
            {
                yLocalMask |= so.QuadTreeBoundingBox.YMask;
            }

            RebuildYMask();
        }


        private void OnDescendantMemberChanged()
        {
            // update our yMask
            RebuildYMask();

            if (parent != null)
            {
                parent.OnDescendantMemberChanged();
            }
        }


        private void RebuildYMask()
        {
            // reset our overall y mask to the mask 
            // defined by our local members only
            YMask = yLocalMask;

            // sum up the masks of our children
            for (int i = 0; i < 4; i++)
            {
                if (children[i] != null)
                {
                    YMask |= children[i].YMask;
                }
            }
        }


        private void RemoveMember(ISceneObject obj)
        {
            sceneObjects.Remove(obj);

            RebuildLocalYMask();

            // notify our parent of the change
            if (parent != null)
            {
                parent.OnDescendantMemberChanged();
            }
        }


        internal List<ISceneObject> SearchLocalMembers(uint searchYMask, BoundingBox worldRect, BoundingFrustum worldFrustum)
        {
            // calling this function assumes that the 2D search rectangle intersects this node,
            // so we need to test against the yMask bit patterns as well as the search 
            // area for our local members
            List<ISceneObject> results = new List<ISceneObject>();

            if ((yLocalMask & searchYMask) > 0)
            {
                foreach (ISceneObject so in sceneObjects)
                {
                    if ((so.QuadTreeBoundingBox.YMask & searchYMask) > 0)
                    {
                        if (worldRect.Intersects(so.WorldSpaceBoundingBox))
                        {
                            if (worldFrustum == null || worldFrustum.Intersects(so.WorldSpaceBoundingBox))
                            {
                                results.Add(so);
                            }
                        }
                    }
                }
            }

            return results;
        }


        internal List<ISceneObject> SearchLocalMembers(uint searchYMask, BoundingFrustum worldFrustum)
        {
            // calling this function assumes that the 
            // 2D search rectangle contains this node completely,
            // so all we need to test against is the 
            // y range specified and the optional frustum
            List<ISceneObject> results = new List<ISceneObject>();

            if ((yLocalMask & searchYMask) > 0)
            {
                foreach (ISceneObject go in sceneObjects)
                {
                    if ((go.QuadTreeBoundingBox.YMask & searchYMask) > 0)
                    {
                        if (worldFrustum == null || worldFrustum.Intersects(go.WorldSpaceBoundingBox))
                        {
                            results.Add(go);
                        }
                    }
                }
            }

            return results;
        }

    }
}
