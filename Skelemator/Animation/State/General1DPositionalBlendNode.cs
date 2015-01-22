using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Skelemator;
using System.Threading;
using SlagformCommon;

namespace Skelemator
{
    public class General1DPositionalBlendNode : AnimationNode
    {
        public List<AnimationNode> children;
        public float BlendPosition;
        public override IEnumerable<AnimationNode> Children { get { return children.AsEnumerable(); } }
        public Dictionary<string, Vector2> Partition;
        public string SyncClipNodeName { get; set; }


        public General1DPositionalBlendNode(General1DPositionalBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            BlendPosition = nodeDesc.BlendPosition;
            children = new List<AnimationNode>();
            Partition = nodeDesc.ChildRangesByName;
            SyncClipNodeName = nodeDesc.SyncClipNodeName;

            for (int c = 0; c < nodeDesc.ChildNodeNames.Count; c++)
            {
                children.Add(AnimationNode.Create(package.NodeDescriptions[nodeDesc.ChildNodeNames[c]], package));
            }

            playbackRate = 1.0f;
            PlaybackRate = nodeDesc.PlaybackRate;
        }


        public override void AdvanceTime(TimeSpan elapsedTime)
        {
            AnimationNode activeNode = GetActiveChild();
            activeNode.AdvanceTime(elapsedTime);

            foreach (AnimationNode an in Children)
            {
                if (an == activeNode)
                    continue;

                bool dummyNodeFound;
                an.Synchronize(activeNode.GetNormalizedTime(SyncClipNodeName, out dummyNodeFound));
            }
        }


        public override Matrix[] GetBoneTransforms()
        {
            AnimationNode activeNode = GetActiveChild();

            BinaryBlendAnimationNode binaryNode = activeNode as BinaryBlendAnimationNode;
            if (binaryNode != null)
            {
                Vector2 range = Partition[activeNode.Name];
                binaryNode.BlendFactor = (BlendPosition - range.X) / (range.Y - range.X);
            }

            return activeNode.GetBoneTransforms();
        }


        public override void AdjustBlendParam(string nodeName, float blendInput)
        {
            if (Name == nodeName)
            {
                BlendPosition = blendInput;
            }
            else
            {
                base.AdjustBlendParam(nodeName, blendInput);
            }
        }


        protected AnimationNode GetActiveChild()
        {
            foreach (AnimationNode an in children)
            {
                Vector2 range = Partition[an.Name];
                if (BlendPosition >= range.X && BlendPosition <= range.Y)
                {
                    return an;
                }
            }

            throw new InvalidOperationException("The partition does not contain the BlendPosition point");
        }

    }
}
