using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Skelemator
{
    public class ClipNode : AnimationNode
    {
        ClipPlayer clipPlayer;
        public override float PlaybackRate { get { return playbackRate; } set { playbackRate = value; } }

        public override IEnumerable<AnimationNode> Children
        {
            get { return new AnimationNode[] { }; }
        }

        public ClipNode(ClipNodeDescription nodeDesc, AnimationPackage package)
        {
            Name = nodeDesc.Name;
            PlaybackRate = nodeDesc.PlaybackRate;
            clipPlayer = new ClipPlayer(package.SkinningData);
            clipPlayer.StartClip(package.SkinningData.AnimationClips[nodeDesc.ClipName]);
        }

        public override void AdvanceTime(TimeSpan elapsedTime)
        {
            TimeSpan elapsedLocalTime = TimeSpan.FromTicks((long)(elapsedTime.Ticks * PlaybackRate));
            clipPlayer.Update(elapsedLocalTime, true);
        }

        public override void SetTime(TimeSpan time)
        {
            clipPlayer.Update(time, false);
        }

        public override void Synchronize(float normalizedTime)
        {
            TimeSpan localTime = TimeSpan.FromTicks((long)(normalizedTime * (float)(clipPlayer.CurrentClip.Duration.Ticks)))
                + clipPlayer.CurrentClip.SyncTime;

            clipPlayer.Update(localTime, false);
        }

        public override Matrix[] GetBoneTransforms()
        {
            return clipPlayer.GetBoneTransforms();
        }

        public override AnimationControlEvents GetActiveControlEvents()
        {
            return clipPlayer.ActiveControlEvents;
        }

        public override float GetNormalizedTime(string nodeName, out bool nodeFound)
        {
            if (nodeName == Name)
            {
                nodeFound = true;
                return (float)((clipPlayer.CurrentTime - clipPlayer.CurrentClip.SyncTime).Ticks %
                    clipPlayer.CurrentClip.Duration.Ticks) /
                    (float)(clipPlayer.CurrentClip.Duration.Ticks);
            }
            else
            {
                nodeFound = false;
                return 0.0f;
            }
        }
    }
}
