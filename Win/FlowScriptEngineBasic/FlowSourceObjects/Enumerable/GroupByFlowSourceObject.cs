using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_GroupBy_Summary")]
    public partial class GroupByFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_GroupBy_Select")]
        public event FlowEventHandler Select;

        public override string Name
        {
            get { return "Enumerable.GroupBy"; }
        }

        [ToolTipText("Enumerable_GroupBy_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_GroupBy_Grouping")]
        public IEnumerable<object> Grouping
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.GroupBy(obj =>
                {
                    Value = obj;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_GroupBy_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_GroupBy_Result")]
        public object Result
        {
            private get;
            set;
        }
    }
}
