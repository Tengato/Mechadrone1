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
    public class BinaryLerpBlendNode : BinaryBlendAnimationNode
    {
        AnimationNode child1;
        AnimationNode child2;

        public override IEnumerable<AnimationNode> Children
        {
            get
            {
                return new AnimationNode[] { child1, child2 };
            }
        }


        public BinaryLerpBlendNode(BinaryLerpBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            BlendFactor = nodeDesc.BlendFactor;
            playbackRate = nodeDesc.PlaybackRate;

            child1 = AnimationNode.Create(package.NodeDescriptions[nodeDesc.Child1NodeName], package);
            child2 = AnimationNode.Create(package.NodeDescriptions[nodeDesc.Child2NodeName], package);

            playbackRate = 1.0f;
            PlaybackRate = nodeDesc.PlaybackRate;   // Make sure the child nodes are populated so this value can propagate down.
        }


        public override Matrix[] GetSkinTransforms()
        {
            Matrix[] child1Transforms = child1.GetSkinTransforms();
            Matrix[] child2Transforms = child2.GetSkinTransforms();

            return SpaceUtils.LerpSkeletalPose(child1Transforms, child2Transforms, BlendFactor);
        }


        public override void AdjustBlendParam(string nodeName, float blendInput)
        {
            if (Name == nodeName)
            {
                BlendFactor = blendInput;
            }
            else
            {
                base.AdjustBlendParam(nodeName, blendInput);
            }
        }

    }
}
