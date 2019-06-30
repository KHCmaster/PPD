using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class SendScoreListNetworkData : NetworkData
    {
        [Key(2)]
        public SongInfo[] SongInfos
        {
            get;
            set;
        }

        public SendScoreListNetworkData()
        {
            MethodType = MethodType.SendScoreList;
        }
    }
}
