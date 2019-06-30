using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class PingUserNetworkData : NetworkData
    {
        [Key(2)]
        public int Ping
        {
            get;
            set;
        }

        public PingUserNetworkData()
        {
            MethodType = MethodType.PingUser;
        }
    }
}
