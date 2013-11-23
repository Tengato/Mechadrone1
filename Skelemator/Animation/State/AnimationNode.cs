using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Skelemator;
using System.Threading;

namespace Skelemator
{
    public abstract class AnimationNode
    {
        public string Name { get; set; }

        protected float playbackRate;
        public virtual float PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                float adjustByFactor = value / playbackRate;
                
                foreach (AnimationNode an in Children)
                {
                    an.PlaybackRate *= adjustByFactor;
                }
                playbackRate = value;
            }
        }

        public abstract IEnumerable<AnimationNode> Children { get; }


        public abstract Matrix[] GetSkinTransforms();


        // Override this if you need to ever capture these messages.
        public virtual void AdjustBlendParam(string nodeName, float blendInput)
        {
            foreach (AnimationNode an in Children)
            {
                an.AdjustBlendParam(nodeName, blendInput);
            }
        }


        // Override this if you need to ever capture these messages.
        public virtual void AdjustBlendParam(string nodeName, Vector2 blendPosition)
        {
            foreach (AnimationNode an in Children)
            {
                an.AdjustBlendParam(nodeName, blendPosition);
            }
        }


        public virtual void AdvanceTime(TimeSpan elapsedTime)
        {
            foreach (AnimationNode an in Children)
            {
                an.AdvanceTime(elapsedTime);
            }
        }


        public virtual void SetTime(TimeSpan time)
        {
            foreach (AnimationNode an in Children)
            {
                an.SetTime(time);
            }
        }


        public virtual void Synchronize()
        {
            foreach (AnimationNode an in Children)
            {
                an.Synchronize();
            }
        }


        public virtual List<AnimationControlEvents> GetActiveControlEvents()
        {
            List<AnimationControlEvents> events = new List<AnimationControlEvents>();

            foreach (AnimationNode an in Children)
            {
                events.AddRange(an.GetActiveControlEvents());
            }
            return events;
        }


        public static AnimationNode Create(AnimationNodeDescription animationNodeDescription, AnimationPackage package)
        {
            AnimationNode result;

            if (animationNodeDescription.GetType() == typeof(ClipNodeDescription))
            {
                ClipNodeDescription and = animationNodeDescription as ClipNodeDescription;
                result = new ClipNode(and, package.SkinningData);
            }
            else if (animationNodeDescription.GetType() == typeof(BinaryLerpBlendNodeDescription))
            {
                BinaryLerpBlendNodeDescription and = animationNodeDescription as BinaryLerpBlendNodeDescription;
                result = new BinaryLerpBlendNode(and, package);
            }
            else if (animationNodeDescription.GetType() == typeof(BinaryAdditiveBlendNodeDescription))
            {
                BinaryAdditiveBlendNodeDescription and = animationNodeDescription as BinaryAdditiveBlendNodeDescription;
                result = new BinaryAdditiveBlendNode(and, package);
            }
            else if (animationNodeDescription.GetType() == typeof(TernaryLerpBlendNodeDescription))
            {
                TernaryLerpBlendNodeDescription and = animationNodeDescription as TernaryLerpBlendNodeDescription;
                result = new TernaryLerpBlendNode(and, package);
            }
            else if (animationNodeDescription.GetType() == typeof(General2DPositionalBlendNodeDescription))
            {
                General2DPositionalBlendNodeDescription and = animationNodeDescription as General2DPositionalBlendNodeDescription;
                result = new General2DPositionalBlendNode(and, package);
            }
            else if (animationNodeDescription.GetType() == typeof(General1DPositionalBlendNodeDescription))
            {
                General1DPositionalBlendNodeDescription and = animationNodeDescription as General1DPositionalBlendNodeDescription;
                result = new General1DPositionalBlendNode(and, package);
            }
            else
            {
                // TODO: just use the Activator here to instantiate all these nodes...
                throw new NotSupportedException();
            }

            return result;
        }
    }
}
