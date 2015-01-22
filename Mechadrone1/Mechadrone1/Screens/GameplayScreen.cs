using System;
using System.Threading;
using System.Linq;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Mechadrone1.Screens
{
    /// <summary>
    /// This screen takes care of the gameplay presentation.
    /// </summary>
    class GameplayScreen : Screen
    {
        public enum ViewportLayout
        {
            Invalid,            // To initialize variables...
            FullScreen,         // 1 HumanView, PlayerIndex = One/0
            VerticalSplit,      // 2 HumanViews, PlayerIndex of left = One/0, right = Two/1
            HorizontalSplit,    // 2 HumanViews, PlayerIndex of top = One/0, bottom = Two/1
            FourWaySplit        // 3 or 4 HumanViews, PlayerIndex of top left = One/0, top right = Two/1, bottom left = Three/2, bottom right = Four/3 (if necessary)
        }

        private Scene mScene;
        private ActorManager mActorMan;
        private ObservableCollection<PlayerView> mViews;
        private ViewportLayout mScreenLayout;
        private List<DrawSegment> mDrawSegments;
        private ContentManager mViewContentLoader;
        private PlayRequest mRequest;
        private float mPauseAlpha; // The amount to darken this screen when it is partially covered by another screen.
        private int mFrameCounter;
        private int mFrameRate;
        private TimeSpan mFrameRateTimer;
        private Queue<double> mFrameTimes;
        private double mPrevFrameTime;

        public GameplayScreen(PlayRequest request)
        {
            mRequest = request;
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            mScene = null;
            mViews = new ObservableCollection<PlayerView>();
            mActorMan = null;
            mScreenLayout = ViewportLayout.Invalid;
            mDrawSegments = new List<DrawSegment>();
            mViewContentLoader = null;
            mPauseAlpha = 0.5f;
            mFrameCounter = 0;
            mFrameRate = 0;
            mFrameRateTimer = new TimeSpan();

            mFrameTimes = new Queue<double>();
            for (int f = 0; f < 30; ++f)
            {
                mFrameTimes.Enqueue(160.0d);
            }

            mPrevFrameTime = -1.0d;
        }

        public override void LoadContent()
        {
            mScene = new Scene();
            mActorMan = new ActorManager(mViews);
            GameResources.ActorManager = mActorMan;

            LevelManifest manifest;
            using (ContentManager manifestLoader = new ContentManager(SharedResources.Game.Services, "Content"))
            {
                manifest = manifestLoader.Load<LevelManifest>(mRequest.LevelName);
                manifestLoader.Unload();    // a LevelManifest does not use any Disposable resources, so this is ok.
            }

            // The IScreenHoncho tells this gameplay screen how to set up the PlayerViews and DrawSegments required to play this level
            IScreenHoncho screenHoncho = Activator.CreateInstance(Type.GetType(manifest.ScreenHonchoTypeName)) as IScreenHoncho;

            mViewContentLoader = new ContentManager(SharedResources.Game.Services, "Content");

            foreach (PlayerInfo player in GameResources.PlaySession.Players)
            {
                mViews.Add(PlayerViewFactory.Create(player,
                                                    mRequest.CharacterSelections[player.PlayerId],
                                                    screenHoncho,
                                                    mViewContentLoader));
            }

            // Determine screen layout and create DrawSegments
            if (screenHoncho.PlayersUseSeparateViewports)
            {
                switch (GameResources.PlaySession.LocalPlayers.Count)
                {
                    case 1:
                        mScreenLayout = ViewportLayout.FullScreen;
                        break;
                    case 2:
                        mScreenLayout = GameOptions.PreferHorizontalSplit ? ViewportLayout.HorizontalSplit : ViewportLayout.VerticalSplit;
                        break;
                    case 3:
                    case 4:
                        mScreenLayout = ViewportLayout.FourWaySplit;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported number of local players.");
                }

                foreach (int playerId in GameResources.PlaySession.LocalPlayers.Keys)
                {
                    HumanView playerView = mViews.Single(v => v.PlayerId == playerId) as HumanView;
                    ICameraProvider cameraPlayerView = playerView as ICameraProvider;
                    if (cameraPlayerView == null)
                        throw new LevelManifestException("When IScreenHoncho.PlayersUseSeparateViewports is true, HumanViews must implement the ICameraProvider interface.");
                    DrawSegment ds = new DrawSegment(mScene, cameraPlayerView.Camera);
                    mDrawSegments.Add(ds);
                    playerView.DrawSegment = ds;
                }
            }
            else
            {
                mScreenLayout = ViewportLayout.FullScreen;
                DrawSegment ds = new DrawSegment(mScene);
                mDrawSegments.Add(ds);
                foreach (int playerId in GameResources.PlaySession.LocalPlayers.Keys)
                {
                    HumanView playerView = mViews.Single(v => v.PlayerId == playerId) as HumanView;
                    playerView.DrawSegment = ds;
                }

            }

            // TODO: P3: Make sure all non-local players are connected
            mScene.Load();

            mActorMan.LoadContent(manifest);

            // TODO: P3: Make sure all remote clients have loaded their game.

            foreach (PlayerView view in mViews)
            {
                view.Load();
            }

            GameResources.LoadNewLevelDelegate = LoadNewLevel;

            SharedResources.Game.GraphicsDevice.DeviceLost += DeviceLostHandler;
            SharedResources.Game.GraphicsDevice.DeviceResetting += DeviceResettingHandler;
            SharedResources.Game.GraphicsDevice.DeviceReset += DeviceResetHandler;
        }

        private void DeviceLostHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("{0} Losing device...", DateTime.Now);
        }

        private void DeviceResettingHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("{0} Resetting device...", DateTime.Now);
        }

        private void DeviceResetHandler(object sender, EventArgs e)
        {
            Debug.WriteLine("{0} Reset device complete.", DateTime.Now);
        }

        public override void UnloadContent()
        {
            mDrawSegments.Clear();
            EffectRegistry.ClearRegistry();
            mScene.Release();
            GameResources.ActorManager.Release();
            GameResources.ActorManager = null;
            GameResources.LoadNewLevelDelegate = null;
            while (mViews.Count > 0)
            {
                mViews[mViews.Count - 1].Unload();
                mViews.Remove(mViews[mViews.Count - 1]);
            }
            mViewContentLoader.Unload();
            mViewContentLoader.Dispose();
            SharedResources.Game.GraphicsDevice.DeviceLost -= DeviceLostHandler;
            SharedResources.Game.GraphicsDevice.DeviceResetting -= DeviceResettingHandler;
            SharedResources.Game.GraphicsDevice.DeviceReset -= DeviceResetHandler;

            // Try to have all the big loaded objects garbage collected:
            mScene = null;
            mActorMan = null;
            System.GC.Collect();
        }

        private void LoadNewLevel(string levelName)
        {
            PlayRequest pr = new PlayRequest();
            pr.LevelName = levelName;
            for (int p = 0; p < GameResources.PlaySession.Players.Count; ++p)
            {
                CharacterInfo testChar;
                if (p < GameResources.GameDossier.Characters.Count)
                {
                    testChar = GameResources.GameDossier.Characters[p];
                }
                else
                {
                    testChar = new CharacterInfo();
                }

                pr.CharacterSelections.Add(GameResources.PlaySession.Players[p].PlayerId, testChar);
            }

            LoadingScreen.Load(ScreenManager, true, ControllingPlayer, new GameplayScreen(pr));
        }

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // 2nd param is false because this object will be handling the covered effect.
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                mPauseAlpha = Math.Min(mPauseAlpha + 1f / 32, 1);
            else
                mPauseAlpha = Math.Max(mPauseAlpha - 1f / 32, 0);

            mFrameRateTimer += gameTime.ElapsedGameTime;

            if (mFrameRateTimer > TimeSpan.FromSeconds(1))
            {
                mFrameRateTimer -= TimeSpan.FromSeconds(1);
                mFrameRate = mFrameCounter;
                mFrameCounter = 0;
            }

            if (IsActive)
            {
                mActorMan.Update(gameTime);
            }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputManager input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // The game pauses either if a user presses the pause button, or if
            // they unplug an active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!

            foreach (PlayerIndex localPlayer in GameResources.PlaySession.LocalPlayers.Values)
            {
                bool gamePadDisconnected = !input.CurrentState.PadState[(int)localPlayer].IsConnected &&
                    input.LastState.PadState[(int)localPlayer].IsConnected;

                if (input.IsPauseGame(localPlayer) || gamePadDisconnected)
                {
                    ScreenManager.AddScreen(new PauseMenuScreen(), localPlayer);
                    return;
                }
            }

            // TODO: P2: Note that RemoteViews are included here, do they need to be?
            foreach (HumanView view in mViews.OfType<HumanView>())
            {
                view.HandleInput(gameTime, input);
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            mFrameCounter++;

            GraphicsDevice gd = SharedResources.Game.GraphicsDevice;

            Viewport wholeScreen = gd.Viewport;

            for (int d = 0; d < mDrawSegments.Count; ++d)
            {
                gd.Viewport = GetViewport(d, mScreenLayout, wholeScreen);
                mDrawSegments[d].Draw(gd.Viewport.AspectRatio, gameTime);
            }

            gd.Viewport = wholeScreen;

            if (mPrevFrameTime < 0)
            {
                mPrevFrameTime = gameTime.TotalGameTime.TotalMilliseconds;
            }
            else
            {
                double nowTime = gameTime.TotalGameTime.TotalMilliseconds;
                mFrameTimes.Dequeue();
                mFrameTimes.Enqueue(nowTime - mPrevFrameTime);
                mPrevFrameTime = nowTime;
            }

            string fps = string.Format("fps: {0}   ({1:F3} ms/frame)", mFrameRate, mFrameTimes.Average());

            Color fpsColor = gameTime.IsRunningSlowly == true ? Color.Orange : Color.White;

            SharedResources.FontManager.BeginText();
            SharedResources.FontManager.DrawText(FontType.ArialSmall, fps, new Vector2(32, 32), fpsColor, true);
            SharedResources.FontManager.EndText();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || mPauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, mPauseAlpha / 2);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        private Viewport GetViewport(int viewportIndex, ViewportLayout mScreenLayout, Viewport baseViewport)
        {
            // viewportIndex:
            // +-----------+      +-----------+      +-----+-----+      +-----+-----+
            // |           |      |     0     |      |     |     |      |  0  |  1  |
            // |     0     |      +-----------+      |  0  |  1  |      +-----+-----+
            // |           |      |     1     |      |     |     |      |  2  |  3  |
            // +-----------+      +-----------+      +-----+-----+      +-----+-----+
            // FullScreen         HorizontalSplit    VerticalSplit      FourWaySplit

            if (mScreenLayout == ViewportLayout.FullScreen)
                return baseViewport;

            Viewport splitViewport = new Viewport(baseViewport.Bounds);
            switch (mScreenLayout)
            {
                case ViewportLayout.HorizontalSplit:
                    if (viewportIndex == 0)
                    {
                        splitViewport.Height /= 2;
                    }
                    else
                    {
                        splitViewport.Y = splitViewport.Height / 2;
                        splitViewport.Height = splitViewport.Height - splitViewport.Y;
                    }

                    break;
                case ViewportLayout.VerticalSplit:
                    if (viewportIndex == 0)
                    {
                        splitViewport.Width /= 2;
                    }
                    else
                    {
                        splitViewport.X = splitViewport.Width / 2;
                        splitViewport.Width = splitViewport.Width - splitViewport.X;
                    }

                    break;
                case ViewportLayout.FourWaySplit:
                    if (viewportIndex == 0 || viewportIndex == 1)
                    {
                        splitViewport.Height /= 2;
                    }
                    else
                    {
                        splitViewport.Y = splitViewport.Height / 2;
                        splitViewport.Height = splitViewport.Height - splitViewport.Y;
                    }

                    if (viewportIndex == 0 || viewportIndex == 2)
                    {
                        splitViewport.Width /= 2;
                    }
                    else
                    {
                        splitViewport.X = splitViewport.Width / 2;
                        splitViewport.Width = splitViewport.Width - splitViewport.X;
                    }

                    break;
                default:
                    throw new InvalidOperationException("Screen layout not defined.");
            }

            return splitViewport;
        }
    }
}
