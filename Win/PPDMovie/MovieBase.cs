using PPDFramework;

namespace PPDMovie
{
    public abstract class MovieBase : MusicPlayerBase
    {
        private const int WM_GRAPHNOTIFY = 0x0400 + 13;

        public override bool IsAudioOnly
        {
            get { return false; }
        }

        protected MovieBase(PPDDevice device, string filename)
            : base(device, filename)
        {
        }
    }
}
