using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class JustGoToMenuNetworkData : NetworkData
    {
        public JustGoToMenuNetworkData()
        {
            MethodType = MethodType.JustGoToMenu;
        }
    }
}
