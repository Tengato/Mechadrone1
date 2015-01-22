using Microsoft.Xna.Framework.Content;
using Manifracture;
using System;

namespace Mechadrone1
{
    class Item
    {
        public CharacterInfo Owner { get; set; }
        public int Value { get; set; }
        public string Name { get; set; }

        // Items must support a 0-argument constructor in order to allow them to be loaded from a manifest.
        public Item()
        {
            Owner = null;
            Value = 3;
            Name = String.Empty;
        }

        public Item(string name)
        {
            Owner = null;
            Value = 3;
            Name = name;
        }

        // This method is used when loading the item from a manifest
        public virtual void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            if (manifest.Properties.ContainsKey(ManifestKeys.VALUE))
                Value = (int)(manifest.Properties[ManifestKeys.VALUE]);
            Name = (string)(manifest.Properties[ManifestKeys.NAME]);
        }
    }
}
