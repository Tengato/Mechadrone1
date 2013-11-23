using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skelemator;

namespace SkelematorPipeline
{
    public struct AnimationPackageData
    {
        public List<ClipData> Clips;
        public List<AnimationStateDescription> AnimationStateDescriptions;
        public List<AnimationNodeDescription> AnimationNodeDescriptions;
        public string InitialStateName;
        public List<TransitionInfo> Transitions;
    }
}
