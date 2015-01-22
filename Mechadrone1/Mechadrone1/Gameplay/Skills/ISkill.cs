using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    interface ISkill
    {
        void UpdateInputState(bool input, BipedControllerComponent bipedController);
        void Initialize(ContentManager contentLoader, ComponentManifest manifest);
    }
}
