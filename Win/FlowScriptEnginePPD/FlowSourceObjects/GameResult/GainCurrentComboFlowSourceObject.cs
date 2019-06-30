using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    [ToolTipText("GameResult_GainCurrentCombo_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class GainCurrentComboFlowSourceObject : GameResultFlowSourceBase
    {
        public override string Name
        {
            get { return "PPD.GameResult.GainCurrentCombo"; }
        }

        [ToolTipText("GameResult_GainCurrentCombo_Gain")]
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
                gameResultManager.CurrentCombo += Gain;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
