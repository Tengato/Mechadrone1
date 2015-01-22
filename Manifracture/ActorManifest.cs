using System.Collections.Generic;

namespace Manifracture
{
    public class ActorManifest
    {
        public string Name { get; set; }
        public List<ComponentManifest> Components { get; set; }
    }
}
