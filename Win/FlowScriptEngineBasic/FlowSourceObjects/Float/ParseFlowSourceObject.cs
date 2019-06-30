using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.FlowSourceObjects.Float
{
    [ToolTipText("Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Float.Parse"; }
        }

        [ToolTipText("Parse_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("Parse_Value")]
        public float Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(A));
            if (float.TryParse(A, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                Value = result;
                OnSuccess();
            }
            else
            {
                Value = 0;
                OnFailed();
            }
        }
    }
}
