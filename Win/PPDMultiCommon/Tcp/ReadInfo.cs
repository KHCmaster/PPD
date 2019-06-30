using PPDMultiCommon.Model;

namespace PPDMultiCommon.Tcp
{
    public class ReadInfo
    {
        public ReadInfo(NetworkData networkData)
        {
            NetworkData = networkData;
        }

        public NetworkData NetworkData
        {
            get;
            private set;
        }
    }
}
