using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1.Screens
{
    class SaveLoadScreen : Screen
    {
        public enum SaveLoadScreenMode
        {
            Save,
            Load,
        };

        public static int MaximumSaveGameDescriptions { get { return 10; } }

        private SaveLoadScreenMode mMode;
        private int mCurrentSlot;
        private ContentManager mContentLoader;
        private Texture2D mBackgroundTexture;
        private Vector2 mBackgroundPosition;
        private Texture2D mBackTexture;
        private Vector2 mBackPosition;
        private Texture2D mDeleteTexture;
        private Vector2 mDeletePosition;
        private Vector2 mDeleteTextPosition;
        private Texture2D mSelectTexture;
        private Vector2 mSelectPosition;
        private Texture2D mHighlightTexture;
        private Vector2 mTitleTextPosition;
        private Vector2 mBackTextPosition;
        private Vector2 mSelectTextPosition;
        private List<SaveGameDescription> mSaveGameDescriptions;

        public SaveLoadScreen(SaveLoadScreenMode mode)
        {
            mMode = mode;
            mDeletePosition = new Vector2(400.0f, 610.0f);
            mDeleteTextPosition = new Vector2(410.0f, 615.0f);
            mSaveGameDescriptions = null;
            TransitionOnTime = TimeSpan.FromSeconds(0.5d);
            TransitionOffTime = TimeSpan.FromSeconds(0.5d);
        }

        public override void LoadContent()
        {
            if (mContentLoader == null)
                mContentLoader = new ContentManager(SharedResources.Game.Services, "Content");

            mBackgroundTexture = mContentLoader.Load<Texture2D>(@"textures\sci fi by chamoth143");
            mBackTexture = mContentLoader.Load<Texture2D>(@"textures\buttons\facebutton_b");
            mSelectTexture = mContentLoader.Load<Texture2D>(@"textures\buttons\facebutton_a");
            mDeleteTexture = mContentLoader.Load<Texture2D>(@"textures\buttons\facebutton_x");
            mHighlightTexture = mContentLoader.Load<Texture2D>(@"Textures\HighlightBar");

            // calculate the image positions
            Viewport viewport = SharedResources.Game.GraphicsDevice.Viewport;
            mBackgroundPosition = new Vector2(
                (viewport.Width - mBackgroundTexture.Width) / 2,
                (viewport.Height - mBackgroundTexture.Height) / 2);
            mBackPosition = mBackgroundPosition + new Vector2(545, 580);
            mSelectPosition = mBackgroundPosition + new Vector2(1120, 580);

            // calculate the text positions
            mTitleTextPosition = mBackgroundPosition + new Vector2(
                (mBackgroundTexture.Width - SharedResources.FontManager.MeasureString(FontType.ArialMedium, "Load").X) / 2,
                60.0f);
            mBackTextPosition = new Vector2(mBackPosition.X + 55, mBackPosition.Y + 15);
            mDeleteTextPosition.X += mDeleteTexture.Width;
            mSelectTextPosition = new Vector2(
                mSelectPosition.X - SharedResources.FontManager.MeasureString(FontType.ArialSmall, "Select").X - 5,
                mSelectPosition.Y + 5);

            StorageManager.GetSaveGameDescriptions(SaveGameDescriptionsRetrievedCallback);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();

            mContentLoader.Unload();
            mContentLoader.Dispose();
        }

        private void SaveGameDescriptionsRetrievedCallback(List<SaveGameDescription> saveGameDescriptions)
        {
            mSaveGameDescriptions = saveGameDescriptions;
        }

        public override void HandleInput(GameTime gameTime, InputManager input)
        {
            PlayerIndex dummyPlayerIndex;
            if (input.IsMenuCancel(ControllingPlayer, out dummyPlayerIndex))
            {
                ExitScreen();
                return;
            }

            // handle selecting a save game
            if (input.IsMenuSelect(ControllingPlayer, out dummyPlayerIndex) &&
                (mSaveGameDescriptions != null))
            {
                switch (mMode)
                {
                    case SaveLoadScreenMode.Load:
                        if ((mCurrentSlot >= 0) &&
                            (mCurrentSlot < mSaveGameDescriptions.Count) &&
                            (mSaveGameDescriptions[mCurrentSlot] != null))
                        {
                            if (GameResources.GameDossier != null)
                            {
                                MessageBoxScreen messageBoxScreen = new MessageBoxScreen("Are you sure you want to load this game?");
                                messageBoxScreen.Accepted += ConfirmLoadMessageBoxAccepted;
                                ScreenManager.AddScreen(messageBoxScreen, ControllingPlayer);
                            }
                            else
                            {
                                ConfirmLoadMessageBoxAccepted(null, EventArgs.Empty);
                            }
                        }
                        break;

                    case SaveLoadScreenMode.Save:
                        if ((mCurrentSlot >= 0) && 
                            (mCurrentSlot <= mSaveGameDescriptions.Count))
                        {
                            if (mCurrentSlot == mSaveGameDescriptions.Count)
                            {
                                ConfirmSaveMessageBoxAccepted(null, EventArgs.Empty);
                            }
                            else
                            {
                                MessageBoxScreen messageBoxScreen = new MessageBoxScreen("Are you sure you want to overwrite this save game?");
                                messageBoxScreen.Accepted += ConfirmSaveMessageBoxAccepted;
                                ScreenManager.AddScreen(messageBoxScreen, ControllingPlayer);
                            }
                        }
                        break;
                }

            }
            // handle deletion
            else if (input.IsMenuDelete(ControllingPlayer) &&
                (mSaveGameDescriptions != null))
            {
                if ((mCurrentSlot >= 0) &&
                    (mCurrentSlot < mSaveGameDescriptions.Count) &&
                    (mSaveGameDescriptions[mCurrentSlot] != null))
                {
                    MessageBoxScreen messageBoxScreen = new MessageBoxScreen(
                        "Are you sure you want to delete this save game?");
                    messageBoxScreen.Accepted += ConfirmDeleteMessageBoxAccepted;
                    ScreenManager.AddScreen(messageBoxScreen, ControllingPlayer);
                }
            }
            // handle cursor-down
            else if (input.IsMenuDown(ControllingPlayer) &&
                (mSaveGameDescriptions != null))
            {
                int maximumSlot = mSaveGameDescriptions.Count;
                if (mMode == SaveLoadScreenMode.Save)
                {
                    maximumSlot = Math.Min(maximumSlot + 1, 
                        MaximumSaveGameDescriptions);
                }
                if (mCurrentSlot < maximumSlot - 1)
                {
                    mCurrentSlot++;
                }
            }
            // handle cursor-up
            else if (input.IsMenuUp(ControllingPlayer) &&
                (mSaveGameDescriptions != null))
            {
                if (mCurrentSlot >= 1)
                {
                    mCurrentSlot--;
                }
            }
        }


        /// <summary>
        /// Callback for the Save Game confirmation message box.
        /// </summary>
        void ConfirmSaveMessageBoxAccepted(object sender, EventArgs e)
        {
            if (mCurrentSlot >= 0 && mCurrentSlot <= mSaveGameDescriptions.Count)
            {
                if (mCurrentSlot == mSaveGameDescriptions.Count)
                {
                    StorageManager.SaveDossier(null);
                }
                else
                {
                    StorageManager.SaveDossier(mSaveGameDescriptions[mCurrentSlot]);
                }
                ExitScreen();
            }
        }


        /// <summary>
        /// Delegate type for the save-game-selected-to-load event.
        /// </summary>
        /// <param name="saveGameDescription">
        /// The description of the file to load.
        /// </param>
        public delegate void LoadingSaveGameHandler(SaveGameDescription saveGameDescription, PlayerIndex playerIndex);

        /// <summary>
        /// Fired when a save game is selected to load.
        /// </summary>
        /// <remarks>
        /// Loading save games exits multiple screens, 
        /// so we use events to move backwards.
        /// </remarks>
        public event LoadingSaveGameHandler LoadingSaveGame;

        /// <summary>
        /// Callback for the Load Game confirmation message box.
        /// </summary>
        void ConfirmLoadMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((mSaveGameDescriptions != null) && (mCurrentSlot >= 0) &&
                (mCurrentSlot < mSaveGameDescriptions.Count) &&
                (mSaveGameDescriptions[mCurrentSlot] != null))
            {
                ExitScreen();
                OnLoadingSaveGame(mSaveGameDescriptions[mCurrentSlot]);
            }
        }

        private void OnLoadingSaveGame(SaveGameDescription saveGameDescription)
        {
            LoadingSaveGameHandler handler = LoadingSaveGame;

            if (handler != null)
            {
                handler(saveGameDescription, ControllingPlayer.Value);
            }
        }


        /// <summary>
        /// Callback for the Delete Game confirmation message box.
        /// </summary>
        void ConfirmDeleteMessageBoxAccepted(object sender, EventArgs e)
        {
            if ((mSaveGameDescriptions != null) && (mCurrentSlot >= 0) &&
                (mCurrentSlot < mSaveGameDescriptions.Count) &&
                (mSaveGameDescriptions[mCurrentSlot] != null))
            {
                StorageManager.DeleteSaveGame(mSaveGameDescriptions[mCurrentSlot]);
            }
        }

        /// <summary>
        /// Draws the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = SharedResources.Game.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            SpriteBatch sb = SharedResources.SpriteBatch;
            FontManager fm = SharedResources.FontManager;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            fm.BeginText();

            Color whiteFade = new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, TransitionAlpha);

            sb.Draw(mBackgroundTexture, fullscreen, whiteFade);

            sb.Draw(mBackTexture, mBackPosition, whiteFade);
            fm.DrawText(FontType.ArialSmall, "Back", mBackTextPosition, whiteFade, true);

            const string text = "image by:\nhttp://chamoth143.deviantart.com/";
            Vector2 position = new Vector2(20.0f, 620.0f);
            fm.DrawText(FontType.ArialSmall, text, position, Color.Multiply(Color.Multiply(Color.SteelBlue, 0.5f), TransitionAlpha), true);

            fm.DrawText(FontType.ArialMedium, (mMode == SaveLoadScreenMode.Load ? "Load" : "Save"), mTitleTextPosition,
                Color.Multiply(Color.GreenYellow, TransitionAlpha), true);

            if ((mSaveGameDescriptions != null))
            {
                for (int i = 0; i < mSaveGameDescriptions.Count; i++)
                {
                    Vector2 descriptionTextPosition = new Vector2(295f,
                        200f + i * (fm.Fonts[(int)(FontType.ArialSmall)].LineSpacing + 40f));
                    Color descriptionTextColor = Color.Multiply(Color.GreenYellow, TransitionAlpha);

                    // if the save game is selected, draw the highlight color
                    if (i == mCurrentSlot)
                    {
                        descriptionTextColor = Color.Multiply(Color.HotPink, TransitionAlpha);
                        sb.Draw(mHighlightTexture, descriptionTextPosition + new Vector2(-100, -23), whiteFade);

                        sb.Draw(mDeleteTexture, mDeletePosition, whiteFade);
                        fm.DrawText(FontType.ArialSmall, "Delete", mDeleteTextPosition, whiteFade, true);

                        sb.Draw(mSelectTexture, mSelectPosition, whiteFade);
                        fm.DrawText(FontType.ArialSmall, "Select", mSelectTextPosition, whiteFade, true);
                    }

                    fm.DrawText(FontType.ArialSmall,
                        mSaveGameDescriptions[i].ChapterName,
                        descriptionTextPosition, descriptionTextColor, true);
                    descriptionTextPosition.X = 650;
                    fm.DrawText(FontType.ArialSmall,
                        mSaveGameDescriptions[i].Description,
                        descriptionTextPosition, descriptionTextColor, true);
                }

                // if there is space for one, add an empty entry
                if (mMode == SaveLoadScreenMode.Save && mSaveGameDescriptions.Count < MaximumSaveGameDescriptions)
                {
                    int i = mSaveGameDescriptions.Count;
                    Vector2 descriptionTextPosition = new Vector2(295f,
                        200f + i * (fm.Fonts[(int)(FontType.ArialSmall)].LineSpacing + 40f));
                    Color descriptionTextColor = Color.Multiply(Color.GreenYellow, TransitionAlpha);

                    // if the save game is selected, draw the highlight color
                    if (i == mCurrentSlot)
                    {
                        descriptionTextColor = Color.Multiply(Color.HotPink, TransitionAlpha);
                        sb.Draw(mHighlightTexture, descriptionTextPosition + new Vector2(-100, -23), whiteFade);
                        sb.Draw(mSelectTexture, mSelectPosition, whiteFade);
                        fm.DrawText(FontType.ArialSmall, "Select", mSelectTextPosition, whiteFade, true);
                    }

                    fm.DrawText(FontType.ArialSmall, "-------empty------", descriptionTextPosition, descriptionTextColor, true);
                    descriptionTextPosition.X = 650;
                    fm.DrawText(FontType.ArialSmall, "-----", descriptionTextPosition, descriptionTextColor, true);
                }
            }

            if (mSaveGameDescriptions == null)
            {
                fm.DrawText(FontType.ArialSmall, 
                    "No storage device available",
                    new Vector2(395f, 200f), Color.Multiply(Color.GreenYellow, TransitionAlpha), true);
            }
            else if (mMode == SaveLoadScreenMode.Load && mSaveGameDescriptions.Count <= 0)
            {
                fm.DrawText(FontType.ArialSmall, 
                    "No save games available",
                    new Vector2(395f, 200f), Color.Multiply(Color.GreenYellow, TransitionAlpha), true);
            }

            sb.End();
            fm.EndText();
        }

    }
}
