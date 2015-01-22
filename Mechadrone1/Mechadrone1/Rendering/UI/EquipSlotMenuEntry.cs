using Mechadrone1.Screens;

namespace Mechadrone1
{
    class EquipSlotMenuEntry : MenuEntry
    {
        public int EquipSlot { get; set; }

        public EquipSlotMenuEntry(string text)
            : base(text)
        {
        }
    }
}
