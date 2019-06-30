namespace PPDMultiCommon.Web
{
    public class DeleteRoomExecutor : HostExecutorBase
    {
        public DeleteRoomExecutor(WebManager webManager)
            : base(webManager)
        {
        }

        protected override void InnerStart()
        {
            webManager.DeleteRoom();
            OnSuccess();
        }
    }
}
