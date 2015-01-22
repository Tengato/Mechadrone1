using System.Collections.Generic;

namespace Manifracture
{
    public class TemplateManifest
    {
        public ActorManifest Actor { get; set;}
        public List<AssetInfo> AssetsToPreload { get; set; }
    }
}
