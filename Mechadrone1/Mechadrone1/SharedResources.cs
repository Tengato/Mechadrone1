using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace Mechadrone1
{
    static class SharedResources
    {
        public static Game Game { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static FontManager FontManager { get; set; }
        public static AudioEngine AudioEngine { get; set; }
        public static WaveBank WaveBank { get; set; }
        public static SoundBank SoundBank { get; set; }
        public static bool GraphicsDeviceReady { get; set; }
        public static bool GraphicsDeviceResetting { get; set; }
        public static StorageDevice StorageDevice { get; set; }
        public static GamerServicesComponent GamerServices { get; set; }

        static SharedResources()
        {
            Game = null;
            SpriteBatch = null;
            FontManager = null;
            AudioEngine = null;
            WaveBank = null;
            SoundBank = null;
            GraphicsDeviceReady = true;
            GraphicsDeviceResetting = false;
            StorageDevice = null;
            GamerServices = null;
        }

    }
}
