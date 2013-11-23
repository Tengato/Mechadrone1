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


        public ClipNode(ClipNodeDescription nodeDesc, SkinningData sd)
        {
            Name = nodeDesc.Name;
            PlaybackRate = nodeDesc.PlaybackRate;
            clipPlayer = new ClipPlayer(sd);
            clipPlayer.StartClip(sd.AnimationClips[nodeDesc.ClipName]);
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


        public override void Synchronize()
        {
            clipPlayer.Update(clipPlayer.CurrentClip.SyncTime, false, Matrix.Identity);
        }


        public override Matrix[] GetSkinTransforms()
        {
            return clipPlayer.GetSkinTransforms();
        }


        public override List<AnimationControlEvents> GetActiveControlEvents()
        {
            return clipPlayer.ActiveControlEvents;
        }
    }
}
