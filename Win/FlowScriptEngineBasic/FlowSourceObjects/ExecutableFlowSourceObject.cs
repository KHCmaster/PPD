using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects
{
    public abstract class ExecutableFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Execute_Success")]
        public event FlowEventHandler Success;

        [ToolTipText("Execute_Failed")]
        public event FlowEventHandler Failed;

        [ToolTipText("Execute_In")]
        public virtual void In(FlowEventArgs e)
        {
            OnSuccess();
        }

        protected void OnSuccess()
        {
            FireEvent(Success);
        }

        protected void OnFailed()
        {
            FireEvent(Failed);
        }
    }
}
