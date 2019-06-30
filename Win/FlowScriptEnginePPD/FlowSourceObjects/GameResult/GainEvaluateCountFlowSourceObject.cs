using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    [ToolTipText("GameResult_GainEvaluateCount_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class GainEvaluateCountFlowSourceObject : GameResultFlowSourceBase
    {
        public override string Name
        {
            get { return "PPD.GameResult.GainEvaluateCount"; }
        }

        [ToolTipText("GameResult_GainEvaluateCount_Gain")]
        public int Gain
        {
            private get;
            set;
        }

        [ToolTipText("GameResult_GainEvaluateCount_Evaluate")]
        public PPDFramework.MarkEvaluateType Evaluate
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Gain));
            SetValue(nameof(Evaluate));
            if (gameResultManager != null)
            {
                gameResultManager.GainEvaluate(Evaluate, Gain);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
