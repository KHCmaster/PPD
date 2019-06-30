using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Sort_Summary")]
    public partial class SortFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        [ToolTipText("ArrayList_Sort_Compare")]
        public event FlowEventHandler Compare;

        public override string Name
        {
            get { return "ArrayList.Sort"; }
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
                var comparer = new CallbackComparer((x, y) =>
                {
                    X = x;
                    Y = y;
                    FireEvent(Compare, true);
                    ProcessChildEvent();
                    SetValue(nameof(Result));
                    return Result;
                });
                ArrayList.Sort(comparer);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
