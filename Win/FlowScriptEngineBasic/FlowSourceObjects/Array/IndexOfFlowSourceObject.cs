namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_IndexOf_Summary")]
    public partial class IndexOfFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.IndexOf"; }
        }

        [ToolTipText("Array_IndexOf_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Array_IndexOf_Index")]
        public int Index
        {
            get;
            private set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                SetValue(nameof(Value));
                Index = System.Array.IndexOf(Array, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
