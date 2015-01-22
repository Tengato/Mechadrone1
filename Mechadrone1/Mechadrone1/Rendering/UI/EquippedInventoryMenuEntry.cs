using Mechadrone1.Screens;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class EquippedInventoryMenuEntry : MenuEntry
    {
        public EquippableItem Item { get; private set; }
        public event EventHandler Unequip;

        public EquippedInventoryMenuEntry(string text, EquippableItem item)
            : base(text)
        {
            Item = item;
            Text = "E* " + Text + " --- " + EquipSlot.Names[item.Owner.EquippedInventory.IndexOf(item)];
        }

        public override void HandleInput(GameTime gameTime, InputManager input, PlayerIndex? controllingPlayer)
        {
            PlayerIndex dummyPlayerIndex;
            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.X, controllingPlayer, out dummyPlayerIndex) ||
                input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.X, controllingPlayer, out dummyPlayerIndex))
                OnUnequip();
        }

        protected void OnUnequip()
        {
            EventHandler handler = Unequip;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
