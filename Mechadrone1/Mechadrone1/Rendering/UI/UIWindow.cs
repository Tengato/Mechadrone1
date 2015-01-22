using System;
using Mechadrone1.Screens;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    abstract class UIWindow
    {
        // Popups don't force underlying windows to exit.
        public bool IsPopup { get; set; }

        // Indicates how long the window takes to transition on/off when it is activated/deactivated.
        public TimeSpan TransitionOnTime { get; set; }
        public TimeSpan TransitionOffTime { get; set; }

        // The current position of the window transition. Ranges from zero (fully active) to one (fully off).
        public float TransitionPosition { get; set; }

        public ScreenState WindowState { get; set; }

        // Setting this to true causes the window to transition off and be removed from the WindowManager.
        public bool IsExiting { get; protected internal set; }

        public IUIWindowManager Manager { get; set; }

        public PlayerIndex? ControllingPlayer { get; internal set; }


        public virtual InputHandler InputHandler { get { return null; } }

        public UIWindow()
        {
            IsPopup = false;
            TransitionOnTime = TimeSpan.Zero;
            TransitionOffTime = TimeSpan.Zero;
            TransitionPosition = 1.0f;
            WindowState = ScreenState.TransitionOn;
            IsExiting = false;
            Manager = null;
            //mOtherWindowHasFocus = false;
        }

        // Updates TransitionPosition and WindowState, removes the window if exiting and it has transitioned off.
        public virtual void Update(GameTime gameTime,
                                   //bool otherWindowHasFocus,
                                   bool coveredByOtherWindow)
        {
            //mOtherWindowHasFocus = otherWindowHasFocus;

            bool isStillTransitioning;
            if (IsExiting)
            {
                // If the window has been flagged for exit, it should transition off.
                WindowState = ScreenState.TransitionOff;

                UpdateTransition(gameTime, TransitionOffTime, 1, out isStillTransitioning);
                if (!isStillTransitioning)
                {
                    // When the transition finishes, remove the window.
                    Manager.RemoveWindow(this);
                }
            }
            else if (coveredByOtherWindow)
            {
                // If the window is covered by another, it should transition off.
                UpdateTransition(gameTime, TransitionOffTime, 1, out isStillTransitioning);
                if (isStillTransitioning)
                {
                    WindowState = ScreenState.TransitionOff;
                }
                else
                {
                    WindowState = ScreenState.Hidden;
                }
            }
            else
            {
                // Otherwise the window should transition on and become active.
                UpdateTransition(gameTime, TransitionOnTime, -1, out isStillTransitioning);
                if (isStillTransitioning)
                {
                    WindowState = ScreenState.TransitionOn;
                }
                else
                {
                    WindowState = ScreenState.Active;
                }
            }
        }

        private void UpdateTransition(GameTime gameTime, TimeSpan time, int direction, out bool isStillTransitioning)
        {
            // How much should we move by?
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1.0f;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);

            // Update the transition position.
            TransitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if (((direction < 0) && (TransitionPosition <= 0.0f)) ||
                ((direction > 0) && (TransitionPosition >= 1.0f)))
            {
                TransitionPosition = MathHelper.Clamp(TransitionPosition, 0.0f, 1.0f);
                isStillTransitioning = false;
            }

            // Otherwise we are still busy transitioning.
            isStillTransitioning = true;
        }

        /// <summary>
        /// This is called when the window should draw itself.
        /// </summary>
        public virtual void Draw(float aspectRatio, GameTime gameTime) { }

        /// <summary>
        /// Tells the window to go away. Unlike Manager.RemoveWindow, which
        /// instantly kills the window, this method respects the transition timings
        /// and will give the window a chance to gradually transition off.
        /// </summary>
        public void ExitWindow()
        {
            if (TransitionOffTime == TimeSpan.Zero)
            {
                // If the window has a zero transition time, remove it immediately.
                Manager.RemoveWindow(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                IsExiting = true;
            }
        }
    }
}
