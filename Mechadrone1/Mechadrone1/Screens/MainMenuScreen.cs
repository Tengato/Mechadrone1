using Microsoft.Xna.Framework;

namespace Mechadrone1.Screens
{
    class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry newGameMenuEntry = new MenuEntry("New Game");
            MenuEntry loadGameMenuEntry = new MenuEntry("Load Game");
            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry creditsMenuEntry = new MenuEntry("Credits");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            newGameMenuEntry.Selected += NewGameMenuEntrySelected;
            loadGameMenuEntry.Selected += LoadGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            creditsMenuEntry.Selected += CreditsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(newGameMenuEntry);
            MenuEntries.Add(loadGameMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(creditsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        private void NewGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            GameResources.GameDossier = GameDossier.CreateNewGame();

            StartGame(e.PlayerIndex);
        }

        private void LoadGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            SaveLoadScreen slScreen = new SaveLoadScreen(SaveLoadScreen.SaveLoadScreenMode.Load);
            slScreen.LoadingSaveGame += FileToLoadChosenHandler;
            ScreenManager.AddScreen(slScreen, e.PlayerIndex);
        }

        private void FileToLoadChosenHandler(SaveGameDescription saveGameDescription, PlayerIndex playerIndex)
        {
            ControllingPlayer = playerIndex;
            StorageManager.LoadFromSaveFile(saveGameDescription, DossierLoadedCallback);
        }

        private void DossierLoadedCallback(GameDossier loadedDossier)
        {
            GameResources.GameDossier = loadedDossier; // Depend on the screen manager to clean this up.
            StartGame(ControllingPlayer.Value);
        }

        private void StartGame(PlayerIndex inputIndex)
        {
            GameResources.PlaySession = new PlaySession(); // Depend on the screen manager to clean this up.

            PlayerInfo playerOne = new PlayerInfo(0, PlayerInfo.PlayerType.Local);
            GameResources.PlaySession.Players.Add(playerOne);
            GameResources.PlaySession.LocalPlayers.Add(playerOne.PlayerId, inputIndex);

            // TODO: P2: Go through character selection screen first.

            PlayRequest testPlay = new PlayRequest();
            testPlay.LevelName = "levels\\Hub";
            CharacterInfo testChar = new CharacterInfo();
            testChar.TemplateName = "Mechadrone";
            testPlay.CharacterSelections.Add(playerOne.PlayerId, testChar);

            LoadingScreen.Load(ScreenManager, true, inputIndex, new GameplayScreen(testPlay));
        }

        private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        private void CreditsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new CreditsScreen(), e.PlayerIndex);
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit this sample?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            SharedResources.Game.Exit();
        }
    }
}
