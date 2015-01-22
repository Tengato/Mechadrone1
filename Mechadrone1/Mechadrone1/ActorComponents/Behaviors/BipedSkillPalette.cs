using System;
using Manifracture;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Mechadrone1
{
    class BipedSkillPalette : Behavior, ICustomizable
    {
        public ISkill[] Skills { get; private set; }

        private CharacterInfo mCharacterInfo;
        private ContentManager mContentLoader;

        public BipedSkillPalette(Actor owner)
            : base(owner)
        {
            Skills = new ISkill[EquipSlot.NUM_SKILL_SLOTS];
            for (int s = 0; s < EquipSlot.NUM_SKILL_SLOTS; ++s)
            {
                Skills[s] = null;
            }

            mCharacterInfo = null;
            mContentLoader = null;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
        }

        public void Customize(CharacterInfo customizations, ContentManager contentLoader)
        {
            mCharacterInfo = customizations;
            mContentLoader = contentLoader;

            for (int q = EquipSlot.SKILL1; q < EquipSlot.NUM_SKILL_SLOTS; ++q)
            {
                if (mCharacterInfo.EquippedInventory[q] != null)
                {
                    LoadSkill(q);
                }
            }

            mCharacterInfo.EquipmentChanged += CharacterUpdatedHandler;
        }

        private void CharacterUpdatedHandler(object sender, EquipmentChangedEventArgs e)
        {
            LoadSkill(e.EquipSlot);
        }

        private void LoadSkill(int equipSlot)
        {
            if (mCharacterInfo.EquippedInventory[equipSlot] != null)
            {
                string assetName = mCharacterInfo.EquippedInventory[equipSlot].EquippedBehaviorAssetName;
                object[] basicCtorParams = new object[] { Owner.Id };
                ComponentManifest skillManifest = mContentLoader.Load<ComponentManifest>(Path.Combine("skills", assetName));
                Type skillType = Type.GetType(skillManifest.TypeFullName);
                Skills[equipSlot - EquipSlot.SKILL1] = Activator.CreateInstance(skillType, basicCtorParams) as ISkill;
                Skills[equipSlot - EquipSlot.SKILL1].Initialize(mContentLoader, skillManifest);
            }
            else
            {
                Skills[equipSlot - EquipSlot.SKILL1] = null;
            }
        }

        public override void Release()
        {
            if (mCharacterInfo != null)
                mCharacterInfo.EquipmentChanged -= CharacterUpdatedHandler;
        }
    }
}
