using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    [ToolTipText("GameResult_GainCurrentLife_Summary")]
    [ModifyData]
    [ModifyDataWarning]
    public partial class GainCurrentLifeFlowSourceObject : GameResultFlowSourceBase
    {
        public override string Name
        {
            get { return "PPD.GameResult.GainCurrentLife"; }
        }

        [ToolTipText("GameResult_GainCurrentLife_Gain")]
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
                gameResultManager.CurrentLife += Gain;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
