using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class KickUserNetworkData : NetworkData
    {
        [Key(2)]
        public int UserId
        {
            get;
            set;
        }

        public KickUserNetworkData()
        {
            MethodType = MethodType.KickUser;
        }
    }
}
