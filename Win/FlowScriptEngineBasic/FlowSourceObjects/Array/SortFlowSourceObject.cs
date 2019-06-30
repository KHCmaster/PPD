using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Sort_Summary")]
    public partial class SortFlowSourceObject : ArrayFlowSourceObjectBase
    {
        [ToolTipText("Array_Sort_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "Array.Sort"; }
        }

        [ToolTipText("Array_Sort_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("Array_Sort_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("Array_Sort_Result")]
        public int Result
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                var comparer = new CallbackComparer((x, y) =>
                {
                    X = x;
                    Y = y;
                    FireEvent(Compare, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
                System.Array.Sort(Array, comparer);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
