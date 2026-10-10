namespace PPDSound
{
    class SoundPlayingInfo(string fileName, int volume, BufferInfo bufferInfo)
    {
        public string FileName
        {
            get;
            private set;
        } = fileName;

        public int Volume
        {
            get;
            private set;
        } = volume;

        public BufferInfo BufferInfo
        {
            get;
            private set;
        } = bufferInfo;
    }
}
