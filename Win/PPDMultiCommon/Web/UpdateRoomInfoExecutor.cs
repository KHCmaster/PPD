namespace PPDMultiCommon.Web
{
    public class UpdateRoomInfoExecutor : HostExecutorBase
    {
        private int playerCount;
        private string currentScoreHash;

        public UpdateRoomInfoExecutor(WebManager webManager, int playerCount, string currentScoreHash)
            : base(webManager)
        {
            this.playerCount = playerCount;
            this.currentScoreHash = currentScoreHash;
        }

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
