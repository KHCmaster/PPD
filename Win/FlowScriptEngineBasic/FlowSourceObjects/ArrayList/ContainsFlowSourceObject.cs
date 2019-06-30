using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Contains_Summary")]
    public partial class ContainsFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Contains"; }
        }

        [ToolTipText("ArrayList_Contains_Object")]
        public object Object
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_Contains_Value")]
        public bool Value
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
                Value = ArrayList.Contains(Object);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
