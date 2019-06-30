using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class PlayMainGameNetworkData : NetworkData
    {
        public PlayMainGameNetworkData()
        {
            MethodType = MethodType.PlayMainGame;
        }
    }
}
