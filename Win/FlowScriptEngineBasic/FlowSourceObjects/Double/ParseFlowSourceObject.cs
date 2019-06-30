using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.FlowSourceObjects.Double
{
    [ToolTipText("Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Double.Parse"; }
        }

        [ToolTipText("Parse_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("Parse_Value")]
        public double Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(A));
            if (double.TryParse(A, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
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
