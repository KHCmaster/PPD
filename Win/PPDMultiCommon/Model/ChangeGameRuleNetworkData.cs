using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeGameRuleNetworkData : NetworkData
    {
        [Key(2)]
        public GameRule GameRule
        {
            get;
            set;
        }

        public ChangeGameRuleNetworkData()
        {
            MethodType = MethodType.ChangeGameRule;
        }
    }
}
