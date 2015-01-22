using System;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class EquippableItem : Item
    {
        public enum SlotCategory
        {
            Skill,
            Class,
            Perk,
        }

        public string EquippedBehaviorAssetName { get; private set; }
        public SlotCategory Category { get; private set; }

        public EquippableItem()
        {
            // This constructor is only useful when loading from a manifest.
            Category = SlotCategory.Skill;
            EquippedBehaviorAssetName = String.Empty;
        }

        public EquippableItem(string equippedBehaviorAssetName, SlotCategory slotCategory, string name) : base(name)
        {
            EquippedBehaviorAssetName = equippedBehaviorAssetName;
            Category = slotCategory;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            base.Initialize(contentLoader, manifest);
            Category = (SlotCategory)(manifest.Properties[ManifestKeys.EQUIP_SLOT_CATEGORY]);
            EquippedBehaviorAssetName = (string)(manifest.Properties[ManifestKeys.EQUIPPED_BEHAVIOR_NAME]);
        }
    }
}
