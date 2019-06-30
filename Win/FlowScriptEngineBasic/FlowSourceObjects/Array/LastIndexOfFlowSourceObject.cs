namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_LastIndexOf_Summary")]
    public partial class LastIndexOfFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.LastIndexOf"; }
        }

        [ToolTipText("Array_LastIndexOf_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("Array_LastIndexOf_Index")]
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
                Index = System.Array.LastIndexOf(Array, Value);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
