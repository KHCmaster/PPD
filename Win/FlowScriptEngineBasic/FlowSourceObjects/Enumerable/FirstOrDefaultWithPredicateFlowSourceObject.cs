using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_FirstOrDefaultWithPredicate_Summary")]
    public partial class FirstOrDefaultWithPredicateFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_FirstOrDefaultWithPredicate_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.FirstOrDefault"; }
        }

        [ToolTipText("Enumerable_FirstOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_FirstOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_FirstOrDefault_First")]
        public object First
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
                    return Enumerable.First(obj =>
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

        [ToolTipText("Enumerable_FirstOrDefaultWithPredicate_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_FirstOrDefaultWithPredicate_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
