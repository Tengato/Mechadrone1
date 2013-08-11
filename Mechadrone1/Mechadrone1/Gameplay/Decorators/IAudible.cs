using System;

namespace Mechadrone1.Gameplay.Decorators
{
    interface IAudible
    {
        event MakeSoundEventHandler MakeSound;
    }

    class MakeSoundEventArgs : EventArgs
    {
        public MakeSoundEventArgs(string soundName, float normalizedVolume)
        {
            SoundName = soundName;
            NormalizedVolume = normalizedVolume;
        }

        public string SoundName;
        public float NormalizedVolume;
    }

    delegate void MakeSoundEventHandler(object sender, MakeSoundEventArgs e);
}
