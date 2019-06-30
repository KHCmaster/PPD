using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class AddEffectNetworkData : NetworkData
    {
        [Key(2)]
        public ItemType ItemType
        {
            get;
            set;
        }

        public AddEffectNetworkData()
        {
            MethodType = MethodType.AddEffect;
        }
    }
}
