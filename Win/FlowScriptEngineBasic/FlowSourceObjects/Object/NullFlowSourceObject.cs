using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Object
{
    [ToolTipText("Object_Null_Summary", "Object_Null_Remark")]
    public partial class NullFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Object.Null"; }
        }

        [ToolTipText("Object_Null_Value")]
        public object Value
        {
            get
            {
                return null;
            }
        }
    }
}
