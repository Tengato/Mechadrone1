using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Skelemator
{
    public class AnimationState
    {

        public string Name { get; set; }
        public TimeSpan MinDuration { get; set; }

        private TimeSpan timeInState;

        protected AnimationNode blendTree;

        public AnimationState(AnimationStateDescription stateDesc, AnimationPackage package)
        {
            Name = stateDesc.Name;
            MinDuration = TimeSpan.FromSeconds(stateDesc.MinDurationInSeconds);
            timeInState = TimeSpan.Zero;
            playbackRate = 1.0f;
            blendTree = AnimationNode.Create(package.NodeDescriptions[stateDesc.BlendTreeNodeName], package);
        }


        protected float playbackRate;
        public float PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                float adjustByFactor = value / playbackRate;

                blendTree.PlaybackRate *= adjustByFactor;

                playbackRate = value;
            }
        }


        public void AdvanceTime(TimeSpan elapsedTime)
        {
            timeInState += elapsedTime;
            blendTree.AdvanceTime(elapsedTime);
        }


        public void AdjustBlendParam(string nodeName, float blendInput)
        {
            blendTree.AdjustBlendParam(nodeName, blendInput);
        }


        public void AdjustBlendParam(string nodeName, Vector2 blendPosition)
        {
            blendTree.AdjustBlendParam(nodeName, blendPosition);
        }


        public List<AnimationControlEvents> GetActiveControlEvents()
        {
            return blendTree.GetActiveControlEvents();
        }


        public bool CanTransitionOut()
        {
            if (timeInState >= MinDuration)
                return true;

            return false;
        }


        public Matrix[] GetSkinTransforms()
        {
            return blendTree.GetSkinTransforms();
        }


        public void Synchronize()
        {
            timeInState = TimeSpan.Zero;
            blendTree.Synchronize();
        }
    }
}
