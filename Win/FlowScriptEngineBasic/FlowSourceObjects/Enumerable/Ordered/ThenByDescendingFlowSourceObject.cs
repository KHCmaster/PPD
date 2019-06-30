using FlowScriptEngine;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Ordered
{
    [ToolTipText("Enumerable_Ordered_ThenByDescending_Summary")]
    public partial class ThenByDescendingFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Ordered_ThenByDescending_Select")]
        public event FlowEventHandler Select;
        [ToolTipText("Enumerable_Ordered_ThenByDescending_Compare")]
        public event FlowEventHandler Compare;

        private IOrderedEnumerable<object> orderedEnumerable;

        public override string Name
        {
            get { return "Enumerable.Ordered.ThenByDescending"; }
        }

        [ToolTipText("Enumerable_Ordered_ThenByDescending_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Ordered_ThenByDescending_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Ordered_ThenByDescending_CompareResult")]
        public int CompareResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Ordered_ThenByDescending_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Ordered_ThenByDescending_SelectResult")]
        public object SelectResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Ordered_ThenByDescending_OrderedEnumerable")]
        public IOrderedEnumerable<object> OrderedEnumerable
        {
            get
            {
                SetValue(nameof(OrderedEnumerable));
                if (orderedEnumerable == null)
                {
                    return null;
                }
                var comparer = new CallbackComparer((x, y) =>
                {
                    X = x;
                    Y = y;
                    FireEvent(Compare, true);
                    ProcessChildEvent();
                    SetValue(nameof(CompareResult));
                    return CompareResult;
                });
                return orderedEnumerable.ThenByDescending(obj =>
                {
                    Value = obj;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(SelectResult));
                    return SelectResult;
                }, comparer);
            }
            set
            {
                orderedEnumerable = value;
            }
        }
    }
}
