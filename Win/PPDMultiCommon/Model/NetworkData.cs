using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class NetworkData
    {
        [Key(0)]
        public MethodType MethodType
        {
            get;
            set;
        }

        [Key(1)]
        public int Id
        {
            get;
            set;
        }
    }
}
