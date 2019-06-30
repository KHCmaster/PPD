namespace PPDPack.V2
{
    class FileInfo
    {
        public string FileName
        {
            get;
            private set;
        }

        public uint FileSize
        {
            get;
            private set;
        }

        public uint Offset
        {
            get;
            private set;
        }

        public FileInfo(string fileName, uint fileSize)
        {
            FileName = fileName;
            FileSize = fileSize;
        }

        public FileInfo(string fileName, uint fileSize, uint offset)
        {
            FileName = fileName;
            FileSize = fileSize;
            Offset = offset;
        }
    }
}
