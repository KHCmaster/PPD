using MessagePack;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class AddMessageNetworkData : NetworkData
    {
        [Key(2)]
        public string Message
        {
            get;
            set;
        }

        public AddMessageNetworkData()
        {
            MethodType = Data.MethodType.AddMessage;
        }
    }
}
