namespace PPDSound
{
    class SoundPlayingInfo
    {
        public string FileName
        {
            get;
            private set;
        }

        public int Volume
        {
            get;
            private set;
        }

        public BufferInfo BufferInfo
        {
            get;
            private set;
        }

        public SoundPlayingInfo(string fileName, int volume, BufferInfo bufferInfo)
        {
            FileName = fileName;
            Volume = volume;
            BufferInfo = bufferInfo;
        }
    }
}
