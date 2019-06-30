using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Reverse_Summary")]
    public partial class ReverseWithLimitFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Reverse"; }
        }

        [ToolTipText("ArrayList_Reverse_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_Reverse_Count")]
        public int Count
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            SetValue(nameof(StartIndex));
            SetValue(nameof(Count));
            if (ArrayList != null)
            {
                ArrayList.Reverse(StartIndex, Count);
            }
        }
    }
}
