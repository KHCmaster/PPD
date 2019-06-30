using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_IndexOf_Summary", "ArrayList_IndexOf_Remark")]
    public partial class IndexOfWithLimitFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.IndexOf"; }
        }

        [ToolTipText("ArrayList_IndexOf_Object")]
        public object Object
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_IndexOf_StartIndex")]
        public int StartIndex
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_IndexOf_Count")]
        public int Count
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_IndexOf_Value")]
        public int Value
        {
            get;
            private set;
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Object));
                SetValue(nameof(StartIndex));
                SetValue(nameof(Count));
                Value = ArrayList.IndexOf(Object, StartIndex, Count);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
