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
    public class Continuous1DLerpBlendNode : AnimationNode
    {
        public Dictionary<float, AnimationNode> ChildrenByPosition { get; private set; }
        public float BlendPosition;
        public override IEnumerable<AnimationNode> Children { get { return ChildrenByPosition.Values.AsEnumerable(); } }


        public Continuous1DLerpBlendNode(Continuous1DLerpBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            BlendPosition = nodeDesc.BlendPosition;
            ChildrenByPosition = new Dictionary<float, AnimationNode>();

            for (int c = 0; c < nodeDesc.ChildNodeNames.Count; c++ )
            {
                AnimationNode childNode = AnimationNode.Create(package.NodeDescriptions[nodeDesc.ChildNodeNames[c]], package);
                ChildrenByPosition.Add(nodeDesc.ChildPositionsByName[nodeDesc.ChildNodeNames[c]], childNode);
            }

            playbackRate = 1.0f;
            PlaybackRate = nodeDesc.PlaybackRate;
        }


        public override Matrix[] GetBoneTransforms()
        {
            // Pick an arbitrary node to 'prime' the refinement loop
            float lowerBoundPosition = ChildrenByPosition.Keys.Min();
            float upperBoundPosition = ChildrenByPosition.Keys.Max();

            foreach (float position in ChildrenByPosition.Keys)
            {
                if (position > lowerBoundPosition && position <= BlendPosition)
                    lowerBoundPosition = position;

                if (position < upperBoundPosition && position >= BlendPosition)
                    upperBoundPosition = position;
            }

            Matrix[] lowerBoundTransforms = ChildrenByPosition[lowerBoundPosition].GetBoneTransforms();

            if (lowerBoundPosition == upperBoundPosition)
                return lowerBoundTransforms;

            Matrix[] upperBoundTransforms = ChildrenByPosition[upperBoundPosition].GetBoneTransforms();
            float blendFactor = (BlendPosition - lowerBoundPosition) /
                (upperBoundPosition - lowerBoundPosition);

            return SpaceUtils.LerpSkeletalPose(lowerBoundTransforms, upperBoundTransforms, blendFactor);
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

    }
}
