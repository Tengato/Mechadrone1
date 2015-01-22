using Microsoft.Xna.Framework.Content;

namespace Mechadrone1
{
    interface ICustomizable
    {
        void Customize(CharacterInfo customizations, ContentManager contentLoader);
    }
}
