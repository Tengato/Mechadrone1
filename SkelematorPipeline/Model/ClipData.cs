using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skelemator;

namespace SkelematorPipeline
{
    public struct ClipData
    {
        public string SourceTake;
        public string Alias;
        public int FirstFrame;
        public int LastFrame;
        public int SyncFrameOffset;
        public bool Loopable;
        public Dictionary<int, AnimationControlEvents> Events;
    }
}
