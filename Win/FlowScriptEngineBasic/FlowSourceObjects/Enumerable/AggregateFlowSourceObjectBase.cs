using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Aggregate_Summary")]
    public partial class AggregateFlowSourceObjectBase : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Aggregate_Accumurate")]
        public event FlowEventHandler Accumurate;

        public override string Name
        {
            get { return "Enumerable.Aggregate"; }
        }

        [ToolTipText("Enumerable_Aggregate_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Aggregate_Aggregate")]
        public object Aggregate
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
                {
                    return null;
                }
                return Enumerable.Aggregate((obj1, obj2) =>
                {
                    Obj1 = obj1;
                    Obj2 = obj2;
                    FireEvent(Accumurate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
            }
        }

        [ToolTipText("Enumerable_Aggregate_Obj1")]
        public object Obj1
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Aggregate_Obj2")]
        public object Obj2
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Aggregate_Result")]
        public object Result
        {
            private get;
            set;
        }
    }
}
