using MessagePack;

namespace PPDMultiCommon.Model
{
    [MessagePackObject]
    public class DeleteUserNetworkData : NetworkData
    {
        public DeleteUserNetworkData()
        {
            MethodType = Data.MethodType.DeleteUser;
        }
    }
}
