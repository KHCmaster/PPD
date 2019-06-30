using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class GoToPlayNetworkData : NetworkData
    {
        [Key(2)]
        public int[] PlayerIds
        {
            get;
            set;
        }

        public GoToPlayNetworkData()
        {
            MethodType = MethodType.GoToPlay;
        }
    }
}
