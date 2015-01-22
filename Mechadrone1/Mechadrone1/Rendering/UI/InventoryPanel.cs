using System.Collections.Generic;
using Mechadrone1.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Mechadrone1
{
    class InventoryPanel : UIMenuWindow
    {
        private CharacterInfo mCharacter;

        public InventoryPanel(CharacterInfo AvatarDesc)
        {
            Title = "Inventory";
            mCharacter = AvatarDesc;
            mCharacter.EquipmentChanged += InventoryChangedHandler;
            mCharacter.InventoryChanged += InventoryChangedHandler;
        }

        public void RefreshItems()
        {
            mMenuEntries.Clear();
            for (int i = 0; i < mCharacter.EquippedInventory.Count; ++i)
            {
                if (mCharacter.EquippedInventory[i] == null)
                    continue;
                EquippedInventoryMenuEntry newEntry = new EquippedInventoryMenuEntry(mCharacter.EquippedInventory[i].Name, mCharacter.EquippedInventory[i]);
                newEntry.Unequip += UnequipHandler;
                mMenuEntries.Add(newEntry);
            }

            for (int i = 0; i < mCharacter.FreeInventory.Count; ++i)
            {
                FreeInventoryMenuEntry newEntry = new FreeInventoryMenuEntry(mCharacter.FreeInventory[i].Name);
                newEntry.Item = mCharacter.FreeInventory[i];
                newEntry.Discard += DiscardHandler;
                newEntry.Equip += EquipHandler;
                mMenuEntries.Add(newEntry);
            }

            if (mSelectedEntryIndex >= mMenuEntries.Count)
                mSelectedEntryIndex = mMenuEntries.Count - 1;
        }

        //private void EquippedItemSelectedHandler(object sender, PlayerIndexEventArgs e)
        //{
        //    EquippedInventoryMenuEntry menuEntry = sender as EquippedInventoryMenuEntry;
        //    Manager.AddWindow(menuEntry.Item.GetContextMenu(), ControllingPlayer);
        //}

        //private void FreeItemSelectedHandler(object sender, PlayerIndexEventArgs e)
        //{
        //    FreeInventoryMenuEntry menuEntry = sender as FreeInventoryMenuEntry;
        //    Manager.AddWindow(menuEntry.Item.GetContextMenu(), ControllingPlayer);
        //}

        private void InventoryChangedHandler(object sender, EventArgs e)
        {
            RefreshItems();
        }

        private void DiscardHandler(object sender, EventArgs e)
        {
            FreeInventoryMenuEntry entry = sender as FreeInventoryMenuEntry;
            mCharacter.DropItem(entry.Item);
        }

        private void EquipHandler(object sender, EventArgs e)
        {
            FreeInventoryMenuEntry entry = (FreeInventoryMenuEntry)sender;
            Manager.AddWindow(new EquipSlotMenu((EquippableItem)(entry.Item)), ControllingPlayer);
        }

        private void UnequipHandler(object sender, EventArgs e)
        {
            EquippedInventoryMenuEntry entry = sender as EquippedInventoryMenuEntry;
            mCharacter.Unequip(mCharacter.EquippedInventory.IndexOf(entry.Item));
        }

        public void Release()
        {
            mCharacter.EquipmentChanged -= InventoryChangedHandler;
            mCharacter.InventoryChanged -= InventoryChangedHandler;
        }
    }
}
