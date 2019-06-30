using MessagePack;
using PPDMultiCommon.Data;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class AddUserNetworkData : NetworkData
    {
        [Key(2)]
        public string Ip
        {
            get;
            set;
        }

        [Key(3)]
        public string Version
        {
            get;
            set;
        }

        [Key(4)]
        public string UserName
        {
            get;
            set;
        }

        [Key(5)]
        public string AccountId
        {
            get;
            set;
        }

        [Key(6)]
        public UserState State
        {
            get;
            set;
        }

        public AddUserNetworkData()
        {
            MethodType = MethodType.AddUser;
        }
    }
}
