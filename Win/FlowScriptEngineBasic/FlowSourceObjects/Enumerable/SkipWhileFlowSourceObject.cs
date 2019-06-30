using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_SkipWhile_Summary")]
    public partial class SkipWhileFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_SkipWhile_Predicate")]
        public event FlowEventHandler Predicate;
        private IEnumerable<object> enumerable;

        public override string Name
        {
            get { return "Enumerable.Skip"; }
        }

        [ToolTipText("Enumerable_SkipWhile_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return System.Linq.Enumerable.Empty<object>();
                }
                return enumerable.SkipWhile(obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
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

        [ToolTipText("Enumerable_SkipWhile_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_SkipWhile_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
