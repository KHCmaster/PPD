using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class PingNetworkData : NetworkData
    {
        [Key(2)]
        public long Time
        {
            get;
            set;
        }

        public PingNetworkData()
        {
            MethodType = MethodType.Ping;
        }
    }
}
