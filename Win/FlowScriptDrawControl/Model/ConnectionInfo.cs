namespace FlowScriptDrawControl.Model
{
    class ConnectionInfo(int srcId, string srcName, int destId, string destName)
    {
        public int SrcId
        {
            get;
            private set;
        } = srcId;

        public string SrcName
        {
            get;
            private set;
        } = srcName;

        public int DestId
        {
            get;
            private set;
        } = destId;

        public string DestName
        {
            get;
            private set;
        } = destName;
    }
}
