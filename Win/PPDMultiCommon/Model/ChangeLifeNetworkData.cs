using MessagePack;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeLifeNetworkData : NetworkData
    {
        [Key(2)]
        public int Life
        {
            get;
            set;
        }

        public ChangeLifeNetworkData()
        {
            MethodType = Data.MethodType.ChangeLife;
        }
    }
}
