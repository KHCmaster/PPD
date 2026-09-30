using Vortice.DirectSound;

namespace PPDSound
{
    class BufferInfo
    {
        public IDirectSoundBuffer Buffer
        {
            get;
            private set;
        }

        public int Frequency
        {
            get;
            private set;
        }

        public BufferInfo(IDirectSoundBuffer buffer)
        {
            Buffer = buffer;
            Frequency = buffer.Frequency;
        }
    }
}
