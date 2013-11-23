using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skelemator
{
    public class AnimationPackage
    {
        public SkinningData SkinningData { get; set; }
        public List<AnimationStateDescription> StateDescriptions { get; set; }
        public Dictionary<string, AnimationNodeDescription> NodeDescriptions { get; set; }
        public string InitialStateName { get; set; }
        public List<TransitionInfo> Transitions { get; set; }


        public AnimationPackage()
        {
        }


        public AnimationPackage(
            SkinningData skinningData, 
            List<AnimationStateDescription> stateDescriptions,
            List<AnimationNodeDescription> nodeDescriptions,
            string initialStateName,
            List<TransitionInfo> transitions)
        {
            InitialStateName = initialStateName;
            SkinningData = skinningData;
            StateDescriptions = stateDescriptions;
            NodeDescriptions = new Dictionary<string, AnimationNodeDescription>();

            foreach (AnimationNodeDescription and in nodeDescriptions)
            {
                NodeDescriptions.Add(and.Name, and);
            }

            Transitions = transitions;
        }
    }
}
