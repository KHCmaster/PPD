using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class SendServerInfoNetworkData : NetworkData
    {
        [Key(2)]
        public string[] AllowedModIds
        {
            get;
            set;
        }

        public SendServerInfoNetworkData()
        {
            MethodType = MethodType.SendServerInfo;
        }
    }
}
