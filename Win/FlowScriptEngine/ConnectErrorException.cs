using System;

namespace FlowScriptEngine
{
    public class ConnectErrorException : Exception
    {
        public string SrcName
        {
            get;
            private set;
        }

        public string DestName
        {
            get;
            private set;
        }

        public string SrcMemberName
        {
            get;
            private set;
        }
        public string DestMemberName
        {
            get;
            private set;
        }

        public ConnectErrorException(Exception e, string srcName, string destName, string srcMemberName, string destMemberName) :
            base("Error in connection", e)
        {
            SrcName = srcName;
            DestName = destName;
            SrcMemberName = srcMemberName;
            DestMemberName = destMemberName;
        }
    }
}
