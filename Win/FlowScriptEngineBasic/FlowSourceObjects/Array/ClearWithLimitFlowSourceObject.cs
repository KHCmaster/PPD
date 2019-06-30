namespace FlowScriptEngineBasic.FlowSourceObjects.Array
{
    [ToolTipText("Array_Clear_Summary")]
    public partial class ClearWithLimitFlowSourceObject : ArrayFlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Array.Clear"; }
        }

        [ToolTipText("Array_Clear_Index")]
        public int Index
        {
            private get;
            set;
        }

        [ToolTipText("Array_Clear_Length")]
        public int Length
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
                SetValue(nameof(Length));
                System.Array.Clear(Array, Index, Length);
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
