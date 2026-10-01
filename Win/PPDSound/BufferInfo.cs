using Vortice.DirectSound;

namespace PPDSound
{
    class BufferInfo(IDirectSoundBuffer buffer)
    {
        public IDirectSoundBuffer Buffer
        {
            get;
            private set;
        } = buffer;

        public int Frequency
        {
            get;
            private set;
        } = buffer.Frequency;
    }
}
