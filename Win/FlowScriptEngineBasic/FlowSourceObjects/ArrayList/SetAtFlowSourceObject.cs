namespace FlowScriptEngineBasic.FlowSourceObjects.ArrayList
{
    [ToolTipText("ArrayList_SetAt_Summary")]
    public partial class SetAtFlowSourceObject : ArrayListFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "ArrayList.SetAt"; }
        }

        [ToolTipText("ArrayList_SetAt_Value")]
        public object Value
        {
            private get;
            set;
        }

        [ToolTipText("ArrayList_SetAt_Index")]
        public int Index
        {
            private get;
            set;
        }

        public override void In(FlowScriptEngine.FlowEventArgs e)
        {
            SetArrayList();
            if (ArrayList != null)
            {
                SetValue(nameof(Value));
                SetValue(nameof(Index));
                ArrayList[Index] = Value;
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
