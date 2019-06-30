using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class SendResultNetworkData : NetworkData
    {
        [Key(2)]
        public Result Result
        {
            get;
            set;
        }

        public SendResultNetworkData()
        {
            MethodType = MethodType.SendResult;
        }
    }
}
