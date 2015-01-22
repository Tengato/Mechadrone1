using System;

namespace Mechadrone1
{
    class ItemEventArgs : EventArgs
    {
        public Item Item { get; private set; }

        public ItemEventArgs(Item item)
        {
            Item = item;
        }
    }
}
