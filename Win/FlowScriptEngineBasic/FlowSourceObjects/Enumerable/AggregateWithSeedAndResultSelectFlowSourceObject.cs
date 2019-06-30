using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_Aggregate_Summary")]
    public partial class AggregateWithSeedAndResultSelectFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Aggregate_Accumurate")]
        public event FlowEventHandler Accumurate;
        [ToolTipText("Enumerable_Aggregate_ResultSelect")]
        public event FlowEventHandler ResultSelect;

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

        [ToolTipText("Enumerable_Aggregate_Seed")]
        public object Seed
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
                SetValue(nameof(Seed));
                return Enumerable.Aggregate(Seed, (obj1, obj2) =>
                {
                    Obj1 = obj1;
                    Obj2 = obj2;
                    FireEvent(Accumurate, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                },
                obj =>
                {
                    ResultSelectValue = obj;
                    FireEvent(ResultSelect, true);
                    ProcessChildEvent();
                    SetValue(nameof(ResultSelectResult));
                    return ResultSelectResult;
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

        [ToolTipText("Enumerable_Aggregate_ResultSelectValue")]
        public object ResultSelectValue
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Aggregate_ResultSelectResult")]
        public object ResultSelectResult
        {
            private get;
            set;
        }
    }
}
