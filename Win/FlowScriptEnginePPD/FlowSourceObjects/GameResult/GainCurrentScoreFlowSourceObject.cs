using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    [ToolTipText("GameResult_GainCurrentScore_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class GainCurrentScoreFlowSourceObject : GameResultFlowSourceBase
    {
        public override string Name
        {
            get { return "PPD.GameResult.GainCurrentScore"; }
        }

        [ToolTipText("GameResult_GainCurrentScore_Gain")]
        public int Gain
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Gain));
            if (gameResultManager != null)
            {
                gameResultManager.GainScore(Gain);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
