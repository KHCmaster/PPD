using MessagePack;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeScoreNetworkData : NetworkData
    {
        [Key(2)]
        public int Score
        {
            get;
            set;
        }

        public ChangeScoreNetworkData()
        {
            MethodType = Data.MethodType.ChangeScore;
        }
    }
}
