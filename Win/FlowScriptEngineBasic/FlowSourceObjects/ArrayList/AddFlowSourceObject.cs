using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Add_Summary")]
    public partial class AddFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Add"; }
        }

        [ToolTipText("ArrayList_Add_Value")]
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
                ArrayList.Add(Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
