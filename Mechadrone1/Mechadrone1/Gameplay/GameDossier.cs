using System;
using System.Collections.Generic;

namespace Mechadrone1
{
    class GameDossier
    {
        public static GameDossier CreateNewGame()
        {
            GameDossier newGame = new GameDossier();
            newGame.Characters.Add(CreateDefaultCharacter());
            return newGame;
        }

        private static CharacterInfo CreateDefaultCharacter()
        {
            CharacterInfo newChar = new CharacterInfo();
            newChar.TemplateName = "Vanquisher";
            newChar.ObtainItem(new EquippableItem("MachineGun", EquippableItem.SlotCategory.Skill, "MachineGun"));
            newChar.ObtainItem(new EquippableItem("BoostSkill", EquippableItem.SlotCategory.Skill, "Booster"));
            newChar.ObtainItem(new EquippableItem("BlasterRifle", EquippableItem.SlotCategory.Skill, "Blaster"));
            newChar.ObtainItem(new EquippableItem("RocketLauncher", EquippableItem.SlotCategory.Skill, "Rockets"));
            newChar.ObtainItem(new Item("Gadget"));
            newChar.ObtainItem(new Item("Widget"));
            newChar.ObtainItem(new Item("PowerCell"));
            newChar.Equip(0, EquipSlot.SKILL1);
            newChar.Equip(0, EquipSlot.SKILL2);
            newChar.Equip(0, EquipSlot.SKILL3);
            newChar.Equip(0, EquipSlot.SKILL4);
            return newChar;
        }

        public List<CharacterInfo> Characters { get; private set; }

        public GameDossier()
        {
            Characters = new List<CharacterInfo>();
        }
    }
}
