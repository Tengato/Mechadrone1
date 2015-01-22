using Microsoft.Xna.Framework;

namespace Mechadrone1
{
    interface IUIWindowManager
    {
        void RemoveWindow(UIWindow window);
        void AddWindow(UIWindow window, PlayerIndex? controllingPlayer);
    }
}
