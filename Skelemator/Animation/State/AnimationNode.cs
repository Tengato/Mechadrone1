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


        public virtual void Synchronize(float normalizedTime)
        {
            foreach (AnimationNode an in Children)
            {
                an.Synchronize(normalizedTime);
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


        public virtual float GetNormalizedTime(string nodeName, out bool nodeFound)
        {
            foreach (AnimationNode an in Children)
            {
                float anNormTime = an.GetNormalizedTime(nodeName, out nodeFound);
                if (nodeFound)
                    return anNormTime;
            }

            nodeFound = false;
            return 0.0f;
        }


        public static AnimationNode Create(AnimationNodeDescription animationNodeDescription, AnimationPackage package)
        {
            Type nodeType = Type.GetType(animationNodeDescription.RuntimeTypeName);
            object[] aniNodeCtorParams = new object[] { animationNodeDescription, package };
            return Activator.CreateInstance(nodeType, aniNodeCtorParams) as AnimationNode;
        }
    }
}
