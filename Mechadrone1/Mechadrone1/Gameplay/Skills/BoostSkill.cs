using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class BoostSkill : ISkill
    {
        public BoostSkill(int actorOwnerId)
        {
        }

        public void Initialize(ContentManager contentLoader, ComponentManifest manifest) { }

        public void UpdateInputState(bool inputState, BipedControllerComponent bipedController)
        {
            if (inputState)
            {
                bipedController.DesiredMovementActions |= BipedControllerComponent.MovementActions.Boosting;
            }
            else
            {
                bipedController.DesiredMovementActions &= ~BipedControllerComponent.MovementActions.Boosting;
            }
        }
    }
}
