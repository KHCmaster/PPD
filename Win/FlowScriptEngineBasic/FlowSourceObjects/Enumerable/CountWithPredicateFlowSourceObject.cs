using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_CountWithPredicate_Summary")]
    public partial class CountWithPredicateFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_CountWithPredicate_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.Count"; }
        }

        [ToolTipText("Enumerable_Count_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Count_Count")]
        public int Count
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return 0;
                }
                return Enumerable.Count(obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_CountWithPredicate_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_CountWithPredicate_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
