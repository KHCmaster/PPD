using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_LastOrDefaultWithPredicate_Summary")]
    public partial class LastOrDefaultWithPredicateFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_LastOrDefaultWithPredicate_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.LastOrDefault"; }
        }

        [ToolTipText("Enumerable_LastOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_LastOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_LastOrDefault_Last")]
        public object Last
        {
            get
            {
                SetValue(nameof(Enumerable));
                SetValue(nameof(Default));
                if (Enumerable == null)
                {
                    return Default;
                }
                try
                {
                    return Enumerable.Last(obj =>
                    {
                        Value = obj;
                        FireEvent(Predicate, true);
                        ProcessChildEvent();
                        SetValue(nameof(Result));
                        return Result;
                    });
                }
                catch
                {
                    return Default;
                }
            }
        }

        [ToolTipText("Enumerable_LastOrDefaultWithPredicate_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_LastOrDefaultWithPredicate_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
