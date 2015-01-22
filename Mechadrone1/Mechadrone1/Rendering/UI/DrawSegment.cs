using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Mechadrone1.Screens;

namespace Mechadrone1
{
    delegate void InputHandler(GameTime gameTime, InputManager input);

    // An object that collects objects required to produce graphics for a Viewport.
    class DrawSegment : IUIWindowManager
    {
        private List<UIWindow> mWindows;
        private List<UIWindow> mWindowsToUpdate;

        // This object should only be manipulated by HumanView objects that have been assigned to use this DrawSegment by the GameplayScreen.
        public UIElementsWindow MainWindow { get { return mWindows[0] as UIElementsWindow;} }

        // Constructor for CameraComponent event-driven camera
        public DrawSegment(Scene scene)
        {
            mWindows = new List<UIWindow>();
            mWindowsToUpdate = new List<UIWindow>();
            AddWindow(new UIElementsWindow(scene), null);
        }

        // Constructor for externally controlled camera
        public DrawSegment(Scene scene, ICamera camera)
        {
            mWindows = new List<UIWindow>();
            mWindowsToUpdate = new List<UIWindow>();
            AddWindow(new UIElementsWindow(scene, camera), null);
        }

        public void AddWindow(UIWindow window, PlayerIndex? controllingPlayer)
        {
            window.Manager = this;
            window.IsExiting = false;
            window.ControllingPlayer = controllingPlayer;

            //window.LoadContent();

            mWindows.Add(window);
        }

        public void RemoveWindow(UIWindow window)
        {
            //window.UnloadContent();

            mWindows.Remove(window);
            mWindowsToUpdate.Remove(window);
        }

        // Returns an object that can handle input, or null - if the main game input processors should take the input.
        public InputHandler GetInputConsumer(PlayerIndex inputIndex)
        {
            for (int w = mWindows.Count - 1; w >= 0; --w)
            {
                if (mWindows[w].WindowState == ScreenState.Active ||
                    mWindows[w].WindowState == ScreenState.TransitionOn)
                return mWindows[w].InputHandler;
            }

            return null;
        }

        public void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            // Make a copy of the master window list, to avoid confusion if
            // the process of updating one window adds or removes others.
            mWindowsToUpdate.Clear();

            foreach (UIWindow window in mWindows)
            {
                mWindowsToUpdate.Add(window);
            }

            // These flags are initialized for the topmost window:
            bool coveredByOtherWindow = false;
            bool isCoveringUnderlyingWindows = false;

            // Loop as long as there are screens waiting to be updated.
            while (mWindowsToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                UIWindow window = mWindowsToUpdate[mWindowsToUpdate.Count - 1];

                mWindowsToUpdate.RemoveAt(mWindowsToUpdate.Count - 1);

                if (window.WindowState == ScreenState.TransitionOn ||
                    window.WindowState == ScreenState.Active)
                {
                    // If this is an active non-popup, inform any subsequent
                    // windows that they are covered by it.
                    isCoveringUnderlyingWindows = !window.IsPopup;
                }

                // Update the screen.
                window.Update(e.GameTime, coveredByOtherWindow);

                // Update flags for next screen:
                coveredByOtherWindow |= isCoveringUnderlyingWindows;
            }
        }

        public virtual void Draw(float aspectRatio, GameTime gameTime)
        {
            // Note: this will loop in back-to-front order.
            foreach (UIWindow window in mWindows)
            {
                if (window.WindowState == ScreenState.Hidden)
                    continue;

                window.Draw(aspectRatio, gameTime);
            }
        }
    }
}
