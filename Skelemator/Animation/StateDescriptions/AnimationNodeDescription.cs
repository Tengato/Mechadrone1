using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Skelemator
{
    public abstract class AnimationNodeDescription
    {
        public string Name;
        public float PlaybackRate;
    }


    public class ClipNodeDescription : AnimationNodeDescription
    {
        public string ClipName;
    }


    public class BinaryLerpBlendNodeDescription : AnimationNodeDescription
    {
        public string Child1NodeName;
        public string Child2NodeName;
        public float BlendFactor;
    }


    public class BinaryAdditiveBlendNodeDescription : AnimationNodeDescription
    {
        public string TargetNodeName;
        public string DifferenceNodeName;
    }


    public class TernaryLerpBlendNodeDescription : AnimationNodeDescription
    {
        public string Child1NodeName;
        public string Child2NodeName;
        public string Child3NodeName;
        public Vector2 Child1NodePosition;
        public Vector2 Child2NodePosition;
        public Vector2 Child3NodePosition;
        public Vector2 BlendPosition;
    }


    public class General2DPositionalBlendNodeDescription : AnimationNodeDescription
    {
        public List<string> TriangleNodeNames;
        public Vector2 BlendPosition;
    }


    public class General1DPositionalBlendNodeDescription : AnimationNodeDescription
    {
        public List<string> ChildNodeNames;
        public Dictionary<string, float> ChildPositionsByName;
        public float BlendPosition;
    }
}
