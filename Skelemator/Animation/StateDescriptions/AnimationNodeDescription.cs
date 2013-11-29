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
        public abstract string RuntimeTypeName { get; }
    }


    public class ClipNodeDescription : AnimationNodeDescription
    {
        public string ClipName;
        public override string RuntimeTypeName { get { return "Skelemator.ClipNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }


    public class BinaryLerpBlendNodeDescription : AnimationNodeDescription
    {
        public string Child1NodeName;
        public string Child2NodeName;
        public float BlendFactor;
        public override string RuntimeTypeName { get { return "Skelemator.BinaryLerpBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }


    public class BinaryAdditiveBlendNodeDescription : AnimationNodeDescription
    {
        public string TargetNodeName;
        public string DifferenceNodeName;
        public override string RuntimeTypeName { get { return "Skelemator.BinaryAdditiveBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
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
        public override string RuntimeTypeName { get { return "Skelemator.TernaryLerpBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }


    public class General2DPositionalBlendNodeDescription : AnimationNodeDescription
    {
        public List<string> TriangleNodeNames;
        public Vector2 BlendPosition;
        public override string RuntimeTypeName { get { return "Skelemator.General2DPositionalBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }


    public class BinaryRateRampBlendNodeDescription : AnimationNodeDescription
    {
        public string ChildNodeName;
        public float Rate1;
        public float Rate2;
        public float BlendFactor;
        public override string RuntimeTypeName { get { return "Skelemator.BinaryRateRampBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }


    public class General1DPositionalBlendNodeDescription : AnimationNodeDescription
    {
        public List<string> ChildNodeNames;
        public Dictionary<string, Vector2> ChildRangesByName;
        public float BlendPosition;
        public string SyncClipNodeName;
        public override string RuntimeTypeName { get { return "Skelemator.General1DPositionalBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }


    public class Continuous1DLerpBlendNodeDescription : AnimationNodeDescription
    {
        public List<string> ChildNodeNames;
        public Dictionary<string, float> ChildPositionsByName;
        public float BlendPosition;
        public override string RuntimeTypeName { get { return "Skelemator.Continuous1DLerpBlendNode, Skelemator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"; } }
    }
}
