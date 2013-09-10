using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Skelemator
{
    [Flags]
    public enum RenderOptions
    {
        None = 0x0,
        RequiresSkeletalPose = 0x01,
        RequiresEnviroMap = 0x02,

        // Put fringe map procedural textures into the processor.
        RequiresFringeMap = 0x04,
        RequiresShadowMap = 0x08,
    }
}
