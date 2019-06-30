using FlowScriptEngine;
using System;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("TemplateIsType_Summary")]
    public abstract class TemplateIsTypeFlowSourceObject<T> : FlowSourceObjectBase
    {
        [ToolTipText("TemplateIsType_Target")]
        public object Target
        {
            private get;
            set;
        }

        [ToolTipText("TemplateIsType_Value")]
        public bool Value
        {
            get
            {
                SetValue(nameof(Target));
                if (Target == null)
                {
                    return false;
                }
                Type type = typeof(T);
                var targetType = Target.GetType();
                return targetType.IsSubclassOf(type) || targetType == type;
            }
        }
    }
}
