using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkelematorPipeline
{
    public struct ClipData
    {
        public string SourceTake;
        public string Alias;
        public int FirstFrame;
        public int LastFrame;
        public bool Loopable;
    }
}
