using Mechadrone1.Screens;
using System;

namespace Mechadrone1
{
    class EquipSlotMenu : UIMenuWindow
    {
        public EquippableItem Item { get; private set; }

        public EquipSlotMenu(EquippableItem item)
        {
            Item = item;
            EquipSlotMenuEntry equipEntry;
            switch (item.Category)
            {
                case EquippableItem.SlotCategory.Skill:
                    for (int s = EquipSlot.SKILL1; s < EquipSlot.NUM_SKILL_SLOTS + EquipSlot.SKILL1; ++s)
                    {
                        equipEntry = new EquipSlotMenuEntry(EquipSlot.Names[s]);
                        equipEntry.EquipSlot = s;
                        equipEntry.Selected += EquipItemSelected;

                        mMenuEntries.Add(equipEntry);
                    }
                    break;
                case EquippableItem.SlotCategory.Class:
                    equipEntry = new EquipSlotMenuEntry(EquipSlot.Names[EquipSlot.CLASS]);
                    equipEntry.EquipSlot = EquipSlot.CLASS;
                    equipEntry.Selected += EquipItemSelected;

                    mMenuEntries.Add(equipEntry);
                    break;
                case EquippableItem.SlotCategory.Perk:
                    for (int s = EquipSlot.PERK1; s < EquipSlot.NUM_PERK_SLOTS + EquipSlot.PERK1; ++s)
                    {
                        equipEntry = new EquipSlotMenuEntry(EquipSlot.Names[s]);
                        equipEntry.EquipSlot = s;
                        equipEntry.Selected += EquipItemSelected;

                        mMenuEntries.Add(equipEntry);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void EquipItemSelected(object sender, PlayerIndexEventArgs e)
        {
            Item.Owner.Equip(Item.Owner.FreeInventory.IndexOf(Item), ((EquipSlotMenuEntry)sender).EquipSlot);
            ExitWindow();
        }
    }
}
