using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class CloseConnectNetworkData : NetworkData
    {
        [Key(2)]
        public CloseConnectReason Reason
        {
            get;
            set;
        }

        public CloseConnectNetworkData()
        {
            MethodType = MethodType.CloseConnect;
        }
    }
}
