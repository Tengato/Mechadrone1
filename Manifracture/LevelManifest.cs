using System.Collections.Generic;
using System.Security;

//#if !XBOX
//[assembly: AllowPartiallyTrustedCallers]
//#endif

namespace Manifracture
{
    public class LevelManifest
    {
        public string ScreenHonchoTypeName { get; set; }
        public List<ActorManifest> Actors { get; set; }
        public List<string> Templates { get; set; }
    }
}
