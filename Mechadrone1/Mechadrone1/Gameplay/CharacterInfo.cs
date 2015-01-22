using System;
using System.Collections.Generic;

namespace Mechadrone1
{
    class CharacterInfo
    {
        public event EquipmentChangedEventHandler EquipmentChanged;
        public event EventHandler InventoryChanged;
        public event EventHandler<ItemEventArgs> ItemDropped;

        public string TemplateName { get; set; }
        public List<Item> FreeInventory { get; private set; }
        public List<EquippableItem> EquippedInventory { get; private set; } // Mutually exclusive with FreeInventory. Index corresponds to EquipSlot, null == no item

        public CharacterInfo()
        {
            TemplateName = String.Empty;
            FreeInventory = new List<Item>();
            EquippedInventory = new List<EquippableItem>();
            for (int e = 0; e < EquipSlot.COUNT; ++e)
            {
                EquippedInventory.Add(null);
            }
        }

        public void ObtainItem(Item item)
        {
            item.Owner = this;
            FreeInventory.Add(item);
            OnInventoryChanged();
        }

        public void Equip(int itemIndex, int equipSlot)
        {
            EquippableItem item = FreeInventory[itemIndex] as EquippableItem;
            if (item == null)
                throw new NotSupportedException();

            switch (equipSlot)
            {
                case EquipSlot.SKILL1:
                case EquipSlot.SKILL2:
                case EquipSlot.SKILL3:
                case EquipSlot.SKILL4:
                case EquipSlot.SKILL5:
                case EquipSlot.SKILL6:
                    if (item.Category != EquippableItem.SlotCategory.Skill)
                        throw new NotSupportedException();
                    break;
                case EquipSlot.CLASS:
                    if (item.Category != EquippableItem.SlotCategory.Class)
                        throw new NotSupportedException();
                    break;
                case EquipSlot.PERK1:
                case EquipSlot.PERK2:
                case EquipSlot.PERK3:
                    if (item.Category != EquippableItem.SlotCategory.Perk)
                        throw new NotSupportedException();
                    break;
                default:
                    throw new NotSupportedException();
            }
            FreeInventory.RemoveAt(itemIndex);
            if (EquippedInventory[equipSlot] != null)
                FreeInventory.Add(EquippedInventory[equipSlot]);
            EquippedInventory[equipSlot] = item;
            OnEquipmentChanged(new EquipmentChangedEventArgs(equipSlot));
        }

        public void Unequip(int equipSlot)
        {
            Item item = EquippedInventory[equipSlot];
            EquippedInventory[equipSlot] = null;
            FreeInventory.Add(item);
            OnEquipmentChanged(new EquipmentChangedEventArgs(equipSlot));
        }

        public void DropItem(Item item)
        {
            FreeInventory.Remove(item);
            item.Owner = null;
            OnInventoryChanged();
            OnItemDropped(item);
        }

        // Call this if an item is equipped or unequipped
        private void OnEquipmentChanged(EquipmentChangedEventArgs e)
        {
            EquipmentChangedEventHandler handler = EquipmentChanged;

            if (handler != null)
                handler(this, e);
        }

        // Call this if an item is obtained or discarded
        private void OnInventoryChanged()
        {
            EventHandler handler = InventoryChanged;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnItemDropped(Item item)
        {
            EventHandler<ItemEventArgs> handler = ItemDropped;

            if (handler != null)
                handler(this, new ItemEventArgs(item));
        }
    }
}
