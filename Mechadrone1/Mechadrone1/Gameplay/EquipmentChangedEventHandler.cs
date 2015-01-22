using System;

namespace Mechadrone1
{
    public class EquipmentChangedEventArgs : EventArgs
    {
        public int EquipSlot { get; private set; }

        public EquipmentChangedEventArgs(int equipSlot)
            : base()
        {
            EquipSlot = equipSlot;
        }
    }

    public delegate void EquipmentChangedEventHandler(object sender, EquipmentChangedEventArgs e);
}
