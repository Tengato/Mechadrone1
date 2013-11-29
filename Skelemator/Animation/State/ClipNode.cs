using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Skelemator;
using System.Threading;

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
            clipPlayer.Update(elapsedLocalTime, true, Matrix.Identity);
        }


        public override void SetTime(TimeSpan time)
        {
            clipPlayer.Update(time, false, Matrix.Identity);
        }


        public override void Synchronize(float normalizedTime)
        {
            TimeSpan localTime = TimeSpan.FromTicks((long)(normalizedTime * (float)(clipPlayer.CurrentClip.Duration.Ticks)))
                + clipPlayer.CurrentClip.SyncTime;

            clipPlayer.Update(localTime, false, Matrix.Identity);
        }


        public override Matrix[] GetSkinTransforms()
        {
            return clipPlayer.GetSkinTransforms();
        }


        public override List<AnimationControlEvents> GetActiveControlEvents()
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
