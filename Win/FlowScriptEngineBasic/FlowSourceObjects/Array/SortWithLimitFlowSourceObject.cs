using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Sort_Summary")]
    public partial class SortWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
    {
        [ToolTipText("Array_Sort_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "Array.Sort"; }
        }

        [ToolTipText("Array_Sort_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("Array_Sort_Length")]
        public int Length
        {
            private get;
            set;
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
                SetValue(nameof(StartIndex));
                SetValue(nameof(Length));
                var comparer = new CallbackComparer((x, y) =>
                {
                    X = x;
                    Y = y;
                    FireEvent(Compare, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
                System.Array.Sort(Array, StartIndex, Length, comparer);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
