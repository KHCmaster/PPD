using MessagePack;
using PPDFrameworkCore;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeSongNetworkData : NetworkData
    {
        [Key(2)]
        public byte[] Hash
        {
            get;
            set;
        }

        [Key(3)]
        public Difficulty Difficulty
        {
            get;
            set;
        }

        public ChangeSongNetworkData()
        {
            MethodType = MethodType.ChangeSong;
        }
    }
}
