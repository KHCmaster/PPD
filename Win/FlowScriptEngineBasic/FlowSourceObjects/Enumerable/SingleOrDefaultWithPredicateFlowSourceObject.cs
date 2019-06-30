using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_SingleOrDefaultWithPredicate_Summary")]
    public partial class SingleOrDefaultWithPredicateFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_SingleOrDefaultWithPredicate_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.SingleOrDefault"; }
        }

        [ToolTipText("Enumerable_SingleOrDefault_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_SingleOrDefault_Default")]
        public object Default
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_SingleOrDefault_Single")]
        public object Single
        {
            get
            {
                SetValue(nameof(Enumerable));
                SetValue(nameof(Default));
                if (Enumerable == null)
                {
                    return null;
                }
                try
                {
                    return Enumerable.SingleOrDefault(obj =>
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

        [ToolTipText("Enumerable_SingleOrDefaultWithPredicate_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_SingleOrDefaultWithPredicate_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
