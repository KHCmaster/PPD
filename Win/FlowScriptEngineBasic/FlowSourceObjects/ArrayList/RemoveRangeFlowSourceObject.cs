using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_RemoveRange_Summary")]
    public partial class RemoveRangeFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.RemoveRange"; }
        }

        [ToolTipText("ArrayList_RemoveRange_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_RemoveRange_Count")]
        public int Count
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Index));
                SetValue(nameof(Count));
                ArrayList.RemoveRange(Index, Count);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
