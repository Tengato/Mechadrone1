namespace Mechadrone1.Screens
{
    class CreditsScreen : MenuScreen
    {
        public CreditsScreen()
            : base("Help")
        {
            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(back);
        }
    }
}
