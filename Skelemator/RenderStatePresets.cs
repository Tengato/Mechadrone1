using System.Security;

//#if !XBOX
//[assembly: AllowPartiallyTrustedCallers]
//#endif

namespace Skelemator
{
    public enum RenderStatePresets
    {
        Default,
        AlphaAdd,       // e.g. fire particles
        AlphaBlend,     // e.g. convex translucent objects
        AlphaBlendNPM,  // e.g. smoke particles whose alpha is adjusted in the shader
        Skybox,
    }   // Where to put cutouts? non-convex translucent objects?
}
