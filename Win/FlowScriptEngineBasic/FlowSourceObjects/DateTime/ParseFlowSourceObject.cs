namespace FlowScriptEngineBasic.FlowSourceObjects.DateTime
{
    [ToolTipText("DateTime_Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "DateTime.Parse"; }
        }

        [ToolTipText("Parse_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("Parse_Value")]
        public System.DateTime Value
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetValue(nameof(A));
            try
            {
                Value = System.DateTime.Parse(A);
                OnSuccess();
            }
            catch
            {
                OnFailed();
            }
        }
    }
}
