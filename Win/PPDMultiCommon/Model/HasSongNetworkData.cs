using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class HasSongNetworkData : NetworkData
    {
        public HasSongNetworkData()
        {
            MethodType = MethodType.HasSong;
        }
    }
}
