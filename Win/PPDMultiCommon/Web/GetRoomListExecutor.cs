namespace PPDMultiCommon.Web
{
    public class GetRoomListExecutor : ExecutorBase
    {

        public RoomInfo[] RoomList
        {
            get;
            private set;
        }

        protected override void InnerStart()
        {
            base.InnerStart();
            RoomList = WebManager.GetRoomList();
            OnSuccess();
        }
    }
}
