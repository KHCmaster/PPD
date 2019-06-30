using PPDFramework.Logger;

namespace FlowScriptEnginePPD.FlowSourceObjects.Debug
{
    [ToolTipText("PPD_Debug_Trace")]
    public partial class TraceFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "PPD.Debug.Trace"; }
        }

        [ToolTipText("PPD_Debug_Trace_Text")]
        public string Text
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(Text));
            LogManager.Instance.AddLog(new LogInfo(LogLevel.Trace, Text));
            OnSuccess();
        }
    }
}
