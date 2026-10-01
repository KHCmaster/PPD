using PPDFramework;

namespace PPDMovie
{
    public abstract class MovieBase(PPDDevice device, string filename) : MusicPlayerBase(device, filename)
    {
        private const int WM_GRAPHNOTIFY = 0x0400 + 13;

        public override bool IsAudioOnly
        {
            get { return false; }
        }
    }
}
