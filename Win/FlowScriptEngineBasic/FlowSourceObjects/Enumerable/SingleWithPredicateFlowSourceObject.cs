using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_SingleWithPredicate_Summary")]
    public partial class SingleWithPredicateFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_SingleWithPredicate_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.Single"; }
        }

        [ToolTipText("Enumerable_Single_Summary")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Single_Single")]
        public object Single
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.Single(obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_SingleWithPredicate_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_SingleWithPredicate_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
