using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Select_Summary")]
    public partial class SelectFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        [ToolTipText("Enumerable_Select_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.Select"; }
        }

        [ToolTipText("Enumerable_Select_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return null;
                }
                return enumerable.Select(obj =>
                {
                    Value = obj;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
            set
            {
                enumerable = value;
            }
        }

        [ToolTipText("Enumerable_Select_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Select_Result")]
        public object Result
        {
            private get;
            set;
        }
    }
}
