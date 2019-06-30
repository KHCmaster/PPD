using FlowScriptEngine;
using System.Globalization;

namespace FlowScriptEngineBasic.FlowSourceObjects.Int32
{
    [ToolTipText("Parse_Summary")]
    public partial class ParseFlowSourceObject : ExecutableFlowSourceObject
    {
        public override string Name
        {
            get { return "Int32.Parse"; }
        }

        [ToolTipText("Parse_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("Parse_Value")]
        public int Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(A));
            if (int.TryParse(A, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
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
