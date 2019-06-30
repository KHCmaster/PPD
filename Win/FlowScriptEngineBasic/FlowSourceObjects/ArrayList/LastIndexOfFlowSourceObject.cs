using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_LastIndexOf_Summary", "ArrayList_LastIndexOf_Remark")]
    public partial class LastIndexOfFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.LastIndexOf"; }
        }

        [ToolTipText("ArrayList_LastIndexOf_Object")]
        public object Object
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_LastIndexOf_Value")]
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
                Value = ArrayList.LastIndexOf(Object);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
