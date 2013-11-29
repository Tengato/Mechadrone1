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
    public class General2DPositionalBlendNode : AnimationNode
    {
        public List<TernaryLerpBlendNode> Triangulation { get; private set; }
        public Vector2 BlendPosition;


        private AnimationNode[] children;
        public override IEnumerable<AnimationNode> Children { get { return children; } }


        public General2DPositionalBlendNode(General2DPositionalBlendNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            BlendPosition = nodeDesc.BlendPosition;
            Triangulation = new List<TernaryLerpBlendNode>();

            foreach (string triNodeName in nodeDesc.TriangleNodeNames)
            {
                Triangulation.Add((TernaryLerpBlendNode)(AnimationNode.Create(package.NodeDescriptions[triNodeName], package)));
            }

            children = new AnimationNode[Triangulation.Count];

            for (int c = 0; c < Triangulation.Count; c++)
            {
                children[c] = Triangulation[c];
            }

            playbackRate = 1.0f;
            PlaybackRate = nodeDesc.PlaybackRate;
        }


        public override Matrix[] GetSkinTransforms()
        {
            foreach (TernaryLerpBlendNode tri in Triangulation)
            {
                Vector3 bc = SpaceUtils.GetBarycentricCoords(tri.Child1Position, tri.Child2Position, tri.Child3Position, BlendPosition);
                if (bc.X >= 0.0f && bc.Y >= 0.0f && bc.Z >= 0.0f)
                    return tri.GetSkinTransforms();
            }

            throw new InvalidOperationException("The triangulation does not contain the BlendPosition point");
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
