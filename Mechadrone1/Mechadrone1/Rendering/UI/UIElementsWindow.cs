using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class UIElementsWindow : UIWindow
    {
        public SortedList<int, GameUIElement> UIElements { get; private set; }
        public Scene Scene { get; private set; }
        public ICamera Camera { get; set; }

        public UIElementsWindow(Scene scene)
            : this(scene, new StaticCamera())
        {
            CameraComponent.CameraUpdated += CameraUpdatedHandler;
        }

        public UIElementsWindow(Scene scene, ICamera camera)
        {
            UIElements = new SortedList<int, GameUIElement>();
            Scene = scene;
            Camera = camera;
        }

        public void CameraUpdatedHandler(object sender, EventArgs e)
        {
            Camera = ((CameraComponent)sender).Camera;
        }

        public override void Draw(float aspectRatio, GameTime gameTime)
        {
            Camera.AspectRatio = aspectRatio;
            // Run through screen elements, back to front.
            foreach (GameUIElement element in UIElements.Values)
            {
                element.Draw(this, gameTime);
            }
        }

    }
}
