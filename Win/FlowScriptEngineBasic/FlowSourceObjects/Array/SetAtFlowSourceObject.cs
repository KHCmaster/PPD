namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_SetAt_Summary")]
    public partial class SetAtFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.SetAt"; }
        }

        [ToolTipText("Array_SetAt_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Array_SetAt_Value")]
        public object Value
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArray();
            if (Array != null)
            {
                SetValue(nameof(Index));
                SetValue(nameof(Value));
                Array[Index] = Value;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
