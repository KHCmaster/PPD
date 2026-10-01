namespace PPDMultiCommon.Web
{
    public class CreateRoomExecutor(WebManager webManager, RoomInfo roomInfo) : HostExecutorBase(webManager)
    {
        private readonly RoomInfo roomInfo = roomInfo;

        protected override void InnerStart()
        {
            if (webManager.CreateRoom(roomInfo))
            {
                OnSuccess();
            }
            else
            {
                OnFail();
            }
        }
    }
}
