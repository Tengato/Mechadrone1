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
    public class TernaryLerpBlendNode : AnimationNode
    {
        AnimationNode child1;
        public Vector2 Child1Position;
        AnimationNode child2;
        public Vector2 Child2Position;
        AnimationNode child3;
        public Vector2 Child3Position;
        public Vector2 BlendPosition;


        public override IEnumerable<AnimationNode> Children
        {
            get { return new AnimationNode[] { child1, child2, child3 }; }
        }


        public TernaryLerpBlendNode(TernaryLerpBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            BlendPosition = nodeDesc.BlendPosition;
            playbackRate = nodeDesc.PlaybackRate;
            Child1Position = nodeDesc.Child1NodePosition;
            Child2Position = nodeDesc.Child2NodePosition;
            Child3Position = nodeDesc.Child3NodePosition;

            child1 = AnimationNode.Create(package.NodeDescriptions[nodeDesc.Child1NodeName], package);
            child2 = AnimationNode.Create(package.NodeDescriptions[nodeDesc.Child2NodeName], package);
            child3 = AnimationNode.Create(package.NodeDescriptions[nodeDesc.Child3NodeName], package);
        }


        public override Matrix[] GetSkinTransforms()
        {
            Matrix[] child1Transforms = child1.GetSkinTransforms();
            Matrix[] child2Transforms = child2.GetSkinTransforms();
            Matrix[] child3Transforms = child2.GetSkinTransforms();

            Matrix[] blendedTransforms = new Matrix[child1Transforms.Length];

            Vector3 scale1;
            Quaternion rot1;
            Vector3 pos1;
            Vector3 scale2;
            Quaternion rot2;
            Vector3 pos2;
            Vector3 scale3;
            Quaternion rot3;
            Vector3 pos3;

            Vector3 bc = SpaceUtils.GetBarycentricCoords(Child1Position, Child2Position, Child3Position, BlendPosition);

            for (int p = 0; p < child1Transforms.Length; p++)
            {
                child1Transforms[p].Decompose(out scale1, out rot1, out pos1);
                child2Transforms[p].Decompose(out scale2, out rot2, out pos2);
                child3Transforms[p].Decompose(out scale3, out rot3, out pos3);
                blendedTransforms[p] = Matrix.CreateScale(scale1 * bc.X + scale2 * bc.Y + scale3 * bc.Z) *
                    Matrix.CreateFromQuaternion(Quaternion.Normalize(rot1 * bc.X + rot2 * bc.Y + rot3 * bc.Z)) *
                    Matrix.CreateTranslation(pos1 * bc.X + pos2 * bc.Y + pos3 * bc.Z);
            }

            return blendedTransforms;
        }


        public override void AdjustBlendParam(string nodeName, Vector2 blendPosition)
        {
            if (Name == nodeName)
            {
                BlendPosition = blendPosition;
            }
            else
            {
                base.AdjustBlendParam(nodeName, blendPosition);
            }
        }

    }
}
