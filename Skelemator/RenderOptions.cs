using System;

namespace Skelemator
{
    [Flags]
    public enum RenderOptions
    {
        None = 0x0,
        RequiresSkeletalPose = 0x01,
        RequiresEnviroMap = 0x02,
        RequiresFringeMap = 0x04,
        RequiresShadowMap = 0x08,
        RequiresHDRLighting = 0x10,
        NoStandardParams = 0x20,
        ParticleParams = 0x40,
        BillboardParams = 0x80,
    }
}
