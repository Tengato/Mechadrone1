using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Skelemator;
using System.Threading;

namespace Skelemator
{
    public class BinaryAdditiveBlendNode : AnimationNode
    {
        AnimationNode differencePose;
        AnimationNode targetPose;


        public override IEnumerable<AnimationNode> Children
        {
            get
            {
                return new AnimationNode[] { differencePose, targetPose };
            }
        }


        public BinaryAdditiveBlendNode(BinaryAdditiveBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;

            targetPose = AnimationNode.Create(package.NodeDescriptions[nodeDesc.TargetNodeName], package);
            differencePose = AnimationNode.Create(package.NodeDescriptions[nodeDesc.DifferenceNodeName], package);

            playbackRate = 1.0f;
            PlaybackRate = nodeDesc.PlaybackRate;
        }


        public override Matrix[] GetSkinTransforms()
        {
            Matrix[] differencePoseTransforms = differencePose.GetSkinTransforms();
            Matrix[] targetPoseTransforms = targetPose.GetSkinTransforms();

            Matrix[] blendedTransforms = new Matrix[differencePoseTransforms.Length];

            for (int p = 0; p < differencePoseTransforms.Length; p++)
            {
                blendedTransforms[p] = differencePoseTransforms[p] *
                    targetPoseTransforms[p];
            }

            return blendedTransforms;

        }

    }
}
