using System;

namespace Mechadrone1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MechadroneGame game = new MechadroneGame())
            {
                game.Run();
                game.Cleanup();
            }
        }
    }
#endif
}

