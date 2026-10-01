using PPDCoreModel;

namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    public abstract class GameResultFlowSourceBase : ExecutableFlowSourceObject
    {
        protected GameResultManager gameResultManager;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.TryGetValue("GameResultManager", out object value))
            {
                gameResultManager = value as GameResultManager;
            }
        }
    }
}
