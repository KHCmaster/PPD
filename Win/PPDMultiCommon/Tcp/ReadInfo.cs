using PPDMultiCommon.Model;

namespace PPDMultiCommon.Tcp
{
    public class ReadInfo(NetworkData networkData)
    {
        public NetworkData NetworkData
        {
            get;
            private set;
        } = networkData;
    }
}
