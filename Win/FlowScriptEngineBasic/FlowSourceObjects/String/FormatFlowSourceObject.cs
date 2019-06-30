using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_Format_Summary")]
    public partial class FormatFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.Format"; }
        }

        [ToolTipText("String_Format_A")]
        public string A
        {
            private get;
            set;
        }

        [ToolTipText("String_Format_Args")]
        public IEnumerable<object> Args
        {
            private get;
            set;
        }

        [ToolTipText("String_Format_Value")]
        public string Value
        {
            get
            {
                SetValue(nameof(A));
                SetValue(nameof(Args));
                if (A != null && Args != null)
                {
                    return System.String.Format(A, Args.ToArray());
                }
                return "";
            }
        }
    }
}
