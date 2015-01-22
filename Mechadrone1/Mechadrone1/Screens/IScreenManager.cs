using Microsoft.Xna.Framework;

namespace Mechadrone1.Screens
{
    interface IScreenManager
    {
        void AddScreen(Screen screen, PlayerIndex? controllingPlayer);
        void RemoveScreen(Screen screen);
        Screen[] GetScreens();
        void FadeBackBufferToBlack(float alpha);
    }
}
