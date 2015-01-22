using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Skelemator
{
    public class BinaryRateRampBlendNode : BinaryBlendAnimationNode
    {
        AnimationNode child;
        float rate1;
        float rate2;

        private float blendFactor;
        public override float BlendFactor
        {
            get { return blendFactor; }
            set
            {
                blendFactor = value;
                PlaybackRate = MathHelper.Lerp(rate1, rate2, blendFactor);
            }
        }

        public override IEnumerable<AnimationNode> Children
        {
            get
            {
                return new AnimationNode[] { child };
            }
        }

        public BinaryRateRampBlendNode(BinaryRateRampBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            rate1 = nodeDesc.Rate1;
            rate2 = nodeDesc.Rate2;

            child = AnimationNode.Create(package.NodeDescriptions[nodeDesc.ChildNodeName], package);

            playbackRate = 1.0f;
            BlendFactor = nodeDesc.BlendFactor;
        }


        public override Matrix[] GetBoneTransforms()
        {
            return child.GetBoneTransforms();
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
