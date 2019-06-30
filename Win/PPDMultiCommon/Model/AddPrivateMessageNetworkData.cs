using MessagePack;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class AddPrivateMessageNetworkData : NetworkData
    {
        [Key(2)]
        public string Message
        {
            get;
            set;
        }

        [Key(3)]
        public int UserId
        {
            get;
            set;
        }

        public AddPrivateMessageNetworkData()
        {
            MethodType = Data.MethodType.AddPrivateMessage;
        }
    }
}
