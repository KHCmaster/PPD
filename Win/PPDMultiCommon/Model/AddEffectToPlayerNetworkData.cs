using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class AddEffectToPlayerNetworkData : AddEffectNetworkData
    {
        [Key(3)]
        public int UserId
        {
            get;
            set;
        }

        public AddEffectToPlayerNetworkData()
        {
            MethodType = MethodType.AddEffectToPlayer;
        }
    }
}
