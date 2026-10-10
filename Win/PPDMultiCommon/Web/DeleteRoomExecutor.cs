namespace PPDMultiCommon.Web
{
    public class DeleteRoomExecutor(WebManager webManager) : HostExecutorBase(webManager)
    {
        protected override void InnerStart()
        {
            webManager.DeleteRoom();
            OnSuccess();
        }
    }
}
