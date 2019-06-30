using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeEvaluateNetworkData : NetworkData
    {
        [Key(2)]
        public MarkEvaluateTypeEx Evaluate
        {
            get;
            set;
        }

        public ChangeEvaluateNetworkData()
        {
            MethodType = MethodType.ChangeEvaluate;
        }
    }
}
