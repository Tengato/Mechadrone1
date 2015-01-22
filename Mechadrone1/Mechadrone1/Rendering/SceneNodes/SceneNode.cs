using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class SceneNode
    {
        protected List<SceneNode> mChildren;
        protected SceneNode mParent;

        public SceneNode()
        {
            mChildren = new List<SceneNode>();
            mParent = null;
        }

        public SceneNode(SceneNode orig)
        {
            mChildren = orig.mChildren;
            mParent = orig.mParent;
        }

        public virtual void PreProcess(TraversalContext context) { }

        public virtual void Process(TraversalContext context) { }

        public virtual void ProcessChildren(TraversalContext context)
        {
            foreach (SceneNode child in mChildren)
            {
                child.PreProcess(context);
                child.Process(context);
                child.ProcessChildren(context);
                child.PostProcess(context);
            }
        }

        public virtual void PostProcess(TraversalContext context) { }

        public virtual void AddChild(SceneNode child)
        {
            mChildren.Add(child);
            child.mParent = this;
        }

        /// <summary>
        /// Traverses the tree, finds the explicit bounding volumes, and transforms them into world space.
        /// </summary>
        /// <returns></returns>
        public List<BoundingBox> FindExplicitBoundingVolumes()
        {
            List<BoundingBox> results = new List<BoundingBox>();
            Stack<Tuple<SceneNode, Matrix>> traversal = new Stack<Tuple<SceneNode, Matrix>>();
            traversal.Push(new Tuple<SceneNode, Matrix>(this, Matrix.Identity));

            while (traversal.Count > 0)
            {
                Tuple<SceneNode, Matrix> currNode = traversal.Pop();

                TransformNode currXformNode = currNode.Item1 as TransformNode;

                Matrix localXform = (currXformNode != null) ? currXformNode.Transform * currNode.Item2 : currNode.Item2;

                IExplicitBoxableVolume currBVNode = currNode.Item1 as IExplicitBoxableVolume;

                if (currBVNode != null)
                {
                    results.Add(currBVNode.BoxTransformedVolume(localXform));
                }
                else
                {
                    foreach (SceneNode child in currNode.Item1.mChildren)
                    {
                        traversal.Push(new Tuple<SceneNode, Matrix>(child, localXform));
                    }
                }
            }

            return results;
        }

        public virtual void ConnectToAnimationComponent(AnimationComponent animationComponent)
        {
            foreach (SceneNode child in mChildren)
            {
                child.ConnectToAnimationComponent(animationComponent);
            }
        }

        public void Release()
        {
            mParent.RemoveChild(this);
        }

        protected virtual void RemoveChild(SceneNode child)
        {
            mChildren.Remove(child);
        }
    }
}
