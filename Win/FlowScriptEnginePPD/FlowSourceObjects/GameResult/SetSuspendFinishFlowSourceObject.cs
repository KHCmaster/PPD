namespace FlowScriptEnginePPD.FlowSourceObjects.GameResult
{
    [ToolTipText("GameResult_SetSuspendFinish_Summary")]
    public partial class SetSuspendFinishFlowSourceObject : GameResultFlowSourceBase
    {
        public override string Name
        {
            get { return "PPD.GameResult.SetSuspendFinish"; }
        }

        [ToolTipText("GameResult_SetSuspendFinish_Value")]
        public bool Value
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Value));
            if (gameResultManager != null)
            {
                gameResultManager.SuspendFinish = Value;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
