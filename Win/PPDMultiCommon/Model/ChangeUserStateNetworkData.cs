using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class ChangeUserStateNetworkData : NetworkData
    {
        [Key(2)]
        public UserState State
        {
            get;
            set;
        }

        public ChangeUserStateNetworkData()
        {
            MethodType = MethodType.ChangeUserState;
        }
    }
}
