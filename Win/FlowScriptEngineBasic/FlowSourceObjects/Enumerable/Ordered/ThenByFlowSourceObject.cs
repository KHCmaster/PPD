using FlowScriptEngine;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable.Ordered
{
    [ToolTipText("Enumerable_Ordered_ThenBy_Summary")]
    public partial class ThenByFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_Ordered_ThenBy_Select")]
        public event FlowEventHandler Select;
        [ToolTipText("Enumerable_Ordered_ThenBy_Compare")]
        public event FlowEventHandler Compare;

        private IOrderedEnumerable<object> orderedEnumerable;

        public override string Name
        {
            get { return "Enumerable.Ordered.ThenBy"; }
        }

        [ToolTipText("Enumerable_Ordered_ThenBy_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Ordered_ThenBy_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Ordered_ThenBy_CompareResult")]
        public int CompareResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Ordered_ThenBy_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_Ordered_ThenBy_SelectResult")]
        public object SelectResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_Ordered_ThenBy_OrderedEnumerable")]
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
                return orderedEnumerable.ThenBy(obj =>
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
