using FlowScriptEngine;
using System;

namespace FlowScriptEngineBasic.FlowSourceObjects
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
                // 元は IsSubclassOf と == で比べていて、型がインターフェースのときはいつも false だった
                return type.IsAssignableFrom(targetType);
            }
        }
    }
}
