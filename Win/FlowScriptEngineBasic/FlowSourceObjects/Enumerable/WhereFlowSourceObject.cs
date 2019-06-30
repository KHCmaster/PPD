using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Where_Summary")]
    public partial class WhereFlowSourceObject : FlowSourceObjectBase
    {
        private IEnumerable<object> enumerable;

        [ToolTipText("Enumerable_Where_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.Where"; }
        }

        [ToolTipText("Enumerable_Where_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (enumerable == null)
                {
                    return null;
                }
                return enumerable.Where(obj =>
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

        [ToolTipText("Enumerable_Where_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Where_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
