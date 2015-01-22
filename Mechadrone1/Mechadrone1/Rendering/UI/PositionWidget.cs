using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    class PositionWidget : GameUIElement
    {
        private int mActorId;

        public PositionWidget(int actorId)
        {
            mActorId = actorId;
        }

        public override void Draw(UIElementsWindow drawSegment, GameTime gameTime)
        {
            Actor avatar = GameResources.ActorManager.GetActorById(mActorId);
            if (avatar != null)
            {
                TransformComponent avatarTransform = avatar.GetComponent<TransformComponent>(ActorComponent.ComponentType.Transform);
                if (avatarTransform != null)
                {
                    Vector2 textLocation;
                    textLocation.X = 20.0f;
                    textLocation.Y = (float)(SharedResources.Game.GraphicsDevice.Viewport.Bounds.Bottom) - 30.0f;
                    SharedResources.FontManager.BeginText();
                    SharedResources.FontManager.DrawText(FontType.ArialSmall, avatarTransform.Translation.ToString(), textLocation, Color.LightGray, false);
                    SharedResources.FontManager.EndText();
                }
            }
        }
    }
}
