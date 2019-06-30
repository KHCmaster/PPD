using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ForceStartNetworkData : NetworkData
    {
        public ForceStartNetworkData()
        {
            MethodType = MethodType.ForceStart;
        }
    }
}
