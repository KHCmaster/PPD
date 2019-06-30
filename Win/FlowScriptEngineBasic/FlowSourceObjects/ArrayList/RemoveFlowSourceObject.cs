using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Remove_Summary")]
    public partial class RemoveFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Remove"; }
        }

        [ToolTipText("ArrayList_Remove_Value")]
        public object Value
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Value));
                ArrayList.Remove(Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
