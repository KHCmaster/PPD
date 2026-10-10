namespace PPDMultiCommon.Web
{
    public class UpdateRoomInfoExecutor(WebManager webManager, int playerCount, string currentScoreHash) : HostExecutorBase(webManager)
    {
        private readonly int playerCount = playerCount;
        private readonly string currentScoreHash = currentScoreHash;

        protected override void InnerStart()
        {
            var status = webManager.UpdateRoomInfo(playerCount, currentScoreHash);
            if (status == System.Net.HttpStatusCode.BadRequest)
            {
                OnFail();
            }
            else
            {
                OnSuccess();
            }
        }
    }
}
