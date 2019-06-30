using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Sort_Summary")]
    public partial class SortWithLimitFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        [ToolTipText("ArrayList_Sort_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "ArrayList.Sort"; }
        }

        [ToolTipText("ArrayList_Sort_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_Sort_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_Sort_X")]
        public object X
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_Sort_Y")]
        public object Y
        {
            get;
            private set;
        }

        [ToolTipText("ArrayList_Sort_Result")]
        public int Result
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(StartIndex));
                SetValue(nameof(Count));
                var comparer = new CallbackComparer((x, y) =>
                {
                    X = x;
                    Y = y;
                    FireEvent(Compare, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
                ArrayList.Sort(StartIndex, Count, comparer);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
