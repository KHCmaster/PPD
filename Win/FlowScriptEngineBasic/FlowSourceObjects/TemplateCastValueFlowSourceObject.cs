using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects
{
    [ToolTipText("TemplateCast_Summary")]
    public abstract class TemplateCastValueFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("TemplateCast_CastValue")]
        public object CastValue
        {
            protected get;
            set;
        }
    }
}
