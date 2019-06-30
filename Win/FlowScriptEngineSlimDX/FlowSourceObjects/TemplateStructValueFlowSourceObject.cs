using FlowScriptEngine;

namespace FlowScriptEngineSlimDX
{
    [ToolTipText("TemplateStruct_Summary")]
    public abstract class TemplateStructValueFlowSourceObject<T> : FlowSourceObjectBase where T : struct
    {
        private T value;
        [ToolTipText("TemplateStruct_Value")]
        public T Value
        {
            get
            {
                SetValue(nameof(Value));
                return value;
            }
            set { this.value = value; }
        }
    }
}
