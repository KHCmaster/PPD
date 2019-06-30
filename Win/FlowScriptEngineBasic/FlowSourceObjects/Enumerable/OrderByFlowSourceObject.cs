using FlowScriptEngine;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptEngineBasic.FlowSourceObjects.Enumerable
{
    [ToolTipText("Enumerable_OrderBy_Summary")]
    public partial class OrderByFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Enumerable_OrderBy_Select")]
        public event FlowEventHandler Select;
        [ToolTipText("Enumerable_OrderBy_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "Enumerable.OrderBy"; }
        }

        [ToolTipText("Enumerable_OrderBy_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_OrderBy_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_OrderBy_CompareResult")]
        public int CompareResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_OrderBy_Value")]
        public object Value
        {
            get;
            private set;
        }

        [ToolTipText("Enumerable_OrderBy_SelectResult")]
        public object SelectResult
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_OrderBy_Enumerable")]
        public IEnumerable<object> Enumerable
        {
            private get;
            set;
        }

        [ToolTipText("Enumerable_OrderBy_OrderedEnumerable")]
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
                return Enumerable.OrderBy(obj =>
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
