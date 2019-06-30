using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Any_Summary")]
    public partial class AnyFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Any_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.Any"; }
        }

        [ToolTipText("Enumerable_Any_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Any_Any")]
        public bool Any
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return false;
                }
                return Enumerable.Any(obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Any_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Any_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
