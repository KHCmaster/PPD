using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects
{
    [ToolTipText("TemplateClass_Summary")]
    public abstract class TemplateClassValueFlowSourceObject<T> : FlowSourceObjectBase where T : class, new()
    {
        [ToolTipText("TemplateClass_Value")]
        public T Value
        {
            get;
            private set;
        }

        [ToolTipText("TemplateClass_In")]
        public void In(FlowEventArgs e)
        {
            Value = new T();
        }

        protected override void OnReset()
        {
            base.OnReset();
            Value = null;
        }
    }
}
