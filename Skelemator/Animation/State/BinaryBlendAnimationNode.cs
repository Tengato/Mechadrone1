using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skelemator
{
    public abstract class BinaryBlendAnimationNode : AnimationNode
    {
        public virtual float BlendFactor { get; set; }
    }
}
