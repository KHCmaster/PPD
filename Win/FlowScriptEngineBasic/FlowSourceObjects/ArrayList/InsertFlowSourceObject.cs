using FlowScriptEngine;

namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_Insert_Summary")]
    public partial class InsertFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.Insert"; }
        }

        [ToolTipText("ArrayList_Insert_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_Insert_Index")]
        public int Index
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
                SetValue(nameof(Index));
                ArrayList.Insert(Index, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
