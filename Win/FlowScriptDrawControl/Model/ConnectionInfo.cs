namespace FlowScriptDrawControl.Model
{
    class ConnectionInfo
    {
        public int SrcId
        {
            get;
            private set;
        }

        public string SrcName
        {
            get;
            private set;
        }

        public int DestId
        {
            get;
            private set;
        }

        public string DestName
        {
            get;
            private set;
        }

        public ConnectionInfo(int srcId, string srcName, int destId, string destName)
        {
            SrcId = srcId;
            SrcName = srcName;
            DestId = destId;
            DestName = destName;
        }
    }
}
