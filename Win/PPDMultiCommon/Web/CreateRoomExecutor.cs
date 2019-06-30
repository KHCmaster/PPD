namespace PPDMultiCommon.Web
{
    public class CreateRoomExecutor : HostExecutorBase
    {
        private RoomInfo roomInfo;

        public CreateRoomExecutor(WebManager webManager, RoomInfo roomInfo)
            : base(webManager)
        {
            this.roomInfo = roomInfo;
        }

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
