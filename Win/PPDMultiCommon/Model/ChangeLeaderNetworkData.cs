using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeLeaderNetworkData : NetworkData
    {
        [Key(2)]
        public int UserId
        {
            get;
            set;
        }

        public ChangeLeaderNetworkData()
        {
            MethodType = MethodType.ChangeLeader;
        }
    }
}
