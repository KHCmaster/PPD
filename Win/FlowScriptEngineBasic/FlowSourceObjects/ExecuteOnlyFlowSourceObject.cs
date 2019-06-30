using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects
{
    public abstract class ExecuteOnlyFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Execute_In")]
        public virtual void In(FlowEventArgs e)
        {
        }
    }
}