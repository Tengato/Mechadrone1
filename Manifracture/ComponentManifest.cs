using System;
using System.Collections.Generic;

namespace Manifracture
{
    public class ComponentManifest
    {
        public string TypeFullName { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
