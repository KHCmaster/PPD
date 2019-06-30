using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_FirstWithPredicate_Summary")]
    public partial class FirstWithPredicateFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_FirstWithPredicate_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.First"; }
        }

        [ToolTipText("Enumerable_First_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_First_First")]
        public object First
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.First(obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_FirstWithPredicate_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_FirstWithPredicate_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
