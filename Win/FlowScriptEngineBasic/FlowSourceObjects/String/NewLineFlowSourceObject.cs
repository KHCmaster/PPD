using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.FlowSourceObjects.String
{
    [ToolTipText("String_NewLine_Summary")]
    public partial class NewLineFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "String.NewLine"; }
        }

        [ToolTipText("String_NewLine_Value")]
        public string Value
        {
            get
            {
                return Environment.NewLine;
            }
        }
    }
}
