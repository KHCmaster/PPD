using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Boolean
{
    [ToolTipText("Boolean_Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Boolean.Parse"; }
        }

        [ToolTipText("Boolean_Parse_Value")]
        public string Value
        {
            private get;
            set;
        }

        [ToolTipText("Boolean_Parse_Result")]
        public bool Result
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Value));
            if (bool.TryParse(Value, out bool result))
            {
                Result = result;
                OnSuccess();
            }
            else
            {
                Result = false;
                OnFailed();
            }
        }
    }
}
