using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class GoToPlayPrepareNetworkData : NetworkData
    {
        public GoToPlayPrepareNetworkData()
        {
            MethodType = MethodType.GoToPlayPrepare;
        }
    }
}
