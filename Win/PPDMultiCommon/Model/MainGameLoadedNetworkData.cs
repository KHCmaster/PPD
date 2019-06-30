using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class MainGameLoadedNetworkData : NetworkData
    {
        public MainGameLoadedNetworkData()
        {
            MethodType = MethodType.MainGameLoaded;
        }
    }
}
