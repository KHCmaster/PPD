namespace FlowScriptEngineBasic.FlowSourceObjects.Stopwatch
{
    [ToolTipText("Stopwatch_Value_Summary")]
    public partial class ValueFlowSourceObject : TemplateClassValueFlowSourceObject<System.Diagnostics.Stopwatch>
    {
        public override string Name
        {
            get { return "Stopwatch.Value"; }
        }
    }
}
