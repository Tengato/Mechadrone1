using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Skelemator
{
    public interface ISkinnedSkeletonPoser
    {
        Matrix[] GetSkinTransforms();
        bool IsActive { get; }
    }
}
