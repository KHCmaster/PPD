using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_OrderByDescending_Summary")]
    public partial class OrderByDescendingFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_OrderByDescending_Select")]
        public event FlowEventHandler Select;
        [ToolTipText("Enumerable_OrderByDescending_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "Enumerable.OrderByDescending"; }
        }

        [ToolTipText("Enumerable_OrderByDescending_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_OrderByDescending_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_OrderByDescending_CompareResult")]
        public int CompareResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_OrderByDescending_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_OrderByDescending_SelectResult")]
        public object SelectResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_OrderByDescending_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_OrderByDescending_OrderedEnumerable")]
        public IOrderedEnumerable<object> OrderedEnumerable
        {
            get
            {
                SetValue(nameof(Enumerable));
                if (Enumerable == null)
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
                return Enumerable.OrderByDescending(obj =>
                {
                    Value = obj;
                    FireEvent(Select, true);
                    ProcessChildEvent();
                    SetValue(nameof(SelectResult));
                    return SelectResult;
                }, comparer);
            }
        }
    }
}
