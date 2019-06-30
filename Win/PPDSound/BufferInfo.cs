using SharpDX.DirectSound;

namespace PPDSound
{
    class BufferInfo
    {
        public SecondarySoundBuffer Buffer
        {
            get;
            private set;
        }

        public int Frequency
        {
            get;
            private set;
        }

        public BufferInfo(SecondarySoundBuffer buffer)
        {
            Buffer = buffer;
            Frequency = buffer.Frequency;
        }
    }
}
