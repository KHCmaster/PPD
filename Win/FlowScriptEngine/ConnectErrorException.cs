using System;

namespace FlowScriptEngine
{
    public class ConnectErrorException(Exception e, string srcName, string destName, string srcMemberName, string destMemberName) : Exception("Error in connection", e)
    {
        public string SrcName
        {
            get;
            private set;
        } = srcName;

        public string DestName
        {
            get;
            private set;
        } = destName;

        public string SrcMemberName
        {
            get;
            private set;
        } = srcMemberName;
        public string DestMemberName
        {
            get;
            private set;
        } = destMemberName;
    }
}
