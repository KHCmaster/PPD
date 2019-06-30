namespace FlowScriptEngineBasic.FlowSourceObjects.TimeSpan
{
    [ToolTipText("TimeSpan_Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "TimeSpan.Parse"; }
        }

        [ToolTipText("Parse_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("Parse_Value")]
        public System.TimeSpan Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(A));
            try
            {
                Value = System.TimeSpan.Parse(A);
                OnSuccess();
            }
            catch
            {
                OnFailed();
            }
        }
    }
}
