using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_All_Summary")]
    public partial class AllFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_All_Predicate")]
        public event FlowEventHandler Predicate;

        public override string Name
        {
            get { return "Enumerable.All"; }
        }

        [ToolTipText("Enumerable_All_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_All_All")]
        public bool All
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return false;
                }
                return Enumerable.All(obj =>
                {
                    Value = obj;
                    FireEvent(Predicate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_All_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_All_Result")]
        public bool Result
        {
            private get;
            set;
        }
    }
}
