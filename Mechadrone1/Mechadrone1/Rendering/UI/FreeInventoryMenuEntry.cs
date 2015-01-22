using Mechadrone1.Screens;
using Microsoft.Xna.Framework;
using System;

namespace Mechadrone1
{
    class FreeInventoryMenuEntry : MenuEntry
    {
        private Item mItem;
        public Item Item
        {
            get { return mItem; }
            set
            {
                mItem = value;
                mEquippableItem = mItem as EquippableItem;
            }
        }
        private EquippableItem mEquippableItem;

        public event EventHandler Discard;
        public event EventHandler Equip;

        public FreeInventoryMenuEntry(string text)
            : base(text)
        {
            Item = null;
        }

        public override void HandleInput(GameTime gameTime, InputManager input, PlayerIndex? controllingPlayer)
        {
            PlayerIndex dummyPlayerIndex;
            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.X, controllingPlayer, out dummyPlayerIndex) ||
                input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.X, controllingPlayer, out dummyPlayerIndex))
                OnDiscard();

            if (mEquippableItem != null &&
                (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.A, controllingPlayer, out dummyPlayerIndex) ||
                input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.A, controllingPlayer, out dummyPlayerIndex)))
                OnEquip();
        }

        protected void OnDiscard()
        {
            EventHandler handler = Discard;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected void OnEquip()
        {
            EventHandler handler = Equip;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public override void Draw(float transitionValue, bool isSelected, GameTime gameTime)
        {
            base.Draw(transitionValue, isSelected, gameTime);

            // Draw some button help

            if (mEquippableItem != null)
            {


            }
        }
    }
}
