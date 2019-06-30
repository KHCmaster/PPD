using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_RemoveAt_Summary")]
    public partial class RemoveAtFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.RemoveAt"; }
        }

        [ToolTipText("ArrayList_RemoveAt_Value")]
        public int Value
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
                ArrayList.RemoveAt(Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
